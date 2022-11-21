using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Xml;


namespace Ecm.Workflow.Activities.ReleaseToArchive
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

        private static Binding GetOneTimeBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "OneTimeBinding";
            binding.CloseTimeout = new TimeSpan(0, 10, 0);
            binding.OpenTimeout = new TimeSpan(0, 1, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.MaxReceivedMessageSize = 2147483647;
            binding.TextEncoding = Encoding.UTF8;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.TransferMode = TransferMode.Buffered;
            binding.MaxBufferSize = 2147483647;
            binding.MaxBufferPoolSize = 2147483647;
            
            XmlDictionaryReaderQuotas readerQuotas = new XmlDictionaryReaderQuotas();
            readerQuotas.MaxDepth = 2147483647;
            readerQuotas.MaxStringContentLength = 2147483647;
            readerQuotas.MaxArrayLength = 2147483647;
            readerQuotas.MaxBytesPerRead = 2147483647;
            readerQuotas.MaxNameTableCharCount = 2147483647;

            binding.ReaderQuotas = readerQuotas;

            BindingElementCollection bec = binding.CreateBindingElements();
            bec.Find<HttpTransportBindingElement>().KeepAliveEnabled = false;
            CustomBinding oneTimeBinding = new CustomBinding(bec);

            return oneTimeBinding;
        }

    }
}
