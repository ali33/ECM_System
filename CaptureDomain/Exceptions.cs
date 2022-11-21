using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.CaptureDomain
{
    public class InvalidBatchDataException : Exception
    {
        public InvalidBatchDataException(string message)
            : base(message)
        {
        }
    }

    public class InvalidTransactionIdException : Exception
    {
        public InvalidTransactionIdException(string message)
            : base(message)
        {
        }
    }
}
