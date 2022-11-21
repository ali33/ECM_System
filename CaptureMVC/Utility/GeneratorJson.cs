using CaptureMVC.Models;
using CaptureMVC.Resources;
using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace CaptureMVC.Utility
{
    /// <summary>
    /// Class helper for search query.
    /// </summary>
    public class SearchUtils
    {
        private const string CONTROL_INPUT_1_VALUE = "<div class=\"input-control text\"><input id=\"value1\" type=\"text\" value=\"{0}\"><button class=\"close\" tabindex=\"-1\">×</button></div>";
        private const string CONTROL_INPUT_2_VALUE = "<div class=\"input-control text number date_first\"><input id=\"value1\" type=\"text\" value=\"{0}\"><button class=\"close\" tabindex=\"-1\">×</button></div><span> - </span><div class=\"input-control text number date_second\"><input id=\"value2\" type=\"text\" value=\"{1}\"><button class=\"close\" tabindex=\"-1\">×</button></div>";
        private const string CONTROL_INPUT_BOOLEAN_TEMPLATE = "<div class=\"input-control select col_center\"><select id=\"value1\">{0}</select></div>";
        private static IList<SearchOperator> SEARCH_OPERATORS_FOR_STRING = new List<SearchOperator>()
                                                                            {
                                                                                SearchOperator.Contains,
                                                                                SearchOperator.EndsWith,
                                                                                SearchOperator.Equal,
                                                                                SearchOperator.NotContains,
                                                                                SearchOperator.NotEqual,
                                                                                SearchOperator.StartsWith
                                                                            };
        private static IList<SearchOperator> SEARCH_OPERATORS_FOR_DATE_DECIMAL_INTEGER = new List<SearchOperator>()
                                                                                        {
                                                                                            SearchOperator.Equal,
                                                                                            SearchOperator.InBetween,
                                                                                            SearchOperator.NotEqual,
                                                                                            SearchOperator.GreaterThan,
                                                                                            SearchOperator.GreaterThanOrEqualTo,
                                                                                            SearchOperator.LessThan,
                                                                                            SearchOperator.LessThanOrEqualTo
                                                                                        };
        private static IList<SearchOperator> SEARCH_OPERATORS_FOR_BOOLEAN_PICKLIST = new List<SearchOperator>()
                                                                            {                                                                               
                                                                                SearchOperator.Equal,                                                                                
                                                                                SearchOperator.NotEqual                                                                              
                                                                            };
        private static IList<SearchOperator> SEARCH_OPERATORS_FOR_NONE = new List<SearchOperator>();
        private static IList<SearchConjunction> SEARCH_CONJUNCTIONS = new List<SearchConjunction>()
                                                                        {                                                                               
                                                                            SearchConjunction.And,
                                                                            SearchConjunction.Or,
                                                                            SearchConjunction.None
                                                                        };

        /// <summary>
        /// Get the HTML control input value in search condition.
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="searchOperator"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static string GetControlInputValue(FieldDataType dataType,
                                                  SearchOperator searchOperator,
                                                  string value1 = null,
                                                  string value2 = null)
        {
            switch (dataType)
            {
                case FieldDataType.String:
                    switch (searchOperator)
                    {
                        case SearchOperator.Contains:
                        case SearchOperator.EndsWith:
                        case SearchOperator.Equal:
                        case SearchOperator.NotContains:
                        case SearchOperator.NotEqual:
                        case SearchOperator.StartsWith:
                            return string.Format(CONTROL_INPUT_1_VALUE, value1);
                    }
                    break;
                case FieldDataType.Date:
                case FieldDataType.Decimal:
                case FieldDataType.Integer:
                    switch (searchOperator)
                    {
                        case SearchOperator.InBetween:
                            return string.Format(CONTROL_INPUT_2_VALUE, value1, value2);
                        case SearchOperator.Equal:
                        case SearchOperator.NotEqual:
                        case SearchOperator.GreaterThan:
                        case SearchOperator.GreaterThanOrEqualTo:
                        case SearchOperator.LessThan:
                        case SearchOperator.LessThanOrEqualTo:
                            return string.Format(CONTROL_INPUT_1_VALUE, value1);
                    }
                    break;
                case FieldDataType.Boolean:
                    switch (searchOperator)
                    {
                        case SearchOperator.Equal:
                        case SearchOperator.NotEqual:
                            return string.Format(CONTROL_INPUT_1_VALUE, value1);
                    }
                    break;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the list search operator for specific data type.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static IList<SearchOperator> GetOperatorsFromType(FieldDataType dataType)
        {
            switch (dataType)
            {
                case FieldDataType.String:
                    return SEARCH_OPERATORS_FOR_STRING;
                case FieldDataType.Date:
                case FieldDataType.Decimal:
                case FieldDataType.Integer:
                    return SEARCH_OPERATORS_FOR_DATE_DECIMAL_INTEGER;
                case FieldDataType.Boolean:
                case FieldDataType.Picklist:
                    return SEARCH_OPERATORS_FOR_BOOLEAN_PICKLIST;
                default:
                    return SEARCH_OPERATORS_FOR_NONE;
            }
        }

        /// <summary>
        /// Get the display string name of search operator
        /// </summary>
        /// <param name="searchOperator"></param>
        /// <returns></returns>
        public static string GetOperatorDisplay(SearchOperator searchOperator)
        {
            switch (searchOperator)
            {
                case SearchOperator.Equal:
                    return QueryResources.Equal;
                case SearchOperator.GreaterThan:
                    return QueryResources.GreaterThan;
                case SearchOperator.GreaterThanOrEqualTo:
                    return QueryResources.GreaterThanOrEqualTo;
                case SearchOperator.LessThan:
                    return QueryResources.LessThan;
                case SearchOperator.LessThanOrEqualTo:
                    return QueryResources.LessThanOrEqualTo;
                case SearchOperator.InBetween:
                    return QueryResources.InBetween;
                case SearchOperator.Contains:
                    return QueryResources.Contains;
                case SearchOperator.NotContains:
                    return QueryResources.NotContains;
                case SearchOperator.NotEqual:
                    return QueryResources.NotEqual;
                case SearchOperator.StartsWith:
                    return QueryResources.StartsWith;
                case SearchOperator.EndsWith:
                    return QueryResources.EndsWith;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Get list search conjunction
        /// </summary>
        /// <returns></returns>
        public static IList<SearchConjunction> GetConjunctions()
        {
            return SEARCH_CONJUNCTIONS;
        }

        /// <summary>
        /// Get the display string of search conjunction
        /// </summary>
        /// <param name="searchConjunction"></param>
        /// <returns></returns>
        public static string GetConjunctionDisplay(SearchConjunction searchConjunction)
        {
            switch (searchConjunction)
            {
                case SearchConjunction.None:
                    return QueryResources.None;
                case SearchConjunction.And:
                    return QueryResources.And;
                case SearchConjunction.Or:
                    return QueryResources.Or;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Get the first operator from list operators by specified type
        /// </summary>
        /// <param name="fieldDataType"></param>
        /// <returns></returns>
        public static SearchOperator GetFirstOperatorFromType(FieldDataType fieldDataType)
        {
            if (fieldDataType == FieldDataType.String)
            {
                return SearchOperator.Contains;
            }
            else
            {
                return SearchOperator.Equal;
            }
        }

        /// <summary>
        /// Get the raw HTML option for select conjunction
        /// </summary>
        /// <returns></returns>
        public static string GetOptionConjuction()
        {
            var optionTemplate = "<option value=\"{0}\">{1}</option>";
            var builder = new StringBuilder();

            // Create search option conjunction
            builder.AppendFormat(optionTemplate,
                                 SearchConjunction.And, SearchUtils.GetConjunctionDisplay(SearchConjunction.And));
            builder.AppendLine();
            builder.AppendFormat(optionTemplate,
                                 SearchConjunction.Or, SearchUtils.GetConjunctionDisplay(SearchConjunction.Or));
            builder.AppendLine();
            builder.AppendFormat(optionTemplate,
                                 SearchConjunction.None, SearchUtils.GetConjunctionDisplay(SearchConjunction.None));

            return builder.ToString();
        }

        /// <summary>
        /// Get the raw HTML option for select operator type string
        /// </summary>
        /// <returns></returns>
        public static string GetOptionOperatorTypeString()
        {
            var optionTemplate = "<option value=\"{0}\">{1}</option>";
            var builder = new StringBuilder();

            // Create search option operator for type string
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.Contains, SearchUtils.GetOperatorDisplay(SearchOperator.Contains));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.NotContains, SearchUtils.GetOperatorDisplay(SearchOperator.NotContains));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.Equal, SearchUtils.GetOperatorDisplay(SearchOperator.Equal));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.NotEqual, SearchUtils.GetOperatorDisplay(SearchOperator.NotEqual));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.StartsWith, SearchUtils.GetOperatorDisplay(SearchOperator.StartsWith));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.EndsWith, SearchUtils.GetOperatorDisplay(SearchOperator.EndsWith));

            return builder.ToString();
        }

        /// <summary>
        /// Get the raw HTML option for select operator type integer and decimal, date.
        /// </summary>
        /// <returns></returns>
        public static string GetOptionOperatorTypeNumber()
        {
            var optionTemplate = "<option value=\"{0}\">{1}</option>";
            var builder = new StringBuilder();

            // Create search option operator for type number
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.Equal, SearchUtils.GetOperatorDisplay(SearchOperator.Equal));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.NotEqual, SearchUtils.GetOperatorDisplay(SearchOperator.NotEqual));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.InBetween, SearchUtils.GetOperatorDisplay(SearchOperator.InBetween));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.GreaterThan, SearchUtils.GetOperatorDisplay(SearchOperator.GreaterThan));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.GreaterThanOrEqualTo, SearchUtils.GetOperatorDisplay(SearchOperator.GreaterThanOrEqualTo));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.LessThan, SearchUtils.GetOperatorDisplay(SearchOperator.LessThan));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.LessThanOrEqualTo, SearchUtils.GetOperatorDisplay(SearchOperator.LessThanOrEqualTo));

            return builder.ToString();
        }

        /// <summary>
        /// Get the raw HTML option for select operator type bool and picklist.
        /// </summary>
        /// <returns></returns>
        public static string GetOptionOperatorTypeBoolean()
        {
            var optionTemplate = "<option value=\"{0}\">{1}</option>";
            var builder = new StringBuilder();

            // Create search option operator for type number
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.Equal, SearchUtils.GetOperatorDisplay(SearchOperator.Equal));
            builder.AppendFormat(optionTemplate,
                                 SearchOperator.NotEqual, SearchUtils.GetOperatorDisplay(SearchOperator.NotEqual));

            return builder.ToString();
        }

        /// <summary>
        /// Get the raw HTML control input for 1 value.
        /// </summary>
        /// <param name="value1"></param>
        /// <returns></returns>
        public static string GetControlInput1Value(string value1 = null)
        {
            return string.Format(CONTROL_INPUT_1_VALUE, value1);
        }

        /// <summary>
        /// Get the raw HTML control input for 2 value.
        /// </summary>
        /// <param name="value1"></param>
        /// <returns></returns>
        public static string GetControlInput2Value(string value1 = null, string value2 = null)
        {
            return string.Format(CONTROL_INPUT_2_VALUE, value1, value2);
        }

        /// <summary>
        /// Get the raw HTML control input for 2 value.
        /// </summary>
        /// <param name="value1"></param>
        /// <returns></returns>
        public static string GetControlInputBoolean()
        {
            var builderOption = new StringBuilder();
            builderOption.Append("<option value=\"\" selected></option>");
            //if (value1.HasValue)
            //{
            //    if (value1.Value)
            //    {
            //        builderOption.AppendFormat("<option value=\"{0}\" selected>{1}</option>", true, CommonResources.True);
            //        builderOption.AppendFormat("<option value=\"{0}\">{1}</option>", false, CommonResources.False);
            //    }
            //    else
            //    {
            //        builderOption.AppendFormat("<option value=\"{0}\">{1}</option>", true, CommonResources.True);
            //        builderOption.AppendFormat("<option value=\"{0}\" selected>{1}</option>", false, CommonResources.False);
            //    }
            //}
            //else
            //{
            builderOption.AppendFormat("<option value=\"{0}\">{1}</option>", true, CommonResources.True);
            builderOption.AppendFormat("<option value=\"{0}\">{1}</option>", false, CommonResources.False);
            //}

            return string.Format(CONTROL_INPUT_BOOLEAN_TEMPLATE, builderOption.ToString());
        }
    }

    /// <summary>
    /// Class helper for field
    /// </summary>
    public class FieldUtils
    {
        public const string DOCUMENT_COUNT = "Doc count";
        public const string PAGE_COUNT = "Page count";
        public const string CREATE_BY = "Created by";
        public const string CREATE_ON = "Created on";
        public const string MODIFIED_BY = "Modified by";
        public const string MODIFIED_ON = "Modified on";
        public const string BATCH_NAME = "Batch name";

        public const string COLUMN_DOCUMENT_COUNT = "COLUMN_DOCUMENT_COUNT";
        public const string COLUMN_PAGE_COUNT = "COLUMN_PAGE_COUNT";
        public const string COLUMN_CREATE_BY = "COLUMN_CREATE_BY";
        public const string COLUMN_CREATE_DATE = "COLUMN_CREATE_ON";

        public const string COLUMN_LOCKED_BY = "COLUMN_LOCKED_BY";
        public const string COLUMN_ACTIVITY_NAME = "COLUMN_ACTIVITY_NAME";
        public const string COLUMN_BLOCKING_DATE = "COLUMN_BLOCKING_DATE";
        public const string COLUMN_LAST_ACCESS_BY = "COLUMN_LAST_ACCESS_BY";
        public const string COLUMN_LAST_ACCESS_DATE = "COLUMN_LAST_ACCESS_DATE";
        public const string COLUMN_COMPLETED = "COLUMN_COMPLETED";
        public const string COLUMN_PROCESSING = "COLUMN_PROCESSING";
        public const string COLUMN_HAS_ERROR = "COLUMN_HAS_ERROR";
        public const string COLUMN_STATUS = "COLUMN_STATUS";

        public const string COLUMN_GUID = "GUID";

        /// <summary>
        /// Get the display name of system field name
        /// </summary>
        /// <param name="systemFieldName"></param>
        /// <returns></returns>
        public static string MapSystemFieldNameDisplay(string systemFieldName)
        {
            switch (systemFieldName)
            {
                case DOCUMENT_COUNT:
                case COLUMN_DOCUMENT_COUNT:
                    return FieldResources.Doc_count;
                case PAGE_COUNT:
                case COLUMN_PAGE_COUNT:
                    return FieldResources.Page_count;
                case CREATE_BY:
                case COLUMN_CREATE_BY:
                    return FieldResources.Created_by;
                case CREATE_ON:
                case COLUMN_CREATE_DATE:
                    return FieldResources.Created_on;
                case MODIFIED_BY:
                    return FieldResources.Modified_by;
                case MODIFIED_ON:
                    return FieldResources.Modified_on;
                case BATCH_NAME:
                    return FieldResources.Batch_name;

                case COLUMN_LOCKED_BY:
                    return FieldResources.Locked_by;
                case COLUMN_ACTIVITY_NAME:
                    return FieldResources.Activity_name;
                case COLUMN_BLOCKING_DATE:
                    return FieldResources.Blocking_date;
                case COLUMN_LAST_ACCESS_BY:
                    return FieldResources.Last_access_by;
                case COLUMN_LAST_ACCESS_DATE:
                    return FieldResources.Last_access_date;
                case COLUMN_COMPLETED:
                    return FieldResources.Complete;
                case COLUMN_PROCESSING:
                    return FieldResources.Processing;
                case COLUMN_HAS_ERROR:
                    return FieldResources.Has_error;
                case COLUMN_STATUS:
                    return FieldResources.Status;

                default:
                    return systemFieldName;
            }
        }

        /// <summary>
        /// Get the system type.
        /// </summary>
        /// <param name="fieldDataType">string FieldDataType</param>
        /// <returns></returns>
        public static Type GetSystemType(FieldDataType fieldDataType)
        {
            switch (fieldDataType)
            {
                case FieldDataType.String:
                    return typeof(string);
                case FieldDataType.Integer:
                    return typeof(int);
                case FieldDataType.Decimal:
                    return typeof(decimal);
                case FieldDataType.Picklist:
                    return typeof(Enum);
                case FieldDataType.Boolean:
                    return typeof(bool);
                case FieldDataType.Date:
                    return typeof(DateTime);
                case FieldDataType.Folder:
                    return typeof(DirectoryInfo);
                case FieldDataType.Table:
                    return typeof(DataTable);
                default:
                    return null;
            }
        }
    }

    public class GeneratorJson
    {
        public static List<Dictionary<string, object>> GetJson(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new

            System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows =
              new List<Dictionary<string, object>>();
            Dictionary<string, object> row = null;

            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName.Trim().Replace("_4E19573E_D42E_4B74_BB81_E3EF56633947", ""), dr[col]);
                    if (col.ColumnName.Replace("_4E19573E_D42E_4B74_BB81_E3EF56633947", "").Equals("DocumentId"))
                    {
                        row.Add("DocumentIdEncrypt", Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(dr[col].ToString()));
                    }
                }
                rows.Add(row);
            }
            return rows;
            //return serializer.Serialize(rows);
        }
        public static string CreateConjunction()
        {
            var sa = new Dictionary<int, String>();
            foreach (Ecm.CaptureDomain.SearchConjunction dataType in Enum.GetValues(typeof(Ecm.CaptureDomain.SearchConjunction)))
            {
                sa.Add((int)dataType, dataType.ToString());


            }
            return "{Conjunctions:[" + String.Join(",", sa.Select(p => "{ID:" + p.Key + ",Name:'" + p.Value + "'}").ToArray()) + "]}";
        }

        public static string CreateDataType()
        {
            var sa = new Dictionary<int, String>();
            foreach (Ecm.CaptureDomain.FieldDataType dataType in Enum.GetValues(typeof(Ecm.CaptureDomain.FieldDataType)))
            {
                sa.Add((int)dataType, dataType.ToString());


            }
            return "{DataTypes:[" + String.Join(",", sa.Select(p => "{Type:" + p.Key + ",Name:'" + p.Value + "'}").ToArray()) + "]}";
        }

        static string CreateOperatorItems_From_DataType(Ecm.CaptureDomain.FieldDataType dataType)
        {
            String strOperatorItems = @"{{
                                            ID:{0},
                                            Name: '{1}',
                                            MumberControl: {2},
                                            Control: '{3}',
                                            IsDate: {4}
                                        }}";
            List<string> listOperatorItems = new List<string>();

            switch (dataType)
            {
                case Ecm.CaptureDomain.FieldDataType.String:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.Contains, Ecm.CaptureDomain.SearchOperator.Contains, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.EndsWith, Ecm.CaptureDomain.SearchOperator.EndsWith, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.Equal, Ecm.CaptureDomain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.NotContains, Ecm.CaptureDomain.SearchOperator.NotContains, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.NotEqual, Ecm.CaptureDomain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.StartsWith, Ecm.CaptureDomain.SearchOperator.StartsWith, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));

                    break;
                case Ecm.CaptureDomain.FieldDataType.Date:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.Equal, Ecm.CaptureDomain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.InBetween, Ecm.CaptureDomain.SearchOperator.InBetween, 2, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.NotEqual, Ecm.CaptureDomain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.GreaterThan, Ecm.CaptureDomain.SearchOperator.GreaterThan, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.GreaterThanOrEqualTo, Ecm.CaptureDomain.SearchOperator.GreaterThanOrEqualTo, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.LessThan, Ecm.CaptureDomain.SearchOperator.LessThan, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.LessThanOrEqualTo, Ecm.CaptureDomain.SearchOperator.LessThanOrEqualTo, 1, "<input  id=\"value1\" type=\"text\" value=\"\">", 1));
                    break;
                case Ecm.CaptureDomain.FieldDataType.Decimal:
                case Ecm.CaptureDomain.FieldDataType.Integer:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.Equal, Ecm.CaptureDomain.SearchOperator.Equal, 1, "<input  id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.InBetween, Ecm.CaptureDomain.SearchOperator.InBetween, 2, "<input  id=\"value1\" type=\"text\" value=\"0\"> <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.NotEqual, Ecm.CaptureDomain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.GreaterThan, Ecm.CaptureDomain.SearchOperator.GreaterThan, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.GreaterThanOrEqualTo, Ecm.CaptureDomain.SearchOperator.GreaterThanOrEqualTo, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.LessThan, Ecm.CaptureDomain.SearchOperator.LessThan, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.LessThanOrEqualTo, Ecm.CaptureDomain.SearchOperator.LessThanOrEqualTo, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    break;
                case Ecm.CaptureDomain.FieldDataType.Boolean:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.Equal, Ecm.CaptureDomain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.CaptureDomain.SearchOperator.NotEqual, Ecm.CaptureDomain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    break;
            }



            return String.Join(",", listOperatorItems);

        }

        public static string GetControlInputValue(FieldDataType dataType, SearchOperator searchOperator, string value1, string value2)
        {
            switch (dataType)
            {
                case FieldDataType.String:
                    switch (searchOperator)
                    {
                        case SearchOperator.Contains:
                        case SearchOperator.EndsWith:
                        case SearchOperator.Equal:
                        case SearchOperator.NotContains:
                        case SearchOperator.NotEqual:
                        case SearchOperator.StartsWith:
                            return "<div class=\"input-control text \" ><input id=\"value1\" type=\"text\" value=\"" + value1 + "\">  <button class=\"close\" tabindex=\"-1\">×</button></div>";
                    }
                    break;
                case FieldDataType.Date:
                case FieldDataType.Decimal:
                case FieldDataType.Integer:
                    switch (searchOperator)
                    {
                        case SearchOperator.InBetween:
                            return "<div class=\"input-control text number date_first\" ><input id=\"value1\" type=\"text\" value=\"" + value1 + "\"><button class=\"close\" tabindex=\"-1\">×</button></div> <span> - </span> <div class=\"input-control text number date_second\"><input id=\"value2\" type=\"text\" value=\"" + value2 + "\"><button class=\"close\" tabindex=\"-1\">×</button></div>";
                        case SearchOperator.Equal:
                        case SearchOperator.NotEqual:
                        case SearchOperator.GreaterThan:
                        case SearchOperator.GreaterThanOrEqualTo:
                        case SearchOperator.LessThan:
                        case SearchOperator.LessThanOrEqualTo:
                            return "<div class=\"input-control text \" ><input id=\"value1\" type=\"text\" value=\"" + value1 + "\">  <button class=\"close\" tabindex=\"-1\">×</button></div>";
                    }
                    break;
                case FieldDataType.Boolean:
                    switch (searchOperator)
                    {
                        case SearchOperator.Equal:
                        case SearchOperator.NotEqual:
                            return "<div class=\"input-control text \" ><input id=\"value1\" type=\"text\" value=\"" + value1 + "\">  <button class=\"close\" tabindex=\"-1\">×</button></div>";
                    }
                    break;
            }

            return String.Empty;
        }

        public static List<SearchOperator> GetOperatorFromType(FieldDataType dataType)
        {

            List<SearchOperator> listOperatorItems = new List<SearchOperator>();

            switch (dataType)
            {
                case FieldDataType.String:
                    listOperatorItems.Add(SearchOperator.Contains);
                    listOperatorItems.Add(SearchOperator.EndsWith);
                    listOperatorItems.Add(SearchOperator.Equal);
                    listOperatorItems.Add(SearchOperator.NotContains);
                    listOperatorItems.Add(SearchOperator.NotEqual);
                    listOperatorItems.Add(SearchOperator.StartsWith);
                    break;
                case FieldDataType.Date:
                case FieldDataType.Decimal:
                case FieldDataType.Integer:
                    listOperatorItems.Add(SearchOperator.Equal);
                    listOperatorItems.Add(SearchOperator.InBetween);
                    listOperatorItems.Add(SearchOperator.NotEqual);
                    listOperatorItems.Add(SearchOperator.GreaterThan);
                    listOperatorItems.Add(SearchOperator.GreaterThanOrEqualTo);
                    listOperatorItems.Add(SearchOperator.LessThan);
                    listOperatorItems.Add(SearchOperator.LessThanOrEqualTo);
                    break;
                case FieldDataType.Boolean:
                    listOperatorItems.Add(SearchOperator.Equal);
                    listOperatorItems.Add(SearchOperator.NotEqual);
                    break;
            }



            return listOperatorItems;
        }


        static string CreateDataType_Item_From_DataType(Ecm.CaptureDomain.FieldDataType dataType)
        {
            String strData_Item = @"{{
    TypeID:{0},
    TypeName:'{1}',
    OperatorItems: [{2}]
}}";



            return String.Format(strData_Item,
                                (int)dataType,
                                dataType.ToString(),
                                CreateOperatorItems_From_DataType(dataType)
                                 );
        }
        public static string CreateDataType_Item()
        {
            String strDataType = @"{{DataType_Items:[
    {0}
    ]
}}";

            List<string> list_item = new List<string>();
            foreach (Ecm.CaptureDomain.FieldDataType dataType in Enum.GetValues(typeof(Ecm.CaptureDomain.FieldDataType)))
            {
                //sa.Add((int)dataType, dataType.ToString());
                list_item.Add(CreateDataType_Item_From_DataType(dataType));
            }
            return String.Format(strDataType, String.Join(",", list_item));


        }



        /// <summary>
        /// Get the display string of search conjunction
        /// </summary>
        /// <param name="searchConjunction"></param>
        /// <returns></returns>
        public static string GetConjunctionDisplay(SearchConjunction searchConjunction)
        {
            switch (searchConjunction)
            {
                case SearchConjunction.None:
                    return QueryResources.None;
                case SearchConjunction.And:
                    return QueryResources.And;
                case SearchConjunction.Or:
                    return QueryResources.Or;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Get the first operator from list operators by specified type
        /// </summary>
        /// <param name="fieldDataType"></param>
        /// <returns></returns>
        public static SearchOperator GetFirstOperatorFromType(FieldDataType fieldDataType)
        {
            if (fieldDataType == FieldDataType.String)
            {
                return SearchOperator.Contains;
            }
            else
            {
                return SearchOperator.Equal;
            }
        }

        /// <summary>
        /// Get enum value from string value
        /// </summary>
        /// <param name="conjucntionString"></param>
        /// <returns></returns>
        public static SearchConjunction GetSearhConjunction(string conjucntionString)
        {
            string conjunction;
            if (string.IsNullOrWhiteSpace(conjucntionString))
            {
                conjunction = string.Empty;
            }
            else
            {
                conjunction = conjucntionString.ToLower();
            }

            switch (conjunction)
            {
                case "and":
                    return SearchConjunction.And;
                case "or":
                    return SearchConjunction.Or;
                case "none":
                    return SearchConjunction.None;
                default:
                    throw new ArgumentException("Invalid value for enum SearchConjunction");
            }
        }
    }
}