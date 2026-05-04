using MP3PlayerV2.Models;
using System.Data;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;
using static BazthalLib.DebugUtils;

namespace MP3PlayerV2.Commands
{  
    public class CommandDispatcher
    {
        private readonly Dictionary<string, ICommandHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);
    
        private readonly List<CommandInfo> _metadata = [];

        private readonly List<LoadedPlugin> _loadedPlugins = [];


        private class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            /// <summary>
            /// Initializes a new instance of the <see cref="PluginLoadContext"/> class with the specified plugin path.
            /// </summary>
            /// <remarks>The <see cref="PluginLoadContext"/> is created as a collectible context,
            /// allowing for dynamic unloading of the plugin.</remarks>
            /// <param name="pluginPath">The file path to the plugin assembly. This path is used to resolve assembly dependencies.</param>
            public PluginLoadContext(string pluginPath) : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
            }

            /// <summary>
            /// Loads an assembly given its <see cref="AssemblyName"/>.
            /// </summary>
            /// <remarks>This method attempts to resolve the assembly path using a custom resolver. If
            /// the path is successfully resolved, the assembly is loaded from the specified path. If the path cannot be
            /// resolved, the method returns <see langword="null"/>.</remarks>
            /// <param name="assemblyName">The name of the assembly to load. Cannot be null.</param>
            /// <returns>The loaded <see cref="Assembly"/> if the assembly is found; otherwise, <see langword="null"/>.</returns>
            protected override Assembly? Load(AssemblyName assemblyName)
            {
                string? path = _resolver.ResolveAssemblyToPath(assemblyName);

                return path != null ? LoadFromAssemblyPath(path) : null;
            }

        }


        private class LoadedPlugin
        {
            public string Path { get; set; } = string.Empty;
            public PluginLoadContext Context { get; set; } = null;
            public Assembly Assembly { get; set; } = null;
            public List<string> Commands { get; set; } = new();
        }

        /// <summary>
        /// Retrieves the metadata information for available commands.
        /// </summary>
        /// <returns>A list of <see cref="CommandInfo"/> objects representing the metadata of each command.</returns>
        public List<CommandInfo> GetMetaData() => _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDispatcher"/> class.
        /// </summary>
        /// <remarks>This constructor loads built-in command handlers and all available plugins to prepare
        /// the dispatcher for handling commands. Ensure that the necessary plugins are available in the application
        /// environment before instantiation.</remarks>
        public CommandDispatcher()
        {
            LoadBuiltInHandlers();
            LoadAllPlugins();
        }

        /// <summary>
        /// Loads and registers command handlers from built-in assemblies.
        /// </summary>
        /// <remarks>This method scans all assemblies currently loaded in the application domain,
        /// excluding those located in directories containing "Plugins" in their path. It then registers each command
        /// type found in these assemblies as a built-in handler.</remarks>
        public void LoadBuiltInHandlers()
        {
            var builtInAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a =>
                    {
                        string? loc = string.Empty;
                        try { loc = a.Location; } catch { }
                        return string.IsNullOrEmpty(loc) || !loc.Contains("Plugins", StringComparison.OrdinalIgnoreCase);
                    });
            foreach (var type  in builtInAssemblies.SelectMany(GetCommandTypesFromAssembly))
                RegisterCommand(type, isPlugin: false);
        }

        /// <summary>
        /// Retrieves a collection of types from the specified assembly that implement the <see cref="ICommandHandler"/>
        /// interface and are decorated with the <see cref="CommandAttribute"/>.
        /// </summary>
        /// <param name="a">The assembly to search for command handler types.</param>
        /// <returns>An enumerable collection of types that implement <see cref="ICommandHandler"/>, are not abstract or
        /// interfaces, and have the <see cref="CommandAttribute"/> applied. If the assembly cannot be fully loaded,
        /// returns the types that could be loaded and meet the criteria.</returns>
        private static IEnumerable<Type> GetCommandTypesFromAssembly(Assembly a)
        {
            try
            {
                return a.GetTypes().Where(t =>
                    typeof(ICommandHandler).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t.GetCustomAttribute<CommandAttribute>() != null);
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t =>
                    t != null &&
                    typeof(ICommandHandler).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t.GetCustomAttribute<CommandAttribute>() != null)!;
            }
        }

        /// <summary>
        /// Registers a command handler for the specified type if it is decorated with a <see cref="CommandAttribute"/>.
        /// </summary>
        /// <remarks>The method attempts to create an instance of the specified type and register it as a
        /// command handler. If a command with the same name already exists, it will be overridden. Logs are generated
        /// for both successful registrations and any exceptions that occur during the process.</remarks>
        /// <param name="type">The <see cref="Type"/> of the command handler to register. Must be decorated with <see
        /// cref="CommandAttribute"/>.</param>
        /// <param name="isPlugin">Indicates whether the command is part of a plugin. If <see langword="true"/>, logs the registration as a
        /// plugin command; otherwise, as a built-in command.</param>
        /// <returns><see langword="true"/> if the command handler was successfully registered; otherwise, <see
        /// langword="false"/>.</returns>
        private bool RegisterCommand(Type type, bool isPlugin)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>();
            if (attr == null) return false;

            try
            {
                if (Activator.CreateInstance(type) is ICommandHandler handler)
                {
                    string commandName = attr.Name;

                    if (_handlers.ContainsKey(commandName))
                    {
                        BazthalLib.DebugUtils.Log(
                            "CommandOverride",
                            commandName,
                            $"Overridden by {type.Assembly.GetName().Name}", logLevel: LogLevel.Info
                        );
                    }

                    _handlers[commandName] = handler;

                    _metadata.Add(new CommandInfo(
                        Name: attr.Name,
                        Author: attr.Author,
                        Version: attr.Version,
                        HandlerType: type.FullName ?? "UnknownType",
                        AssemblyName: type.Assembly.GetName().Name ?? "UnknownAssembly",
                        Description: attr.Description,
                        Example: attr.Example,
                        Category: attr.Category ?? type.Namespace?.Split('.')?.Last() ?? "Uncategorized"
                    ));

                    BazthalLib.DebugUtils.Log(
                        isPlugin ? "PluginCommandRegister" : "BuiltInCommandRegister",
                        commandName,
                        $"Registered from {type.Assembly.GetName().Name}", logLevel: LogLevel.Info
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                BazthalLib.DebugUtils.Log("Handler Load", type.FullName, ex.ToString(), logLevel: LogLevel.Error);
            }

            return false;
        }

        /// <summary>
        /// Loads all plugin assemblies from the Plugins directory located in the application's startup path.
        /// </summary>
        /// <remarks>This method searches for all DLL files in the Plugins directory and attempts to load
        /// each one as a plugin. If the Plugins directory does not exist, the method returns without performing any
        /// actions.</remarks>
        private void LoadAllPlugins() 
        {
            string pluginPath = Path.Combine(Application.StartupPath, "Plugins");
               if (!Directory.Exists(pluginPath)) 
                return;

            foreach (var dll in Directory.GetFiles(pluginPath, "*.dll"))
            {
                LoadPlugin(dll);
            }

        }

        /// <summary>
        /// Loads a plugin from the specified DLL file path and registers its commands.
        /// </summary>
        /// <remarks>This method attempts to load an assembly from the provided DLL path using a custom
        /// plugin load context. It extracts command types from the assembly and registers them as plugin commands.
        /// Successfully loaded plugins are tracked internally, and relevant information is logged. If an error occurs
        /// during loading, it is logged as an error.</remarks>
        /// <param name="dllPath">The file path to the DLL containing the plugin to load. Must not be null or empty.</param>
        public void LoadPlugin(string  dllPath)
        {
            try { 
                var context = new PluginLoadContext(dllPath);
                var assembly = context.LoadFromAssemblyPath(dllPath);

                var commandTypes = GetCommandTypesFromAssembly(assembly).ToList();
                var registeredCommands = new List<string>();

            foreach (var type in commandTypes)
            {
                if (RegisterCommand(type, isPlugin: true))
                    registeredCommands.Add(type.GetCustomAttribute<CommandAttribute>()?.Name ?? type.Name);
            }

            _loadedPlugins.Add(new LoadedPlugin
            {
                Path = dllPath,
                Context = context,
                Assembly = assembly,
                Commands = registeredCommands
            });

            BazthalLib.DebugUtils.Log("PluginLoader", "Loaded", dllPath, logLevel: LogLevel.Info);
        }
            catch (Exception ex)
            {
                BazthalLib.DebugUtils.Log("Plugin Loader", dllPath, ex.ToString(), logLevel: LogLevel.Error);
            }
}
        public void UnloadPlugin(string dllPath)
        {
            var plugin = _loadedPlugins.FirstOrDefault(p => string.Equals(p.Path, dllPath, StringComparison.OrdinalIgnoreCase));
            if (plugin == null) return;

            foreach (var cmd in plugin.Commands)
            {
                _handlers.Remove(cmd);
                _metadata.RemoveAll(m => m.Name.Equals(cmd, StringComparison.OrdinalIgnoreCase));
            }

            plugin.Context.Unload();
            _loadedPlugins.Remove(plugin);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            BazthalLib.DebugUtils.Log("PluginUnload", dllPath, "Successfully unloaded", logLevel: LogLevel.Info);
        }

        /// <summary>
        /// Processes a raw JSON message to execute a corresponding command if recognized.
        /// </summary>
        /// <remarks>The method attempts to deserialize the <paramref name="rawMessage"/> into a <see
        /// cref="PlayerCommand"/> object. If the command is valid and recognized, it is executed using the appropriate
        /// handler. If the command is invalid, unrecognized, or if an error occurs during deserialization or execution,
        /// the method responds with an error message and logs the exception details.</remarks>
        /// <param name="rawMessage">The raw JSON string representing the command to be executed.</param>
        /// <param name="context">The context in which the command is executed, providing response capabilities.</param>
        /// <param name="options">The JSON serializer options used for deserializing the command.</param>
        /// <returns><see langword="true"/> if the command is successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Dispatch(string rawMessage, CommandContext context, JsonSerializerOptions options)
        {
            try
            {
                var cmd = JsonSerializer.Deserialize<PlayerCommand>(rawMessage, options);

                if (cmd == null || string.IsNullOrWhiteSpace(cmd.Command))
                {
                    context.Respond("Dispatch", false, "Invalid command payload", null);
                    return false;
                }

                string commandName = cmd.Command.ToLowerInvariant();

                if (_handlers.TryGetValue(commandName, out var handler))
                {
                    return handler.Execute(cmd, context);
                }

                context.Respond("Dispatch", false, $"Unknown Command: {cmd.Command}", null);
                return false;
            }
            catch (JsonException jex)
            {
                context.Respond("Dispatch", false, "Failed to parse command JSON", jex.Message);
                BazthalLib.DebugUtils.Log("Dispatch", "JsonException", jex.ToString(), logLevel: LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                context.Respond("Dispatch", false, "Command execution failed", ex.Message);
                BazthalLib.DebugUtils.Log("Dispatch", "GeneralException", ex.ToString(), logLevel: LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Lists all registered commands along with their origins and type names.
        /// </summary>
        /// <remarks>The origin indicates whether the command is part of the core system or provided by a
        /// plugin.</remarks>
        /// <returns>An enumerable collection of tuples, each containing the command name, its origin (either "Core" or
        /// "Plugin"), and the full type name of the handler.</returns>
        public IEnumerable<(string Command, string Origin, string TypeName)> ListRegisteredCommands()
        {
            foreach (var kvp in _handlers)
            {
                string origin = kvp.Value.GetType().Assembly == Assembly.GetExecutingAssembly()
                    ? "Core"
                    : "Plugin";

                yield return (kvp.Key, origin, kvp.Value.GetType().FullName ?? "Unknown");
            }
        }
        
        /// <summary>
        /// Generates a JSON representation of the metadata with configurable formatting options.
        /// </summary>
        /// <param name="indent">Specifies whether the JSON output should be indented for readability. Defaults to <see langword="true"/>.</param>
        /// <param name="relaxedFormat">Determines if the JSON encoder should use relaxed escaping rules. Defaults to <see langword="true"/>.</param>
        /// <returns>A JSON string representing the metadata.</returns>
        public string GenerateDocumentationJson(bool indent = true, bool relaxedFormat = true)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = indent,
                Encoder = relaxedFormat
                    ? JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    : JavaScriptEncoder.Default
            };

            return JsonSerializer.Serialize(_metadata, jsonOptions);
        }
      
    }
}
