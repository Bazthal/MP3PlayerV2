namespace MP3PlayerV2
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ThemeSetter = new BazthalLib.Controls.ThemeColorsSetter();
            gb_WebSocketServer = new BazthalLib.Controls.ThemableGroupBox();
            btn_WSS_Stop = new BazthalLib.Controls.ThemableButton();
            btn_WSS_Start = new BazthalLib.Controls.ThemableButton();
            cb_WSS_Auto_Start = new BazthalLib.Controls.ThemableCheckBox();
            tb_WSS_Endpoint = new BazthalLib.Controls.ThemableTextBox();
            tb_WSS_Port = new BazthalLib.Controls.ThemableTextBox();
            tb_WSS_Address = new BazthalLib.Controls.ThemableTextBox();
            lbl_WSS_Endpoint = new BazthalLib.Controls.ThemableLabel();
            lbl_WSS_Port = new BazthalLib.Controls.ThemableLabel();
            lbl_WSS_Address = new BazthalLib.Controls.ThemableLabel();
            tabControlBase = new BazthalLib.Controls.ThemableTabControlBase();
            tabPage_Theme_Websocket = new TabPage();
            tabPage_TrackRating = new TabPage();
            gb_StarRating = new BazthalLib.Controls.ThemableGroupBox();
            lbl_TwoStar = new BazthalLib.Controls.ThemableLabel();
            nud_TwoStarMinScore = new BazthalLib.Controls.ThemableNumericUpDown();
            lbl_ThreeStar = new BazthalLib.Controls.ThemableLabel();
            lbl_FourStar = new BazthalLib.Controls.ThemableLabel();
            lbl_FiveStar = new BazthalLib.Controls.ThemableLabel();
            nud_ThreeStarMinScore = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_FourStarMinScore = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_FiveStarMinScore = new BazthalLib.Controls.ThemableNumericUpDown();
            gb_Thresholds = new BazthalLib.Controls.ThemableGroupBox();
            lbl_LeadOutImmunity = new BazthalLib.Controls.ThemableLabel();
            lbl_LeadInImmunity = new BazthalLib.Controls.ThemableLabel();
            nud_LeadOutImmunity = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_LeadInImmunity = new BazthalLib.Controls.ThemableNumericUpDown();
            lbl_Val_EarlySkipThreshold = new BazthalLib.Controls.ThemableLabel();
            lbl_EarlySkipThreshold = new BazthalLib.Controls.ThemableLabel();
            tb_EarlySkipThresholdPercent = new BazthalLib.Controls.ThemableTrackBar();
            gb_Rewards = new BazthalLib.Controls.ThemableGroupBox();
            lbl_Like = new BazthalLib.Controls.ThemableLabel();
            lbl_Replay = new BazthalLib.Controls.ThemableLabel();
            lbl_NoSkip = new BazthalLib.Controls.ThemableLabel();
            nud_Like = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_Replay = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_NoSkip = new BazthalLib.Controls.ThemableNumericUpDown();
            gb_MonthlyDecay = new BazthalLib.Controls.ThemableGroupBox();
            lbl_MonthlDecay = new BazthalLib.Controls.ThemableLabel();
            tb_MonthlyDecay = new BazthalLib.Controls.ThemableTrackBar();
            gb_Penalty = new BazthalLib.Controls.ThemableGroupBox();
            lbl_Dislike = new BazthalLib.Controls.ThemableLabel();
            nud_Dislike = new BazthalLib.Controls.ThemableNumericUpDown();
            lbl_SeekToEnd = new BazthalLib.Controls.ThemableLabel();
            lbl_MidSkip = new BazthalLib.Controls.ThemableLabel();
            lbl_EarlySkip = new BazthalLib.Controls.ThemableLabel();
            nud_SeekToEnd = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_MidSkip = new BazthalLib.Controls.ThemableNumericUpDown();
            nud_EarlySkip = new BazthalLib.Controls.ThemableNumericUpDown();
            tabPage_Database_Shuffle = new TabPage();
            gb_Database = new BazthalLib.Controls.ThemableGroupBox();
            btn_Prune = new BazthalLib.Controls.ThemableButton();
            lbl_PruneDays = new BazthalLib.Controls.ThemableLabel();
            btn_ImportDB = new BazthalLib.Controls.ThemableButton();
            btn_ResetDB = new BazthalLib.Controls.ThemableButton();
            nud_PruneDays = new BazthalLib.Controls.ThemableNumericUpDown();
            chk_ArhiveInstead = new BazthalLib.Controls.ThemableCheckBox();
            chk_EnablePrune = new BazthalLib.Controls.ThemableCheckBox();
            gb_Smart_Shuffle = new BazthalLib.Controls.ThemableGroupBox();
            lbl_Stacklimit = new BazthalLib.Controls.ThemableLabel();
            nud_StackLimit = new BazthalLib.Controls.ThemableNumericUpDown();
            lbl_UserRated = new BazthalLib.Controls.ThemableLabel();
            lbl_Skipped = new BazthalLib.Controls.ThemableLabel();
            lbl_Played = new BazthalLib.Controls.ThemableLabel();
            lbl_UnPlayed = new BazthalLib.Controls.ThemableLabel();
            tb_UserRatedWeight = new BazthalLib.Controls.ThemableTrackBar();
            tb_SkippedWeight = new BazthalLib.Controls.ThemableTrackBar();
            tb_PlayedWeight = new BazthalLib.Controls.ThemableTrackBar();
            tb_UnplayedWeight = new BazthalLib.Controls.ThemableTrackBar();
            cb_Shuffle_Modes = new BazthalLib.Controls.ThemableComboBox();
            lbl_Shuffle = new BazthalLib.Controls.ThemableLabel();
            btn_Apply = new BazthalLib.Controls.ThemableButton();
            tabControlHeader = new BazthalLib.Controls.ThemableTabControlHeader();
            btn_ResetToDefault = new BazthalLib.Controls.ThemableButton();
            gb_WebSocketServer.SuspendLayout();
            tabControlBase.SuspendLayout();
            tabPage_Theme_Websocket.SuspendLayout();
            tabPage_TrackRating.SuspendLayout();
            gb_StarRating.SuspendLayout();
            gb_Thresholds.SuspendLayout();
            gb_Rewards.SuspendLayout();
            gb_MonthlyDecay.SuspendLayout();
            gb_Penalty.SuspendLayout();
            tabPage_Database_Shuffle.SuspendLayout();
            gb_Database.SuspendLayout();
            gb_Smart_Shuffle.SuspendLayout();
            SuspendLayout();
            // 
            // ThemeSetter
            // 
            ThemeSetter.AutoLoadConfig = true;
            ThemeSetter.ConfigFilePath = "";
            ThemeSetter.Location = new Point(6, 3);
            ThemeSetter.MaximumSize = new Size(350, 250);
            ThemeSetter.MinimumSize = new Size(350, 250);
            ThemeSetter.Name = "ThemeSetter";
            ThemeSetter.Size = new Size(350, 250);
            ThemeSetter.TabIndex = 0;
            // 
            // gb_WebSocketServer
            // 
            gb_WebSocketServer.Controls.Add(btn_WSS_Stop);
            gb_WebSocketServer.Controls.Add(btn_WSS_Start);
            gb_WebSocketServer.Controls.Add(cb_WSS_Auto_Start);
            gb_WebSocketServer.Controls.Add(tb_WSS_Endpoint);
            gb_WebSocketServer.Controls.Add(tb_WSS_Port);
            gb_WebSocketServer.Controls.Add(tb_WSS_Address);
            gb_WebSocketServer.Controls.Add(lbl_WSS_Endpoint);
            gb_WebSocketServer.Controls.Add(lbl_WSS_Port);
            gb_WebSocketServer.Controls.Add(lbl_WSS_Address);
            gb_WebSocketServer.Location = new Point(380, 4);
            gb_WebSocketServer.Name = "gb_WebSocketServer";
            gb_WebSocketServer.Size = new Size(269, 181);
            gb_WebSocketServer.TabIndex = 1;
            gb_WebSocketServer.TabStop = false;
            gb_WebSocketServer.Text = "Web Socket Server";
            // 
            // btn_WSS_Stop
            // 
            btn_WSS_Stop.FlatAppearance.BorderSize = 0;
            btn_WSS_Stop.Location = new Point(103, 141);
            btn_WSS_Stop.Name = "btn_WSS_Stop";
            btn_WSS_Stop.Size = new Size(75, 23);
            btn_WSS_Stop.TabIndex = 8;
            btn_WSS_Stop.Text = "Stop";
            btn_WSS_Stop.UseVisualStyleBackColor = true;
            btn_WSS_Stop.Click += Stop_Click;
            // 
            // btn_WSS_Start
            // 
            btn_WSS_Start.FlatAppearance.BorderSize = 0;
            btn_WSS_Start.Location = new Point(12, 141);
            btn_WSS_Start.Name = "btn_WSS_Start";
            btn_WSS_Start.Size = new Size(75, 23);
            btn_WSS_Start.TabIndex = 7;
            btn_WSS_Start.Text = "Start";
            btn_WSS_Start.UseVisualStyleBackColor = true;
            btn_WSS_Start.Click += Start_Click;
            // 
            // cb_WSS_Auto_Start
            // 
            cb_WSS_Auto_Start.AccentColor = SystemColors.Highlight;
            cb_WSS_Auto_Start.AutoSize = true;
            cb_WSS_Auto_Start.BorderColor = SystemColors.ControlDark;
            cb_WSS_Auto_Start.Location = new Point(12, 116);
            cb_WSS_Auto_Start.Name = "cb_WSS_Auto_Start";
            cb_WSS_Auto_Start.Size = new Size(114, 19);
            cb_WSS_Auto_Start.TabIndex = 6;
            cb_WSS_Auto_Start.Text = "Auto Start Server";
            cb_WSS_Auto_Start.UseVisualStyleBackColor = true;
            // 
            // tb_WSS_Endpoint
            // 
            tb_WSS_Endpoint.Location = new Point(76, 87);
            tb_WSS_Endpoint.Name = "tb_WSS_Endpoint";
            tb_WSS_Endpoint.Size = new Size(161, 23);
            tb_WSS_Endpoint.TabIndex = 5;
            // 
            // tb_WSS_Port
            // 
            tb_WSS_Port.Location = new Point(76, 58);
            tb_WSS_Port.Name = "tb_WSS_Port";
            tb_WSS_Port.Size = new Size(161, 23);
            tb_WSS_Port.TabIndex = 4;
            // 
            // tb_WSS_Address
            // 
            tb_WSS_Address.Location = new Point(76, 29);
            tb_WSS_Address.Name = "tb_WSS_Address";
            tb_WSS_Address.Size = new Size(161, 23);
            tb_WSS_Address.TabIndex = 3;
            // 
            // lbl_WSS_Endpoint
            // 
            lbl_WSS_Endpoint.AutoSize = true;
            lbl_WSS_Endpoint.BorderColor = Color.Gray;
            lbl_WSS_Endpoint.Location = new Point(12, 89);
            lbl_WSS_Endpoint.Name = "lbl_WSS_Endpoint";
            lbl_WSS_Endpoint.Size = new Size(58, 15);
            lbl_WSS_Endpoint.TabIndex = 2;
            lbl_WSS_Endpoint.Text = "Endpoint:";
            // 
            // lbl_WSS_Port
            // 
            lbl_WSS_Port.AutoSize = true;
            lbl_WSS_Port.BorderColor = Color.Gray;
            lbl_WSS_Port.Location = new Point(12, 60);
            lbl_WSS_Port.Name = "lbl_WSS_Port";
            lbl_WSS_Port.Size = new Size(32, 15);
            lbl_WSS_Port.TabIndex = 1;
            lbl_WSS_Port.Text = "Port:";
            // 
            // lbl_WSS_Address
            // 
            lbl_WSS_Address.AutoSize = true;
            lbl_WSS_Address.BorderColor = Color.Gray;
            lbl_WSS_Address.Location = new Point(12, 31);
            lbl_WSS_Address.Name = "lbl_WSS_Address";
            lbl_WSS_Address.Size = new Size(55, 15);
            lbl_WSS_Address.TabIndex = 0;
            lbl_WSS_Address.Text = "Address: ";
            // 
            // tabControlBase
            // 
            tabControlBase.Appearance = TabAppearance.FlatButtons;
            tabControlBase.Controls.Add(tabPage_Theme_Websocket);
            tabControlBase.Controls.Add(tabPage_TrackRating);
            tabControlBase.Controls.Add(tabPage_Database_Shuffle);
            tabControlBase.Dock = DockStyle.Bottom;
            tabControlBase.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlBase.EnableBorder = false;
            tabControlBase.ItemSize = new Size(0, 1);
            tabControlBase.Location = new Point(0, 49);
            tabControlBase.Name = "tabControlBase";
            tabControlBase.SelectedIndex = 0;
            tabControlBase.Size = new Size(679, 281);
            tabControlBase.SizeMode = TabSizeMode.Fixed;
            tabControlBase.TabIndex = 11;
            // 
            // tabPage_Theme_Websocket
            // 
            tabPage_Theme_Websocket.BackColor = SystemColors.Control;
            tabPage_Theme_Websocket.Controls.Add(ThemeSetter);
            tabPage_Theme_Websocket.Controls.Add(gb_WebSocketServer);
            tabPage_Theme_Websocket.Location = new Point(4, 5);
            tabPage_Theme_Websocket.Name = "tabPage_Theme_Websocket";
            tabPage_Theme_Websocket.Padding = new Padding(3);
            tabPage_Theme_Websocket.Size = new Size(671, 272);
            tabPage_Theme_Websocket.TabIndex = 0;
            tabPage_Theme_Websocket.Text = "Theming & Websocket";
            tabPage_Theme_Websocket.UseVisualStyleBackColor = true;
            // 
            // tabPage_TrackRating
            // 
            tabPage_TrackRating.Controls.Add(gb_StarRating);
            tabPage_TrackRating.Controls.Add(gb_Thresholds);
            tabPage_TrackRating.Controls.Add(gb_Rewards);
            tabPage_TrackRating.Controls.Add(gb_MonthlyDecay);
            tabPage_TrackRating.Controls.Add(gb_Penalty);
            tabPage_TrackRating.Location = new Point(4, 5);
            tabPage_TrackRating.Name = "tabPage_TrackRating";
            tabPage_TrackRating.Padding = new Padding(3);
            tabPage_TrackRating.Size = new Size(671, 272);
            tabPage_TrackRating.TabIndex = 2;
            tabPage_TrackRating.Tag = "";
            tabPage_TrackRating.Text = "Track Rating";
            tabPage_TrackRating.UseVisualStyleBackColor = true;
            // 
            // gb_StarRating
            // 
            gb_StarRating.Controls.Add(lbl_TwoStar);
            gb_StarRating.Controls.Add(nud_TwoStarMinScore);
            gb_StarRating.Controls.Add(lbl_ThreeStar);
            gb_StarRating.Controls.Add(lbl_FourStar);
            gb_StarRating.Controls.Add(lbl_FiveStar);
            gb_StarRating.Controls.Add(nud_ThreeStarMinScore);
            gb_StarRating.Controls.Add(nud_FourStarMinScore);
            gb_StarRating.Controls.Add(nud_FiveStarMinScore);
            gb_StarRating.Location = new Point(451, 6);
            gb_StarRating.Name = "gb_StarRating";
            gb_StarRating.Size = new Size(197, 141);
            gb_StarRating.TabIndex = 11;
            gb_StarRating.TabStop = false;
            gb_StarRating.Text = "Star Rating";
            // 
            // lbl_TwoStar
            // 
            lbl_TwoStar.AutoSize = true;
            lbl_TwoStar.BorderColor = Color.Gray;
            lbl_TwoStar.Location = new Point(6, 106);
            lbl_TwoStar.Name = "lbl_TwoStar";
            lbl_TwoStar.Size = new Size(111, 15);
            lbl_TwoStar.TabIndex = 24;
            lbl_TwoStar.Text = "Two Star Min Score:";
            // 
            // nud_TwoStarMinScore
            // 
            nud_TwoStarMinScore.Location = new Point(131, 101);
            nud_TwoStarMinScore.Name = "nud_TwoStarMinScore";
            nud_TwoStarMinScore.Padding = new Padding(1);
            nud_TwoStarMinScore.Size = new Size(56, 23);
            nud_TwoStarMinScore.TabIndex = 23;
            nud_TwoStarMinScore.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lbl_ThreeStar
            // 
            lbl_ThreeStar.AutoSize = true;
            lbl_ThreeStar.BorderColor = Color.Gray;
            lbl_ThreeStar.Location = new Point(6, 77);
            lbl_ThreeStar.Name = "lbl_ThreeStar";
            lbl_ThreeStar.Size = new Size(119, 15);
            lbl_ThreeStar.TabIndex = 22;
            lbl_ThreeStar.Text = "Three Star Min Score:";
            // 
            // lbl_FourStar
            // 
            lbl_FourStar.AutoSize = true;
            lbl_FourStar.BorderColor = Color.Gray;
            lbl_FourStar.Location = new Point(6, 49);
            lbl_FourStar.Name = "lbl_FourStar";
            lbl_FourStar.Size = new Size(113, 15);
            lbl_FourStar.TabIndex = 21;
            lbl_FourStar.Text = "Four Star Min Score:";
            // 
            // lbl_FiveStar
            // 
            lbl_FiveStar.AutoSize = true;
            lbl_FiveStar.BorderColor = Color.Gray;
            lbl_FiveStar.Location = new Point(6, 19);
            lbl_FiveStar.Name = "lbl_FiveStar";
            lbl_FiveStar.Size = new Size(110, 15);
            lbl_FiveStar.TabIndex = 20;
            lbl_FiveStar.Text = "Five Star Min Score:";
            // 
            // nud_ThreeStarMinScore
            // 
            nud_ThreeStarMinScore.Location = new Point(131, 72);
            nud_ThreeStarMinScore.Name = "nud_ThreeStarMinScore";
            nud_ThreeStarMinScore.Padding = new Padding(1);
            nud_ThreeStarMinScore.Size = new Size(56, 23);
            nud_ThreeStarMinScore.TabIndex = 19;
            nud_ThreeStarMinScore.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_FourStarMinScore
            // 
            nud_FourStarMinScore.Location = new Point(131, 43);
            nud_FourStarMinScore.Name = "nud_FourStarMinScore";
            nud_FourStarMinScore.Padding = new Padding(1);
            nud_FourStarMinScore.Size = new Size(56, 23);
            nud_FourStarMinScore.TabIndex = 18;
            nud_FourStarMinScore.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_FiveStarMinScore
            // 
            nud_FiveStarMinScore.Location = new Point(131, 14);
            nud_FiveStarMinScore.Name = "nud_FiveStarMinScore";
            nud_FiveStarMinScore.Padding = new Padding(1);
            nud_FiveStarMinScore.Size = new Size(56, 23);
            nud_FiveStarMinScore.TabIndex = 17;
            nud_FiveStarMinScore.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // gb_Thresholds
            // 
            gb_Thresholds.Controls.Add(lbl_LeadOutImmunity);
            gb_Thresholds.Controls.Add(lbl_LeadInImmunity);
            gb_Thresholds.Controls.Add(nud_LeadOutImmunity);
            gb_Thresholds.Controls.Add(nud_LeadInImmunity);
            gb_Thresholds.Controls.Add(lbl_Val_EarlySkipThreshold);
            gb_Thresholds.Controls.Add(lbl_EarlySkipThreshold);
            gb_Thresholds.Controls.Add(tb_EarlySkipThresholdPercent);
            gb_Thresholds.Location = new Point(8, 143);
            gb_Thresholds.Name = "gb_Thresholds";
            gb_Thresholds.Size = new Size(293, 105);
            gb_Thresholds.TabIndex = 10;
            gb_Thresholds.TabStop = false;
            gb_Thresholds.Text = "Thresholds";
            // 
            // lbl_LeadOutImmunity
            // 
            lbl_LeadOutImmunity.AutoSize = true;
            lbl_LeadOutImmunity.BorderColor = Color.Gray;
            lbl_LeadOutImmunity.Location = new Point(8, 75);
            lbl_LeadOutImmunity.Name = "lbl_LeadOutImmunity";
            lbl_LeadOutImmunity.Size = new Size(113, 15);
            lbl_LeadOutImmunity.TabIndex = 36;
            lbl_LeadOutImmunity.Text = "Lead Out Immunity:";
            // 
            // lbl_LeadInImmunity
            // 
            lbl_LeadInImmunity.AutoSize = true;
            lbl_LeadInImmunity.BorderColor = Color.Gray;
            lbl_LeadInImmunity.Location = new Point(8, 48);
            lbl_LeadInImmunity.Name = "lbl_LeadInImmunity";
            lbl_LeadInImmunity.Size = new Size(103, 15);
            lbl_LeadInImmunity.TabIndex = 35;
            lbl_LeadInImmunity.Text = "Lead In Immunity:";
            // 
            // nud_LeadOutImmunity
            // 
            nud_LeadOutImmunity.Location = new Point(129, 71);
            nud_LeadOutImmunity.Name = "nud_LeadOutImmunity";
            nud_LeadOutImmunity.Padding = new Padding(1);
            nud_LeadOutImmunity.Size = new Size(56, 23);
            nud_LeadOutImmunity.TabIndex = 34;
            nud_LeadOutImmunity.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_LeadInImmunity
            // 
            nud_LeadInImmunity.Location = new Point(129, 42);
            nud_LeadInImmunity.Name = "nud_LeadInImmunity";
            nud_LeadInImmunity.Padding = new Padding(1);
            nud_LeadInImmunity.Size = new Size(56, 23);
            nud_LeadInImmunity.TabIndex = 33;
            nud_LeadInImmunity.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lbl_Val_EarlySkipThreshold
            // 
            lbl_Val_EarlySkipThreshold.AutoSize = true;
            lbl_Val_EarlySkipThreshold.BorderColor = Color.Gray;
            lbl_Val_EarlySkipThreshold.Location = new Point(72, 23);
            lbl_Val_EarlySkipThreshold.Name = "lbl_Val_EarlySkipThreshold";
            lbl_Val_EarlySkipThreshold.Size = new Size(38, 15);
            lbl_Val_EarlySkipThreshold.TabIndex = 32;
            lbl_Val_EarlySkipThreshold.Text = "0.00%";
            // 
            // lbl_EarlySkipThreshold
            // 
            lbl_EarlySkipThreshold.AutoSize = true;
            lbl_EarlySkipThreshold.BorderColor = Color.Gray;
            lbl_EarlySkipThreshold.Location = new Point(8, 24);
            lbl_EarlySkipThreshold.Name = "lbl_EarlySkipThreshold";
            lbl_EarlySkipThreshold.Size = new Size(60, 15);
            lbl_EarlySkipThreshold.TabIndex = 29;
            lbl_EarlySkipThreshold.Text = "Early Skip:";
            // 
            // tb_EarlySkipThresholdPercent
            // 
            tb_EarlySkipThresholdPercent.BorderColor = SystemColors.ActiveBorder;
            tb_EarlySkipThresholdPercent.EnableBorder = false;
            tb_EarlySkipThresholdPercent.Location = new Point(116, 23);
            tb_EarlySkipThresholdPercent.Name = "tb_EarlySkipThresholdPercent";
            tb_EarlySkipThresholdPercent.RoundedThumb = false;
            tb_EarlySkipThresholdPercent.SelectedItemBackColor = SystemColors.Highlight;
            tb_EarlySkipThresholdPercent.SelectedItemForeColor = SystemColors.HighlightText;
            tb_EarlySkipThresholdPercent.Size = new Size(137, 16);
            tb_EarlySkipThresholdPercent.TabIndex = 16;
            tb_EarlySkipThresholdPercent.UseProgressFill = false;
            tb_EarlySkipThresholdPercent.Scroll += EarlySkip_Scroll;
            // 
            // gb_Rewards
            // 
            gb_Rewards.Controls.Add(lbl_Like);
            gb_Rewards.Controls.Add(lbl_Replay);
            gb_Rewards.Controls.Add(lbl_NoSkip);
            gb_Rewards.Controls.Add(nud_Like);
            gb_Rewards.Controls.Add(nud_Replay);
            gb_Rewards.Controls.Add(nud_NoSkip);
            gb_Rewards.Location = new Point(8, 6);
            gb_Rewards.Name = "gb_Rewards";
            gb_Rewards.Size = new Size(138, 131);
            gb_Rewards.TabIndex = 9;
            gb_Rewards.TabStop = false;
            gb_Rewards.Text = "Rewards";
            // 
            // lbl_Like
            // 
            lbl_Like.AutoSize = true;
            lbl_Like.BorderColor = Color.Gray;
            lbl_Like.Location = new Point(6, 80);
            lbl_Like.Name = "lbl_Like";
            lbl_Like.Size = new Size(31, 15);
            lbl_Like.TabIndex = 28;
            lbl_Like.Text = "Like:";
            // 
            // lbl_Replay
            // 
            lbl_Replay.AutoSize = true;
            lbl_Replay.BorderColor = Color.Gray;
            lbl_Replay.Location = new Point(6, 54);
            lbl_Replay.Name = "lbl_Replay";
            lbl_Replay.Size = new Size(45, 15);
            lbl_Replay.TabIndex = 27;
            lbl_Replay.Text = "Replay:";
            // 
            // lbl_NoSkip
            // 
            lbl_NoSkip.AutoSize = true;
            lbl_NoSkip.BorderColor = Color.Gray;
            lbl_NoSkip.Location = new Point(6, 27);
            lbl_NoSkip.Name = "lbl_NoSkip";
            lbl_NoSkip.Size = new Size(51, 15);
            lbl_NoSkip.TabIndex = 26;
            lbl_NoSkip.Text = "No Skip:";
            // 
            // nud_Like
            // 
            nud_Like.Location = new Point(63, 77);
            nud_Like.Name = "nud_Like";
            nud_Like.Padding = new Padding(1);
            nud_Like.Size = new Size(56, 23);
            nud_Like.TabIndex = 25;
            nud_Like.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_Replay
            // 
            nud_Replay.Location = new Point(63, 48);
            nud_Replay.Name = "nud_Replay";
            nud_Replay.Padding = new Padding(1);
            nud_Replay.Size = new Size(56, 23);
            nud_Replay.TabIndex = 24;
            nud_Replay.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_NoSkip
            // 
            nud_NoSkip.Location = new Point(63, 19);
            nud_NoSkip.Name = "nud_NoSkip";
            nud_NoSkip.Padding = new Padding(1);
            nud_NoSkip.Size = new Size(56, 23);
            nud_NoSkip.TabIndex = 23;
            nud_NoSkip.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // gb_MonthlyDecay
            // 
            gb_MonthlyDecay.Controls.Add(lbl_MonthlDecay);
            gb_MonthlyDecay.Controls.Add(tb_MonthlyDecay);
            gb_MonthlyDecay.Location = new Point(451, 153);
            gb_MonthlyDecay.Name = "gb_MonthlyDecay";
            gb_MonthlyDecay.Size = new Size(200, 50);
            gb_MonthlyDecay.TabIndex = 8;
            gb_MonthlyDecay.TabStop = false;
            gb_MonthlyDecay.Text = "Montly Decay";
            // 
            // lbl_MonthlDecay
            // 
            lbl_MonthlDecay.AutoSize = true;
            lbl_MonthlDecay.BorderColor = Color.Gray;
            lbl_MonthlDecay.Location = new Point(6, 23);
            lbl_MonthlDecay.Name = "lbl_MonthlDecay";
            lbl_MonthlDecay.Size = new Size(38, 15);
            lbl_MonthlDecay.TabIndex = 19;
            lbl_MonthlDecay.Text = "0.00%";
            // 
            // tb_MonthlyDecay
            // 
            tb_MonthlyDecay.BorderColor = SystemColors.ActiveBorder;
            tb_MonthlyDecay.EnableBorder = false;
            tb_MonthlyDecay.Location = new Point(50, 22);
            tb_MonthlyDecay.Name = "tb_MonthlyDecay";
            tb_MonthlyDecay.RoundedThumb = false;
            tb_MonthlyDecay.SelectedItemBackColor = SystemColors.Highlight;
            tb_MonthlyDecay.SelectedItemForeColor = SystemColors.HighlightText;
            tb_MonthlyDecay.Size = new Size(137, 16);
            tb_MonthlyDecay.TabIndex = 16;
            tb_MonthlyDecay.UseProgressFill = false;
            tb_MonthlyDecay.Scroll += MonthlyDecay_Scroll;
            // 
            // gb_Penalty
            // 
            gb_Penalty.Controls.Add(lbl_Dislike);
            gb_Penalty.Controls.Add(nud_Dislike);
            gb_Penalty.Controls.Add(lbl_SeekToEnd);
            gb_Penalty.Controls.Add(lbl_MidSkip);
            gb_Penalty.Controls.Add(lbl_EarlySkip);
            gb_Penalty.Controls.Add(nud_SeekToEnd);
            gb_Penalty.Controls.Add(nud_MidSkip);
            gb_Penalty.Controls.Add(nud_EarlySkip);
            gb_Penalty.Location = new Point(152, 6);
            gb_Penalty.Name = "gb_Penalty";
            gb_Penalty.Size = new Size(149, 130);
            gb_Penalty.TabIndex = 7;
            gb_Penalty.TabStop = false;
            gb_Penalty.Text = "Penalties";
            // 
            // lbl_Dislike
            // 
            lbl_Dislike.AutoSize = true;
            lbl_Dislike.BorderColor = Color.Gray;
            lbl_Dislike.Location = new Point(6, 101);
            lbl_Dislike.Name = "lbl_Dislike";
            lbl_Dislike.Size = new Size(44, 15);
            lbl_Dislike.TabIndex = 24;
            lbl_Dislike.Text = "Dislike:";
            // 
            // nud_Dislike
            // 
            nud_Dislike.Location = new Point(85, 98);
            nud_Dislike.Name = "nud_Dislike";
            nud_Dislike.Padding = new Padding(1);
            nud_Dislike.Size = new Size(56, 23);
            nud_Dislike.TabIndex = 23;
            nud_Dislike.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lbl_SeekToEnd
            // 
            lbl_SeekToEnd.AutoSize = true;
            lbl_SeekToEnd.BorderColor = Color.Gray;
            lbl_SeekToEnd.Location = new Point(6, 72);
            lbl_SeekToEnd.Name = "lbl_SeekToEnd";
            lbl_SeekToEnd.Size = new Size(73, 15);
            lbl_SeekToEnd.TabIndex = 22;
            lbl_SeekToEnd.Text = "Seek To End:";
            // 
            // lbl_MidSkip
            // 
            lbl_MidSkip.AutoSize = true;
            lbl_MidSkip.BorderColor = Color.Gray;
            lbl_MidSkip.Location = new Point(6, 46);
            lbl_MidSkip.Name = "lbl_MidSkip";
            lbl_MidSkip.Size = new Size(56, 15);
            lbl_MidSkip.TabIndex = 21;
            lbl_MidSkip.Text = "Mid Skip:";
            // 
            // lbl_EarlySkip
            // 
            lbl_EarlySkip.AutoSize = true;
            lbl_EarlySkip.BorderColor = Color.Gray;
            lbl_EarlySkip.Location = new Point(6, 19);
            lbl_EarlySkip.Name = "lbl_EarlySkip";
            lbl_EarlySkip.Size = new Size(60, 15);
            lbl_EarlySkip.TabIndex = 20;
            lbl_EarlySkip.Text = "Early Skip:";
            // 
            // nud_SeekToEnd
            // 
            nud_SeekToEnd.Location = new Point(85, 69);
            nud_SeekToEnd.Name = "nud_SeekToEnd";
            nud_SeekToEnd.Padding = new Padding(1);
            nud_SeekToEnd.Size = new Size(56, 23);
            nud_SeekToEnd.TabIndex = 19;
            nud_SeekToEnd.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_MidSkip
            // 
            nud_MidSkip.Location = new Point(85, 40);
            nud_MidSkip.Name = "nud_MidSkip";
            nud_MidSkip.Padding = new Padding(1);
            nud_MidSkip.Size = new Size(56, 23);
            nud_MidSkip.TabIndex = 18;
            nud_MidSkip.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // nud_EarlySkip
            // 
            nud_EarlySkip.Location = new Point(85, 11);
            nud_EarlySkip.Name = "nud_EarlySkip";
            nud_EarlySkip.Padding = new Padding(1);
            nud_EarlySkip.Size = new Size(56, 23);
            nud_EarlySkip.TabIndex = 17;
            nud_EarlySkip.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // tabPage_Database_Shuffle
            // 
            tabPage_Database_Shuffle.BackColor = SystemColors.Control;
            tabPage_Database_Shuffle.Controls.Add(gb_Database);
            tabPage_Database_Shuffle.Controls.Add(gb_Smart_Shuffle);
            tabPage_Database_Shuffle.Location = new Point(4, 5);
            tabPage_Database_Shuffle.Name = "tabPage_Database_Shuffle";
            tabPage_Database_Shuffle.Padding = new Padding(3);
            tabPage_Database_Shuffle.Size = new Size(671, 272);
            tabPage_Database_Shuffle.TabIndex = 1;
            tabPage_Database_Shuffle.Tag = "";
            tabPage_Database_Shuffle.Text = "Database  & Smart Shuffle";
            tabPage_Database_Shuffle.UseVisualStyleBackColor = true;
            // 
            // gb_Database
            // 
            gb_Database.Controls.Add(btn_Prune);
            gb_Database.Controls.Add(lbl_PruneDays);
            gb_Database.Controls.Add(btn_ImportDB);
            gb_Database.Controls.Add(btn_ResetDB);
            gb_Database.Controls.Add(nud_PruneDays);
            gb_Database.Controls.Add(chk_ArhiveInstead);
            gb_Database.Controls.Add(chk_EnablePrune);
            gb_Database.Location = new Point(6, 6);
            gb_Database.Name = "gb_Database";
            gb_Database.Size = new Size(261, 155);
            gb_Database.TabIndex = 1;
            gb_Database.TabStop = false;
            gb_Database.Text = "Database Management";
            // 
            // btn_Prune
            // 
            btn_Prune.FlatAppearance.BorderSize = 0;
            btn_Prune.Location = new Point(168, 113);
            btn_Prune.Name = "btn_Prune";
            btn_Prune.Size = new Size(75, 23);
            btn_Prune.TabIndex = 13;
            btn_Prune.Text = "Prune Now";
            btn_Prune.UseVisualStyleBackColor = true;
            btn_Prune.Click += PruneBtn_Click;
            // 
            // lbl_PruneDays
            // 
            lbl_PruneDays.AutoSize = true;
            lbl_PruneDays.BorderColor = Color.Gray;
            lbl_PruneDays.Location = new Point(68, 81);
            lbl_PruneDays.Name = "lbl_PruneDays";
            lbl_PruneDays.Size = new Size(103, 15);
            lbl_PruneDays.TabIndex = 12;
            lbl_PruneDays.Text = "Days old to prune:";
            // 
            // btn_ImportDB
            // 
            btn_ImportDB.FlatAppearance.BorderSize = 0;
            btn_ImportDB.Location = new Point(87, 113);
            btn_ImportDB.Name = "btn_ImportDB";
            btn_ImportDB.Size = new Size(75, 23);
            btn_ImportDB.TabIndex = 11;
            btn_ImportDB.Text = "Import DB";
            btn_ImportDB.UseVisualStyleBackColor = true;
            btn_ImportDB.Click += MigrateDB_Click;
            // 
            // btn_ResetDB
            // 
            btn_ResetDB.FlatAppearance.BorderSize = 0;
            btn_ResetDB.Location = new Point(6, 113);
            btn_ResetDB.Name = "btn_ResetDB";
            btn_ResetDB.Size = new Size(75, 23);
            btn_ResetDB.TabIndex = 10;
            btn_ResetDB.Text = "Reset DB";
            btn_ResetDB.UseVisualStyleBackColor = true;
            btn_ResetDB.Click += ResetDB_Click;
            // 
            // nud_PruneDays
            // 
            nud_PruneDays.Location = new Point(6, 77);
            nud_PruneDays.Maximum = 366;
            nud_PruneDays.Name = "nud_PruneDays";
            nud_PruneDays.Padding = new Padding(1);
            nud_PruneDays.Size = new Size(56, 23);
            nud_PruneDays.TabIndex = 7;
            nud_PruneDays.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // chk_ArhiveInstead
            // 
            chk_ArhiveInstead.AccentColor = SystemColors.Highlight;
            chk_ArhiveInstead.AutoSize = true;
            chk_ArhiveInstead.BorderColor = SystemColors.ControlDark;
            chk_ArhiveInstead.Location = new Point(6, 52);
            chk_ArhiveInstead.Name = "chk_ArhiveInstead";
            chk_ArhiveInstead.Size = new Size(130, 19);
            chk_ArhiveInstead.TabIndex = 1;
            chk_ArhiveInstead.Text = "Archive Over Delete";
            chk_ArhiveInstead.UseVisualStyleBackColor = true;
            // 
            // chk_EnablePrune
            // 
            chk_EnablePrune.AccentColor = SystemColors.Highlight;
            chk_EnablePrune.AutoSize = true;
            chk_EnablePrune.BorderColor = SystemColors.ControlDark;
            chk_EnablePrune.Location = new Point(6, 27);
            chk_EnablePrune.Name = "chk_EnablePrune";
            chk_EnablePrune.Size = new Size(109, 19);
            chk_EnablePrune.TabIndex = 0;
            chk_EnablePrune.Text = "Enable Pruining";
            chk_EnablePrune.UseVisualStyleBackColor = true;
            // 
            // gb_Smart_Shuffle
            // 
            gb_Smart_Shuffle.Controls.Add(lbl_Stacklimit);
            gb_Smart_Shuffle.Controls.Add(nud_StackLimit);
            gb_Smart_Shuffle.Controls.Add(lbl_UserRated);
            gb_Smart_Shuffle.Controls.Add(lbl_Skipped);
            gb_Smart_Shuffle.Controls.Add(lbl_Played);
            gb_Smart_Shuffle.Controls.Add(lbl_UnPlayed);
            gb_Smart_Shuffle.Controls.Add(tb_UserRatedWeight);
            gb_Smart_Shuffle.Controls.Add(tb_SkippedWeight);
            gb_Smart_Shuffle.Controls.Add(tb_PlayedWeight);
            gb_Smart_Shuffle.Controls.Add(tb_UnplayedWeight);
            gb_Smart_Shuffle.Controls.Add(cb_Shuffle_Modes);
            gb_Smart_Shuffle.Controls.Add(lbl_Shuffle);
            gb_Smart_Shuffle.Location = new Point(393, 6);
            gb_Smart_Shuffle.Name = "gb_Smart_Shuffle";
            gb_Smart_Shuffle.Size = new Size(272, 187);
            gb_Smart_Shuffle.TabIndex = 0;
            gb_Smart_Shuffle.TabStop = false;
            gb_Smart_Shuffle.Text = "Smart Shuffle";
            // 
            // lbl_Stacklimit
            // 
            lbl_Stacklimit.AutoSize = true;
            lbl_Stacklimit.BorderColor = Color.Gray;
            lbl_Stacklimit.Location = new Point(14, 148);
            lbl_Stacklimit.Name = "lbl_Stacklimit";
            lbl_Stacklimit.Size = new Size(109, 15);
            lbl_Stacklimit.TabIndex = 13;
            lbl_Stacklimit.Text = "Track History Limit:";
            // 
            // nud_StackLimit
            // 
            nud_StackLimit.Location = new Point(129, 144);
            nud_StackLimit.Maximum = 9999;
            nud_StackLimit.Name = "nud_StackLimit";
            nud_StackLimit.Padding = new Padding(1);
            nud_StackLimit.Size = new Size(56, 23);
            nud_StackLimit.TabIndex = 10;
            nud_StackLimit.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // lbl_UserRated
            // 
            lbl_UserRated.AutoSize = true;
            lbl_UserRated.BorderColor = Color.Gray;
            lbl_UserRated.Location = new Point(174, 121);
            lbl_UserRated.Name = "lbl_UserRated";
            lbl_UserRated.Size = new Size(95, 15);
            lbl_UserRated.TabIndex = 9;
            lbl_UserRated.Text = "0.00 - User Rated";
            // 
            // lbl_Skipped
            // 
            lbl_Skipped.AutoSize = true;
            lbl_Skipped.BorderColor = Color.Gray;
            lbl_Skipped.Location = new Point(174, 100);
            lbl_Skipped.Name = "lbl_Skipped";
            lbl_Skipped.Size = new Size(81, 15);
            lbl_Skipped.TabIndex = 8;
            lbl_Skipped.Text = "0.00 - Skipped";
            // 
            // lbl_Played
            // 
            lbl_Played.AutoSize = true;
            lbl_Played.BorderColor = Color.Gray;
            lbl_Played.Location = new Point(174, 77);
            lbl_Played.Name = "lbl_Played";
            lbl_Played.Size = new Size(74, 15);
            lbl_Played.TabIndex = 7;
            lbl_Played.Text = "0.00 - Played";
            // 
            // lbl_UnPlayed
            // 
            lbl_UnPlayed.AutoSize = true;
            lbl_UnPlayed.BorderColor = Color.Gray;
            lbl_UnPlayed.Location = new Point(174, 56);
            lbl_UnPlayed.Name = "lbl_UnPlayed";
            lbl_UnPlayed.Size = new Size(89, 15);
            lbl_UnPlayed.TabIndex = 6;
            lbl_UnPlayed.Text = "0.00 - Unplayed";
            // 
            // tb_UserRatedWeight
            // 
            tb_UserRatedWeight.BorderColor = SystemColors.ActiveBorder;
            tb_UserRatedWeight.EnableBorder = false;
            tb_UserRatedWeight.Location = new Point(14, 122);
            tb_UserRatedWeight.Name = "tb_UserRatedWeight";
            tb_UserRatedWeight.RoundedThumb = false;
            tb_UserRatedWeight.SelectedItemBackColor = SystemColors.Highlight;
            tb_UserRatedWeight.SelectedItemForeColor = SystemColors.HighlightText;
            tb_UserRatedWeight.Size = new Size(150, 16);
            tb_UserRatedWeight.TabIndex = 5;
            tb_UserRatedWeight.UseProgressFill = false;
            tb_UserRatedWeight.Scroll += UserRatedTrackBar_Scroll;
            // 
            // tb_SkippedWeight
            // 
            tb_SkippedWeight.BorderColor = SystemColors.ActiveBorder;
            tb_SkippedWeight.EnableBorder = false;
            tb_SkippedWeight.Location = new Point(14, 100);
            tb_SkippedWeight.Name = "tb_SkippedWeight";
            tb_SkippedWeight.RoundedThumb = false;
            tb_SkippedWeight.SelectedItemBackColor = SystemColors.Highlight;
            tb_SkippedWeight.SelectedItemForeColor = SystemColors.HighlightText;
            tb_SkippedWeight.Size = new Size(150, 16);
            tb_SkippedWeight.TabIndex = 4;
            tb_SkippedWeight.UseProgressFill = false;
            tb_SkippedWeight.Scroll += SkippedTrackBar_Scroll;
            // 
            // tb_PlayedWeight
            // 
            tb_PlayedWeight.BorderColor = SystemColors.ActiveBorder;
            tb_PlayedWeight.EnableBorder = false;
            tb_PlayedWeight.Location = new Point(14, 78);
            tb_PlayedWeight.Name = "tb_PlayedWeight";
            tb_PlayedWeight.RoundedThumb = false;
            tb_PlayedWeight.SelectedItemBackColor = SystemColors.Highlight;
            tb_PlayedWeight.SelectedItemForeColor = SystemColors.HighlightText;
            tb_PlayedWeight.Size = new Size(150, 16);
            tb_PlayedWeight.TabIndex = 3;
            tb_PlayedWeight.UseProgressFill = false;
            tb_PlayedWeight.Scroll += PlayedTrackBar_Scroll;
            // 
            // tb_UnplayedWeight
            // 
            tb_UnplayedWeight.BorderColor = SystemColors.ActiveBorder;
            tb_UnplayedWeight.EnableBorder = false;
            tb_UnplayedWeight.Location = new Point(14, 56);
            tb_UnplayedWeight.Name = "tb_UnplayedWeight";
            tb_UnplayedWeight.RoundedThumb = false;
            tb_UnplayedWeight.SelectedItemBackColor = SystemColors.Highlight;
            tb_UnplayedWeight.SelectedItemForeColor = SystemColors.HighlightText;
            tb_UnplayedWeight.Size = new Size(150, 16);
            tb_UnplayedWeight.TabIndex = 2;
            tb_UnplayedWeight.UseProgressFill = false;
            tb_UnplayedWeight.Scroll += UnplayedTrackBar_Scroll;
            // 
            // cb_Shuffle_Modes
            // 
            cb_Shuffle_Modes.BorderColor = SystemColors.ActiveBorder;
            cb_Shuffle_Modes.BufferHeight = 5;
            cb_Shuffle_Modes.Location = new Point(14, 27);
            cb_Shuffle_Modes.Name = "cb_Shuffle_Modes";
            cb_Shuffle_Modes.SelectedItemBackColor = SystemColors.Highlight;
            cb_Shuffle_Modes.SelectedItemForeColor = SystemColors.HighlightText;
            cb_Shuffle_Modes.Size = new Size(150, 23);
            cb_Shuffle_Modes.TabIndex = 1;
            // 
            // lbl_Shuffle
            // 
            lbl_Shuffle.AutoSize = true;
            lbl_Shuffle.BorderColor = Color.Gray;
            lbl_Shuffle.Location = new Point(172, 31);
            lbl_Shuffle.Name = "lbl_Shuffle";
            lbl_Shuffle.Size = new Size(81, 15);
            lbl_Shuffle.TabIndex = 0;
            lbl_Shuffle.Text = "Shuffle Mode:";
            // 
            // btn_Apply
            // 
            btn_Apply.CornerRadius = 3;
            btn_Apply.FlatAppearance.BorderSize = 0;
            btn_Apply.Location = new Point(583, 297);
            btn_Apply.Name = "btn_Apply";
            btn_Apply.Size = new Size(75, 23);
            btn_Apply.TabIndex = 13;
            btn_Apply.Text = "Apply";
            btn_Apply.UseVisualStyleBackColor = true;
            btn_Apply.Click += Apply_Click;
            // 
            // tabControlHeader
            // 
            tabControlHeader.AutoSize = true;
            tabControlHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tabControlHeader.ButtonCornerRadius = 5;
            tabControlHeader.Dock = DockStyle.Top;
            tabControlHeader.LinkedTabControlName = "tabControlBase";
            tabControlHeader.Location = new Point(0, 0);
            tabControlHeader.Name = "tabControlHeader";
            tabControlHeader.Padding = new Padding(5);
            tabControlHeader.Size = new Size(679, 43);
            tabControlHeader.TabIndex = 14;
            tabControlHeader.WrapContents = false;
            // 
            // btn_ResetToDefault
            // 
            btn_ResetToDefault.CornerRadius = 3;
            btn_ResetToDefault.FlatAppearance.BorderSize = 0;
            btn_ResetToDefault.Location = new Point(499, 297);
            btn_ResetToDefault.Name = "btn_ResetToDefault";
            btn_ResetToDefault.Size = new Size(75, 23);
            btn_ResetToDefault.TabIndex = 15;
            btn_ResetToDefault.Text = "Reset";
            btn_ResetToDefault.UseVisualStyleBackColor = true;
            btn_ResetToDefault.Click += ResetButton_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(679, 330);
            Controls.Add(btn_ResetToDefault);
            Controls.Add(tabControlHeader);
            Controls.Add(btn_Apply);
            Controls.Add(tabControlBase);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            gb_WebSocketServer.ResumeLayout(false);
            gb_WebSocketServer.PerformLayout();
            tabControlBase.ResumeLayout(false);
            tabPage_Theme_Websocket.ResumeLayout(false);
            tabPage_TrackRating.ResumeLayout(false);
            gb_StarRating.ResumeLayout(false);
            gb_StarRating.PerformLayout();
            gb_Thresholds.ResumeLayout(false);
            gb_Thresholds.PerformLayout();
            gb_Rewards.ResumeLayout(false);
            gb_Rewards.PerformLayout();
            gb_MonthlyDecay.ResumeLayout(false);
            gb_MonthlyDecay.PerformLayout();
            gb_Penalty.ResumeLayout(false);
            gb_Penalty.PerformLayout();
            tabPage_Database_Shuffle.ResumeLayout(false);
            gb_Database.ResumeLayout(false);
            gb_Database.PerformLayout();
            gb_Smart_Shuffle.ResumeLayout(false);
            gb_Smart_Shuffle.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private BazthalLib.Controls.ThemeColorsSetter ThemeSetter;
        private BazthalLib.Controls.ThemableGroupBox gb_WebSocketServer;
        private BazthalLib.Controls.ThemableLabel lbl_WSS_Address;
        private BazthalLib.Controls.ThemableLabel lbl_WSS_Endpoint;
        private BazthalLib.Controls.ThemableLabel lbl_WSS_Port;
        private BazthalLib.Controls.ThemableButton btn_WSS_Stop;
        private BazthalLib.Controls.ThemableButton btn_WSS_Start;
        private BazthalLib.Controls.ThemableCheckBox cb_WSS_Auto_Start;
        private BazthalLib.Controls.ThemableTextBox tb_WSS_Endpoint;
        private BazthalLib.Controls.ThemableTextBox tb_WSS_Port;
        private BazthalLib.Controls.ThemableTextBox tb_WSS_Address;
        private BazthalLib.Controls.ThemableTabControlBase tabControlBase;
        private TabPage tabPage_Theme_Websocket;
        private TabPage tabPage_Database_Shuffle;
        private BazthalLib.Controls.ThemableButton btn_Apply;
        private BazthalLib.Controls.ThemableTabControlHeader tabControlHeader;
        private BazthalLib.Controls.ThemableButton btn_ResetDB;
        private BazthalLib.Controls.ThemableGroupBox gb_Smart_Shuffle;
        private BazthalLib.Controls.ThemableLabel lbl_Shuffle;
        private BazthalLib.Controls.ThemableComboBox cb_Shuffle_Modes;
        private BazthalLib.Controls.ThemableGroupBox gb_Database;
        private BazthalLib.Controls.ThemableCheckBox chk_ArhiveInstead;
        private BazthalLib.Controls.ThemableCheckBox chk_EnablePrune;
        private BazthalLib.Controls.ThemableTrackBar tb_UserRatedWeight;
        private BazthalLib.Controls.ThemableTrackBar tb_SkippedWeight;
        private BazthalLib.Controls.ThemableTrackBar tb_PlayedWeight;
        private BazthalLib.Controls.ThemableTrackBar tb_UnplayedWeight;
        private BazthalLib.Controls.ThemableNumericUpDown nud_PruneDays;
        private BazthalLib.Controls.ThemableButton btn_ImportDB;
        private TabPage tabPage_TrackRating;
        private BazthalLib.Controls.ThemableLabel lbl_PruneDays;
        private BazthalLib.Controls.ThemableTrackBar themableTrackBar5;
        private BazthalLib.Controls.ThemableTextBox themableTextBox5;
        private BazthalLib.Controls.ThemableComboBox themableComboBox1;
        private BazthalLib.Controls.ThemableCheckBox themableCheckBox3;
        private BazthalLib.Controls.ThemableGroupBox themableGroupBox2;
        private BazthalLib.Controls.ThemableLabel lbl_UserRated;
        private BazthalLib.Controls.ThemableLabel lbl_Skipped;
        private BazthalLib.Controls.ThemableLabel lbl_Played;
        private BazthalLib.Controls.ThemableLabel lbl_UnPlayed;
        private BazthalLib.Controls.ThemableButton btn_Prune;
        private BazthalLib.Controls.ThemableNumericUpDown nud_StackLimit;
        private BazthalLib.Controls.ThemableLabel lbl_Stacklimit;
        private BazthalLib.Controls.ThemableGroupBox gb_StarRating;
        private BazthalLib.Controls.ThemableLabel lbl_TwoStar;
        private BazthalLib.Controls.ThemableNumericUpDown nud_TwoStarMinScore;
        private BazthalLib.Controls.ThemableLabel lbl_ThreeStar;
        private BazthalLib.Controls.ThemableLabel lbl_FourStar;
        private BazthalLib.Controls.ThemableLabel lbl_FiveStar;
        private BazthalLib.Controls.ThemableNumericUpDown nud_ThreeStarMinScore;
        private BazthalLib.Controls.ThemableNumericUpDown nud_FourStarMinScore;
        private BazthalLib.Controls.ThemableNumericUpDown nud_FiveStarMinScore;
        private BazthalLib.Controls.ThemableGroupBox gb_Thresholds;
        private BazthalLib.Controls.ThemableLabel lbl_LeadOutImmunity;
        private BazthalLib.Controls.ThemableLabel lbl_LeadInImmunity;
        private BazthalLib.Controls.ThemableNumericUpDown nud_LeadOutImmunity;
        private BazthalLib.Controls.ThemableNumericUpDown nud_LeadInImmunity;
        private BazthalLib.Controls.ThemableLabel lbl_Val_EarlySkipThreshold;
        private BazthalLib.Controls.ThemableLabel lbl_EarlySkipThreshold;
        private BazthalLib.Controls.ThemableTrackBar tb_EarlySkipThresholdPercent;
        private BazthalLib.Controls.ThemableGroupBox gb_Rewards;
        private BazthalLib.Controls.ThemableLabel lbl_Like;
        private BazthalLib.Controls.ThemableLabel lbl_Replay;
        private BazthalLib.Controls.ThemableLabel lbl_NoSkip;
        private BazthalLib.Controls.ThemableNumericUpDown nud_Like;
        private BazthalLib.Controls.ThemableNumericUpDown nud_Replay;
        private BazthalLib.Controls.ThemableNumericUpDown nud_NoSkip;
        private BazthalLib.Controls.ThemableGroupBox gb_MonthlyDecay;
        private BazthalLib.Controls.ThemableLabel lbl_MonthlDecay;
        private BazthalLib.Controls.ThemableTrackBar tb_MonthlyDecay;
        private BazthalLib.Controls.ThemableGroupBox gb_Penalty;
        private BazthalLib.Controls.ThemableLabel lbl_Dislike;
        private BazthalLib.Controls.ThemableNumericUpDown nud_Dislike;
        private BazthalLib.Controls.ThemableLabel lbl_SeekToEnd;
        private BazthalLib.Controls.ThemableLabel lbl_MidSkip;
        private BazthalLib.Controls.ThemableLabel lbl_EarlySkip;
        private BazthalLib.Controls.ThemableNumericUpDown nud_SeekToEnd;
        private BazthalLib.Controls.ThemableNumericUpDown nud_MidSkip;
        private BazthalLib.Controls.ThemableNumericUpDown nud_EarlySkip;
        private BazthalLib.Controls.ThemableButton btn_ResetToDefault;
    }
}