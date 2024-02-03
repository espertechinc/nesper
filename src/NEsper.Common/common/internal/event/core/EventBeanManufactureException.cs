///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Thrown to indicate a problem creating or populating an underlying event objects.
    /// </summary>
    public class EventBeanManufactureException : Exception
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">cause</param>
        public EventBeanManufactureException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        protected EventBeanManufactureException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}