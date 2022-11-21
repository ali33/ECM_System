using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("LookupConfiguration")]
    public class DocumentLookupInfo
    {
        public Guid DocumentTypeId { get; set; }

        [XmlArray("LookupInfos"), XmlArrayItem(typeof(LookupInfo), ElementName = "LookupInfo")]
        public List<LookupInfo> LookupInfos
        {
            get;
            set;
        }
    }
}
