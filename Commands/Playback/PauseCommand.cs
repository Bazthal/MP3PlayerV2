using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Pause" command for a player.
    /// </summary>
    /// <remarks>This command pauses the current operation in the given context. It responds with a
    /// confirmation message and invokes the pause action within the command context.</remarks>
    [Command(
        name: "pause",
        author: "Bazthal",
        version: "1.0.0",
        description: "Pauses playback; if already paused, resumes playback.",
        category: "Playback",
        example: "{ \"Command\": \"pause\" }"
        )]
    public class PauseCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>This method responds to the command with a success message and invokes the pause
        /// action within the provided context.</remarks>
        /// <param name="cmd">The player command to be executed.</param>
        /// <param name="ctx">The context in which the command is executed, providing methods for responding and invoking actions.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            ctx.Respond(cmd.Command, true, "Pause Command Called", null);
            ctx.Invoke(() => ctx.Pause());
            return true;
        }
    }
}
