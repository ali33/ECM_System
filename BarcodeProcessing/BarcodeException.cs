using System;

namespace Ecm.BarcodeProcessing
{
    public class BarcodeException : Exception
    {
        public BarcodeException()
        {
        }

        public BarcodeException(string message)
            : base(message)
        {
        }

        public BarcodeException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
