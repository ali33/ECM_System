using System;
using System.Xml.Serialization;
using Ecm.BarcodeDomain;

namespace Ecm.CaptureBarcodeProcessing
{
    [XmlType("BarcodeAction")]
    public class BarcodeAction
    {
        public int BarcodePositionInDoc { get; set; }

        public string BarcodeStartsWith { get; set; }

        public BarcodeType BarcodeType { get; set; }

        public string CopyValueToFieldName { get; set; }

        public bool HasDoLookup { get; set; }

        public bool IsDocumentSeparator { get; set; }

        public bool RemoveDocumentSeparator { get; set; }
    }
}

