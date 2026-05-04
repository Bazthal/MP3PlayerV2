using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Stop" command for a player.
    /// </summary>
    /// <remarks>This command stops the current operation within the provided context.</remarks>
    [Command(
        name: "stop",
        author: "Bazthal",
        version: "1.0.0",
        description: "Stops playback of the currently active track.",
        category: "Playback",
        example: "{ \"Command\": \"stop\" }"
        )]
    public class StopCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <param name="cmd">The player command to be executed.</param>
        /// <param name="ctx">The context in which the command is executed, providing methods for responding and invoking actions.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            ctx.Respond(cmd.Command, true, "Stop Command Called", null);
            ctx.Invoke(() => ctx.Stop());
            return true;
        }
    }
}
