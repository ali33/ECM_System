using System;
using System.Runtime.Serialization;

namespace Ecm.CameraLib
{
    /// <summary>
    /// This exception is thrown for all camera related errors.  
    /// Notice that we have to provide all 4 constructors below to make sure our exceptions work in all cases.
    /// </summary>
    public class CameraException : Exception
    {
        public CameraException()
        {
        }

        public CameraException(string msg)
            : base(msg)
        {
        }

        public CameraException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        protected CameraException(SerializationInfo serInfo, StreamingContext streamCon)
            : base(serInfo, streamCon)
        {
        }
    }
}
