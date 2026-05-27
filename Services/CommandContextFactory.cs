using CSCore.SoundOut;
using MP3PlayerV2.Commands;
using MP3PlayerV2.Models;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Factory for creating CommandContext instances from the ApplicationStateService.
    /// This eliminates the need to manually wire up delegates in the form.
    /// </summary>
    public static class CommandContextFactory
    {
        /// <summary>
        /// Creates a CommandContext from the current application state.
        /// </summary>
        public static CommandContext Create()
        {
            var state = ApplicationStateService.Instance;

            return new CommandContext
            {
                #region Core

                Invoke = action => state.InvokeOnUI?.Invoke(action),
                GetApplicationStateService = () => state,
                Respond = (command, success, message, data) => state.WebSocket.BuildResponse(command, success, message, data),
                RespondRaw = (command, success, message, data) => state.WebSocket.BuildResponse(command, success, message, data, raw: true),
                BuildSearchRegex = searchTerm => state.TrackSearch.BuildSearchRegex(searchTerm),
                NormalizeText = text => state.TrackSearch.NormalizeText(text),

                #endregion Core

                #region Audio Device

                GetAudioDeviceCount = () => state.AudioDevices.Devices.Count,
                GetAudioDeviceNameAt = i =>
                {
                    var device = state.AudioDevices.GetDevice(i);
                    return device?.FriendlyName ?? string.Empty;
                },
                SetAudioDeviceIndex = i => state.SetSelectedAudioDeviceIndex?.Invoke(i),
                GetSelectedAudioDevice = () =>
                {
                    int index = state.GetSelectedAudioDeviceIndex?.Invoke() ?? -1;
                    if (index >= 0)
                    {
                        var device = state.AudioDevices.GetDevice(index);
                        return device?.FriendlyName ?? "Unknown";
                    }
                    return "Unknown";
                },

                #endregion Audio Device

                #region Playback

                GetPlaybackState = () => state.AudioPlayback.State,
                Play = track =>
                {
                    state.InvokeOnUI?.Invoke(() =>
                    {
                        if (track != null)
                        {
                            int index = state.PlaylistManager.IndexOf(track);
                            if (index >= 0)
                            {
                                state.SetSelectedTrackIndex?.Invoke(index);
                            }
                        }
                    });
                },
                Pause = () => state.InvokeOnUI?.Invoke(() => { }),
                Stop = () => state.InvokeOnUI?.Invoke(() => { }),
                Next = automatic => state.InvokeOnUI?.Invoke(() => { }),
                Previous = () => state.InvokeOnUI?.Invoke(() => { }),
                Shuffle = () => state.InvokeOnUI?.Invoke(() => { }),
                GetCurrentTrack = () => state.CurrentTrack?.ToString() ?? string.Empty,

                #endregion Playback

                #region Volume

                GetVolumeLevel = () => state.VolumeLevel,
                SetVolume = vol =>
                {
                    state.VolumeLevel = vol;
                    state.AudioPlayback.VolumeLevel = vol;
                    state.SetVolumeSliderValue?.Invoke(vol);
                },

                #endregion Volume

                #region Position

                GetPosition = () => state.AudioPlayback.IsDisposed ? 0.0 : state.AudioPlayback.Position.TotalSeconds,
                GetDuration = () => state.AudioPlayback.IsDisposed ? 0.0 : state.AudioPlayback.Duration.TotalSeconds,
                SeekTo = seconds =>
                {
                    if (!state.AudioPlayback.IsDisposed && state.AudioPlayback.CanSeek)
                    {
                        state.AudioPlayback.SeekTo(seconds);
                        state.SetTrackingSliderValue?.Invoke((int)seconds);
                    }
                },

                #endregion Position

                #region Playlist

                GetPlaylistCount = () => state.PlaylistManager.Count,
                GetPlaylistTracks = () => state.PlaylistManager.Tracks,
                IsPlaylistEmpty = () => state.PlaylistManager.Count == 0,
                GetPlaylistModeCount = () => Enum.GetValues(typeof(PlaylistOption)).Length,
                GetPlaylistModeNameAt = i =>
                {
                    var values = Enum.GetValues(typeof(PlaylistOption));
                    if (i >= 0 && i < values.Length)
                    {
                        return ((PlaylistOption)values.GetValue(i)!).ToString().Replace("_", " ");
                    }
                    return string.Empty;
                },
                SetPlaylistModeIndex = i => state.SetSelectedPlaylistOptionIndex?.Invoke(i),
                GetSelectedPlaylistMode = () =>
                {
                    int index = state.GetSelectedPlaylistOptionIndex?.Invoke() ?? -1;
                    if (index >= 0)
                    {
                        var values = Enum.GetValues(typeof(PlaylistOption));
                        if (index < values.Length)
                        {
                            return ((PlaylistOption)values.GetValue(index)!).ToString().Replace("_", " ");
                        }
                    }
                    return "Unknown";
                },
                GetShuffleModeCount = () => Enum.GetValues(typeof(SmartShuffleMode)).Length,
                GetShuffleModeNameAt = i =>
                {
                    var values = Enum.GetValues(typeof(SmartShuffleMode));
                    if (i >= 0 && i < values.Length)
                    {
                        return ((SmartShuffleMode)values.GetValue(i)!).ToString();
                    }
                    return string.Empty;

                },
                SetShuffleMode = mode => {
                    state.Settings.SmartShuffle.Mode = mode;

                    bool parsed = Enum.TryParse(mode, out SmartShuffleMode modeOut);
                    if (parsed)
                    {
                        ApplicationStateService.Instance.SmartShuffleMode = modeOut;
                    }
                },
                GetSelectedShuffleMode = () => 
                {
                    return state.GetSelectedShuffleMode!() ?? "Unknown";
                },
                SortPlaylist = (field, desc) =>
                {
                    state.InvokeOnUI?.Invoke(() => { });
                },

                GetPlaylistIndex = (track) =>
                {
                    int index = state.PlaylistManager.IndexOf(track);
                    return index;
                },

                #endregion Playlist

                #region Track Selection

                SelectTrackByIndex = i => state.SetSelectedTrackIndex?.Invoke(i),
                SelectTrackByName = name =>
                {
                    var bestMatch = state.TrackSearch.FindBestMatch(state.PlaylistManager.Tracks, name);
                    if (bestMatch != null)
                    {
                        int index = state.PlaylistManager.IndexOf(bestMatch);
                        state.SetSelectedTrackIndex?.Invoke(index);
                        state.WebSocket.BuildResponse("Select", true, $"Match found: {bestMatch}");
                    }
                    else
                    {
                        state.WebSocket.BuildResponse("Select", false, "No matching item found in the playlist");
                    }
                },
                GetSelectedTrackName = () =>
                {
                    int index = state.GetSelectedTrackIndex?.Invoke() ?? -1;
                    if (index >= 0)
                    {
                        var track = state.PlaylistManager.Get(index);
                        return track?.ToString() ?? "Unknown";
                    }
                    return "Unknown";
                },
                SelectRandomTrack = () =>
                {
                    int index = state.PlaylistManager.Tracks.Any() ? new Random().Next(0, state.PlaylistManager.Tracks.Count) : -1;
                    if (index >= 0)
                    {
                        state.SetSelectedTrackIndex?.Invoke(index);
                        state.WebSocket.BuildResponse("SelectRandom", true, $"Random track selected: {state.PlaylistManager.Get(index)}");
                    }
                    else
                    {
                        state.WebSocket.BuildResponse("SelectRandom", false, "No tracks available to select");
                    }
                },
                #endregion Track Selection

                #region Queue

                QueueTrackByName = name =>
                {
                    var bestMatch = state.TrackSearch.FindBestMatch(state.PlaylistManager.Tracks, name);
                    if (bestMatch != null)
                    {
                        state.TrackNavigation.EnqueueTrack(bestMatch.ToString());
                        state.WebSocket.BuildResponse("Queue", true, $"Added {bestMatch} to the Queue");
                    }
                    else
                    {
                        state.WebSocket.BuildResponse("Queue", false, "No matching item found in the playlist");
                    }
                },
                GetQueuedTracks = () => state.TrackNavigation.GetQueueCopy(),
                ClearQueue = () => state.TrackNavigation.ClearQueue(),

                #endregion Queue

                #region Statistics

                CountByName = searchTerm =>
                {
                    var matches = state.TrackSearch.FindTracksByName(state.PlaylistManager.Tracks, searchTerm);
                    bool includeData = state.Settings.CommandBehaviour.IncludeTracksInCount;
                    if (matches.Count > 0) 
                    state.WebSocket.BuildResponse("Count", true, $"{matches.Count} matching item(s) found in the playlist for: {searchTerm}", includeData ? matches : null);
                    else
                        state.WebSocket.BuildResponse("Count", false, "No matching item found in the playlist");
                },
                CountByPlayData = data =>
                {
                    var matches = state.TrackSearch.FindTracksByPlayData(state.PlaylistManager.Tracks, data);
                    bool includeData = state.Settings.CommandBehaviour.IncludeTracksInCount;
                    state.WebSocket.BuildResponse("Count", true, $"{matches.Count} {data} track(s) found in the playlist", includeData ? matches : null);
                },
                ResetStat = async (range, stat) =>
                {
                    await Task.CompletedTask;
                },

                #endregion Statistics

                #region Command Metadata

                ListRegisteredCommands = () =>
                {
                    return Enumerable.Empty<(string, string, string)>();
                },
                GetMetaData = () => new List<CommandInfo>(),

                #endregion Command Metadata

                #region Utilities

                DebugLog = (category, name, message, logLevelString) =>
                {
                    if (!Enum.TryParse<BazthalLib.DebugUtils.LogLevel>(logLevelString, true, out var logLevel))
                        logLevel = BazthalLib.DebugUtils.LogLevel.Info;

                    BazthalLib.DebugUtils.Log(category, name, message, logLevel: logLevel);
                },

                #endregion Utilities
            };
        }
    }
}
