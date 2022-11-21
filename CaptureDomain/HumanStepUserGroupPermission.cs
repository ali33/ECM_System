using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the permission of each user group on a human step of the workflow.
    /// </summary>
    [DataContract]
    public class HumanStepUserGroupPermission : BaseHumanStepPermission
    {
        public HumanStepUserGroupPermission()
        {
            //DocTypePermissions = new List<BaseHumanStepDocTypePermission>();
        }

        /// <summary>
        /// Clone to another object.
        /// </summary>
        /// <returns>The <see cref="HumanStepUserGroupPermission"/> object.</returns>
        public HumanStepUserGroupPermission Clone()
        {
            HumanStepUserGroupPermission permission = new HumanStepUserGroupPermission
                                                          {
                                                              CanAnnotate = CanAnnotate,
                                                              CanDelete = CanDelete,
                                                              CanDownloadFilesOnDemand = CanDownloadFilesOnDemand,
                                                              CanEmail = CanEmail,
                                                              CanModifyDocument = CanModifyDocument,
                                                              CanModifyIndexes = CanModifyIndexes,
                                                              CanPrint = CanPrint,
                                                              CanReject = CanReject,
                                                              CanReleaseLoosePage = CanReleaseLoosePage,
                                                              CanSendLink = CanSendLink,
                                                              CanViewOtherItems = CanViewOtherItems,
                                                              UserGroupId = UserGroupId
                                                          };

            //DocTypePermissions.ForEach(p => permission.DocTypePermissions.Add(p.Clone()));

            return permission;
        }

        /// <summary>
        /// Identifier of the user group is assigned permission
        /// </summary>
        [DataMember]
        public Guid UserGroupId { get; set; }

        //[DataMember]
        //public List<BaseHumanStepDocTypePermission> DocTypePermissions { get; set; }
    }

    [DataContract]
    public class BaseHumanStepDocTypePermission
    {
        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public string DocTypeName { get; set; }

        [DataMember]
        public bool CanAccess { get; set; }

        [DataMember]
        public bool CanSeeRestrictedField { get; set; }

        public BaseHumanStepDocTypePermission Clone()
        {
            return new BaseHumanStepDocTypePermission
                       {
                           DocTypeId = DocTypeId,
                           CanAccess = CanAccess,
                           CanSeeRestrictedField = CanSeeRestrictedField
                       };
        }
    }

    /// <summary>
    /// Represent the detail of permission
    /// </summary>
    [DataContract]
    public class BaseHumanStepPermission
    {
        /// <summary>
        /// Allow user to modify the documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanModifyDocument { get; set; }

        /// <summary>
        /// Allow user to modify the field value of documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanModifyIndexes { get; set; }

        /// <summary>
        /// Allow user to delete the documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanDelete { get; set; }

        /// <summary>
        /// Allow user to add annotations on documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanAnnotate { get; set; }

        /// <summary>
        /// Allow user to print documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanPrint { get; set; }

        /// <summary>
        /// Allow user to send email documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanEmail { get; set; }

        /// <summary>
        /// Allow user to send email of the links to documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanSendLink { get; set; }

        /// <summary>
        /// Allow user to download binary of pages in documents on demand
        /// </summary>
        [DataMember]
        public bool CanDownloadFilesOnDemand { get; set; }

        /// <summary>
        /// Allow user to release loose pages when doing capture
        /// </summary>
        [DataMember]
        public bool CanReleaseLoosePage { get; set; }

        /// <summary>
        /// Allow user to reject documents are waiting on a human step
        /// </summary>
        [DataMember]
        public bool CanReject { get; set; }

        /// <summary>
        /// Allow user to see work items of other users on a human step
        /// </summary>
        [DataMember]
        public bool CanViewOtherItems { get; set; }

        /// <summary>
        /// Allow user to delegate work items for another user action
        /// </summary>
        [DataMember]
        public bool CanDelegateItems { get; set; }
    }

}
