using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the "Next" command to advance to the next item in the playlist.
    /// </summary>
    /// <remarks>This command checks if the playlist is empty before attempting to advance.  If the playlist
    /// is empty, it responds with an appropriate message and does not proceed. Otherwise, it increments the skip
    /// counter and advances to the next item.</remarks>
    [Command(
        name: "next",
        author: "Bazthal",
        version: "1.0.0",
        description: "Skip the current track and play the next track in the playlist.",
        category: "Playback",
        example: "{ \"Command\": \"next\" }"
        )]
    public class NextCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>If the playlist is empty, the method responds with an appropriate message and returns
        /// <see langword="false"/>. Otherwise, it triggers the next command in the context and returns <see
        /// langword="true"/>.</remarks>
        /// <param name="cmd">The player command to execute.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist and response handling.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                return false;
            }

            ctx.Respond(cmd.Command, true, "Next Command Called", null);
            ctx.Invoke(() => ctx.Next(false));
            return true;
        }
    }
}
