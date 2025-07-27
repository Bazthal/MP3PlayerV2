
using CSCore.SoundOut;
using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.System
{
    /// <summary>
    /// Handles the "NowPlaying" command to provide information about the currently playing track.
    /// </summary>
    /// <remarks>This command checks the current playback state and responds with the name of the track if it
    /// is playing. If the playback is stopped or paused, it responds with a message indicating that nothing is
    /// currently playing.</remarks>
    [Command("mowplaying")]
    public class NowPlayingCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            var state = ctx.GetPlaybackState();

            if (state == PlaybackState.Stopped || state == PlaybackState.Paused)
            {
                ctx.Respond(false, "Not currently playing", null);
                return false;
            }

            if (state == PlaybackState.Playing)
            {
                int index = ctx.GetCurrentTrackIndex();
                string name = ctx.GetCurrentTrack;
                ctx.Respond(true, name, null);
                return true;
            }

            ctx.Respond(false, "Unknown playback state", null);
            return false;
        }
    }

}
