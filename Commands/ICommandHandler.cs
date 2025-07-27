
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
        bool Execute(PlayerCommand cmd, CommandContext ctx);
    }
}
