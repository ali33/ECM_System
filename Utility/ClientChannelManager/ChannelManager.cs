using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Collections.ObjectModel;
using System.Threading;

namespace Ecm.Utility.ProxyHelper
{
    public class ChannelManager<T> where T : class
    {
        private ReaderWriterLockSlim collectionLock = new ReaderWriterLockSlim();

        public Dictionary<string, ChannelFactory> channelFactoryCollection = new Dictionary<string, ChannelFactory>();

        public ClientChannel<T> GetChannel(string endpointName, string userName, string password)
        {
            ChannelFactory<T> factory = null;

            collectionLock.EnterUpgradeableReadLock();
            bool createNewFactory = false;
            string key = endpointName;
            if (string.IsNullOrEmpty(password))
            {
                key = userName;
            }
            try
            {
                if (channelFactoryCollection.Keys.Contains(key))
                {
                    factory = (ChannelFactory<T>)channelFactoryCollection[key];
                    if (!string.IsNullOrEmpty(password))
                    {
                        var oldCredentials = factory.Endpoint.Contract.Behaviors.Find<CredentialsInserter>();
                        createNewFactory = (oldCredentials.Username != userName || oldCredentials.Password != password);

                        if (createNewFactory)
                        {
                            collectionLock.EnterWriteLock();
                            try
                            {
                                channelFactoryCollection.Remove(endpointName);
                            }
                            finally
                            {
                                factory.Close();
                                collectionLock.ExitWriteLock();
                            }
                        }
                    }
                }
                if (!channelFactoryCollection.Keys.Contains(key) || createNewFactory)
                {
                    //ServiceEndpoint endpoint = GetServiceEndpoint(endpointAddress);

                    factory = new ChannelFactory<T>(endpointName);
                    factory.Endpoint.Contract.Behaviors.Add(new CredentialsInserter(userName, password));

                    collectionLock.EnterWriteLock();
                    try
                    {
                        channelFactoryCollection.Add(key, factory);
                    }
                    finally
                    {
                        collectionLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                collectionLock.ExitUpgradeableReadLock();
            }

            ClientChannel<T> client = new ClientChannel<T>(factory.CreateChannel());
            return client;
        }

        public ClientChannel<T> GetChannel(Binding binding, string endpointName, string userName, string password)
        {
            ChannelFactory<T> factory = null;

            collectionLock.EnterUpgradeableReadLock();
            bool createNewFactory = false;
            string key = endpointName;
            if (string.IsNullOrEmpty(password))
            {
                key = userName;
            }
            try
            {
                if (channelFactoryCollection.Keys.Contains(key))
                {
                    factory = (ChannelFactory<T>)channelFactoryCollection[key];
                    if (!string.IsNullOrEmpty(password))
                    {
                        var oldCredentials = factory.Endpoint.Contract.Behaviors.Find<CredentialsInserter>();
                        createNewFactory = (oldCredentials.Username != userName || oldCredentials.Password != password);

                        if (createNewFactory)
                        {
                            collectionLock.EnterWriteLock();
                            try
                            {
                                channelFactoryCollection.Remove(endpointName);
                            }
                            finally
                            {
                                factory.Close();
                                collectionLock.ExitWriteLock();
                            }
                        }
                    }
                }
                if (!channelFactoryCollection.Keys.Contains(key) || createNewFactory)
                {
                    //ServiceEndpoint endpoint = GetServiceEndpoint(endpointAddress);

                    factory = new ChannelFactory<T>(binding, endpointName);
                    factory.Endpoint.Contract.Behaviors.Add(new CredentialsInserter(userName, password));

                    collectionLock.EnterWriteLock();
                    try
                    {
                        channelFactoryCollection.Add(key, factory);
                    }
                    finally
                    {
                        collectionLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                collectionLock.ExitUpgradeableReadLock();
            }

            ClientChannel<T> client = new ClientChannel<T>(factory.CreateChannel());
            return client;
        }

        public void Close()
        {
            foreach (ChannelFactory factory in channelFactoryCollection.Values)
            {
                try
                {
                    factory.Close();
                }
                catch (Exception)
                {
                    factory.Abort();
                }
            }
        }

        private ServiceEndpoint GetServiceEndpoint(string endpointAddress)
        {
            System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding(System.ServiceModel.SecurityMode.None);
            binding.MaxReceivedMessageSize = 2147483647;

            MetadataExchangeClient mexClient = new MetadataExchangeClient(binding);
                //(new Uri(endpointAddress), 
                //                                                          MetadataExchangeClientMode.HttpGet);
            mexClient.ResolveMetadataReferences = true;
            MetadataSet metaDocs = mexClient.GetMetadata(new EndpointAddress(endpointAddress));

            WsdlImporter importer = new WsdlImporter(metaDocs);
            //ServiceContractGenerator generator = new ServiceContractGenerator();
            //Collection<ContractDescription> contracts = importer.ImportAllContracts();

            var endpoints = importer.ImportAllEndpoints();
            if (endpoints != null && endpoints.Count > 0)
            {
                return endpoints[0];
            }
            return null;
        }

        #region Singleton implementation

        ChannelManager()
        {
            /* Intentionally left blank. */
        }

        public static ChannelManager<T> Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            /* Explicit static constructor to tell C# compiler 
             * not to mark type as beforefieldinit. */
            static Nested()
            {
            }

            internal static readonly ChannelManager<T> instance = new ChannelManager<T>();
        }

        #endregion

        //public ClientChannel<global::Ecm.Service.Contract.ICapture> GetChannel(Binding binding, AppSettings appSettings, string UserName, string Password)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
