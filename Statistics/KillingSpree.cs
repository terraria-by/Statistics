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
        public static Config config = new Config();
        public static string configPath = Path.Combine(TShock.SavePath, "StatsAnnouncements.json");

       public static void SendKillingNotice(int killCount)
       {
           if (killCount < 0)
               return;
           int killLevel = -1;
           for (int i = 0; i < config.KillingSpreeThreshold.Length; i++)
           {
               if (i == config.KillingSpreeThreshold.Length - 1)
               {
                   killLevel = i;
                   break;
               }
               else if (killCount < config.KillingSpreeThreshold[i + 1])
               {
                   killLevel = i;
                   break;
               }
           }
 
           Announcements.ConsoleSendMessage(killLevel.ToString());
           if (killLevel < 0)
               return;

            string output = config.KillingSpreeMessage[killLevel];

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
