using BazthalLib;
using MP3PlayerV2.Models;
using System;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MP3PlayerV2.Services
{
    /// <summary>
    /// Manages WebSocket server operations including lifecycle, broadcasting, and message handling.
    /// </summary>
    public class WebSocketService : IDisposable
    {
        #region Fields

        private WebSocketServer? _server;
        private Thread? _serverThread;
        private bool _running;
        //private string _lastResponse = string.Empty;
        private readonly ConcurrentQueue<string> _pendingResponses = new();
        private static string _commandEndpoint = ConfigManager.Settings.WebSocket.EndPoint;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the WebSocket server is currently running.
        /// </summary>
        public bool IsRunning => _running;
        /*
        /// <summary>
        /// Gets the last response message that was built.
        /// </summary>
        public string LastResponse => _lastResponse;
        */
        #endregion

        #region Events

        /// <summary>
        /// Occurs when a command message is received from a client.
        /// </summary>
        public event EventHandler<CommandReceivedEventArgs>? CommandReceived;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the WebSocket server on a background thread.
        /// </summary>
        /// <param name="address">The IP address to bind to (e.g., "127.0.0.1").</param>
        /// <param name="port">The port number to listen on.</param>
        /// <param name="commandEndpoint">The endpoint path for command processing (e.g., "/commands").</param>
        /// <param name="onStartError">Optional callback for handling start errors.</param>
        public void Start(string address, int port, string commandEndpoint, Action<Exception>? onStartError = null)
        {
            if (_running)
            {
                DebugUtils.Log("WebSocketService", "Start", "Server is already running", logLevel: DebugUtils.LogLevel.Warning);
                return;
            }

            _serverThread = new Thread(() => StartServerInternal(address, port, commandEndpoint, onStartError))
            {
                IsBackground = true,
                Name = "WebSocket Server Thread"
            };
            
            _serverThread.Start();
            _running = true;
        }

        /// <summary>
        /// Stops the WebSocket server and releases resources.
        /// </summary>
        public void Stop()
        {
            if (!_running)
                return;

            _running = false;
            _serverThread?.Join(TimeSpan.FromSeconds(5));
            _server?.Stop();
            
            DebugUtils.Log("WebSocketService", "Stop", "WebSocket server stopped", logLevel: DebugUtils.LogLevel.Info);
        }

        /// <summary>
        /// Broadcasts a message to all connected clients on the now playing channel.
        /// </summary>
        /// <param name="trackInfo">The track information to broadcast.</param>
        public void BroadcastNowPlaying(object trackInfo)
        {
            if (!_running || _server == null)
            {
                DebugUtils.Log("WebSocketService", "Broadcast", "Server not running, broadcast skipped", logLevel: DebugUtils.LogLevel.Info);
                return;
            }

            try
            {
                string jsonMessage = JsonSerializer.Serialize(trackInfo, _jsonOptions);
                _server.WebSocketServices["/nowplaying"]?.Sessions.Broadcast(jsonMessage);
                
                DebugUtils.Log("WebSocketService", "Broadcast", "Now playing info broadcasted", logLevel: DebugUtils.LogLevel.Info);
            }
            catch (Exception ex)
            {
                DebugUtils.Log("WebSocketService", "Broadcast", $"Error broadcasting: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
            }
        }

        /// <summary>
        /// Broadcasts a command response to all clients on the command channel.
        /// </summary>
        /// <param name="response">The response message to broadcast.</param>
        public void BroadcastCommandResponse(string response)
        {
            if (!_running || _server == null)
                return;

            try
            {
                _server.WebSocketServices[_commandEndpoint]?.Sessions.Broadcast(response);
                DebugUtils.Log("WebSocketService", "BroadcastResponse", "Response broadcasted", logLevel: DebugUtils.LogLevel.Info);
            }
            catch (Exception ex)
            {
                DebugUtils.Log("WebSocketService", "BroadcastResponse", $"Error: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
            }
        }

        /// <summary>
        /// Builds a JSON response message.
        /// </summary>
        /// <param name="success">Whether the operation was successful.</param>
        /// <param name="message">The response message.</param>
        /// <param name="data">Optional data to include in the response.</param>
        /// <param name="raw">If true, data is treated as raw JSON string.</param>
        /// <returns>The formatted JSON response.</returns>
        public string BuildResponse(string command, bool success, string message, object? data = null, bool raw = false)
        {
            string result = success ? "Success" : "Fail";

            string response;
            if (raw && data is string jsonString)
            {
                response = $"{{\"Command\":\"{command}\",\"Result\":\"{result}\",\"Message\":\"{message}\",\"Data\":{jsonString}}}";
            }
            else
            {
                var payload = new { Command = command, Result = result, Message = message, Data = data };
                response = JsonSerializer.Serialize(payload, _jsonOptions);
            }

            //_lastResponse = response;
            _pendingResponses.Enqueue(response);
            return response;
        }

        #endregion

        #region Private Methods

        private void StartServerInternal(string address, int port, string commandEndpoint, Action<Exception>? onStartError)
        {
            string endpoint = commandEndpoint.StartsWith('/') ? commandEndpoint : $"/{commandEndpoint}";
            string serverUrl = $"ws://{address}:{port}";

            try
            {
                _server = new WebSocketServer(serverUrl);
                
                _server.AddWebSocketService<CommandExecutorBehavior>(endpoint, behavior => 
                {
                    behavior.Initialize(this);
                });
                
                _server.AddWebSocketService<BroadcastBehavior>("/nowplaying");
                
                DebugUtils.Log("WebSocketService", "Setup", $"Added services: {endpoint}, /nowplaying", logLevel: DebugUtils.LogLevel.Info);

                _server.Start();

                if (_server.IsListening)
                {
                    DebugUtils.Log("WebSocketService", "Start", $"Server started at {serverUrl}", logLevel: DebugUtils.LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _running = false;
                DebugUtils.Log("WebSocketService", "Start", $"Failed to start: {ex.Message}", logLevel: DebugUtils.LogLevel.Error);
                onStartError?.Invoke(ex);
                return;
            }

            // Keep thread alive
            while (_running)
            {
                Thread.Sleep(100);
            }

            DebugUtils.Log("WebSocketService", "Loop", "Server loop exited", logLevel: DebugUtils.LogLevel.Info);
            _server?.Stop();
        }

        private void OnCommandReceived(string message)
        {
            CommandReceived?.Invoke(this, new CommandReceivedEventArgs(message));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Stop();
            _server = null;
        }

        #endregion

        #region Inner Classes - WebSocket Behaviors

        /// <summary>
        /// WebSocket behavior for broadcasting now playing information.
        /// </summary>
        private class BroadcastBehavior : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                base.OnOpen();
                
                if (Context.WebSocket.ReadyState == WebSocketState.Open)
                {
                    var welcomeMsg = new { Message = "This is a Broadcast channel stay connected to see what's playing" };
                    string json = JsonSerializer.Serialize(welcomeMsg, _jsonOptions);
                    Send(json);
                    
                    DebugUtils.Log("BroadcastBehavior", "OnOpen", "Sent welcome message", logLevel: DebugUtils.LogLevel.Info);
                }
                else
                {
                    DebugUtils.Log("BroadcastBehavior", "OnOpen", "Unable to send welcome - socket not open", logLevel: DebugUtils.LogLevel.Warning);
                }
            }
        }

        /// <summary>
        /// WebSocket behavior for processing command messages.
        /// </summary>
        private class CommandExecutorBehavior : WebSocketBehavior
        {
            private WebSocketService _service;
#nullable disable
            public CommandExecutorBehavior()
            {
            }

#nullable enable
            public void Initialize(WebSocketService service)
            {
                _service = service;
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                DebugUtils.Log("CommandExecutor", "OnMessage", $"Received: {e.Data}", logLevel: DebugUtils.LogLevel.Info);

                while (_service._pendingResponses.TryDequeue(out _))
                {
                }

                // Raise event for command handling
                _service.OnCommandReceived(e.Data);

                // Send response if available
                if (Context.WebSocket.ReadyState == WebSocketState.Open)
                {
                    if (_service._pendingResponses.TryDequeue(out string? response) && !string.IsNullOrEmpty(response))
                    {
                        _service.BroadcastCommandResponse(response);
                        DebugUtils.Log("CommandExecutor", "OnMessage", "Response broadcasted", logLevel: DebugUtils.LogLevel.Info);
                    }
                }
                else
                {
                    DebugUtils.Log("CommandExecutor", "OnMessage", "Unable to send response - socket not open", logLevel: DebugUtils.LogLevel.Warning);
                }
            }
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event arguments for command received events.
    /// </summary>
    public class CommandReceivedEventArgs : EventArgs
    {
        public string Message { get; }

        public CommandReceivedEventArgs(string message)
        {
            Message = message;
        }
    }

    #endregion
}
