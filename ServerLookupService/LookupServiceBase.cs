using System;
using System.Collections.Generic;
using System.Data;

namespace Ecm.ServerLookupService
{
    public abstract class LookupServiceBase : ILookupService
    {
        public virtual bool TestConnection(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            string connectionString = ConnectionStringBuilder.GetConnectionString(providerType, dbType, databaseName, username, password, host, port);
            IDbConnection connection = LookupFactory.CreateConnection(connectionString, dbType, providerType);

            bool result = (connection.State == ConnectionState.Open);
            if (result)
            {
                connection.Close();                
            }

            return result;
        }

        public virtual List<string> Schemas(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(@"Select SCHEMA_NAME From INFORMATION_SCHEMA.SCHEMATA Where CATALOG_NAME = '" +
                                                                   databaseName + "' ORDER BY SCHEMA_NAME");

            return GetListObjectName(table, "SCHEMA_NAME");
        }

        public virtual List<string> TableNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"Select TABLE_NAME From INFORMATION_SCHEMA.TABLES Where TABLE_TYPE='BASE TABLE' And TABLE_CATALOG = '" +
                    databaseName + "' AND TABLE_SCHEMA='" + schema + "' ORDER BY TABLE_NAME");

            return GetListObjectName(table, "TABLE_NAME");
        }

        public virtual List<string> ViewNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"Select TABLE_NAME From INFORMATION_SCHEMA.VIEWS Where TABLE_CATALOG = " + "'" +
                    databaseName + "' AND TABLE_SCHEMA='" + schema + "' ORDER BY TABLE_NAME");

            return GetListObjectName(table, "TABLE_NAME");
        }

        public virtual List<string> StoredProcedureNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"SELECT ROUTINE_NAME AS STORED_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='" + schema + "' ORDER BY SPECIFIC_NAME");

            return GetListObjectName(table, "STORED_NAME");
        }

        public virtual IDictionary<string, string> ColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string tableName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"Select COLUMN_NAME, DATA_TYPE From INFORMATION_SCHEMA.COLUMNS Where TABLE_CATALOG = '" +
                        databaseName + "' And TABLE_NAME= \'" + tableName + "' ORDER BY COLUMN_NAME");

            Dictionary<string, string> tables = new Dictionary<string, string>();
            foreach (DataRow row in table.Rows)
            {
                tables.Add(Convert.ToString(row["COLUMN_NAME"]), Convert.ToString(row["DATA_TYPE"]));
            }

            return tables;
        }

        public virtual DataTable ParameterNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string storedName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string sql = "SELECT PARAMETER_NAME as Name, ORDINAL_POSITION as OrderIndex, DATA_TYPE as DataType, PARAMETER_MODE as Mode " +
                         "FROM INFORMATION_SCHEMA.PARAMETERS " +
                         "WHERE SPECIFIC_NAME = '" + storedName + "' AND PARAMETER_MODE = 'IN' " +
                         "ORDER BY ORDINAL_POSITION";
            DataTable dt = dbExecute.ExecuteQueryGetDataTable(sql);
            return dt;
        }

        public virtual IDictionary<string, string> ViewColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string viewName)
        {
            return ColumnNames(dbType, providerType, databaseName, username, password, host, port, schema, viewName);
        }

        public abstract List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port);

        public LookupExecute GetDBExecute(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            string connectionString = ConnectionStringBuilder.GetConnectionString(providerType, dbType, databaseName, username, password, host, port);
            return new LookupExecute(connectionString, dbType, providerType);
        }

        public static List<string> GetListObjectName(DataTable table, string columnName)
        {
            List<string> tables = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                tables.Add(Convert.ToString(row[columnName]));
            }

            return tables;
        }
    }
}