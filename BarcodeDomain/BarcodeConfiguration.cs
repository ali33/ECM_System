using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ecm.BarcodeDomain
{
    [DataContract]
    public class BatchBarcodeConfiguration
    {
        [DataMember]
        public List<ReadAction> ReadActions { get; set; }
        [DataMember]
        public List<SeparationAction> SeparationActions { get; set; }

        [DataMember]
        public bool TransferBarcodeToClientProcessing { get; set; }
    }

}
