namespace Ecm.ContentViewer.BarcodeHelper
{
    using System;
    using System.Runtime.Serialization;

    public class BarcodeException : Exception
    {
        public BarcodeException()
        {
        }

        public BarcodeException(string message) : base(message)
        {
        }

        protected BarcodeException(SerializationInfo serInfo, StreamingContext streamCon) : base(serInfo, streamCon)
        {
        }

        public BarcodeException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}

