///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    [TestFixture]
    public class TestFilterSpecParamComparator : CommonTest
    {
        [SetUp]
        public void SetUp()
        {
            comparator = new FilterSpecParamComparator();
        }

        private FilterSpecParamComparator comparator;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestCompareAll()
        {
            var sorted = new SortedSet<FilterOperator>(comparator);

            foreach (var op in EnumHelper.GetValues<FilterOperator>())
            {
                sorted.Add(op);
            }

            Assert.AreEqual(FilterOperator.EQUAL, sorted.First());
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, sorted.Last());
            Assert.AreEqual(
                "[EQUAL, IS, IN_LIST_OF_VALUES, ADVANCED_INDEX, RANGE_OPEN, RANGE_HALF_OPEN, RANGE_HALF_CLOSED, RANGE_CLOSED, LESS, LESS_OR_EQUAL, GREATER_OR_EQUAL, GREATER, NOT_RANGE_CLOSED, NOT_RANGE_HALF_CLOSED, NOT_RANGE_HALF_OPEN, NOT_RANGE_OPEN, NOT_IN_LIST_OF_VALUES, NOT_EQUAL, IS_NOT, BOOLEAN_EXPRESSION]",
                sorted.ToString());

            log.Debug(".testCompareAll " + sorted.RenderAny());
        }

        [Test]
        public void TestCompareOneByOne()
        {
            var param1 = FilterOperator.EQUAL;
            var param4 = FilterOperator.RANGE_CLOSED;
            var param7 = FilterOperator.GREATER;
            var param8 = FilterOperator.NOT_EQUAL;
            var param9 = FilterOperator.IN_LIST_OF_VALUES;
            var param10 = FilterOperator.NOT_RANGE_CLOSED;
            var param11 = FilterOperator.NOT_IN_LIST_OF_VALUES;

            // Compare same comparison types
            Assert.IsTrue(comparator.Compare(param8, param1) == 1);
            Assert.IsTrue(comparator.Compare(param1, param8) == -1);

            Assert.IsTrue(comparator.Compare(param4, param4) == 0);

            // Compare across comparison types
            Assert.IsTrue(comparator.Compare(param7, param1) == 1);
            Assert.IsTrue(comparator.Compare(param1, param7) == -1);

            Assert.IsTrue(comparator.Compare(param4, param1) == 1);
            Assert.IsTrue(comparator.Compare(param1, param4) == -1);

            // 'in' is before all but after equals
            Assert.IsTrue(comparator.Compare(param9, param4) == -1);
            Assert.IsTrue(comparator.Compare(param9, param9) == 0);
            Assert.IsTrue(comparator.Compare(param9, param1) == 1);

            // inverted range is lower rank
            Assert.IsTrue(comparator.Compare(param10, param1) == 1);
            Assert.IsTrue(comparator.Compare(param10, param8) == -1);

            // not-in is lower rank
            Assert.IsTrue(comparator.Compare(param11, param1) == 1);
            Assert.IsTrue(comparator.Compare(param11, param8) == -1);
        }
    }
} // end of namespace