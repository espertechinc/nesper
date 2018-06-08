///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.supportunit.util
{
    public class OccuranceIntermediate
    {
        private readonly long high;
        private readonly IList<Pair<long, EventBean[]>> items;
        private readonly long low;

        public OccuranceIntermediate(long low, long high)
        {
            this.low = low;
            this.high = high;
            items = new List<Pair<long, EventBean[]>>();
        }

        public IList<Pair<long, EventBean[]>> Items
        {
            get { return items; }
        }

        public long Low
        {
            get { return low; }
        }

        public long High
        {
            get { return high; }
        }
    }
}
