using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System;
using Ecm.CaptureDomain;
using Ecm.Utility;

namespace Ecm.CaptureDAO.Context
{
    public class DapperContext : IDisposable
    {
        internal DbTransaction CurrentTransaction
        {
            get
            {
                return _transaction;
            }
        }

        internal DbConnection Connection { get; private set; }

        private DbTransaction _transaction;

        private bool _disposed;
        private string _connectionString;
        private const string ENCRYPTED_KEY = "D4A88355-7148-4FF2-A626-151A40F57330";

        public DapperContext(User user)
        {
            //_connectionString = user.CaptureConnectionString;
            //SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(_connectionString);
            //string password = connBuilder.Password;
            //connBuilder.Password = CryptographyHelper.DecryptDatabasePasswordUsingSymmetricAlgorithm(password, ENCRYPTED_KEY);
            //_connectionString = connBuilder.ConnectionString;
            _connectionString = ConnectionStringEncryptionHelper.GetDecryptpedConnectionString(user.CaptureConnectionString, ENCRYPTED_KEY);
            Connection = GetDbConnection(_connectionString);
            OpenConnection();
        }

        public DapperContext(string connectionString)
        {
            //_connectionString = connectionString;
            //SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(_connectionString);
            //string password = connBuilder.Password;
            //connBuilder.Password = CryptographyHelper.DecryptDatabasePasswordUsingSymmetricAlgorithm(password, ENCRYPTED_KEY);
            //_connectionString = connBuilder.ConnectionString;

            _connectionString = ConnectionStringEncryptionHelper.GetDecryptpedConnectionString(connectionString, ENCRYPTED_KEY);
            Connection = GetDbConnection(_connectionString);
            OpenConnection();
        }

        public void BeginTransaction()
        {
            _transaction = Connection.BeginTransaction();
        }

        private void CloseConnection()
        {
            if (Connection != null && Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }

        private void OpenConnection()
        {
            if (Connection != null && Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
            }
        }

        public static DbConnection GetDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }


        public void Dispose()
        {
            Dispose(true);
        }

        ~DapperContext()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // always close the connection
                CloseConnection();

                if (disposing)
                {
                    GC.Collect(); // collects all unused memory
                    GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                    GC.Collect();
                }

                _disposed = true;
            }
        }
    }
}