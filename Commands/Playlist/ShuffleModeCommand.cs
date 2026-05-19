
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the 'shufflemode' command to display or change the current shuffle mode for the playlist.
    /// </summary>
    /// <remarks>This command supports multiple operations based on the provided value: omitting the value or
    /// specifying "get" returns the current shuffle mode; specifying "list" returns all available shuffle modes;
    /// providing a mode name sets the shuffle mode to the specified value if it exists. The command responds with the
    /// result of the operation, including success or failure messages. This class is typically used within a command
    /// handling framework for playlist management.</remarks>
    [Command(
    name: "shufflemode",
    author: "Bazthal",
    version: "1.0.0",
    description: "Show or change the shuffle mode. No value (or \"get\") returns the current mode; \"list\" returns all available modes; providing a mode name sets that mode.",
    category: "Playlist",
    example: "{ \"Command\": \"shufflemode\" , \"Value\": \"get\" }"
    )]
    public class ShuffleModeCommand : ICommandHandler
    {
        /// <summary>
        /// Executes a shuffle mode command based on the specified player command and command context.
        /// </summary>
        /// <remarks>If the command value is 'get' or empty, the current shuffle mode is returned. If the
        /// value is 'list', all available shuffle modes are listed. Otherwise, the method attempts to set the shuffle
        /// mode to the specified value. If the value does not match any available shuffle mode, the method returns
        /// false.</remarks>
        /// <param name="cmd">The player command to execute. The command's value determines the shuffle mode operation to perform. Cannot
        /// be null.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to shuffle mode operations and response
        /// handling. Cannot be null.</param>
        /// <returns>true if the command was recognized and executed successfully; otherwise, false.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value) || cmd.Value.Equals("get", StringComparison.InvariantCultureIgnoreCase))
            {
                var curMode = ctx.GetSelectedShuffleMode();
                ctx.Respond(cmd.Command, true, $"Shuffle mode is currently set to {curMode}", null);
                return true;
            }

            string mode = cmd.Value.ToLowerInvariant();
            switch (mode)
            {
                case "list":
                    List<string> shuffleModes = new();
                    for (int i = 0; i < ctx.GetShuffleModeCount(); i++ )
                    {
                        string name = ctx.GetShuffleModeNameAt(i);
                        shuffleModes.Add(name);
                    }
                    ctx.Respond(cmd.Command, true, "List of shuffle modes", shuffleModes);
                    return true;
                default:
                    for (int i = 0; i < ctx.GetShuffleModeCount(); i++)
                    {
                        string name = ctx.GetShuffleModeNameAt(i);
                        if (name.Contains(cmd.Value, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ctx.SetShuffleMode(name);
                            ctx.Respond(cmd.Command, true, $"Shuffle mode has been set to: {name}", null);
                            return true;
                        }
                    }
                    break;
            }
            ctx.Respond(cmd.Command, false, $"ShuffleMode not found {cmd.Value}", null);
            return false;
        }
    }
}