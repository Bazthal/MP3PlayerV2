using System.Text.Json.Serialization;
namespace MP3PlayerV2.Commands
{
    /// <summary>
    /// Represents metadata about a command, including its name, author, handler type, and other descriptive details.
    /// </summary>
    /// <remarks>This record is used to encapsulate information about a command, such as its name, author, and
    /// the type responsible for handling it. It also includes optional fields for a description, an example, and a
    /// category to help organize and document the command.</remarks>
    /// <param name="Name">The name of the command. This value is required and cannot be null.</param>
    /// <param name="Author">The author of the command. This value is optional and can be null.</param>
    /// <param name="HandlerType">The fully qualified type name of the handler responsible for executing the command. This value is required.</param>
    /// <param name="AssemblyName">The name of the assembly where the handler type is defined. This value is required.</param>
    /// <param name="Description">An optional description of the command. This value can be null.</param>
    /// <param name="Category">An optional category to which the command belongs. This value can be null.</param>
    /// <param name="Example">An optional example demonstrating the usage of the command. This value can be null. The value is serialized
    /// using the <see cref="BazthalLib.Extensibility.Serialization.InlineJsonConverter"/>.</param>
    public record CommandInfo(
        string Name,
        string? Author,
        string? Version,
        string HandlerType,
        string AssemblyName,
        string? Description,
        string? Category,
        [property: JsonConverter(typeof(BazthalLib.Extensibility.Serialization.InlineJsonConverter))]
        string? Example
        );

}