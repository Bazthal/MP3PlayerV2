using BazthalLib;
using BazthalLib.Controls;
using LiteDB;
using MP3PlayerV2.Models;
using System.Diagnostics;

namespace MP3PlayerV2.Services
{
    public record MigrationProgress(int Current, int Total, string EtaText);
    public static class TrackDatabase
    {
        /// <summary>
        /// Represents the file path to the database used for storing play data.
        /// </summary>
        internal static readonly string DatabasePath = Path.Combine(Application.StartupPath, "Playdata/data.db");

        /// <summary>
        /// Saves or updates the specified track in the database.
        /// </summary>
        /// <remarks>This method uses a LiteDB database to store track information. If a track with the
        /// same identifier already exists, it will be updated; otherwise, a new entry will be created.</remarks>
        /// <param name = "track">The track to be saved or updated. Cannot be null.</param>
        public static void SaveStats(Track track)
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");
            track.LastUpdated = DateTime.UtcNow; //set the last updated at the time of saving to database

            col.Upsert(track);
        }

        /// <summary>
        /// Loads playback statistics for the specified track from the database.
        /// </summary>
        /// <param name="track">The <see cref="Track"/> instance to populate with loaded statistics. Cannot be null.</param>
        /// <remarks>
        /// If statistics for the track exist in the database, they are applied to the provided <paramref name="track"/> instance.
        /// If no statistics are found, the method does nothing.
        /// Any exceptions encountered during loading are logged using <see cref="DebugUtils.Log"/>.
        /// </remarks>
        public static void LoadStats(Track track)
        {
            {
                try
                {
                    using var db = new LiteDatabase(DatabasePath);
                    var col = db.GetCollection<Track>("tracks");
                    var saved = col.FindById(track.Guid);
                    if (saved != null)
                    {
                        ApplyStats(track, saved);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    DebugUtils.Log("Error", "Load Stats", ex.Message);
                }
            }
        }

        /// <summary>
        /// Copies statistical data from the source track to the target track.
        /// </summary>
        /// <remarks>This method updates the target track with play counts, skip counts, ratings, flags, and other
        /// statistical information from the source track. It is typically used to synchronize or transfer track statistics
        /// between two instances.</remarks>
        /// <param name="target">The track to which the statistical data will be applied.</param>
        /// <param name="source">The track from which the statistical data will be copied.</param>
        private static void ApplyStats(Track target, Track source)
        {
            // Counts
            target.PlayCount = source.PlayCount;
            target.SkipCount = source.SkipCount;
            target.PlayCompleteCount = source.PlayCompleteCount;

            // Dates
            target.LastPlayed = source.LastPlayed;
            target.LastStatReset = source.LastStatReset;
            target.LastDecayApplied = source.LastDecayApplied;

            // Ratings
            target.RatingScore = source.RatingScore;
            target.StarRating = source.StarRating;

            // Flags
            target.Liked = source.Liked;
            target.Disliked = source.Disliked;

        }

        /// <summary>
        /// Retrieves the total number of tracks stored in the database.
        /// </summary>
        /// <remarks>This method accesses the database specified by <c>DatabasePath</c> and counts the
        /// entries  in the "tracks" collection. Ensure that the database file exists and is accessible before  calling
        /// this method.</remarks>
        /// <returns>The total count of tracks in the database. Returns 0 if no tracks are found.</returns>
        public static int GetTrackCount()
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");
            return col.Count();
        }

        /// <summary>
        /// Retrieves all tracks from the database.
        /// </summary>
        /// <remarks>This method uses a LiteDB database to fetch the tracks. Ensure that the database file
        /// exists at the specified <c>DatabasePath</c> and contains a collection named "tracks".</remarks>
        /// <returns>A list of <see cref="Track"/> objects representing all tracks stored in the database. If no tracks are
        /// found, returns an empty list.</returns>
        public static List<Track> GetAllTracks()
        {
            using var db = new LiteDatabase(DatabasePath);
            return db.GetCollection<Track>("tracks").FindAll().ToList();
        }

