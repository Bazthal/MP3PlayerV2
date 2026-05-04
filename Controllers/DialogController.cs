using BazthalLib.Controls;
using BazthalLib.Systems.IO;
using BazthalLib.UI;
using MP3PlayerV2.Models;
using MP3PlayerV2.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP3PlayerV2.Controllers
{
    /// <summary>
    /// Coordinates dialog operations and file handling.
    /// </summary>
    public class DialogController
    {
        private readonly PlaylistController _playlistController;
        private readonly Form _parentForm;

        private static readonly HashSet<string> SupportedAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
            { ".flac", ".m4a", ".mp2", ".mp3", ".wav", ".wma" };
        private static readonly HashSet<string> SupportedPlaylistExtensions = new(StringComparer.OrdinalIgnoreCase)
            { ".m3u", ".m3u8", ".jsonpl" };

        public DialogController(PlaylistController playlistController, Form parentForm)
        {
            _playlistController = playlistController ?? throw new ArgumentNullException(nameof(playlistController));
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
        }

        /// <summary>
        /// Shows a processing dialog centered on the parent form.
        /// </summary>
        public ThemableProcessingDialog ShowProcessingDialog(string title, bool showProgress = true, bool showCancelButton = true)
        {
            var dialog = new ThemableProcessingDialog(title, showProgress, showCancelButton)
            {
                StartPosition = FormStartPosition.Manual,
                Icon = _parentForm.Icon
            };

            dialog.Location = new Point(
                _parentForm.Location.X + (_parentForm.Width - dialog.Width) / 2,
                _parentForm.Location.Y + (_parentForm.Height - dialog.Height) / 2
            );

            dialog.Show(_parentForm);
            return dialog;
        }

        /// <summary>
        /// Handles adding files to the playlist with progress dialog.
        /// </summary>
        public async Task<List<Track>> AddFilesAsync(string[]? files = null, Action? onComplete = null)
        {
            files ??= Files.ChooseFiles("", "Music Files|*.flac;*.m4a;*.mp2;*.mp3;*.wav;*.wma");
            if (files == null || files.Length == 0)
                return new List<Track>();

            var dialog = ShowProcessingDialog("Adding tracks...");

            try
            {
                var newTracks = await _playlistController.ProcessFilesAsync(
                    files,
                    (current, total, eta) => dialog.SetProgress("Track", current, total, eta),
                    replacePlaylist: false,
                    dialog.Token
                );

                dialog.SetCompleted($"Added {newTracks.Count} track(s).");
                dialog.CloseAfter(1000);

                onComplete?.Invoke();
                return newTracks;
            }
            catch (OperationCanceledException)
            {
                dialog.SetCompleted("Operation cancelled.");
                dialog.CloseAfter(1000);
                return new List<Track>();
            }
            catch (Exception ex)
            {
                dialog.SetCompleted($"Error: {ex.Message}");
                dialog.CloseAfter(2000);
                return new List<Track>();
            }
        }

        /// <summary>
        /// Handles loading a playlist with progress dialog.
        /// </summary>
        public async Task LoadPlaylistAsync(string? filePath = null)
        {
            filePath ??= Files.ChooseFile("", PlaylistController.GetPlaylistFilter(), "Open Playlist");
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var dialog = ShowProcessingDialog("Loading Playlist");
            var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                switch (ext)
                {
                    case ".jsonpl":
                        await _playlistController.LoadFromJsonPl(filePath);
                        break;
                    case ".m3u":
                    case ".m3u8":
                        await _playlistController.LoadFromM3U(filePath,
                            (current, total, eta) => dialog.Invoke(() => dialog.SetProgress("Track", current, total, eta)),
                            dialog.Token);
                        break;
                }

                dialog.SetCompleted("Playlist loaded.");
            }
            catch (OperationCanceledException)
            {
                dialog.SetCompleted("Cancelled by user.");
            }
            catch (Exception ex)
            {
                dialog.SetCompleted($"Error: {ex.Message}");
            }
            finally
            {
                dialog.CloseAfter(1000);
            }
        }

        /// <summary>
        /// Handles saving a playlist with progress dialog.
        /// </summary>
        public async Task SavePlaylistAsync(List<Track>? trackList = null, string? defaultExtension = null)
        {
            string filter = PlaylistController.GetPlaylistFilter(defaultExtension ?? ".m3u");
            string saveFileName = Files.SaveFile("", filter, "Save Playlist", true);
            if (string.IsNullOrWhiteSpace(saveFileName))
                return;

            var dialog = ShowProcessingDialog("Saving Playlist...", showCancelButton: false);
            var ext = System.IO.Path.GetExtension(saveFileName).ToLowerInvariant();

            try
            {
                switch (ext)
                {
                    case ".jsonpl":
                        await _playlistController.SaveToJsonPl(saveFileName, trackList);
                        break;
                    case ".m3u":
                    case ".m3u8":
                        await _playlistController.SaveToM3U(saveFileName, trackList);
                        break;
                }

                dialog.SetCompleted("Playlist saved successfully.");
            }
            catch (Exception ex)
            {
                dialog.SetCompleted($"Error: {ex.Message}");
            }
            finally
            {
                dialog.CloseAfter(1000);
            }
        }

        /// <summary>
        /// Handles dropped files, identifying audio files and playlists.
        /// </summary>
        public async Task<bool> HandleDroppedFilesAsync(string[] files, bool autoPlay = false, Action<bool>? onPlaylistLoaded = null)
        {
            List<string> validAudioFiles = new();

            foreach (var path in files)
            {
                if (System.IO.Directory.Exists(path))
                {
                    validAudioFiles.AddRange(FindAudioFiles(path));
                }
                else if (System.IO.File.Exists(path))
                {
                    string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();

                    if (SupportedAudioExtensions.Contains(ext))
                    {
                        validAudioFiles.Add(path);
                    }
                    else if (SupportedPlaylistExtensions.Contains(ext))
                    {
                        if (validAudioFiles.Count <= 0)
                        {
                            await LoadPlaylistAsync(path);
                            onPlaylistLoaded?.Invoke(autoPlay);
                            return true;
                        }
                    }
                }
            }

            if (validAudioFiles.Count > 0)
            {
                await AddFilesAsync(validAudioFiles.ToArray());
                onPlaylistLoaded?.Invoke(false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Recursively finds audio files in a folder.
        /// </summary>
        private static List<string> FindAudioFiles(string folderPath)
        {
            var list = new List<string>();

            try
            {
                if (!System.IO.Directory.Exists(folderPath))
                    return list;

                foreach (var file in System.IO.Directory.EnumerateFiles(folderPath, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    if (SupportedAudioExtensions.Contains(System.IO.Path.GetExtension(file)))
                    {
                        list.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                BazthalLib.DebugUtils.Log("DialogController", "FindAudioFiles",
                    $"Error scanning folder '{folderPath}': {ex.Message}", logLevel: BazthalLib.DebugUtils.LogLevel.Error);
            }

            return list;
        }
    }
}
