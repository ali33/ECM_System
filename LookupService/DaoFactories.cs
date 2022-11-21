namespace Ecm.LookupService
{
    /// <summary>
    /// The purpose of this class is to contain the methods to get the instant of data access class.
    /// </summary>
    public class DaoFactories
    {
        public static DaoFactory GetFactory(string dataProvider)
        {
            // Return the requested DaoFactory
            switch (dataProvider)
            {
                case "System.Data.OleDb": return new Oledb.AccessDaoFactory();
                case "System.Data.SqlClient": return new SqlServer.SqlServerDaoFactory();
                case "System.Data.OracleClient": return new Oracle.OracleDaoFactory();
                default: return new SqlServer.SqlServerDaoFactory();
            }
        }
    }
}
