using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ecm.BarcodeDomain
{
    [DataContract]
    public class CopyValueToField
    {
        [DataMember]
        public string FieldGuid { get; set; }
        [DataMember]
        public string FieldName { get; set; }
        [DataMember]
        public int Position { get; set; }
    }

}
