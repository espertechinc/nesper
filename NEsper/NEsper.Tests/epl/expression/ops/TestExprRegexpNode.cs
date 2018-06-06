///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
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
    public class TestExprRegexpNode 
    {
        private ExprRegexpNode _regexpNodeNormal;
        private ExprRegexpNode _regexpNodeNot;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _regexpNodeNormal = SupportExprNodeFactory.MakeRegexpNode(false);
            _regexpNodeNot = SupportExprNodeFactory.MakeRegexpNode(true);
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(Boolean?), _regexpNodeNormal.ReturnType);
            Assert.AreEqual(typeof(Boolean?), _regexpNodeNot.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprRegexpNode(true));
    
            // singe child node not possible, must be 2 at least
            _regexpNodeNormal = new ExprRegexpNode(false);
            _regexpNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_regexpNodeNormal);
    
            // test a type mismatch
            _regexpNodeNormal = new ExprRegexpNode(true);
            _regexpNodeNormal.AddChildNode(new SupportExprNode("sx"));
            _regexpNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_regexpNodeNormal);
    
            // test numeric supported
            _regexpNodeNormal = new ExprRegexpNode(false);
            _regexpNodeNormal.AddChildNode(new SupportExprNode(4));
            _regexpNodeNormal.AddChildNode(new SupportExprNode("sx"));
        }
    
        [Test]
        public void TestEvaluate()
        {
            Assert.IsFalse(_regexpNodeNormal.Evaluate(new EvaluateParams(MakeEvent("bcd"), false, null)).AsBoolean());
            Assert.IsTrue(_regexpNodeNormal.Evaluate(new EvaluateParams(MakeEvent("ab"), false, null)).AsBoolean());
            Assert.IsTrue(_regexpNodeNot.Evaluate(new EvaluateParams(MakeEvent("bcd"), false, null)).AsBoolean());
            Assert.IsFalse(_regexpNodeNot.Evaluate(new EvaluateParams(MakeEvent("ab"), false, null)).AsBoolean());
        }
    
        [Test]
        public void TestEquals()
        {
            ExprRegexpNode otherRegexpNodeNot = SupportExprNodeFactory.MakeRegexpNode(true);
    
            Assert.IsTrue(_regexpNodeNot.EqualsNode(otherRegexpNodeNot, false));
            Assert.IsFalse(_regexpNodeNormal.EqualsNode(otherRegexpNodeNot, false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("s0.TheString regexp \"[a-z][a-z]\"", _regexpNodeNormal.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("s0.TheString not regexp \"[a-z][a-z]\"", _regexpNodeNot.ToExpressionStringMinPrecedenceSafe());
        }
    
        private static EventBean[] MakeEvent(String stringValue)
        {
            var theEvent = new SupportBean {TheString = stringValue};
            return new[] {SupportEventBeanFactory.CreateObject(theEvent)};
        }
    
        private void TryInvalidValidate(ExprRegexpNode exprLikeRegexpNode)
        {
            try {
                exprLikeRegexpNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    }
}
