using System.Collections.Generic;
using Ecm.Domain;
using System.Data;

namespace ArchiveMVC.Models.DataProvider
{
    public class LookupProvider : ProviderBase
    {

        public LookupProvider(string userName, string password)
        {
            Configure(userName, password);
        }

        ///// <summary>
        ///// Lấy DataSources theo connetionString, type, dataProvider
        ///// </summary>
        ///// <param name="connetionString">connectionString của DataSource cần lấy</param>
        ///// <param name="type">type của DataSource cần lấy </param>
        ///// <param name="dataProvider">dataProvider của DataSource cần lấy</param>
        ///// <returns></returns>
        //public IDictionary<string, LookupDataSourceType> GetDataSources(string connetionString, LookupDataSourceType type, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetDataSource(connetionString, type, dataProvider);
        //    }
        //}


        ///// <summary>
        ///// Lấy Parameters theo connectionString, storedName, dataProvider
        ///// </summary>
        ///// <param name="connectionString">connectionString để lấy Parameters</param>
        ///// <param name="storedName">storedName để lấy Parameters</param>
        ///// <param name="dataProvider">dataProvider để lấy Parameters</param>
        ///// <returns></returns>
        //public DataTable GetParameters(string connectionString, string storedName, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetParameters(connectionString, storedName, dataProvider);
        //    }
        //}



        ///// <summary>
        ///// Lấy số cột theo sourceName, connectionString, type, dataProvider
        ///// </summary>
        ///// <param name="sourceName">sourceName để lấy số cột</param>
        ///// <param name="connectionString">connectionString để lấy số cột</param>
        ///// <param name="type">type để lấy số cột</param>
        ///// <param name="dataProvider">dataProvider để lấy số cột</param>
        ///// <returns></returns>
        //public IDictionary<string, string> GetColumns(string sourceName, string connectionString, LookupDataSourceType type, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetColumns(sourceName, connectionString, type, dataProvider);
        //    }
        //}


        ///// <summary>
        ///// Kiểm tra kết nối
        ///// </summary>
        ///// <param name="connectionString">connectionString để kiểm tra kết nối</param>
        ///// <param name="dataProvider">dataProvider để kiểm tra kết nối</param>
        ///// <returns></returns>
        //public bool TestConnection(string connectionString, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.TestConnection(connectionString, dataProvider);
        //    }
        //}

        ///// <summary>
        ///// Lấy tên cơ sở dữ liệu theo connectionString, dataProvider
        ///// </summary>
        ///// <param name="connectionString">chuỗi kết nối để lấy tên cơ sở dữ liệu</param>
        ///// <param name="dataProvider">dataProvider để kiểm tra kết nối</param>
        ///// <returns></returns>
        //public List<string> GetDatabaseNames(string connectionString, string dataProvider)
        //{
        //    using (var client = GetArchiveClientChannel())
        //    {
        //        return client.Channel.GetDatabaseNames(connectionString, dataProvider);
        //    }
        //}


        ///// <summary>
        ///// tìm kiếm dữ liệu theo fieldMetaData , fieldValue
        ///// </summary>
        ///// <param name="fieldMetaData">fieldMetaData để tìm dữ liệu</param>
        ///// <param name="fieldValue">fieldValue để tìm dữ liệu</param>
        ///// <returns></returns>
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
    }
}
