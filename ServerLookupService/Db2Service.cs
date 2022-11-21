using System;
using System.Collections.Generic;
using System.Data;



namespace Ecm.ServerLookupService
{
    public class Db2Service : LookupServiceBase
    {
        public override List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            throw new NotSupportedException();
        }

        public override List<string> Schemas(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(@"SELECT SCHEMANAME AS SCHEMA_NAME from SYSCAT.SCHEMATA ORDER BY SCHEMA_NAME");

            return GetListObjectName(table, "SCHEMA_NAME");
        }

        public override List<string> TableNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = @"SELECT TABNAME AS TABLE_NAME FROM SYSCAT.TABLES WHERE TABSCHEMA='" + schema +
                           "' ORDER BY TABLE_NAME";
            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);

            return GetListObjectName(table, "TABLE_NAME");
        }

        public override List<string> ViewNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = @"SELECT VIEWNAME AS TABLE_NAME FROM SYSCAT.VIEWS WHERE VIEWSCHEMA='" + schema +
                           "' ORDER BY TABLE_NAME";
            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);

            return GetListObjectName(table, "TABLE_NAME");
        }

        public override IDictionary<string, string> ColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string tableName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = @"SELECT COLNAME AS COLUMN_NAME, TYPENAME AS DATA_TYPE FROM SYSCAT.COLUMNS WHERE TABSCHEMA = '" +
                schema + "' And TABNAME= \'" + tableName + "' ORDER BY COLUMN_NAME";
            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);
            Dictionary<string, string> tables = new Dictionary<string, string>();
            foreach (DataRow row in table.Rows)
            {
                tables.Add(Convert.ToString(row["COLUMN_NAME"]), Convert.ToString(row["DATA_TYPE"]));
            }

            return tables;
        }

        public override List<string> StoredProcedureNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = @"SELECT ROUTINENAME AS STORED_NAME FROM SYSCAT.ROUTINES WHERE ROUTINESCHEMA='" +
                           schema + "' ORDER BY STORED_NAME";
            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);

            return GetListObjectName(table, "STORED_NAME");
        }
    }
}