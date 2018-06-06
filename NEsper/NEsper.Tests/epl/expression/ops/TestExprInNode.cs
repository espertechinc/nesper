///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprInNode 
    {
        private ExprInNode _inNodeNormal;
        private ExprInNode _inNodeNotIn;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _inNodeNormal = SupportExprNodeFactory.MakeInSetNode(false);
            _inNodeNotIn = SupportExprNodeFactory.MakeInSetNode(true);
        }
    
        [Test]
        public void TestGetType() 
        {
            Assert.AreEqual(typeof(bool?), _inNodeNormal.ExprEvaluator.ReturnType);
            Assert.AreEqual(typeof(bool?), _inNodeNotIn.ExprEvaluator.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            _inNodeNormal = SupportExprNodeFactory.MakeInSetNode(true);
            _inNodeNormal.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprInNodeImpl(true));
    
            // singe child node not possible, must be 2 at least
            _inNodeNormal = new ExprInNodeImpl(true);
            _inNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_inNodeNormal);
    
            // test a type mismatch
            _inNodeNormal = new ExprInNodeImpl(true);
            _inNodeNormal.AddChildNode(new SupportExprNode("sx"));
            _inNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_inNodeNormal);
        }
    
        [Test]
        public void TestEvaluate()
        {
            Assert.IsFalse((bool) _inNodeNormal.Evaluate(new EvaluateParams(MakeEvent(0), false, null)));
            Assert.IsTrue((bool) _inNodeNormal.Evaluate(new EvaluateParams(MakeEvent(1), false, null)));
            Assert.IsTrue((bool) _inNodeNormal.Evaluate(new EvaluateParams(MakeEvent(2), false, null)));
            Assert.IsFalse((bool) _inNodeNormal.Evaluate(new EvaluateParams(MakeEvent(3), false, null)));
    
            Assert.IsTrue((bool) _inNodeNotIn.Evaluate(new EvaluateParams(MakeEvent(0), false, null)));
            Assert.IsFalse((bool) _inNodeNotIn.Evaluate(new EvaluateParams(MakeEvent(1), false, null)));
            Assert.IsFalse((bool) _inNodeNotIn.Evaluate(new EvaluateParams(MakeEvent(2), false, null)));
            Assert.IsTrue((bool) _inNodeNotIn.Evaluate(new EvaluateParams(MakeEvent(3), false, null)));
        }
    
        [Test]
        public void TestEquals() 
        {
            ExprInNode otherInNodeNormal = SupportExprNodeFactory.MakeInSetNode(false);
            ExprInNode otherInNodeNotIn = SupportExprNodeFactory.MakeInSetNode(true);
    
            Assert.IsTrue(_inNodeNormal.EqualsNode(otherInNodeNormal, false));
            Assert.IsTrue(_inNodeNotIn.EqualsNode(otherInNodeNotIn, false));
    
            Assert.IsFalse(_inNodeNormal.EqualsNode(otherInNodeNotIn, false));
            Assert.IsFalse(_inNodeNotIn.EqualsNode(otherInNodeNormal, false));
            Assert.IsFalse(_inNodeNotIn.EqualsNode(SupportExprNodeFactory.MakeCaseSyntax1Node(), false));
            Assert.IsFalse(_inNodeNormal.EqualsNode(SupportExprNodeFactory.MakeCaseSyntax1Node(), false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("s0.IntPrimitive in (1,2)", _inNodeNormal.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("s0.IntPrimitive not in (1,2)", _inNodeNotIn.ToExpressionStringMinPrecedenceSafe());
        }
    
        private static EventBean[] MakeEvent(int intPrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return new EventBean[] {SupportEventBeanFactory.CreateObject(theEvent)};
        }
    
        private void TryInvalidValidate(ExprInNode exprInNode)
        {
            try {
                exprInNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    }
}
