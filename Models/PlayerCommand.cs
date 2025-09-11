namespace MP3PlayerV2.Models
{
    /// <summary>
    /// Represents a command issued by a player, including optional parameters for additional context.
    /// </summary>
    /// <remarks>This class encapsulates a player's command along with optional metadata such as a value,
    /// order, or range. It can be used to represent and process player actions in a structured format.</remarks>
    public class PlayerCommand
    {
        public string Command { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Order { get; set; }
        public string? Range { get; set; }

    }
}
