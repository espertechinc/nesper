///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestQueryGraphValue
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestRangeRelOp()
        {
            TryAdd("B", QueryGraphRangeEnum.GREATER_OR_EQUAL, MakeIdent("A"),      // read a >= b
                   "C", QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"),         // read a <= c
                    new object[][] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_CLOSED, "A" } });
    
            TryAdd("B", QueryGraphRangeEnum.GREATER, MakeIdent("A"),      // read a > b
                   "C", QueryGraphRangeEnum.LESS, MakeIdent("A"),         // read a < c
                    new object[][] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_OPEN, "A" } });
    
            TryAdd("B", QueryGraphRangeEnum.GREATER_OR_EQUAL, MakeIdent("A"),      // read a >= b
                   "C", QueryGraphRangeEnum.LESS, MakeIdent("A"),                  // read a < c
                    new object[][] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_HALF_OPEN, "A" } });
    
            TryAdd("B", QueryGraphRangeEnum.GREATER, MakeIdent("A"),                       // read a > b
                   "C", QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"),                  // read a <= c
                    new object[][] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_HALF_CLOSED, "A" } });
    
            // sanity
            TryAdd("B", QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"),                     // read a <= b
                   "C", QueryGraphRangeEnum.GREATER_OR_EQUAL, MakeIdent("A"),                  // read a >= c
                    new object[][] { new object[] { null, "C", "B", QueryGraphRangeEnum.RANGE_CLOSED, "A" } });
        }
    
        private ExprIdentNode MakeIdent(string prop)
        {
            return new ExprIdentNodeImpl(
                new BeanEventType(
                    _container, null, 0, typeof(MyEvent),
                    _container.Resolve<EventAdapterService>(), null), prop, 1);
        }
    
        private void TryAdd(string propertyKeyOne, QueryGraphRangeEnum opOne, ExprIdentNode valueOne,
                            string propertyKeyTwo, QueryGraphRangeEnum opTwo, ExprIdentNode valueTwo,
                            object[][] expected) {
    
            var value = new QueryGraphValue();
            value.AddRelOp(MakeIdent(propertyKeyOne), opOne, valueOne, true);
            value.AddRelOp(MakeIdent(propertyKeyTwo), opTwo, valueTwo, true);
            AssertRanges(expected, value);
    
            value = new QueryGraphValue();
            value.AddRelOp(MakeIdent(propertyKeyTwo), opTwo, valueTwo, true);
            value.AddRelOp(MakeIdent(propertyKeyOne), opOne, valueOne, true);
            AssertRanges(expected, value);
        }
    
        [Test]
        public void TestNoDup()
        {
            var value = new QueryGraphValue();
            value.AddRelOp(MakeIdent("B"), QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"), false);
            value.AddRelOp(MakeIdent("B"), QueryGraphRangeEnum.LESS_OR_EQUAL, MakeIdent("A"), false);
            AssertRanges(new object[][] { new object[] { "B", null, null, QueryGraphRangeEnum.LESS_OR_EQUAL, "A" } }, value);
    
            value = new QueryGraphValue();
            value.AddRange(QueryGraphRangeEnum.RANGE_CLOSED, MakeIdent("B"), MakeIdent("C"), MakeIdent("A"));
            value.AddRange(QueryGraphRangeEnum.RANGE_CLOSED, MakeIdent("B"), MakeIdent("C"), MakeIdent("A"));
            AssertRanges(new object[][] { new object[] { null, "B", "C", QueryGraphRangeEnum.RANGE_CLOSED, "A" } }, value);
        }
    
        private void AssertRanges(object[][] ranges, QueryGraphValue value)
        {
            Assert.AreEqual(ranges.Length, value.Items.Count);
    
            var count = -1;
            foreach (var desc in value.Items) {
                count++;
                var r = (QueryGraphValueEntryRange) desc.Entry;
    
                Assert.AreEqual(ranges[count][3], r.RangeType);
                Assert.AreEqual(ranges[count][4], desc.IndexExprs[0].ToExpressionStringMinPrecedenceSafe());
    
                if (r is QueryGraphValueEntryRangeRelOp) {
                    var relOp = (QueryGraphValueEntryRangeRelOp) r;
                    Assert.AreEqual(ranges[count][0], GetProp(relOp.Expression));
                }
                else {
                    var rangeIn = (QueryGraphValueEntryRangeIn) r;
                    Assert.AreEqual(ranges[count][1], GetProp(rangeIn.ExprStart));
                    Assert.AreEqual(ranges[count][2], GetProp(rangeIn.ExprEnd));
                }
            }
        }
    
        private string GetProp(ExprNode node)
        {
            return ((ExprIdentNode) node).UnresolvedPropertyName;
        }
    
        public class MyEvent
        {
            public int A { get; private set; }

            public int B { get; private set; }

            public int C { get; private set; }

            public int D { get; private set; }

            public int E { get; private set; }
        }
    }
}
