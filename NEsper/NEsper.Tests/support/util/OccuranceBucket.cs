///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace com.espertech.esper.support.util
{
    public class OccuranceBucket
    {
        private readonly int countEntry;
        private readonly int countTotal;
        private readonly long high;
        private readonly long low;

        public OccuranceBucket(long low, long high, int countEntry, int countTotal)
        {
            this.low = low;
            this.high = high;
            this.countEntry = countEntry;
            this.countTotal = countTotal;
            InnerBuckets = new List<OccuranceBucket>();
        }

        public long Low
        {
            get { return low; }
        }

        public long High
        {
            get { return high; }
        }

        public int CountEntry
        {
            get { return countEntry; }
        }

        public int CountTotal
        {
            get { return countTotal; }
        }

        public IList<OccuranceBucket> InnerBuckets { get; set; }
    }
}
