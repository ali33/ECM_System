using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.SecurityDao.Domain
{
    /// <summary>
    /// Represent the user group in the system
    /// </summary>
    [DataContract]
    public class PrimaryUserGroup
    {
        /// <summary>
        /// Initialize new object of the user group
        /// </summary>
        public PrimaryUserGroup()
        {
            Users = new List<PrimaryUser>();
        }

        /// <summary>
        /// Identifier of this user group
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Name of this user group
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// There are 2 types of user group: Built-in and Active Directory
        /// </summary>
        [DataMember]
        public int Type { get; set; }

        /// <summary>
        /// Collection of users belong to this user group
        /// </summary>
        [DataMember]
        public List<PrimaryUser> Users { get; set; }
    }
}