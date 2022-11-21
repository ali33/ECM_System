using System.Collections.Generic;
using Ecm.Domain;
using System;

namespace Ecm.Model.DataProvider
{
    public class ActionLogProvider : ProviderBase
    {
        public ActionLogModel GetActionLog(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModel(client.Channel.GetActionLog(id));
            }
        }

        public List<ActionLogModel> GetActionLog(int index, int pageSize, out long totalRow)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetActionLogs(index, pageSize, out totalRow));
            }
        }

        public List<ActionLogModel> GetActionLogAll()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetActionLogAll());
            }
        }

        public List<ActionLogModel> SearchActionLog(string expression, int index, int pageSize, out long totalRow)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.SearchActionLogs(expression, index, pageSize, out totalRow));
            }
        }

        public List<ActionLogModel> SearchActionLog(string expression)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.SearchActionLogs(expression));
            }
        }

        public List<ActionLogModel> GetActionLogByDocument(Guid docId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetLogByDocument(docId));
            }
        }

        public void DeleteLog(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.DeleteLog(id);
            }
        }

        public void AddLog(string message, ActionName actionName, ObjectType type, Guid objectId)
        {
            using (var client = GetArchiveClientChannel())
            {
                client.Channel.AddActionLog(message, actionName, type, objectId);
            }
        }
    }
}
