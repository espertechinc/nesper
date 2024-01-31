///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.rep;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Interface for indicating a result in the form of a single row of multiple events, which could
    ///     represent either a full result over all streams or a partial result over a subset of streams.
    /// </summary>
    public interface ResultAssembler
    {
        /// <summary>
        ///     Publish a result row.
        /// </summary>
        /// <param name="row">is the result to publish</param>
        /// <param name="fromStreamNum">is the originitor that publishes the row</param>
        /// <param name="myEvent">is optional and is the event that led to the row result</param>
        /// <param name="myNode">is optional and is the result node of the event that led to the row result</param>
        /// <param name="resultFinalRows">is the final result rows</param>
        /// <param name="resultRootEvent">root event</param>
        void Result(
            EventBean[] row,
            int fromStreamNum,
            EventBean myEvent,
            Node myNode,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent);
    }
} // end of namespace