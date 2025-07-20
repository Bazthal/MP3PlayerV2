namespace MP3PlayerV2
{
    partial class MP3PlayerV2
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MP3PlayerV2));
            ctrls_Panel = new BazthalLib.Controls.ThemablePanel();
            Btn_Next = new BazthalLib.Controls.ThemableButton();
            Btn_Stop = new BazthalLib.Controls.ThemableButton();
            Btn_Pause = new BazthalLib.Controls.ThemableButton();
            Btn_Play = new BazthalLib.Controls.ThemableButton();
            Btn_Back = new BazthalLib.Controls.ThemableButton();
            Cur_Track_Label = new BazthalLib.Controls.ThemableLabel();
            slider_Panel = new BazthalLib.Controls.ThemablePanel();
            Volume_Slider = new BazthalLib.Controls.ThemableTrackBar();
            Tracking_Slider = new BazthalLib.Controls.ThemableTrackBar();
            playListBox = new BazthalLib.Controls.ThemableListBox();
            cms_Main = new ContextMenuStrip(components);
            cms_Add = new ToolStripMenuItem();
            cms_Remove = new ToolStripMenuItem();
            cms_Sort = new ToolStripMenuItem();
            cms_Sort_Artist_Asc = new ToolStripMenuItem();
            cms_Sort_Artist_Desc = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            cms_Sort_Title_Asc = new ToolStripMenuItem();
            cms_Sort_Title_Desc = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            cms_Sort_Play_Count_Asc = new ToolStripMenuItem();
            cms_Sort_Play_Count_Desc = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            cms_Sort_Last_Played_Asc = new ToolStripMenuItem();
            cms_Sort_Last_Played_Desc = new ToolStripMenuItem();
            cms_Shuffle = new ToolStripMenuItem();
            cms_Clear = new ToolStripMenuItem();
            cms_Load = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            cms_Save = new ToolStripMenuItem();
            cms_Exit = new ToolStripMenuItem();
            playlist_Panel = new BazthalLib.Controls.ThemablePanel();
            btn_Shuffle = new BazthalLib.Controls.ThemableButton();
            btn_Save = new BazthalLib.Controls.ThemableButton();
            btn_Open = new BazthalLib.Controls.ThemableButton();
            playList_Options = new BazthalLib.Controls.ThemableComboBox();
            btn_Remove = new BazthalLib.Controls.ThemableButton();
            btn_Add = new BazthalLib.Controls.ThemableButton();
            PlayTimer = new System.Windows.Forms.Timer(components);
            ts_Main = new BazthalLib.Controls.ThemableToolStrip();
            ts_Btn_Settings = new BazthalLib.Controls.ThemableToolStripButton();
            AudioDeviceList = new BazthalLib.Controls.ThemableToolStripComboBox();
            ctrls_Panel.SuspendLayout();
            slider_Panel.SuspendLayout();
            cms_Main.SuspendLayout();
            playlist_Panel.SuspendLayout();
            ts_Main.SuspendLayout();
            SuspendLayout();
            // 
            // ctrls_Panel
            // 
            ctrls_Panel.Controls.Add(Btn_Next);
            ctrls_Panel.Controls.Add(Btn_Stop);
            ctrls_Panel.Controls.Add(Btn_Pause);
            ctrls_Panel.Controls.Add(Btn_Play);
            ctrls_Panel.Controls.Add(Btn_Back);
            ctrls_Panel.Controls.Add(Cur_Track_Label);
            ctrls_Panel.Dock = DockStyle.Top;
            ctrls_Panel.Location = new Point(0, 29);
            ctrls_Panel.Name = "ctrls_Panel";
            ctrls_Panel.Size = new Size(363, 33);
            ctrls_Panel.TabIndex = 2;
            // 
            // Btn_Next
            // 
            Btn_Next.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_Next.EnableBorder = false;
            Btn_Next.FlatAppearance.BorderSize = 0;
            Btn_Next.FocusWrapAroundImage = true;
            Btn_Next.Location = new Point(329, 8);
            Btn_Next.Name = "Btn_Next";
            Btn_Next.RoundCorners = false;
            Btn_Next.Size = new Size(20, 16);
            Btn_Next.TabIndex = 5;
            Btn_Next.TintedImage = (Image)resources.GetObject("Btn_Next.TintedImage");
            Btn_Next.UseAccentForTintedImage = true;
            Btn_Next.UseVisualStyleBackColor = true;
            Btn_Next.Click += NextButton_Click;
            // 
            // Btn_Stop
            // 
            Btn_Stop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_Stop.EnableBorder = false;
            Btn_Stop.FlatAppearance.BorderSize = 0;
            Btn_Stop.FocusWrapAroundImage = true;
            Btn_Stop.Location = new Point(308, 8);
            Btn_Stop.Name = "Btn_Stop";
            Btn_Stop.RoundCorners = false;
            Btn_Stop.Size = new Size(17, 16);
            Btn_Stop.TabIndex = 4;
            Btn_Stop.Text = "Stop";
            Btn_Stop.TintedImage = (Image)resources.GetObject("Btn_Stop.TintedImage");
            Btn_Stop.UseAccentForTintedImage = true;
            Btn_Stop.UseVisualStyleBackColor = true;
            Btn_Stop.Click += StopButton_Click;
            // 
            // Btn_Pause
            // 
            Btn_Pause.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_Pause.EnableBorder = false;
            Btn_Pause.FlatAppearance.BorderSize = 0;
            Btn_Pause.FocusWrapAroundImage = true;
            Btn_Pause.Location = new Point(287, 8);
            Btn_Pause.Name = "Btn_Pause";
            Btn_Pause.RoundCorners = false;
            Btn_Pause.Size = new Size(17, 16);
            Btn_Pause.TabIndex = 3;
            Btn_Pause.Text = "Pause";
            Btn_Pause.TintedImage = (Image)resources.GetObject("Btn_Pause.TintedImage");
            Btn_Pause.UseAccentForTintedImage = true;
            Btn_Pause.UseVisualStyleBackColor = true;
            Btn_Pause.Click += PauseButton_Click;
            // 
            // Btn_Play
            // 
            Btn_Play.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_Play.EnableBorder = false;
            Btn_Play.FlatAppearance.BorderSize = 0;
            Btn_Play.FocusWrapAroundImage = true;
            Btn_Play.Location = new Point(266, 8);
            Btn_Play.Name = "Btn_Play";
            Btn_Play.RoundCorners = false;
            Btn_Play.Size = new Size(17, 16);
            Btn_Play.TabIndex = 2;
            Btn_Play.Text = "Play";
            Btn_Play.TintedImage = (Image)resources.GetObject("Btn_Play.TintedImage");
            Btn_Play.UseAccentForTintedImage = true;
            Btn_Play.UseVisualStyleBackColor = true;
            Btn_Play.Click += PlayButton_Click;
            // 
            // Btn_Back
            // 
            Btn_Back.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_Back.EnableBorder = false;
            Btn_Back.FlatAppearance.BorderSize = 0;
            Btn_Back.FocusWrapAroundImage = true;
            Btn_Back.Location = new Point(242, 8);
            Btn_Back.Name = "Btn_Back";
            Btn_Back.RoundCorners = false;
            Btn_Back.Size = new Size(20, 16);
            Btn_Back.TabIndex = 1;
            Btn_Back.Text = "Back";
            Btn_Back.TintedImage = (Image)resources.GetObject("Btn_Back.TintedImage");
            Btn_Back.UseAccentForTintedImage = true;
            Btn_Back.UseVisualStyleBackColor = true;
            Btn_Back.Click += PreviousButton_Click;
            // 
            // Cur_Track_Label
            // 
            Cur_Track_Label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Cur_Track_Label.BorderColor = Color.Gray;
            Cur_Track_Label.EnableBorder = true;
            Cur_Track_Label.Location = new Point(4, 4);
            Cur_Track_Label.Name = "Cur_Track_Label";
            Cur_Track_Label.Size = new Size(217, 25);
            Cur_Track_Label.TabIndex = 0;
            Cur_Track_Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // slider_Panel
            // 
            slider_Panel.Controls.Add(Volume_Slider);
            slider_Panel.Controls.Add(Tracking_Slider);
            slider_Panel.Dock = DockStyle.Top;
            slider_Panel.Location = new Point(0, 62);
            slider_Panel.Name = "slider_Panel";
            slider_Panel.Size = new Size(363, 20);
            slider_Panel.TabIndex = 3;
            // 
            // Volume_Slider
            // 
            Volume_Slider.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Volume_Slider.BorderColor = SystemColors.ActiveBorder;
            Volume_Slider.EnableBorder = false;
            Volume_Slider.Location = new Point(252, 2);
            Volume_Slider.Name = "Volume_Slider";
            Volume_Slider.RoundedThumb = false;
            Volume_Slider.SelectedItemBackColor = SystemColors.Highlight;
            Volume_Slider.SelectedItemForeColor = SystemColors.HighlightText;
            Volume_Slider.Size = new Size(105, 16);
            Volume_Slider.TabIndex = 5;
            Volume_Slider.ThumbSize = 10;
            Volume_Slider.UseProgressFill = false;
            Volume_Slider.Value = 1;
            Volume_Slider.ScrollCompleted += Volume_ScrollCompleted;
            // 
            // Tracking_Slider
            // 
            Tracking_Slider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Tracking_Slider.BorderColor = SystemColors.ActiveBorder;
            Tracking_Slider.EnableBorder = false;
            Tracking_Slider.Location = new Point(4, 2);
            Tracking_Slider.Name = "Tracking_Slider";
            Tracking_Slider.RoundedThumb = false;
            Tracking_Slider.SelectedItemBackColor = SystemColors.Highlight;
            Tracking_Slider.SelectedItemForeColor = SystemColors.HighlightText;
            Tracking_Slider.Size = new Size(242, 16);
            Tracking_Slider.TabIndex = 4;
            Tracking_Slider.ThumbSize = 10;
            Tracking_Slider.ScrollCompleted += Tracking_Slider_ScrollCompleted;
            // 
            // playListBox
            // 
            playListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            playListBox.BorderColor = SystemColors.ActiveBorder;
            playListBox.ContextMenuStrip = cms_Main;
            playListBox.EnableHorizontalScroll = false;
            playListBox.Font = new Font("Segoe UI", 9F);
            playListBox.ItemHeight = 15;
            playListBox.Location = new Point(5, 5);
            playListBox.Name = "playListBox";
            playListBox.SelectedItemBackColor = SystemColors.Highlight;
            playListBox.SelectedItemForeColor = SystemColors.HighlightText;
            playListBox.Size = new Size(353, 316);
            playListBox.TabIndex = 0;
            playListBox.DoubleClick += PlayList_DoubleClick;
            playListBox.KeyDown += PlayList_KeyDown;
            // 
            // cms_Main
            // 
            cms_Main.Items.AddRange(new ToolStripItem[] { cms_Add, cms_Remove, cms_Sort, cms_Shuffle, cms_Clear, cms_Load, toolStripSeparator1, cms_Save, cms_Exit });
            cms_Main.Name = "cms_Main";
            cms_Main.RenderMode = ToolStripRenderMode.System;
            cms_Main.Size = new Size(165, 186);
            // 
            // cms_Add
            // 
            cms_Add.Name = "cms_Add";
            cms_Add.Size = new Size(164, 22);
            cms_Add.Text = "Add Music";
            cms_Add.Click += AddButton_Click;
            // 
            // cms_Remove
            // 
            cms_Remove.Name = "cms_Remove";
            cms_Remove.Size = new Size(164, 22);
            cms_Remove.Text = "Remove Selected";
            cms_Remove.Click += RemoveButton_Click;
            // 
            // cms_Sort
            // 
            cms_Sort.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Artist_Asc, cms_Sort_Artist_Desc, toolStripSeparator2, cms_Sort_Title_Asc, cms_Sort_Title_Desc, toolStripSeparator3, cms_Sort_Play_Count_Asc, cms_Sort_Play_Count_Desc, toolStripSeparator4, cms_Sort_Last_Played_Asc, cms_Sort_Last_Played_Desc });
            cms_Sort.Name = "cms_Sort";
            cms_Sort.Size = new Size(164, 22);
            cms_Sort.Text = "Sort Playlist";
            // 
            // cms_Sort_Artist_Asc
            // 
            cms_Sort_Artist_Asc.Name = "cms_Sort_Artist_Asc";
            cms_Sort_Artist_Asc.Size = new Size(209, 22);
            cms_Sort_Artist_Asc.Tag = "Artist|false";
            cms_Sort_Artist_Asc.Text = "Artist - Ascending";
            cms_Sort_Artist_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Artist_Desc
            // 
            cms_Sort_Artist_Desc.Name = "cms_Sort_Artist_Desc";
            cms_Sort_Artist_Desc.Size = new Size(209, 22);
            cms_Sort_Artist_Desc.Tag = "Artist|true";
            cms_Sort_Artist_Desc.Text = "Artist - Descending";
            cms_Sort_Artist_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(206, 6);
            // 
            // cms_Sort_Title_Asc
            // 
            cms_Sort_Title_Asc.Name = "cms_Sort_Title_Asc";
            cms_Sort_Title_Asc.Size = new Size(209, 22);
            cms_Sort_Title_Asc.Tag = "Title|false";
            cms_Sort_Title_Asc.Text = "Title  - Ascending";
            cms_Sort_Title_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Title_Desc
            // 
            cms_Sort_Title_Desc.Name = "cms_Sort_Title_Desc";
            cms_Sort_Title_Desc.Size = new Size(209, 22);
            cms_Sort_Title_Desc.Tag = "Title|true";
            cms_Sort_Title_Desc.Text = "Title - Descending";
            cms_Sort_Title_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(206, 6);
            // 
            // cms_Sort_Play_Count_Asc
            // 
            cms_Sort_Play_Count_Asc.Name = "cms_Sort_Play_Count_Asc";
            cms_Sort_Play_Count_Asc.Size = new Size(209, 22);
            cms_Sort_Play_Count_Asc.Tag = "PlayCount|false";
            cms_Sort_Play_Count_Asc.Text = "Play Count - Ascending";
            cms_Sort_Play_Count_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Play_Count_Desc
            // 
            cms_Sort_Play_Count_Desc.Name = "cms_Sort_Play_Count_Desc";
            cms_Sort_Play_Count_Desc.Size = new Size(209, 22);
            cms_Sort_Play_Count_Desc.Tag = "PlayCount|true";
            cms_Sort_Play_Count_Desc.Text = "Play Count - Descending";
            cms_Sort_Play_Count_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(206, 6);
            // 
            // cms_Sort_Last_Played_Asc
            // 
            cms_Sort_Last_Played_Asc.Name = "cms_Sort_Last_Played_Asc";
            cms_Sort_Last_Played_Asc.Size = new Size(209, 22);
            cms_Sort_Last_Played_Asc.Tag = "LastPlayed|false";
            cms_Sort_Last_Played_Asc.Text = "Last Played - Ascending";
            cms_Sort_Last_Played_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Last_Played_Desc
            // 
            cms_Sort_Last_Played_Desc.Name = "cms_Sort_Last_Played_Desc";
            cms_Sort_Last_Played_Desc.Size = new Size(209, 22);
            cms_Sort_Last_Played_Desc.Tag = "LastPlayed|true";
            cms_Sort_Last_Played_Desc.Text = "Last Played  - Descending";
            cms_Sort_Last_Played_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Shuffle
            // 
            cms_Shuffle.Name = "cms_Shuffle";
            cms_Shuffle.Size = new Size(164, 22);
            cms_Shuffle.Text = "Shuffle Playlist";
            cms_Shuffle.Click += ShuffleButton_Click;
            // 
            // cms_Clear
            // 
            cms_Clear.Name = "cms_Clear";
            cms_Clear.Size = new Size(164, 22);
            cms_Clear.Text = "Clear Playlist";
            cms_Clear.Click += ClearPlaylistButton_Click;
            // 
            // cms_Load
            // 
            cms_Load.Name = "cms_Load";
            cms_Load.Size = new Size(164, 22);
            cms_Load.Text = "Load Playlist";
            cms_Load.Click += OpenPlaylistButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(161, 6);
            // 
            // cms_Save
            // 
            cms_Save.Name = "cms_Save";
            cms_Save.Size = new Size(164, 22);
            cms_Save.Text = "Save Playlist";
            cms_Save.Click += SavePlaylistButton_Click;
            // 
            // cms_Exit
            // 
            cms_Exit.Name = "cms_Exit";
            cms_Exit.Size = new Size(164, 22);
            cms_Exit.Text = "Exit";
            cms_Exit.Click += ExitButton_Click;
            // 
            // playlist_Panel
            // 
            playlist_Panel.Controls.Add(btn_Shuffle);
            playlist_Panel.Controls.Add(btn_Save);
            playlist_Panel.Controls.Add(btn_Open);
            playlist_Panel.Controls.Add(playList_Options);
            playlist_Panel.Controls.Add(btn_Remove);
            playlist_Panel.Controls.Add(btn_Add);
            playlist_Panel.Controls.Add(playListBox);
            playlist_Panel.Dock = DockStyle.Fill;
            playlist_Panel.Location = new Point(0, 82);
            playlist_Panel.Name = "playlist_Panel";
            playlist_Panel.Size = new Size(363, 362);
            playlist_Panel.TabIndex = 6;
            // 
            // btn_Shuffle
            // 
            btn_Shuffle.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_Shuffle.EnableBorder = false;
            btn_Shuffle.FlatAppearance.BorderSize = 0;
            btn_Shuffle.FocusWrapAroundImage = true;
            btn_Shuffle.Location = new Point(294, 337);
            btn_Shuffle.Name = "btn_Shuffle";
            btn_Shuffle.RoundCorners = false;
            btn_Shuffle.Size = new Size(17, 16);
            btn_Shuffle.TabIndex = 4;
            btn_Shuffle.Text = "themableButton1";
            btn_Shuffle.TintedImage = (Image)resources.GetObject("btn_Shuffle.TintedImage");
            btn_Shuffle.UseAccentForTintedImage = true;
            btn_Shuffle.UseVisualStyleBackColor = true;
            btn_Shuffle.Click += ShuffleButton_Click;
            // 
            // btn_Save
            // 
            btn_Save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_Save.EnableBorder = false;
            btn_Save.FlatAppearance.BorderSize = 0;
            btn_Save.FocusWrapAroundImage = true;
            btn_Save.Location = new Point(339, 335);
            btn_Save.Name = "btn_Save";
            btn_Save.RoundCorners = false;
            btn_Save.Size = new Size(16, 17);
            btn_Save.TabIndex = 6;
            btn_Save.Text = "Save";
            btn_Save.TintedImage = (Image)resources.GetObject("btn_Save.TintedImage");
            btn_Save.UseAccentForTintedImage = true;
            btn_Save.UseVisualStyleBackColor = true;
            btn_Save.Click += SavePlaylistButton_Click;
            // 
            // btn_Open
            // 
            btn_Open.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_Open.EnableBorder = false;
            btn_Open.FlatAppearance.BorderSize = 0;
            btn_Open.FocusWrapAroundImage = true;
            btn_Open.Location = new Point(317, 336);
            btn_Open.Name = "btn_Open";
            btn_Open.RoundCorners = false;
            btn_Open.Size = new Size(17, 16);
            btn_Open.TabIndex = 5;
            btn_Open.Text = "Open";
            btn_Open.TintedImage = (Image)resources.GetObject("btn_Open.TintedImage");
            btn_Open.UseAccentForTintedImage = true;
            btn_Open.UseVisualStyleBackColor = true;
            btn_Open.Click += OpenPlaylistButton_Click;
            // 
            // playList_Options
            // 
            playList_Options.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            playList_Options.BorderColor = SystemColors.ActiveBorder;
            playList_Options.BufferHeight = 17;
            playList_Options.Location = new Point(94, 331);
            playList_Options.Name = "playList_Options";
            playList_Options.SelectedItemBackColor = SystemColors.Highlight;
            playList_Options.SelectedItemForeColor = SystemColors.HighlightText;
            playList_Options.Size = new Size(168, 23);
            playList_Options.TabIndex = 3;
            playList_Options.SelectedIndexChanged += PlayListOptions_SelectedIndexChanged;
            // 
            // btn_Remove
            // 
            btn_Remove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn_Remove.EnableBorder = false;
            btn_Remove.FlatAppearance.BorderSize = 0;
            btn_Remove.FocusWrapAroundImage = true;
            btn_Remove.Location = new Point(26, 338);
            btn_Remove.Name = "btn_Remove";
            btn_Remove.RoundCorners = false;
            btn_Remove.Size = new Size(16, 16);
            btn_Remove.TabIndex = 2;
            btn_Remove.Text = "Remove";
            btn_Remove.TintedImage = (Image)resources.GetObject("btn_Remove.TintedImage");
            btn_Remove.UseAccentForTintedImage = true;
            btn_Remove.UseVisualStyleBackColor = true;
            btn_Remove.Click += RemoveButton_Click;
            // 
            // btn_Add
            // 
            btn_Add.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btn_Add.EnableBorder = false;
            btn_Add.FlatAppearance.BorderSize = 0;
            btn_Add.FocusWrapAroundImage = true;
            btn_Add.Location = new Point(5, 338);
            btn_Add.Name = "btn_Add";
            btn_Add.RoundCorners = false;
            btn_Add.Size = new Size(16, 16);
            btn_Add.TabIndex = 1;
            btn_Add.Text = "Add";
            btn_Add.TintedImage = (Image)resources.GetObject("btn_Add.TintedImage");
            btn_Add.UseAccentForTintedImage = true;
            btn_Add.UseVisualStyleBackColor = true;
            btn_Add.Click += AddButton_Click;
            // 
            // PlayTimer
            // 
            PlayTimer.Tick += PlayTimer_Tick;
            // 
            // ts_Main
            // 
            ts_Main.AutoSize = false;
            ts_Main.GripStyle = ToolStripGripStyle.Hidden;
            ts_Main.Items.AddRange(new ToolStripItem[] { ts_Btn_Settings, AudioDeviceList });
            ts_Main.Location = new Point(0, 0);
            ts_Main.Name = "ts_Main";
            ts_Main.Padding = new Padding(6, 3, 6, 3);
            ts_Main.Size = new Size(363, 29);
            ts_Main.TabIndex = 1;
            // 
            // ts_Btn_Settings
            // 
            ts_Btn_Settings.AccentColor = Color.DodgerBlue;
            ts_Btn_Settings.Alignment = ToolStripItemAlignment.Right;
            ts_Btn_Settings.AllowFocus = true;
            ts_Btn_Settings.AutoSize = false;
            ts_Btn_Settings.BorderColor = Color.Empty;
            ts_Btn_Settings.CornerRadius = 5;
            ts_Btn_Settings.EnableBorder = false;
            ts_Btn_Settings.FocusWrapAroundImage = true;
            ts_Btn_Settings.MatchImageSize = true;
            ts_Btn_Settings.Name = "ts_Btn_Settings";
            ts_Btn_Settings.RoundCorners = false;
            ts_Btn_Settings.Size = new Size(16, 16);
            ts_Btn_Settings.Text = "Settings";
            ts_Btn_Settings.TintedImage = (Image)resources.GetObject("ts_Btn_Settings.TintedImage");
            ts_Btn_Settings.UseAccentColor = true;
            ts_Btn_Settings.UseAccentForTintedImage = true;
            ts_Btn_Settings.UseThemeColors = true;
            ts_Btn_Settings.UseTightFocusBorder = true;
            ts_Btn_Settings.Click += OpenSettingButton_Click;
            // 
            // AudioDeviceList
            // 
            AudioDeviceList.AccentColor = Color.DodgerBlue;
            AudioDeviceList.AutoSize = false;
            AudioDeviceList.BorderColor = SystemColors.ActiveBorder;
            AudioDeviceList.BufferHeight = 20;
            AudioDeviceList.ItemHeight = 10;
            AudioDeviceList.Margin = new Padding(0, 1, 5, 2);
            AudioDeviceList.Name = "AudioDeviceList";
            AudioDeviceList.SelectedItemBackColor = SystemColors.Highlight;
            AudioDeviceList.SelectedItemForeColor = SystemColors.HighlightText;
            AudioDeviceList.Size = new Size(150, 22);
            AudioDeviceList.UseThemeColors = true;
            AudioDeviceList.SelectedIndexChanged += AudioDeviceList_SelectedIndexChanged;
            // 
            // MP3PlayerV2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(363, 444);
            Controls.Add(playlist_Panel);
            Controls.Add(slider_Panel);
            Controls.Add(ctrls_Panel);
            Controls.Add(ts_Main);
            MinimumSize = new Size(379, 483);
            Name = "MP3PlayerV2";
            Text = "MP3 Player";
            FormClosing += CloseForm;
            ctrls_Panel.ResumeLayout(false);
            slider_Panel.ResumeLayout(false);
            cms_Main.ResumeLayout(false);
            playlist_Panel.ResumeLayout(false);
            ts_Main.ResumeLayout(false);
            ts_Main.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private BazthalLib.Controls.ThemablePanel ctrls_Panel;
        private BazthalLib.Controls.ThemableLabel Cur_Track_Label;
        private BazthalLib.Controls.ThemablePanel slider_Panel;
        private BazthalLib.Controls.ThemableTrackBar Tracking_Slider;
        private BazthalLib.Controls.ThemableTrackBar Volume_Slider;
        private BazthalLib.Controls.ThemableButton Btn_Back;
        private BazthalLib.Controls.ThemableButton Btn_Play;
        private BazthalLib.Controls.ThemableButton Btn_Stop;
        private BazthalLib.Controls.ThemableButton Btn_Pause;
        private BazthalLib.Controls.ThemableButton Btn_Next;
        private BazthalLib.Controls.ThemableListBox playListBox;
        private BazthalLib.Controls.ThemablePanel playlist_Panel;
        private BazthalLib.Controls.ThemableButton btn_Add;
        private BazthalLib.Controls.ThemableButton btn_Open;
        private BazthalLib.Controls.ThemableComboBox playList_Options;
        private BazthalLib.Controls.ThemableButton btn_Remove;
        private BazthalLib.Controls.ThemableButton btn_Save;
        private BazthalLib.Controls.ThemableButton btn_Shuffle;
        private System.Windows.Forms.Timer PlayTimer;
        private BazthalLib.Controls.ThemableToolStrip ts_Main;
        private BazthalLib.Controls.ThemableToolStripComboBox AudioDeviceList;
        private BazthalLib.Controls.ThemableToolStripButton ts_Btn_Settings;
        private ContextMenuStrip cms_Main;
        private ToolStripMenuItem cms_Add;
        private ToolStripMenuItem cms_Remove;
        private ToolStripMenuItem cms_Shuffle;
        private ToolStripMenuItem cms_Clear;
        private ToolStripMenuItem cms_Load;
        private ToolStripMenuItem cms_Save;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem cms_Exit;
        private ToolStripMenuItem cms_Sort;
        private ToolStripMenuItem cms_Sort_Artist_Asc;
        private ToolStripMenuItem cms_Sort_Artist_Desc;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem cms_Sort_Title_Asc;
        private ToolStripMenuItem cms_Sort_Title_Desc;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem cms_Sort_Play_Count_Asc;
        private ToolStripMenuItem cms_Sort_Play_Count_Desc;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem cms_Sort_Last_Played_Asc;
        private ToolStripMenuItem cms_Sort_Last_Played_Desc;
    }
}
