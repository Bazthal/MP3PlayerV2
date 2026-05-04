using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
using MP3PlayerV2.Models;
using System;
using System.IO;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Provides audio playback functionality using CSCore.
    /// </summary>
    public class AudioPlaybackService : IDisposable
    {
        #region Fields

        private WasapiOut? _soundOut;
        private IWaveSource? _waveSource;
        private VolumeSource? _volumeSource;
        private bool _disposed = true;
        private int _volumeLevel = 100;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the playback state changes.
        /// </summary>
        public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;

        /// <summary>
        /// Occurs when a track has finished playing.
        /// </summary>
        public event EventHandler<TrackEndedEventArgs>? TrackEnded;

        /// <summary>
        /// Occurs when the playback position changes.
        /// </summary>
        public event EventHandler<PositionChangedEventArgs>? PositionChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current playback state.
        /// </summary>
        public PlaybackState State => _soundOut?.PlaybackState ?? PlaybackState.Stopped;

        /// <summary>
        /// Gets the current playback position.
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                if (_waveSource == null) return TimeSpan.Zero;
                return TimeSpan.FromSeconds(_waveSource.Position / (double)_waveSource.WaveFormat.BytesPerSecond);
            }
        }

        /// <summary>
        /// Gets the total duration of the current track.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (_waveSource == null) return TimeSpan.Zero;
                return TimeSpan.FromSeconds(_waveSource.Length / (double)_waveSource.WaveFormat.BytesPerSecond);
            }
        }

        /// <summary>
        /// Gets whether the audio source can seek.
        /// </summary>
        public bool CanSeek => _waveSource?.CanSeek ?? false;

        /// <summary>
        /// Gets or sets the volume level (0-100).
        /// </summary>
        public int VolumeLevel
        {
            get => _volumeLevel;
            set
            {
                _volumeLevel = Math.Clamp(value, 0, 100);
                if (_volumeSource != null)
                    _volumeSource.Volume = _volumeLevel / 100f;
            }
        }

        /// <summary>
        /// Gets whether playback resources are currently disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the audio playback system with the specified audio file and optional device ID.
        /// </summary>
        /// <param name="filePath">The path to the audio file to be played.</param>
        /// <param name="deviceId">The optional ID of the audio output device.</param>
        /// <returns>True if initialization was successful; otherwise, false.</returns>
        public bool Initialize(string filePath, string deviceId = "")
        {
            DisposeResources();

            if (!File.Exists(filePath))
            {
                BazthalLib.DebugUtils.Log("AudioPlaybackService", "Initialize", 
                    "File not found.", logLevel: BazthalLib.DebugUtils.LogLevel.Error);
                return false;
            }

            try
            {
                _disposed = false;

                var sampleSource = CodecFactory.Instance.GetCodec(filePath).ToSampleSource();
                _volumeSource = new VolumeSource(sampleSource);
                _waveSource = _volumeSource.ToWaveSource(16);

                _soundOut = string.IsNullOrEmpty(deviceId)
                    ? new WasapiOut()
                    : new WasapiOut { Device = new MMDeviceEnumerator().GetDevice(deviceId) };

                _soundOut.Initialize(_waveSource);
                _soundOut.Stopped += OnSoundOutStopped;

                // Apply current volume
                VolumeLevel = _volumeLevel;

                BazthalLib.DebugUtils.Log("AudioPlaybackService", "Initialize", 
                    $"Initialized: {filePath} on device: {deviceId}", 
                    logLevel: BazthalLib.DebugUtils.LogLevel.Info);

                return true;
            }
            catch (Exception ex)
            {
                BazthalLib.DebugUtils.Log("AudioPlaybackService", "Initialize", 
                    $"Error: {ex.Message}", logLevel: BazthalLib.DebugUtils.LogLevel.Error);
                DisposeResources();
                return false;
            }
        }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        public void Play()
        {
            if (_soundOut == null || _disposed) return;

            var previousState = _soundOut.PlaybackState;

            if (_soundOut.PlaybackState == PlaybackState.Paused)
            {
                _soundOut.Resume();
            }
            else if (_soundOut.PlaybackState == PlaybackState.Stopped)
            {
                _soundOut.Play();
            }

            OnPlaybackStateChanged(previousState, _soundOut.PlaybackState);
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public void Pause()
        {
            if (_soundOut == null || _disposed) return;

            var previousState = _soundOut.PlaybackState;

            if (_soundOut.PlaybackState == PlaybackState.Playing)
            {
                _soundOut.Pause();
            }
            else if (_soundOut.PlaybackState == PlaybackState.Paused)
            {
                _soundOut.Resume();
            }

            OnPlaybackStateChanged(previousState, _soundOut.PlaybackState);
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void Stop()
        {
            if (_soundOut == null || _disposed) return;

            var previousState = _soundOut.PlaybackState;
            _soundOut.Stop();
            OnPlaybackStateChanged(previousState, PlaybackState.Stopped);
        }

        /// <summary>
        /// Seeks to the specified position in seconds.
        /// </summary>
        /// <param name="seconds">The position in seconds to seek to.</param>
        public void SeekTo(double seconds)
        {
            if (_waveSource == null || !_waveSource.CanSeek) return;

            long bytePosition = (long)(seconds * _waveSource.WaveFormat.BytesPerSecond);
            bytePosition = Math.Min(bytePosition, _waveSource.Length);
            _waveSource.Position = bytePosition;

            OnPositionChanged(Position);
        }

        /// <summary>
        /// Changes the audio output device.
        /// </summary>
        /// <param name="deviceId">The ID of the new audio device.</param>
        /// <param name="preservePosition">Whether to preserve the current playback position.</param>
        /// <param name="filePath">The file path to reinitialize with.</param>
        /// <returns>True if the device was changed successfully; otherwise, false.</returns>
        public bool ChangeDevice(string deviceId, bool preservePosition, string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            var currentPosition = preservePosition ? _waveSource?.Position ?? 0 : 0;
            var wasPlaying = State != PlaybackState.Stopped;

            DisposeResources();

            if (!Initialize(filePath, deviceId))
                return false;

            if (preservePosition && _waveSource != null)
                _waveSource.Position = currentPosition;

            if (wasPlaying)
                Play();

            return true;
        }

        /// <summary>
        /// Releases all resources used by the audio playback components.
        /// </summary>
        public void Dispose()
        {
            DisposeResources();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private void DisposeResources()
        {
            if (_soundOut != null)
            {
                _soundOut.Stopped -= OnSoundOutStopped;
                try { _soundOut.Stop(); } catch { }
                try { _soundOut.Dispose(); } catch { }
                _soundOut = null;
            }

            if (_waveSource != null)
            {
                try { _waveSource.Dispose(); } catch { }
                _waveSource = null;
            }

            _volumeSource = null;
            _disposed = true;
        }

        private void OnSoundOutStopped(object? sender, PlaybackStoppedEventArgs e)
        {
            if (_disposed || _soundOut == null || _waveSource == null)
                return;

            long buffer = (long)(_waveSource.WaveFormat.SampleRate * 0.07);
            bool reachedEnd = _waveSource.Position + buffer >= _waveSource.Length;

            BazthalLib.DebugUtils.Log("AudioPlaybackService", "OnSoundOutStopped", 
                $"Position: {_waveSource.Position}, Length: {_waveSource.Length}, ReachedEnd: {reachedEnd}", 
                logLevel: BazthalLib.DebugUtils.LogLevel.Info);

            TrackEnded?.Invoke(this, new TrackEndedEventArgs(reachedEnd, e.Exception));
        }

        private void OnPlaybackStateChanged(PlaybackState oldState, PlaybackState newState)
        {
            if (oldState != newState)
            {
                PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(oldState, newState));
            }
        }

        private void OnPositionChanged(TimeSpan position)
        {
            PositionChanged?.Invoke(this, new PositionChangedEventArgs(position));
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Provides data for playback state changed events.
    /// </summary>
    public class PlaybackStateChangedEventArgs : EventArgs
    {
        public PlaybackState OldState { get; }
        public PlaybackState NewState { get; }

        public PlaybackStateChangedEventArgs(PlaybackState oldState, PlaybackState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Provides data for track ended events.
    /// </summary>
    public class TrackEndedEventArgs : EventArgs
    {
        public bool ReachedEnd { get; }
        public Exception? Exception { get; }

        public TrackEndedEventArgs(bool reachedEnd, Exception? exception = null)
        {
            ReachedEnd = reachedEnd;
            Exception = exception;
        }
    }

    /// <summary>
    /// Provides data for position changed events.
    /// </summary>
    public class PositionChangedEventArgs : EventArgs
    {
        public TimeSpan Position { get; }

        public PositionChangedEventArgs(TimeSpan position)
        {
            Position = position;
        }
    }

    #endregion
}
