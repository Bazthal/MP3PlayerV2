using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Previous" command for navigating to the previous item in a playlist.
    /// </summary>
    /// <remarks>This command checks if the playlist is empty before attempting to navigate to the previous
    /// item. If the playlist is empty, it responds with a failure message. Otherwise, it invokes the previous item
    /// navigation and responds with a success message.</remarks>
    [Command("previous")]
    public class PreviousCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            ctx.Respond(true, "Previous Command Called", null);
            ctx.Invoke(() => ctx.Previous());
            return true;
        }
    }
}
