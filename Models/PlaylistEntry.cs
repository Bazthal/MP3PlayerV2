namespace MP3PlayerV2.Models
{

    /// <summary>
    /// Represents an entry in a playlist, containing metadata about a track such as its title, artist, and file path.
    /// </summary>
    /// <remarks>This class provides a simplified representation of a track for use in playlist files, 
    /// avoiding the inclusion of unnecessary data. It includes essential metadata such as the track's unique
    /// identifier,  file path, and basic descriptive information.</remarks>
    public class PlaylistEntry
    {
        //This is a simplified version of Track for use in Playlist files to avoid saving unnecessary data.

        public Guid Guid { get; set; }
        public string Hash { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Title { get; set; } = "Unknown Title";
        public string Artist { get; set; } = "Unknown Artist";
        public string? Album { get; set; } = string.Empty;
        public int? DurationSeconds { get; set; } = 0;
    }
}
