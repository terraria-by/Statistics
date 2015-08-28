using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace Statistics
{
    public static class Commands
    {
        public static TShockAPI.TSPlayer player;
        public static Config config = new Config();

        public static void Core(CommandArgs args)
        {
            player = args.Player;
            string[] statList = { "MobKills", "BossKills", "PlayerKills" };

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. /stats [flag] <player name>");
                args.Player.SendErrorMessage(
                    "Valid flags: -l : list, -k : kills, -t : time, -s : seen, -hs : highscores, -d : damage");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "-kl":
                    KillingSpree.SendKillingNotice(Int32.Parse(args.Parameters[1]));
                    break;

                case "-o":
                case "-options":

                    Announcements.ConsoleSendMessage(string.Format(" isActive {0}", config.isActive));
                    Announcements.ConsoleSendMessage(string.Format(" byTime {0}", config.byTime));
                    Announcements.ConsoleSendMessage(string.Format(" showTimeStamp {0}", config.showTimeStamp));
                    Announcements.ConsoleSendMessage(string.Format(" tellConsole {0}", config.tellConsole));
                    //                    TSPlayer.Server.SendMessage(string.Format(" showTimeStamp {0}", config.showTimeStamp), Color.Green);

                    Announcements.ConsoleSendMessage(string.Format(" showKills {0}", config.showKills));
                    Announcements.ConsoleSendMessage(string.Format(" KillstimeInterval {0}", config.KillstimeInterval));
                    Announcements.ConsoleSendMessage(string.Format(" KillstimeOffset {0}", config.KillstimeOffset));
                    Announcements.ConsoleSendMessage(string.Format(" KillsColor {0}", string.Join(",", config.KillsColor)));

                    Announcements.ConsoleSendMessage(string.Format(" DamagetimeInterval {0}", config.DamagetimeInterval));
                    Announcements.ConsoleSendMessage(string.Format(" showDamageKills {0}", config.showDamage));
                   Announcements.ConsoleSendMessage(string.Format(" DamagetimeOffset {0}", config.DamagetimeOffset));
                   Announcements.ConsoleSendMessage(string.Format(" DamageColor {0}", string.Join(",", config.DamageColor)));

                    Announcements.ConsoleSendMessage(string.Format(" DeathstimeInterval {0}", config.DeathstimeInterval));
                    Announcements.ConsoleSendMessage(string.Format(" showDeathsKills {0}", config.showDeaths));
                    Announcements.ConsoleSendMessage(string.Format(" DeathstimeOffset {0}", config.DeathstimeOffset));
                    Announcements.ConsoleSendMessage(string.Format(" DeathsColor {0}", string.Join(",", config.DeathsColor)));

                    break;

                case "-r":
                case "-reload":
                    Announcements.loadConfig();
                    Announcements.setupAnnouncements();
                    Announcements.closeAnnouncements();
                    Announcements.ConsoleSendMessage(string.Format(" Announcements config reloaded"));

                    break;

                case "-n":
                case "-notify":
                    if (args.Parameters.Count < 2)
                        Announcements.SendNoticeAll("");
                    else
                    {
                        if (statList.Contains(args.Parameters[1]))
                            Announcements.SendNoticeAll(args.Parameters[1]);
                        else
                            Announcements.ConsoleSendMessage(string.Format(" Invalid stats option"));
                    }
                    break;

                case "-l":
                case "-list":
                    {
                        Dictionary<string, int[]> statsList = new Dictionary<string, int[]>();
                        int page = 0, totalPages = 0;

                        totalPages = Statistics.database.CountAllPlayers();
                        if (totalPages == 0)
                        {
                            args.Player.SendErrorMessage("No statistical data available");
                            return;
                        }

                        var lineColor = Color.Yellow;
                        totalPages = (totalPages / 5) + 1;
                        statsList.Clear();
                        if (args.Parameters.Count < 2)
                        {
                            statsList = Statistics.database.GetAllPlayers(page, 0);
                            page = 1;
                        }
                        else
                        {
                            bool isNum = Int32.TryParse(args.Parameters[1], out page);
                            if (isNum)
                            {
                                statsList = Statistics.database.GetAllPlayers(page, 0);

                                if (!args.Player.RealPlayer)
                                    args.Player.SendInfoMessage("Statistics List - Page {0} of {1}", page, totalPages);
                                else
                                    args.Player.SendMessage(string.Format("Statistics List - Page {0} of {1}", page, totalPages), lineColor);
                            }
                            else
                            {
                                var user = TShock.Users.GetUsers().Find(u => u.Name.StartsWith(args.Parameters[1]));

                                if (user == null)
                                    args.Player.SendErrorMessage("No users found matching the name '{0}'", args.Parameters[1]);
                                else
                                    statsList = Statistics.database.GetAllPlayers(1, user.ID);
                            }
                        }
                        foreach (KeyValuePair<string, int[]> stat in statsList)
                        {
                            string statsName = stat.Key;
                            int[] stats = stat.Value;
                            TimeSpan ts = new TimeSpan(0, 0, 0, stats[2]);
                            var total = ts.Add(new TimeSpan(0, 0, 0, Statistics.TimeCache[stats[0]]));
                            if (!args.Player.RealPlayer)
                                args.Player.SendInfoMessage(" {0}, player died {1} kills: player {4} mob {5} boss {6} - damage: mob {7} boss {8} player {9} received {10} on for {2} logins {3}",
        statsName, stats[1], total.SToString(), stats[3], stats[4], stats[5], stats[6], stats[7], stats[8], stats[9], stats[10]);
                            else
                                args.Player.SendMessage(string.Format(" {0}, player died {1} kills: player {4} mob {5} boss {6} - damage: mob {7} boss {8} player {9} received {10} on for {2} logins {3}",
        statsName, stats[1], total.SToString(), stats[3], stats[4], stats[5], stats[6], stats[7], stats[8], stats[9], stats[10]), lineColor);
                        }
                    }
                    break;
                /*
            case "-list":
                {
                    Dictionary<string, int[]> statsList = new Dictionary<string, int[]>();
                    var page = 1;
                    double Num;

                    if (args.Parameters.Count < 2)
                    {
                        statsList = Statistics.database.GetAllPlayers(page, 0);
                        page = 1;
                    }
                    else
                    {
                        bool isNum = double.TryParse(args.Parameters[1], out Num);
                        if (isNum)
                        {
                            if (HsPagination.TryParsePageNumber(args.Parameters, 1, args.Player, out page))
                                statsList = Statistics.database.GetAllPlayers(page, 0);
                        }
                        else
                        {
                            var user = TShock.Users.GetUsers().Find(u => u.Name.StartsWith(args.Parameters[1]));

                            if (user == null)
                                args.Player.SendErrorMessage("No users found matching the name '{0}'", args.Parameters[1]);
                            else
                                statsList = Statistics.database.GetAllPlayers(page, user.ID);
                        }
                    }

                    StatsPagination.SendPage(args.Player, page, statsList, new StatsPagination.FormatSettings
                       {
                           FooterFormat = "use /stats -ls {0} for more stats",
                           FooterTextColor = Color.Lime,
                           HeaderFormat = "Statistics List - Page {0} of {1}",
                           HeaderTextColor = Color.Lime,
                           IncludeFooter = true,
                           IncludeHeader = true,
                           MaxLinesPerPage = 5,
                           NothingToDisplayString = "No statistical data available"
                       });
                }
                break;
                */
                case "-k":
                case "-kills":
                    {
                        if (args.Parameters.Count < 2)
                        {
                            var kills = Statistics.database.GetKills(args.Player.User.ID);
                            if (kills == null)
                                args.Player.SendErrorMessage("Unable to discover your killcount. Sorry.");
                            else
                                args.Player.SendSuccessMessage(
                                    "You have killed {0} player{4}, {1} mob{5}, {2} boss{6} and died {3} time{7}",
                                    kills[0], kills[1], kills[2], kills[3],
                                    kills[0].Suffix(), kills[1].Suffix(), kills[2].Suffix(true), kills[3].Suffix());
                        }
                        else
                        {
                            var name = args.Parameters[1];
                            var user = TShock.Users.GetUsers().Find(u => u.Name.StartsWith(name));
                            if (user == null)
                                args.Player.SendErrorMessage("No users found matching the name '{0}'", name);
                            else
                            {
                                var kills = Statistics.database.GetKills(user.ID);
                                if (kills == null)
                                    args.Player.SendErrorMessage("Unable to discover the killcount of {0}. Sorry.",
                                        user.Name);
                                else
                                {
                                    args.Player.SendSuccessMessage(
                                        "{0} has killed {1} player{5}, {2} mob{6}, {3} boss{7} and died {4} time{8}",
                                        user.Name, kills[0], kills[1], kills[2], kills[3],
                                        kills[0].Suffix(), kills[1].Suffix(), kills[2].Suffix(true), kills[3].Suffix());
                                }
                            }
                        }
                        break;
                    }
                case "-t":
                case "-time":
                    {
                        var logins = 1;
                        if (args.Parameters.Count < 2)
                        {
                            var times = Statistics.database.GetTimes(args.Player.User.ID, ref logins);
                            if (times == null)
                                args.Player.SendErrorMessage("Unable to discover your times. Sorry.");
                            else
                            {
                                var total = times[1].Add(new TimeSpan(0, 0, 0, Statistics.TimeCache[args.Player.Index]));
                                args.Player.SendSuccessMessage("You have played for {0}.", total.SToString());
                                args.Player.SendSuccessMessage("You have been registered for {0}.", times[0].SToString());
                                args.Player.SendSuccessMessage("You have logged in {0} times.", logins);
                            }
                        }
                        else
                        {
                            var name = args.Parameters[1];
                            var users = GetUsers(name);
                            if (users.Count > 1)
                            {
                                args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                    name, string.Join(", ", users.Select(u => u.Name)));
                                break;
                            }
                            if (users.Count == 0)
                            {
                                args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                                break;
                            }

                            var user = users[0];

                            var times = Statistics.database.GetTimes(user.ID, ref logins);
                            if (times == null)
                                args.Player.SendErrorMessage("Unable to discover the times of {0}. Sorry.",
                                    user.Name);
                            else
                            {
                                args.Player.SendSuccessMessage("{0} has played for {1}.", user.Name,
                                    times[1].SToString());
                                args.Player.SendSuccessMessage("{0} has been registered for {1}.", user.Name,
                                    times[0].SToString());
                                args.Player.SendSuccessMessage("{0} has logged in {1} times.", user.Name, logins);
                            }
                        }
                        break;
                    }
                case "-s":
                case "-seen":
                    {
                        if (args.Parameters.Count < 2)
                            args.Player.SendErrorMessage("Invalid syntax. /stats -s [player name]");
                        else
                        {
                            var name = args.Parameters[1];
                            var users = GetUsers(name);
                            if (users.Count > 1)
                            {
                                args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                    name, string.Join(", ", users.Select(u => u.Name)));
                                break;
                            }
                            if (users.Count == 0)
                            {
                                args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                                break;
                            }

                            var user = users[0];
                            var seen = Statistics.database.GetLastSeen(user.ID);
                            if (seen == TimeSpan.MaxValue)
                                args.Player.SendErrorMessage("Unable to find {0}'s last login time.",
                                    user.Name);
                            else
                                args.Player.SendSuccessMessage("{0} last logged in {1} ago.", user.Name, seen.SToString());
                        }

                        break;
                    }
                case "-hs":
                case "-highscores":
                    {
                        var highscores = new Dictionary<string, int>();
                        var page = 1;
                        if (args.Parameters.Count < 2)
                            highscores = Statistics.database.GetHighScores(1);
                        else if (HsPagination.TryParsePageNumber(args.Parameters, 1, args.Player, out page))
                            highscores = Statistics.database.GetHighScores(page);

                        HsPagination.SendPage(args.Player, page, highscores, new HsPagination.FormatSettings
                        {
                            FooterFormat = "use /stats -hs {0} for more high scores",
                            FooterTextColor = Color.Lime,
                            HeaderFormat = "High Scores- Page {0} of {1}",
                            HeaderTextColor = Color.Lime,
                            IncludeFooter = true,
                            IncludeHeader = true,
                            MaxLinesPerPage = 5,
                            NothingToDisplayString = "No highscores available"
                        });

                        break;
                    }
                case "-d":
                case "-damage":
                    {
                        if (args.Parameters.Count < 2)
                        {
                            var damages = Statistics.database.GetDamage(args.Player.User.ID);
                            if (damages == null)
                            {
                                args.Player.SendErrorMessage("Unable to discover your damage statistics. Sorry.");
                                return;
                            }
                            args.Player.SendSuccessMessage("You have dealt {0} damage to mobs, {1} damage to bosses "
                                                           + "and {2} damage to players.", damages[0], damages[1], damages[2]);
                            args.Player.SendSuccessMessage("You have been dealt {0} damage.", damages[3]);
                        }
                        else
                        {
                            var name = args.Parameters[1];
                            var users = GetUsers(name);
                            if (users.Count > 1)
                            {
                                args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                    name, string.Join(", ", users.Select(u => u.Name)));
                                break;
                            }
                            if (users.Count == 0)
                            {
                                args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                                break;
                            }

                            var user = users[0];
                            var damages = Statistics.database.GetDamage(user.ID);
                            if (damages == null)
                            {
                                args.Player.SendErrorMessage("Unable to discover your damage statistics. Sorry.");
                                return;
                            }
                            args.Player.SendSuccessMessage("{0} has dealt {1} damage to mobs, {2} damage to bosses "
                                + "and {3} damage to players.", user.Name, damages[0], damages[1], damages[2]);
                            args.Player.SendSuccessMessage("{0} has been dealt {1} damage.", user.Name, damages[3]);
                        }
                        break;
                    }
                case "-ix":
                case "-infox":
                    {
                        break;
                    }
            }
        }

        private static List<User> GetUsers(string username)
        {
            var users = TShock.Users.GetUsers();
            var ret = new List<User>();
            foreach (var user in users)
            {
                if (user.Name.Equals(username))
                    return new List<User> { user };
                if (user.Name.StartsWith(username))
                    ret.Add(user);
            }
            return ret;
        }
    }
}