using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Playlist
{
    /// <summary>
    /// Handles the "PlaylistMode" command, allowing users to query or set the current playlist mode.
    /// </summary>
    /// <remarks>This command checks the available playlist modes and sets the mode based on the provided
    /// command value. If no value is provided, it returns the current playlist mode. If the specified mode is not
    /// found, it responds with an appropriate message.</remarks>
    [Command(
        name: "playlistmode",
        author: "Bazthal",
        version: "1.0.0",
        description: "Show or change the playlist mode. No value (or \"get\") returns the current mode; \"list\" returns all available modes; providing a mode name sets that mode.",
        category: "Playlist",
        example: "{ \"Command\": \"playlistmode\" , \"Value\": \"Sequential\" }"
        )]
    public class PlaylistModeCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command to manage or query the playlist mode.
        /// </summary>
        /// <remarks>This method supports the following commands: <list type="bullet"> <item>
        /// <description> If the command value is <c>null</c>, whitespace, or "get" (case-insensitive), the current
        /// playlist mode is retrieved and returned in the response. </description> </item> <item> <description> If the
        /// command value is "list" (case-insensitive), a list of all available playlist modes is returned in the
        /// response. </description> </item> <item> <description> For any other value, the method attempts to match the
        /// value to an available playlist mode. If a match is found, the playlist mode is updated, and a confirmation
        /// is returned in the response. </description> </item> </list> If the command value does not match any of the
        /// above cases, an error response is returned indicating that the option was not found.</remarks>
        /// <param name="cmd">The command to execute, containing the desired action or query value.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to playlist modes and response handling.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(cmd.Value) || cmd.Value.Equals("get", StringComparison.InvariantCultureIgnoreCase))
            {
                string curMode = ctx.GetSelectedPlaylistMode();
                ctx.Respond(cmd.Command, true, $"Playlist mode is currently set to {curMode}", null);
                return true;
            }

            string mode = cmd.Value.ToLowerInvariant();

            switch (mode)
            {
                case "list":
                    List<string> playlistMode = new();
                    for (int i = 0; i < ctx.GetPlaylistModeCount(); i++)
                    {
                        string name = ctx.GetPlaylistModeNameAt(i);
                        playlistMode.Add(name);
                    }
                    ctx.Respond(cmd.Command, true, "List of playlist modes", playlistMode);
                    return true;
                default:
                    for (int i = 0; i < ctx.GetPlaylistModeCount(); i++)
                    {
                        string name = ctx.GetPlaylistModeNameAt(i);
                        if (name.Contains(cmd.Value, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ctx.SetPlaylistModeIndex(i);
                            ctx.Respond(cmd.Command, true, $"Mode set to {ctx.GetSelectedPlaylistMode()}", null);
                            return true;
                        }
                    }
                    break;
            }

            ctx.Respond(cmd.Command, false, $"Option not found: {cmd.Value}", null);
            return false;
        }
    }

}
