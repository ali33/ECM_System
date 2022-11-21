using System.Collections.Generic;
using Ecm.CaptureDomain;
using System;

namespace Ecm.CaptureModel.DataProvider
{
    public class ActionLogProvider : ProviderBase
    {
        public ActionLogModel GetActionLog(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetActionLogModel(client.Channel.GetActionLog(id));
            }
        }

        public List<ActionLogModel> GetActionLog(int index, int pageSize, out long totalRow)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetActionLogs(index, pageSize, out totalRow));
            }
        }

        public List<ActionLogModel> GetActionLogAll()
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetActionLogAll());
            }
        }

        public List<ActionLogModel> SearchActionLog(string expression, int index, int pageSize, out long totalRow)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.SearchActionLogs(expression, index, pageSize, out totalRow));
            }
        }

        public List<ActionLogModel> SearchActionLog(string expression)
        {
            using (var client = GetCaptureClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.SearchActionLogs(expression));
            }
        }

        public void DeleteLog(Guid id)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.DeleteLog(id);
            }
        }

        public void AddLog(string message, ActionName actionName, ObjectType type, Guid objectId)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.AddActionLog(message, actionName, type, objectId);
            }
        }
    }
}
