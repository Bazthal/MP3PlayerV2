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
    [Command("count")]
    internal class CountComand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() <= 0)
            {
                ctx.Respond(false, "Playlist is empty", null);
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
                        ctx.CountByPlayData(mode);
                        return true;
                    default:
                        ctx.CountByName(cmd.Value);
                        return true;
                }

            }
            return false;

        }
    }
}
