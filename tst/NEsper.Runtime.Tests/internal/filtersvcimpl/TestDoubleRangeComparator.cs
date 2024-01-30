///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestDoubleRangeComparator : AbstractRuntimeTest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test, RunInApplicationDomain]
        public void TestComparator()
        {
            var sorted = new SortedSet<DoubleRange>(new DoubleRangeComparator());

            double[][] TEST_SET = {
                new double[] {10, 20}, // 4
                new double[] {10, 15}, // 2
                new double[] {10, 25}, // 5
                new double[] {5, 20}, // 0
                new double[] {5, 25}, // 1
                new double[] {15, 20}, // 6
                new double[] {10, 16}
            }; // 3

            int[] EXPECTED_INDEX = { 3, 4, 1, 6, 0, 2, 5 };

            // Sort
            var ranges = new DoubleRange[TEST_SET.Length];
            for (var i = 0; i < TEST_SET.Length; i++)
            {
                ranges[i] = new DoubleRange(TEST_SET[i][0], TEST_SET[i][1]);
                sorted.Add(ranges[i]);
            }

            // Check results
            var count = 0;
            for (IEnumerator<DoubleRange> i = sorted.GetEnumerator(); i.MoveNext();)
            {
                DoubleRange range = i.Current;
                var indexExpected = EXPECTED_INDEX[count];
                var expected = ranges[indexExpected];

                Log.Debug(
                    ".testComparator count=" +
                    count +
                    " range=" +
                    range +
                    " expected=" +
                    expected);

                Assert.AreEqual(range, expected);
                count++;
            }

            Assert.AreEqual(count, TEST_SET.Length);
        }
    }
} // end of namespace
