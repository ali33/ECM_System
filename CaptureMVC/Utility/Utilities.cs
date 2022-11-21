using CaptureMVC.Models;
using Ecm.CaptureDomain;
using Ecm.ScriptEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace CaptureMVC.Utility
{
    public class Utilities
    {
        public static string UserName
        {
            get
            {
                return GetSession(Constant.UserName) as string;
            }
            set
            {
                SetSession(Constant.UserName, value);
            }
        }
        public static string Password
        {
            get
            {
                return GetSession(Constant.Password) as string;
            }
            set
            {
                SetSession(Constant.Password, value);
            }
        }
        public static string RawPassword
        {
            get
            {
                return GetSession(Constant.RAW_PASSWORD).ToString();
            }
            set
            {
                SetSession(Constant.RAW_PASSWORD, value);
            }
        }
        public static bool IsAdmin
        {
            get
            {
                return (bool)GetSession(Constant.IS_ADMIN);
            }
            set
            {
                SetSession(Constant.IS_ADMIN, value);
            }
        }
        public static Language Language
        {
            get
            {
                return GetSession(Constant.LANGUAGE) as Language;
            }
            set
            {
                SetSession(Constant.LANGUAGE, value);
            }
        }
        public static Guid UserId
        {
            get
            {
                return (Guid)GetSession(Constant.UserID);
            }
            set
            {
                SetSession(Constant.UserID, value);
            }
        }
        public static int ItemsPerPage
        {
            get
            {
                var itemsPerPage = GetSession(Constant.ITEMS_PER_PAGE);
                if (null != itemsPerPage)
                {
                    return (int)itemsPerPage;
                }

                return 0;
            }
            set
            {
                SetSession(Constant.ITEMS_PER_PAGE, value);
            }
        }

        public static object GetSession(string key)
        {
            return System.Web.HttpContext.Current.Session[key];
        }
        public static void SetSession(string key, object value)
        {
            System.Web.HttpContext.Current.Session[key] = value;
        }

        public static int GetRotateAngle(int angle)
        {
            if (angle < 0)
            {
                do
                {
                    angle += 360;
                } while (angle < 0);
            }
            else if (angle > 359)
            {
                do
                {
                    angle -= 360;
                } while (angle > 359);
            }

            return angle;
        }

        public static Color GetRedactionBackground()
        {
            return Color.FromArgb(102, 102, 102);
        }

        /// <summary>
        /// Get the path of temp file folder store in app config
        /// </summary>
        /// <returns></returns>
        public static string GetFolderTempFiles()
        {
            return string.Format("{0}", ConfigurationManager.AppSettings[Constant.APP_KEY_FOLDER_TEMP_FILES]);
        }

        /// <summary>
        /// Validate value of field.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool ValidateFieldValue(ViewSaveIndexModel index, BatchFieldMetaData field)
        {
            if (string.IsNullOrWhiteSpace(index.Value))
            {
                index.Value = null;
                return true;
            }

            switch (field.DataTypeEnum)
            {
                case FieldDataType.String:
                    if (index.Value.Length > field.MaxLength)
                    {
                        return false;
                    }
                    break;

                case FieldDataType.Integer:
                    int intValue;
                    if (!int.TryParse(index.Value, out intValue))
                    {
                        return false;
                    }
                    break;

                case FieldDataType.Decimal:
                    decimal decimalValue;
                    index.Value = index.Value.Replace(",", ".");
                    if (!decimal.TryParse(index.Value,
                                          NumberStyles.None, CultureInfo.InvariantCulture, out decimalValue))
                    {
                        return false;
                    }
                    break;

                case FieldDataType.Boolean:
                    bool boolValue;
                    if (!bool.TryParse(index.Value, out boolValue))
                    {
                        return false;
                    }
                    break;

                case FieldDataType.Date:
                    DateTime dateValue;
                    if (!DateTime.TryParseExact(index.Value, "yyyy-MM-dd",
                                               CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                    {
                        return false;
                    }
                    index.Value = dateValue.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    break;

                default:
                    return false;
            }


            return true;
        }

        /// <summary>
        /// Validate value of field.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool ValidateFieldValue(ViewSaveIndexModel index, DocumentFieldMetaData field,
                                              List<TableFieldValue> tables, Guid docId)
        {
            if (field.DataTypeEnum == FieldDataType.Picklist)
            {
                #region Pick list type

                // Check mandatory
                if (string.IsNullOrWhiteSpace(index.Value))
                {
                    if (field.IsRequired)
                    {
                        return false;
                    }
                    else
                    {
                        index.Value = null;
                        return true;
                    }
                }

                var picklistItem = field.Picklists.SingleOrDefault(h => h.Id.ToString() == index.Value);
                if (picklistItem == null)
                {
                    return false;
                }
                else
                {
                    index.Value = picklistItem.Value;
                    return true;
                }

                #endregion
            }
            else if (field.DataTypeEnum == FieldDataType.Table)
            {
                //#region Table type

                var newTables = new List<TableFieldValue>();

                for (int i = 0; i < index.Rows.Count; i++)
                {
                    var row = index.Rows[i];
                    for (int j = 0; j < row.Cols.Count; j++)
                    {
                        var col = row.Cols[j];
                        var flgIsNew = false;
                        var fieldCol = field.Children.SingleOrDefault(h => h.Id.ToString() == col.FieldId);

                        if (fieldCol == null)
                        {
                            continue;
                        }
                        TableFieldValue cell;
                        if (col.Id.StartsWith("new-id-"))
                        {
                            try
                            {
                                cell = new TableFieldValue();
                                cell.Id = Guid.NewGuid();
                                cell.DocId = docId;
                                cell.Field = fieldCol;
                                cell.FieldId = cell.Field.Id;
                                flgIsNew = true;
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            cell = tables.SingleOrDefault(h => h.Id.ToString() == col.Id);
                        }

                        if (cell == null)
                        {
                            continue;
                        }

                        var colValue = string.Format("{0}", col.Value);
                        if (string.IsNullOrWhiteSpace(colValue))
                        {
                            cell.RowNumber = i;
                            cell.Value = null;
                            newTables.Add(cell);
                            continue;
                        }

                        var flgIsValid = true;
                        switch (cell.Field.DataTypeEnum)
                        {
                            case FieldDataType.String:
                                if (colValue.Length > fieldCol.MaxLength)
                                {
                                    flgIsValid = false;
                                }
                                break;

                            case FieldDataType.Integer:
                                int intValue;
                                if (!int.TryParse(colValue, out intValue))
                                {
                                    flgIsValid = false;
                                }
                                break;

                            case FieldDataType.Decimal:
                                decimal decimalValue;
                                colValue = colValue.Replace(",", ".");
                                if (!decimal.TryParse(colValue,
                                                      NumberStyles.None, CultureInfo.InvariantCulture,
                                                      out decimalValue))
                                {
                                    flgIsValid = false;
                                }
                                break;

                            case FieldDataType.Date:
                                DateTime dateValue;
                                if (!DateTime.TryParseExact(colValue, "yyyy-MM-dd",
                                                           CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                           out dateValue))
                                {
                                    flgIsValid = false;
                                }
                                colValue = dateValue.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                break;

                            default:
                                flgIsValid = false;
                                break;
                        }

                        if (flgIsValid)
                        {
                            cell.RowNumber = i;
                            cell.Value = colValue;
                        }
                        else if (flgIsNew)
                        {
                            continue;
                        }

                        newTables.Add(cell);
                    }
                }

                tables.Clear();
                tables.AddRange(newTables);
                return true;
            }
            else if (field.DataTypeEnum == FieldDataType.Folder)
            {
                return false;
            }
            else
            {
                #region String, Int, Decimal, Boolean, Date data type
                // Check mandatory
                if (string.IsNullOrWhiteSpace(index.Value))
                {
                    if (field.IsRequired)
                    {
                        return false;
                    }
                    else
                    {
                        index.Value = null;
                        return true;
                    }
                }

                // Check value data type
                if (field.DataTypeEnum == FieldDataType.Integer)
                {
                    // Check value data type
                    int tempInt;
                    if (!int.TryParse(index.Value, out tempInt))
                    {
                        return false;
                    }
                }
                else if (field.DataTypeEnum == FieldDataType.Decimal)
                {
                    // Check value data type
                    decimal tempDecimal;
                    index.Value = index.Value.Replace(",", ".");
                    if (!decimal.TryParse(index.Value, NumberStyles.None, CultureInfo.InvariantCulture,
                                          out tempDecimal))
                    {
                        return false;
                    }
                }
                else if (field.DataTypeEnum == FieldDataType.Boolean)
                {
                    bool tempBool;
                    if (!bool.TryParse(index.Value, out tempBool))
                    {
                        return false;
                    }
                }
                else if (field.DataTypeEnum == FieldDataType.Date)
                {
                    DateTime tempDate;
                    if (!DateTime.TryParseExact(index.Value, "yyyy-MM-dd", null, DateTimeStyles.None, out tempDate))
                    {
                        return false;
                    }
                    index.Value = tempDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else // Case string type
                {
                    if (index.Value.Length > field.MaxLength)
                    {
                        return false;
                    }
                }
                #endregion

                // Validate script
                if (!string.IsNullOrWhiteSpace(field.ValidationScript))
                {
                    var scriptValue = field.ValidationScript.Replace("<<Value>>", index.Value);
                    var script = CSharpScriptEngine.script.Replace("<<ScriptHere>>", scriptValue);
                    var ass = CSharpScriptEngine.CompileCode(script);

                    if (!CSharpScriptEngine.RunScript(ass))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Setting Settings
        {
            get
            {
                return (Setting)GetSession(Constant.SETTINGS);
            }
            set
            {
                SetSession(Constant.SETTINGS, value);
            }
        }

        /// <summary>
        /// Get long string date time with format of supported language (En, Vi)
        /// </summary>
        /// <param name="date">Date time to string</param>
        /// <returns>Long string date.</returns>
        public static string GetLongStringDate(DateTime date)
        {
            var language = Language;

            if (language == null)
            {
                return date.ToString("M/d/yyyy h:mm:ss tt");
            }

            // English
            if ("English".Equals(language.Name, StringComparison.OrdinalIgnoreCase))
            {
                return date.ToString("M/d/yyyy h:mm:ss tt");
            }

            // Vietnamese
            if ("Vietnamese".Equals(language.Name, StringComparison.OrdinalIgnoreCase))
            {
                return date.ToString("d/M/yyyy h:mm:ss tt");
            }

            // Default
            return date.ToString("M/d/yyyy h:mm:ss tt");
        }


    }
}