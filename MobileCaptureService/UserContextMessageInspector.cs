using System.ServiceModel.Dispatcher;
using System.ServiceModel;

namespace Ecm.MobileCaptureService
{
    public class UserContextMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request,
                                          IClientChannel channel,
                                          InstanceContext instanceContext)
        {
            //if (OperationContext.Current.Extensions.Find<UserContext>().UserContextProperty == null)
            //    OperationContext.Current.Extensions.Add(new UserContext());

            return request.Headers.MessageId;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            //OperationContext.Current.Extensions.Remove(UserContext.Current);
        }
    }
}
