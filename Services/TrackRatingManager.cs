using MP3PlayerV2.Models;


namespace MP3PlayerV2.Services
{
    public static class TrackRatingManager
    {
        /// <summary>
        /// Applies logic for when a track starts playing, incrementing play count and updating the star rating.
        /// </summary>
        /// <param name="track">The track being played.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        public static void ApplyPlayStart(Track track, TrackRatingSettings settings)
        {
            track.PlayCount++;
            track.LastUpdated = DateTime.Now;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Applies logic for when a track is played to completion, rewarding and updating the star rating.
        /// </summary>
        /// <param name="track">The track played to completion.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        public static void ApplyPlayCompleted(Track track, TrackRatingSettings settings)
        {
            track.PlayCompleteCount++;
            track.RatingScore += settings.NoSkipReward;
            track.LastUpdated = DateTime.Now;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Applies logic for when a track is replayed, rewarding and updating the star rating.
        /// </summary>
        /// <param name="track">The track being replayed.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        public static void ApplyReplay(Track track, TrackRatingSettings settings)
        {
            track.RatingScore += settings.ReplayReward;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Applies logic for when a track is skipped, penalizing and updating the star rating based on skip timing.
        /// </summary>
        /// <param name="track">The track being skipped.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        /// <param name="secondsPlayed">The number of seconds played before skipping.</param>
        public static void ApplySkip(Track track, TrackRatingSettings settings, int secondsPlayed)
        {
            track.SkipCount++;
            if (track.DurationSeconds is null || track.DurationSeconds <= 0) return;

            int totalDuration = track.DurationSeconds.Value;

            // Lead-in immunity
            if (secondsPlayed <= settings.LeadInImmunitySeconds)
                return;

            // Lead-out immunity
            if (secondsPlayed >= totalDuration - settings.LeadOutImmunitySeconds)
                return;

            double progress = (double)secondsPlayed / totalDuration;
            if (progress < settings.EarlySkipThresholdPercent)
                track.RatingScore -= settings.EarlySkipPenalty;
            else
                track.RatingScore -= settings.MidSkipPenalty;

            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Applies logic for when a track is seeked to the end, penalizing and updating the star rating.
        /// </summary>
        /// <param name="track">The track being seeked to the end.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        public static void ApplySeekToEnd(Track track, TrackRatingSettings settings)
        {
            track.RatingScore -= settings.SeekToEndPenalty;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Marks the specified track as liked and adjusts its rating score based on the provided settings.
        /// </summary>
        /// <remarks>This method updates the track's <see cref="Track.RatingScore"/> by adding the value
        /// of <see cref="TrackRatingSettings.ManualLikeBoost"/>. It also sets the <see cref="Track.Liked"/> property to
        /// <see langword="true"/> and the <see cref="Track.Disliked"/> property to <see langword="false"/>.
        /// Additionally, the star rating of the track is updated using the provided settings.</remarks>
        /// <param name="track">The track to be marked as liked. Must not be null.</param>
        /// <param name="settings">The settings that define the boost to apply to the track's rating score. Must not be null.</param>
        public static void ApplyManualLike(Track track, TrackRatingSettings settings)
        {
            if (track.Liked) return;


            if (track.Disliked)
                track.RatingScore += settings.ManualDislikePenalty;

            track.RatingScore += settings.ManualLikeBoost;
            track.Liked = true;
            track.Disliked = false;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Applies a manual dislike to the specified track, adjusting its rating score and updating its state.
        /// </summary>
        /// <remarks>This method decreases the track's rating score by the penalty specified in <paramref
        /// name="settings"/>  and marks the track as disliked. If the track was previously liked, its liked state is
        /// cleared. Additionally, the star rating of the track is updated based on the new rating score.</remarks>
        /// <param name="track">The track to which the manual dislike will be applied. Cannot be null.</param>
        /// <param name="settings">The settings that define the penalty applied to the track's rating score. Cannot be null.</param>
        public static void ApplyManualDislike(Track track, TrackRatingSettings settings)
        {
            if (track.Disliked) return;

            if (track.Liked)
                track.RatingScore -= settings.ManualLikeBoost;

            track.RatingScore -= settings.ManualDislikePenalty;
            track.Disliked = true;
            track.Liked = false;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Resets the like and dislike status of a track and adjusts its rating score accordingly.
        /// </summary>
        /// <remarks>If the track was previously liked, the rating score is decreased by the value of 
        /// <see cref="TrackRatingSettings.ManualLikeBoost"/>. If the track was previously disliked, the rating  score
        /// is increased by the value of <see cref="TrackRatingSettings.ManualDislikePenalty"/>. After resetting the
        /// like and dislike status, the track's star rating is updated using the provided settings.</remarks>
        /// <param name="track">The track whose like and dislike status will be reset. Cannot be null.</param>
        /// <param name="settings">The settings used to adjust the track's rating score based on its previous like or dislike status. Cannot be
        /// null.</param>
        public static void ApplyNeutral(Track track, TrackRatingSettings settings)
        {
            if (track.Liked)
                track.RatingScore -= settings.ManualLikeBoost;
            else if (track.Disliked)
                track.RatingScore += settings.ManualDislikePenalty;

            track.Liked = false;
            track.Disliked = false;
            UpdateStarRating(track, settings);
        }

        /// <summary>
        /// Applies monthly decay to the track's rating score if 30 days have passed since the last decay.
        /// </summary>
        /// <param name="track">The track to apply decay to.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        public static void ApplyMonthlyDecay(Track track, TrackRatingSettings settings)
        {
            var now = DateTime.Now;
            if (track.LastDecayApplied == null ||
                (now - track.LastDecayApplied.Value).TotalDays >= 30)
            {
                track.RatingScore = (int)(track.RatingScore * (1 - settings.MonthlyDecayPercent));
                track.LastDecayApplied = now;
                UpdateStarRating(track, settings);
            }
        }

        /// <summary>
        /// Updates the star rating of a track based on its current rating score and the provided settings.
        /// </summary>
        /// <param name="track">The track to update.</param>
        /// <param name="settings">The rating settings to use for calculation.</param>
        private static void UpdateStarRating(Track track, TrackRatingSettings settings)
        {
            if (track.RatingScore >= settings.FiveStarMinScore)
                track.StarRating = 5;
            else if (track.RatingScore >= settings.FourStarMinScore)
                track.StarRating = 4;
            else if (track.RatingScore >= settings.ThreeStarMinScore)
                track.StarRating = 3;
            else if (track.RatingScore >= settings.TwoStarMinScore)
                track.StarRating = 2;
            else
                track.StarRating = 1;

        }
    }

}
