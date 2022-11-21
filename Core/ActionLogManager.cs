using System;
using System.Collections.Generic;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.Domain;
using System.Configuration;
using System.Linq;
using System.Security;
using Ecm.SecurityDao;

namespace Ecm.Core
{
    public class ActionLogManager : ManagerBase
    {
        public ActionLogManager(User loginUser) : base(loginUser)
        {
        }

        public ActionLog GetActionLog(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!CheckViewLogPermission(dataContext))
                {
                    return null;
                }

                ActionLog actionLog = new ActionLogDao(dataContext).GetById(id);

                AssignUserToActionLogs(new List<ActionLog> { actionLog });

                return actionLog;
            }
        }

        public List<ActionLog> GetActionLogs(int index, int pageSize, out long totalRow)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!CheckViewLogPermission(dataContext))
                {
                    totalRow = 0;
                    return null;
                }

                List<ActionLog> actionLogs = new ActionLogDao(dataContext).GetActionLogs(index, pageSize, out totalRow);
                AssignUserToActionLogs(actionLogs);

                return actionLogs;
            }
        }

        public List<ActionLog> GetActionLogs()
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!CheckViewLogPermission(dataContext))
                {
                    return null;
                }

                List<ActionLog> actionLogs = new ActionLogDao(dataContext).GetAll();
                AssignUserToActionLogs(actionLogs);
                return actionLogs;
            }

        }

        public List<ActionLog> SearchActionLogs(string expression, int index, int pageSize, out long totalRow)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!CheckViewLogPermission(dataContext))
                {
                    totalRow = 0;
                    return null;
                }

                List<ActionLog> actionLogs;
                if (!string.IsNullOrEmpty(expression))
                {
                    actionLogs = new ActionLogDao(dataContext).SearchActionLogs(expression, index, pageSize, out totalRow);
                }
                else
                {
                    actionLogs = new ActionLogDao(dataContext).GetActionLogs(index, pageSize, out totalRow);
                }

                AssignUserToActionLogs(actionLogs);
                return actionLogs;
            }
        }

        public List<ActionLog> SearchActionLogs(string expression)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!CheckViewLogPermission(dataContext))
                {
                    return null;
                }

                List<ActionLog> actionLogs;
                if (!string.IsNullOrEmpty(expression))
                {
                    actionLogs = new ActionLogDao(dataContext).SearchActionLogs(expression);
                }
                else
                {
                    actionLogs = new ActionLogDao(dataContext).GetAll();
                }

                AssignUserToActionLogs(actionLogs);
                return actionLogs;
            }
        }

        public void AddLog(string logMessage, User user, ActionName actionName, ObjectType? objectType, Guid? objectId)
        {
            var log = new ActionLog
            {
                ActionName = GetActionName(actionName),
                IpAddress = user.ClientHost,
                LoggedDate = DateTime.Now,
                Message = logMessage
            };

            if (objectId != null)
            {
                log.ObjectId = objectId.Value;
            }

            if (objectType != null)
            {
                log.ObjectType = objectType.Value == ObjectType.Document ? "Document" : "Page";
            }

            log.Username = user.UserName;
            
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                new ActionLogDao(dataContext).Add(log);
            }
        }

        public void DeleteLog(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                CheckDeleteLogPermission(dataContext);
                new ActionLogDao(dataContext).Delete(id);
            }
        }

        public Document GetLogDocument(Guid documentId, ActionName actionNameEnum)
        {
            throw new NotImplementedException();
        }

        public Page GetLogPage(Guid pageId, ActionName actionNameEnum)
        {
            throw new NotImplementedException();
        }

        private string GetActionName(ActionName actionEnum)
        {
            switch (actionEnum)
            {
                case ActionName.GetUser:
                    return "GetUser";
                case ActionName.AddUser:
                    return "AddUser";
                case ActionName.UpdateUser:
                    return "UpdateUser";
                case ActionName.DeleteUser:
                    return "DeleteUser";
                case ActionName.ChangePassword:
                    return "ChangePassword";
                case ActionName.GetUserGroup:
                    return "GetUserGroup";
                case ActionName.AddUserGroup:
                    return "AddUserGroup";
                case ActionName.UpdateUserGroup:
                    return "UpdateUserGroup";
                case ActionName.DeleteUserGroup:
                    return "DeleteUserGroup";
                case ActionName.AddMemberShip:
                    return "AddMemberShip";
                case ActionName.UpdateMembership:
                    return "UpdateMemberShip";
                case ActionName.DeleteMemberShip:
                    return "DeleteMemberShip";
                case ActionName.GetDocumentType:
                    return "GetDocumentType";
                case ActionName.AddDocumentType:
                    return "AddDocumentType";
                case ActionName.UpdateDocumentType:
                    return "UpdateDocumentType";
                case ActionName.DeleteDocumentType:
                    return "DeleteDocumentType";
                case ActionName.GetFieldMetaData:
                    return "GetFieldMetaData";
                case ActionName.AddFieldMetaData:
                    return "AddFieldMetaData";
                case ActionName.UpdateFieldMetaData:
                    return "UpdateFieldMetaData";
                case ActionName.DeleteFieldMetaData:
                    return "DeleteFieldMetaData";
                case ActionName.GetFieldValue:
                    return "GetFieldValue";
                case ActionName.AddFieldValue:
                    return "AddFieldValue";
                case ActionName.UpdateFieldValue:
                    return "UpdateFieldValue";
                case ActionName.DeleteFieldValue:
                    return "DeleteFieldValue";
                case ActionName.DoSearchDocument:
                    return "DoSearchDocument";
                case ActionName.DoGlobalSearchDocument:
                    return "DoGlobalSearchDocument";
                case ActionName.ViewDocument:
                    return "ViewDocument";
                case ActionName.GetDocument:
                    return "GetDocument";
                case ActionName.AddDocument:
                    return "AddDocument";
                case ActionName.UpdateDocument:
                    return "UpdateDocument";
                case ActionName.DeleteDocument:
                    return "DeleteDocument";
                case ActionName.ViewPage:
                    return "ViewPage";
                case ActionName.ReplacePage:
                    return "ReplacePage";
                case ActionName.InsertPage:
                    return "InsertPage";
                case ActionName.UpdatePage:
                    return "UpdatePage";
                case ActionName.RotatePage:
                    return "RotatePage";
                case ActionName.DeletePage:
                    return "DeletePage";
                case ActionName.AddAnnotation:
                    return "AddAnnotation";
                case ActionName.UpdateAnnotation:
                    return "UpdateAnnotation";
                case ActionName.DeleteAnnotation:
                    return "DeleteAnnotation";
                case ActionName.SendEmail:
                    return "SendMail";
                case ActionName.Print:
                    return "Print";
                case ActionName.Login:
                    return "Login";
                case ActionName.Save:
                    return "Save";
                case ActionName.GetDocumentTypePermission:
                    return "GetDocumentTypePermission";
                case ActionName.AddDocumentTypePermission:
                    return "AddDocumentTypePermission";
                case ActionName.DeleteDocumentTypePermission:
                    return "DeleteDocumentTypePermission";
                case ActionName.UpdateDocumentTypePermission:
                    return "UpdateDocumentTypePermission";
                case ActionName.GetAnnotationPermission:
                    return "GetAnnotationPermission";
                case ActionName.AddAnnotationPermission:
                    return "AddAnnotationPermission";
                case ActionName.DeleteAnnotationPermission:
                    return "DeleteAnnotationPermission";
                case ActionName.UpdateAnnotationPermission:
                    return "UpdateAnnotationPermission";
                case ActionName.AddLookupMapping:
                    return "AddLookupMapping";
                case ActionName.UpdateLookupMapping:
                    return "UpdateLookupMapping";
                case ActionName.DeleteLookupMapping:
                    return "DeleteLookupMapping";
                case ActionName.AddLookupInfo:
                    return "AddLookupInfo";
                case ActionName.UpdateLookupInfo:
                    return "UpdateLookupInfo";
                case ActionName.DeleteLookupInfo:
                    return "DeleteLookupInfo";
                case ActionName.AddPicklist:
                    return "AddPicklist";
                case ActionName.DeletePicklist:
                    return "DeletePicklist";
                case ActionName.UpdatePicklist:
                    return "UpdatePicklist";
                default:
                    return actionEnum.ToString();
            }
        }

        public List<ActionLog> GetLogByDoc(Guid docId)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!CheckViewLogPermission(dataContext))
                {
                    return null;
                }

                List<ActionLog> actionLogs;
                Document document = new DocumentDao(dataContext).GetById(docId);
                if (document != null)
                {
                    actionLogs = new ActionLogDao(dataContext).GetByDocument(docId);
                }
                else
                {
                    actionLogs = new ActionLogDao(dataContext).GetByDeletedDocument(docId);
                }

                AssignUserToActionLogs(actionLogs);

                return actionLogs;
            }
        }

        private void AssignUserToActionLogs(IEnumerable<ActionLog> actionLogs)
        {
            using (Ecm.Context.DapperContext primaryDbContext = new Ecm.Context.DapperContext())
            {
                UserPrimaryDao primaryUserDao = new UserPrimaryDao(primaryDbContext);
                foreach (ActionLog actionLog in actionLogs)
                {
                    actionLog.User = GetUser(primaryUserDao.GetByUserName(actionLog.Username));
                }
            }
        }

        private bool CheckViewLogPermission(DapperContext context)
        {
            if (!LoginUser.IsAdmin)
            {
                AuditPermissionDao auditPermissionDao = new AuditPermissionDao(context);
                //TODO:[Trac] need to review, I think we should not get all the records here but 1 only
                List<Guid> groupIds = null;

                using (Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                }

                List<AuditPermission> auditPermissions = auditPermissionDao.GetByUser(groupIds);

                if (auditPermissions.Any(p => p.AllowedViewLog))
                {
                    return true;
                }

            }
            else
            {
                return true;
            }

            return false;
        }

        private void CheckDeleteLogPermission(DapperContext context)
        {
            if (!LoginUser.IsAdmin)
            {
                //TODO:[Trac] need to review, I think we should not get all the records here but 1 only
                List<Guid> groupIds = null;

                using (Context.DapperContext primaryContext = new Context.DapperContext())
                {
                    groupIds = new UserGroupDao(primaryContext).GetByUser(LoginUser.Id).Select(p => p.Id).ToList();
                }

                AuditPermissionDao auditPermissionDao = new AuditPermissionDao(context);
                List<AuditPermission> auditPermissions = auditPermissionDao.GetByUser(groupIds);

                if (!auditPermissions.Any(p => p.AllowedDeleteLog))
                {
                    throw new SecurityException(string.Format("User {0} doesn't have permission to delete action log.", LoginUser.UserName));
                }
            }
        }
    }
}