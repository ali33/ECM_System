using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class Membership
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }
    }
}