        /// <summary>
        /// Deletes all data from the database and rebuilds its structure.
        /// </summary>
        /// <remarks>This method removes the "tracks" collection from the database and performs a database
        /// rebuild operation.  Use this method with caution, as it will result in the loss of all data in the specified
        /// collection.</remarks>
        public static void NukeDatabase()
        {
            try
            {
                using var db = new LiteDatabase(DatabasePath);

                db.DropCollection("tracks");

                db.Rebuild();
            }
            catch (Exception ex)
            {
                ThemableMessageBox.Show(ex.Message);
                DebugUtils.Log("Track Database", "Nuke Database", ex.Message);
            }
        }

        /// <summary>
        /// Resets the in-memory playback statistics and metadata for the specified track.
        /// </summary>
        /// <remarks>This method resets properties such as play count, skip count, rating, and other
        /// related metadata  to their default values. The <see cref="Track.LastStatReset"/> property is updated to the
        /// current UTC time.</remarks>
        /// <param name="track">The track whose playback statistics and metadata will be reset. Cannot be <see langword="null"/>.</param>
        public static void ResetInMemory(Track track)
        {
            if (track == null) return;
            track.PlayCount = 0;
            track.PlayCompleteCount = 0;
            track.SkipCount = 0;
            track.LastPlayed = null;
            track.Liked = false;
            track.Disliked = false;
            track.RatingScore = 0;
            track.StarRating = 0;
            track.LastStatReset = DateTime.UtcNow;
        }

