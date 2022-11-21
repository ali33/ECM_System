using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class AnnotationPermission
    {
        // Methods
        public static AnnotationPermission GetAllowAll()
        {
            return new AnnotationPermission { CanAddHighlight = true, CanAddRedaction = true, CanAddText = true, CanDeleteHighlight = true, CanDeleteRedaction = true, CanDeleteText = true, CanHideRedaction = true, CanSeeHighlight = true, CanSeeText = true };
        }

        // Properties
        [DataMember]
        public bool CanAddHighlight { get; set; }

        [DataMember]
        public bool CanAddRedaction { get; set; }

        [DataMember]
        public bool CanAddText { get; set; }

        [DataMember]
        public bool CanDeleteHighlight { get; set; }

        [DataMember]
        public bool CanDeleteRedaction { get; set; }

        [DataMember]
        public bool CanDeleteText { get; set; }

        [DataMember]
        public bool CanHideRedaction { get; set; }

        [DataMember]
        public bool CanSeeHighlight { get; set; }

        [DataMember]
        public bool CanSeeText { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public Guid UserGroupId { get; set; }
    }

}
