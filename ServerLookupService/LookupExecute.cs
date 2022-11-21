using System;
using System.Data;
using System.Data.Common;



namespace Ecm.ServerLookupService
{
    public class LookupExecute
    {
        private readonly String _connectionString;
        private readonly DatabaseType _dbType;
        private readonly ProviderType _providerType;
        
        public LookupExecute(string connectionString, DatabaseType dbtype, ProviderType providerType)
        {
            _dbType = dbtype;
            _providerType = providerType;
            _connectionString = connectionString;
        }

        public DataSet ExecuteQueryGetDataSet(string query)
        {
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(query, dbConnection, _dbType, _providerType);
            IDbDataAdapter dbDataAdapter = LookupFactory.CreateAdapter(dbCommand, _dbType, _providerType);

            try
            {
                DataSet dataSet = new DataSet();
                dbDataAdapter.Fill(dataSet);

                return dataSet;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public DataSet ExecuteQueryGetDataSet(string query, DbParameter[] parameters)
        {
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(query, dbConnection, _dbType, _providerType);

            if (parameters != null && parameters.Length > 0)
            {
                foreach (DbParameter dbParameter in parameters)
                {
                    dbCommand.Parameters.Add(dbParameter);
                }
            }

            IDbDataAdapter dbDataAdapter = LookupFactory.CreateAdapter(dbCommand, _dbType, _providerType);

            try
            {
                DataSet dataSet = new DataSet();
                dbDataAdapter.Fill(dataSet);

                return dataSet;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public DataTable ExecuteQueryGetDataTable(string query)
        {
            DataTable dataTable = new DataTable("Table");
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(query, dbConnection, _dbType, _providerType);

            try
            {
                DbDataReader dataReader = (DbDataReader)dbCommand.ExecuteReader();

                if (dataReader != null)
                {
                    if (dataReader.HasRows)
                    {
                        object[] values = new object[dataReader.FieldCount];

                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            dataTable.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                        }

                        while (dataReader.Read())
                        {
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                values[i] = dataReader.GetValue(i);
                            }
                            dataTable.Rows.Add(values);
                        }
                    }
                }

                return dataTable;
            }
            catch {
                return null;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public DataTable ExecuteQueryGetDataTable(string query, DbParameter[] parameters)
        {
            DataTable dataTable = new DataTable("Table");
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(query, dbConnection, _dbType, _providerType);

            if (parameters != null && parameters.Length > 0)
            {
                foreach (DbParameter dbParameter in parameters)
                {
                    dbCommand.Parameters.Add(dbParameter);
                }
            }

            try
            {
                DbDataReader dataReader = (DbDataReader) dbCommand.ExecuteReader();

                if (dataReader != null)
                {
                    if (dataReader.HasRows)
                    {
                        object[] values = new object[dataReader.FieldCount];

                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            dataTable.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                        }

                        while (dataReader.Read())
                        {
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                values[i] = dataReader.GetValue(i);
                            }
                            dataTable.Rows.Add(values);
                        }
                    }
                }

                return dataTable;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public DataTable ExecuteQueryGetDataTable(string commandText, CommandType commandType, DbParameter[] parameters)
        {
            DataTable dataTable = new DataTable("Table");
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(commandText, dbConnection, _dbType, _providerType);
            dbCommand.CommandType = commandType;

            if (parameters != null && parameters.Length > 0)
            {
                foreach (DbParameter dbParameter in parameters)
                {
                    dbCommand.Parameters.Add(dbParameter);
                }
            }

            try
            {
                DbDataReader dataReader = (DbDataReader) dbCommand.ExecuteReader();

                if (dataReader != null)
                {
                    object[] values = new object[dataReader.FieldCount];

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        dataTable.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                    }

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                values[i] = dataReader.GetValue(i);
                            }
                            dataTable.Rows.Add(values);
                        }
                    }
                }

                return dataTable;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public DataRow ExecuteQueryGetDataRow(string query)
        {
            DataTable dataTable = new DataTable();
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(query, dbConnection, _dbType, _providerType);

            try
            {
                DbDataReader dataReader = (DbDataReader) dbCommand.ExecuteReader();
                dataReader.Read();

                if (dataReader.HasRows)
                {
                    object[] values = new object[dataReader.FieldCount];

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        dataTable.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                        values[i] = dataReader.GetValue(i);
                    }

                    dataTable.Rows.Add(values);

                    return dataTable.Rows[0];
                }

                return null;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public DataRow ExecuteQueryGetDataRow(string query, DbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(query, dbConnection, _dbType, _providerType);

            if (parameters != null && parameters.Length > 0)
            {
                foreach (DbParameter dbParameter in parameters)
                {
                    dbCommand.Parameters.Add(dbParameter);
                }
            }
            try
            {
                DbDataReader dataReader = (DbDataReader) dbCommand.ExecuteReader();
                dataReader.Read();

                if (dataReader.HasRows)
                {
                    object[] values = new object[dataReader.FieldCount];

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        dataTable.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                        values[i] = dataReader.GetValue(i);
                    }

                    dataTable.Rows.Add(values);
                    dbConnection.Close();

                    return dataTable.Rows[0];
                }

                return null;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public bool ExecuteNoneQuery(CommandType commandtype, string transaction)
        {
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(transaction, dbConnection, _dbType, _providerType);

            try
            {
                dbCommand.CommandType = commandtype;
                dbCommand.ExecuteNonQuery();

                return true;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public bool ExecuteNoneQuery(CommandType commandtype, string transaction, DbParameter[] parameters)
        {
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(transaction, dbConnection, _dbType, _providerType);

            try
            {
                dbCommand.CommandType = commandtype;

                if (parameters != null && parameters.Length > 0)
                {
                    foreach (DbParameter dbParameter in parameters)
                    {
                        dbCommand.Parameters.Add(dbParameter);
                    }
                }

                dbCommand.ExecuteNonQuery();

                return true;
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
            }
        }

        public IDbCommand GetCommand(string commandText, CommandType commandType)
        {
            IDbConnection dbConnection = LookupFactory.CreateConnection(_connectionString, _dbType, _providerType);
            IDbCommand dbCommand = LookupFactory.CreateCommand(commandText, dbConnection, _dbType, _providerType);
            dbCommand.CommandType = commandType;
            
            return dbCommand;
        }
    }
}