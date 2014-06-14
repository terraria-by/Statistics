using System.Net;
using System.Linq;
using System.Collections.Generic;
using TShockAPI;

namespace Statistics
{
    public static class SCommands
    {

        #region UI Extended

        public static void CmdUiEx(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /uix [playerName]");
                return;
            }

            if (args.Parameters[0] == "self")
            {
                SPlayer player = Statistics.Tools.GetPlayer(args.Player.Index);
                if (player == null)
                    return;

                if (player.TsPlayer.IsLoggedIn)
                {
                    int pageNumber;
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
                        return;

                    Tools.SendUix(args.Player, player, pageNumber);
                }
                else
                    args.Player.SendErrorMessage("You must be logged in to use this on yourself");
            }
            else
            {
                string name;
                var needNumber = false;
                if (args.Parameters.Count > 1)
                {
                    var newArgs = new List<string>(args.Parameters);
                    newArgs.RemoveAt(newArgs.Count - 1);
                    name = string.Join(" ", newArgs);
                    needNumber = true;
                }
                else
                    name = string.Join(" ", args.Parameters);

                int pageNumber;
                if (!PaginationTools.TryParsePageNumber(args.Parameters,
                    needNumber ? args.Parameters.Count - 1 : args.Parameters.Count + 1, args.Player, out pageNumber))
                    return;

                IPAddress ip;
                if (IPAddress.TryParse(name, out ip))
                {
                    var players = Statistics.Tools.GetPlayersByIp(ip.ToString());

                    if (players.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
                        return;
                    }

                    if (players.Count == 0)
                    {
                        args.Player.SendErrorMessage("Unable to match IP address");

                        var splayers = Statistics.Tools.GetStoredPlayersByIp(ip.ToString());

                        if (splayers.Count > 1)
                        {
                            TShock.Utils.SendMultipleMatchError(args.Player, splayers.Select(p => p.name));
                            return;
                        }
                        if (splayers.Count == 0)
                        {
                            args.Player.SendErrorMessage("No matches found for query '{0}'", name);
                            return;
                        }

                        var splayer = splayers[0];
                        Tools.SendUix(args.Player, splayer, pageNumber);

                        return;
                    }

                    var player = players[0];

                    Tools.SendUix(args.Player, player, pageNumber);
                    return;
                }

                var players2 = Statistics.Tools.GetPlayers(name);

                if (players2.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, players2.Select(p => p.Name));
                    return;
                }
                if (players2.Count == 0)
                {

                    args.Player.SendErrorMessage(
                        "Invalid player. Try /check name {0} to make sure you're using the right username",
                        name);
                    return;
                }

                var player2 = players2[0];

