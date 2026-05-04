using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "select" command, allowing a user to select a track from a playlist by name or index, or to select a
    /// random track.
    /// </summary>
    /// <remarks>This command can be used to select a specific track by providing its name or index, or to
    /// select a random track by specifying "random" as the value. The command will respond with a success or failure
    /// message based on the outcome of the selection.</remarks>
    [Command(
        name: "select",
        author: "Bazthal",
        version: "1.0.0",
        description: "Selects a track from the playlist by name, by index, or at random, then starts playback.",
        category: "Playlist",
        example: "{ \"Command\": \"select\" , \"Value\": \"Bohemian Rhapsody\" }"
        )]
    public class SelectCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>This method processes the command based on the value provided in <paramref
        /// name="cmd"/>. If the value is "random", a random track is selected and played. If the value is a valid
        /// integer, the track at the specified index is selected and played. Otherwise, the method attempts to select
        /// and play a track by name. <para> The method returns <see langword="false"/> if the command value is null,
        /// empty, or whitespace, or if the playlist is empty. In these cases, an appropriate response is sent via the
        /// <paramref name="ctx"/>. </para></remarks>
        /// <param name="cmd">The command to execute, containing the value that determines the action to perform.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to playlist operations and responses.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(cmd.Command, false, "Value not set; cancelling selection", null);
                return false;
            }

            if (ctx.IsPlaylistEmpty())
            {
                ctx.Respond(cmd.Command, false, "Playlist is empty", null);
                return false;
            }

            if (cmd.Value.Equals("random", StringComparison.InvariantCultureIgnoreCase))
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectRandomTrack();
                    ctx.Play(null);
                });
                //ctx.Respond(cmd.Command, true, $"Random track selected: {ctx.GetSelectedTrackName()}", null);
                return true;
            }

            if (int.TryParse(cmd.Value, out int index))
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectTrackByIndex(index);
                    ctx.Play(null);
                });
            }
            else
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectTrackByName(cmd.Value);
                    ctx.Play(null);
                });
            }

//            ctx.Respond(cmd.Command, true, $"Selected: {ctx.GetSelectedTrackName()}", null);
            return true;
        }
    }

}
