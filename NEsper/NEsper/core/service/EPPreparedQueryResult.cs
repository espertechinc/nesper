///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>The result of executing a prepared query. </summary>
    public class EPPreparedQueryResult
    {
        /// <summary>Ctor. </summary>
        /// <param name="eventType">is the type of event produced by the query</param>
        /// <param name="result">the result rows</param>
        public EPPreparedQueryResult(EventType eventType, EventBean[] result)
        {
            EventType = eventType;
            Result = result;
        }

        /// <summary>Returs the event type representing the selected columns. </summary>
        /// <value>metadata</value>
        public EventType EventType { get; private set; }

        /// <summary>Returns the query result. </summary>
        /// <value>result rows</value>
        public EventBean[] Result { get; private set; }
    }
}
