///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprRelationalOpNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            opNode = new ExprRelationalOpNodeImpl(RelationalOpEnum.GE);
        }

        private ExprRelationalOpNode opNode;

        private ExprRelationalOpNode MakeNode(
            object valueLeft,
            Type typeLeft,
            object valueRight,
            Type typeRight)
        {
            ExprRelationalOpNode relOpNode = new ExprRelationalOpNodeImpl(RelationalOpEnum.GE);
            relOpNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
            relOpNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
            SupportExprNodeUtil.Validate(container, relOpNode);
            return relOpNode;
        }

        [Test]
        public void TestEqualsNode()
        {
            ClassicAssert.IsTrue(opNode.EqualsNode(opNode, false));
            ClassicAssert.IsFalse(opNode.EqualsNode(new ExprRelationalOpNodeImpl(RelationalOpEnum.LE), false));
            ClassicAssert.IsFalse(opNode.EqualsNode(new ExprOrNode(), false));
        }

        [Test]
        public void TestEvaluate()
        {
            var childOne = new SupportExprNode("d");
            var childTwo = new SupportExprNode("c");
            opNode.AddChildNode(childOne);
            opNode.AddChildNode(childTwo);
            opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container)); // Type initialization

            ClassicAssert.AreEqual(true, opNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            childOne.Value = "c";
            ClassicAssert.AreEqual(true, opNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            childOne.Value = "b";
            ClassicAssert.AreEqual(false, opNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            opNode = MakeNode(null, typeof(int?), 2, typeof(int?));
            ClassicAssert.AreEqual(null, opNode.Forge.ExprEvaluator.Evaluate(null, false, null));
            opNode = MakeNode(1, typeof(int?), null, typeof(int?));
            ClassicAssert.AreEqual(null, opNode.Forge.ExprEvaluator.Evaluate(null, false, null));
            opNode = MakeNode(null, typeof(int?), null, typeof(int?));
            ClassicAssert.AreEqual(null, opNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            opNode.AddChildNode(new SupportExprNode(typeof(long?)));
            opNode.AddChildNode(new SupportExprNode(typeof(int)));
            opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            ClassicAssert.AreEqual(typeof(bool?), opNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            opNode.AddChildNode(new SupportExprNode(10));
            opNode.AddChildNode(new SupportExprNode(5));
            ClassicAssert.AreEqual("10>=5", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(opNode));
        }

        [Test]
        public void TestValidate()
        {
            // Test success
            opNode.AddChildNode(new SupportExprNode(typeof(string)));
            opNode.AddChildNode(new SupportExprNode(typeof(string)));
            opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            opNode.ChildNodes = new[] {
                new SupportExprNode(typeof(string))
            };

            // Test too few nodes under this node
            try
            {
                opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected
            }

            // Test mismatch type
            opNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // Test type cannot be compared
            opNode.ChildNodes = new[] {
                new SupportExprNode(typeof(bool?))
            };
            opNode.AddChildNode(new SupportExprNode(typeof(bool?)));

            try
            {
                opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
