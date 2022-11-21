using System;
using System.Runtime.Serialization;
namespace Ecm.Domain
{
    [DataContract]
    public class LookupParameter
    {
        [DataMember]
        private string _parameterValue;

        [DataMember]
        public string ParameterName { get; set; }

        [DataMember]
        public string ParameterType { get; set; }

        [DataMember]
        public string OrderIndex { get; set; }

        [DataMember]
        public string Mode { get; set; }

        [DataMember]
        public string ParameterValue { get; set; }
    
        [DataMember]
        public bool IsRequired { get; set; }
    }
}
