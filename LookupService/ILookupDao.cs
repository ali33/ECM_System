using System.Collections.Generic;
using System.Data;

namespace Ecm.LookupService
{
    public interface ILookupDao
    {
        DataTable GetLookupData(string queryString, string value, LookupType type, string connectionString);
        IDictionary<string, string> GetDataSource(DataSourceType type, string connectionString);
        DataTable GetParameters(string storedName, string connectionString);
        IDictionary<string, string> GetColumns(string sourceName, string connectionString, DataSourceType type);
        bool TestConnection(string connectionString);
        IList<string> GetDatabaseNames(string connectionString);
    }
}
