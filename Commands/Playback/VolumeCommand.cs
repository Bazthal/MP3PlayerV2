using MP3PlayerV2.Models;
namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the "Volume" command to get or set the volume level.
    /// </summary>
    /// <remarks>This command can be used to query the current volume level or to set a new volume level. If
    /// the command value is empty or whitespace, it returns the current volume level. If a valid integer is provided,
    /// it sets the volume to that level.</remarks>
    [Command("volume")]
    public class VolumeCommand : ICommandHandler
    {
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(true, $"Volume is currently: {ctx.GetVolumeLevel}%", null);
                return true;
            }

            if (int.TryParse(cmd.Value, out int vol))
            {
                ctx.Respond(true, $"Volume set to {vol}%", null);
                ctx.Invoke(() => ctx.Volume(vol));
                return true;
            }
            else
            {
                ctx.Respond(false, $"Couldn't parse [{cmd.Value}] as a number", null);
                return false;

            }
        }
    }
}
