using System.Collections.Generic;
using System.Xml.Serialization;
using Ecm.Model;

namespace Ecm.DocViewer.Model
{
    public class RecoveryDoc
    {
        public DocumentModel DocumentData { get; set; }

        [XmlArray("Pages")]
        [XmlArrayItem("Page")]
        public List<RecoveryPage> Pages { get; set; }
    }
}
