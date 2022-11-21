using System;


namespace Ecm.ServerLookupService
{
    public sealed class LookupServiceFactories
    {
        public static ILookupService GetDBService(DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.MsSql:
                    return new MsSqlService();
                case DatabaseType.MySql:
                    return new MySqService();
                case DatabaseType.PostgreSql:
                    return new PostgreSqlService();
                case DatabaseType.Oracle:
                    return new OracleService();
                case DatabaseType.DB2:
                    return new Db2Service();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}