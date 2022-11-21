using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ecm.ContentViewer.Model
{
    public class RecoveryDoc
    {
        public ContentModel DocumentData { get; set; }

        [XmlArray("Pages")]
        [XmlArrayItem("Page")]
        public List<RecoveryPage> Pages { get; set; }
    }
}
