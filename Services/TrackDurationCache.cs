using MP3PlayerV2.Models;
using System.Collections.Concurrent;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Manages caching of track durations for efficient retrieval.
    /// </summary>
    public class TrackDurationCache
    {
        private readonly ConcurrentDictionary<Guid, TimeSpan> _durations = new();

        /// <summary>
        /// Stores the duration for a specific track.
        /// </summary>
        /// <param name="trackGuid">The unique identifier for the track.</param>
        /// <param name="duration">The duration of the track.</param>
        public void SetDuration(Guid trackGuid, TimeSpan duration)
        {
            _durations[trackGuid] = duration;
        }

        /// <summary>
        /// Retrieves the duration of the specified track.
        /// </summary>
        /// <param name="track">The track for which to retrieve the duration.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the duration of the track.</returns>
        public TimeSpan GetDuration(Track track)
        {
            if (track.Guid != Guid.Empty && _durations.TryGetValue(track.Guid, out var span))
                return span;
#nullable disable
            return TimeSpan.FromSeconds((long)track.DurationSeconds);
#nullable enable
        }

        /// <summary>
        /// Attempts to get the duration for a specific track GUID.
        /// </summary>
        /// <param name="trackGuid">The unique identifier for the track.</param>
        /// <param name="duration">The duration if found.</param>
        /// <returns>True if the duration was found; otherwise, false.</returns>
        public bool TryGetDuration(Guid trackGuid, out TimeSpan duration)
        {
            return _durations.TryGetValue(trackGuid, out duration);
        }

        /// <summary>
        /// Clears all cached durations.
        /// </summary>
        public void Clear()
        {
            _durations.Clear();
        }
    }
}
