using System;
using System.Runtime.Serialization;

namespace Ecm.Utility.Exceptions
{
    public class WcfException : Exception
    {
        public WcfException()
        {
        }

        public WcfException(string message)
            : base(message)
        {
        }

        public WcfException(string message, Exception ex)
            : base(message, ex)
        {
        }

        protected WcfException(SerializationInfo serInfo, StreamingContext streamCon)
            : base(serInfo, streamCon)
        {
        }
    }
}