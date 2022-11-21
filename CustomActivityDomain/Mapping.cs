using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("ReleaseInfo")]
    public class Mapping
    {
        public Guid ReleaseDocumentTypeId { get; set; }

        public Guid CaptureDocumentTypeId { get; set; }

        [XmlArray("FieldMaps"), XmlArrayItem(typeof(MappingField), ElementName = "MappingField")]
        public List<MappingField> FieldMaps { get; set; }
    }
}
