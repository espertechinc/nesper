///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.unindexed;
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan for a full table scan.
    /// </summary>
    public class FullTableScanLookupPlanFactory : TableLookupPlan
    {
        public FullTableScanLookupPlanFactory(int lookupStream, int indexedStream, TableLookupIndexReqKey[] indexes) :
            base(lookupStream, indexedStream, indexes)
        {
        }

        protected override JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTable, EventType[] eventTypes)
        {
            var index = (UnindexedEventTable) eventTable[0];
            return new FullTableScanLookupStrategy(index);
        }

        public override string ToString()
        {
            return "FullTableScanLookupPlan " +
                   base.ToString();
        }
    }
} // end of namespace