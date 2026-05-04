using BazthalLib;
using CSCore.SoundOut;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using System;

namespace MP3PlayerV2.Controllers
{
    /// <summary>
    /// Coordinates playback operations between UI and services.
    /// </summary>
    public class PlaybackController
    {
        private readonly AudioPlaybackService _audioPlayback;
        private readonly TrackNavigationService _trackNavigation;
        private readonly PlaylistManager _playlistManager;
        private readonly AudioDeviceService _audioDevices;
        private readonly AppSettings _settings;

        private bool _trackEnd = false;
        private bool _userSeeked = false;
        private Track? _currentTrack = null;
        private string _currentFilePath = string.Empty;
        private int _volumeLevel = 100;

        public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
        public event EventHandler<TrackChangedEventArgs>? TrackChanged;
        public event EventHandler<TrackEndedEventArgs>? TrackEnded;
        public event EventHandler<PositionUpdatedEventArgs>? PositionUpdated;

        public PlaybackController(
            AudioPlaybackService audioPlayback,
            TrackNavigationService trackNavigation,
            PlaylistManager playlistManager,
            AudioDeviceService audioDevices,
            AppSettings settings)
        {
            _audioPlayback = audioPlayback ?? throw new ArgumentNullException(nameof(audioPlayback));
            _trackNavigation = trackNavigation ?? throw new ArgumentNullException(nameof(trackNavigation));
            _playlistManager = playlistManager ?? throw new ArgumentNullException(nameof(playlistManager));
            _audioDevices = audioDevices ?? throw new ArgumentNullException(nameof(audioDevices));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _audioPlayback.PlaybackStateChanged += OnAudioPlaybackStateChanged;
            _audioPlayback.TrackEnded += OnAudioTrackEnded;
        }

        public Track? CurrentTrack => _currentTrack;
        public string CurrentFilePath => _currentFilePath;
        public PlaybackState State => _audioPlayback.State;
        public bool IsDisposed => _audioPlayback.IsDisposed;
        public TimeSpan Position => _audioPlayback.Position;
        public TimeSpan Duration => _audioPlayback.Duration;
        public bool CanSeek => _audioPlayback.CanSeek;
        public int VolumeLevel
        {
            get => _volumeLevel;
            set
            {
                _volumeLevel = value;
                _audioPlayback.VolumeLevel = value;
            }
        }

        private void OnAudioPlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs e)
        {
            PlaybackStateChanged?.Invoke(this, e);
        }

        private void OnAudioTrackEnded(object? sender, TrackEndedEventArgs e)
        {
            if (e.ReachedEnd)
            {
                _trackEnd = true;
                DebugUtils.Log("PlaybackController", "TrackEnded",
                    "Track reached end naturally", logLevel: DebugUtils.LogLevel.Info);
            }
            TrackEnded?.Invoke(this, e);
        }

