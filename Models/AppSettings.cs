namespace MP3PlayerV2.Models
{
    /// <summary>
    /// Represents the application settings, including configuration versions and various feature-specific settings.
    /// </summary>
    /// <remarks>This class provides a centralized configuration model for the application, grouping settings
    /// for playback,  play data, WebSocket communication, smart shuffle, track rating, and command behavior. Each
    /// property  corresponds to a specific feature or module within the application.</remarks>
    public class AppSettings
    {
        public int ConfigVersion { get; set; } = 1;
        public PlaybackSettings Playback { get; set; } = new();
        public PlayDataSettings PlayData { get; set; } = new();
        public WebSocketSettings WebSocket { get; set; } = new();
        public SmartShuffleSettings SmartShuffle { get; set; } = new();
        public TrackRatingSettings TrackRating { get; set; } = new();
        public CommandBehaviourSettings CommandBehaviour { get; set; } = new();
    }

    /// <summary>
    /// Represents the settings used to configure playback behavior, including volume level, audio device, playlist
    /// mode, and track history limits.
    /// </summary>
    /// <remarks>This class provides properties to customize playback settings for an audio player.  Use these
    /// settings to control the playback environment, such as the output device, volume, and playback mode.</remarks>
    public class PlaybackSettings
    {
        public int VolumeLvl { get; set; } = 100;
        public string AudioDevice { get; set; } = string.Empty;
        public string PlayListMode { get; set; } = string.Empty;
        public int MaxTrackHistory { get; set; } = 100;
    }

    /// <summary>
    /// Represents the settings used to configure data management behavior for play data.
    /// </summary>
    /// <remarks>This class provides options for controlling how play data is pruned or archived.  Use the
    /// properties to enable or disable pruning, specify whether data should be archived  instead of deleted, and define
    /// the retention period for pruning.</remarks>
    public class PlayDataSettings
    {
        public bool EnablePruning { get; set; } = false;
        public bool ArchiveOverDelete { get; set; } = false;
        public int PruneDays { get; set; } = 90;
    }

    /// <summary>
    /// Represents the configuration settings for a WebSocket connection.
    /// </summary>
    /// <remarks>This class provides properties to configure the WebSocket server, including the address,
    /// port,  endpoint, and whether the server should start automatically. These settings are typically used  to
    /// initialize and manage a WebSocket server instance.</remarks>
    public class WebSocketSettings
    {
        public string Address { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8080;
        public string EndPoint { get; set; } = "/";
        public bool AutoStart { get; set; } = false;
    }

    /// <summary>
    /// Represents the configuration settings for the smart shuffle feature.
    /// </summary>
    /// <remarks>The <see cref="SmartShuffleSettings"/> class allows customization of the shuffle behavior, 
    /// including the mode of operation and the weighting criteria for shuffling.</remarks>
    public class SmartShuffleSettings
    {
        public string Mode { get; set; } = "UnplayedFirst";
        public ShuffleWeights Weights { get; set; } = new();
    }

    /// <summary>
    /// Represents a set of weights used to influence the prioritization of items during a shuffle operation.
    /// </summary>
    /// <remarks>The weights determine the relative importance of different item states, such as whether an
    /// item has been played, skipped, or rated by the user. Higher weights increase the likelihood of an item being
    /// prioritized during shuffling.</remarks>
    public class ShuffleWeights
    {
        //Weights are not yet used
        public float Unplayed { get; set; } = 1f;
        public float Played { get; set; } = 0.5f;
        public float Skipped { get; set; } = 0.25f;
        public float UserRated { get; set; } = 0.75f;
    }

    /// <summary>
    /// Represents the configuration settings used to calculate track ratings based on user interactions.
    /// </summary>
    /// <remarks>This class provides configurable penalties, rewards, thresholds, and other parameters that
    /// influence  the scoring and rating of tracks. These settings are used to adjust the star rating of tracks based 
    /// on user behavior, such as skipping, replaying, or liking/disliking tracks. It also includes parameters  for
    /// monthly decay and immunity periods to fine-tune the rating system.</remarks>
    public class TrackRatingSettings
    {
        //Penalties 
        public int EarlySkipPenalty { get; set; } = 3;
        public int MidSkipPenalty { get; set; } = 2;
        public int SeekToEndPenalty { get; set; } = 0;
        public int ManualDislikePenalty { get; set; } = 5;


        //Rewards
        public int NoSkipReward { get; set; } = 1;
        public int ReplayReward { get; set; } = 2;
        public int ManualLikeBoost { get; set; } = 5;


        //Thresholds
        public double EarlySkipThresholdPercent { get; set; } = 0.3;
        public int LeadInImmunitySeconds { get; set; } = 2;
        public int LeadOutImmunitySeconds { get; set; } = 10;

        //Star Rating Calculations
        public int FiveStarMinScore { get; set; } = 20;
        public int FourStarMinScore { get; set; } = 15;
        public int ThreeStarMinScore { get; set; } = 10;
        public int TwoStarMinScore { get; set; } = 5;

        //Monthly Decay
        public double MonthlyDecayPercent { get; set; } = 0.05;
    }

    /// <summary>
    /// Represents the configuration settings that control the behavior of commands in the application.
    /// </summary>
    /// <remarks>This class provides options to customize specific behaviors, such as whether to include
    /// tracks in counts and whether to allow remote database wipe operations. These settings can be adjusted to suit
    /// the needs of the application.</remarks>
    public class CommandBehaviourSettings
    {
        public bool IncludeTracksInCount { get; set; } = false;
        public bool AllowRemoteDatabaseWipe { get; set; } = false;

    }
}

