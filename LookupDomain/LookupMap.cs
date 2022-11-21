using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.LookupDomain
{
    [DataContract]
    [Serializable()]
    [XmlRoot("Lookup")]
    public class LookupMap: ICloneable
    {
        [DataMember]
        public Guid FieldId { get; set; }

        [DataMember]
        public string FieldName
        {
            get;
            set;
        }

        [DataMember]
        public string DataColumn
        {
            get;
            set;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
