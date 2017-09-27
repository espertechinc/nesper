///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.supportunit.util
{
    public class OccuranceResult
    {
        private readonly IList<OccuranceBucket> buckets;
        private readonly int countEntry;
        private readonly int countTotal;
        private readonly long high;
        private readonly long low;
        private long resolution;

        public OccuranceResult(int countEntry, int countTotal, long low, long high, long resolution,
                               IList<OccuranceBucket> buckets)
        {
            this.countEntry = countEntry;
            this.countTotal = countTotal;
            this.low = low;
            this.high = high;
            this.resolution = resolution;
            this.buckets = buckets;
        }

        public int CountEntry
        {
            get { return countEntry; }
        }

        public int CountTotal
        {
            get { return countTotal; }
        }

        public long? Low
        {
            get { return low; }
        }

        public long? High
        {
            get { return high; }
        }

        public IList<OccuranceBucket> Buckets
        {
            get { return buckets; }
        }

        public override String ToString()
        {
            var writer = new StringWriter();
            writer.WriteLine("Total " + countTotal + " entries " + countEntry);

            int count = 0;
            foreach (OccuranceBucket bucket in buckets) {
                Render(writer, 0, Convert.ToString(count), low, bucket);
            }

            return writer.ToString();
        }

        private static void Render(TextWriter writer, int indent, String identifier, long start, OccuranceBucket bucket)
        {
            double lowRelative = (bucket.Low - start)/1d/OccuranceAnalyzer.MSEC_DIVISIOR;
            double highRelative = (bucket.High - start)/1d/OccuranceAnalyzer.MSEC_DIVISIOR;

            AddIndent(writer, indent);
            writer.WriteLine("{0} [{1}, {2}] {3} entries {4}",
                             identifier,
                             lowRelative,
                             highRelative,
                             bucket.CountTotal,
                             bucket.CountEntry);

            int count = 0;
            foreach (OccuranceBucket inner in bucket.InnerBuckets) {
                Render(writer, indent + 1, identifier + "." + count, bucket.Low, inner);
                count++;
            }
        }

        private static void AddIndent(TextWriter writer, int indent)
        {
            for (int i = 0; i < indent; i++) {
                writer.Write("  ");
            }
        }
    }
}
