///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;
using com.espertech.esper.view.internals;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprPriorNode 
    {
        private ExprPriorNode _priorNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _priorNode = SupportExprNodeFactory.MakePriorNode();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(double), _priorNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            _priorNode = new ExprPriorNode();
    
            // No subnodes: Exception is thrown.
            TryInvalidValidate(_priorNode);
    
            // singe child node not possible, must be 2 at least
            _priorNode.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_priorNode);
        }
    
        [Test]
        public void TestEvaluate()
        {
            PriorEventBufferUnbound buffer = new PriorEventBufferUnbound(10);
            _priorNode.PriorStrategy = new ExprPriorEvalStrategyRandomAccess(buffer);
            EventBean[] events = new EventBean[] {MakeEvent(1d), MakeEvent(10d)};
            buffer.Update(events, null);
    
            Assert.AreEqual(1d, _priorNode.Evaluate(new EvaluateParams(events, true, null)));
        }
    
        [Test]
        public void TestEquals()
        {
            ExprPriorNode node1 = new ExprPriorNode();
            Assert.IsTrue(node1.EqualsNode(_priorNode, false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("prior(1,s0.DoublePrimitive)", _priorNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        private EventBean MakeEvent(double doublePrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.DoublePrimitive = doublePrimitive;
            return SupportEventBeanFactory.CreateObject(theEvent);
        }
    
        private void TryInvalidValidate(ExprPriorNode exprPriorNode)
        {
            try {
                exprPriorNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    }
}
