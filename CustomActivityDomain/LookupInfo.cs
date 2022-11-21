using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    [Serializable()]
    [XmlRoot("LookupConfiguration")]
    public class LookupInfo
    {
        public LookupConnection LookupConnection { get; set; }

        public Guid FieldId { get; set; }

        public string SqlCommand
        {
            get;
            set;
        }

        public int LookupType
        {
            get;
            set;
        }

        public string SourceName
        {
            get;
            set;
        }

        public int MinPrefixLength
        {
            get;
            set;
        }

        public bool LookupAtLostFocus
        {
            get;
            set;
        }

        public int MaxLookupRow
        {
            get;
            set;
        }

        public bool ApplyClientLookup { get; set; }

        public string LookupColumn { get; set; }

        public string LookupOperator { get; set; }

        [XmlArray("Mappings"), XmlArrayItem(typeof(LookupMapping), ElementName = "LookupMapping")]
        public List<LookupMapping> Mappings { get; set; }

        [XmlArray("Parameters"), XmlArrayItem(typeof(LookupParameter), ElementName = "LookupParameter")]
        public List<LookupParameter> Parameters { get; set; }
    }
}
