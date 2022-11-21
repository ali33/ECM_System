using System;
using System.Collections.Generic;
using System.Data;


namespace Ecm.ServerLookupService
{
    public class OracleService : LookupServiceBase
    {
        public override List<string> Schemas(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(GetSchemasSQL);

            return GetListObjectName(table, "SCHEMA_NAME");
        }

        public override List<string> TableNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = GetTablesSQL + " WHERE TABLE_TYPE='BASE TABLE'" + " AND TABLE_SCHEMA='"
                           + schema + "' ORDER BY TABLE_NAME";

            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);

            return GetListObjectName(table, "TABLE_NAME");
        }

        public override List<string> ViewNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = GetTablesSQL + " WHERE TABLE_TYPE='VIEW'" + " AND TABLE_SCHEMA='"
                           + schema + "' ORDER BY TABLE_NAME";

            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);

            return GetListObjectName(table, "TABLE_NAME");
        }

        public override IDictionary<string, string> ColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string tableName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = GetColumnsSQL + "WHERE TABLE_SCHEMA = '" +
                           schema + "' AND TABLE_NAME= \'" + tableName + "' ORDER BY COLUMN_NAME";
            
            DataTable table = dbExecute.ExecuteQueryGetDataTable(query);
            Dictionary<string, string> tables = new Dictionary<string, string>();
            
            foreach (DataRow row in table.Rows)
            {
                tables.Add(Convert.ToString(row["COLUMN_NAME"]), Convert.ToString(row["DATA_TYPE"]));
            }

            return tables;
        }

        public override DataTable ParameterNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string storedName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string query = @"SELECT ARGUMENT_NAME as Name, POSITION as OrderIndex, DATA_TYPE as DataType, IN_OUT as Mode 
                             FROM   all_arguments
                             WHERE  OWNER = '" + schema + @"' and
                                    OBJECT_NAME = '" + storedName + @"'
                             ORDER BY POSITION";
            
            return dbExecute.ExecuteQueryGetDataTable(query);
        }

        public override List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(@"SELECT instance_name AS NAME FROM v$instance ORDER BY NAME");

            return GetListObjectName(table, "NAME");
        }

        public override List<string> StoredProcedureNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(
                    @"SELECT DISTINCT name AS STORED_NAME FROM dba_source WHERE TYPE = 'PROCEDURE' AND OWNER ='" +
                    schema + "' ORDER BY STORED_NAME");

            return GetListObjectName(table, "STORED_NAME");
        }

        private const string GetSchemasSQL =
            @"SELECT SCHEMA_NAME FROM (WITH char_set AS (SELECT value$ cs FROM sys.props$ WHERE name = 'NLS_CHARACTERSET')
                                        SELECT SYS_CONTEXT('userenv', 'DB_NAME') CATALOG_NAME, USERname SCHEMA_NAME, char_set.cs 
                                        DEFAULT_CHARACTER_SET_NAME, SYS_CONTEXT('USERENV','NLS_SORT') 
                                        DEFAULT_COLLATION_NAME, TO_CHAR(NULL) SQL_PATH
                                        FROM char_set, all_users) ";

        private const string GetTablesSQL =
            @"SELECT TABLE_NAME FROM (SELECT SYS_CONTEXT('userenv', 'DB_NAME') TABLE_CATALOG, 
                                        owner TABLE_SCHEMA, TABLE_NAME, 
                                         case 
                                         when iot_type = 'Y'
                                           then 'IOT'
                                         when temporary = 'Y'
                                           then 'TEMP'
                                         else
                                          'BASE TABLE'
                                         end table_type         
                                        FROM all_tables
                                        UNION ALL
                                        SELECT SYS_CONTEXT('userenv', 'DB_NAME') TABLE_CATALOG, owner TABLE_SCHEMA, view_name table_name, 'VIEW' table_type
                                        FROM all_views ) tables ";

        private const string GetColumnsSQL =
            @"SELECT COLUMN_NAME, DATA_TYPE FROM (
                                        SELECT SYS_CONTEXT('userenv', 'DB_NAME') TABLE_CATALOG,
                                               owner TABLE_SCHEMA,
                                               table_name, 
                                               column_name,
                                               data_type,
                                               data_type_mod,
                                               decode(data_type_owner, null, to_char(null), SYS_CONTEXT('userenv', 'DB_NAME')) domain_catalog,
                                               data_type_owner domain_schema,
                                               data_length character_maximum_length,
                                               data_length character_octet_length,
                                               data_length,
                                               data_precision numeric_precision,
                                               data_scale numeric_scale,
                                               nullable is_nullable,
                                               column_id ordinal_position,
                                               default_length,
                                               data_default column_default,
                                               num_distinct,
                                               low_value,
                                               high_value,
                                               density,
                                               num_nulls,
                                               num_buckets,
                                               last_analyzed,
                                               sample_size,
                                               SYS_CONTEXT('userenv', 'DB_NAME') character_set_catalog,
                                               'SYS' character_set_schema,
                                               SYS_CONTEXT('userenv', 'DB_NAME') collation_catalog,
                                               'SYS' collation_schema,
                                               character_set_name,
                                               char_col_decl_length,
                                               global_stats,
                                               user_stats,
                                               avg_col_len,
                                               char_length,
                                               char_used,
                                               v80_fmt_image,
                                               data_upgraded,
                                               histogram
                                      FROM all_tab_columns av
                                    ) columns ";

        
    }
}