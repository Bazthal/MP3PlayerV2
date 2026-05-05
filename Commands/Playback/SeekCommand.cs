using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playback
{
    /// <summary>
    /// Handles the "Seek" command to get or set the playback position.
    /// </summary>
    /// <remarks>This command can be used to query the current playback position or to seek to a specific position in seconds. 
    /// If the command value is empty or whitespace, it returns the current position and duration. 
    /// If a valid number is provided, it seeks to that position.</remarks>
    [Command(
        name: "seek",
        author: "Bazthal",
        version: "1.0.0",
        description: "Gets or sets the playback position in seconds. If a value is provided, seeks to that position; if no value is provided, returns the current position and duration.",
        category: "Playback",
        example: "{ \"Command\": \"seek\" , \"Value\": \"30\" }"
        )]
    public class SeekCommand : ICommandHandler
    {
        /// <summary>
        /// Executes a seek-related command, either retrieving the current position or seeking to a new one.
        /// </summary>
        /// <remarks>If <paramref name="cmd"/> contains a null, empty, or whitespace value, the method
        /// responds with the current playback position and duration. If the value can be parsed as a number, the method seeks
        /// to the specified position and responds with confirmation. If the value cannot be parsed as a number, the
        /// method responds with an error message.</remarks>
        /// <param name="cmd">The command to execute, containing the value to parse as the desired position in seconds.</param>
        /// <param name="ctx">The context in which the command is executed, providing methods for responding and managing playback position.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value))
            {
                var position = ctx.GetPosition();
                var duration = ctx.GetDuration();
                ctx.Respond(cmd.Command, true, 
                    $"Position: {FormatTime(position)} / {FormatTime(duration)} ({position:F1}s / {duration:F1}s)", null);
                return true;
            }

            if (double.TryParse(cmd.Value, out double seconds))
            {
                var duration = ctx.GetDuration();

                if (seconds < 0)
                {
                    ctx.Respond(cmd.Command, false, "Position cannot be negative", null);
                    return false;
                }

                if (seconds > duration)
                {
                    ctx.Respond(cmd.Command, false, 
                        $"Position {seconds:F1}s exceeds track duration {duration:F1}s", null);
                    return false;
                }

                ctx.Invoke(() => ctx.SeekTo(seconds));
                ctx.Respond(cmd.Command, true, $"Seeked to {FormatTime(seconds)} ({seconds:F1}s)", null);
                return true;
            }
            else
            {
                ctx.Respond(cmd.Command, false, $"Couldn't parse [{cmd.Value}] as a number", null);
                return false;
            }
        }

        /// <summary>
        /// Formats a time in seconds to MM:SS format.
        /// </summary>
        private static string FormatTime(double totalSeconds)
        {
            int minutes = (int)(totalSeconds / 60);
            int seconds = (int)(totalSeconds % 60);
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
