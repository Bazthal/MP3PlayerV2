using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    [Command("sort")]
    internal class SortCommand : ICommandHandler
    {
        /// <summary>
        /// Executes a sorting command on the playlist based on the specified field and order.
        /// </summary>
        /// <remarks>The method sorts the playlist by the specified field in either ascending or
        /// descending order. Valid fields are "artist", "title", "lastplayed", and "playcount". Valid orders are "up",
        /// "down", "ascending", and "descending". If the playlist is empty or the command parameters are invalid, the
        /// method responds with an error message and returns <see langword="false"/>.</remarks>
        /// <param name="cmd">The command containing the field to sort by and the order of sorting.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist and response methods.</param>
        /// <returns><see langword="true"/> if the playlist was successfully sorted; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() <= 1)
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(false, "Value not set; cancelling sorting", null);
                return false;
            }

            bool descending;

            if (string.IsNullOrWhiteSpace(cmd.Order))
            {
                ctx.Respond(false, "Order not set; cancelling sort", null);
                return false;
            }

            string order = cmd.Order.Trim().ToLowerInvariant();
            switch (order)
            {
                case "up":
                case "ascending":
                    descending = false;
                    break;
                case "down":
                case "descending":
                    descending = true;
                    break;
                default:
                    ctx.Respond(false, $"Invalid sort order '{cmd.Order}'. Order should be Up, Down, Ascending or Descending.", null);
                    return false;
            }

            string sortField = cmd.Value.ToLowerInvariant();
            Action sortAction = sortField switch
            {
                "artist" => () => ctx.SortPlaylist(t => t.Artist, descending),
                "title" => () => ctx.SortPlaylist(t => t.Title, descending),
                "lastplayed" => () => ctx.SortPlaylist(t => t.LastPlayed, descending),
                "playcount" => () => ctx.SortPlaylist(t => t.PlayCount, descending),
                _ => null
            };

            if (sortAction == null)
            {
                ctx.Respond(false, $"Invalid sort field '{cmd.Value}'", null);
                return false;
            }

            ctx.Invoke(sortAction);
            ctx.Respond(true, $"Playlist has been sorted by: {cmd.Value}, Descending: {descending}", null);
            return true;
        }
    }
}
