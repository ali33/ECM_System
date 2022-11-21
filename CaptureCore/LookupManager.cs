using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Ecm.CaptureDomain;
using Ecm.ServerLookupService;
using Ecm.Utility;
using Ecm.LookupDomain;
using System.IO;
using System.Data.Common;
using Ecm.CaptureDAO.Context;
using Ecm.CaptureDAO;

namespace Ecm.CaptureCore
{
    public class LookupManager : ManagerBase
    {
        //public LookupInfo GetLookupInfo(string userName, string lookupKey)
        //{

        //    string lookupXml = _appSettingManager.GetAppSettingValue(userName, lookupKey);
        //    XmlTextReader reader = new XmlTextReader(new StringReader(lookupXml));
        //    DBLookupInfo dbLookupInfo = (DBLookupInfo)_lookupSerializer.Deserialize(reader);
        //    dbLookupInfo.ConnectionInfo.Password = UtilsCryptography.DecryptUsingSymmetricAlgorithm(dbLookupInfo.ConnectionInfo.Password);

        //    return dbLookupInfo;
        //}

        //public LookupInfo GetLookupInfoMapping(string userName, string lookupKey)
        //{
        //    LookupInfo lookupInfo = new LookupInfo
        //    {
        //        LookupMapping = new List<string>(),
        //        MinPrefixLength = 3
        //    };

        //    DBLookupInfo dbLookupInfo = GetLookupInfo(userName, lookupKey);
        //    lookupInfo.MinPrefixLength = dbLookupInfo.MinPrefixLength;
        //    lookupInfo.LookupMapping = dbLookupInfo.LookupMapping;
        //    lookupInfo.RuntimeMappingInfo = dbLookupInfo.RuntimeMappingInfo;
        //    lookupInfo.LookupWhenTabOut = dbLookupInfo.LookupWhenTabOut;

        //    return lookupInfo;
        //}

        public DataTable GetLookupData(LookupInfo lookupInfo, Dictionary<string, string> mappingValue)
        {
            CommonValidator.CheckNull(lookupInfo);
            CommonValidator.CheckNull(mappingValue);
            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType);
            string query = lookupInfo.QueryCommand;
            foreach (KeyValuePair<string, string> value in mappingValue)
            {
                query = query.Replace(string.Format("<<{0}>>", value.Key), value.Value);
            }

            DataTable dataTable = dbService.GetDBExecute((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType,
                (ServerLookupService.ProviderType)lookupInfo.ConnectionInfo.ProviderType, lookupInfo.ConnectionInfo.DatabaseName,
                lookupInfo.ConnectionInfo.Username, lookupInfo.ConnectionInfo.Password, lookupInfo.ConnectionInfo.Host, lookupInfo.ConnectionInfo.Port).ExecuteQueryGetDataTable(query);
            dataTable.TableName = "Lookup";

            return dataTable;
        }

        public DataTable GetLookupData(LookupInfo lookupInfo, string lookupValue)
        {
            CommonValidator.CheckNull(lookupInfo);
            if (string.IsNullOrWhiteSpace(lookupValue))
            {
                lookupValue = string.Empty;
            }
            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType);
            string query = lookupInfo.QueryCommand;
            query = query.Replace("<<value>>", lookupValue);

