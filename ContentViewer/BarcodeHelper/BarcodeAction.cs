using System;
using System.Xml.Serialization;
using Ecm.ContentViewer.Model;

namespace Ecm.ContentViewer.BarcodeHelper
{
    public class BarcodeAction
    {
        public int BarcodePositionInDoc { get; set; }

        public string BarcodeStartsWith { get; set; }

        public BarcodeTypeModel BarcodeType { get; set; }

        public string CopyValueToFieldName { get; set; }

        public bool HasDoLookup { get; set; }

        public bool IsDocumentSeparator { get; set; }

        public bool RemoveDocumentSeparator { get; set; }
    }
}

