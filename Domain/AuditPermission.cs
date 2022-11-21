using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the permission on Auditing module of a user group.
    /// </summary>
    [DataContract]
    public class AuditPermission
    {
        /// <summary>
        /// Identifier of the permission
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the document type.
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Identifier of the user group is assigned permission
        /// </summary>
        [DataMember]
        public Guid UserGroupId { get; set; }

        /// <summary>
        /// Allow user can see the history of documents
        /// </summary>
        [DataMember]
        public bool AllowedAudit { get; set; }

        /// <summary>
        /// Allow user can see the action log in the system.
        /// </summary>
        [DataMember]
        public bool AllowedViewLog { get; set; }

        /// <summary>
        /// Allow user can delete the action log.
        /// </summary>
        [DataMember]
        public bool AllowedDeleteLog { get; set; }

        /// <summary>
        /// Allow user can see the report of auditing.
        /// </summary>
        [DataMember]
        public bool AllowedViewReport { get; set; }

        /// <summary>
        /// Allow user can restore a document from a specific version
        /// </summary>
        [DataMember]
        public bool AllowedRestoreDocument { get; set; }
    }
}