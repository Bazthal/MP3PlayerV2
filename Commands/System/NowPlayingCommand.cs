
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
    [Command("nowplaying")]
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
                var tracks = ctx.GetPlaylistTracks();
                string name = ctx.GetCurrentTrack;

                foreach (var track in tracks)
                {
                    if (track.ToString() == name)
                    {
                        var dispTime = $"{track.LastPlayed?.ToLocalTime().ToShortDateString()}-{track.LastPlayed?.ToLocalTime().ToShortTimeString()}";
                        var nowPlayingInfo = new { NowPlaying = track.ToString(), track.PlayCount, track.LastPlayed, LocalTime = dispTime };
                        ctx.Respond(true, "Now Playing", nowPlayingInfo);
                    }
                }
                //  ctx.Respond(true, name, null);

                return true;
            }

            ctx.Respond(false, "Unknown playback state", null);
            return false;
        }
    }

}
