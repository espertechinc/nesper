using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.compat.threading.locks
{
    public class LockException : Exception
    {
        public LockException()
        {
        }

        public LockException(string message) : base(message)
        {
        }

        public LockException(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }
    }
}