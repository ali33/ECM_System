using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Ecm.MobileCaptureService.Properties;
using log4net;
using System.ServiceModel.Web;
using System.Net;
using System.Text;

namespace Ecm.MobileCaptureService
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
            var prop = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];

            //if (string.IsNullOrWhiteSpace(prop.Headers[HttpRequestHeader.Authorization]))
            //{
            //    return null;
            //}


            string username = null;
            string passwordHash = string.Empty;
            string clientHost = null;

            //username = "sa";
            //passwordHash = "wrUlFSPYh9s=";
            //clientHost = "abc";

            //Retrieves Credentials from request header
            if (!String.IsNullOrWhiteSpace(prop.Headers["username"]))
            {
                username = prop.Headers["username"];
            }

            if (!String.IsNullOrWhiteSpace(prop.Headers["passwordHash"]))
            {
                passwordHash = prop.Headers["passwordHash"];
            }

            if (!String.IsNullOrWhiteSpace(prop.Headers["clientHost"]))
            {
                clientHost = prop.Headers["clientHost"];
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

                throw new WebFaultException<string>(string.Format(ErrorMessages.AuthorizedUserFail, username),
                                                    HttpStatusCode.Unauthorized);
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            return;
        }
    }
}