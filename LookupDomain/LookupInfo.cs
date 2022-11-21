using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.LookupDomain
{
    [DataContract]
    [XmlRoot]
    public class LookupInfo
    {
        [DataMember]
        [XmlElement]
        public Guid FieldId { get; set; }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public ConnectionInfo ConnectionInfo
        {
            get;
            set;
        }

        [DataMember(IsRequired = false)]
        [XmlElement]
        public string LookupObjectName
        {
            get;
            set;
        }

        [DataMember(IsRequired = false)]
        [XmlElement]
        public LookupType LookupType
        {
            get;
            set;
        }

        [DataMember(IsRequired = false)]
        [XmlElement]
        public string QueryCommand
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public int MinPrefixLength
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public List<string> LookupMapping
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public List<string> RuntimeMappingInfo
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public bool LookupWhenTabOut
        {
            get;
            set;
        }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public int MaxLookupRow { get; set; }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public List<LookupMap> LookupMaps { get; set; }

        [DataMember(IsRequired = true)]
        [XmlElement]
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Define the lookup column name (Table column that looked up by)
        /// </summary>
        [DataMember]
        [XmlElement]
        public string LookupColumn { get; set; }

        /// <summary>
        /// Define the lookup operator
        /// </summary>
        [DataMember]
        [XmlElement]
        public string LookupOperator { get; set; }
    }
}