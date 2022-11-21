using System;
using System.Collections.Generic;
using System.Data;

namespace Ecm.ServerLookupService
{
    public class MySqService : LookupServiceBase
    {
        public override List<string> TableNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = '" +
                    databaseName + "' ORDER BY TABLE_NAME");

            return GetListObjectName(table, "TABLE_NAME");
        }

        public override List<string> ViewNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = " + "'" +
                    databaseName + "' ORDER BY TABLE_NAME");

            return GetListObjectName(table, "TABLE_NAME");
        }

        public override IDictionary<string, string> ColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string tableName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '" +
                    databaseName + "' AND TABLE_NAME= \'" + tableName + "' ORDER BY COLUMN_NAME");

            Dictionary<string, string> tables = new Dictionary<string, string>();
            foreach (DataRow row in table.Rows)
            {
                tables.Add(Convert.ToString(row["COLUMN_NAME"]), Convert.ToString(row["DATA_TYPE"]));
            }

            return tables;
        }

        public override List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"SELECT SCHEMA_NAME AS NAME FROM INFORMATION_SCHEMA.SCHEMATA ORDER BY SCHEMA_NAME");

            return GetListObjectName(table, "NAME");
        }

        public override List<string> Schemas(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            List<string> table = new List<string>();
            table.Add(databaseName);

            return table;
        }

    }
}