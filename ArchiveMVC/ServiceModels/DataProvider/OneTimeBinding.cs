using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace ArchiveMVC.Models.DataProvider
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

        static OneTimeBinding()
        {
        }

        private OneTimeBinding()
        {
        }
        /// <summary>
        /// lấy OneTimeBinding
        /// </summary>
        /// <returns></returns>
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
