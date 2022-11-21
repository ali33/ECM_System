using Ecm.Domain;
using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using System.Configuration;

namespace Ecm.Model.DataProvider
{
    public abstract class ProviderBase
    {
        public static void PingServer()
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            //ArchiveClient.Ping();

            using (ClientChannel<IArchive> clientChannel = ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, UserName, Password))
            {
                clientChannel.Channel.Ping();
            }
        }

        public static void WriteFileToServer(byte[] fileBytes, string fileName)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            using (ClientChannel<IArchive> clientChannel = ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, UserName, Password))
            {
                clientChannel.Channel.WriteFileToServer(fileBytes, fileName);
            }
        }

        public static void Configure(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public static void Close()
        {
            ChannelManager<IArchive>.Instance.Close();
        }

        public ClientChannel<IArchive> GetArchiveClientChannel()
        {
            return ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, UserName, Password);
        }

        public ClientChannel<IArchive> GetOneTimeClientChannel()
        {
            return ChannelManager<IArchive>.Instance.GetChannel(OneTimeBinding.Instance, ConfigurationManager.AppSettings["ArchiveServiceUrl"], UserName, Password);
        }

        public ClientChannel<IArchive> GetOneTimeClientChannel(string serviceUrl)
        {
            return ChannelManager<IArchive>.Instance.GetChannel(OneTimeBinding.Instance, serviceUrl, UserName, Password);
        }

        public static ClientChannel<IArchive> GetLoggingClientChannel()
        {
            return ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, UserName, Password);
        }

        public void AddLog(string message, ActionName actionName)
        {
            using (var client = GetArchiveClientChannel())
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

        //public static Exception ThrowException(Exception ex)
        //{
        //    _archiveClient = null;
        //    _loggingClient = null;
        //    if (ex is FaultException || ex is CommunicationException || ex is TimeoutException)
        //    {
        //        throw new WcfException(ex.Message);
        //    }

        //    throw ex;
        //}

        public static string UserName { get; private set; }

        public static string Password { get; private set; }

        private const string ARCHIVE_ENDPOINT = "ArchiveEndPoint";
    }
}