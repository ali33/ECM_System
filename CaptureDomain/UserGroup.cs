using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the user group in the system
    /// </summary>
    [DataContract]
    public class UserGroup
    {
        /// <summary>
        /// Initialize new object of the user group
        /// </summary>
        public UserGroup()
        {
            Users = new List<User>();
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
        public List<User> Users { get; set; }
    }
}