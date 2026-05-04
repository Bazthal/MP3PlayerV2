using BazthalLib;
using BazthalLib.Configuration;
using BazthalLib.Systems.IO;
using MP3PlayerV2.Models;
using System.Text.Json;
using static BazthalLib.DebugUtils;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Provides methods to manage application configuration settings, including loading, saving, and migrating
    /// settings.
    /// </summary>
    /// <remarks>The <see cref="ConfigManager"/> class handles the application's configuration by reading from
    /// and writing to a JSON file. It ensures that settings are up-to-date by migrating old configuration formats to
    /// the current format. The class also logs relevant information and errors during these operations. The
    /// configuration file is located at the application's startup path, and a backup is created before any save
    /// operation.</remarks>
    public static class ConfigManager
    {

        private static readonly string ConfigPath = Path.Combine(Application.StartupPath, "Config/Settings.json");
        private static readonly int CurrentConfigVersion = 1;

        public static AppSettings Settings = new();

        /// <summary>
        /// Loads the application settings from the configuration file.
        /// </summary>
        /// <remarks>If the configuration file does not exist, a new file is created with default
        /// settings. If the configuration file is in an old format, it is migrated to the new format and saved. Logs
        /// information and errors during the loading process.</remarks>
        public static void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                DebugUtils.Log("Config Manager", "Load", "File Not found, creating new file", logLevel: LogLevel.Info);
                Settings = new AppSettings();
                Save();
                return;
            }

            JSON<AppSettings> jsonConfig = new JSON<AppSettings>(ConfigPath);

            try 
            {
                string rawJson = File.ReadAllText(ConfigPath);
                using var jsonDoc = JsonDocument.Parse(rawJson);
                var root = jsonDoc.RootElement;

                int configVersion = 0;
                if (root.TryGetProperty("ConfigVersion", out var versionProp) && versionProp.ValueKind == JsonValueKind.Number)
                {
                    configVersion = versionProp.GetInt32();
                }

                if (configVersion == 0)
                {
                    //Create backup of old config
                    Files.MigrationBackUp(ConfigPath);
                    DebugUtils.Log("Config Manager", "Load", "Migrating from old config format", logLevel: LogLevel.Info);
                    Settings = MigrateFromOldConfig(root);

                    // Save migrated config
                    string migratedJson = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigPath, migratedJson);
                }
                else
                {
                    jsonConfig.Load();
                    Settings = jsonConfig.Data;
                }
            }
            catch (Exception ex) 
            {
                DebugUtils.Log("Config Manager", "Load", $"Failed to load config: {ex.Message}", logLevel: LogLevel.Error);
            }
        }


        /// <summary>
        /// Migrates configuration settings from an old JSON format to a new <see cref="AppSettings"/> instance.
        /// </summary>
        /// <remarks>This method extracts playback and websocket settings from the provided JSON element.
        /// Default values are used if certain properties are not present or are of an unexpected type.</remarks>
        /// <param name="root">A <see cref="JsonElement"/> representing the root of the old configuration JSON.</param>
        /// <returns>An <see cref="AppSettings"/> object populated with settings from the old configuration.</returns>
        private static AppSettings MigrateFromOldConfig(JsonElement root)
        {
            var newConfig = new AppSettings();
            // Playback Settings
            newConfig.Playback.VolumeLvl = root.TryGetProperty("VolumeLvl", out var vol) && vol.ValueKind == JsonValueKind.Number ? vol.GetInt32() : 100;
            newConfig.Playback.AudioDevice = root.TryGetProperty("AudioDevice", out var audioDev) && audioDev.ValueKind == JsonValueKind.String ? audioDev.GetString()! : string.Empty;
            newConfig.Playback.PlayListMode = root.TryGetProperty("PlayListMode", out var plMode) && plMode.ValueKind == JsonValueKind.String ? plMode.GetString()! : string.Empty;

            //Websocket Settings
            newConfig.WebSocket.Address = root.TryGetProperty("WebSocketAddress", out var wsAddr) && wsAddr.ValueKind == JsonValueKind.String ? wsAddr.GetString()! : "127.0.0.1";
            newConfig.WebSocket.Port = root.TryGetProperty("WebSocketPort", out var wsPort) && wsPort.ValueKind == JsonValueKind.Number ? wsPort.GetInt32() : 8080;
            newConfig.WebSocket.EndPoint = root.TryGetProperty("WebSocketEndPoint", out var wsEnd) && wsEnd.ValueKind == JsonValueKind.String ? wsEnd.GetString()! : "/";
            newConfig.WebSocket.AutoStart = root.TryGetProperty("AutoStart", out var autoStart) && autoStart.ValueKind == JsonValueKind.True;

            newConfig.ConfigVersion = CurrentConfigVersion;

            return newConfig;
        }
        
        /// <summary>
        /// Saves the current configuration settings to a file.
        /// </summary>
        /// <remarks>This method serializes the current settings into a JSON format and writes them to the
        /// specified configuration file. It creates a backup of the existing configuration file before saving the new
        /// settings.</remarks>
        public static void Save()
        {
            try 
            {
                // Backup existing config before saving
                Files.CreateBackup(ConfigPath, 3);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(ConfigPath, json);
            } 
            catch (Exception ex) 
            {
                DebugUtils.Log("Config Manager", "Save", $"Failed to save config: {ex.Message}");
            }            
        }        
    }
}
