namespace MP3PlayerV2.Models
{
    /// <summary>
    /// Represents a command issued by a player, including an optional value associated with the command.
    /// </summary>
    /// <remarks>This class is used to encapsulate a player's command and any additional data that may be
    /// required to execute the command.</remarks>
    public class PlayerCommand
    {
        public string Command { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Order { get; set; }

    }
}
