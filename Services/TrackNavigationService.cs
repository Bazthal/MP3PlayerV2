using BazthalLib.Systems;
using MP3PlayerV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Manages track navigation including history, queue, and smart selection algorithms.
    /// </summary>
    public class TrackNavigationService
    {
        #region Fields

        private readonly LimitedStack<Track> _trackHistory;
        private readonly Queue<string> _trackQueue = new();
        private readonly Random _random = new();
        private int _maxHistorySize = 100;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a track is added to the history.
        /// </summary>
        public event EventHandler<TrackHistoryEventArgs>? TrackAddedToHistory;

        /// <summary>
        /// Occurs when a track is added to the queue.
        /// </summary>
        public event EventHandler<TrackQueuedEventArgs>? TrackQueued;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of tracks in the history.
        /// </summary>
        public int HistoryCount => _trackHistory.Count;

        /// <summary>
        /// Gets the number of tracks in the queue.
        /// </summary>
        public int QueueCount => _trackQueue.Count;

        /// <summary>
        /// Gets or sets the maximum history size.
        /// </summary>
        public int MaxHistorySize
        {
            get => _maxHistorySize;
            set
            {
                _maxHistorySize = Math.Max(1, Math.Min(value, 9999));
                _trackHistory.EnsureCapacity(_maxHistorySize);
            }
        }

        #endregion

        #region Constructor

        public TrackNavigationService(int maxHistorySize = 100)
        {
            _maxHistorySize = Math.Max(1, Math.Min(maxHistorySize, 9999));
            _trackHistory = new LimitedStack<Track>(_maxHistorySize);
        }

        #endregion

        #region Public Methods - Navigation

        /// <summary>
        /// Determines the next track to play based on the specified options and playlist.
        /// </summary>
        /// <param name="currentTrack">The currently playing track.</param>
        /// <param name="currentIndex">The index of the current track in the playlist.</param>
        /// <param name="playlistCount">The total number of tracks in the playlist.</param>
        /// <param name="playlistOption">The playlist playback mode.</param>
        /// <param name="smartShuffleMode">The smart shuffle mode (if applicable).</param>
        /// <param name="getTrackAt">Function to retrieve a track at a specific index.</param>
        /// <param name="getAllTracks">Function to retrieve all tracks for smart shuffle.</param>
        /// <returns>A tuple containing the next track and its index, or null if end of playlist.</returns>
        public (Track? track, int index) GetNextTrack(
            Track? currentTrack,
            int currentIndex,
            int playlistCount,
            PlaylistOption playlistOption,
            SmartShuffleMode smartShuffleMode,
            Func<int, Track?> getTrackAt,
            Func<IEnumerable<Track>> getAllTracks)
        {
            if (playlistCount == 0)
                return (null, -1);

            // Add current track to history
            if (currentTrack != null)
            {
                AddToHistory(currentTrack);
            }

            // Check queue first (remote control override)
            var queuedTrack = DequeueTrack(getTrackAt, playlistCount);
            if (queuedTrack.track != null)
                return queuedTrack;

            // Determine next track based on playlist option
            return playlistOption switch
            {
                PlaylistOption.RepeatPlaylist => GetNextInRepeatMode(currentIndex, playlistCount, getTrackAt),
                PlaylistOption.RepeatTrack => (currentTrack, currentIndex),
                PlaylistOption.RandomTrack => GetRandomTrack(playlistCount, currentTrack, getTrackAt),
                PlaylistOption.SmartShuffle => GetSmartShuffleTrack(smartShuffleMode, getAllTracks, getTrackAt, playlistCount),
                PlaylistOption.Sequential => GetNextSequential(currentIndex, playlistCount, getTrackAt),
                _ => GetNextSequential(currentIndex, playlistCount, getTrackAt)
            };
        }

        /// <summary>
        /// Determines the previous track to play.
        /// </summary>
        /// <param name="currentPosition">Current playback position in seconds.</param>
        /// <param name="currentIndex">The index of the current track.</param>
        /// <param name="playlistCount">The total number of tracks in the playlist.</param>
        /// <param name="getTrackAt">Function to retrieve a track at a specific index.</param>
        /// <param name="restartThreshold">Threshold in seconds to restart current track instead of going back.</param>
        /// <returns>A tuple containing the previous track and its index.</returns>
        public (Track? track, int index, bool shouldRestart) GetPreviousTrack(
            double currentPosition,
            int currentIndex,
            int playlistCount,
            Func<int, Track?> getTrackAt,
            double restartThreshold = 5.0)
        {
            if (playlistCount == 0)
                return (null, -1, false);

            // If we're more than threshold seconds into the track, restart it
            if (currentPosition > restartThreshold)
            {
                return (getTrackAt(currentIndex), currentIndex, true);
            }

            // Try to get from history
            while (_trackHistory.Count > 0)
            {
                var previousTrack = _trackHistory.Pop();
                
                // Find this track in the playlist
                for (int i = 0; i < playlistCount; i++)
                {
                    var track = getTrackAt(i);
                    if (track != null && track.Guid == previousTrack.Guid)
                    {
                        return (track, i, false);
                    }
                }
            }

            // Fallback: go to previous index
            if (currentIndex > 0)
            {
                return (getTrackAt(currentIndex - 1), currentIndex - 1, false);
            }

            return (null, -1, false);
        }

        #endregion

        #region Public Methods - History & Queue

        /// <summary>
        /// Adds a track to the history.
        /// </summary>
        public void AddToHistory(Track track)
        {
            if (track != null)
            {
                _trackHistory.Push(track);
                TrackAddedToHistory?.Invoke(this, new TrackHistoryEventArgs(track));
            }
        }

        /// <summary>
        /// Enqueues a track by its string representation.
        /// </summary>
        public void EnqueueTrack(string trackString)
        {
            if (!string.IsNullOrWhiteSpace(trackString))
            {
                _trackQueue.Enqueue(trackString);
                TrackQueued?.Invoke(this, new TrackQueuedEventArgs(trackString));
            }
        }

        /// <summary>
        /// Clears the track queue.
        /// </summary>
        public void ClearQueue()
        {
            _trackQueue.Clear();
        }

        /// <summary>
        /// Gets a copy of the current queue.
        /// </summary>
        public Queue<string> GetQueueCopy()
        {
            return new Queue<string>(_trackQueue);
        }

        /// <summary>
        /// Clears the track history.
        /// </summary>
        public void ClearHistory()
        {
            _trackHistory.Clear();
        }

        #endregion

        #region Private Methods - Navigation Logic

        private (Track? track, int index) DequeueTrack(Func<int, Track?> getTrackAt, int playlistCount)
        {
            while (_trackQueue.Count > 0)
            {
                string queuedTrack = _trackQueue.Peek();

                // Search for matching track
                for (int i = 0; i < playlistCount; i++)
                {
                    var track = getTrackAt(i);
                    if (track != null && track.ToString() == queuedTrack)
                    {
                        _trackQueue.Dequeue();
                        return (track, i);
                    }
                }

                // No match found, remove stale entry
                _trackQueue.Dequeue();
            }

            return (null, -1);
        }

        private (Track? track, int index) GetNextInRepeatMode(int currentIndex, int playlistCount, Func<int, Track?> getTrackAt)
        {
            int nextIndex = currentIndex < playlistCount - 1 ? currentIndex + 1 : 0;
            return (getTrackAt(nextIndex), nextIndex);
        }

        private (Track? track, int index) GetNextSequential(int currentIndex, int playlistCount, Func<int, Track?> getTrackAt)
        {
            if (currentIndex < playlistCount - 1)
            {
                int nextIndex = currentIndex + 1;
                return (getTrackAt(nextIndex), nextIndex);
            }

            // End of playlist
            return (null, -1);
        }

        private (Track? track, int index) GetRandomTrack(int playlistCount, Track? exclude, Func<int, Track?> getTrackAt)
        {
            int randomIndex = _random.Next(0, playlistCount);
            
            // If we have more than one track and got the same as current, try again once
            if (exclude != null && playlistCount > 1)
            {
                var selected = getTrackAt(randomIndex);
                if (selected?.Guid == exclude.Guid)
                {
                    randomIndex = _random.Next(0, playlistCount);
                }
            }

            return (getTrackAt(randomIndex), randomIndex);
        }

        private (Track? track, int index) GetSmartShuffleTrack(
            SmartShuffleMode mode,
            Func<IEnumerable<Track>> getAllTracks,
            Func<int, Track?> getTrackAt,
            int playlistCount)
        {
            var allTracks = getAllTracks().ToList();
            var selectedTrack = PickSmartTrack(mode, allTracks, _trackHistory);

            if (selectedTrack == null)
                return (null, -1);

            // Find index of selected track
            for (int i = 0; i < playlistCount; i++)
            {
                var track = getTrackAt(i);
                if (track?.Guid == selectedTrack.Guid)
                {
                    return (track, i);
                }
            }

            return (null, -1);
        }

        #endregion

        #region Private Methods - Smart Shuffle

        private Track? PickSmartTrack(SmartShuffleMode mode, List<Track> tracks, LimitedStack<Track> history)
        {
            if (!tracks.Any()) return null;

            var recentTrackSet = new HashSet<Guid>(history.Select(h => h.Guid));

            List<Track> LeastPlayedOf(IEnumerable<Track> source, bool avoidRecent)
            {
                var grouped = source.GroupBy(t => t.PlayCount ?? 0)
                                    .OrderBy(g => g.Key)
                                    .FirstOrDefault();

                if (grouped == null) return new List<Track>();

                var bucket = grouped.ToList();

                if (avoidRecent)
                {
                    var nonRecent = bucket.Where(t => !recentTrackSet.Contains(t.Guid)).ToList();
                    if (nonRecent.Any()) return nonRecent;
                }

                return bucket;
            }

            List<Track> FallbackCandidates()
            {
                var nonRecent = tracks.Where(t => !recentTrackSet.Contains(t.Guid)).ToList();
                return nonRecent.Any() ? nonRecent : tracks;
            }

            Track? selectedTrack = mode switch
            {
                SmartShuffleMode.UnplayedFirst => PickUnplayedFirst(tracks, recentTrackSet, LeastPlayedOf),
                SmartShuffleMode.MostPlayed => PickMostPlayed(tracks, recentTrackSet, FallbackCandidates),
                SmartShuffleMode.LikedOnly => PickLikedOnly(tracks, LeastPlayedOf),
                SmartShuffleMode.AvoidDisliked => PickAvoidDisliked(tracks, FallbackCandidates),
                _ => null
            };

            // Final fallback
            selectedTrack ??= PickRandom(LeastPlayedOf(FallbackCandidates(), avoidRecent: true), null);

            return selectedTrack;
        }

        private Track? PickUnplayedFirst(List<Track> tracks, HashSet<Guid> recentTrackSet, Func<IEnumerable<Track>, bool, List<Track>> leastPlayedOf)
        {
            var unplayed = tracks.Where(t => (t.PlayCount ?? 0) == 0).ToList();
            return PickRandom(unplayed.Any() ? unplayed : leastPlayedOf(tracks, false), null);
        }

        private Track? PickMostPlayed(List<Track> tracks, HashSet<Guid> recentTrackSet, Func<List<Track>> fallbackCandidates)
        {
            var maxPlays = tracks.Max(t => t.PlayCount ?? 0);
            if (maxPlays == 0)
            {
                return PickRandom(fallbackCandidates(), null);
            }

            var mostPlayed = tracks
                .Where(t => (t.PlayCount ?? 0) == maxPlays && !recentTrackSet.Contains(t.Guid))
                .ToList();

            return PickRandom(mostPlayed.Any() ? mostPlayed : fallbackCandidates(), null);
        }

        private Track? PickLikedOnly(List<Track> tracks, Func<IEnumerable<Track>, bool, List<Track>> leastPlayedOf)
        {
            var liked = tracks.Where(t => t.Liked == true).ToList();
            if (liked.Any())
                return PickRandom(leastPlayedOf(liked, true), null);
            return null;
        }

        private Track? PickAvoidDisliked(List<Track> tracks, Func<List<Track>> fallbackCandidates)
        {
            var noDislike = tracks.Where(t => t.Disliked == false).ToList();
            return PickRandom(noDislike.Any() ? noDislike : fallbackCandidates(), null);
        }

        private Track? PickRandom(List<Track> list, Track? exclude)
        {
            if (list == null || list.Count == 0)
                return null;

            if (exclude != null && list.Count > 1)
            {
                Track selected;
                do
                {
                    selected = list[_random.Next(list.Count)];
                } while (selected.Guid == exclude.Guid);
                return selected;
            }

            return list[_random.Next(list.Count)];
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// Playlist playback options.
    /// </summary>
    public enum PlaylistOption
    {
        Sequential,
        RepeatPlaylist,
        RepeatTrack,
        RandomTrack,
        SmartShuffle
    }

    /// <summary>
    /// Smart shuffle modes.
    /// </summary>
    public enum SmartShuffleMode
    {
        UnplayedFirst,
        MostPlayed,
        LikedOnly,
        AvoidDisliked,
        WeightedByRating,
        UnratedFirst
    }

    #endregion

    #region Event Args

    /// <summary>
    /// Provides data for track history events.
    /// </summary>
    public class TrackHistoryEventArgs : EventArgs
    {
        public Track Track { get; }

        public TrackHistoryEventArgs(Track track)
        {
            Track = track;
        }
    }

    /// <summary>
    /// Provides data for track queued events.
    /// </summary>
    public class TrackQueuedEventArgs : EventArgs
    {
        public string TrackString { get; }

        public TrackQueuedEventArgs(string trackString)
        {
            TrackString = trackString;
        }
    }

    #endregion
}
