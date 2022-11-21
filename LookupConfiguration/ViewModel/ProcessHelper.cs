using System;
using Ecm.Mvvm;
using System.ServiceModel;
using Ecm.Utility.Exceptions;
using System.Resources;
using System.Reflection;
using Ecm.Workflow.Activities.CustomActivityModel.DataProvider;

namespace Ecm.Workflow.Activities.LookupConfiguration.ViewModel
{
    public class ProcessHelper
    {
        private static readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());

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
            using (var loggingClient = CaptureProvider.GetLoggingClientChannel())
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