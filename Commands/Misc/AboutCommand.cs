using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Misc
{
    /// <summary>
    /// Handles the execution of the "aboutCommand" to list metadata for a specified command or all commands.
    /// </summary>
    /// <remarks>This command provides metadata information for a specified command or lists metadata for all
    /// commands if "all" is specified. It is useful for retrieving command descriptions and details within the
    /// application.</remarks>
    [Command(
        name: "aboutCommand",
        author: "Bazthal",
        version: "1.0.0",
        description: "Lists Metadata from a specified command or full list from all commands",
        example: "{ \"Command\": \"aboutcommand\" , \"Value\": \"play\" }",
        category: "Misc"
)]
    public class AboutCommand : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given context.
        /// </summary>
        /// <remarks>If the command value is "all", the method responds with metadata for all commands. If
        /// the command value matches a specific metadata name, it responds with the metadata for that command. If no
        /// match is found, it responds with an error message indicating the command was not found.</remarks>
        /// <param name="cmd">The player command to execute, containing the command value to be processed.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to metadata and response handling.</param>
        /// <returns><see langword="true"/> if the command is successfully executed and a response is generated; otherwise, <see
        /// langword="false"/> if the command value is empty or does not match any metadata.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            var metadata = ctx.GetMetaData().ToList();
            List<object> args = new List<object>();


            if (String.IsNullOrWhiteSpace(cmd.Value))
            {
                ctx.Respond(cmd.Command, false, "Value is empty", null);
                return false;
            }

            if (cmd.Value.Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var item in metadata)
                {
                    args.Add(item);
                }
                ctx.Respond(cmd.Command, true, "Metadata for all commands", args);
                return true;
            }

            foreach (var item in metadata)
            {
                if (item.Name.Equals(cmd.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    ctx.Respond(cmd.Command, true, $"Metadata for {item.Name} command", item);
                    return true;
                }
            }    


            ctx.Respond(cmd.Command, false, "Command not found check spelling and try again", cmd.Value);
            return false;
        }
    }
}
