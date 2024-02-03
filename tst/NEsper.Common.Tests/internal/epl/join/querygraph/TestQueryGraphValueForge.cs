///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.supportunit.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [TestFixture]
    public class TestQueryGraphValueForge : AbstractCommonTest
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
            ClassicAssert.AreEqual(ranges.Length, value.Items.Count);

            var count = -1;
            foreach (var desc in value.Items)
            {
                count++;
                var r = (QueryGraphValueEntryRangeForge) desc.Entry;

                ClassicAssert.AreEqual(ranges[count][3], r.Type);
                ClassicAssert.AreEqual(ranges[count][4], ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(desc.IndexExprs[0]));

                if (r is QueryGraphValueEntryRangeRelOpForge)
                {
                    var relOp = (QueryGraphValueEntryRangeRelOpForge) r;
                    ClassicAssert.AreEqual(ranges[count][0], GetProp(relOp.Expression));
                }
                else
                {
                    var rangeIn = (QueryGraphValueEntryRangeInForge) r;
                    ClassicAssert.AreEqual(ranges[count][1], GetProp(rangeIn.ExprStart));
                    ClassicAssert.AreEqual(ranges[count][2], GetProp(rangeIn.ExprEnd));
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
            value.AddRelOp(MakeIdent("B"), QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"), false);
            value.AddRelOp(MakeIdent("B"), QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"), false);
            AssertRanges(new[] { new object[] { "B", null, null, QueryGraphRangeEnum.LESS_OR_EQUAL, "A" } }, value);

            value = new QueryGraphValueForge();
            value.AddRange(QueryGraphRangeEnum.RANGE_CLOSED, MakeIdent("B"), MakeIdent("C"), MakeIdent("A"));
            value.AddRange(QueryGraphRangeEnum.RANGE_CLOSED, MakeIdent("B"), MakeIdent("C"), MakeIdent("A"));
            AssertRanges(new[] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_CLOSED, "A" } }, value);
        }

        [Test]
        public void TestRangeRelOp()
        {
            TryAdd(
                "B",
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                MakeIdent("A"), // read a >= b
                "C",
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                MakeIdent("A"), // read a <= c
                new[] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_CLOSED, "A" } });

            TryAdd(
                "B",
                QueryGraphRangeEnum.GREATER,
                MakeIdent("A"), // read a > b
                "C",
                QueryGraphRangeEnum.LESS,
                MakeIdent("A"), // read a < c
                new[] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_OPEN, "A" } });

            TryAdd(
                "B",
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                MakeIdent("A"), // read a >= b
                "C",
                QueryGraphRangeEnum.LESS,
                MakeIdent("A"), // read a < c
                new[] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_HALF_OPEN, "A" } });

            TryAdd(
                "B",
                QueryGraphRangeEnum.GREATER,
                MakeIdent("A"), // read a > b
                "C",
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                MakeIdent("A"), // read a <= c
                new[] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_HALF_CLOSED, "A" } });

            // sanity
            TryAdd(
                "B",
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                MakeIdent("A"), // read a <= b
                "C",
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                MakeIdent("A"), // read a >= c
                new[] { new object[] { null, "C", "B", QueryGraphRangeEnum.RANGE_CLOSED, "A" } });
        }
    }
} // end of namespace