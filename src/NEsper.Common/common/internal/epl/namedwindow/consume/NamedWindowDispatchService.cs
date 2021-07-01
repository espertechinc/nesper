///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    /// <summary>
    /// Service to manage named window dispatches, locks and processors on an runtime level.
    /// </summary>
    public interface NamedWindowDispatchService
    {
        /// <summary>
        /// For use to add a result of a named window that must be dispatched to consuming views.
        /// </summary>
        /// <param name="delta">is the result to dispatch</param>
        /// <param name="consumers">is the destination of the dispatch, a map of statements to one or more consuming views</param>
        /// <param name="latchFactory">latch factory</param>
        void AddDispatch(
            NamedWindowConsumerLatchFactory latchFactory,
            NamedWindowDeltaData delta,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers);

        /// <summary>
        /// Dispatch events of the insert and remove stream of named windows to consumers, as part of the
        /// main event processing or dispatch loop.
        /// </summary>
        /// <returns>send events to consuming statements</returns>
        bool Dispatch();

        /// <summary>
        /// Destroy service.
        /// </summary>
        void Destroy();
    }
} // end of namespace