///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// A Stats instance gathers percentile based on a given histogram
    /// This class is thread unsafe.
    /// </summary>
    /// <unknown>
    /// @see com.espertech.esper.example.benchmark.server.StatsHolder for thread safe access
    /// Use CreateAndMergeFrom(proto) for best effort merge of this instance into the proto instance
    /// (no read / write lock is performed so the actual counts are a best effort)
    /// </unknown>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
    public class Stats
    {
        private int mustReset = 0;

        public readonly String name;
        public readonly String unit;
        private long count;
        private double avg;

        private int[] histogram;
        private long[] counts;

        public Stats(String name, String unit, params int[] hists)
        {//10, 20, (20+ implicit)
            this.name = name;
            this.unit = unit;
            histogram = new int[hists.Length + 1];//we add one slot for the implicit 20+
            Array.Copy(hists, 0, histogram, 0, hists.Length);
            histogram[histogram.Length - 1] = hists[hists.Length - 1] + 1;
            counts = new long[histogram.Length];
            for (int i = 0; i < counts.Length; i++)
                counts[i] = 0;
        }

        /// <summary>
        /// Use this method to merge this stat instance into a prototype one (for thread safe read only snapshoting)
        /// </summary>
        public static Stats CreateAndMergeFrom(Stats model)
        {
            Stats r = new Stats(model.name, model.unit, 0);
            r.histogram = new int[model.histogram.Length];
            Array.Copy(model.histogram, 0, r.histogram, 0, model.histogram.Length);
            r.counts = new long[model.histogram.Length];

            r.Merge(model);
            return r;
        }

        public void Update(long ns)
        {
            if (Interlocked.CompareExchange(ref mustReset, 1, 0) == 0)
                Internal_reset();

            if (ns < 0)
                return;

            count++;
            avg = (avg * (count - 1) + ns) / count;
            if (ns >= histogram[histogram.Length - 1])
            {
                counts[counts.Length - 1]++;
            }
            else
            {
                int index = 0;
                foreach (int level in histogram)
                {
                    if (ns < level)
                    {
                        counts[index]++;
                        break;
                    }
                    index++;
                }
            }
        }

        public void Dump()
        {
            Console.WriteLine("---Stats - " + name + " (unit: " + unit + ")");
            Console.WriteLine("  Avg: {0:F} #{1}", avg, count);
            int index = 0;
            long lastLevel = 0;
            long occurCumul = 0;
            foreach (long occur in counts)
            {
                occurCumul += occur;
                if (index != counts.Length - 1)
                {
                    Console.WriteLine("  {0,7} < {1,7}: {2,6:F2}% {3,6:F2}%% #{4}",
                            lastLevel, histogram[index], (float)occur / count * 100,
                            (float)occurCumul / count * 100, occur);
                    lastLevel = histogram[index];
                }
                else
                {
                    Console.WriteLine("  {0,7} <    more: {1,6:F2}%% {2,6:F2}%% #{3}", lastLevel, (float)occur / count * 100, 100f, occur);
                }
                index++;
            }
        }

        public void Merge(Stats stats)
        {
            // we assume same histogram - no check done here
            count += stats.count;
            avg = ((avg * count) + (stats.avg * stats.count)) / (count + stats.count);
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] += stats.counts[i];
            }
        }

        private void Internal_reset()
        {
            count = 0;
            avg = 0;
            for (int i = 0; i < counts.Length; i++)
                counts[i] = 0;
        }

        public void Reset()
        {
            Interlocked.Exchange(ref mustReset, 1);
        }
    }
} // End of namespace
