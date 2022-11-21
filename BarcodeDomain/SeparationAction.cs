using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ecm.BarcodeDomain
{
    [DataContract]
    public class SeparationAction
    {
        [DataMember]
        public int BarcodePositionInDoc { get; set; }
        [DataMember]
        public int BarcodeType { get; set; }
        [DataMember]
        public Guid DocTypeId { get; set; }
        [DataMember]
        public bool HasSpecifyDocumentType { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public bool RemoveSeparatorPage { get; set; }
        [DataMember]
        public string StartsWith { get; set; }
    }

}
