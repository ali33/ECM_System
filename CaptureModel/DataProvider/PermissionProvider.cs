using System.Collections.ObjectModel;
using Ecm.CaptureDomain;
using System.Collections.Generic;
using System;

namespace Ecm.CaptureModel.DataProvider
{
    public class PermissionProvider : ProviderBase
    {
        public ObservableCollection<BatchTypeModel> GetBatchTypes()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchTypeModels(client.Channel.GetBatchTypesUnderPermissionConfiguration());
            }
        }

        public ObservableCollection<UserGroupModel> GetUserGroups()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetUserGroupModels(client.Channel.GetUserGroupsUnderPermissionConfiguration());
            }
        }

        public BatchTypePermissionModel GetBatchTypePermission(Guid groupId, Guid batchTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetBatchTypePermissionModel(client.Channel.GetBatchTypePermission(groupId, batchTypeId));
            }
        }

        public DocTypePermissionModel GetDocTypePermission(Guid groupId, Guid docTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetDocTypePermissionModel(client.Channel.GetDocumentTypePermission(groupId, docTypeId));
            }
        }

        public DocTypePermissionModel GetDocTypePermissionByUser(Guid userId, Guid docTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetDocTypePermissionModel(client.Channel.GetDocumentTypePermissionByUser(userId, docTypeId));
            }
        }

        public ObservableCollection<DocumentFieldPermissionModel> GetFieldPermission(Guid userGroupId, Guid docTypeId)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetFieldPermissionModels(client.Channel.GetFieldPermission(userGroupId, docTypeId));
            }
        }

        public void SaveBatchTypePermission(BatchTypePermissionModel permissionModel, List<DocTypePermissionModel> documentTypePermissions)
        {
            using (var client = GetCaptureClientChannel())
            {
                BatchTypePermission permission = ObjectMapper.GetBatchTypePermission(permissionModel);
                List<DocumentTypePermission> docTypePermissions = ObjectMapper.GetDocTypePermissions(documentTypePermissions);
                client.Channel.SavePermission(permission, docTypePermissions);
            }
        }
    }
}
