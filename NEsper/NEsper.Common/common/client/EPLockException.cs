using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client
{
    [Serializable]
    public class EPLockException : EPRuntimeException
    {
        public EPLockException(string message) : base(message)
        {
        }

        public EPLockException(
            string message,
            Exception cause) : base(message, cause)
        {
        }

        public EPLockException(Exception cause) : base(cause)
        {
        }

        protected EPLockException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}