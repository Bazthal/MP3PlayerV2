using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Play" command within a given context.
    /// </summary>
    /// <remarks>This command checks if there are any playlists available in the context before attempting to
    /// play. If no playlists are available, it responds with an appropriate message and does not proceed with
    /// playback.</remarks>
    [Command(
        name: "play",
        author: "Bazthal",
        version: "1.0.0",
        description: "Starts playback of the currently selected track in the playlist",
        category: "Playback",
        example: "{ \"Command\": \"play\" }"
        )]
    public class PlayCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.`
        /// </summary>
        /// <remarks>This method checks if the playlist in the provided context is empty before executing
        /// the play command. If the playlist is empty, the method responds with an error message and returns <see
        /// langword="false"/>. Otherwise, it responds with a success message, invokes the play operation, and returns
        /// <see langword="true"/>.</remarks>
        /// <param name="cmd">The player command to execute. This parameter is currently unused but reserved for future extensions.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist and playback operations.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                return false;
            }

            ctx.Respond(cmd.Command, true, "Play Command Called", null);
            ctx.Invoke(() => ctx.Play(null));
            return true;
        }
    }
}
