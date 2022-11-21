using System;
using System.Runtime.Serialization;

namespace Ecm.SecurityDao.Domain
{
    /// <summary>
    /// Represent the relationship between the <see cref="PrimaryUser"/> and <see cref="PrimaryUserGroup"/>
    /// </summary>
    [DataContract]
    public sealed class PrimaryMembership
    {
        /// <summary>
        /// Identifier of the relationship
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="PrimaryUserGroup"/>
        /// </summary>
        [DataMember]
        public Guid UserGroupId { get; set; }

        /// <summary>
        /// Identifier of the <see cref="PrimaryUser"/>
        /// </summary>
        [DataMember]
        public Guid UserId { get; set; }
    }
}