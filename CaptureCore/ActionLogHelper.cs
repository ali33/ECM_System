using System;
using Ecm.CaptureDAO;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDomain;

namespace Ecm.CaptureCore
{
    public class ActionLogHelper
    {
        public static void AddActionLog(string logMessage, User user, ActionName actionName, WorkflowObjectType? objectType, Guid? objectId, DapperContext context)
        {
            ActionLogDao actionLogDao = new ActionLogDao(context);
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

            actionLogDao.Add(log);
        }

        private static string GetTypeName(WorkflowObjectType type)
        {
            switch (type)
            {
                case WorkflowObjectType.Batch:
                    return "Batch";
                case WorkflowObjectType.Document:
                    return "Document";
            }

            return type.ToString();
        }

        private static string GetActionName(ActionName actionEnum)
        {
            switch (actionEnum)
            {
                case ActionName.GetUser:
                    return "GetUser";
                case ActionName.ChangePassword:
                    return "ChangePassword";
                default:
                    return actionEnum.ToString();
            }
        }
    }
}