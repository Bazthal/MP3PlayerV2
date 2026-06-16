using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "count" command, which provides information about the playlist or specific items within it.
    /// </summary>
    /// <remarks>This command processes the "count" operation based on the provided value in the <see
    /// cref="PlayerCommand" />. If the playlist is empty, the command responds with an appropriate message and does not
    /// perform further actions. Supported values for the command include: <list type="bullet">
    /// <item><term>"unplayed"</term>: Counts items in the playlist that have not been played.</item>
    /// <item><term>"liked"</term>: Counts items in the playlist that are marked as liked.</item>
    /// <item><term>"disliked"</term>: Counts items in the playlist that are marked as disliked.</item>
    /// <item><term>Other values</term>: Counts items in the playlist by name.</item> </list></remarks>
    [Command(
        name: "count",
        author: "Bazthal",
        version: "1.0.0",
        description: "Counts the number of occurrences of the search terms in the playlist",
        category: "Playlist",
        example: "{ \"Command\": \"Count\", \"Value\": \"Bohemian Rhapsody\" }"
        )]
    internal class CountComand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>The method processes the command based on the provided value in <paramref
        /// name="cmd"/>. If the playlist is empty,  the method responds with an error message and returns <see
        /// langword="false"/>. If the command value matches a  recognized mode (e.g., "unplayed", "liked", "disliked",
        /// or "neutral"), the corresponding play data is counted.  Otherwise, the method attempts to count by name
        /// using the provided value.</remarks>
        /// <param name="cmd">The player command to execute, which may include a value indicating the mode or filter to apply.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to playlist data and response handling.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() <= 0)
            {
                ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(cmd.Value))
            {
                var mode = cmd.Value.ToLowerInvariant();
                switch (mode)
                {
                    case "unplayed":
                    case "liked":
                    case "disliked":
                    case "neutral":
                        {
                            var state = ctx.GetApplicationStateService();
                            var matches = state.TrackSearch.FindTracksByPlayData(state.PlaylistManager.Tracks, mode);
                            bool includeData = state.Settings.CommandBehaviour.IncludeTracksInCount;
                            ctx.Respond(cmd.Command, true, $"{matches.Count} {mode} track(s) found in the playlist", includeData ? matches : null);
                            return true;
                        }
                    default:
                        {
                            var state = ctx.GetApplicationStateService();
                            var matches = state.TrackSearch.FindTracksByName(state.PlaylistManager.Tracks, mode);
                            bool includeData = state.Settings.CommandBehaviour.IncludeTracksInCount;
                            if (matches.Count > 0)
                            {
                                ctx.Respond(cmd.Command, true, $"{matches.Count} matching item(s) found in the playlist for: {mode}", includeData ? matches : null);
                            }
                            else
                            {
                                ctx.Respond(cmd.Command, false, "No matching item found in the playlist", null);
                            }
                            return true;
                        }
                }

            }
            return false;

        }
    }
}
