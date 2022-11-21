using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Ecm.Utility
{
    public class ConnectionStringEncryptionHelper
    {
        public static string GetDecryptpedConnectionString(string connectionString, string key)
        {
            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(connectionString);
            string password = connBuilder.Password;
            connBuilder.Password = CryptographyHelper.DecryptDatabasePasswordUsingSymmetricAlgorithm(password, key);
            return connBuilder.ConnectionString;
        }
    }
}
