using System;
using Ecm.DAO;
using Ecm.DAO.Context;
using Ecm.Domain;

namespace Ecm.Core
{
    public class ActionLogHelper
    {
        public static void AddActionLog(string logMessage, User user, ActionName actionName, ObjectType? objectType, Guid? objectId, DapperContext context)
        {
            try
            {
                //context.BeginTransaction();
                ActionLogDao actionLogDao = new ActionLogDao(context);
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
                log.User = user;
                actionLogDao.Add(log);
                //context.Commit();
            }
            catch (Exception)
            {
                //context.Rollback();
            }
        }

        public static string GetActionName(ActionName actionEnum)
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
    }
}