using System.Collections.Generic;
using System.Xml.Serialization;
using Ecm.CaptureModel;

namespace Ecm.CaptureViewer.Model
{
    [XmlRoot("RecoveryDoc")]
    public class RecoveryDoc
    {
        public DocumentModel DocumentData { get; set; }

        [XmlArray("Pages"), XmlArrayItem(typeof(RecoveryPage), ElementName = "Pages")]
        public List<RecoveryPage> Pages { get; set; }
    }
}
