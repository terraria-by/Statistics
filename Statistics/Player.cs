using TShockAPI;
using Terraria;

namespace Statistics
{
    public class SPlayer
    {
        public readonly int index;

        public TSPlayer TsPlayer { get { return TShock.Players[index]; } }
        public string Name { get { return Main.player[index].name; } }
        public StoredPlayer storage;

        public bool afk;
        public int afkCount;
        public float LastPosX { get; set; }
        public float LastPosY { get; set; }

        public int timePlayed;

        public string firstLogin;
        public string lastSeen;
        public int loginCount;
        public string knownAccounts;
        public string knownIPs;

        public int deaths;
        public int kills;
        public int mobkills;
        public int bosskills;

        public Vector2 posPoint;

        public SPlayer killingPlayer;

        public SPlayer(int index)
        {
            this.index = index;
            LastPosX = TShock.Players[this.index].X;
            LastPosY = TShock.Players[this.index].Y;
        }
    }

    public class StoredPlayer
    {
        public readonly string name;
        public string firstLogin;
        public string lastSeen;
        public int totalTime;
        public int loginCount;
        public string knownAccounts;
        public string knownIPs;

        public int kills;
        public int deaths;
        public int mobkills;
        public int bosskills;

        public StoredPlayer(string name, string firstLogin, string lastSeen, int totalTime, int loginCount,
            string knownAccounts, string knownIPs, int kills, int deaths, int mobkills, int bosskills)
        {
            this.name = name;
            this.firstLogin = firstLogin;
            this.lastSeen = lastSeen;
            this.totalTime = totalTime;
            this.loginCount = loginCount;
            this.knownAccounts = knownAccounts;
            this.knownIPs = knownIPs;
            this.kills = kills;
            this.deaths = deaths;
            this.mobkills = mobkills;
            this.bosskills = bosskills;
        }
    }
}
