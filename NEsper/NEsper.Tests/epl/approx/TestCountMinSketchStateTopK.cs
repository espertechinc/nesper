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

using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.epl.approx
{
    [TestFixture]
    public class TestCountMinSketchStateTopK 
    {
        [Test]
        public void TestTopK()
        {
            const int space = 10000;
            const int points = 100000;
            const int topkMax = 100;
    
            var random = new Random();
            var topk = new CountMinSketchStateTopk(topkMax);
            var sent = new Dictionary<Blob, long?>();
            for (var i = 0; i < points; i++) {
                // for simple population: Blob bytes = generateBytesModulo(i, space);
                var bytes = GenerateBytesRandom(random, space);
                var count = sent.Get(bytes);
                if (count == null) {
                    sent.Put(bytes, 1L);
                    topk.UpdateExpectIncreasing(bytes.Data, 1);
                }
                else {
                    sent.Put(bytes, count + 1);
                    topk.UpdateExpectIncreasing(bytes.Data, count.Value + 1);
                }
    
                if (i > 0 && i % 100000 == 0) {
                    Console.WriteLine("Completed {0}", i);
                }
            }
    
            // compare
            var top = topk.TopKValues;
    
            // assert filled
            if (sent.Count < topkMax) {
                Assert.AreEqual(sent.Count, top.Count);
            }
            else {
                Assert.AreEqual(topkMax, top.Count);
            }
    
            // assert no duplicate values
            var set = new HashSet<Blob>();
            foreach (var topBytes in top) {
                Assert.IsTrue(set.Add(topBytes));
            }
    
            // assert order descending
            long? lastFreq = null;
            foreach (var topBytes in top) {
                var freq = sent.Get(topBytes);
                if (lastFreq != null) {
                    Assert.IsTrue(freq <= lastFreq);
                }
                lastFreq = freq;
            }
        }
    
        [Test]
        public void TestFlow()
        {
            // top-k for 3
            var spec = new CountMinSketchSpec(TestCountMinSketchStateHashes.GetDefaultSpec(), 3, new CountMinSketchAgentStringUTF16());
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
    
        private void UpdateAssert(CountMinSketchState state, string value, string expected)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            state.Add(bytes, 1);
            var topkValues = state.TopKValues;
            var topkList = new List<Pair<long, object>>();
            foreach (var topkValue in topkValues)
            {
                var array = topkValue.Data;
                var frequency = state.Frequency(array);
                var text = Encoding.UTF8.GetString(array);
                topkList.Add(new Pair<long, object>(frequency, text));
            }
            AssertList(expected, topkList);
        }
    
        private void AssertList(string pairText, IList<Pair<long, object>> asList)
        {
            var pairs = pairText.Split(',');
            Assert.AreEqual(pairs.Length, asList.Count, "received " + asList.Render());
            foreach (var pair in pairs)
            {
                var pairArr = pair.Split('=');
                var pairExpected = new Pair<long, object>(long.Parse(pairArr[1]), pairArr[0]);
                var found = asList.Remove(pairExpected);
                Assert.IsTrue(found, "failed to find " + pairExpected + " among remaining " + asList.Render());
            }
        }

        public static Blob GenerateBytesRandom(Random random, int space)
        {
            var val = random.Next(space);
            byte[] bytes = val.ToString().GetUnicodeBytes();
            return new Blob(bytes);
        }

        public static Blob GenerateBytesModulo(int num, int space)
        {
            string value = (num%space).ToString();
            return new Blob(value.GetUnicodeBytes());
        }
    }
}
