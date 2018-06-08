///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    public class TestExprCountNode : TestExprAggregateNodeAdapter
    {
        private ExprCountNode _wildcardCount;
    
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            ValidatedNodeToTest = MakeNode(5, typeof(int));
    
            _wildcardCount = new ExprCountNode(false);
            _wildcardCount.AddChildNode(new ExprWildcardImpl());
            SupportExprNodeFactory.Validate3Stream(_wildcardCount);
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(long), ValidatedNodeToTest.ReturnType);
            Assert.AreEqual(typeof(long), _wildcardCount.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("count(5)", ValidatedNodeToTest.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("count(*)", _wildcardCount.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(ValidatedNodeToTest.EqualsNode(ValidatedNodeToTest, false));
            Assert.IsFalse(ValidatedNodeToTest.EqualsNode(new ExprSumNode(false), false));
            Assert.IsTrue(_wildcardCount.EqualsNode(_wildcardCount, false));
        }

        [Test]
        public override void TestEvaluate()
        {
            base.TestEvaluate();
        }
    
        private ExprCountNode MakeNode(Object value, Type type)
        {
            ExprCountNode countNode = new ExprCountNode(false);
            countNode.AddChildNode(new SupportExprNode(value, type));
            SupportExprNodeFactory.Validate3Stream(countNode);
            return countNode;
        }
    }
}
