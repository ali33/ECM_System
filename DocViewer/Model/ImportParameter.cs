using System.Collections.Generic;
using Ecm.Model;

namespace Ecm.DocViewer.Model
{
    internal class ImportParameter
    {
        public EnterContentAction Action { get; set; }
        public string[] SourceFiles { get; set; }
        public Dictionary<string, string> SplitedPageFiles { get; set; }
        public List<FileTypeModel> SplitedPageTypes { get; set; } 
        public object Param1 { get; set; }
        public object Param2 { get; set; }
    }
}
