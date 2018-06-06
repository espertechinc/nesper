///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    public class TestExprAvgNode : TestExprAggregateNodeAdapter
    {
        private ExprAvgNode _avgNodeDistinct;
    
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            ValidatedNodeToTest = MakeNode(5, typeof(int), false);
            _avgNodeDistinct = MakeNode(6, typeof(int), true);
        }
    
        [Test]
        public void TestAggregation()
        {
            var agg = new AggregatorAvg();
            Assert.AreEqual(null, agg.Value);
    
            agg.Enter(5);
            Assert.AreEqual(5d, agg.Value);
    
            agg.Enter(10);
            Assert.AreEqual(7.5d, agg.Value);
    
            agg.Leave(5);
            Assert.AreEqual(10d, agg.Value);
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(double), ValidatedNodeToTest.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("avg(5)", ValidatedNodeToTest.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("avg(distinct 6)", _avgNodeDistinct.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(ValidatedNodeToTest.EqualsNode(ValidatedNodeToTest, false));
            Assert.IsFalse(ValidatedNodeToTest.EqualsNode(new ExprSumNode(false), false));
        }

        [Test]
        public override void TestEvaluate()
        {
            base.TestEvaluate();
        }
    
        private static ExprAvgNode MakeNode(Object value, Type type, bool isDistinct)
        {
            var avgNode = new ExprAvgNode(isDistinct);
            avgNode.AddChildNode(new SupportExprNode(value, type));
            SupportExprNodeFactory.Validate3Stream(avgNode);
            return avgNode;
        }
    }
}
