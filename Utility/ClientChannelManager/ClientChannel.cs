using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace Ecm.Utility.ProxyHelper
{
    public class ClientChannel<T> : IDisposable
    {
        public ClientChannel(T channel)
        {
            _channel = channel;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ClientChannel()
        {
            Dispose(false);
        }

        public T Channel
        {
            get
            {
                //((ICommunicationObject)_channel).Open();
                return _channel;
            }
        }

        private void CloseChannel()
        {
            
            ICommunicationObject client = (ICommunicationObject)_channel;
            try
            {
                client.Close();
            }
            catch (CommunicationException)
            {
                client.Abort();
            }
            catch (TimeoutException)
            {
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                CloseChannel();
            }
        }

        private T _channel;
    }
}
