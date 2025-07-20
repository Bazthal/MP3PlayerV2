namespace MP3PlayerV2
{

    /// <summary>
    /// Represents a form for configuring application settings related to the WebSocket server.
    /// </summary>
    /// <remarks>The <see cref="SettingsForm"/> class provides a user interface for viewing and modifying
    /// WebSocket server settings such as address, port, endpoint, and auto-start options. It initializes with the
    /// provided application settings and allows users to apply changes through the form's controls. The form also
    /// manages the state of the WebSocket server by enabling or disabling control buttons based on the server's current
    /// status.</remarks>
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsForm"/> class with the specified application settings.
        /// </summary>
        /// <remarks>This constructor sets up the form by registering it for theming, initializing its
        /// components, and populating the form fields with values from the provided <paramref name="appSettings"/>. It
        /// also checks the status of the WebSocket server upon initialization.</remarks>
        /// <param name="appSettings">The application settings used to initialize the form's fields. Cannot be null.</param>
        public SettingsForm(AppSettings appSettings)
        {
            BazthalLib.UI.Theming.RegisterForm(this);
            InitializeComponent();
            _appSettings = appSettings;

            tb_WSS_Address.Text = _appSettings.WebSocketAddress;
            tb_WSS_Port.Text = _appSettings.WebSocketPort.ToString();
            tb_WSS_Endpoint.Text = _appSettings.WebSocketEndPoint;
            cb_WSS_Auto_Start.Checked = _appSettings.AutoStart;

            GetWSServerStatus();
        }

        /// <summary>
        /// Updates the enabled state of the WebSocket server control buttons based on the server's current status.
        /// </summary>
        /// <remarks>This method checks the status of the WebSocket server and enables or disables the
        /// start and stop buttons accordingly. If the server is running, the start button is disabled and the stop
        /// button is enabled. Otherwise, the start button is enabled and the stop button is disabled.</remarks>
        private void GetWSServerStatus()
        {
            if (MP3PlayerV2.Instance.GetWssStatus)
            {
                btn_WSS_Start.Enabled = false; 
                btn_WSS_Stop.Enabled = true;             
            }
            else
            {
                btn_WSS_Start.Enabled = true;
                btn_WSS_Stop.Enabled = false;            
            }

        }

        /// <summary>
        /// Updates the application settings with the current values from the user interface.
        /// </summary>
        /// <remarks>This method retrieves the WebSocket address, port, endpoint, and auto-start settings
        /// from the corresponding UI elements and updates the application settings accordingly. If the port value is
        /// not a valid integer, it defaults to 8080.</remarks>
        private void UpdateSetting()
        {
            _appSettings.WebSocketAddress = tb_WSS_Address.Text;
            _appSettings.WebSocketPort = int.TryParse(tb_WSS_Port.Text, out var port) ? port : 8080;
            _appSettings.WebSocketEndPoint = tb_WSS_Endpoint.Text;
            _appSettings.AutoStart = cb_WSS_Auto_Start.Checked;
         }

        /// <summary>
        /// Handles the click event for the Apply button, updating settings and closing the dialog.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Apply button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void Apply_Click(object sender, EventArgs e)
        {
            UpdateSetting();
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the click event for the Start button, initiating the server start process.
        /// </summary>
        /// <remarks>This method updates the necessary settings, starts the server thread, and retrieves
        /// the current status of the WebSocket server.</remarks>
        /// <param name="sender">The source of the event, typically the Start button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void Start_Click(object sender, EventArgs e)
        {
            UpdateSetting();
            MP3PlayerV2.Instance.StartServerThread();
            GetWSServerStatus();
        }

        /// <summary>
        /// Handles the click event for stopping the server.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Stop_Click(object sender, EventArgs e) 
        { 
            MP3PlayerV2.Instance.StopServerThread();
            GetWSServerStatus();
        }
    }
}
