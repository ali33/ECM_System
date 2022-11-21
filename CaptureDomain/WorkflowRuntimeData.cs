using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the data the workflow engine use to transfer between steps in the workflow. This class is used internally
    /// </summary>
    [DataContract]
    public class WorkflowRuntimeData
    {
        /// <summary>
        /// Identifier of the object: Document or Batch
        /// </summary>
        [DataMember]
        public object ObjectID { get; set; }

        /// <summary>
        /// Document or batch the workflow is applied 
        /// </summary>
        [DataMember]
        public WorkflowObjectType ObjectType { get; set; }

        /// <summary>
        /// Identifier of the <see cref="User"/> the workflow engine use to run the workflow
        /// </summary>
        [DataMember]
        public User User { get; set; }

        ///// <summary>
        ///// The user name of the <see cref="User"/> the workflow engine use to run the workflow
        ///// </summary>
        //[DataMember]
        //public string UserName { get; set; }

        ///// <summary>
        ///// The hashed password of the <see cref="User"/>
        ///// </summary>
        //[DataMember]
        //public string PasswordHash { get; set; }

        ///// <summary>
        ///// The IP address of the <see cref="User"/>
        ///// </summary>
        //[DataMember]
        //public string IPAddress { get; set; }

        ///// <summary>
        ///// Connection string of <see cref="User"/>. This property is used internally
        ///// </summary>
        //[DataMember]
        //public string UserConnectionstring { get; set; }
        ///// <summary>
        ///// Whether this user is administrator or not
        ///// </summary>
        //[DataMember]
        //public bool IsAdmin { get; set; }


    }
}