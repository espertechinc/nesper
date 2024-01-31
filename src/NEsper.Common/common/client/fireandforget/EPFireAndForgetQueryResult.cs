///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.fireandforget
{
    /// <summary>
    ///     Result for fire-and-forget queries.
    /// </summary>
    public interface EPFireAndForgetQueryResult
    {
        /// <summary>
        ///     Returns an array representing query result rows, may return a null value or empty array to indicate an empty result
        ///     set.
        /// </summary>
        /// <value>result array</value>
        EventBean[] Array { get; }

        /// <summary>
        ///     Returns the event type of the result.
        /// </summary>
        /// <value>event type of result row</value>
        EventType EventType { get; }

        /// <summary>
        ///     Returns an iterator representing query result rows.
        /// </summary>
        /// <returns>result row iterator</returns>
        IEnumerator<EventBean> GetEnumerator();
    }
} // end of namespace