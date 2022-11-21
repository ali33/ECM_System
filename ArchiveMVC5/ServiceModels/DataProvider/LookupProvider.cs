using System.Collections.Generic;
using Ecm.Domain;
using System.Data;
using System;

namespace ArchiveMVC5.Models.DataProvider
{
    public class LookupProvider : ProviderBase
    {

        public LookupProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        public DataTable GetLookupData(LookupInfoModel lookupInfoModel, string value)
        {
            using (var client = GetArchiveClientChannel())
            {
                LookupInfo lookupInfo = ObjectMapper.GetLookupInfo(lookupInfoModel);
                return client.Channel.GetLookupData(lookupInfo, value);
            }
        }

        public bool TestConnection(LookupConnectionModel connectionInfo)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.TestConnection(connection);
            }
        }

        public bool TestQueryParam(string query, List<string> fieldNames)
        {
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.TestQueryParam(query, fieldNames);
            }
        }

        public List<string> GetDatabaseName(LookupConnectionModel connectionInfo)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.GetDatabaseNames(connection);
            }
        }

        public List<string> GetSchemas(LookupConnectionModel connectionInfo)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.GetSchemas(connection);
            }
        }

        public List<string> GetTableNames(LookupConnectionModel connectionInfo)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.GetTableNames(connection);
            }
        }

        public List<string> GetViewNames(LookupConnectionModel connectionInfo)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.GetViewNames(connection);
            }
        }

        public List<string> GetStoredProcedureNames(LookupConnectionModel connectionInfo)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
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
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.GetRuntimeValueParams(sqlCommand);
            }
        }

        public DataTable GetParameters(LookupConnectionModel connectionInfo, string storedName)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.GetParameterNames(connection, storedName);
            }
        }

        public Dictionary<string, string> GetColumns(LookupConnectionModel connectionInfo, string sourceName, LookupDataSourceType sourceType)
        {
            using (var client = GetArchiveClientChannel())
            {
                var connection = ObjectMapper.GetConnectionInfo(connectionInfo);
                return client.Channel.GetColummnNames(connection, sourceName, sourceType);
            }
        }

        public LookupInfo GetLookupInfo(Guid fieldId)
        {
            using (var client = GetArchiveClientChannel())
            {
                return client.Channel.GetLookupInfo(fieldId);
            }
        }
    }
}
