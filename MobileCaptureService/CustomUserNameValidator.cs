using System.IdentityModel.Selectors;
using Ecm.CaptureCore;
using Ecm.CaptureDomain;
using System.ServiceModel;
using Ecm.MobileCaptureService.Properties;
using log4net;

namespace Ecm.MobileCaptureService
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
                User user = _securityManager.AuthorizeMobile(userName, passwordHash);
                if (OperationContext.Current.Extensions.Find<UserContext>() == null)
                {
                    OperationContext.Current.Extensions.Add(new UserContext());
                    UserContext.Current.User = user;

                    var wfSystem = _securityManager.Authorize("WorkflowSystem", "TzmdoMVgNmQ5QMXJDuLBKgKg6CYfx73S/8dPX8Ytva+Eu3hlFNVoAg==");
                    UserContext.Current.WorkflowSystemUser = wfSystem;
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