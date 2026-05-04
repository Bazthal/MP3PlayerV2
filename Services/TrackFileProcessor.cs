using BazthalLib;
using MP3PlayerV2.Models;
using System.Diagnostics;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Handles processing of audio files to create Track objects.
    /// </summary>
    public class TrackFileProcessor
    {
        private readonly TrackDurationCache _durationCache;

        public TrackFileProcessor(TrackDurationCache durationCache)
        {
            _durationCache = durationCache;
        }

        /// <summary>
        /// Creates a <see cref="Track"/> object from the specified audio file.
        /// </summary>
        /// <param name="path">The full file path to the audio file.</param>
        /// <param name="guidCache">An optional dictionary used to cache and assign a unique <see cref="Guid"/> to the track.</param>
        /// <returns>A <see cref="Track"/> object containing metadata extracted from the audio file, or <see langword="null"/> 
        /// if the file could not be processed.</returns>
        public Track? CreateTrackFromFile(string path, Dictionary<string, Guid>? guidCache = null)
        {
            try
            {
                using (var tagFile = TagLib.File.Create(path))
                {
                    var duration = tagFile.Properties.Duration;
                    var track = new Track
                    {
                        FilePath = path,
                        Title = !string.IsNullOrEmpty(tagFile.Tag.Title)
                            ? tagFile.Tag.Title
                            : Path.GetFileNameWithoutExtension(path),
                        Artist = tagFile.Tag.Performers?.Length > 0
                            ? string.Join("/", tagFile.Tag.Performers)
                            : "Unknown Artist",
                        Album = !string.IsNullOrEmpty(tagFile.Tag.Album) ? tagFile.Tag.Album : "",
                        DurationSeconds = (int)duration.TotalSeconds,
                        Hash = TrackDatabase.ComputeFileHash(path, false),
                    };

                    if (guidCache != null)
                        track.Guid = TrackDatabase.AssignGuidFromCache(track, guidCache);

                    _durationCache.SetDuration(track.Guid, duration);

                    return track;
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Track Model", path, ex.Message, logLevel: DebugUtils.LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Processes a collection of file paths asynchronously, creating tracks from the files.
        /// </summary>
        /// <param name="files">A collection of file paths to process.</param>
        /// <param name="reportProgress">An optional callback to report progress during processing.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation containing a list of <see cref="Track"/> objects.</returns>
        public async Task<List<Track>> ProcessFilesAsync(
            IEnumerable<string> files,
            Action<int, int, string?>? reportProgress,
            CancellationToken cancellationToken)
        {
            var paths = files.ToList();
            int total = paths.Count;
            int completed = 0;
            var stopwatch = Stopwatch.StartNew();

            var guidCache = TrackDatabase.PreloadTrackGuids();

            int processorCount = Environment.ProcessorCount;
            int maxConcurrency = Math.Min(processorCount * 2, processorCount < 8 ? 20 : 40);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task<(int Index, Track? Track)>>();

            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                var index = i;

                await semaphore.WaitAsync(CancellationToken.None);

                if (cancellationToken.IsCancellationRequested)
                {
                    semaphore.Release();
                    break;
                }

                var task = Task.Run(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var track = CreateTrackFromFile(path, guidCache);
                        return (Index: index, Track: track);
                    }
                    catch
                    {
                        return (Index: index, Track: null);
                    }
                    finally
                    {
                        int current = Interlocked.Increment(ref completed);
                        string? etaText = null;

                        if (current % MP3PlayerV2._updateStep == 0 || current == total)
                        {
                            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            double avgPerFile = elapsedSeconds / current;
                            int remaining = total - current;
                            double etaSeconds = avgPerFile * remaining;

                            etaText = $"ETA: {TimeSpan.FromSeconds(etaSeconds):mm\\:ss}";
                        }

                        reportProgress?.Invoke(current, total, etaText);
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            var orderedTracks = results
                .Where(r => r.Track != null)
                .OrderBy(r => r.Index)
                .Select(r => r.Track!)
                .ToList();

            TrackDatabase.LoadStatsBatch(orderedTracks);

            return orderedTracks;
        }
    }
}
