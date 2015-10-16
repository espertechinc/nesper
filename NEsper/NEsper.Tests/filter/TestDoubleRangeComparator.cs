///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestDoubleRangeComparator 
    {
        [Test]
        public void TestComparator()
        {
            var sorted = new SortedSet<DoubleRange>(new DoubleRangeComparator());

            double[][] testSet =
                {
                    new double[] {10, 20}, // 4
                    new double[] {10, 15}, // 2
                    new double[] {10, 25}, // 5
                    new double[] {5, 20}, // 0
                    new double[] {5, 25}, // 1
                    new double[] {15, 20}, // 6
                    new double[] {10, 16} // 3
                };
    
            int[] expectedIndex = {3, 4, 1, 6, 0, 2, 5};
    
            // Sort
            var ranges = new DoubleRange[testSet.Length];
            for (int i = 0; i < testSet.Length; i++)
            {
                ranges[i] = new DoubleRange(testSet[i][0], testSet[i][1]);
                sorted.Add(ranges[i]);
            }
    
            // Check results
            int count = 0;
            for (IEnumerator<DoubleRange> i = sorted.GetEnumerator(); i.MoveNext();)
            {
                DoubleRange range = i.Current;
                int indexExpected = expectedIndex[count];
                DoubleRange expected = ranges[indexExpected];
    
                Log.Debug(".testComparator count=" + count +
                        " range=" + range +
                        " expected=" + expected);
    
                Assert.AreEqual(range, expected);
                count++;
            }
            Assert.AreEqual(count, testSet.Length);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
