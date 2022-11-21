using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ecm.BarcodeDomain;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ZombieInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int TotalSightings { get; set; }
    }
}
