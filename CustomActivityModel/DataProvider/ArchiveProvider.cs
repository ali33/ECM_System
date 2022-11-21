using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Service.Contract;
using Ecm.Utility.ProxyHelper;
using Ecm.Domain;
using System.Net;
using System.Net.Sockets;
using ArchiveModel = Ecm.Model;
using System.Collections.ObjectModel;

namespace Ecm.Workflow.Activities.CustomActivityModel.DataProvider
{
    public class ArchiveProvider
    {
        public string _userName;
        public string _password;
        public string _endpoint;

        public ArchiveProvider(string userName, string password, string endpoint)
        {
            _userName = userName;
            _password = password;
            _endpoint = endpoint;
        }

        public void ConfigUserInfo(string username, string passwordHash)
        {
            _userName = username;
            _password = passwordHash;
        }

        private ClientChannel<IArchive> GetOneTimeClientChannel(string serviceUrl, string username, string password)
        {
            return ChannelManager<IArchive>.Instance.GetChannel(OneTimeBinding.Instance, serviceUrl, username, password);
        }

        private ClientChannel<IArchive> GetOneTimeClientChannel(string serviceUrl)
        {
            string noUser = "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A";
            return ChannelManager<IArchive>.Instance.GetChannel(OneTimeBinding.Instance, serviceUrl, noUser, string.Empty);
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

        public User LoginToArchive()
        {
            using (var client = GetOneTimeClientChannel(_endpoint))
            {
                string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(_password);

                User user = client.Channel.Login(_userName, encryptedPassword, GetHost());

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                }

                return user;
            }
        }

        public User LoginToArchiveWithExitingUser()
        {
            using (var client = GetOneTimeClientChannel(_endpoint))
            {
                string encryptedPassword = Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(_password);

                User user = client.Channel.Login(_userName, _password, GetHost());

                if (user != null)
                {
                    user.EncryptedPassword = encryptedPassword;
                }

                return user;
            }
        }

        //public User VerifyUser()
        //{
        //    using (var client = GetOneTimeClientChannel(_endpoint))
        //    {
        //        User user = client.Channel.VerifyUser(_userName, _password);

        //        if (user != null)
        //        {
        //            user.EncryptedPassword = encryptedPassword;
        //        }

        //        return user;
        //    }
        //}

        public ObservableCollection<ArchiveModel.DocumentTypeModel> GetDocumentTypes()
        {
            using (var client = GetOneTimeClientChannel(_endpoint, _userName, _password))
            {
                List<DocumentType> documentTypes = client.Channel.GetDocumentTypes();
                ObservableCollection<ArchiveModel.DocumentTypeModel> documentTypeModels = ArchiveModel.DataProvider.ObjectMapper.GetDocumentTypeModels(documentTypes);

                return documentTypeModels;
            }
        }

        public FieldMetaData GetArchiveFieldMetaDataByName(string fieldName, Guid docTypeId)
        {
            using (var client = GetOneTimeClientChannel(_endpoint, _userName, _password))
            {
                return client.Channel.GetDocumentType(docTypeId).FieldMetaDatas.FirstOrDefault(p => p.Name == fieldName);
            }
        }

        public void InsertArchiveDocument(Domain.Document document)
        {
            using (var client = GetOneTimeClientChannel(_endpoint, _userName, _password))
            {
                client.Channel.InsertDocuments(new List<Document> { document });
            }
        }


    }
}
