using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TShockAPI;
namespace Statistics
{
    static class KillingSpree
    {
        public static Config config = Statistics.config;

        private static string[] killOptions = { "Mob", "Boss", "Player", "All" };
        public static void SendKillingNotice(string playerName, int playerId)
        {
            config = Statistics.config;
            int killCount = 0;
            int[] killList;
            string killType = "";
            killList = Statistics.database.GetKills(playerId);
//            Console.WriteLine(playerId + ":" + config.KillingSpreeType);
            int slot = 0;
            for (int i = 0; i < killOptions.Length; i++)
            {
                if (killOptions[i].Equals(config.KillingSpreeType))
                {
                    killType = config.KillingSpreeType;
                    switch (killType)
                    {
                        case "Mob":
                            slot = 0;
                            break;
                        case "Boss":
                            slot = 1;
                            break;
                        case "Player":
                            slot = 2;
                            break;
                    }
                    if (slot == 0)
                        killCount = killList[0] + killList[1] + killList[2];
                    else
                        killCount = killList[slot];
                    break;
                }
            }
//            Console.WriteLine(killCount + ":" + killType);
            if (killType.Length == 0)
                return;
            
            if (killCount < 0)
                return;
            int killLevel = -1;
            for (int i = 0; i < config.KillingSpreeThreshold.Length; i++)
            {
                if (killCount == config.KillingSpreeThreshold[i])
                {
                    killLevel = i;
                    break;
                }
            }
 
            if (killLevel < 0)
                return;

            string output = playerName + " " + config.KillingSpreeMessage[killLevel];

            if (config.showTimeStamp)
            {
                DateTime date = DateTime.Now;
                TSPlayer.All.SendMessage(string.Format(" {0}", date), Color.Red);
                if (config.tellConsole)
                    TSPlayer.Server.SendMessage(string.Format(" {0}", date), Color.Red);
            }
            TSPlayer.All.SendMessage(output, Convert.ToByte(config.KillingSpreeColor[0]), Convert.ToByte(config.KillingSpreeColor[1]), Convert.ToByte(config.KillingSpreeColor[2]));
            if (config.tellConsole)
                Announcements.ConsoleSendMessage(output);

        }
    }
}
