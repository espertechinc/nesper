///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    ///     Context provided to <seealso cref="ExceptionHandler" /> implementations providing
    ///     exception-contextual information as well as the exception itself,
    ///     for use with inbound pools and for exceptions unassociated to statements when using inbound pools.
    /// </summary>
    public class ExceptionHandlerContextUnassociated
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="exception">exception</param>
        /// <param name="currentEvent">the event when applicable</param>
        public ExceptionHandlerContextUnassociated(string engineURI, Exception exception, object currentEvent)
        {
            EngineURI = engineURI;
            Exception = exception;
            CurrentEvent = currentEvent;
        }

        /// <summary>
        ///     Returns the engine URI.
        /// </summary>
        /// <value>engine URI</value>
        public string EngineURI { get; }

        /// <summary>
        ///     Returns the exception.
        /// </summary>
        /// <value>exception</value>
        public Exception Exception { get; }

        /// <summary>
        ///     Returns the current event, when available.
        /// </summary>
        /// <value>current event or null if not available</value>
        public object CurrentEvent { get; }
    }
} // end of namespace