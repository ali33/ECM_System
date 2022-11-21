using System;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using System.ServiceModel;
using Ecm.Audit;
using Ecm.Utility.Exceptions;
using System.Collections.ObjectModel;
using Ecm.Model;
using System.Collections.Generic;

namespace Ecm.Audit.ViewModel
{
    public class ProcessHelper
    {
        public static void ProcessException(Exception ex)
        {
            if (ex is EndpointNotFoundException || ex is CommunicationException || ex is TimeoutException)
            {
                DialogService.ShowErrorDialog(Resources.uiConnectFail);
            }
            else if (ex is WcfException)
            {
                DialogService.ShowErrorDialog(ex.Message);
            }
            else
            {
                using (var loggingClient = ProviderBase.GetLoggingClientChannel())
                {
                    try
                    {
                        loggingClient.Channel.Log(ex.Message, ex.StackTrace);
                    }
                    catch
                    {
                    }
                }

                if (ex is TwainException)
                {
                    DialogService.ShowErrorDialog(ex.Message);
                }
                else
                {
                    DialogService.ShowErrorDialog(Resources.uiGeneralError);
                }
            }
        }

        public static string BuildSearchExpression(ObservableCollection<SearchExpressionViewModel> searchs)
        {
            string expression = string.Empty;

            foreach (var searchViewModel in searchs)
            {
                string strExpr = string.Empty;
                string operatorValuePair = string.Empty;
                string name = searchViewModel.Search.Name.Trim().Replace(" ", string.Empty);
                string value = searchViewModel.Search.Value;
                string value1 = searchViewModel.Search.Value1;
                string searchOperator = searchViewModel.Search.Operator;
                string searchCondition = searchViewModel.Search.Condition;
                string dataType = searchViewModel.Search.DataType;
                string format = "yyyy-MM-dd HH:mm:ss";

                if (!string.IsNullOrEmpty(searchViewModel.Search.Value))
                {
                    if (searchViewModel.Search.Condition != null && !string.IsNullOrEmpty(expression))
                    {
                        strExpr += " " + searchCondition + " ";
                    }

                    if (searchOperator == "InBetween")
                    {
                        //patent Field > @Field AND Field < @Field1
                        string date1 = Convert.ToDateTime(value + " 00:00:00").ToString(format);
                        string date2 = Convert.ToDateTime(value1 + " 23:59:59").ToString(format);
                        strExpr += name + " >= '" + date1 + "' AND " + name + " <= '" + date2 + "' ";
                    }
                    else
                    {
                        if (searchOperator == Common.CONTAINS)
                        {
                            operatorValuePair = name + " like '%" + value + "%' ";
                        }
                        else if (searchOperator == Common.NOT_CONTAINS)
                        {
                            operatorValuePair = name + " not like '%" + value + "%' ";
                        }
                        else if (searchOperator == Common.ENDS_WITH)
                        {
                            operatorValuePair = name + " like '%" + value + "' ";
                        }
                        else if (searchOperator == Common.STARTS_WITH)
                        {
                            operatorValuePair = name + " like '" + value + "%' ";
                        }
                        //else if (searchOperator == Common.GREATER_THAN)
                        //{
                        //    operatorValuePair = name + GetOperatorSymbol(searchOperator) + value;
                        //}
                        //else if (searchOperator == Common.LESS_THAN)
                        //{
                        //    operatorValuePair = name + GetOperatorSymbol(searchOperator) + value;
                        //}
                        //else if(searchOperator == Common.GREATER_THAN_OR_EQUAL_TO)
                        //{
                        //    operatorValuePair = name + GetOperatorSymbol(searchOperator) + value;
                        //}
                        //else if(searchOperator == Common.LESS_THAN_OR_EQUAL_TO)
                        //{
                        //    operatorValuePair = name + GetOperatorSymbol(searchOperator) + value;
                        //}
                        else
                        {
                            if (dataType == "Date")
                            {
                                string date1 = Convert.ToDateTime(value + " 00:00:00").ToString(format);
                                string date2 = Convert.ToDateTime(value + " 23:59:59").ToString(format);
                                if (searchOperator == Common.EQUAL)
                                {
                                    operatorValuePair = name + " >= '" + date1 + "' AND " + name + " <= '" + date2 + "' ";
                                }
                                else if(searchOperator == Common.NOT_EQUAL)
                                {
                                    operatorValuePair = name + " < '" + date1 + "' OR " + name + " > '" + date2 + "' ";

                                }
                            }
                            else
                            {
                                if (dataType == "Date")
                                {
                                    if(searchOperator == Common.GREATER_THAN || searchOperator == Common.LESS_THAN_OR_EQUAL_TO)
                                    {
                                        value = Convert.ToDateTime(value + " 23:59:59").ToString(format);
                                    }
                                    else if(searchOperator == Common.GREATER_THAN_OR_EQUAL_TO || searchOperator == Common.LESS_THAN)
                                    {
                                        value = Convert.ToDateTime(value + " 00:00:00").ToString(format);
                                    }
                                }

                                operatorValuePair = name + " " + GetOperatorSymbol(searchOperator) + " '" + value + "' ";
                            }
                        }

                        strExpr += operatorValuePair;
                    }

                    expression += strExpr;
                }
            }

            if (expression.StartsWith("AND"))
            {
                expression = expression.Substring(3);
            }

            if (expression.StartsWith("OR"))
            {
                expression = expression.Substring(2);
            }

            if (expression.EndsWith("AND"))
            {
                expression = expression.Substring(0, expression.Length - 3);
            }

            if (expression.EndsWith("OR"))
            {
                expression = expression.Substring(0, expression.Length - 2);
            }

            return expression;
        }

