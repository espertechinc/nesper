///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprLikeNode : AbstractCommonTest
    {
        [SetUp]
        public void SetupTest()
        {
            likeNodeNormal = supportExprNodeFactory.MakeLikeNode(false, null);
            likeNodeNot = supportExprNodeFactory.MakeLikeNode(true, null);
            likeNodeNormalEscaped = supportExprNodeFactory.MakeLikeNode(false, "!");
        }

        private ExprLikeNode likeNodeNormal;
        private ExprLikeNode likeNodeNot;
        private ExprLikeNode likeNodeNormalEscaped;

        private EventBean[] MakeEvent(string stringValue)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            return new[] { SupportEventBeanFactory.CreateObject(supportEventTypeFactory, theEvent) };
        }

        private void TryInvalidValidate(ExprLikeNode exprLikeRegexpNode)
        {
            try
            {
                exprLikeRegexpNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEquals()
        {
            var otherLikeNodeNot = supportExprNodeFactory.MakeLikeNode(true, "@");
            var otherLikeNodeNot2 = supportExprNodeFactory.MakeLikeNode(true, "!");

            Assert.IsTrue(likeNodeNot.EqualsNode(otherLikeNodeNot2, false));
            Assert.IsTrue(otherLikeNodeNot2.EqualsNode(otherLikeNodeNot, false)); // Escape char itself is an expression
            Assert.IsFalse(likeNodeNormal.EqualsNode(otherLikeNodeNot, false));
        }

        [Test, RunInApplicationDomain]
        public void TestEvaluate()
        {
            // Build :      s0.string like "%abc__"  (with or witout escape)
            Assert.IsFalse((bool) likeNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent("abcx"), false, null));
            Assert.IsTrue((bool) likeNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent("dskfsljkdfabcxx"), false, null));
            Assert.IsTrue((bool) likeNodeNot.Forge.ExprEvaluator.Evaluate(MakeEvent("abcx"), false, null));
            Assert.IsFalse((bool) likeNodeNot.Forge.ExprEvaluator.Evaluate(MakeEvent("dskfsljkdfabcxx"), false, null));
        }

        [Test, RunInApplicationDomain]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), likeNodeNormal.Type);
            Assert.AreEqual(typeof(bool?), likeNodeNot.Type);
            Assert.AreEqual(typeof(bool?), likeNodeNormalEscaped.Type);
        }

        [Test, RunInApplicationDomain]
        public void TestToExpressionString()
        {
            Assert.AreEqual("s0.TheString like \"%abc__\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(likeNodeNormal));
            Assert.AreEqual("s0.TheString not like \"%abc__\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(likeNodeNot));
            Assert.AreEqual(
                "s0.TheString like \"%abc__\" escape \"!\"",
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(likeNodeNormalEscaped));
        }

        [Test, RunInApplicationDomain]
        public void TestValidate()
        {
            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprLikeNode(true));

            // singe child node not possible, must be 2 at least
            likeNodeNormal = new ExprLikeNode(false);
            likeNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(likeNodeNormal);

            // test a type mismatch
            likeNodeNormal = new ExprLikeNode(true);
            likeNodeNormal.AddChildNode(new SupportExprNode("sx"));
            likeNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(likeNodeNormal);

            // test numeric supported
            likeNodeNormal = new ExprLikeNode(false);
            likeNodeNormal.AddChildNode(new SupportExprNode(4));
            likeNodeNormal.AddChildNode(new SupportExprNode("sx"));

            // test invalid escape char
            likeNodeNormal = new ExprLikeNode(false);
            likeNodeNormal.AddChildNode(new SupportExprNode(4));
            likeNodeNormal.AddChildNode(new SupportExprNode("sx"));
            likeNodeNormal.AddChildNode(new SupportExprNode(5));
        }
    }
} // end of namespace
