using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using Ecm.Mvvm;
using Ecm.Model.DataProvider;

namespace Ecm.OutlookClient.ViewModel
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
    }
}
