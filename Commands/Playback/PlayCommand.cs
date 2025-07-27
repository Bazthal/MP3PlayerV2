using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the execution of the "Play" command within a given context.
    /// </summary>
    /// <remarks>This command checks if there are any playlists available in the context before attempting to
    /// play. If no playlists are available, it responds with an appropriate message and does not proceed with
    /// playback.</remarks>
    [Command("play")]
    public class PlayCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (ctx.GetPlaylistCount() == 0)
            {
                ctx.Respond(false, "Playlist is empty", null);
                return false;
            }

            ctx.Respond(true, "Play Command Called", null);
            ctx.Invoke(() => ctx.Play());
            return true;
        }
    }
}
