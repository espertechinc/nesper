///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class TestExprRegexpNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            regexpNodeNormal = supportExprNodeFactory.MakeRegexpNode(false);
            regexpNodeNot = supportExprNodeFactory.MakeRegexpNode(true);
        }

        private ExprRegexpNode regexpNodeNormal;
        private ExprRegexpNode regexpNodeNot;

        private EventBean[] MakeEvent(string stringValue)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            return new[] { SupportEventBeanFactory.CreateObject(supportEventTypeFactory, theEvent) };
        }

        private void TryInvalidValidate(ExprRegexpNode exprLikeRegexpNode)
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
            var otherRegexpNodeNot = supportExprNodeFactory.MakeRegexpNode(true);

            ClassicAssert.IsTrue(regexpNodeNot.EqualsNode(otherRegexpNodeNot, false));
            ClassicAssert.IsFalse(regexpNodeNormal.EqualsNode(otherRegexpNodeNot, false));
        }

        [Test]
        public void TestEvaluate()
        {
            ClassicAssert.IsFalse((bool) regexpNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent("bcd"), false, null));
            ClassicAssert.IsTrue((bool) regexpNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent("ab"), false, null));
            ClassicAssert.IsTrue((bool) regexpNodeNot.Forge.ExprEvaluator.Evaluate(MakeEvent("bcd"), false, null));
            ClassicAssert.IsFalse((bool) regexpNodeNot.Forge.ExprEvaluator.Evaluate(MakeEvent("ab"), false, null));
        }

        [Test]
        public void TestGetType()
        {
            ClassicAssert.AreEqual(typeof(bool?), regexpNodeNormal.Type);
            ClassicAssert.AreEqual(typeof(bool?), regexpNodeNot.Type);
        }

        [Test]
        public void TestToExpressionString()
        {
            ClassicAssert.AreEqual("s0.TheString regexp \"[a-z][a-z]\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(regexpNodeNormal));
            ClassicAssert.AreEqual("s0.TheString not regexp \"[a-z][a-z]\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(regexpNodeNot));
        }

        [Test]
        public void TestValidate()
        {
            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprRegexpNode(true));

            // singe child node not possible, must be 2 at least
            regexpNodeNormal = new ExprRegexpNode(false);
            regexpNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(regexpNodeNormal);

            // test a type mismatch
            regexpNodeNormal = new ExprRegexpNode(true);
            regexpNodeNormal.AddChildNode(new SupportExprNode("sx"));
            regexpNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(regexpNodeNormal);

            // test numeric supported
            regexpNodeNormal = new ExprRegexpNode(false);
            regexpNodeNormal.AddChildNode(new SupportExprNode(4));
            regexpNodeNormal.AddChildNode(new SupportExprNode("sx"));
        }
    }
} // end of namespace
