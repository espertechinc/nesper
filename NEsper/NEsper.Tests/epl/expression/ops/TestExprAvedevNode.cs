///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    public class TestExprAvedevNode : TestExprAggregateNodeAdapter
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            ValidatedNodeToTest= MakeNode(5, typeof(int));
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(double),ValidatedNodeToTest.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("avedev(5)", ValidatedNodeToTest.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(ValidatedNodeToTest.EqualsNode(ValidatedNodeToTest, false));
            Assert.IsFalse(ValidatedNodeToTest.EqualsNode(new ExprStddevNode(false), false));
        }
    
        [Test]
        public void TestAggregateFunction()
        {
            AggregationMethodFactory aggFactory = ValidatedNodeToTest.Factory;
            AggregationMethod agg = aggFactory.Make();

            Assert.IsNull(agg.Value);
    
            agg.Enter(82);
            Assert.AreEqual(0D, agg.Value);
    
            agg.Enter(78);
            Assert.AreEqual(2D, agg.Value);
    
            agg.Enter(70);
            double result = agg.Value.AsDouble();
            Assert.AreEqual("4.4444", result.ToString().Substring(0, 6));
    
            agg.Enter(58);
            Assert.AreEqual(8D, agg.Value);
    
            agg.Enter(42);
            Assert.AreEqual(12.8D, agg.Value);
    
            agg.Leave(82);
            Assert.AreEqual(12D, agg.Value);
    
            agg.Leave(58);
            result = agg.Value.AsDouble();
            Assert.AreEqual("14.2222", result.ToString().Substring(0, 7));
        }

        [Test]
        public override void TestEvaluate()
        {
            base.TestEvaluate();
        }

        private ExprAvedevNode MakeNode(Object value, Type type)
        {
            ExprAvedevNode avedevNode = new ExprAvedevNode(false);
            avedevNode.AddChildNode(new SupportExprNode(value, type));
            SupportExprNodeFactory.Validate3Stream(avedevNode);
            return avedevNode;
        }
    }
}
