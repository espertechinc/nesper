///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
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

        public EventTable[] MakeEventTables(
            ExprEvaluatorContext exprEvaluatorContext,
            int? subqueryNumber)
        {
            return new EventTable[] { new UnindexedEventTableImpl(streamNum) };
        }

        public Type EventTableClass => typeof(UnindexedEventTable);

        public string ToQueryPlan()
        {
            return GetType().Name + " streamNum=" + streamNum;
        }

        public int StreamNum => streamNum;
    }
} // end of namespace