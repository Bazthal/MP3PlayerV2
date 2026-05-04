using MP3PlayerV2.Models;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Manages a collection of tracks in a playlist, providing functionality to add, remove, shuffle, and reorder tracks.
    /// </summary>
    /// <remarks>The <see cref="PlaylistManager"/> class focuses solely on managing the playlist collection.
    /// For file I/O operations, use <see cref="PlaylistFileService"/>.
    /// For track creation from files, use <see cref="TrackFileProcessor"/>.</remarks>
    public class PlaylistManager
    {
        private readonly List<Track> _tracks = [];
        private readonly TrackDurationCache _durationCache;
        private readonly TrackFileProcessor _trackProcessor;
        private readonly PlaylistFileService _fileService;

        public PlaylistManager()
        {
            _durationCache = new TrackDurationCache();
            _trackProcessor = new TrackFileProcessor(_durationCache);
            _fileService = new PlaylistFileService(_trackProcessor);
        }

        /// <summary>
        /// Gets the track duration cache for accessing track durations.
        /// </summary>
        public TrackDurationCache DurationCache => _durationCache;

        /// <summary>
        /// Gets the track file processor for creating tracks from audio files.
        /// </summary>
        public TrackFileProcessor TrackProcessor => _trackProcessor;

        /// <summary>
        /// Gets the playlist file service for loading and saving playlists.
        /// </summary>
        public PlaylistFileService FileService => _fileService;

        /// <summary>
        /// Gets a read-only list of tracks.
        /// </summary>
        public IReadOnlyList<Track> Tracks => _tracks.AsReadOnly();

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified track in the collection.
        /// </summary>
        /// <param name="track">The track to locate in the collection. The value can be <see langword="null"/>.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="track"/> within the collection, if found;
        /// otherwise, -1.</returns>
        public int IndexOf(Track track)
        {
            return _tracks.IndexOf(track);
        }

        /// <summary>
        /// Occurs when the playlist is modified.
        /// </summary>
        /// <remarks>Subscribe to this event to be notified whenever the playlist changes. This can
        /// include additions, deletions, or reordering of items within the playlist.</remarks>
        public event Action? PlaylistChanged;

        /// <summary>
        /// Adds a track to the playlist.
        /// </summary>
        /// <remarks>Invokes the <see cref="PlaylistChanged"/> event after adding the track.</remarks>
        /// <param name="track">The track to add to the playlist. Cannot be null.</param>
        public void Add(Track track)
        {
            _tracks.Add(track);
            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Adds a collection of tracks to the playlist.
        /// </summary>
        /// <remarks>After adding the tracks, the <see cref="PlaylistChanged"/> event is invoked to notify
        /// subscribers of the update.</remarks>
        /// <param name="tracks">The collection of tracks to add. Cannot be null.</param>
        public void AddRange(IEnumerable<Track> tracks)
        {
            _tracks.AddRange(tracks);
            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Removes the track at the specified index from the playlist.
        /// </summary>
        /// <remarks>Invokes the <see cref="PlaylistChanged"/> event after a track is successfully
        /// removed.</remarks>
        /// <param name="index">The zero-based index of the track to remove. Must be within the range of the playlist.</param>
        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _tracks.Count)
            {
                _tracks.RemoveAt(index);
                PlaylistChanged?.Invoke();
            }
        }

        /// <summary>
        /// Clears all tracks from the playlist.
        /// </summary>
        /// <remarks>This method removes all tracks from the playlist and triggers the <see
        /// cref="PlaylistChanged"/> event.</remarks>
        public void Clear()
        {
            _tracks.Clear();
            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Randomizes the order of tracks in the playlist.
        /// </summary>
        /// <remarks>This method shuffles the tracks in the playlist using a random number generator,
        /// ensuring that each track has an equal chance of appearing in any position. After shuffling, the <see
        /// cref="PlaylistChanged"/> event is invoked to notify subscribers of the change in the playlist
        /// order.</remarks>
        public void Shuffle()
        {
            var rng = new Random();
            int n = _tracks.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (_tracks[n], _tracks[k]) = (_tracks[k], _tracks[n]);
            }
            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Retrieves the track at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the track to retrieve. Must be within the range of available tracks.</param>
        /// <returns>The <see cref="Track"/> at the specified index, or <see langword="null"/> if the index is out of range.</returns>
        public Track? Get(int index)
        {
            if (index >= 0 && index < _tracks.Count)
                return _tracks[index];
            return null;
        }

        /// <summary>
        /// Searches for a track that contains the specified text in its string representation.
        /// </summary>
        /// <remarks>The search is case-sensitive and performs a substring match. If multiple tracks
        /// contain the specified text, only the first match is returned.</remarks>
        /// <param name="text">The text to search for within the string representation of tracks. Cannot be null, empty, or consist only of
        /// whitespace.</param>
        /// <returns>The first <see cref="Track"/> that contains the specified text in its string representation, or <see
        /// langword="null"/> if no such track is found.</returns>
        public Track? GetByText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            Track track = new();

            for (int i = 0; i < Count; i++)
            {
                if (_tracks[i].ToString().Equals(text))
                { track = _tracks[i]; break; }
            }

            return track != null ? track : null;

        }
        
        /// <summary>
        /// Reorders items in the playlist based on the specified old and new indices.
        /// </summary>
        /// <remarks>This method reorders multiple items in the playlist in a single operation. If only
        /// one item is being moved, the operation is optimized for that case. The indices in <paramref
        /// name="oldIndices"/> and  <paramref name="newIndices"/> must be aligned such that the item at position
        /// <c>i</c> in  <paramref name="oldIndices"/> is moved to the position specified by <paramref
        /// name="newIndices"/>[i].  After the reordering operation, the <c>PlaylistChanged</c> event is raised to
        /// notify listeners of the update.</remarks>
        /// <param name="oldIndices">A read-only list of integers representing the current indices of the items to be reordered. Each index must
        /// correspond to an item in the playlist.</param>
        /// <param name="newIndices">A read-only list of integers representing the target indices for the items being reordered. Each index
        /// specifies the new position for the corresponding item in <paramref name="oldIndices"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="oldIndices"/> and <paramref name="newIndices"/> do not have the same count.</exception>
        public void Reorder(IReadOnlyList<int> oldIndices, IReadOnlyList<int> newIndices)
        {
            if (oldIndices.Count != newIndices.Count)
                throw new ArgumentException("Old and new indices must have the same count.");

            if (oldIndices.Count == 1)
            {
                Move(oldIndices[0], newIndices[0]);
                return;
            }

            var movedTracks = oldIndices.Select(i => _tracks[i]).ToList();

            foreach (var i in oldIndices.OrderByDescending(i => i))
            {
                _tracks.RemoveAt(i);
            }

            for (int n = 0; n < movedTracks.Count; n++)
            {
                int targetIndex = newIndices[n];
                if (targetIndex > _tracks.Count)
                    targetIndex = _tracks.Count;

                _tracks.Insert(targetIndex, movedTracks[n]);
            }

            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Moves an item from one index to another within the playlist.
        /// </summary>
        /// <remarks>If <paramref name="newIndex"/> is greater than <paramref name="oldIndex"/>, the
        /// target index is adjusted to account for the removal of the item at <paramref name="oldIndex"/>. After the
        /// move operation, the <c>PlaylistChanged</c> event is raised to notify subscribers of the change.</remarks>
        /// <param name="oldIndex">The zero-based index of the item to move. Must be within the valid range of the playlist.</param>
        /// <param name="newIndex">The zero-based index to which the item should be moved. If the value is less than 0, the item is moved to
        /// the beginning of the playlist. If the value is greater than or equal to the number of items in the playlist,
        /// the item is moved to the end.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="oldIndex"/> is less than 0 or greater than or equal to the number of items in the
        /// playlist.</exception>
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= _tracks.Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0) newIndex = 0;
            if (newIndex >= _tracks.Count) newIndex = _tracks.Count - 1;

            var track = _tracks[oldIndex];
            _tracks.RemoveAt(oldIndex);

            if (newIndex > oldIndex) newIndex--;

            _tracks.Insert(newIndex, track);

            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Sets the order of tracks in the collection.
        /// </summary>
        /// <remarks>This method clears the existing collection of tracks and replaces it with the
        /// specified tracks in the given order. The collection will exactly match the order of the provided <paramref
        /// name="orderedTracks"/>.</remarks>
        /// <param name="orderedTracks">An <see cref="IEnumerable{T}"/> of <see cref="Track"/> objects representing the tracks in the desired order.</param>
        public void SetOrder(IEnumerable<Track> orderedTracks)
        {
            _tracks.Clear();
            _tracks.AddRange(orderedTracks);
        }

        /// <summary>
        /// Retrieves the duration of the specified track.
        /// </summary>
        /// <param name="track">The track for which to retrieve the duration.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the duration of the track.</returns>
        public TimeSpan GetDuration(Track track)
        {
            return _durationCache.GetDuration(track);
        }

        /// <summary>
        /// Processes a collection of file paths asynchronously, creating tracks from the files and optionally replacing
        /// the current playlist.
        /// </summary>
        /// <param name="files">A collection of file paths to process.</param>
        /// <param name="reportProgress">An optional callback to report progress during processing.</param>
        /// <param name="replacePlaylist">A value indicating whether to replace the current playlist with the processed tracks.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation containing a list of <see cref="Track"/> objects.</returns>
        public async Task<List<Track>> ProcessFilesAsync(
            IEnumerable<string> files,
            Action<int, int, string?>? reportProgress,
            bool replacePlaylist,
            CancellationToken cancellationToken)
        {
            var orderedTracks = await _trackProcessor.ProcessFilesAsync(files, reportProgress, cancellationToken);

            if (replacePlaylist)
            {
                _tracks.Clear();
                _tracks.AddRange(orderedTracks);
            }
            else
            {
                AddRange(orderedTracks);
            }
            PlaylistChanged?.Invoke();
            MP3PlayerV2.Instance.CleanupIfNeeded(orderedTracks.Count);
            return orderedTracks;
        }

        public int Count => _tracks.Count;

        /// <summary>
        /// Saves the current playlist to a file in M3U format.
        /// </summary>
        /// <param name="filePath">The path to the file where the M3U playlist will be saved.</param>
        /// <param name="overrideList">An optional list of tracks to save instead of the current playlist.</param>
        public void SaveToM3U(string filePath, List<Track>? overrideList = null)
        {
            var lst = overrideList ?? _tracks;
            _fileService.SaveToM3U(filePath, lst);
        }

        /// <summary>
        /// Saves the playlist to a JSON file at the specified file path.
        /// </summary>
        /// <param name="filePath">The full path of the file where the playlist will be saved.</param>
        /// <param name="overrideList">An optional list of tracks to save instead of the default playlist.</param>
        public void SaveToJsonPl(string filePath, List<Track>? overrideList = null)
        {
            var lst = overrideList ?? _tracks;
            _fileService.SaveToJsonPl(filePath, lst);
        }

        /// <summary>
        /// Loads a playlist from a JSON file and updates the current playlist with the loaded tracks.
        /// </summary>
        /// <param name="filePath">The path to the JSON file containing the playlist data.</param>
        /// <param name="reportProgress">An optional callback to report progress during the loading process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public void LoadFromJsonPl(
            string filePath,
            Action<int, int, string?>? reportProgress = null,
            CancellationToken cancellationToken = default)
        {
            var tracks = _fileService.LoadFromJsonPl(filePath, reportProgress, cancellationToken);
            _tracks.Clear();
            AddRange(tracks);
            PlaylistChanged?.Invoke();
        }

        /// <summary>
        /// Loads a playlist from an M3U or M3U8 file and returns a list of tracks.
        /// </summary>
        /// <param name="filePath">The path to the M3U or M3U8 file to load.</param>
        /// <param name="reportProgress">An optional callback to report progress during the loading process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A list of <see cref="Track"/> objects representing the tracks in the playlist.</returns>
        public async Task<List<Track>> LoadFromM3U(
            string filePath,
            Action<int, int, string?>? reportProgress = null,
            CancellationToken cancellationToken = default)
        {
            var orderedTracks = await _fileService.LoadFromM3U(filePath, reportProgress, cancellationToken);
            _tracks.Clear();
            _tracks.AddRange(orderedTracks);
            PlaylistChanged?.Invoke();
            MP3PlayerV2.Instance.CleanupIfNeeded(orderedTracks.Count);
            return orderedTracks;
        }
    }

}
