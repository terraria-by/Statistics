using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

using TShockAPI;
using Terraria;
namespace Statistics
{
    static class KillingSpree
    {
        public static Config config = Statistics.config;

        private static string[] killOptions = { "Mob", "Boss", "Player", "All" };
        private static readonly Timer SpreeTimer = new Timer(30 * 1000);
        private static bool inBlitzEvent = false; 
        private static bool beatClock = false;
        internal static readonly Dictionary<int, int> KillingSpreeMatch = new Dictionary<int, int>();

        static KillingSpree()
        {
            SpreeTimer.Elapsed += notifySpreeEnd;
            SpreeTimer.AutoReset = false;
        }
        public static void ClearBlitzEvent(int playerId)
        {
            KillingSpreeMatch.Remove(playerId);
        }
        public static void SendKillingNotice(string playerName, int playerId, int MobKills, int BossKills, int PlayerKills)
        {
            config = Statistics.config;
            int killCount = 0;
            int slot = 0; 
            int[] killList;
            string killType = "";

            killList = Statistics.database.GetKills(playerId);
            if (inBlitzEvent)
            {
                slot = 0;
                for (int i = 0; i < killOptions.Length; i++)
                {
                    if (killOptions[i].Equals(config.BlitzEventType))
                    {
                        killType = config.BlitzEventType;
                        switch (killType)
                        {
                            case "Mob":
                                killCount = MobKills;
                                break;
                            case "Boss":
                                killCount = BossKills;
                                break;
                            case "Player":
                                killCount = PlayerKills;
                                break;
                            case "All":
                                killCount = MobKills + BossKills + PlayerKills;
                                break;
                        }
                         break;
                    }
                }
                if (KillingSpreeMatch.ContainsKey(playerId))
                    KillingSpreeMatch[playerId] += killCount;
                else
                    KillingSpreeMatch.Add(playerId, killCount);
                if (KillingSpreeMatch[playerId] >= config.BlitzEventGoal)
                {
                    beatClock = true;
                    notifySpreeEnd(null, null);
                }
            }

//            Console.WriteLine(playerId + ":" + killList[0]);
            slot = 0;
            for (int i = 0; i < killOptions.Length; i++)
            {
                if (killOptions[i].Equals(config.KillingSpreeType))
                {
                    killType = config.KillingSpreeType;
                    switch (killType)
                    {
                        case "Mob":
                            slot = 0;
                            break;
                        case "Boss":
                            slot = 1;
                            break;
                        case "Player":
                            slot = 2;
                            break;
                    }
                    if (slot == 0)
                        killCount = killList[0] + killList[1] + killList[2];
                    else
                        killCount = killList[slot];
                    break;
                }
            }
            //            Console.WriteLine(killCount + ":" + killType);
            if (killType.Length == 0)
                return;

            if (killCount < 0)
                return;
            int killLevel = -1;
            for (int i = 0; i < config.KillingSpreeThreshold.Length; i++)
            {
                if (killCount == config.KillingSpreeThreshold[i])
                {
                    killLevel = i;
                    break;
                }
            }

            if (killLevel < 0)
                return;

            string output = playerName + " " + config.KillingSpreeMessage[killLevel];

            if (config.showTimeStamp)
            {
                DateTime date = DateTime.Now;
                TSPlayer.All.SendMessage(string.Format(" {0}", date), Color.Red);
                if (config.tellConsole)
                    TSPlayer.Server.SendMessage(string.Format(" {0}", date), Color.Red);
            }
            TSPlayer.All.SendMessage(output, Convert.ToByte(config.KillingSpreeColor[0]), Convert.ToByte(config.KillingSpreeColor[1]), Convert.ToByte(config.KillingSpreeColor[2]));
            if (config.tellConsole)
                Announcements.ConsoleSendMessage(output);

        }
        public static void StartSpree()
        {
            DateTime date = DateTime.Now;

            if (SpreeTimer.Enabled)
            {
                TSPlayer.All.SendMessage(string.Format(" Blitz Event already started."), Convert.ToByte(config.KillingSpreeColor[0]), Convert.ToByte(config.KillingSpreeColor[1]), Convert.ToByte(config.KillingSpreeColor[2]));
                if (config.tellConsole)
                    Announcements.ConsoleSendMessage(string.Format(" Blitz Event already started."));
                return;
            }
            double startSpree = (config.BlitzEventStart - date).TotalMilliseconds;

            TSPlayer.All.SendMessage(string.Format("A {1}blitz will start at {0}, good luck!", config.BlitzEventStart, FormatTimeSpan(new TimeSpan(0, 0, config.BlitzEventLength))), Convert.ToByte(config.KillingSpreeColor[0]), Convert.ToByte(config.KillingSpreeColor[1]), Convert.ToByte(config.KillingSpreeColor[2]));
            if (config.tellConsole)
                Announcements.ConsoleSendMessage(string.Format(" A {1}blitz will start at {0}, good luck!", config.BlitzEventStart, FormatTimeSpan(new TimeSpan(0, 0, config.BlitzEventLength))));

            SpreeTimer.Stop();
            SpreeTimer.Interval = startSpree;    // in seconds
            SpreeTimer.Start();
            KillingSpreeMatch.Clear();
        }
        private static void notifySpreeEnd(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (inBlitzEvent)
            {
                if (config.tellConsole)
                    Announcements.ConsoleSendMessage(string.Format(" Blitz Event stopped at {0}.", DateTime.Now));
                SpreeTimer.Stop();

                if (KillingSpreeMatch.Count == 0)
                {
                    if (config.tellConsole)
                        Announcements.ConsoleSendMessage(string.Format("No winners for this Blitz Event"));
                    TSPlayer.All.SendMessage(string.Format("No winners for this Blitz Event"), Convert.ToByte(config.BlitzEventColor[0]), Convert.ToByte(config.BlitzEventColor[1]), Convert.ToByte(config.BlitzEventColor[2]));
                }
                else
                {
                    int playerId = KillingSpreeMatch.FirstOrDefault(x => x.Value == KillingSpreeMatch.Values.Max()).Key;
                    var player = TShock.Users.GetUserByID(playerId);

                    if (beatClock)
                    {
                        DateTime date = DateTime.Now;
                        TimeSpan span = date.Subtract(config.BlitzEventStart);
                        string output = "";
 //                       output += span.Days + " days ";
                        output += span.Hours + " hours ";
                        output += span.Minutes + " minutes ";
                        output += span.Seconds + " seconds";

                        TSPlayer.All.SendMessage(string.Format("Blitz winner {0} beat the clock with {1} {2} kills in {3}.", player.Name, KillingSpreeMatch[playerId], config.BlitzEventType, output), Convert.ToByte(config.BlitzEventColor[0]), Convert.ToByte(config.BlitzEventColor[1]), Convert.ToByte(config.BlitzEventColor[2]));
                        if (config.tellConsole)
                            Announcements.ConsoleSendMessage(string.Format("Blitz winner is {0} who has {1} {2} kills", player.Name, KillingSpreeMatch[playerId], config.BlitzEventType));
                     }
                    else
                    {
                        if (config.tellConsole)
                            Announcements.ConsoleSendMessage(string.Format("Blitz winner is {0} who has {1} {2} kills", player.Name, KillingSpreeMatch[playerId], config.BlitzEventType));
                        TSPlayer.All.SendMessage(string.Format("Blitz winner is {0} who has {1} {2} kills", player.Name, KillingSpreeMatch[playerId], config.BlitzEventType), Convert.ToByte(config.BlitzEventColor[0]), Convert.ToByte(config.BlitzEventColor[1]), Convert.ToByte(config.BlitzEventColor[2]));
                    }
                }
                KillingSpreeMatch.Clear();
                inBlitzEvent = false;
            }
            else
            {
                TSPlayer.All.SendMessage(string.Format(" Blitz Event started at {0}.", DateTime.Now), Convert.ToByte(config.BlitzEventColor[0]), Convert.ToByte(config.BlitzEventColor[1]), Convert.ToByte(config.BlitzEventColor[2]));
                if (config.tellConsole)
                    Announcements.ConsoleSendMessage(string.Format(" Blitz Event started at {0}.", DateTime.Now));
                SpreeTimer.Stop();
                SpreeTimer.Interval = config.BlitzEventLength * 1000;    // in seconds
                SpreeTimer.Start();
                inBlitzEvent = true;
            }
        }

        public static string FormatTimeSpan(this TimeSpan obj)
    {
        StringBuilder sb = new StringBuilder();
        if (obj.Hours != 0)
        {
            sb.Append(obj.Hours);
            sb.Append(" "); 
            sb.Append("hrs");
            sb.Append(" ");
        }
        if (obj.Minutes != 0)
        {
            sb.Append(obj.Minutes);
            sb.Append(" "); 
            sb.Append("mins");
            sb.Append(" ");
        }
        if (obj.Seconds != 0)
        {
            sb.Append(obj.Seconds);
            sb.Append(" "); 
            sb.Append("secs");
            sb.Append(" ");
        }
        if (obj.Milliseconds != 0 )
        {
            sb.Append(obj.Milliseconds);
            sb.Append(" "); 
            sb.Append("Milliseconds");
            sb.Append(" ");
        }
        return sb.ToString();
    }
    }
}
