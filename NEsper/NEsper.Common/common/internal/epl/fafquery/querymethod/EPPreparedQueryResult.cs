///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     The result of executing a prepared query.
    /// </summary>
    public class EPPreparedQueryResult
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">is the type of event produced by the query</param>
        /// <param name="result">the result rows</param>
        public EPPreparedQueryResult(EventType eventType, EventBean[] result)
        {
            EventType = eventType;
            Result = result;
        }

        /// <summary>
        ///     Returns the event type representing the selected columns.
        /// </summary>
        /// <returns>metadata</returns>
        public EventType EventType { get; }

        /// <summary>
        ///     Returns the query result.
        /// </summary>
        /// <returns>result rows</returns>
        public EventBean[] Result { get; }

        public static EPPreparedQueryResult Empty(EventType eventType)
        {
            return new EPPreparedQueryResult(eventType, CollectionUtil.EVENTBEANARRAY_EMPTY);
        }
    }
} // end of namespace