using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Previous" command for navigating to the previous item in a playlist.
    /// </summary>
    /// <remarks>This command checks if the playlist is empty before attempting to navigate to the previous
    /// item. If the playlist is empty, it responds with a failure message. Otherwise, it invokes the previous item
    /// navigation and responds with a success message.</remarks>
    [Command(
        name: "previous",
        author: "Bazthal",
        version: "1.0.0",
        description: "Goes back to the beginning of the current track if it has played for more than 5 seconds; otherwise goes to the previous track",
        category: "Playback",
        example: "{ \"Command\": \"previous\" }"
        )]
    public class PreviousCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the "Previous" command for the player, navigating to the previous item in the playlist.
        /// </summary>
        /// <remarks>If the playlist is empty, the method responds with an error message and returns <see
        /// langword="false"/>. Otherwise, it invokes the "Previous" action in the context and returns <see
        /// langword="true"/>.</remarks>
        /// <param name="cmd">The command to be executed. This parameter is not used directly in this method.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist and response handling.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                return false;
            }

            ctx.Respond(cmd.Command, true, "Previous Command Called", null);
            ctx.Invoke(() => ctx.Previous());
            return true;
        }
    }
}
