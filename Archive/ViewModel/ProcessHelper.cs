using System;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using System.Resources;
using System.Reflection;
using Ecm.Model;

namespace Ecm.Archive.ViewModel
{
    public class ProcessHelper
    {
        private static ResourceManager _resource = new ResourceManager("Ecm.Archive.Resources", Assembly.GetExecutingAssembly());
        public static void ProcessException(Exception ex)
        {
            //DialogService.ShowMessageDialog(ex.Message);
            //DialogService.ShowMessageDialog(ex.StackTrace);
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
                if (ex is TwainException)
                {
                    DialogService.ShowErrorDialog(ex.Message);
                }
                else
                {
                    DialogService.ShowErrorDialog(_resource.GetString("uiGeneralError"));
                }
            }
            LogException(ex);
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
    }
}
