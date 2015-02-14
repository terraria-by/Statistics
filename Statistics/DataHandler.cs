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
                {PacketTypes.NpcStrike, HandleNpcEvent},
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
                    Log.Error(ex.ToString());
                }
            }
            return false;
        }

        private static bool HandleNpcEvent(GetDataHandlerArgs args)
        {
            var index = args.Player.Index;
            var npcId = (byte) args.Data.ReadByte();
            args.Data.ReadByte();
            var damage = args.Data.ReadInt16();
            var crit = args.Data.ReadBoolean();
            var player = TShock.Players.First(p => p.Index == index);

            if (Main.npc[npcId].target < 255)
            {
                var critical = 1;
                if (crit)
                    critical = 2;
                var hitDamage = (damage - Main.npc[npcId].defense/2)*critical;

                if (hitDamage > Main.npc[npcId].life && Main.npc[npcId].active && Main.npc[npcId].life > 0)
                {
                    //not a boss kill
                    if (!Main.npc[npcId].boss)
                        Statistics.database.UpdateKills(player.UserAccountName, KillType.Mob);
                    //a boss kill
                    else
                        Statistics.database.UpdateKills(player.UserAccountName, KillType.Boss);

                    Statistics.database.UpdateHighScores(player.UserAccountName);
                }
            }
            else
                return true;

            return false;
        }

        private static bool HandlePlayerKillMe(GetDataHandlerArgs args)
        {
            var index = args.Player.Index;
            args.Data.ReadByte();
            args.Data.ReadByte();
            args.Data.ReadInt16();
            var pvp = args.Data.ReadBoolean();
            var player = TShock.Players.First(p => p.Index == index);

            if (Statistics.PlayerKilling[player] != null)
            {
                if (pvp)
                {
                    Statistics.database.UpdateKills(Statistics.PlayerKilling[player].UserAccountName, KillType.Player);
                    Statistics.database.UpdateDeaths(player.UserAccountName);

                    Statistics.database.UpdateHighScores(Statistics.PlayerKilling[player].UserAccountName);
                }
                Statistics.PlayerKilling[player] = null;
            }
            else
                Statistics.database.UpdateDeaths(player.UserAccountName);

            Statistics.database.UpdateHighScores(player.UserAccountName);

            return false;
        }

        private static bool HandlePlayerDamage(GetDataHandlerArgs args)
        {
            var index = args.Player.Index;
            var playerId = (byte) args.Data.ReadByte();
            args.Data.ReadByte();
            args.Data.ReadInt16();
            var player = TShock.Players.First(p => p.Index == playerId);
            args.Data.ReadBoolean();
            args.Data.ReadByte();

            Statistics.PlayerKilling[player] = index != playerId ? TShock.Players.First(p => p.Index == index) : null;

            return false;
        }
    }
}