using LiteDB;

namespace MP3PlayerV2.Models
{
    /// <summary>
    /// Represents a music track with metadata such as title, artist, and playback statistics.
    /// </summary>
    /// <remarks>The <see cref="Track"/> class provides properties to store information about a music track,
    /// including its file path, title, artist, duration, play count, last played date, times skipped, and user rating.
    /// This class can be used to manage and display track information in music applications.</remarks>
    public class Track
    {
        [BsonId]
        public Guid Guid { get; set; } = Guid.Empty;
        public string Hash { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Title { get; set; } = "Unknown Title";
        public string Artist { get; set; } = "Unknown Artist";
        public string? Album { get; set; } = string.Empty;
        public int? DurationSeconds { get; set; } = 0;

        public int? PlayCount { get; set; } = 0;
        public int? SkipCount { get; set; } = 0;
        public int? PlayCompleteCount { get; set; } = 0;
        public DateTime? LastPlayed { get; set; }
        public DateTime? LastStatReset { get; set; }
        public DateTime? LastUpdated { get; set; }


        public int RatingScore { get; set; } = 0;
        public int StarRating { get; set; } = 0;
        public bool Liked { get; set; } = false;
        public bool Disliked { get; set; } = false;
        public DateTime? LastDecayApplied { get; set; }

        public int? MigrationVersion { get; set; } = 0;
        
        public override string ToString() => $"{Artist} - {Title}";

    }


}

