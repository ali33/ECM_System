namespace Ecm.CaptureBarcodeProcessing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlType("BarcodeConfiguration")]
    public class BarcodeConfiguration
    {
        [XmlArrayItem("BarcodeAction"), XmlArray("BarcodeActions")]
        public List<BarcodeAction> BarcodeActions { get; set; }

        public string DocTypeName { get; set; }
    }
}

