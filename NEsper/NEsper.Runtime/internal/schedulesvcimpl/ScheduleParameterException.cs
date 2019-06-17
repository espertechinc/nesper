using System;

namespace com.espertech.esper.runtime.@internal.schedulesvcimpl
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This exception is thrown to indicate a problem with schedule parameters.
    /// </summary>
    [Serializable]
    public class ScheduleParameterException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">is the error message</param>
        public ScheduleParameterException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor for an inner exception and message.
        /// </summary>
        /// <param name="message">is the error message</param>
        /// <param name="cause">is the inner exception</param>
        public ScheduleParameterException(string message, Exception cause) : base(message, cause)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cause">is the inner exception</param>
        public ScheduleParameterException(Exception cause) : base(null, cause)
        {
        }
    }
} // end of namespace