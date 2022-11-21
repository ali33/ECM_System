namespace Microsoft.Tools.TestClient
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ExceptionUtility
    {
        internal static bool IsFatal(Exception exception)
        {
            while (exception != null)
            {
                if (((exception is OutOfMemoryException) && !(exception is InsufficientMemoryException)) || (((exception is ThreadAbortException) || (exception is AccessViolationException)) || (exception is SEHException)))
                {
                    return true;
                }
                if (!(exception is TypeInitializationException) && !(exception is TargetInvocationException))
                {
                    break;
                }
                exception = exception.InnerException;
            }
            return false;
        }

        internal ArgumentException ThrowHelperArgument(string message)
        {
            return (ArgumentException) this.ThrowHelperError(new ArgumentException(message));
        }

        internal ArgumentException ThrowHelperArgument(string paramName, string message)
        {
            return (ArgumentException) this.ThrowHelperError(new ArgumentException(message, paramName));
        }

        internal ArgumentNullException ThrowHelperArgumentNull(string paramName)
        {
            return (ArgumentNullException) this.ThrowHelperError(new ArgumentNullException(paramName));
        }

        internal ArgumentNullException ThrowHelperArgumentNull(string paramName, string message)
        {
            return (ArgumentNullException) this.ThrowHelperError(new ArgumentNullException(paramName, message));
        }

        internal Exception ThrowHelperCritical(Exception exception)
        {
            return exception;
        }

        internal Exception ThrowHelperError(Exception exception)
        {
            return exception;
        }

        internal Exception ThrowHelperWarning(Exception exception)
        {
            return exception;
        }
    }
}

