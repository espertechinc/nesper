///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprOrNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            orNode = new ExprOrNode();
        }

        private ExprOrNode orNode;

        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(orNode.EqualsNode(orNode, false));
            Assert.IsFalse(orNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN), false));
            Assert.IsTrue(orNode.EqualsNode(new ExprOrNode(), false));
        }

        [Test]
        public void TestEvaluate()
        {
            orNode.AddChildNode(new SupportBoolExprNode(true));
            orNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(container, orNode);
            Assert.IsTrue((bool) orNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            orNode = new ExprOrNode();
            orNode.AddChildNode(new SupportBoolExprNode(false));
            orNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(container, orNode);
            Assert.IsFalse((bool) orNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            orNode = new ExprOrNode();
            orNode.AddChildNode(new SupportExprNode(null, typeof(bool?)));
            orNode.AddChildNode(new SupportExprNode(false));
            SupportExprNodeUtil.Validate(container, orNode);
            Assert.IsNull(orNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), orNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            orNode.AddChildNode(new SupportExprNode(true));
            orNode.AddChildNode(new SupportExprNode(false));
            Assert.AreEqual("True or False", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(orNode));
        }

        [Test]
        public void TestValidate()
        {
            // test success
            orNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            orNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            orNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            // test failure, type mismatch
            orNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                orNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }

            // test failed - with just one child
            orNode = new ExprOrNode();
            orNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            try
            {
                orNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
        }
    }
} // end of namespace
