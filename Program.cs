namespace MP3PlayerV2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            BazthalLib.Systems.IO.Files.CreateDirectory("Config");

            ApplicationConfiguration.Initialize();
#pragma warning disable WFO5001 
            Application.SetColorMode(SystemColorMode.System);
#pragma warning restore WFO5001 
            Application.Run(new MP3PlayerV2());

        }
    }
}