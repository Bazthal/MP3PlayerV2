using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "PlaylistMode" command, allowing users to query or set the current playlist mode.
    /// </summary>
    /// <remarks>This command checks the available playlist modes and sets the mode based on the provided
    /// command value. If no value is provided, it returns the current playlist mode. If the specified mode is not
    /// found, it responds with an appropriate message.</remarks>
    [Command("playlistmode")]
    public class PlaylistModeCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            int count = ctx.GetPlaylistModeCount();
            if (count == 0)
            {
                ctx.Respond(false, "No playlist modes available", null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                string curMode = ctx.GetSelectedPlaylistMode();
                ctx.Respond(false, $"Current Playlist mode is set to {curMode}", null);
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                string name = ctx.GetPlaylistModeNameAt(i);
                if (name.Contains(cmd.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    ctx.SetPlaylistModeIndex(i);
                    ctx.Respond(true, $"Mode set to {ctx.GetSelectedPlaylistMode()}", null);
                    return true;
                }
            }

            ctx.Respond(false, $"Option not found: {cmd.Value}", null);
            return false;
        }
    }

}
