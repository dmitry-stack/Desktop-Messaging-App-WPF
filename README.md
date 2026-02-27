# WPF Chat Application

A real-time TCP chat application built with C# and WPF, consisting of a standalone **Server** and a **Client**. Multiple clients can connect simultaneously, authenticate with a password, and exchange messages in real time. The server operator can broadcast messages and kick users from the admin panel.

---

## Projects

### Server 
A WPF application that hosts the TCP listener and manages all connected clients. It provides a GUI for monitoring connections, viewing logs, sending server-wide messages, and kicking users.

### Client 
A WPF chat client with a message bubble UI. Users connect to a server by entering a username, password, and server address. Messages from the current user appear on the right; messages from others appear on the left. System and server messages are centered.

---

## Features

- Password-protected server with per-connection authentication
- Real-time message broadcasting to all connected clients
- Join/leave system notifications
- Server-side message broadcasting from the admin panel
- Kick functionality to forcibly disconnect a user
- Relative timestamps on message bubbles (e.g. "Now", "5m ago", "14:32"), refreshed every 30 seconds
- Clean disconnect handling on both client and server sides

---

## Requirements

- .NET 8.0 or later
- Windows (WPF is Windows-only)
- Visual Studio 2022 (recommended) or the `dotnet` CLI

---

## Getting Started

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd <your-repo-folder>
```

### 2. Build both projects

Open the solution in Visual Studio and build, or use the CLI:

```bash
dotnet build Server/Server.csproj
dotnet build Wpf2/Wpf2.csproj
```

### 3. Start the Server

Run the `Server` project. In the server window:

1. Enter the **IP address** to listen on (e.g. `127.0.0.1` for local, or your machine's LAN IP for network access).
2. Enter a **port** (e.g. `5000`).
3. Enter a **password** that clients will use to authenticate.
4. Click **Start**.

### 4. Connect a Client

Run the `Wpf2` project. In the chat window:

1. Go to **File → Connect**.
2. In the login dialog, enter:
   - **Username** — your display name
   - **Password** — must match the server password
   - **Address** — the server's IP address
   - **Port** — the server's port
3. Click **Submit**.

Once authenticated, you can start sending messages immediately.

---

## Usage

### Sending Messages

Type in the input box at the bottom and press **Enter** or click the send button. Hold **Shift + Enter** to insert a newline without sending.

### Server Admin Panel

| Control | Description |
|---|---|
| Connected users list | Shows all currently connected usernames |
| Log panel | Timestamped log of all connections, disconnections, and messages |
| Server message box | Type a message and press Enter or click Send to broadcast as `[SERVER]` |
| Kick button | Select a user from the list and click Kick to forcibly disconnect them |

---

## Project Structure

```
/
├── Server/
│   ├── MainWindow.xaml         # Server admin UI layout
│   ├── MainWindow.xaml.cs      # Server logic: listener, client handling, broadcast, kick
│   └── App.xaml(.cs)
│
└── Wpf2/
    ├── MainWindow.xaml         # Main chat window layout
    ├── MainWindow.xaml.cs      # Client logic: connect, send, receive, disconnect
    ├── LoginForm.xaml          # Login dialog layout
    ├── LoginForm.xaml.cs       # Login dialog logic and TCP handshake
    ├── UserControl1.xaml       # Message bubble UI layout
    ├── UserControl1.xaml.cs    # Message bubble logic and timestamp formatting
    ├── MessageModel.cs         # Data model for a single message (INotifyPropertyChanged)
    └── App.xaml(.cs)
```

---

## Authentication Protocol

Communication uses plain UTF-8 text over TCP. On connection, the client sends credentials as a single line:

```
username|password\n
```

The server responds with either:

```
OK
```
or
```
AUTH_FAIL
```

After a successful `OK`, all subsequent lines from the client are treated as chat messages and broadcast to all other connected clients in the format `username: message`.

---

## Known Limitations

- **No encryption** — credentials and messages are transmitted as plaintext. Do not use over untrusted networks.
- **No message history** — clients only see messages received after they connect.
- **Single server password** — all users share the same password; there are no individual user accounts.
