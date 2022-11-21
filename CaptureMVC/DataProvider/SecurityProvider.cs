using Ecm.CaptureDomain;
using Ecm.Service.Contract;
using Ecm.Utility;
using Ecm.Utility.ProxyHelper;
using System.Net;
using System.Net.Sockets;

namespace CaptureMVC.DataProvider
{
    public class SecurityProvider : ProviderBase
    {
        public User Login(string username, string password)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            string encryptedPassword = CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                var user = client.Channel.Login(username, encryptedPassword, GetHost());

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                    Configure(username, encryptedPassword);
                }

                return user;
            }
        }

        public User AuthoriseUser(string username, string password)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            string encryptedPassword = Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                var user = client.Channel.AuthoriseUser(username, encryptedPassword);

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                    Configure(username, encryptedPassword);
                }

                return user;
            }
        }

        public User ChangePassword(string userName, string oldEncryptedPassword, string newEncryptedPassword)
        {
            using (ClientChannel<ICapture> client = GetCaptureClientChannel())
            {
                var newUser = client.Channel.ChangePassword(userName, oldEncryptedPassword, newEncryptedPassword);
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