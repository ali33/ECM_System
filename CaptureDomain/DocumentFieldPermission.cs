using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    [XmlRoot("DocumentTypePermission")]
    public class DocumentFieldPermission
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid FieldId { get; set; }

        //[DataMember]
        //public DocumentFieldMetaData Field { get; set; }

        //[DataMember]
        //public DocumentType DocumentType { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }

        [DataMember]
        public bool CanRead { get; set; }

        [DataMember]
        public bool CanWrite { get; set; }

        public static DocumentFieldPermission GetAll()
        {
            return new DocumentFieldPermission
            {
                CanRead = true,
                CanWrite = true
            };
        }
    }
}