                Tools.SendUix(args.Player, player2, pageNumber);
            }
        }

        #endregion

        #region UI Character

        public static void CmdUic(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /uix [playerName]");
                return;
            }

            if (args.Parameters[0] == "self")
            {
                SPlayer player = Statistics.Tools.GetPlayer(args.Player.Index);
                if (player == null)
                    return;

                if (player.TsPlayer.IsLoggedIn)
                {
                    int pageNumber;
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
                        return;

                    Tools.SendUic(args.Player, player, pageNumber);
                }
                else
                    args.Player.SendErrorMessage("You must be logged in to use this on yourself");
            }
            else
            {
                string name;
                var needNumber = false;
                if (args.Parameters.Count > 1)
                {
                    var newArgs = new List<string>(args.Parameters);
                    newArgs.RemoveAt(newArgs.Count - 1);
                    name = string.Join(" ", newArgs);
                    needNumber = true;
                }
                else
                    name = string.Join(" ", args.Parameters);

                int pageNumber;
                if (!PaginationTools.TryParsePageNumber(args.Parameters,
                    needNumber ? args.Parameters.Count - 1 : args.Parameters.Count + 1, args.Player, out pageNumber))
                    return;

                IPAddress ip;
                if (IPAddress.TryParse(name, out ip))
                {
                    var players = Statistics.Tools.GetPlayersByIp(ip.ToString());

                    if (players.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
                        return;
                    }

                    if (players.Count == 0)
                    {
                        var splayers = Statistics.Tools.GetStoredPlayersByIp(ip.ToString());

                        if (splayers.Count > 1)
                        {
                            TShock.Utils.SendMultipleMatchError(args.Player, splayers.Select(p => p.name));
                            return;
                        }
                        if (splayers.Count == 0)
                        {
                            args.Player.SendErrorMessage("No matches found for query '{0}'", name);
                            return;
                        }

                        var splayer = splayers[0];
                        Tools.SendUic(args.Player, splayer, pageNumber);

                        return;
                    }

                    var player = players[0];

                    Tools.SendUix(args.Player, player, pageNumber);
                    return;
                }

                var players2 = Statistics.Tools.GetPlayers(name);

                if (players2.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, players2.Select(p => p.Name));
                    return;
                }
                if (players2.Count == 0)
                {

                    args.Player.SendErrorMessage(
                        "Invalid player. Try /check name {0} to make sure you're using the right username",
                        name);
                    return;
                }

                var player2 = players2[0];

                Tools.SendUic(args.Player, player2, pageNumber);
            }
        }

        #endregion


        #region Time Check

        public static void CmdCheckTime(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /check time [playerName]");
                return;
            }

            if (args.Parameters[1] == "self")
            {
                var self = Statistics.Tools.GetPlayer(args.Player.Index);

                if (self == null)
                {
                    args.Player.SendErrorMessage("Something broke. Please try logging in again");
                    return;
                }

                if (!self.TsPlayer.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("You must be logged in to perform this action");
                    return;
                }

                if (args.Player == TSPlayer.Server)
                {
                    args.Player.SendErrorMessage("The console has no stats to check");
                    return;
                }

                args.Player.SendSuccessMessage("You have been playing for " + self.TimePlayed());
            }
            else
            {
                args.Parameters.RemoveAt(0);
                var name = string.Join(" ", args.Parameters);

                var players = Statistics.Tools.GetPlayers(name);

                if (players.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
                    return;
                }

                if (players.Count == 0)
                {
                    var splayers = Statistics.Tools.GetStoredPlayers(name);
                    if (splayers.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, splayers.Select(p => p.name));
                        return;
                    }

                    if (splayers.Count == 0)
                    {
                        args.Player.SendErrorMessage("No matches found for query '{0}'", name);
                        return;
                    }

                    var ply = splayers[0];
                    args.Player.SendSuccessMessage("{0} has been playing for {1}", ply.name, ply.TimePlayed());
                    return;
                }

                var player = players[0];
                if (!player.TsPlayer.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("{0} must be logged in", player.Name);
                    return;
                }
                args.Player.SendSuccessMessage("{0} has been played for {1}", player.TimePlayed(), player.Name);
            }
        }

        #endregion

        #region Name Check

        public static void CmdCheckName(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /check name [playerName]");
                return;
            }

            args.Parameters.RemoveAt(0);
            var name = string.Join(" ", args.Parameters);

            var players = TShock.Utils.FindPlayer(name);

            if (players.Count > 1)
                TShock.Utils.SendMultipleMatchError(args.Player, players.Select(ply => ply.Name));
            else if (players.Count == 1)
            {
                var player = players[0];
                if (player.IsLoggedIn)
                    args.Player.SendInfoMessage("User name of {0} is {1}", player.Name, player.UserAccountName);
                else
                    args.Player.SendErrorMessage("{0} is not logged in", player.Name);
            }
            else
                args.Player.SendErrorMessage("No matches found for query '{0}'", name);
        }

        #endregion

        #region Kill Check

        public static void CmdCheckKills(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /check kills [playerName]");
                return;
            }

            if (args.Parameters[1] == "self")
            {
                var self = Statistics.Tools.GetPlayer(args.Player.Index);
                if (self == null)
                {
                    args.Player.SendErrorMessage("Something broke. Please try logging in again");
                    return;
                }

                if (!self.TsPlayer.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("You must be logged in to use this on yourself");
                    return;
                }

                if (args.Player == TSPlayer.Server)
                {
                    args.Player.SendErrorMessage("The console has no stats to check");
                    return;
                }

                args.Player.SendInfoMessage(
                    "You have killed {0} player{4}, {1} mob{5}, {2} boss{6} and died {3} time{7}",
                    self.kills, self.mobkills, self.bosskills, self.deaths,
                    Tools.Suffix(self.kills), Tools.Suffix(self.mobkills),
                    Tools.Suffix(self.bosskills), Tools.Suffix(self.deaths));
            }
            else
            {
                var name = string.Join(" ", args.Parameters);

                var players = Statistics.Tools.GetPlayers(name);

                if (players.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
                    return;
                }

                if (players.Count == 0)
                {
                    var splayers = Statistics.Tools.GetStoredPlayers(name);

                    if (splayers.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, splayers.Select(p => p.name));
                        return;
                    }
                    if (splayers.Count == 0)
                    {
                        args.Player.SendErrorMessage("No matches found for query '{0}'", name);
                        return;
                    }

                    var splayer = splayers[0];

                    args.Player.SendInfoMessage(
                        "{0} has killed {1} player{5}, {2} mob{6}, {3} boss{7} and died {4} time{8}",
                        splayer.name, splayer.kills, splayer.mobkills, splayer.bosskills,
                        splayer.deaths,
                        Tools.Suffix(splayer.kills), Tools.Suffix(splayer.mobkills),
                        Tools.Suffix(splayer.bosskills), Tools.Suffix(splayer.deaths));
                    return;
                }

                var player = players[0];

                if (!player.TsPlayer.IsLoggedIn)
                {
                    args.Player.SendErrorMessage("{0} is not logged in", player.Name);
                    return;
                }

                args.Player.SendInfoMessage(
                    "{0} has killed {1} player{5}, {2} mob{6}, {3} boss{7} and died {4} time{8}",
                    player.TsPlayer.UserAccountName, player.kills, player.mobkills, player.bosskills,
                    player.deaths,
                    Tools.Suffix(player.kills), Tools.Suffix(player.mobkills),
                    Tools.Suffix(player.bosskills), Tools.Suffix(player.deaths));
            }
        }

        #endregion

        #region AFK Check

        public static void CmdCheckAfk(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /check afk [playerName\\self]");
                return;
            }

            if (args.Parameters[1] == "self")
            {
                var self = Statistics.Tools.GetPlayer(args.Player.Index);

                if (self == null)
                {
                    args.Player.SendErrorMessage("Something broke. Please try logging in again");
                    return;
                }
                if (!self.afk)
                {
                    args.Player.SendInfoMessage("You are not listed as 'away'");
                    return;
                }

                args.Player.SendInfoMessage("You have been away for {0} seconds", self.afkCount);
            }
            else
            {
                args.Parameters.RemoveAt(0);
                var name = string.Join(" ", args.Parameters);

                var players = Statistics.Tools.GetPlayers(name);

                if (players.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
                    return;
                }
                if (players.Count == 0)
                {
                    args.Player.SendErrorMessage("No matches found for query '{0}'", name);
                    return;
                }

                var player = players[0];

                if (player.afk)
                    args.Player.SendInfoMessage("{0} has been away for {1} second{0}",
                        player.Name, player.afkCount, Tools.Suffix(player.afkCount));
                else
                    args.Player.SendInfoMessage("{0} is not away", player.Name);
            }
        }

        #endregion
    }
}
