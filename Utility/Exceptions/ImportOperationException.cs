using System;
using System.Runtime.Serialization;

namespace Ecm.Utility.Exceptions
{
    public class ImportOperationException : Exception
    {
        public ImportOperationException()
        {
        }

        public ImportOperationException(string message)
            : base(message)
        {
        }

        public ImportOperationException(string message, Exception exception)
            : base(message, exception)
        {
        }

        protected ImportOperationException(SerializationInfo serInfo, StreamingContext streamCon)
            : base(serInfo, streamCon)
        {
        }
    }
}