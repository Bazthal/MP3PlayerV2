# MP3PlayerV2 ChangeLog

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