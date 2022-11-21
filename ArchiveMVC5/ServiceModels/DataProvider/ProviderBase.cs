using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using System.Configuration;

namespace ArchiveMVC5.Models.DataProvider
{
    public abstract class ProviderBase
    {
        public ProviderBase()
        {
        }

        public ProviderBase(string userName, string password)
        {
        
            Configure( userName, password);            
        }
        

        /// <summary>
        /// Ping đến Server
        /// </summary>
        public  void PingServer()
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            //ArchiveClient.Ping();

            using (ClientChannel<IArchive> clientChannel = ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, UserName, Password))
            {
                clientChannel.Channel.Ping();
            }
        }


        /// <summary>
        /// ghi file lên server
        /// </summary>
        /// <param name="fileBytes">mảng byte</param>
        /// <param name="fileName">tên file</param>
        public  void WriteFileToServer(byte[] fileBytes, string fileName)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            using (ClientChannel<IArchive> clientChannel = ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, UserName, Password))
            {
                clientChannel.Channel.WriteFileToServer(fileBytes, fileName);
            }
        }


        /// <summary>
        /// thay đổi username và password
        /// </summary>
        /// <param name="userName">Username cần thay đổi</param>
        /// <param name="password">password mới </param>
        public  void Configure(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public  void Close()
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

        public ClientChannel<IArchive> GetLoggingClientChannel()
        {
            return ChannelManager<IArchive>.Instance.GetChannel(ARCHIVE_ENDPOINT, "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
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

        public  string UserName { get;  set; }

        public  string Password { get;  set; }

        private  string ARCHIVE_ENDPOINT = "ArchiveEndPoint";
    }
}