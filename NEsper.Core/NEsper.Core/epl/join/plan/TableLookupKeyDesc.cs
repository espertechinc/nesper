///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.join.plan
{
    public class TableLookupKeyDesc
    {
        public TableLookupKeyDesc(IList<QueryGraphValueEntryHashKeyed> hashes, IList<QueryGraphValueEntryRange> ranges)
        {
            Hashes = hashes;
            Ranges = ranges;
        }

        public IList<QueryGraphValueEntryHashKeyed> Hashes { get; private set; }

        public IList<QueryGraphValueEntryRange> Ranges { get; private set; }

        public override string ToString()
        {
            return "TableLookupKeyDesc{" +
                    "hashes=" + QueryGraphValueEntryHashKeyed.ToQueryPlan(Hashes) +
                    ", btree=" + QueryGraphValueEntryRange.ToQueryPlan(Ranges) +
                    '}';
        }
    }
}