using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the relationship between the <see cref="User"/> and <see cref="UserGroup"/>
    /// </summary>
    [DataContract]
    public sealed class Membership
    {
        /// <summary>
        /// Identifier of the relationship
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="UserGroup"/>
        /// </summary>
        [DataMember]
        public Guid UserGroupId { get; set; }

        /// <summary>
        /// Identifier of the <see cref="User"/>
        /// </summary>
        [DataMember]
        public Guid UserId { get; set; }
    }
}