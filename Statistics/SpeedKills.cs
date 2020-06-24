using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using TShockAPI;
using Terraria;
namespace Statistics
{
  static class SpeedKills
    {
      private static Timer[] speedTimers = new Timer[TShock.Config.MaxSlots + TShock.Config.ReservedSlots];
        public static Config config = Statistics.config;
        static SpeedKills()
        {
            config = Statistics.config;

        }
        public static void StartSpeedKill()
        {
            string output = "Speed Spree is already running, use -stop first.";
            if (config.SpeedSpree)
            {
                    if (config.tellConsole)
                        Announcements.ConsoleSendMessage(output);
                return;
            }

            if (Statistics.statsDebug)
            {
                output = "Speed Spree has now started. You will have " + KillingSpree.FormatTimeSpan(new TimeSpan(0, 0, config.SpeedSpreeTimeout)) + "to kill.";
                TSPlayer.All.SendMessage(output, Convert.ToByte(config.SpeedSpreeColor[0]), Convert.ToByte(config.SpeedSpreeColor[1]), Convert.ToByte(config.SpeedSpreeColor[2]));
                if (config.tellConsole)
                    Announcements.ConsoleSendMessage(output);
            }
            config.SpeedSpree = true;
        }

        public static void StopSpeedKill()
        {
            for (int i = 0; i < speedTimers.Length; i++)
            {
                if (speedTimers[i] != null)
                {
                    speedTimers[i].Stop();
                    speedTimers[i].Dispose();
                    speedTimers[i] = null;
                }
            }
            if (Statistics.statsDebug)
            {
                string output = "Speed Spree has now stopped.";
                TSPlayer.All.SendMessage(output, Convert.ToByte(config.SpeedSpreeColor[0]), Convert.ToByte(config.SpeedSpreeColor[1]), Convert.ToByte(config.SpeedSpreeColor[2]));
                if (config.tellConsole)
                    Announcements.ConsoleSendMessage(output);
            }
            config.SpeedSpree = false;
        }

        public static void listTimers()
        {
            DateTime fire;
//            Console.WriteLine("Max timers: " + speedTimers.Length);
            for (int i = 0; i < speedTimers.Length; i++)
            {
                if (speedTimers[i] != null)
                {
                    fire = DateTime.Now.AddMilliseconds(speedTimers[i].Interval);
//                    Console.WriteLine("Player " + i + " fires at " + fire);
                }
           }
        }
        public static void NewPlayer(int playerId)
        {
            if (!config.SpeedSpree)
                return;
//            Console.WriteLine(playerId + " set");
            if (speedTimers[playerId] == null)
                speedTimers[playerId] = new Timer();
            else
                return;     // already clock running

            speedTimers[playerId].Elapsed += new ElapsedEventHandler(TimerHasExpired);
            speedTimers[playerId].Interval = config.SpeedSpreeTimeout * 1000;
            speedTimers[playerId].AutoReset = false;
            speedTimers[playerId].Start();

        }

        public static void PlayerKill(int playerId)
        {
            if (!config.SpeedSpree)
                return;
 //           Console.WriteLine(playerId + " kill set");
            if (speedTimers[playerId] == null)
            {
                speedTimers[playerId] = new Timer();
                speedTimers[playerId].Elapsed += new ElapsedEventHandler(TimerHasExpired);
 //               Console.WriteLine(playerId + " new kill set");

            }
            if (Statistics.statsDebug)
            {
                string output = "You have " + KillingSpree.FormatTimeSpan(new TimeSpan(0, 0, config.SpeedSpreeTimeout)) + "to kill something. " + DateTime.Now;
                TShock.Players[playerId].SendMessage(output, Convert.ToByte(config.SpeedSpreeColor[0]), Convert.ToByte(config.SpeedSpreeColor[1]), Convert.ToByte(config.SpeedSpreeColor[2]));
                if (config.tellConsole)
                    Announcements.ConsoleSendMessage(output);
            }

            speedTimers[playerId].Stop(); 
            speedTimers[playerId].Interval = config.SpeedSpreeTimeout * 1000;
            speedTimers[playerId].AutoReset = false;
            speedTimers[playerId].Start();

        }

      public static void ResetPlayer(int playerId)
        {
            if (!config.SpeedSpree)
                return;

            if (speedTimers[playerId] == null)
                return;

            speedTimers[playerId].Stop();
            speedTimers[playerId].Dispose();
            speedTimers[playerId] = null;
//            Console.WriteLine(playerId + " reset " + DateTime.Now);

        }

        public static void AnnounceSpree(int playerId)
        {
            if (!config.SpeedSpree)
                return;

            string output = config.SpeedSpreeAnnouncement;
            TShock.Players[playerId].SendMessage(output, Convert.ToByte(config.SpeedSpreeColor[0]), Convert.ToByte(config.SpeedSpreeColor[1]), Convert.ToByte(config.SpeedSpreeColor[2]));
            if (config.tellConsole)
                Announcements.ConsoleSendMessage(output);

        }
        private static void TimerHasExpired(Object source, System.Timers.ElapsedEventArgs e)
        {
            for (int i = 0; i < speedTimers.Length; i++)
            {
                if (speedTimers[i] == source)
                {
                    ResetPlayer(i);
                    Statistics.database.CloseKillingSpree(TShock.Players[i].Account.ID); // byDii
                    string output = "You have lost your killing spree!";
                    TShock.Players[i].SendMessage(output, Convert.ToByte(config.SpeedSpreeColor[0]), Convert.ToByte(config.SpeedSpreeColor[1]), Convert.ToByte(config.SpeedSpreeColor[2]));
                    if (Statistics.statsDebug)
                    {
                        output = TShock.Players[i].Name + " has lost their killing spree!";
                        Announcements.ConsoleSendMessage(output);
                    }
                }
           }
        }

    }
}
