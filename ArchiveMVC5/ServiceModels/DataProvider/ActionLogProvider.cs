using System.Collections.Generic;
using Ecm.Domain;
using System;

namespace ArchiveMVC5.Models.DataProvider
{
    public class ActionLogProvider : ProviderBase
    {

        public ActionLogProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// Lấy thông tin ActionLog theo ID
        /// </summary>
        /// <param name="id">id ActionLog cần lấy</param>
        /// <returns>ActionLogModel</returns>
        public ActionLogModel GetActionLog(Guid id)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModel(client.Channel.GetActionLog(id));
            }
        }

        /// <summary>
        /// Lấy danh sách thông tin ActionLog theo index và pageSize
        /// </summary>
        /// <param name="index">index ActionLog cần lấy</param>
        /// <param name="pageSize">pageSize Actionlog cần lấy</param>
        /// <param name="totalRow">totalRow Actionlog cần lấy</param>
        /// <returns></returns>
        public List<ActionLogModel> GetActionLog(int index, int pageSize, out long totalRow)
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetActionLogs(index, pageSize, out totalRow));
            }
        }
        /// <summary>
        /// Lấy tất cả ActionLog
        /// </summary>
        /// <returns></returns>
        public List<ActionLogModel> GetActionLogAll()
        {
            using (var client = GetArchiveClientChannel())
            {
                return ObjectMapper.GetActionLogModels(client.Channel.GetActionLogAll());
            }
        }
        /// <summary>
        /// Tìm kiếm các ActionLog theo expression, index, pageSize
        /// </summary>
        /// <param name="expression">Diễn tả ActionLog cần tìm</param>
        /// <param name="index">index Actionlog cần tìm</param>
        /// <param name="pageSize">pageSize Actionlog cần tìm</param>
        /// <param name="totalRow"></param>
        /// <returns></returns>
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
