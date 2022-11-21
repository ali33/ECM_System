using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using System.Net;
using System.Net.Sockets;
using Ecm.CaptureDomain;
using System.Configuration;
using System.ServiceModel;
using System.Resources;
using Ecm.Utility.Exceptions;
using Ecm.Mvvm;
using System.Reflection;

namespace Ecm.Workflow.Activities.CustomActivityModel.DataProvider
{
    public class CaptureProvider
    {
        public string _userName;
        public string _password;
        private static string _endpoint = ConfigurationManager.AppSettings["CaptureServiceUrl"].ToString();
        private const string noUser = "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A";

        public CaptureProvider(string userName, string password)
        {
            _userName = userName;
            _password = password;
        }

        public void ConfigUserInfo(string username, string passwordHash)
        {
            _userName = username;
            _password = passwordHash;
        }

        private static ClientChannel<ICapture> GetOneTimeClientChannel(string username, string password)
        {
            return ChannelManager<ICapture>.Instance.GetChannel(OneTimeBinding.Instance, _endpoint, username, password);
        }

        private static ClientChannel<ICapture> GetOneTimeClientChannel()
        {
            return ChannelManager<ICapture>.Instance.GetChannel(OneTimeBinding.Instance, _endpoint, noUser, string.Empty);
        }

        public static ClientChannel<ICapture> GetLoggingClientChannel()
        {
            return ChannelManager<ICapture>.Instance.GetChannel(OneTimeBinding.Instance, _endpoint, noUser, string.Empty);
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

        public User LoginToCapture()
        {
            using (var client = GetOneTimeClientChannel())
            {
                string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(_password);

                User user = client.Channel.Login(_userName, encryptedPassword, GetHost());

                return user;
            }
        }

        public List<UserGroupModel> GetUserGroups()
        {
            using (var client = GetOneTimeClientChannel(_userName, _password))
            {
                List<UserGroupModel> groups = new List<UserGroupModel>();
                client.Channel.GetUserGroups().ForEach(p => groups.Add(new UserGroupModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Users =  new System.Collections.ObjectModel.ObservableCollection<UserModel>(GetUserModels(p.Users))
                }));

                return groups;
            }
        }

        private List<UserModel> GetUserModels(List<User> users)
        {
            List<UserModel> userModels = new List<UserModel>();
            users.ForEach(p => userModels.Add(new UserModel
            {
                EmailAddress = p.Email,
                Fullname = p.FullName,
                Id = p.Id,
                IsAdmin = p.IsAdmin,
                Password = p.Password,
                Username = p.UserName
            }));

            return userModels;
        }

        private static readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.CustomActivityModel.Resource", Assembly.GetExecutingAssembly());

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
            using (var loggingClient = GetLoggingClientChannel())
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
