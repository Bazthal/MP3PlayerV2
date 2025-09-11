using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.System
{
    /// <summary>
    /// Handles the <c>ResetStat</c> command, allowing the user to reset specific playback statistics
    /// (such as play count, last played date, or skip count) for a defined playlist range.
    /// </summary>
    /// <remarks>
    /// This command validates the provided playlist context, ensures a target statistic is specified,
    /// and confirms that a valid range or order parameter is supplied before performing the reset.
    /// </remarks>
    [Command("ResetStat")]
    public class ResetStatsCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the ResetStat command to clear a specified statistic for a given playlist range.
        /// </summary>
        /// <param name="cmd">
        /// The command data, including the statistic name (<c>playcount</c>, <c>lastplayed</c>, 
        /// <c>skipcount</c>, or <c>all</c>) and the range/order to target.
        /// </param>
        /// <param name="ctx">
        /// The command context, providing playlist state, reset logic, and response handling.
        /// </param>
        /// <returns>
        /// <c>true</c> if the statistic was successfully reset; otherwise, <c>false</c>.
        /// </returns>

        private readonly AppSettings _appSettings;
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.IsPlaylistEmpty())
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(false, "No Stat selected for reset", null);
                return false;
            }

            //Temporally use both Range and Order to allow adoption to new property
            if (string.IsNullOrWhiteSpace(cmd.Range) && string.IsNullOrWhiteSpace(cmd.Order))
            {
                ctx.Respond(false, "Range is not set", null);
                return false;
            }

            string stat = cmd.Value.Trim().ToLowerInvariant();
            string range = string.Empty;

            if (!string.IsNullOrWhiteSpace(cmd.Range)) { range = cmd.Range.Trim().ToLowerInvariant(); }
            else if (!string.IsNullOrWhiteSpace(cmd.Order)) { range = cmd.Order.Trim().ToLowerInvariant(); }

            bool validStat = false;
            switch (stat)
            {
                case "playcount":
                case "lastplayed":
                case "skipcount":
                case "liked":
                case "disliked":
                case "rating":
                    validStat = true;
                    break;
                case "all":
                    validStat = _appSettings.CommandBehaviour.AllowRemoteDatabaseWipe;
                    break;
                default:
                    ctx.Respond(false, "Unknown stat set for reset", cmd.Value);
                    validStat = false;
                    return false;
            }


            if (validStat && !string.IsNullOrWhiteSpace(range))
            {
                ctx.ResetStat(range, stat);
                ctx.Respond(true, $"{stat} stat has been reset for range: {range}", null);
                return true;
            }

            ctx.Respond(false, "Unable to reset stats", null);
            return false;
        }
    }
}
