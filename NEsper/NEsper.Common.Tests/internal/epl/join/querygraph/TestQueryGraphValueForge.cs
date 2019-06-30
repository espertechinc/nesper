///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [TestFixture]
    public class TestQueryGraphValueForge : CommonTest
    {
        private ExprIdentNode MakeIdent(string prop)
        {
            return new ExprIdentNodeImpl(
                supportEventTypeFactory.CreateBeanType(typeof(SupportABCDEEvent)), prop, 0);
        }

        private void TryAdd(
            string propertyKeyOne,
            QueryGraphRangeEnum opOne,
            ExprIdentNode valueOne,
            string propertyKeyTwo,
            QueryGraphRangeEnum opTwo,
            ExprIdentNode valueTwo,
            object[][] expected)
        {
            var value = new QueryGraphValueForge();
            value.AddRelOp(MakeIdent(propertyKeyOne), opOne, valueOne, true);
            value.AddRelOp(MakeIdent(propertyKeyTwo), opTwo, valueTwo, true);
            AssertRanges(expected, value);

            value = new QueryGraphValueForge();
            value.AddRelOp(MakeIdent(propertyKeyTwo), opTwo, valueTwo, true);
            value.AddRelOp(MakeIdent(propertyKeyOne), opOne, valueOne, true);
            AssertRanges(expected, value);
        }

        private void AssertRanges(
            object[][] ranges,
            QueryGraphValueForge value)
        {
            Assert.AreEqual(ranges.Length, value.Items.Count);

            var count = -1;
            foreach (var desc in value.Items)
            {
                count++;
                var r = (QueryGraphValueEntryRangeForge) desc.Entry;

                Assert.AreEqual(ranges[count][3], r.Type);
                Assert.AreEqual(ranges[count][4], ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(desc.IndexExprs[0]));

                if (r is QueryGraphValueEntryRangeRelOpForge)
                {
                    var relOp = (QueryGraphValueEntryRangeRelOpForge) r;
                    Assert.AreEqual(ranges[count][0], GetProp(relOp.Expression));
                }
                else
                {
                    var rangeIn = (QueryGraphValueEntryRangeInForge) r;
                    Assert.AreEqual(ranges[count][1], GetProp(rangeIn.ExprStart));
                    Assert.AreEqual(ranges[count][2], GetProp(rangeIn.ExprEnd));
                }
            }
        }

        private string GetProp(ExprNode node)
        {
            return ((ExprIdentNode) node).UnresolvedPropertyName;
        }

        [Test]
        public void TestNoDup()
        {
            var value = new QueryGraphValueForge();
            value.AddRelOp(MakeIdent("b"), QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("a"), false);
            value.AddRelOp(MakeIdent("b"), QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("a"), false);
            AssertRanges(new[] { new object[] { "b", null, null, QueryGraphRangeEnum.LESS_OR_EQUAL, "a" } }, value);

            value = new QueryGraphValueForge();
            value.AddRange(QueryGraphRangeEnum.RANGE_CLOSED, MakeIdent("b"), MakeIdent("c"), MakeIdent("a"));
            value.AddRange(QueryGraphRangeEnum.RANGE_CLOSED, MakeIdent("b"), MakeIdent("c"), MakeIdent("a"));
            AssertRanges(new[] { new object[] { null, "b", "c", QueryGraphRangeEnum.RANGE_CLOSED, "a" } }, value);
        }

        [Test]
        public void TestRangeRelOp()
        {
            TryAdd(
                "b",
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                MakeIdent("a"), // read a >= b
                "c",
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                MakeIdent("a"), // read a <= c
                new[] { new object[] { null, "b", "c", QueryGraphRangeEnum.RANGE_CLOSED, "a" } });

            TryAdd(
                "b",
                QueryGraphRangeEnum.GREATER,
                MakeIdent("a"), // read a > b
                "c",
                QueryGraphRangeEnum.LESS,
                MakeIdent("a"), // read a < c
                new[] { new object[] { null, "b", "c", QueryGraphRangeEnum.RANGE_OPEN, "a" } });

            TryAdd(
                "b",
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                MakeIdent("a"), // read a >= b
                "c",
                QueryGraphRangeEnum.LESS,
                MakeIdent("a"), // read a < c
                new[] { new object[] { null, "b", "c", QueryGraphRangeEnum.RANGE_HALF_OPEN, "a" } });

            TryAdd(
                "b",
                QueryGraphRangeEnum.GREATER,
                MakeIdent("a"), // read a > b
                "c",
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                MakeIdent("a"), // read a <= c
                new[] { new object[] { null, "b", "c", QueryGraphRangeEnum.RANGE_HALF_CLOSED, "a" } });

            // sanity
            TryAdd(
                "b",
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                MakeIdent("a"), // read a <= b
                "c",
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                MakeIdent("a"), // read a >= c
                new[] { new object[] { null, "c", "b", QueryGraphRangeEnum.RANGE_CLOSED, "a" } });
        }
    }
} // end of namespace