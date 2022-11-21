using System.Collections.Generic;
using System.Xml.Serialization;
using Ecm.CaptureModel;

namespace Ecm.CaptureViewer.Model
{
    [XmlRoot("Recovery")]
    public class RecoveryBatch
    {
        public BatchModel BatchData { get; set; }

        public int Version { get; set; }

        [XmlArray("Documents"), XmlArrayItem(typeof(RecoveryDoc), ElementName = "RecoveryDoc")]
        public List<RecoveryDoc> Documents { get; set; }

        [XmlArray("Pages"), XmlArrayItem(typeof(RecoveryPage), ElementName = "Pages")]
        public List<RecoveryPage> Pages { get; set; }
    }
}
