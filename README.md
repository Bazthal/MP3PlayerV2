# MP3 Player

A lightweight, themable MP3 player built for streamers and power users.  
Powered by the **BazthalLib** UI framework, which emphasizes modern design, customization, and remote integration.

![Theme Preview](https://imgur.com/V34L196.png)

---

## Features

### Audio Output Selection
- Instantly switch between system and virtual audio devices  
- No restart required  
- Great for switching between stream mix, speakers, and virtual cables

### Playback Modes
- **Sequential** – Play tracks in order  
- **Repeat Playlist / Repeat Track** – Loop the full playlist or current track  
- **Random Track** – Traditional random playback  
- **Smart Shuffle** – Plays unplayed tracks first, then least played  

### Playlist Management
- Create, save, and load multiple playlists  
- Powerful context menu with:
  - **Add / Remove Tracks**
  - **Edit Playlist**
  - **Shuffle or Clear Playlist**
  - **Sorting by:**
    - Artist (A-Z / Z-A)
    - Title (A-Z / Z-A)
    - Play Count (High → Low / Low → High)
    - Last Played (Newest → Oldest / Oldest → Newest)

### WebSocket Remote Control
Seamless remote integration for streaming or automation setups. The built-in **WebSocket server** broadcasts track updates and accepts basic playback commands.

#### Features:
- **Remote Control:** Play, Pause, Next, Previous (customizable via tools like Streamer.bot)
- **Now Playing Broadcast:** Sends JSON payload when a track changes to /nowplaying endpoint

#### Example JSON Payload:
```json
{
  "NowPlaying": "The Jam - Going Underground",
  "PlayCount": 1,
  "LastPlayed": "2025-07-19T21:25:06.8598442Z",
  "LocalTime": "19/07/2025 22:25"
}
```

- `NowPlaying`: Current track metadata  
- `PlayCount`: Number of times the track has been played  
- `LastPlayed`: UTC timestamp  
- `LocalTime`: Display-formatted local time  

#### Server Configuration:
- Set custom **IP**, **Port**, and **Endpoint**
- Optional **Auto Start** toggle
- Manual **Start/Stop** buttons included

### Supported WebSocket Commands

### JSON Format
```json
{ "Command": "command_name", "Value": "optional_value" }
```

### CLI Format
```bash
SendWsCommand.exe <host:port/endpoint> <command> <value>
```

Example:
```bash
SendWsCommand.exe localhost:8080/comm select random
```

## Playback Commands

| Command     | Value | Description |
|-------------|--------|-------------|
| `Play`      | –      | Starts/resumes playback. |
| `Pause`     | –      | Pauses current playback. |
| `Stop`      | –      | Stops playback. |
| `Next`      | –      | Skips to the next track. Fails if playlist is empty. |
| `Previous`  | –      | Returns to the previous track. Fails if playlist is empty. |
| `NowPlaying`| –      | Returns current track info if something is playing. |

---

## Volume & Device

| Command     | Value          | Description |
|-------------|----------------|-------------|
| `Volume`    | `0–100`        | Sets playback volume to the specified percentage. |
| `Device`    | *string*       | Changes audio output device by name match. Requires at least two available devices. |

---

## Playback Mode

| Command          | Value                            | Description |
|------------------|----------------------------------|-------------|
| `PlaylistMode`   | `Sequential`, `Repeat Track`, `Repeat Playlist`, `Random`, `Smart Shuffle` | Sets the playlist playback mode. Partial matches allowed (e.g., `"repeat"`). |

---

## Playlist Interaction

| Command     | Value                   | Description |
|-------------|-------------------------|-------------|
| `Shuffle`   | –                       | Shuffles the current playlist. Requires at least 2 tracks. |
| `Select`    | *Track name*, `random`, or index | Plays the first matching track by name, a random track, or by index. |
| `List`      | *Track name* or `unplayed` | Returns an array of matching tracks or all unplayed tracks. |
| `Count`     | *Track name* or `unplayed` | Returns the count of matching or unplayed tracks. |

---

## Example CLI Usage

```bash
SendWsCommand.exe localhost:8080/comm play
SendWsCommand.exe localhost:8080/comm pause
SendWsCommand.exe localhost:8080/comm select "Under Pressure"
SendWsCommand.exe localhost:8080/comm playlistmode "Smart Shuffle"
SendWsCommand.exe localhost:8080/comm device "Cable Output"
```

---

## Notes

- Keys `"Command"` and `"Value"` are **case-sensitive**.
- Values like track names or filter options (`"unplayed"`) are **not case-sensitive**.
- Commands return human-readable responses via WebSocket for success/failure.
- Use your `SendWsCommand.exe` in Streamer.bot or any automation tool via command-line execution.

- OBS overlays (via JSON source or script)
- Twitch chat bots (Streamer.bot, NodeCG, etc.)
- Companion apps or mobile controllers

---

## Custom Theme Support

Theme every aspect of the UI:
- Background, Text, Accent, Borders, Selected/Disabled items  
- Color picker supports:
  - RGB / Hex / HSL
  - Quick palette selections
  - Updates on runtime
  - Light / Dark switching is based on system color mode at app startup
	- Default colors were set for this in mind
  - Automatic saving/loading of theme configuration with fallback to default style if currupted.
- Backed by the **BazthalLib** theming system for dynamic and reusable styles

---

## UI Customization – Buttons & Icons

All buttons (playback, shuffle, settings, etc.) use a **tintable image renderer** that allows full theming:

- Tinted images adapt to **AccentColor**
- Shape of the button is conformed around the shape of the image.
- Consistent visual identity across the app
- Enables dynamic recoloring for themes without requiring multiple image assets

This approach supports:
- User-defined theme colors
- Streamlined icon management

---

## Screenshots

> Interface examples with themes, settings, and playlist options

| Audio Device | Sorting Menu | Theme | HotPink Theme | Settings | Playback Options |
|-------------|--------------|-------|---------------|----------|------------------|
| ![Audio](https://imgur.com/I7w1BKh.png) | ![Sort](https://imgur.com/OGDzOzs.png) | ![Main](https://imgur.com/V34L196.png) | ![HotPink](https://imgur.com/4ZK0z4b.png)| ![Settings](https://imgur.com/bJfe3tr.png) | ![Options](https://imgur.com/rJsjFmj.png) |

---

## Notes

- Built from real streaming & usage needs  
- Designed for power users needing flexible control  
- No bloat – lightweight, responsive, and highly configurable  
- Handles large music collections - 22k tracks tested.
- A loading screen is displayed
- Ideal as a background app or interactive stream overlay controller  
---

## Installation

Coming soon!  
Executable + installer in development.

> Source code will be released with roadmap and contribution guide once ready.

---

## Feedback & Contributions

Suggestions and feature ideas are always welcome.  
Open to collaboration after public release.

---

## Licenses

This project is licensed under the MIT License.

It uses the following third-party libraries:

- [TagLib#](https://github.com/mono/taglib-sharp) – LGPL-2.1
- [CSCore](https://github.com/filoe/cscore) – Microsoft Public License (Ms-PL)
- [LiteDB](https://github.com/mbdavid/LiteDB) – MIT
- [websocket-sharp](https://github.com/sta/websocket-sharp) – MIT

Please consult their respective licenses for more information

---

##  Powered By

- **BazthalLib** – Custom themable controls and reusable component system  
- Designed for integration with tools like **Streamer.bot** and **Twitch chat**  
- Built to grow with your streaming setup
