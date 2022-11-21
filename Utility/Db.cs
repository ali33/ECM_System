using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Ecm.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class Db
    {
        public static int Update(string sql, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public static int Insert(string sql, bool getId, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    connection.Open();
                    command.ExecuteNonQuery();

                    int id = -1;

                    // Check if new identity is needed.
                    if (getId)
                    {
                        // Execute db specific autonumber or identity retrieval code
                        // SELECT SCOPE_IDENTITY() -- for SQL Server
                        // SELECT @@IDENTITY -- for MS Access
                        // SELECT MySequence.NEXTVAL FROM DUAL -- for Oracle
                        string identitySelect;
                        switch(dataProvider)
                        {
                            // Access
                            case "System.Data.OleDb":
                                identitySelect = "SELECT @@IDENTITY";
                                break;
                            // Sql Server
                            case "System.Data.SqlClient":
                                identitySelect = "SELECT SCOPE_IDENTITY()";
                                break;
                            // Oracle
                            case "System.Data.OracleClient":
                                identitySelect = "SELECT MySequence.NEXTVAL FROM DUAL";
                                break;
                            default:
                                identitySelect = "SELECT @@IDENTITY";
                                break;
                        }
                        command.CommandText = identitySelect;
                        id = int.Parse(command.ExecuteScalar().ToString());
                    }
                    return id;
                }
            }
        }

        public static void Insert(string sql, string connectionString, string dataProvider)
        {
            Insert(sql, false, connectionString, dataProvider);
        }

        public static DataSet GetDataSet(string sql, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    using (DbDataAdapter adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public static DataSet GetDataSet(string sql, DbParameter[] parameters, CommandType type, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = type;
                    command.CommandText = sql;

                    foreach (DbParameter para in parameters)
                    {
                        command.Parameters.Add(para);
                    }

                    using (DbDataAdapter adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public static DataTable GetDataTable(string sql, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    using (DbDataAdapter adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        return ds.Tables[0] ;
                    }
                }
            }
        }

        public static DataTable GetDataTable(string sql, DbParameter[] parameters, CommandType type, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = type;
                    command.CommandText = sql;
                    if (parameters != null)
                    {
                        foreach (DbParameter para in parameters)
                        {
                            command.Parameters.Add(para);
                        }
                    }

                    using (DbDataAdapter adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        return ds.Tables[0];
                    }
                }
            }
        }

        public static DataRow GetDataRow(string sql, CommandType type, string connectionString, string dataProvider)
        {
            DataRow row = null;

            DataTable dt = GetDataTable(sql, connectionString, dataProvider);
            if (dt.Rows.Count > 0)
            {
                row = dt.Rows[0];
            }

            return row;
        }

        public static DataRow GetDataRow(string sql, CommandType type, DbParameter[] parameters, string connectionString, string dataProvider)
        {
            DataRow row = null;

            DataTable dt = GetDataTable(sql, parameters, type, connectionString, dataProvider);
            if (dt.Rows.Count > 0)
            {
                row = dt.Rows[0];
            }

            return row;
        }

        public static object GetScalar(string sql, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }
       
        public static object GetScalar(string storedName, DbParameter[] parameters, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = storedName;
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        public static DbDataReader GetDataReader(string sql, DbParameter[] parameters, CommandType type, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
            DbDataReader dr = null;
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = type;
                    command.CommandText = sql;

                    foreach (DbParameter para in parameters)
                    {
                        command.Parameters.Add(para);
                    }

                    dr = command.ExecuteReader();
                }
            }

            return dr;
        }

        public static DbDataReader GetDataReader(string sql, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
            DbDataReader dr = null;
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    dr = command.ExecuteReader();
                }
            }

            return dr;
        }

        public static string Escape(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
                return "'" + s.Trim().Replace("'", "''") + "'";
        }

        public static string Escape(string s, int maxLength)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
            {
                s = s.Trim();
                if (s.Length > maxLength) s = s.Substring(0, maxLength - 1);
                return "'" + s.Trim().Replace("'", "''") + "'";
            }
        }

        public static bool ConnectToDB(string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                try
                {
                    connection.Open();
                    return true;
                }
                catch
                {
                    try
                    {
                        connection.Open();
                    }
                    catch
                    {
                        return false;
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }

            return false;
        }

        public static void ExecuteNoneQuery(string sql, string connectionString, string dataProvider)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
