using System.Collections.Generic;
using Ecm.Domain;
using System.Data;
using System;

namespace Ecm.Model.DataProvider
{
    public class LookupProvider : ProviderBase
    {
        //public IDictionary<string, LookupDataSourceType> GetDataSources(string connetionString, LookupDataSourceType type, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetDataSource(connetionString, type, dataProvider);
        //    }
        //}

        //public DataTable GetParameters(string connectionString, string storedName, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetParameters(connectionString, storedName, dataProvider);
        //    }
        //}

        //public IDictionary<string, string> GetColumns(string sourceName, string connectionString, LookupDataSourceType type, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetColumns(sourceName, connectionString, type, dataProvider);
        //    }
        //}

        //public bool TestConnection(string connectionString, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.TestConnection(connectionString, dataProvider);
        //    }
        //}

        //public List<string> GetDatabaseNames(string connectionString, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetDatabaseNames(connectionString, dataProvider);
        //    }
        //}

        //public DataTable GetLookupData(FieldMetaDataModel fieldMetaData, string fieldValue)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetLookupData(ObjectMapper.GetFieldMetaData(fieldMetaData), fieldValue);
        //    }
        //}

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



        //private LookupInfo GetLookupInfo(LookupInfoModel model)
        //{
        //    if (model == null)
        //    {
        //        return null;
        //    }

        //    return new LookupInfo
        //    {
        //        ConnectionInfo = GetConnectionInfo(model.Connection),
        //        FieldId = model.FieldId,
        //        LookupMapping = model.FieldMappings.Select(p => p.FieldName).ToList(),
        //        LookupObjectName = model.SourceName,
        //        LookupType = (LookupType)model.LookupType,
        //        LookupWhenTabOut = model.LookupAtLostFocus,
        //        MinPrefixLength = model.MinPrefixLength,
        //        QueryCommand = model.SqlCommand,
        //        RuntimeMappingInfo = model.FieldMappings.Select(p => p.FieldName).ToList()
        //    };

        //}
    }
}
