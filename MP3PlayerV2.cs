using BazthalLib;
using BazthalLib.Configuration;
using BazthalLib.Controls;
using BazthalLib.Systems.IO;
using BazthalLib.UI;
using CSCore;
using CSCore.SoundOut;
using MP3PlayerV2.Commands;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MP3PlayerV2
{

    public partial class MP3PlayerV2 : Form
    {
        #region Fields

#nullable enable
        private readonly AudioPlaybackService _audioPlayback = new();
        private readonly AudioDeviceService _audioDevices = new();
        private readonly TrackNavigationService _trackNavigation = new();
        private readonly PlaylistOperationsService _playlistOps = new();
        private readonly WebSocketService _webSocket = new();
        private readonly TrackSearchService _trackSearch = new();
#nullable disable

        public static MP3PlayerV2 Instance { get; private set; }

        //private readonly ListBox _audioDeviceIDList = new();

        #region Playback Settings

        private int _volumeLevel = 100;
        //private static int _maxTrackHistory = 100;

        #endregion Playback Settings

        #region Playback Utilities

        private bool _trackEnd = false;
        private static string _currentTrackFilePath;
        private static Track _currentTrackModel = null;
        private static readonly Random _rng = new();
        private readonly PlaylistManager _playlistManager = new();
        internal static int _updateStep = 1;
        private bool _userSeeked = false;

        private static SmartShuffleMode _smartShuffleMode = SmartShuffleMode.UnplayedFirst;

        #endregion Playback Utilities

        #region Track Rating Settings

        private static int _leadInImmunity = 2;
        private static int _leadOutImmunity = 10;

        #endregion Track Rating Settings

        #region WebSocket Utilities

        private readonly CommandDispatcher _dispatcher = new();
        private bool _includeData = false;

        #endregion WebSocket Utilities

        #region Configuration

        private static readonly JsonSerializerOptions _caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };
        internal AppSettings _settings = ConfigManager.Settings;
        private readonly string _customThemeConfig = Path.Combine(Application.StartupPath, "Config/CustomTheme.json");

        private static string _defaultPlaylistExtentionFilter = "M3U Playlists|*.m3u;*.m3u8|jsonPlaylist|*.jsonpl|All Supported|*.m3u;*.m3u8;*.jsonpl";
        private static readonly HashSet<string> SupportedAudioExtensions = new(StringComparer.OrdinalIgnoreCase) { ".flac", ".m4a", ".mp2", ".mp3", ".wav", ".wma" };
        private static readonly HashSet<string> SupportedPlaylistExtensions = new(StringComparer.OrdinalIgnoreCase) { ".m3u", ".m3u8", ".jsonpl" };

        #endregion Configuration

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MP3PlayerV2"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the MP3 player by initializing components, registering the
        /// form for theming,  and loading audio devices, themes, and settings. It also subscribes to playlist change
        /// events to update  the playlist UI and optionally starts the server thread if auto-start is
        /// enabled.</remarks>
        public MP3PlayerV2(string[] args = null)
        {
            Instance = this;
            InitializeComponent();
            Theming.RegisterForm(this);

            // Initialize ApplicationStateService with all services and state
            var appState = ApplicationStateService.Instance;
            appState.AudioPlayback = _audioPlayback;
            appState.AudioDevices = _audioDevices;
            appState.TrackNavigation = _trackNavigation;
            appState.PlaylistOperations = _playlistOps;
            appState.WebSocket = _webSocket;
            appState.TrackSearch = _trackSearch;
            appState.PlaylistManager = _playlistManager;
            appState.Settings = _settings;

            // Set up UI callbacks
            appState.InvokeOnUI = action => this.Invoke(action);
            appState.GetSelectedAudioDeviceIndex = () => AudioDeviceList.SelectedIndex;
            appState.SetSelectedAudioDeviceIndex = i => AudioDeviceList.SelectedIndex = i;
            appState.GetSelectedPlaylistOptionIndex = () => playList_Options.SelectedIndex;
            appState.SetSelectedPlaylistOptionIndex = i => playList_Options.SelectedIndex = i;
            appState.GetSelectedShuffleMode = () => _settings.SmartShuffle.Mode;
            appState.GetSelectedTrackIndex = () => playListBox.SelectedIndex;
            appState.SetSelectedTrackIndex = i => playListBox.SelectedIndex = i;
            appState.SetCurrentTrackLabel = text => Cur_Track_Label.Text = text;
            appState.SetFormTitle = text => this.Text = text;
            appState.SetTrackingSliderMaximum = max => Tracking_Slider.Maximum = max;
            appState.SetTrackingSliderValue = val => Tracking_Slider.Value = val;
            appState.GetTrackingSliderValue = () => Tracking_Slider.Value;
            appState.SetVolumeSliderValue = val => Volume_Slider.Value = val;
            appState.EnsureTrackVisible = index => playListBox.EnsureVisible(index);
            appState.GetPlaylistCount = () => playListBox.Items.Count;

            // Initialize audio devices
            _audioDevices.EnumerateDevices();
            PopulateAudioDeviceList();

            playList_Options.Items.Clear();
            foreach (PlaylistOption option in Enum.GetValues(typeof(PlaylistOption)))
            {
                playList_Options.Items.Add(option.ToString().Replace("_", " "));
            }

            //These should be last thing to process Before doing anything that uses it
            LoadThemeFromJson();
            LoadSettings();

            // Wire up audio playback events
            _audioPlayback.PlaybackStateChanged += OnPlaybackStateChanged;
            _audioPlayback.TrackEnded += OnTrackEnded;

            // Wire up audio device events
            _audioDevices.DevicesChanged += OnAudioDevicesChanged;

            _playlistManager.PlaylistChanged += () =>
            {
                playListBox.Items.Clear();
                foreach (var track in _playlistManager.Tracks)
                {
                    playListBox.Items.Add(track);
                }

                if (_playlistManager.Count <= 0)
                    playListBox.SelectedIndex = -1;
            };

            if (_settings.WebSocket.AutoStart)
            {
                StartWebSocketServer();
            }

            if (args != null && args.Length != 0)
            {
                bool autoPlay = _settings.Application.AutoPlayOnFileAssocLaunch;
                HandleDroppedFiles(args, autoPlay);
            }
            //Allows the user to keep the size and location of player between launches
            if (_settings.Application.RestoreSizeAndPosition)
            {
                this.Size = _settings.Application.PlayerSize;
                this.Location = _settings.Application.PlayerLocation;
            }
            else
            { this.StartPosition = FormStartPosition.WindowsDefaultLocation; }
            var lastfile = _settings.Application.LastPlaylistOpened;
            
            if (_settings.Application.AutoOpenLastPlaylist == true && (lastfile != string.Empty && File.Exists(lastfile)) ) 
            { LoadPlaylist(lastfile); }
        }

        #endregion Constructor

        #region Methods

        #region Core Playback Logic

#nullable enable
        /// <summary>
        /// Handles playback state changes from the audio service.
        /// </summary>
        private void OnPlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs e)
        {
            UpdatePlaybackState(e.NewState);
        }

        /// <summary>
        /// Handles track ended events from the audio service.
        /// </summary>
        private void OnTrackEnded(object? sender, TrackEndedEventArgs e)
        {
            if (e.ReachedEnd)
            {
                _trackEnd = true;
                DebugUtils.Log("AudioPlaybackService", "TrackEnded",
                    $"Track reached end naturally", logLevel: DebugUtils.LogLevel.Info);
            }
            else
            {
                DebugUtils.Log("AudioPlaybackService", "TrackEnded",
                    $"Track stopped by user", logLevel: DebugUtils.LogLevel.Info);
            }
        }
