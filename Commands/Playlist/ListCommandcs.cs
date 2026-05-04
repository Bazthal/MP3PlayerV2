using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "list" command, which filters and displays tracks from the playlist based on the specified criteria.
    /// </summary>
    /// <remarks>This command supports filtering tracks by predefined modes such as "unplayed", "liked", or
    /// "disliked",  or by a custom search value. If the playlist is empty or the command value is invalid, the
    /// operation is canceled.</remarks>
    [Command(
        name: "list",
        author: "Bazthal",
        version: "1.0.0",
        description: "List all playlist items that match the provided search terms.",
        category: "Playlist",
        example: "{ \"Command\": \"List\", \"Value\": \"Unplayed\" }"
        )]
    public class ListCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>This method processes a filter command to identify tracks in the playlist that match
        /// the specified criteria. Supported filter modes include: <list type="bullet">
        /// <item><description><c>unplayed</c>: Tracks that have not been played.</description></item>
        /// <item><description><c>liked</c>: Tracks marked as liked.</description></item>
        /// <item><description><c>disliked</c>: Tracks marked as disliked.</description></item>
        /// <item><description><c>neutral</c>: Tracks that are neither liked nor disliked.</description></item>
        /// <item><description>A custom search string, which is matched against track names using a case-insensitive
        /// regular expression.</description></item> </list> If the playlist is empty or the filter value is not
        /// provided, the method returns <see langword="false"/> and sends an appropriate response.</remarks>
        /// <param name="cmd">The command to execute, including the filter value to apply.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist and response handling.</param>
        /// <returns><see langword="true"/> if the command executes successfully and a response is sent; otherwise, <see
        /// langword="false"/> if the command is invalid or cannot be processed.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.IsPlaylistEmpty())
            {
                ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(cmd.Command, false, "Value not set; cancelling listing", null);
                return false;
            }

            var allTracks = ctx.GetPlaylistTracks();
            var matches = new List<Track>();

            var mode = cmd.Value.ToLowerInvariant();

            switch (mode)
            {
                case "unplayed":
                    matches = allTracks.Where(t => (t.PlayCount ?? 0) == 0).ToList();
                    break;
                case "liked":
                    matches = allTracks.Where(t => t.Liked == true).ToList();
                    break;
                case "disliked":
                    matches = allTracks.Where(t => t.Disliked == true).ToList();
                    break;
                case "neutral":
                    matches = allTracks.Where(t => t.Disliked == false && t.Liked == false).ToList();
                    break;
                default:
                    var regex = ctx.BuildSearchRegex(cmd.Value);

                    foreach (var track in allTracks)
                    {
                        string trackName = ctx.NormalizeText(track.ToString());
                        if (regex.IsMatch(trackName))
                            matches.Add(track);
                    }
                    break;
            }

            ctx.Respond(cmd.Command, true, $"{matches.Count} Match(es) found for filter {cmd.Value}", matches);
            return true;
        }
    }
}

