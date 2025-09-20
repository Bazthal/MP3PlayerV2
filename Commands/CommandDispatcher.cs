using MP3PlayerV2.Models;
using System.Reflection;
using System.Text.Json;

namespace MP3PlayerV2.Commands
{
    public class CommandDispatcher
    {
        /// <summary>
        /// Represents a collection of command handlers, indexed by their command names.
        /// </summary>
        /// <remarks>This dictionary is used to store and retrieve command handlers based on the command
        /// name. It is intended for internal use within the command processing system.</remarks>
        private readonly Dictionary<string, ICommandHandler> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDispatcher"/> class.
        /// </summary>
        /// <remarks>The constructor initializes the command dispatcher by loading command
        /// handlers.</remarks>
        public CommandDispatcher()
        {
            _handlers = LoadHandlers();
        }

        /// <summary>
        /// Loads and initializes command handlers from the current application domain.
        /// </summary>
        /// <remarks>This method scans all assemblies in the current application domain for types that
        /// implement the <see cref="ICommandHandler"/> interface and are decorated with the <see
        /// cref="CommandAttribute"/>. It instantiates each handler and adds it to a dictionary, keyed by the command
        /// name specified in the attribute.</remarks>
        /// <returns>A dictionary containing command names as keys and their corresponding <see cref="ICommandHandler"/>
        /// instances as values.</returns>
        private Dictionary<string, ICommandHandler> LoadHandlers()
        {
            var handlers = new Dictionary<string, ICommandHandler>();

            string pluginPath = Path.Combine(Application.StartupPath, "Plugins");

            if (Directory.Exists(pluginPath))
            {
                foreach (var dll in Directory.GetFiles(pluginPath, "*.dll"))
                {
                    try
                    {
                        Assembly.LoadFrom(dll);
                        BazthalLib.DebugUtils.Log("PluginLoader", "Loaded", dll);
                    }
                    catch (Exception ex)
                    {
                        BazthalLib.DebugUtils.Log("PluginLoaderError", dll, ex.ToString());
                    }
                }
            }

            var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(ICommandHandler).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.GetCustomAttribute<CommandAttribute>() != null) ;

            foreach (var type in commandTypes)
            {
                var attr = type.GetCustomAttribute<CommandAttribute>();
                try
                {
                    if (Activator.CreateInstance(type) is ICommandHandler handler)
                    {
                        handlers[attr.Name] = handler;
                        BazthalLib.DebugUtils.Log("Dictionary", "Handlers", $"{handler}");
                    }
                }
                catch (Exception ex)
                {
                    BazthalLib.DebugUtils.Log("HandlerLoadError", type.FullName, ex.ToString());
                }

            }
            return handlers;
        }

        /// <summary>
        /// Dispatches a command by deserializing a raw JSON message and executing the corresponding command handler.
        /// </summary>
        /// <remarks>This method attempts to deserialize the provided JSON message into a
        /// <c>PlayerCommand</c> object. If the command is recognized and a corresponding handler is found, the handler
        /// is executed. If the command is unknown or deserialization fails, the method responds with an appropriate
        /// error message.</remarks>
        /// <param name="rawMessage">The raw JSON string representing the command to be dispatched.</param>
        /// <param name="context">The context in which the command is executed, providing necessary execution details and response
        /// capabilities.</param>
        /// <param name="options">The JSON serializer options used for deserializing the command.</param>
        /// <returns><see langword="true"/> if the command is successfully executed by a handler; otherwise, <see
        /// langword="false"/>.</returns>
        public bool Dispatch(string rawMessage, CommandContext context, JsonSerializerOptions options)
        {
            try
            {
                var cmd = JsonSerializer.Deserialize<PlayerCommand>(rawMessage, options);
                if (cmd == null || string.IsNullOrWhiteSpace(cmd.Command))
                    return false;

                if (_handlers.TryGetValue(cmd.Command.ToLowerInvariant(), out var handler))
                {
                    return handler.Execute(cmd, context);
                }
                context.Respond(false, $"Unkown Command: {cmd.Command}", null);
                return false;
            }
            catch (Exception ex)
            {
                context.Respond(false, "Failed to parse command", ex.Message);
                return false;
            }


        }
    }
}
