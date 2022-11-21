namespace Ecm.ContentViewer.BarcodeHelper
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public class BarcodeConfiguration
    {
        public List<BarcodeAction> BarcodeActions { get; set; }

        public string ContentTypeName { get; set; }
    }
}

