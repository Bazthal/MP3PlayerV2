using BazthalLib.Controls;
using BazthalLib.Systems.IO;
using MP3PlayerV2.Models;
using System.Diagnostics;
using System.Text;

namespace MP3PlayerV2
{
    /// <summary>
    /// Represents a dialog window for displaying and managing detailed information about a specific track.
    /// </summary>
    /// <remarks>This dialog is designed to provide a user-friendly interface for viewing and interacting with
    /// metadata and properties of a track, such as title, artist, album, duration, play counts, user ratings, and more.
    /// It includes controls for displaying album art, navigating to the file's location, and closing the dialog. <para>
    /// The dialog is initialized with a fixed size, a centered position relative to its parent, and a non-resizable
    /// border. It does not display an icon or appear in the taskbar. </para> <para> The dialog can optionally be
    /// pre-populated with track information by passing a <see cref="Track"/> object to the constructor. If no track is
    /// provided, the dialog initializes with default values. </para></remarks>
    public class TrackInformationDialog : Form
    {
        #region fields
        private string _filePath = string.Empty;
        BazthalLib.UI.ThemeColors _themeColors;

        private ThemableLabel lbl_Title, tb_Title, lbl_Artist, tb_Artist, lbl_Album, tb_Album, lbl_FilePath, tb_FilePath,
            lbl_Duration, tb_Duration, lbl_Genre, tb_Genre, lbl_Date, tb_Date, lbl_TrackNumber, tb_TrackNumber, lbl_BitRate, tb_BitRate,
            lbl_PlayCount, tb_PlayCount, lbl_PlayCompleteCount, tb_PlayCompleteCount, lbl_SkipCount, tb_SkipCount,
            lbl_RatingScore, tb_RatingScore, lbl_UserRating, tb_UserRating, tb_Comments, lbl_Comments;

        private ThemablePictureBox pb_AlbumArtBox;
        private ThemableButton btn_OpenFolder, btn_CloseDialog;

        #endregion fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackInformationDialog"/> class, optionally pre-populating it
        /// with track information.
        /// </summary>
        /// <remarks>This dialog is designed to display and manage information about a specific track.  It
        /// is initialized with a fixed size, a centered position relative to its parent, and a non-resizable border.
        /// The dialog does not display an icon or appear in the taskbar.</remarks>
        /// <param name="track">The <see cref="Track"/> object containing the information to display in the dialog.  If <see
        /// langword="null"/>, the dialog will be initialized without pre-populated track information.</param>
#nullable disable
        public TrackInformationDialog(Track track = null)
        {
            BazthalLib.UI.Theming.RegisterForm(this);
            Size = new Size(650, 520);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Icon = MP3PlayerV2.Instance.Icon;
            MinimizeBox = false;
            ShowInTaskbar = false;

            InitializeControls();
            LayoutControls();
            HookEvents();

            if (track != null)
            {
                PopulateTrackInfo(track);

            }

        }

        #endregion Constructor

        #region Initialization Helpers