#nullable disable
        /// <summary>
        /// Starts the playback timer, resetting it if it is already running.
        /// </summary>
        /// <remarks>This method ensures that the playback timer is stopped and reset before starting it
        /// again. It is typically used to manage playback timing in scenarios where precise control over the timer is
        /// required.</remarks>
        private void StartPlaybackTimer()
        {
            PlayTimer.Stop(); // ensures it's reset
            PlayTimer.Start();
        }

        /// <summary>
        /// Updates the playback state and adjusts the playback timer accordingly.
        /// </summary>
        /// <remarks>When the playback state is set to <see cref="PlaybackState.Playing"/>, the playback
        /// timer is started.  If the state is set to <see cref="PlaybackState.Paused"/> or <see
        /// cref="PlaybackState.Stopped"/>, the playback timer is stopped.</remarks>
        /// <param name="state">The new playback state to apply. Must be one of the <see cref="PlaybackState"/> enumeration values.</param>
        private void UpdatePlaybackState(PlaybackState state)
        {
            switch (state)
            {
                case PlaybackState.Playing:
                    StartPlaybackTimer();
                    break;
                case PlaybackState.Paused:
                case PlaybackState.Stopped:
                    PlayTimer.Stop();
                    break;
            }
        }

        /// <summary>
        /// Initiates playback of the selected audio track from the playlist.
        /// </summary>
        /// <remarks>If a track is currently playing and it is different from the selected track, the
        /// current track is stopped before playing the new one. If playback is paused, it resumes from the paused
        /// position. The method updates the UI with the current track information and broadcasts the now playing
        /// information via WebSocket if the server is running. Playback statistics are saved to the database.</remarks>
        private void Play(Track playitem = null)
        {
            var appState = ApplicationStateService.Instance;

            if (_audioPlayback.State == PlaybackState.Paused)
            {
                _audioPlayback.Play();
                var currentTrackText = appState.SetCurrentTrackLabel != null ?
                    string.Empty : Cur_Track_Label.Text;
                appState.SetFormTitle?.Invoke($"MP3 Player - {currentTrackText}");
                DebugUtils.Log("Play", "Pause-Resumed", $"{currentTrackText}", logLevel: DebugUtils.LogLevel.Info);
                return;
            }

            if (_audioPlayback.State == PlaybackState.Playing)
            {
                var currentTrackText = appState.SetCurrentTrackLabel != null ?
                    string.Empty : Cur_Track_Label.Text;
                var selectedTrackIndex = appState.GetSelectedTrackIndex?.Invoke() ?? -1;
                var selectedItem = selectedTrackIndex >= 0 ? playListBox.SelectedItem : null;

                if (string.IsNullOrWhiteSpace(currentTrackText) ||
                    (selectedItem != null && !currentTrackText.Contains(selectedItem.ToString())))
                {
                    Stop();
                    DebugUtils.Log("No Track Match", "Play",
                        $"Match Not Found: {currentTrackText} is not {selectedItem?.ToString()}",
                        logLevel: DebugUtils.LogLevel.Info);
                }
                else
                {
                    DebugUtils.Log("Track Match", "Play", $"Match Found: {currentTrackText}",
                        logLevel: DebugUtils.LogLevel.Info);
                    return;
                }
            }

            var playlistCount = appState.GetPlaylistCount?.Invoke() ?? 0;
            var selectedIndex = appState.GetSelectedTrackIndex?.Invoke() ?? -1;

            if (playlistCount <= 0 || selectedIndex == -1)
                return;

            var deviceID = "";
            var audioDeviceIndex = appState.GetSelectedAudioDeviceIndex?.Invoke() ?? -1;
            if (audioDeviceIndex >= 0)
            {
                var device = _audioDevices.GetDevice(audioDeviceIndex);
                deviceID = device?.DeviceID ?? "";
            }

            var track = playitem ?? (Track)playListBox.SelectedItem;
            if (track == null) return;

            _currentTrackFilePath = track.FilePath;
            _currentTrackModel = track;

            appState.CurrentTrackFilePath = _currentTrackFilePath;
            appState.CurrentTrack = _currentTrackModel;

            if (!_audioPlayback.Initialize(_currentTrackFilePath, deviceID))
            {
                string message = $"Could Not find the file to play:\n{_currentTrackFilePath}";
                ThemableMessageBox.Show(message, "Unable to Play!", MessageBoxButtons.OK, 10000, MessageBoxIcon.Information);
                return;
            }

            _trackEnd = false;
            _audioPlayback.VolumeLevel = _volumeLevel;

            var trackText = track.ToString();
            appState.SetCurrentTrackLabel?.Invoke(trackText);
            appState.SetFormTitle?.Invoke($"MP3 Player - {trackText}");

            appState.SetTrackingSliderMaximum?.Invoke((int)_audioPlayback.Duration.TotalSeconds);
            appState.SetTrackingSliderValue?.Invoke(0);

            _audioPlayback.Play();

            TrackRatingManager.ApplyPlayStart(track, _settings.TrackRating);
            var previouslyPlayed = track.LastPlayed.HasValue ? track.LastPlayed.Value.ToLocalTime().ToString("g") : "Never";
            track.LastPlayed = DateTime.UtcNow;
            var dispTime = track.LastPlayed?.ToLocalTime().ToString("g");
            // var dispTime = $"{track.LastPlayed?.ToLocalTime().ToShortDateString()}-{track.LastPlayed?.ToLocalTime().ToShortTimeString()}";


            try
            {
                TrackDatabase.SaveStats(track);
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Play", "Save Stats", $"Saving to data base error: {ex.Message}",
                    logLevel: DebugUtils.LogLevel.Error);
            }

            DebugUtils.Log("Play", appState.SetFormTitle != null ? "Title Set" : Text,
                $"Current Track: {_currentTrackFilePath}",
                logLevel: DebugUtils.LogLevel.Info);
            DebugUtils.Log("Tracking", "Play", $"{_audioPlayback.Duration}",
                logLevel: DebugUtils.LogLevel.Info);

            if (!_webSocket.IsRunning)
            {
                DebugUtils.Log("Play", "WebSocket", "Websocket isn't running no broadcast sent",
                    logLevel: DebugUtils.LogLevel.Info);
                return;
            }

            var nowPlayingInfo = new { NowPlaying = track.ToString(), track.PlayCount, PlaylistIndex = _playlistManager.IndexOf(track), track.LastPlayed, LocalTime = dispTime, PlayedPreviously = previouslyPlayed };
            _webSocket.BroadcastNowPlaying(nowPlayingInfo);
        }

        /// <summary>
        /// Toggles the playback state between playing and paused.
        /// </summary>
        /// <remarks>If the playback is currently paused, this method resumes playback and updates the UI
        /// to reflect the playing state. If the playback is currently playing, this method pauses playback and updates
        /// the UI to reflect the paused state.</remarks>
        private void Pause()
        {
            var appState = ApplicationStateService.Instance;
            var currentTrackText = appState.SetCurrentTrackLabel != null ?
                Cur_Track_Label.Text : string.Empty;

            if (_audioPlayback.State == PlaybackState.Paused)
            {
                _audioPlayback.Play();
                appState.SetFormTitle?.Invoke($"MP3 Player - {currentTrackText}");
                return;
            }

            if (_audioPlayback.State == PlaybackState.Playing)
            {
                _audioPlayback.Pause();
                appState.SetFormTitle?.Invoke($"MP3 Player - {currentTrackText} - Paused");
                return;
            }
        }

        /// <summary>
        /// Stops the playback of the current audio track.
        /// </summary>
        /// <remarks>This method halts any ongoing audio playback and resets the player state.  It is safe
        /// to call this method multiple times; subsequent calls will have no effect if the player is already
        /// stopped.</remarks>
        private void Stop()
        {
            if (_audioPlayback.IsDisposed) return;

            ApplicationStateService.Instance.SetFormTitle?.Invoke("MP3 Player");
            _audioPlayback.Stop();
            PlayTimer?.Stop();
        }

        /// <summary>
        /// Advances to the next track in the playlist based on the current playlist options.
        /// </summary>
        /// <remarks>The method handles different playlist options such as repeating the playlist, repeating the current
        /// track, selecting a random track, or using a smart shuffle. If the end of the playlist is reached in sequential mode,
        /// playback stops and the player is reset.</remarks>
        /// <param name="automatic">Indicates whether the track change is automatic. If <see langword="false"/>, the current track's skip count is
        /// incremented.</param>
        private void NextTrack(bool automatic = false)
        {
            if (_playlistManager.Count <= 0)
            {
                DebugUtils.Log("Next", "No Tracks", "No tracks in the playlist to play.", logLevel: DebugUtils.LogLevel.Info);
                return;
            }

            if (!automatic)
            {
                if (_currentTrackModel != null && !_audioPlayback.IsDisposed)
                {
                    TimeSpan pos = _audioPlayback.Position;
                    TimeSpan len = _audioPlayback.Duration;

                    if (IsSkipValid(pos, len))
                    {
                        TrackRatingManager.ApplySkip(_currentTrackModel, _settings.TrackRating, pos.Seconds);
                        try { TrackDatabase.SaveStats(_currentTrackModel); }
                        catch (Exception ex) { DebugUtils.Log("Next", "Save Stats", $"Error: {ex.Message}", logLevel: DebugUtils.LogLevel.Error); }
                    }
                }
            }

            PlaylistOption option = PlaylistOption.Sequential;
            if (playList_Options.SelectedIndex != -1)
            {
                string selectedText = playList_Options.SelectedItem.ToString().Replace(" ", "_");
                _ = Enum.TryParse(selectedText, out option);
            }

            var (nextTrack, nextIndex) = _trackNavigation.GetNextTrack(
                _currentTrackModel,
                playListBox.SelectedIndex,
                _playlistManager.Count,
                option,
                ApplicationStateService.Instance.SmartShuffleMode,
                //_smartShuffleMode,
                i => _playlistManager.Get(i),
                () => _playlistManager.Tracks
            );

            if (nextTrack == null && nextIndex == -1)
            {
                ResetUI();
                var appState = ApplicationStateService.Instance;
                appState.SetSelectedTrackIndex?.Invoke(-1);
                appState.SetTrackingSliderValue?.Invoke(0);
                _audioPlayback.Stop();
                _audioPlayback.Dispose();
                return;
            }

            if (option == PlaylistOption.RepeatTrack)
            {
                TrackRatingManager.ApplyReplay(_currentTrackModel, _settings.TrackRating);
            }

            if (nextTrack != null)
            {
                _audioPlayback.Stop();
                playListBox.SelectedIndex = nextIndex;
                Play(nextTrack);
            }
        }

        /// <summary>
        /// Navigates to the previous track in the playlist, or restarts the current track if it has been playing for more than 5 seconds.
        /// </summary>
        /// <remarks>
        /// If the current track's playback position exceeds 5 seconds, playback is restarted from the beginning of the track.
        /// Otherwise, the method attempts to move to the previous track using the track history stack or, if unavailable, by selecting the previous item in the playlist.
        /// </remarks>
        private void PreviousTrack()
        {
            if (_playlistManager.Count <= 0)
            {
                DebugUtils.Log("Previous", "No Tracks", "No tracks in the playlist to play.", logLevel: DebugUtils.LogLevel.Info);
                return;
            }

            double currentPosition = _audioPlayback.IsDisposed ? 0 : _audioPlayback.Position.TotalSeconds;

            var (previousTrack, previousIndex, shouldRestart) = _trackNavigation.GetPreviousTrack(
                currentPosition,
                playListBox.SelectedIndex,
                _playlistManager.Count,
                i => _playlistManager.Get(i),
                restartThreshold: 5.0
            );

            if (shouldRestart)
            {
                _audioPlayback.SeekTo(0);
                return;
            }

            if (previousTrack != null && previousIndex >= 0)
            {
                _audioPlayback.Stop();
                playListBox.SelectedIndex = previousIndex;
                Play();
            }
        }

        /// <summary>
        /// Sets the volume level for the audio source.
        /// </summary>
        /// <remarks>This method adjusts the volume of the audio source based on the current volume level.
        /// The volume is clamped between 0.0 (mute) and 1.0 (full volume).</remarks>
        private void SetVolume(int vol)
        {
            _volumeLevel = vol;
            _audioPlayback.VolumeLevel = vol;
            ApplicationStateService.Instance.SetVolumeSliderValue?.Invoke(_volumeLevel);
            ApplicationStateService.Instance.VolumeLevel = vol;
        }

        /// <summary>
        /// Seeks the current position of the audio stream to the specified time in seconds.
        /// </summary>
        /// <remarks>This method adjusts the position of the audio stream if the underlying wave source
        /// supports seeking. If the specified time exceeds the length of the audio stream, the position is set to the
        /// end of the stream.</remarks>
        /// <param name="seconds">The time position, in seconds, to seek to. Must be non-negative.</param>
        public void SeekTo(double seconds)
        {
            if (!_audioPlayback.CanSeek) return;

            _audioPlayback.SeekTo(seconds);

            double tolerance = 0.5;
            double currentSeconds = _audioPlayback.Position.TotalSeconds;
            double totalSeconds = _audioPlayback.Duration.TotalSeconds;

            if (currentSeconds <= tolerance)
            {
                TrackRatingManager.ApplyReplay(_currentTrackModel, _settings.TrackRating);
            }
            else if (totalSeconds - currentSeconds <= tolerance)
            {
                _userSeeked = true;
                TrackRatingManager.ApplySeekToEnd(_currentTrackModel, _settings.TrackRating);
            }
        }

        /// <summary>
        /// Updates the position of the trackbar to reflect the current playback position of the audio source.
        /// </summary>
        /// <remarks>This method calculates the current playback time in seconds from the audio source's
        /// position and updates the trackbar slider accordingly. Ensure that the audio source is not null before
        /// calling this method.</remarks>
        private void UpdateTrackbar()
        {
            ApplicationStateService.Instance.SetTrackingSliderValue?.Invoke((int)_audioPlayback.Position.TotalSeconds);
        }

        #endregion Core Playback Logic

        #region Playdata Methods

