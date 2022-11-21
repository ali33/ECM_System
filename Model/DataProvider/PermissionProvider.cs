using System.Collections.ObjectModel;
using Ecm.Domain;

namespace Ecm.Model.DataProvider
{
    public class PermissionProvider : ProviderBase
    {
        public ObservableCollection<DocumentTypeModel> GetDocTypes()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypeModels(client.Channel.GetDocTypesUnderPermissionConfiguration());
            }
        }

        public ObservableCollection<UserGroupModel> GetUserGroups()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetUserGroupModels(client.Channel.GetUserGroupsUnderPermissionConfiguration());
            }
        }

        public DocumentTypePermissionModel GetPermission(UserGroupModel group, DocumentTypeModel docType)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetDocumentTypePermissionModel(client.Channel.GetDocTypePermission(ObjectMapper.GetUserGroup(group), ObjectMapper.GetDocumentType(docType)));
            }
        }

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

        public AnnotationPermissionModel GetAnnotationPermission(UserGroupModel userGroup, DocumentTypeModel docType)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAnnotationPermissionModel(client.Channel.GetAnnotationPermission(ObjectMapper.GetUserGroup(userGroup), ObjectMapper.GetDocumentType(docType)));
            }
        }

        public AuditPermissionModel GetAuditPermission(UserGroupModel userGroup, DocumentTypeModel docType)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetAuditPermissionModel(client.Channel.GetAuditPermission(ObjectMapper.GetUserGroup(userGroup), ObjectMapper.GetDocumentType(docType)));
            }
        }

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
