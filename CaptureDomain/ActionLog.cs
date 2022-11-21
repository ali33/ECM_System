using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the log of an action that user manipulate on the CloudECM.
    /// </summary>
    [DataContract]
    public class ActionLog
    {
        /// <summary>
        /// The identifier of the log
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// The user name of user is manipulating.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// The current date that the action occurs
        /// </summary>
        [DataMember]
        public DateTime LoggedDate { get; set; }

        /// <summary>
        /// Name of the action
        /// </summary>
        [DataMember]
        public string ActionName { get; set; }

        /// <summary>
        /// Where the action occurs.
        /// </summary>
        [DataMember]
        public string IpAddress { get; set; }

        /// <summary>
        /// The custom message of the action
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Which object the action occurs. It can be occurred on <see cref="Document"/> or <see cref="Page"/> objects. This is used internally.
        /// </summary>
        [DataMember]
        public string ObjectType { get; set; }

        /// <summary>
        /// The identifier of the object that the action occurs. Mean the document identifier or page identifier. This is used internally.
        /// </summary>
        [DataMember]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// The user is doing the action.
        /// </summary>
        [DataMember]
        public User User { get; set; }
    }
}