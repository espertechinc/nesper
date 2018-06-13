///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.approx
{
    [TestFixture]
    public class TestCountMinSketchStateHashes
    {
        [Test]
        public void TestSimpleFlow()
        {
            var state = CountMinSketchStateHashes.MakeState(GetDefaultSpec());

            Add(state, "hello", 100);
            Assert.AreEqual(100, EstimateCount(state, "hello"));

            Add(state, "text", 1);
            Assert.AreEqual(1, EstimateCount(state, "text"));

            Add(state, "hello", 3);
            Assert.AreEqual(103, EstimateCount(state, "hello"));
            Assert.AreEqual(1, EstimateCount(state, "text"));
        }

        [Test]
        public void TestSpace()
        {
            const double eps = 0.001;
            const double confidence = 0.999;

            const int space = 2000;
            const int points = 100000;

            const bool randomized = true;

            var random = new Random();
            var spec = new CountMinSketchSpecHashes(eps, confidence, 123456);
            var state = CountMinSketchStateHashes.MakeState(spec);

            var sent = new Dictionary<Blob, long?>();
            for (var i = 0; i < points; i++)
            {
                Blob bytes;
                if (randomized) {
                    bytes = TestCountMinSketchStateTopK.GenerateBytesRandom(random, space);
                } else {
                    bytes = TestCountMinSketchStateTopK.GenerateBytesModulo(i, space);
                }
                state.Add(bytes.Data, 1);

                var count = sent.Get(bytes);
                if (count == null) {
                    sent.Put(bytes, 1L);
                }
                else {
                    sent.Put(bytes, count + 1);
                }

                if (i > 0 && i % 100000 == 0)
                {
                    Console.WriteLine("Completed {0}", i);
                }
            }

            // compare
            var errors = 0;
            foreach (var entry in sent) {
                var frequency = state.EstimateCount(entry.Key.Data);
                if (frequency != entry.Value) {
                    Console.WriteLine("Expected {0} received {1}", entry.Value, frequency);
                    errors++;
                }
            }
            Console.WriteLine("Found {0} errors at space {1} sent {2}", errors, space, points);
            Assert.IsTrue(eps * points > errors);
        }

        [Test]
        public void TestPerformanceMurmurHash()
        {
            const int warmupLoopCount = 1; // 1000000;
            const int measureLoopCount = 1; // 1000000000;

            // init
            var texts = new string[] {"joe", "melissa", "townhall", "ballpark", "trial-by-error", "house", "teamwork", "recommendation", "partial", "soccer ball"};
            var bytes = new byte[texts.Length][];
            for (var i = 0; i < texts.Length; i++) {
                bytes[i] = Encoding.Unicode.GetBytes(texts[i]);
            }

            // warmup
            for (var i = 0; i < warmupLoopCount; i++) {
                var bytearr = bytes[i % bytes.Length];
                var code = MurmurHash.Hash(bytearr, 0, bytearr.Length, 0);
                if (code == 0) {
                    Console.WriteLine("A zero code");
                }
            }

            // run
            // 23.3 for 1G for MurmurHash.hash
            var delta = PerformanceObserver.TimeNano(() =>
            {
                for (var i = 0; i < measureLoopCount; i++)
                {
                    var bytearr = bytes[i % bytes.Length];
                    var codeOne = MurmurHash.Hash(bytearr, 0, bytearr.Length, 0);
                    if (codeOne == 0)
                    {
                        Console.WriteLine("A zero code");
                    }
                }
            });
            // Comment me in - Console.WriteLine("Delta " + (delta / 1000000000.0));
        }

        internal static CountMinSketchSpecHashes GetDefaultSpec()
        {
            const double epsOfTotalCount = 0.0001;
            const double confidence = 0.99;
            const int seed = 1234567;
            return new CountMinSketchSpecHashes(epsOfTotalCount, confidence, seed);
        }

        private static long EstimateCount(CountMinSketchStateHashes state, string item)
        {
            return state.EstimateCount(GetBytes(item));
        }

        private static void Add(CountMinSketchStateHashes state, string item, long count)
        {
            state.Add(GetBytes(item), count);
        }

        private static byte[] GetBytes(string item) {
            return Encoding.Unicode.GetBytes(item);
            //return item.GetBytes("UTF-16");
        }
    }
}
