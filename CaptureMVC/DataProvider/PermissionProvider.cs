
using Ecm.CaptureDomain;
using Ecm.Utility;
using System.Collections.Generic;

namespace CaptureMVC.DataProvider
{
    public class PermissionProvider : ProviderBase
    {
        //public PermissionProvider(string userName, string password)
        //{
        //    Configure(userName, password);
        //}
        //<summary>
        //get DocType
        //</summary>
        //<returns></returns>
        public List<BatchType> GetBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetBatchTypesUnderPermissionConfiguration();
            }
        }
        /// <summary>
        /// get UserGroup
        /// </summary>
        /// <returns></returns>
        public List<UserGroup> GetUserGroups()
        {
            using (var client = GetCaptureClientChannel())
            {
                return client.Channel.GetUserGroupsUnderPermissionConfiguration();
            }
        }
        /// <summary>
        ///// get Permission from UserGroupModel, DocumentTypeModel
        ///// </summary>
        ///// <param name="group">UserGroupModel</param>
        ///// <param name="docType">DocumentTypeModel</param>
        ///// <returns></returns>
        //public DocumentTypePermission GetPermission(UserGroup group, DocumentType docType)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return client.Channel.GetDocumentTypePermission(group, docType);
        //    }
        //}

        ///// <summary>
        ///// save Permission
        ///// </summary>
        ///// <param name="permissionModel">DocumentTypePermissionModel</param>
        ///// <param name="annotationPermissionModel">AnnotationPermissionModel</param>
        ///// <param name="auditPermissionModel">AuditPermissionModel</param>
        //public void SavePermission(DocumentTypePermission permissionModel, AnnotationPermission annotationPermissionModel, AuditPermission auditPermissionModel)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        DocumentTypePermission permission = ObjectMapper.GetDocumentTypePermission(permissionModel);
        //        AnnotationPermission annotationPermission = ObjectMapper.GetAnnotationPermission(annotationPermissionModel);
        //        AuditPermission auditPermission = ObjectMapper.GetAuditPermission(auditPermissionModel);
        //        client.Channel.SavePermission(permission, annotationPermission, auditPermission);
        //    }
        //}

        ////<summary>
        ////get AnnotationPermission from UserGroupModel,DocumentTypeModel
        ////</summary>
        ////<param name="userGroup">UserGroupModel</param>
        ////<param name="docType">DocumentTypeModel</param>
        ////<returns></returns>
        //public AnnotationPermission GetAnnotationPermission(UserGroup userGroup, DocumentType docType)
        //{
        //    using (var client = GetCaptureClientChannel())
        //    {
        //        return client.Channel.GetAnnotationPermission(userGroup,docType);
        //    }
        //}

        ///// <summary>
        /////lấy AuditPermission từ UserModel
        ///// </summary>
        ///// <param name="user">UserModel</param>
        ///// <returns></returns>
        //public AuditPermissionModel GetAuditPermissionByUser(UserModel user)
        //{
        //    if (user.IsAdmin)
        //    {
        //        return new AuditPermissionModel
        //        {
        //            AllowedAudit = true,
        //            AllowedDeleteLog = true,
        //            AllowedRestoreDocument = true,
        //            AllowedViewLog = true,
        //            AllowedViewReport = true
        //        };
        //    }

        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return ObjectMapper.GetAuditPermissionModel(client.Channel.GetAuditPermissionByUser(ObjectMapper.GetUser(user)));
        //    }
        //}
    }
}
