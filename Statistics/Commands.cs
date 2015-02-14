using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace Statistics
{
    public static class Commands
    {
        public static void Core(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. /info [flag] <player name>");
                args.Player.SendErrorMessage(
                    "Valid flags: -k : kills, -t : time, -s : seen, -hs : highscores, -i : info, -ix : extended info");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "-k":
                case "-kills":
                {
                    if (args.Parameters.Count < 2)
                    {
                        var kills = Statistics.database.GetKills(args.Player.UserAccountName);
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
                            var kills = Statistics.database.GetKills(user.Name);
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
                    if (args.Parameters.Count < 2)
                    {
                        var times = Statistics.database.GetTimes(args.Player.UserAccountName);
                        if (times == null)
                            args.Player.SendErrorMessage("Unable to discover your times. Sorry.");
                        else
                        {
                            args.Player.SendSuccessMessage("You have played for {0}.", times[1].SToString());
                            args.Player.SendSuccessMessage("You have been registered for {0}.", times[0].SToString());
                        }
                    }
                    else
                    {
                        var name = args.Parameters[1];
                        var users = TShock.Users.GetUsers().Where(u => u.Name.StartsWith(name)).ToList();
                        if (users.Count > 1)
                        {
                            args.Player.SendErrorMessage("More than one user matched your search '{0}'",
                                name);
                            break;
                        }
                        if (users.Count == 0)
                        {
                            args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                            break;
                        }

                        var user = users[0];

                        var times = Statistics.database.GetTimes(user.Name);
                        if (times == null)
                            args.Player.SendErrorMessage("Unable to discover the times of {0}. Sorry.",
                                user.Name);
                        else
                        {
                            args.Player.SendSuccessMessage("{0} has played for {1}.", user.Name,
                                times[1].SToString());
                            args.Player.SendSuccessMessage("{0} has been registered for {1}.", user.Name,
                                times[0].SToString());
                        }
                    }
                    break;
                }
                case "-s":
                case "-seen":
                {
                    if (args.Parameters.Count < 2)
                        args.Player.SendErrorMessage("Invalid syntax. /statistics -s [player name]");
                    else
                    {
                        var name = args.Parameters[1];
                        var users = TShock.Users.GetUsers().Where(u => u.Name.StartsWith(name)).ToList();
                        if (users.Count > 1)
                        {
                            args.Player.SendErrorMessage("More than one user matched your search '{0}'",
                                name);
                            break;
                        }
                        if (users.Count == 0)
                        {
                            args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                            break;
                        }

                        var user = users[0];
                        var seen = Statistics.database.GetLastSeen(user.Name);
                        if (seen == TimeSpan.MaxValue)
                            args.Player.SendErrorMessage("Unable to find {0}'s last online time.",
                                user.Name);
                        else
                            args.Player.SendSuccessMessage("{0} was last online {1} ago.", user.Name, seen.SToString());
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
                        FooterFormat = "use /info -hs {0} for more high scores",
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
                case "-i":
                case "-info":
                {

                    break;
                }
                case "-ix":
                case "-infox":
                {
                    break;
                }
            }
        }
    }
}