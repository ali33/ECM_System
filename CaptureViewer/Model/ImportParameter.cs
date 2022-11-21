using System.Collections.Generic;
using Ecm.CaptureModel;

namespace Ecm.CaptureViewer.Model
{
    internal class ImportParameter
    {
        public EnterContentAction Action { get; set; }
        public string[] SourceFiles { get; set; }
        public List<string> SplitedPageFiles { get; set; }
        public List<FileTypeModel> SplitedPageTypes { get; set; } 
        public object Param1 { get; set; }
        public object Param2 { get; set; }
    }
}
