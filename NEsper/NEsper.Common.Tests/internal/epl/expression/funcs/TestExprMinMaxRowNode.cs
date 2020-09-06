///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    [TestFixture]
    public class TestExprMinMaxRowNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
        }

        private ExprMinMaxRowNode minMaxNode;

        private void SetupNode(
            ExprMinMaxRowNode nodeMin,
            int intValue,
            double doubleValue,
            float? floatValue)
        {
            nodeMin.AddChildNode(new SupportExprNode(intValue));
            nodeMin.AddChildNode(new SupportExprNode(doubleValue));
            if (floatValue != null)
            {
                nodeMin.AddChildNode(new SupportExprNode(floatValue));
            }

            nodeMin.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
        }

        private ExprMinMaxRowNode MakeNode(
            object valueOne,
            Type typeOne,
            object valueTwo,
            Type typeTwo,
            object valueThree,
            Type typeThree)
        {
            var maxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            maxNode.AddChildNode(new SupportExprNode(valueOne, typeOne));
            maxNode.AddChildNode(new SupportExprNode(valueTwo, typeTwo));
            maxNode.AddChildNode(new SupportExprNode(valueThree, typeThree));
            maxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            return maxNode;
        }

        [Test, RunInApplicationDomain]
        public void TestEqualsNode()
        {
            Assert.IsTrue(minMaxNode.EqualsNode(minMaxNode, false));
            Assert.IsFalse(minMaxNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN), false));
            Assert.IsFalse(minMaxNode.EqualsNode(new ExprOrNode(), false));
        }

        [Test, RunInApplicationDomain]
        public void TestEvaluate()
        {
            minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            SetupNode(minMaxNode, 10, 1.5, null);
            Assert.AreEqual(10d, minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            SetupNode(minMaxNode, 1, 1.5, null);
            Assert.AreEqual(1.5d, minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MIN);
            SetupNode(minMaxNode, 1, 1.5, null);
            Assert.AreEqual(1d, minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            SetupNode(minMaxNode, 1, 1.5, 2.0f);
            Assert.AreEqual(2.0d, minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MIN);
            SetupNode(minMaxNode, 6, 3.5, 2.0f);
            Assert.AreEqual(2.0d, minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            minMaxNode = MakeNode(null, typeof(int?), 5, typeof(int?), 6, typeof(int?));
            Assert.IsNull(minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));
            minMaxNode = MakeNode(7, typeof(int?), null, typeof(int?), 6, typeof(int?));
            Assert.IsNull(minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));
            minMaxNode = MakeNode(3, typeof(int?), 5, typeof(int?), null, typeof(int?));
            Assert.IsNull(minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));
            minMaxNode = MakeNode(null, typeof(int?), null, typeof(int?), null, typeof(int?));
            Assert.IsNull(minMaxNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test, RunInApplicationDomain]
        public void TestGetType()
        {
            minMaxNode.AddChildNode(new SupportExprNode(typeof(double?)));
            minMaxNode.AddChildNode(new SupportExprNode(typeof(int?)));
            minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual(typeof(double?), minMaxNode.Forge.EvaluationType);

            minMaxNode.AddChildNode(new SupportExprNode(typeof(double?)));
            minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual(typeof(double?), minMaxNode.Forge.EvaluationType);
        }

        [Test, RunInApplicationDomain]
        public void TestToExpressionString()
        {
            minMaxNode.AddChildNode(new SupportExprNode(9d));
            minMaxNode.AddChildNode(new SupportExprNode(6));
            Assert.AreEqual("max(9.0d,6)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(minMaxNode));
            minMaxNode.AddChildNode(new SupportExprNode(0.5d));
            Assert.AreEqual("max(9.0d,6,0.5d)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(minMaxNode));
        }

        [Test, RunInApplicationDomain]
        public void TestValidate()
        {
            // Must have 2 or more subnodes
            try
            {
                minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // Must have only number-type subnodes
            minMaxNode.AddChildNode(new SupportExprNode(typeof(string)));
            minMaxNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
