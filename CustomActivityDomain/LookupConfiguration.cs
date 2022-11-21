using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    public class LookupConfigurationInfo
    {
        public List<LookupInfo> BatchFieldLookupInfo { get; set; }

        public List<DocumentLookupInfo> DocumentFieldLookupInfo { get; set; }
    }
}
