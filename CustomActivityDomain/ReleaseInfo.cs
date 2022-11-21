using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    public class ReleaseInfo
    {
        public LoginInfo LoginInfo { get; set; }

        [XmlArray("MappingInfos"), XmlArrayItem(typeof(Mapping), ElementName = "Mapping")]
        public List<Mapping> MappingInfos { get; set; }

    }
}
