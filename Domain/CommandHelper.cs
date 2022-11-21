using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ecm.Domain
{
    public sealed class CommandHelper
    {
        public static List<string> GetRuntimeValueParams(string sqlCommand)
        {
            Regex regexObj = new Regex(@"(?<runtime_group><{2}[a-z_A-Z_0-9-\s\t#'`.!@$%^&*_+=-|()]*>{2})", RegexOptions.IgnoreCase);
            Match matchResults = regexObj.Match(sqlCommand);
            List<string> runtimes = new List<string>();

            while (matchResults.Success)
            {
                for (int i = 1; i < matchResults.Groups.Count; i++)
                {
                    Group groupObj = matchResults.Groups[i];
                    if (groupObj.Success)
                    {
                        runtimes.Add(groupObj.Value.Replace("<", string.Empty).Replace(">", string.Empty));
                    }
                }

                matchResults = matchResults.NextMatch();
            }

            return runtimes;
        }

        public static string GenerateExampleSqlCommand(DatabaseType dbType, LookupType lookupType, string dbSchema)
        {
            List<string> indexFields = new List<string> { "ContentField1", "ContentField2" };
            const string lookupField = "ContentField1";
            const string maxLookupRow = "[max rows returned to client]";

            switch (lookupType)
            {
                case LookupType.Stored:
                    return GenerateCallStoredCommand(dbType, "databaseStoredProcedure", dbSchema, "ContentField value");
                case LookupType.Table:
                    return GenerateSelectCommand(indexFields, lookupField, dbType, "databaseTable", dbSchema, maxLookupRow, true, false, false);
                case LookupType.View:
                    return GenerateSelectCommand(indexFields, lookupField, dbType, "databaseView", dbSchema, maxLookupRow, true, false, false);
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GenerateExampleSqlCommandWithBatchInfo(DatabaseType dbType, LookupType lookupType, string dbSchema)
        {
            List<string> indexFields = new List<string> { "ContentField1", "ContentField2" };
            const string lookupField = "ContentField1";
            const string maxLookupRow = "[max rows returned to client]";

            switch (lookupType)
            {
                case LookupType.Stored:
                    return GenerateCallStoredCommand(dbType, "databaseStoredProcedure", dbSchema, "ContentField value");
                case LookupType.Table:
                    return GenerateSelectCommandWithBatchInfo(indexFields, lookupField, dbType, "databaseTable", dbSchema, maxLookupRow, true, false, false);
                case LookupType.View:
                    return GenerateSelectCommandWithBatchInfo(indexFields, lookupField, dbType, "databaseView", dbSchema, maxLookupRow, true, false, false);
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GenerateExampleSqlCommandForPicklist(DatabaseType dbType, LookupType lookupType, string dbSchema, bool configPicklistValues)
        {
            List<string> indexFields = new List<string> { "ContentField1", "ContentField2" };
            const string picklistField = "ContentField1";
            const string maxLookupRow = "[max rows returned to client]";

            switch (lookupType)
            {
                case LookupType.Stored:
                    return GenerateCallStoredCommand(dbType, "databaseStoredProcedure", dbSchema, configPicklistValues? "constant value" : "ContentField value");
                case LookupType.Table:
                    return GenerateSelectCommand(indexFields, picklistField, dbType, "databaseTable", dbSchema, maxLookupRow, true, true, configPicklistValues);
                case LookupType.View:
                    return GenerateSelectCommand(indexFields, picklistField, dbType, "databaseView", dbSchema, maxLookupRow, true, true, configPicklistValues);
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GenerateSqlCommand(List<string> indexFields, string lookupField, DatabaseType dbType,
                                                LookupType lookupType, string sourceName, string dbSchema)
        {
            switch (lookupType)
            {
                case LookupType.Stored:
                    return GenerateCallStoredCommand(dbType, sourceName, dbSchema, lookupField);
                case LookupType.View:
                    return GenerateSelectCommand(indexFields, lookupField, dbType, sourceName, dbSchema, "10", false, false, false);
                case LookupType.Table:
                    return GenerateSelectCommand(indexFields, lookupField, dbType, sourceName, dbSchema, "10", false, false, false);
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GenerateSqlCommandForPicklist(List<string> indexFields, string lookupField, DatabaseType dbType,
                                                           LookupType lookupType, string sourceName, string dbSchema, bool configPicklistValues)
        {
            switch (lookupType)
            {
                case LookupType.Stored:
                    return GenerateCallStoredCommand(dbType, sourceName, dbSchema, lookupField);
                case LookupType.View:
                    return GenerateSelectCommand(indexFields, lookupField, dbType, sourceName, dbSchema, "10", false, true, configPicklistValues);
                case LookupType.Table:
                    return GenerateSelectCommand(indexFields, lookupField, dbType, sourceName, dbSchema, "10", false, true, configPicklistValues);
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GenerateCallStoredCommand(DatabaseType dbType, string sourceName, string dbSchema, string lookupField)
        {
            string callStoredCommand;
            string openChar;
            string closeChar;

            switch (dbType)
            {
                case DatabaseType.MsSql:
                    openChar = "[";
                    closeChar = "]";
                    callStoredCommand = "EXEC {3}{0}{4}.{3}{1}{4} @storeParam = '<<{2}>>'";
                    break;
                case DatabaseType.MySql:
                    openChar = closeChar = "`";
                    callStoredCommand = "CALL {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
                    break;
                case DatabaseType.PostgreSql:
                    openChar = closeChar = "\"";
                    callStoredCommand = "SELECT * FROM {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
                    break;
                case DatabaseType.DB2:
                    openChar = closeChar = "\"";
                    callStoredCommand = "CALL {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
                    break;
                case DatabaseType.Oracle:
                    openChar = closeChar = "\"";
                    callStoredCommand = "CALL {3}{0}{4}.{3}{1}{4}(<<{2}>>)";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return string.Format(callStoredCommand, dbSchema, sourceName, lookupField, openChar, closeChar);
        }

        private static string GenerateSelectCommand(List<string> indexFields, string lookupField, DatabaseType dbType,
                                                    string sourceName, string dbSchema, string maxLookupRow, bool isExample,
                                                    bool isForPicklist, bool configPicklistValues)
        {
            string sqlQueryTemplate;
            string openChar;
            string closeChar;

            switch (dbType)
            {
                case DatabaseType.MsSql:
                    sqlQueryTemplate = "SELECT TOP {0} {1} FROM {2} WHERE {3}";
                    openChar = "[";
                    closeChar = "]";
                    break;
                case DatabaseType.MySql:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} LIMIT 0, {0}";
                    openChar = closeChar = "`";
                    break;
                case DatabaseType.PostgreSql:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} LIMIT {0}";
                    openChar = closeChar = "\"";
                    break;
                case DatabaseType.Oracle:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} ROWNUM <= {0}";
                    openChar = closeChar = "\"";
                    break;
                case DatabaseType.DB2:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} FETCH FIRST {0} ROWS ONLY";
                    openChar = closeChar = "\"";
                    break;
                default:
                    throw new NotSupportedException();
            }

            // Build SELECT part
            string selectPart = string.Empty;
            string dbLookupField = lookupField;
            int i = 1;
            const string mapping = "{2}{0}{3} AS {2}{1}{3}, ";

            if (isForPicklist)
            {
                selectPart = string.Format(mapping, "databaseField", dbLookupField, openChar, closeChar);
                selectPart = selectPart.Remove(selectPart.Length - 2);
            }
            else
            {
                foreach (string field in indexFields)
                {
                    if(!field.Contains("batch."))
                    {
                        if (field == lookupField)
                        {
                            dbLookupField = "databaseField" + i;
                        }

                        selectPart += string.Format(mapping, "databaseField" + i, field, openChar, closeChar);
                        i++;    
                    }
                }
                selectPart = selectPart.Remove(selectPart.Length - 2);
            }

            // Build WHERE part
            string wherePart = string.Empty;
            if (isForPicklist)
            {
                if (configPicklistValues)
                {
                    wherePart = string.Format("{2}{0}{3} LIKE '%{1}%'", "databaseField", "constant value", openChar, closeChar);
                }
                else
                {
                    i = 1;
                    foreach (string field in indexFields)
                    {
                        if (field != lookupField)
                        {
                            dbLookupField = "databaseField" + i;
                            string condition = string.Format("{2}{0}{3} LIKE '%<<{1}>>%'", dbLookupField, field, openChar, closeChar);
                            wherePart += string.Format(" AND {0}", condition);
                            i++;
                        }
                    }

                    if (wherePart != string.Empty)
                    {
                        wherePart = wherePart.Substring(5);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(lookupField))
                {
                    wherePart = string.Format("{2}{0}{3} LIKE '%<<{1}>>%'", dbLookupField, isExample ? "ContentField value" : lookupField, openChar, closeChar);    
                }
                else
                {
                    wherePart = string.Format("{2}{0}{3} LIKE '%<<{1}>>%'", indexFields[0], isExample ? "ContentField value" : indexFields[0], openChar, closeChar);    
                }
            }

            // Build FROM part
            string fromPart = string.Format("{2}{0}{3}.{2}{1}{3}", dbSchema, sourceName, openChar, closeChar);

            // Return SQL command)
            string sqlQuery = string.Format(sqlQueryTemplate, maxLookupRow, selectPart, fromPart, wherePart);

            if (string.IsNullOrEmpty(wherePart))
            {
                // Trim WHERE token
                switch (dbType)
                {
                    case DatabaseType.MsSql:
                        sqlQuery = sqlQuery.TrimEnd();
                        sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 5);
                        break;
                }
            }

            return sqlQuery;
        }

        private static string GenerateSelectCommandWithBatchInfo(List<string> indexFields, string lookupField, DatabaseType dbType,
                                                    string sourceName, string dbSchema, string maxLookupRow, bool isExample,
                                                    bool isForPicklist, bool configPicklistValues)
        {
            string sqlQueryTemplate;
            string openChar;
            string closeChar;

            switch (dbType)
            {
                case DatabaseType.MsSql:
                    sqlQueryTemplate = "SELECT TOP {0} {1} FROM {2} WHERE {3}";
                    openChar = "[";
                    closeChar = "]";
                    break;
                case DatabaseType.MySql:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} LIMIT 0, {0}";
                    openChar = closeChar = "`";
                    break;
                case DatabaseType.PostgreSql:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} LIMIT {0}";
                    openChar = closeChar = "\"";
                    break;
                case DatabaseType.Oracle:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} ROWNUM <= {0}";
                    openChar = closeChar = "\"";
                    break;
                case DatabaseType.DB2:
                    sqlQueryTemplate = "SELECT {1} FROM {2} WHERE {3} FETCH FIRST {0} ROWS ONLY";
                    openChar = closeChar = "\"";
                    break;
                default:
                    throw new NotSupportedException();
            }

            // Build SELECT part
            string selectPart = string.Empty;
            string dbLookupField = lookupField;
            int i = 1;
            const string mapping = "{2}{0}{3} AS {2}{1}{3}, ";

            if (isForPicklist)
            {
                selectPart = string.Format(mapping, "databaseField", dbLookupField, openChar, closeChar);
                selectPart = selectPart.Remove(selectPart.Length - 2);
            }
            else
            {
                foreach (string field in indexFields)
                {
                    if(i%2 == 0)
                    {
                        selectPart += string.Format(mapping, "<<batch.ContentField value>>", field, openChar, closeChar);
                    }
                    else
                    {
                        selectPart += string.Format(mapping, "databaseField" + i, field, openChar, closeChar);
                    }
                    i++;
                }
                selectPart = selectPart.Remove(selectPart.Length - 2);
            }

            // Build WHERE part
            string wherePart = string.Empty;
            if (isForPicklist)
            {
                if (configPicklistValues)
                {
                    wherePart = string.Format("{2}{0}{3} LIKE '%{1}%'", "databaseField", "constant value", openChar, closeChar);
                }
                else
                {
                    i = 1;
                    foreach (string field in indexFields)
                    {
                        if (field != lookupField)
                        {
                            dbLookupField = "databaseField" + i;
                            string condition = string.Format("{2}{0}{3} LIKE '%<<{1}>>%'", dbLookupField, field, openChar, closeChar);
                            wherePart += string.Format(" AND {0}", condition);
                            i++;
                        }
                    }

                    if (wherePart != string.Empty)
                    {
                        wherePart = wherePart.Substring(5);
                    }
                }
            }
            else
            {
                wherePart = string.Format("{2}{0}{3} LIKE '%<<{1}>>%'", indexFields[0], isExample ? "ContentField value" : indexFields[0], openChar, closeChar);
                wherePart += string.Format(" AND {2}{0}{3} LIKE '%<<{1}>>%'", indexFields[1], isExample ? "batch.ContentField value" : indexFields[1], openChar, closeChar);
            }
            
            // Build FROM part
            string fromPart = string.Format("{2}{0}{3}.{2}{1}{3}", dbSchema, sourceName, openChar, closeChar);

            // Return SQL command)
            string sqlQuery = string.Format(sqlQueryTemplate, maxLookupRow, selectPart, fromPart, wherePart);

            if (string.IsNullOrEmpty(wherePart))
            {
                // Trim WHERE token
                switch (dbType)
                {
                    case DatabaseType.MsSql:
                        sqlQuery = sqlQuery.TrimEnd();
                        sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 5);
                        break;
                }
            }

            return sqlQuery;
        }
    }
}