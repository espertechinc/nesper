///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan for a full table scan.
    /// </summary>
    public class FullTableScanUniquePerKeyLookupPlan : TableLookupPlan
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to full table scan</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        public FullTableScanUniquePerKeyLookupPlan(int lookupStream, int indexedStream, TableLookupIndexReqKey indexNum)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] {indexNum})
        {
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get
            {
                return new TableLookupKeyDesc(
                    Collections.GetEmptyList<QueryGraphValueEntryHashKeyed>(),
                    Collections.GetEmptyList<QueryGraphValueEntryRange>());
            }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            return new FullTableScanUniqueValueLookupStrategy((EventTableAsSet) eventTable[0]);
        }
    
        public override string ToString()
        {
            return "FullTableScanLookupPlan " + base.ToString();
        }
    
    }
}
