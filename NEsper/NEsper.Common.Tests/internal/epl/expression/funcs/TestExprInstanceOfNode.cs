///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    [TestFixture]
    public class TestExprInstanceOfNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            instanceofNodes = new ExprInstanceofNode[5];

            instanceofNodes[0] = new ExprInstanceofNode(new[] { "long" });
            instanceofNodes[0].AddChildNode(new SupportExprNode(1l, typeof(long?)));

            instanceofNodes[1] = new ExprInstanceofNode(new[] { typeof(SupportBean).FullName, "int", "string" });
            instanceofNodes[1].AddChildNode(new SupportExprNode("", typeof(string)));

            instanceofNodes[2] = new ExprInstanceofNode(new[] { "string" });
            instanceofNodes[2].AddChildNode(new SupportExprNode(null, typeof(bool?)));

            instanceofNodes[3] = new ExprInstanceofNode(new[] { "string", "char" });
            instanceofNodes[3].AddChildNode(new SupportExprNode(new SupportBean(), typeof(object)));

            instanceofNodes[4] = new ExprInstanceofNode(new[] { "int", "float", typeof(SupportBean).FullName });
            instanceofNodes[4].AddChildNode(new SupportExprNode(new SupportBean(), typeof(object)));
        }

        private ExprInstanceofNode[] instanceofNodes;

        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(instanceofNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            Assert.IsFalse(instanceofNodes[0].EqualsNode(instanceofNodes[1], false));
            Assert.IsTrue(instanceofNodes[0].EqualsNode(instanceofNodes[0], false));
        }

        [Test]
        public void TestEvaluate()
        {
            for (var i = 0; i < instanceofNodes.Length; i++)
            {
                instanceofNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            Assert.AreEqual(true, instanceofNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(true, instanceofNodes[1].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(false, instanceofNodes[2].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(false, instanceofNodes[3].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(true, instanceofNodes[4].Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            for (var i = 0; i < instanceofNodes.Length; i++)
            {
                instanceofNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.AreEqual(typeof(bool?), instanceofNodes[i].Forge.EvaluationType);
            }
        }

        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual(
                "instanceof(\"\"," + typeof(SupportBean).FullName + ",int,string)",
                ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(instanceofNodes[1]));
        }

        [Test]
        public void TestValidate()
        {
            var instanceofNode = new ExprInstanceofNode(new string[0]);
            instanceofNode.AddChildNode(new SupportExprNode(1));

            // Test too few nodes under this node
            try
            {
                instanceofNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // Test node result type not fitting
            instanceofNode.AddChildNode(new SupportExprNode("s"));
            try
            {
                instanceofNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
