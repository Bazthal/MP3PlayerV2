﻿using System.Text;
using BazthalLib;

namespace MP3PlayerV2
{
    /// <summary>
    /// Manages a collection of tracks in a playlist, providing functionality to add, remove, shuffle, and persist
    /// tracks.
    /// </summary>
    /// <remarks>The <see cref="PlaylistManager"/> class allows for the management of a playlist through
    /// various operations such as adding or removing tracks, shuffling the playlist, and saving or loading playlists in
    /// M3U format. It also provides event notifications when the playlist is modified.</remarks>
    public class PlaylistManager
    {

        private readonly List<Track> _tracks = [];

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

        public int Count => _tracks.Count;

        /// <summary>
        /// Saves the current playlist to a file in M3U format.
        /// </summary>
        /// <remarks>The M3U file format is a plain text format used for creating multimedia playlists.
        /// Each track in the playlist is represented by an entry that includes metadata such as duration and artist
        /// information.</remarks>
        /// <param name="filePath">The path to the file where the M3U playlist will be saved. Cannot be null or empty.</param>
        public void SaveToM3U(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");

            foreach (var track in _tracks)
            {
                sb.AppendLine($"#EXTINF:{track.DurationSeconds},{track.Artist} - {track.Title}");
                sb.AppendLine(track.FilePath);
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Normalizes a file path by handling URI schemes, decoding URL-encoded characters, and resolving relative
        /// paths.
        /// </summary>
        /// <remarks>This method attempts to decode URI paths that start with "file://". If the path is a
        /// relative path starting with "..\", it resolves it to the user's Music folder.</remarks>
        /// <param name="path">The file path to normalize. This can be a URI or a relative path.</param>
        /// <returns>A normalized file path. Returns an empty string if the input path is null, empty, or consists only of
        /// white-space characters.</returns>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            // Try URI decoding if it starts with file://
            if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(path);
                    path = uri.LocalPath;
                }
                catch
                {
                    path = path.Replace("file:///", "").Replace('/', '\\');
                }
            }

            // Decode URL-encoded characters
            path = Uri.UnescapeDataString(path);

            // Replace relative Music path if applicable( Windows Media player does this nonsence)
            if (path.StartsWith("..\\"))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), path[3..]);
            }

            return path.Trim();

        }

        /// <summary>
        /// Loads a playlist from an M3U or M3U8 file and returns a list of tracks.
        /// </summary>
        /// <remarks>The method reads the specified M3U or M3U8 file, extracts track paths, and retrieves
        /// metadata for each track using the TagLib library.  It supports concurrent processing to improve performance,
        /// with a maximum concurrency level based on the number of processor cores. Tracks are returned in the order
        /// they appear in the playlist file. If a track's metadata cannot be read, it is skipped.</remarks>
        /// <param name="filePath">The path to the M3U or M3U8 file to load. The file extension determines the encoding used: UTF-8 for ".m3u8"
        /// and the system default for ".m3u".</param>
        /// <returns>A list of <see cref="Track"/> objects representing the tracks in the playlist. The list is empty if no valid
        /// tracks are found.</returns>
        public async Task<List<Track>> LoadFromM3U(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var encoding = ext == ".m3u8" ? Encoding.UTF8 : Encoding.Default;
            var lines = File.ReadAllLines(filePath, encoding);

            var paths = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                {
                    string? path = (i + 1 < lines.Length) ? NormalizePath(lines[++i].Trim()) : null;
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                        paths.Add(path);
                }
                else if (!line.StartsWith('#'))
                {
                    var path = NormalizePath(line);
                    if (File.Exists(path))
                        paths.Add(path);
                }
            }
            int maxConcurrency = Math.Min(Environment.ProcessorCount * 2, 20);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task<(int Index, Track? Track)>>();

            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                var index = i;

                await semaphore.WaitAsync();

                var task = Task.Run(() =>
                {
                    try
                    {
                        var tagFile = TagLib.File.Create(path);
                        var track = new Track
                        {
                            FilePath = path,
                            Title = !string.IsNullOrEmpty(tagFile.Tag.Title)
                                ? tagFile.Tag.Title
                                : Path.GetFileNameWithoutExtension(path),
                            Artist = (tagFile.Tag.Performers != null && tagFile.Tag.Performers.Length > 0)
                                ? string.Join("/", tagFile.Tag.Performers)
                                : "Unknown Artist",
                            Album = !string.IsNullOrEmpty(tagFile.Tag.Album) ? tagFile.Tag.Album : "",
                            DurationSeconds = (int)tagFile.Properties.Duration.TotalSeconds,
                            Hash = TrackDatabase.ComputeFileHash(path)
                        };
                        
                        return (Index: index, Track: track);
                    }
                    catch (Exception ex)
                    {
                        DebugUtils.Log("LoadFromM3U", "PlaylistManager", ex.Message);
#nullable disable
                        return (Index: index, Track: null);
#nullable enable
                    }
                    finally
                    {
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

            var orderedTracks = results
                .Where(r => r.Track != null)
                .OrderBy(r => r.Index)
                .Select(r => r.Track!)
                .ToList();

            _tracks.Clear();
            _tracks.AddRange(orderedTracks);
            PlaylistChanged?.Invoke();

            return orderedTracks;
        }

    }

}
