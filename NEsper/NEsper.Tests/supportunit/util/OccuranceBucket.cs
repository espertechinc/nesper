///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportunit.util
{
    public class OccuranceBucket
    {
        private readonly int _countEntry;
        private readonly int _countTotal;
        private readonly long _high;
        private readonly long _low;

        public OccuranceBucket(long low, long high, int countEntry, int countTotal)
        {
            _low = low;
            _high = high;
            _countEntry = countEntry;
            _countTotal = countTotal;
            InnerBuckets = new List<OccuranceBucket>();
        }

        public long Low
        {
            get { return _low; }
        }

        public long High
        {
            get { return _high; }
        }

        public int CountEntry
        {
            get { return _countEntry; }
        }

        public int CountTotal
        {
            get { return _countTotal; }
        }

        public IList<OccuranceBucket> InnerBuckets { get; set; }
    }
}
