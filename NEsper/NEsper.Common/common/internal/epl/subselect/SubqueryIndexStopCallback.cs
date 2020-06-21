///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    /// <summary>
    ///     Implements a stop callback for use with subqueries to clear their indexes
    ///     when a statement is stopped.
    /// </summary>
    public class SubqueryIndexStopCallback : AgentInstanceMgmtCallback
    {
        private readonly EventTable[] eventIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventIndex">index to clear</param>
        public SubqueryIndexStopCallback(EventTable[] eventIndex)
        {
            this.eventIndex = eventIndex;
        }

        // Clear out index on statement stop

        public void Stop(AgentInstanceStopServices services)
        {
            if (eventIndex != null) {
                foreach (var table in eventIndex) {
                    table.Destroy();
                }
            }
        }
    }
} // end of namespace