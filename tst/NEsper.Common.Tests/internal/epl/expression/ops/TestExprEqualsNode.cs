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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprEqualsNode : AbstractCommonTest
    {
        private ExprEqualsNode[] equalsNodes;

        [SetUp]
        public void SetUp()
        {
            equalsNodes = new ExprEqualsNode[4];
            equalsNodes[0] = new ExprEqualsNodeImpl(false, false);

            equalsNodes[1] = new ExprEqualsNodeImpl(false, false);
            equalsNodes[1].AddChildNode(new SupportExprNode(1L));
            equalsNodes[1].AddChildNode(new SupportExprNode(1));
            equalsNodes[1].Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            equalsNodes[2] = new ExprEqualsNodeImpl(true, false);
            equalsNodes[2].AddChildNode(new SupportExprNode(1.5D));
            equalsNodes[2].AddChildNode(new SupportExprNode(1));
            equalsNodes[2].Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            equalsNodes[3] = new ExprEqualsNodeImpl(false, false);
            equalsNodes[3].AddChildNode(new SupportExprNode(1D));
            equalsNodes[3].AddChildNode(new SupportExprNode(1));
            equalsNodes[3].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
        }

        private ExprEqualsNode MakeNode(
            object valueLeft,
            object valueRight,
            bool isNot)
        {
            ExprEqualsNode equalsNode = new ExprEqualsNodeImpl(isNot, false);
            equalsNode.AddChildNode(new SupportExprNode(valueLeft));
            equalsNode.AddChildNode(new SupportExprNode(valueRight));
            SupportExprNodeUtil.Validate(container, equalsNode);
            return equalsNode;
        }

        private ExprEqualsNode MakeNode(
            object valueLeft,
            Type typeLeft,
            object valueRight,
            Type typeRight,
            bool isNot)
        {
            ExprEqualsNode equalsNode = new ExprEqualsNodeImpl(isNot, false);
            equalsNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
            equalsNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
            SupportExprNodeUtil.Validate(container, equalsNode);
            return equalsNode;
        }

        [Test]
        public void TestEqualsNode()
        {
            ClassicAssert.IsTrue(equalsNodes[0].EqualsNode(equalsNodes[1], false));
            ClassicAssert.IsFalse(equalsNodes[0].EqualsNode(equalsNodes[2], false));
        }

        [Test]
        public void TestEvaluateEquals()
        {
            equalsNodes[0] = MakeNode(true, false, false);
            ClassicAssert.IsFalse((bool) equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(false, false, false);
            ClassicAssert.IsTrue((bool) equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(true, true, false);
            ClassicAssert.IsTrue((bool) equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(true, typeof(bool?), null, typeof(bool?), false);
            ClassicAssert.IsNull(equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(null, typeof(string), "ss", typeof(string), false);
            ClassicAssert.IsNull(equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(null, typeof(string), null, typeof(string), false);
            ClassicAssert.IsNull(equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            // try a long and int
            equalsNodes[1].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            ClassicAssert.IsTrue((bool) equalsNodes[1].Forge.ExprEvaluator.Evaluate(null, false, null));

            // try a double and int
            equalsNodes[2].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            ClassicAssert.IsTrue((bool) equalsNodes[2].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[3].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            ClassicAssert.IsTrue((bool) equalsNodes[3].Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestEvaluateNotEquals()
        {
            equalsNodes[0] = MakeNode(true, false, true);
            ClassicAssert.IsTrue((bool) equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(false, false, true);
            ClassicAssert.IsFalse((bool) equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(true, true, true);
            ClassicAssert.IsFalse((bool) equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(true, typeof(bool?), null, typeof(bool?), true);
            ClassicAssert.IsNull(equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(null, typeof(string), "ss", typeof(string), true);
            ClassicAssert.IsNull(equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));

            equalsNodes[0] = MakeNode(null, typeof(string), null, typeof(string), true);
            ClassicAssert.IsNull(equalsNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            ClassicAssert.AreEqual(typeof(bool?), equalsNodes[1].Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            equalsNodes[0].AddChildNode(new SupportExprNode(true));
            equalsNodes[0].AddChildNode(new SupportExprNode(false));
            ClassicAssert.AreEqual("true=false", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(equalsNodes[0]));
        }

        [Test]
        public void TestValidate()
        {
            // Test success
            equalsNodes[0].AddChildNode(new SupportExprNode(typeof(string)));
            equalsNodes[0].AddChildNode(new SupportExprNode(typeof(string)));
            equalsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            equalsNodes[1].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            equalsNodes[2].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            equalsNodes[3].Validate(SupportExprValidationContextFactory.MakeEmpty(container));

            equalsNodes[0].ChildNodes = new[] {
                new SupportExprNode(typeof(string))
            };

            // Test too few nodes under this node
            try
            {
                equalsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // Test mismatch type
            equalsNodes[0].AddChildNode(new SupportExprNode(typeof(bool?)));
            try
            {
                equalsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
