using System.Net;
using System.Net.Sockets;
using Ecm.Utility.ProxyHelper;
using Ecm.Service.Contract;

namespace Ecm.CaptureModel.DataProvider
{
    public class SecurityProvider : ProviderBase
    {
        public UserModel Login(string username, string password)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                var userModel = client.Channel.Login(username, encryptedPassword, GetHost());
                UserModel user = ObjectMapper.GetUserModel(userModel);

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                    Configure(username, encryptedPassword);
                }

                return user;
            }
        }

        public UserModel AuthoriseUser(string username, string password)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                UserModel user = ObjectMapper.GetUserModel(client.Channel.AuthoriseUser(username, encryptedPassword));

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                    Configure(username, encryptedPassword);
                }

                return user;
            }
        }

        public UserModel ChangePassword(string userName, string oldEncryptedPassword, string newEncryptedPassword)
        {
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                UserModel newUser = ObjectMapper.GetUserModel(client.Channel.ChangePassword(userName, oldEncryptedPassword, newEncryptedPassword));
                return newUser;
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

        public void ResetPassword(string username)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                client.Channel.ResetPassword(username);
            }
        }

    }
}