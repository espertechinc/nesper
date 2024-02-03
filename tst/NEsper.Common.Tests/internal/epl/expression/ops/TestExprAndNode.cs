///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.IsTrue(andNode.EqualsNode(new ExprAndNodeImpl(), false));
            ClassicAssert.IsFalse(andNode.EqualsNode(new ExprOrNode(), false));
        }

        [Test]
        public void TestEvaluate()
        {
            andNode.AddChildNode(new SupportBoolExprNode(true));
            andNode.AddChildNode(new SupportBoolExprNode(true));
            SupportExprNodeUtil.Validate(container, andNode);
            ClassicAssert.IsTrue((bool) andNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            andNode = new ExprAndNodeImpl();
            andNode.AddChildNode(new SupportBoolExprNode(true));
            andNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(container, andNode);
            ClassicAssert.IsFalse((bool) andNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            ClassicAssert.AreEqual(typeof(bool?), andNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            andNode.AddChildNode(new SupportExprNode(true));
            andNode.AddChildNode(new SupportExprNode(false));

            ClassicAssert.AreEqual("true and false", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(andNode));
        }

        [Test]
        public void TestValidate()
        {
            // test success
            andNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            andNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            andNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            // test failure, Type mismatch
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
