///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprMathNode : CommonTest
    {
        private ExprMathNode arithNode;

        [SetUp]
        public void SetUp()
        {
            arithNode = new ExprMathNode(MathArithTypeEnum.ADD, false, false);
        }

        private ExprMathNode MakeNode(
            object valueLeft,
            Type typeLeft,
            object valueRight,
            Type typeRight)
        {
            var mathNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
            mathNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
            mathNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
            SupportExprNodeUtil.Validate(container, mathNode);
            return mathNode;
        }

        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(arithNode.EqualsNode(arithNode, false));
            Assert.IsFalse(arithNode.EqualsNode(new ExprMathNode(MathArithTypeEnum.DIVIDE, false, false), false));
        }

        [Test]
        public void TestEvaluate()
        {
            arithNode.AddChildNode(new SupportExprNode(10));
            arithNode.AddChildNode(new SupportExprNode(1.5));
            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, arithNode, SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual(11.5d, arithNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            arithNode = MakeNode(null, typeof(int?), 5d, typeof(double?));
            Assert.IsNull(arithNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            arithNode = MakeNode(5, typeof(int?), null, typeof(double?));
            Assert.IsNull(arithNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            arithNode = MakeNode(null, typeof(int?), null, typeof(double?));
            Assert.IsNull(arithNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            arithNode.AddChildNode(new SupportExprNode(typeof(double?)));
            arithNode.AddChildNode(new SupportExprNode(typeof(int?)));
            arithNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual(typeof(double?), arithNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            // Build (5*(4-2)), not the same as 5*4-2
            var arithNodeChild = new ExprMathNode(MathArithTypeEnum.SUBTRACT, false, false);
            arithNodeChild.AddChildNode(new SupportExprNode(4));
            arithNodeChild.AddChildNode(new SupportExprNode(2));

            arithNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
            arithNode.AddChildNode(new SupportExprNode(5));
            arithNode.AddChildNode(arithNodeChild);

            Assert.AreEqual("5*(4-2)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(arithNode));
        }

        [Test]
        public void TestValidate()
        {
            // Must have exactly 2 subnodes
            try
            {
                arithNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }

            // Must have only number-type subnodes
            arithNode.AddChildNode(new SupportExprNode(typeof(string)));
            arithNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                arithNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
        }
    }
} // end of namespace