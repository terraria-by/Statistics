using System;
using System.Collections.Generic;
using Terraria;
using System.IO;
using TShockAPI;
using System.IO.Streams;
using System.Linq;

namespace Statistics
{
	internal delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);

	internal class GetDataHandlerArgs : EventArgs
	{
		public TSPlayer Player { get; private set; }
		public MemoryStream Data { get; private set; }

		public GetDataHandlerArgs(TSPlayer player, MemoryStream data)
		{
			Player = player;
			Data = data;
		}
	}

	internal static class GetDataHandlers
	{
		private static Dictionary<PacketTypes, GetDataHandlerDelegate> _getDataHandlerDelegates;

		public static void InitGetDataHandler()
		{
			_getDataHandlerDelegates = new Dictionary<PacketTypes, GetDataHandlerDelegate>
			{
				{PacketTypes.PlayerKillMe, HandlePlayerKillMe},
				{PacketTypes.PlayerDamage, HandlePlayerDamage},
				{PacketTypes.NpcStrike, HandleNpcEvent}
			};
		}

		public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data)
		{
			GetDataHandlerDelegate handler;
			if (_getDataHandlerDelegates.TryGetValue(type, out handler))
			{
				try
				{
					return handler(new GetDataHandlerArgs(player, data));
				}
				catch (Exception ex)
				{
					TShock.Log.Error(ex.ToString());
				}
			}
			return false;
		}

		private static bool HandleNpcEvent(GetDataHandlerArgs args)
		{
			if (args.Player == null) return false;
			var index = args.Player.Index;
			var npcId = (byte) args.Data.ReadByte();
			args.Data.ReadByte();
			var damage = args.Data.ReadInt16();
			var crit = args.Data.ReadBoolean();
			var player = TShock.Players.First(p => p != null && p.IsLoggedIn && p.Index == index);

			if (player == null)
				return false;

			if (Main.npc[npcId].target < 255)
			{
				var critical = 1;
				if (crit)
					critical = 2;
				var hitDamage = (damage - Main.npc[npcId].defense/2)*critical;

				if (hitDamage > Main.npc[npcId].life && Main.npc[npcId].active && Main.npc[npcId].life > 0)
				{
					//not a boss kill
					if (!Main.npc[npcId].boss && !Main.npc[npcId].friendly)
					{
						Statistics.database.UpdateKills(player.UserID, KillType.Mob);
						Statistics.SentDamageCache[player.Index][KillType.Mob] += Main.npc[npcId].life;
						//Push damage to database on kill
						Statistics.database.UpdateMobDamageGiven(player.UserID, player.Index);
					}
					//a boss kill
					else
					{
						Statistics.database.UpdateKills(player.UserID, KillType.Boss);
						Statistics.SentDamageCache[player.Index][KillType.Boss] += Main.npc[npcId].life;
						Statistics.database.UpdateBossDamageGiven(player.UserID, player.Index);
					}

					//Push player damage dealt and damage received as well
					Statistics.database.UpdatePlayerDamageGiven(player.UserID, player.Index);
					Statistics.database.UpdateDamageReceived(player.UserID, player.Index);
					Statistics.database.UpdateHighScores(player.UserID);
				}
				else
				{
					if (!Main.npc[npcId].boss)
						Statistics.SentDamageCache[player.Index][KillType.Mob] += hitDamage;
					else
						Statistics.SentDamageCache[player.Index][KillType.Boss] += hitDamage;
				}
			}
			else
				return true;

			return false;
		}

		private static bool HandlePlayerKillMe(GetDataHandlerArgs args)
		{
			if (args.Player == null) return false;
			var index = args.Player.Index;
			args.Data.ReadByte();
			args.Data.ReadByte();
			args.Data.ReadInt16();
			var pvp = args.Data.ReadBoolean();
			var player = TShock.Players.First(p => p != null && p.IsLoggedIn && p.Index == index);

			if (player == null)
				return false;

			if (Statistics.PlayerKilling[player] != null)
			{
				//Only update killer if the killer is logged in
				if (Statistics.PlayerKilling[player].IsLoggedIn && pvp)
				{
					Statistics.database.UpdateKills(Statistics.PlayerKilling[player].UserID, KillType.Player);
					Statistics.database.UpdateHighScores(Statistics.PlayerKilling[player].UserID);
					Statistics.database.UpdatePlayerDamageGiven(Statistics.PlayerKilling[player].UserID,
						Statistics.PlayerKilling[player].Index);
					Statistics.database.UpdateDamageReceived(Statistics.PlayerKilling[player].UserID,
						Statistics.PlayerKilling[player].Index);
				}
				Statistics.PlayerKilling[player] = null;
			}

			Statistics.database.UpdateDeaths(player.UserID);
			Statistics.database.UpdatePlayerDamageGiven(player.UserID, player.Index);
			//update all received damage on death
			Statistics.database.UpdateDamageReceived(player.UserID, player.Index);
			Statistics.database.UpdateHighScores(player.UserID);

			return false;
		}

		private static bool HandlePlayerDamage(GetDataHandlerArgs args)
		{
			if (args.Player == null) return false;
			var index = args.Player.Index;
			var playerId = (byte) args.Data.ReadByte();
			args.Data.ReadByte();
			var damage = args.Data.ReadInt16();
			//player being attacked
			var player = TShock.Players.First(p => p != null && p.IsLoggedIn && p.Index == index);

			if (player == null)
				return false;

			var crit = args.Data.ReadBoolean();
			args.Data.ReadByte();

			//Attacking player
			Statistics.PlayerKilling[player] = index != playerId ? args.Player : null;

			damage = (short) Main.CalculateDamage(damage, player.TPlayer.statDefense);

			if (Statistics.PlayerKilling[player] != null)
			{
				Statistics.SentDamageCache[args.Player.Index][KillType.Player] += damage;
				Statistics.RecvDamageCache[player.Index] += damage;
			}
			else
				Statistics.RecvDamageCache[player.Index] += (damage*(crit ? 2 : 1));

			return false;
		}
	}
}