using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("Mapping")]
    public class MappingField
    {
        public Guid CaptureFieldId { get; set; }

        public Guid ArchiveFieldId { get; set; }

        public List<MappingField> MappingFields { get; set; }
    }
}
