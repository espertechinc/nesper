///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprAndNode : AbstractCommonTest
    {
        private ExprAndNode andNode;

        [SetUp]
        public void SetUp()
        {
            andNode = new ExprAndNodeImpl();
        }

        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(andNode.EqualsNode(new ExprAndNodeImpl(), false));
            Assert.IsFalse(andNode.EqualsNode(new ExprOrNode(), false));
        }

        [Test]
        public void TestEvaluate()
        {
            andNode.AddChildNode(new SupportBoolExprNode(true));
            andNode.AddChildNode(new SupportBoolExprNode(true));
            SupportExprNodeUtil.Validate(container, andNode);
            Assert.IsTrue((bool) andNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            andNode = new ExprAndNodeImpl();
            andNode.AddChildNode(new SupportBoolExprNode(true));
            andNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(container, andNode);
            Assert.IsFalse((bool) andNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), andNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            andNode.AddChildNode(new SupportExprNode(true));
            andNode.AddChildNode(new SupportExprNode(false));

            Assert.AreEqual("true and false", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(andNode));
        }

        [Test]
        public void TestValidate()
        {
            // test success
            andNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            andNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            andNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            // test failure, type mismatch
            andNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                andNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // test failed - with just one child
            andNode = new ExprAndNodeImpl();
            andNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            try
            {
                andNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
