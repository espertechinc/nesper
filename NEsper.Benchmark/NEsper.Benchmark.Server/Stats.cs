///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private int _mustReset = 0;

        public readonly string name;
        public readonly string unit;
        private long _count;
        private double _avg;

        private int[] _histogram;
        private long[] _counts;

        public Stats(string name, string unit, params int[] hists)
        {//10, 20, (20+ implicit)
            this.name = name;
            this.unit = unit;
            _histogram = new int[hists.Length + 1];//we add one slot for the implicit 20+
            Array.Copy(hists, 0, _histogram, 0, hists.Length);
            _histogram[_histogram.Length - 1] = hists[hists.Length - 1] + 1;
            _counts = new long[_histogram.Length];
            for (var i = 0; i < _counts.Length; i++)
                _counts[i] = 0;
        }

        /// <summary>
        /// Use this method to merge this stat instance into a prototype one (for thread safe read only snapshoting)
        /// </summary>
        public static Stats CreateAndMergeFrom(Stats model)
        {
            var r = new Stats(model.name, model.unit, 0);
            r._histogram = new int[model._histogram.Length];
            Array.Copy(model._histogram, 0, r._histogram, 0, model._histogram.Length);
            r._counts = new long[model._histogram.Length];

            r.Merge(model);
            return r;
        }

        public void Update(long ns)
        {
            if (Interlocked.CompareExchange(ref _mustReset, 1, 0) == 0)
                Internal_reset();

            if (ns < 0)
                return;

            _count++;
            _avg = (_avg * (_count - 1) + ns) / _count;
            if (ns >= _histogram[_histogram.Length - 1])
            {
                _counts[_counts.Length - 1]++;
            }
            else
            {
                var index = 0;
                foreach (var level in _histogram)
                {
                    if (ns < level)
                    {
                        _counts[index]++;
                        break;
                    }
                    index++;
                }
            }
        }

        public void Dump()
        {
            Console.WriteLine("---Stats - " + name + " (unit: " + unit + ")");
            Console.WriteLine("  Avg: {0:F} #{1}", _avg, _count);
            var index = 0;
            long lastLevel = 0;
            long occurCumul = 0;
            foreach (var occur in _counts)
            {
                occurCumul += occur;
                if (index != _counts.Length - 1)
                {
                    Console.WriteLine("  {0,7} < {1,7}: {2,6:F2}% {3,6:F2}%% #{4}",
                            lastLevel, _histogram[index], (float)occur / _count * 100,
                            (float)occurCumul / _count * 100, occur);
                    lastLevel = _histogram[index];
                }
                else
                {
                    Console.WriteLine("  {0,7} <    more: {1,6:F2}%% {2,6:F2}%% #{3}", lastLevel, (float)occur / _count * 100, 100f, occur);
                }
                index++;
            }
        }

        public void Merge(Stats stats)
        {
            // we assume same histogram - no check done here
            _count += stats._count;
            _avg = ((_avg * _count) + (stats._avg * stats._count)) / (_count + stats._count);
            for (var i = 0; i < _counts.Length; i++)
            {
                _counts[i] += stats._counts[i];
            }
        }

        private void Internal_reset()
        {
            _count = 0;
            _avg = 0;
            for (var i = 0; i < _counts.Length; i++)
                _counts[i] = 0;
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _mustReset, 1);
        }
    }
} // End of namespace
