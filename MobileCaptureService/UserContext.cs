using System.ServiceModel;
using Ecm.CaptureDomain;

namespace Ecm.MobileCaptureService
{
    public class UserContext : IExtension<OperationContext>
    {
        //The "current" custom context
        public static UserContext Current
        {
            get { return OperationContext.Current.Extensions.Find<UserContext>(); }
        }

        #region IExtension<OperationContext> Members

        public void Attach(OperationContext owner)
        {
            //no-op
        }

        public void Detach(OperationContext owner)
        {
            //no-op
        }

        #endregion

        //You can have lots more of these -- this is the stuff that you
        //want to store on your custom context
        public User User { get; set; }

        public User WorkflowSystemUser { get; set; }
    }
}