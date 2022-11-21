using System.Net;
using System.Net.Sockets;
using Ecm.Utility.ProxyHelper;
using Ecm.Service.Contract;

namespace Ecm.Model.DataProvider
{
    public class SecurityProvider : ProviderBase
    {
        public UserModel Login(string username, string password)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                UserModel user = ObjectMapper.GetUserModel(client.Channel.Login(username, encryptedPassword, GetHost()));

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                    Configure(user.Username, user.Password);
                }

                return user;
            }
        }

        public UserModel ChangePassword(string userName, string oldEncryptedPassword, string newEncryptedPassword)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                UserModel newUser = ObjectMapper.GetUserModel(client.Channel.ChangePassword(userName, oldEncryptedPassword, newEncryptedPassword));
                return newUser;
            }
        }

        public void ResetPassword(string username)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.ResetPassword(username);
            }
        }

        private string GetHost()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostName);
            IPAddress host = IPAddress.None;
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    host = ip;
                    break;
                }
            }

            return hostName + " (" + host + ")";
        }
    }
}