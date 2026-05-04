using BazthalLib;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MP3PlayerV2.Controllers
{
    /// <summary>
    /// Coordinates playlist operations between UI and services.
    /// </summary>
    public class PlaylistController
    {
        private readonly PlaylistManager _playlistManager;
        private readonly PlaylistOperationsService _playlistOps;
        private static readonly Random _rng = new();

        public event EventHandler? PlaylistChanged;

        public PlaylistController(PlaylistManager playlistManager, PlaylistOperationsService playlistOps)
        {
            _playlistManager = playlistManager ?? throw new ArgumentNullException(nameof(playlistManager));
            _playlistOps = playlistOps ?? throw new ArgumentNullException(nameof(playlistOps));

            _playlistManager.PlaylistChanged += () => PlaylistChanged?.Invoke(this, EventArgs.Empty);
        }

        public int Count => _playlistManager.Count;
        public IReadOnlyList<Track> Tracks => _playlistManager.Tracks;

        public Track? Get(int index) => _playlistManager.Get(index);

        public async Task<List<Track>> ProcessFilesAsync(
            IEnumerable<string> files,
            Action<int, int, string?>? reportProgress,
            bool replacePlaylist,
            CancellationToken cancellationToken)
        {
            return await _playlistManager.ProcessFilesAsync(files, reportProgress, replacePlaylist, cancellationToken);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _playlistManager.Count) return;
            _playlistManager.RemoveAt(index);
        }

        public void AddRange(List<Track> tracks)
        {
            _playlistManager.AddRange(tracks);
        }

        public void Clear()
        {
            _playlistManager.Clear();
        }

        public void Shuffle()
        {
            if (_playlistManager.Count <= 1) return;

            var shuffled = _playlistOps.ShuffleTracks(_playlistManager.Tracks);
            _playlistManager.Clear();
            _playlistManager.AddRange(shuffled);

            DebugUtils.Log("PlaylistController", "Shuffle",
                "Playlist shuffled", logLevel: DebugUtils.LogLevel.Info);
        }

        public void Sort(string sortField, bool descending)
        {
            if (_playlistManager.Count <= 1) return;

            var sorted = _playlistOps.SortTracks(_playlistManager.Tracks, sortField, descending);

            if (sorted == null)
            {
                DebugUtils.Log("PlaylistController", "Sort",
                    $"Invalid sort field: {sortField}", logLevel: DebugUtils.LogLevel.Warning);
                return;
            }

            _playlistManager.Clear();
            _playlistManager.AddRange(sorted);

            DebugUtils.Log("PlaylistController", "Sort",
                $"Sorted by {sortField} (descending: {descending})", logLevel: DebugUtils.LogLevel.Info);
        }

        public void SetOrder(IEnumerable<Track> tracks)
        {
            _playlistManager.SetOrder(tracks);
        }

        public async Task LoadFromM3U(string filePath, Action<int, int, string?>? reportProgress, CancellationToken cancellationToken)
        {
            await _playlistManager.LoadFromM3U(filePath, reportProgress, cancellationToken);
        }

        public async Task LoadFromJsonPl(string filePath)
        {
            await Task.Run(() => _playlistManager.LoadFromJsonPl(filePath));
        }

        public async Task SaveToM3U(string filePath, List<Track>? tracks = null)
        {
            await Task.Run(() => _playlistManager.SaveToM3U(filePath, tracks));
        }

        public async Task SaveToJsonPl(string filePath, List<Track>? tracks = null)
        {
            await Task.Run(() => _playlistManager.SaveToJsonPl(filePath, tracks));
        }

        public List<Track> GenerateSmartPlaylist(SmartShuffleMode mode)
        {
            return _playlistOps.GenerateSmartPlaylist(_playlistManager.Tracks, mode);
        }

        public TimeSpan CalculateTotalDuration(List<Track> tracks)
        {
            return _playlistOps.CalculateTotalDuration(tracks);
        }

        public static string GetPlaylistFilter(string defaultExtension = ".m3u")
        {
            return defaultExtension == ".jsonpl"
                ? "jsonPlaylist|*.jsonpl|M3U Playlists|*.m3u;*.m3u8|All Supported|*.m3u;*.m3u8;*.jsonpl"
                : "M3U Playlists|*.m3u;*.m3u8|jsonPlaylist|*.jsonpl|All Supported|*.m3u;*.m3u8;*.jsonpl";
        }
    }
}
