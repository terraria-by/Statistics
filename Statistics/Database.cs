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
        private static SqlTableCreator _creator;
        private static bool _mysql;

        internal void Import()
        {
            using (var reader = Statistics.tshock.QueryReader("SELECT Username FROM Users"))
            {
                while (reader.Read())
                {
                    var query = _mysql
                        ? "INSERT INTO Statistics (Username, PlayerKills, Deaths, MobKills, BossKills, Logins, Time) " +
                          "VALUES (@0, @1, @2, @3, @4, @5, @6) ON DUPLICATE KEY UPDATE Time=Time"
                        : "INSERT OR IGNORE INTO Statistics (Username, PlayerKills, Deaths, MobKills, BossKills, Logins, Time) " +
                          "VALUES (@0, @1, @2, @3, @4, @5, @6)";
                    Query(query, reader.Get<string>("Username"), 0, 0, 0, 0, 0, 0);

                    query = _mysql
                        ? "INSERT INTO Highscores (Username, Score) VALUES (@0, @1) ON DUPLICATE KEY UPDATE Score=Score"
                        : "INSERT OR IGNORE INTO Highscores (Username, Score) VALUES (@0, @1)";
                    Query(query, reader.Get<string>("Username"), 0);
                }
            }
        }

        internal void CheckUpdateInclude(string username)
        {
            using (var reader = QueryReader("SELECT Logins FROM Statistics WHERE Username = @0", username))
            {
                if (reader.Read())
                    Query("UPDATE Statistics SET Logins = Logins + 1 WHERE Username = @0", username);
                else
                {
                    Query("INSERT INTO Statistics (Username, PlayerKills, Deaths, MobKills, BossKills, Logins, Time) " +
                          "VALUES (@0, @1, @2, @3, @4, @5, @6)", username, 0, 0, 0, 0, 0, 0);
                    Query("INSERT INTO Highscores (Username, Score) VALUES (@0, @1)", username, 0);
                }
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
            _creator.EnsureExists(table);
        }

        internal void EnsureExists(params SqlTable[] tables)
        {
            foreach (var table in tables)
                EnsureExists(table);
        }

        internal void UpdateTime(string username, int time)
        {
            var query = string.Format("UPDATE Statistics SET Time = Time + {0} WHERE Username = @0",
                time);
            Query(query, username);
        }

        internal void UpdateKills(string username, KillType type)
        {
            var query = string.Format("UPDATE Statistics SET {0}Kills = {0}Kills + 1 WHERE Username = @0",
                type);

            Query(query, username);
        }

        internal void UpdateDeaths(string username)
        {
            Query("UPDATE Statistics SET Deaths = Deaths + 1 WHERE Username = @0", username);
        }

        internal void UpdateHighScores(string username)
        {
            var kills = GetKills(username);
            var score = ((2*kills[0]) + kills[1] + (3*kills[2]))/(kills[3] == 0 ? 1 : kills[3]);

            var query = string.Format("UPDATE Highscores SET Score = {0} WHERE Username = @0",
                score);
            Query(query, username);
        }

        internal int[] GetKills(string username)
        {
            using (
                var reader =
                    QueryReader("SELECT PlayerKills, MobKills, BossKills, Deaths FROM Statistics WHERE Username = @0",
                        username))
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
        /// <param name="username"></param>
        /// <returns></returns>
        internal TimeSpan[] GetTimes(string username)
        {
            var ts = new TimeSpan[2];
            using (
                var reader = Statistics.tshock.QueryReader("SELECT Registered FROM Users WHERE Username = @0",
                    username))
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

            using (var reader = QueryReader("SELECT Time from Statistics WHERE Username = @0", username))
            {
                if (reader.Read())
                    ts[1] = new TimeSpan(0, 0, 0, reader.Get<int>("Time"));
                else
                    return null;
            }

            return ts;
        }

        internal TimeSpan GetLastSeen(string username)
        {
            using (var reader = Statistics.tshock.QueryReader("SELECT LastAccessed FROM Users WHERE Username = @0",
                username))
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
                    ret.Add(reader.Get<string>("Username"), reader.Get<int>("Score"));
            }
            return ret;
        }


        private Database(IDbConnection db)
        {
            _db = db;

            _creator = new SqlTableCreator(_db,
                _db.GetSqlType() == SqlType.Sqlite
                    ? (IQueryBuilder) new SqliteQueryCreator()
                    : new MysqlQueryCreator());
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
                    _mysql = true;
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