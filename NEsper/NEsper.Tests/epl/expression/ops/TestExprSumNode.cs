///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    public class TestExprSumNode : TestExprAggregateNodeAdapter
    {
        private ExprSumNode _sumNode;
        private IContainer _container;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _container = SupportContainer.Reset();
            _sumNode = new ExprSumNode(false);
    
            ValidatedNodeToTest= MakeNode(5, typeof(int));
        }
    
        [Test]
        public void TestGetType()
        {
            _sumNode.AddChildNode(new SupportExprNode(typeof(int)));
            SupportExprNodeFactory.Validate3Stream(_sumNode);
            Assert.AreEqual(typeof(int), _sumNode.ReturnType);
    
            _sumNode = new ExprSumNode(false);
            _sumNode.AddChildNode(new SupportExprNode(typeof(float?)));
            SupportExprNodeFactory.Validate3Stream(_sumNode);
            Assert.AreEqual(typeof(float), _sumNode.ReturnType);
    
            _sumNode = new ExprSumNode(false);
            _sumNode.AddChildNode(new SupportExprNode(typeof(short)));
            SupportExprNodeFactory.Validate3Stream(_sumNode);
            Assert.AreEqual(typeof(int), _sumNode.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            // Build sum(4-2)
            var arithNodeChild = new ExprMathNode(MathArithTypeEnum.SUBTRACT, false, false);
            arithNodeChild.AddChildNode(new SupportExprNode(4));
            arithNodeChild.AddChildNode(new SupportExprNode(2));
    
            _sumNode = new ExprSumNode(false);
            _sumNode.AddChildNode(arithNodeChild);
    
            Assert.AreEqual("sum(4-2)", _sumNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestValidate()
        {
            // Must have exactly 1 subnodes
            try
            {
                _sumNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Must have only number-type subnodes
            _sumNode.AddChildNode(new SupportExprNode(typeof(string)));
            _sumNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _sumNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_sumNode.EqualsNode(_sumNode, false));
            Assert.IsFalse(_sumNode.EqualsNode(new ExprOrNode(), false));
        }

        [Test]
        public override void TestEvaluate()
        {
            base.TestEvaluate();
        }
        
        private ExprSumNode MakeNode(Object value, Type type)
        {
            var sumNode = new ExprSumNode(false);
            sumNode.AddChildNode(new SupportExprNode(value, type));
            SupportExprNodeFactory.Validate3Stream(sumNode);
            return sumNode;
        }
    }
}
