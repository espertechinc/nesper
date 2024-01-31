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

using NUnit.Framework;
using NUnit.Framework.Legacy;

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

        [Test]
        public void TestEquals()
        {
            var otherLikeNodeNot = supportExprNodeFactory.MakeLikeNode(true, "@");
            var otherLikeNodeNot2 = supportExprNodeFactory.MakeLikeNode(true, "!");

            ClassicAssert.IsTrue(likeNodeNot.EqualsNode(otherLikeNodeNot2, false));
            ClassicAssert.IsTrue(otherLikeNodeNot2.EqualsNode(otherLikeNodeNot, false)); // Escape char itself is an expression
            ClassicAssert.IsFalse(likeNodeNormal.EqualsNode(otherLikeNodeNot, false));
        }

        [Test]
        public void TestEvaluate()
        {
            // Build :      s0.string like "%abc__"  (with or witout escape)
            ClassicAssert.IsFalse((bool) likeNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent("abcx"), false, null));
            ClassicAssert.IsTrue((bool) likeNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent("dskfsljkdfabcxx"), false, null));
            ClassicAssert.IsTrue((bool) likeNodeNot.Forge.ExprEvaluator.Evaluate(MakeEvent("abcx"), false, null));
            ClassicAssert.IsFalse((bool) likeNodeNot.Forge.ExprEvaluator.Evaluate(MakeEvent("dskfsljkdfabcxx"), false, null));
        }

        [Test]
        public void TestGetType()
        {
            ClassicAssert.AreEqual(typeof(bool?), likeNodeNormal.Type);
            ClassicAssert.AreEqual(typeof(bool?), likeNodeNot.Type);
            ClassicAssert.AreEqual(typeof(bool?), likeNodeNormalEscaped.Type);
        }

        [Test]
        public void TestToExpressionString()
        {
            ClassicAssert.AreEqual("s0.TheString like \"%abc__\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(likeNodeNormal));
            ClassicAssert.AreEqual("s0.TheString not like \"%abc__\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(likeNodeNot));
            ClassicAssert.AreEqual(
                "s0.TheString like \"%abc__\" escape \"!\"",
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(likeNodeNormalEscaped));
        }

        [Test]
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
