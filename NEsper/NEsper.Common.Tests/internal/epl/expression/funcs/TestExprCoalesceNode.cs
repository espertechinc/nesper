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
    public class TestExprCoalesceNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            coalesceNodes = new ExprCoalesceNode[5];

            coalesceNodes[0] = new ExprCoalesceNode();
            coalesceNodes[0].AddChildNode(new SupportExprNode(null, typeof(long?)));
            coalesceNodes[0].AddChildNode(new SupportExprNode(null, typeof(int)));
            coalesceNodes[0].AddChildNode(new SupportExprNode(4, typeof(byte)));

            coalesceNodes[1] = new ExprCoalesceNode();
            coalesceNodes[1].AddChildNode(new SupportExprNode(null, typeof(string)));
            coalesceNodes[1].AddChildNode(new SupportExprNode("a", typeof(string)));

            coalesceNodes[2] = new ExprCoalesceNode();
            coalesceNodes[2].AddChildNode(new SupportExprNode(null, typeof(bool?)));
            coalesceNodes[2].AddChildNode(new SupportExprNode(true, typeof(bool)));

            coalesceNodes[3] = new ExprCoalesceNode();
            coalesceNodes[3].AddChildNode(new SupportExprNode(null, typeof(char)));
            coalesceNodes[3].AddChildNode(new SupportExprNode(null, typeof(char?)));
            coalesceNodes[3].AddChildNode(new SupportExprNode(null, typeof(char)));
            coalesceNodes[3].AddChildNode(new SupportExprNode('b', typeof(char?)));

            coalesceNodes[4] = new ExprCoalesceNode();
            coalesceNodes[4].AddChildNode(new SupportExprNode(5, typeof(float)));
            coalesceNodes[4].AddChildNode(new SupportExprNode(null, typeof(double?)));
        }

        private ExprCoalesceNode[] coalesceNodes;

        [Test, RunInApplicationDomain]
        public void TestEquals()
        {
            Assert.IsFalse(coalesceNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            Assert.IsTrue(coalesceNodes[0].EqualsNode(coalesceNodes[1], false));
        }

        [Test, RunInApplicationDomain]
        public void TestEvaluate()
        {
            for (var i = 0; i < coalesceNodes.Length; i++)
            {
                coalesceNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            Assert.AreEqual(4L, coalesceNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual("a", coalesceNodes[1].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(true, coalesceNodes[2].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual('b', coalesceNodes[3].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(5D, coalesceNodes[4].Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test, RunInApplicationDomain]
        public void TestGetType()
        {
            for (var i = 0; i < coalesceNodes.Length; i++)
            {
                coalesceNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            Assert.AreEqual(typeof(long?), coalesceNodes[0].Forge.EvaluationType);
            Assert.AreEqual(typeof(string), coalesceNodes[1].Forge.EvaluationType);
            Assert.AreEqual(typeof(bool?), coalesceNodes[2].Forge.EvaluationType);
            Assert.AreEqual(typeof(char?), coalesceNodes[3].Forge.EvaluationType);
            Assert.AreEqual(typeof(double?), coalesceNodes[4].Forge.EvaluationType);
        }

        [Test, RunInApplicationDomain]
        public void TestToExpressionString()
        {
            coalesceNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual("coalesce(null,null,4)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(coalesceNodes[0]));
        }

        [Test, RunInApplicationDomain]
        public void TestValidate()
        {
            var coalesceNode = new ExprCoalesceNode();
            coalesceNode.AddChildNode(new SupportExprNode(1));

            // Test too few nodes under this node
            try
            {
                coalesceNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // Test node result type not fitting
            coalesceNode.AddChildNode(new SupportExprNode("s"));
            try
            {
                coalesceNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
