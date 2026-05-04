using MP3PlayerV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Provides operations for manipulating playlists including sorting and filtering.
    /// </summary>
    public class PlaylistOperationsService
    {
        #region Sorting

        /// <summary>
        /// Sorts a collection of tracks based on the specified field and order.
        /// </summary>
        /// <param name="tracks">The tracks to sort.</param>
        /// <param name="sortField">The field to sort by (Artist, Title, Album, PlayCount, LastPlayed, Liked, Disliked, Rating).</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        /// <returns>A sorted list of tracks, or null if the sort field is invalid.</returns>
        public List<Track>? SortTracks(IEnumerable<Track> tracks, string sortField, bool descending = false)
        {
            if (tracks == null || !tracks.Any())
                return new List<Track>();

            var trackList = tracks.ToList();

            var sorted = sortField?.ToLowerInvariant() switch
            {
                "artist" => OrderBy(trackList, t => t.Artist, descending),
                "title" => OrderBy(trackList, t => t.Title, descending),
                "album" => OrderBy(trackList, t => (t.Album?.ToLowerInvariant() ?? "", t.Artist?.ToLowerInvariant() ?? ""), descending),
                "playcount" => OrderBy(trackList, t => t.PlayCount, descending),
                "lastplayed" => OrderBy(trackList, t => t.LastPlayed ?? DateTime.MinValue, descending),
                "liked" => OrderBy(trackList, t => t.Liked, descending),
                "disliked" => OrderBy(trackList, t => t.Disliked, descending),
                "rating" => OrderBy(trackList, t => t.RatingScore, descending),
                _ => null
            };

            return sorted;
        }

        /// <summary>
        /// Shuffles a collection of tracks using the Fisher-Yates algorithm.
        /// </summary>
        /// <param name="tracks">The tracks to shuffle.</param>
        /// <returns>A shuffled list of tracks.</returns>
        public List<Track> ShuffleTracks(IEnumerable<Track> tracks)
        {
            if (tracks == null || !tracks.Any())
                return new List<Track>();

            var trackList = tracks.ToList();
            var random = new Random();
            
            // Fisher-Yates shuffle
            for (int i = trackList.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (trackList[j], trackList[i]) = (trackList[i], trackList[j]);
            }

            return trackList;
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Filters tracks based on the specified smart shuffle mode.
        /// </summary>
        /// <param name="tracks">The tracks to filter.</param>
        /// <param name="mode">The smart shuffle mode to apply.</param>
        /// <returns>A filtered list of tracks based on the mode criteria.</returns>
        public List<Track> GenerateSmartPlaylist(IEnumerable<Track> tracks, SmartShuffleMode mode)
        {
            if (tracks == null || !tracks.Any())
                return new List<Track>();

            var trackList = tracks.ToList();

            return mode switch
            {
                SmartShuffleMode.UnplayedFirst => FilterUnplayed(trackList),
                SmartShuffleMode.MostPlayed => FilterMostPlayed(trackList),
                SmartShuffleMode.LikedOnly => FilterLiked(trackList),
                SmartShuffleMode.AvoidDisliked => FilterAvoidDisliked(trackList),
                SmartShuffleMode.WeightedByRating => new List<Track>(), // Not yet implemented
                SmartShuffleMode.UnratedFirst => new List<Track>(), // Not yet implemented
                _ => new List<Track>()
            };
        }

        /// <summary>
        /// Calculates the total duration of a collection of tracks.
        /// </summary>
        /// <param name="tracks">The tracks to calculate duration for.</param>
        /// <returns>The total duration as a TimeSpan.</returns>
        public TimeSpan CalculateTotalDuration(IEnumerable<Track> tracks)
        {
            if (tracks == null || !tracks.Any())
                return TimeSpan.Zero;

            var totalSeconds = tracks.Sum(t => t.DurationSeconds ?? 0);
            return TimeSpan.FromSeconds(totalSeconds);
        }

        #endregion

        #region Private Helper Methods

        private List<Track> OrderBy<TKey>(List<Track> tracks, Func<Track, TKey> keySelector, bool descending)
        {
            return descending
                ? tracks.OrderByDescending(keySelector).ToList()
                : tracks.OrderBy(keySelector).ToList();
        }

        private List<Track> FilterUnplayed(List<Track> tracks)
        {
            return tracks.Where(t => (t.PlayCount ?? 0) == 0).ToList();
        }

        private List<Track> FilterMostPlayed(List<Track> tracks)
        {
            if (!tracks.Any(t => t.PlayCount.HasValue))
                return new List<Track>();

            var avg = tracks.Where(t => t.PlayCount.HasValue)
                           .Average(t => t.PlayCount!.Value);
            var minRequired = Math.Max(2, (int)Math.Ceiling(avg));

            var filtered = tracks
                .Where(t => (t.PlayCount ?? 0) >= minRequired)
                .OrderByDescending(t => t.PlayCount)
                .ToList();

            // Fallback if no tracks meet the criteria
            if (!filtered.Any())
            {
                var fallbackCount = Math.Max(10, (int)Math.Ceiling(tracks.Count * 0.05));
                filtered = tracks
                    .Where(t => t.PlayCount.HasValue)
                    .OrderByDescending(t => t.PlayCount)
                    .Take(fallbackCount)
                    .ToList();
            }

            return filtered;
        }

        private List<Track> FilterLiked(List<Track> tracks)
        {
            return tracks.Where(t => t.Liked == true).ToList();
        }

        private List<Track> FilterAvoidDisliked(List<Track> tracks)
        {
            return tracks.Where(t => t.Disliked == false).ToList();
        }

        #endregion
    }
}