            DataTable dataTable = dbService.GetDBExecute((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType,
                (ServerLookupService.ProviderType)lookupInfo.ConnectionInfo.ProviderType, lookupInfo.ConnectionInfo.DatabaseName,
                lookupInfo.ConnectionInfo.Username, lookupInfo.ConnectionInfo.Password, lookupInfo.ConnectionInfo.Host, lookupInfo.ConnectionInfo.Port).ExecuteQueryGetDataTable(query);

            if (dataTable == null)
            {
                return new DataTable { TableName = "Lookup" };
            }

            dataTable.TableName = "Lookup";

            DataTable result = new DataTable { TableName = "Lookup" };

            if (lookupInfo.LookupType == LookupDomain.LookupType.Stored)
            {
                DataTable lookupFilter = GetLookupDataFormStored(dataTable, lookupInfo, lookupValue);

                var mappingColumn = new Dictionary<int, int>();
                var index = 0;
                var indexFilter = 0;

                foreach (DataColumn column in lookupFilter.Columns)
                {
                    LookupMap map = lookupInfo.LookupMaps.FirstOrDefault(p => p.DataColumn == column.ColumnName);

                    if (map != null && !string.IsNullOrEmpty(map.FieldName))
                    {
                        result.Columns.Add(new DataColumn { ColumnName = map.FieldName });
                        mappingColumn.Add(index, indexFilter);
                        index++;
                    }

                    indexFilter++;
                }

                DataRow resultRow;
                int maxRow = Convert.ToInt32(lookupInfo.MaxLookupRow);

                if (lookupFilter.Rows.Count < maxRow || maxRow == 0)
                {
                    maxRow = lookupFilter.Rows.Count;
                }

                for (int j = 0; j < maxRow; j++)
                {
                    resultRow = result.NewRow();

                    for (int i = 0; i < result.Columns.Count; i++)
                    {
                        resultRow[i] = lookupFilter.Rows[j][mappingColumn[i]];
                    }

                    result.Rows.Add(resultRow);
                }

                return result;
            }

            return dataTable;
        }

        //public DataTable GetLookupData(string userName, string lookupKey, Dictionary<string, string> mappingValue)
        //{
        //    DBLookupInfo dbLookupInfo = GetLookupInfo(userName, lookupKey);
        //    return GetLookupData(userName, dbLookupInfo, mappingValue);
        //}

        //public string UpdateLookupInfo(string userName, string passwordHash, int repositoryId, string fieldName, DBLookupInfo lookupInfo)
        //{
        //    CommonValidator.CheckNull(passwordHash);
        //    CommonValidator.CheckNull(repositoryId);
        //    CommonValidator.CheckNull(fieldName);
        //    CommonValidator.CheckNull(lookupInfo);

        //    string encriptPass = CryptographyHelper.EncryptUsingSymmetricAlgorithm(lookupInfo.ConnectionInfo.Password);
        //    string originalPass = lookupInfo.ConnectionInfo.Password;
        //    lookupInfo.ConnectionInfo.Password = encriptPass;

        //    Stream writer = new MemoryStream();

        //    _lookupSerializer.Serialize(writer, lookupInfo);
        //    writer.Position = 0;
        //    StreamReader reader = new StreamReader(writer);
        //    string xmlString = reader.ReadToEnd();

        //    var repositoryManager = new ContentStoreDB.RepositoryManager(userName);
        //    string lookupkey = repositoryManager.UpdateIndexFieldLookupInfo(passwordHash, repositoryId, fieldName, xmlString);
        //    lookupInfo.ConnectionInfo.Password = originalPass;

        //    return lookupkey;
        //}

        public List<string> GetDatabaseNames(ConnectionInfo connectionInfo)
        {
            CommonValidator.CheckNull(connectionInfo);

            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);

            return dbService.DatabaseNames((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port);
        }

        public List<string> GetDatabaseSchemas(ConnectionInfo connectionInfo)
        {
            CommonValidator.CheckNull(connectionInfo);

            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);

            return dbService.Schemas((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port);
        }

        public List<string> GetTableNames(ConnectionInfo connectionInfo)
        {
            CommonValidator.CheckNull(connectionInfo);

            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);

            return dbService.TableNames((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port, connectionInfo.Schema);
        }

        public List<string> GetViewNames(ConnectionInfo connectionInfo)
        {
            CommonValidator.CheckNull(connectionInfo);

            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);
            return dbService.ViewNames((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port, connectionInfo.Schema);
        }

        public Dictionary<string, string> GetColumns(ConnectionInfo connectionInfo, string sourceName, LookupDataSourceType sourceType)
        {
            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);

