using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

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
					"Valid flags: -k : kills, -t : time, -s : seen, -hs : highscores, -d : damage");
				return;
			}

			switch (args.Parameters[0].ToLowerInvariant())
			{
				case "-k":
				case "-kills":
				{
					if (args.Parameters.Count < 2)
					{
						var kills = Statistics.database.GetKills(args.Player.UserID);
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
						var times = Statistics.database.GetTimes(args.Player.UserID, ref logins);
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
						args.Player.SendErrorMessage("Invalid syntax. /info -s [player name]");
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
				case "-d":
				case "-damage":
				{
					int mob = 0, boss = 0, player = 0, received = 0;
					if (args.Parameters.Count < 2)
					{
						Statistics.database.GetDamage(args.Player.UserID, ref mob, ref boss, ref player, ref received);
						args.Player.SendSuccessMessage("You have dealt {0} damage to mobs, {1} damage to bosses "
						                               + "and {2} damage to players.", mob, boss, player);
						args.Player.SendSuccessMessage("You have been dealt {0} damage.", received);
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
						Statistics.database.GetDamage(user.ID, ref mob, ref boss, ref player, ref received);
						args.Player.SendSuccessMessage("{0} has dealt {1} damage to mobs, {2} damage to bosses " 
							+ "and {3} damage to players.", user.Name, mob, boss, player);
						args.Player.SendSuccessMessage("{0} has been dealt {1} damage.", user.Name, received);
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
					return new List<User> {user};
				if (user.Name.StartsWith(username))
					ret.Add(user);
			}
			return ret;
		}
	}
}