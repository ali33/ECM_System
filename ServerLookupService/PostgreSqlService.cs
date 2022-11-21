using System.Collections.Generic;
using System.Data;

namespace Ecm.ServerLookupService
{
    public class PostgreSqlService : LookupServiceBase
    {
        public override List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(@"SELECT datname as NAME FROM pg_database ORDER BY datname");

            return GetListObjectName(table, "NAME");
        }

        public override DataTable ParameterNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string storedName)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            string sql = "SELECT parameters.PARAMETER_NAME as Name, parameters.ORDINAL_POSITION as OrderIndex, parameters.DATA_TYPE as DataType," +
                         "parameters.PARAMETER_MODE as Mode " +
                         "FROM INFORMATION_SCHEMA.PARAMETERS JOIN INFORMATION_SCHEMA.ROUTINES ON routines.specific_name = parameters.specific_name " +
                         "WHERE routines.routine_name = '" + storedName + "' AND PARAMETER_MODE = 'IN' " +
                         "ORDER BY ORDINAL_POSITION";
            DataTable dt = dbExecute.ExecuteQueryGetDataTable(sql);
            return dt;
        }
    }
}