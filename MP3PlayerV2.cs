using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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

#nullable enable
        private WasapiOut _soundOut;
        private IWaveSource _waveSource;
        private VolumeSource _volumeSource;

        private string? _lastEta = null;

#nullable disable

        public static MP3PlayerV2 Instance { get; private set; }

        private bool _disposed = true;
        private bool _userStopped = false;
        private bool _trackEnd = false;

        private int _volumeLevel = 100;
        private static string _currentTrack;
        private readonly ListBox _audioDeviceIDList = new();

        //WebSocket Server
        private string _webSocketAddress = "127.0.0.1";
        private int _webSocketPort = 8080;
        private string _webSocketEndPoint = "/";
        private bool _autoStart = false;

        private static string _commandResponse = string.Empty;
        private WebSocketServer _server;
        private Thread _serverThread;
        private bool _running = false;
        public bool GetWssStatus { get => _running; }
        private readonly CommandDispatcher _dispatcher = new();

        private static readonly Random _rng = new();
        private readonly PlaylistManager _playlistManager = new();
        private readonly Stack<int> _trackHistory = new();

        //Configuration
        private static readonly JsonSerializerOptions _jsonOption = new () { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        private static readonly JsonSerializerOptions _caseInsensitiveOptions = new(){PropertyNameCaseInsensitive = true};
        internal AppSettings _settings;
        private JSON<AppSettings> _jsonConfig;
        private readonly string _customThemeConfig = "Config/CustomTheme.json";
        private readonly string _settingsConfig = "Config/Settings.json";

        private static readonly HashSet<string> SupportedAudioExtensions = new(StringComparer.OrdinalIgnoreCase) { ".flac", ".m4a", ".mp2", ".mp3", ".wav", ".wma" };
        private static readonly HashSet<string> SupportedPlaylistExtensions = new(StringComparer.OrdinalIgnoreCase) { ".m3u", ".m3u8" };

        internal int VolumeLevel { get => _volumeLevel;}
        #endregion Fields

        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MP3PlayerV2"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the MP3 player by initializing components, registering the
        /// form for theming,  and loading audio devices, themes, and settings. It also subscribes to playlist change
        /// events to update  the playlist UI and optionally starts the server thread if auto-start is
        /// enabled.</remarks>
        public MP3PlayerV2()
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
                    playListBox.Items.Add(track); // Track.ToString()
                if (playListBox.Items.Count <= 0)
                    playListBox.SelectedIndex = -1;
            };
        
            if (_autoStart)
            {
                StartServerThread();
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

            DebugUtils.Log("Initilize CSCore", Name, $"File Path: {filePath}");

            if (System.IO.File.Exists(filePath))
            {
                var sampleSource = CodecFactory.Instance.GetCodec(filePath)
                    .ToSampleSource();

                _volumeSource = new VolumeSource(sampleSource);

                _waveSource = _volumeSource.ToWaveSource(16);
            }
            else
            {
                DebugUtils.Log("Initilize CSCore", Name, "File not found.");
                return false;
            }

            _soundOut = (string.IsNullOrEmpty(deviceID))
                ? new WasapiOut()
                : new WasapiOut { Device = new MMDeviceEnumerator().GetDevice(deviceID) };

            DebugUtils.Log("Initilize CSCore", Name, $"Device ID: {deviceID}");
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
                    //Check to see if the currently played track is the same as the slected track in the play list
                    if (!string.IsNullOrWhiteSpace(_currentTrack) || !_currentTrack.Contains(playListBox.SelectedItem.ToString()))
                    {
                        //Stop Then process the new track
                        Stop();
                        DebugUtils.Log("No Track Match", "Play", $"Match Not Found: {_currentTrack} is not {playListBox.SelectedItem}");
                    }
                    else
                    {
                        DebugUtils.Log("Track Match", "Play", $"Match Found: {_currentTrack}");
                        //if it's the same do nothing and exit here
                        return;
                    }
                }

                //Check to see if there something paused and resume this
                if (_soundOut?.PlaybackState == PlaybackState.Paused)
                {
                    _soundOut?.Resume();
                    this.Text = $"MP3 Player - {Cur_Track_Label.Text}";
                    DebugUtils.Log("Play", "Pause-Resumed", $"{_currentTrack}");
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
                    _currentTrack = track.FilePath;
                if (InitializeCSCore(track.FilePath, deviceID))
                {
                    Cur_Track_Label.Text = track.ToString();

                    this.Text = $"MP3 Player - {Cur_Track_Label.Text}";

                    //Configure Trackbar to Music File
                    Tracking_Slider.Maximum = (int)_waveSource?.GetLength().TotalSeconds;
                    Tracking_Slider.Value = 0;

                    _soundOut?.Play();
                    PlayTimer.Start();
                    track.PlayCount++;
                    track.LastPlayed = DateTime.UtcNow;
                    var dispTime = $"{track.LastPlayed?.ToLocalTime().ToShortDateString()}-{track.LastPlayed?.ToLocalTime().ToShortTimeString()}";

                    //Save stats to database
                    try
                    {
                        TrackDatabase.SaveStats(track);
                    }
                    catch (Exception ex) { DebugUtils.Log("Play", "Save Stats", $"Saving to data base error: {ex.Message}"); }

                    DebugUtils.Log("Play", Text, $"Current Track: {_currentTrack}");
                    DebugUtils.Log("Tracking", "Play", $"{_waveSource?.GetLength()}");

                    if (!_running) { DebugUtils.Log("Play", "WebSocket", "Websocket isn't running no broadcast sent"); return; }
                    var nowPlayingInfo = new { NowPlaying = track.ToString(), track.PlayCount, track.LastPlayed, LocalTime = dispTime };
                    string jsonMessage = JsonSerializer.Serialize(nowPlayingInfo, _jsonOption);
                    _server?.WebSocketServices["/nowplaying"].Sessions.Broadcast(jsonMessage);
                }
                else
                {
                    string message = $"Could Not find the file to play:\n{track.FilePath}";
                    //Send a message to the user stating the file could not be found and autocloses the message box after 10 seconds
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
                    PlayTimer.Start();
                    return;
                }

                if (_soundOut?.PlaybackState == PlaybackState.Playing)
                {
                    _soundOut?.Pause();
                    this.Text = $"MP3 Player - {Cur_Track_Label.Text} - Paused";
                    PlayTimer.Stop();
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
            if (!automatic)
            {
                var track = _playlistManager.Get(playListBox.SelectedIndex);
                if (track != null)
                {
                    track.TimesSkipped++;

                    try
                    {
                        TrackDatabase.SaveStats(track);
                    }
                    catch (Exception ex) 
                    {
                        DebugUtils.Log("Play", "Save Stats", $"Saving to data base error: {ex.Message}");
                    }
                }
            }

            if (playListBox.SelectedIndex >= 0)
            {
                _trackHistory.Push(playListBox.SelectedIndex);
            }

            bool endOfPlaylist = false;

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
                            PlayTimer.Stop();
                            this.Text = "MP3 Player";
                            Cur_Track_Label.Text = "";
                            playListBox.SelectedIndex = -1;
                            Tracking_Slider.Value = 0;
                            endOfPlaylist = true;
                            _soundOut?.Stop();
                            DisposeCSCore();
                        }
                        else playListBox.SelectedIndex++;
                        break;
                    }
            }

            if (!endOfPlaylist)
            {
                playListBox.EnsureVisible(playListBox.SelectedIndex);
                _soundOut?.Stop();
                Play();
            }
        }

        /// <summary>
        /// Moves to the previous track in the playlist or restarts the current track if it has been playing for more
        /// than 5 seconds.
        /// </summary>
        /// <remarks>If the current track has been playing for more than 5 seconds, it will be restarted.
        /// Otherwise, the method will attempt to move to the previous track in the history or the playlist.</remarks>
        private void PreviousTrack()
        {
            if (_waveSource != null && _waveSource.GetPosition().TotalSeconds > 5)
            {
                _waveSource.SetPosition(TimeSpan.Zero);
                return;
            }

            if (_trackHistory.Count > 0)
            {
                int previousIndex = _trackHistory.Pop();
                playListBox.SelectedIndex = previousIndex;
            }
            else if (playListBox.SelectedIndex > 0)
            {
                playListBox.SelectedIndex--;
            }
            else
            {
                return;
            }

            playListBox.EnsureVisible(playListBox.SelectedIndex);
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
                bytePosition = Math.Min(bytePosition, _waveSource.Length); // clamp
                _waveSource.Position = bytePosition;
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
            // InitilizeCSCore will setup to new device on setup
            if (_soundOut == null || _disposed) { return; }
            var currentPosition = _waveSource?.Position ?? 0;
            _soundOut?.Stop();
            DisposeCSCore();

            InitializeCSCore(_currentTrack, deviceId);

            _waveSource.Position = currentPosition;
            _soundOut.Play();
            //   _deviceChanged = true;
        }

        #endregion Device Management

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
        private static Track? PickSmartTrack(IEnumerable<Track> tracks)
        {
            var trackslist = tracks.ToList();
            var unplayed = trackslist.Where(t => (t.PlayCount ?? 0) == 0).ToList();
                if (unplayed.Count > 0)
                    return PickRandom(unplayed);

            int minPlays = trackslist.Min(t => t.PlayCount ?? 0);
            var leastPlayed = trackslist.Where(t => (t.PlayCount ?? 0) == minPlays).ToList();

            return PickRandom(leastPlayed);
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

        /// <summary>
        /// Searches for audio files within the specified folder and its subdirectories.
        /// </summary>
        /// <remarks>The method searches recursively through all subdirectories of the specified folder.
        /// Only files with extensions that are included in the <c>SupportedAudioExtensions</c> collection are
        /// considered audio files.</remarks>
        /// <param name="folderPath">The path to the folder where the search for audio files will be conducted. Must be a valid directory path.</param>
        /// <returns>A list of file paths representing the audio files found. Returns an empty list if no audio files are found
        /// or if the directory does not exist.</returns>
        private static List<string>FindAudioFile(string folderPath)
        {
            var list = new List<string>();

            try
            {
                if (!Directory.Exists(folderPath))
                    return list;

                foreach (var file in Directory.EnumerateFiles(folderPath, "*.*" , SearchOption.AllDirectories))
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
#nullable disable
        /// <summary>
        /// Counts the number of tracks in the playlist that have not been played.
        /// </summary>
        /// <remarks>This method evaluates the play count of each track in the playlist and determines how
        /// many tracks have a play count of zero. The result is stored in a command response string indicating the
        /// number of unplayed tracks.</remarks>
        private void CountUnplayed()
        {
            var cachedlist = _playlistManager.Tracks.ToList();
            var count = cachedlist.Where(t => (t.PlayCount ?? 0) == 0).ToList().Count;
            BuildResponseMessage(true, $"{count} Unplayed track(s) found in the playlist");
        }
                
        /// <summary>
        /// Counts the number of tracks in the playlist whose names contain the specified substring.
        /// </summary>
        /// <remarks>This method updates the command response with the count of matching tracks found in
        /// the playlist.</remarks>
        /// <param name="name">The substring to search for within track names. The search is case-insensitive.</param>
        private void CountTrackByName(string name)
        {
            var count = 0;

            for (int i = 0; i < _playlistManager.Count; i++)
            {
                var track = _playlistManager.Get(i);

                if (track != null)
                {
                    string trackname = NormalizeText(track.ToString());
                    string normalizedSearch = NormalizeText(name);

                    if (trackname.Contains(normalizedSearch))
                        count++;

                }
            }
            BuildResponseMessage(true, $"{count} matching item(s) found in the playlist");
        }

        /// <summary>
        /// Selects a track from the playlist by searching for a name that matches the specified search term.
        /// </summary>
        /// <remarks>This method searches the playlist starting from the currently selected track and
        /// wraps around if necessary. If a matching track is found, it updates the selected index in the playlist,
        /// ensures the track is visible, and starts playback. If no match is found, the selection remains
        /// unchanged.</remarks>
        /// <param name="searchTerm">The term to search for within the track names. Cannot be null or empty.</param>
        private void SelectTrackByName(string searchTerm)
        {
            int currentIndex = playListBox.SelectedIndex;
            for (int offset = 1; offset < _playlistManager.Count; offset++)
            {
                int index = (currentIndex + offset) % _playlistManager.Count;
                var track = _playlistManager.Get(index);
                if (track != null)
                {
                    string trackName = NormalizeText(track.ToString());
                    string normalizedSearch = NormalizeText(searchTerm);

                    if (trackName.Contains(normalizedSearch))
                    {
                        playListBox.SelectedIndex = index;
                        playListBox.EnsureVisible(index);
                        BuildResponseMessage(true,  $"Match found: {track}");
                        Stop();
                        Play();
                        return;
                    }
                }
            }
            BuildResponseMessage(true, $"No matching item found in the playlist");
        }

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
            playListBox.EnsureVisible(playListBox.SelectedIndex);
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
            playListBox.EnsureVisible(Number);
        }


        /// <summary>
        /// Adds music tracks to the playlist from the specified files or prompts the user to select files if none are
        /// provided.
        /// </summary>
        /// <remarks>This method processes each file to extract track metadata and adds the resulting
        /// tracks to the playlist.  It utilizes concurrent processing to improve performance, with a maximum
        /// concurrency level based on the number of processor cores.</remarks>
        /// <param name="droppedItems">An optional array of file paths to add. If null or empty, the user will be prompted to select files.</param>
#nullable enable
        private async void AddItem(string[]? droppedItems = null)
        {
            string[]? files = droppedItems;
            if (files == null || files.Length == 0)
            {
                files = Files.ChooseFiles("", "Music Files|*.flac;*.m4a;*.mp2;*.mp3;*.wav;*.wma");
                if (files == null || files.Length == 0) return;
            }
            var dialog = new ThemableProcessingDialog("Adding tracks...");
            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = new(
                this.Location.X + (this.Width - dialog.Width) / 2,
                this.Location.Y + (this.Height - dialog.Height) / 2
            );

            dialog.Show(this);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int total = files.Length;
            int progress = 0;

            int maxConcurrency = Math.Min(Environment.ProcessorCount * 2, 20);
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
                            return (Index: index, Track: null);
                        var tagFile = TagLib.File.Create(file);
                        var track = new Track
                        {
                            FilePath = file,                                                 
                            Title = !string.IsNullOrEmpty(tagFile.Tag.Title) ? tagFile.Tag.Title.Replace("\r", "").Replace("\n", "") : Path.GetFileNameWithoutExtension(file),

                            Artist = tagFile.Tag.Performers != null && tagFile.Tag.Performers.Length > 0 ? string.Join("/", tagFile.Tag.Performers) : "Unknown Artist",
                            Album = !string.IsNullOrEmpty(tagFile.Tag.Album) ? tagFile.Tag.Album : "",
                            DurationSeconds = (int)tagFile.Properties.Duration.TotalSeconds,
                            Hash = TrackDatabase.ComputeFileHash(file)
                        };

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

                        if (currentProgress % 50 == 0 || currentProgress == total)
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

            if (playListBox.Items.Count > 0 && playListBox.SelectedIndex == -1)
                playListBox.SelectedIndex = playListBox.Items.Count - 1;

            dialog.SetCompleted($"Added {newTracks.Count} track(s).");
            dialog.CloseAfter(1000);
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

            var dialog = new ThemableProcessingDialog("Saving Playlist...", showProgress: true, showCancelButton: false);
            dialog.StartPosition = FormStartPosition.Manual;
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
        /// Loads a playlist from a specified file or prompts the user to select a file if none is provided.
        /// </summary>
        /// <remarks>This method updates the playlist display and attempts to restore the previously
        /// selected item if it exists in the new playlist.</remarks>
        /// <param name="droppedItem">The path to the playlist file to load. If <see langword="null"/> or empty, the user will be prompted to
        /// select a file.</param>
        private async void LoadPlaylist(string droppedItem = null)
        {
            string lastItem = null;
            if (playListBox.SelectedIndex != -1)
            { 
                lastItem = playListBox.SelectedItem.ToString();
            }

            string loadFileName = droppedItem;
            if (loadFileName == null || loadFileName.Length == 0)
            {
                loadFileName = Files.ChooseFile("", "M3U Playlists|*.m3u;*.m3u8", "Open Playlist");
                if (string.IsNullOrWhiteSpace(loadFileName)) return;
            }

            var dialog = new ThemableProcessingDialog("Loading Playlist");
            dialog.StartPosition = FormStartPosition.Manual;
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
                // If the last index is not -1 
                if (lastItem != null)
                {
                    for (int i = 0; i < playListBox.Items.Count - 1; i++)
                    {
                        if (playListBox.Items[i].ToString().Contains(lastItem))
                            playListBox.SelectedIndex = i;
                    }
                }
                else
                {
                    playListBox.SelectedIndex = 0;
                }
            }
            playListBox.EnsureVisible(playListBox.SelectedIndex);

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
                //Update WebSocket Info
                _webSocketAddress = _settings.WebSocketAddress;
                _webSocketPort = _settings.WebSocketPort;
                _webSocketEndPoint = _settings.WebSocketEndPoint;
                _autoStart = _settings.AutoStart;
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
        /// Loads the application settings from a configuration file.
        /// </summary>
        /// <remarks>If the configuration file exists, this method loads the settings into the
        /// application, updating various properties and controls with the loaded values. If the file does not exist, it
        /// initializes the settings with default values and creates a new configuration file.</remarks>
        private void LoadSettings()
        {
            _jsonConfig = new JSON<AppSettings>(_settingsConfig);

            if (File.Exists(_settingsConfig))
            {
                _jsonConfig.Load();
                _settings = _jsonConfig.Data;

                _volumeLevel = _settings.VolumeLvl;
                AudioDeviceList.SelectedItem = _settings.AudioDevice;
                playList_Options.SelectedItem = _settings.PlayListMode;
                _webSocketAddress = _settings.WebSocketAddress;
                _webSocketPort = _settings.WebSocketPort;
                _webSocketEndPoint = _settings.WebSocketEndPoint;
                _autoStart = _settings.AutoStart;

                SetVolume(_volumeLevel);
                

            }
            else
            {
                DebugUtils.Log("Load Settings", Name, "File Not found creating new file");
                //Initilize _settings to defaults for saving
                _settings = new();
                SaveSettings();
            }
        }

        /// <summary>
        /// Saves the current application settings to a configuration file.
        /// </summary>
        /// <remarks>This method updates the settings object with the current application state, including
        /// volume level, selected audio device, playlist mode, and WebSocket configuration. It then creates a backup of
        /// the existing settings configuration and saves the updated settings to a JSON file.</remarks>
        private void SaveSettings()
        {
            if (_settings == null)
            {
                DebugUtils.Log("Save Settings", Name, "_settings is NULL Aborting");
                return;
            }
            _settings.VolumeLvl = _volumeLevel;
            if (AudioDeviceList.SelectedIndex == -1 && AudioDeviceList.Items.Count > 0) { AudioDeviceList.SelectedIndex = 0; }
            _settings.AudioDevice = AudioDeviceList.SelectedItem.ToString(); 
            if (playList_Options.SelectedIndex == -1 && playList_Options.Items.Count >0 ) { playList_Options.SelectedIndex = 0; } 
            _settings.PlayListMode = playList_Options.SelectedItem.ToString();
            _settings.WebSocketAddress = _webSocketAddress;
            _settings.WebSocketPort = _webSocketPort;
            _settings.WebSocketEndPoint = _webSocketEndPoint;
            _settings.AutoStart = _autoStart;

            _jsonConfig.SetData(_settings);

            Files.CreateBackup(_settingsConfig, 3);
            _jsonConfig.Save();
        }      

        #endregion Configuration

        #region UI Event Handlers

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
        private void ClearPlaylistButton_Click(object sender, EventArgs e) { DisposeCSCore(); ResetUIText(); _playlistManager.Clear();  }

        /// <summary>
        /// Handles the Click event of the Exit button, closing the current form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ExitButton_Click(object sender, EventArgs e) { this.Close(); }

        /// <summary>
        /// Handles the click event for the sort playlist menu button, sorting the playlist based on the specified
        /// criteria.
        /// </summary>
        /// <remarks>The <paramref name="sender"/> is expected to have a tag in the format
        /// "Field|Descending", where "Field" specifies the playlist attribute to sort by (e.g., "Artist", "Title",
        /// "PlayCount", "LastPlayed"), and "Descending" is a boolean indicating the sort order. If the tag format is
        /// invalid or the field is unrecognized, the method logs an appropriate message.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="ToolStripMenuItem"/> with a <see cref="string"/> tag.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void SortPlaylistMenuButton_Click(object sender, EventArgs e) 
        {
           if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string tag)
            {
                // Expecting format: "Field|Descending"
                var parts = tag.Split('|');
                if (parts.Length == 2)
                {
                    string sortField = parts[0];
                    bool descending = bool.TryParse(parts[1], out var desc) && desc;

                    DebugUtils.Log("Sort Playlist", Name, $"Sorting Playlist by: {sortField} | Descending: {descending}");

                    switch (sortField)
                    {
                        case "Artist":
                            SortPlaylist(t => t.Artist, descending);
                            break;
                        case "Title":
                            SortPlaylist(t => t.Title, descending);
                            break;
                        case "PlayCount":
                            SortPlaylist(t => t.PlayCount, descending);
                            break;
                        case "LastPlayed":
                            SortPlaylist(t => t.LastPlayed ?? DateTime.MinValue, descending);
                            break;
                        default:
                            DebugUtils.Log("Sort Playlist", Name, $" Unknown Sortfield: {sortField}");
                            break;
                    }
                }
                else
                {
                    DebugUtils.Log("Sort Playlist", Name, $"Invalid Tag Format: {tag}");
                }
            }
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
        /// Handles key press events for the playlist, enabling playback control through keyboard shortcuts.
        /// </summary>
        /// <remarks>This method allows users to control playback using specific keys when the playlist
        /// control is focused. Supported keys include Enter for play, Space for pause, Delete for removing an item, and
        /// media keys for track navigation and playback control.</remarks>
        /// <param name="sender">The source of the event, typically the playlist control.</param>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data, including the key pressed.</param>
        private void PlayList_KeyDown(object sender, KeyEventArgs e) 
        {
            //These reqire the listbox control to be focused to accept these controls
            switch (e.KeyCode)
            {
                case Keys.Enter: { Play(); break; }
                case Keys.Space: { Pause(); break; }
                case Keys.Delete: { RemoveItem(); break;}
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
        /// Handles the drag-and-drop operation for the playlist, adding valid audio files or loading playlists.
        /// </summary>
        /// <remarks>This method processes files and directories dropped onto the playlist. It adds valid
        /// audio files to the playlist or loads a playlist file if no audio files are present. Supported file types are
        /// determined by the <c>SupportedAudioExtensions</c> and <c>SupportedPlaylistExtensions</c>
        /// collections.</remarks>
        /// <param name="sender">The source of the drag event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void PlayList_DragDrop(object sender, DragEventArgs e) 
        {

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> validAudioFiles = [];

            foreach (var path in droppedFiles)
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
                            LoadPlaylist(path);
                            return;
                        }
                    }
                }
            }

            if (validAudioFiles.Count > 0)
            {
                AddItem([.. validAudioFiles]);
            }

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
                    NextTrack(true);
                    _trackEnd = false;
                }
            }
        }

        #endregion Timer

        #region Form Events

        /// <summary>
        /// Handles the form closing event, prompting the user for confirmation if playback is active.
        /// </summary>
        /// <remarks>If playback is currently active or resources have not been disposed, the user is
        /// prompted to confirm the closure. If the user chooses not to close, the form closing is canceled.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            //If playback is active ask user if they are sure they want to close
            if (_soundOut?.PlaybackState == PlaybackState.Playing || !_disposed)
            {
                if (ThemableMessageBox.Show("Are you sure you want to close", "Close", MessageBoxButtons.YesNo) == DialogResult.No) //If the user said no to close cancel closing else cleanup steam and close form
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
                    Send($"This is a Broadcast channel stay connected to see whats playing");
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

               form.HandleCommand(e.Data);
               
                        Sessions.Broadcast(_commandResponse);

                /* if (succeeded)
                {
                    DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Received Message: " + e.Data);
                    if (this.Context.WebSocket.ReadyState == WebSocketState.Open)
                    {
                        Sessions.Broadcast(_commandResponse);
                        //Send($"Command Executed: {e.Data} {_commandResponce}");
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
                        Sessions.Broadcast(_commandResponse);
                        //Send($"Command Failed: {e.Data} {_commandResponce}");
                        DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Sent failure response message");
                    }
                    else
                    {
                        DebugUtils.Log("CommandExecuter - Websocket", "OnMessage", "Unable to send message back");
                    }
                }*/
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
        internal static void BuildResponseMessage(bool success, string message, string data = null)
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
            if (_settings.WebSocketEndPoint.StartsWith('/'))
            { ep = _settings.WebSocketEndPoint; }
            else
            { ep = $"/{_settings.WebSocketEndPoint}"; }

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
            _serverThread = new Thread(() => StartServer(_settings.WebSocketAddress, _settings.WebSocketPort.ToString())) { IsBackground = true };
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
                CountUnplayed = CountUnplayed,

                GetAudioDeviceCount = () => AudioDeviceList.Items.Count,
                GetAudioDeviceNameAt = i => AudioDeviceList.Items[i]?.ToString() ?? string.Empty,
                SetAudioDeviceIndex = i => AudioDeviceList.SelectedIndex = i,
                GetSelectedAudioDevice = () => AudioDeviceList.SelectedItem?.ToString() ?? "Unknown",

                GetPlaylistModeCount = () => playList_Options.Items.Count,
                GetPlaylistModeNameAt = i => playList_Options.Items[i]?.ToString() ?? "",
                SetPlaylistModeIndex = i => playList_Options.SelectedIndex = i,
                GetSelectedPlaylistMode = () => playList_Options.SelectedItem?.ToString() ?? "",

                SelectTrackByIndex = i => playListBox.SelectedIndex = i,
                EnsureTrackVisible = () => playListBox.EnsureVisible(playListBox.SelectedIndex),
                SelectTrackByName = name => SelectTrackByName(name),
                GetSelectedTrackName = () => playListBox.SelectedItem?.ToString() ?? "Unknown",
                SelectRandomTrack = () => GetRandomTrack(),

                IsPlaylistEmpty = () => playListBox.Items.Count == 0,
                GetPlaylistTracks = () => _playlistManager.Tracks,
                NormalizeText = s => NormalizeText(s), // your existing method
                SetCommandResponseJson = json => _commandResponse = json,
                JsonOptions = _jsonOption,


                GetPlaybackState = () => _soundOut?.PlaybackState ?? PlaybackState.Stopped,
                GetCurrentTrackIndex = () => playListBox.SelectedIndex,
                GetCurrentTrack = Cur_Track_Label.Text,

                SortPlaylist = (selector, desc) => SortPlaylist(selector, desc),
            };

            return _dispatcher.Dispatch(message, context, _caseInsensitiveOptions);
        }
        #endregion Websocket integration

    }

}