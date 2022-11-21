using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class BatchTypePermission
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }

        [DataMember]
        public bool CanCapture { get; set; }

        [DataMember]
        public bool CanAccess { get; set; }

        [DataMember]
        public bool CanIndex { get; set; }

        [DataMember]
        public bool CanClassify { get; set; }

        public static BatchTypePermission GetAllowAll()
        {
            return new BatchTypePermission
            {
                CanCapture = true,
                CanAccess = true,
                CanClassify = true, 
                CanIndex = true
            };
        }
    }
}
