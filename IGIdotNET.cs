using Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static IGI.NET.IGIEntity;

namespace IGI.NET
{
    public class IGIServer
    {
        //Config class
        public class IGIServerConfig
        {
            //Required configs
            public string IP;
            public int PORT;

            //Optional Configs
            public string BasicPacket = "basic";
            public string InfoPacket = "info";
            public string StatusPacket = "status";
            public string PlayersPacket = "players";
        }

        public IGIServerConfig config = new IGIServerConfig();

        public ServerInfo ServerInfo_Live = new ServerInfo();
        public PlayersInfo PlayersInfo_Live = new PlayersInfo();


        //Custom structs
        public struct BasicInfo
        {
            public string Game_name { get; }
            public string Game_version { get; }
            public string Location { get; }
            public string Query_id { get; }

            public BasicInfo(string gameName, string gameVersion, string location, string queryId)
            {
                Game_name = gameName;
                Game_version = gameVersion;
                Location = location;
                Query_id = queryId;
            }
        }
        public struct ServerInfo
        {
            public string Hostname { get; }
            public int Hostport { get; }
            public string Mapname { get; }
            public string Gametype { get; }
            public int Numplayers { get; }
            public int Maxplayers { get; }
            public string Gamemode { get; }
            public int Bandwidth { get; }
            public double Maxout { get; }
            public int Uptime { get; }
            public string Timeleft { get; }
            public int CurrentRound { get; }
            public int MaxRounds { get; }
            public string QueryId { get; }

            public ServerInfo(
            string hostname, int hostport, string mapname, string gametype,
            int numplayers, int maxplayers, string gamemode, int bandwidth,
            double maxout, int uptime, string timeleft, int currentRound, int maxRounds, string queryId)
            {
                Hostname = hostname;
                Hostport = hostport;
                Mapname = mapname;
                Gametype = gametype;
                Numplayers = numplayers;
                Maxplayers = maxplayers;
                Gamemode = gamemode;
                Bandwidth = bandwidth;
                Maxout = maxout;
                Uptime = uptime;
                Timeleft = timeleft;
                CurrentRound = currentRound;
                MaxRounds = maxRounds;
                QueryId = queryId;
            }

            public override string ToString()
            {
                string finalStr = "";
                PropertyInfo[] properties = typeof(ServerInfo).GetProperties();

                foreach (var property in properties)
                {
                    finalStr += $"{property.Name}: {property.GetValue(this)}\n";
                }
                return finalStr;
            }
        }
        public struct PlayerMetadata
        {
            public int PlayerID { get; }
            public string PlayerName { get; }
            public string PlayerFrags { get; }
            public string PlayerDeaths { get; }
            public int PlayerPing { get; }
            public int PlayerTeam { get; }

            public PlayerMetadata(int playerId, string playerName, string playerFrags, string playerDeaths, int playerPing, int playerTeam)
            {
                PlayerID = playerId;
                PlayerName = playerName;
                PlayerFrags = playerFrags;
                PlayerDeaths = playerDeaths;
                PlayerPing = playerPing;
                PlayerTeam = playerTeam;
            }

            public void PrintDetails()
            {
                PropertyInfo[] properties = typeof(PlayerMetadata).GetProperties();

                foreach (var property in properties)
                {
                    Console.WriteLine($"{property.Name}: {property.GetValue(this)}");
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is PlayerMetadata)
                {
                    var other = (PlayerMetadata)obj;
                    return PlayerID == other.PlayerID;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return PlayerID.GetHashCode();
            }
        }
        public class PlayersInfo
        {
            public string Team0Name { get; }
            public string Team1Name { get; }
            public int Team0Score { get; }
            public int Team1Score { get; }
            public string QueryId { get; }
            public List<PlayerMetadata> Players = new List<PlayerMetadata>();

            public PlayersInfo(string team0Name = "", string team1Name = "", int team0Score = 0, int team1Score = 0, string queryId = "", List<PlayerMetadata> players = null)
            {
                Team0Name = team0Name;
                Team1Name = team1Name;
                Team0Score = team0Score;
                Team1Score = team1Score;
                QueryId = queryId;
                if (players == null)
                {
                    Players = new List<PlayerMetadata>();
                }
                else
                {
                    Players = players;
                }
            }

            public void PrintDetails()
            {
                Console.WriteLine("Game Details:");
                PropertyInfo[] properties = typeof(PlayersInfo).GetProperties();

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(List<PlayerMetadata>))
                    {
                        Console.WriteLine($"{property.Name}:");
                        foreach (var player in Players)
                        {
                            player.PrintDetails();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{property.Name}: {property.GetValue(this)}");
                    }
                }
            }
        }
        public struct GameInfo
        {
            // Basic Info
            public string GameName { get; }
            public string GameVersion { get; }
            public string Location { get; }
            public string QueryId { get; }

            // Server Info
            public string Hostname { get; }
            public int Hostport { get; }
            public string MapName { get; }
            public string GameType { get; }
            public int NumPlayers { get; }
            public int MaxPlayers { get; }
            public string GameMode { get; }
            public int CurrentRound { get; }
            public int MaxRounds { get; }

            // Team Info
            public string Team0Name { get; }
            public string Team1Name { get; }
            public int Team0Score { get; }
            public int Team1Score { get; }

            // Additional Game Settings
            public int TimeLimit { get; }
            public bool AutoBalance { get; }
            public bool TeamDamage { get; }
            public bool Sniper { get; }
            public bool BombReposition { get; }
            public bool SpecMode { get; }
            public int PingMax { get; }
            public float PacketLossMax { get; }
            public int Bandwidth { get; }
            public float MaxOutput { get; }
            public bool Dedicated { get; }
            public bool PasswordProtected { get; }

            // List of Players
            public List<PlayerMetadata> Players { get; }

            public GameInfo(string gameName, string gameVersion, string location, string queryId,
                            string hostname, int hostport, string mapName, string gameType,
                            int numPlayers, int maxPlayers, string gameMode, int currentRound, int maxRounds,
                            string team0Name, string team1Name, int team0Score, int team1Score, int timeLimit,
                            bool autoBalance, bool teamDamage, bool sniper, bool bombReposition,
                            bool specMode, int pingMax, float packetLossMax, int bandwidth, float maxOutput,
                            bool dedicated, bool passwordProtected,
                            List<PlayerMetadata> players)
            {
                GameName = gameName;
                GameVersion = gameVersion;
                Location = location;
                QueryId = queryId;
                Hostname = hostname;
                Hostport = hostport;
                MapName = mapName;
                GameType = gameType;
                NumPlayers = numPlayers;
                MaxPlayers = maxPlayers;
                GameMode = gameMode;
                CurrentRound = currentRound;
                MaxRounds = maxRounds;
                Team0Name = team0Name;
                Team1Name = team1Name;
                Team0Score = team0Score;
                Team1Score = team1Score;
                TimeLimit = timeLimit;
                AutoBalance = autoBalance;
                TeamDamage = teamDamage;
                Sniper = sniper;
                BombReposition = bombReposition;
                SpecMode = specMode;
                PingMax = pingMax;
                PacketLossMax = packetLossMax;
                Bandwidth = bandwidth;
                MaxOutput = maxOutput;
                Dedicated = dedicated;
                PasswordProtected = passwordProtected;
                Players = players;
            }

            public void PrintDetails()
            {
                PropertyInfo[] properties = typeof(GameInfo).GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(List<PlayerMetadata>))
                    {
                        Console.WriteLine($"{property.Name}:");
                        foreach (var player in Players)
                        {
                            player.PrintDetails();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{property.Name}: {property.GetValue(this)}");
                    }
                }
            }
        }



        //Custom events
        public delegate void PlayerJoinedEventHandler(PlayerMetadata player);
        public event PlayerJoinedEventHandler PlayerJoined;
        public delegate void PlayerLeftEventHandler(PlayerMetadata player);
        public event PlayerLeftEventHandler PlayerLeft;
        public delegate void MapChangedEventHandler(string oldMap, string newMap);
        public event MapChangedEventHandler MapChanged;
        public delegate void RoundOverEventHandler(int oldRound, int newRound);
        public event RoundOverEventHandler RoundOver;
        public delegate void PlayerTeamChangedHandler(PlayerMetadata player, int oldTeam, int newTeam);
        public event PlayerTeamChangedHandler PlayerTeamChanged;
        public delegate void PlayerNameChangedEventHandler(PlayerMetadata player, string oldPlayerName, string newPlayerName);
        public event PlayerNameChangedEventHandler PlayerNameChanged;

        //Constructor
        public IGIServer(string ip, int port)
        {
            config.IP = ip;
            config.PORT = port;
        }



