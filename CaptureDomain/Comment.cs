using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class Comment
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid InstanceId { get; set; }

        [DataMember]
        public bool IsBatchId { get; set; }

        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public byte[] Photo { get; set; }

        [DataMember]
        public string BatchName { get; set; }
    }

    [DataContract]
    public class ListCommentOptimize
    {
        [DataMember]
        public Dictionary<string, string> Photos { get; set; }

        [DataMember]
        public List<Comment> Comments { get; set; }
    }


}
