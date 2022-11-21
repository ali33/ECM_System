using System;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ecm.CaptureModel;
using Ecm.CaptureDomain;

namespace Ecm.Capture.ViewModel
{
    public class ProcessHelper
    {
        private static ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());
        public static void ProcessException(Exception ex)
        {
            if (ex.Message == "Work item is have no longer existed.")
            {
                DialogService.ShowErrorDialog(_resource.GetString("uiWorkitemNotExist"));
                return;
            }
            else if ((ex.Message == "Your batch transaction ID is different from last active transaction ID"))
            {
                DialogService.ShowErrorDialog(_resource.GetString("uiWorkitemInvalidTransactionId"));
                return;
            }

            if (ex is EndpointNotFoundException || ex is CommunicationException || ex is TimeoutException)
            {
                DialogService.ShowErrorDialog(_resource.GetString("uiConnectFail"));
            }
            else if (ex is WcfException)
            {
                DialogService.ShowErrorDialog(ex.Message);
            }
            else
            {
                LogException(ex);

                if (ex is TwainException)
                {
                    DialogService.ShowErrorDialog(ex.Message);
                }
                else
                {
                    DialogService.ShowErrorDialog(_resource.GetString("uiGeneralError"));
                }
            }
        }

        public static void LogException(Exception ex)
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
        }

        public static void AddActionLog(ActionLogModel actionLog)
        {
            ProviderBase.AddLog(actionLog.Message, (int)actionLog.ActionNameEnum);
        }

        public static string BuildSearchExpression(ObservableCollection<SearchExpressionViewModel> searchs)
        {
            string expression = string.Empty;
            foreach (SearchExpressionViewModel searchExpressionViewModel in searchs)
            {
                if (!string.IsNullOrEmpty(searchExpressionViewModel.SearchQueryExpression.Value1))
                {
                    string itemName = searchExpressionViewModel.SearchQueryExpression.Field.Name.Trim().Replace(" ", string.Empty);
                    if (searchExpressionViewModel.SearchQueryExpression != null)
                    {
                        if (!string.IsNullOrEmpty(expression))
                        {
                            expression += " " + GetConditionString(searchExpressionViewModel.SearchQueryExpression.Condition) + " ";
                        }
                    }
                    if (searchExpressionViewModel.SearchQueryExpression.Field.DataType == FieldDataType.String)
                    {
                        switch (searchExpressionViewModel.SearchQueryExpression.Operator)
                        {
                            case SearchOperator.Contains:
                                expression += itemName + " like '%" + searchExpressionViewModel.SearchQueryExpression.Value1 + "%' ";
                                break;
                            case SearchOperator.NotContains:
                                expression += itemName + " not like '%" + searchExpressionViewModel.SearchQueryExpression.Value1 + "%' ";
                                break;
                            case SearchOperator.EndsWith:
                                expression += itemName + " like '%" + searchExpressionViewModel.SearchQueryExpression.Value1 + "' ";
                                break;
                            case SearchOperator.StartsWith:
                                expression += itemName + " like '" + searchExpressionViewModel.SearchQueryExpression.Value1 + "%' ";
                                break;
                            case SearchOperator.Equal:
                                expression += itemName + " = '" + searchExpressionViewModel.SearchQueryExpression.Value1 + "' ";
                                break;
                            case SearchOperator.NotEqual:
                                expression += itemName + " <> '" + searchExpressionViewModel.SearchQueryExpression.Value1 + "' ";
                                break;
                            default:
                                expression += itemName + " like '%" + searchExpressionViewModel.SearchQueryExpression.Value1 + "%' ";
                                break;
                        }
                    }
                    else if (searchExpressionViewModel.SearchQueryExpression.Field.DataType == FieldDataType.Date)
                    {
                        switch (searchExpressionViewModel.SearchQueryExpression.Operator)
                        {
                            case SearchOperator.Equal:
                                expression += "(";
                                expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += "AND ";
                                expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += ") ";
                                break;
                            case SearchOperator.NotEqual:
                                expression += "(";
                                expression += itemName + " < '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += "OR ";
                                expression += itemName + " > '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += ")";
                                break;
                            case SearchOperator.GreaterThan:
                                expression += itemName + " > '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case SearchOperator.GreaterThanOrEqualTo:
                                expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case SearchOperator.LessThan:
                                expression += itemName + " < '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case SearchOperator.LessThanOrEqualTo:
                                expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                break;
                            case SearchOperator.InBetween:
                                if (!string.IsNullOrEmpty(searchExpressionViewModel.SearchQueryExpression.Value1))
                                {
                                    expression += "(";
                                    expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                    expression += "AND ";
                                    expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value2 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                    expression += ") ";
                                }
                                break;
                            default:
                                expression += "(";
                                expression += itemName + " >= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                                expression += "AND ";
                                expression += itemName + " <= '" + Convert.ToDateTime(searchExpressionViewModel.SearchQueryExpression.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "' ";
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
                if (string.IsNullOrEmpty(searchViewModel.SearchQueryExpression.Value1))
                {
                    parameterValue.Add(searchViewModel.SearchQueryExpression.Field.Name.Trim(), searchViewModel.SearchQueryExpression.Value1);
                    if (searchViewModel.SearchQueryExpression.Operator == SearchOperator.InBetween)
                    {
                        parameterValue.Add(searchViewModel.SearchQueryExpression.Field.Name.Trim() + "1", searchViewModel.SearchQueryExpression.Value2);
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

        public static string GetConditionString(SearchConjunction conditionEnum)
        {
            switch (conditionEnum)
            {
                case SearchConjunction.None:
                    return null;
                case SearchConjunction.And:
                    return "AND";
                case SearchConjunction.Or:
                    return "OR";
                default:
                    return null;
            }
        }

    }
}
