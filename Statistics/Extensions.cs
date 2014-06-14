using System;
using System.Collections.Generic;
using System.Text;

namespace Statistics
{
    public static class Extensions
    {
        public static StoredPlayer AddAndReturn(this List<StoredPlayer> list, StoredPlayer player)
        {
            list.Add(player);
            return player;
        }

        public static void SaveStats(this SPlayer player)
        {
            if (player == null || player.storage == null) return;

            player.storage.totalTime = player.timePlayed;
            player.storage.firstLogin = player.firstLogin;
            player.storage.lastSeen = DateTime.Now.ToString("G");
            player.storage.loginCount = player.loginCount;
            player.storage.knownAccounts = player.knownAccounts;
            player.storage.knownIPs = player.knownIPs;
            player.storage.kills = player.kills;
            player.storage.deaths = player.deaths;
            player.storage.mobkills = player.mobkills;
            player.storage.bosskills = player.bosskills;
            Statistics.database.SaveUser(player.storage);
        }

        public static void SyncStats(this SPlayer player)
        {
            if (player.storage == null) return;

            player.timePlayed = player.storage.totalTime;
            player.firstLogin = player.storage.firstLogin;
            player.lastSeen = DateTime.UtcNow.ToString("G");
            player.loginCount = player.storage.loginCount + 1;
            player.knownAccounts = player.storage.knownAccounts;
            player.knownIPs = player.storage.knownIPs;

            player.kills = player.storage.kills;
            player.deaths = player.storage.deaths;
            player.mobkills = player.storage.mobkills;
            player.bosskills = player.storage.bosskills;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string TimePlayed(this SPlayer player)
        {
            var time = player.timePlayed * 1d;
            var weeks = Math.Floor(time/604800);
            var days = Math.Floor(((time/604800) - weeks)*7);

            var ts = new TimeSpan(0, 0, 0, (int) time);

            var sb = new StringBuilder();

            if (weeks > 0)
                sb.Append(string.Format("{0} week{1}{2}", weeks, Tools.Suffix((int)weeks),
                    days > 0 || ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
            if (days > 0)
                sb.Append(string.Format("{0} day{1}{2}", days, Tools.Suffix((int)days),
                    ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
            if (ts.Hours > 0)
                sb.Append(string.Format("{0} hour{1}{2}", ts.Hours, Tools.Suffix(ts.Hours),
                    ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));

            if (ts.Minutes > 0)
                sb.Append(string.Format("{0} minute{1}{2}", ts.Minutes, Tools.Suffix(ts.Minutes),
                    ts.Seconds > 0 ? ", " : ""));

            if (ts.Seconds > 0)
                sb.Append(string.Format("{0} second{1}", ts.Seconds, Tools.Suffix(ts.Seconds)));

            return sb.ToString();
        }

        public static string TimePlayed(this StoredPlayer player)
        {
            var time = player.totalTime * 1d;
            var weeks = Math.Floor(time / 604800);
            var days = Math.Floor(((time / 604800) - weeks) * 7);

            var ts = new TimeSpan(0, 0, 0, (int)time);

            var sb = new StringBuilder();

            if (weeks > 0)
                sb.Append(string.Format("{0} week{1}{2}", weeks, Tools.Suffix((int)weeks),
                    days > 0 || ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
            if (days > 0)
                sb.Append(string.Format("{0} day{1}{2}", days, Tools.Suffix((int)days),
                    ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
            if (ts.Hours > 0)
                sb.Append(string.Format("{0} hour{1}{2}", ts.Hours, Tools.Suffix(ts.Hours),
                    ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));

            if (ts.Minutes > 0)
                sb.Append(string.Format("{0} minute{1}{2}", ts.Minutes, Tools.Suffix(ts.Minutes),
                    ts.Seconds > 0 ? ", " : ""));

            if (ts.Seconds > 0)
                sb.Append(string.Format("{0} second{1}", ts.Seconds, Tools.Suffix(ts.Seconds)));

            return sb.ToString();
        }

        public static string TimeSpanPlayed(this TimeSpan ts)
        {
            var sb = new StringBuilder();
            if (ts.Hours > 0)
                sb.Append(string.Format("{0} hour{1}{2}", ts.Hours, Tools.Suffix(ts.Hours),
                    ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));

            if (ts.Minutes > 0)
                sb.Append(string.Format("{0} minute{1}{2}", ts.Minutes, Tools.Suffix(ts.Minutes),
                    ts.Seconds > 0 ? ", " : ""));

            if (ts.Seconds > 0)
                sb.Append(string.Format("{0} second{1}", ts.Seconds, Tools.Suffix(ts.Seconds)));

            return sb.ToString();
        }
    }
}
