namespace MP3PlayerV2.Services
{
    internal static class GlobalErrorCatcher
    {
        private static readonly string LogFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CrashLog.txt");

        /// <summary>
        /// Initializes global exception handling for the application.
        /// </summary>
        /// <remarks>This method sets up handlers for unhandled exceptions in the following contexts:
        /// <list type="bullet"> <item><description>UI thread exceptions via <see
        /// cref="Application.ThreadException"/>.</description></item> <item><description>AppDomain unhandled exceptions
        /// via <see cref="AppDomain.UnhandledException"/>.</description></item> <item><description>Unobserved task
        /// exceptions via <see cref="TaskScheduler.UnobservedTaskException"/>.</description></item> </list> Each
        /// handler logs the exception details and ensures proper handling where applicable.</remarks>
        public static void Init()
        {
            Application.ThreadException += (s, e) => Log("UI Thread", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Log("AppDomain", e.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log("TaskScheduler", e.Exception);
                e.SetObserved();
            };
        }

        /// <summary>
        /// Logs an exception message with a timestamp and source to a log file.
        /// </summary>
        /// <remarks>The log entry includes the current timestamp, the specified source, and the exception
        /// details (if provided). The log is appended to a file specified by the <c>LogFile</c> field.</remarks>
        /// <param name="source">The source or context of the log entry, typically identifying where the exception occurred.</param>
        /// <param name="ex">The exception to log. Can be <see langword="null"/> to log only the source.</param>
        private static void Log(string source, Exception? ex)
        {
            var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {ex}";
            File.AppendAllText(LogFile, msg + Environment.NewLine);
        }
    }

}
