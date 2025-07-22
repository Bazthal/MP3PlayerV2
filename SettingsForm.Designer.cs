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
            themeColorsSetter1 = new BazthalLib.Controls.ThemeColorsSetter();
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
            btn_Apply = new BazthalLib.Controls.ThemableButton();
            gb_WebSocketServer.SuspendLayout();
            SuspendLayout();
            // 
            // themeColorsSetter1
            // 
            themeColorsSetter1.AutoLoadConfig = true;
            themeColorsSetter1.ConfigFilePath = "Config\\CustomTheme.json";
            themeColorsSetter1.Location = new Point(11, 13);
            themeColorsSetter1.MaximumSize = new Size(350, 250);
            themeColorsSetter1.MinimumSize = new Size(350, 250);
            themeColorsSetter1.Name = "themeColorsSetter1";
            themeColorsSetter1.Size = new Size(350, 250);
            themeColorsSetter1.TabIndex = 0;
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
            gb_WebSocketServer.Location = new Point(381, 13);
            gb_WebSocketServer.Name = "gb_WebSocketServer";
            gb_WebSocketServer.Size = new Size(269, 178);
            gb_WebSocketServer.TabIndex = 1;
            gb_WebSocketServer.TabStop = false;
            gb_WebSocketServer.Text = "Web Socket Server";
            // 
            // btn_WSS_Stop
            // 
            btn_WSS_Stop.FlatAppearance.BorderSize = 0;
            btn_WSS_Stop.Location = new Point(109, 141);
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
            btn_WSS_Start.Location = new Point(15, 141);
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
            cb_WSS_Auto_Start.Location = new Point(15, 116);
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
            // btn_Apply
            // 
            btn_Apply.CornerRadius = 3;
            btn_Apply.FlatAppearance.BorderSize = 0;
            btn_Apply.Location = new Point(575, 240);
            btn_Apply.Name = "btn_Apply";
            btn_Apply.Size = new Size(75, 23);
            btn_Apply.TabIndex = 10;
            btn_Apply.Text = "Apply";
            btn_Apply.UseVisualStyleBackColor = true;
            btn_Apply.Click += Apply_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(663, 275);
            Controls.Add(btn_Apply);
            Controls.Add(gb_WebSocketServer);
            Controls.Add(themeColorsSetter1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            gb_WebSocketServer.ResumeLayout(false);
            gb_WebSocketServer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private BazthalLib.Controls.ThemeColorsSetter themeColorsSetter1;
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
        private BazthalLib.Controls.ThemableButton btn_Apply;
    }
}