        public static string BuildSearchExpression2(ObservableCollection<SearchExpressionViewModel> searchs)
        {
            string expression = string.Empty;
            foreach (SearchExpressionViewModel searchExpressionViewModel in searchs)
            {
                if(!string.IsNullOrEmpty(searchExpressionViewModel.Search.Value))
                {
                    string itemName = searchExpressionViewModel.Search.Name.Trim().Replace(" ", string.Empty);
                    if(!string.IsNullOrEmpty(searchExpressionViewModel.Search.Condition))
                    {
                        if(!string.IsNullOrEmpty(expression))
                        {
                            expression += " " + searchExpressionViewModel.Search.Condition + " ";
                        }
                    }
                    if(searchExpressionViewModel.Search.DataType.ToLower() == "string")
                    {
                        switch(searchExpressionViewModel.Search.Operator)
                        {
                            case Common.CONTAINS:
                                expression += itemName + " like '%" + searchExpressionViewModel.Search.Value + "%' ";
                                break;
                            case Common.NOT_CONTAINS:
                                expression += itemName + " not like '%" + searchExpressionViewModel.Search.Value + "%' ";
                                break;
                            case Common.ENDS_WITH:
                                expression += itemName + " like '%" + searchExpressionViewModel.Search.Value + "' ";
                                break;
                            case Common.STARTS_WITH:
                                expression += itemName + " like '" + searchExpressionViewModel.Search.Value + "%' ";
                                break;
                            case Common.EQUAL:
                                expression += itemName + " = '" + searchExpressionViewModel.Search.Value + "' ";
                                break;
                            case Common.NOT_EQUAL:
                                expression += itemName + " <> '" + searchExpressionViewModel.Search.Value + "' ";
                                break;
                            default:
                                expression += itemName + " like '%" + searchExpressionViewModel.Search.Value + "%' ";
                                break;
                        }
                    }
                    else if (searchExpressionViewModel.Search.DataType.ToLower() == "date")
                    {
                        switch (searchExpressionViewModel.Search.Operator)
                        {
                            case Common.EQUAL:
                                expression += "(";
                                expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += "AND ";
                                expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += ") ";
                                break;
                            case Common.NOT_EQUAL:
                                expression += "(";
                                expression += itemName + " < '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += "OR ";
                                expression += itemName + " > '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += ")";
                                break;
                            case Common.GREATER_THAN:
                                expression += itemName + " > '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case Common.GREATER_THAN_OR_EQUAL_TO:
                                expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case Common.LESS_THAN:
                                expression += itemName + " < '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case Common.LESS_THAN_OR_EQUAL_TO:
                                expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case Common.IN_BETWEEN:
                                if (!string.IsNullOrEmpty(searchExpressionViewModel.Search.Value1))
                                {
                                    expression += "(";
                                    expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                    expression += "AND ";
                                    expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                    expression += ") ";
                                }
                                break;
                            default:
                                expression += "(";
                                expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += "AND ";
                                expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.Search.Value + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += ") ";
                                break;
                        }
                    }
                    
                }
            }

            return expression;
        }

        private static Dictionary<string, object> GetParameterValues(ObservableCollection<SearchExpressionViewModel> searchs)
        {
            Dictionary<string, object> parameterValue = new Dictionary<string, object>();
            foreach (var searchViewModel in searchs)
            {
                if (string.IsNullOrEmpty(searchViewModel.Search.Value))
                {
                    parameterValue.Add(searchViewModel.Search.Name.Trim(), searchViewModel.Search.Value);
                    if (searchViewModel.Search.Operator == "InBetween")
                    {
                        parameterValue.Add(searchViewModel.Search.Name.Trim() + "1", searchViewModel.Search.Value1);
                    }
                }
            }

            return parameterValue;
        }

        private static string GetOperatorSymbol(string operatorWord)
        {
            switch (operatorWord)
            {
                case Common.EQUAL:
                    return "=";
                case Common.GREATER_THAN:
                    return ">";
                case Common.GREATER_THAN_OR_EQUAL_TO:
                    return ">=";
                case Common.LESS_THAN:
                    return "<";
                case Common.LESS_THAN_OR_EQUAL_TO:
                    return "<=";
                case Common.NOT_EQUAL:
                    return "!=";
                default:
                    return null;
            }
        }

    }
}
