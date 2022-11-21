using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class BatchInfoMobile
    {
        public BatchInfoMobile()
        {
            Documents = new List<DocumentInfoMobile>();
            FieldValues = new List<BatchFieldValue>();
            Comments = new List<Comment>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string BatchName { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }

        [DataMember]
        public string BatchTypeName { get; set; }

        [DataMember]
        public int DocCount { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }

        [DataMember]
        public BatchPermission BatchPermission { get; set; }

        [DataMember]
        public List<DocumentInfoMobile> Documents { get; set; }

        [DataMember]
        public List<BatchFieldValue> FieldValues { get; set; }

        [DataMember]
        public List<Comment> Comments { get; set; }


    }
}
