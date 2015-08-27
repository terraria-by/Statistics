using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics
{
    public class Config
    {
        public bool isActive = true;

        public bool byTime = true;
        public int KillstimeInterval = 10;    // in minutes
        public int KillstimeOffset = 1 * 5;    // in minutes
        public int[] KillsColor = { 255, 0, 0 };  // Red

        public int DamagetimeInterval = 10;    // in minutes
        public int DamagetimeOffset = 2 * 5;    // in minutes
        public int[] DamageColor = { 0, 0, 255 };  // Blue

        public int DeathstimeInterval = 10;    // in minutes
        public int DeathstimeOffset = 3 * 5;    // in minutes
        public int[] DeathsColor = { 0, 255, 0 };  // Green

        public bool showKills = true;
        public bool showDamage = true;
        public bool showDeaths = true;

        public bool tellConsole = true;
        public ConsoleColor consoleColor = ConsoleColor.White;  // 15
        public bool showTimeStamp = false;

        /*  Console Colors index into zero based
Black The color black. 
 DarkBlue The color dark blue. 
 DarkGreen The color dark green. 
 DarkCyan The color dark cyan (dark blue-green). 
 DarkRed The color dark red. 
 DarkMagenta The color dark magenta (dark purplish-red). 
 DarkYellow The color dark yellow (ochre). 
 Gray The color gray. 
 DarkGray The color dark gray. 
 Blue The color blue. 
 Green The color green. 
 Cyan The color cyan (blue-green). 
 Red The color red. 
 Magenta The color magenta (purplish-red). 
 Yellow The color yellow. 
 White The color white. 
*/
        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            return !File.Exists(path)
                ? new Config()
                : JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}

