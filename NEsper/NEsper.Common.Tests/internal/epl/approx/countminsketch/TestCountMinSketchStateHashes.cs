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
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    [TestFixture]
    public class TestCountMinSketchStateHashes : AbstractTestBase
    {
        public static CountMinSketchSpecHashes DefaultSpec
        {
            get {
                var epsOfTotalCount = 0.0001;
                var confidence = 0.99;
                var seed = 1234567;
                return new CountMinSketchSpecHashes(epsOfTotalCount, confidence, seed);
            }
        }

        private long EstimateCount(
            CountMinSketchStateHashes state,
            string item)
        {
            return state.EstimateCount(GetBytes(item));
        }

        private void Add(
            CountMinSketchStateHashes state,
            string item,
            long count)
        {
            state.Add(GetBytes(item), count);
        }

        private static byte[] GetBytes(string item)
        {
            return Encoding.UTF8.GetBytes(item);
        }

        [Test]
        public void TestPerformanceMurmurHash()
        {
            var warmupLoopCount = 1; // 1000000;
            var measureLoopCount = 1; // 1000000000;

            // init
            string[] texts = {
                "joe", "melissa", "townhall",
                "ballpark", "trial-by-error",
                "house", "teamwork",
                "recommendation", "partial",
                "soccer ball"
            };
            var bytes = new byte[texts.Length][];
            for (var i = 0; i < texts.Length; i++)
            {
                bytes[i] = texts[i].GetUTF8Bytes();
            }

            // warmup
            for (var i = 0; i < warmupLoopCount; i++)
            {
                var bytearr = bytes[i % bytes.Length];
                var code = MurmurHash.Hash(bytearr, 0, bytearr.Length, 0);
                if (code == 0)
                {
                    Console.Out.WriteLine("A zero code");
                }
            }

            // run
            // 23.3 for 1G for MurmurHash.hash
            long start = PerformanceObserver.NanoTime;
            for (var i = 0; i < measureLoopCount; i++)
            {
                var bytearr = bytes[i % bytes.Length];
                var codeOne = MurmurHash.Hash(bytearr, 0, bytearr.Length, 0);
                if (codeOne == 0)
                {
                    Console.Out.WriteLine("A zero code");
                }
            }

            var delta = PerformanceObserver.NanoTime - start;
            // Comment me in - System.out.println("Delta " + (delta / 1000000000.0));
        }

        [Test]
        public void TestSimpleFlow()
        {
            var state = CountMinSketchStateHashes.MakeState(DefaultSpec);

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
            var eps = 0.001;
            var confidence = 0.999;

            var space = 2000;
            var points = 100000;

            var randomized = true;

            var random = new Random();
            var spec = new CountMinSketchSpecHashes(eps, confidence, 123456);
            var state = CountMinSketchStateHashes.MakeState(spec);

            IDictionary<ByteBuffer, long> sent = new Dictionary<ByteBuffer, long>();
            for (var i = 0; i < points; i++)
            {
                ByteBuffer bytes;
                if (randomized)
                {
                    bytes = TestCountMinSketchStateTopK.GenerateBytesRandom(random, space);
                }
                else
                {
                    bytes = TestCountMinSketchStateTopK.GenerateBytesModulo(i, space);
                }

                state.Add(bytes.Array, 1);

                if (!sent.TryGetValue(bytes, out var count))
                {
                    sent.Put(bytes, 1L);
                }
                else
                {
                    sent.Put(bytes, count + 1);
                }

                if (i > 0 && i % 100000 == 0)
                {
                    Console.Out.WriteLine("Completed " + i);
                }
            }

            // compare
            var errors = 0;
            foreach (var entry in sent)
            {
                var frequency = state.EstimateCount(entry.Key.Array);
                if (frequency != entry.Value)
                {
                    Console.Out.WriteLine("Expected " + entry.Value + " received " + frequency);
                    errors++;
                }
            }

            Console.Out.WriteLine("Found " + errors + " errors at space " + space + " sent " + points);
            Assert.That(eps * points, Is.GreaterThan(errors));
        }
    }
} // end of namespace
