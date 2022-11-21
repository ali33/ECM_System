using System;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;
using System.Collections.Generic;
using Ecm.SecurityDao;

namespace Ecm.CaptureCore
{
    public class ActionLogManager : ManagerBase
    {
        public ActionLogManager(User loginUser) : base(loginUser)
        {
        }

        public void AddLog(string logMessage, User user, ActionName actionName, ObjectType? objectType, Guid? objectId, DapperContext context)
        {
            var log = new ActionLog
            {
                Id = Guid.NewGuid(),
                ActionName = GetActionName(actionName),
                IpAddress = user.ClientHost,
                LoggedDate = DateTime.Now,
                Message = logMessage,
                Username = user.UserName
            };

            if (objectId != null)
            {
                log.ObjectId = objectId.Value;
            }

            if (objectType != null)
            {
                log.ObjectType = GetTypeName(objectType.Value);
            }
            
            new ActionLogDao(context).Add(log);
        }

        public void AddLog(string logMessage, User user, ActionName actionName, ObjectType? objectType, Guid? objectId)
        {
            var log = new ActionLog
            {
                Id = Guid.NewGuid(),
                ActionName = GetActionName(actionName),
                IpAddress = user.ClientHost,
                LoggedDate = DateTime.Now,
                Message = logMessage,
                Username = user.UserName
            };

            if (objectId != null)
            {
                log.ObjectId = objectId.Value;
            }

            if (objectType != null)
            {
                log.ObjectType = GetTypeName(objectType.Value);
            }

            using (DapperContext context = new DapperContext(LoginUser.CaptureConnectionString))
            {
                try
                {
                    context.BeginTransaction();
                    new ActionLogDao(context).Add(log);
                    context.Commit();
                }
                catch
                {
                    context.Rollback();
                }
            }
        }

        public ActionLog GetActionLog(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                ActionLog actionLog = new ActionLogDao(dataContext).GetById(id);

                AssignUserToActionLogs(new List<ActionLog> { actionLog });

                return actionLog;
            }
        }

        public List<ActionLog> GetActionLogs(int index, int pageSize, out long totalRow)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                if (!LoginUser.IsAdmin)
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
                if (!LoginUser.IsAdmin)
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
                if (!LoginUser.IsAdmin)
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
                if (!LoginUser.IsAdmin)
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

        public void DeleteLog(Guid id)
        {
            using (DapperContext dataContext = new DapperContext(LoginUser))
            {
                new ActionLogDao(dataContext).Delete(id);
            }
        }

        private string GetTypeName(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Batch:
                    return "Batch";
                case ObjectType.Document:
                    return "Document";
                case ObjectType.Page:
                    return "Page";
            }

            return type.ToString();
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
                case ActionName.GetBatchType:
                    return "GetBatchType";
                case ActionName.CreateBatchType:
                    return "CreateBatchType";
                case ActionName.UpdateBatchType:
                    return "UpdateBatchType";
                case ActionName.DeleteBatchType:
                    return "DeleteBatchType";
                case ActionName.CreateBatch:
                    return "CreateBatch";
                case ActionName.UpdateBatch:
                    return "UpdateBatch";
                case ActionName.DeleteBatch:
                    return "DeleteBatch";
                case ActionName.ApprovedBatch:
                    return "ApproveBatch";
                case ActionName.RejectedBatch:
                    return "RejectBatch";
                case ActionName.DelegatedBatch:
                    return "DelegateBatch";
                case ActionName.LockedBatch:
                    return "LockBatch";
                case ActionName.UnlockedBatch:
                    return "UnLockBatch";
                case ActionName.ResumeBatch:
                    return "ResumeBatch";
                case ActionName.GetLockedBatch:
                    return "GetLockedBatch";
                case ActionName.GetProcessingBatch:
                    return "GetProcessingBatch";
                case ActionName.GetAwaitingBatch:
                    return "GetAwaitingBatch";
                case ActionName.GetErrorBatch:
                    return "GetErrorBatch";
                case ActionName.ReleaseBatch:
                    return "ReleaseBatch";
                case ActionName.ReleaseToArchive:
                    return "ReleaseToArchive";
                case ActionName.UpdateBatchAfterProcessBarcode:
                    return "UpdateBatchAfterProcessBarcode";
                case ActionName.CreateBatchFieldValue:
                    return "CreateBatchFieldValue";
                case ActionName.UpdateBatchFieldValue:
                    return "UpdateBatchFieldValue";
                case ActionName.DeleteBatchFieldValue:
                    return "DeleteBatchFieldValue";
                case ActionName.CreateBatchFieldMetaData:
                    return "CreateBatchFieldMetaData";
                case ActionName.UpdateBatchFieldMetaData:
                    return "UpdateBatchFieldMetaData";
                case ActionName.DeleteBatchFieldMetaData:
                    return "DeleteBatchFieldMetaData";
                case ActionName.GetBatchFieldMetaData:
                    return "GetBatchFieldMetaData";
                case ActionName.GetBatchFieldValue:
                    return "GetBatchFieldValue";
                case ActionName.AddComment:
                    return "AddComment";
                case ActionName.GetBatchItem:
                    return "GetBatchItem";
                case ActionName.GetBatchData:
                    return "GetBatchData";
                case ActionName.ProcessBarcode:
                    return "ProcessBarcode";
                case ActionName.SentNotify:
                    return "SentNotify";
                case ActionName.ReloadSearchResult:
                    return "ReloadSearchResult";
                default:
                    return actionEnum.ToString();
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

    }
}