///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.threading;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// A container for thread local Stats instances.
    /// Upon dump, data is gathered and merged from all registered thread local instances.
    /// The stat name is used as a key for a dump.
    /// </summary>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
    public class StatsHolder
    {
        public static int[] DEFAULT_MS = new[] { 1, 5, 10, 50, 100, 250, 500, 1000 };//ms
        public static int[] DEFAULT_NS = new[] { 5, 10, 15, 20, 25, 50, 100, 500, 1000, 2500, 5000 };//micro secs

        static StatsHolder()
        {
            for (int i = 0; i < DEFAULT_NS.Length; i++)
                DEFAULT_NS[i] *= 1000;//turn to ns
        }

        private static readonly List<Stats> STATS = new List<Stats>();

        public class ThreadStats
        {
            public Stats Engine;
            public Stats Server;
            public Stats EndToEnd;
        }

        private static readonly IThreadLocal<ThreadStats> threadStats =
            new FastThreadLocal<ThreadStats>(
                delegate
                    {
                        var threadStats = new ThreadStats();
                        lock(STATS) {
                            threadStats.Engine = new Stats("engine", "ns", DEFAULT_NS);
                            threadStats.Server = new Stats("server", "ns", DEFAULT_NS);
                            threadStats.EndToEnd = new Stats("endToEnd", "ms", DEFAULT_MS);

                            STATS.Add(threadStats.Engine);
                            STATS.Add(threadStats.Server);
                            STATS.Add(threadStats.EndToEnd);
                        }
                        return threadStats;
                    });

        public static ThreadStats All
        {
            get { return threadStats.GetOrCreate(); }
        }

        public static Stats Engine
        {
            get { return threadStats.GetOrCreate().Engine; }
        }

        public static Stats Server
        {
            get { return threadStats.GetOrCreate().Server; }
        }

        public static Stats EndToEnd
        {
            get { return threadStats.GetOrCreate().EndToEnd; }
        }

        public static void Remove(Stats stats)
        {
            lock (STATS) {
                STATS.Remove(stats);
            }
        }

        public static void Dump(String name)
        {
            Stats sum = null;
            lock (STATS) {
                foreach (Stats s in STATS) {
                    if (name == s.name) {
                        if (sum == null)
                            sum = Stats.CreateAndMergeFrom(s);
                        else
                            sum.Merge(s);
                    }
                }
            }
            if (sum != null)
                sum.Dump();
        }

        public static void Reset()
        {
            lock (STATS) {
                foreach (Stats s in STATS) {
                    s.Reset();
                }
            }
        }
    }
} // End of namespace
