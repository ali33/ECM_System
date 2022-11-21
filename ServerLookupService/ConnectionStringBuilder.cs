using System;
using System.Configuration;
using System.Data.OracleClient;
using System.Data.SqlClient;

using IBM.Data.DB2;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Ecm.ServerLookupService
{

    public class ConnectionStringBuilder
    {
        public static String GetConnectionString(ProviderType providerType, DatabaseType dbType, string databaseName, string username, string password, string host, int port)
        {
            string connectionString;
            if (providerType == ProviderType.OleDb)
            {
                connectionString = BuildOleDbConnnectionString(dbType, databaseName, username, password, host, port);
            }
            else
            {
                switch (dbType)
                {
                    case DatabaseType.MsSql:
                        connectionString = BuildMsSqlConnnectionString(databaseName, username, password, host, port);
                        break;
                    case DatabaseType.MySql:
                        connectionString = BuildMySqlConnnectionString(databaseName, username, password, host, port);
                        break;
                    case DatabaseType.Oracle:
                        connectionString = BuildOracleConnnectionString(databaseName, username, password, host, port);
                        break;
                    case DatabaseType.DB2:
                        connectionString = BuildDB2ConnnectionString(databaseName, username, password, host, port);
                        break;
                    case DatabaseType.PostgreSql:
                        connectionString = BuildPostgreSqlConnnectionString(databaseName, username, password, host, port);
                        break;
                    default:
                        throw new Exception("Not Supported");
                }
            }

            return connectionString;
        }

        private static string BuildMsSqlConnnectionString(string databaseName, string username, string password, string host, int port)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = host + ", " + port;
            builder["Initial Catalog"] = databaseName;

            builder.UserID = username;
            builder.Password = password;

            return builder.ConnectionString;
        }

        private static string BuildMySqlConnnectionString(string databaseName, string username, string password, string host, int port)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = host;
            builder.Port = Convert.ToUInt32(port);
            builder.Database = databaseName;

            builder.UserID = username;
            builder.Password = password;

            return builder.ConnectionString;
        }

        private static string BuildPostgreSqlConnnectionString(string databaseName, string username, string password, string host, int port)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            builder.Host = host;
            builder.Database = databaseName;
            builder.Port = port;

            builder.UserName = username;
            builder.Password = password;

            return builder.ConnectionString;
        }

        private static string BuildOracleConnnectionString(string databaseName, string username, string password, string host, int port)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
            builder.DataSource = host + ":" + port + "/" + databaseName;

            builder.UserID = username;
            builder.Password = password;

            return builder.ConnectionString;
        }

        private static string BuildDB2ConnnectionString(string databaseName, string username, string password, string host, int port)
        {
            DB2ConnectionStringBuilder builder = new DB2ConnectionStringBuilder();
            builder.Server = host + ":" + port;
            builder.Database = databaseName;

            builder.UserID = username;
            builder.Password = password;

            return builder.ConnectionString;
        }

        private static string BuildOleDbConnnectionString(DatabaseType dbType, string databaseName, string username, string password, string host, int port)
        {
            switch (dbType)
            {
                case DatabaseType.MsSql:
                    return "Provider=" + ConfigurationManager.AppSettings.Get("MSSQL_OLEDB_PROVIDER") + ";" +
                           BuildMsSqlConnnectionString(databaseName, username, password, host, port);
                case DatabaseType.PostgreSql:
                    return "Provider=" + ConfigurationManager.AppSettings.Get("POSTGRESQL_OLEDB_PROVIDER") + ";Host=" +
                           host + ";Port=" + port + ";USER ID=" + username
                           + ";PASSWORD=" + password;
                case DatabaseType.MySql:
                    return "Provider=" + ConfigurationManager.AppSettings.Get("MYSQL_OLEDB_PROVIDER") + ";" +
                           BuildMySqlConnnectionString(databaseName, username, password, host, port);
                case DatabaseType.Oracle:
                    return "Provider=" + ConfigurationManager.AppSettings.Get("ORACLE_OLEDB_PROVIDER") + ";" +
                           BuildOracleConnnectionString(databaseName, username, password, host, port);
                case DatabaseType.DB2:
                    return "Provider=" + ConfigurationManager.AppSettings.Get("DB2_OLEDB_PROVIDER") + ";" +
                           BuildDB2ConnnectionString(databaseName, username, password, host, port);
                default:
                    throw new Exception("Not Supported");
            }
        }
    }
}