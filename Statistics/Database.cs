using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace Statistics
{
   public class Database
    {
       private static IDbConnection _db;

       public Database(IDbConnection db)
       {
           _db = db;

           var sqlCreator = new SqlTableCreator(_db,
               _db.GetSqlType() == SqlType.Sqlite
                   ? (IQueryBuilder) new SqliteQueryCreator()
                   : new MysqlQueryCreator());

           var table = new SqlTable("Stats",
               new SqlColumn("ID", MySqlDbType.Int32) {Primary = true, AutoIncrement = true},
               new SqlColumn("Name", MySqlDbType.VarChar, 50) {Unique = true},
               new SqlColumn("Time", MySqlDbType.Int32),
               new SqlColumn("FirstLogin", MySqlDbType.Text),
               new SqlColumn("LastSeen", MySqlDbType.Text),
               new SqlColumn("Kills", MySqlDbType.Int32),
               new SqlColumn("Deaths", MySqlDbType.Int32),
               new SqlColumn("MobKills", MySqlDbType.Int32),
               new SqlColumn("BossKills", MySqlDbType.Int32),
               new SqlColumn("KnownAccounts", MySqlDbType.Text),
               new SqlColumn("KnownIPs", MySqlDbType.Text),
               new SqlColumn("LoginCount", MySqlDbType.Int32)
               );
           sqlCreator.EnsureExists(table);
       }

       public static void SyncUsers()
       {
           var count = 0;
           using (var reader = _db.QueryReader("SELECT * FROM Stats"))
           {
               while (reader.Read())
               {
                   var name = reader.Get<string>("Name");
                   var totalTime = reader.Get<int>("Time");
                   var firstLogin = reader.Get<string>("FirstLogin");
                   var lastSeen = reader.Get<string>("LastSeen");

                   var knownAccounts = reader.Get<string>("KnownAccounts");
                   var knownIPs = reader.Get<string>("KnownIPs");
                   var loginCount = reader.Get<int>("LoginCount");

                   var kills = reader.Get<int>("Kills");
                   var deaths = reader.Get<int>("Deaths");
                   var mobKills = reader.Get<int>("MobKills");
                   var bossKills = reader.Get<int>("BossKills");

                   Statistics.StoredPlayers.Add(new StoredPlayer(name, firstLogin, lastSeen, totalTime, loginCount, knownAccounts, knownIPs,
                       kills, deaths, mobKills, bossKills));
                   count++;
               }
           }

           Console.WriteLine("Synced {0} player{1}", count, count.Suffix());
       }

       public static void SyncHighScores()
       {
           foreach (var player in Statistics.StoredPlayers)
           {
               Statistics.HighScores.highScores.AddAndReturn(new HighScore(player.name, player.kills,
                   player.mobkills,
                   player.deaths, player.bosskills, player.totalTime));
           }
       }

       public void SaveState()
       {
           foreach (var player in Statistics.Players.Where(player => player.TsPlayer.IsLoggedIn))
               player.SaveStats();

           Log.ConsoleInfo("[STATISTICS] Database save complete");
       }

       public void AddUser(StoredPlayer storage)
       {
           _db.Query("INSERT INTO Stats " +
                     "(Name, Time, FirstLogin, LastSeen, KnownAccounts, KnownIPs, LoginCount, Kills, Deaths, MobKills, BossKills) " +
                     "VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10)",
               storage.name, storage.totalTime, storage.firstLogin, storage.lastSeen, storage.knownAccounts,
               storage.knownIPs,
               storage.loginCount, storage.kills, storage.deaths, storage.mobkills, storage.bosskills);
       }

       public void SaveUser(StoredPlayer player)
       {
           _db.Query("UPDATE Stats SET Time = @0, LastSeen = @1, Kills = @2, Deaths = @3, MobKills = @4, " +
                     "BossKills = @5, LoginCount = @6 WHERE Name = @7",
               player.totalTime, DateTime.Now.ToString("G"), player.kills, player.deaths, player.mobkills,
               player.bosskills, player.loginCount, player.name);
       }
    }
}
