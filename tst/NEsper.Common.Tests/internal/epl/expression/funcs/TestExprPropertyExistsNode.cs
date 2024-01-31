///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    [TestFixture]
    public class TestExprPropertyExistsNode : AbstractCommonTest
    {
        private SupportExprNodeFactory _supportExprNodeFactory;
        private ExprPropertyExistsNode[] _existsNodes;

        [SetUp]
        public void SetupTest()
        {
            _supportExprNodeFactory = SupportExprNodeFactory.GetInstance(container);

            _existsNodes = new ExprPropertyExistsNode[2];

            _existsNodes[0] = new ExprPropertyExistsNode();
            _existsNodes[0].AddChildNode(_supportExprNodeFactory.MakeIdentNode("dummy?", "s0"));

            _existsNodes[1] = new ExprPropertyExistsNode();
            _existsNodes[1].AddChildNode(_supportExprNodeFactory.MakeIdentNode("BoolPrimitive?", "s0"));
        }


        [Test]
        public void TestEquals()
        {
            ClassicAssert.IsFalse(_existsNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            ClassicAssert.IsTrue(_existsNodes[0].EqualsNode(_existsNodes[1], false));
        }

        [Test]
        public void TestEvaluate()
        {
            for (var i = 0; i < _existsNodes.Length; i++)
            {
                _existsNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            ClassicAssert.AreEqual(false, _existsNodes[0].Evaluate(new EventBean[3], false, null));
            ClassicAssert.AreEqual(false, _existsNodes[1].Evaluate(new EventBean[3], false, null));

            EventBean[] events = { SupportEventBeanFactory.MakeEvents(supportEventTypeFactory, new string[1])[0] };
            ClassicAssert.AreEqual(false, _existsNodes[0].Evaluate(events, false, null));
            ClassicAssert.AreEqual(true, _existsNodes[1].Evaluate(events, false, null));
        }

        [Test]
        public void TestGetType()
        {
            for (var i = 0; i < _existsNodes.Length; i++)
            {
                _existsNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                ClassicAssert.AreEqual(typeof(bool?), _existsNodes[i].EvaluationType);
            }
        }

        [Test]
        public void TestToExpressionString()
        {
            _existsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            ClassicAssert.AreEqual("exists(s0.dummy?)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_existsNodes[0]));
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
