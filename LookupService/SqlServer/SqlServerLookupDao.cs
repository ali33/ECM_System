using Ecm.Utility;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Ecm.LookupService.SqlServer
{
    /// <summary>
    /// The purpose of this class is to support execute data from SQL Server database
    /// </summary>
    public class SqlServerLookupDao : ILookupDao
    {
        private const string _sysObjectQuery = "SELECT name as Name, xtype as Type FROM SYSOBJECTS";
        private const string _dataProvider = "System.Data.SqlClient";
        private const string _sysColumnsQuery = "SELECT Col.name as Name, Ty.name as Type " +
                                                "FROM Syscolumns Col JOIN Systypes Ty ON Col.xtype = Ty.xtype " +
                                                "WHERE id=(SELECT id from sysobjects WHERE xtype='{0}' and name='{1}') AND Ty.name <> 'sysname' " +
                                                "ORDER BY Col.Name";
        private const string _sysQuery = "SELECT name FROM sys.databases WHERE database_id > 4";

        //Public methods
        public DataTable GetLookupData(string queryString, string value, LookupType type, string connectionString)
        {
            DataTable lookupData;

            if (type == LookupType.StoredProcedure)
            {
                const string storedName = "sp_executesql";
                var parameters = new[] { new SqlParameter("@Sql", queryString) };
                lookupData = Db.GetDataTable(storedName, parameters, CommandType.StoredProcedure, connectionString, _dataProvider);
            }
            else
            {
                queryString = queryString.Replace("<<value>>", value);
                lookupData = Db.GetDataTable(queryString, connectionString, _dataProvider);
            }

            return lookupData;
        }

        public IDictionary<string, string> GetDataSource(DataSourceType type, string connectionString)
        {
            var datasources = new Dictionary<string, string>();
            string sql = string.Empty;

            if (type == DataSourceType.ALL)
            {
                sql = _sysObjectQuery + " WHERE xtype = 'v' OR xtype = 'u' OR xtype = 'p'";
            }
            else if (type == DataSourceType.VIEW)
            {
                sql = _sysObjectQuery + " WHERE xtype = 'v'";
            }
            else if (type == DataSourceType.TABLE)
            {
                sql = _sysObjectQuery + " WHERE xtype='u'";
            }
            else if (type == DataSourceType.STORED_PROCEDURE)
            {
                sql = _sysObjectQuery + " WHERE xtype='p'";
            }

            sql += " ORDER BY name";

            DataTable dt = Db.GetDataTable(sql, connectionString, _dataProvider);

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Name"] != null && row["Type"] != null)
                    {
                        datasources.Add(row["Name"].ToString() , row["Type"].ToString());
                    }
                }
            }
            
            return datasources;
        }

        public DataTable GetParameters(string storedName, string connectionString)
        {
            string sql = "SELECT PARAMETER_NAME as Name, ORDINAL_POSITION as OrderIndex, DATA_TYPE as DataType, PARAMETER_MODE as Mode " +
                         "FROM INFORMATION_SCHEMA.PARAMETERS " +
                         "WHERE SPECIFIC_NAME = '" + storedName + "' AND PARAMETER_MODE = 'IN' " +
                         "ORDER BY ORDINAL_POSITION";

            DataTable dt = Db.GetDataTable(sql, connectionString, _dataProvider);
                
            return dt;
        }

        public IDictionary<string, string> GetColumns(string sourceName, string connectionString, DataSourceType type)
        {
            Dictionary<string, string> cols;

            if (type == DataSourceType.VIEW || type == DataSourceType.TABLE)
            {
                cols = (Dictionary<string, string>) GetColumnsFromTable(sourceName, connectionString, type);
            }
            else
            {
                cols = (Dictionary<string, string>) GetColumnsFromStored(sourceName, connectionString);
            }

            return cols;
        }

        public bool TestConnection(string connectionString)
        {
            return Db.ConnectToDB(connectionString, _dataProvider);
        }

        //Private methods
        private IDictionary<string, string> GetColumnsFromTable(string sourceName, string connectionString, DataSourceType type)
        {
            string sql = string.Empty;
            var cols = new Dictionary<string, string>();

            if (type == DataSourceType.VIEW)
            {
                sql = string.Format(_sysColumnsQuery, "V", sourceName);
            }
            else if (type == DataSourceType.TABLE)
            {
                sql = string.Format(_sysColumnsQuery, "U", sourceName);
            }

            DataTable dt = Db.GetDataTable(sql, connectionString, _dataProvider);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Name"] != null && row["Type"] != null)
                    {
                        cols.Add(row["Name"].ToString(), row["Type"].ToString());
                    }
                }
            }

            return cols;
        }

        private IDictionary<string, string> GetColumnsFromStored(string sourceName, string connectionString)
        {
            DataTable paras = GetParameters(sourceName, connectionString);
            DataTable colsName;

            if (paras.Rows.Count > 0)
            {
                var sqlParas = new SqlParameter[paras.Rows.Count];

                for (int i = 0; i < paras.Rows.Count; i++)
                {
                    DataRow row = paras.Rows[i];
                    sqlParas[i] = new SqlParameter {ParameterName = row["Name"].ToString()};

                    switch (row["DataType"].ToString())
                    {
                        case "int":
                        case "decimal":
                        case "tinyint":
                        case "bigint":
                        case "float":
                        case "bit":
                            sqlParas[i].Value = 0;
                            break;
                        case "char":
                        case "varchar":
                        case "nvarchar":
                        case "text":
                        case "ntext":
                            sqlParas[i].Value = string.Empty;
                            break;
                        case "date":
                        case "datetime":
                            sqlParas[i].Value = "1900-01-01";
                            break;
                    }

                    sqlParas[i].Direction = ParameterDirection.Input;
                }

                colsName = Db.GetDataTable(sourceName, sqlParas, CommandType.StoredProcedure, connectionString, _dataProvider);
            }
            else
            {
                colsName = Db.GetDataTable(sourceName, null, CommandType.StoredProcedure, connectionString, _dataProvider);
            }

            return colsName.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => col.DataType.Name);
        }

        public IList<string> GetDatabaseNames(string connectionString)
        {
            DataTable dt = Db.GetDataTable(_sysQuery, connectionString, _dataProvider);
            IList<string> dbNames = new List<string>();
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Name"] != null)
                    {
                        dbNames.Add(row["Name"].ToString());
                    }
                }
            }

            return dbNames;
        }
    }
}
