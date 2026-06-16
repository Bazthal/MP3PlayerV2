using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the 'highlight' command to select or highlight a track in the playlist by name, index, or at random.
    /// </summary>
    /// <remarks>If the command value is "random", a random track in the playlist is highlighted. If the value
    /// is a valid integer, the track at that index is selected. Otherwise, the command attempts to highlight a track by
    /// name. The command has no effect if the value is not set or the playlist is empty.</remarks>
    [Command(
        name: "highlight",
        author: "Bazthal",
        version: "1.0.0",
        description: "Highlights a track from the playlist by name, by index, or at random on the playlist UI",
        category: "Playlist",
        example: "{ \"Command\": \"highlight\" , \"Value\": \"Bohemian Rhapsody\" }"
        )]
    public class HighlightCommand : ICommandHandler
    {
        /// <summary>
        /// Processes a player command to select or highlight a track in the playlist based on the provided value.
        /// </summary>
        /// <remarks>If the command value is "random", a random track is highlighted. If the value is a
        /// valid integer, the track at that index is selected. Otherwise, the method attempts to select a track by
        /// name. The method returns false if the value is not set or the playlist is empty.</remarks>
        /// <param name="cmd">The player command containing the selection value and command identifier. The value may specify a track
        /// index, track name, or the keyword "random".</param>
        /// <param name="ctx">The command context used to access playlist state, perform selection actions, and send responses.</param>
        /// <returns>true if a track was successfully highlighted; otherwise, false.</returns>
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

            //Random selection may not be the best idea for highlighting, this was almost a full copy from the select command with the playback removed

            if (cmd.Value.Equals("random", StringComparison.InvariantCultureIgnoreCase))
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectRandomTrack();
                });
                ctx.Respond(cmd.Command, true, $"Random track highlighted: {ctx.GetSelectedTrackName()}", null);
                return true;
            }

            if (int.TryParse(cmd.Value, out int index))
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectTrackByIndex(index);
                });
                return true;
            }
            else
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectTrackByName(cmd.Value);
                    ctx.Respond(cmd.Command, true, $"Highlighted: {ctx.GetSelectedTrackName()}", null);

                });
                return true;
            }

        }
    }

}
