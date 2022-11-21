
using Ecm.Domain;
using System.Collections.Generic;

namespace ArchiveMVC5.Models.DataProvider
{
    public class PermissionProvider : ProviderBase
    {
        public PermissionProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// lấy DocType
        /// </summary>
        /// <returns></returns>
        public List<DocumentTypeModel> GetDocTypes()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypeModels(client.Channel.GetDocTypesUnderPermissionConfiguration());
            }
        }
        /// <summary>
        /// lấy UserGroup
        /// </summary>
        /// <returns></returns>
        public List<UserGroupModel> GetUserGroups()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetUserGroupModels(client.Channel.GetUserGroupsUnderPermissionConfiguration());
            }
        }
        /// <summary>
        /// lấy Permission từ UserGroupModel, DocumentTypeModel
        /// </summary>
        /// <param name="group">UserGroupModel</param>
        /// <param name="docType">DocumentTypeModel</param>
        /// <returns></returns>
        public DocumentTypePermissionModel GetPermission(UserGroupModel group, DocumentTypeModel docType)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypePermissionModel(client.Channel.GetDocTypePermission(ObjectMapper.GetUserGroup(group), ObjectMapper.GetDocumentType(docType)));
            }
        }


        /// <summary>
        /// lưu Permission
        /// </summary>
        /// <param name="permissionModel">DocumentTypePermissionModel</param>
        /// <param name="annotationPermissionModel">AnnotationPermissionModel</param>
        /// <param name="auditPermissionModel">AuditPermissionModel</param>
        public void SavePermission(DocumentTypePermissionModel permissionModel, AnnotationPermissionModel annotationPermissionModel, AuditPermissionModel auditPermissionModel) 
        {
            using (var client = GetArchiveClientChannel())
            {
                DocumentTypePermission permission = ObjectMapper.GetDocumentTypePermission(permissionModel);
                AnnotationPermission annotationPermission = ObjectMapper.GetAnnotationPermission(annotationPermissionModel);
                AuditPermission auditPermission = ObjectMapper.GetAuditPermission(auditPermissionModel);
                client.Channel.SavePermission(permission, annotationPermission, auditPermission);
            }
        }

        /// <summary>
        /// lấy AnnotationPermission từ UserGroupModel,DocumentTypeModel
        /// </summary>
        /// <param name="userGroup">UserGroupModel</param>
        /// <param name="docType">DocumentTypeModel</param>
        /// <returns></returns>
        public AnnotationPermissionModel GetAnnotationPermission(UserGroupModel userGroup, DocumentTypeModel docType)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAnnotationPermissionModel(client.Channel.GetAnnotationPermission(ObjectMapper.GetUserGroup(userGroup), ObjectMapper.GetDocumentType(docType)));
            }
        }


        /// <summary>
        /// lấy AuditPermission từ UserGroupModel,DocumentTypeModel
        /// </summary>
        /// <param name="userGroup">UserGroupModel</param>
        /// <param name="docType">DocumentTypeModel</param>
        /// <returns></returns>
        public AuditPermissionModel GetAuditPermission(UserGroupModel userGroup, DocumentTypeModel docType)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAuditPermissionModel(client.Channel.GetAuditPermission(ObjectMapper.GetUserGroup(userGroup), ObjectMapper.GetDocumentType(docType)));
            }
        }
        /// <summary>
        ///lấy AuditPermission từ UserModel
        /// </summary>
        /// <param name="user">UserModel</param>
        /// <returns></returns>
        public AuditPermissionModel GetAuditPermissionByUser(UserModel user)
        {
            if (user.IsAdmin)
            {
                return new AuditPermissionModel
                {
                    AllowedAudit = true,
                    AllowedDeleteLog = true,
                    AllowedRestoreDocument = true,
                    AllowedViewLog = true,
                    AllowedViewReport = true
                };
            }

            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAuditPermissionModel(client.Channel.GetAuditPermissionByUser(ObjectMapper.GetUser(user)));
            }
        }
    }
}
