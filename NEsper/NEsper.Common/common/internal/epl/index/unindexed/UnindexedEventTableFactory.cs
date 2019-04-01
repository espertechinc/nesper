///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.unindexed
{
    /// <summary>
    /// Factory for simple table of events without an index.
    /// </summary>
    public class UnindexedEventTableFactory : EventTableFactory
    {
        protected readonly int streamNum;

        public UnindexedEventTableFactory(int streamNum)
        {
            this.streamNum = streamNum;
        }

        public EventTable[] MakeEventTables(AgentInstanceContext agentInstanceContext, int? subqueryNumber)
        {
            return new EventTable[] { new UnindexedEventTableImpl(streamNum) };
        }

        public Type EventTableClass
        {
            get => typeof(UnindexedEventTable);
        }

        public string ToQueryPlan()
        {
            return this.GetType().Name + " streamNum=" + streamNum;
        }

        public int StreamNum
        {
            get => streamNum;
        }
    }
} // end of namespace