using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class DocumentInfoMobile
    {
        public DocumentInfoMobile()
        {
            Pages = new List<PageInfoMobile>();
            FieldValues = new List<DocumentFieldValue>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string DocName { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public string DocTypeName { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }

        [DataMember]
        public AnnotationPermission AnotationPermission { get; set; }

        [DataMember]
        public List<PageInfoMobile> Pages { get; set; }

        [DataMember]
        public Page FirstPage { get; set; }

        [DataMember]
        public List<DocumentFieldValue> FieldValues { get; set; }

    }
}
