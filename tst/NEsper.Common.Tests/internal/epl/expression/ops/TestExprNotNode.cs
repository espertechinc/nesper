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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprNotNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            notNode = new ExprNotNode();
        }

        private ExprNotNode notNode;

        [Test]
        public void TestEqualsNode()
        {
            ClassicAssert.IsTrue(notNode.EqualsNode(notNode, false));
            ClassicAssert.IsFalse(notNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN), false));
            ClassicAssert.IsFalse(notNode.EqualsNode(new ExprOrNode(), false));
            ClassicAssert.IsTrue(notNode.EqualsNode(new ExprNotNode(), false));
        }

        [Test]
        public void TestEvaluate()
        {
            notNode.AddChildNode(new SupportBoolExprNode(true));
            SupportExprNodeUtil.Validate(container, notNode);
            ClassicAssert.IsFalse((bool) notNode.Evaluate(null, false, null));

            notNode = new ExprNotNode();
            notNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(container, notNode);
            ClassicAssert.IsTrue((bool) notNode.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            ClassicAssert.AreEqual(typeof(bool?), notNode.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            notNode.AddChildNode(new SupportExprNode(true));
            ClassicAssert.AreEqual("not true", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(notNode));
        }

        [Test]
        public void TestValidate()
        {
            // fails with zero expressions
            try
            {
                notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // fails with too many sub-expressions
            notNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            notNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            try
            {
                notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // test failure, Type mismatch
            notNode = new ExprNotNode();
            notNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // validates
            notNode = new ExprNotNode();
            notNode.AddChildNode(new SupportExprNode(typeof(bool?)));
            notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
        }
    }
} // end of namespace
