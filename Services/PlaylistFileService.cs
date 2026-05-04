using MP3PlayerV2.Models;
using System.Text;
using System.Text.Encodings.Web;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Handles loading and saving playlists in various file formats (M3U, JSON).
    /// </summary>
    public class PlaylistFileService
    {
        private readonly TrackFileProcessor _trackProcessor;

        public PlaylistFileService(TrackFileProcessor trackProcessor)
        {
            _trackProcessor = trackProcessor;
        }

        /// <summary>
        /// Normalizes a file path by handling URI schemes, decoding URL-encoded characters, and resolving relative
        /// paths.
        /// </summary>
        /// <param name="path">The file path to normalize.</param>
        /// <returns>A normalized file path.</returns>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
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

            path = Uri.UnescapeDataString(path);

            if (path.StartsWith("..\\"))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), path[3..]);
            }

            return path.Trim();
        }

        /// <summary>
        /// Saves the playlist to a file in M3U format.
        /// </summary>
        /// <param name="filePath">The path to the file where the M3U playlist will be saved.</param>
        /// <param name="tracks">The list of tracks to save.</param>
        public void SaveToM3U(string filePath, List<Track> tracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");

            foreach (var track in tracks)
            {
                sb.AppendLine($"#EXTINF:{track.DurationSeconds},{track.Artist} - {track.Title}");
                sb.AppendLine(track.FilePath);
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Saves the playlist to a JSON file at the specified file path.
        /// </summary>
        /// <param name="filePath">The full path of the file where the playlist will be saved.</param>
        /// <param name="tracks">The list of tracks to save.</param>
        public void SaveToJsonPl(string filePath, List<Track> tracks)
        {
            var dtos = tracks.Select(t => new PlaylistEntry
            {
                Guid = t.Guid,
                Hash = t.Hash,
                FilePath = t.FilePath,
                Title = t.Title,
                Artist = t.Artist,
                Album = t.Album,
                DurationSeconds = t.DurationSeconds
            }).ToList();

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            var json = System.Text.Json.JsonSerializer.Serialize(dtos, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a playlist from a JSON file.
        /// </summary>
        /// <param name="filePath">The path to the JSON file containing the playlist data.</param>
        /// <param name="reportProgress">An optional callback to report progress during the loading process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A list of tracks loaded from the JSON file.</returns>
        public List<Track> LoadFromJsonPl(
            string filePath,
            Action<int, int, string?>? reportProgress = null,
            CancellationToken cancellationToken = default)
        {
            var json = File.ReadAllText(filePath);

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var entries = System.Text.Json.JsonSerializer.Deserialize<List<PlaylistEntry>>(json, jsonOptions);
            if (entries == null || entries.Count == 0) 
                return new List<Track>();

            var tracks = new List<Track>();
            int total = entries.Count;
            int index = 0;

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var track = new Track
                {
                    Guid = entry.Guid,
                    Hash = entry.Hash,
                    FilePath = NormalizePath(entry.FilePath),
                    Title = entry.Title,
                    Artist = entry.Artist,
                    Album = entry.Album,
                    DurationSeconds = entry.DurationSeconds
                };

                if (File.Exists(track.FilePath))
                {
                    tracks.Add(track);
                }
                reportProgress?.Invoke(++index, total, null);
            }

            TrackDatabase.LoadStatsBatch(tracks);

            return tracks;
        }

        /// <summary>
        /// Loads a playlist from an M3U or M3U8 file.
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
                    string? path = i + 1 < lines.Length ? NormalizePath(lines[++i].Trim()) : null;
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

            var orderedTracks = await _trackProcessor.ProcessFilesAsync(
                paths,
                reportProgress,
                cancellationToken
            );

            return orderedTracks;
        }
    }
}
