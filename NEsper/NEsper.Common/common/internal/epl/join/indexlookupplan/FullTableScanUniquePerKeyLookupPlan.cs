///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan for a full table scan.
    /// </summary>
    public class FullTableScanUniquePerKeyLookupPlan : TableLookupPlan
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to full table scan</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        public FullTableScanUniquePerKeyLookupPlan(
            int lookupStream,
            int indexedStream,
            TableLookupIndexReqKey[] indexNum)
            : base(
                lookupStream, indexedStream, indexNum)
        {
        }

        protected override JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTable,
            EventType[] eventTypes)
        {
            return new FullTableScanUniqueValueLookupStrategy((EventTableAsSet) eventTable[0]);
        }

        public override string ToString()
        {
            return "FullTableScanLookupPlan " +
                   base.ToString();
        }
    }
} // end of namespace