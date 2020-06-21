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

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    [TestFixture]
    public class TestCountMinSketchStateTopK : AbstractCommonTest
    {
        private void UpdateAssert(
            CountMinSketchState state,
            string value,
            string expected)
        {
            state.Add(value.GetUTF8Bytes(), 1);
            var topkValues = state.TopKValues;
            IList<Pair<long, object>> topkList = new List<Pair<long, object>>();
            foreach (var topkValue in topkValues)
            {
                var frequency = state.Frequency(topkValue.Array);
                var text = Encoding.UTF8.GetString(topkValue.Array);
                topkList.Add(new Pair<long, object>(frequency, text));
            }

            AssertList(expected, topkList);
        }

        private void AssertList(
            string pairText,
            IList<Pair<long, object>> asList)
        {
            var pairs = pairText.Split(',');
            Assert.AreEqual(pairs.Length, asList.Count, "received " + asList);
            foreach (var pair in pairs)
            {
                var pairArr = pair.Split('=');
                var pairExpected = new Pair<long, object>(long.Parse(pairArr[1]), pairArr[0]);
                var found = asList.Remove(pairExpected);
                Assert.IsTrue(found, "failed to find " + pairExpected + " among remaining " + asList);
            }
        }

        public static ByteBuffer GenerateBytesRandom(
            Random random,
            int space)
        {
            var val = random.Next(space);
            var bytes = Convert.ToString(val).GetUTF8Bytes();
            return new ByteBuffer(bytes);
        }

        public static ByteBuffer GenerateBytesModulo(
            int num,
            int space)
        {
            var value = Convert.ToString(num % space);
            return new ByteBuffer(value.GetUTF8Bytes());
        }

        [Test, RunInApplicationDomain]
        public void TestFlow()
        {
            // top-k for 3
            var spec = new CountMinSketchSpec();
            spec.HashesSpec = TestCountMinSketchStateHashes.DefaultSpec;
            spec.TopkSpec = 3;
            spec.Agent = new CountMinSketchAgentStringUTF16();
            var state = CountMinSketchState.MakeState(spec);

            UpdateAssert(state, "a", "a=1");
            UpdateAssert(state, "b", "a=1,b=1");
            UpdateAssert(state, "a", "a=2,b=1");
            UpdateAssert(state, "c", "a=2,b=1,c=1");
            UpdateAssert(state, "d", "a=2,b=1,c=1");
            UpdateAssert(state, "c", "a=2,b=1,c=2");
            UpdateAssert(state, "a", "a=3,b=1,c=2");
            UpdateAssert(state, "d", "a=3,d=2,c=2");
            UpdateAssert(state, "e", "a=3,d=2,c=2");
            UpdateAssert(state, "e", "a=3,d=2,c=2");
            UpdateAssert(state, "e", "a=3,e=3,c=2");
            UpdateAssert(state, "d", "a=3,e=3,d=3");
            UpdateAssert(state, "c", "a=3,e=3,d=3");
            UpdateAssert(state, "c", "a=3,e=3,c=4");
        }

        [Test, RunInApplicationDomain]
        public void TestTopK()
        {
            var space = 10000;
            var points = 100000;
            var topkMax = 100;

            var random = new Random();
            var topk = new CountMinSketchStateTopk(topkMax);
            var sent = new Dictionary<ByteBuffer, long>();
            for (var i = 0; i < points; i++)
            {
                // for simple population: ByteBuffer bytes = generateBytesModulo(i, space);
                var bytes = GenerateBytesRandom(random, space);
                //var bytes = GenerateBytesModulo(i, space);
                if (!sent.TryGetValue(bytes, out var count))
                {
                    sent.Put(bytes, 1L);
                    topk.UpdateExpectIncreasing(bytes.Array, 1);
                }
                else
                {
                    sent.Put(bytes, count + 1);
                    topk.UpdateExpectIncreasing(bytes.Array, count + 1);
                }

                if (i > 0 && i % 100000 == 0)
                {
                    Console.Out.WriteLine("Completed " + i);
                }
            }

            // compare
            var top = topk.TopKValues;

            // assert filled
            if (sent.Count < topkMax)
            {
                Assert.AreEqual(sent.Count, top.Count);
            }
            else
            {
                Assert.AreEqual(topkMax, top.Count);
            }

            // assert no duplicate values
            ISet<ByteBuffer> set = new HashSet<ByteBuffer>();
            foreach (var topBytes in top)
            {
                Assert.IsTrue(set.Add(topBytes));
            }

            // assert order descending
            long? lastFreq = null;
            foreach (var topBytes in top)
            {
                var freq = sent.Get(topBytes);
                if (lastFreq != null)
                {
                    Assert.IsTrue(freq <= lastFreq);
                }

                lastFreq = freq;
            }
        }
    }
} // end of namespace
