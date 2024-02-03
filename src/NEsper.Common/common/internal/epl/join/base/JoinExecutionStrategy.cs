///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Strategy for executing a join.
    /// </summary>
    public interface JoinExecutionStrategy
    {
        /// <summary>
        ///     Execute join. The first dimension in the 2-dim arrays is the stream that generated the events,
        ///     and the second dimension is the actual events generated.
        /// </summary>
        /// <param name="newDataPerStream">new events for each stream</param>
        /// <param name="oldDataPerStream">old events for each stream</param>
        void Join(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream);

        /// <summary>
        ///     A static join is for use with iterating over join statements.
        /// </summary>
        /// <returns>set of rows, each row with two or more events, one for each stream</returns>
        ISet<MultiKeyArrayOfKeys<EventBean>> StaticJoin();
    }
} // end of namespace