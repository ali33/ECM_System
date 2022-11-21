using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CaptureMVC.DataProvider
{
    public sealed class OneTimeBinding
    {
        private static readonly Binding _binding = GetOneTimeBinding();

        public static Binding Instance
        {
            get
            {
                return _binding;
            }
        }

        private static Binding GetOneTimeBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding("OneTimeBinding");

            BindingElementCollection bec = binding.CreateBindingElements();
            bec.Find<HttpTransportBindingElement>().KeepAliveEnabled = false;
            CustomBinding oneTimeBinding = new CustomBinding(bec);

            return oneTimeBinding;
        }

    }
}
