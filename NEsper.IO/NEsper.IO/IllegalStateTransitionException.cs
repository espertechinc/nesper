using System;

namespace com.espertech.esperio
{
    /// <summary>
    /// Thrown when an illegal Adapter state transition is attempted.
    /// </summary>
    public class IllegalStateTransitionException : EPException
    {
        /// <summary>
        /// <param name="message">an explanation of the cause of the exception</param>
        /// </summary>
        public IllegalStateTransitionException(String message)
            : base(message)
        {
        }
    }
}