using MP3PlayerV2.Models;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Central state management service that provides access to application-wide state and services.
    /// This eliminates the need to pass instance fields between the form and services.
    /// </summary>
    public class ApplicationStateService
    {
        private static ApplicationStateService? _instance;
        private static readonly object _lock = new();

        public static ApplicationStateService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ApplicationStateService();
                    }
                }
                return _instance;
            }
        }

        #region Services

        public AudioPlaybackService AudioPlayback { get; set; } = null!;
        public AudioDeviceService AudioDevices { get; set; } = null!;
        public TrackNavigationService TrackNavigation { get; set; } = null!;
        public PlaylistOperationsService PlaylistOperations { get; set; } = null!;
        public WebSocketService WebSocket { get; set; } = null!;
        public TrackSearchService TrackSearch { get; set; } = null!;
        public PlaylistManager PlaylistManager { get; set; } = null!;

        #endregion

        #region State

        public Track? CurrentTrack { get; set; }
        public string CurrentTrackFilePath { get; set; } = string.Empty;
        public int VolumeLevel { get; set; } = 100;
        public AppSettings Settings { get; set; } = null!;
        public SmartShuffleMode SmartShuffleMode { get; set; } = SmartShuffleMode.UnplayedFirst;

        #endregion

        #region UI Callbacks

        /// <summary>
        /// Invokes an action on the UI thread.
        /// </summary>
        public Action<Action>? InvokeOnUI { get; set; }

        /// <summary>
        /// Gets the selected audio device index from the UI.
        /// </summary>
        public Func<int>? GetSelectedAudioDeviceIndex { get; set; }

        /// <summary>
        /// Sets the selected audio device index in the UI.
        /// </summary>
        public Action<int>? SetSelectedAudioDeviceIndex { get; set; }

        /// <summary>
        /// Gets the selected playlist option index from the UI.
        /// </summary>
        public Func<int>? GetSelectedPlaylistOptionIndex { get; set; }

        /// <summary>
        /// Sets the selected playlist option index in the UI.
        /// </summary>
        public Action<int>? SetSelectedPlaylistOptionIndex { get; set; }

        /// <summary>
        /// Gets or sets a delegate that returns the currently selected shuffle mode.
        /// </summary>
        public Func<string>? GetSelectedShuffleMode { get; set; }

        /// <summary>
        /// Gets the selected track index from the UI.
        /// </summary>
        public Func<int>? GetSelectedTrackIndex { get; set; }

        /// <summary>
        /// Sets the selected track index in the UI.
        /// </summary>
        public Action<int>? SetSelectedTrackIndex { get; set; }

        /// <summary>
        /// Sets the current track label text in the UI.
        /// </summary>
        public Action<string>? SetCurrentTrackLabel { get; set; }

        /// <summary>
        /// Sets the form title text in the UI.
        /// </summary>
        public Action<string>? SetFormTitle { get; set; }

        /// <summary>
        /// Sets the tracking slider maximum value in the UI.
        /// </summary>
        public Action<int>? SetTrackingSliderMaximum { get; set; }

        /// <summary>
        /// Sets the tracking slider value in the UI.
        /// </summary>
        public Action<int>? SetTrackingSliderValue { get; set; }

        /// <summary>
        /// Gets the tracking slider value from the UI.
        /// </summary>
        public Func<int>? GetTrackingSliderValue { get; set; }

        /// <summary>
        /// Sets the volume slider value in the UI.
        /// </summary>
        public Action<int>? SetVolumeSliderValue { get; set; }

        /// <summary>
        /// Ensures a track is visible in the playlist UI.
        /// </summary>
        public Action<int>? EnsureTrackVisible { get; set; }

        /// <summary>
        /// Gets the playlist count from the UI.
        /// </summary>
        public Func<int>? GetPlaylistCount { get; set; }

        #endregion

        private ApplicationStateService()
        {
        }

        /// <summary>
        /// Resets the singleton instance (useful for testing).
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
    }
}
