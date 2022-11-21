using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.Utility
{
    public static class UrlKeywordReplace
    {
        public static string GenerateUrlData(string text)
        {
            string returnText = text.Replace("?", "question");
            returnText = text.Replace("&", "and");
            returnText = text.Replace("=", "equal");

            return returnText;
        }

        public static string GenerateText(string urlData)
        {
            string returnText = urlData.Replace("question", "?");
            returnText = urlData.Replace("and", "&");
            returnText = urlData.Replace("equal", "=");

            return returnText;
        }
    }
}
