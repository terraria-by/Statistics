using System;
using System.Collections.Generic;
using Terraria;
using System.IO;
using TShockAPI;
using System.IO.Streams;

namespace Statistics
{
    internal delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);
    internal class GetDataHandlerArgs : EventArgs
    {
        public TSPlayer Player { get; private set; }
        public MemoryStream Data { get; private set; }

        public Player TPlayer
        {
            get { return Player.TPlayer; }
        }

        public GetDataHandlerArgs(TSPlayer player, MemoryStream data)
        {
            Player = player;
            Data = data;
        }
    }
    internal static class GetDataHandlers
    {
        private static Dictionary<PacketTypes, GetDataHandlerDelegate> GetDataHandlerDelegates;

        public static void InitGetDataHandler()
        {
            GetDataHandlerDelegates = new Dictionary<PacketTypes, GetDataHandlerDelegate>
            {
                {PacketTypes.PlayerKillMe, HandlePlayerKillMe},             
                {PacketTypes.PlayerDamage, HandlePlayerDamage},
                {PacketTypes.NpcStrike, HandleNPCEvent},
            };
        }

        public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data)
        {
            GetDataHandlerDelegate handler;
            if (GetDataHandlerDelegates.TryGetValue(type, out handler))
            {
                try
                {
                    return handler(new GetDataHandlerArgs(player, data));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return false;
        }

        private static bool HandleNPCEvent(GetDataHandlerArgs args)
        {
            var index = args.Player.Index;
            var npcId = (byte)args.Data.ReadByte();
            var hitDirection = (byte)args.Data.ReadByte();
            var damage = args.Data.ReadInt16();
            var crit = args.Data.ReadBoolean();
            var player = Statistics.Tools.GetPlayer(index);

            if (Main.npc[npcId].target < 255)
            {
                var critical = 1;
                if (crit)
                    critical = 2;
                var hitDamage = (damage - Main.npc[npcId].defense / 2) * critical;

                if (hitDamage > Main.npc[npcId].life && Main.npc[npcId].active && Main.npc[npcId].life > 0)
                {
                    if (!Main.npc[npcId].boss)
                        player.mobkills++;
                    else
                        player.bosskills++;
                }
            }
            else
                return true;

            return false;
        }

        private static bool HandlePlayerKillMe(GetDataHandlerArgs args)
        {
            var index = args.Player.Index;
            var playerId = (byte)args.Data.ReadByte();
            var hitDirection = (byte)args.Data.ReadByte();
            var damage = args.Data.ReadInt16();
            var pvp = args.Data.ReadBoolean();
            var player = Statistics.Tools.GetPlayer(playerId);

            if (player.killingPlayer != null)
            {
                if (pvp)
                {
                    player.killingPlayer.kills++;
                    player.deaths++;
                }
                player.killingPlayer = null;
            }
            else
                player.deaths++;

            return false;
        }

        private static bool HandlePlayerDamage(GetDataHandlerArgs args)
        {
            var index = args.Player.Index;
            var playerId = (byte)args.Data.ReadByte();
            var hitDirection = (byte)args.Data.ReadByte();
            var damage = args.Data.ReadInt16();
            var player = Statistics.Tools.GetPlayer(playerId);
            var pvp = args.Data.ReadBoolean();
            var crit = (byte)args.Data.ReadByte();

            player.killingPlayer = index != playerId ? Statistics.Tools.GetPlayer(index) : null;

            return false;
        }
    }
}