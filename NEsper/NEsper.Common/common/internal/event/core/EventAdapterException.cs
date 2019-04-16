///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     This exception is thrown to indicate a problem resolving an event type by name.
    /// </summary>
    [Serializable]
    public class EventAdapterException : EPException
    {
        /// <summary> Ctor.</summary>
        /// <param name="message">
        ///     error message
        /// </param>
        public EventAdapterException(string message)
            : base(message)
        {
        }

        /// <summary> Ctor.</summary>
        /// <param name="message">
        ///     error message
        /// </param>
        /// <param name="nested">
        ///     nested exception
        /// </param>
        public EventAdapterException(
            string message,
            Exception nested)
            : base(message, nested)
        {
        }
    }
}