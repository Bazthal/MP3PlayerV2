using CSCore.SoundOut;
using System.Text.RegularExpressions;

namespace MP3PlayerV2.Models
{
    /// <summary>
    /// Provides a context for executing commands related to audio playback and playlist management.
    /// </summary>
    /// <remarks>The <see cref="CommandContext"/> class encapsulates various actions and functions that allow
    /// interaction with audio devices, playlists, and playback controls. It provides methods to query and manipulate
    /// the current state of audio playback, including selecting tracks, adjusting volume, and managing playlists. This
    /// class is designed to be used in environments where command-based control of audio playback is
    /// required.</remarks>
    public class CommandContext
    {
        public Action<Action> Invoke { get; set; }
        public Action<bool, string, object?> Respond { get; set; }
        public Func<int> GetPlaylistCount { get; set; }
        public Func<int> GetAudioDeviceCount { get; set; }
        public Func<int, string> GetAudioDeviceNameAt { get; set; }
        public Action<int> SetAudioDeviceIndex { get; set; }
        public Func<string> GetSelectedAudioDevice { get; set; }
        public Func<int> GetPlaylistModeCount { get; set; }
        public Func<int, string> GetPlaylistModeNameAt { get; set; }
        public Action<int> SetPlaylistModeIndex { get; set; }
        public Func<string> GetSelectedPlaylistMode { get; set; }
        public Func<PlaybackState> GetPlaybackState { get; set; }
        public string GetCurrentTrack { get; set; }
        public int GetVolumeLevel { get; set; }
        public Func<string, Regex> BuildSearchRegex { get; set; }
        public Action<int> SelectTrackByIndex { get; set; }
        public Action<string> SelectTrackByName { get; set; }
        public Func<string> GetSelectedTrackName { get; set; }
        public Action<string> QueueTrackByName { get; set; }
        public Queue<string> GetQueuedTracks { get; set; }
        public Action ClearQueue { get; set; }
        public Action SelectRandomTrack { get; set; }

        public Func<bool> IsPlaylistEmpty { get; set; }
        public Func<IEnumerable<Track>> GetPlaylistTracks { get; set; }
        public Func<string, string> NormalizeText { get; set; }
        public Action Play { get; set; }
        public Action Pause { get; set; }
        public Action Stop { get; set; }
        public Action<bool> Next { get; set; }
        public Action Previous { get; set; }
        public Action<int> Volume { get; set; }
        public Action Shuffle { get; set; }
        public Action<Func<Track, object>, bool> SortPlaylist { get; set; }
        public Action<string> CountByPlayData { get; set; }
        public Action<string> CountByName { get; set; }
        public Action<string, string> ResetStat { get; set; }
    }
}
