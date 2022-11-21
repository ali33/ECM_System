using System.Collections.Generic;
using System.Linq;
using System.Security;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using Ecm.SecurityDao;
using System;

namespace Ecm.CaptureCore
{
    public class PermissionManager : ManagerBase
    {
        public PermissionManager(User loginUser) : base(loginUser)
        {
        }

        /// <summary>
        /// Use this method to get batch types only, not include other information
        /// </summary>
        /// <returns></returns>
        public List<BatchType> GetBatchTypes()
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new BatchTypeDao(dataContext).GetAll();
            }
        }

        public List<UserGroup> GetUserGroups()
        {
            using (Ecm.Context.DapperContext dataContext = new Ecm.Context.DapperContext())
            {
                return GetUserGroups(new UserGroupDao(dataContext).GetAll());
            }
        }

        public BatchTypePermission GetBatchTypePermissionByUser(Guid userId, Guid batchTypeId)
        {
            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {                
                List<Guid> userGroupIds = GetUserGroups(new UserGroupDao(primaryContext).GetByUser(userId)).Select(p=>p.Id).ToList();
                using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
                {
                    List<BatchTypePermission> permissions = new BatchTypePermissionDao(context).GetByUser(userGroupIds, batchTypeId);
                    BatchTypePermission permission = new BatchTypePermission();

                    for (int i = 0; i < permissions.Count; i++)
                    {
                        var per = permissions[i];
                        if (i == 0)
                        {
                            permission = per;
                        }
                        else
                        {
                            permission.CanCapture |= per.CanCapture;
                            permission.CanAccess |= per.CanAccess;
                            permission.CanClassify |= per.CanClassify;
                            permission.CanIndex |= per.CanIndex;
                        }
                    }

                    return permission;
                }
            }
        }

        public BatchTypePermission GetBatchPermision(Guid userGroupId, Guid batchTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new BatchTypePermissionDao(dataContext).GetBatchTypePermission(batchTypeId, userGroupId);
            }
        }

        public DocumentFieldPermission GetFieldPermission(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentFieldPermissionDao permissionDao = new DocumentFieldPermissionDao(dataContext);
                return permissionDao.GetFieldPermission(id);
            }
        }

        public List<DocumentFieldPermission> GetFieldPermission(Guid userGroupId, Guid docTypeId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                DocumentFieldPermissionDao permissionDao = new DocumentFieldPermissionDao(dataContext);
                return permissionDao.GetFieldPermissions(docTypeId, userGroupId);
            }
        }

        public void SavePermission(BatchTypePermission batchPermission, List<DocumentTypePermission> documentTypePermissions)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                try
                {
                    dataContext.BeginTransaction();
                    BatchTypePermissionDao batchTypePermissionDao = new BatchTypePermissionDao(dataContext);
                    DocumentFieldPermissionDao permissionDao = new DocumentFieldPermissionDao(dataContext);
                    DocumentTypePermissionDao docTypePermissionDao = new DocumentTypePermissionDao(dataContext);

                    SaveBatchPermission(batchPermission, batchTypePermissionDao);

                    SaveDocumentTypePermission(documentTypePermissions, docTypePermissionDao, permissionDao);

                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public void SaveBatchPermission(BatchTypePermission permission, BatchTypePermissionDao batchTypePermissionDao)
        {
            if (permission.Id == Guid.Empty)
            {
                batchTypePermissionDao.Add(permission);
            }
            else
            {
                batchTypePermissionDao.Update(permission);
            }
        }

        //public void SaveFieldPermission(List<DocumentFieldPermission> fieldPermissions, DocumentFieldPermissionDao permissionDao)
        //{
        //    foreach (DocumentFieldPermission fieldPermission in fieldPermissions)
        //    {
        //        if (fieldPermission.Id == Guid.Empty)
        //        {
        //            permissionDao.Add(fieldPermission);
        //        }
        //        else
        //        {
        //            permissionDao.Update(fieldPermission);
        //        }
        //    }
        //}

        public void SaveDocumentTypePermission(List<DocumentTypePermission> docTypePermissions, DocumentTypePermissionDao docTypePermissionDao, DocumentFieldPermissionDao fieldPermissionDao)
        {
            foreach (DocumentTypePermission permission in docTypePermissions)
            {
                var docTypePermission = docTypePermissionDao.GetById(permission.DocTypeId, permission.UserGroupId);

                if (docTypePermission == null)
                {
                    docTypePermissionDao.Insert(permission);
                }
                else
                {
                    docTypePermissionDao.Update(permission);
                }

                //foreach (DocumentFieldPermission fieldPermission in permission.FieldPermissions)
                //{
                //    if (fieldPermission.Id == Guid.Empty)
                //    {
                //        fieldPermissionDao.Add(fieldPermission);
                //    }
                //    else
                //    {
                //        fieldPermissionDao.Update(fieldPermission);
                //    }
                //}
            }
        }

        public DocumentTypePermission GetDocumentTypePermissionByUser(Guid userId, Guid documentTypeId)
        {
            using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
            {
                List<Guid> userGroupIds = GetUserGroups(new UserGroupDao(primaryContext).GetByUser(userId)).Select(p => p.Id).ToList();
                using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
                {
                    List<DocumentTypePermission> permissions = new DocumentTypePermissionDao(context).GetByGroupRangeAndDocType(userGroupIds, documentTypeId);
                    DocumentTypePermission permission = new DocumentTypePermission();

                    for (int i = 0; i < permissions.Count; i++)
                    {
                        var per = permissions[i];
                        if (i == 0)
                        {
                            permission = per;
                        }
                        else
                        {
                            permission.CanAccess |= per.CanAccess;
                        }
                    }

                    return permission;
                }
            }
        }

        public DocumentTypePermission GetDocumentTypePermission(Guid userGroupId, Guid documentTypeId)
        {
            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                DocumentTypePermission permission = new DocumentTypePermissionDao(context).GetById(documentTypeId, userGroupId);
                return permission;
            }
        }
    }
}
