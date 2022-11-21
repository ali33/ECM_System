using System;
using System.Runtime.Serialization;

namespace Ecm.Utility.Exceptions
{
    public class TwainException : Exception
    {
        public TwainException()
        {
        }

        public TwainException(string msg)
            : base(msg)
        {
        }

        public TwainException(string pMessage, Exception pInnerException)
            : base(pMessage, pInnerException)
        {
        }

        protected TwainException(SerializationInfo serInfo, StreamingContext streamCon)
            : base(serInfo, streamCon)
        {
        }
    }
}