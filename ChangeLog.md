# MP3PlayerV2 ChangeLog

## 1.2.2

### UI & Dialog Enhancements
- Set processing dialog to use the application icon during initialization
- Simplified the logic for restoring the current track selection after shuffling the playlist order

### Stability & Bug Fixes
- Fixed potential crash from call `NextTrack` without playing anything before

### Library & Dependency Updates
- Updated BazthalLib to version 1.1.5 to enable listbox sorting

### Playlist Management
- Enhanced playlist management with new methods for reordering tracks in `PlaylistManager`.

## 1.2.1

### Track Information Dialog
- Added `TrackInformationDialog` to show more track information to the user 

### Command Handling & Extensibility
- `CommandDispatcher` now loads external command DLLs from the `Plugins` folder for extensibility.
  - Enhanced error handling in `CommandDispatcher` to gracefully manage null values and loading failures.
- Added `GlobalErrorCatcher` in `program.cs` to enhance exception handling and logging.
- - The response from connecting to "Now Playing" endpoint is now formatted as JSON to match the rest of the responses.
- Added `GetByText` method to `PlaylistManager` to return a track by matching a string.
  - This enables users to re-order playlists in a future update.

### Smart Shuffle
- Introduced *Smart Shuffle* modes: `Liked` (plays only liked tracks) and `AvoidDisliked` (skips disliked tracks) for more personalized playback experience.
  - This helps build the play data database with more accurate usage information.

### Bug Fixes
- Fixed previously commented-out keybinds in the listbox, restoring focused playback controls.

## 1.2.0 - Massive Overhaul

### Major Features & Enhancements
- **Track Queuing:** Added `QueueTrackByName` method to queue tracks by name. Queued tracks now play first, regardless of playlist mode.
- **Advanced Command Handling:** Enhanced `CommandContext` with new actions and properties for advanced track queuing. Introduced `QueueCommand` and `ResetStatsCommand` for queue management and statistics reset.
- **UI Improvements:** Added menu options to reset track statistics. Moved `EnsureVisible` logic to the selected index changed event for consistent UI behavior.
- **Database Overhaul:**
  - Improved `TrackDatabase.cs` with methods to check track existence and reset statistics.
  - Changed track ID from hash to GUID for faster loading (new database is incompatible with previous versions).
  - Added import method to migrate existing play data into the new database (import before playing music files to avoid duplicates).
  - Implemented asynchronous database writes via `LiteDbWriteQueue`.
  - Database file is now stored in a subfolder to keep the root directory clear.
  - Added `PruneOldDataAsync` to automatically remove outdated entries if tracks have not been updated within a specified number of days.
- **Track History:** Updated track history to use a limited stack of track models instead of integers for improved navigation and data integrity.
- **Track Ratings:** Introduced `TrackRatingManager` for automated track preference ratings based on playback interaction.
- **Command Enhancements:** The `Count` and `List` commands now support counting and listing tracks by their rating status, including liked, disliked, unplayed, and total tracks.
- When loading large music collections (100+ tracks), garbage collection is triggered to prevent excessive memory usage.

### Playback & Audio
- Skipping the last few seconds of a track no longer counts as a valid skip.
- Playback now resumes without always restarting the current track.
- Fixed `PlayTimer` to restart correctly when playback resumes.
- Switching the audio output device now restarts the playback timer, ensuring accurate timing if the timer was previously paused.

### File & Instance Management
- Player can now load files by dragging them onto the executable or by providing them as command line arguments.
  - Paths now use the application's startup directory, ensuring files are referenced with absolute paths.
- Enforces single-instance operation (when not debugging), forwarding any launch arguments to the first instance for seamless file association support.
- When launched via file association, the player will automatically start playback: the first track for playlists, and the most recently added track for individual music files.

### Miscellaneous
- Corrected command names and response messages across multiple commands.
- Commands now support an optional `Range` parameter for batch operations.
- Added a new application icon designed to complement the default dark theme.
- Added an option in the settings window to restore all settings to their default values.

## 1.1.2

- Fixed pause not functioning from a inverted check
- Updated BazthalLib to version 1.1.3 to resolve trackbar resizing issues.
- Introduced a new `sort` command in command handling logic.
- Enhanced `PlayerCommand` class with an optional `Order` property.
- Improved JSON deserialization to be case-insensitive for better command handling.
- Enhanced `PreviousTrack` to restart the current track if played for over 5 seconds.
  - Added a `Stack<int>` for track history to navigate back to previously played tracks.
- Introduced new command classes (`NextCommand`, `PauseCommand`, `PlayCommand`, etc.) implementing `ICommandHandler`.
- Introduced `CommandContext` for structured command execution handling.
- Improved overall project structure for better organization and maintainability.
- Modified `SetVolume` to accept a volume parameter
- Updated WebSocket command handling to broadcast responses to all clients.
  - This should allow for streamer.bot WebSocket Client message trigger automation based on commands
- The Processing window would now correct display at the centre of the player where ever it gets moved to
  - Loading process for playlist and adding items can now be cancelled and would add what's processed already

## 1.0.2
- Implemented drag-and-drop functionality for the playlist, allowing users to add files and folders directly.
- Updated JSON response handling by introducing `BuildResponseMessage` and correcting spelling errors in variable names.
- Enhanced keyboard shortcuts for playback control and improved audio file/playlist loading processes.
- Updated project version to 1.0.2 and made minor README adjustments.
- Applied code style improvements for consistency and readability.

## 1.0.1
- Added `resetUIText` method to reset UI text.
- Modified `InitilizeCSCore` to return a boolean.
- Improved `Play` method for better track handling.
- Updated `RemoveItem` to stop playback for selected items.
- Enhanced `ClearPlaylistButton_Click` to dispose audio resources.
- Allowed item deletion in `PlayList_KeyDown`.
- Revised README with new image links and feature descriptions.
- Deleted outdated binary image files.
- Images are now on imgur

## 1.0.0
- Initial Realease
