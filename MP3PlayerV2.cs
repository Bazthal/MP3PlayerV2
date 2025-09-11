using BazthalLib;
using BazthalLib.Configuration;
using BazthalLib.Controls;
using BazthalLib.Systems.IO;
using BazthalLib.UI;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
using MP3PlayerV2.Commands;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MP3PlayerV2
{

    public partial class MP3PlayerV2 : Form
    {
        #region Fields

        /// <summary>
        /// Specifies the available options for playing a playlist.
        /// </summary>
        /// <remarks>The <see cref="PlayListOptions"/> enumeration provides different modes for playing
        /// tracks in a playlist. These options can be used to control the order and repetition of tracks during
        /// playback.</remarks>
        public enum PlayListOptions
        {
            Sequential = 0,
            Repeat_PlayList,
            Repeat_Track,
            Random_Track,
            Smart_Shuffle
        }

        /// <summary>
        /// Specifies the modes available for smart shuffle functionality.
        /// </summary>
        /// <remarks>Smart shuffle modes determine the order in which items are played based on specific
        /// criteria. Use this enumeration to configure the behavior of a smart shuffle feature.</remarks>
        public enum SmartShuffleMode
        {
            UnplayedFirst,
            MostPlayed,
            WeightedByRating,
            UnratedFirst
        }

#nullable enable
        private WasapiOut _soundOut;
        private IWaveSource _waveSource;
        private VolumeSource _volumeSource;

        private string? _lastEta = null;

#nullable disable

        public static MP3PlayerV2 Instance { get; private set; }

        private readonly ListBox _audioDeviceIDList = new();

        #region Playback Settings

        private int _volumeLevel = 100;
        private static int _maxTrackHistory = 100;

        #endregion Playback Settings

        #region Playback Utilities


        private bool _disposed = true;
        private bool _userStopped = false;
        private bool _trackEnd = false;
        private static string _currentTrackFilePath;
        private static Track _currentTrackModel;
        private static readonly Random _rng = new();
        private readonly PlaylistManager _playlistManager = new();
        private readonly BazthalLib.Systems.LimitedStack<Track> _trackHistory = new(_maxTrackHistory);
        private readonly Queue<string> _trackQueue = new();
        internal static int _updateStep = 1;
        private bool _userSeeked = false;

        private static SmartShuffleMode _smartShuffleMode = SmartShuffleMode.UnplayedFirst;

        #endregion Playback Utilities

        #region Track Rating Settings

        private static int _leadInImmunity = 2;
        private static int _leadOutImmunity = 10;

        #endregion Track Rating Settings

        #region WebSocket Settings

        private string _webSocketAddress = "127.0.0.1";
        private int _webSocketPort = 8080;
        private string _webSocketEndPoint = "/";
        private bool _autoStart = false;

        #endregion WebSocket Settings

        #region WebSocket Utilities

        private static string _commandResponse = string.Empty;
        private WebSocketServer _server;
        private Thread _serverThread;
        private bool _running = false;
        public bool GetWssStatus { get => _running; }
        private readonly CommandDispatcher _dispatcher = new();
        private bool _includeData = false;

        #endregion WebSocket Utilities

        #region Configuration

        private static readonly int _lastConfigVersion = 0;
        private static readonly int _newConfigVersion = 1;
        private static readonly JsonSerializerOptions _jsonOption = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        private static readonly JsonSerializerOptions _caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };
        internal AppSettings _settings;
        private JSON<AppSettings> _jsonConfig;
        private readonly string _customThemeConfig = Path.Combine(Application.StartupPath, "Config/CustomTheme.json");
        private readonly string _settingsConfig = Path.Combine(Application.StartupPath, "Config/Settings.json");

        private static readonly HashSet<string> SupportedAudioExtensions = new(StringComparer.OrdinalIgnoreCase) { ".flac", ".m4a", ".mp2", ".mp3", ".wav", ".wma" };
        private static readonly HashSet<string> SupportedPlaylistExtensions = new(StringComparer.OrdinalIgnoreCase) { ".m3u", ".m3u8" };

        #endregion Configuration

        #endregion Fields

        #region Contructor

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
            GetAudioDevices();
            playList_Options.Items.Clear();
            foreach (PlayListOptions option in Enum.GetValues(typeof(PlayListOptions)))
            {
                playList_Options.Items.Add(option.ToString().Replace("_", " "));
            }

            //These should be last thing to process Before doing anything that uses it
            LoadThemeFromJson();
            LoadSettings();

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


            if (_autoStart)
            {
                StartServerThread();
            }

            if (args != null && args.Length != 0)
            {
                HandleDroppedFiles(args, true);
            }
        }

        #endregion Contructor

        #region Private Methods

        #region Core Playback Logic

        /// <summary>
        /// Releases all resources used by the CSCore playback components.
        /// </summary>
        /// <remarks>This method stops and disposes of the sound output and wave source components, 
        /// ensuring that all associated resources are properly released. After calling this method,  the playback
        /// components cannot be used again unless they are reinitialized.</remarks>
        private void DisposeCSCore()
        {
            if (_soundOut != null)
            {
                _soundOut.Stopped -= OnSoundOutStopped;
                _soundOut.Stop();
                _soundOut.Dispose();
                _soundOut = null;
            }

            _waveSource?.Dispose();
            _waveSource = null;

            _disposed = true;
        }

        /// <summary>
        /// Initializes the CSCore audio playback system with the specified audio file and optional device ID.
        /// </summary>
        /// <remarks>This method prepares the audio playback by setting up the necessary audio sources and
        /// output devices. It disposes of any previous audio resources before initializing new ones. If the specified
        /// file does not exist, the method logs an error and returns without initializing the playback
        /// system.</remarks>
        /// <param name="filePath">The path to the audio file to be played. The file must exist at the specified path.</param>
        /// <param name="deviceID">The optional ID of the audio output device. If not specified, the default audio device is used.</param>
        private bool InitializeCSCore(string filePath, string deviceID = "")
        {
            DisposeCSCore(); // Clean up before reinitializing

            _disposed = false;
            _trackEnd = false;
            _userStopped = false;

            DebugUtils.Log("Initialize CSCore", Name, $"File Path: {filePath}");

            if (System.IO.File.Exists(filePath))
            {
                var sampleSource = CodecFactory.Instance.GetCodec(filePath)
                    .ToSampleSource();

                _volumeSource = new VolumeSource(sampleSource);

                _waveSource = _volumeSource.ToWaveSource(16);
            }
            else
            {
                DebugUtils.Log("Initialize CSCore", Name, "File not found.");
                return false;
            }

            _soundOut = (string.IsNullOrEmpty(deviceID))
                ? new WasapiOut()
                : new WasapiOut { Device = new MMDeviceEnumerator().GetDevice(deviceID) };

            DebugUtils.Log("Initialize CSCore", Name, $"Device ID: {deviceID}");
            _soundOut.Initialize(_waveSource);
            SetVolume(_volumeLevel);
            _soundOut.Stopped += OnSoundOutStopped;
            return true;
        }

        /// <summary>
        /// Handles the event when sound playback is stopped.
        /// </summary>
        /// <remarks>This method determines whether the playback stopped due to reaching the end of the
        /// track or by user intervention. It updates internal state flags accordingly.</remarks>
        /// <param name="sender">The source of the event, typically the sound output device.</param>
        /// <param name="e">The event data containing information about the playback stop event.</param>
        private void OnSoundOutStopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (_disposed || _soundOut == null || _waveSource == null)
                return;
            long buffer = (long)(_waveSource?.WaveFormat.SampleRate * 0.07);

            try
            {
                DebugUtils.Log("Sound Out Stopped", sender.ToString(), $"Pos: {_waveSource.Position + buffer} - Length: {_waveSource.Length}");

                if ((_soundOut.PlaybackState == PlaybackState.Stopped &&
                    _waveSource.Position + buffer >= _waveSource.Length))
                {
                    _trackEnd = true;
                    _userStopped = false;
                }
                else
                {
                    _userStopped = true;
                }
            }
            catch (ObjectDisposedException)
            {
                DebugUtils.Log("Sound Stopped", sender.ToString(), e.Exception.Message);
            }
        }

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
        private void Play()
        {
            // Check to see it's null first before trying see if it's playing or paused
            if (_soundOut != null)
            {
                //Check to see if it's currently active playing audio
                if (_soundOut?.PlaybackState == PlaybackState.Playing)
                {
                    //Check to see if the currently played track is the same as the selected track in the play list
                    if (string.IsNullOrWhiteSpace(Cur_Track_Label.Text) || !Cur_Track_Label.Text.Contains(playListBox.SelectedItem.ToString()))
                    {
                        ;
                        //Stop Then process the new track
                        Stop();
                        DebugUtils.Log("No Track Match", "Play", $"Match Not Found: {Cur_Track_Label.Text} is not {playListBox.SelectedItem}");
                    }
                    else
                    {
                        DebugUtils.Log("Track Match", "Play", $"Match Found: {Cur_Track_Label.Text}");
                        //if it's the same do nothing and exit here
                        return;
                    }
                }

                //Check to see if there something paused and resume this
                if (_soundOut?.PlaybackState == PlaybackState.Paused)
                {
                    _soundOut?.Resume();
                    UpdatePlaybackState(_soundOut.PlaybackState);
                    this.Text = $"MP3 Player - {Cur_Track_Label.Text}";
                    DebugUtils.Log("Play", "Pause-Resumed", $"{Cur_Track_Label.Text}");
                    return;
                }
            }

            var deviceID = "";

            if (playListBox.Items.Count > 0 && playListBox.SelectedIndex != -1)
            {

                if (AudioDeviceList.SelectedIndex != -1)
                {
                    _audioDeviceIDList.SelectedIndex = AudioDeviceList.SelectedIndex;
                    deviceID = _audioDeviceIDList.SelectedItem.ToString();
                }

                var track = _playlistManager.Get(playListBox.SelectedIndex);
                _currentTrackFilePath = track.FilePath;
                _currentTrackModel = track;
                if (InitializeCSCore(_currentTrackFilePath, deviceID))
                {
                    Cur_Track_Label.Text = track.ToString();

                    this.Text = $"MP3 Player - {Cur_Track_Label.Text}";

                    //Configure Trackbar to Music File
                    Tracking_Slider.Maximum = (int)_waveSource?.GetLength().TotalSeconds;
                    Tracking_Slider.Value = 0;

                    _soundOut?.Play();
                    UpdatePlaybackState(_soundOut.PlaybackState);

                    TrackRatingManager.ApplyPlayStart(track, _settings.TrackRating);
                    //track.PlayCount++;
                    track.LastPlayed = DateTime.UtcNow;
                    var dispTime = $"{track.LastPlayed?.ToLocalTime().ToShortDateString()}-{track.LastPlayed?.ToLocalTime().ToShortTimeString()}";

                    //Save stats to database
                    try
                    {
                        TrackDatabase.SaveStats(track);
                    }
                    catch (Exception ex) { DebugUtils.Log("Play", "Save Stats", $"Saving to data base error: {ex.Message}"); }

                    DebugUtils.Log("Play", Text, $"Current Track: {_currentTrackFilePath}");
                    DebugUtils.Log("Tracking", "Play", $"{_waveSource?.GetLength()}");

                    if (!_running) { DebugUtils.Log("Play", "WebSocket", "Websocket isn't running no broadcast sent"); return; }
                    var nowPlayingInfo = new { NowPlaying = track.ToString(), track.PlayCount, track.LastPlayed, LocalTime = dispTime };
                    string jsonMessage = JsonSerializer.Serialize(nowPlayingInfo, _jsonOption);
                    _server?.WebSocketServices["/nowplaying"].Sessions.Broadcast(jsonMessage);
                }
                else
                {
                    string message = $"Could Not find the file to play:\n{_currentTrackFilePath}";
                    //Send a message to the user stating the file could not be found and auto-closes the message box after 10 seconds
                    ThemableMessageBox.Show(message, "Unable to Play!", MessageBoxButtons.OK, 10000, MessageBoxIcon.Information);
                }
            }
        }


        /// <summary>
        /// Toggles the playback state between playing and paused.
        /// </summary>
        /// <remarks>If the playback is currently paused, this method resumes playback and updates the UI
        /// to reflect the playing state. If the playback is currently playing, this method pauses playback and updates
        /// the UI to reflect the paused state.</remarks>
        private void Pause()
        {
            if (_soundOut == null)
                return;

            if (_soundOut?.PlaybackState == PlaybackState.Paused)
            {
                _soundOut?.Resume();
                this.Text = $"MP3 Player - {Cur_Track_Label.Text}";
                UpdatePlaybackState(_soundOut.PlaybackState);
                return;
            }

            if (_soundOut?.PlaybackState == PlaybackState.Playing)
            {
                _soundOut?.Pause();
                this.Text = $"MP3 Player - {Cur_Track_Label.Text} - Paused";
                UpdatePlaybackState(_soundOut.PlaybackState);
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
            //Don't try if already cleared
            if (_disposed) return;
            this.Text = "MP3 Player";
            _soundOut?.Stop();
            _userStopped = true;
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
                DebugUtils.Log("Next", "No Tracks", "No tracks in the playlist to play.");
                return;
            }

            if (!automatic)
            {
                var track = _playlistManager.Get(playListBox.SelectedIndex);
                if (track != null && _waveSource != null)
                {
                    TimeSpan pos = _waveSource.GetPosition();
                    TimeSpan len = _waveSource.GetLength();

                    if (IsSkipValid(pos, len))
                    {
                        TrackRatingManager.ApplySkip(track, _settings.TrackRating, pos.Seconds);
                        //track.SkipCount++;
                        try { TrackDatabase.SaveStats(track); }
                        catch (Exception ex) { DebugUtils.Log("Next", "Save Stats", $"Error: {ex.Message}"); }
                    }
                }
            }


            if (playListBox.SelectedIndex >= 0)
            {
                //This needs to change to to the current track and not based on the playlist selection
                _trackHistory.Push(_currentTrackModel);
                //_trackHistory.Push(playListBox.SelectedItem.ToString());
            }

            bool endOfPlaylist = false;

            // Remote control override
            while (_trackQueue.Count > 0)
            {
                string queuedTrack = _trackQueue.Peek();
                int index = -1;

                // Search for a matching track in the playlist
                for (int i = 0; i < _playlistManager.Count; i++)
                {
                    var track = _playlistManager.Get(i);
                    if (track != null && track.ToString() == queuedTrack)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    _trackQueue.Dequeue();
                    playListBox.SelectedIndex = index;
                    Play();
                    return;
                }
                else
                {
                    // No match found, remove this stale entry and check the next one
                    _trackQueue.Dequeue();
                }
            }

            PlayListOptions option = PlayListOptions.Sequential; // Default fallback

            if (playList_Options.SelectedIndex != -1)
            {
                string selectedText = playList_Options.SelectedItem.ToString().Replace(" ", "_");
                _ = Enum.TryParse(selectedText, out option);
            }
            switch (option)
            {
                case PlayListOptions.Repeat_PlayList:
                    {
                        if (playListBox.SelectedIndex != playListBox.Items.Count - 1)
                            playListBox.SelectedIndex++;
                        else
                            playListBox.SelectedIndex = 0;
                        break;
                    }
                case PlayListOptions.Repeat_Track:
                    {
                        TrackRatingManager.ApplyReplay(_currentTrackModel, _settings.TrackRating);
                        //Don't do anything else here as stopping and starting playback again will restart the track
                        break;
                    }
                case PlayListOptions.Random_Track: { GetRandomTrack(); break; }
                case PlayListOptions.Smart_Shuffle:
                    {
                        var track = PickSmartTrack(_playlistManager.Tracks);
                        if (track == null) return;

                        int index = _playlistManager.IndexOf(track);
                        if (index < 0) return;

                        playListBox.SelectedIndex = index;
                        break;
                    }
                default:
                    {

                        if (playListBox.SelectedIndex == playListBox.Items.Count - 1)
                        {

                            this.Text = "MP3 Player";
                            Cur_Track_Label.Text = "";
                            playListBox.SelectedIndex = -1;
                            Tracking_Slider.Value = 0;
                            endOfPlaylist = true;

                            if (_soundOut == null)
                                return;

                            _soundOut?.Stop();
                            UpdatePlaybackState(_soundOut.PlaybackState);
                            DisposeCSCore();
                        }
                        else playListBox.SelectedIndex++;
                        break;
                    }
            }

            if (!endOfPlaylist)
            {
                _soundOut?.Stop();
                Play();
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
                DebugUtils.Log("Previous", "No Tracks", "No tracks in the playlist to play.");
                return;
            }

            if (_waveSource != null && _waveSource.GetPosition().TotalSeconds > 5)
            {
                _waveSource.SetPosition(TimeSpan.Zero);
                return;
            }

            while (_trackHistory.Count > 0)
            {
                //var prev = _trackHistory.Peek();
                var prev = _trackHistory.Peek();
                int index = -1;

                // Search for a matching track in the playlist
                for (int i = 0; i < _playlistManager.Count; i++)
                {
                    var track = _playlistManager.Get(i);
                    //if (track != null && track.ToString() == prev)
                    if (track != null && track == prev)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    _trackHistory.Pop();

                    playListBox.SelectedIndex = index;
                    Play();
                    return;
                }
                else
                {
                    DebugUtils.Log("Previous Track", "Not Found", "Last Played was not found in the playlist");
                    // No match found, remove this stale entry and check the next one
                    _trackHistory.Pop();
                }
            }

            if (playListBox.SelectedIndex > 0)
            {
                playListBox.SelectedIndex--;
            }
            else
            {
                return;
            }

            _soundOut?.Stop();
            Play();

        }

        /// <summary>
        /// Sets the volume level for the audio source.
        /// </summary>
        /// <remarks>This method adjusts the volume of the audio source based on the current volume level.
        /// The volume is clamped between 0.0 (mute) and 1.0 (full volume).</remarks>
        private void SetVolume(int vol)
        {
            _volumeLevel = vol;
            if (_volumeSource != null)
                _volumeSource.Volume = Math.Clamp(vol / 100f, 0.0f, 1.0f); // 0 = mute, 1 = full
                                                                           //Set Volume slider value to match current volume level 
            Volume_Slider.Value = (int)_volumeLevel;

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
            if (_waveSource != null && _waveSource.CanSeek)
            {
                long bytePosition = (long)(seconds * _waveSource.WaveFormat.BytesPerSecond);
                bytePosition = Math.Min(bytePosition, _waveSource.Length);
                _waveSource.Position = bytePosition;

                double tolerance = 0.5;
                double currentSeconds = _waveSource.GetPosition().TotalSeconds;
                double totalSeconds = _waveSource.GetLength().TotalSeconds;

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
        }



        /// <summary>
        /// Updates the position of the trackbar to reflect the current playback position of the audio source.
        /// </summary>
        /// <remarks>This method calculates the current playback time in seconds from the audio source's
        /// position and updates the trackbar slider accordingly. Ensure that the audio source is not null before
        /// calling this method.</remarks>
        private void UpdateTrackbar()
        {
            if (_waveSource != null)
            {
                var currentSeconds = _waveSource.Position / (double)_waveSource.WaveFormat.BytesPerSecond;
                Tracking_Slider.Value = (int)currentSeconds;
            }
        }

        #endregion Core Playback Logic

        #region Playdata Methods

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
                                selectedTrack = _playlistManager.Get(playListBox.SelectedIndex);
                        });
                    }
                    else
                    {
                        if (playListBox.SelectedIndex >= 0)
                            selectedTrack = _playlistManager.Get(playListBox.SelectedIndex);
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
                DebugUtils.Log("Stats Reset", "Canceled", oce.Message);
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
            catch (Exception ex) { DebugUtils.Log("Reset Stats", "Dialog Update", ex.Message); }
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
        private async Task ResetTrackStatsParallelAsync(string tag, ThemableProcessingDialog? dialog, CancellationToken cancellationToken)
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

                var task = Task.Run(async () =>
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
        /// <remarks>If the rating is changed, the updated track statistics are saved to the database. If
        /// an error occurs during  the save operation, it is logged but does not affect the return value.</remarks>
        /// <param name="index">The index of the track in the playlist. Must be a valid index within the playlist.</param>
        /// <param name="mode">The rating mode to apply to the track. Valid values are <see langword="like"/>, <see langword="dislike"/>, 
        /// and <see langword="neutral"/>. The comparison is case-insensitive.</param>
        /// <param name="track">The track to be rated. If <paramref name="track"/> is <see langword="null"/>, the track at the specified 
        /// <paramref name="index"/> in the playlist will be retrieved automatically.</param>
        /// <returns><see langword="true"/> if the track's rating was successfully updated; otherwise, <see langword="false"/>.</returns>
        private bool UserRateTrack(int index, string mode, Track track = null)
        {
            track ??= _playlistManager.Get(index);

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
                catch (Exception ex) { DebugUtils.Log("Like - Dislike", "Save Stats", $"Saving to data base error: {ex.Message}"); }
            }
            return changed;
        }
        #endregion Playdata Methods

        #region Device Management

        /// <summary>
        /// Populates the list of available audio output devices.
        /// </summary>
        /// <remarks>This method retrieves all active audio rendering devices and adds their names to the <see
        /// cref="AudioDeviceList"/>. If a device name contains parentheses, only the portion before the first parenthesis is
        /// added. The method also stores the device IDs in the <see cref="_audioDeviceIDList"/>. If any devices are found, the
        /// first device is selected by default.</remarks>
        private void GetAudioDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
                if (device.FriendlyName.Contains('('))
                {
                    string[] split = device.FriendlyName.Split('(');
                    AudioDeviceList.Items.Add(split[0].Trim());
                }
                else { AudioDeviceList.Items.Add(device.FriendlyName); }

                _audioDeviceIDList.Items.Add(device.DeviceID);

            }
            if (AudioDeviceList.Items.Count > 0) AudioDeviceList.SelectedIndex = 0;

        }

        /// <summary>
        /// Changes the audio output device to the specified device ID.
        /// </summary>
        /// <remarks>This method stops the current audio output, disposes of the existing audio resources,
        /// and initializes the audio system with the new device. The playback position is preserved across the device
        /// change.</remarks>
        /// <param name="deviceId">The identifier of the audio device to switch to. Cannot be null or empty.</param>
        private void ChangeAudioDevice(string deviceId)
        {
            // Don't go through the process of setting up new device if there is no stream
            // InitializeCSCore will setup to new device on setup
            if (_soundOut == null || _disposed) { return; }
            var currentPosition = _waveSource?.Position ?? 0;
            _soundOut?.Stop();
            DisposeCSCore();

            InitializeCSCore(_currentTrackFilePath, deviceId);

            _waveSource.Position = currentPosition;

            _soundOut.Play();
            StartPlaybackTimer();
        }

        #endregion Device Management

        #region Command Helpers

        /// <summary>
        /// Normalizes the specified text by removing diacritics and standardizing quotation marks.
        /// </summary>
        /// <param name="text">The input text to be normalized. Can be null or whitespace.</param>
        /// <returns>A normalized version of the input text with diacritics removed, standardized quotation marks, and converted
        /// to lowercase. Returns the original text if it is null or whitespace.</returns>
        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove diacritics
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace("’", "'")
                     .Replace("‘", "'")
                     .Replace("´", "'")
                     .Replace("`", "'")
                     .Replace("“", "\"")
                     .Replace("”", "\"")
                     .Replace("″", "\"")
                     .Trim()
                     .ToLowerInvariant();
        }

        /// <summary>
        /// Counts the number of tracks in the playlist that have not been played.
        /// </summary>
        /// <remarks>This method evaluates the play count of each track in the playlist and determines how
        /// many tracks have a play count of zero. The result is stored in a command response string indicating the
        /// number of unplayed tracks.</remarks>
        private void CountTrackByPlayData(string data)
        {
            var cachedlist = _playlistManager.Tracks.ToList();
            var matches = new List<Track>();
            switch (data?.ToLowerInvariant())
            {
                case "unplayed":
                    matches = cachedlist.Where(t => (t.PlayCount ?? 0) == 0).ToList();
                    break;
                case "liked":
                    matches = cachedlist.Where(t => t.Liked == true).ToList();
                    break;
                case "disliked":
                    matches = cachedlist.Where(t => t.Disliked == true).ToList();
                    break;
            }

            BuildResponseMessage(true, $"{matches.Count} {data} track(s) found in the playlist", _includeData ? matches : null);
            matches.Clear();
        }

        /// <summary>
        /// Calculates a match score between a track name and a search term based on their similarity.
        /// </summary>
        /// <remarks>The match score is calculated based on several factors, including whether the track
        /// name starts with the search term, and whether all words in the search term appear in order within the track
        /// name. The method performs a case-insensitive comparison.</remarks>
        /// <param name="trackName">The name of the track to evaluate. Cannot be null.</param>
        /// <param name="searchTerm">The search term to compare against the track name. Cannot be null.</param>
        /// <returns>An integer representing the match score. A higher score indicates a closer match between the track name and
        /// the search term.</returns>
        private int CalculateMatchScore(string trackName, string searchTerm)
        {
            int score = 0;
            string normalizedTrack = NormalizeText(trackName);
            string normalizedSearch = NormalizeText(searchTerm);

            score += 10;

            if (normalizedTrack.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                score += 5;

            var words = normalizedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int lastIndex = -1;
            bool allWordsInOrder = true;

            foreach (var word in words)
            {
                int idx = normalizedTrack.IndexOf(word, lastIndex + 1, StringComparison.OrdinalIgnoreCase);
                if (idx == -1)
                {
                    allWordsInOrder = false;
                    break;
                }
                lastIndex = idx;
            }

            if (allWordsInOrder)
                score += 3;

            return score;
        }

        /// <summary>
        /// Builds a regular expression to match the specified search term, supporting wildcard characters and partial
        /// matches.
        /// </summary>
        /// <remarks>The method normalizes the input search term and converts wildcard characters (*) into
        /// a pattern that matches zero or more characters. If no wildcard is present, the search term is split into
        /// words, and the resulting pattern matches any sequence of characters between the words.</remarks>
        /// <param name="searchTerm">The search term to convert into a regular expression. Wildcard characters (*) are supported.</param>
        /// <returns>A <see cref="Regex"/> instance that matches the specified search term. The resulting pattern is
        /// case-insensitive and allows for partial matches.</returns>
        private static Regex BuildSearchRegex(string searchTerm)
        {
            string normalizedSearch = NormalizeText(searchTerm);

            normalizedSearch = Regex.Replace(normalizedSearch, @"([*])\1+", "$1");

            string pattern;

            if (normalizedSearch.Contains("*"))
            {
                pattern = Regex.Escape(normalizedSearch).Replace("\\*", ".*");
            }
            else
            {

                pattern = string.Join(".*", normalizedSearch
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Regex.Escape));

                pattern = ".*" + pattern + ".*";
            }

            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Searches the playlist for tracks whose names match the specified search term and generates a response
        /// message.
        /// </summary>
        /// <remarks>This method uses a regular expression to perform the search and normalizes track
        /// names before matching. If matching tracks are found, a response message is generated indicating the number
        /// of matches. If no matches are found, a response message is generated indicating that no matches were
        /// found.</remarks>
        /// <param name="searchTerm">The term to search for in track names. The search is case-insensitive and supports partial matches.</param>
        private void CountTrackByName(string searchTerm)
        {
            var regex = BuildSearchRegex(searchTerm);
            var matches = new List<Track>();

            for (int i = 0; i < _playlistManager.Count; i++)
            {
                var track = _playlistManager.Get(i);
                if (track == null) continue;

                string trackName = NormalizeText(track.ToString());

                if (regex.IsMatch(trackName))
                    matches.Add(track);
            }

            if (matches.Count > 0)
                BuildResponseMessage(true, $"{matches.Count} matching item(s) found in the playlist for: {searchTerm}", _includeData ? matches : null);
            else
                BuildResponseMessage(false, "No matching item found in the playlist");
            matches.Clear();
        }

        /// <summary>
        /// Selects a track from the playlist based on the specified search term.
        /// </summary>
        /// <remarks>The method searches the playlist for the best matching track using a scoring
        /// mechanism. If a match is found, the track is selected,  and playback is started. If no match is found, an
        /// appropriate response message is generated.</remarks>
        /// <param name="searchTerm">The term used to search for a matching track. This value is case-insensitive and may include partial or full
        /// track names.</param>
        private void SelectTrackByName(string searchTerm)
        {
            var regex = BuildSearchRegex(searchTerm);
            Track bestMatch = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < _playlistManager.Count; i++)
            {
                var track = _playlistManager.Get(i);
                if (track == null) continue;

                string trackName = NormalizeText(track.ToString());
                if (regex.IsMatch(trackName))
                {
                    int score = CalculateMatchScore(trackName, searchTerm);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = track;
                    }
                }
            }

            if (bestMatch != null)
            {
                int index = _playlistManager.IndexOf(bestMatch);
                playListBox.SelectedIndex = index;
                BuildResponseMessage(true, $"Match found: {bestMatch}");
                Stop();
                Play();
                return;
            }

            BuildResponseMessage(false, "No matching item found in the playlist");
        }

        /// <summary>
        /// Searches for a track in the playlist by name and adds the best match to the queue.
        /// </summary>
        /// <remarks>The method evaluates all tracks in the playlist and selects the one that best matches
        /// the provided search term. If a match is found, the track is added to the queue, and a success message is
        /// generated. If no match is found, a failure message is generated instead.</remarks>
        /// <param name="searchTerm">The search term used to find a matching track. This is compared against the names of tracks in the playlist.</param>
        private void QueueTrackByName(string searchTerm)
        {
            var regex = BuildSearchRegex(searchTerm);
            Track bestMatch = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < _playlistManager.Count; i++)
            {
                var track = _playlistManager.Get(i);
                if (track == null) continue;

                string trackName = NormalizeText(track.ToString());
                if (regex.IsMatch(trackName))
                {
                    int score = CalculateMatchScore(trackName, searchTerm);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = track;
                    }
                }
            }

            if (bestMatch != null)
            {
                _trackQueue.Enqueue(bestMatch.ToString());
                BuildResponseMessage(true, $"Added {bestMatch} to the Queue");
                return;
            }

            BuildResponseMessage(false, "No matching item found in the playlist");
        }

        #endregion Command Helplers

        #region Helper Methods

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
            void handler()
            {
                _playlistManager.PlaylistChanged -= handler;

                BeginInvoke(new Action(() =>
                {
                    if (playListBox.Items.Count > 0)
                    {
                        playListBox.SelectedIndex = firstTrack ? 0 : playListBox.Items.Count - 1;
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
            catch (Exception ex) { DebugUtils.Log("Drop Folder", "Recursive Search", $"Error scanning folder '{folderPath}': {ex.Message}"); }

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
                    DebugUtils.Log("Garbage Collection", "Cleanup", $"Released {(before - after) / 1024 / 1024} MB after adding {addedCount} tracks");
                });
            }
        }

        #endregion Helper Methods

        #region Playlist Management 

        /// <summary>
        /// Selects a track from a collection of tracks, prioritizing those that have not been played.
        /// </summary>
        /// <remarks>The method first attempts to select a track that has not been played. If all tracks
        /// have been played, it selects one of the tracks with the minimum play count.</remarks>
        /// <param name="tracks">A collection of tracks to choose from. Each track may have a play count associated with it.</param>
        /// <returns>A <see cref="Track"/> object that is either unplayed or among the least played tracks in the collection.
        /// Returns <see langword="null"/> if the collection is empty.</returns>
#nullable enable
        private Track? PickSmartTrack(IEnumerable<Track> tracks)
        {
            var trackslist = tracks.ToList();
            Track? track = null;

            // Create a lookup set of recent track GUIDs
            var recentTrackSet = new HashSet<Guid>(_trackHistory.Select(h => h.Guid));

            switch (_smartShuffleMode)
            {
                case SmartShuffleMode.UnplayedFirst:
                    var unplayed = trackslist
                        .Where(t => (t.PlayCount ?? 0) == 0)
                        .ToList();

                    if (unplayed.Count > 0)
                        track = PickRandom(unplayed);
                    else
                    {
                        int minPlays = trackslist.Min(t => t.PlayCount ?? 0);
                        var leastPlayed = trackslist
                            .Where(t => (t.PlayCount ?? 0) == minPlays)
                            .ToList();

                        track = PickRandom(leastPlayed);
                    }
                    break;

                case SmartShuffleMode.MostPlayed:
                    var maxPlays = trackslist.Max(t => t.PlayCount ?? 0);

                    if (maxPlays == 0) // fresh DB
                    {
                        track = PickRandom(trackslist
                            .Where(t => !recentTrackSet.Contains(t.Guid))
                            .ToList());
                        break;
                    }

                    var mostPlayed = trackslist
                        .Where(t => (t.PlayCount ?? 0) == maxPlays && !recentTrackSet.Contains(t.Guid))
                        .ToList();

                    track = PickRandom(mostPlayed.Any()
                        ? mostPlayed
                        : trackslist.Where(t => !recentTrackSet.Contains(t.Guid)).ToList());
                    break;

                case SmartShuffleMode.WeightedByRating:
                case SmartShuffleMode.UnratedFirst:
                    DebugUtils.Log("Smart Shuffle", "Pick Smart Track", $"Rating System not yet Implemented");
                    break;

                default:
                    DebugUtils.Log("Smart Shuffle", "Pick Smart Track", $"Unknown smart shuffle mode: {_smartShuffleMode}");
                    break;
            }

            return track;
        }

        /// <summary>
        /// Selects a random <see cref="Track"/> from the specified list.
        /// </summary>
        /// <param name="list">The list of <see cref="Track"/> objects to choose from. Must not be null or empty.</param>
        /// <returns>A randomly selected <see cref="Track"/> from the list, or <see langword="null"/> if the list is null or
        /// empty.</returns>
        private static Track? PickRandom(List<Track> list, Track? exclude = null)
        {
            if (list == null || list.Count == 0)
                return null;

            if (exclude != null && list.Count > 1)
            {
                Track selected;
                do
                {
                    selected = list[_rng.Next(list.Count)];
                } while (selected == exclude);
                return selected;
            }

            return list[_rng.Next(list.Count)];
        }

#nullable disable

        /// <summary>
        /// Shuffles the current playlist and restores the original track selection.
        /// </summary>
        /// <remarks>This method randomizes the order of tracks in the playlist while ensuring that the
        /// currently selected track remains selected after the shuffle. It is useful for maintaining the user's current
        /// listening position in a shuffled playlist.</remarks>
        private void ShufflePlaylist()
        {
            _playlistManager.Shuffle();
            // restore original selection
            for (int i = 0; i < playListBox.Items.Count - 1; i++)
            {
                if (!string.IsNullOrWhiteSpace(Cur_Track_Label.Text))
                {
                    if (playListBox.Items[i].ToString().Contains(Cur_Track_Label.Text))
                    {
                        DebugUtils.Log("Shuffle", "Restore Selection", $"{playListBox.Items[i]} = {Cur_Track_Label.Text}");
                        playListBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the playlist based on a specified key and order.
        /// </summary>
        /// <remarks>This method clears the current playlist and repopulates it with the sorted tracks. It
        /// also updates the UI list box to reflect the new order.</remarks>
        /// <param name="keySelector">A function to extract a key from a track for sorting purposes.</param>
        /// <param name="descending">A boolean value indicating whether the sorting should be in descending order. Defaults to <see
        /// langword="false"/> for ascending order.</param>
        private void SortPlaylist(Func<Track, object> keySelector, bool descending = false)
        {
            var sorted = descending
                ? [.. _playlistManager.Tracks.OrderByDescending(keySelector).ToList()]
                : _playlistManager.Tracks.OrderBy(keySelector).ToList();

            _playlistManager.Clear();
            _playlistManager.AddRange(sorted);
        }

        /// <summary>
        /// Selects a random track from the playlist and updates the selection.
        /// </summary>
        /// <remarks>This method updates the selected track in the playlist to a random track. If
        /// <paramref name="blockLast"/> is set to <see langword="true"/>, the method will avoid selecting the track
        /// that was last played.</remarks>
        /// <param name="blockLast">If <see langword="true"/>, ensures that the last played track is not selected again.</param>
        private void GetRandomTrack(bool blockLast = false)
        {
            Random Rndnumber = new();
            int Number = Rndnumber.Next(0, playListBox.Items.Count);
            string LastSong = playListBox.GetItemText(playListBox.SelectedItem);
            if (blockLast && LastSong == playListBox.GetItemText(playListBox.Items[Number]))
            {
                Number = Rndnumber.Next(0, playListBox.Items.Count);
            }
            playListBox.SelectedIndex = Number;
        }



        /// <summary>
        /// Adds one or more tracks to the playlist, either from the specified file paths or by prompting the user to
        /// select files.
        /// </summary>
        /// <remarks>This method processes the specified or selected files to extract track metadata, such
        /// as title, artist, album, and duration.  Tracks are then added to the playlist and the database. If the
        /// operation is canceled, no tracks are added. <para> The method supports concurrent processing of files to
        /// improve performance, with a limit on the maximum degree of concurrency. </para> <para> A progress dialog is
        /// displayed during the operation, showing the current progress and an estimated time of completion (ETA).
        /// </para></remarks>
        /// <param name="droppedItems">An optional array of file paths representing the tracks to add. If <see langword="null"/> or empty, the user
        /// will be prompted to select files.</param>
#nullable enable
        private async void AddItem(string[]? droppedItems = null)
        {
            string[]? files = droppedItems;
            if (files == null || files.Length == 0)
            {
                files = Files.ChooseFiles("", "Music Files|*.flac;*.m4a;*.mp2;*.mp3;*.wav;*.wma");
                if (files == null || files.Length == 0) return;
            }
            var dialog = new ThemableProcessingDialog("Adding tracks...") { StartPosition = FormStartPosition.Manual };
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );

            dialog.Show(this);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int total = files.Length;
            int progress = 0;
            var guidCache = TrackDatabase.PreloadTrackGuids();

            int processorCount = Environment.ProcessorCount;
            int maxConcurrency = Math.Min(processorCount * 2, processorCount < 8 ? 20 : 40);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task<(int Index, Track? Track)>>();

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var index = i;

                await semaphore.WaitAsync(); // wait for a slot

                if (dialog.Token.IsCancellationRequested)
                    break;

                var task = Task.Run(() =>
                {
                    try
                    {
                        if (dialog.Token.IsCancellationRequested)
#nullable disable
                            return (Index: index, Track: null);
#nullable enable
                        using var tagFile = TagLib.File.Create(file);
                        var track = new Track
                        {
                            FilePath = file,
                            Title = !string.IsNullOrEmpty(tagFile.Tag.Title) ? tagFile.Tag.Title.Replace("\r", "").Replace("\n", "") : Path.GetFileNameWithoutExtension(file),
                            Artist = tagFile.Tag.Performers?.Length > 0 ? string.Join("/", tagFile.Tag.Performers) : "Unknown Artist",
                            Album = !string.IsNullOrEmpty(tagFile.Tag.Album) ? tagFile.Tag.Album : "",
                            DurationSeconds = (int)tagFile.Properties.Duration.TotalSeconds,
                            Hash = TrackDatabase.ComputeFileHash(file, false)
                        };

                        track.Guid = TrackDatabase.AssignGuidFromCache(track, guidCache);
                        DebugUtils.Log("Track Add", $"{index}", $"{track}");
                        return (Index: index, Track: track);
                    }
                    catch (Exception ex)
                    {
                        DebugUtils.Log("AddItem", this.Name, ex.Message);
#nullable disable
                        return (Index: index, Track: null);
#nullable enable
                    }
                    finally
                    {
                        int currentProgress = Interlocked.Increment(ref progress);

                        string? etaText = null;

                        if (currentProgress % _updateStep == 0 || currentProgress == total)
                        {
                            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            double avgPerFile = elapsedSeconds / currentProgress;
                            int remaining = total - currentProgress;
                            double etaSeconds = avgPerFile * remaining;

                            etaText = $"ETA: {TimeSpan.FromSeconds(etaSeconds):mm\\:ss}";
                            _lastEta = etaText;
                        }
                        else
                        {
                            etaText = _lastEta ?? "ETA: --:--";
                        }

                        dialog.SetProgress("Track", currentProgress, total, etaText);
                        semaphore.Release();
                    }
                });
#nullable disable
                tasks.Add(task);
#nullable enable
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                DebugUtils.Log("ADD Items", "result", $"{result}");

                if (result.Track != null)
                {
                    TrackDatabase.LoadStats(result.Track);
                }
            }

            var newTracks = results
                .Where(r => r.Track != null)
                .OrderBy(r => r.Index)
                .Select(r => r.Track!)
                .ToList();



            _playlistManager.AddRange(newTracks);

            dialog.SetCompleted($"Added {newTracks.Count} track(s).");
            dialog.CloseAfter(1000);

            if (_playlistManager.Count > 0 && playListBox.SelectedIndex >= 0)
                playListBox.SelectedIndex = playListBox.Items.Count - 1;

            CleanupIfNeeded(newTracks.Count);
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
                if (!string.IsNullOrWhiteSpace(Cur_Track_Label.Text) && Cur_Track_Label.Text.Contains(playListBox.SelectedItem.ToString()))
                {
                    DisposeCSCore();
                    ResetUIText();
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
        private async void SavePlaylist()
        {
            string saveFileName = Files.SaveFile("", "M3U Playlists|*.m3u", "Save Playlist");
            if (string.IsNullOrWhiteSpace(saveFileName)) return;

            var dialog = new ThemableProcessingDialog("Saving Playlist...", showProgress: true, showCancelButton: false) { StartPosition = FormStartPosition.Manual };
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );

            dialog.Show(this);

            await Task.Run(() => _playlistManager.SaveToM3U(saveFileName));

            dialog.SetCompleted("Playlist saved successfully.");
            dialog.CloseAfter(1000);
        }

        /// <summary>
        /// Loads a playlist from the specified file or prompts the user to select a playlist file.
        /// </summary>
        /// <remarks>This method displays a progress dialog while the playlist is being loaded. If the
        /// operation is canceled, the dialog will indicate the cancellation. The method ensures that a valid playlist
        /// is loaded and updates the playlist display accordingly.</remarks>
        /// <param name="droppedItem">The path to the playlist file to load. If <see langword="null"/> or empty, the user will be prompted to
        /// select a file.</param>
        /// <param name="keepSelection">A value indicating whether to retain the currently selected track in the playlist, if it exists. If <see
        /// langword="true"/>, the selection will be preserved if the track is found in the new playlist; otherwise, the
        /// first track will be selected.</param>
        private async void LoadPlaylist(string droppedItem = null, bool keepSelection = true)
        {
            string lastItem = null;
            if (!string.IsNullOrWhiteSpace(Cur_Track_Label.Text))
            {
                lastItem = Cur_Track_Label.Text;
            }

            string loadFileName = droppedItem;
            if (loadFileName == null || loadFileName.Length == 0)
            {
                loadFileName = Files.ChooseFile("", "M3U Playlists|*.m3u;*.m3u8", "Open Playlist");
                if (string.IsNullOrWhiteSpace(loadFileName)) return;
            }

            var dialog = new ThemableProcessingDialog("Loading Playlist") { StartPosition = FormStartPosition.Manual };
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );
            dialog.Show(this);

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


            if (playListBox.Items.Count > 0)
            {
                if (lastItem != null && keepSelection)
                {
                    bool found = false;
                    for (int i = 0; i < playListBox.Items.Count; i++)
                    {
                        if (playListBox.Items[i].ToString() == lastItem)
                        {
                            playListBox.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        playListBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    playListBox.SelectedIndex = 0;
                }
            }


            dialog.SetCompleted("Playlist loaded.");
            dialog.CloseAfter(1000);
        }

        #endregion Playlist Management

        #region Configuration

        /// <summary>
        /// Resets the user interface text to its default state.
        /// </summary>
        /// <remarks>Sets the main window title to "MP3 Player" and clears the current track
        /// label.</remarks>
        private void ResetUIText()
        {
            this.Text = "MP3 Player";
            Cur_Track_Label.Text = string.Empty;
        }

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
                _maxTrackHistory = _settings.Playback.MaxTrackHistory;
                bool parsed = Enum.TryParse(_settings.SmartShuffle.Mode, out SmartShuffleMode mode);
                if (parsed) _smartShuffleMode = mode;

                _includeData = _settings.CommandBehaviour.IncludeTracksInCount;

                //Update WebSocket Info
                _webSocketAddress = _settings.WebSocket.Address;
                _webSocketPort = _settings.WebSocket.Port;
                _webSocketEndPoint = _settings.WebSocket.EndPoint;
                _autoStart = _settings.WebSocket.AutoStart;
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
                var config = new JSON<ThemeColors>(_customThemeConfig, [new ThemeColorsJsonConverter()]);
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
        /// Loads application settings from the configuration file, migrating from an old format if necessary.
        /// </summary>
        /// <remarks>
        /// If the settings file exists, this method reads and parses its JSON content. If the configuration version is outdated,
        /// it creates a backup, migrates the settings to the new format, and saves the updated configuration. Otherwise, it loads
        /// the settings using the <see cref="JSON{AppSettings}"/> helper. Playback and WebSocket settings are applied to the UI and
        /// internal fields. If the settings file does not exist, a new settings file is created with default values.
        /// </remarks>
        private void LoadSettings()
        {
            _jsonConfig = new JSON<AppSettings>(_settingsConfig);

            if (File.Exists(_settingsConfig))
            {
                string rawJson = File.ReadAllText(_settingsConfig);
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
                    Files.MigrationBackUp(_settingsConfig);
                    DebugUtils.Log("Load Settings", Name, "Migrating from old config format");
                    _settings = MigrateFromOldConfig(root);

                    // Save migrated config
                    string migratedJson = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_settingsConfig, migratedJson);
                }
                else
                {
                    _jsonConfig.Load();
                    _settings = _jsonConfig.Data;
                }

                //Playback Settings
                _volumeLevel = _settings.Playback.VolumeLvl;
                AudioDeviceList.SelectedItem = _settings.Playback.AudioDevice;
                playList_Options.SelectedItem = _settings.Playback.PlayListMode;
                _maxTrackHistory = _settings.Playback.MaxTrackHistory;

                //Websocket Settings
                _webSocketAddress = _settings.WebSocket.Address;
                _webSocketPort = _settings.WebSocket.Port;
                _webSocketEndPoint = _settings.WebSocket.EndPoint;
                _autoStart = _settings.WebSocket.AutoStart;

                //Track Rating Settings
                _leadInImmunity = _settings.TrackRating.LeadInImmunitySeconds;
                _leadOutImmunity = _settings.TrackRating.LeadOutImmunitySeconds;

                _includeData = _settings.CommandBehaviour.IncludeTracksInCount;
                //Smart Shuffle Settings

                bool parsed = Enum.TryParse(_settings.SmartShuffle.Mode, out SmartShuffleMode mode);
                if (parsed) _smartShuffleMode = mode;


                //New settings to be added here

                SetVolume(_volumeLevel);
            }
            else
            {
                DebugUtils.Log("Load Settings", Name, "File Not found, creating new file");
                _settings = new AppSettings();
                SaveSettings();
            }
        }

        /// <summary>
        /// Migrates configuration data from an old JSON format to the current <see cref="AppSettings"/> structure.
        /// </summary>
        /// <param name="root">The <see cref="JsonElement"/> representing the root of the old configuration JSON.</param>
        /// <returns>
        /// A new <see cref="AppSettings"/> instance populated with values extracted from the old configuration.
        /// If a property is missing or of an unexpected type, a default value is used.
        /// </returns>
        /// <remarks>
        /// This method reads playback and WebSocket settings from the provided <paramref name="root"/> element.
        /// It sets default values for missing properties and updates the <c>ConfigVersion</c> to the latest version.
        /// </remarks>
        private AppSettings MigrateFromOldConfig(JsonElement root)
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

            newConfig.ConfigVersion = _newConfigVersion;

            return newConfig;
        }

        /// <summary>
        /// Saves the current application settings to a JSON configuration file.
        /// </summary>
        /// <remarks>
        /// This method updates the playback and WebSocket settings in the <see cref="AppSettings"/> object,
        /// ensures the configuration version is current, and writes the settings to the configuration file.
        /// It also creates a backup of the existing configuration file before saving.
        /// </remarks>
        private void SaveSettings()
        {
            if (_settings == null)
            {
                DebugUtils.Log("Save Settings", Name, "_settings is NULL, aborting");
                return;
            }

            //Playback Settings
            _settings.Playback.VolumeLvl = _volumeLevel;

            if (AudioDeviceList.SelectedIndex == -1 && AudioDeviceList.Items.Count > 0)
                AudioDeviceList.SelectedIndex = 0;
            _settings.Playback.AudioDevice = AudioDeviceList.SelectedItem?.ToString() ?? string.Empty;

            if (playList_Options.SelectedIndex == -1 && playList_Options.Items.Count > 0)
                playList_Options.SelectedIndex = 0;
            _settings.Playback.PlayListMode = playList_Options.SelectedItem?.ToString() ?? string.Empty;

            _settings.Playback.MaxTrackHistory = _maxTrackHistory;

            //WebSocket Settings
            _settings.WebSocket.Address = _webSocketAddress;
            _settings.WebSocket.Port = _webSocketPort;
            _settings.WebSocket.EndPoint = _webSocketEndPoint;
            _settings.WebSocket.AutoStart = _autoStart;

            //Track Rating Settings
            _settings.TrackRating.LeadInImmunitySeconds = _leadInImmunity;
            _settings.TrackRating.LeadOutImmunitySeconds = _leadOutImmunity;

            // New Settings Here

            // Ensure ConfigVersion is set
            if (_settings.ConfigVersion == _lastConfigVersion)
                _settings.ConfigVersion = _newConfigVersion;

            _jsonConfig.SetData(_settings);

            // Backup existing config before saving
            Files.CreateBackup(_settingsConfig, 3);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_settingsConfig, json);
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
            if (track == null) return;

            //Make sure all is unchecked
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
            var index = playListBox.SelectedIndex;
            var track = _playlistManager.Get(index);

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
        private void ClearPlaylistButton_Click(object sender, EventArgs e) { DisposeCSCore(); ResetUIText(); _playlistManager.Clear(); }

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
                // Expecting format: "Field|Order"
                var parts = tag.Split('|');
                if (parts.Length == 2)
                {
                    string sortField = parts[0];
                    bool descending = bool.TryParse(parts[1], out var desc) && desc;

                    DebugUtils.Log("Sort Playlist", Name, $"Sorting Playlist by: {sortField} | Descending: {descending}");

                    switch (sortField?.ToLowerInvariant())
                    {
                        case "artist":
                            SortPlaylist(t => t.Artist, descending);
                            Debug.WriteLine($"Sorting Playlist by: Artist | Descending: {descending}");
                            break;
                        case "title":
                            SortPlaylist(t => t.Title, descending);
                            Debug.WriteLine($"Sorting Playlist by: Title | Descending: {descending}");
                            break;
                        case "album":
                            SortPlaylist(t => t.Album, descending);
                            Debug.WriteLine($"Sorting Playlist by: Album | Descending: {descending}");
                            break;
                        case "playcount":
                            SortPlaylist(t => t.PlayCount, descending);
                            Debug.WriteLine($"Sorting Playlist by: PlayCount | Descending: {descending}");
                            break;
                        case "lastplayed":
                            SortPlaylist(t => t.LastPlayed ?? DateTime.MinValue, descending);
                            Debug.WriteLine($"Sorting Playlist by: Artist | Descending: {descending}");
                            break;
                        case "liked":
                            SortPlaylist(t => t.Liked, descending);
                            Debug.WriteLine($"Sorting Playlist by: Liked | Descending: {descending}");
                            break;
                        case "disliked":
                            SortPlaylist(t => t.Disliked, descending);
                            Debug.WriteLine($"Sorting Playlist by: Disliked | Descending: {descending}");
                            break;
                        case "rating":
                            SortPlaylist(t => t.RatingScore, descending);
                            Debug.WriteLine($"Sorting Playlist by: Rating | Descending: {descending}");
                            break;
                        default:
                            DebugUtils.Log("Sort Playlist", Name, $" Unknown Sortfield: {sortField}");
                            Debug.WriteLine($" Unknown Sortfield: {sortField}");
                            break;
                    }
                }
                else
                {
                    DebugUtils.Log("Sort Playlist", Name, $"Invalid Tag Format: {tag}");
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
        private void LikeMenuButton_Click(object sender, EventArgs e) { UserRateTrack(playListBox.SelectedIndex, "like"); }

        /// <summary>
        /// Handles the click event for the Neutral menu button.
        /// </summary>
        /// <remarks>This method updates the user rating for the currently selected item in the playlist to "neutral."
        /// Ensure that <see cref="playListBox.SelectedIndex"/> is valid before invoking this method.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void NeutralMenuButton_Click(object sender, EventArgs e) { UserRateTrack(playListBox.SelectedIndex, "neutral"); }

        /// <summary>
        /// Handles the click event for the "Dislike" menu button.
        /// </summary>
        /// <remarks>This method updates the rating of the currently selected track in the playlist to
        /// "dislike." Ensure that a valid track is selected in the playlist before invoking this method.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click action.</param>
        private void DislikeMenuButton_Click(object sender, EventArgs e) { UserRateTrack(playListBox.SelectedIndex, "dislike"); }


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
            DebugUtils.Log("Volume Setter", this.AccessibleName, $"Volume Level {Volume_Slider.Value / 100f}");
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
        private void PlayList_SelectedIndexChanged(object sender, EventArgs e) { if (playListBox.SelectedIndex != -1) playListBox.EnsureVisible(playListBox.SelectedIndex); }

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
            if (e.Control && e.KeyCode == Keys.L) { UserRateTrack(playListBox.SelectedIndex, "like"); return; }
            else if (e.Control && e.KeyCode == Keys.D) { UserRateTrack(playListBox.SelectedIndex, "dislike"); return; }
            else if (e.Control && e.KeyCode == Keys.N) { UserRateTrack(playListBox.SelectedIndex, "neutral"); return; }
            /*
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
            */
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

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            HandleDroppedFiles(droppedFiles);
        }

        #endregion Listbox

        #region ComboBoxs

        /// <summary>
        /// Handles the event when the selected index of the audio device list changes.
        /// </summary>
        /// <remarks>Updates the internal audio device ID list to match the selected index of the audio
        /// device list and changes the audio device to the newly selected device.</remarks>
        /// <param name="sender">The source of the event, typically the audio device list control.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void AudioDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DebugUtils.Log("Selected Index Change", "Audio Device List", $"{AudioDeviceList.SelectedIndex}");
            if (AudioDeviceList.SelectedIndex != -1) //Make sure it's always a valid selection
                _audioDeviceIDList.SelectedIndex = AudioDeviceList.SelectedIndex;
            ChangeAudioDevice(_audioDeviceIDList.SelectedItem.ToString());
        }
        private void PlayListOptions_SelectedIndexChanged(object sender, EventArgs e) { }

        #endregion ComboBoxs

        #region Timer

        /// <summary>
        /// Handles the tick event of the play timer, updating the trackbar and advancing to the next track if
        /// necessary.
        /// </summary>
        /// <remarks>This method updates the trackbar position if the slider is not being dragged. If the
        /// current track has ended and the user has not stopped playback, it advances to the next track.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (!Tracking_Slider.Dragging)
            {
                UpdateTrackbar();
                if (_trackEnd && !_userStopped)
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
            //If playback is active ask user if they are sure they want to close
            if (_soundOut?.PlaybackState == PlaybackState.Playing || !_disposed)
            {
                if (ThemableMessageBox.Show("Are you sure you want to close", "Close", MessageBoxButtons.YesNo) == DialogResult.No) //If the user said no to close cancel closing else clean up steam and close form
                { e.Cancel = true; return; }
                Stop();
                DisposeCSCore();

            }
        }

        #endregion Form Events

        #endregion UI Event Handlers

        #endregion Private Functions

        #region Websocket integration

        /// <summary>
        /// Represents a WebSocket behavior for broadcasting messages to connected clients.
        /// </summary>
        /// <remarks>This class extends the <see cref="WebSocketBehavior"/> to provide functionality for sending broadcast
        /// messages to clients upon connection. It sends a welcome message to the client when the WebSocket connection is
        /// successfully opened.</remarks>
        internal class Broadcast : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                base.OnOpen();
                if (this.Context.WebSocket.ReadyState == WebSocketState.Open)
                {
                    Send($"This is a Broadcast channel stay connected to see what's playing");
                    DebugUtils.Log("Now Playing - Websocket", "OnOpen", "Sent welcome message to client");
                }
                else
                {
                    DebugUtils.Log("Now Playing - Websocket", "OnOpen", "Unable to send welcome message");
                }
            }
        }

        /// <summary>
        /// Represents a WebSocket behavior that processes incoming messages as commands and sends back execution
        /// results.
        /// </summary>
        /// <remarks>The <see cref="CommandExecutor"/> class listens for messages over a WebSocket
        /// connection. Upon receiving a message, it attempts to handle the message as a command using the <see
        /// cref="MP3PlayerV2"/> instance. It logs the process and sends a response indicating whether the command was
        /// executed successfully or failed.</remarks>
        internal class CommandExecutor : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                var form = MP3PlayerV2.Instance;
                if (form == null) return;

                bool succeeded = form.HandleCommand(e.Data);

                if (succeeded)
                {
                    DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Received Message: " + e.Data);

                    if (this.Context.WebSocket.ReadyState == WebSocketState.Open)
                    {
                        //          Send(_commandResponse);
                        DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Sent confirmation response message");
                    }
                    else
                    {
                        DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Unable to send message back");
                    }
                }
                else
                {
                    DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Received Message: " + e.Data);
                    if (this.Context.WebSocket.ReadyState == WebSocketState.Open)
                    {

                        //        Send(_commandResponse);
                        DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Sent failure response message");
                    }
                    else
                    {
                        DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Unable to send message back");
                    }
                }

                Sessions.Broadcast(_commandResponse);
            }
        }

        /// <summary>
        /// Constructs a JSON response message indicating the result of an operation.
        /// </summary>
        /// <param name="success">A boolean value indicating whether the operation was successful. <see langword="true"/> for success;
        /// otherwise, <see langword="false"/>.</param>
        /// <param name="message">A descriptive message providing additional information about the operation's result.</param>
        /// <param name="data">Optional. Additional data to include in the response. Can be <see langword="null"/> if no additional data is
        /// provided.</param>
        internal static void BuildResponseMessage(bool success, string message, object data = null)
        {
            string Result = success ? "Success" : "Fail";

            var msg = new { Result, Message = message, Data = data };

            _commandResponse = JsonSerializer.Serialize(msg, _jsonOption);
        }

        /// <summary>
        /// Starts the WebSocket server at the specified address and port.
        /// </summary>
        /// <remarks>This method initializes the WebSocket server and adds services for handling WebSocket
        /// connections. It attempts to start the server and logs the status. If the server fails to start, it disables
        /// the auto-start server feature and saves the settings. The server will continue running until the application sets
        /// the running flag to false.</remarks>
        /// <param name="address">The IP address or hostname where the server will listen for incoming connections.</param>
        /// <param name="port">The port number on which the server will listen for incoming connections.</param>
        private void StartServer(string address, string port)
        {
            string ep;
            if (_settings.WebSocket.EndPoint.StartsWith('/'))
            { ep = _settings.WebSocket.EndPoint; }
            else
            { ep = $"/{_settings.WebSocket.EndPoint}"; }

            //_server = new WebSocketServer("ws://" + address + ":" + port);
            _server = new WebSocketServer("ws://" + address + ":" + port);
            _server.AddWebSocketService<CommandExecutor>(ep);
            _server.AddWebSocketService<Broadcast>("/nowplaying");
            DebugUtils.Log("Websocket", "Add Service", $"{ep}");
            DebugUtils.Log("Websocket", "Add Service", $"/nowplaying");

            try
            {
                _server.Start();
            }
            catch
            {
                _running = false;
                DebugUtils.Log("WebSocket Server", "Start", $"Unable to start Websocket server ws://{address}:{port}{ep} - Turning of Auto-Start");
                _autoStart = false;
                SaveSettings();
                ThemableMessageBox.Show($"WebSocket Server failed to start - check for another websocket sever running with the same details \n\n ws://{address}:{port}{ep} \n\n Auto-Start has been turned off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            if (_server.IsListening)
            {
                DebugUtils.Log("Websocket", "Start", "Server Started with " + "ws://" + address + ":" + port);
            }
            while (_running)
            {
                Thread.Sleep(100); // Keep the thread alive
            }

            DebugUtils.Log("WebSocket Server", "Stop", "Web Socket Server has stopped");
            _server.Stop();

        }

        /// <summary>
        /// Starts a new background thread to run the server if it is not already running.
        /// </summary>
        /// <remarks>This method initializes and starts a server thread using the specified WebSocket
        /// address and port. The server runs in the background, allowing the main application to continue
        /// executing.</remarks>
        internal void StartServerThread()
        {
            if (_running) return;
            _serverThread = new Thread(() => StartServer(_settings.WebSocket.Address, _settings.WebSocket.Port.ToString())) { IsBackground = true };
            //_serverThread = new Thread(() => StartServer(_webSocketAddress, _webSocketPort.ToString())) { IsBackground = true };
            _serverThread.Start();
            _running = true;
        }

        /// <summary>
        /// Stops the server thread and releases associated resources.
        /// </summary>
        /// <remarks>This method sets the running state to false, waits for the server thread to
        /// terminate,  and stops the server. Ensure that the server is running before calling this method.</remarks>
        internal void StopServerThread()
        {
            _running = false;
            _serverThread?.Join(); // Wait for the thread to exit
            _server?.Stop();
        }

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
            var context = new CommandContext
            {
                Invoke = action => this.Invoke(action),
                Respond = BuildResponseMessage,
                GetPlaylistCount = () => playListBox.Items.Count,
                Play = Play,
                Pause = Pause,
                Stop = Stop,
                Next = (automatic) => NextTrack(automatic),
                Previous = PreviousTrack,
                GetVolumeLevel = _volumeLevel,
                Volume = SetVolume,

                Shuffle = ShufflePlaylist,
                CountByName = CountTrackByName,
                CountByPlayData = (playdata) => CountTrackByPlayData(playdata),

                GetAudioDeviceCount = () => AudioDeviceList.Items.Count,
                GetAudioDeviceNameAt = i => AudioDeviceList.Items[i]?.ToString() ?? string.Empty,
                SetAudioDeviceIndex = i => AudioDeviceList.SelectedIndex = i,
                GetSelectedAudioDevice = () => AudioDeviceList.SelectedItem?.ToString() ?? "Unknown",

                GetPlaylistModeCount = () => playList_Options.Items.Count,
                GetPlaylistModeNameAt = i => playList_Options.Items[i]?.ToString() ?? "",
                SetPlaylistModeIndex = i => playList_Options.SelectedIndex = i,
                GetSelectedPlaylistMode = () => playList_Options.SelectedItem?.ToString() ?? "",

                SelectTrackByIndex = i => playListBox.SelectedIndex = i,
                SelectTrackByName = name => SelectTrackByName(name),
                GetSelectedTrackName = () => playListBox.SelectedItem?.ToString() ?? "Unknown",
                SelectRandomTrack = () => GetRandomTrack(),

                IsPlaylistEmpty = () => playListBox.Items.Count == 0,
                GetPlaylistTracks = () => _playlistManager.Tracks,
                NormalizeText = s => NormalizeText(s),

                GetPlaybackState = () => _soundOut?.PlaybackState ?? PlaybackState.Stopped,
                GetCurrentTrack = Cur_Track_Label.Text,

                BuildSearchRegex = BuildSearchRegex,
                SortPlaylist = (selector, desc) => SortPlaylist(selector, desc),
                QueueTrackByName = name => QueueTrackByName(name),
                GetQueuedTracks = new Queue<string>(_trackQueue),
                ClearQueue = _trackQueue.Clear,
                ResetStat = async (range, stat) => await ResetTrackStatsAdaptiveAsync(range, stat),

            };

            return _dispatcher.Dispatch(message, context, _caseInsensitiveOptions);
        }

        #endregion Websocket integration

    }

}