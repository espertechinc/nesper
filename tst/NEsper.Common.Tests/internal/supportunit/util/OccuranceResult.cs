///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class OccuranceResult
    {
        private readonly long resolution;

        public OccuranceResult(
            int countEntry,
            int countTotal,
            long? low,
            long? high,
            long resolution,
            IList<OccuranceBucket> buckets)
        {
            CountEntry = countEntry;
            CountTotal = countTotal;
            Low = low;
            High = high;
            this.resolution = resolution;
            Buckets = buckets;
        }

        public int CountEntry { get; }

        public int CountTotal { get; }

        public long? Low { get; }

        public long? High { get; }

        public IList<OccuranceBucket> Buckets { get; }

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("Total " + CountTotal + " entries " + CountEntry);
            buf.Append("\n");

            var count = 0;
            foreach (var bucket in Buckets)
            {
                Render(buf, 0, Convert.ToString(count), Low.Value, bucket);
            }

            return buf.ToString();
        }

        private static void Render(
            StringBuilder buf,
            int indent,
            string identifier,
            long start,
            OccuranceBucket bucket)
        {
            var lowRelative = (bucket.Low - start) / 1d / OccuranceAnalyzer.MSEC_DIVISIOR;
            var highRelative = (bucket.High - start) / 1d / OccuranceAnalyzer.MSEC_DIVISIOR;

            AddIndent(buf, indent);
            buf.Append(identifier);
            buf.Append(" ");
            buf.Append("[").Append(lowRelative).Append(", ").Append(highRelative).Append("]");
            buf.Append(" ");
            buf.Append(bucket.CountTotal).Append(" entries ").Append(bucket.CountEntry);
            buf.Append("\n");

            var count = 0;
            foreach (var inner in bucket.InnerBuckets)
            {
                Render(buf, indent + 1, identifier + "." + count, bucket.Low, inner);
                count++;
            }
        }

        private static void AddIndent(
            StringBuilder buf,
            int indent)
        {
            for (var i = 0; i < indent; i++)
            {
                buf.Append(" ");
                buf.Append(" ");
            }
        }
    }
} // end of namespace
