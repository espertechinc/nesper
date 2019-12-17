///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class OccuranceBucket
    {
        public OccuranceBucket(
            long low,
            long high,
            int countEntry,
            int countTotal)
        {
            Low = low;
            High = high;
            CountEntry = countEntry;
            CountTotal = countTotal;
            InnerBuckets = new EmptyList<OccuranceBucket>();
        }

        public long Low { get; }

        public long High { get; }

        public int CountEntry { get; }

        public int CountTotal { get; }

        public IList<OccuranceBucket> InnerBuckets { get; set; }
    }
} // end of namespace
