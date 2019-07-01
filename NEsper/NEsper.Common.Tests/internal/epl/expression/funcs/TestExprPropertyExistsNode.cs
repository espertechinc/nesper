///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    [TestFixture]
    public class TestExprPropertyExistsNode : AbstractTestBase
    {
        private SupportExprNodeFactory supportExprNodeFactory;
        private ExprPropertyExistsNode[] existsNodes;

        [SetUp]
        public void SetupTest()
        {
            supportExprNodeFactory = SupportExprNodeFactory.GetInstance(container);

            existsNodes = new ExprPropertyExistsNode[2];

            existsNodes[0] = new ExprPropertyExistsNode();
            existsNodes[0].AddChildNode(supportExprNodeFactory.MakeIdentNode("dummy?", "s0"));

            existsNodes[1] = new ExprPropertyExistsNode();
            existsNodes[1].AddChildNode(supportExprNodeFactory.MakeIdentNode("boolPrimitive?", "s0"));
        }


        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(existsNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            Assert.IsTrue(existsNodes[0].EqualsNode(existsNodes[1], false));
        }

        [Test]
        public void TestEvaluate()
        {
            for (var i = 0; i < existsNodes.Length; i++)
            {
                existsNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            Assert.AreEqual(false, existsNodes[0].Evaluate(new EventBean[3], false, null));
            Assert.AreEqual(false, existsNodes[1].Evaluate(new EventBean[3], false, null));

            EventBean[] events = { SupportEventBeanFactory.MakeEvents(supportEventTypeFactory, new string[1])[0] };
            Assert.AreEqual(false, existsNodes[0].Evaluate(events, false, null));
            Assert.AreEqual(true, existsNodes[1].Evaluate(events, false, null));
        }

        [Test]
        public void TestGetType()
        {
            for (var i = 0; i < existsNodes.Length; i++)
            {
                existsNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.AreEqual(typeof(bool?), existsNodes[i].EvaluationType);
            }
        }

        [Test]
        public void TestToExpressionString()
        {
            existsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual("exists(s0.dummy?)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(existsNodes[0]));
        }

        [Test]
        public void TestValidate()
        {
            var castNode = new ExprPropertyExistsNode();

            // Test too few nodes under this node
            try
            {
                castNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            castNode.AddChildNode(new SupportExprNode(1));
            try
            {
                castNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
