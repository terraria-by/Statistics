using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace Statistics
{
    public class Tools
    {
        public readonly SubCommandHandler handler = new SubCommandHandler();

        #region Find player methods
        /// <summary>
        /// Returns a player through index matching
        /// </summary>
        /// <param name="index">int Player index</param>
        /// <returns>SPlayer or null</returns>
        public SPlayer GetPlayer(int index)
        {
            return Statistics.Players.FirstOrDefault(player => player.index == index);
        }

        /// <summary>
        /// Returns an sPlayer through UserAccountName matching
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<SPlayer> GetPlayers(string name)
        {
            var matches = new List<SPlayer>();
            foreach (var player in Statistics.Players.Where(player => player.TsPlayer.IsLoggedIn))
            {
                if (String.Equals(player.TsPlayer.UserAccountName, name, StringComparison.CurrentCultureIgnoreCase))
                    return new List<SPlayer> { player };
                if (player.TsPlayer.UserAccountName.ToLower().Contains(name.ToLower()) && !matches.Contains(player))
                    matches.Add(player);
            }

            return matches;
        }

        public static void SendUic<T>(TSPlayer receiver, T player, int page)
        {
            if (player is SPlayer)
            {
                var ply = player as SPlayer;
                if (ply.TsPlayer.IsLoggedIn)
                {
                    var uixInfo = new List<string>();
                    var time1 = DateTime.Now.Subtract(DateTime.Parse(ply.firstLogin));

                    uixInfo.Add(string.Format("UIC info for {0}", ply.Name));
                    uixInfo.Add(string.Format("First login: {0} ({1} ago)",
                        ply.firstLogin, time1.TimeSpanPlayed()));

                    uixInfo.Add("Last seen: Now");
                    uixInfo.Add(string.Format("Overall play time: {0}", ply.TimePlayed()));
                    uixInfo.Add(string.Format("Logged in {0} times since registering", ply.loginCount));

                    PaginationTools.SendPage(receiver, page, uixInfo, new PaginationTools.Settings
                    {
                        HeaderFormat = "User Information [Page {0} of {1}]",
                        HeaderTextColor = Color.Lime,
                        LineTextColor = Color.White,
                        FooterFormat = string.Format("/uix {0} {1} for more", ply.Name, page + 1),
                        FooterTextColor = Color.Lime
                    });
                }
                else
                    receiver.SendErrorMessage("{0} is not logged in", ply.Name);
            }
            else
            {
                var ply = player as StoredPlayer;

                var uixInfo = new List<string>();
                var time1 = DateTime.Now.Subtract(DateTime.Parse(ply.firstLogin));

                uixInfo.Add(string.Format("UIC info for {0}", ply.name));
                uixInfo.Add(string.Format("First login: {0} ({1} ago)",
                    ply.firstLogin, time1.TimeSpanPlayed()));

                uixInfo.Add(string.Format("Last seen: {0}",
                    DateTime.Now.Subtract(DateTime.Parse(ply.lastSeen)).TimeSpanPlayed()));
                uixInfo.Add(string.Format("Overall play time: {0}", ply.TimePlayed()));
                uixInfo.Add(string.Format("Logged in {0} times since registering", ply.loginCount));

                PaginationTools.SendPage(receiver, page, uixInfo, new PaginationTools.Settings
                {
                    HeaderFormat = "User Information [Page {0} of {1}]",
                    HeaderTextColor = Color.Lime,
                    LineTextColor = Color.White,
                    FooterFormat = string.Format("/uix {0} {1} for more", ply.name, page + 1),
                    FooterTextColor = Color.Lime
                });
            }
        }

        public static void SendUix<T>(TSPlayer receiver, T player, int page)
        {
            if (player is SPlayer)
            {
                var ply = player as SPlayer;
                if (ply.TsPlayer.IsLoggedIn)
                {
                    var uixInfo = new List<string>();
                    var time1 = DateTime.Now.Subtract(DateTime.Parse(ply.firstLogin));

                    uixInfo.Add(string.Format("UIX info for {0}", ply.Name));
                    uixInfo.Add(string.Format("{0} is a member of group {1}", ply.Name,
                        ply.TsPlayer.Group.Name));
                    uixInfo.Add(string.Format("First login: {0} ({1} ago)",
                        ply.firstLogin, time1.TimeSpanPlayed()));

                    uixInfo.Add("Last seen: Now");
                    uixInfo.Add(string.Format("Overall play time: {0}", ply.TimePlayed()));
                    uixInfo.Add(string.Format("Logged in {0} times since registering", ply.loginCount));
                    uixInfo.Add(string.Format("Known accounts: {0}",
                        ply.knownAccounts.Length > 0 ? ply.knownAccounts : "None"));
                    uixInfo.Add(string.Format("Known IPs: {0}",
                        ply.knownIPs.Length > 0 ? ply.knownIPs : "None"));

                    PaginationTools.SendPage(receiver, page, uixInfo, new PaginationTools.Settings
                    {
                        HeaderFormat = "Extended User Information [Page {0} of {1}]",
                        HeaderTextColor = Color.Lime,
                        LineTextColor = Color.White,
                        FooterFormat = string.Format("/uix {0} {1} for more", ply.Name, page + 1),
                        FooterTextColor = Color.Lime
                    });
                }
                else
                    receiver.SendErrorMessage("{0} is not logged in", ply.Name);
            }
            else
            {
                var ply = player as StoredPlayer;

                var uixInfo = new List<string>();
                var time1 = DateTime.Now.Subtract(DateTime.Parse(ply.firstLogin));

                uixInfo.Add(string.Format("UIX info for {0}", ply.name));
                uixInfo.Add(string.Format("{0} is a member of group {1}", ply.name,
                    TShock.Users.GetUserByName(ply.name).Group));
                uixInfo.Add(string.Format("First login: {0} ({1} ago)",
                    ply.firstLogin, time1.TimeSpanPlayed()));

                uixInfo.Add(string.Format("Last seen: {0}", 
                    DateTime.Now.Subtract(DateTime.Parse(ply.lastSeen)).TimeSpanPlayed()));
                uixInfo.Add(string.Format("Overall play time: {0}", ply.TimePlayed()));
                uixInfo.Add(string.Format("Logged in {0} times since registering", ply.loginCount));
                uixInfo.Add(string.Format("Known accounts: {0}",
                    ply.knownAccounts.Length > 0 ? ply.knownAccounts : "None"));
                uixInfo.Add(string.Format("Known IPs: {0}",
                    ply.knownIPs.Length > 0 ? ply.knownIPs : "None"));

                PaginationTools.SendPage(receiver, page, uixInfo, new PaginationTools.Settings
                {
                    HeaderFormat = "Extended User Information [Page {0} of {1}]",
                    HeaderTextColor = Color.Lime,
                    LineTextColor = Color.White,
                    FooterFormat = string.Format("/uix {0} {1} for more", ply.name, page + 1),
                    FooterTextColor = Color.Lime
                });
            }
        }

        public List<SPlayer> GetPlayersByIp(string ip)
        {
            var matches = new List<SPlayer>();
            foreach (var player in Statistics.Players)
            {
                if (player.TsPlayer.IP == ip)
                    return new List<SPlayer> { player };
                if (player.TsPlayer.IP.Contains(ip))
                    matches.Add(player);
            }

            return matches;
        }

        public List<StoredPlayer> GetStoredPlayersByIp(string ip)
        {
            var matches = new List<StoredPlayer>();
            foreach (var storedplayer in Statistics.StoredPlayers)
            {
                if (storedplayer.knownIPs == ip)
                    return new List<StoredPlayer> { storedplayer };
                if (storedplayer.knownIPs.Contains(ip))
                    matches.Add(storedplayer);
            }

            return matches;
        }

        public List<StoredPlayer> GetStoredPlayers(string name)
        {
            var matches = new List<StoredPlayer>();
            foreach (var storedplayer in Statistics.StoredPlayers)
            {
                if (String.Equals(storedplayer.name, name, StringComparison.CurrentCultureIgnoreCase))
                    return new List<StoredPlayer> { storedplayer };
                if (storedplayer.name.ToLower().Contains(name.ToLower()) && !matches.Contains(storedplayer))
                    matches.Add(storedplayer);
            }

            return matches;
        }

        public StoredPlayer GetStoredPlayer(string accountName, string accountIp)
        {
            return Statistics.StoredPlayers.FirstOrDefault(storedplayer => storedplayer.knownAccounts.Contains(accountName) && storedplayer.knownAccounts.Contains(accountIp));
        }

        #endregion


        public void RegisterSubs()
        {
            handler.RegisterSubcommand("time", SCommands.CmdCheckTime, "stats.time", "stats.*");
            handler.RegisterSubcommand("afk", SCommands.CmdCheckAfk, "stats.afk", "stats.*");
            handler.RegisterSubcommand("kills", SCommands.CmdCheckKills, "stats.kills", "stats.*");
            handler.RegisterSubcommand("name", SCommands.CmdCheckName, "stats.name", "stats.*");

            handler.helpText = "Valid subcommands of /check:|[time \\ afk \\ kills]|Syntax: /check [option] [playerName \\ self]";
        }
    }

    public class SubCommandHandler
    {
        private readonly List<SubCommand> _subCommands = new List<SubCommand>();

        public string helpText;

        public SubCommandHandler()
        {
            RegisterSubcommand("help", DisplayHelpText);
        }

        private void DisplayHelpText(CommandArgs args)
        {
            foreach (var item in helpText.Split('|'))
                args.Player.SendInfoMessage(item);
        }

        public void RegisterSubcommand(string command, Action<CommandArgs> func, params string[] permissions)
        {
            _subCommands.Add(new SubCommand(command, func, permissions));
        }
        public void RegisterSubcommand(string command, Action<CommandArgs> func, string permission)
        {
            _subCommands.Add(new SubCommand(command, func, permission));
        }

        public void RunSubcommand(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var newargs = new CommandArgs(args.Message, args.Player, args.Parameters.GetRange(1, args.Parameters.Count - 1));
                try
                {
                    var count = _subCommands.Find(
                        command => command.name == args.Parameters[0]).permissions.Count(
                            perm => !args.Player.Group.HasPermission(perm));

                    if (count == _subCommands.Find(command => command.name == args.Parameters[0]).permissions.Count)
                        args.Player.SendErrorMessage("You do not have permission to use that command");
                    else
                        _subCommands.Find(command => command.name == args.Parameters[0]).func.Invoke(args);
                }
                catch (Exception e)
                {
                    args.Player.SendErrorMessage("Command failed.");
                    Log.Error(e.Message);
                    _subCommands.Find(command => command.name == "help").func.Invoke(newargs);
                }
            }
            else
                _subCommands.Find(command => command.name == "help").func.Invoke(args);
        }
    }

    public class SubCommand
    {
        public readonly List<string> permissions;
        public readonly string name;
        public readonly Action<CommandArgs> func;

        public SubCommand(string name, Action<CommandArgs> func, params string[] permissions)
        {
            this.permissions = new List<string>(permissions);
            this.name = name;
            this.func = func;
        }
        public SubCommand(string name, Action<CommandArgs> func, string permission)
        {
            permissions.Add(permission);
            this.name = name;
            this.func = func;
        }
        public SubCommand(string name, Action<CommandArgs> func)
        {
            this.name = name;
            this.func = func;
        }
    }
}
