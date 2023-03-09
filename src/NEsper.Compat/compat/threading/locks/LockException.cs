using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.compat.threading.locks
{
    [Serializable]
    public class LockException : Exception
    {
        public LockException()
        {
        }

        protected LockException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
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