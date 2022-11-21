using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class DocumentPermission
    {
        public DocumentPermission()
        {
            FieldPermissions = new List<DocumentFieldPermission>();
        }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }

        [DataMember]
        public bool CanSeeRestrictedField { get; set; }

        [DataMember]
        [XmlArray("FieldPermissions"), XmlArrayItem(typeof(DocumentFieldPermission), ElementName = "DocumentFieldPermission")]
        public List<DocumentFieldPermission> FieldPermissions { get; set; }

        public static DocumentPermission GetAll()
        {
            return new DocumentPermission
            {
                CanSeeRestrictedField = true
            };

        }
    }
}
