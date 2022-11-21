using System.Net;
using System.Net.Sockets;
using Ecm.Utility.ProxyHelper;
using Ecm.Service.Contract;
using System.Collections.Generic;

namespace ArchiveMVC.Models.DataProvider
{
    public class SecurityProvider : ProviderBase
    {

        public SecurityProvider()
        {
            
        }
        /// <summary>
        /// đăng nhập với username và password
        /// </summary>
        /// <param name="username">tên đăng nhập</param>
        /// <param name="password">mật khẩu để đăng nhập</param>
        /// <returns></returns>
        public UserModel Login(string username, string password)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            string encryptedPassword = Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(password);
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                UserModel user = ObjectMapper.GetUserModel(client.Channel.Login(username, encryptedPassword, GetHost()));
                if (user != null)
                {
                    Configure(user.Username, user.Password);
                }

                return user;
            }
        }


        /// <summary>
        /// thay đổi password
        /// </summary>
        /// <param name="userName">tên đăng nhập</param>
        /// <param name="oldEncryptedPassword">mật khẩu cũ</param>
        /// <param name="newEncryptedPassword">mật khẩu mới</param>
        /// <returns></returns>
        public UserModel ChangePassword(string userName, string oldEncryptedPassword, string newEncryptedPassword)
        {
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                UserModel newUser = ObjectMapper.GetUserModel(client.Channel.ChangePassword(userName, oldEncryptedPassword, newEncryptedPassword));
                return newUser;
            }
        }

        public IList<DocumentTypeModel> GetCapturedDocumentTypes()
        {            
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {                
                var docTypes = client.Channel.GetCapturedDocumentTypes();
                return ObjectMapper.GetDocumentTypeModels(docTypes);
            }
        }


        /// <summary>
        /// đặt lại password cho tên đăng nhập
        /// </summary>
        /// <param name="username">tên đăng nhập</param>
        public void ResetPassword(string username)
        {
            Configure("NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A", string.Empty);
            using (ClientChannel<IArchive> client = GetArchiveClientChannel())
            {
                client.Channel.ResetPassword(username);
            }
        }
        /// <summary>
        /// lấy tên máy chủ
        /// </summary>
        /// <returns></returns>
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