            if (sourceType == LookupDataSourceType.StoredProcedure)
            {
                DataTable parameterTable = GetParameters(connectionInfo, sourceName);
                DbParameter[] parameters = new DbParameter[parameterTable.Rows.Count];
                var dbExecute = dbService.GetDBExecute((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port);
                IDbCommand command = dbExecute.GetCommand(sourceName, CommandType.StoredProcedure);

                for (int i = 0; i < parameterTable.Rows.Count; i++)
                {
                    DataRow row = parameterTable.Rows[i];
                    parameters[i] = (DbParameter)command.CreateParameter();// new DbParameter { ParameterName = row["Name"].ToString() };
                    parameters[i].ParameterName = row["Name"].ToString();

                    switch (row["DataType"].ToString())
                    {
                        case "int":
                        case "decimal":
                        case "tinyint":
                        case "bigint":
                        case "float":
                        case "bit":
                            parameters[i].Value = 0;
                            break;
                        case "char":
                        case "varchar":
                        case "nvarchar":
                        case "text":
                        case "ntext":
                            parameters[i].Value = string.Empty;
                            break;
                        case "date":
                        case "datetime":
                            parameters[i].Value = "1900-01-01";
                            break;
                    }

                    parameters[i].Direction = ParameterDirection.Input;
                }

                DataTable dataTable = dbService.GetDBExecute((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port).ExecuteQueryGetDataTable(sourceName, CommandType.StoredProcedure, parameters);

                return dataTable.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => col.DataType.Name); ;
            }
            else
            {
                return dbService.ColumnNames((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port, connectionInfo.Schema, sourceName) as Dictionary<string, string>;
            }

        }

        public DataTable GetParameters(ConnectionInfo connectionInfo, string storedName)
        {
            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);
            return dbService.ParameterNames((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port, connectionInfo.Schema, storedName);
        }

        public List<string> GetStoredProcedureNames(ConnectionInfo connectionInfo)
        {
            CommonValidator.CheckNull(connectionInfo);

            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType);

            return dbService.StoredProcedureNames((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port, connectionInfo.Schema);
        }

        public bool TestConnection(ConnectionInfo connectionInfo)
        {
            CommonValidator.CheckNull(connectionInfo);

            return LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)connectionInfo.DbType).TestConnection((ServerLookupService.DatabaseType)connectionInfo.DbType,
                (ServerLookupService.ProviderType)connectionInfo.ProviderType, connectionInfo.DatabaseName,
                connectionInfo.Username, connectionInfo.Password, connectionInfo.Host, connectionInfo.Port);
        }

        public string GenerateSqlCommand(List<string> indexField, string lookupField, LookupDomain.DatabaseType dbType, LookupDomain.LookupType lookupType, string sourceName, string dbSchema)
        {
            return CommandHelper.GenerateSqlCommand(indexField, lookupField, dbType, lookupType, sourceName, dbSchema);
        }

        public string GenerateExampleSqlCommand(LookupDomain.DatabaseType dbType, LookupDomain.LookupType lookupType, string dbSchema)
        {
            return CommandHelper.GenerateExampleSqlCommand(dbType, lookupType, dbSchema);
        }

        public string GenerateExampleSqlCommandWithBatchInfo(LookupDomain.DatabaseType dbType, LookupDomain.LookupType lookupType, string dbSchema)
        {
            return CommandHelper.GenerateExampleSqlCommandWithBatchInfo(dbType, lookupType, dbSchema);
        }

        public List<string> GetRuntimeValueParams(string sqlCommand)
        {
            return CommandHelper.GetRuntimeValueParams(sqlCommand);
        }

        public bool TestQueryParam(string query, List<string> fieldNames)
        {
            bool hasInvalidParam = true;
            string invalidParamName = string.Empty;
            List<string> paramNames = CommandHelper.GetRuntimeValueParams(query);

            foreach (string paramName in paramNames)
            {
                if (!fieldNames.Contains(paramName))
                {
                    invalidParamName += Environment.NewLine + string.Format("Invalid field '{0}'.", paramName);
                }
            }

            if (!string.IsNullOrEmpty(invalidParamName))
            {
                hasInvalidParam = false;
                invalidParamName = invalidParamName.Substring(Environment.NewLine.Length);
                throw new Exception(invalidParamName);
            }

            return hasInvalidParam;
        }

