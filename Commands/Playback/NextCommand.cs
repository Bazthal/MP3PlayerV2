using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the "Next" command to advance to the next item in the playlist.
    /// </summary>
    /// <remarks>This command checks if the playlist is empty before attempting to advance.  If the playlist
    /// is empty, it responds with an appropriate message and does not proceed. Otherwise, it increments the skip
    /// counter and advances to the next item.</remarks>
    [Command("next")]
    public class NextCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            ctx.Respond(true, "Next Command Called", null);
            ctx.Invoke(() => ctx.Next(false));
            return true;
        }
    }
}
