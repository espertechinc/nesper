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
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprMathNode 
    {
        private ExprMathNode _arithNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _arithNode = new ExprMathNode(MathArithTypeEnum.ADD, false, false);
        }
    
        [Test]
        public void TestGetType()
        {
            _arithNode.AddChildNode(new SupportExprNode(typeof(double?)));
            _arithNode.AddChildNode(new SupportExprNode(typeof(int)));
            _arithNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual(typeof(double?), _arithNode.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            // Build (5*(4-2)), not the same as 5*4-2
            ExprMathNode arithNodeChild = new ExprMathNode(MathArithTypeEnum.SUBTRACT, false, false);
            arithNodeChild.AddChildNode(new SupportExprNode(4));
            arithNodeChild.AddChildNode(new SupportExprNode(2));
    
            _arithNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
            _arithNode.AddChildNode(new SupportExprNode(5));
            _arithNode.AddChildNode(arithNodeChild);
    
            Assert.AreEqual("5*(4-2)", _arithNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestValidate()
        {
            // Must have exactly 2 subnodes
            try
            {
                _arithNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Must have only number-type subnodes
            _arithNode.AddChildNode(new SupportExprNode(typeof(string)));
            _arithNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _arithNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestEvaluate()
        {
            _arithNode.AddChildNode(new SupportExprNode(10));
            _arithNode.AddChildNode(new SupportExprNode(1.5));
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, _arithNode, SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual(11.5d, _arithNode.Evaluate(new EvaluateParams(null, false, null)));
    
            _arithNode = MakeNode(null, typeof(int), 5d, typeof(double?));
            Assert.IsNull(_arithNode.Evaluate(new EvaluateParams(null, false, null)));
    
            _arithNode = MakeNode(5, typeof(int), null, typeof(double?));
            Assert.IsNull(_arithNode.Evaluate(new EvaluateParams(null, false, null)));
    
            _arithNode = MakeNode(null, typeof(int), null, typeof(double?));
            Assert.IsNull(_arithNode.Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_arithNode.EqualsNode(_arithNode, false));
            Assert.IsFalse(_arithNode.EqualsNode(new ExprMathNode(MathArithTypeEnum.DIVIDE, false, false), false));
        }
    
        private ExprMathNode MakeNode(Object valueLeft, Type typeLeft, Object valueRight, Type typeRight)
        {
            ExprMathNode mathNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
            mathNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
            mathNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
            SupportExprNodeUtil.Validate(mathNode);
            return mathNode;
        }
    }
}