        //public void RemoveLookup(string userName, string passwordHash, int repositoryId, string fieldName)
        //{
        //    var repositoryManager = new ContentStoreDB.RepositoryManager(userName);
        //    repositoryManager.UpdateIndexFieldLookupInfo(passwordHash, repositoryId, fieldName, string.Empty);
        //}

        //public bool DoesLookupContainField(string userName, string passwordHash, int repositoryId, string fieldName)
        //{
        //    CommonValidator.CheckNull(repositoryId);
        //    CommonValidator.CheckNull(indexName);

        //    var repositoryManager = new ContentStoreDB.RepositoryManager(userName);
        //    Repository repository = repositoryManager.GetRepositoryById(passwordHash, repositoryId);

        //    foreach (IndexField index in repository.IndexFields.Values)
        //    {
        //        if (index.HasLookup)
        //        {
        //            DBLookupInfo lookupInfo = GetLookupInfo(userName, index.AppSettingsDBLookupKey);
        //            if (lookupInfo.LookupMapping.Contains(indexName) || lookupInfo.RuntimeMappingInfo.Contains(indexName))
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        //private readonly IAppSettingManager _appSettingManager = new AppSettingManager();

        //private readonly XmlSerializer _lookupSerializer = new XmlSerializer(typeof(DBLookupInfo));

        //private string GenerateCallStoredCommand(ConnectionInfo connectionInfo, string sourceName, string dbSchema)
        //{
        //    string callStoredCommand;
        //    string openChar;
        //    string closeChar;
        //    DataTable para = GetParameters(connectionInfo, sourceName);

        //    switch (connectionInfo.DbType)
        //    {
        //        case DatabaseType.MsSql:
        //            openChar = "[";
        //            closeChar = "]";
        //            callStoredCommand = "EXEC {3}{0}{4}.{3}{1}{4} {2}";
        //            break;
        //        case DatabaseType.MySql:
        //            openChar = closeChar = "`";
        //            callStoredCommand = "CALL {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
        //            break;
        //        case DatabaseType.PostgreSql:
        //            openChar = closeChar = "\"";
        //            callStoredCommand = "SELECT * FROM {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
        //            break;
        //        case DatabaseType.DB2:
        //            openChar = closeChar = "\"";
        //            callStoredCommand = "CALL {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
        //            break;
        //        case DatabaseType.Oracle:
        //            openChar = closeChar = "\"";
        //            callStoredCommand = "CALL {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
        //            break;
        //        default:
        //            throw new NotSupportedException();
        //    }

        //    return string.Format(callStoredCommand, dbSchema, sourceName, openChar, closeChar);
        //}

        public void UpdateBatchLookup(Guid fieldId, string xml, User user, Guid? lookupActivityId)
        {
            using (DapperContext context = new DapperContext(user))
            {
                BatchFieldMetaDataDao batchFieldDao = new BatchFieldMetaDataDao(context);
                batchFieldDao.UpdateLookup(fieldId, xml, lookupActivityId);
            }
        }

        public void UpdateDocumentLookup(Guid fieldId, string xml, User user, Guid? lookupActivityId)
        {
            using (DapperContext context = new DapperContext(user))
            {
                DocFieldMetaDataDao docFieldDao = new DocFieldMetaDataDao(context);
                docFieldDao.UpdateLookup(fieldId, xml, lookupActivityId);
            }
        }

        private DataTable GetLookupDataFormStored(DataTable lookupData, LookupInfo info, string lookupValue)
        {
            return new LookupDao().GetLookupData(lookupData, "#temLookup", info.LookupOperator, info.LookupColumn, info.MaxLookupRow, lookupValue);
        }
    }
}
