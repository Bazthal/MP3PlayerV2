using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace MP3PlayerV2
{
    internal static class Program
    {
        private static Mutex? _mutex;
        private const string AppMutex = "MP3PlayerV2_Mutex";
        private const string PipeName = "MP3PlayerV2_Pipe";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <remarks>This method initializes the application, ensures only a single instance is running
        /// (unless a debugger is attached),  and starts the main application form. It also sets up necessary
        /// directories and handles inter-process communication  for passing arguments to an existing
        /// instance.</remarks>
        /// <param name="args">Optional command-line arguments passed to the application. These arguments are forwarded to an existing
        /// instance if one is already running.</param>
        [STAThread]
        static void Main(string[]? args = null)
        {
            BazthalLib.Systems.IO.Files.CreateDirectory(Path.Combine(Application.StartupPath, "Config"));
            BazthalLib.Systems.IO.Files.CreateDirectory(Path.Combine(Application.StartupPath, "Playdata"));

            //Register Error handler logging
            Services.GlobalErrorCatcher.Init();

            bool isNewInstance;
            _mutex = new Mutex(true, AppMutex, out isNewInstance);
            // Allow multiple instances when a debugger is attached, enabling simultaneous player usage and development
            bool allowMultiple = Debugger.IsAttached;

            if (!isNewInstance && !allowMultiple)
            {
                if (args != null && args.Length > 0)
                {
                    using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                    client.Connect(1000);
                    using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                    writer.WriteLine(string.Join("|", args));
                }
                return;
            }

            if (!allowMultiple)
            {
                StartPipeServer();
            }

            ApplicationConfiguration.Initialize();
#pragma warning disable WFO5001
            Application.SetColorMode(SystemColorMode.System);
#pragma warning restore WFO5001
            Application.Run(new MP3PlayerV2(args));
        }
        /// <summary>
        /// Starts a named pipe server that listens for incoming connections and processes data sent through the pipe.
        /// </summary>
        /// <remarks>This method runs the server on a background task, continuously waiting for
        /// connections. When a connection is  established, it reads a line of text from the client, splits it into file
        /// paths, and invokes the  <see cref="MP3PlayerV2.HandleDroppedFiles"/> method to handle the files. The method
        /// assumes the data is  UTF-8 encoded and delimited by the '|' character.</remarks>
        private static void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                    server.WaitForConnection();
                    using var reader = new StreamReader(server, Encoding.UTF8);
                    string? line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] files = line.Split('|');
                        MP3PlayerV2.Instance?.Invoke(() =>
                        {
                            MP3PlayerV2.Instance.HandleDroppedFiles(files, true);
                        });
                    }
                }
            });
        }
    }
}