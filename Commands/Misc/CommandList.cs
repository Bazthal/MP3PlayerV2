using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands.Misc
{
    /// <summary>
    /// Handles the execution of the "commandlist" command, which lists all registered commands.
    /// </summary>
    /// <remarks>This command retrieves a list of all commands currently registered in the system and formats
    /// them for display. The list includes the command name, its origin, and type. The formatted list is then sent as a
    /// response to the command context.</remarks>
    [Command(
        name: "commandlist",
        author:"Bazthal",
        version: "1.0.0",
        description: "Lists all registered commands",
        example: "{ \"Command\": \"commandlist\" }",
        category: "Misc"
        )]
    public class CommandList : ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>This method retrieves a list of registered commands from the provided <paramref
        /// name="ctx"/> and responds with a formatted list of these commands.</remarks>
        /// <param name="cmd">The player command to be executed. This parameter is not used in the current implementation.</param>
        /// <param name="ctx">The context in which the command is executed, providing access to registered commands and response
        /// mechanisms.</param>
        /// <returns><see langword="true"/> if the command execution completes successfully; otherwise, <see langword="false"/>.</returns>
        public bool Execute(PlayerCommand cmd, CommandContext ctx)
        {
            //List Registered Commands

            var tempList = new List<string>();
            var commands = ctx.ListRegisteredCommands();
            foreach (var (name, origin, type) in commands)
            {
                tempList.Add($"{origin}: {name} -> {type}");
            }
            ctx.Respond(cmd.Command, true, $"Registered Commands List", tempList);

            return true;
        }
    }
}
