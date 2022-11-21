using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using Ecm.Mvvm;
using Ecm.Model.DataProvider;
using System.Resources;
using System.Reflection;

namespace Ecm.CustomAddin.ViewModel
{
    public class ProcessHelper
    {
        private static ResourceManager _resource = new ResourceManager("Ecm.CustomAddin.Resources", Assembly.GetExecutingAssembly());
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
                    DialogService.ShowErrorDialog(_resource.GetString("uiGeneralError"));
                }
            }
        }
    }
}