        /// <summary>
        /// Computes a SHA-1 hash for the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file to hash.</param>
        /// <param name="full">
        /// If <see langword="true"/>, computes the hash using the entire file contents.
        /// If <see langword="false"/>, computes the hash using the file length, the first 8 KB, and the last 8 KB (if the file is larger than 8 KB).
        /// </param>
        /// <returns>The computed hash as a lowercase hexadecimal string.</returns>
        public static string ComputeFileHash(string filePath, bool full = false) // Default to partial file for hash
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: false);

            if (full)
            {
                var hashBytes = sha1.ComputeHash(stream);
                return Convert.ToHexStringLower(hashBytes);
            }
            else
            {
                const int bufferSize = 8192;
                var buffer = new byte[bufferSize];
                int read;

                var lengthBytes = BitConverter.GetBytes(stream.Length);
                sha1.TransformBlock(lengthBytes, 0, lengthBytes.Length, lengthBytes, 0);

                if ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    sha1.TransformBlock(buffer, 0, read, buffer, 0);

                if (stream.Length > bufferSize)
                {
                    stream.Seek(-bufferSize, SeekOrigin.End);
                    if ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        sha1.TransformBlock(buffer, 0, read, buffer, 0);
                }

                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return Convert.ToHexStringLower(sha1.Hash!);
            }
        }

        /// <summary>
        /// Preloads track GUIDs from the database and returns a dictionary mapping track identifiers to their GUIDs.
        /// </summary>
        /// <remarks>This method retrieves all tracks from the database and filters out any tracks with an
        /// empty GUID. The dictionary keys are determined by the track's hash if available; otherwise, the file path is
        /// used.</remarks>
        /// <returns>A dictionary where the keys are track identifiers (either the hash or file path) and the values are the
        /// corresponding GUIDs. The dictionary will be empty if no valid tracks are found.</returns>
        public static Dictionary<string, Guid> PreloadTrackGuids()
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");

            return col.FindAll()
                      .Where(t => t.Guid != Guid.Empty)
                      .ToDictionary(
                          t => !string.IsNullOrEmpty(t.Hash) ? t.Hash : t.FilePath,
                          t => t.Guid
                      );
        }

        /// <summary>
        /// Assigns a <see cref="Guid"/> to the specified <paramref name="track"/> object using a provided cache.
        /// </summary>
        /// <remarks>The method attempts to assign a <see cref="Guid"/> to the <paramref name="track"/> in
        /// the following order: <list type="number"> <item>If the track's <see cref="Track.Hash"/> is not null or
        /// empty, it checks the cache for a matching <see cref="Guid"/>.</item> <item>If no match is found, it checks
        /// the cache using the track's <see cref="Track.FilePath"/>.</item> <item>If neither lookup succeeds, it
        /// assigns a new <see cref="Guid"/> to the track, unless the track already has a non-empty <see
        /// cref="Guid"/>.</item> </list></remarks>
        /// <param name="track">The track object to which the <see cref="Guid"/> will be assigned. Must not be <c>null</c>, and its <see
        /// cref="Track.FilePath"/> must not be null or whitespace.</param>
        /// <param name="cache">A dictionary containing cached <see cref="Guid"/> values, keyed by track hash or file path.</param>
        /// <returns>The <see cref="Guid"/> assigned to the <paramref name="track"/>. Returns <see cref="Guid.Empty"/> if the
        /// <paramref name="track"/> is invalid. If no cached <see cref="Guid"/> is found, a new <see cref="Guid"/> is
        /// generated.</returns>
        public static Guid AssignGuidFromCache(Track track, Dictionary<string, Guid> cache)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.FilePath))
                return Guid.Empty;

            if (!string.IsNullOrEmpty(track.Hash) && cache.TryGetValue(track.Hash, out var guid))
                return track.Guid = guid;

            if (cache.TryGetValue(track.FilePath, out guid))
                return track.Guid = guid;

            return track.Guid = track.Guid != Guid.Empty ? track.Guid : Guid.NewGuid();
        }

        /// <summary>
        /// Determines whether a track with the specified hash exists in the database.
        /// </summary>
        /// <param name = "hash">The unique hash of the track to check for existence. Cannot be null or empty.</param>
        /// <returns><see langword="true"/> if a track with the specified hash exists in the database; otherwise, <see 
        ///langword="false"/>.</returns>
        public static bool TrackExists(Guid guid)
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");
            return col.Exists(t => t.Guid == guid);
        }

        /// <summary>
        /// Creates a backup of the track database file.
        /// </summary>
        /// <remarks>
        /// This method uses the BazthalLib library to create up to 10 backup copies of the database file.
        /// </remarks>
        internal static void BackUpDatabase()
        {
            BazthalLib.Systems.IO.Files.CreateBackup(DatabasePath, 10);
        }

        /// <summary>
        /// Deletes the statistics for the specified track from the database.
        /// </summary>
        /// <param name="track">The <see cref="Track"/> whose statistics should be deleted. Cannot be null.</param>
        public static void DeleteStats(Track track)
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");
            col.Delete(track.Guid);
        }

        /// <summary>
        /// Resets the specified statistic for the given track to its default value.
        /// </summary>
        /// <remarks>This method updates both the in-memory <paramref name="track"/> object and the
        /// corresponding record in the database. The <see cref="Track.LastStatReset"/> property is updated to the
        /// current UTC time.</remarks>
        /// <param name="track">The track whose statistic will be reset. Cannot be <see langword="null"/>.</param>
        /// <param name="tag">The name of the statistic to reset. Supported values are: <list type="bullet">
        /// <item><description><c>"playcount"</c> - Resets the play count and play complete count to
        /// 0.</description></item> <item><description><c>"skipcount"</c> - Resets the skip count to
        /// 0.</description></item> <item><description><c>"lastplayed"</c> - Clears the last played
        /// timestamp.</description></item> <item><description><c>"liked"</c> - Sets the liked status to <see
        /// langword="false"/>.</description></item> <item><description><c>"disliked"</c> - Sets the disliked status to
        /// <see langword="false"/>.</description></item> <item><description><c>"ratingscore"</c> - Resets the rating
        /// score and star rating to 0.</description></item> </list> The comparison is case-insensitive. If <paramref
        /// name="tag"/> is <see langword="null"/> or does not match a supported value, no action is taken.</param>
        public static void ResetStats(Track track, string tag)
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<Track>("tracks");
            var existing = col.FindById(track.Guid);
            if (existing == null)
                return;
            switch (tag?.ToLowerInvariant())
            {
                case "playcount":
                    existing.PlayCount = 0;
                    track.PlayCount = 0;
                    existing.PlayCompleteCount = 0;
                    track.PlayCompleteCount = 0;
                    break;
                case "skipcount":
                    existing.SkipCount = 0;
                    track.SkipCount = 0;
                    break;
                case "lastplayed":
                    existing.LastPlayed = null;
                    track.LastPlayed = null;
                    break;
                case "liked":
                    existing.Liked = false;
                    track.Liked = false;
                    break;
                case "disliked":
                    existing.Disliked = false;
                    track.Disliked = false;
                    break;
                case "ratingscore":
                    existing.RatingScore = 0;
                    track.RatingScore = 0;
                    existing.StarRating = 0;
                    track.StarRating = 0;
                    break;
            }

            existing.LastStatReset = DateTime.UtcNow;
            track.LastStatReset = existing.LastStatReset;
            col.Update(existing);
        }

        /// <summary>
        /// Imports tracks from a legacy LiteDB database file into the current track database.
        /// </summary>
        /// <param name="legacyDatabasePath">The file path to the legacy LiteDB database containing track data.</param>
        /// <param name="dialog">A <see cref="ThemableProcessingDialog"/> instance used to display progress and status updates during import.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>The number of tracks successfully imported into the new database.</returns>
        /// <remarks>
        /// This method reads all track records from the specified legacy database, computes a fast hash for each track file,
        /// and inserts or merges them into the new database. Duplicate tracks are deduplicated by hash and file path, with
        /// playback statistics merged. Progress is reported to the provided dialog, and the operation supports concurrent
        /// processing and cancellation.
        /// </remarks>
        public static async Task<int> ImportTracksAsync(string legacyDatabasePath, ThemableProcessingDialog dialog, CancellationToken cancellationToken)
        {
            if (!File.Exists(legacyDatabasePath))
                throw new FileNotFoundException("Legacy database not found", legacyDatabasePath);

            using var legacyDb = new LiteDatabase(legacyDatabasePath);
            var legacyCol = legacyDb.GetCollection("tracks");

            using var newDb = new LiteDatabase(DatabasePath);
            var newCol = newDb.GetCollection<Track>("tracks");

            var legacyDocs = legacyCol.FindAll().ToList();
            int total = legacyDocs.Count;
            int processed = 0;
            int imported = 0;

            if (total == 0)
                return 0;

            var stopwatch = Stopwatch.StartNew();
            string? lastEta = null;

            int processorCount = Environment.ProcessorCount;
            int maxConcurrency = Math.Min(processorCount * 2, processorCount < 8 ? 20 : 40);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            BackUpDatabase();

            foreach (var doc in legacyDocs)
            {
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(() =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        // Extract legacy info
                        string? filePath = doc.TryGetValue("FilePath", out var fpVal) ? fpVal.AsString : null;
                        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                            return;
                        string fastHash = ComputeFileHash(filePath, full: false);

                        // Build new Track with GUID as ID
                        var track = new Track
                        {
                            Guid = Guid.NewGuid(),
                            FilePath = filePath,
                            Title = doc.TryGetValue("Title", out var titleVal) ? titleVal.AsString : "Unknown Title",
                            Artist = doc.TryGetValue("Artist", out var artistVal) ? artistVal.AsString : "Unknown Artist",
                            Album = doc.TryGetValue("Album", out var albumVal) ? albumVal.AsString : null,
                            DurationSeconds = doc.TryGetValue("DurationSeconds", out var durationVal) ? durationVal : 0,
                            Hash = fastHash,
                            PlayCount = doc.TryGetValue("PlayCount", out var pcVal) ? (int?)pcVal.AsInt32 : null,
                            SkipCount = doc.TryGetValue("TimesSkipped", out var tsVal) ? (int?)tsVal.AsInt32 : null,
                            LastPlayed = doc.TryGetValue("LastPlayed", out var lastplayed) ? (DateTime?)lastplayed : null,
                            MigrationVersion = 1,
                            LastUpdated = DateTime.UtcNow
                        };

                        LiteDbWriteQueue.Enqueue(
                            () =>
                            {
                                var existing = newCol.FindOne(x => x.Hash == track.Hash);

                                if (existing == null)
                                    existing = newCol.FindOne(x => x.FilePath == track.FilePath);

                                if (existing == null)
                                {
                                    newCol.Insert(track);
                                    Interlocked.Increment(ref imported);
                                }
                                else
                                {
                                    // Merge stats if duplicate
                                    existing.PlayCount = Math.Max(existing.PlayCount ?? 0, track.PlayCount ?? 0);
                                    existing.SkipCount = Math.Max(existing.SkipCount ?? 0, track.SkipCount ?? 0);
                                    existing.LastUpdated = DateTime.UtcNow;

                                    existing.Hash = track.Hash;

                                    newCol.Update(existing);
                                }
                            },
                            () =>
                            {
                                int current = Interlocked.Increment(ref processed);

                                if (current % 10 == 0 || current == total)
                                {
                                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                                    double avg = elapsed / current;
                                    double etaSec = avg * (total - current);
                                    lastEta = $"ETA: {TimeSpan.FromSeconds(etaSec):mm\\:ss}";
                                }

                                dialog.Invoke(() =>
                                    dialog.SetProgress("Importing", "Tracks", current, total, lastEta)
                                );
                            },
                            cancellationToken
                        );

                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            await LiteDbWriteQueue.WaitForEmptyAsync();

            return imported;
        }

        /// <summary>
        /// Asynchronously prunes old track data from the database based on the specified settings.
        /// </summary>
        /// <remarks>This method identifies tracks in the database that have not been updated within the
        /// time period specified by <see cref="PlayDataSettings.PruneDays"/> and either deletes or archives them based
        /// on the settings. Progress is reported through the provided dialog, and the operation can be canceled using
        /// the <paramref name="cancellationToken"/> or the dialog's cancellation token. <para> If pruning is disabled
        /// in the settings or the prune days value is less than or equal to zero, the method returns immediately with a
        /// result of 0. </para> <para> The method uses a background queue to process database write operations,
        /// ensuring that all pruning actions are completed before returning. </para></remarks>
        /// <param name="settings">The settings that control the pruning operation, including the cutoff age and whether to archive tracks
        /// instead of deleting them.</param>
        /// <param name="dialog">A dialog used to display progress and status updates during the pruning operation.</param>
        /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of tracks that were
        /// successfully pruned.</returns>
        public static async Task<int> PruneOldDataAsync(PlayDataSettings settings, ThemableProcessingDialog dialog, CancellationToken cancellationToken = default)
        {
            if (!settings.EnablePruning || settings.PruneDays <= 0)
                return 0;

            int pruned = 0;
            int total = 0;
            int processed = 0;
            var stopwatch = Stopwatch.StartNew();
            string? lastEta = null;

            dialog.Show();
            var cutoff = DateTime.UtcNow.AddDays(-settings.PruneDays);

            List<Track> candidates;
            using (var db = new LiteDatabase(DatabasePath))
            {
                var col = db.GetCollection<Track>("tracks");
                candidates = col.Find(t => t.LastUpdated == null || t.LastUpdated < cutoff).ToList();
            }
            total = candidates.Count;

            if (total == 0)
            {
                dialog.SetCompleted("No old tracks found to prune.");
                dialog.CloseAfter(1000);
                return 0;
            }

            dialog.SetProgress("Preparing to prune", 0, total, "ETA: --:--");

            BackUpDatabase();
            foreach (var track in candidates)
            {
                if (cancellationToken.IsCancellationRequested || dialog.Token.IsCancellationRequested)
                    break;

                LiteDbWriteQueue.Enqueue(() =>
                {
                    try
                    {
                        using var db2 = new LiteDatabase(DatabasePath);
                        var col2 = db2.GetCollection<Track>("tracks");

                        if (settings.ArchiveOverDelete)
                        {
                            var archiveCol = db2.GetCollection<Track>("archived_tracks");
                            archiveCol.Insert(track);
                        }

                        col2.Delete(track.Guid);
                        DebugUtils.Log("Prune", "Database", $"{track} - {track.LastPlayed}");
                        Interlocked.Increment(ref pruned);
                    }
                    catch (Exception ex)
                    {
                        DebugUtils.Log("Prune", "Database", ex.ToString());
                    }
                    finally
                    {

                        int current = Interlocked.Increment(ref processed);

                        string etaText;
                        if (current % 10 == 0 || current == total)
                        {
                            double elapsed = stopwatch.Elapsed.TotalSeconds;
                            double avg = elapsed / current;
                            double etaSec = avg * (total - current);
                            etaText = $"ETA: {TimeSpan.FromSeconds(etaSec):mm\\:ss}";
                            lastEta = etaText;
                        }
                        else
                        {
                            etaText = lastEta ?? "ETA: --:--";
                        }

                        dialog.Invoke(() =>
                            dialog.SetProgress("Pruning", "tracks", current, total, etaText)
                        );
                    }
                });
            }

            await LiteDbWriteQueue.WaitForEmptyAsync();

            dialog.SetCompleted($"Pruned {pruned} track(s).");
            dialog.CloseAfter(1000);
            return pruned;
        }


    }
}