using CSCore.SoundOut;
using MP3PlayerV2.Commands;
using System.Text.RegularExpressions;
using MP3PlayerV2.Services;

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
        #region Core

        public Action<Action> Invoke { get; set; } = _ => { };
        public Func<ApplicationStateService> GetApplicationStateService { get; set; } = () => ApplicationStateService.Instance;

        public Action<string, bool, string, object?> Respond { get; set; } = (_, _, _, _) => { };
        public Action<string, bool, string, string> RespondRaw { get; set; } = (_, _, _, _) => { };

        public Func<string, Regex> BuildSearchRegex { get; set; } = _ => new Regex(".*");

        public Func<string, string> NormalizeText { get; set; } = s => s;

        #endregion Core

        #region Audio Device
        public Func<int> GetAudioDeviceCount { get; set; } = () => 0;

        public Func<int, string> GetAudioDeviceNameAt { get; set; } = _ => string.Empty;

        public Action<int> SetAudioDeviceIndex { get; set; } = _ => { };

        public Func<string> GetSelectedAudioDevice { get; set; } = () => "Unknown";

        #endregion Audio Device

        #region Playback

        public Func<PlaybackState> GetPlaybackState { get; set; } = () => PlaybackState.Stopped;

        public Action<Track> Play { get; set; } = _ => { };

        public Action Pause { get; set; } = () => { };

        public Action Stop { get; set; } = () => { };

        public Action<bool> Next { get; set; } = _ => { };

        public Action Previous { get; set; } = () => { };

        public Action Shuffle { get; set; } = () => { };

        public Func<string> GetCurrentTrack { get; set; } = () => string.Empty;

        #endregion Playback

        #region Volume

        public Func<int> GetVolumeLevel { get; set; } = () => 0;

        public Action<int> SetVolume { get; set; } = _ => { };

        #endregion Volume

        #region Playlist

        public Func<int> GetPlaylistCount { get; set; } = () => 0;

        public Func<IEnumerable<Track>> GetPlaylistTracks { get; set; } = () => Enumerable.Empty<Track>();

        public Func<bool> IsPlaylistEmpty { get; set; } = () => true;

        public Func<int> GetPlaylistModeCount { get; set; } = () => 0;

        public Func<int, string> GetPlaylistModeNameAt { get; set; } = _ => string.Empty;

        public Action<int> SetPlaylistModeIndex { get; set; } = _ => { };

        public Func<string> GetSelectedPlaylistMode { get; set; } = () => "Unknown";

        public Action<string, bool> SortPlaylist { get; set; } = (_, _) => { };

        public Func<Track, int> GetPlaylistIndex { get; set; } = _ => -1;

        #endregion Playlist

        #region Track Selection

        public Action<int> SelectTrackByIndex { get; set; } = _ => { };

        public Action<string> SelectTrackByName { get; set; } = _ => { };

        public Func<string> GetSelectedTrackName { get; set; } = () => "Unknown";

        public Action SelectRandomTrack { get; set; } = () => { };

        #endregion Track Selection

        #region Queue

        public Action<string> QueueTrackByName { get; set; } = _ => { };

        public Func<Queue<string>> GetQueuedTracks { get; set; } = () => new Queue<string>();

        public Action ClearQueue { get; set; } = () => { };

        #endregion Queue

        #region Statistics

        public Action<string> CountByName { get; set; } = _ => { };

        public Action<string> CountByPlayData { get; set; } = _ => { };

        public Func<string, string, Task> ResetStat { get; set; } = async (_, _) => await Task.CompletedTask;

        #endregion Statistics

        #region Command Metadata

        public Func<IEnumerable<(string Command, string Origin, string TypeName)>> ListRegisteredCommands { get; set; }
            = () => Enumerable.Empty<(string, string, string)>();
        
        public Func<List<CommandInfo>> GetMetaData = () => new List<CommandInfo>();

        #endregion Command Metadata

        #region Utilities

        public Action<string, string, string, string> DebugLog { get; set; } = (_, _, _, _) => { };

        #endregion Utilities
    }
}
