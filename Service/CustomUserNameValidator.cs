using System.IdentityModel.Selectors;
using Ecm.Core;
using System.ServiceModel;
using Ecm.Domain;
using log4net;

namespace Ecm.Service
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