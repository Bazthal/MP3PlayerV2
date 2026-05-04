using BazthalLib;
using BazthalLib.Controls;
using static BazthalLib.DebugUtils;
using LiteDB;
using MP3PlayerV2.Models;
using System.Diagnostics;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Represents a report detailing the results of a deduplication operation,  including merged and skipped items, as
    /// well as the total number of deletions.
    /// </summary>
    /// <remarks>This class provides a summary of the deduplication process by categorizing  the results into
    /// merged and skipped pairs. The <see cref="DeletedCount"/>  property reflects the total number of deletions based
    /// on the merged pairs.</remarks>
    public class DedupeReport
    {
        public List<MergePair> Merged { get; set; } = new();
        public List<MergePair> Skipped { get; set; } = new();
        public int DeletedCount => Merged.Count;
    }

    /// <summary>
    /// Represents a pair of tracks where one is considered the canonical version, and the other is a duplicate.
    /// </summary>
    /// <remarks>This class is typically used in scenarios where duplicate tracks need to be identified and
    /// resolved. The <see cref="Canonical"/> property holds the primary track, while the <see cref="Duplicate"/>
    /// property holds the duplicate track that may be merged or removed.</remarks>
    public class MergePair
    {
        public Track Canonical { get; set; } = new();
        public Track Duplicate { get; set; } = new();
    }

    /// <summary>
    /// Represents the progress of a migration operation, including the current step, total steps, and an estimated time
    /// of arrival (ETA) as text.
    /// </summary>
    /// <param name="Current">The current step number of the migration process. Must be a non-negative integer.</param>
    /// <param name="Total">The total number of steps in the migration process. Must be a positive integer.</param>
    /// <param name="EtaText">A textual representation of the estimated time remaining for the migration to complete. Can be null or empty if
    /// no ETA is available.</param>
    public record MigrationProgress(int Current, int Total, string EtaText);

    public static class TrackDatabase
    {
        private static readonly AppSettings _settings = new();

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
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");
                EnsureIndexes(col);
                track.LastUpdated = DateTime.UtcNow;

                col.Upsert(track);
            }
            catch { }
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
                DebugUtils.Log("Error", "Load Stats", ex.Message, logLevel: DebugUtils.LogLevel.Error);
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
            try
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
            catch { }

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
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");
                return col.Count();
            }
            catch { return 0; }
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
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                return db.GetCollection<Track>("tracks").FindAll().ToList();
            }
            catch { return []; }
        }

        /// <summary>
        /// Loads statistics for multiple tracks in a single database operation.
        /// </summary>
        /// <param name="tracks">The collection of tracks to load statistics for.</param>
        /// <remarks>This method is optimized for bulk loading, retrieving all track stats in a single
        /// database query rather than individual queries per track. Significantly faster than calling
        /// <see cref="LoadStats"/> for each track individually.</remarks>
        public static void LoadStatsBatch(IEnumerable<Track> tracks)
        {
            try
            {
                var trackList = tracks.ToList();
                if (trackList.Count == 0) return;

                var guids = trackList.Select(t => t.Guid).ToHashSet();

                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");

                var savedTracks = col.Find(t => guids.Contains(t.Guid)).ToDictionary(t => t.Guid);

                foreach (var track in trackList)
                {
                    if (savedTracks.TryGetValue(track.Guid, out var saved))
                    {
                        ApplyStats(track, saved);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Track Database", "Load Stats Batch", ex.Message, logLevel: DebugUtils.LogLevel.Error);
            }
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
                DebugUtils.Log("Track Database", "Nuke Database", ex.Message, logLevel: DebugUtils.LogLevel.Error);
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
        /// Preloads a dictionary mapping file paths and hashes to their corresponding track GUIDs.
        /// </summary>
        /// <remarks>This method retrieves all tracks from the database and creates a dictionary where
        /// each track's file path and hash (if available) are mapped to its GUID. Tracks with an empty GUID are
        /// excluded from the dictionary. The comparison for keys in the dictionary is case-insensitive.</remarks>
        /// <returns>A dictionary where the keys are file paths and hashes (case-insensitive), and the values are the
        /// corresponding track GUIDs. The dictionary will be empty if no valid tracks are found.</returns>
        public static Dictionary<string, Guid> PreloadTrackGuids()
        {
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");

                EnsureIndexes(col);

                var dict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

                foreach (var t in col.FindAll().Where(t => t.Guid != Guid.Empty))
                {
                    if (!string.IsNullOrEmpty(t.Hash))
                        dict[t.Hash] = t.Guid;
                    else if (!string.IsNullOrEmpty(t.FilePath))
                        dict[t.FilePath] = t.Guid;
                }

                return dict;
            }
            catch (Exception ex) {
                DebugUtils.Log("Track Database", "PreloadTrackGUID", $"Error preloading GUID's: {ex.Message}", logLevel:LogLevel.Error);
                return new (); }
        }

        /// <summary>
        /// Ensures that the database collection has the necessary indexes for optimal query performance.
        /// </summary>
        /// <param name="collection">The LiteDB collection to ensure indexes on.</param>
        private static void EnsureIndexes(ILiteCollection<Track> collection)
        {
            try
            {
                collection.EnsureIndex(x => x.Guid, unique: true);
                collection.EnsureIndex(x => x.Hash);
                collection.EnsureIndex(x => x.FilePath);
            }
            catch { }
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
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");
                return col.Exists(t => t.Guid == guid);
            }
            catch 
            {
                return false;           
            }

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
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");
                col.Delete(track.Guid);
            }
            catch { }
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
            try
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");
                var existing = col.FindById(track.Guid);
                var baseScore = track.Liked ? ConfigManager.Settings.TrackRating.ManualLikeBoost : track.Disliked ? ConfigManager.Settings.TrackRating.ManualDislikePenalty : 0;

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
                        existing.RatingScore = baseScore;
                        track.RatingScore = baseScore;
                        existing.StarRating = 0;
                        track.StarRating = 0;
                        break;
                }

                existing.LastStatReset = DateTime.UtcNow;
                track.LastStatReset = existing.LastStatReset;
                col.Update(existing);
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Track Database", "Reset Stats", $"Error With reseting stats: {ex.Message}", logLevel: LogLevel.Error);
            }
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
                        DebugUtils.Log("Prune", "Database", $"{track} - {track.LastPlayed}", logLevel: DebugUtils.LogLevel.Info);
                        Interlocked.Increment(ref pruned);
                    }
                    catch (Exception ex)
                    {
                        DebugUtils.Log("Prune", "Database", ex.ToString(), logLevel: LogLevel.Error);
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

        
        /// <summary>
        /// Asynchronously deduplicates tracks in the database by grouping them based on file path and optionally by hash.
        /// </summary>
        /// <param name="reportProgress">An optional callback to report progress, receiving the current group index, total group count, and an optional status message.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests during the deduplication process.</param>
        /// <param name="dryRun">If <c>true</c>, performs a simulation without making any changes to the database; otherwise, applies deduplication changes.</param>
        /// <param name="includeHashGrouping">If <c>true</c>, performs an additional deduplication pass grouping tracks by their hash values.</param>
        /// <param name="saveReportToFile">If <c>true</c>, saves the deduplication report as a JSON file in the playdata directory.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="DedupeReport"/> summarizing merged and skipped duplicates.</returns>
        /// <remarks>This method identifies duplicate tracks by grouping them first by file path and, if enabled, by hash.
        /// For each group with more than one track, it selects a canonical track (most recently updated) and merges statistics from duplicates.
        /// Duplicates are either deleted or archived based on application settings. The process can be canceled via the provided token.
        /// A detailed report of merged and skipped pairs is returned and optionally saved to disk.</remarks>
        public static async Task<DedupeReport> DeduplicateDatabaseAsync(Action<int, int, string?>? reportProgress = null, CancellationToken cancellationToken = default, bool dryRun = false, bool includeHashGrouping = true, bool saveReportToFile = true)
        {
            var report = new DedupeReport();

            if (!dryRun)
                BackUpDatabase();

            await Task.Run(() =>
            {
                using var db = new LiteDatabase(DatabasePath);
                var col = db.GetCollection<Track>("tracks");
                var deletedCol = _settings.PlayData.ArchiveOverDelete ? db.GetCollection<Track>("tracks_deleted") : null;

                var allTracks = col.FindAll().ToList();

                var groups = allTracks
                    .Where(t => !string.IsNullOrEmpty(t.FilePath))
                    .GroupBy(t => t.FilePath, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int totalGroups = groups.Count;
                int processedGroups = 0;

                foreach (var group in groups)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    processedGroups++;
                    reportProgress?.Invoke(processedGroups, totalGroups, null);

                    var tracks = group.ToList();
                    if (tracks.Count <= 1)
                        continue;

                    var canonical = tracks
                        .OrderByDescending(t => t.LastUpdated ?? DateTime.MinValue)
                        .First();

                    foreach (var duplicate in tracks.Where(t => t.Guid != canonical.Guid))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (canonical.DurationSeconds == null || duplicate.DurationSeconds == null ||
                            Math.Abs(canonical.DurationSeconds.Value - duplicate.DurationSeconds.Value) > 2)
                        {
                            report.Skipped.Add(new MergePair { Canonical = canonical, Duplicate = duplicate });
                            continue;
                        }

                        report.Merged.Add(new MergePair { Canonical = canonical, Duplicate = duplicate });

                        if (!dryRun)
                        {
                            MergeTrackData(canonical, duplicate);
                            if (_settings.PlayData.ArchiveOverDelete)
                                deletedCol?.Upsert(duplicate);
                            col.Delete(duplicate.Guid);
                        }
                    }

                    if (!dryRun)
                        col.Update(canonical);
                }

                if (includeHashGrouping)
                {
                    var hashGroups = allTracks
                        .Where(t => !string.IsNullOrEmpty(t.Hash))
                        .GroupBy(t => t.Hash, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    int totalHashGroups = hashGroups.Count;
                    int processedHashGroups = 0;

                    foreach (var group in hashGroups)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        processedHashGroups++;
                        reportProgress?.Invoke(processedHashGroups, totalHashGroups, $"Hash Group {processedHashGroups}/{totalHashGroups}");

                        var tracks = group.ToList();
                        if (tracks.Count <= 1)
                            continue;

                        var canonical = tracks
                            .OrderByDescending(t => t.LastUpdated ?? DateTime.MinValue)
                            .First();

                        foreach (var duplicate in tracks.Where(t => t.Guid != canonical.Guid))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (canonical.DurationSeconds == null || duplicate.DurationSeconds == null ||
                                Math.Abs(canonical.DurationSeconds.Value - duplicate.DurationSeconds.Value) > 2)
                            {
                                report.Skipped.Add(new MergePair { Canonical = canonical, Duplicate = duplicate });
                                continue;
                            }

                            report.Merged.Add(new MergePair { Canonical = canonical, Duplicate = duplicate });

                            if (!dryRun)
                            {
                                MergeTrackData(canonical, duplicate);
                                if (_settings.PlayData.ArchiveOverDelete)
                                    deletedCol?.Upsert(duplicate);
                                col.Delete(duplicate.Guid);
                            }
                        }

                        if (!dryRun)
                            col.Update(canonical);
                    }
                }
            }, cancellationToken);

            if (saveReportToFile)
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(
                        report,
                        options: new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    var fileName = $"playdata/dedupe_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    var path = Path.Combine(Application.StartupPath, fileName);
                    File.WriteAllText(path, json);
                }
                catch (Exception ex)
                {
                    DebugUtils.Log("Dedupe Save to JSON", "TrackDatabase", $"report: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
                }
            }

            return report;
        }

        /// <summary>
        /// Merges the statistical and metadata information from a duplicate track into a canonical track.
        /// </summary>
        /// <remarks>This method combines play counts, skip counts, and other statistical data from the
        /// duplicate track into the canonical track. For date-related fields, the most recent value is retained. For
        /// ratings, the higher value is preserved. Boolean fields such as <see cref="Track.Liked"/> and <see
        /// cref="Track.Disliked"/> are combined using a logical OR operation.</remarks>
        /// <param name="canonical">The canonical <see cref="Track"/> object that will be updated with merged data.</param>
        /// <param name="duplicate">The duplicate <see cref="Track"/> object whose data will be merged into the canonical track.</param>
        private static void MergeTrackData(Track canonical, Track duplicate)
        {
            canonical.PlayCount = (canonical.PlayCount ?? 0) + (duplicate.PlayCount ?? 0);
            canonical.SkipCount = (canonical.SkipCount ?? 0) + (duplicate.SkipCount ?? 0);
            canonical.PlayCompleteCount = (canonical.PlayCompleteCount ?? 0) + (duplicate.PlayCompleteCount ?? 0);

            if (duplicate.LastPlayed.HasValue &&
                (!canonical.LastPlayed.HasValue || duplicate.LastPlayed > canonical.LastPlayed))
            {
                canonical.LastPlayed = duplicate.LastPlayed;
            }

            if (duplicate.LastStatReset.HasValue &&
                (!canonical.LastStatReset.HasValue || duplicate.LastStatReset > canonical.LastStatReset))
            {
                canonical.LastStatReset = duplicate.LastStatReset;
            }

            if (duplicate.RatingScore > canonical.RatingScore)
                canonical.RatingScore = duplicate.RatingScore;

            if (duplicate.StarRating > canonical.StarRating)
                canonical.StarRating = duplicate.StarRating;

            canonical.Liked = canonical.Liked || duplicate.Liked;
            canonical.Disliked = canonical.Disliked || duplicate.Disliked;

            if (duplicate.LastDecayApplied.HasValue &&
                (!canonical.LastDecayApplied.HasValue || duplicate.LastDecayApplied > canonical.LastDecayApplied))
            {
                canonical.LastDecayApplied = duplicate.LastDecayApplied;
            }
        }

    }
}