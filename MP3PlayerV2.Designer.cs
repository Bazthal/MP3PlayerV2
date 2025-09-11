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
            cms_Sort_Artist = new ToolStripMenuItem();
            cms_Sort_Artist_Asc = new ToolStripMenuItem();
            cms_Sort_Artist_Desc = new ToolStripMenuItem();
            cms_Sort_Title = new ToolStripMenuItem();
            cms_Sort_Title_Asc = new ToolStripMenuItem();
            cms_Sort_Title_Desc = new ToolStripMenuItem();
            cms_Sort_Album = new ToolStripMenuItem();
            cms_Sort_Album_Asc = new ToolStripMenuItem();
            cms_Sort_Album_Desc = new ToolStripMenuItem();
            cms_Sort_PlayCount = new ToolStripMenuItem();
            cms_Sort_Play_Count_Asc = new ToolStripMenuItem();
            cms_Sort_Play_Count_Desc = new ToolStripMenuItem();
            cms_Sort_LastPlayed = new ToolStripMenuItem();
            cms_Sort_Last_Played_Asc = new ToolStripMenuItem();
            cms_Sort_Last_Played_Desc = new ToolStripMenuItem();
            cms_Sort_Liked = new ToolStripMenuItem();
            cms_Sort_Liked_Asc = new ToolStripMenuItem();
            cms_Sort_Liked_Desc = new ToolStripMenuItem();
            cms_Sort_Disliked = new ToolStripMenuItem();
            cms_Sort_Disliked_Asc = new ToolStripMenuItem();
            cms_Sort_Disliked_Desc = new ToolStripMenuItem();
            cms_Sort_Rating = new ToolStripMenuItem();
            cms_Sort_Rating_Asc = new ToolStripMenuItem();
            cms_Sort_Rating_Desc = new ToolStripMenuItem();
            cms_Shuffle = new ToolStripMenuItem();
            cms_Clear = new ToolStripMenuItem();
            cms_Load = new ToolStripMenuItem();
            cms_Save = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            cms_RateTrack = new ToolStripMenuItem();
            cms_Like = new ToolStripMenuItem();
            cms_Neutral = new ToolStripMenuItem();
            cms_Dislike = new ToolStripMenuItem();
            cms_Reset = new ToolStripMenuItem();
            cms_Reset_PlayCount = new ToolStripMenuItem();
            cms_Reset_PlayCount_Selected = new ToolStripMenuItem();
            cms_Reset_PlayCount_ALL = new ToolStripMenuItem();
            cms_Reset_LastPlayed = new ToolStripMenuItem();
            cms_Reset_LastPlayed_Selected = new ToolStripMenuItem();
            cms_Reset_LastPlayed_ALL = new ToolStripMenuItem();
            cms_Reset_SkipCount = new ToolStripMenuItem();
            cms_Reset_SkipCount_Selected = new ToolStripMenuItem();
            cms_Reset_SkipCount_ALL = new ToolStripMenuItem();
            cms_Reset_Liked = new ToolStripMenuItem();
            cms_Reset_Liked_Selected = new ToolStripMenuItem();
            cms_Reset_Liked_All = new ToolStripMenuItem();
            cms_Reset_Disliked = new ToolStripMenuItem();
            cms_Reset_Disliked_Selected = new ToolStripMenuItem();
            cms_Reset_Disliked_All = new ToolStripMenuItem();
            cms_Reset_RatingScore = new ToolStripMenuItem();
            cms_Reset_RatingScore_Selected = new ToolStripMenuItem();
            cms_Reset_RatingScore_All = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            cms_Reset_All = new ToolStripMenuItem();
            cms_Exit = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
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
            resources.ApplyResources(ctrls_Panel, "ctrls_Panel");
            ctrls_Panel.Name = "ctrls_Panel";
            // 
            // Btn_Next
            // 
            resources.ApplyResources(Btn_Next, "Btn_Next");
            Btn_Next.EnableBorder = false;
            Btn_Next.FlatAppearance.BorderSize = 0;
            Btn_Next.FocusWrapAroundImage = true;
            Btn_Next.Name = "Btn_Next";
            Btn_Next.RoundCorners = false;
            Btn_Next.TintedImage = (Image)resources.GetObject("Btn_Next.TintedImage");
            Btn_Next.UseAccentForTintedImage = true;
            Btn_Next.UseVisualStyleBackColor = true;
            Btn_Next.Click += NextButton_Click;
            // 
            // Btn_Stop
            // 
            resources.ApplyResources(Btn_Stop, "Btn_Stop");
            Btn_Stop.EnableBorder = false;
            Btn_Stop.FlatAppearance.BorderSize = 0;
            Btn_Stop.FocusWrapAroundImage = true;
            Btn_Stop.Name = "Btn_Stop";
            Btn_Stop.RoundCorners = false;
            Btn_Stop.TintedImage = (Image)resources.GetObject("Btn_Stop.TintedImage");
            Btn_Stop.UseAccentForTintedImage = true;
            Btn_Stop.UseVisualStyleBackColor = true;
            Btn_Stop.Click += StopButton_Click;
            // 
            // Btn_Pause
            // 
            resources.ApplyResources(Btn_Pause, "Btn_Pause");
            Btn_Pause.EnableBorder = false;
            Btn_Pause.FlatAppearance.BorderSize = 0;
            Btn_Pause.FocusWrapAroundImage = true;
            Btn_Pause.Name = "Btn_Pause";
            Btn_Pause.RoundCorners = false;
            Btn_Pause.TintedImage = (Image)resources.GetObject("Btn_Pause.TintedImage");
            Btn_Pause.UseAccentForTintedImage = true;
            Btn_Pause.UseVisualStyleBackColor = true;
            Btn_Pause.Click += PauseButton_Click;
            // 
            // Btn_Play
            // 
            resources.ApplyResources(Btn_Play, "Btn_Play");
            Btn_Play.EnableBorder = false;
            Btn_Play.FlatAppearance.BorderSize = 0;
            Btn_Play.FocusWrapAroundImage = true;
            Btn_Play.Name = "Btn_Play";
            Btn_Play.RoundCorners = false;
            Btn_Play.TintedImage = (Image)resources.GetObject("Btn_Play.TintedImage");
            Btn_Play.UseAccentForTintedImage = true;
            Btn_Play.UseVisualStyleBackColor = true;
            Btn_Play.Click += PlayButton_Click;
            // 
            // Btn_Back
            // 
            resources.ApplyResources(Btn_Back, "Btn_Back");
            Btn_Back.EnableBorder = false;
            Btn_Back.FlatAppearance.BorderSize = 0;
            Btn_Back.FocusWrapAroundImage = true;
            Btn_Back.Name = "Btn_Back";
            Btn_Back.RoundCorners = false;
            Btn_Back.TintedImage = (Image)resources.GetObject("Btn_Back.TintedImage");
            Btn_Back.UseAccentForTintedImage = true;
            Btn_Back.UseVisualStyleBackColor = true;
            Btn_Back.Click += PreviousButton_Click;
            // 
            // Cur_Track_Label
            // 
            resources.ApplyResources(Cur_Track_Label, "Cur_Track_Label");
            Cur_Track_Label.BorderColor = Color.Gray;
            Cur_Track_Label.EnableBorder = true;
            Cur_Track_Label.Name = "Cur_Track_Label";
            Cur_Track_Label.UseMnemonic = false;
            // 
            // slider_Panel
            // 
            slider_Panel.Controls.Add(Volume_Slider);
            slider_Panel.Controls.Add(Tracking_Slider);
            resources.ApplyResources(slider_Panel, "slider_Panel");
            slider_Panel.Name = "slider_Panel";
            // 
            // Volume_Slider
            // 
            resources.ApplyResources(Volume_Slider, "Volume_Slider");
            Volume_Slider.BorderColor = SystemColors.ActiveBorder;
            Volume_Slider.EnableBorder = false;
            Volume_Slider.Name = "Volume_Slider";
            Volume_Slider.RoundedThumb = false;
            Volume_Slider.SelectedItemBackColor = SystemColors.Highlight;
            Volume_Slider.SelectedItemForeColor = SystemColors.HighlightText;
            Volume_Slider.ThumbSize = 10;
            Volume_Slider.UseProgressFill = false;
            Volume_Slider.Value = 1;
            Volume_Slider.ScrollCompleted += Volume_ScrollCompleted;
            // 
            // Tracking_Slider
            // 
            resources.ApplyResources(Tracking_Slider, "Tracking_Slider");
            Tracking_Slider.BorderColor = SystemColors.ActiveBorder;
            Tracking_Slider.EnableBorder = false;
            Tracking_Slider.Name = "Tracking_Slider";
            Tracking_Slider.RoundedThumb = false;
            Tracking_Slider.SelectedItemBackColor = SystemColors.Highlight;
            Tracking_Slider.SelectedItemForeColor = SystemColors.HighlightText;
            Tracking_Slider.ThumbSize = 10;
            Tracking_Slider.ScrollCompleted += Tracking_Slider_ScrollCompleted;
            // 
            // playListBox
            // 
            playListBox.AllowDrop = true;
            resources.ApplyResources(playListBox, "playListBox");
            playListBox.BorderColor = SystemColors.ActiveBorder;
            playListBox.ContextMenuStrip = cms_Main;
            playListBox.EnableHorizontalScroll = false;
            playListBox.ItemHeight = 15;
            playListBox.Name = "playListBox";
            playListBox.SelectedItemBackColor = SystemColors.Highlight;
            playListBox.SelectedItemForeColor = SystemColors.HighlightText;
            playListBox.SelectedIndexChanged += PlayList_SelectedIndexChanged;
            playListBox.DragDrop += PlayList_DragDrop;
            playListBox.DragEnter += PlayList_DragEnter;
            playListBox.DoubleClick += PlayList_DoubleClick;
            playListBox.KeyDown += PlayList_KeyDown;
            // 
            // cms_Main
            // 
            cms_Main.Items.AddRange(new ToolStripItem[] { cms_Add, cms_Remove, cms_Sort, cms_Shuffle, cms_Clear, cms_Load, cms_Save, toolStripSeparator5, cms_RateTrack, cms_Reset, cms_Exit });
            cms_Main.Name = "cms_Main";
            cms_Main.RenderMode = ToolStripRenderMode.System;
            resources.ApplyResources(cms_Main, "cms_Main");
            cms_Main.Opening += ContextMenu_Opening;
            // 
            // cms_Add
            // 
            cms_Add.Name = "cms_Add";
            resources.ApplyResources(cms_Add, "cms_Add");
            cms_Add.Click += AddButton_Click;
            // 
            // cms_Remove
            // 
            cms_Remove.Name = "cms_Remove";
            resources.ApplyResources(cms_Remove, "cms_Remove");
            cms_Remove.Click += RemoveButton_Click;
            // 
            // cms_Sort
            // 
            cms_Sort.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Artist, cms_Sort_Title, cms_Sort_Album, cms_Sort_PlayCount, cms_Sort_LastPlayed, cms_Sort_Liked, cms_Sort_Disliked, cms_Sort_Rating });
            cms_Sort.Name = "cms_Sort";
            resources.ApplyResources(cms_Sort, "cms_Sort");
            // 
            // cms_Sort_Artist
            // 
            cms_Sort_Artist.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Artist_Asc, cms_Sort_Artist_Desc });
            cms_Sort_Artist.Name = "cms_Sort_Artist";
            resources.ApplyResources(cms_Sort_Artist, "cms_Sort_Artist");
            // 
            // cms_Sort_Artist_Asc
            // 
            cms_Sort_Artist_Asc.Name = "cms_Sort_Artist_Asc";
            resources.ApplyResources(cms_Sort_Artist_Asc, "cms_Sort_Artist_Asc");
            cms_Sort_Artist_Asc.Tag = "Artist|false";
            cms_Sort_Artist_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Artist_Desc
            // 
            cms_Sort_Artist_Desc.Name = "cms_Sort_Artist_Desc";
            resources.ApplyResources(cms_Sort_Artist_Desc, "cms_Sort_Artist_Desc");
            cms_Sort_Artist_Desc.Tag = "Artist|true";
            cms_Sort_Artist_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Title
            // 
            cms_Sort_Title.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Title_Asc, cms_Sort_Title_Desc });
            cms_Sort_Title.Name = "cms_Sort_Title";
            resources.ApplyResources(cms_Sort_Title, "cms_Sort_Title");
            // 
            // cms_Sort_Title_Asc
            // 
            cms_Sort_Title_Asc.Name = "cms_Sort_Title_Asc";
            resources.ApplyResources(cms_Sort_Title_Asc, "cms_Sort_Title_Asc");
            cms_Sort_Title_Asc.Tag = "Title|false";
            cms_Sort_Title_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Title_Desc
            // 
            cms_Sort_Title_Desc.Name = "cms_Sort_Title_Desc";
            resources.ApplyResources(cms_Sort_Title_Desc, "cms_Sort_Title_Desc");
            cms_Sort_Title_Desc.Tag = "Title|true";
            cms_Sort_Title_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Album
            // 
            cms_Sort_Album.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Album_Asc, cms_Sort_Album_Desc });
            cms_Sort_Album.Name = "cms_Sort_Album";
            resources.ApplyResources(cms_Sort_Album, "cms_Sort_Album");
            // 
            // cms_Sort_Album_Asc
            // 
            cms_Sort_Album_Asc.Name = "cms_Sort_Album_Asc";
            resources.ApplyResources(cms_Sort_Album_Asc, "cms_Sort_Album_Asc");
            cms_Sort_Album_Asc.Tag = "Album|false";
            cms_Sort_Album_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Album_Desc
            // 
            cms_Sort_Album_Desc.Name = "cms_Sort_Album_Desc";
            resources.ApplyResources(cms_Sort_Album_Desc, "cms_Sort_Album_Desc");
            cms_Sort_Album_Desc.Tag = "Album|true";
            cms_Sort_Album_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_PlayCount
            // 
            cms_Sort_PlayCount.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Play_Count_Asc, cms_Sort_Play_Count_Desc });
            cms_Sort_PlayCount.Name = "cms_Sort_PlayCount";
            resources.ApplyResources(cms_Sort_PlayCount, "cms_Sort_PlayCount");
            // 
            // cms_Sort_Play_Count_Asc
            // 
            cms_Sort_Play_Count_Asc.Name = "cms_Sort_Play_Count_Asc";
            resources.ApplyResources(cms_Sort_Play_Count_Asc, "cms_Sort_Play_Count_Asc");
            cms_Sort_Play_Count_Asc.Tag = "PlayCount|false";
            cms_Sort_Play_Count_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Play_Count_Desc
            // 
            cms_Sort_Play_Count_Desc.Name = "cms_Sort_Play_Count_Desc";
            resources.ApplyResources(cms_Sort_Play_Count_Desc, "cms_Sort_Play_Count_Desc");
            cms_Sort_Play_Count_Desc.Tag = "PlayCount|true";
            cms_Sort_Play_Count_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_LastPlayed
            // 
            cms_Sort_LastPlayed.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Last_Played_Asc, cms_Sort_Last_Played_Desc });
            cms_Sort_LastPlayed.Name = "cms_Sort_LastPlayed";
            resources.ApplyResources(cms_Sort_LastPlayed, "cms_Sort_LastPlayed");
            // 
            // cms_Sort_Last_Played_Asc
            // 
            cms_Sort_Last_Played_Asc.Name = "cms_Sort_Last_Played_Asc";
            resources.ApplyResources(cms_Sort_Last_Played_Asc, "cms_Sort_Last_Played_Asc");
            cms_Sort_Last_Played_Asc.Tag = "LastPlayed|false";
            cms_Sort_Last_Played_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Last_Played_Desc
            // 
            cms_Sort_Last_Played_Desc.Name = "cms_Sort_Last_Played_Desc";
            resources.ApplyResources(cms_Sort_Last_Played_Desc, "cms_Sort_Last_Played_Desc");
            cms_Sort_Last_Played_Desc.Tag = "LastPlayed|true";
            cms_Sort_Last_Played_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Liked
            // 
            cms_Sort_Liked.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Liked_Asc, cms_Sort_Liked_Desc });
            cms_Sort_Liked.Name = "cms_Sort_Liked";
            resources.ApplyResources(cms_Sort_Liked, "cms_Sort_Liked");
            // 
            // cms_Sort_Liked_Asc
            // 
            cms_Sort_Liked_Asc.Name = "cms_Sort_Liked_Asc";
            resources.ApplyResources(cms_Sort_Liked_Asc, "cms_Sort_Liked_Asc");
            cms_Sort_Liked_Asc.Tag = "Liked|false";
            cms_Sort_Liked_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Liked_Desc
            // 
            cms_Sort_Liked_Desc.Name = "cms_Sort_Liked_Desc";
            resources.ApplyResources(cms_Sort_Liked_Desc, "cms_Sort_Liked_Desc");
            cms_Sort_Liked_Desc.Tag = "Liked|true";
            cms_Sort_Liked_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Disliked
            // 
            cms_Sort_Disliked.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Disliked_Asc, cms_Sort_Disliked_Desc });
            cms_Sort_Disliked.Name = "cms_Sort_Disliked";
            resources.ApplyResources(cms_Sort_Disliked, "cms_Sort_Disliked");
            // 
            // cms_Sort_Disliked_Asc
            // 
            cms_Sort_Disliked_Asc.Name = "cms_Sort_Disliked_Asc";
            resources.ApplyResources(cms_Sort_Disliked_Asc, "cms_Sort_Disliked_Asc");
            cms_Sort_Disliked_Asc.Tag = "Disliked|false";
            cms_Sort_Disliked_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Disliked_Desc
            // 
            cms_Sort_Disliked_Desc.Name = "cms_Sort_Disliked_Desc";
            resources.ApplyResources(cms_Sort_Disliked_Desc, "cms_Sort_Disliked_Desc");
            cms_Sort_Disliked_Desc.Tag = "Disliked|true";
            cms_Sort_Disliked_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Rating
            // 
            cms_Sort_Rating.DropDownItems.AddRange(new ToolStripItem[] { cms_Sort_Rating_Asc, cms_Sort_Rating_Desc });
            cms_Sort_Rating.Name = "cms_Sort_Rating";
            resources.ApplyResources(cms_Sort_Rating, "cms_Sort_Rating");
            // 
            // cms_Sort_Rating_Asc
            // 
            cms_Sort_Rating_Asc.Name = "cms_Sort_Rating_Asc";
            resources.ApplyResources(cms_Sort_Rating_Asc, "cms_Sort_Rating_Asc");
            cms_Sort_Rating_Asc.Tag = "Rating|false";
            cms_Sort_Rating_Asc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Sort_Rating_Desc
            // 
            cms_Sort_Rating_Desc.Name = "cms_Sort_Rating_Desc";
            resources.ApplyResources(cms_Sort_Rating_Desc, "cms_Sort_Rating_Desc");
            cms_Sort_Rating_Desc.Tag = "Rating|true";
            cms_Sort_Rating_Desc.Click += SortPlaylistMenuButton_Click;
            // 
            // cms_Shuffle
            // 
            cms_Shuffle.Name = "cms_Shuffle";
            resources.ApplyResources(cms_Shuffle, "cms_Shuffle");
            cms_Shuffle.Click += ShuffleButton_Click;
            // 
            // cms_Clear
            // 
            cms_Clear.Name = "cms_Clear";
            resources.ApplyResources(cms_Clear, "cms_Clear");
            cms_Clear.Click += ClearPlaylistButton_Click;
            // 
            // cms_Load
            // 
            cms_Load.Name = "cms_Load";
            resources.ApplyResources(cms_Load, "cms_Load");
            cms_Load.Click += OpenPlaylistButton_Click;
            // 
            // cms_Save
            // 
            cms_Save.Name = "cms_Save";
            resources.ApplyResources(cms_Save, "cms_Save");
            cms_Save.Click += SavePlaylistButton_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(toolStripSeparator5, "toolStripSeparator5");
            // 
            // cms_RateTrack
            // 
            cms_RateTrack.DropDownItems.AddRange(new ToolStripItem[] { cms_Like, cms_Neutral, cms_Dislike });
            cms_RateTrack.Name = "cms_RateTrack";
            resources.ApplyResources(cms_RateTrack, "cms_RateTrack");
            // 
            // cms_Like
            // 
            cms_Like.Name = "cms_Like";
            resources.ApplyResources(cms_Like, "cms_Like");
            cms_Like.Click += LikeMenuButton_Click;
            // 
            // cms_Neutral
            // 
            cms_Neutral.Name = "cms_Neutral";
            resources.ApplyResources(cms_Neutral, "cms_Neutral");
            cms_Neutral.Click += NeutralMenuButton_Click;
            // 
            // cms_Dislike
            // 
            cms_Dislike.Name = "cms_Dislike";
            resources.ApplyResources(cms_Dislike, "cms_Dislike");
            cms_Dislike.Click += DislikeMenuButton_Click;
            // 
            // cms_Reset
            // 
            cms_Reset.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_PlayCount, cms_Reset_LastPlayed, cms_Reset_SkipCount, cms_Reset_Liked, cms_Reset_Disliked, cms_Reset_RatingScore, toolStripSeparator6, cms_Reset_All });
            cms_Reset.Name = "cms_Reset";
            resources.ApplyResources(cms_Reset, "cms_Reset");
            // 
            // cms_Reset_PlayCount
            // 
            cms_Reset_PlayCount.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_PlayCount_Selected, cms_Reset_PlayCount_ALL });
            cms_Reset_PlayCount.Name = "cms_Reset_PlayCount";
            resources.ApplyResources(cms_Reset_PlayCount, "cms_Reset_PlayCount");
            // 
            // cms_Reset_PlayCount_Selected
            // 
            cms_Reset_PlayCount_Selected.Name = "cms_Reset_PlayCount_Selected";
            resources.ApplyResources(cms_Reset_PlayCount_Selected, "cms_Reset_PlayCount_Selected");
            cms_Reset_PlayCount_Selected.Tag = "PlayCount|Selected";
            cms_Reset_PlayCount_Selected.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_PlayCount_ALL
            // 
            cms_Reset_PlayCount_ALL.Name = "cms_Reset_PlayCount_ALL";
            resources.ApplyResources(cms_Reset_PlayCount_ALL, "cms_Reset_PlayCount_ALL");
            cms_Reset_PlayCount_ALL.Tag = "PlayCount|All";
            cms_Reset_PlayCount_ALL.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_LastPlayed
            // 
            cms_Reset_LastPlayed.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_LastPlayed_Selected, cms_Reset_LastPlayed_ALL });
            cms_Reset_LastPlayed.Name = "cms_Reset_LastPlayed";
            resources.ApplyResources(cms_Reset_LastPlayed, "cms_Reset_LastPlayed");
            // 
            // cms_Reset_LastPlayed_Selected
            // 
            cms_Reset_LastPlayed_Selected.Name = "cms_Reset_LastPlayed_Selected";
            resources.ApplyResources(cms_Reset_LastPlayed_Selected, "cms_Reset_LastPlayed_Selected");
            cms_Reset_LastPlayed_Selected.Tag = "LastPlayed|Selected";
            cms_Reset_LastPlayed_Selected.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_LastPlayed_ALL
            // 
            cms_Reset_LastPlayed_ALL.Name = "cms_Reset_LastPlayed_ALL";
            resources.ApplyResources(cms_Reset_LastPlayed_ALL, "cms_Reset_LastPlayed_ALL");
            cms_Reset_LastPlayed_ALL.Tag = "LastPlayed|All";
            cms_Reset_LastPlayed_ALL.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_SkipCount
            // 
            cms_Reset_SkipCount.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_SkipCount_Selected, cms_Reset_SkipCount_ALL });
            cms_Reset_SkipCount.Name = "cms_Reset_SkipCount";
            resources.ApplyResources(cms_Reset_SkipCount, "cms_Reset_SkipCount");
            // 
            // cms_Reset_SkipCount_Selected
            // 
            cms_Reset_SkipCount_Selected.Name = "cms_Reset_SkipCount_Selected";
            resources.ApplyResources(cms_Reset_SkipCount_Selected, "cms_Reset_SkipCount_Selected");
            cms_Reset_SkipCount_Selected.Tag = "SkipCount|Selected";
            cms_Reset_SkipCount_Selected.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_SkipCount_ALL
            // 
            cms_Reset_SkipCount_ALL.Name = "cms_Reset_SkipCount_ALL";
            resources.ApplyResources(cms_Reset_SkipCount_ALL, "cms_Reset_SkipCount_ALL");
            cms_Reset_SkipCount_ALL.Tag = "SkipCount|All";
            cms_Reset_SkipCount_ALL.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_Liked
            // 
            cms_Reset_Liked.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_Liked_Selected, cms_Reset_Liked_All });
            cms_Reset_Liked.Name = "cms_Reset_Liked";
            resources.ApplyResources(cms_Reset_Liked, "cms_Reset_Liked");
            // 
            // cms_Reset_Liked_Selected
            // 
            cms_Reset_Liked_Selected.Name = "cms_Reset_Liked_Selected";
            resources.ApplyResources(cms_Reset_Liked_Selected, "cms_Reset_Liked_Selected");
            cms_Reset_Liked_Selected.Tag = "Liked|Selected";
            cms_Reset_Liked_Selected.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_Liked_All
            // 
            cms_Reset_Liked_All.Name = "cms_Reset_Liked_All";
            resources.ApplyResources(cms_Reset_Liked_All, "cms_Reset_Liked_All");
            cms_Reset_Liked_All.Tag = "Liked|ALL";
            cms_Reset_Liked_All.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_Disliked
            // 
            cms_Reset_Disliked.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_Disliked_Selected, cms_Reset_Disliked_All });
            cms_Reset_Disliked.Name = "cms_Reset_Disliked";
            resources.ApplyResources(cms_Reset_Disliked, "cms_Reset_Disliked");
            // 
            // cms_Reset_Disliked_Selected
            // 
            cms_Reset_Disliked_Selected.Name = "cms_Reset_Disliked_Selected";
            resources.ApplyResources(cms_Reset_Disliked_Selected, "cms_Reset_Disliked_Selected");
            cms_Reset_Disliked_Selected.Tag = "Disliked|Selected";
            cms_Reset_Disliked_Selected.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_Disliked_All
            // 
            cms_Reset_Disliked_All.Name = "cms_Reset_Disliked_All";
            resources.ApplyResources(cms_Reset_Disliked_All, "cms_Reset_Disliked_All");
            cms_Reset_Disliked_All.Tag = "Disliked|ALL";
            cms_Reset_Disliked_All.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_RatingScore
            // 
            cms_Reset_RatingScore.DropDownItems.AddRange(new ToolStripItem[] { cms_Reset_RatingScore_Selected, cms_Reset_RatingScore_All });
            cms_Reset_RatingScore.Name = "cms_Reset_RatingScore";
            resources.ApplyResources(cms_Reset_RatingScore, "cms_Reset_RatingScore");
            // 
            // cms_Reset_RatingScore_Selected
            // 
            cms_Reset_RatingScore_Selected.Name = "cms_Reset_RatingScore_Selected";
            resources.ApplyResources(cms_Reset_RatingScore_Selected, "cms_Reset_RatingScore_Selected");
            cms_Reset_RatingScore_Selected.Tag = "RatingScore|Selected";
            cms_Reset_RatingScore_Selected.Click += ResetMenuButton_Click;
            // 
            // cms_Reset_RatingScore_All
            // 
            cms_Reset_RatingScore_All.Name = "cms_Reset_RatingScore_All";
            resources.ApplyResources(cms_Reset_RatingScore_All, "cms_Reset_RatingScore_All");
            cms_Reset_RatingScore_All.Tag = "RatingScore|ALL";
            cms_Reset_RatingScore_All.Click += ResetMenuButton_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(toolStripSeparator6, "toolStripSeparator6");
            // 
            // cms_Reset_All
            // 
            cms_Reset_All.Name = "cms_Reset_All";
            resources.ApplyResources(cms_Reset_All, "cms_Reset_All");
            cms_Reset_All.Tag = "All|All";
            cms_Reset_All.Click += ResetMenuButton_Click;
            // 
            // cms_Exit
            // 
            cms_Exit.Name = "cms_Exit";
            resources.ApplyResources(cms_Exit, "cms_Exit");
            cms_Exit.Click += ExitButton_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
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
            resources.ApplyResources(playlist_Panel, "playlist_Panel");
            playlist_Panel.Name = "playlist_Panel";
            // 
            // btn_Shuffle
            // 
            resources.ApplyResources(btn_Shuffle, "btn_Shuffle");
            btn_Shuffle.EnableBorder = false;
            btn_Shuffle.FlatAppearance.BorderSize = 0;
            btn_Shuffle.FocusWrapAroundImage = true;
            btn_Shuffle.Name = "btn_Shuffle";
            btn_Shuffle.RoundCorners = false;
            btn_Shuffle.TintedImage = (Image)resources.GetObject("btn_Shuffle.TintedImage");
            btn_Shuffle.UseAccentForTintedImage = true;
            btn_Shuffle.UseVisualStyleBackColor = true;
            btn_Shuffle.Click += ShuffleButton_Click;
            // 
            // btn_Save
            // 
            resources.ApplyResources(btn_Save, "btn_Save");
            btn_Save.EnableBorder = false;
            btn_Save.FlatAppearance.BorderSize = 0;
            btn_Save.FocusWrapAroundImage = true;
            btn_Save.Name = "btn_Save";
            btn_Save.RoundCorners = false;
            btn_Save.TintedImage = (Image)resources.GetObject("btn_Save.TintedImage");
            btn_Save.UseAccentForTintedImage = true;
            btn_Save.UseVisualStyleBackColor = true;
            btn_Save.Click += SavePlaylistButton_Click;
            // 
            // btn_Open
            // 
            resources.ApplyResources(btn_Open, "btn_Open");
            btn_Open.EnableBorder = false;
            btn_Open.FlatAppearance.BorderSize = 0;
            btn_Open.FocusWrapAroundImage = true;
            btn_Open.Name = "btn_Open";
            btn_Open.RoundCorners = false;
            btn_Open.TintedImage = (Image)resources.GetObject("btn_Open.TintedImage");
            btn_Open.UseAccentForTintedImage = true;
            btn_Open.UseVisualStyleBackColor = true;
            btn_Open.Click += OpenPlaylistButton_Click;
            // 
            // playList_Options
            // 
            resources.ApplyResources(playList_Options, "playList_Options");
            playList_Options.BorderColor = SystemColors.ActiveBorder;
            playList_Options.BufferHeight = 17;
            playList_Options.Name = "playList_Options";
            playList_Options.SelectedItemBackColor = SystemColors.Highlight;
            playList_Options.SelectedItemForeColor = SystemColors.HighlightText;
            // 
            // btn_Remove
            // 
            resources.ApplyResources(btn_Remove, "btn_Remove");
            btn_Remove.EnableBorder = false;
            btn_Remove.FlatAppearance.BorderSize = 0;
            btn_Remove.FocusWrapAroundImage = true;
            btn_Remove.Name = "btn_Remove";
            btn_Remove.RoundCorners = false;
            btn_Remove.TintedImage = (Image)resources.GetObject("btn_Remove.TintedImage");
            btn_Remove.UseAccentForTintedImage = true;
            btn_Remove.UseVisualStyleBackColor = true;
            btn_Remove.Click += RemoveButton_Click;
            // 
            // btn_Add
            // 
            resources.ApplyResources(btn_Add, "btn_Add");
            btn_Add.EnableBorder = false;
            btn_Add.FlatAppearance.BorderSize = 0;
            btn_Add.FocusWrapAroundImage = true;
            btn_Add.Name = "btn_Add";
            btn_Add.RoundCorners = false;
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
            resources.ApplyResources(ts_Main, "ts_Main");
            ts_Main.GripStyle = ToolStripGripStyle.Hidden;
            ts_Main.Items.AddRange(new ToolStripItem[] { ts_Btn_Settings, AudioDeviceList });
            ts_Main.Name = "ts_Main";
            // 
            // ts_Btn_Settings
            // 
            ts_Btn_Settings.AccentColor = Color.DodgerBlue;
            ts_Btn_Settings.Alignment = ToolStripItemAlignment.Right;
            ts_Btn_Settings.AllowFocus = true;
            resources.ApplyResources(ts_Btn_Settings, "ts_Btn_Settings");
            ts_Btn_Settings.BorderColor = Color.Empty;
            ts_Btn_Settings.CornerRadius = 5;
            ts_Btn_Settings.EnableBorder = false;
            ts_Btn_Settings.FocusWrapAroundImage = true;
            ts_Btn_Settings.MatchImageSize = true;
            ts_Btn_Settings.Name = "ts_Btn_Settings";
            ts_Btn_Settings.RoundCorners = false;
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
            resources.ApplyResources(AudioDeviceList, "AudioDeviceList");
            AudioDeviceList.BorderColor = SystemColors.ActiveBorder;
            AudioDeviceList.BufferHeight = 5;
            AudioDeviceList.ItemHeight = 12;
            AudioDeviceList.Margin = new Padding(0, 1, 5, 2);
            AudioDeviceList.Name = "AudioDeviceList";
            AudioDeviceList.SelectedItemBackColor = SystemColors.Highlight;
            AudioDeviceList.SelectedItemForeColor = SystemColors.HighlightText;
            AudioDeviceList.UseThemeColors = true;
            AudioDeviceList.SelectedIndexChanged += AudioDeviceList_SelectedIndexChanged;
            // 
            // MP3PlayerV2
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(playlist_Panel);
            Controls.Add(slider_Panel);
            Controls.Add(ctrls_Panel);
            Controls.Add(ts_Main);
            Name = "MP3PlayerV2";
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
        private ToolStripMenuItem cms_Exit;
        private ToolStripMenuItem cms_Sort;
        private ToolStripMenuItem cms_Sort_Artist;
        private ToolStripMenuItem cms_Sort_Artist_Asc;
        private ToolStripMenuItem cms_Sort_Artist_Desc;
        private ToolStripMenuItem cms_Sort_Title;
        private ToolStripMenuItem cms_Sort_Title_Asc;
        private ToolStripMenuItem cms_Sort_Title_Desc;
        private ToolStripMenuItem cms_Sort_PlayCount;
        private ToolStripMenuItem cms_Sort_Play_Count_Asc;
        private ToolStripMenuItem cms_Sort_Play_Count_Desc;
        private ToolStripMenuItem cms_Sort_LastPlayed;
        private ToolStripMenuItem cms_Sort_Last_Played_Asc;
        private ToolStripMenuItem cms_Sort_Last_Played_Desc;
        private ToolStripMenuItem cms_Sort_Album;
        private ToolStripMenuItem cms_Sort_Album_Asc;
        private ToolStripMenuItem cms_Sort_Album_Desc;
        private ToolStripMenuItem cms_Sort_Liked;
        private ToolStripMenuItem cms_Sort_Liked_Asc;
        private ToolStripMenuItem cms_Sort_Liked_Desc;
        private ToolStripMenuItem cms_Sort_Disliked;
        private ToolStripMenuItem cms_Sort_Disliked_Asc;
        private ToolStripMenuItem cms_Sort_Disliked_Desc;
        private ToolStripMenuItem cms_Sort_Rating;
        private ToolStripMenuItem cms_Sort_Rating_Asc;
        private ToolStripMenuItem cms_Sort_Rating_Desc;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem cms_Reset;
        private ToolStripMenuItem cms_Reset_PlayCount;
        private ToolStripMenuItem cms_Reset_PlayCount_Selected;
        private ToolStripMenuItem cms_Reset_PlayCount_ALL;
        private ToolStripMenuItem cms_Reset_LastPlayed;
        private ToolStripMenuItem cms_Reset_LastPlayed_Selected;
        private ToolStripMenuItem cms_Reset_LastPlayed_ALL;
        private ToolStripMenuItem cms_Reset_SkipCount;
        private ToolStripMenuItem cms_Reset_SkipCount_Selected;
        private ToolStripMenuItem cms_Reset_SkipCount_ALL;
        private ToolStripMenuItem cms_Reset_All;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem cms_RateTrack;
        private ToolStripMenuItem cms_Like;
        private ToolStripMenuItem cms_Dislike;
        private ToolStripMenuItem cms_Neutral;
        private ToolStripMenuItem cms_Reset_Liked;
        private ToolStripMenuItem cms_Reset_Disliked;
        private ToolStripMenuItem cms_Reset_RatingScore;
        private ToolStripMenuItem cms_Reset_Liked_Selected;
        private ToolStripMenuItem cms_Reset_Liked_All;
        private ToolStripMenuItem cms_Reset_Disliked_Selected;
        private ToolStripMenuItem cms_Reset_Disliked_All;
        private ToolStripMenuItem cms_Reset_RatingScore_Selected;
        private ToolStripMenuItem cms_Reset_RatingScore_All;
    }
}
