using System;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ViewModel
{
    public class ProcessHelper
    {
        private static readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ConfigurationView", Assembly.GetExecutingAssembly());

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
    }
}