#nullable enable
        /// <summary>
        /// Resets track statistics in the database based on the specified mode and tag.
        /// </summary>
        /// <remarks>This method performs a reset operation on track statistics in the database. The
        /// behavior depends on the combination of the <paramref name="mode"/> and <paramref name="tag"/> parameters:
        /// <list type="bullet"> <item> <description>If <paramref name="mode"/> is "all" and <paramref name="tag"/> is
        /// "all", the entire database is reset, and all in-memory tracks are cleared.</description> </item> <item>
        /// <description>If <paramref name="mode"/> is "all" and <paramref name="tag"/> is a specific value, the reset
        /// is performed either in parallel or serially, depending on the number of tracks.</description> </item> <item>
        /// <description>If <paramref name="mode"/> is "selected", only the currently selected track is reset, provided
        /// it exists in the database.</description> </item> </list> If a <paramref name="dialog"/> is provided, it is
        /// used to display progress and handle cancellation. The operation respects the cancellation token associated
        /// with the dialog.</remarks>
        /// <param name="mode">Specifies the scope of the reset operation. Valid values are "all" to reset all tracks or "selected" to
        /// reset only the currently selected track.</param>
        /// <param name="tag">Specifies the tag to filter which statistics are reset. Use "all" to reset all tags.</param>
        /// <param name="dialog">An optional <see cref="ThemableProcessingDialog"/> instance to display progress and handle cancellation. If
        /// null, no dialog is shown.</param>
        /// <returns></returns>
        internal async Task ResetTrackStatsAdaptiveAsync(string mode, string tag, ThemableProcessingDialog? dialog = null)
        {

            tag = tag.ToLowerInvariant();
            mode = mode.ToLowerInvariant();
            var cancellationToken = dialog?.Token ?? CancellationToken.None;
            const int parallelThreshold = 200;

            try
            {
                TrackDatabase.BackUpDatabase();
                if (mode == "all" && tag == "all")
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        TrackDatabase.NukeDatabase();

                        foreach (var track in _playlistManager.Tracks)
                        {
                            TrackDatabase.ResetInMemory(track);
                        }
                    }

                    dialog?.Invoke(() =>
                    {
                        dialog.SetCompleted("Database reset complete.");
                        dialog.CloseAfter(1000);
                    });

                    return;
                }

                else if (mode == "all")
                {
                    int totalTracks = TrackDatabase.GetTrackCount();
                    if (totalTracks >= parallelThreshold)
                        await ResetTrackStatsParallelAsync(tag, dialog, cancellationToken);
                    else
                        await Task.Run(() => ResetTrackStatsSerial(tag, dialog, cancellationToken), cancellationToken);
                }
                else if (mode == "selected")
                {
                    Track? selectedTrack = null;
                    if (playListBox.InvokeRequired)
                    {
                        playListBox.Invoke(() =>
                        {
                            if (playListBox.SelectedIndex >= 0)
                                selectedTrack = (Track?)playListBox.SelectedItem;
                        });
                    }
                    else
                    {
                        if (playListBox.SelectedIndex >= 0)
                            selectedTrack = (Track?)playListBox.SelectedItem;
                    }

                    if (selectedTrack != null && TrackDatabase.TrackExists(selectedTrack.Guid))
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            LiteDbWriteQueue.Enqueue(() => TrackDatabase.ResetStats(selectedTrack, tag));
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                DebugUtils.Log("Stats Reset", "Canceled", oce.Message, logLevel: DebugUtils.LogLevel.Error);
            }

            await LiteDbWriteQueue.WaitForEmptyAsync();
            try
            {
                dialog?.Invoke(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        dialog.SetCompleted("Cancelled by user.");
                    else
                        dialog.SetCompleted("Track stats reset complete.");

                    dialog.CloseAfter(1000);
                });
            }
            catch (Exception ex) { DebugUtils.Log("Reset Stats", "Dialog Update", ex.Message, logLevel: DebugUtils.LogLevel.Error); }
        }

        /// <summary>
        /// Resets the statistics for all tracks in the database in parallel, using a specified tag to filter the reset
        /// operation.
        /// </summary>
        /// <remarks>This method processes tracks in parallel to improve performance, with a maximum
        /// concurrency level determined by the system's processor count. Progress updates are provided through the
        /// <paramref name="dialog"/> parameter, if specified. The operation supports cancellation via the <paramref
        /// name="cancellationToken"/> parameter.</remarks>
        /// <param name="tag">The tag used to identify the statistics to reset for each track.</param>
        /// <param name="dialog">An optional dialog used to display progress updates during the operation. Can be <see langword="null"/>.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. If cancellation is requested, the operation will terminate
        /// early.</param>
        /// <returns></returns>
        private static async Task ResetTrackStatsParallelAsync(string tag, ThemableProcessingDialog? dialog, CancellationToken cancellationToken)
        {
            var allTracks = TrackDatabase.GetAllTracks();
            int total = allTracks.Count;
            int progress = 0;
            var stopwatch = Stopwatch.StartNew();

            int processorCount = Environment.ProcessorCount;
            int maxConcurrency = Math.Min(processorCount * 2, processorCount < 8 ? 20 : 40);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var track in allTracks)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(() =>
                {
                    try
                    {
                        LiteDbWriteQueue.Enqueue(
                            () => TrackDatabase.ResetStats(track, tag),
                            () =>
                            {
                                int currentProgress = Interlocked.Increment(ref progress);

                                if (dialog != null && (currentProgress % _updateStep == 0 || currentProgress == total))
                                {
                                    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                                    double avgPerFile = elapsedSeconds / currentProgress;
                                    double etaSeconds = avgPerFile * (total - currentProgress);
                                    string lastEta = $"ETA: {TimeSpan.FromSeconds(etaSeconds):mm\\:ss}";

                                    dialog.Invoke(() =>
                                    {
                                        dialog.SetProgress("Track", currentProgress, total, lastEta);
                                    });
                                }
                            },
                            cancellationToken
                        );
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Resets the statistics for all tracks in the database, applying the specified tag to each reset operation.
        /// </summary>
        /// <remarks>This method processes all tracks in the database sequentially, resetting their
        /// statistics and optionally updating a progress dialog. The progress dialog, if provided, displays the current
        /// progress, total tracks, and an estimated time remaining (ETA). The operation respects the provided
        /// cancellation token, allowing the caller to cancel the process at any time.</remarks>
        /// <param name="tag">The tag to associate with the reset operation for each track.</param>
        /// <param name="dialog">An optional <see cref="ThemableProcessingDialog"/> instance used to display progress updates during the
        /// operation. If <see langword="null"/>, no progress updates will be shown.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation. If cancellation is requested,
        /// the operation will stop processing tracks.</param>
        private void ResetTrackStatsSerial(string tag, ThemableProcessingDialog? dialog, CancellationToken cancellationToken)
        {
            var allTracks = TrackDatabase.GetAllTracks();
            int total = allTracks.Count;
            int progress = 0;
            var stopwatch = Stopwatch.StartNew();

            foreach (var track in allTracks)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                LiteDbWriteQueue.Enqueue(() => TrackDatabase.ResetStats(track, tag), cancellationToken: cancellationToken);

                progress++;

                if (dialog != null && (progress % _updateStep == 0 || progress == total))
                {
                    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                    double avgPerFile = elapsedSeconds / progress;
                    double etaSeconds = avgPerFile * (total - progress);
                    string lastEta = $"ETA: {TimeSpan.FromSeconds(etaSeconds):mm\\:ss}";

                    dialog.Invoke(() =>
                    {
                        dialog.SetProgress("Track", progress, total, lastEta);
                    });
                }
            }
        }

        /// <summary>
        /// Determines whether a track skip should be counted as a valid skip event.
        /// </summary>
        /// <remarks>
        /// A skip is considered valid if it occurs after the first 2 seconds of playback (to avoid counting accidental skips at the start)
        /// and before the last 10 seconds of the track (to avoid counting skips near the end as valid).
        /// </remarks>
        /// <param name="position">The current playback position within the track.</param>
        /// <param name="totalDuration">The total duration of the track.</param>
        /// <returns><see langword="true"/> if the skip is valid and should be counted; otherwise, <see langword="false"/>.</returns>
        private static bool IsSkipValid(TimeSpan position, TimeSpan totalDuration)
        {
            var remaining = totalDuration - position;

            if (position < TimeSpan.FromSeconds(_leadInImmunity))
                return false;
            if (remaining <= TimeSpan.FromSeconds(_leadOutImmunity))
                return false;

            return true;
        }

        /// <summary>
        /// Opens a dialog to select a legacy LiteDB track database file and imports its tracks into the current application database.
        /// </summary>
        /// <remarks>
        /// Displays a processing dialog during the import operation. If the user cancels the operation, a cancellation message is shown.
        /// Upon completion, the dialog reports the number of tracks imported and closes automatically after a short delay.
        /// </remarks>
        internal async void RunImport()
        {
            string legacyDbPath = Files.ChooseFile("Select Legacy Track Database", "LiteDB files (*.db)|*.db|All files (*.*)|*.*");
            if (string.IsNullOrWhiteSpace(legacyDbPath)) return;

            var dialog = new ThemableProcessingDialog("Importing tracks...");
            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Icon = Icon;
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );

            dialog.Show(this);

            try
            {
                int imported = await TrackDatabase.ImportTracksAsync(
                    legacyDbPath,
                    dialog,
                    dialog.Token
                );

                dialog.SetCompleted($"Import finished. Imported {imported} track(s).");
            }
            catch (OperationCanceledException)
            {
                dialog.SetCompleted("Import canceled. You can resume later.");
            }

            dialog.CloseAfter(2000);
        }

        /// <summary>
        /// Updates the rating of a track based on the specified mode.
        /// </summary>
        /// <remarks>If the rating is successfully updated, the changes are saved to the database. If an
        /// error occurs while saving, the error is logged, but the method does not throw an exception.</remarks>
        /// <param name="item">The name or identifier of the track to be rated. If <paramref name="track"/> is not provided, this value is
        /// used to retrieve the track.</param>
        /// <param name="mode">The rating mode to apply. Valid values are: <list type="bullet"> <item><term>"like"</term> - Marks the track
        /// as liked.</item> <item><term>"dislike"</term> - Marks the track as disliked.</item>
        /// <item><term>"neutral"</term> - Removes any like or dislike rating from the track.</item> </list> The
        /// comparison is case-insensitive.</param>
        /// <param name="track">The <see cref="Track"/> object to be rated. If null, the track is retrieved using <paramref name="item"/>.</param>
        /// <returns><see langword="true"/> if the track's rating was changed; otherwise, <see langword="false"/>.</returns>
        private bool UserRateTrack(string mode, Track? track = null, int index = -1)
        {
            track ??= _playlistManager.Get(index);

            if (track == null)
                return false;

            bool changed = false;
            switch (mode?.ToLowerInvariant())
            {
                case "like":
                    if (!track.Liked)
                    {
                        TrackRatingManager.ApplyManualLike(track, _settings.TrackRating);
                        changed = true;
                    }
                    break;
                case "dislike":
                    if (!track.Disliked)
                    {
                        TrackRatingManager.ApplyManualDislike(track, _settings.TrackRating);
                        changed = true;
                    }
                    break;
                case "neutral":
                    TrackRatingManager.ApplyNeutral(track, _settings.TrackRating);
                    changed = true;
                    break;
                default:
                    return false;
            }

            if (changed)
            {
                //Save stats to database
                try
                {
                    TrackDatabase.SaveStats(track);
                }
                catch (Exception ex) { DebugUtils.Log("Like - Dislike", "Save Stats", $"Saving to data base error: {ex.Message}", logLevel: DebugUtils.LogLevel.Error); }
            }
            return changed;
        }

        #endregion Playdata Methods

        #region Device Management

        /// <summary>
        /// Populates the audio device dropdown with available devices.
        /// </summary>
        private void PopulateAudioDeviceList()
        {
            AudioDeviceList.Items.Clear();
            AudioDeviceList.Items.AddRange(_audioDevices.Devices.ToArray());
        }

        /// <summary>
        /// Handles audio device changes from the service.
        /// </summary>
        private void OnAudioDevicesChanged(object? sender, DevicesChangedEventArgs e)
        {
            AudioDeviceList.Items.Clear();
            AudioDeviceList.Items.AddRange(e.Devices.ToArray());

            if (e.SuggestedIndex >= 0 && e.SuggestedIndex < AudioDeviceList.Items.Count)
            {
                AudioDeviceList.SelectedIndex = e.SuggestedIndex;

                var device = _audioDevices.GetDevice(e.SuggestedIndex);
                if (device != null)
                {
                    ChangeAudioDevice(device.DeviceID, false);
                }
            }

            BazthalLib.DebugUtils.Log("AudioDeviceService", "DevicesChanged",
                $"Device list updated. Selected index: {e.SuggestedIndex}",
                logLevel: BazthalLib.DebugUtils.LogLevel.Info);
        }

        /// <summary>
        /// Processes Windows messages and refreshes the list of audio devices when a device change message is received.
        /// </summary>
        /// <remarks>Overrides the default message processing to handle device change notifications. When
        /// a WM_DEVICECHANGE message is received, the method updates the audio device list to reflect any changes. For
        /// all other messages, the base implementation is called.</remarks>
        /// <param name="m">A reference to the Windows message to process.</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_DEVICECHANGE = 0x0219;

            if (m.Msg == WM_DEVICECHANGE)
            {
                _audioDevices.RefreshDevices();
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Changes the audio output device to the specified device ID.
        /// </summary>
        /// <remarks>This method stops the current audio output, disposes of the existing audio resources,
        /// and initializes the audio system with the new device. The playback position is preserved across the device
        /// change.</remarks>
        /// <param name="deviceId">The identifier of the audio device to switch to. Cannot be null or empty.</param>
        private void ChangeAudioDevice(string deviceId, bool userChosen = false)
        {
            // Don't go through the process of setting up new device if there is no stream
            if (_audioPlayback.IsDisposed) { return; }

            var wasPlaying = _audioPlayback.State == PlaybackState.Playing;
            var model = _currentTrackModel;

            if (userChosen)
            {
                int deviceIndex = _audioDevices.FindDeviceIndex(deviceId);
                if (deviceIndex >= 0)
                {
                    _audioDevices.MarkDeviceAsUserSelected(deviceIndex);
                }
            }

            if (_audioPlayback.ChangeDevice(deviceId, true, _currentTrackFilePath))
            {
                _currentTrackModel = model;

                if (wasPlaying)
                {
                    StartPlaybackTimer();
                }
            }
        }

        #endregion Device Management

       #region Helper Methods

        /// <summary>
        /// Resets the user interface text to its default state.
        /// </summary>
        /// <remarks>Sets the main window title to "MP3 Player" and clears the current track
        /// label.</remarks>
        private static void ResetUI()
        {
            var appState = ApplicationStateService.Instance;
            appState.SetFormTitle?.Invoke("MP3 Player");
            appState.SetCurrentTrackLabel?.Invoke(string.Empty);
            appState.SetTrackingSliderValue?.Invoke(0);
        }

        /// <summary>
        /// Handles the processing of dropped files, identifying valid audio files and playlists,  and optionally
        /// initiating playback.
        /// </summary>
        /// <remarks>This method processes the provided file paths to identify valid audio files and
        /// playlists.  If a directory is provided, it searches for audio files within the directory.  Supported file
        /// types are determined by the <c>SupportedAudioExtensions</c> and <c>SupportedPlaylistExtensions</c>
        /// collections.  If a playlist file is encountered and no audio files have been added yet, the playlist is
        /// loaded directly. If valid audio files are found, they are added to the current playlist, and playback may be
        /// initiated  depending on the value of <paramref name="autoPlay"/>.</remarks>
        /// <param name="files">An array of file paths to process. Each path can represent a file or a directory.</param>
        /// <param name="autoPlay">A boolean value indicating whether playback should automatically start after processing. If <see
        /// langword="true"/>, playback begins immediately after adding the first valid track or playlist.</param>
        internal void HandleDroppedFiles(string[] files, bool autoPlay = false)
        {
            List<string> validAudioFiles = new();

            foreach (var path in files)
            {
                if (Directory.Exists(path))
                {
                    validAudioFiles.AddRange(FindAudioFile(path));
                }
                else if (File.Exists(path))
                {
                    string ext = Path.GetExtension(path).ToLowerInvariant();

                    if (SupportedAudioExtensions.Contains(ext))
                    {
                        validAudioFiles.Add(path);
                    }
                    else if (SupportedPlaylistExtensions.Contains(ext))
                    {
                        if (validAudioFiles.Count <= 0)
                        {
                            if (autoPlay)
                                AttachAutoPlayHandler(firstTrack: true);

                            LoadPlaylist(path, false);
                            return;
                        }
                    }
                }
            }

            if (validAudioFiles.Count > 0)
            {
                if (autoPlay)
                    AttachAutoPlayHandler(firstTrack: false);

                AddItem(validAudioFiles.ToArray());
            }
        }

        /// <summary>
        /// Attaches an event handler to automatically play a track when the playlist changes.
        /// </summary>
        /// <remarks>This method subscribes to the <see cref="PlaylistManager.PlaylistChanged"/> event and
        /// ensures that a track is automatically selected and played when the playlist is updated. The handler is
        /// removed after it is invoked to prevent repeated execution.</remarks>
        /// <param name="firstTrack">A value indicating whether the first track in the playlist should be selected and played. If <see
        /// langword="true"/>, the first track is selected; otherwise, the last track is selected.</param>
        private void AttachAutoPlayHandler(bool firstTrack)
        {
            var appState = ApplicationStateService.Instance;

            void handler()
            {
                _playlistManager.PlaylistChanged -= handler;

                BeginInvoke(new Action(() =>
                {
                    var count = appState.GetPlaylistCount?.Invoke() ?? 0;
                    if (count > 0)
                    {
                        appState.SetSelectedTrackIndex?.Invoke(firstTrack ? 0 : count - 1);
                        Play();
                    }
                }));
            }
            _playlistManager.PlaylistChanged += handler;
        }

        /// <summary>
        /// Searches for audio files within the specified folder and its subdirectories.
        /// </summary>
        /// <remarks>The method searches recursively through all subdirectories of the specified folder.
        /// Only files with extensions that are included in the <c>SupportedAudioExtensions</c> collection are
        /// considered audio files.</remarks>
        /// <param name="folderPath">The path to the folder where the search for audio files will be conducted. Must be a valid directory path.</param>
        /// <returns>A list of file paths representing the audio files found. Returns an empty list if no audio files are found
        /// or if the directory does not exist.</returns>
        private static List<string> FindAudioFile(string folderPath)
        {
            var list = new List<string>();

            try
            {
                if (!Directory.Exists(folderPath))
                    return list;

                foreach (var file in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories))
                {
                    if (SupportedAudioExtensions.Contains(Path.GetExtension(file)))
                    {
                        list.Add(file);
                    }
                }
            }
            catch (Exception ex) { DebugUtils.Log("Drop Folder", "Recursive Search", $"Error scanning folder '{folderPath}': {ex.Message}", logLevel: DebugUtils.LogLevel.Error); }

            return list;
        }

        /// <summary>
        /// Performs a garbage collection operation if the specified number of items added exceeds a threshold.
        /// </summary>
        /// <remarks>This method initiates an aggressive garbage collection process on a background thread
        /// to reclaim memory. The operation is performed asynchronously and logs the amount of memory released after
        /// the collection.</remarks>
        /// <param name="addedCount">The number of items added. If this value is greater than 100, a garbage collection operation is triggered.</param>
        internal void CleanupIfNeeded(int addedCount)
        {
            if (addedCount > 100)
            {
                long before = GC.GetTotalMemory(forceFullCollection: false);

                Task.Run(() =>
                {
                    GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);

                    long after = GC.GetTotalMemory(forceFullCollection: true);
                    DebugUtils.Log("Garbage Collection", "Cleanup", $"Released {(before - after) / 1024 / 1024} MB after adding {addedCount} tracks", logLevel: DebugUtils.LogLevel.Info);
                });
            }
        }

        #endregion Helper Methods

        #region Playlist Management

        /// <summary>
        /// Shuffles the current playlist and restores the original track selection.
        /// </summary>
        /// <remarks>This method randomizes the order of tracks in the playlist while ensuring that the
        /// currently selected track remains selected after the shuffle. It is useful for maintaining the user's current
        /// listening position in a shuffled playlist.</remarks>
        private void ShufflePlaylist()
        {
            var shuffled = _playlistOps.ShuffleTracks(_playlistManager.Tracks);
            _playlistManager.Clear();
            _playlistManager.AddRange(shuffled);

            // Restore original selection
            int index = 0;
            if (_currentTrackModel != null)
            {
                index = _playlistManager.IndexOf(_currentTrackModel);
            }
            playListBox.SelectedIndex = index;
        }

        /// <summary>
        /// Sorts the playlist based on a specified field and order.
        /// </summary>
        /// <remarks>This method clears the current playlist and repopulates it with the sorted tracks. It
        /// also updates the UI list box to reflect the new order.</remarks>
        /// <param name="sortField">The field to sort by (Artist, Title, Album, PlayCount, LastPlayed, Liked, Disliked, Rating).</param>
        /// <param name="descending">A boolean value indicating whether the sorting should be in descending order. Defaults to <see
        /// langword="false"/> for ascending order.</param>
        private void SortPlaylist(string sortField, bool descending = false)
        {
            var sorted = _playlistOps.SortTracks(_playlistManager.Tracks, sortField, descending);

            if (sorted == null)
            {
                DebugUtils.Log("Sort Playlist", "SortPlaylist",
                    $"Invalid sort field: {sortField}", logLevel: DebugUtils.LogLevel.Warning);
                return;
            }

            _playlistManager.Clear();
            _playlistManager.AddRange(sorted);
        }

