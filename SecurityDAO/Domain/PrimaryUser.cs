using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.SecurityDao.Domain
{
    /// <summary>
    /// Represent the user in the system
    /// </summary>
    [DataContract]
    public class PrimaryUser
    {
        /// <summary>
        /// Initialize new object for user
        /// </summary>
        public PrimaryUser()
        {
            UserGroups = new List<PrimaryUserGroup>();
        }

        /// <summary>
        /// Identifier of this user
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// User name of this user
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Password of this user
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Email of this user
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Full name of this user
        /// </summary>
        [DataMember]
        public string FullName { get; set; }

        /// <summary>
        /// There are 2 types of user: Built-in and Active Directory
        /// </summary>
        [DataMember]
        public int Type { get; set; }

        /// <summary>
        /// Whether this user is administrator or not
        /// </summary>
        [DataMember]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// This property is used internally for tracking the client host that user is using the system
        /// </summary>
        [DataMember]
        public string ClientHost { get; set; }

        /// <summary>
        /// Collection of user groups that this user belong to
        /// </summary>
        [DataMember]
        public List<PrimaryUserGroup> UserGroups { get; set; }

        /// <summary>
        /// Identifier of the language this user prefer to localization the system
        /// </summary>
        [DataMember]
        public Guid? LanguageId { get; set; }

        /// <summary>
        /// The language object
        /// </summary>
        [DataMember]
        public PrimaryLanguage Language { get; set; }

        /// <summary>
        /// Photo of the user
        /// </summary>
        [DataMember]
        public byte[] Photo { get; set; }

        /// <summary>
        /// This property is used internally
        /// </summary>
        [DataMember] 
        public string ArchiveConnectionString { get; set; }

        [DataMember] 
        public string CaptureConnectionString { get; set; }

    }
}