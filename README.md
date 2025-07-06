# IGI.NET - Comprehensive Game Server Interaction Library

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/godzaryan/IGI.NET/blob/main/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/godzaryan/IGI.NET?style=social)](https://github.com/godzaryan/IGI.NET/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/godzaryan/IGI.NET?style=social)](https://github.com/godzaryan/IGI.NET/network/members)

![IGI.NET Banner](https://placehold.co/800x400/282C34/F0F0F0?text=IGI.NET+Game+Server+Interaction)

IGI.NET is a robust C# library meticulously crafted for deep interaction with **IGI (Project IGI 2: Covert Strike)** game servers. It provides a comprehensive and easy-to-use set of functionalities to query server status, retrieve detailed player information, listen for a wide array of in-game events, and even execute powerful RCON commands. Whether you're an aspiring developer building a custom server monitoring tool, a dedicated community member creating a unique game client, or an administrator designing an advanced utility, IGI.NET aims to significantly simplify your development process by abstracting complex network interactions and low-level game memory manipulation.

---

## 📖 Table of Contents

* [🌟 Features](#-features)
* [📦 Installation](#-installation)
* [🚀 Usage Overview](#-usage-overview)
    * [`IGIServer` - Server & Player Data](#igiserver---server--player-data)
    * [`IGICommander` - RCON & Advanced Events](#igicommander---rcon--advanced-events)
* [📚 Data Structures](#-data-structures)
* [🎮 Practical Examples](#-practical-examples)
* [🤝 Contributing](#-contributing)
* [📄 License](#-license)
* [📧 Contact](#-contact)

---

## 🌟 Features

IGI.NET is packed with features to give you unparalleled control and insight into your IGI game servers:

* **🌐 Real-time Server Metrics**: Instantly retrieve critical server data including hostname, current map, player counts, game mode, uptime, and more. Get a snapshot of your server's health and activity.
* **👥 Granular Player Insights**: Access detailed player information such as IDs, names, IP addresses, kill/death ratios, ping, team affiliations, in-game money, and even RCON admin status. Understand individual player performance and presence.
* **⚡ Dynamic Event Monitoring**: React to key in-game events as they unfold. Detect player joins/leaves, team changes, name changes, and crucial combat events like kills (with hit location, type, and weapon). Track player spawns, suicides, weapon purchases, and in-game chat messages.
* **🛠️ Powerful RCON Control**: Execute remote console commands to manage your server efficiently. Announce messages, kick players, restart maps, or navigate to specific maps with simple function calls.
* **🧠 Direct Memory Interaction**: For advanced use cases, manipulate in-game memory directly. Set and even "freeze" player values like money or health, enabling unique custom game modes or debugging scenarios.
* **🛡️ Robust & Asynchronous**: Designed with comprehensive error handling and asynchronous event processing, ensuring your applications remain stable and responsive.
* **📝 Integrated Logging**: Automatically logs library activity and errors, simplifying debugging and operational monitoring.
* **🚀 Server Lifecycle Management**: Programmatically start a dedicated IGI 2 server instance.
* **📊 Configuration Parsing**: Interpret detailed network configuration settings directly from server output.

---

## 📦 Installation

To integrate IGI.NET into your C# project:

1.  **Download `IGIdotNET.cs`**: Obtain the `IGIdotNET.cs` file from this repository.
2.  **Add to Your Project**: Include `IGIdotNET.cs` in your C# project via Visual Studio or your preferred IDE.
3.  **Dependencies**: Ensure your project references standard .NET libraries (`System.*`). Crucially, this library relies on a separate memory-editing library, typically named `Memory.dll`. You will need to acquire this dependency and add it as a reference to your project.

---

## 🚀 Usage Overview

IGI.NET provides two core classes: `IGIServer` for general server queries and high-level events, and `IGICommander` for RCON, detailed log-based events, and memory manipulation.

### `IGIServer` - Server & Player Data

The `IGIServer` class is your gateway to essential server and player data, fetched via UDP queries.

**Initialization:**
Connect to your IGI server by providing its IP address and port:

```csharp
using IGI.NET;
IGIServer server = new IGIServer("127.0.0.1", 26000); // Your server IP and Port
````

**Key Capabilities:**

  * **Instant Server Status**: Retrieve the server's hostname, current map, player count, and game type with `server.GetInfoData()`.
  * **Comprehensive Game Details**: Get a full overview of game settings, including team scores, round limits, and server rules using `server.GetStatusData()`.
  * **Live Player Roster**: Obtain a list of all active players, their scores, pings, and teams with `server.GetPlayersData()`.
  * **Real-time Event Streams**: Initiate background listeners to automatically detect and notify your application about:
      * Players joining or leaving (`server.PlayerJoined`, `server.PlayerLeft`).
      * Map changes (`server.MapChanged`).
      * Round transitions (`server.RoundOver`).
      * Player name or team changes (`server.PlayerNameChanged`, `server.PlayerTeamChanged`).

### `IGICommander` - RCON & Advanced Events

The `IGICommander` class provides administrative control via RCON and detailed event detection by parsing game logs, alongside direct memory interaction.

**Initialization:**
Connect to your server using its IP, port, and RCON password:

```csharp
using IGI.NET;
IGICommander commander = new IGICommander("127.0.0.1", 26000, "your_rcon_password");
```

**Key Capabilities:**

  * **Deep Event Insights**: By monitoring the game's `Multiplayer.log`, `IGICommander` can dispatch events for:
      * Player kills (including precise hit locations like `Head`, `Back`, `Chest` and `HitType` like `Bullet`, `Explosive`).
      * Spawn kills and team kills.
      * In-game chat messages.
      * Player spawns, suicides (e.g., `FellFromHeight`, `SentryGun`), and weapon purchases.
      * Objective completions and bomb placements.
      * Console commands executed on the server.
      * Detection of RCON administrators.
      * Memory-based events like changes in ground weapon count or map ID.
      * *Enable these by calling `await commander.ListenLogAsync(true);`*
  * **Full RCON Control**: Send powerful commands to your server:
      * `commander.lo_announce("Message");` - Broadcast server-wide announcements.
      * `commander.sv_kick(playerID);` - Remove problematic players.
      * `commander.sv_restartmap();` - Instantly restart the current map.
      * `commander.sv_gotomap(mapID);` - Change to any map by ID.
      * `commander.sv_teamdamage(true/false);` - Toggle friendly fire.
      * `commander.sv_finger();` - Retrieve detailed RCON player lists.
      * `commander.sv_listmaps();` - Get a list of all available maps.
  * **Direct Memory Manipulation**: Interact with the running game process (requires `Memory.dll`):
      * `commander.SetPlayerValue("PlayerName", "Offset", "Value", freeze: true);` - Modify player attributes (e.g., set money, freeze health).
      * `commander.UnfreezeAll();` / `commander.UnfreezePlayer("PlayerName");` - Release frozen values.
      * `commander.ResetAllPlayerStats();` - Clear all player statistics.
  * **Server Process Management**: Programmatically start a dedicated IGI 2 server instance with `commander.StartServer()`.

-----

## 📚 Data Structures

IGI.NET provides well-defined C\# structs and classes to represent all retrieved game data, ensuring type safety and ease of use. Key structures include:

  * **`BasicInfo`**: Core game details (name, version, location).
  * **`ServerInfo`**: Comprehensive server status (hostname, map, player counts, uptime).
  * **`PlayerMetadata` (IGIServer)**: Basic player stats (ID, name, kills, deaths, ping, team).
  * **`PlayersInfo`**: Team scores and a list of all players.
  * **`GameInfo`**: A consolidated view of all server and game settings, including player list.
  * **`PlayerMetadata` (IGICommander)**: Enhanced player data for event tracking (IP, admin status, weapon purchases, detailed stats).
  * **`Map`**: Map ID and name.
  * **`sv_finger_Player`**: Player data from RCON `sv_finger` command.
  * **Enumerations**: `HitLocation`, `HitType`, `SuicideType`, `PlayerState` for clear event context.
  * **`Networkconfig`**: Detailed server configuration parameters parsed from game output.

-----

## 🎮 Practical Examples

IGI.NET empowers you to build a variety of applications:

  * **Custom Server Dashboards**: Display live player counts, current map, and server health in a custom web or desktop application.
  * **Automated Admin Bots**: Create bots that automatically kick cheaters, announce server rules, or manage map rotations based on in-game events.
  * **Enhanced Game Clients**: Develop custom clients that provide richer player information, detailed kill feeds, or unique in-game statistics not available in the default client.
  * **Game Mode Modding**: Utilize memory manipulation to experiment with custom game rules, player abilities, or resource management.
  * **Community Tools**: Build tools for competitive matches, stat tracking, or player management for your IGI community.

The `Example Usage` section in the `IGIdotNET.cs` file (or a separate `Program.cs` in a demo project) provides a comprehensive C\# code example demonstrating how to initialize and use both `IGIServer` and `IGICommander` classes, subscribe to their events, and execute various commands.

-----

## 🤝 Contributing

We welcome contributions to IGI.NET\! If you have suggestions, bug reports, or want to contribute code, please feel free to:

1.  **Fork the repository.**
2.  **Create a new branch** for your feature or bug fix: `git checkout -b feature/your-feature-name` or `git checkout -b bugfix/fix-description`.
3.  **Make your changes** and ensure they adhere to the existing coding style.
4.  **Write clear, concise commit messages.**
5.  **Submit a Pull Request** explaining your changes and their benefits.

-----

## 📄 License

This project is licensed under the [MIT License](https://www.google.com/url?sa=E&source=gmail&q=https://github.com/godzaryan/IGI.NET/blob/main/LICENSE).

As the sole author and owner of this repository, I kindly request that any significant use, modification, or distribution of this code explicitly acknowledges my authorship. While the MIT license grants broad permissions, I appreciate being contacted for commercial projects or large-scale integrations.

-----

## 📧 Contact

For any questions, suggestions, or collaborations, feel free to reach out via:

  * **GitHub Issues**: [github.com/godzaryan/IGI.NET/issues](https://www.google.com/search?q=https://github.com/godzaryan/IGI.NET/issues)
  * **My GitHub Profile**: [github.com/godzaryan](https://www.google.com/search?q=https://github.com/godzaryan)

-----

*Built with ❤️ for the IGI Community.*
