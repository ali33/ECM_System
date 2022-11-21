using System;

namespace Ecm.CaptureCore
{
    public class CaptureException : Exception
    {
        public CaptureError ErrorCode { get; private set; }

        public CaptureException(CaptureError code, string message = "") : base(message)
        {

        }
    }
}