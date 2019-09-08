using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.common.client
{
    public class EPLockException : EPRuntimeException
    {
        public EPLockException(string message) : base(message)
        {
        }

        public EPLockException(string message,
            Exception cause) : base(message, cause)
        {
        }

        public EPLockException(Exception cause) : base(cause)
        {
        }
    }
}
