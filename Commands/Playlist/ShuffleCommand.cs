using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Represents a command that shuffles the current playlist.
    /// </summary>
    /// <remarks>This command is used to randomize the order of tracks in the playlist. It can only be
    /// executed if the playlist contains more than one track. If the playlist is empty or contains only one track, the
    /// shuffle operation will not be performed, and an appropriate response will be sent.</remarks>
    [Command(
        name: "shuffle",
        author: "Bazthal",
        version: "1.0.0",
        description: "Shuffles the current playlist by randomizing the order of its tracks.",
        category: "Playlist",
        example: "{ \"Command\": \"Shuffle\" }"
        )]
    internal class ShuffleCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command to shuffle the playlist.
        /// </summary>
        /// <remarks>This method checks the number of tracks in the playlist and shuffles it if there are
        /// more than one. It sends a response indicating the result of the operation.</remarks>
        /// <param name="cmd">The player command to be executed.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to the playlist and response mechanisms.</param>
        /// <returns><see langword="true"/> if the playlist was successfully shuffled; otherwise, <see langword="false"/> if the
        /// playlist is empty or contains only one track.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() <= 1)
            { ctx.Respond(cmd.Command, false, "Playlist is either empty or only has 1 track", null); return false; }

            ctx.Respond(cmd.Command, true, "Playlist has been shuffled", null);
            ctx.Invoke(ctx.Shuffle);
            return true;

        }
    }
}
