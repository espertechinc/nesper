///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.exec.sorted;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class SortedTableLookupPlanFactory : TableLookupPlan
    {
        internal readonly QueryGraphValueEntryRange rangeKeyPair;

        public SortedTableLookupPlanFactory(
            int lookupStream,
            int indexedStream,
            TableLookupIndexReqKey[] indexNum,
            QueryGraphValueEntryRange rangeKeyPair)
            : base(lookupStream, indexedStream, indexNum)
        {
            this.rangeKeyPair = rangeKeyPair;
        }

        protected override JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTables,
            EventType[] eventTypes)
        {
            var index = (PropertySortedEventTable)eventTables[0];
            return new SortedTableLookupStrategy(lookupStream, -1, rangeKeyPair, index);
        }
    }
} // end of namespace