        public bool Play(Track? track = null, int? selectedIndex = null, string? deviceId = null)
        {
            // Resume from pause
            if (_audioPlayback.State == PlaybackState.Paused && track == null)
            {
                _audioPlayback.Play();
                DebugUtils.Log("PlaybackController", "Play",
                    "Resumed from pause", logLevel: DebugUtils.LogLevel.Info);
                return true;
            }

            // Validate we have a track to play
            if (track == null)
            {
                if (_playlistManager.Count <= 0 || selectedIndex == null || selectedIndex < 0)
                    return false;
                track = _playlistManager.Get(selectedIndex.Value);
            }

            if (track == null) return false;

            // Get device ID if not provided
            if (string.IsNullOrEmpty(deviceId) && !string.IsNullOrEmpty(_audioDevices.LastSelectedDeviceId))
            {
                deviceId = _audioDevices.LastSelectedDeviceId;
            }

            // Initialize playback
            _currentFilePath = track.FilePath;

            if (!_audioPlayback.Initialize(_currentFilePath, deviceId))
            {
                DebugUtils.Log("PlaybackController", "Play",
                    $"Failed to initialize: {_currentFilePath}", logLevel: DebugUtils.LogLevel.Error);
                return false;
            }

            _currentTrack = track;
            _trackEnd = false;
            _audioPlayback.VolumeLevel = _volumeLevel;

            // Start playback
            _audioPlayback.Play();

            // Apply track rating for play start
            TrackRatingManager.ApplyPlayStart(track, _settings.TrackRating);
            track.LastPlayed = DateTime.UtcNow;

            // Save to database
            try
            {
                TrackDatabase.SaveStats(track);
            }
            catch (Exception ex)
            {
                DebugUtils.Log("PlaybackController", "Play",
                    $"Error saving stats: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
            }

            // Notify track changed
            TrackChanged?.Invoke(this, new TrackChangedEventArgs(track));

            DebugUtils.Log("PlaybackController", "Play",
                $"Playing: {track}", logLevel: DebugUtils.LogLevel.Info);

            return true;
        }

        public void Pause()
        {
            if (_audioPlayback.State == PlaybackState.Paused)
            {
                _audioPlayback.Play();
                return;
            }

            if (_audioPlayback.State == PlaybackState.Playing)
            {
                _audioPlayback.Pause();
            }
        }

        public void Stop()
        {
            if (_audioPlayback.IsDisposed) return;
            _audioPlayback.Stop();
        }

        public (Track? nextTrack, int nextIndex) NextTrack(
            int currentIndex,
            PlaylistOption option,
            SmartShuffleMode smartShuffleMode,
            bool automatic = false)
        {
            if (_playlistManager.Count <= 0)
            {
                DebugUtils.Log("PlaybackController", "NextTrack",
                    "No tracks in playlist", logLevel: DebugUtils.LogLevel.Info);
                return (null, -1);
            }

            // Handle skip rating if user manually skipped
            if (!automatic && _currentTrack != null && !_audioPlayback.IsDisposed)
            {
                TimeSpan pos = _audioPlayback.Position;
                TimeSpan len = _audioPlayback.Duration;

                if (IsSkipValid(pos, len))
                {
                    TrackRatingManager.ApplySkip(_currentTrack, _settings.TrackRating, pos.Seconds);
                    try { TrackDatabase.SaveStats(_currentTrack); }
                    catch (Exception ex)
                    {
                        DebugUtils.Log("PlaybackController", "NextTrack",
                            $"Error saving stats: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
                    }
                }
            }

            // Get next track from navigation service
            var (nextTrack, nextIndex) = _trackNavigation.GetNextTrack(
                _currentTrack,
                currentIndex,
                _playlistManager.Count,
                option,
                smartShuffleMode,
                i => _playlistManager.Get(i),
                () => _playlistManager.Tracks
            );

            // Handle repeat track
            if (option == PlaylistOption.RepeatTrack && _currentTrack != null)
            {
                TrackRatingManager.ApplyReplay(_currentTrack, _settings.TrackRating);
            }

            return (nextTrack, nextIndex);
        }

        public (Track? previousTrack, int previousIndex, bool shouldRestart) PreviousTrack(
            int currentIndex,
            double restartThreshold = 5.0)
        {
            if (_playlistManager.Count <= 0)
            {
                DebugUtils.Log("PlaybackController", "PreviousTrack",
                    "No tracks in playlist", logLevel: DebugUtils.LogLevel.Info);
                return (null, -1, false);
            }

            double currentPosition = _audioPlayback.IsDisposed ? 0 : _audioPlayback.Position.TotalSeconds;

            return _trackNavigation.GetPreviousTrack(
                currentPosition,
                currentIndex,
                _playlistManager.Count,
                i => _playlistManager.Get(i),
                restartThreshold
            );
        }

        public void SeekTo(double seconds)
        {
            if (!_audioPlayback.CanSeek) return;

            _audioPlayback.SeekTo(seconds);

            double tolerance = 0.5;
            double currentSeconds = _audioPlayback.Position.TotalSeconds;
            double totalSeconds = _audioPlayback.Duration.TotalSeconds;

            if (_currentTrack != null)
            {
                if (currentSeconds <= tolerance)
                {
                    TrackRatingManager.ApplyReplay(_currentTrack, _settings.TrackRating);
                }
                else if (totalSeconds - currentSeconds <= tolerance)
                {
                    _userSeeked = true;
                    TrackRatingManager.ApplySeekToEnd(_currentTrack, _settings.TrackRating);
                }
            }
        }

        public void UpdatePosition()
        {
            if (_audioPlayback.IsDisposed) return;

            PositionUpdated?.Invoke(this, new PositionUpdatedEventArgs(
                _audioPlayback.Position,
                _audioPlayback.Duration
            ));
        }

        public bool HandleTrackEnd()
        {
            if (!_trackEnd) return false;

            if (_currentTrack != null)
            {
                if (_userSeeked)
                {
                    TrackRatingManager.ApplySeekToEnd(_currentTrack, _settings.TrackRating);
                }
                else
                {
                    TrackRatingManager.ApplyPlayCompleted(_currentTrack, _settings.TrackRating);
                }

                try
                {
                    TrackDatabase.SaveStats(_currentTrack);
                }
                catch (Exception ex)
                {
                    DebugUtils.Log("PlaybackController", "HandleTrackEnd",
                        $"Error saving stats: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
                }
            }

            _trackEnd = false;
            _userSeeked = false;
            return true;
        }

        public void ResetTrackEndFlag()
        {
            _trackEnd = false;
            _userSeeked = false;
        }

        public void Dispose()
        {
            _audioPlayback?.Dispose();
        }

        private bool IsSkipValid(TimeSpan position, TimeSpan duration)
        {
            int leadIn = _settings.TrackRating.LeadInImmunitySeconds;
            int leadOut = _settings.TrackRating.LeadOutImmunitySeconds;

            return position.Seconds > leadIn &&
                   duration.Subtract(position).Seconds > leadOut;
        }
    }

    public class TrackChangedEventArgs : EventArgs
    {
        public Track Track { get; }
        public TrackChangedEventArgs(Track track) => Track = track;
    }

    public class PositionUpdatedEventArgs : EventArgs
    {
        public TimeSpan Position { get; }
        public TimeSpan Duration { get; }
        public PositionUpdatedEventArgs(TimeSpan position, TimeSpan duration)
        {
            Position = position;
            Duration = duration;
        }
    }
}
