using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Represents a command that sorts a playlist based on a specified field and order.
    /// </summary>
    /// <remarks>The <see cref="SortCommand"/> allows sorting a playlist by various fields such as "artist",
    /// "title",  "lastplayed", "playcount", and others. The sorting can be performed in ascending or descending order, 
    /// with valid order values being "up", "down", "ascending", and "descending". <para> If the playlist contains fewer
    /// than two tracks, or if the provided field or order is invalid, the  command will not perform any sorting and
    /// will return an error response. </para></remarks>
    [Command(
        name: "sort",
        author: "Bazthal",
        version: "1.0.0",
        description: "Sorts the playlist by fields such as artist, album, title, play count, last played, rating, liked, or disliked, in ascending or descending order.",
        category: "Playlist",
        example: "{ \"Command\": \"Sort\", \"Value\": \"Artist\", \"Order\": \"ascending\" }"
        )]
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
                ctx.Respond(cmd.Command, false, "Playlist is either empty or only has 1 track", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(cmd.Command, false, "Value not set; cancelling sorting", null);
                return false;
            }

            bool descending;

            if (string.IsNullOrWhiteSpace(cmd.Order))
            {
                ctx.Respond(cmd.Command, false, "Order not set; cancelling sort", null);
                return false;
            }

            string order = cmd.Order.Trim().ToLowerInvariant();
            switch (order)
            {
                case "up":
                case "ascending":
                case "asc":
                    descending = false;
                    break;
                case "down":
                case "descending":
                case "desc":
                    descending = true;
                    break;
                default:
                    ctx.Respond(cmd.Command, false, $"Invalid sort order '{cmd.Order}'. Order should be Up, Down, Ascending or Descending.", null);
                    return false;
            }

            string sortField = cmd.Value.ToLowerInvariant();

            // Validate sort field
            var validFields = new[] { "artist", "title", "album", "playcount", "lastplayed", "liked", "disliked", "rating" };
            if (!validFields.Contains(sortField))
            {
                ctx.Respond(cmd.Command, false, $"Invalid sort field '{cmd.Value}'. Valid fields are: {string.Join(", ", validFields)}", null);
                return false;
            }

            ctx.Invoke(() => ctx.SortPlaylist(sortField, descending));
            ctx.Respond(cmd.Command, true, $"Playlist has been sorted by: {cmd.Value}, Descending: {descending}", null);
            return true;
        }
    }
}
