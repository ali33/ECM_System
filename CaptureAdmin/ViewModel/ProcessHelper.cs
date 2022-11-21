using System;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using System.Resources;
using System.Reflection;
using System.Collections.ObjectModel;
using Ecm.CaptureModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class ProcessHelper
    {
        private static readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());

        public static void ProcessException(Exception ex)
        {
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

        public static string BuildSearchExpression(ObservableCollection<SearchExpressionViewModel> searchs)
        {
            string expression = string.Empty;
            foreach (SearchExpressionViewModel searchExpressionViewModel in searchs)
            {
                if (!string.IsNullOrEmpty(searchExpressionViewModel.Search.Value))
                {
                    string itemName = searchExpressionViewModel.Search.Name.Trim().Replace(" ", string.Empty);
                    if (!string.IsNullOrEmpty(searchExpressionViewModel.Search.Condition))
                    {
                        if (!string.IsNullOrEmpty(expression))
                        {
                            expression += " " + searchExpressionViewModel.Search.Condition + " ";
                        }
                    }
                    if (searchExpressionViewModel.Search.DataType.ToLower() == "string")
                    {
                        switch (searchExpressionViewModel.Search.Operator)
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

    }
}