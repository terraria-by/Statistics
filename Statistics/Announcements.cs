using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using System.Timers;
using Microsoft.Xna.Framework;
using TShockAPI;


namespace Statistics
{
    public static class Announcements
    {
        public static Config config = Statistics.config;

        private static readonly Timer KillstoNotify = new Timer(1000);
        private static readonly Timer DamagetoNotify = new Timer(1000);
        private static readonly Timer DeathstoNotify = new Timer(1000);

        static Announcements()
        {
            KillstoNotify.Elapsed += notifyKillsOnElapsed;
            KillstoNotify.AutoReset = true;
            DamagetoNotify.Elapsed += notifyDamageOnElapsed;
            DamagetoNotify.AutoReset = true;
            DeathstoNotify.Elapsed += notifyDeathsOnElapsed;
            DeathstoNotify.AutoReset = true;
        }
        public static void SendNoticeAll(string statType)
        {
            if (statType.Length == 0)
            {
                if (config.showTimeStamp)
                {
                    DateTime date = DateTime.Now;
                    TSPlayer.All.SendMessage(string.Format(" {0}", date), Color.Red);
                    if (config.tellConsole)
                        TSPlayer.Server.SendMessage(string.Format(" {0}", date), Color.Red);
                }
                SendNotice("MobKills", " 5 Top Mob kills: ", config.KillsColor);
                SendNotice("BossKills", " 5 Top Boss kills: ", config.KillsColor);
                SendNotice("PlayerKills", " 5 Top Player kills: ", config.KillsColor);

                SendNotice("MobDamageGiven", " 5 Top Mob Damage Given: ", config.DamageColor);
                SendNotice("BossDamageGiven", " 5 Top Boss Damage Given: ", config.DamageColor);
                SendNotice("PlayerDamageGiven", " 5 Top Player Damage Given: ", config.DamageColor);

                SendNotice("Deaths", " 5 Top Deaths: ", config.DeathsColor);
                SendNotice("DamageReceived", " 5 Top Damage Received: ", config.DeathsColor);
            }
            else
                SendNotice(statType, " 5 Top " + statType + ": ", config.KillsColor);
        }

        public static void SendNotice(string statType, string heading, int[] highlight)
        {
            List<KeyValuePair<string, int>> statsList = new List<KeyValuePair<string, int>>();
            string output = heading;

            statsList.Clear();
            statsList = Statistics.database.GetHighPlayers(statType);

            foreach (KeyValuePair<string, int> stat in statsList)
            {
                string statsName = stat.Key;
                int stats = stat.Value;

                output = output + string.Format(" {0}:{1}", statsName, stats);
            }
            TSPlayer.All.SendMessage(output, Convert.ToByte(highlight[0]), Convert.ToByte(highlight[1]), Convert.ToByte(highlight[2]));
            if (config.tellConsole)
                ConsoleSendMessage(output);
        }

        public static void ConsoleSendMessage(string msg)
        {
            Console.ForegroundColor = config.consoleColor;
            Console.WriteLine(msg);
            Console.ResetColor();

        }

        public static void stopAnnouncements()
        {
            KillstoNotify.Stop();
            DamagetoNotify.Stop();
            DeathstoNotify.Stop();
        }

