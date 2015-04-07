///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>Results of an on-demand (fire-and-forget non-continuous) query. </summary>
    public interface EPOnDemandQueryResult
    {
        /// <summary>Returns an array representing query result rows, may return a null value or empty array to indicate an empty result set.</summary>
        /// <returns>result array</returns>
        EventBean[] Array { get; }

        /// <summary>Returns the event type of the result. </summary>
        /// <returns>event type of result row</returns>
        EventType EventType { get; }

        /// <summary>Returns an enumerator representing query result rows. </summary>
        /// <returns>result row enumerator</returns>
        IEnumerator<EventBean> GetEnumerator();
    }
}
