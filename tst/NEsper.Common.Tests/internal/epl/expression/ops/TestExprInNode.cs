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
    public class TestExprInNode : AbstractCommonTest
    {
        [SetUp]
        public void SetupTest()
        {
            inNodeNormal = supportExprNodeFactory.MakeInSetNode(false);
            inNodeNotIn = supportExprNodeFactory.MakeInSetNode(true);
        }

        private ExprInNode inNodeNormal;
        private ExprInNode inNodeNotIn;

        private EventBean[] MakeEvent(int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return new[] { SupportEventBeanFactory.CreateObject(supportEventTypeFactory, theEvent) };
        }

        private void TryInvalidValidate(ExprInNode exprInNode)
        {
            try
            {
                exprInNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
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
            var otherInNodeNormal = supportExprNodeFactory.MakeInSetNode(false);
            var otherInNodeNotIn = supportExprNodeFactory.MakeInSetNode(true);

            ClassicAssert.IsTrue(inNodeNormal.EqualsNode(otherInNodeNormal, false));
            ClassicAssert.IsTrue(inNodeNotIn.EqualsNode(otherInNodeNotIn, false));

            ClassicAssert.IsFalse(inNodeNormal.EqualsNode(otherInNodeNotIn, false));
            ClassicAssert.IsFalse(inNodeNotIn.EqualsNode(otherInNodeNormal, false));
            ClassicAssert.IsFalse(inNodeNotIn.EqualsNode(supportExprNodeFactory.MakeCaseSyntax1Node(), false));
            ClassicAssert.IsFalse(inNodeNormal.EqualsNode(supportExprNodeFactory.MakeCaseSyntax1Node(), false));
        }

        [Test]
        public void TestEvaluate()
        {
            ClassicAssert.IsFalse((bool) inNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent(0), false, null));
            ClassicAssert.IsTrue((bool) inNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent(1), false, null));
            ClassicAssert.IsTrue((bool) inNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent(2), false, null));
            ClassicAssert.IsFalse((bool) inNodeNormal.Forge.ExprEvaluator.Evaluate(MakeEvent(3), false, null));

            ClassicAssert.IsTrue((bool) inNodeNotIn.Forge.ExprEvaluator.Evaluate(MakeEvent(0), false, null));
            ClassicAssert.IsFalse((bool) inNodeNotIn.Forge.ExprEvaluator.Evaluate(MakeEvent(1), false, null));
            ClassicAssert.IsFalse((bool) inNodeNotIn.Forge.ExprEvaluator.Evaluate(MakeEvent(2), false, null));
            ClassicAssert.IsTrue((bool) inNodeNotIn.Forge.ExprEvaluator.Evaluate(MakeEvent(3), false, null));
        }

        [Test]
        public void TestGetType()
        {
            ClassicAssert.AreEqual(typeof(bool?), inNodeNormal.Forge.EvaluationType);
            ClassicAssert.AreEqual(typeof(bool?), inNodeNotIn.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            ClassicAssert.AreEqual("s0.IntPrimitive in (1,2)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(inNodeNormal));
            ClassicAssert.AreEqual("s0.IntPrimitive not in (1,2)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(inNodeNotIn));
        }

        [Test]
        public void TestValidate()
        {
            inNodeNormal = supportExprNodeFactory.MakeInSetNode(true);
            inNodeNormal.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprInNodeImpl(true));

            // singe child node not possible, must be 2 at least
            inNodeNormal = new ExprInNodeImpl(true);
            inNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(inNodeNormal);

            // test a type mismatch
            inNodeNormal = new ExprInNodeImpl(true);
            inNodeNormal.AddChildNode(new SupportExprNode("sx"));
            inNodeNormal.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(inNodeNormal);
        }
    }
} // end of namespace
