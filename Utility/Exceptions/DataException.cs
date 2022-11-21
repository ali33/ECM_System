using System;
using System.Runtime.Serialization;

namespace Ecm.Utility.Exceptions
{
    public class DataException : Exception
    {
        public DataException()
        {
        }

        public DataException(string message)
            : base(message)
        {
        }

        public DataException(string message, Exception ex)
            : base(message, ex)
        {
        }

        protected DataException(SerializationInfo serInfo, StreamingContext streamCon)
            : base(serInfo, streamCon)
        {
        }
    }
}