using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using System.Configuration;

namespace CaptureMVC.DataProvider
{
    public abstract class ProviderBase
    {
        public ProviderBase()
        {
        }

        public ProviderBase(string userName, string password)
        {
            Configure(userName, password);
        }

        /// <summary>
        /// Ping đến Server
        /// </summary>
        public static void PingServer()
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);

            using (ClientChannel<ICapture> clientChannel = ChannelManager<ICapture>.Instance.GetChannel(CAPTURE_ENDPOINT, UserName, Password))
            {
                clientChannel.Channel.Ping();
            }
        }

        /// <summary>
        /// thay đổi username và password
        /// </summary>
        /// <param name="userName">Username cần thay đổi</param>
        /// <param name="password">password mới </param>
        public static void Configure(string userName, string password)
        {
            UserName = userName;
            Password = password;
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