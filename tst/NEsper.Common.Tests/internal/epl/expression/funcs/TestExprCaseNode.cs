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

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    [TestFixture]
    public class TestExprCaseNode : AbstractCommonTest
    {
        private void TryInvalidValidate(ExprCaseNode exprCaseNode)
        {
            try {
                exprCaseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException) {
                // expected
            }
        }

        private EventBean[] MakeEvent(int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return new[] {SupportEventBeanFactory.CreateObject(supportEventTypeFactory, theEvent) };
        }

        [Test]
        public void TestEquals()
        {
            var caseNode = supportExprNodeFactory.MakeCaseSyntax1Node();
            var otherCaseNode = supportExprNodeFactory.MakeCaseSyntax1Node();
            var caseNodeSyntax2 = supportExprNodeFactory.MakeCaseSyntax2Node();
            var otherCaseNodeSyntax2 = supportExprNodeFactory.MakeCaseSyntax2Node();

            ClassicAssert.IsTrue(caseNode.EqualsNode(otherCaseNode, false));
            ClassicAssert.IsTrue(otherCaseNode.EqualsNode(caseNode, false));
            ClassicAssert.IsFalse(caseNode.EqualsNode(caseNodeSyntax2, false));
            ClassicAssert.IsFalse(caseNodeSyntax2.EqualsNode(caseNode, false));
            ClassicAssert.IsTrue(caseNodeSyntax2.EqualsNode(otherCaseNodeSyntax2, false));
        }

        [Test]
        public void TestEvaluate()
        {
            var caseNode = supportExprNodeFactory.MakeCaseSyntax1Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            ClassicAssert.AreEqual("a", caseNode.Forge.ExprEvaluator.Evaluate(MakeEvent(1), false, null));
            ClassicAssert.AreEqual("b", caseNode.Forge.ExprEvaluator.Evaluate(MakeEvent(2), false, null));
            ClassicAssert.AreEqual("c", caseNode.Forge.ExprEvaluator.Evaluate(MakeEvent(3), false, null));

            caseNode = supportExprNodeFactory.MakeCaseSyntax2Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            ClassicAssert.AreEqual("a", caseNode.Forge.ExprEvaluator.Evaluate(MakeEvent(1), false, null));
            ClassicAssert.AreEqual("b", caseNode.Forge.ExprEvaluator.Evaluate(MakeEvent(2), false, null));
            ClassicAssert.AreEqual("c", caseNode.Forge.ExprEvaluator.Evaluate(MakeEvent(3), false, null));
        }

        [Test]
        public void TestGetType()
        {
            // Template expression is:
            // case when (so.FloatPrimitive>s1.ShortBoxed) then count(5) when (so.LongPrimitive>s1.IntPrimitive) then (25 + 130.5) else (3*3) end
            var caseNode = supportExprNodeFactory.MakeCaseSyntax1Node();
            ClassicAssert.AreEqual(typeof(string), caseNode.Forge.EvaluationType);

            // case when (2.5>2) then count(5) when (1>3) then (25 + 130.5) else (3*3) end
            // First when node is true, case node type is the first when node type.
            caseNode = supportExprNodeFactory.MakeCaseSyntax2Node();
            ClassicAssert.AreEqual(typeof(string), caseNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            var _caseNode = supportExprNodeFactory.MakeCaseSyntax1Node();
            ClassicAssert.AreEqual(
                "case when s0.IntPrimitive=1 then \"a\" when s0.IntPrimitive=2 then \"b\" else \"c\" end",
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_caseNode));

            _caseNode = supportExprNodeFactory.MakeCaseSyntax2Node();
            ClassicAssert.AreEqual(
                "case s0.IntPrimitive when 1 then \"a\" when 2 then \"b\" else \"c\" end",
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_caseNode));
        }

        [Test]
        public void TestValidate()
        {
            var caseNode = supportExprNodeFactory.MakeCaseSyntax1Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            caseNode = supportExprNodeFactory.MakeCaseSyntax2Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            // No subnodes: Exception is thrown.
            TryInvalidValidate(new ExprCaseNode(false));
            TryInvalidValidate(new ExprCaseNode(true));

            // singe child node not possible, must be 2 at least
            caseNode = new ExprCaseNode(false);
            caseNode.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(caseNode);

            // in a case 1 expression (e.g. case when a=b then 1 else 2) the when child nodes must return boolean
            caseNode.AddChildNode(new SupportExprNode(2));
            TryInvalidValidate(caseNode);

            // in a case 2 expression (e.g. case a when b then 1 else 2) then a and b types must be comparable
            caseNode = new ExprCaseNode(true);
            caseNode.AddChildNode(new SupportExprNode("a"));
            caseNode.AddChildNode(new SupportExprNode(1));
            caseNode.AddChildNode(new SupportExprNode(2));
            TryInvalidValidate(caseNode);
        }
    }
} // end of namespace
