using MP3PlayerV2.Models;
namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the "Volume" command to get or set the volume level.
    /// </summary>
    /// <remarks>This command can be used to query the current volume level or to set a new volume level. If
    /// the command value is empty or whitespace, it returns the current volume level. If a valid integer is provided,
    /// it sets the volume to that level.</remarks>
    [Command(
        name: "volume",
        author: "Bazthal",
        version: "1.0.0",
        description: "Gets or sets the volume. If a value is provided, sets the volume to that value; if no value is provided, returns the current volume level.",
        category: "Playback",
        example: "{ \"Command\": \"volume\" , \"Value\": \"50\" }"
        )]
    public class VolumeCommand : ICommandHandler
    {
        /// <summary>
        /// Executes a volume-related command, either retrieving the current volume level or setting a new one.
        /// </summary>
        /// <remarks>If <paramref name="cmd"/> contains a null, empty, or whitespace value, the method
        /// responds with the current volume level. If the value can be parsed as an integer, the method sets the volume
        /// to the specified level and responds with confirmation. If the value cannot be parsed as an integer, the
        /// method responds with an error message.</remarks>
        /// <param name="cmd">The command to execute, containing the value to parse as the desired volume level.</param>
        /// <param name="ctx">The context in which the command is executed, providing methods for responding and managing volume.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(cmd.Command, true, $"Volume is currently: {ctx.GetVolumeLevel()}%", null);
                return true;
            }

            if (int.TryParse(cmd.Value, out int vol))
            {
                ctx.Respond(cmd.Command, true, $"Volume set to {vol}%", null);
                ctx.Invoke(() => ctx.SetVolume(vol));
                return true;
            }
            else
            {
                ctx.Respond(cmd.Command, false, $"Couldn't parse [{cmd.Value}] as a number", null);
                return false;

            }
        }
    }
}
