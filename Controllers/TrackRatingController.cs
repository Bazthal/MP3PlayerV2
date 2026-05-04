using BazthalLib;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using System;

namespace MP3PlayerV2.Controllers
{
    /// <summary>
    /// Coordinates track rating operations.
    /// </summary>
    public class TrackRatingController
    {
        private readonly PlaylistManager _playlistManager;
        private readonly AppSettings _settings;

        public TrackRatingController(PlaylistManager playlistManager, AppSettings settings)
        {
            _playlistManager = playlistManager ?? throw new ArgumentNullException(nameof(playlistManager));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Rates a track with the specified mode.
        /// </summary>
        /// <param name="mode">Rating mode: "like", "dislike", or "neutral"</param>
        /// <param name="track">The track to rate (optional if index provided)</param>
        /// <param name="index">The playlist index of the track (optional if track provided)</param>
        /// <returns>True if the rating was changed; otherwise false</returns>
        public bool RateTrack(string mode, Track? track = null, int index = -1)
        {
            track ??= _playlistManager.Get(index);

            if (track == null)
                return false;

            bool changed = false;
            switch (mode?.ToLowerInvariant())
            {
                case "like":
                    if (!track.Liked)
                    {
                        TrackRatingManager.ApplyManualLike(track, _settings.TrackRating);
                        changed = true;
                    }
                    break;
                case "dislike":
                    if (!track.Disliked)
                    {
                        TrackRatingManager.ApplyManualDislike(track, _settings.TrackRating);
                        changed = true;
                    }
                    break;
                case "neutral":
                    TrackRatingManager.ApplyNeutral(track, _settings.TrackRating);
                    changed = true;
                    break;
                default:
                    return false;
            }

            if (changed)
            {
                try
                {
                    TrackDatabase.SaveStats(track);
                }
                catch (Exception ex)
                {
                    DebugUtils.Log("TrackRatingController", "RateTrack",
                        $"Error saving stats: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
                }
            }

            return changed;
        }

        /// <summary>
        /// Determines if a skip is valid for rating purposes.
        /// </summary>
        public static bool IsSkipValid(TimeSpan position, TimeSpan totalDuration, int leadInImmunity = 2, int leadOutImmunity = 10)
        {
            var remaining = totalDuration - position;

            if (position < TimeSpan.FromSeconds(leadInImmunity))
                return false;
            if (remaining <= TimeSpan.FromSeconds(leadOutImmunity))
                return false;

            return true;
        }
    }
}
