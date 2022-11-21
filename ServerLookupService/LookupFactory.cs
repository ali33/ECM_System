using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;

using IBM.Data.DB2;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.DataAccess.Client;

namespace Ecm.ServerLookupService
{
    public static class LookupFactory
    {
        public static IDbConnection CreateConnection(string conn, DatabaseType dbtype, ProviderType providerType)
        {
            IDbConnection dbConnection;

            if (providerType == ProviderType.OleDb)
            {
                dbConnection = new OleDbConnection(conn);
            }
            else
            {
                switch (dbtype)
                {
                    case DatabaseType.MsSql:
                        dbConnection = new SqlConnection(conn);
                        break;
                    case DatabaseType.MySql:
                        dbConnection = new MySqlConnection(conn);
                        break;
                    case DatabaseType.PostgreSql:
                        dbConnection = new NpgsqlConnection(conn);
                        break;
                    case DatabaseType.Oracle:
                        dbConnection = new OracleConnection(conn);
                        break;
                    case DatabaseType.DB2:
                        dbConnection = new DB2Connection(conn);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            return dbConnection;
        }

        public static IDbCommand CreateCommand(string commandText, IDbConnection conn, DatabaseType dbtype,
                                               ProviderType providerType)
        {
            IDbCommand dbCommand;

            if (providerType == ProviderType.OleDb)
            {
                dbCommand = new OleDbCommand(commandText, (OleDbConnection) conn);
            }
            else
            {
                switch (dbtype)
                {
                    case DatabaseType.MySql:
                        dbCommand = new MySqlCommand(commandText, (MySqlConnection) conn);
                        break;
                    case DatabaseType.MsSql:
                        dbCommand = new SqlCommand(commandText, (SqlConnection) conn);
                        break;
                    case DatabaseType.PostgreSql:
                        dbCommand = new NpgsqlCommand(commandText, (NpgsqlConnection) conn);
                        break;
                    case DatabaseType.Oracle:
                        dbCommand = new OracleCommand(commandText, (OracleConnection) conn);
                        break;
                    case DatabaseType.DB2:
                        dbCommand = new DB2Command(commandText, (DB2Connection) conn);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            return dbCommand;
        }

        public static DbDataAdapter CreateAdapter(IDbCommand command, DatabaseType dbtype, ProviderType providerType)
        {
            DbDataAdapter dbDataAdapter;

            if (providerType == ProviderType.OleDb)
            {
                dbDataAdapter = new OleDbDataAdapter((OleDbCommand) command);
            }
            else
            {
                switch (dbtype)
                {
                    case DatabaseType.MsSql:
                        dbDataAdapter = new SqlDataAdapter((SqlCommand) command);
                        break;
                    case DatabaseType.MySql:
                        dbDataAdapter = new MySqlDataAdapter((MySqlCommand) command);
                        break;
                    case DatabaseType.PostgreSql:
                        dbDataAdapter = new NpgsqlDataAdapter((NpgsqlCommand) command);
                        break;
                    case DatabaseType.Oracle:
                        dbDataAdapter = new OracleDataAdapter((OracleCommand) command);
                        break;
                    case DatabaseType.DB2:
                        dbDataAdapter = new DB2DataAdapter((DB2Command) command);
                        break;
                    default:
                       throw new NotSupportedException();
                }
            }

            return dbDataAdapter;
        }
    }
}