///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>Plan for a full table scan. </summary>
    public class FullTableScanLookupPlan : TableLookupPlan
    {
        /// <summary>Ctor. </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to full table scan</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        public FullTableScanLookupPlan(int lookupStream, int indexedStream, TableLookupIndexReqKey indexNum)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] { indexNum })
        {
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get
            {
                return new TableLookupKeyDesc(
                    new List<QueryGraphValueEntryHashKeyed>(),
                    new List<QueryGraphValueEntryRange>());
            }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            UnindexedEventTable index = (UnindexedEventTable) eventTable[0];
            return new FullTableScanLookupStrategy(index);
        }
    
        public override String ToString()
        {
            return "FullTableScanLookupPlan " +
                    base.ToString();
        }
    
    }
}
