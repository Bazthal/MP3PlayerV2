namespace MP3PlayerV2.Commands
{
    /// <summary>
    /// Specifies that a class represents a command with a given name.
    /// </summary>
    /// <remarks>This attribute is used to annotate classes that define commands, associating them with a
    /// specific command name. The command name is stored in lowercase using the invariant culture.</remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class with the specified command name.
        /// </summary>
        /// <param name="name">The name of the command. This value is converted to lowercase invariant culture.</param>
        public CommandAttribute(string name) => Name = name.ToLowerInvariant();
    }
}
