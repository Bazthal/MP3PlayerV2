using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "count" command, which performs counting operations on a playlist based on the specified criteria.
    /// </summary>
    /// <remarks>This command can count the total number of items in a playlist or filter the count based on
    /// specific criteria such as unplayed items.</remarks>
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
                if (cmd.Value.Equals("unplayed", StringComparison.InvariantCultureIgnoreCase))
                {
                    ctx.CountUnplayed();
                    return true;
                }
                else
                {
                    ctx.CountByName(cmd.Value);
                }
            }

            return false;
        }
    }
}
