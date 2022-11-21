using System.Collections.Generic;
using System.Security;
using System.Linq;
using Ecm.Domain;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.SecurityDao;
using System;

namespace Ecm.Core
{
    public class PermissionManager : ManagerBase
    {
        public PermissionManager(User loginUser) : base (loginUser)
        {
        }

        public List<DocumentType> GetDocTypes()
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new DocTypeDao(dataContext).GetAll();
            }
        }


        public List<UserGroup> GetUserGroups()
        {
            using (Ecm.Context.DapperContext dataContext = new Ecm.Context.DapperContext())
            {
                return GetUserGroups(new UserGroupDao(dataContext).GetAll());
            }
        }

        public DocumentTypePermission GetDocTypePermision(UserGroup userGroup, DocumentType docType)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new DocumentTypePermissionDao(dataContext).GetDocTypePermission(docType.Id, userGroup.Id);
            }
        }

        public void SavePermission(DocumentTypePermission documentTypePermission, AnnotationPermission annotationPermission, AuditPermission auditPermission)
        {
            if (!LoginUser.IsAdmin)
            {
                throw new SecurityException(string.Format("User {0} doesn't have permission to save document permission.", LoginUser.UserName));
            }

            if (documentTypePermission == null || annotationPermission == null || auditPermission == null)
            {
                return;
            }

            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                try
                {
                    DocumentType docType = new DocTypeDao(dataContext).GetById(documentTypePermission.DocTypeId);
                    UserGroup group;
                    using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                    {
                        group = GetUserGroup(new UserGroupDao(primaryContext).GetById(documentTypePermission.UserGroupId));
                    }

                    dataContext.BeginTransaction();
                    SaveDocTypePermission(documentTypePermission, docType, group, dataContext, new DocumentTypePermissionDao(dataContext));
                    SaveAnnotationPermission(annotationPermission, docType, group, dataContext, new AnnotationPermissionDao(dataContext));
                    SaveAuditPermission(auditPermission, docType, group, dataContext, new AuditPermissionDao(dataContext));
                    dataContext.Commit();
                }
                catch
                {
                    dataContext.Rollback();
                    throw;
                }
            }
        }

        public DocumentTypePermission GetDocTypePermissionForUser(User user, DocumentType docType)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                List<Guid> groupIds = null;
                using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    groupIds = new UserGroupDao(primaryContext).GetByUser(user.Id).Select(p => p.Id).ToList();
                }

                List<DocumentTypePermission> permissions = new DocumentTypePermissionDao(dataContext).GetByUser(groupIds, docType.Id);
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
                        permission.AllowedAppendPage |= per.AllowedAppendPage;
                        permission.AllowedCapture |= per.AllowedCapture;
                        permission.AllowedDeletePage |= per.AllowedDeletePage;
                        permission.AllowedDownloadOffline |= per.AllowedDownloadOffline;
                        permission.AllowedEmailDocument |= per.AllowedEmailDocument;
                        permission.AllowedExportFieldValue |= per.AllowedExportFieldValue;
                        permission.AllowedHideAllAnnotation |= per.AllowedHideAllAnnotation;
                        permission.AllowedReplacePage |= per.AllowedReplacePage;
                        permission.AllowedRotatePage |= per.AllowedRotatePage;
                        permission.AllowedSearch |= per.AllowedSearch;
                        permission.AllowedSeeRetrictedField |= per.AllowedSeeRetrictedField;
                        permission.AllowedUpdateFieldValue |= per.AllowedUpdateFieldValue;
                        permission.AlowedPrintDocument |= per.AlowedPrintDocument;
                    }
                }

                return permission;
            }
        }

        public AnnotationPermission GetAnnotationPermission(UserGroup userGroup, DocumentType docType)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new AnnotationPermissionDao(dataContext).GetAnnotationPermission(docType.Id, userGroup.Id);
            }
        }

        public AnnotationPermission GetAnnotationPermissionForUser(User user, DocumentType docType)
        {
            using (DapperContext dataContext = new DapperContext(user))
            {
                List<Guid> groupIds = null;
                using (Ecm.Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    groupIds = new UserGroupDao(primaryContext).GetByUser(user.Id).Select(p => p.Id).ToList();
                }

                List<AnnotationPermission> permissions = new AnnotationPermissionDao(dataContext).GetByUser(docType.Id, groupIds);
                AnnotationPermission permission = new AnnotationPermission();
                for (int i = 0; i < permissions.Count; i++)
                {
                    var per = permissions[i];
                    if (i == 0)
                    {
                        permission = per;
                    }
                    else
                    {
                        permission.AllowedAddHighlight |= per.AllowedAddHighlight;
                        permission.AllowedAddRedaction |= per.AllowedAddRedaction;
                        permission.AllowedAddText |= per.AllowedAddText;
                        permission.AllowedDeleteHighlight |= per.AllowedDeleteHighlight;
                        permission.AllowedDeleteRedaction |= per.AllowedDeleteRedaction;
                        permission.AllowedDeleteText |= per.AllowedDeleteText;
                        permission.AllowedHideRedaction |= per.AllowedHideRedaction;
                        permission.AllowedSeeHighlight |= per.AllowedSeeHighlight;
                        permission.AllowedSeeText |= per.AllowedSeeText;
                    }
                }

                return permission;
            }
        }

        public AuditPermission GetAuditPermission(UserGroup userGroup, DocumentType docType)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                return new AuditPermissionDao(dataContext).GetAuditPermission(docType.Id, userGroup.Id);
            }
        }

        public AuditPermission GetAuditPermission(User user)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                List<Guid> groupIds = null;
                using (Context.DapperContext primaryContent = new Context.DapperContext())
                {
                    UserGroupDao userGroupDao = new UserGroupDao(primaryContent);
                    groupIds = userGroupDao.GetByUser(user.Id).Select(p => p.Id).ToList();
                }

                List<AuditPermission> auditPermissions = new AuditPermissionDao(dataContext).GetByUser(groupIds);
                AuditPermission auditPermission = new AuditPermission();

                for (var i = 0; i < auditPermissions.Count; i++)
                {
                    var auditPer = auditPermissions[i];
                    if (i == 0)
                    {
                        auditPermission = auditPer;
                    }
                    else
                    {
                        auditPermission.AllowedAudit |= auditPer.AllowedAudit;
                        auditPermission.AllowedDeleteLog |= auditPer.AllowedDeleteLog;
                        auditPermission.AllowedRestoreDocument |= auditPer.AllowedRestoreDocument;
                        auditPermission.AllowedViewLog |= auditPer.AllowedViewLog;
                        auditPermission.AllowedViewReport = auditPer.AllowedViewReport;
                    }
                }

                return auditPermission;
            }
        }

        private void SaveAnnotationPermission(AnnotationPermission permission, DocumentType docType, UserGroup group,
            DapperContext dataContext, AnnotationPermissionDao annotationPermissionDao)
        {
            if (permission.Id == Guid.Empty)
            {
                annotationPermissionDao.Add(permission);
                ActionLogHelper.AddActionLog("Add annotation permission for document type name " + docType.Name + " and user group name " + group.Name + " successfully", LoginUser, ActionName.GetAnnotationPermission, null, null, dataContext);
            }
            else
            {
                if (!permission.AllowedAddHighlight && !permission.AllowedAddRedaction && !permission.AllowedAddText && !permission.AllowedDeleteHighlight &&
                    !permission.AllowedDeleteRedaction && !permission.AllowedDeleteText && !permission.AllowedHideRedaction && !permission.AllowedSeeHighlight &&
                    !permission.AllowedSeeText)
                {
                    annotationPermissionDao.Delete(permission.Id);
                    ActionLogHelper.AddActionLog("Delete annotation permission for document type name " + docType.Name + " and user group name " + group.Name + " successfully", LoginUser, ActionName.DeleteAnnotationPermission, null, null, dataContext);
                }
                else
                {
                    annotationPermissionDao.Update(permission);
                    ActionLogHelper.AddActionLog("Update annotation permission for document type name " + docType.Name + " and user group name " + group.Name + " successfully", LoginUser, ActionName.UpdateAnnotationPermission, null, null, dataContext);
                }
            }
        }

        private void SaveDocTypePermission(DocumentTypePermission permission, DocumentType docType, UserGroup group,
            DapperContext dataContext, DocumentTypePermissionDao docTypePermissionDao)
        {
            if (permission.Id == Guid.Empty)
            {
                docTypePermissionDao.Add(permission);
                ActionLogHelper.AddActionLog("Add permission for document type name " + docType.Name + " and user group name " + group.Name + " successfully", LoginUser, ActionName.AddDocumentTypePermission, null, null, dataContext);
            }
            else
            {
                if (!permission.AllowedAppendPage && !permission.AllowedCapture && !permission.AllowedDeletePage &&
                    !permission.AllowedDownloadOffline && !permission.AllowedEmailDocument && !permission.AllowedExportFieldValue &&
                    !permission.AllowedHideAllAnnotation && !permission.AllowedReplacePage && !permission.AllowedRotatePage &&
                    !permission.AllowedSearch && !permission.AllowedSeeRetrictedField && !permission.AllowedUpdateFieldValue &&
                    !permission.AlowedPrintDocument)
                {
                    docTypePermissionDao.Delete(permission.Id);
                    ActionLogHelper.AddActionLog("Delete permission for document type name " + docType.Name + " and user group name " + group.Name + " successfully", LoginUser, ActionName.DeleteDocumentTypePermission, null, null, dataContext);
                }
                else
                {
                    docTypePermissionDao.Update(permission);
                    ActionLogHelper.AddActionLog("Update permission for document type name " + docType.Name + " and user group name " + group.Name + " successfully", LoginUser, ActionName.UpdateDocumentTypePermission, null, null, dataContext);
                }
            }
        }

        private void SaveAuditPermission(AuditPermission auditPermission, DocumentType docType, UserGroup userGroup, DapperContext dataContext,
            AuditPermissionDao auditPermissionDao)
        {
            if (auditPermission.Id == Guid.Empty)
            {
                auditPermissionDao.Add(auditPermission);
                ActionLogHelper.AddActionLog("Add audit permission for document type name " + docType.Name + " and user group name " + userGroup.Name + " successfully", LoginUser, ActionName.AddAuditPermission, null, null, dataContext);
            }
            else
            {
                if (!auditPermission.AllowedAudit && !auditPermission.AllowedDeleteLog && !auditPermission.AllowedRestoreDocument &&
                    !auditPermission.AllowedViewLog && !auditPermission.AllowedViewReport)
                {
                    auditPermissionDao.Delete(auditPermission.Id);
                    ActionLogHelper.AddActionLog("Delete audit permission from document type name " + docType.Name +" and user group name " + userGroup.Name + " successfully", LoginUser, ActionName.DeleteAuditPermission, null, null, dataContext);
                }
                else
                {
                    auditPermissionDao.Update(auditPermission);
                    ActionLogHelper.AddActionLog("Update audit permission for document type name " + docType.Name + " and user group name " + userGroup.Name + " successfully", LoginUser, ActionName.UpdateAuditPermission, null, null, dataContext);
                }
            }
        }
    }
}