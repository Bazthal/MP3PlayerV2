using MP3PlayerV2.Models;

namespace MP3PlayerV2.Commands
{
    /// <summary>
    /// Defines a handler for processing player commands within a specified context.
    /// </summary>
    /// <remarks>Implementations of this interface should provide the logic to handle different types of
    /// player commands and determine the appropriate actions based on the command and its context.</remarks>
    public interface ICommandHandler
    {
        /// <summary>
        /// Executes the specified player command within the given command context.
        /// </summary>
        /// <remarks>The success of the execution depends on the validity of the command and the state of
        /// the provided context.</remarks>
        /// <param name="cmd">The player command to execute. This determines the action to be performed.</param>
        /// <param name="ctx">The context in which the command is executed, providing necessary state and resources.</param>
        /// <returns><see langword="true"/> if the command was successfully executed; otherwise, <see langword="false"/>.</returns>
        bool Execute(PlayerCommand cmd, CommandContext ctx);
    }
}