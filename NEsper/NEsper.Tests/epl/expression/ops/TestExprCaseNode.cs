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
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprCaseNode 
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestGetType()
        {
            // Template expression is:
            // case when (so.floatPrimitive>s1.shortBoxed) then count(5) when (so.LongPrimitive>s1.IntPrimitive) then (25 + 130.5) else (3*3) end
            ExprCaseNode caseNode = SupportExprNodeFactory.MakeCaseSyntax1Node();
            Assert.AreEqual(typeof(string), caseNode.ReturnType);
    
            // case when (2.5>2) then count(5) when (1>3) then (25 + 130.5) else (3*3) end
            // First when node is true, case node type is the first when node type.
            caseNode = SupportExprNodeFactory.MakeCaseSyntax2Node();
            Assert.AreEqual(typeof(string), caseNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            ExprCaseNode caseNode = SupportExprNodeFactory.MakeCaseSyntax1Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            caseNode = SupportExprNodeFactory.MakeCaseSyntax2Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
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
    
        [Test]
        public void TestEvaluate()
        {
            ExprCaseNode caseNode = SupportExprNodeFactory.MakeCaseSyntax1Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            Assert.AreEqual("a", caseNode.Evaluate(new EvaluateParams(MakeEvent(1), false, null)));
            Assert.AreEqual("b", caseNode.Evaluate(new EvaluateParams(MakeEvent(2), false, null)));
            Assert.AreEqual("c", caseNode.Evaluate(new EvaluateParams(MakeEvent(3), false, null)));
    
            caseNode = SupportExprNodeFactory.MakeCaseSyntax2Node();
            caseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            Assert.AreEqual("a", caseNode.Evaluate(new EvaluateParams(MakeEvent(1), false, null)));
            Assert.AreEqual("b", caseNode.Evaluate(new EvaluateParams(MakeEvent(2), false, null)));
            Assert.AreEqual("c", caseNode.Evaluate(new EvaluateParams(MakeEvent(3), false, null)));
        }
    
        [Test]
        public void TestEquals()
        {
            ExprCaseNode caseNode = SupportExprNodeFactory.MakeCaseSyntax1Node();
            ExprCaseNode otherCaseNode = SupportExprNodeFactory.MakeCaseSyntax1Node();
            ExprCaseNode caseNodeSyntax2 = SupportExprNodeFactory.MakeCaseSyntax2Node();
            ExprCaseNode otherCaseNodeSyntax2 = SupportExprNodeFactory.MakeCaseSyntax2Node();
    
            Assert.IsTrue(caseNode.EqualsNode(otherCaseNode, false));
            Assert.IsTrue(otherCaseNode.EqualsNode(caseNode, false));
            Assert.IsFalse(caseNode.EqualsNode(caseNodeSyntax2, false));
            Assert.IsFalse(caseNodeSyntax2.EqualsNode(caseNode, false));
            Assert.IsTrue(caseNodeSyntax2.EqualsNode(otherCaseNodeSyntax2, false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            ExprCaseNode _caseNode = SupportExprNodeFactory.MakeCaseSyntax1Node();
            Assert.AreEqual("case when s0.IntPrimitive=1 then \"a\" when s0.IntPrimitive=2 then \"b\" else \"c\" end", _caseNode.ToExpressionStringMinPrecedenceSafe());
    
            _caseNode = SupportExprNodeFactory.MakeCaseSyntax2Node();
            Assert.AreEqual("case s0.IntPrimitive when 1 then \"a\" when 2 then \"b\" else \"c\" end", _caseNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        private void TryInvalidValidate(ExprCaseNode exprCaseNode)
        {
            try {
                exprCaseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    
        private EventBean[] MakeEvent(int intPrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return new EventBean[] {SupportEventBeanFactory.CreateObject(theEvent)};
        }
    }
}
