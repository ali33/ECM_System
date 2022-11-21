using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;

namespace Ecm.ContentExtractor
{
    public class HtmlExtractor
    {
        private HtmlDocument html = new HtmlDocument();

        public string ExtractText(string htmlFile)
        {
            html.Load(htmlFile);
            return html.DocumentNode.InnerText;
        }

    }
}
