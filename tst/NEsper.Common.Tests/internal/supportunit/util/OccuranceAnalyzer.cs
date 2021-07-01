///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class OccuranceAnalyzer
    {
        public const long RESOLUTION = 1000 * 1000 * 1000L;
        public const long MSEC_DIVISIOR = 1000 * 1000L;

        public static OccuranceResult Analyze(
            IList<Pair<long, EventBean[]>> occurances,
            long[] granularities)
        {
            var low = long.MaxValue;
            var high = long.MinValue;
            var countTotal = 0;

            foreach (var entry in occurances)
            {
                var time = entry.First;
                if (time < low)
                {
                    low = time;
                }

                if (time > high)
                {
                    high = time;
                }

                countTotal += entry.Second.Length;
            }

            var buckets = RecursiveAnalyze(occurances, granularities, 0, low, high);
            return new OccuranceResult(occurances.Count, countTotal, low, high, RESOLUTION, buckets);
        }

        public static IList<OccuranceBucket> RecursiveAnalyze(
            IList<Pair<long, EventBean[]>> occurances,
            long[] granularities,
            int level,
            long start,
            long end)
        {
            // form buckets
            var granularity = granularities[level];
            IDictionary<int, OccuranceIntermediate> intermediates = new LinkedHashMap<int, OccuranceIntermediate>();
            var countBucket = 0;
            for (var offset = start; offset < end; offset += granularity)
            {
                var intermediate = new OccuranceIntermediate(offset, offset + granularity - 1);
                intermediates.Put(countBucket, intermediate);
                countBucket++;
            }

            // sort into bucket
            foreach (var entry in occurances)
            {
                long time = entry.First;
                var delta = time - start;
                var bucket = (int) (delta / granularity);
                var intermediate = intermediates.Get(bucket);
                intermediate.Items.Add(entry);
            }

            // report each bucket
            IList<OccuranceBucket> buckets = new List<OccuranceBucket>();
            foreach (KeyValuePair<int, OccuranceIntermediate> pair in intermediates)
            {
                var inter = pair.Value;
                var bucket = GetBucket(inter);
                buckets.Add(bucket);

                // for buckets within buckets
                if (level < granularities.Length - 1 && !inter.Items.IsEmpty())
                {
                    bucket.InnerBuckets = RecursiveAnalyze(inter.Items, granularities, level + 1, inter.Low, inter.High);
                }
            }

            return buckets;
        }

        private static OccuranceBucket GetBucket(OccuranceIntermediate inter)
        {
            var countTotal = 0;
            foreach (Pair<long, EventBean[]> entry in inter.Items)
            {
                countTotal += entry.Second.Length;
            }

            return new OccuranceBucket(inter.Low, inter.High, inter.Items.Count, countTotal);
        }
    }
} // end of namespace
