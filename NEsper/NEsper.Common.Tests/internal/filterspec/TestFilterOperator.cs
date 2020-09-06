///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.filterspec
{
    [TestFixture]
    public class TestFilterOperator : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestRanges()
        {
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, false, false) == FilterOperator.RANGE_OPEN);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, true, false) == FilterOperator.RANGE_CLOSED);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, false, false) == FilterOperator.RANGE_HALF_OPEN);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, true, false) == FilterOperator.RANGE_HALF_CLOSED);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, false, true) == FilterOperator.NOT_RANGE_OPEN);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, true, true) == FilterOperator.NOT_RANGE_CLOSED);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, false, true) == FilterOperator.NOT_RANGE_HALF_OPEN);
            Assert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, true, true) == FilterOperator.NOT_RANGE_HALF_CLOSED);
        }

        [Test, RunInApplicationDomain]
        public void TestIsComparison()
        {
            Assert.IsTrue(FilterOperator.GREATER.IsComparisonOperator());
            Assert.IsTrue(FilterOperator.GREATER_OR_EQUAL.IsComparisonOperator());
            Assert.IsTrue(FilterOperator.LESS.IsComparisonOperator());
            Assert.IsTrue(FilterOperator.LESS_OR_EQUAL.IsComparisonOperator());
            Assert.IsFalse(FilterOperator.RANGE_CLOSED.IsComparisonOperator());
            Assert.IsFalse(FilterOperator.EQUAL.IsComparisonOperator());
            Assert.IsFalse(FilterOperator.NOT_EQUAL.IsComparisonOperator());
        }

        [Test, RunInApplicationDomain]
        public void TestIsRange()
        {
            Assert.IsTrue(FilterOperator.RANGE_OPEN.IsRangeOperator());
            Assert.IsTrue(FilterOperator.RANGE_CLOSED.IsRangeOperator());
            Assert.IsTrue(FilterOperator.RANGE_HALF_OPEN.IsRangeOperator());
            Assert.IsTrue(FilterOperator.RANGE_HALF_CLOSED.IsRangeOperator());
            Assert.IsFalse(FilterOperator.NOT_RANGE_HALF_CLOSED.IsRangeOperator());
            Assert.IsFalse(FilterOperator.NOT_RANGE_OPEN.IsRangeOperator());
            Assert.IsFalse(FilterOperator.NOT_RANGE_CLOSED.IsRangeOperator());
            Assert.IsFalse(FilterOperator.NOT_RANGE_HALF_OPEN.IsRangeOperator());
            Assert.IsFalse(FilterOperator.LESS.IsRangeOperator());
            Assert.IsFalse(FilterOperator.EQUAL.IsRangeOperator());
            Assert.IsFalse(FilterOperator.NOT_EQUAL.IsRangeOperator());
        }

        [Test, RunInApplicationDomain]
        public void TestIsInvertedRange()
        {
            Assert.IsFalse(FilterOperator.RANGE_OPEN.IsInvertedRangeOperator());
            Assert.IsFalse(FilterOperator.RANGE_CLOSED.IsInvertedRangeOperator());
            Assert.IsFalse(FilterOperator.RANGE_HALF_OPEN.IsInvertedRangeOperator());
            Assert.IsFalse(FilterOperator.RANGE_HALF_CLOSED.IsInvertedRangeOperator());
            Assert.IsTrue(FilterOperator.NOT_RANGE_HALF_CLOSED.IsInvertedRangeOperator());
            Assert.IsTrue(FilterOperator.NOT_RANGE_OPEN.IsInvertedRangeOperator());
            Assert.IsTrue(FilterOperator.NOT_RANGE_CLOSED.IsInvertedRangeOperator());
            Assert.IsTrue(FilterOperator.NOT_RANGE_HALF_OPEN.IsInvertedRangeOperator());
            Assert.IsFalse(FilterOperator.LESS.IsInvertedRangeOperator());
            Assert.IsFalse(FilterOperator.EQUAL.IsInvertedRangeOperator());
            Assert.IsFalse(FilterOperator.NOT_EQUAL.IsInvertedRangeOperator());
        }
    }
} // end of namespace
