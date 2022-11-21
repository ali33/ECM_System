using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the permission on <see cref="Annotation"/> of a <see cref="UserGroup"/> on a <see cref="DocumentType"/>.
    /// </summary>
    [DataContract]
    public class AnnotationPermission
    {
        /// <summary>
        /// Identifier of the permission.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the document type.
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Identifier of the user group.
        /// </summary>
        [DataMember]
        public Guid UserGroupId { get; set; }

        /// <summary>
        /// Allow user can view the Text <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedSeeText { get; set; }

        /// <summary>
        /// Allow user can create the Text <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedAddText { get; set; }

        /// <summary>
        /// Allow user can delete the Text <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedDeleteText { get; set; }

        /// <summary>
        /// Allow user can see the Highlight <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedSeeHighlight { get; set; }

        /// <summary>
        /// Allow user can create the Highlight <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedAddHighlight { get; set; }

        /// <summary>
        /// Allow user can delete the Highlight <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedDeleteHighlight { get; set; }

        /// <summary>
        /// Allow user can hide the Redaction <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedHideRedaction { get; set; }

        /// <summary>
        /// Allow user can create the Redaction <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedAddRedaction { get; set; }

        /// <summary>
        /// Allow user can delete the Redaction <see cref="Annotation"/>
        /// </summary>
        [DataMember]
        public bool AllowedDeleteRedaction { get; set; }

        /// <summary>
        /// Internal use only.
        /// </summary>
        public static AnnotationPermission GetAllowAll()
        {
            return new AnnotationPermission
            {
                AllowedAddHighlight = true,
                AllowedAddRedaction = true,
                AllowedAddText = true,
                AllowedDeleteHighlight = true,
                AllowedDeleteRedaction = true,
                AllowedDeleteText = true,
                AllowedHideRedaction = true,
                AllowedSeeHighlight = true,
                AllowedSeeText = true
            };
        }
    }
}