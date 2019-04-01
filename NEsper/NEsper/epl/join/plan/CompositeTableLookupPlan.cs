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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class CompositeTableLookupPlan : TableLookupPlan
    {
        private readonly IList<QueryGraphValueEntryHashKeyed> _hashKeys;
        private readonly IList<QueryGraphValueEntryRange> _rangeKeyPairs;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to index table lookup</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="hashKeys">The hash keys.</param>
        /// <param name="rangeKeyPairs">The range key pairs.</param>
        public CompositeTableLookupPlan(int lookupStream, int indexedStream, TableLookupIndexReqKey indexNum, IList<QueryGraphValueEntryHashKeyed> hashKeys, IList<QueryGraphValueEntryRange> rangeKeyPairs)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] { indexNum })
        {
            _hashKeys = hashKeys;
            _rangeKeyPairs = rangeKeyPairs;
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get { return new TableLookupKeyDesc(_hashKeys, _rangeKeyPairs); }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            var index = (PropertyCompositeEventTable)eventTable[0];
            return new CompositeTableLookupStrategy(eventTypes[LookupStream], LookupStream, _hashKeys, _rangeKeyPairs, index);
        }

        public override String ToString()
        {
            return string.Format("CompositeTableLookupPlan {0} directKeys={1} rangeKeys={2}",
                base.ToString(),
                _hashKeys.Render(),
                _rangeKeyPairs.Render());
        }
    }
}
