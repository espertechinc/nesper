///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class SortedTableLookupPlan : TableLookupPlan
    {
        private readonly QueryGraphValueEntryRange _rangeKeyPair;
        private readonly int _lookupStream;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to index table lookup</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="rangeKeyPair">The range key pair.</param>
        public SortedTableLookupPlan(int lookupStream, int indexedStream, TableLookupIndexReqKey indexNum, QueryGraphValueEntryRange rangeKeyPair)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] { indexNum })
        {
            _rangeKeyPair = rangeKeyPair;
            _lookupStream = lookupStream;
        }

        public QueryGraphValueEntryRange RangeKeyPair
        {
            get { return _rangeKeyPair; }
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get { return new TableLookupKeyDesc(new QueryGraphValueEntryHashKeyed[0], new[] {_rangeKeyPair}); }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            PropertySortedEventTable index = (PropertySortedEventTable) eventTable[0];
            return new SortedTableLookupStrategy(_lookupStream, -1, _rangeKeyPair, null, index);
        }
    
        public override String ToString()
        {
            return string.Format("SortedTableLookupPlan {0} keyProperties={1}", base.ToString(), _rangeKeyPair.Render());
        }
    }
}
