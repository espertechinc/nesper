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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportunit.util
{
    public class OccuranceAnalyzer
    {
        public static readonly long MSEC_DIVISIOR = 1000*1000L;
        public static readonly long RESOLUTION = 1000*1000*1000L;

        public static OccuranceResult Analyze(List<Pair<long, EventBean[]>> occurances, long[] granularities)
        {
            long low = long.MaxValue;
            long high = long.MinValue;
            int countTotal = 0;

            foreach (var entry in occurances) {
                long time = entry.First;
                if (time < low) {
                    low = time;
                }
                if (time > high) {
                    high = time;
                }
                countTotal += entry.Second.Length;
            }

            IList<OccuranceBucket> buckets = RecursiveAnalyze(occurances, granularities, 0, low, high);
            return new OccuranceResult(occurances.Count, countTotal, low, high, RESOLUTION, buckets);
        }

        public static IList<OccuranceBucket> RecursiveAnalyze(IList<Pair<long, EventBean[]>> occurances,
                                                              long[] granularities,
                                                              int level,
                                                              long start,
                                                              long end)
        {
            // form buckets
            long granularity = granularities[level];
            IDictionary<int, OccuranceIntermediate> intermediates = new LinkedHashMap<int, OccuranceIntermediate>();
            int countBucket = 0;
            for (long offset = start; offset < end; offset += granularity) {
                var intermediate = new OccuranceIntermediate(offset, offset + granularity - 1);
                intermediates.Put(countBucket, intermediate);
                countBucket++;
            }

            // sort into bucket
            foreach (var entry in occurances) {
                long time = entry.First;
                long delta = time - start;
                var bucket = (int) (delta/granularity);
                OccuranceIntermediate intermediate = intermediates.Get(bucket);
                intermediate.Items.Add(entry);
            }

            // report each bucket6
            IList<OccuranceBucket> buckets = new List<OccuranceBucket>();
            foreach (var pair in intermediates) {
                OccuranceIntermediate inter = pair.Value;
                OccuranceBucket bucket = GetBucket(inter);
                buckets.Add(bucket);

                // for buckets within buckets
                if ((level < (granularities.Length - 1) && (inter.Items.IsNotEmpty()))) {
                    bucket.InnerBuckets = RecursiveAnalyze(inter.Items, granularities, level + 1, inter.Low, inter.High);
                }
            }

            return buckets;
        }

        private static OccuranceBucket GetBucket(OccuranceIntermediate inter)
        {
            int countTotal = 0;
            foreach (var entry in inter.Items) {
                countTotal += entry.Second.Length;
            }

            return new OccuranceBucket(inter.Low, inter.High, inter.Items.Count, countTotal);
        }
    }
}
