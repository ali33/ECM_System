using System.IdentityModel.Selectors;
using Ecm.CaptureCore;
using Ecm.CaptureDomain;
using System.ServiceModel;
using Ecm.CaptureService.Properties;
using log4net;

namespace Ecm.CaptureService
{
    public class CustomUserNameValidator : UserNamePasswordValidator
    {
        private readonly SecurityManager _securityManager = new SecurityManager();
        private readonly ILog _log = LogManager.GetLogger(typeof(CustomUserNameValidator));
        public string ClientHost { get; set; }

        public override void Validate(string userName, string passwordHash)
        {
            if (userName != null)
            {
                User user = _securityManager.Authorize(userName, passwordHash);
                if (OperationContext.Current.Extensions.Find<UserContext>() == null)
                {
                    OperationContext.Current.Extensions.Add(new UserContext());
                    UserContext.Current.User = user;
                }
            }
            else
            {
                _log.Error(ErrorMessages.UnknowUser);
                throw new FaultException(ErrorMessages.UnknowUser);
            }
        }
    }
}