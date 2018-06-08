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
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprLikeNode 
    {
        private ExprLikeNode _likeNodeNormal;
        private ExprLikeNode _likeNodeNot;
        private ExprLikeNode _likeNodeNormalEscaped;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _likeNodeNormal = SupportExprNodeFactory.MakeLikeNode(false, null);
            _likeNodeNot = SupportExprNodeFactory.MakeLikeNode(true, null);
            _likeNodeNormalEscaped = SupportExprNodeFactory.MakeLikeNode(false, "!");
        }
    
        [Test]
        public void TestGetType() 
        {
            Assert.AreEqual(typeof(bool?), _likeNodeNormal.ReturnType);
            Assert.AreEqual(typeof(bool?), _likeNodeNot.ReturnType);
            Assert.AreEqual(typeof(bool?), _likeNodeNormalEscaped.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprLikeNode(true));
    
            // singe child node not possible, must be 2 at least
            _likeNodeNormal = new ExprLikeNode(false);
            _likeNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_likeNodeNormal);
    
            // test a type mismatch
            _likeNodeNormal = new ExprLikeNode(true);
            _likeNodeNormal.AddChildNode(new SupportExprNode("sx"));
            _likeNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_likeNodeNormal);
    
            // test numeric supported
            _likeNodeNormal = new ExprLikeNode(false);
            _likeNodeNormal.AddChildNode(new SupportExprNode(4));
            _likeNodeNormal.AddChildNode(new SupportExprNode("sx"));
    
            // test invalid escape char
            _likeNodeNormal = new ExprLikeNode(false);
            _likeNodeNormal.AddChildNode(new SupportExprNode(4));
            _likeNodeNormal.AddChildNode(new SupportExprNode("sx"));
            _likeNodeNormal.AddChildNode(new SupportExprNode(5));
        }
    
        [Test]
        public void TestEvaluate()
        {
            // Build :      s0.TheString like "%abc__"  (with or witout escape)
            Assert.IsFalse(_likeNodeNormal.Evaluate(new EvaluateParams(MakeEvent("abcx"), false, null)).AsBoolean());
            Assert.IsTrue(_likeNodeNormal.Evaluate(new EvaluateParams(MakeEvent("dskfsljkdfabcxx"), false, null)).AsBoolean());
            Assert.IsTrue(_likeNodeNot.Evaluate(new EvaluateParams(MakeEvent("abcx"), false, null)).AsBoolean());
            Assert.IsFalse(_likeNodeNot.Evaluate(new EvaluateParams(MakeEvent("dskfsljkdfabcxx"), false, null)).AsBoolean());
        }
    
        [Test]
        public void TestEquals()
        {
            ExprLikeNode otherLikeNodeNot = SupportExprNodeFactory.MakeLikeNode(true, "@");
            ExprLikeNode otherLikeNodeNot2 = SupportExprNodeFactory.MakeLikeNode(true, "!");
    
            Assert.IsTrue(_likeNodeNot.EqualsNode(otherLikeNodeNot2, false));
            Assert.IsTrue(otherLikeNodeNot2.EqualsNode(otherLikeNodeNot, false)); // Escape char itself is an expression
            Assert.IsFalse(_likeNodeNormal.EqualsNode(otherLikeNodeNot, false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("s0.TheString like \"%abc__\"", _likeNodeNormal.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("s0.TheString not like \"%abc__\"", _likeNodeNot.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("s0.TheString like \"%abc__\" escape \"!\"", _likeNodeNormalEscaped.ToExpressionStringMinPrecedenceSafe());
        }
    
        private EventBean[] MakeEvent(String stringValue)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            return new EventBean[] {SupportEventBeanFactory.CreateObject(theEvent)};
        }
    
        private void TryInvalidValidate(ExprLikeNode exprLikeRegexpNode)
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
