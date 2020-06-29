using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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
				{PacketTypes.PlayerDeathV2, HandlePlayerKillMe}, // 44 .PlayerKillMe
				{PacketTypes.PlayerHurtV2, HandlePlayerDamage}, // 26 .PlayerDamage
				{PacketTypes.NpcStrike, HandleNpcEvent} // 28
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

	  private static readonly int[] _nonTargetingAI =
	  {
	    75, // Rider AI, usually boss parts and mobs riding each other
      80  // Martian probe
	  };

	  private static readonly int[] _nonTargetingMobTypes =
	  {
	    NPCID.CultistArcherBlue,
	    NPCID.CultistArcherWhite,
	    NPCID.CultistDevote,
      NPCID.CultistBoss
	  };


    private static bool IsTargeting(NPC npc)
	  {
	    if (npc.target > 255)
	    {
	      if (_nonTargetingAI.Contains(npc.aiStyle))
	        return true;
        else if (_nonTargetingMobTypes.Contains(npc.type))
	        return true;
	      else
	        return false;
	    }

	    return true;
	  }

	  private static readonly int[] _bossParts =
	  {
	    NPCID.MartianSaucer,
	    NPCID.MartianSaucerCannon,
	    NPCID.MartianSaucerTurret
    };

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

			if (IsTargeting(Main.npc[npcId]))
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
                        Statistics.database.UpdateKillingSpree(player.Account.ID, 1, 0, 0); // byDii
						Statistics.database.UpdateKills(player.Account.ID, KillType.Mob); // byDii
                        KillingSpree.SendKillingNotice(player.Name, player.Account.ID, 1, 0, 0); // byDii
                        SpeedKills.PlayerKill(index);
                        Statistics.SentDamageCache[player.Index][KillType.Mob] += Main.npc[npcId].life;
						//Push damage to database on kill
						Statistics.database.UpdateMobDamageGiven(player.Account.ID, player.Index); // byDii
					}
					//a boss kill
					else
					{
                        Statistics.database.UpdateKillingSpree(player.Account.ID, 0, 1, 0); // byDii
                        Statistics.database.UpdateKills(player.Account.ID, KillType.Boss); // byDii
                        KillingSpree.SendKillingNotice(player.Name, player.Account.ID, 0, 1, 0); // byDii
                        SpeedKills.PlayerKill(index); 
                        Statistics.SentDamageCache[player.Index][KillType.Boss] += Main.npc[npcId].life;
						Statistics.database.UpdateBossDamageGiven(player.Account.ID, player.Index); // byDii
					}

					//Push player damage dealt and damage received as well
					Statistics.database.UpdatePlayerDamageGiven(player.Account.ID, player.Index); // byDii
					Statistics.database.UpdateDamageReceived(player.Account.ID, player.Index); // byDii
					Statistics.database.UpdateHighScores(player.Account.ID); // byDii
				}
				else
				{
				  if (Main.npc[npcId].boss || _bossParts.Contains(Main.npc[npcId].type)) // boss parts
				    Statistics.SentDamageCache[player.Index][KillType.Boss] += hitDamage;
				  else
				    Statistics.SentDamageCache[player.Index][KillType.Mob] += hitDamage;
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

            if (player.Name == null)
                return false;

//            Console.WriteLine("player  " + player.User.ID);
            if (Statistics.PlayerKilling[player] != null)
			{
				//Only update killer if the killer is logged in
				if (Statistics.PlayerKilling[player].IsLoggedIn && pvp)
				{
                    Statistics.database.UpdateKillingSpree(Statistics.PlayerKilling[player].Account.ID, 0, 0, 1); // byDii
                    Statistics.database.UpdateKills(Statistics.PlayerKilling[player].Account.ID, KillType.Player); // byDii
                    KillingSpree.SendKillingNotice(player.Name, player.Account.ID, 0, 0, 1); // byDii
                    SpeedKills.PlayerKill(index); 
                    Statistics.database.UpdateHighScores(Statistics.PlayerKilling[player].Account.ID); // byDii
					Statistics.database.UpdatePlayerDamageGiven(Statistics.PlayerKilling[player].Account.ID, // byDii
						Statistics.PlayerKilling[player].Index);
					Statistics.database.UpdateDamageReceived(Statistics.PlayerKilling[player].Account.ID, // byDii
						Statistics.PlayerKilling[player].Index);
				}
				Statistics.PlayerKilling[player] = null;
 			}

			Statistics.database.UpdateDeaths(player.Account.ID); // byDii
			Statistics.database.UpdatePlayerDamageGiven(player.Account.ID, player.Index); // byDii
			//update all received damage on death
			Statistics.database.UpdateDamageReceived(player.Account.ID, player.Index); // byDii
			Statistics.database.UpdateHighScores(player.Account.ID); // byDii

            Statistics.database.CloseKillingSpree(player.Account.ID); // byDii
            KillingSpree.ClearBlitzEvent(player.Account.ID); // byDii
            SpeedKills.ResetPlayer(index);

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

			damage = (short) Main.CalculateDamagePlayersTake(damage, player.TPlayer.statDefense); // .CalculateDamage

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