using System;
using System.Collections.Generic;
using System.Data;

namespace Ecm.LookupService.Oracle
{
    public class OracleLookupDao : ILookupDao
    {
        public DataTable GetLookupData(string queryString, string value, LookupType type, string connectionString)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetDataSource(DataSourceType type, string connectionString)
        {
            throw new NotImplementedException();
        }

        public DataTable GetParameters(string storedName, string connectionString)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetColumns(string sourceName, string connectionString, DataSourceType type)
        {
            throw new NotImplementedException();
        }

        public bool TestConnection(string connectionString)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetDatabaseNames(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
