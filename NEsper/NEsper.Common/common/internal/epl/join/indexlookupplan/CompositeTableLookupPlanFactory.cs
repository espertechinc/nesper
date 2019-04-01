///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.composite;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class CompositeTableLookupPlanFactory : TableLookupPlan
    {
        private readonly ExprEvaluator hashKeys;
        private readonly QueryGraphValueEntryRange[] rangeKeyPairs;

        public CompositeTableLookupPlanFactory(
            int lookupStream, int indexedStream, TableLookupIndexReqKey[] indexNum, ExprEvaluator hashKeys,
            QueryGraphValueEntryRange[] rangeKeyPairs) : base(lookupStream, indexedStream, indexNum)
        {
            this.hashKeys = hashKeys;
            this.rangeKeyPairs = rangeKeyPairs;
        }

        protected override JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTables, EventType[] eventTypes)
        {
            var index = (PropertyCompositeEventTable) eventTables[0];
            return new CompositeTableLookupStrategy(
                eventTypes[LookupStream], LookupStream,
                hashKeys, rangeKeyPairs, index);
        }
    }
} // end of namespace