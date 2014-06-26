using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Terraria;
using TerrariaApi.Server;

using TShockAPI;

namespace Statistics
{
    [ApiVersion(1, 16)]
    public class Statistics : TerrariaPlugin
    {
        public static readonly List<SPlayer> Players = new List<SPlayer>();
        public static readonly List<StoredPlayer> StoredPlayers = new List<StoredPlayer>();
        public static readonly HighScores HighScores = new HighScores();
        private IDbConnection _db;
        public static Database database;
        public static readonly Tools Tools = new Tools();

        private Timers _timers;

        /// <summary>
        /// Not yet implemented
        /// </summary>
        public static bool Ssc { get { return TShock.Config.ServerSideCharacter; } }

        public override string Author
        { get { return "WhiteX"; } }

        public override string Description
        { get { return "Statistics for players"; } }

        public override string Name
        { get { return "Statistics"; } }

        public override Version Version
        { get { return new Version(1, 3); } }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    var dispose = new Thread(DisposeThread);

                    dispose.Start();
                    dispose.Join();
                }
                catch (Exception x)
                {
                    Log.ConsoleError(x.ToString());
                }
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                ServerApi.Hooks.NetGetData.Deregister(this, GetData);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PostLogin;

            }
            base.Dispose(disposing);
        }

        private static void DisposeThread()
        {
            database.SaveState();
            Thread.Sleep(600);
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            ServerApi.Hooks.NetGetData.Register(this, GetData);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PostLogin;

            GetDataHandlers.InitGetDataHandler();
        }

        #region OnInitialize

        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("stats.check", CmdCheck, "check")
            {
                HelpText = "Base command for the Statistics plugin. Allows you to view statistics about players"
            });

            Commands.ChatCommands.Add(new Command("stats.uix", SCommands.CmdUiEx, "uix")
            {
                HelpText = "Extended user information"
            });

            Commands.ChatCommands.Add(new Command("stats.uic", SCommands.CmdUic, "uic")
            {
                HelpText = "Provides information about a player's character"
            });

            Commands.ChatCommands.Add(new Command("stats.highscores", SCommands.CmdHighScores, "hs", "highscores")
            {
                HelpText = "Displays server highscores, calculated by kills, deaths and time played"
            });

            Tools.RegisterSubs();


            if (TShock.Config.StorageType.ToLower() == "sqlite")
                _db = new SqliteConnection(string.Format("uri=file://{0},Version=3", Path.Combine(TShock.SavePath, "Statistics.sqlite")));
            else if (TShock.Config.StorageType.ToLower() == "mysql")
            {
                try
                {
                    var host = TShock.Config.MySqlHost.Split(':');
                    _db = new MySqlConnection
                    {
                        ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword
                            )
                    };
                }
                catch (MySqlException x)
                {
                    Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
            }
            else
                throw new Exception("Invalid storage type.");

            database = new Database(_db);
        }
        #endregion

        #region PostInitialize

        private void PostInitialize(EventArgs args)
        {
            Database.SyncUsers();
            Database.SyncHighScores();
            _timers = new Timers();
            _timers.Start();
        }
        #endregion

        #region PostLogin

        private static void PostLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs args)
        {
            SPlayer player = Tools.GetPlayer(args.Player.Index);

            if (player == null)
            {
                Players.Add(new SPlayer(args.Player.Index));
                PostLogin(new TShockAPI.Hooks.PlayerPostLoginEventArgs(args.Player));
                return;
            }

            var storage = Tools.GetStoredPlayers(args.Player.UserAccountName);

            if (storage.Count > 1)
            {
                Log.ConsoleError("Multiple match error! --Attempting to obtain stored player for {0} resulted in" +
                                 " {1} matches: {2}", args.Player.UserAccountName,
                    Tools.GetStoredPlayers(args.Player.UserAccountName).Count,
                    Tools.GetStoredPlayers(args.Player.UserAccountName).Select(p => p.name));
                return;
            }
            if (storage.Count == 0)
            {
                Log.ConsoleInfo("{0} has been registered to Statistics", args.Player.UserAccountName);

                player.storage = StoredPlayers.AddAndReturn(new StoredPlayer(args.Player.UserAccountName,
                    DateTime.Now.ToString("G"), DateTime.Now.ToString("G"), 0, 1,
                    args.Player.UserAccountName, args.Player.IP, 0, 0, 0, 0));

                database.AddUser(player.storage);
                player.SyncStats();

                HighScores.highScores.Add(new HighScore(player.Name, player.kills, player.mobkills, player.deaths,
                    player.bosskills, player.timePlayed));

                return;
            }

            player.storage = storage[0];

            if (player.storage.knownIPs.Length > 0)
            {
                if (!player.storage.knownIPs.Contains(args.Player.IP))
                    player.storage.knownIPs += ", " + args.Player.IP;
            }
            else
                player.storage.knownIPs = args.Player.IP;

            if (player.storage.knownAccounts.Length > 0)
            {
                if (!player.storage.knownAccounts.Contains(args.Player.UserAccountName))
                    player.storage.knownAccounts += ", " + args.Player.UserAccountName;
            }
            else
                player.storage.knownAccounts = args.Player.UserAccountName;

            player.SyncStats();

            Log.ConsoleInfo("Successfully linked account {0} with stored player {1}",
                args.Player.UserAccountName, player.storage.name);
        }

        #endregion

        #region OnLeave

        private static void OnLeave(LeaveEventArgs args)
        {
            SPlayer player = Tools.GetPlayer(args.Who);
            if (player == null)
                return;

            player.SaveStats();

            Players.RemoveAll(p => p.index == player.index);
        }
        #endregion

        #region OnGreet

        private static void OnGreet(GreetPlayerEventArgs args)
        {
            var player = Tools.GetPlayer(args.Who);

            if (player != null)
                return;

            Players.Add(new SPlayer(args.Who));

            if (TShock.Config.DisableUUIDLogin) return;

            if (TShock.Players[args.Who].IsLoggedIn)
                PostLogin(new TShockAPI.Hooks.PlayerPostLoginEventArgs(TShock.Players[args.Who]));
        }
        #endregion

        #region GetData

        private static void GetData(GetDataEventArgs args)
        {
            var type = args.MsgID;
            var player = TShock.Players[args.Msg.whoAmI];

            if (player == null)
            {
                args.Handled = true;
                return;
            }

            if (!player.ConnectionAlive)
            {
                args.Handled = true;
                return;
            }

            using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {
                try
                {
                    if (GetDataHandlers.HandlerGetData(type, player, data))
                        args.Handled = true;
                }
                catch (Exception ex)
                {
                    Log.ConsoleError(ex.ToString());
                }
            }
        }
        #endregion

        #region OnChat

        private static void OnChat(ServerChatEventArgs args)
        {
            var player = Tools.GetPlayer(args.Who);
            if (player == null) return;

            if (args.Text.StartsWith("/check")) return;

            if (player.afkCount > 0)
                player.afkCount = 0;

            if (player.afk)
            {
                player.afk = false;
            }
        }

        #endregion

        public Statistics(Main game)
            : base(game)
        {
            Order = 100;
        }

        private static void CmdCheck(CommandArgs args)
        {
            Tools.handler.RunSubcommand(args);
        }
    }
}
