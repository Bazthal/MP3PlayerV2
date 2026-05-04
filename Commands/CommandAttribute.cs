namespace MP3PlayerV2.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string? Author { get; init; }
        public string? Version { get; init; }
        public string? Description { get; init; }
        public string? Category { get; }
        public string? Example { get; init; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class with the specified command name.
        /// </summary>
        /// <param name="name">The name of the command. This value is converted to lowercase invariant culture.</param>
        public CommandAttribute(string name)
        {
            Name = name.ToLowerInvariant();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class with the specified command name and
        /// optional metadata.
        /// </summary>
        /// <remarks>The <see cref="CommandAttribute"/> is used to annotate methods with metadata about a
        /// command, including its name, author, description, example, and category.</remarks>
        /// <param name="name">The name of the command. This value is required and will be converted to lowercase invariant.</param>
        /// <param name="author">The author of the command. This parameter is optional and can be null.</param>
        /// <param name="description">A brief description of the command. This parameter is optional and can be null.</param>
        /// <param name="category">The category to which the command belongs. This parameter is optional and can be null.</param>
        /// <param name="example">An example usage of the command. This parameter is optional and can be null.</param>
        public CommandAttribute(
            string name,
            string? author = null,
            string? version = null,
            string? description = null,
            string? category = null,
            string? example = null)
        {
            Name = name.ToLowerInvariant();
            Author = author;
            Version = version;
            Category = category;
            Description = description;
            Example = example;
        }
    }
}