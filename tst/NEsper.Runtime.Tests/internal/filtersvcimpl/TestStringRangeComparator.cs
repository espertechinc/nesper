///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestStringRangeComparator : AbstractRuntimeTest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test, RunInApplicationDomain]
        public void TestComparator()
        {
            var sorted = new SortedSet<StringRange>(new StringRangeComparator());

            string[][] TEST_SET = {
                new[] {"B", "G"},
                new[] {"B", "F"},
                new[] {null, "E"},
                new[] {"A", "F"},
                new[] {"A", "G"}
            };

            int[] EXPECTED_INDEX = { 2, 3, 4, 1, 0 };

            // Sort
            var ranges = new StringRange[TEST_SET.Length];
            for (var i = 0; i < TEST_SET.Length; i++)
            {
                ranges[i] = new StringRange(TEST_SET[i][0], TEST_SET[i][1]);
                sorted.Add(ranges[i]);
            }

            Log.Info("sorted=" + sorted);

            // Check results
            var count = 0;
            for (IEnumerator<StringRange> i = sorted.GetEnumerator(); i.MoveNext();)
            {
                StringRange range = i.Current;
                var indexExpected = EXPECTED_INDEX[count];
                var expected = ranges[indexExpected];

                Log.Debug(
                    ".testComparator count=" +
                    count +
                    " range=" +
                    range +
                    " expected=" +
                    expected);

                ClassicAssert.AreEqual(range, expected, "failed at count " + count);
                count++;
            }

            ClassicAssert.AreEqual(count, TEST_SET.Length);
        }
    }
} // end of namespace
