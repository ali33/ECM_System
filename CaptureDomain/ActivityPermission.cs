using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ecm.CaptureDomain
{
    [Serializable()]
    public class ActivityPermission
    {
        public ActivityPermission()
        {
            UserGroupPermissions = new List<UserGroupPermission>();
            AnnotationPermissions = new List<AnnotationPermission>();
            DocumentPermissions = new List<DocumentPermission>();
        }

        [XmlArray("UserGroupPermissions"), XmlArrayItem(typeof(UserGroupPermission), ElementName = "HumanStepUserGroupPermission")]
        public List<UserGroupPermission> UserGroupPermissions { get; set; }

        [XmlArray("AnnotationPermissions"), XmlArrayItem(typeof(AnnotationPermission), ElementName = "AnnotationPermission")]
        public List<AnnotationPermission> AnnotationPermissions { get; set; }

        [XmlArray("DocumentPermissions"), XmlArrayItem(typeof(DocumentPermission), ElementName = "DocumentPermission")]
        public List<DocumentPermission> DocumentPermissions { get; set; }
    }
}