#nullable enable
        /// <summary>
        /// Adds one or more audio tracks to the playlist, either from the specified files or by prompting the user to
        /// select files.
        /// </summary>
        /// <remarks>This method displays a progress dialog while processing the files and updates the
        /// playlist upon completion.  If the operation is cancelled or an error occurs, the dialog will display an
        /// appropriate message.</remarks>
        /// <param name="droppedItems">An optional array of file paths representing the audio files to add. If <see langword="null"/> or empty, the
        /// user will be prompted to select files.</param>
        private async void AddItem(string[]? droppedItems = null)
        {
            var appState = ApplicationStateService.Instance;

            string[]? files = droppedItems;
            if (files == null || files.Length == 0)
            {
                files = Files.ChooseFiles("", "Music Files|*.flac;*.m4a;*.mp2;*.mp3;*.wav;*.wma");
                if (files == null || files.Length == 0) return;
            }

            var dialog = new ThemableProcessingDialog("Adding tracks...") { StartPosition = FormStartPosition.Manual };
            dialog.Icon = Icon;
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );

            dialog.Show(this);

            try
            {
                var newTracks = await _playlistManager.ProcessFilesAsync(
                    files,
                    (current, total, eta) => dialog.SetProgress("Track", current, total, eta),
                    replacePlaylist: false,
                    dialog.Token
                );

                dialog.SetCompleted($"Added {newTracks.Count} track(s).");
                dialog.CloseAfter(1000);

                var playlistCount = appState.GetPlaylistCount?.Invoke() ?? 0;
                var selectedIndex = appState.GetSelectedTrackIndex?.Invoke() ?? -1;

                if (playlistCount > 0 && selectedIndex >= 0)
                    appState.SetSelectedTrackIndex?.Invoke(playlistCount - 1);

                CleanupIfNeeded(newTracks.Count);
            }
            catch (OperationCanceledException)
            {
                dialog.SetCompleted("Operation cancelled.");
                dialog.CloseAfter(1000);
            }
            catch (Exception ex)
            {
                dialog.SetCompleted($"Error: {ex.Message}");
                dialog.CloseAfter(2000);
            }
        }

