using System;
using System.Reflection;

namespace ReactGraph.NodeInfo
{
    class Maybe<T> : IMaybe
    {
        Exception exception;
        T value;

        public void NewValue(T newValue)
        {
            value = newValue;
            HasValue = true;
        }

        public void CouldNotCalculate(Exception ex)
        {
            exception = ex;
            value = default(T);
            HasValue = false;
        }

        public bool HasValue { get; private set; }

        object IMaybe.Value { get { return Value; } }

        public T Value
        {
            get
            {
                if (HasValue)
                    return value;

                if (exception == null)
                    throw new InvalidOperationException("No value or exception has been recorded");

                PreserveStackTrace(exception);
                throw exception;
            }
        }

        public Exception Exception
        {
            get
            {
                if (exception == null)
                    throw new InvalidOperationException("No exception has been recorded");

                return exception;
            }
        }

        // http://weblogs.asp.net/fmarguerie/archive/2008/01/02/rethrowing-exceptions-and-preserving-the-full-call-stack-trace.aspx
        static void PreserveStackTrace(Exception exception)
        {
            MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
              BindingFlags.Instance | BindingFlags.NonPublic);
            preserveStackTrace.Invoke(exception, null);
        }
    }
}