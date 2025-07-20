# SendWsCommand Utility

**SendWsCommand** is a lightweight command-line utility designed to send JSON-formatted WebSocket messages — designed especially for use with **Streamer.bot**, **Stream Deck**, or any automation system that can launch external executables. This tool adds remote control capabilities to applications that use WebSocket servers for receiving JSON commands.

## Features

- Sends JSON-formatted commands to a WebSocket server.
- Ideal for controlling an MP3 player or similar tools that accept WebSocket-based commands.
- Self-contained, portable executable — no need for external dependencies.

## Usage

You can run this tool from any automation system that supports launching external executables with arguments.

```bash
SendWsCommand.exe <WebSocketServerURL> <Command> [Value]
```

### Example Commands

```bash
SendWsCommand.exe localhost:8080/comm next
SendWsCommand.exe localhost:8080/comm volume 69
```

- The first argument is the WebSocket server address and endpoint (e.g., `host:port/path`).
- The second argument is the command name.
- The optional third argument is the value associated with the command.

**Example output JSON:**

- `{"Command": "next"}`
- `{"Command": "volume", "Value": "69"}`

## Key Features

- Self-contained executable (~12MB):
  - No need to install .NET or other runtimes.
  - Trimmed from a typical ~50MB application for minimal footprint.
- Fully portable — can run from any directory.
- Optimized for bundling with the MP3 player binary distribution.

## Integration

Although optional, this utility is highly recommended when using:

- **Streamer.bot**
- **Stream Deck**
- Custom automation scripts

By bundling this tool with your MP3 player, users can easily set up remote control actions using simple command-line calls from their favorite automation tools.
