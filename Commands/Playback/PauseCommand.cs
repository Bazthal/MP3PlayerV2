using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Pause" command for a player.
    /// </summary>
    /// <remarks>This command pauses the current operation in the given context. It responds with a
    /// confirmation message and invokes the pause action within the command context.</remarks>
    [Command("pause")]
    public class PauseCommand : ICommandHandler
    {    
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            ctx.Respond(true, "Pause Command Called", null);
            ctx.Invoke(() => ctx.Pause());
            return true;
        }
    }
}
