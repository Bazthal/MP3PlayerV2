using BazthalLib;
using BazthalLib.Controls;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using static MP3PlayerV2.MP3PlayerV2;

namespace MP3PlayerV2
{

    /// <summary>
    /// Represents a form for configuring application settings related to the WebSocket server.
    /// </summary>
    /// <remarks>The <see cref = "SettingsForm"/> class provides a user interface for viewing and modifying
    /// WebSocket server settings such as address, port, endpoint, and auto-start options. It initializes with the
    /// provided application settings and allows users to apply changes through the form's controls. The form also
    /// manages the state of the WebSocket server by enabling or disabling control buttons based on the server's current
    /// status.</remarks>
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref = "SettingsForm"/> class with the specified application settings.
        /// </summary>
        /// <remarks>This constructor sets up the form by registering it for theming, initializing its
        /// components, and populating the form fields with values from the provided <paramref name = "appSettings"/>. It
        /// also checks the status of the WebSocket server upon initialization.</remarks>
        /// <param name = "appSettings">The application settings used to initialize the form's fields. Cannot be null.</param>
        public SettingsForm(AppSettings appSettings)
        {
            BazthalLib.UI.Theming.RegisterForm(this);
            InitializeComponent();
            //Set Config path to use full application startup path instead of relative path
            ThemeSetter.ConfigFilePath = Path.Combine(Application.StartupPath, "Config\\CustomTheme.json");

            cb_Shuffle_Modes.Items.Clear();// Should already be empty but for safety
            // cb_Shuffle_Modes.Items.AddRange(Enum.GetNames(typeof(SmartShuffleMode)));
            cb_Shuffle_Modes.Items.AddRange([SmartShuffleMode.UnplayedFirst, SmartShuffleMode.MostPlayed]); // For Now just add the ones implemented
            _appSettings = appSettings;
            UpdateFromSettings();
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
        /// Resets the application settings to their default values.
        /// </summary>
        /// <remarks>This method initializes all application settings to their default configurations and
        /// ensures that the current settings are updated and saved. It is typically used to restore the application to
        /// a known default state.</remarks>
        private void SetDefault()
        {
            _appSettings.Playback = new PlaybackSettings();
            _appSettings.PlayData = new PlayDataSettings();
            _appSettings.WebSocket = new WebSocketSettings();
            _appSettings.SmartShuffle = new SmartShuffleSettings();
            _appSettings.TrackRating = new TrackRatingSettings();
            _appSettings.CommandBehaviour = new CommandBehaviourSettings();
            UpdateFromSettings(); // Load From Settings
            UpdateSettings(); // Save Default Settings
        }

        /// <summary>
        /// Updates the user interface controls to reflect the current application settings.
        /// </summary>
        /// <remarks>This method synchronizes the values of various UI elements, such as numeric up-down
        /// controls,  checkboxes, and text boxes, with the corresponding values from the application's settings object.
        /// It ensures that the displayed settings in the UI are consistent with the underlying configuration.</remarks>
        private void UpdateFromSettings()
        {
            //Playback Settings
            nud_StackLimit.Value = _appSettings.Playback.MaxTrackHistory;

            //Smart Shuffle Settings
            cb_Shuffle_Modes.SelectedItem = Enum.TryParse(_appSettings.SmartShuffle.Mode, out SmartShuffleMode mode) ? mode.ToString() : SmartShuffleMode.UnplayedFirst.ToString();

            tb_PlayedWeight.Value = (int)(_appSettings.SmartShuffle.Weights.Played * 100);
            tb_UnplayedWeight.Value = (int)(_appSettings.SmartShuffle.Weights.Unplayed * 100);
            tb_SkippedWeight.Value = (int)(_appSettings.SmartShuffle.Weights.Skipped * 100);
            tb_UserRatedWeight.Value = (int)(_appSettings.SmartShuffle.Weights.UserRated * 100);

            //Track Rating Settings
            nud_EarlySkip.Value = _appSettings.TrackRating.EarlySkipPenalty;
            nud_MidSkip.Value = _appSettings.TrackRating.MidSkipPenalty;
            nud_SeekToEnd.Value = _appSettings.TrackRating.SeekToEndPenalty;
            nud_Dislike.Value = _appSettings.TrackRating.ManualDislikePenalty;

            nud_NoSkip.Value = _appSettings.TrackRating.NoSkipReward;
            nud_Replay.Value = _appSettings.TrackRating.ReplayReward;
            nud_Like.Value = _appSettings.TrackRating.ManualLikeBoost;

            tb_EarlySkipThresholdPercent.Value = (int)(_appSettings.TrackRating.EarlySkipThresholdPercent * 100);
            nud_LeadInImmunity.Value = _appSettings.TrackRating.LeadInImmunitySeconds;
            nud_LeadOutImmunity.Value = _appSettings.TrackRating.LeadOutImmunitySeconds;

            tb_MonthlyDecay.Value = (int)(_appSettings.TrackRating.MonthlyDecayPercent * 100);

            nud_FiveStarMinScore.Value = _appSettings.TrackRating.FiveStarMinScore;
            nud_FourStarMinScore.Value = _appSettings.TrackRating.FourStarMinScore;
            nud_ThreeStarMinScore.Value = _appSettings.TrackRating.ThreeStarMinScore;
            nud_TwoStarMinScore.Value = _appSettings.TrackRating.TwoStarMinScore;

            //Database Management Settings
            chk_EnablePrune.Checked = _appSettings.PlayData.EnablePruning;
            chk_ArhiveInstead.Checked = _appSettings.PlayData.ArchiveOverDelete;
            nud_PruneDays.Value = _appSettings.PlayData.PruneDays;


            //WebSocket Settings
            tb_WSS_Address.Text = _appSettings.WebSocket.Address;
            tb_WSS_Port.Text = _appSettings.WebSocket.Port.ToString();
            tb_WSS_Endpoint.Text = _appSettings.WebSocket.EndPoint;
            cb_WSS_Auto_Start.Checked = _appSettings.WebSocket.AutoStart;


        }

        /// <summary>
        /// Updates the application settings based on the current user interface values.
        /// </summary>
        /// <remarks>This method synchronizes the values from the user interface controls with the
        /// application's settings. It updates various categories of settings, including playback, smart shuffle, track
        /// rating, database  management, and WebSocket configuration. The updated settings are applied immediately and
        /// persist  for the application's runtime.</remarks>
        private void UpdateSettings()
        {
            //Playback Settings
            _appSettings.Playback.MaxTrackHistory = (int)nud_StackLimit.Value;

            //Smart Shuffle Settings
            _appSettings.SmartShuffle.Mode = cb_Shuffle_Modes.SelectedItem;

            _appSettings.SmartShuffle.Weights.Played = tb_PlayedWeight.Value / 100f;
            _appSettings.SmartShuffle.Weights.Unplayed = tb_UnplayedWeight.Value / 100f;
            _appSettings.SmartShuffle.Weights.Skipped = tb_SkippedWeight.Value / 100f;
            _appSettings.SmartShuffle.Weights.UserRated = tb_UserRatedWeight.Value / 100f;

            //Track Rating Settings
            _appSettings.TrackRating.EarlySkipPenalty = (int)nud_EarlySkip.Value;
            _appSettings.TrackRating.MidSkipPenalty = (int)nud_MidSkip.Value;
            _appSettings.TrackRating.SeekToEndPenalty = (int)nud_SeekToEnd.Value;
            _appSettings.TrackRating.ManualDislikePenalty = (int)nud_Dislike.Value;

            _appSettings.TrackRating.NoSkipReward = (int)nud_NoSkip.Value;
            _appSettings.TrackRating.ReplayReward = (int)nud_Replay.Value;
            _appSettings.TrackRating.ManualLikeBoost = (int)nud_Like.Value;

            _appSettings.TrackRating.EarlySkipThresholdPercent = tb_EarlySkipThresholdPercent.Value / 100.0;
            _appSettings.TrackRating.LeadInImmunitySeconds = (int)nud_LeadInImmunity.Value;
            _appSettings.TrackRating.LeadOutImmunitySeconds = (int)nud_LeadOutImmunity.Value;

            _appSettings.TrackRating.MonthlyDecayPercent = tb_MonthlyDecay.Value / 100.00;

            _appSettings.TrackRating.FiveStarMinScore = (int)nud_FiveStarMinScore.Value;
            _appSettings.TrackRating.FourStarMinScore = (int)nud_FourStarMinScore.Value;
            _appSettings.TrackRating.ThreeStarMinScore = (int)nud_ThreeStarMinScore.Value;
            _appSettings.TrackRating.TwoStarMinScore = (int)nud_TwoStarMinScore.Value;

            //Database Management Settings
            _appSettings.PlayData.EnablePruning = chk_EnablePrune.Checked;
            _appSettings.PlayData.ArchiveOverDelete = chk_ArhiveInstead.Checked;
            _appSettings.PlayData.PruneDays = (int)nud_PruneDays.Value;

            //WebSocket Settings
            _appSettings.WebSocket.Address = tb_WSS_Address.Text;
            _appSettings.WebSocket.Port = int.TryParse(tb_WSS_Port.Text, out var port) ? port : 8080;
            _appSettings.WebSocket.EndPoint = tb_WSS_Endpoint.Text;
            _appSettings.WebSocket.AutoStart = cb_WSS_Auto_Start.Checked;
        }

        /// <summary>
        /// Handles the click event for the Apply button, updating settings and closing the dialog.
        /// </summary>
        /// <param name = "sender">The source of the event, typically the Apply button.</param>
        /// <param name = "e">The event data associated with the click event.</param>
        private void Apply_Click(object sender, EventArgs e)
        {
            UpdateSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the click event for the Start button, initiating the server start process.
        /// </summary>
        /// <remarks>This method updates the necessary settings, starts the server thread, and retrieves
        /// the current status of the WebSocket server.</remarks>
        /// <param name = "sender">The source of the event, typically the Start button.</param>
        /// <param name = "e">The event data associated with the click event.</param>
        private void Start_Click(object sender, EventArgs e)
        {
            UpdateSettings();
            MP3PlayerV2.Instance.StartServerThread();
            GetWSServerStatus();
        }

        /// <summary>
        /// Handles the click event for stopping the server.
        /// </summary>
        /// <param name = "sender">The source of the event.</param>
        /// <param name = "e">The event data.</param>
        private void Stop_Click(object sender, EventArgs e)
        {
            MP3PlayerV2.Instance.StopServerThread();
            GetWSServerStatus();
        }

        /// <summary>
        /// Handles the click event to reset the database and all associated play data.
        /// </summary>
        /// <remarks>Displays a confirmation dialog to the user before proceeding. If the user confirms, 
        /// a processing dialog is shown while the database reset operation is performed asynchronously.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ResetDB_Click(object sender, EventArgs e)
        {
            if (ThemableMessageBox.Show("This would nuke the database and all playdata", "Are You Sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {

                var dialog = new ThemableProcessingDialog($"Resetting ...") { StartPosition = FormStartPosition.Manual };
                dialog.Show(this);
                dialog.Location = new Point(
                    this.Location.X + (this.Width - dialog.Width) / 2,
                    this.Location.Y + (this.Height - dialog.Height) / 2
                );

                await MP3PlayerV2.Instance.ResetTrackStatsAdaptiveAsync(mode: "all", tag: "all", dialog);
            }
        }

        /// <summary>
        /// Handles the click event for the "Migrate Database" button, prompting the user for confirmation  before
        /// attempting to import existing play data.
        /// </summary>
        /// <remarks>Displays a confirmation dialog to the user. If the user confirms, the method
        /// initiates the  import process for existing play data by invoking the <see cref="MP3PlayerV2.RunImport"/>
        /// method.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void MigrateDB_Click(Object sender, EventArgs e)
        {
            if (ThemableMessageBox.Show("This would attempt to import existing playdata", "Are You Sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                MP3PlayerV2.Instance.RunImport();
            }
        }

        /// <summary>
        /// Handles the click event for the "Prune" button, initiating the pruning of old track data.
        /// </summary>
        /// <remarks>This method updates the application settings and displays a processing dialog while
        /// pruning tracks. If pruning is disabled in the application settings, a message box is displayed
        /// instead.</remarks>
        /// <param name="sender">The source of the event, typically the "Prune" button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private async void PruneBtn_Click(object sender, EventArgs e)
        {
            UpdateSettings();

            ThemableProcessingDialog dlg = new ThemableProcessingDialog("Pruning tracks...") { StartPosition = FormStartPosition.Manual };
            dlg.Location = new(
                this.Location.X + (this.Width - dlg.Width) / 2,
                this.Location.Y + (this.Height - dlg.Height) / 2
            );
            if (!_appSettings.PlayData.EnablePruning)
            {
                ThemableMessageBox.Show("Pruning is not enabled", "Unabled to start pruning", MessageBoxButtons.OK);
            }
            else
            {
                await TrackDatabase.PruneOldDataAsync(_appSettings.PlayData, dlg);
            }
        }

        /// <summary>
        /// Handles the scroll event of the unplayed track bar.
        /// </summary>
        /// <remarks>Updates the label to display the current value of the track bar as a formatted
        /// unplayed weight.</remarks>
        /// <param name="sender">The source of the event, typically the track bar control.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void UnplayedTrackBar_Scroll(object sender, EventArgs e)
        {
            lbl_UnPlayed.Text = $"{tb_UnplayedWeight.Value / 100f} - Unplayed";
        }

        /// <summary>
        /// Handles the scroll event of the track bar that adjusts the "played" weight.
        /// </summary>
        /// <param name="sender">The source of the event, typically the track bar.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void PlayedTrackBar_Scroll(object sender, EventArgs e)
        {
            lbl_Played.Text = $"{tb_PlayedWeight.Value / 100f} - Played";

        }

        /// <summary>
        /// Handles the scroll event of the skipped track bar.
        /// </summary>
        /// <remarks>Updates the label to display the current value of the track bar, formatted as a
        /// floating-point number.</remarks>
        /// <param name="sender">The source of the event, typically the track bar control.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void SkippedTrackBar_Scroll(object sender, EventArgs e)
        {
            lbl_Skipped.Text = $"{tb_SkippedWeight.Value / 100f} - Skipped";
        }

        /// <summary>
        /// Handles the scroll event of the user-rated track bar.
        /// </summary>
        /// <remarks>Updates the label to display the current value of the track bar as a formatted user
        /// rating.</remarks>
        /// <param name="sender">The source of the event, typically the track bar control.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void UserRatedTrackBar_Scroll(object sender, EventArgs e)
        {
            lbl_UserRated.Text = $"{tb_UserRatedWeight.Value / 100f} - User Rated";
        }

        /// <summary>
        /// Handles the scroll event for the Monthly Decay track bar.
        /// </summary>
        /// <remarks>Updates the label to display the current value of the track bar as a
        /// percentage.</remarks>
        /// <param name="sender">The source of the event, typically the track bar control.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void MonthlyDecay_Scroll(object sender, EventArgs e)
        {
            lbl_MonthlDecay.Text = $"{tb_MonthlyDecay.Value}%";
        }

        /// <summary>
        /// Handles the scroll event for the Early Skip threshold slider.
        /// </summary>
        /// <remarks>Updates the label to display the current value of the Early Skip threshold slider as
        /// a percentage.</remarks>
        /// <param name="sender">The source of the event, typically the slider control.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void EarlySkip_Scroll(object sender, EventArgs e)
        {
            lbl_Val_EarlySkipThreshold.Text = $"{tb_EarlySkipThresholdPercent.Value}%";
        }

        /// <summary>
        /// Handles the click event of the Reset button and resets the application state to its default values.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Reset button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void ResetButton_Click(object sender, EventArgs e)
        {
            SetDefault();
        }
    }
}