using System;
using System.Collections.Generic;
using System.Linq;
using Ecm.Domain;
using Ecm.Utility;
using System.Data;
using Ecm.ServerLookupService;
using System.Data.Common;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using Ecm.DAO;
using Ecm.DAO.Context;

namespace Ecm.Core
{
    public class LookupManager : ManagerBase
    {
        public LookupInfo GetLookupInfo(Guid fieldId, User user)
        {
            using (DapperContext dataContext = new DapperContext(user))
            {
                FieldMetaDataDao fieldDao = new FieldMetaDataDao(dataContext);
                FieldMetaData field = fieldDao.GetById(fieldId);
                LookupInfo lookupInfo = null;

                if (field.IsLookup && field.LookupXML != null)
                {
                    field.LookupInfo = UtilsSerializer.Deserialize<LookupInfo>(field.LookupXML);
                    field.LookupMaps = field.LookupInfo.LookupMaps;
                    foreach (LookupMap mapping in field.LookupMaps)
                    {
                        FieldMetaData archiveField = fieldDao.GetById(mapping.ArchiveFieldId);
                        if (archiveField != null)
                        {
                            mapping.Name = archiveField.Name;
                        }
                    }

                    lookupInfo = field.LookupInfo;
                }

                return lookupInfo;
            }
        }

        public DataTable GetLookupData(LookupInfo lookupInfo, Dictionary<string, string> mappingValue)
        {
            CommonValidator.CheckNull(lookupInfo);
            CommonValidator.CheckNull(mappingValue);
            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType);
            string query = lookupInfo.SqlCommand;
            DataTable result = new DataTable { TableName = "Lookup" };

            foreach (KeyValuePair<string, string> value in mappingValue)
            {
                query = query.Replace(string.Format("<<{0}>>", value.Key), value.Value);
            }

            DataTable dataTable = dbService.GetDBExecute((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType,
                (ServerLookupService.ProviderType)lookupInfo.ConnectionInfo.ProviderType, lookupInfo.ConnectionInfo.DatabaseName,
                lookupInfo.ConnectionInfo.Username, lookupInfo.ConnectionInfo.Password, lookupInfo.ConnectionInfo.Host, lookupInfo.ConnectionInfo.Port).ExecuteQueryGetDataTable(query);
            dataTable.TableName = "Lookup";

            if (lookupInfo.LookupType == (int)Domain.LookupType.Stored)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    LookupMap map = lookupInfo.LookupMaps.FirstOrDefault(p => p.DataColumn == column.ColumnName);

                    if (map != null && !string.IsNullOrEmpty(map.Name))
                    {
                        result.Columns.Add(new DataColumn { ColumnName = map.Name });
                    }
                }

                DataRow resultRow;
                int maxRow = Convert.ToInt32(lookupInfo.MaxLookupRow);

                if (dataTable.Rows.Count < maxRow || maxRow == 0)
                {
                    maxRow = dataTable.Rows.Count;
                }

                for (int j = 0; j < maxRow; j++)
                {
                    resultRow = result.NewRow();

                    for (int i = 0; i < result.Columns.Count; i++)
                    {
                        resultRow[i] = dataTable.Rows[j][i].ToString();
                    }

                    result.Rows.Add(resultRow);
                }

                return result;
            }
            return dataTable;
        }

        public DataTable GetLookupData(LookupInfo lookupInfo, string lookupValue)
        {
            CommonValidator.CheckNull(lookupInfo);
            CommonValidator.CheckNull(lookupValue);
            ILookupService dbService = LookupServiceFactories.GetDBService((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType);
            string query = lookupInfo.SqlCommand;
            DataTable result = new DataTable { TableName = "Lookup" };

            if (lookupInfo.LookupType != (int)Domain.LookupType.Stored)
            {
                query = query.Replace("<<value>>", lookupValue);
            }

            DataTable dataTable = dbService.GetDBExecute((ServerLookupService.DatabaseType)lookupInfo.ConnectionInfo.DbType,
                (ServerLookupService.ProviderType)lookupInfo.ConnectionInfo.ProviderType, lookupInfo.ConnectionInfo.DatabaseName,
                lookupInfo.ConnectionInfo.Username, lookupInfo.ConnectionInfo.Password, lookupInfo.ConnectionInfo.Host, lookupInfo.ConnectionInfo.Port).ExecuteQueryGetDataTable(query);
            dataTable.TableName = "Lookup";



            if (lookupInfo.LookupType == (int)Domain.LookupType.Stored)
            {
                DataTable lookupFilter = GetLookupDataFormStored(dataTable, lookupInfo, lookupValue);

                var mappingColumn = new Dictionary<int, int>();
                var index = 0;
                var indexFilter = 0;

                foreach (DataColumn column in lookupFilter.Columns)
                {
                    LookupMap map = lookupInfo.LookupMaps.FirstOrDefault(p => p.DataColumn == column.ColumnName);

                    if (map != null && !string.IsNullOrEmpty(map.Name))
                    {
                        result.Columns.Add(new DataColumn { ColumnName = map.Name });
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

        public string GenerateSqlCommand(List<string> indexField, string lookupField, Domain.DatabaseType dbType, Domain.LookupType lookupType, string sourceName, string dbSchema)
        {
            return CommandHelper.GenerateSqlCommand(indexField, lookupField, dbType, lookupType, sourceName, dbSchema);
        }

        public string GenerateExampleSqlCommand(Domain.DatabaseType dbType, Domain.LookupType lookupType, string dbSchema)
        {
            return CommandHelper.GenerateExampleSqlCommand(dbType, lookupType, dbSchema);
        }

        public string GenerateExampleSqlCommandWithBatchInfo(Domain.DatabaseType dbType, Domain.LookupType lookupType, string dbSchema)
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

        private DataTable GetLookupDataFormStored(DataTable lookupData, LookupInfo info, string lookupValue)
        {
            //string conectionString = LoginUser.ArchiveConnectionString;
            //SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ECMConnectionString"].ConnectionString);
            //string server = connBuilder.DataSource;
            //string username = connBuilder.UserID;
            //string pass = Utility.CryptographyHelper.DecryptDatabasePasswordUsingSymmetricAlgorithm(connBuilder.Password, "D4A88355-7148-4FF2-A626-151A40F57330");
            //StringBuilder sql = new StringBuilder();
            //string insertSql = @"SELECT * into #temLookup FROM OPENROWSET('SQLNCLI', 'Server=" + server + ";uid=" + username + ";pwd=" + pass + "','{0}'";
            //string selectSql = "SELECT * FROM #temLookup Where {0}";
            //string whereSql = "";
            //insertSql = string.Format(insertSql, );

            //sql.AppendLine(insertSql);
            return new LookupDao().GetLookupData(lookupData, "#temLookup", info.LookupOperator, info.LookupColumn, info.MaxLookupRow, lookupValue);
        }
    }

}