        /// <summary>
        /// Initializes and configures the controls used in the user interface.
        /// </summary>
        /// <remarks>This method creates and sets up various UI elements, including labels, text boxes,
        /// buttons,  and other controls, with their respective properties such as location, size, text, and default
        /// values. It is intended to be called during the initialization phase of the form or dialog to ensure all 
        /// controls are properly instantiated and ready for use.</remarks>
        private void InitializeControls()
        {
            //PictureBox
            pb_AlbumArtBox = new() { Location = new Point(10, 8), Size = new Size(128, 128), EnableBorder = true, SizeMode = PictureBoxSizeMode.Zoom };

            //Labels and Textblocks
            lbl_Title = new() { Location = new Point(144, 8), AutoSize = true, Text = "Title:" };
            tb_Title = new() { Location = new Point(144, 26), Size = new(478, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Unknown Title" };
            lbl_Artist = new() { Location = new Point(144, 52), AutoSize = true, Text = "Artist:" };
            tb_Artist = new() { Location = new Point(144, 70), Size = new(478, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Unknown Artist" };
            lbl_Album = new() { Location = new Point(144, 96), AutoSize = true, Text = "Album:" };
            tb_Album = new() { Location = new Point(144, 114), Size = new(478, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Unknown Album" };
            lbl_FilePath = new() { Location = new Point(10, 139), AutoSize = true, Text = "Location:" };
            tb_FilePath = new() { Location = new Point(10, 157), Size = new(508, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "No File Found" };
            lbl_Duration = new() { Location = new Point(10, 183), AutoSize = true, Text = "Duration:" };
            tb_Duration = new() { Location = new Point(10, 200), Size = new(78, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "4:20" };
            lbl_Genre = new() { Location = new Point(94, 183), AutoSize = true, Text = "Genre:" };
            tb_Genre = new() { Location = new Point(94, 200), Size = new(237, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Unknown Genre" };
            lbl_TrackNumber = new() { Location = new Point(339, 183), AutoSize = true, Text = "Track Number:" };
            tb_TrackNumber = new() { Location = new Point(339, 200), Size = new(100, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = $"1 / 10" };
            lbl_Date = new() { Location = new Point(445, 183), AutoSize = true, Text = "Date:" };
            tb_Date = new() { Location = new Point(445, 200), Size = new(73, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "6969" };
            lbl_BitRate = new() { Location = new Point(524, 183), AutoSize = true, Text = "Bit Rate:" };
            tb_BitRate = new() { Location = new Point(524, 200), Size = new(78, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "320kbps" };
            lbl_PlayCount = new() { Location = new Point(10, 226), AutoSize = true, Text = "Play Count:" };
            tb_PlayCount = new() { Location = new Point(10, 244), Size = new(100, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "0" };
            lbl_PlayCompleteCount = new() { Location = new Point(116, 226), AutoSize = true, Text = "Complete Plays:" };
            tb_PlayCompleteCount = new() { Location = new Point(116, 244), Size = new(100, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "0" };
            lbl_SkipCount = new() { Location = new Point(222, 226), AutoSize = true, Text = "Skip Count:" };
            tb_SkipCount = new() { Location = new Point(222, 244), Size = new(100, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "0" };
            lbl_RatingScore = new() { Location = new Point(328, 226), AutoSize = true, Text = "Rating Score:" };
            tb_RatingScore = new() { Location = new Point(328, 244), Size = new(100, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "0" };
            lbl_UserRating = new() { Location = new Point(434, 226), AutoSize = true, Text = "User Rating:" };
            tb_UserRating = new() { Location = new Point(434, 244), Size = new(84, 23), EnableBorder = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Neutral" };
            lbl_Comments = new() { Location = new Point(10, 270), AutoSize = true, Text = "Comments:" };
            tb_Comments = new() { Location = new Point(10, 288), Size = new(612, 148), EnableBorder = true, TextAlign = ContentAlignment.TopLeft, Text = "Comment" };

            //Buttons
            btn_OpenFolder = new() { Location = new Point(524, 156), Size = new(100, 23), Text = "Open Folder", TabIndex = 0, RoundCorners = true, CornerRadius = 5 };
            btn_CloseDialog = new() { Location = new Point(547, 447), Size = new(75, 23), Text = "Close", TabIndex = 1, RoundCorners = true, CornerRadius = 5 };
        }

        /// <summary>
        /// Arranges and adds the controls to the form.
        /// </summary>
        /// <remarks>This method organizes the layout of various controls, including text boxes, labels, 
        /// and buttons, by adding them to the form's <see cref="Control.ControlCollection"/>.  It ensures that all
        /// necessary UI elements are included in the form for proper display  and interaction.</remarks>
        private void LayoutControls()
        {
            this.Controls.AddRange(new Control[]
                {
                    pb_AlbumArtBox,
                    tb_Title, tb_Artist, tb_Album, tb_FilePath, tb_Genre,
                    tb_Date, tb_TrackNumber, tb_BitRate, tb_Comments,
                    tb_PlayCount, tb_SkipCount, tb_RatingScore, tb_PlayCompleteCount, tb_UserRating, tb_Duration,

                    lbl_Title, lbl_Artist, lbl_Album, lbl_FilePath, lbl_Genre, lbl_Date,
                    lbl_TrackNumber, lbl_BitRate, lbl_Comments, lbl_PlayCount, lbl_SkipCount, lbl_RatingScore,
                    lbl_PlayCompleteCount, lbl_UserRating, lbl_Duration,

                    btn_OpenFolder, btn_CloseDialog
                });
        }

        /// <summary>
        /// Subscribes to the click events of the dialog's buttons.
        /// </summary>
        /// <remarks>This method attaches event handlers to the <see cref="btn_CloseDialog"/> and  <see
        /// cref="btn_OpenFolder"/> buttons. The handlers are invoked when the respective  buttons are
        /// clicked.</remarks>
        private void HookEvents()
        {
            btn_CloseDialog.Click += (s, e) => Close_Click();
            btn_OpenFolder.Click += (s, e) => OpenFolder_Click();
            this.FormClosing += (s, e) => { this.Dispose(); };
        }

        #endregion Initialization Helpers

        #region Helper Methods

        /// <summary>
        /// Opens the folder containing the specified file in Windows Explorer and selects the file.
        /// </summary>
        /// <remarks>This method uses the file path stored in the <c>_filePath</c> field. If the file path
        /// is null, empty,  or consists only of whitespace, the method does nothing. The method launches Windows
        /// Explorer  and highlights the file specified by <c>_filePath</c>.</remarks>
        private void OpenFolder_Click()
        {
            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string explorerPath = Path.Combine(windowsDir, "explorer.exe");

                Execution.RunExecutable(explorerPath, $"/select,\"{_filePath}\"");
            }
        }

        /// <summary>
        /// Handles the click event for the Close button, closing the current dialog with an <see
        /// cref="DialogResult.OK"/> result.
        /// </summary>
        /// <remarks>This method sets the <see cref="DialogResult"/> property to <see
        /// cref="DialogResult.OK"/>  and then closes the dialog. It is typically used to confirm an action and close
        /// the dialog.</remarks>
        private void Close_Click() { DialogResult = DialogResult.OK; this.Close(); }

        /// <summary>
        /// Populates the UI with detailed information about the specified track.
        /// </summary>
        /// <remarks>This method updates various UI elements, such as text boxes and image controls, to
        /// reflect the track's metadata,  including title, artist, album, file path, duration, play counts, user
        /// rating, genre, release year, track number,  bit rate, and comments. If the track contains album art, it is
        /// displayed in the album art box.</remarks>
        /// <param name="track">The <see cref="Track"/> object containing metadata and playback statistics to display.  This parameter
        /// cannot be <see langword="null"/>.</param>
        private void PopulateTrackInfo(Track track)
        {
            this.Text = $"Track Information - {track.ToString()}";
            tb_Title.Text = track.Title;
            tb_Artist.Text = track.Artist;
            tb_Album.Text = track.Album;
            tb_FilePath.Text = track.FilePath;
            _filePath = track.FilePath;
            tb_Duration.Text = $"{TimeSpan.FromSeconds((long)track?.DurationSeconds):mm\\:ss}";

            tb_PlayCount.Text = track.PlayCount.ToString();
            tb_PlayCompleteCount.Text = track.PlayCompleteCount.ToString();
            tb_SkipCount.Text = track.SkipCount.ToString();
            tb_RatingScore.Text = track.RatingScore.ToString();

            if (!track.Liked && !track.Disliked)
                tb_UserRating.Text = "Neutral";
            else if (track.Liked && !track.Disliked)
                tb_UserRating.Text = "Liked";
            else tb_UserRating.Text = "Disliked";

            using var tagFile = TagLib.File.Create(_filePath);

            var sb = new StringBuilder();
            for (int i = 0; i < tagFile.Tag.Genres.Count(); i++)
            {
                sb.Append(tagFile.Tag.Genres[i]);
            }
            tb_Genre.Text = sb.ToString();

            tb_Date.Text = tagFile.Tag.Year.ToString();
            tb_TrackNumber.Text = $"{tagFile.Tag.Track} / {tagFile.Tag.TrackCount}";
            tb_BitRate.Text = $"{tagFile.Properties.AudioBitrate} kbps";
            tb_Comments.Text = tagFile.Tag.Comment;

            if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
            {
                var pic = tagFile.Tag.Pictures[0];
                pb_AlbumArtBox.Image = GetImage(pic.Data.Data);
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // TrackInformationDialog
            // 
            ClientSize = new Size(284, 261);
            Name = "TrackInformationDialog";
            ResumeLayout(false);

        }

        /// <summary>
        /// Creates a resized <see cref="Bitmap"/> from the provided byte array representing an image.
        /// </summary>
        /// <remarks>The method maintains the aspect ratio of the original image while ensuring that the
        /// resized image does not exceed the specified dimensions. If the original image is smaller than the specified
        /// dimensions, it is returned without resizing.</remarks>
        /// <param name="bytes">The byte array containing the image data. Must represent a valid image format.</param>
        /// <param name="resizeWidth">The maximum width of the resized image, in pixels. Defaults to 128.</param>
        /// <param name="resizeHeight">The maximum height of the resized image, in pixels. Defaults to 128.</param>
        /// <returns>A <see cref="Bitmap"/> object representing the resized image, or <see langword="null"/> if the input byte
        /// array does not contain valid image data.</returns>
        private static Bitmap? GetImage(Byte[] bytes, int resizeWidth = 128, int resizeHeight = 128)
        {
            using var ms = new MemoryStream(bytes);
            using var original = Image.FromStream(ms);

            int newWidth = original.Width;
            int newHeight = original.Height;

            if (newWidth > resizeWidth || newHeight > resizeHeight)
            {
                float ratioX = (float)resizeWidth / newWidth;
                float ratioY = (float)resizeHeight / newHeight;
                float ratio = Math.Min(ratioX, ratioY);

                newWidth = (int)(newWidth * ratio);
                newHeight = (int)(newHeight * ratio);

            }
            var img = new Bitmap(original, new Size(newWidth, newHeight));
            original.Dispose();
            ms.Dispose();
            return img;
        }
        #endregion Helper Methods
    }
}

