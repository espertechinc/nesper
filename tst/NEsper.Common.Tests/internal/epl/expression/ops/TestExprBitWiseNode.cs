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

using NUnit.Framework;
using NUnit.Framework.Legacy;

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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestEqualsNode()
        {
            Log.Debug(".testEqualsNode");
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            ClassicAssert.IsTrue(_bitWiseNode.EqualsNode(_bitWiseNode, false));
            ClassicAssert.IsFalse(_bitWiseNode.EqualsNode(new ExprBitWiseNode(BitWiseOpEnum.BXOR), false));
        }

        [Test]
        public void TestEvaluate()
        {
            Log.Debug(".testEvaluate");
            _bitWiseNode.AddChildNode(new SupportExprNode(10));
            _bitWiseNode.AddChildNode(new SupportExprNode(12));
            ExprNodeUtilityValidate.GetValidatedSubtree(
                ExprNodeOrigin.SELECT,
                _bitWiseNode,
                SupportExprValidationContextFactory.MakeEmpty(container));
            ClassicAssert.AreEqual(8, _bitWiseNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            Log.Debug(".testGetType");
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(double?)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
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
            ClassicAssert.AreEqual(typeof(long?), _bitWiseNode.Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            Log.Debug(".testToExpressionString");
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            _bitWiseNode.AddChildNode(new SupportExprNode(4));
            _bitWiseNode.AddChildNode(new SupportExprNode(2));
            ClassicAssert.AreEqual("4&2", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_bitWiseNode));
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
            catch (ExprValidationException)
            {
                // Expected
                Log.Debug("No nodes in the expression");
            }

            // Must have only number or boolean-type subnodes
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(string)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
