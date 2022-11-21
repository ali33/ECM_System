using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Ecm.Service
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CredentialsExtractorAttribute : Attribute, IContractBehavior, IDispatchMessageInspector
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(CredentialsExtractorAttribute));

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint,
                                         BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
                                          DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.MessageInspectors.Add(this);
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            string username = null;
            string passwordHash = null;
            string clientHost = null;
            log4net.Config.XmlConfigurator.Configure();

            //Retrieves Credentials from request header
            int i = request.Headers.FindHeader("username", "sec");
            if (-1 != i)
            {
                username = request.Headers.GetHeader<string>("username", "sec");
            }

            int j = request.Headers.FindHeader("passwordHash", "sec");
            if (-1 != j)
            {
                passwordHash = request.Headers.GetHeader<string>("passwordHash", "sec");
            }

            int k = request.Headers.FindHeader("clientHost", "sec");
            if (-1 != k)
            {
                clientHost = request.Headers.GetHeader<string>("clientHost", "sec");
            }

            if (username == "NO_USER_C13198AB_01B0_43F0_B527_1490DA6FB93A")
            {
                return null;
            }

            try
            {
                var customValidator = new CustomUserNameValidator { ClientHost = clientHost };
                customValidator.Validate(username, passwordHash);
                UserContext.Current.User.ClientHost = clientHost;
            }
            catch (Exception ex)
            {
                _log.Error(string.Format(ErrorMessages.AuthorizedUserFail, username), ex);
                throw new FaultException(string.Format(ErrorMessages.AuthorizedUserFail, username));
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            return;
        }
    }
}