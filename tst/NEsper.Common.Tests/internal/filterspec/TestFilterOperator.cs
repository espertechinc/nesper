///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.filterspec
{
    [TestFixture]
    public class TestFilterOperator : AbstractCommonTest
    {
        [Test]
        public void TestFromText()
        {
            ClassicAssert.AreEqual(FilterOperator.EQUAL, FilterOperatorExtensions.FromText("="));
            foreach (var op in EnumHelper.GetValues<FilterOperator>()) {
                ClassicAssert.AreEqual(op, FilterOperatorExtensions.FromText(op.GetTextualOp()));
            }
        }

        [Test]
        public void TestRanges()
        {
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, false, false) == FilterOperator.RANGE_OPEN);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, true, false) == FilterOperator.RANGE_CLOSED);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, false, false) == FilterOperator.RANGE_HALF_OPEN);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, true, false) == FilterOperator.RANGE_HALF_CLOSED);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, false, true) == FilterOperator.NOT_RANGE_OPEN);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, true, true) == FilterOperator.NOT_RANGE_CLOSED);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(true, false, true) == FilterOperator.NOT_RANGE_HALF_OPEN);
            ClassicAssert.IsTrue(FilterOperatorExtensions.ParseRangeOperator(false, true, true) == FilterOperator.NOT_RANGE_HALF_CLOSED);
        }

        [Test]
        public void TestIsComparison()
        {
            ClassicAssert.IsTrue(FilterOperator.GREATER.IsComparisonOperator());
            ClassicAssert.IsTrue(FilterOperator.GREATER_OR_EQUAL.IsComparisonOperator());
            ClassicAssert.IsTrue(FilterOperator.LESS.IsComparisonOperator());
            ClassicAssert.IsTrue(FilterOperator.LESS_OR_EQUAL.IsComparisonOperator());
            ClassicAssert.IsFalse(FilterOperator.RANGE_CLOSED.IsComparisonOperator());
            ClassicAssert.IsFalse(FilterOperator.EQUAL.IsComparisonOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_EQUAL.IsComparisonOperator());
        }

        [Test]
        public void TestIsRange()
        {
            ClassicAssert.IsTrue(FilterOperator.RANGE_OPEN.IsRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.RANGE_CLOSED.IsRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.RANGE_HALF_OPEN.IsRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.RANGE_HALF_CLOSED.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_RANGE_HALF_CLOSED.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_RANGE_OPEN.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_RANGE_CLOSED.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_RANGE_HALF_OPEN.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.LESS.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.EQUAL.IsRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_EQUAL.IsRangeOperator());
        }

        [Test]
        public void TestIsInvertedRange()
        {
            ClassicAssert.IsFalse(FilterOperator.RANGE_OPEN.IsInvertedRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.RANGE_CLOSED.IsInvertedRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.RANGE_HALF_OPEN.IsInvertedRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.RANGE_HALF_CLOSED.IsInvertedRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.NOT_RANGE_HALF_CLOSED.IsInvertedRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.NOT_RANGE_OPEN.IsInvertedRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.NOT_RANGE_CLOSED.IsInvertedRangeOperator());
            ClassicAssert.IsTrue(FilterOperator.NOT_RANGE_HALF_OPEN.IsInvertedRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.LESS.IsInvertedRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.EQUAL.IsInvertedRangeOperator());
            ClassicAssert.IsFalse(FilterOperator.NOT_EQUAL.IsInvertedRangeOperator());
        }
    }
} // end of namespace
