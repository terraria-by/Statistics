using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace Statistics
{
    public class HighScores
    {
        public readonly List<HighScore> highScores = new List<HighScore>();

        public void DisplayHighScores(TSPlayer player, int page = 1)
        {
            var ordered = (from x in highScores orderby -x.score select x).ToList();

            var hs = ordered.ToDictionary(obj => obj.name, obj => obj.score);

            HsPagination.SendPage(player, page, hs, new HsPagination.FormatSettings
            {
                FooterFormat = "use /hs {0} for more high scores", FooterTextColor = Color.Lime,
                HeaderFormat = "High Scores- Page {0} of {1}", HeaderTextColor = Color.Lime,
                IncludeFooter = true, IncludeHeader = true, MaxLinesPerPage = 5,
                NothingToDisplayString = "No highscores available"
            });
            
            //if (plyHs == null)
            //{
            //    //IRC support
            //    try
            //    {
            //        player.SendInfoMessage("Current top {0}player{1}:", ordered.Count >= 3 ? "3 " : "",
            //            ordered.Count.Suffix());
            //        player.SendSuccessMessage("1. {0} with {1} point{2}", globalHs.name, globalHs.score,
            //            globalHs.score.Suffix());

            //        if (ordered.Count >= 3)
            //        {
            //            player.SendInfoMessage("2. {0} with {1} point{2}", ordered[1].name, ordered[1].score,
            //                ordered[1].score.Suffix());

            //            player.SendWarningMessage("3. {0} with {1} point{2}", ordered[2].name, ordered[2].score,
            //                ordered[2].score.Suffix());
            //        }
            //        return;
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.ConsoleError(ex.ToString());
            //        return;
            //    }
            //}

            //if (globalHs == null)
            //{
            //    player.SendErrorMessage("Error encountered with high scores: No global high score found!");
            //    return;
            //}

            //player.SendInfoMessage("Your high-scores position: {0} ({1} points)", ordered.IndexOf(plyHs) + 1,
            //    plyHs.score);
            //player.SendInfoMessage("Current top {0}player{1}:", ordered.Count >= 3 ? "3 " : "",
            //    ordered.Count >= 3 ? "s" : "");
            //player.SendSuccessMessage("1. {0} with {1} points", globalHs.name, globalHs.score);
            //if (ordered.Count >= 3)
            //{
            //    player.SendInfoMessage("2. {0} with {1} points", ordered[1].name, ordered[1].score);
            //    player.SendWarningMessage("3. {0} with {1} points", ordered[2].name, ordered[2].score);
            //}
        }
    }

    public class HighScore
    {
        public string name;
        public int score;

        public HighScore(string name, int kills, int mobKills, int deaths, int bossKills, int time)
        {
            this.name = name;
            score = ((2*kills) + mobKills + (3*bossKills))/(deaths == 0 ? 1 : deaths);
            score += (time/60);
        }

        public void UpdateHighScore(int kills, int mobKills, int deaths, int bossKills, int time)
        {
            score = ((2*kills) + mobKills + (3*bossKills))/(deaths == 0 ? 1 : deaths);
            score += (time/60);
        }
    }
}