        public static void setupAnnouncements()
        {
            config = Statistics.config;
            if (config.isActive)
            {
                if (config.showKills)
                {
                    KillstoNotify.Stop();
                    KillstoNotify.Interval = (config.KillstimeOffset) * 60 * 1000;
                    KillstoNotify.Start();

                    if (config.tellConsole)
                    {
                        DateTime date = DateTime.Now;
                        DateTime startingWhen;
                        TimeSpan time;
                        if (config.DeathstimeOffset == 0)
                            time = new TimeSpan(0, 0, 0, 30);
                        else
                            time = new TimeSpan(0, 0, config.KillstimeOffset, 0);
                        startingWhen = date.Add(time);
                        ConsoleSendMessage(string.Format(" Kills stats will start at {0} every {1} minutes.", startingWhen, config.KillstimeInterval));
                    }
                }

                if (config.showDamage)
                {
                    DamagetoNotify.Stop();
                    DamagetoNotify.Interval = config.DamagetimeInterval * 60 * 1000;
                    DamagetoNotify.Start();

                    if (config.tellConsole)
                    {
                        DateTime date = DateTime.Now;
                        DateTime startingWhen;
                        TimeSpan time;
                        if (config.DeathstimeOffset == 0)
                            time = new TimeSpan(0, 0, 0, 30);
                        else
                            time = new TimeSpan(0, 0, config.DamagetimeOffset, 0);
                        startingWhen = date.Add(time);
                        ConsoleSendMessage(string.Format(" Damage stats will start at {0} every {1} minutes.", startingWhen, config.DamagetimeInterval));
                    }
                }
                if (config.showDeaths)
                {
                    DeathstoNotify.Stop();
                    DeathstoNotify.Interval = config.DeathstimeInterval * 60 * 1000;
                    DeathstoNotify.Start();

                    if (config.tellConsole)
                    {
                        DateTime date = DateTime.Now;
                        DateTime startingWhen;
                        TimeSpan time;
                        if (config.DeathstimeOffset == 0)
                            time = new TimeSpan(0, 0, 0, 30);
                        else
                            time = new TimeSpan(0, 0, config.DeathstimeOffset, 0);
                        startingWhen = date.Add(time);
                        ConsoleSendMessage(string.Format(" Deaths stats will start at {0} every {1} minutes.", startingWhen, config.DeathstimeInterval));
                    }
                }
            }
        }

        private static void notifyKillsOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (config.showTimeStamp)
            {
                DateTime date = DateTime.Now;
                TSPlayer.All.SendMessage(string.Format(" {0}", date), Color.Red);
                if (config.tellConsole)
                    TSPlayer.Server.SendMessage(string.Format(" {0}", date), Color.Red);
            }
            SendNotice("MobKills", " 5 Top Mob kills: ", config.KillsColor);
            SendNotice("BossKills", " 5 Top Boss kills: ", config.KillsColor);
            SendNotice("PlayerKills", " 5 Top Player kills: ", config.KillsColor);
            KillstoNotify.Stop();
            KillstoNotify.Interval = config.KillstimeInterval * 60 * 1000;
            KillstoNotify.Start();
        }

        private static void notifyDamageOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (config.showTimeStamp)
            {
                DateTime date = DateTime.Now;
                TSPlayer.All.SendMessage(string.Format(" {0}", date), Color.Red);
                if (config.tellConsole)
                    TSPlayer.Server.SendMessage(string.Format(" {0}", date), Color.Red);
            }
            SendNotice("MobDamageGiven", " 5 Top Mob Damage Given: ", config.DamageColor);
            SendNotice("BossDamageGiven", " 5 Top Boss Damage Given: ", config.DamageColor);
            SendNotice("PlayerDamageGiven", " 5 Top Player Damage Given: ", config.DamageColor);
            DamagetoNotify.Stop();
            DamagetoNotify.Interval = config.DamagetimeInterval * 60 * 1000;
            DamagetoNotify.Start();
        }

        private static void notifyDeathsOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (config.showTimeStamp)
            {
                DateTime date = DateTime.Now;
                TSPlayer.All.SendMessage(string.Format(" {0}", date), Color.Red);
                if (config.tellConsole)
                    TSPlayer.Server.SendMessage(string.Format(" {0}", date), Color.Red);
            }
            SendNotice("Deaths", " 5 Top Deaths: ", config.DeathsColor);
            SendNotice("DamageReceived", " 5 Top Damage Received: ", config.DeathsColor);
            DeathstoNotify.Stop();
            DeathstoNotify.Interval = config.DeathstimeInterval * 60 * 1000;
            DeathstoNotify.Start();
        }
    }
}