#nullable disable
        /// <summary>
        /// Removes the currently selected item from the playlist.
        /// </summary>
        /// <remarks>If the selected item is currently being played, the method will stop playback and
        /// reset the UI text.</remarks>
        private void RemoveItem()
        {
            int index = playListBox.SelectedIndex;
            if (index >= 0)
            {
                if (_currentTrackModel != null && _currentTrackModel.Equals(_playlistManager.Get(index)))
                // if (!string.IsNullOrWhiteSpace(Cur_Track_Label.Text) && Cur_Track_Label.Text.Contains(playListBox.SelectedItem.ToString()))
                {
                    _audioPlayback.Dispose();
                    ResetUI();
                }
                _playlistManager.RemoveAt(index);
            }
        }

        /// <summary>
        /// Saves the current playlist to a file in M3U format.
        /// </summary>
        /// <remarks>This method displays a file save dialog to select the destination file path. If a
        /// valid file path is provided, the playlist is saved asynchronously. A processing dialog is shown during the
        /// save operation and closes upon completion.</remarks>
        private async void SavePlaylist(List<Track> trackList = null)
        {
            string saveFileName = Files.SaveFile("", _defaultPlaylistExtentionFilter, "Save Playlist", true);
            if (string.IsNullOrWhiteSpace(saveFileName)) return;

            var dialog = new ThemableProcessingDialog("Saving Playlist...", showProgress: true, showCancelButton: false) { StartPosition = FormStartPosition.Manual };
            dialog.Icon = Icon;
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );

            dialog.Show(this);
            var ext = Path.GetExtension(saveFileName).ToLowerInvariant();
            DebugUtils.Log("Save Playlist", "File Extension", $"Saving playlist as '{ext}'", logLevel: DebugUtils.LogLevel.Info);
            switch (ext)
            {
                case ".jsonpl":
                    await Task.Run(() => _playlistManager.SaveToJsonPl(saveFileName, trackList));
                    break;
                case ".m3u":
                    try
                    {
                        await Task.Run(() => _playlistManager.SaveToM3U(saveFileName, trackList));
                    }
                    catch (Exception ex)
                    {
                        dialog.SetCompleted($"Error: {ex.Message}");
                        dialog.CloseAfter(2000);
                        return;
                    }
                    break;

            }

            //await Task.Run(() => _playlistManager.SaveToM3U(saveFileName, trackList));

            dialog.SetCompleted("Playlist saved successfully.");
            dialog.CloseAfter(1000);
        }

        /// <summary>
        /// Loads a playlist from the specified file or prompts the user to select a playlist file.
        /// </summary>
        /// <remarks>This method displays a progress dialog while the playlist is being loaded. If the
        /// operation is cancelled, the dialog will indicate the cancellation. The method ensures that a valid playlist
        /// is loaded and updates the playlist display accordingly.</remarks>
        /// <param name="droppedItem">The path to the playlist file to load. If <see langword="null"/> or empty, the user will be prompted to
        /// select a file.</param>
        /// <param name="keepSelection">A value indicating whether to retain the currently selected track in the playlist, if it exists. If <see
        /// langword="true"/>, the selection will be preserved if the track is found in the new playlist; otherwise, the
        /// first track will be selected.</param>
        private async void LoadPlaylist(string droppedItem = null, bool keepSelection = true)
        {
            var appState = ApplicationStateService.Instance;

            string loadFileName = droppedItem;
            if (loadFileName == null || loadFileName.Length == 0)
            {
                loadFileName = Files.ChooseFile("", _defaultPlaylistExtentionFilter, "Open Playlist");
                if (string.IsNullOrWhiteSpace(loadFileName)) return;
            }

            var dialog = new ThemableProcessingDialog("Loading Playlist") { StartPosition = FormStartPosition.Manual };
            dialog.Icon = Icon;
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );
            dialog.Show(this);

            var ext = Path.GetExtension(loadFileName).ToLowerInvariant();
            switch (ext)
            {
                case ".jsonpl":
                    await Task.Run(() => _playlistManager.LoadFromJsonPl(loadFileName));
                    break;
                case ".m3u":
                    try
                    {
                        await Task.Run(() => _playlistManager.LoadFromM3U(loadFileName, (current, total, eta) =>
                        {
                            dialog.Invoke(() => dialog.SetProgress("Track", current, total, eta));
                        }, dialog.Token));
                    }
                    catch (OperationCanceledException)
                    {
                        dialog.SetCompleted("Cancelled by user.");
                    }
                    finally
                    {
                        dialog.CloseAfter(1000);
                    }
                    break;
            }



            string lastItem = null;
            if (_currentTrackModel != null)
            {
                lastItem = _currentTrackModel.ToString();
            }

            var playlistCount = appState.GetPlaylistCount?.Invoke() ?? 0;
            if (playlistCount > 0)
            {
                if (lastItem != null && keepSelection)
                {
                    bool found = false;
                    for (int i = 0; i < playlistCount; i++)
                    {
                        var item = playListBox.Items[i];
                        if (item.ToString() == lastItem)
                        {
                            appState.SetSelectedTrackIndex?.Invoke(i);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        appState.SetSelectedTrackIndex?.Invoke(0);
                    }
                }
                else
                {
                    appState.SetSelectedTrackIndex?.Invoke(0);
                }
            }

            _settings.Application.LastPlaylistOpened = loadFileName;
            dialog.SetCompleted("Playlist loaded.");
            dialog.CloseAfter(1000);
        }

        /// <summary>
        /// Generates a playlist based on the specified smart shuffle mode and a collection of tracks.
        /// </summary>
        /// <remarks>The method applies the specified shuffle mode to filter and sort the provided tracks.
        /// If no tracks match the criteria for the selected mode, a message box is displayed to inform the user,  and
        /// no playlist is generated. Otherwise, the user is prompted to choose an action: load the playlist,  save it
        /// to a file, or close without taking action.</remarks>
        /// <param name="mode">The <see cref="SmartShuffleMode"/> to use for filtering and ordering the playlist.  Determines the criteria
        /// for selecting tracks (e.g., un-played tracks, most played tracks, etc.).</param>
        /// <param name="tracks">A collection of <see cref="Track"/> objects to be considered for playlist generation.  The collection must
        /// not be empty.</param>
        private void GeneratePlayList(SmartShuffleMode mode, IEnumerable<Track> tracks)
        {
            var trackList = tracks.ToList();
            if (trackList.Count <= 0) return;

            string modeText = mode.ToString().Replace("_", " ");
            var filteredList = _playlistOps.GenerateSmartPlaylist(trackList, mode);

            if (filteredList.Count <= 0)
            {
                ThemableMessageBox.Show($"No tracks match the chosen filter: {{{modeText}}}",
                    "Generation Failed", MessageBoxButtons.OK, autoCloseMilliseconds: 3000, MessageBoxIcon.Asterisk);
                return;
            }

            var total = _playlistOps.CalculateTotalDuration(filteredList);
            string durationText = $"{(int)total.TotalHours:D2}:{total.Minutes:D2}:{total.Seconds:D2}";

            string trackWord = filteredList.Count == 1 ? "track" : "tracks";
            string msg =
                $"Generated a playlist with {filteredList.Count} {modeText} {trackWord}. \n" +
                $"Duration: {durationText}\n\n" +
                "Choose an action:";

            var result = ThemableMessageBox.Show($"{msg}", "Playlist Generated",
                [("&Load", DialogResult.OK), ("&Save", DialogResult.Continue), ("&Close", DialogResult.Cancel)]);

            switch (result)
            {
                case DialogResult.OK:
                    _playlistManager.Clear();
                    _playlistManager.AddRange(filteredList);
                    break;
                case DialogResult.Continue:
                    SavePlaylist(filteredList);
                    break;
                case DialogResult.Cancel:
                    break;
            }
        }

        #endregion Playlist Management

        #region Configuration

        /// <summary>
        /// Opens the settings window, allowing the user to modify application settings.
        /// </summary>
        /// <remarks>If the user confirms the changes, the application updates its WebSocket configuration
        /// and other settings accordingly.</remarks>
        private void OpenSettingsWindow()
        {
            using var settingsForm = new SettingsForm(_settings);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _trackNavigation.MaxHistorySize = Math.Min(_settings.Playback.MaxTrackHistory, 9999);

                bool parsed = Enum.TryParse(_settings.SmartShuffle.Mode, out SmartShuffleMode mode);
                if (parsed) _smartShuffleMode = mode;

                _includeData = _settings.CommandBehaviour.IncludeTracksInCount;

                //Update WebSocket Info
                //_webSocketAddress = _settings.WebSocket.Address;
                //_webSocketPort = _settings.WebSocket.Port;
                //_webSocketEndPoint = _settings.WebSocket.EndPoint;
                //_autoStart = _settings.WebSocket.AutoStart;
                SaveSettings();
            }
        }

        /// <summary>
        /// Loads a custom theme configuration from a JSON file and applies it to the application.
        /// </summary>
        /// <remarks>This method checks for the existence of a custom theme configuration file. If the
        /// file is found, it loads the theme settings from the JSON file and applies them as the custom theme. The
        /// method then sets the application's theme to the custom theme.</remarks>
        private void LoadThemeFromJson()
        {
            if (File.Exists(_customThemeConfig))
            {
                var config = new JSON<ThemeColors>(_customThemeConfig, [new BazthalLib.Extensibility.Serialization.ThemeColorsJsonConverter()]);
                config.Load();

                var theme = config.Data;

                var custom = new ThemeColors
                {
                    AccentColor = theme.AccentColor,
                    BackColor = theme.BackColor,
                    BorderColor = theme.BorderColor,
                    DisabledColor = theme.DisabledColor,
                    ForeColor = theme.ForeColor,
                    SelectedItemBackColor = theme.SelectedItemBackColor,
                    SelectedItemForeColor = theme.SelectedItemForeColor
                };
                Theming.SetCustomTheme(custom);
            }
            Theming.SetTheme(Theming.AppTheme.Custom);

        }

        /// <summary>
        /// Loads and applies the application settings from the configuration source.
        /// </summary>
        /// <remarks>This method initializes various application settings, including playback, websocket,
        /// track rating, and smart shuffle configurations. It ensures that the settings are applied to the relevant
        /// components and prepares the application for operation based on the user's preferences.</remarks>
        private void LoadSettings()
        {
            //Playback Settings
            _volumeLevel = _settings.Playback.VolumeLvl;

            // Find and select the saved audio device
            int deviceIndex = _audioDevices.FindDeviceByName(_settings.Playback.AudioDevice);
            if (deviceIndex >= 0)
            {
                AudioDeviceList.SelectedIndex = deviceIndex;
            }

            playList_Options.SelectedItem = _settings.Playback.PlayListMode;
            _trackNavigation.MaxHistorySize = Math.Min(_settings.Playback.MaxTrackHistory, 9999);

            //Websocket Settings
            //_webSocketAddress = _settings.WebSocket.Address;
            //_webSocketPort = _settings.WebSocket.Port;
            //_webSocketEndPoint = _settings.WebSocket.EndPoint;
            //_autoStart = _settings.WebSocket.AutoStart;

            //Track Rating Settings
            _leadInImmunity = _settings.TrackRating.LeadInImmunitySeconds;
            _leadOutImmunity = _settings.TrackRating.LeadOutImmunitySeconds;

            _includeData = _settings.CommandBehaviour.IncludeTracksInCount;
            //Smart Shuffle Settings

            bool parsed = Enum.TryParse(_settings.SmartShuffle.Mode, out SmartShuffleMode mode);
            if (parsed)
            {
                _smartShuffleMode = mode;
                ApplicationStateService.Instance.SmartShuffleMode = mode;
            }

            _defaultPlaylistExtentionFilter = _settings.Application.DefaultPlaylistExtension == ".jsonpl"
            ? "jsonPlaylist|*.jsonpl|M3U Playlists|*.m3u;*.m3u8|All Supported|*.m3u;*.m3u8;*.jsonpl"
            : "M3U Playlists|*.m3u;*.m3u8|jsonPlaylist|*.jsonpl|All Supported|*.m3u;*.m3u8;*.jsonpl"; ;
            //New settings to be added here

            SetVolume(_volumeLevel);
        }

        /// <summary>
        /// Saves the current application settings to persistent storage.
        /// </summary>
        /// <remarks>This method updates the playback, WebSocket, and track rating settings based on the
        /// current application state and persists them using the <see cref="ConfigManager.Save"/> method. It ensures
        /// that default selections are made for audio devices and playlist options if none are currently
        /// selected.</remarks>
        private void SaveSettings()
        {
            //Playback Settings
            _settings.Playback.VolumeLvl = _volumeLevel;

            if (AudioDeviceList.SelectedIndex == -1 && AudioDeviceList.Items.Count > 0)
                AudioDeviceList.SelectedIndex = 0;
            _settings.Playback.AudioDevice = AudioDeviceList.SelectedItem?.ToString() ?? string.Empty;

            if (playList_Options.SelectedIndex == -1 && playList_Options.Items.Count > 0)
                playList_Options.SelectedIndex = 0;
            _settings.Playback.PlayListMode = playList_Options.SelectedItem?.ToString() ?? string.Empty;

            _settings.Playback.MaxTrackHistory = _trackNavigation.MaxHistorySize;

            ////WebSocket Settings
            //_settings.WebSocket.Address = _webSocketAddress;
            //_settings.WebSocket.Port = _webSocketPort;
            //_settings.WebSocket.EndPoint = _webSocketEndPoint;
            //_settings.WebSocket.AutoStart = _autoStart;

            //Track Rating Settings
            _settings.TrackRating.LeadInImmunitySeconds = _leadInImmunity;
            _settings.TrackRating.LeadOutImmunitySeconds = _leadOutImmunity;

            _settings.Application.PlayerSize = this.Size;

            _settings.Application.PlayerLocation = this.Location;
            // New Settings Here

            ConfigManager.Save();

        }

        #endregion Configuration

        #region UI Event Handlers

        #region ContextMenu

        /// <summary>
        /// Updates the state of the rating menu items in the specified context menu based on the rating status of the
        /// provided track.
        /// </summary>
        /// <remarks>This method ensures that only one rating menu item is checked at a time, reflecting
        /// the current rating of the track. If the track is liked, the "Like" menu item is checked. If the track is
        /// disliked, the "Dislike" menu item is checked. If the track has a neutral rating, the "Neutral" menu item is
        /// checked.</remarks>
        /// <param name="menu">The <see cref="ContextMenuStrip"/> containing the rating menu items to update.</param>
        /// <param name="track">The <see cref="Track"/> whose rating status determines the menu item states.  Cannot be <see
        /// langword="null"/>.</param>
        private void UpdateRatingMenuItems(ContextMenuStrip menu, Track track)
        {
            if (menu == null || track == null) return;

            cms_Like.Checked = false;
            cms_Dislike.Checked = false;
            cms_Neutral.Checked = false;


            if (track.Liked)
                cms_Like.Checked = true;
            else if (track.Disliked)
                cms_Dislike.Checked = true;
            else
                cms_Neutral.Checked = true;
        }

        /// <summary>
        /// Handles the <see cref="ContextMenuStrip.Opening"/> event to prepare the context menu  for the currently
        /// selected playlist item.
        /// </summary>
        /// <remarks>This method updates the context menu items based on the currently selected track in
        /// the playlist. Ensure that a valid item is selected in the playlist before the context menu is
        /// opened.</remarks>
        /// <param name="sender">The source of the event, typically the context menu.</param>
        /// <param name="e">The event data, which can be used to cancel the opening of the context menu.</param>
        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (playListBox.SelectedItem == null) return;
            var track = (Track)playListBox.SelectedItem;
            UpdateRatingMenuItems(cms_Main, track);
        }

        #endregion ContextMenu

        #region Buttons

        /// <summary>
        /// Handles the click event of the Play button, initiating playback.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Play button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void PlayButton_Click(object sender, EventArgs e) { Play(); }

        /// <summary>
        /// Pauses the current operation when the pause button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event, typically the pause button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void PauseButton_Click(object sender, EventArgs e) { Pause(); }

        /// <summary>
        /// Handles the click event of the Stop button, triggering the stop operation.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Stop button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void StopButton_Click(object sender, EventArgs e) { Stop(); }

        /// <summary>
        /// Advances to the next track in the playlist.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void NextButton_Click(object sender, EventArgs e) { NextTrack(); }

        /// <summary>
        /// Handles the click event for the Previous button, navigating to the previous track.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Previous button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void PreviousButton_Click(object sender, EventArgs e) { PreviousTrack(); }

        /// <summary>
        /// Handles the click event for the Shuffle button, triggering the shuffling of the playlist.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Shuffle button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void ShuffleButton_Click(object sender, EventArgs e) { ShufflePlaylist(); }

        /// <summary>
        /// Handles the click event of the Add button, triggering the addition of a new item.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Add button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void AddButton_Click(object sender, EventArgs e) { AddItem(); }

        /// <summary>
        /// Handles the click event of the Remove button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void RemoveButton_Click(object sender, EventArgs e) { RemoveItem(); }

        /// <summary>
        /// Handles the click event of the Open Playlist button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OpenPlaylistButton_Click(object sender, EventArgs e) { LoadPlaylist(); }

        /// <summary>
        /// Handles the click event of the Save Playlist button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SavePlaylistButton_Click(object sender, EventArgs e) { SavePlaylist(); }

        /// <summary>
        /// Handles the click event of the settings button to open the settings window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OpenSettingButton_Click(object sender, EventArgs e) { OpenSettingsWindow(); }

        /// <summary>
        /// Handles the click event of the Clear Playlist button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClearPlaylistButton_Click(object sender, EventArgs e) { _audioPlayback.Dispose(); ResetUI(); _playlistManager.Clear(); }

        /// <summary>
        /// Handles the Click event of the Exit button, closing the current form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ExitButton_Click(object sender, EventArgs e) { this.Close(); }

        /// <summary>
        /// Handles the click event for the "Sort Playlist" menu button, sorting the playlist based on the selected
        /// criteria.
        /// </summary>
        /// <remarks>The method parses the tag of the clicked menu item to determine the sorting field and
        /// order. Supported fields include "Artist", "Title", "Album", "PlayCount", "LastPlayed", "Liked", "DisLiked",
        /// and "Rating". If the tag format is invalid or the field is unrecognized, the method logs an appropriate
        /// message.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="ToolStripMenuItem"/> with a <see cref="string"/> tag in
        /// the format "Field|Order", where "Field" specifies the sorting field (e.g., "Artist", "Title") and "Order"
        /// specifies the sort order ("true" for descending, "false" for ascending).</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void SortPlaylistMenuButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string tag)
            {
                var parts = tag.Split('|');
                if (parts.Length == 2)
                {
                    string sortField = parts[0];
                    bool descending = bool.TryParse(parts[1], out var desc) && desc;

                    DebugUtils.Log("Sort Playlist", Name, $"Sorting Playlist by: {sortField} | Descending: {descending}", logLevel: DebugUtils.LogLevel.Info);

                    SortPlaylist(sortField, descending);
                }
                else
                {
                    DebugUtils.Log("Sort Playlist", Name, $"Invalid Tag Format: {tag}", logLevel: DebugUtils.LogLevel.Warning);
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Reset" menu button, initiating a reset operation based on the menu item's
        /// tag.
        /// </summary>
        /// <remarks>The <paramref name="sender"/> must be a <see cref="ToolStripMenuItem"/> with a <see
        /// cref="ToolStripItem.Tag"/>  containing a string in the format "resetTag|mode". The method extracts the reset
        /// tag and mode from the tag,  displays a processing dialog, and performs the reset operation
        /// asynchronously.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="ToolStripMenuItem"/> with a valid <see
        /// cref="ToolStripItem.Tag"/>.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private async void ResetMenuButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string tag)
            {
                var parts = tag.Split('|');
                if (parts.Length != 2) return;

                string resetTag = parts[0];
                string mode = parts[1].ToLowerInvariant();

                var dialog = new ThemableProcessingDialog($"Resetting {resetTag}...") { StartPosition = FormStartPosition.Manual };
                dialog.Show(this);
                dialog.Icon = Icon;
                dialog.Location = new Point(
                    this.Location.X + (this.Width - dialog.Width) / 2,
                    this.Location.Y + (this.Height - dialog.Height) / 2
                );

                await ResetTrackStatsAdaptiveAsync(mode, resetTag, dialog);
            }
        }

        /// <summary>
        /// Handles the click event for the "Like" menu button.
        /// </summary>
        /// <remarks>This method triggers the rating of the currently selected track in the playlist as
        /// "liked." Ensure that <see cref="playListBox.SelectedIndex"/> is valid before invoking this method.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void LikeMenuButton_Click(object sender, EventArgs e) { UserRateTrack("like", (Track)playListBox.SelectedItem, playListBox.SelectedIndex); }

        /// <summary>
        /// Handles the click event for the Neutral menu button.
        /// </summary>
        /// <remarks>This method updates the user rating for the currently selected item in the playlist to "neutral."
        /// Ensure that <see cref="playListBox.SelectedIndex"/> is valid before invoking this method.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void NeutralMenuButton_Click(object sender, EventArgs e) { UserRateTrack("neutral", (Track)playListBox.SelectedItem, playListBox.SelectedIndex); }

        /// <summary>
        /// Handles the click event for the "Dislike" menu button.
        /// </summary>
        /// <remarks>This method updates the rating of the currently selected track in the playlist to
        /// "dislike." Ensure that a valid track is selected in the playlist before invoking this method.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void DislikeMenuButton_Click(object sender, EventArgs e) { UserRateTrack("dislike", (Track)playListBox.SelectedItem, playListBox.SelectedIndex); }

        /// <summary>
        /// Displays detailed information about the currently selected track in the playlist.
        /// </summary>
        /// <remarks>This method retrieves the currently selected track from the playlist and displays its
        /// details  in a message box. If no track is selected or the playlist is empty, the method exits without 
        /// performing any action.</remarks>
        /// <param name="sender">The source of the event. Typically the button that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void TrackInfo_Click(object sender, EventArgs e)
        {
            var appState = ApplicationStateService.Instance;
            var playlistCount = appState.GetPlaylistCount?.Invoke() ?? 0;
            var selectedIndex = appState.GetSelectedTrackIndex?.Invoke() ?? -1;

            if (playlistCount == 0 || selectedIndex == -1) return;

            var curTrack = (Track)playListBox.SelectedItem;

            if (curTrack == null) return;

            TrackDatabase.LoadStats(curTrack);

            WinForms.OpenForm<TrackInformationDialog>(
                action: form => form.PopulateTrackInfo(curTrack),
                args: curTrack,
                centerToParent: true,
                allowMultiple: true,
                maxInstances: 3,
                reuseMode: FormReuseMode.Rotate);
        }

        /// <summary>
        /// Handles the click event for generating a playlist based on the selected shuffle mode.
        /// </summary>
        /// <remarks>The method retrieves the shuffle mode from the <see cref="ToolStripMenuItem.Tag"/>
        /// property, parses it into a <see cref="SmartShuffleMode"/>, and generates a playlist using the specified mode
        /// and the available tracks.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="ToolStripMenuItem"/> with a <see cref="string"/> value
        /// in its <see cref="ToolStripItem.Tag"/> property.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void GeneratePlaylist_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string tag)
            {
                _ = Enum.TryParse(tag, out SmartShuffleMode mode);
                GeneratePlayList(mode, _playlistManager.Tracks);
            }
        }

        /// <summary>
        /// Displays the total duration of the current playlist in a message box.
        /// </summary>
        /// <remarks>If the playlist is empty, a message box indicating this is displayed instead. The
        /// total duration is calculated as the sum of the durations of all tracks in the playlist and is formatted as
        /// "HH:mm:ss".</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data associated with the triggering event.</param>
        private void ShowPlaylistDuration_Click(object sender, EventArgs e)
        {
            var playlist = _playlistManager.Tracks;
            if (!playlist.Any())
            {
                ThemableMessageBox.Show("The playlist is empty.", "Duration", MessageBoxButtons.OK, autoCloseMilliseconds: 10000);
                return;
            }

            var total = _playlistOps.CalculateTotalDuration(playlist);
            string durationText = $"{(int)total.TotalHours:D2}:{total.Minutes:D2}:{total.Seconds:D2}";

            ThemableMessageBox.Show($"Current playlist has {playlist.Count} track(s).\nTotal duration: {durationText}",
                "Playlist Duration", MessageBoxButtons.OK, autoCloseMilliseconds: 10000);
        }

        #endregion Buttons

        #region Sliders 

        /// <summary>
        /// Handles the completion of a volume slider scroll event.
        /// </summary>
        /// <remarks>This method updates the internal volume level based on the slider's value, applies
        /// the new volume setting, and saves the updated settings. It also logs the current volume level for debugging
        /// purposes.</remarks>
        /// <param name="sender">The source of the event, typically the volume slider control.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
        private void Volume_ScrollCompleted(object sender, EventArgs e)
        {
            _volumeLevel = Volume_Slider.Value;
            SetVolume(_volumeLevel);
            SaveSettings();
            DebugUtils.Log("Volume Setter", this.AccessibleName, $"Volume Level {Volume_Slider.Value / 100f}", logLevel: DebugUtils.LogLevel.Info);
        }

        /// <summary>
        /// Handles the completion of a scroll event on the tracking slider.
        /// </summary>
        /// <remarks>This method is triggered when the user finishes scrolling the slider, and it seeks to
        /// the position indicated by the current value of the slider.</remarks>
        /// <param name="sender">The source of the event, typically the tracking slider control.</param>
        /// <param name="e">The event data associated with the scroll completion.</param>
        private void Tracking_Slider_ScrollCompleted(object sender, EventArgs e)
        {
            SeekTo(Tracking_Slider.Value);
        }

        #endregion Sliders

        #region Listbox

        /// <summary>
        /// Handles the double-click event on the playlist to start playback.
        /// </summary>
        /// <remarks>This method is triggered when a user double-clicks an item in the playlist,
        /// initiating the playback of the selected media item.</remarks>
        /// <param name="sender">The source of the event, typically the playlist control.</param>
        /// <param name="e">The event data associated with the double-click action.</param>
        private void PlayList_DoubleClick(object sender, EventArgs e) { Play(); }

        /// <summary>
        /// Handles the <see cref="ListBox.SelectedIndexChanged"/> event for the playlist.
        /// </summary>
        /// <remarks>Ensures that the selected item in the playlist is visible when the selection
        /// changes.</remarks>
        /// <param name="sender">The source of the event, typically the playlist <see cref="ListBox"/>.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void PlayList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var appState = ApplicationStateService.Instance;
            var selectedIndex = appState.GetSelectedTrackIndex?.Invoke() ?? -1;
            if (selectedIndex != -1)
                appState.EnsureTrackVisible?.Invoke(selectedIndex);
        }

        /// <summary>
        /// Handles key press events for the playlist, enabling keyboard shortcuts for playback control and track
        /// rating.
        /// </summary>
        /// <remarks>This method supports various keyboard shortcuts for interacting with the playlist:
        /// <list type="bullet"> <item><description><c>Ctrl+L</c>: Rates the selected track as
        /// "like".</description></item> <item><description><c>Ctrl+D</c>: Rates the selected track as
        /// "dislike".</description></item> <item><description><c>Ctrl+N</c>: Rates the selected track as
        /// "neutral".</description></item> <item><description><c>Enter</c>: Plays the selected
        /// track.</description></item> <item><description><c>Space</c>: Pauses playback.</description></item>
        /// <item><description><c>Delete</c>: Removes the selected track from the playlist.</description></item>
        /// <item><description>Media keys (<c>MediaNextTrack</c>, <c>MediaPreviousTrack</c>, <c>MediaPlayPause</c>,
        /// <c>MediaStop</c>): Control playback accordingly.</description></item> </list> The playlist control must have
        /// focus for these shortcuts to be recognized.</remarks>
        /// <param name="sender">The source of the event, typically the playlist control.</param>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data, including the key pressed and modifier keys.</param>
        private void PlayList_KeyDown(object sender, KeyEventArgs e)
        {
            //These require the list-box control to be focused to accept these controls

            //Not Final Key combos
            if (e.Control && e.KeyCode == Keys.L) { UserRateTrack("like", (Track)playListBox.SelectedItem, playListBox.SelectedIndex); return; }
            else if (e.Control && e.KeyCode == Keys.D) { UserRateTrack("dislike", (Track)playListBox.SelectedItem, playListBox.SelectedIndex); return; }
            else if (e.Control && e.KeyCode == Keys.N) { UserRateTrack("neutral", (Track)playListBox.SelectedItem, playListBox.SelectedIndex); return; }

            switch (e.KeyCode)
            {
                case Keys.Enter: { Play(); break; }
                case Keys.Space: { Pause(); break; }
                case Keys.Delete: { RemoveItem(); break; }
                case Keys.MediaNextTrack: { NextTrack(); break; }
                case Keys.MediaPreviousTrack: { PreviousTrack(); break; }
                case Keys.MediaPlayPause: { Pause(); break; }
                case Keys.MediaStop: { Stop(); break; }
            }

        }

        /// <summary>
        /// Handles the drag enter event for the playlist, determining the effect of the drag-and-drop operation.
        /// </summary>
        /// <remarks>Sets the drag-and-drop effect to <see cref="DragDropEffects.Copy"/> if the data being
        /// dragged is a file drop. Otherwise, sets the effect to <see cref="DragDropEffects.None"/>.</remarks>
        /// <param name="sender">The source of the event, typically the control onto which the items are being dragged.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data, including the data being dragged.</param>
        private void PlayList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            { e.Effect = DragDropEffects.Copy; }
            else { e.Effect = DragDropEffects.None; }
        }

        /// <summary>
        /// Handles the drag-and-drop operation for the playlist, processing files dropped onto the control.
        /// </summary>
        /// <remarks>This method processes files dropped onto the control by verifying that the data
        /// contains file paths  and then passing the file paths to the appropriate handler for further
        /// processing.</remarks>
        /// <param name="sender">The source of the event, typically the control where the files were dropped.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> containing data about the drag-and-drop operation.</param>
        private void PlayList_DragDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                HandleDroppedFiles(droppedFiles);
            }

        }

        /// <summary>
        /// Handles the event triggered when a playlist is reordered.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data containing the old and new indices of the reordered items.</param>
        private void Playlist_Reordered(object sender, BazthalLib.Events.ItemsReorderedEventArgs e)
        {
            var listBox = (ThemableListBox)sender;

            _playlistManager.SetOrder(listBox.Items.Cast<Track>());

        }

        #endregion Listbox

        #region ComboBox


        /// <summary>
        /// Handles the event when the selected index of the audio device list changes.
        /// </summary>
        /// <remarks>Updates the internal audio device ID list to match the selected index of the audio
        /// device list and changes the audio device to the newly selected device.</remarks>
        /// <param name="sender">The source of the event, typically the audio device list control.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void AudioDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_audioDevices.IsRefreshing) return;

            DebugUtils.Log("Selected Index Change", "Audio Device List",
                $"{AudioDeviceList.SelectedIndex}", logLevel: DebugUtils.LogLevel.Info);

            if (AudioDeviceList.SelectedIndex >= 0)
            {
                var device = _audioDevices.GetDevice(AudioDeviceList.SelectedIndex);
                if (device != null)
                {
                    _audioDevices.LastSelectedDeviceId = device.DeviceID;
                    ChangeAudioDevice(device.DeviceID, userChosen: true);
                }
            }
        }

        private void PlayListOptions_SelectedIndexChanged(object sender, EventArgs e) { }

        #endregion ComboBox

        #region Timer

        /// <summary>
        /// Handles the tick event of the play timer, updating the trackbar and advancing to the next track if
        /// necessary.
        /// </summary>
        /// <remarks>This method updates the trackbar position if the slider is not being dragged. If the
        /// current track has ended naturally, it advances to the next track.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (!Tracking_Slider.Dragging)
            {
                UpdateTrackbar();
                if (_trackEnd)
                {
                    if (_userSeeked)
                    { TrackRatingManager.ApplySeekToEnd(_currentTrackModel, _settings.TrackRating); }
                    else
                    { TrackRatingManager.ApplyPlayCompleted(_currentTrackModel, _settings.TrackRating); }

                    TrackDatabase.SaveStats(_currentTrackModel);
                    NextTrack(true);
                    _trackEnd = false;
                    _userSeeked = false;
                }
            }
        }

        #endregion Timer

        #region Form Events

        /// <summary>
        /// Handles the form closing event, prompting the user for confirmation if playback is active.
        /// </summary>
        /// <remarks>If playback is currently active or resources have not been disposed, the user is
        /// prompted to confirm the closure. If the user chooses not to close, the form closing is cancelled.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            if (_settings.Application.EnableConfirmClose)
            {
                if (_audioPlayback.State == PlaybackState.Playing || !_audioPlayback.IsDisposed)
                {
                    if (ThemableMessageBox.Show("Are you sure you want to close", "Close", MessageBoxButtons.YesNo) == DialogResult.No)
                    { e.Cancel = true; return; }
                }
            }
            Stop();
            _audioPlayback.Dispose();
            _webSocket.Dispose();
        }

        #endregion Form Events

        #endregion UI Event Handlers

        #endregion Methods

        #region Websocket integration

        /// <summary>
        /// Starts the WebSocket server using the configured settings.
        /// </summary>
        private void StartWebSocketServer()
        {
            if (_webSocket.IsRunning)
                return;
            /*
            _webSocket.CommandReceived += (sender, e) =>
            {
                HandleCommand(e.Message);
            };
            */
            
            _webSocket.CommandReceived += (sender, e) => BeginInvoke(() => HandleCommand(e.Message));
            _webSocket.Start(
                _settings.WebSocket.Address,
                _settings.WebSocket.Port,
                _settings.WebSocket.EndPoint,
                onStartError: ex =>
                {
                    this.Invoke(() =>
                    {
                        _settings.WebSocket.AutoStart = false;
                        SaveSettings();

                        string errorMsg = $"WebSocket Server failed to start - check for another websocket server running with the same details\n\n" +
                                        $"ws://{_settings.WebSocket.Address}:{_settings.WebSocket.Port}/{_settings.WebSocket.EndPoint}\n\n" +
                                        $"Auto-Start has been turned off.";

                        ThemableMessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                });
        }

        /// <summary>
        /// Stops the WebSocket server.
        /// </summary>
        private void StopWebSocketServer()
        {
            _webSocket.Stop();
        }

        /// <summary>
        /// Starts the WebSocket server (public wrapper for settings form).
        /// </summary>
        public void StartServerThread()
        {
            StartWebSocketServer();
        }

        /// <summary>
        /// Stops the WebSocket server (public wrapper for settings form).
        /// </summary>
        public void StopServerThread()
        {
            StopWebSocketServer();
        }

        /// <summary>
        /// Gets the WebSocket server running status.
        /// </summary>
        public bool GetWssStatus => _webSocket.IsRunning;

        /// <summary>
        /// Processes a command message and executes the corresponding actions within the application context.
        /// </summary>
        /// <remarks>The method utilizes a <see cref="CommandContext"/> to provide various operations
        /// related to playlist management, audio device selection, and playback control. The context is used to
        /// interpret and execute the command specified by the <paramref name="message"/>.</remarks>
        /// <param name="message">The command message to be handled. This message determines the actions to be executed.</param>
        /// <returns><see langword="true"/> if the command was successfully dispatched and handled; otherwise, <see
        /// langword="false"/>.</returns>
        internal bool HandleCommand(string message)
        {
            var appState = ApplicationStateService.Instance;
            appState.CurrentTrack = _currentTrackModel;
            appState.CurrentTrackFilePath = _currentTrackFilePath;
            appState.VolumeLevel = _volumeLevel;
            appState.SmartShuffleMode = _smartShuffleMode;

            var context = CommandContextFactory.Create();

            context.Play = track => Play(track);
            context.Pause = Pause;
            context.Stop = Stop;
            context.Next = automatic => NextTrack(automatic);
            context.Previous = PreviousTrack;
            context.Shuffle = ShufflePlaylist;
            context.SortPlaylist = (field, desc) => SortPlaylist(field, desc);
            context.ResetStat = async (range, stat) => await ResetTrackStatsAdaptiveAsync(range, stat);
            context.SeekTo = seconds => SeekTo(seconds);
            context.ListRegisteredCommands = _dispatcher.ListRegisteredCommands;
            context.GetMetaData = _dispatcher.GetMetaData;

            return _dispatcher.Dispatch(message, context, _caseInsensitiveOptions);
        }

        #endregion Websocket integration
    }

}