///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestStringRangeComparator 
    {
        [Test]
        public void TestComparator()
        {
            SortedSet<StringRange> sorted = new SortedSet<StringRange>(
                new StringRangeComparator());
    
            String[][] TEST_SET =
            {
                new[] {"B", "G"},
                new[] {"B", "F"},
                new[] {null, "E"},
                new[] {"A", "F"},
                new[] {"A", "G"},
            };
    
            int[] EXPECTED_INDEX = {2,3,4,1,0};
    
            // Sort
            var ranges = new StringRange[TEST_SET.Length];
            for (int i = 0; i < TEST_SET.Length; i++)
            {
                ranges[i] = new StringRange(TEST_SET[i][0], TEST_SET[i][1]);
                sorted.Add(ranges[i]);
            }
    
            // Check results
            int count = 0;

            foreach(var range in sorted)
            {
                var indexExpected = EXPECTED_INDEX[count];
                var expected = ranges[indexExpected];
    
                Log.Debug(".testComparator count=" + count +
                        " range=" + range +
                        " expected=" + expected);

                Assert.AreEqual(range, expected, "failed at count " + count);
                count++;
            }
            Assert.AreEqual(count, TEST_SET.Length);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
