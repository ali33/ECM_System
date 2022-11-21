using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using System.Configuration;
using Ecm.LookupDomain;
using System.Data;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.CustomActivityModel.DataProvider
{
    public class LookupProvider
    {
        private static string _endpoint = ConfigurationManager.AppSettings["CaptureServiceUrl"].ToString();
        private const string noUser = "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A";

        private static ClientChannel<ICapture> GetOneTimeClientChannel(string username, string password)
        {
            return ChannelManager<ICapture>.Instance.GetChannel(OneTimeBinding.Instance, _endpoint, username, password);
        }

        private static ClientChannel<ICapture> GetOneTimeClientChannel()
        {
            return ChannelManager<ICapture>.Instance.GetChannel(OneTimeBinding.Instance, _endpoint, noUser, string.Empty);
        }

        public DataTable GetLookupData(LookupInfoModel lookupInfoModel, string value)
        {
            using (var client = GetOneTimeClientChannel())
            {
                LookupInfo lookupInfo = GetLookupInfo(lookupInfoModel);
                return client.Channel.GetLookupData(lookupInfo, value);
            }
        }

        public bool TestConnection(LookupConnectionModel connectionInfo)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.TestConnection(connection);
            }
        }

        public bool TestQueryParam(string query, List<string> fieldNames)
        {
            using (var client = GetOneTimeClientChannel())
            {
                return client.Channel.TestQueryParam(query, fieldNames);
            }
        }

        public List<string> GetDatabaseName(LookupConnectionModel connectionInfo)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetDatabaseNames(connection);
            }
        }

        public List<string> GetSchemas(LookupConnectionModel connectionInfo)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetSchemas(connection);
            }
        }

        public List<string> GetTableNames(LookupConnectionModel connectionInfo)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetTableNames(connection);
            }
        }

        public List<string> GetViewNames(LookupConnectionModel connectionInfo)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetViewNames(connection);
            }
        }

        public List<string> GetStoredProcedureNames(LookupConnectionModel connectionInfo)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetStoredProcedureNames(connection);
            }
        }

        public List<string> GetDataSources(LookupConnectionModel connectionInfo, LookupDataSourceType type)
        {
            switch (type)
            {
                case LookupDataSourceType.StoredProcedure:
                    return GetStoredProcedureNames(connectionInfo);
                case LookupDataSourceType.Table:
                    return GetTableNames(connectionInfo);
                case LookupDataSourceType.View:
                    return GetViewNames(connectionInfo);
                default:
                    return null;
            }
        }

        public List<string> GetRuntimeValueParams(string sqlCommand)
        {
            using (var client = GetOneTimeClientChannel())
            {
                return client.Channel.GetRuntimeValueParams(sqlCommand);
            }
        }

        public DataTable GetParameters(LookupConnectionModel connectionInfo, string storedName)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetParameterNames(connection, storedName);
            }
        }

        public Dictionary<string, string> GetColumns(LookupConnectionModel connectionInfo, string sourceName, LookupDataSourceType sourceType)
        {
            using (var client = GetOneTimeClientChannel())
            {
                var connection = GetConnectionInfo(connectionInfo);
                return client.Channel.GetColummnNames(connection, sourceName, sourceType);
            }
        }

        public void UpdateBatchLookup(Guid fieldId, string xml, User user, Guid? lookupActivityId)
        {
            using (var client = GetOneTimeClientChannel(user.UserName, user.EncryptedPassword))
            {
                client.Channel.UpdateBatchLookup(fieldId, xml, lookupActivityId);
            }
        }

        public void UpdateDocumentLookup(Guid fieldId, string xml, User user, Guid? lookupActivityId)
        {
            using (var client = GetOneTimeClientChannel(user.UserName, user.EncryptedPassword))
            {
                client.Channel.UpdateDocumentLookup(fieldId, xml, lookupActivityId);
            }
        }

        private ConnectionInfo GetConnectionInfo(LookupConnectionModel connectionInfo)
        {
            if (connectionInfo == null)
            {
                return null;
            }
            return new ConnectionInfo
                {
                    DatabaseName = connectionInfo.DatabaseName,
                    DbType = (DatabaseType)connectionInfo.DatabaseType,
                    Host = connectionInfo.Host,
                    Password = connectionInfo.Password,
                    Port = connectionInfo.Port,
                    ProviderType = (ProviderType)connectionInfo.ProviderType,
                    Schema = connectionInfo.Schema,
                    Username = connectionInfo.Username
                };

        }

        private LookupInfo GetLookupInfo(LookupInfoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new LookupInfo
            {
                ConnectionInfo = GetConnectionInfo(model.Connection),
                FieldId = model.FieldId,
                LookupMapping = model.FieldMappings.Select(p => p.FieldName).ToList(),
                LookupObjectName = model.SourceName,
                LookupType = (LookupType)model.LookupType,
                LookupWhenTabOut = model.LookupAtLostFocus,
                MinPrefixLength = model.MinPrefixLength,
                QueryCommand = model.SqlCommand,
                RuntimeMappingInfo = model.FieldMappings.Select(p => p.FieldName).ToList(),
                LookupColumn = model.LookupColumn,
                LookupOperator = model.LookupOperator,
                LookupMaps = model.FieldMappings.Where(h=> !string.IsNullOrWhiteSpace(h.DataColumn)).Select(h=> new LookupMap() {
                    DataColumn = h.DataColumn,
                    FieldId = h.FieldId,
                    FieldName = h.FieldName
                }).ToList()
            };

        }
    }
}
