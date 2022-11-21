using System.Collections.Generic;

using System.Data;

namespace Ecm.ServerLookupService
{
    public interface ILookupService
    {
        bool TestConnection(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port);

        LookupExecute GetDBExecute(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port);

        List<string> DatabaseNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port);

        List<string> Schemas(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port);

        List<string> TableNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema);

        List<string> ViewNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema);

        List<string> StoredProcedureNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema);

        IDictionary<string, string> ColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string tableName);

        DataTable ParameterNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string storeName);

        IDictionary<string, string> ViewColumnNames(DatabaseType dbType, ProviderType providerType, string databaseName, string username, string password, string host, int port, string schema, string viewName);
    }
}