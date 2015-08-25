using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using MySql.Data.MySqlClient;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

using System.Reflection;

namespace Statistics
{
	[ApiVersion(1, 21)]
	public class Statistics : TerrariaPlugin
	{
		internal static Database tshock;
		internal static Database database;
		internal static readonly Dictionary<TSPlayer, TSPlayer> PlayerKilling = new Dictionary<TSPlayer, TSPlayer>();
		internal static readonly int[] TimeCache = new int[Main.player.Length];

		internal static readonly Dictionary<KillType, int>[] SentDamageCache = 
			new Dictionary<KillType, int>[Main.player.Length];

		internal static readonly int[] RecvDamageCache = new int[Main.player.Length];

		private readonly Timer _counter = new Timer(1000);
		private readonly Timer _timeSaver = new Timer(60*1000*5);

		public override string Author
		{
			get { return "Grandpa-G"; }     //Forked from WhiteXZ with permission
		}

		public override string Description
		{
			get { return "Stat tracking for Terraria"; }
		}

		public override string Name
		{
			get { return "Statistics"; }
		}

		public override Version Version
		{
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}


		public Statistics(Main game)
			: base(game)
		{
		}

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, GetData);
			ServerApi.Hooks.NetGreetPlayer.Register(this, GreetPlayer);
			ServerApi.Hooks.ServerLeave.Register(this, PlayerLeave);
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);

			PlayerHooks.PlayerPostLogin += PlayerPostLogin;

			GetDataHandlers.InitGetDataHandler();

			_counter.Elapsed += CounterOnElapsed;
			_counter.Start();
			_timeSaver.Elapsed += TimeSaverOnElapsed;
			_timeSaver.Start();


			TShockAPI.Commands.ChatCommands.Add(new Command("statistics.root", Commands.Core, "info"));
		}

		private void OnInitialize(EventArgs args)
		{
			database = Database.InitDb("Statistics");
			tshock = Database.InitDb("tshock");

			var table = new SqlTable("Statistics",
				new SqlColumn("ID", MySqlDbType.Int32) {Unique = true, Primary = true, AutoIncrement = true},
				new SqlColumn("UserID", MySqlDbType.Int32) {Unique = true},
				new SqlColumn("Time", MySqlDbType.Int32),
				new SqlColumn("PlayerKills", MySqlDbType.Int32),
				new SqlColumn("Deaths", MySqlDbType.Int32),
				new SqlColumn("MobKills", MySqlDbType.Int32),
				new SqlColumn("BossKills", MySqlDbType.Int32),
				new SqlColumn("Logins", MySqlDbType.Int32),
				new SqlColumn("MobDamageGiven", MySqlDbType.Int32),
				new SqlColumn("BossDamageGiven", MySqlDbType.Int32),
				new SqlColumn("PlayerDamageGiven", MySqlDbType.Int32),
				new SqlColumn("DamageReceived", MySqlDbType.Int32));

			var table2 = new SqlTable("Highscores",
				new SqlColumn("ID", MySqlDbType.Int32) {Unique = true, Primary = true, AutoIncrement = true},
				new SqlColumn("UserID", MySqlDbType.Int32) {Unique = true},
				new SqlColumn("Score", MySqlDbType.Int32));

			database.EnsureExists(table, table2);

			//database.Import();
		}

		private static void TimeSaverOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			foreach (var player in TShock.Players)
				if (player != null && player.ConnectionAlive && player.RealPlayer && player.IsLoggedIn)
				{
					database.UpdateTime(player.User.ID, TimeCache[player.Index]);
					TimeCache[player.Index] = 0;
				}
		}

		private static void CounterOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			foreach (var player in TShock.Players)
				if (player != null && player.ConnectionAlive && player.RealPlayer && player.IsLoggedIn)
					TimeCache[player.Index]++;
		}

		private static void PlayerPostLogin(PlayerPostLoginEventArgs args)
		{
			database.CheckUpdateInclude(args.Player.User.ID);
		}

		private static void GreetPlayer(GreetPlayerEventArgs args)
		{
			if (TShock.Players[args.Who] == null) return;

			if (PlayerKilling.ContainsKey(TShock.Players[args.Who]))
				PlayerKilling.Remove(TShock.Players[args.Who]);

			PlayerKilling.Add(TShock.Players[args.Who], null);

			TimeCache[args.Who] = 0;

			SentDamageCache[args.Who] = new Dictionary<KillType, int>
			{
				{KillType.Mob, 0},
				{KillType.Boss, 0},
				{KillType.Player, 0}
			};

			RecvDamageCache[args.Who] = 0;
		}

		private static void PlayerLeave(LeaveEventArgs args)
		{
			if (TShock.Players[args.Who] == null) return;

			if (PlayerKilling.ContainsKey(TShock.Players[args.Who]))
				PlayerKilling.Remove(TShock.Players[args.Who]);

			if (TShock.Players[args.Who].IsLoggedIn)
			{
				database.UpdateTime(TShock.Players[args.Who].User.ID, TimeCache[args.Who]);
				TimeCache[args.Who] = 0;
			}
		}

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
					TShock.Log.ConsoleError(ex.ToString());
				}
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, GetData);
				ServerApi.Hooks.NetGreetPlayer.Deregister(this, GreetPlayer);
				ServerApi.Hooks.ServerLeave.Deregister(this, PlayerLeave);
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);

				PlayerHooks.PlayerPostLogin -= PlayerPostLogin;
			}
			base.Dispose(disposing);
		}
	}

	public static class Extensions
	{
		public static string SToString(this TimeSpan ts)
		{
			var sb = new StringBuilder();
			if (ts.Days > 0)
				sb.Append(string.Format("{0} day{1}{2}", ts.Days, ts.Days.Suffix(),
					ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
			if (ts.Hours > 0)
				sb.Append(string.Format("{0} hour{1}{2}", ts.Hours, ts.Hours.Suffix(),
					ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
			if (ts.Minutes > 0)
				sb.Append(string.Format("{0} minute{1}{2}", ts.Minutes, ts.Minutes.Suffix(),
					ts.Seconds > 0 ? ", " : ""));
			if (ts.Seconds > 0 || sb.Length == 0)
				sb.Append(string.Format("{0} second{1}", ts.Seconds, ts.Seconds.Suffix()));

			if (sb.Length == 0)
			{
				TShock.Log.ConsoleInfo("Timespan error. Possible time check of an unplayed account.");
				return "an unknown period of time";
			}
			return sb.ToString();
		}

		public static string Suffix(this int s, bool es = false)
		{
			if (s > 1 || s == 0)
				return (es ? "es" : "s");

			return string.Empty;
		}
	}
}