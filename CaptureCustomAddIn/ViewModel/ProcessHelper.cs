using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using Ecm.Mvvm;
using Ecm.CaptureModel.DataProvider;
using System.Resources;
using System.Reflection;
using Ecm.CaptureModel;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class ProcessHelper
    {
        private static ResourceManager _resource = new ResourceManager("Ecm.CaptureCustomAddin.Resources", Assembly.GetExecutingAssembly());
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
