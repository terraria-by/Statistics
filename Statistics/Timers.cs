using System.Linq;
using System.Timers;
using TShockAPI;

namespace Statistics
{
    public class Timers
    {
        private readonly Timer _aTimer = new Timer(1000);
        private readonly Timer _uTimer = new Timer(60 * 1000);
        private readonly Timer _databaseSaver = new Timer(600 * 1000);

        public void Start()
        {
            _aTimer.Enabled = true;
            _aTimer.Elapsed += AfkTimer;

            _uTimer.Enabled = true;
            _uTimer.Elapsed += UpdateTimer;

            _databaseSaver.Enabled = true;
            _databaseSaver.Elapsed += DatabaseTimer;
        }

        void DatabaseTimer(object sender, ElapsedEventArgs args)
        {
            Statistics.database.SaveState();
        }

        void AfkTimer(object sender, ElapsedEventArgs args)
        {
            /* Needs advanced afk checks */
            foreach (var player in Statistics.Players)
            {
                if ((int)player.TsPlayer.X == (int)player.LastPosX && (int)player.TsPlayer.Y == (int)player.LastPosY)
                {
                    player.afkCount++;
                    if (player.afkCount > 300)
                    {
                        if (!player.afk)
                        {
                            TSPlayer.All.SendInfoMessage("{0} is now away", player.TsPlayer.Name);
                            player.TsPlayer.SendWarningMessage("You are now marked as away.");
                            player.TsPlayer.SendWarningMessage("This time is not being counted towards your statistics");
                            player.afk = true;
                        }
                    }
                }
                else
                {
                    player.timePlayed++;
                    if (player.afk)
                        player.afk = false;

                    if (player.afkCount > 0)
                        player.afkCount = 0;
                }

                player.LastPosX = player.TsPlayer.X;
                player.LastPosY = player.TsPlayer.Y;
            }
        }

        void UpdateTimer(object sender, ElapsedEventArgs args)
        {
            foreach (var player in Statistics.Players.Where(player => !player.afk && player.TsPlayer.IsLoggedIn))
                player.SyncStats();
        }
    }
}
