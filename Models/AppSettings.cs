namespace MP3PlayerV2.Models
{
    /// <summary>
    /// Represents the application settings for configuring audio and network parameters.
    /// </summary>
    /// <remarks>This class provides properties to configure audio settings such as volume level and audio
    /// device, as well as network settings including WebSocket address, port, and endpoint. These settings can be used
    /// to initialize or modify the application's behavior regarding audio output and WebSocket communication.</remarks>
    public class AppSettings
    {
        public int VolumeLvl { get; set; } = 100;
        public string AudioDevice { get; set; } = string.Empty;
        public string PlayListMode { get; set; } = string.Empty;
        public string WebSocketAddress { get; set; } = "127.0.0.1";
        public int WebSocketPort { get; set; } = 8080;
        public string WebSocketEndPoint { get; set; } = "/";
        public bool AutoStart { get; set; } = false;
    }

}
