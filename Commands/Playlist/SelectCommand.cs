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
    [Command("select")]
    public class SelectCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(false, "Value not set; cancelling selection", null);
                return false;
            }

            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            if (cmd.Value.Equals("random", StringComparison.InvariantCultureIgnoreCase))
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectRandomTrack();
                    ctx.Play();
                });
                ctx.Respond(true, $"Random track selected: {ctx.GetSelectedTrackName()}", null);
                return true;
            }

            if (int.TryParse(cmd.Value, out int index))
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectTrackByIndex(index);
                    ctx.EnsureTrackVisible();
                    ctx.Play();
                });
            }
            else
            {
                ctx.Invoke(() =>
                {
                    ctx.SelectTrackByName(cmd.Value);
                    ctx.Play();
                });
            }

            ctx.Respond(true, $"Selected: {ctx.GetSelectedTrackName()}", null);
            return true;
        }
    }

}
