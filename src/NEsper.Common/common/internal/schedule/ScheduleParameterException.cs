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
    /// This exception is thrown to indicate a problem with schedule parameters.
    /// </summary>
    public class ScheduleParameterException : Exception
    {
        /// <summary>Constructor. </summary>
        /// <param name="message">is the error message</param>
        public ScheduleParameterException(string message)
            : base(message)
        {
        }

        /// <summary>Constructor for an inner exception and message. </summary>
        /// <param name="message">is the error message</param>
        /// <param name="innerException">is the inner exception</param>
        public ScheduleParameterException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="innerException">is the inner exception</param>
        public ScheduleParameterException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }
    }
}