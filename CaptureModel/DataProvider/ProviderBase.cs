using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using System.Configuration;
using Ecm.CaptureDomain;

namespace Ecm.CaptureModel.DataProvider
{
    public abstract class ProviderBase
    {
        public static void Configure(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
        public static void PingServer()
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            ChannelManager<ICapture>.Instance.GetChannel(CAPTURE_ENDPOINT, "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty).Channel.Ping();
        }

        public void AddLog(string message, ActionName actionName)
        {
            using (var client = GetCaptureClientChannel())
            {
                client.Channel.AddActionLog(message, actionName, null, null);
            }
        }

        public static void AddLog(string message, int actionName)
        {
            using (var client = GetLoggingClientChannel())
            {
                client.Channel.AddActionLog(message, (ActionName)actionName, null, null);
            }
        }

        public static void Close()
        {
            ChannelManager<ICapture>.Instance.Close();
        }

        public ClientChannel<ICapture> GetCaptureClientChannel()
        {
            return ChannelManager<ICapture>.Instance.GetChannel(CAPTURE_ENDPOINT, UserName, Password);
        }

        public ClientChannel<ICapture> GetOneTimeCaptureClientChannel()
        {
            return ChannelManager<ICapture>.Instance.GetChannel(OneTimeBinding.Instance, ConfigurationManager.AppSettings["CaptureServiceUrl"], UserName, Password);
        }

        public static ClientChannel<ICapture> GetLoggingClientChannel()
        {
            return ChannelManager<ICapture>.Instance.GetChannel(CAPTURE_ENDPOINT, UserName, Password);
        }

        public static string UserName { get; private set; }

        public static string Password { get; private set; }

        private const string CAPTURE_ENDPOINT = "CaptureEndPoint";
    }
}