using System.Collections.Generic;
using System.Data;


using System;

namespace Ecm.ServerLookupService
{
    public class MsSqlService : LookupServiceBase
    {
        public override List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port)
        {
            LookupExecute dbExecute = GetDBExecute(dbType, providerType, databaseName, username, password, host, port);
            DataTable table = dbExecute.ExecuteQueryGetDataTable(@"SELECT NAME FROM sys.databases ORDER BY NAME");

            return GetListObjectName(table, "NAME");
        }
    }
}