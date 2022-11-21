using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Ecm.Utility
{
    public class CredentialsInserter : IContractBehavior, IClientMessageInspector
    {
        public CredentialsInserter(string username, string passwordHash)
        {
            _username = username;
            _passwordHash = passwordHash;
            _host = GetHost();
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            return;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var messageHeaderUsername = new MessageHeader<string>(_username);
            request.Headers.Add(messageHeaderUsername.GetUntypedHeader("username", "sec"));

            var messageHeaderPass = new MessageHeader<string>(_passwordHash);
            request.Headers.Add(messageHeaderPass.GetUntypedHeader("passwordHash", "sec"));

            var messageHeaderIp = new MessageHeader<string>(_host);
            request.Headers.Add(messageHeaderIp.GetUntypedHeader("clientHost", "sec"));
            
            return null;
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint,
                                         BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
                                        ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
                                          DispatchRuntime dispatchRuntime)
        {
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
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

        public string Username
        {
            get { return _username; }
        }

        public string Password
        {
            get { return _passwordHash; }
        }

        private readonly string _username;
        private readonly string _passwordHash;
        private readonly string _host;
    }
}