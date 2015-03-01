using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace Statistics
{
	public enum KillType
	{
		Mob = 0,
		Boss,
		Player
	}

	public class Database
	{
		private readonly IDbConnection _db;
		private static bool MySql { get; set; }

		//internal void Import()
		//{
		//	using (var reader = Statistics.tshock.QueryReader("SELECT ID FROM Users"))
		//	{
		//		while (reader.Read())
		//		{
		//			var query = MySql
		//				? "INSERT INTO Statistics (UserID, PlayerKills, Deaths, MobKills, BossKills, Logins, Time) " +
		//				  "VALUES (@0, @1, @2, @3, @4, @5, @6) ON DUPLICATE KEY UPDATE Time=Time"
		//				: "INSERT OR IGNORE INTO Statistics (UserID, PlayerKills, Deaths, MobKills, BossKills, Logins, Time) " +
		//				  "VALUES (@0, @1, @2, @3, @4, @5, @6)";
		//			Query(query, reader.Get<string>("ID"), 0, 0, 0, 0, 0, 0);

		//			query = MySql
		//				? "INSERT INTO Highscores (UserID, Score) VALUES (@0, @1) ON DUPLICATE KEY UPDATE Score=Score"
		//				: "INSERT OR IGNORE INTO Highscores (UserID, Score) VALUES (@0, @1)";
		//			Query(query, reader.Get<string>("ID"), 0);
		//		}
		//	}
		//}

		internal void CheckUpdateInclude(int userId)
		{
			var update = false;
			using (var reader = QueryReader("SELECT Logins FROM Statistics WHERE UserID = @0", userId))
			{
				if (reader.Read())
					update = true;
			}
			if (update)
				Query("UPDATE Statistics SET Logins = Logins + 1 WHERE UserID = @0", userId);
			else
			{
				Query("INSERT INTO Statistics (UserID, PlayerKills, Deaths, MobKills, BossKills, Logins, Time) " +
				      "VALUES (@0, @1, @2, @3, @4, @5, @6)", userId, 0, 0, 0, 0, 0, 0);
				Query("INSERT INTO Highscores (UserID, Score) VALUES (@0, @1)", userId, 0);
			}
		}

		internal QueryResult QueryReader(string query, params object[] args)
		{
			return _db.QueryReader(query, args);
		}

		internal int Query(string query, params object[] args)
		{
			return _db.Query(query, args);
		}

		internal void EnsureExists(SqlTable table)
		{
			var creator = new SqlTableCreator(_db,
				_db.GetSqlType() == SqlType.Sqlite
					? (IQueryBuilder) new SqliteQueryCreator()
					: new MysqlQueryCreator());

			creator.EnsureExists(table);
		}

		internal void EnsureExists(params SqlTable[] tables)
		{
			foreach (var table in tables)
				EnsureExists(table);
		}

		internal void UpdateTime(int userId, int time)
		{
			var query = string.Format("UPDATE Statistics SET Time = Time + {0} WHERE UserID = @0",
				time);
			Query(query, userId);
		}

		internal void UpdateKills(int userId, KillType type)
		{
			var query = string.Format("UPDATE Statistics SET {0}Kills = {0}Kills + 1 WHERE UserID = @0",
				type);

			Query(query, userId);
		}

		internal void UpdateDeaths(int userId)
		{
			Query("UPDATE Statistics SET Deaths = Deaths + 1 WHERE UserID = @0", userId);
		}

		internal void UpdateHighScores(int userId)
		{
			var kills = GetKills(userId);
			var score = ((2*kills[0]) + kills[1] + (3*kills[2]))/(kills[3] == 0 ? 1 : kills[3]);

			var query = string.Format("UPDATE Highscores SET Score = {0} WHERE UserID = @0",
				score);
			Query(query, userId);
		}

		internal int[] GetKills(int userId)
		{
			using (
				var reader =
					QueryReader("SELECT PlayerKills, MobKills, BossKills, Deaths FROM Statistics WHERE UserID = @0",
						userId))
			{
				if (reader.Read())
				{
					return new[]
					{
						reader.Get<int>("PlayerKills"), reader.Get<int>("MobKills"), reader.Get<int>("BossKills"),
						reader.Get<int>("Deaths")
					};
				}
			}
			return null;
		}

		/// <summary>
		/// Returns an array of timespans. 
		/// [0] -> registered time
		/// [1] -> played time
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		internal TimeSpan[] GetTimes(int userId)
		{
			var ts = new TimeSpan[2];
			using (
				var reader = Statistics.tshock.QueryReader("SELECT Registered FROM Users WHERE ID = @0",
					userId))
			{
				if (reader.Read())
				{
					ts[0] = DateTime.UtcNow -
					        DateTime.ParseExact(reader.Get<string>("Registered"), "s", CultureInfo.CurrentCulture,
						        DateTimeStyles.AdjustToUniversal);
				}
				else
					return null;
			}

			using (var reader = QueryReader("SELECT Time from Statistics WHERE UserID = @0", userId))
			{
				if (reader.Read())
					ts[1] = new TimeSpan(0, 0, 0, reader.Get<int>("Time"));
				else
					return null;
			}

			return ts;
		}

		internal TimeSpan GetLastSeen(int userId)
		{
			using (var reader = Statistics.tshock.QueryReader("SELECT LastAccessed FROM Users WHERE ID = @0",
				userId))
			{
				if (reader.Read())
				{
					return DateTime.UtcNow -
					       DateTime.ParseExact(reader.Get<string>("LastAccessed"), "s", CultureInfo.CurrentCulture,
						       DateTimeStyles.AdjustToUniversal);
				}
			}
			return TimeSpan.MaxValue;
		}

		internal Dictionary<string, int> GetHighScores(int page)
		{
			var ret = new Dictionary<string, int>();
			var index = (page - 1)*5;
			var query = string.Format("SELECT * FROM Highscores ORDER BY -Score LIMIT {0}, {1}", index, index + 5);
			using (var reader = QueryReader(query))
			{
				while (reader.Read())
					ret.Add(TShock.Users.GetUserByID(reader.Get<int>("UserID")).Name, reader.Get<int>("Score"));
			}
			return ret;
		}


		private Database(IDbConnection db)
		{
			_db = db;
		}

		public static Database InitDb(string name)
		{
			IDbConnection idb;

			if (TShock.Config.StorageType.ToLower() == "sqlite")
				idb =
					new SqliteConnection(string.Format("uri=file://{0},Version=3",
						Path.Combine(TShock.SavePath, name + ".sqlite")));

			else if (TShock.Config.StorageType.ToLower() == "mysql")
			{
				try
				{
					var host = TShock.Config.MySqlHost.Split(':');
					idb = new MySqlConnection
					{
						ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
							host[0],
							host.Length == 1 ? "3306" : host[1],
							TShock.Config.MySqlDbName,
							TShock.Config.MySqlUsername,
							TShock.Config.MySqlPassword
							)
					};
					MySql = true;
				}
				catch (MySqlException x)
				{
					Log.Error(x.ToString());
					throw new Exception("MySQL not setup correctly.");
				}
			}
			else
				throw new Exception("Invalid storage type.");

			var db = new Database(idb);
			return db;
		}
	}
}