///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>
    /// This exception is thrown to indicate a problem with scheduling.
    /// </summary>
    public class ScheduleServiceException : Exception
    {
        /// <summary> Constructor.</summary>
        /// <param name="message">is the error message
        /// </param>
        public ScheduleServiceException(string message)
            : base(message)
        {
        }

        /// <summary> Constructor for an inner exception and message.</summary>
        /// <param name="message">is the error message
        /// </param>
        /// <param name="cause">is the inner exception
        /// </param>
        public ScheduleServiceException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary> Constructor.</summary>
        /// <param name="cause">is the inner exception
        /// </param>
        public ScheduleServiceException(Exception cause)
            : base(string.Empty, cause)
        {
        }

        protected ScheduleServiceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}