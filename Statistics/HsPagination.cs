using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace Statistics
{
    /// <summary>
    /// Shameless copy/pasta of PaginationTools, with some edits
    /// </summary>
    public static class HsPagination
    {
        public class FormatSettings
        {
            public bool IncludeHeader { get; set; }

            private string _headerFormat;

            public string HeaderFormat
            {
                get { return _headerFormat; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException();

                    _headerFormat = value;
                }
            }

            public Color HeaderTextColor { get; set; }
            public bool IncludeFooter { get; set; }

            private string _footerFormat;

            public string FooterFormat
            {
                get { return _footerFormat; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException();

                    _footerFormat = value;
                }
            }

            public Color FooterTextColor { get; set; }
            public string NothingToDisplayString { get; set; }

            private int _maxLinesPerPage;

            public int MaxLinesPerPage
            {
                get { return _maxLinesPerPage; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentException("The value has to be greater than zero.");

                    _maxLinesPerPage = value;
                }
            }

            private int _pageLimit;

            public int PageLimit
            {
                get { return _pageLimit; }
                set
                {
                    if (value < 0)
                        throw new ArgumentException("The value has to be greater than or equal to zero.");

                    _pageLimit = value;
                }
            }

            public FormatSettings()
            {
                IncludeHeader = true;
                _headerFormat = "Page {0} of {1}";
                HeaderTextColor = Color.Green;
                IncludeFooter = true;
                _footerFormat = "Type /<command> {0} for more.";
                FooterTextColor = Color.Yellow;
                NothingToDisplayString = null;
                _maxLinesPerPage = 4;
                _pageLimit = 0;
            }
        }

        public static void SendPage(
            TSPlayer player, int pageNumber, Dictionary<string, int> dictionary, int dataToPaginateCount,
            FormatSettings settings = null)
        {
            if (settings == null)
                settings = new FormatSettings();

            if (dataToPaginateCount == 0)
            {
                if (settings.NothingToDisplayString != null)
                {
                    if (!player.RealPlayer)
                        player.SendSuccessMessage(settings.NothingToDisplayString);
                    else
                        player.SendMessage(settings.NothingToDisplayString, settings.HeaderTextColor);
                }
                return;
            }

            var pageCount = ((dataToPaginateCount - 1)/settings.MaxLinesPerPage) + 1;
            if (settings.PageLimit > 0 && pageCount > settings.PageLimit)
                pageCount = settings.PageLimit;
            if (pageNumber > pageCount)
                pageNumber = pageCount;

            if (settings.IncludeHeader)
            {
                if (!player.RealPlayer)
                    player.SendSuccessMessage(string.Format(settings.HeaderFormat, pageNumber, pageCount));
                else
                    player.SendMessage(string.Format(settings.HeaderFormat, pageNumber, pageCount),
                        settings.HeaderTextColor);
            }

            var listOffset = (pageNumber - 1)*settings.MaxLinesPerPage;
            var offsetCounter = 0;
            var lineCounter = 0;

            foreach (var lineData in dictionary)
            {
                if (offsetCounter++ < listOffset)
                    continue;
                if (lineCounter++ == settings.MaxLinesPerPage)
                    break;

                var lineColor = Color.Yellow;
                var hsName = lineData.Key;
                var hsScore = lineData.Value;
                var index = dictionary.Keys.ToList().IndexOf(hsName) + 1;

                if (index == 1)
                    lineColor = Color.Cyan;
                if (index == 2)
                    lineColor = Color.ForestGreen;
                if (index == 3)
                    lineColor = Color.OrangeRed;

                if (string.Equals(hsName, player.UserAccountName, StringComparison.CurrentCultureIgnoreCase))
                    lineColor = Color.White;

                if (!string.IsNullOrEmpty(hsName))
                {
                    if (!player.RealPlayer)
                        player.SendInfoMessage("{0}. {1} with {2} point{3}",
                            index, hsName, hsScore, hsScore.Suffix());
                    else
                        player.SendMessage(string.Format("{0}. {1} with {2} point{3}",
                            index, hsName, hsScore, hsScore.Suffix()), lineColor);
                }
            }

            if (lineCounter == 0)
            {
                if (settings.NothingToDisplayString != null)
                {
                    if (!player.RealPlayer)
                        player.SendSuccessMessage(settings.NothingToDisplayString);
                    else
                        player.SendMessage(settings.NothingToDisplayString, settings.HeaderTextColor);
                }
            }
            else if (settings.IncludeFooter && pageNumber + 1 <= pageCount)
            {
                if (!player.RealPlayer)
                    player.SendInfoMessage(string.Format(settings.FooterFormat, pageNumber + 1, pageNumber, pageCount));
                else
                    player.SendMessage(string.Format(settings.FooterFormat, pageNumber + 1, pageNumber, pageCount),
                        settings.FooterTextColor);
            }
        }

        public static void SendPage(TSPlayer player, int pageNumber, Dictionary<string, int> dataToPaginate,
            FormatSettings settings = null)
        {
            SendPage(player, pageNumber, dataToPaginate, dataToPaginate.Count, settings);
        }

        public static bool TryParsePageNumber(List<string> commandParameters, int expectedParameterIndex,
            TSPlayer errorMessageReceiver, out int pageNumber)
        {
            pageNumber = 1;
            if (commandParameters.Count <= expectedParameterIndex)
                return true;

            string pageNumberRaw = commandParameters[expectedParameterIndex];
            if (!int.TryParse(pageNumberRaw, out pageNumber) || pageNumber < 1)
            {
                if (errorMessageReceiver != null)
                    errorMessageReceiver.SendErrorMessage("\"{0}\" is not a valid page number.", pageNumberRaw);

                pageNumber = 1;
                return false;
            }

            return true;
        }
    }
}