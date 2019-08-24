///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprBitWiseNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
        }

        private ExprBitWiseNode _bitWiseNode;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestEqualsNode()
        {
            log.Debug(".testEqualsNode");
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            Assert.IsTrue(_bitWiseNode.EqualsNode(_bitWiseNode, false));
            Assert.IsFalse(_bitWiseNode.EqualsNode(new ExprBitWiseNode(BitWiseOpEnum.BXOR), false));
        }

        [Test]
        public void TestEvaluate()
        {
            log.Debug(".testEvaluate");
            _bitWiseNode.AddChildNode(new SupportExprNode(10));
            _bitWiseNode.AddChildNode(new SupportExprNode(12));
            ExprNodeUtilityValidate.GetValidatedSubtree(
                ExprNodeOrigin.SELECT,
                _bitWiseNode,
                SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual(8, _bitWiseNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            log.Debug(".testGetType");
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(double?)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }

            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(long?)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(long?)));
            ExprNodeUtilityValidate.GetValidatedSubtree(
                ExprNodeOrigin.SELECT,
                _bitWiseNode,
                SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual(typeof(long?), _bitWiseNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            log.Debug(".testToExpressionString");
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            _bitWiseNode.AddChildNode(new SupportExprNode(4));
            _bitWiseNode.AddChildNode(new SupportExprNode(2));
            Assert.AreEqual("4&2", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_bitWiseNode));
        }

        [Test]
        public void TestValidate()
        {
            // Must have exactly 2 subnodes
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
                log.Debug("No nodes in the expression");
            }

            // Must have only number or boolean-type subnodes
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(string)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
        }
    }
} // end of namespace
