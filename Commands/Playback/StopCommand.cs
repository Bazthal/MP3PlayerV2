
using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Stop" command for a player.
    /// </summary>
    /// <remarks>This command stops the current operation within the provided context.</remarks>
    [Command("stop")]
    public class StopCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
                ctx.Respond(true, "Stop Command Called", null);
            ctx.Invoke(() => ctx.Stop());
            return true;
        }
    }
}