        //Public methods
        public ServerInfo GetInfoData(int timeout = 2000)
        {
            var infodata = SendAndReceiveUdpPacket("\\" + config.InfoPacket + "\\", timeout);

            if (infodata == null)
                return new ServerInfo();

            string hostname = string.Empty;
            int hostport = 0;
            string mapname = string.Empty;
            string gametype = string.Empty;
            int numplayers = 0;
            int maxplayers = 0;
            string gamemode = string.Empty;
            int bandwidth = 0;
            double maxout = 0.0;
            int uptime = 0;
            string timeleft = string.Empty;
            int currentRound = 0;
            int maxRounds = 0;
            string queryId = string.Empty;

            string[] parts = infodata.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                switch (parts[i])
                {
                    case "hostname":
                        hostname = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "hostport":
                        hostport = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int hp) ? hp : 0;
                        break;
                    case "mapname":
                        mapname = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "gametype":
                        gametype = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "numplayers":
                        numplayers = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int np) ? np : 0;
                        break;
                    case "maxplayers":
                        maxplayers = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int mp) ? mp : 0;
                        break;
                    case "gamemode":
                        gamemode = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "bwidth":
                        bandwidth = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int bw) ? bw : 0;
                        break;
                    case "maxout":
                        maxout = i + 1 < parts.Length && double.TryParse(parts[i + 1], out double mo) ? mo : 0.0;
                        break;
                    case "uptime":
                        uptime = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int up) ? up : 0;
                        break;
                    case "timeleft":
                        timeleft = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "mapstat":
                        if (i + 1 < parts.Length && parts[i + 1].StartsWith("roundlimit_"))
                        {
                            string[] roundParts = parts[i + 1].Split(new char[] { '_', '/' }, StringSplitOptions.RemoveEmptyEntries);
                            if (roundParts.Length == 3)
                            {
                                int.TryParse(roundParts[1], out currentRound);
                                int.TryParse(roundParts[2], out maxRounds);
                            }
                        }
                        break;
                    case "queryid":
                        queryId = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                }
            }

            return new ServerInfo(
                hostname, hostport, mapname, gametype, numplayers, maxplayers,
                gamemode, bandwidth, maxout, uptime, timeleft, currentRound, maxRounds, queryId);
        }

        public BasicInfo GetBasicData(int timeout = 2000)
        {
            var basicdata = SendAndReceiveUdpPacket("\\" + config.BasicPacket + "\\", timeout);

            if (basicdata == null)
                return new BasicInfo();

            string gameName = string.Empty;
            string gameVersion = string.Empty;
            string location = string.Empty;
            string queryId = string.Empty;

            string[] parts = basicdata.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                switch (parts[i])
                {
                    case "gamename":
                        gameName = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "gamever":
                        gameVersion = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "location":
                        location = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "queryid":
                        queryId = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                }
            }

            return new BasicInfo(gameName, gameVersion, location, queryId);
        }

        public PlayersInfo GetPlayersData(int timeout = 2000)
        {
            var playersdata = SendAndReceiveUdpPacket("\\" + config.PlayersPacket + "\\", timeout);

            if (playersdata == null)
                return new PlayersInfo();

            string team0Name = string.Empty;
            string team1Name = string.Empty;
            int team0Score = 0;
            int team1Score = 0;
            string queryId = string.Empty;
            List<PlayerMetadata> players = new List<PlayerMetadata>();

            string[] parts = playersdata.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                switch (parts[i])
                {
                    case "team_t0":
                        team0Name = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "team_t1":
                        team1Name = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "score_t0":
                        team0Score = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int s0) ? s0 : 0;
                        break;
                    case "score_t1":
                        team1Score = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int s1) ? s1 : 0;
                        break;
                    case "queryid":
                        queryId = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    default:
                        if (parts[i].StartsWith("player_"))
                        {
                            // Extract player details
                            int playerId = int.Parse(parts[i].Substring(parts[i].IndexOf('_') + 1));
                            string playerName = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                            string playerFrags = "NA";
                            string playerDeaths = "NA";
                            int playerPing = 0;
                            int playerTeam = 0;

                            for (int j = i + 2; j < parts.Length; j += 2)
                            {
                                if (parts[j].StartsWith("player_"))
                                    break;

                                switch (parts[j])
                                {
                                    case string frags when frags.StartsWith("frags_"):
                                        playerFrags = j + 1 < parts.Length ? parts[j + 1] : "NA";
                                        break;
                                    case string deaths when deaths.StartsWith("deaths_"):
                                        playerDeaths = j + 1 < parts.Length ? parts[j + 1] : "NA";
                                        break;
                                    case string ping when ping.StartsWith("ping_"):
                                        playerPing = j + 1 < parts.Length && int.TryParse(parts[j + 1], out int p) ? p : 0;
                                        break;
                                    case string team when team.StartsWith("team_"):
                                        playerTeam = j + 1 < parts.Length && int.TryParse(parts[j + 1], out int t) ? t : 0;
                                        break;
                                }
                            }

                            players.Add(new PlayerMetadata(playerId, playerName, playerFrags, playerDeaths, playerPing, playerTeam));
                        }
                        break;
                }
            }

            return new PlayersInfo(team0Name, team1Name, team0Score, team1Score, queryId, players);
        }

        public GameInfo GetStatusData(int timeout = 2000)
        {
            var statusdata = SendAndReceiveUdpPacket("\\" + config.StatusPacket + "\\", timeout);

            if (statusdata == null)
                return new GameInfo();

            string gameName = string.Empty;
            string gameVersion = string.Empty;
            string location = string.Empty;
            string queryId = string.Empty;
            string hostname = string.Empty;
            int hostport = 0;
            string mapName = string.Empty;
            string gameType = string.Empty;
            int numPlayers = 0;
            int maxPlayers = 0;
            string gameMode = string.Empty;
            int currentRound = 0;
            int maxRounds = 0;
            int timeLimit = 0;
            bool autoBalance = false;
            bool teamDamage = false;
            bool sniper = false;
            bool bombReposition = false;
            bool specMode = false;
            int pingMax = 0;
            float packetLossMax = 0.0f;
            int bandwidth = 0;
            float maxOutput = 0.0f;
            bool dedicated = false;
            bool passwordProtected = false;
            string team0Name = string.Empty;
            string team1Name = string.Empty;
            int team0Score = 0;
            int team1Score = 0;
            List<PlayerMetadata> players = new List<PlayerMetadata>();

            string[] parts = statusdata.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                switch (parts[i])
                {
                    case "gamename":
                        gameName = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "gamever":
                        gameVersion = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "location":
                        location = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "queryid":
                        queryId = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "hostname":
                        hostname = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "hostport":
                        hostport = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int h) ? h : 0;
                        break;
                    case "mapname":
                        mapName = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "gametype":
                        gameType = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "numplayers":
                        numPlayers = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int np) ? np : 0;
                        break;
                    case "maxplayers":
                        maxPlayers = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int mp) ? mp : 0;
                        break;
                    case "gamemode":
                        gameMode = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "mapstat":
                        if (i + 1 < parts.Length && parts[i + 1].StartsWith("roundlimit_"))
                        {
                            string[] roundParts = parts[i + 1].Substring("roundlimit_".Length).Split('/');
                            currentRound = roundParts.Length > 0 && int.TryParse(roundParts[0], out int cr) ? cr : 0;
                            maxRounds = roundParts.Length > 1 && int.TryParse(roundParts[1], out int mr) ? mr : 0;
                        }
                        break;
                    case "team_t0":
                        team0Name = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "team_t1":
                        team1Name = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                        break;
                    case "score_t0":
                        team0Score = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int ts0) ? ts0 : 0;
                        break;
                    case "score_t1":
                        team1Score = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int ts1) ? ts1 : 0;
                        break;
                    case "timelimit":
                        timeLimit = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int tl) ? tl : 0;
                        break;
                    case "autobalance":
                        autoBalance = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int ab) ? ab == 1 : false;
                        break;
                    case "teamdamage":
                        teamDamage = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int td) ? td == 1 : false;
                        break;
                    case "sniper":
                        sniper = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int sn) ? sn == 1 : false;
                        break;
                    case "bombrepos":
                        bombReposition = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int br) ? br == 1 : false;
                        break;
                    case "specmode":
                        specMode = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int sm) ? sm == 1 : false;
                        break;
                    case "pingmax":
                        pingMax = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int pm) ? pm : 0;
                        break;
                    case "plossmax":
                        packetLossMax = i + 1 < parts.Length && float.TryParse(parts[i + 1], out float plm) ? plm : 0.0f;
                        break;
                    case "bwidth":
                        bandwidth = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int bw) ? bw : 0;
                        break;
                    case "maxout":
                        maxOutput = i + 1 < parts.Length && float.TryParse(parts[i + 1], out float mo) ? mo : 0.0f;
                        break;
                    case "dedicated":
                        dedicated = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int d) ? d == 1 : false;
                        break;
                    case "password":
                        passwordProtected = i + 1 < parts.Length && int.TryParse(parts[i + 1], out int pw) ? pw == 1 : false;
                        break;
                    default:
                        // Player parsing
                        if (parts[i].StartsWith("player_"))
                        {
                            // Extract player details
                            int playerId = int.Parse(parts[i].Substring(parts[i].IndexOf('_') + 1));
                            string playerName = i + 1 < parts.Length ? parts[i + 1] : string.Empty;
                            string playerFrags = "NA";
                            string playerDeaths = "NA";
                            int playerPing = 0;
                            int playerTeam = 0;

                            for (int j = i + 2; j < parts.Length; j += 2)
                            {
                                if (parts[j].StartsWith("player_"))
                                    break;

                                switch (parts[j])
                                {
                                    case string frags when frags.StartsWith("frags_"):
                                        playerFrags = j + 1 < parts.Length ? parts[j + 1] : "NA";
                                        break;
                                    case string deaths when deaths.StartsWith("deaths_"):
                                        playerDeaths = j + 1 < parts.Length ? parts[j + 1] : "NA";
                                        break;
                                    case string ping when ping.StartsWith("ping_"):
                                        playerPing = j + 1 < parts.Length && int.TryParse(parts[j + 1], out int p) ? p : 0;
                                        break;
                                    case string team when team.StartsWith("team_"):
                                        playerTeam = j + 1 < parts.Length && int.TryParse(parts[j + 1], out int t) ? t : 0;
                                        break;
                                }
                            }

                            players.Add(new PlayerMetadata(playerId, playerName, playerFrags, playerDeaths, playerPing, playerTeam));
                        }
                        break;
                }
            }

            return new GameInfo(gameName, gameVersion, location, queryId,
                        hostname, hostport, mapName, gameType,
                        numPlayers, maxPlayers, gameMode, currentRound, maxRounds,
                        team0Name, team1Name, team0Score, team1Score,
                        timeLimit, autoBalance, teamDamage, sniper, bombReposition,
                        specMode, pingMax, packetLossMax, bandwidth, maxOutput,
                        dedicated, passwordProtected, players);
        }

        public string QueryPacket(string packet, int timeout = 2000)
        {
            return SendAndReceiveUdpPacket(packet, timeout);
        }

        public void ListenPlayers(int sleepDelay = 2000)
        {
            var lastPlayersInfo = PlayersInfo_Live = GetPlayersData();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                while (true)
                {
                    var currentPlayersInfo = PlayersInfo_Live = GetPlayersData();

                    if (String.IsNullOrEmpty(currentPlayersInfo.QueryId))
                        continue;

                    // Checking Player left
                    var playersLeft = lastPlayersInfo.Players.Except(currentPlayersInfo.Players).ToList();
                    foreach (var player in playersLeft)
                    {
                        OnPlayerLeft(player);
                    }

                    // Checking Player joined
                    var playersJoined = currentPlayersInfo.Players.Except(lastPlayersInfo.Players).ToList();
                    foreach (var player in playersJoined)
                    {
                        OnPlayerJoined(player);
                    }


                    // Detect players who changed teams
                    var lastPlayersDict = lastPlayersInfo.Players.ToDictionary(player => player.PlayerID);

                    foreach (var currentPlayer in currentPlayersInfo.Players)
                    {
                        if (lastPlayersDict.TryGetValue(currentPlayer.PlayerID, out var lastPlayer) &&
                            lastPlayer.PlayerTeam != currentPlayer.PlayerTeam)
                        {
                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;

                                PlayerTeamChanged?.Invoke(currentPlayer, lastPlayer.PlayerTeam, currentPlayer.PlayerTeam);

                            }).Start();

                        }
                    }

                    // Detect players whose names have changed
                    foreach (var currentPlayer in currentPlayersInfo.Players)
                    {
                        if (lastPlayersDict.TryGetValue(currentPlayer.PlayerID, out var lastPlayer) &&
                            lastPlayer.PlayerName != currentPlayer.PlayerName)
                        {
                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;

                                PlayerNameChanged?.Invoke(currentPlayer, lastPlayer.PlayerName, currentPlayer.PlayerName);

                            }).Start();

                        }
                    }

                    lastPlayersInfo = currentPlayersInfo;

                    Thread.Sleep(sleepDelay);
                }

            }).Start();

        }

        public void ListenServer(int sleepDelay = 2000)
        {
            var lastServerInfo = ServerInfo_Live = GetInfoData();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                while (true)
                {
                    var currentServerInfo = ServerInfo_Live = GetInfoData();

                    if (String.IsNullOrEmpty(currentServerInfo.QueryId))
                        continue;

                    // Checking if the map has changed
                    if (!string.Equals(lastServerInfo.Mapname, currentServerInfo.Mapname, StringComparison.Ordinal))
                    {
                        OnMapChanged(lastServerInfo.Mapname, currentServerInfo.Mapname);
                    }

                    // Checking if the round is over
                    if (lastServerInfo.CurrentRound != currentServerInfo.CurrentRound)
                    {
                        OnRoundOver(lastServerInfo.CurrentRound, currentServerInfo.CurrentRound);
                    }

                    lastServerInfo = currentServerInfo;

                    Thread.Sleep(sleepDelay);
                }

            }).Start();

        }





        //Private methods
        protected virtual void OnMapChanged(string oldMap, string newMap)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                MapChanged?.Invoke(oldMap, newMap);

            }).Start();

        }

        protected virtual void OnRoundOver(int oldRound, int newRound)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                RoundOver?.Invoke(oldRound, newRound);

            }).Start();

        }

        protected virtual void OnPlayerJoined(PlayerMetadata player)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                PlayerJoined?.Invoke(player);

            }).Start();

        }

        protected virtual void OnPlayerLeft(PlayerMetadata player)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                PlayerLeft?.Invoke(player);

            }).Start();

        }

        string SendAndReceiveUdpPacket(string message, int timeout = 500)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed.TotalSeconds < 3)
            {
                try
                {
                    using (var udpClient = new UdpClient())
                    {
                        byte[] sendData = Encoding.UTF8.GetBytes(message);

                        udpClient.Client.SendTimeout = timeout;
                        udpClient.Client.ReceiveTimeout = timeout;

                        udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);

                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, config.PORT);
                        byte[] receiveData = udpClient.Receive(ref remoteEndPoint);
                        var finalData = Encoding.UTF8.GetString(receiveData);

                        if (!string.IsNullOrEmpty(finalData) && finalData.Contains("queryid"))
                            return finalData;
                    }
                }
                catch (Exception)
                {

                }
            }

            stopwatch.Stop();
            return null;
        }
    }

    public class IGICommander
    {
        //config class
        public class IGICommanderconfig
        {
            //Required configs
            public string IP;
            public int PORT;
            public string RCON;
        }

        Mem mem = new Mem();
        static string ongroundaddress = "igi2.exe+0x079533A0,0";
        static string activemapaddress = "igi2.exe+0x7953154";

        public IGICommanderconfig config = new IGICommanderconfig();

        public delegate void PlayerKilledEventHandler(PlayerMetadata Victim, PlayerMetadata Attacker, HitLocation hitLocation, HitType hitType, string weaponName);
        public event PlayerKilledEventHandler PlayerKilled;
        public delegate void PlayerSpawnKilledEventHandler(PlayerMetadata Victim, PlayerMetadata Attacker, string weaponName);
        public event PlayerSpawnKilledEventHandler PlayerSpawnKilled;
        public delegate void PlayerTeamKilledEventHandler(PlayerMetadata Victim, PlayerMetadata Attacker, HitLocation hitLocation, HitType hitType, string weaponName);
        public event PlayerTeamKilledEventHandler PlayerTeamKilled;
        public void InvokeTeamKill(PlayerMetadata Victim, PlayerMetadata Attacker, IGICommander.HitLocation hitLocation, IGICommander.HitType hitType, string weaponName)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                PlayerTeamKilled?.Invoke(Victim, Attacker, hitLocation, hitType, weaponName);

            }).Start();

        }
        public delegate void InGameChat_ReceivedEventHandler(PlayerMetadata Player, string message);
        public event InGameChat_ReceivedEventHandler InGameChat_Received;
        public delegate void PlayerSpawnedEventHandler(PlayerMetadata Player);
        public event PlayerSpawnedEventHandler PlayerSpawned;
        public delegate void WeaponPurchasedEventHandler(PlayerMetadata Player, int weaaponID, string weaponName);
        public event WeaponPurchasedEventHandler WeaponPurchased;
        public delegate void PlayerJoiningEventHandler(PlayerMetadata Player);
        public event PlayerJoiningEventHandler PlayerJoining;
        public delegate void PlayerJoinedEventHandler(PlayerMetadata Player);
        public event PlayerJoinedEventHandler PlayerJoined;
        public delegate void DuplicateNamePlayerJoinedEventHandler(PlayerMetadata Player);
        public event DuplicateNamePlayerJoinedEventHandler DuplicateNamePlayerJoined;
        public delegate void DuplicateIPPlayerJoinedEventHandler(PlayerMetadata Player);
        public event DuplicateIPPlayerJoinedEventHandler DuplicateIPPlayerJoined;
        public delegate void PlayerSuicidedEventHandler(PlayerMetadata Player, SuicideType suicideType);
        public event PlayerSuicidedEventHandler PlayerSuicided;
        public delegate void PlayerLeftEventHandler(PlayerMetadata Player);
        public event PlayerLeftEventHandler PlayerLeft;
        public delegate void ObjectiveWonEventHandler(string teamName);
        public event ObjectiveWonEventHandler ObjectiveWon;
        public delegate void BombPlacedEventHandler();
        public event BombPlacedEventHandler BombPlaced;
        public delegate void ConsoleCommandReceivedEventHandler(string senderIP, string command);
        public event ConsoleCommandReceivedEventHandler ConsoleCommand_Received;
        public delegate void RconPlayerDetectedEventHandler(PlayerMetadata Player);
        public event RconPlayerDetectedEventHandler RconPlayer_Detected;
        public delegate void MapChangedEventHandler(int oldMapID, int newMapID);
        public event MapChangedEventHandler MapChanged;
        public delegate void QTaskCountChangedEventHandler(int oldQTaskCount, int newQTaskCount);
        public event QTaskCountChangedEventHandler QTaskCountChanged;


        public Dictionary<int, string> PICKUP_ID = new Dictionary<int, string>() {
            { 1, "AK-47" },
            { 2, "APC" },
            { 3, "AUG" },
            { 4, "M82A1" },
            { 5, "M82A6-T" },
            { 6, "BINOCULARS" },
            { 7, "C4BOMB" },
            { 8, "MAGNUM" },
            { 9, "D-EAGLE" },
            { 10, "SVD DRAGUNOV" },
            { 11, "FLASHBANG" },
            { 12, "G11" },
            { 13, "G36" },
            { 14, "G-17 SD" },
            { 15, "GRENADE" },
            { 16, "M1014" },
            { 17, "JACKHAMMER" },
            { 18, "COMBAT KNIFE" },
            { 19, "LASER DESIGNATOR" },
            { 20, "M2HB" },
            { 21, "M16 A2" },
            { 22, "MAC-10" },
            { 23, "MAKAROV" },
            { 24, "MEDIPACK" },
            { 25, "MIL" },
            { 26, "FN MINIMI (SAW)" },
            { 27, "MP5A3" },
            { 28, "PROXIMITYMINE" },
            { 29, "PSG-1" },
            { 30, "PSG-1SD" },
            { 31, "IR SAM" },
            { 32, "RPG-7" },
            { 33, "LAW 80" },
            { 34, "SENTRYGUN" },
            { 35, "SMG-2" },
            { 36, "SMOKEGRENADE" },
            { 37, "SOCOM" },
            { 38, "SPAS-12" },
            { 39, "T80" },
            { 40, "THERMAL" },
            { 41, "TYPE 64 SMG" },
            { 42, "UZI" },
            { 43, "TWIN UZI" },
            { 44, "MAKAROVCLIP" },
            { 45, "SOCOMCLIP" },
            { 46, "GLOCKCLIP" },
            { 47, "COLTANACONDACLIP" },
            { 48, "DESERTEAGLECLIP" },
            { 49, "MP5CLIP" },
            { 50, "UZICLIP" },
            { 51, "UZIX2CLIP" },
            { 52, "SMG2CLIP" },
            { 53, "MAC10CLIP" },
            { 54, "TYPE64CLIP" },
            { 55, "G11CLIP" },
            { 56, "AUGCLIP" },
            { 57, "G36CLIP" },
            { 58, "M16CLIP" },
            { 59, "AK47CLIP" },
            { 60, "AK74CLIP" },
            { 61, "PSG1CLIP" },
            { 62, "PSG1SDCLIP" },
            { 63, "DRAGUNOVCLIP" },
            { 64, "BARRETCLIP" },
            { 65, "XM1041CLIP" },
            { 66, "JACKHAMMERCLIP" },
            { 67, "SPAS12CLIP" },
            { 68, "MINIMICLIP" },
            { 69, "M2HBCLIP" },
            { 70, "M203CLIP" },
            { 71, "PROXIMITYMINE" },
            { 72, "MEDIPACK" },
            { 73, "GRENADE" },
            { 74, "SMOKEGRENADE" },
            { 75, "C4BOMB" },
            { 76, "FLASHBANG" },
            { 77, "RPG7CLIP" },
            { 78, "RPG18CLIP" }};

        public ConcurrentDictionary<int, PlayerMetadata> PlayersInfo_Live = new ConcurrentDictionary<int, PlayerMetadata>();

        public class PlayerMetadata
        {
            public int PlayerID = 0;
            public string PlayerName = "";
            public string IP = "";
            public int Kills = 0;
            public int Deaths = 0;
            public bool isAdmin = false;
            public int WeaponPurchaseCount = 0;
            public PlayerState PlayerState = PlayerState.Dead;
            public bool Verified = false;
            public int SpawnKillsCount = 0;
            public int TeamKillsCount = 0;
            public List<string> Messages = new List<string>();
            public int WarningsCount = 0;
            public bool ChangedTeam = false;
            public int CurrentSpawnWeaponPurchaseCount = 0;

            private int lastWeaponCount = 0;
            private IGICommander commander;

            public PlayerMetadata(IGICommander parentclass)
            {
                commander = parentclass;

                System.Timers.Timer timer = new System.Timers.Timer(700);
                timer.Elapsed += (sender, e) =>
                {
                    if (lastWeaponCount != WeaponPurchaseCount)
                    {
                        var diff = WeaponPurchaseCount - lastWeaponCount;
                        if (diff > 2)
                        {
                            WarningsCount++;
                        }

                        lastWeaponCount = WeaponPurchaseCount;
                    }
                };
                timer.AutoReset = true;
                timer.Start();
            }

            public void Kick(string reason = "")
            {
                commander.sv_kick(PlayerID);
                if (!string.IsNullOrEmpty(reason))
                {
                    commander.lo_announce(reason);
                }
            }

            public void AddKills(int Amount)
            {
                Kills += Amount;
            }

            public void AddDeaths(int Amount)
            {
                Deaths += Amount;
            }

            public void SetName(string name)
            {
                PlayerName = name;
            }

            public void SetIP(string ip)
            {
                IP = ip;
            }

            public void SetAdmin()
            {
                isAdmin = true;
            }

            public void AddWeaponPurchaseCount(int Amount)
            {
                WeaponPurchaseCount += Amount;
                CurrentSpawnWeaponPurchaseCount += Amount;
            }

            public void SetID(int id)
            {
                PlayerID = id;
            }

            public void SetPlayerState(PlayerState playerState)
            {
                PlayerState = playerState;
            }

            public void VerifyPlayer()
            {
                Verified = true;
            }

            public void AddSpawnKillsCount(int Amount)
            {
                SpawnKillsCount += Amount;
            }

            public void AddTeamKillsCount(int Amount)
            {
                TeamKillsCount += Amount;
            }

            public void AddMessages(string message)
            {
                Messages.Add(message);
            }

            public void AddWarningsCount(int Amount)
            {
                WarningsCount += Amount;
            }

            public void ChangeTeam()
            {
                if (ChangedTeam)
                    ChangedTeam = false;
                else
                    ChangedTeam = true;
            }

            public void ClearSpawnWeaponsCount()
            {
                CurrentSpawnWeaponPurchaseCount = 0;
            }
        }

        public int GetPlayerId(string playerNameOrID)
        {
            if (int.TryParse(playerNameOrID, out int id_))
            {
                if (PlayersInfo_Live.ContainsKey(id_))
                {
                    return id_;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var player = PlayersInfo_Live.Where(x => x.Value.PlayerName == playerNameOrID);
                if (player != null)
                {
                    return player.FirstOrDefault().Key;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string GetPlayerName(string playerNameOrID)
        {
            if (int.TryParse(playerNameOrID, out int id_))
            {
                if (PlayersInfo_Live.ContainsKey(id_))
                {
                    return PlayersInfo_Live[id_].PlayerName;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return playerNameOrID;
            }
        }

        public struct Map
        {
            public int ID;
            public string Name;
        }

        public struct sv_finger_Player
        {
            public int ID;
            public string Name;
            public string IP;
        }

        public enum HitLocation
        {
            Head,
            Back,
            Groin,
            Chest,
            Leg,
            Arm,
            Body
        }
        public enum HitType
        {
            Bullet,
            Explosive,
            Melee
        }
        public enum SuicideType
        {
            FellFromHeight,
            MenuSuicide,
            SentryGun,
            Explosion,
            Mine
        }
        public enum PlayerState
        {
            Alive,
            Dead
        }


        //Constructor
        public IGICommander(string ip, int port, string rcon)
        {
            config.IP = ip;
            config.PORT = port;
            config.RCON = rcon;
        }


        string entityList = "igi2.exe+079533AC,238,10";
        string numPlayersAddress = "igi2.exe+07953344,0";
        List<(string playerName, string address, string datatype, string value)> frozenValues = new List<(string playerName, string address, string datatype, string value)>();
        public void SetPlayerValue(string playerName, string offset, string newVal, bool freeze)
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;

                try
                {
                    var mem_numPlayers = mem.ReadInt(numPlayersAddress);
                    if (mem_numPlayers != 0)
                    {
                        string tempEnt = entityList;

                        for (int i = 0; i < mem_numPlayers; i++)
                        {
                            var playername = mem.ReadString(tempEnt + "," + "54");

                            if (playername == playerName)
                            {
                                if (freeze && !frozenValues.Any(x => x.address == tempEnt + "," + offset))
                                {
                                    mem.FreezeValue(tempEnt + "," + offset, "int", newVal);
                                    frozenValues.Add((playerName, tempEnt + "," + offset, "int", newVal));
                                }
                                else if (!freeze)
                                {
                                    mem.WriteMemory(tempEnt + "," + offset, "int", newVal);
                                }

                                break;
                            }

                            tempEnt += ",4";
                            Thread.Sleep(10);
                        }
                        tempEnt = entityList;
                    }
                }
                catch (Exception ec)
                {
                    Log("[MEMORY WRITE] " + ec.ToString());
                }

            }).Start();
        }

        public void UnfreezeAll()
        {
            foreach (var cur in frozenValues)
            {
                mem.UnfreezeValue(cur.address);
            }
            frozenValues.Clear();
        }

        public void UnfreezePlayer(string playerName)
        {
            foreach (var cur in frozenValues)
            {
                if (cur.playerName == playerName)
                {
                    mem.UnfreezeValue(cur.address);
                }
            }
            frozenValues.RemoveAll(x => x.playerName == playerName);
        }

        private void Log(string message)
        {
            try
            {
                string logMessage = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + message + Environment.NewLine;
                File.AppendAllText("IGIDotNet_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log", logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while logging: " + ex.Message);
            }
        }

        public async Task ListenLogAsync(bool createBackupLog = false, string backupLogFileName = "MultiplayerBackup.log")
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;

                //Cleans the log file
                try
                {
                    File.WriteAllText("Multiplayer.log", string.Empty);
                }
                catch (Exception ec)
                {
                    //Log("[ERROR] Cannot clear log file");
                }

                try
                {
                    using (FileStream fileStream = File.Open("Multiplayer.log", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamReader reader = new StreamReader(fileStream);
                        StreamWriter writer = new StreamWriter(fileStream);

                        while (true)
                        {
                            try
                            {
                                string curRawData = await reader.ReadToEndAsync();

                                if (!String.IsNullOrEmpty(curRawData))
                                {
                                    //Clean the log file
                                    //FileStream fs = new FileStream("Multiplayer.log", FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite);
                                    //fs.WriteByte(Convert.ToByte(' '));
                                    //fs.Close();
                                    //Clean the log file
                                    await writer.WriteAsync("");

                                    //Writes data to backup file
                                    //if (createBackupLog)
                                    //{
                                    //   File.AppendAllText(backupLogFileName, curRawData);
                                    //}

                                    //Validating current data
                                    if (curRawData.Length < 11 || String.IsNullOrEmpty(curRawData) || String.IsNullOrWhiteSpace(curRawData))
                                    {
                                        continue;
                                    }

                                    //Validating current data
                                    if (curRawData.Contains(" was killed in his spawn area by ") ||
                                    curRawData.Contains(" was shot in the head by ") ||
                                    curRawData.Contains(" was pummeled to death by ") ||
                                    curRawData.Contains(" was cowardly shot in the back and killed by ") ||
                                    curRawData.Contains(" was killed by a shot in the groin by ") ||
                                    curRawData.Contains(" was killed by a shot in the chest by ") ||
                                    curRawData.Contains(" was killed by a shot in the leg by ") ||
                                    curRawData.Contains(" was killed by a shot in the arm by ") ||
                                    curRawData.Contains(" was stabbed to death by ") ||
                                    curRawData.Contains(" was blown to smithereens by ") ||
                                    curRawData.Contains(" did a close inspection of a sentry gun") ||
                                    curRawData.Contains(": ") ||
                                    curRawData.Contains("AddEquipment:") ||
                                    curRawData.Contains("] created locally") ||
                                    curRawData.Contains("Client: Remote spawned[") ||
                                    curRawData.Contains("Popped new networkID for joining client: [") ||
                                    curRawData.Contains("suicided[") ||
                                    curRawData.Contains("fell to his death[") ||
                                    curRawData.Contains("left the game[") ||
                                    curRawData.Contains("won objective") ||
                                    curRawData.Contains("Bomb placed[") ||
                                    curRawData.Contains("Consoled: '") ||
                                    curRawData.Contains("] --Players") ||
                                    curRawData.Contains("Client_Deletehandler: Removing [") ||
                                    curRawData.Contains(" was blown to smithereens") ||
                                    curRawData.Contains(" stepped on a mine and was blown to smithereens"))
                                    {
                                        new Thread(async () =>
                                        {
                                            Thread.CurrentThread.IsBackground = true;

                                            foreach (var curLineX in curRawData.Split('\n'))
                                            {
                                                if (curLineX.Length < 11 || String.IsNullOrEmpty(curLineX) || String.IsNullOrWhiteSpace(curLineX))
                                                {
                                                    continue;
                                                }

                                                var curLine = curLineX.Remove(0, 11);

                                                // Player Killed =>
                                                if (curLine.Contains(" was shot in the head by ") ||
                                                    curLine.Contains(" was pummeled to death by ") ||
                                                    curLine.Contains(" was cowardly shot in the back and killed by ") ||
                                                    curLine.Contains(" was killed by a shot in the groin by ") ||
                                                    curLine.Contains(" was killed by a shot in the chest by ") ||
                                                    curLine.Contains(" was killed by a shot in the leg by ") ||
                                                    curLine.Contains(" was killed by a shot in the arm by ") ||
                                                    curLine.Contains(" was stabbed to death by ") ||
                                                    curLine.Contains(" was blown to smithereens by "))
                                                {
                                                    HitLocation HitLocation = HitLocation.Body;
                                                    HitType HitType = HitType.Bullet;
                                                    string AttackerName = "", VictimName = "", WeaponName = "";

                                                    string parsingData = curLine
                                                        .Replace(" was shot in the head by ", ",")
                                                        .Replace(" was pummeled to death by ", ",")
                                                        .Replace(" was cowardly shot in the back and killed by ", ",")
                                                        .Replace(" was killed by a shot in the groin by ", ",")
                                                        .Replace(" was killed by a shot in the chest by ", ",")
                                                        .Replace(" was killed by a shot in the leg by ", ",")
                                                        .Replace(" was killed by a shot in the arm by ", ",")
                                                        .Replace(" was stabbed to death by ", ",")
                                                        .Replace(" was blown to smithereens by ", ",")
                                                        .Replace("][", ",")
                                                        .Replace(" [", ",")
                                                        .Replace("[", ",");

                                                    string[] parts = parsingData.Split(',');

                                                    if (parts.Length < 3)
                                                        continue;

                                                    VictimName = parts[0].Trim();
                                                    AttackerName = parts[1].Trim();
                                                    WeaponName = parts[2].Trim();

                                                    if (WeaponName.Contains("**"))
                                                        WeaponName = "Unknown object";

                                                    // Set HitLocation & HitType
                                                    if (curLine.Contains(" was shot in the head by "))
                                                        HitLocation = HitLocation.Head;
                                                    else if (curLine.Contains(" was pummeled to death by "))
                                                    {
                                                        HitType = HitType.Melee;
                                                        HitLocation = HitLocation.Body;
                                                    }
                                                    else if (curLine.Contains(" was cowardly shot in the back and killed by "))
                                                        HitLocation = HitLocation.Back;
                                                    else if (curLine.Contains(" was killed by a shot in the groin by "))
                                                        HitLocation = HitLocation.Groin;
                                                    else if (curLine.Contains(" was killed by a shot in the chest by "))
                                                        HitLocation = HitLocation.Chest;
                                                    else if (curLine.Contains(" was killed by a shot in the leg by "))
                                                        HitLocation = HitLocation.Leg;
                                                    else if (curLine.Contains(" was killed by a shot in the arm by "))
                                                        HitLocation = HitLocation.Arm;
                                                    else if (curLine.Contains(" was stabbed to death by "))
                                                    {
                                                        HitType = HitType.Melee;
                                                        HitLocation = HitLocation.Body;
                                                    }
                                                    else if (curLine.Contains(" was blown to smithereens by "))
                                                    {
                                                        HitType = HitType.Explosive;
                                                        HitLocation = HitLocation.Body;
                                                    }

                                                    PlayerMetadata victim = new PlayerMetadata(this);
                                                    PlayerMetadata attacker = new PlayerMetadata(this);

                                                    foreach (var cur in PlayersInfo_Live.Values.Where(x => x.PlayerName == AttackerName))
                                                    {
                                                        cur.AddKills(1);
                                                        attacker = cur;
                                                    }

                                                    foreach (var cur in PlayersInfo_Live.Values.Where(x => x.PlayerName == VictimName))
                                                    {
                                                        cur.SetPlayerState(PlayerState.Dead);
                                                        cur.AddDeaths(1);
                                                        victim = cur;
                                                    }

                                                    new Thread(() =>
                                                    {
                                                        Thread.CurrentThread.IsBackground = true;
                                                        PlayerKilled?.Invoke(victim, attacker, HitLocation, HitType, WeaponName);
                                                    }).Start();
                                                }

                                                // Spawn Killed =>
                                                else if (curLine.Contains(" was killed in his spawn area by "))
                                                {
                                                    string AttackerName = "", VictimName = "", WeaponName = "";

                                                    string parsingData = curLine
                                                        .Replace(" was killed in his spawn area by ", ",")
                                                        .Replace("][", ",")
                                                        .Replace(" [", ",")
                                                        .Replace("[", ",");

                                                    string[] parts = parsingData.Split(',');

                                                    if (parts.Length < 3)
                                                        continue;

                                                    VictimName = parts[0].Trim();
                                                    AttackerName = parts[1].Trim();
                                                    WeaponName = parts[2].Trim();

                                                    PlayerMetadata victim = new PlayerMetadata(this);
                                                    PlayerMetadata attacker = new PlayerMetadata(this);

                                                    foreach (var cur in PlayersInfo_Live.Values.Where(x => x.PlayerName == AttackerName))
                                                    {
                                                        cur.AddKills(1);
                                                        cur.AddSpawnKillsCount(1);
                                                        attacker = cur;
                                                    }

                                                    foreach (var cur in PlayersInfo_Live.Values.Where(x => x.PlayerName == VictimName))
                                                    {
                                                        cur.SetPlayerState(PlayerState.Dead);
                                                        cur.AddDeaths(1);
                                                        victim = cur;
                                                    }

                                                    new Thread(() =>
                                                    {
                                                        Thread.CurrentThread.IsBackground = true;
                                                        PlayerSpawnKilled?.Invoke(victim, attacker, WeaponName);
                                                    }).Start();
                                                }


                                                //Player spawned =>
                                                else if (curLine.Contains("Client: Remote spawned"))
                                                {
                                                    var parsingData = curLine.Remove(0, curLine.IndexOf("Client: Remote spawned"))
                                                    .Replace("Client: Remote spawned[", "")
                                                    .Replace("][", ",")
                                                    .Replace("]", "");

                                                    PlayerMetadata player = new PlayerMetadata(this);
                                                    foreach (var cur in PlayersInfo_Live.Values.Where(x => x.PlayerID == Convert.ToInt32(parsingData.Split(',')[0])))
                                                    {
                                                        cur.SetPlayerState(PlayerState.Alive);
                                                        cur.ClearSpawnWeaponsCount();
                                                        player = cur;
                                                    }

                                                    new Thread(() =>
                                                    {
                                                        Thread.CurrentThread.IsBackground = true;

                                                        PlayerSpawned?.Invoke(player);

                                                    }).Start();


                                                }

                                                // Weapon Purchased =>
                                                else if (curLine.Contains("AddEquipment:"))
                                                {
                                                    try
                                                    {
                                                        var parsingData = curLine.Remove(0, curLine.IndexOf("AddEquipment:"))
                                                                                  .Replace("AddEquipment: Adding itemID=", "")
                                                                                  .Replace(" to client[", ",")
                                                                                  .Replace("]", "")
                                                                                  .Trim();

                                                        var dataParts = parsingData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                        if (dataParts.Length < 2) continue;

                                                        if (!int.TryParse(dataParts[1].Trim(), out int playerId)) continue;
                                                        if (!int.TryParse(dataParts[0].Trim(), out int pickupId)) continue;
                                                        if (!PlayersInfo_Live.ContainsKey(playerId)) continue;
                                                        if (!PICKUP_ID.ContainsKey(pickupId)) continue;

                                                        if (pickupId < 44)
                                                        {
                                                            try { PlayersInfo_Live[playerId].AddWeaponPurchaseCount(1); }
                                                            catch (Exception ex) { Log("[1] " + ex.ToString()); }
                                                        }

                                                        new Thread(() =>
                                                        {
                                                            Thread.CurrentThread.IsBackground = true;
                                                            string weaponName = "Unknown";
                                                            try
                                                            {
                                                                weaponName = PICKUP_ID[pickupId];
                                                            }
                                                            catch
                                                            {
                                                                Log($"[ERROR] Cannot find weapon ID! [Weapon ID: {pickupId}, Weapon name: {weaponName}]");
                                                            }

                                                            if (pickupId < 44)
                                                            {
                                                                WeaponPurchased?.Invoke(PlayersInfo_Live[playerId], pickupId, weaponName);
                                                            }
                                                        }).Start();
                                                    }
                                                    catch { continue; }
                                                }


                                                // Player Joined =>
                                                else if (curLine.Contains("] created locally"))
                                                {
                                                    try
                                                    {
                                                        var parsingData = curLine.Replace("] created locally", "")
                                                                                 .Replace("][", ",")
                                                                                 .Replace("Client[", "")
                                                                                 .Trim();

                                                        var dataParts = parsingData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                        if (dataParts.Length < 2) continue;

                                                        if (!int.TryParse(dataParts[0].Trim(), out int playerId)) continue;
                                                        var playerName = dataParts[1].Trim();

                                                        bool isDuplicateName = PlayersInfo_Live.Any(x => x.Value.PlayerName == playerName);

                                                        if (!PlayersInfo_Live.ContainsKey(playerId))
                                                        {
                                                            var newPlayer = new PlayerMetadata(this)
                                                            {
                                                                PlayerID = playerId,
                                                                PlayerName = playerName,
                                                                PlayerState = PlayerState.Dead
                                                            };
                                                            PlayersInfo_Live.TryAdd(playerId, newPlayer);
                                                        }
                                                        else
                                                        {
                                                            try { PlayersInfo_Live[playerId].SetName(playerName); }
                                                            catch (Exception ex) { Log("[2] " + ex.ToString()); }
                                                        }

                                                        new Thread(() =>
                                                        {
                                                            Thread.CurrentThread.IsBackground = true;
                                                            PlayerJoined?.Invoke(PlayersInfo_Live[playerId]);
                                                        }).Start();

                                                        if (isDuplicateName)
                                                        {
                                                            new Thread(() =>
                                                            {
                                                                Thread.CurrentThread.IsBackground = true;
                                                                DuplicateNamePlayerJoined?.Invoke(PlayersInfo_Live[playerId]);
                                                            }).Start();
                                                        }
                                                    }
                                                    catch { continue; }
                                                }


                                                // Player Joining =>
                                                else if (curLine.Contains("Popped new networkID for joining client: ["))
                                                {
                                                    try
                                                    {
                                                        var parsingData = curLine.Replace("Popped new networkID for joining client: [", "")
                                                                                 .Replace("][", ",")
                                                                                 .Trim();

                                                        var dataParts = parsingData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                        if (dataParts.Length < 2) continue;

                                                        if (!int.TryParse(dataParts[0].Trim(), out int playerId)) continue;
                                                        var ipAddress = dataParts[1].Split(':')[0].Trim();

                                                        bool isDuplicateIP = PlayersInfo_Live.Any(x => x.Value.IP == ipAddress);

                                                        if (!PlayersInfo_Live.ContainsKey(playerId))
                                                        {
                                                            var newPlayer = new PlayerMetadata(this)
                                                            {
                                                                PlayerID = playerId,
                                                                IP = ipAddress,
                                                                PlayerState = PlayerState.Dead
                                                            };
                                                            PlayersInfo_Live.TryAdd(playerId, newPlayer);
                                                        }
                                                        else
                                                        {
                                                            try { PlayersInfo_Live[playerId].SetIP(ipAddress); }
                                                            catch (Exception ex) { Log("[3] " + ex.ToString()); }
                                                        }

                                                        new Thread(() =>
                                                        {
                                                            Thread.CurrentThread.IsBackground = true;
                                                            PlayerJoining?.Invoke(PlayersInfo_Live[playerId]);
                                                        }).Start();

                                                        if (isDuplicateIP)
                                                        {
                                                            new Thread(() =>
                                                            {
                                                                Thread.CurrentThread.IsBackground = true;
                                                                DuplicateIPPlayerJoined?.Invoke(PlayersInfo_Live[playerId]);
                                                            }).Start();
                                                        }
                                                    }
                                                    catch { continue; }
                                                }

                                                else if (curLine.Contains("suicided["))
                                                {
                                                    HandleSuicide(curLine, "suicided[", SuicideType.MenuSuicide);
                                                }
                                                else if (curLine.Contains(" fell to his death["))
                                                {
                                                    HandleSuicide(curLine, " fell to his death[", SuicideType.FellFromHeight);
                                                }
                                                else if (curLine.Contains(" did a close inspection of a sentry gun"))
                                                {
                                                    HandleSuicide(curLine, " did a close inspection of a sentry gun", SuicideType.SentryGun);
                                                }
                                                else if (curLine.Contains(" was blown to smithereens"))
                                                {
                                                    HandleSuicide(curLine, " was blown to smithereens", SuicideType.Explosion);
                                                }
                                                else if (curLine.Contains(" stepped on a mine and was blown to smithereens"))
                                                {
                                                    HandleSuicide(curLine, " stepped on a mine and was blown to smithereens", SuicideType.Mine);
                                                }


                                                // Player left =>
                                                else if (curLine.Contains("Client_Deletehandler: Removing ["))
                                                {
                                                    try
                                                    {
                                                        var parsingData = curLine
                                                            .Replace("Client_Deletehandler: Removing [", "")
                                                            .Replace("]", ",");

                                                        var parts = parsingData.Split(',');
                                                        if (parts.Length >= 2 && int.TryParse(parts[0], out int ID))
                                                        {
                                                            string Name = parts[1];

                                                            if (PlayersInfo_Live.ContainsKey(ID))
                                                            {
                                                                var player = PlayersInfo_Live[ID];

                                                                new Thread(() =>
                                                                {
                                                                    Thread.CurrentThread.IsBackground = true;
                                                                    try { PlayerLeft?.Invoke(player); } catch { }
                                                                }).Start();

                                                                if (frozenValues.Any(x => x.playerName == player.PlayerName))
                                                                {
                                                                    mem.UnfreezeValue(frozenValues.Where(x => x.playerName == player.PlayerName).FirstOrDefault().address);
                                                                    frozenValues.Remove(frozenValues.Where(x => x.playerName == player.PlayerName).FirstOrDefault());
                                                                }

                                                                PlayersInfo_Live.TryRemove(ID, out _);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log("[PlayerLeft] " + ex);
                                                    }
                                                }

                                                // Objective won =>
                                                else if (curLine.Contains(" won objective"))
                                                {
                                                    try
                                                    {
                                                        var teamName = curLine.Replace(" won objective", "").Trim();

                                                        new Thread(() =>
                                                        {
                                                            Thread.CurrentThread.IsBackground = true;
                                                            try { ObjectiveWon?.Invoke(teamName); } catch { }
                                                        }).Start();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log("[ObjectiveWon] " + ex);
                                                    }
                                                }

                                                // Bomb planted =>
                                                else if (curLine.Contains("Bomb placed["))
                                                {
                                                    try
                                                    {
                                                        new Thread(() =>
                                                        {
                                                            Thread.CurrentThread.IsBackground = true;
                                                            try { BombPlaced?.Invoke(); } catch { }
                                                        }).Start();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log("[BombPlaced] " + ex);
                                                    }
                                                }

                                                // Console command received =>
                                                else if (curLine.Contains("Consoled: '"))
                                                {
                                                    try
                                                    {
                                                        var parsingData = curLine.Substring(curLine.IndexOf("Consoled: '"))
                                                            .Replace("Consoled: '", "")
                                                            .Replace("' run from ", ",");

                                                        var parts = parsingData.Split(',');

                                                        if (parts.Length >= 2)
                                                        {
                                                            string Command = parts[0].Trim();
                                                            string IP = parts[1].Split(':')[0].Trim();

                                                            new Thread(() =>
                                                            {
                                                                Thread.CurrentThread.IsBackground = true;
                                                                try { ConsoleCommand_Received?.Invoke(IP, Command); } catch { }
                                                            }).Start();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log("[ConsoleCommand] " + ex);
                                                    }
                                                }

                                                // RCON Player Detected =>
                                                else if (curLine.Contains("]*") && curLine.Contains(":"))
                                                {
                                                    try
                                                    {
                                                        var match = Regex.Match(curLine, @"\[(\d+)\]\*?\s?([^\[]+)");
                                                        if (match.Success)
                                                        {
                                                            int playerID = int.Parse(match.Groups[1].Value);
                                                            string playerName = match.Groups[2].Value.Trim();

                                                            if (PlayersInfo_Live.TryGetValue(playerID, out var player) && !player.isAdmin)
                                                            {
                                                                player.SetAdmin();

                                                                new Thread(() =>
                                                                {
                                                                    Thread.CurrentThread.IsBackground = true;
                                                                    try { RconPlayer_Detected?.Invoke(player); } catch { }
                                                                }).Start();
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log("[RconPlayer] " + ex);
                                                    }
                                                }

                                                // In-game Chat Received =>
                                                else if (curLine.Contains(": ") && curLine.IndexOf(':') > 0 && curLine[curLine.IndexOf(':') - 1] != ' ')
                                                {
                                                    try
                                                    {
                                                        string parsingData = curLine;

                                                        // Filter out known system messages
                                                        string[] ignoredPrefixes = {
                                                        "Period: ", "Network: ", "Client_DeleteHandler: ", "TeamManager: ", "IsClientMapCorrect: ",
                                                        "IsPasswordCorrect: ", "Network_RemoveFromLoadQueue: ", "Network_AddToLoadQueue: ", "Client: ",
                                                        "Server: ", "Server_AuthCallBack: ", "Consoled: ", "NetworkCommand_ParseEval: ",
                                                        "Weapon_SingleActivateHandler: ", "NetworkCommand_Eval_Response: ",
                                                        "NetworkCommand_Eval_ProcessRequest: ", "Winsock description: ", "Winsock system status: ",
                                                        "MapControl_SetLoading(): ", "First time loading: ", "Late network(> 200ms): ",
                                                        "Popped new networkID for joining client: ", "AddEquipment: ", "Client_GameMessageSpawnHandler: ",
                                                        "GunPickup_IsAbleToPickup: ", "Consoled_configListMessage: ", "NetworkCommand_Textlist_Response",
                                                        "Network_CloseUDPSocket", "LeaveGame", "Client_Deletehandler"
                                                    };

                                                        if (!ignoredPrefixes.Any(p => parsingData.StartsWith(p)))
                                                        {
                                                            string playerName = parsingData.Split(':')[0].Trim();
                                                            string message = parsingData.Substring(playerName.Length + 2).Trim();

                                                            var match = PlayersInfo_Live
                                                                .FirstOrDefault(p => p.Value != null && p.Value.PlayerName == playerName);

                                                            if (match.Value != null)
                                                            {
                                                                try
                                                                {
                                                                    match.Value.AddMessages(message);
                                                                }
                                                                catch (Exception e1)
                                                                {
                                                                    Log("[AddMessageError] " + e1);
                                                                }

                                                                new Thread(() =>
                                                                {
                                                                    Thread.CurrentThread.IsBackground = true;
                                                                    try
                                                                    {
                                                                        InGameChat_Received?.Invoke(match.Value, message);
                                                                    }
                                                                    catch (Exception e2)
                                                                    {
                                                                        Log("[InvokeChatError] " + e2);
                                                                    }
                                                                }).Start();
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log("[ChatReceivedError] " + ex);
                                                    }
                                                }

                                            }

                                        }).Start();
                                    }
                                }
                            }
                            catch (Exception ec)
                            {
                                Log(ec.ToString());
                                continue;
                            }

                            await Task.Delay(50);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log("[ERROR] " + ex.ToString());
                }
            }).Start();
        }

        private void HandleSuicide(string curLine, string triggerPhrase, SuicideType suicideType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(curLine) || !curLine.Contains(triggerPhrase))
                    return;

                var playerName = curLine.Substring(0, curLine.IndexOf(triggerPhrase)).Trim();

                if (string.IsNullOrWhiteSpace(playerName))
                    return;

                PlayerMetadata foundPlayer = null;

                foreach (var cur in PlayersInfo_Live.Values.Where(x => x.PlayerName == playerName))
                {
                    try
                    {
                        cur.AddDeaths(1);
                        cur.SetPlayerState(PlayerState.Dead);
                        foundPlayer = cur;
                    }
                    catch (Exception ex)
                    {
                        Log($"[HandleSuicide][InnerLoop] {ex}");
                        continue;
                    }
                }

                if (foundPlayer == null)
                    return;

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    try
                    {
                        PlayerSuicided?.Invoke(foundPlayer, suicideType);
                    }
                    catch (Exception ex)
                    {
                        Log($"[HandleSuicide][Callback] {ex}");
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Log($"[HandleSuicide][Outer] {ex}");
            }
        }


        public async Task ListenRconPlayers()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                while (true)
                {
                    string svfinger_data = sv_finger();

                    if (svfinger_data == null)
                    {
                        continue;
                    }

                    //PARSING SV FINGER
                    if (svfinger_data.Contains(" *"))
                    {
                        foreach (var c in PlayersInfo_Live.Where(x => svfinger_data.Contains(" *" + x.Value.PlayerName + "-drec") && !x.Value.isAdmin))
                        {
                            try
                            {
                                PlayersInfo_Live[c.Key].SetAdmin();
                            }
                            catch (Exception ec)
                            {
                                Log("[6]" + ec.ToString());
                            }

                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;

                                RconPlayer_Detected?.Invoke(PlayersInfo_Live[c.Key]);

                            }).Start();

                            Thread.Sleep(1);
                        }
                    }

                    Thread.Sleep(3000);
                }


            }).Start();
        }

        public async Task ListenMemoryEvents(int igi2ProcessID)
        {
            //WEAPON LIMIT CALCULATOR
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                mem.OpenProcess(igi2ProcessID);

                int lastItemIndex = 0;
                int lastMapIndex = 0;
                while (true)
                {
                    var activeMap = mem.ReadInt(activemapaddress);
                    var groundWeapons = mem.ReadInt(ongroundaddress);

                    //EVENT FOR GROUND WEAPON CHANGE
                    if (lastItemIndex != groundWeapons)
                    {
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;

                            QTaskCountChanged?.Invoke(lastItemIndex, groundWeapons);

                        }).Start();

                        lastItemIndex = groundWeapons;
                    }

                    //EVENT FOR MAP CHANGE
                    if (lastMapIndex != activeMap)
                    {
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;

                            ResetAllPlayerStats();
                            MapChanged?.Invoke(lastMapIndex, activeMap);


                        }).Start();

                        lastMapIndex = activeMap;
                    }

                    Thread.Sleep(100);
                }

            }).Start();
        }

        public void SendRconCommand(string message, int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes(message);
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                }
            }
            catch (Exception)
            {

            }
        }

        public string SendReceiveRconCommand(string message, int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes(message);
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, config.PORT);
                    string finalData = "";

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);

                    while (true)
                    {
                        try
                        {
                            byte[] receiveData = udpClient.Receive(ref remoteEndPoint);
                            string receivedText = Encoding.UTF8.GetString(receiveData);

                            finalData += receivedText;
                        }
                        catch (SocketException)
                        {
                            break;
                        }

                        Thread.Sleep(1);
                    }

                    if (!string.IsNullOrEmpty(finalData))
                        return finalData;
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public void ResetAllPlayerStats()
        {
            foreach (var player in PlayersInfo_Live.Values)
            {
                player.Kills = 0;
                player.Deaths = 0;
                player.WarningsCount = 0;
                player.Messages.Clear();
                player.WeaponPurchaseCount = 0;
                player.SpawnKillsCount = 0;
                player.TeamKillsCount = 0;
            }
        }

        public List<sv_finger_Player> Parse_sv_finger(string sv_finger_data)
        {
            var result = new List<sv_finger_Player>();
            var playerLineRegex = new Regex(@"\$\[(\d+)\]\s+(\w+)-.*");
            var playerIPRegex = new Regex(@"\$\[(\d+)\]\s+([\d\.]+):\d+.*");

            var lines = sv_finger_data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            string currentPlayerID = null;
            string currentPlayerName = null;

            foreach (var line in lines)
            {
                var playerLineMatch = playerLineRegex.Match(line);
                if (playerLineMatch.Success)
                {
                    currentPlayerID = playerLineMatch.Groups[1].Value;
                    currentPlayerName = playerLineMatch.Groups[2].Value;
                }

                var playerIPMatch = playerIPRegex.Match(line);
                if (playerIPMatch.Success && playerIPMatch.Groups[1].Value == currentPlayerID)
                {
                    string playerIP = playerIPMatch.Groups[2].Value;
                    result.Add(new sv_finger_Player
                    {
                        ID = Convert.ToInt32(currentPlayerID),
                        Name = currentPlayerName,
                        IP = playerIP
                    });

                    currentPlayerID = null;
                    currentPlayerName = null;
                }
            }

            return result;
        }

        public List<Map> Parse_sv_listmaps(string svlistmaps_data)
        {
            var result = new List<Map>();
            var lines = svlistmaps_data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Match patterns like "[1]   Map Name"
                var match = Regex.Match(line, @"\[(\d+)\]\s+(.*?)(\s+\[.*?\])?$");
                if (match.Success)
                {
                    Map map = new Map();
                    map.ID = Convert.ToInt32(match.Groups[1].Value);
                    map.Name = match.Groups[2].Value.Trim();
                    result.Add(map);
                }
            }

            return result;
        }

        public string sv_listmaps(int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("/sv listmaps");
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, config.PORT);
                    string finalData = "";

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);

                    byte[] rconCheck = udpClient.Receive(ref remoteEndPoint);
                    if (!Encoding.UTF8.GetString(rconCheck).Contains("Access granted"))
                    {
                        return null;
                    }

                    while (true)
                    {
                        try
                        {
                            byte[] receiveData = udpClient.Receive(ref remoteEndPoint);
                            var recData = Encoding.UTF8.GetString(receiveData);
                            finalData += recData;
                            if (recData.Contains("NetManager_ListMapsCB called"))
                            {
                                break;
                            }
                        }
                        catch (SocketException)
                        {
                            break;
                        }

                        Thread.Sleep(1);
                    }

                    if (!string.IsNullOrEmpty(finalData))
                        return finalData;
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        public string sv_finger(int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("/sv chamar");
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, config.PORT);
                    string finalData = "";

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);

                    byte[] rconCheck = udpClient.Receive(ref remoteEndPoint);
                    if (!Encoding.UTF8.GetString(rconCheck).Contains("Access granted"))
                    {
                        return null;
                    }

                    while (true)
                    {
                        try
                        {
                            byte[] receiveData = udpClient.Receive(ref remoteEndPoint);
                            var recData = Encoding.UTF8.GetString(receiveData);
                            finalData += recData;
                            if (recData.Contains("No players found") || recData.Contains("Player status"))
                            {
                                break;
                            }
                        }
                        catch (SocketException)
                        {
                            break;
                        }

                        Thread.Sleep(10);
                    }

                    if (!string.IsNullOrEmpty(finalData))
                        return finalData;
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public void lo_announce(string message, int timeout = 500, int delayBtwMsg = 300)
        {

            if (message.Length > 38)
            {
                var parts = SplitAnnouncement(message);
                foreach (var cur in parts)
                {
                    try
                    {
                        using (var udpClient = new UdpClient())
                        {
                            byte[] sendData = Encoding.UTF8.GetBytes("/lo announce(\"" + cur + "\");");
                            byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                            udpClient.Client.SendTimeout = timeout;
                            udpClient.Client.ReceiveTimeout = timeout;

                            udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                            udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                            break;
                        }
                    }
                    catch (Exception)
                    {

                    }

                    Thread.Sleep(delayBtwMsg);
                }
            }
            else
            {
                try
                {
                    using (var udpClient = new UdpClient())
                    {
                        byte[] sendData = Encoding.UTF8.GetBytes("/lo announce(\"" + message + "\");");
                        byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                        udpClient.Client.SendTimeout = timeout;
                        udpClient.Client.ReceiveTimeout = timeout;

                        udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                        udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                    }
                }
                catch (Exception)
                {

                }

            }


        }

        private List<string> SplitAnnouncement(string input)
        {
            List<string> result = new List<string>();
            string[] words = input.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                // If adding the next word exceeds the maxLength, add the currentLine to the result
                if (currentLine.Length + word.Length + 1 > 38)
                {
                    result.Add(currentLine.Trim());
                    currentLine = ""; // Start a new line
                }

                // Add the word to the current line
                currentLine += (currentLine.Length > 0 ? " " : "") + word;
            }

            // Add any remaining words in the current line
            if (currentLine.Length > 0)
            {
                result.Add(currentLine.Trim());
            }

            return result;
        }


        public void sv_teamdamage(bool Enabled, int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData;
                    if (Enabled)
                    {
                        sendData = Encoding.UTF8.GetBytes("/sv teamdamage 1");
                    }
                    else
                    {
                        sendData = Encoding.UTF8.GetBytes("/sv teamdamage 0");
                    }

                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                }
            }
            catch (Exception)
            {

            }
        }

        public void sv_kick(int playerID, int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("/sv kick " + playerID.ToString());
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                    Thread.Sleep(100);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                }
            }
            catch (Exception)
            {

            }
        }

        public void sv_restartmap(int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("/sv restartmap");
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                    Thread.Sleep(100);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                }
            }
            catch (Exception)
            {

            }
        }

        public void sv_gotomap(int mapID, int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("/sv gotomap " + mapID.ToString());
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                }
            }
            catch (Exception)
            {

            }
        }

        public void sv_gotonext(int timeout = 500)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("/sv gotonext");
                    byte[] rcon = Encoding.UTF8.GetBytes(config.RCON);

                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    udpClient.Send(rcon, rcon.Length, config.IP, config.PORT);
                    udpClient.Send(sendData, sendData.Length, config.IP, config.PORT);
                }
            }
            catch (Exception)
            {

            }
        }

        public struct Networkconfig
        {
            public string ServerName { get; set; }
            public string ServerPassword { get; set; }
            public string SuperUserPassword { get; set; }
            public int ServerPort { get; set; }
            public int ClientPort { get; set; }
            public int MaxPlayers { get; set; }
            public int SpecMode { get; set; }
            public bool TeamDamage { get; set; }
            public bool AutoBalance { get; set; }
            public bool Warmup { get; set; }
            public bool Public { get; set; }
            public List<int> ActiveMaps { get; set; }
            public int MoneyStart { get; set; }
            public int MoneyCap { get; set; }
            public int MoneyKill { get; set; }
            public int MoneyTeamKill { get; set; }
            public int MoneyPlayerObjWin { get; set; }
            public int MoneyTeamObjWin { get; set; }
            public int MoneyTeamObjLost { get; set; }
            public int MoneyMissionWin { get; set; }
            public int MoneyMissionLost { get; set; }
            public int MapRounds { get; set; }
            public int MapTime { get; set; }
            public int MapTeamScore { get; set; }
            public int ObjTime { get; set; }
            public int BombTime { get; set; }
            public int SpawnCost { get; set; }
            public int SpawnTimer { get; set; }
            public int SpawnSafeTimer { get; set; }
            public bool AllowSniperRifles { get; set; }
            public int Smooth { get; set; }
            public int Bandwidth { get; set; }
            public int Choke { get; set; }
            public int FillPercent { get; set; }
            public int Timeout { get; set; }
            public bool AutoKick { get; set; }
            public string PlayerName { get; set; }
            public int BombRepoTime { get; set; }
            public bool ForceFirstSpec { get; set; }
            public int PingMax { get; set; }
            public int PlossMax { get; set; }
            public int IdleMax { get; set; }
            public int GoutMax { get; set; }

            public override string ToString()
            {
                var localCopy = this; // Copy the struct to a local variable
                return string.Join(", \n",
                    localCopy.GetType().GetProperties().Select(p => $"{p.Name}: {p.GetValue(localCopy)}")) + "\n\n" + string.Join("\n", ActiveMaps);
            }

        }

        public Networkconfig ParseNetworkconfig(string rawData)
        {
            Networkconfig config = new Networkconfig
            {
                ActiveMaps = new List<int>()
            };

            string[] lines = rawData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                // Ignore lines that do not start with 'lo '
                if (!line.TrimStart().StartsWith("lo "))
                    continue;

                try
                {
                    // Remove 'lo ' prefix and trim the line
                    string lineContent = line.Substring(3).Trim().Replace("(\"", " ");

                    // Split the line by spaces to handle both single-value and key-value cases
                    string[] parts = lineContent.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    // If there are not enough parts, log and continue to next line
                    if (parts.Length < 2)
                    {
                        continue;
                    }

                    // The key is the first part, and the value is the second part (trimmed)
                    string key = parts[0].Trim();
                    string value = lineContent.Remove(0, key.Length);

                    // Check if the value is enclosed in parentheses and quotes, and clean it up
                    if (value.EndsWith(");"))
                    {
                        value = value.Substring(1, value.Length - 3).Trim('"');
                    }

                    // Handle parsing based on key
                    switch (key)
                    {
                        case "svname":
                            config.ServerName = value;
                            break;
                        case "svpassword":
                            config.ServerPassword = value;
                            break;
                        case "rconpass":
                            config.SuperUserPassword = value;
                            break;
                        case "playername":
                            config.PlayerName = value;
                            break;
                        case "svport":
                            config.ServerPort = int.TryParse(value, out var svport) ? svport : 0;
                            break;
                        case "clport":
                            config.ClientPort = int.TryParse(value, out var clport) ? clport : 0;
                            break;
                        case "maxplayers":
                            config.MaxPlayers = int.TryParse(value, out var maxPlayers) ? maxPlayers : 0;
                            break;
                        case "specmode":
                            config.SpecMode = int.TryParse(value, out var specMode) ? specMode : 0;
                            break;
                        case "teamdamage":
                            config.TeamDamage = ParseBool(value);
                            break;
                        case "autobalance":
                            config.AutoBalance = ParseBool(value);
                            break;
                        case "warmup":
                            config.Warmup = ParseBool(value);
                            break;
                        case "public":
                            config.Public = ParseBool(value);
                            break;
                        case "activatemap":
                            if (int.TryParse(value, out var mapId))
                                config.ActiveMaps.Add(mapId);
                            break;
                        case "moneystart":
                            config.MoneyStart = int.TryParse(value, out var moneyStart) ? moneyStart : 0;
                            break;
                        case "moneycap":
                            config.MoneyCap = int.TryParse(value, out var moneyCap) ? moneyCap : 0;
                            break;
                        case "moneykill":
                            config.MoneyKill = int.TryParse(value, out var moneyKill) ? moneyKill : 0;
                            break;
                        case "moneyteamkill":
                            config.MoneyTeamKill = int.TryParse(value, out var moneyTeamKill) ? moneyTeamKill : 0;
                            break;
                        case "moneyplayerobjwin":
                            config.MoneyPlayerObjWin = int.TryParse(value, out var moneyPlayerObjWin) ? moneyPlayerObjWin : 0;
                            break;
                        case "moneyteamobjwin":
                            config.MoneyTeamObjWin = int.TryParse(value, out var moneyTeamObjWin) ? moneyTeamObjWin : 0;
                            break;
                        case "moneyteamobjlost":
                            config.MoneyTeamObjLost = int.TryParse(value, out var moneyTeamObjLost) ? moneyTeamObjLost : 0;
                            break;
                        case "moneymissionwin":
                            config.MoneyMissionWin = int.TryParse(value, out var moneyMissionWin) ? moneyMissionWin : 0;
                            break;
                        case "moneymissionlost":
                            config.MoneyMissionLost = int.TryParse(value, out var moneyMissionLost) ? moneyMissionLost : 0;
                            break;
                        case "maprounds":
                            config.MapRounds = int.TryParse(value, out var mapRounds) ? mapRounds : 0;
                            break;
                        case "maptime":
                            config.MapTime = int.TryParse(value, out var mapTime) ? mapTime : 0;
                            break;
                        case "mapteamscore":
                            config.MapTeamScore = int.TryParse(value, out var mapTeamScore) ? mapTeamScore : 0;
                            break;
                        case "objtime":
                            config.ObjTime = int.TryParse(value, out var objTime) ? objTime : 0;
                            break;
                        case "bombtime":
                            config.BombTime = int.TryParse(value, out var bombTime) ? bombTime : 0;
                            break;
                        case "spawncost":
                            config.SpawnCost = int.TryParse(value, out var spawnCost) ? spawnCost : 0;
                            break;
                        case "spawntimer":
                            config.SpawnTimer = int.TryParse(value, out var spawnTimer) ? spawnTimer : 0;
                            break;
                        case "spawnsafetimer":
                            config.SpawnSafeTimer = int.TryParse(value, out var spawnSafeTimer) ? spawnSafeTimer : 0;
                            break;
                        case "allowsniperrifles":
                            config.AllowSniperRifles = ParseBool(value);
                            break;
                        case "smooth":
                            config.Smooth = int.TryParse(value, out var smooth) ? smooth : 0;
                            break;
                        case "bandwidth":
                            config.Bandwidth = int.TryParse(value, out var bandwidth) ? bandwidth : 0;
                            break;
                        case "choke":
                            config.Choke = int.TryParse(value, out var choke) ? choke : 0;
                            break;
                        case "fillpercent":
                            config.FillPercent = int.TryParse(value, out var fillPercent) ? fillPercent : 0;
                            break;
                        case "timeout":
                            config.Timeout = int.TryParse(value, out var timeout) ? timeout : 0;
                            break;
                        case "autokick":
                            config.AutoKick = ParseBool(value);
                            break;
                        case "bombrepostime":
                            config.BombRepoTime = int.TryParse(value, out var bombRepoTime) ? bombRepoTime : 0;
                            break;
                        case "forcefirstspec":
                            config.ForceFirstSpec = ParseBool(value);
                            break;
                        case "pingmax":
                            config.PingMax = int.TryParse(value, out var pingMax) ? pingMax : 0;
                            break;
                        case "plossmax":
                            config.PlossMax = int.TryParse(value, out var plossMax) ? plossMax : 0;
                            break;
                        case "idlemax":
                            config.IdleMax = int.TryParse(value, out var idleMax) ? idleMax : 0;
                            break;
                        case "goutmax":
                            config.GoutMax = int.TryParse(value, out var goutMax) ? goutMax : 0;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error parsing line '{line}': {ex.Message}");
                }
            }

            return config;
        }

        public Process StartServer()
        {
            Process igi2;

            if (Process.GetProcessesByName("igi2").Length != 0)
            {
                foreach (var cur in Process.GetProcessesByName("igi2"))
                {
                    cur.Kill();
                }
            }
            igi2 = Process.Start("igi2.exe", "serverdedicated window quite");

            return igi2;
        }

        private bool ParseBool(string value) => value == "1";

    }

    public class IGIEntity
    {
        private Mem mem = new Mem();
        private Process _attachedProcess;

        public List<Player> Players_Live { get; private set; } = new List<Player>();

        private CancellationTokenSource _cancellationTokenSource;
        private Task _monitoringTask;
        private bool _isMonitoring = false;

        public event EventHandler<List<Player>> PlayersUpdated;

        public event EventHandler<string> StatusMessage;

        // New Events for player state changes
        public event EventHandler<Player> PlayerJoined;
        public event EventHandler<Player> PlayerLeft;
        public event EventHandler<PlayerTeamChangedEventArgs> PlayerTeamChanged;

        public class PlayerTeamChangedEventArgs : EventArgs
        {
            public Player Player { get; }
            public int OldTeam { get; }
            public int NewTeam { get; }

            public PlayerTeamChangedEventArgs(Player player, int oldTeam, int newTeam)
            {
                Player = player;
                OldTeam = oldTeam;
                NewTeam = newTeam;
            }
        }

        public class Player : IEquatable<Player>
        {
            public int ID { get; set; }
            public bool RCON { get; set; }
            public bool NetworkMonitoring { get; set; }
            public int Team { get; set; }
            public string Name { get; set; }
            public string TeamName { get; set; }
            public string Model { get; set; }
            public int Idle { get; set; }
            public int Ping { get; set; }
            public int LastPing { get; set; }
            public int AveragePing { get; set; }
            public int MaximumPing { get; set; }
            public string PacketLoss { get; set; }
            public int Kills { get; set; }
            public int Deaths { get; set; }
            public int Objectives { get; set; }
            public int Money { get; set; }

            public Player(int PlayerID = -1, bool PlayerRCON = false, bool PlayerNetworkMonitoring = false, int PlayerTeam = -1, string PlayerName = "",
                          string PlayerTeamName = "", string PlayerModel = "", int PlayerIdle = -1, int PlayerPing = -1, int PlayerLastPing = -1, int PlayerAveragePing = -1,
                          int PlayerMaximumPing = -1, string PlayerPacketLoss = "", int PlayerKills = -1, int PlayerDeaths = -1, int PlayerObjectives = -1, int PlayerMoney = -1)
            {
                ID = PlayerID;
                RCON = PlayerRCON;
                NetworkMonitoring = PlayerNetworkMonitoring;
                Team = PlayerTeam;
                Name = PlayerName;
                TeamName = PlayerTeamName;
                Model = PlayerModel;
                Idle = PlayerIdle;
                Ping = PlayerPing;
                LastPing = PlayerLastPing;
                AveragePing = PlayerAveragePing;
                MaximumPing = PlayerMaximumPing;
                PacketLoss = PlayerPacketLoss;
                Kills = PlayerKills;
                Deaths = PlayerDeaths;
                Objectives = PlayerObjectives;
                Money = PlayerMoney;
            }

            public override bool Equals(object obj)
            {
                return obj is Player other && Equals(other);
            }

            public bool Equals(Player other)
            {
                return (ID == other.ID && Name == other.Name);
            }
        }

        public static class config
        {
            public static string processName = "";
            public static bool autoAttachToProcess = true;

            public static string EnitityListAddress = "igi2.exe+079533AC+238+10";
            public static string nextPlayerOffset = "4";
            public static string BufferSize = "igi2.exe+07953134,8,8,A4";

            public static class MapStatAddresses
            {
                public static string NumberOfPlayers = "igi2.exe+07953540+0";
                public static string DroppedWeaponsCount = "igi2.exe+0x079533A0,0";
                public static string MapID = "igi2.exe+0x7953154";
                public static string CurrentRound = "igi2.exe+07953134,8,8,0,4,34";
                public static string ServerPassword = "igi2.exe+7B2A400";
            }

            public static class StatOffsets
            {
                public static string ID = "28";
                public static string RCON = "30";
                public static string NetworkMonitoring = "34";
                public static string Team = "40";
                public static string Name = "54";
                public static string TeamName = "94";
                public static string PlayerModel = "D4";
                public static string Idle = "E8";
                public static string Ping = "10C";
                public static string LastPing = "110";
                public static string AveragePing = "114";
                public static string MaximumPing = "118";
                public static string PacketLoss = "11C";
                public static string Kills = "144";
                public static string Deaths = "148";
                public static string Objectives = "14C";
                public static string Money = "264";
            }
        }

        public IGIEntity(string processName = "igi2.exe")
        {
            config.processName = processName;
        }

        public void StartMonitoring(int pollingIntervalMs = 100)
        {
            if (_isMonitoring)
            {
                StatusMessage?.Invoke(this, "Monitoring is already active.");
                return;
            }

            _isMonitoring = true;
            _cancellationTokenSource = new CancellationTokenSource();

            _monitoringTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_attachedProcess == null || _attachedProcess.HasExited)
                        {
                            mem.CloseProcess();
                            StatusMessage?.Invoke(this, $"Attempting to attach to {config.processName}...");

                            Process[] processes = Process.GetProcessesByName(config.processName);
                            if (processes.Length > 0)
                            {
                                _attachedProcess = processes[0];
                                if (mem.OpenProcess(_attachedProcess.Id))
                                {
                                    StatusMessage?.Invoke(this, $"Successfully attached to {config.processName} (PID: {_attachedProcess.Id}).");
                                }
                                else
                                {
                                    _attachedProcess = null;
                                    StatusMessage?.Invoke(this, $"Failed to attach to {config.processName} by PID. Retrying in {pollingIntervalMs}ms...");
                                    await Task.Delay(pollingIntervalMs, _cancellationTokenSource.Token);
                                    continue;
                                }
                            }
                            else
                            {
                                _attachedProcess = null;
                                StatusMessage?.Invoke(this, $"Process '{config.processName}' not found. Retrying in {pollingIntervalMs}ms...");
                                await Task.Delay(pollingIntervalMs, _cancellationTokenSource.Token);
                                continue;
                            }
                        }

                        var numPlayers = TryReadInt(config.MapStatAddresses.NumberOfPlayers);

                        List<Player> currentPlayers = new List<Player>();

                        if (numPlayers > 0 && numPlayers < 100)
                        {
                            for (int i = 0; i < numPlayers; i++)
                            {
                                Player curPlayer = new Player();
                                string currentPlayerBasePointerChain = $"{config.EnitityListAddress},{i * int.Parse(config.nextPlayerOffset):X}";

                                curPlayer.ID = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.ID);
                                curPlayer.RCON = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.RCON) == 1;
                                curPlayer.NetworkMonitoring = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.NetworkMonitoring) == 1;
                                curPlayer.Team = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Team);
                                curPlayer.Name = TryReadString(currentPlayerBasePointerChain + "," + config.StatOffsets.Name, 64);
                                curPlayer.TeamName = TryReadString(currentPlayerBasePointerChain + "," + config.StatOffsets.TeamName, 64);
                                curPlayer.Model = TryReadString(currentPlayerBasePointerChain + "," + config.StatOffsets.PlayerModel, 64);
                                curPlayer.Idle = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Idle);
                                curPlayer.Ping = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Ping);
                                curPlayer.LastPing = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.LastPing);
                                curPlayer.AveragePing = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.AveragePing);
                                curPlayer.MaximumPing = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.MaximumPing);
                                curPlayer.PacketLoss = TryReadString(currentPlayerBasePointerChain + "," + config.StatOffsets.PacketLoss, 32);
                                curPlayer.Kills = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Kills);
                                curPlayer.Deaths = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Deaths);
                                curPlayer.Objectives = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Objectives);
                                curPlayer.Money = TryReadInt(currentPlayerBasePointerChain + "," + config.StatOffsets.Money);

                                if (curPlayer.ID != -1 && !string.IsNullOrWhiteSpace(curPlayer.Name) && curPlayer.Name != "null")
                                {
                                    currentPlayers.Add(curPlayer);
                                }
                            }
                        }
                        else if (numPlayers == 0)
                        {
                        }
                        else
                        {
                            StatusMessage?.Invoke(this, $"Warning: Unexpected number of players read: {numPlayers}. Skipping player list update.");
                        }

                        // --- Event Triggering Logic ---
                        // Players who exist in currentPlayers but not in Players_Live
                        var playersJoined = currentPlayers.Except(Players_Live).ToList();
                        foreach (var player in playersJoined)
                        {
                            PlayerJoined?.Invoke(this, player);
                        }

                        // Players who exist in Players_Live but not in currentPlayers
                        var playersLeft = Players_Live.Except(currentPlayers).ToList();
                        foreach (var player in playersLeft)
                        {
                            PlayerLeft?.Invoke(this, player);
                        }

                        // Team changes: find players in both lists by ID and Name, then compare Team property
                        foreach (var newPlayer in currentPlayers)
                        {
                            var oldPlayer = Players_Live.FirstOrDefault(p => p.ID == newPlayer.ID && p.Name == newPlayer.Name);
                            if (oldPlayer != null) // If the player existed in the previous list
                            {
                                // Check for team change, excluding -1 (invalid/default team)
                                if (newPlayer.Team != oldPlayer.Team && newPlayer.Team != -1 && oldPlayer.Team != -1)
                                {
                                    PlayerTeamChanged?.Invoke(this, new PlayerTeamChangedEventArgs(newPlayer, oldPlayer.Team, newPlayer.Team));
                                }
                            }
                        }
                        // --- End Event Triggering Logic ---

                        Players_Live = currentPlayers;
                        PlayersUpdated?.Invoke(this, currentPlayers);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        StatusMessage?.Invoke(this, $"Error during monitoring: {ex.Message}");
                        _attachedProcess = null;
                    }

                    await Task.Delay(pollingIntervalMs, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
        }

        public async Task StopMonitoring()
        {
            if (!_isMonitoring)
            {
                StatusMessage?.Invoke(this, "Monitoring is not active.");
                return;
            }

            StatusMessage?.Invoke(this, "Stopping monitoring...");
            _cancellationTokenSource?.Cancel();

            if (_monitoringTask != null)
            {
                await Task.WhenAny(_monitoringTask, Task.Delay(5000));
                if (!_monitoringTask.IsCompleted)
                {
                    StatusMessage?.Invoke(this, "Monitoring task did not complete gracefully within timeout. Forcing close.");
                }
            }

            mem.CloseProcess();
            _attachedProcess?.Dispose();
            _attachedProcess = null;
            _isMonitoring = false;
            StatusMessage?.Invoke(this, "Monitoring stopped.");
        }

        private int TryReadInt(string address, int defaultValue = -1)
        {
            try
            {
                return mem.ReadInt(address);
            }
            catch
            {
                return defaultValue;
            }
        }

        private string TryReadString(string address, int length, string defaultValue = "")
        {
            try
            {
                return mem.ReadString(address, length: length);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}