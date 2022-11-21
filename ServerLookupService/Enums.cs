namespace Ecm.ServerLookupService
{
    public enum DatabaseType
    {
        MsSql,
        MySql,
        Oracle,
        PostgreSql,
        DB2
    }

    public enum LookupType
    {
        Table,
        View,
        Stored
    }

    public enum ProviderType
    {
        OleDb = 1,
        AdoNet = 2
    }
}
