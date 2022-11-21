using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ecm.ContentViewer.Model
{
    [XmlType("Batch")]
    public class RecoveryBatch
    {
        public BatchModel BatchData { get; set; }

        public int Version { get; set; }

        [XmlArray("Docs")]
        [XmlArrayItem("Doc")]
        public List<RecoveryDoc> Documents { get; set; }

        [XmlArray("LoosePages")]
        [XmlArrayItem("LoosePage")]
        public List<RecoveryPage> Pages { get; set; }
    }
}
