using Ecm.CaptureDAO.Context;
using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Ecm.CaptureDAO
{
    public class LookupDao
    {
       
        public DataTable GetLookupData(DataTable lookupData, string tableName, string lookupOperator, string lookupColumn, int maxRow, string lookupValue)
        {
            try
            {
                var builderQuery = new StringBuilder();
                var builderInsert = new StringBuilder();
                var builderCreateColumns = new StringBuilder();
                var builderCreateTable = new StringBuilder();

                string insertStatement = @"INSERT INTO " + tableName + " VALUES({0})";
                string createTable = @"CREATE TABLE {0} ({1})";
                string tableColumns = string.Empty;

                var haveCreateSqlTempTable = false;

                foreach (DataRow row in lookupData.Rows)
                {
                    string values = string.Empty;

                    foreach (DataColumn col in lookupData.Columns)
                    {
                        string comma = string.Empty;
                        string sqlDataType = string.Empty;

                        switch (col.DataType.Name.ToLower())
                        {
                            case "string":
                                comma = "'";
                                sqlDataType = "NVARCHAR(MAX)";
                                break;
                            case "datetime":
                                comma = "'";
                                sqlDataType = "DATETIME";
                                break;
                            case "int16":
                            case "int32":
                            case "int64":
                                sqlDataType = "INT";
                                break;
                            case "decimal":
                                sqlDataType = "DECIMAL(18,2)";
                                break;
                            case "double":
                                sqlDataType = "FLOAT";
                                break;
                            case "boolean":
                            case "bool":
                                sqlDataType = "BIT";
                                break;
                            default:
                                sqlDataType = "NVARCHAR(MAX)";
                                break;
                        }

                        if (values == string.Empty)
                        {
                            values = comma + row[col].ToString() + comma;
                        }
                        else
                        {
                            values += "," + comma + row[col] + comma;
                        }

                        if (tableColumns == string.Empty)
                        {
                            tableColumns += "[" + col.ColumnName + "] " + sqlDataType;
                        }
                        else
                        {
                            tableColumns += ", [" + col.ColumnName + "] " + sqlDataType;
                        }

                    }

                    if (!haveCreateSqlTempTable)
                    {
                        builderCreateTable.AppendFormat(createTable, tableName, tableColumns);
                        haveCreateSqlTempTable = true;
                    }

                    builderInsert.AppendFormat(insertStatement, values);
                }

                string selectQuery = string.Empty;
                if (maxRow != 0)
                {
                    selectQuery = @"SELECT TOP " + maxRow + " * FROM " + tableName + " WHERE {0}"; ;
                }
                else
                {
                    selectQuery = @"SELECT * FROM " + tableName + " WHERE {0}"; ;
                }

                // Query drop temp table use to store data for search
                var dropTableQuery =
                                    @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
                                    DROP TABLE {0};";
                // Query create temp table use to store data for search

                builderQuery.AppendFormat(dropTableQuery, tableName);
                builderQuery.AppendLine();
                builderQuery.AppendLine();
                //Create tem table
                builderQuery.AppendLine(builderCreateTable.ToString());
                // Insert table
                builderQuery.AppendLine(builderInsert.ToString());
                // Select query
                string whereClause = BuildWhereClause(lookupOperator, lookupColumn);
                whereClause = whereClause.Replace("<<value>>", lookupValue);

                builderQuery.AppendFormat(selectQuery, whereClause);
                builderQuery.AppendLine();
                builderQuery.AppendLine();
                // Drop table
                builderQuery.AppendFormat(dropTableQuery, tableName);

                string connectionString = ConfigurationManager.ConnectionStrings["ECMConnectionString"].ConnectionString;
                connectionString = ConnectionStringEncryptionHelper.GetDecryptpedConnectionString(connectionString, "D4A88355-7148-4FF2-A626-151A40F57330");

                using (SqlConnection connec = new SqlConnection(connectionString))
                {

                    if (connec.State == ConnectionState.Closed)
                    {
                        connec.Open();
                    }
                    SqlDataAdapter da = new SqlDataAdapter(builderQuery.ToString(), connec);
                    DataTable dt = new DataTable("Lookup");
                    da.Fill(dt);

                    return dt;
                }
            }
            catch
            {
                return new DataTable("Lookup");
            }
        }

        private string BuildWhereClause(string lookupOperator, string lookupColumn)
        {
            string searchOperator = string.Empty;
            string whereClause = string.Empty;
            switch (lookupOperator)
            {
                case "Equal":
                    searchOperator = "=";
                    break;
                case "GreaterThan":
                    searchOperator = ">";
                    break;
                case "GreaterThanOrEqualTo":
                    searchOperator = ">=";
                    break;
                case "LessThan":
                    searchOperator = "<";
                    break;
                case "LessThanOrEqualTo":
                    searchOperator = "<=";
                    break;
                case "Contains":
                case "StartsWith":
                case "EndsWith":
                    searchOperator = "LIKE";
                    break;
                case "NotContains":
                    searchOperator = "NOT LIKE";
                    break;
                case "NotEqual":
                    searchOperator = "<>";
                    break;
            }

            string value = "<<value>>";

            if (lookupOperator == "Contains")
            {
                value = "%<<value>>%";
            }

            if (lookupOperator == "Contains" || lookupOperator == "NotContains")
            {
                value = "'%<<value>>%'";
            }

            if (lookupOperator == "StartsWith")
            {
                value = "'<<value>>%'";
            }

            if (lookupOperator == "EndsWith")
            {
                value = "'%<<value>>'";
            }

            if (string.IsNullOrEmpty(whereClause))
            {
                whereClause += "[" + lookupColumn + "]" + " " + searchOperator + " " + value;
            }
            else
            {
                whereClause += "AND " + "[" + lookupColumn + "]" + " " + searchOperator + " " + value;
            }

            return whereClause;
        }

    }
}
