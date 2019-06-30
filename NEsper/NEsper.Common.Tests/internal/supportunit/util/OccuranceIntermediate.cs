///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class OccuranceIntermediate
    {
        public OccuranceIntermediate(
            long low,
            long high)
        {
            Low = low;
            High = high;
            Items = new List<Pair<long, EventBean[]>>();
        }

        public long Low { get; }

        public long High { get; }

        public IList<Pair<long, EventBean[]>> Items { get; }
    }
} // end of namespace