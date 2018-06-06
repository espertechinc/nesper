///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;

using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprStreamUnderlyingNode 
    {
        private ExprStreamUnderlyingNodeImpl _node;
        private StreamTypeService _streamTypeService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _node = new ExprStreamUnderlyingNodeImpl("s0", false);
            _streamTypeService = new SupportStreamTypeSvc3Stream();
        }
    
        [Test]
        public void TestValidateInvalid()
        {
            try
            {
                var abyss = _node.StreamId;
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
    
            try
            {
                var abyss = _node.ExprEvaluator.ReturnType;
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestValidate()
        {
            _node.Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(0, _node.StreamId);
            Assert.AreEqual(typeof(SupportBean), _node.ReturnType);
    
            TryInvalidValidate(new ExprStreamUnderlyingNodeImpl("", false));
            TryInvalidValidate(new ExprStreamUnderlyingNodeImpl("dummy", false));
        }
    
        [Test]
        public void TestEvaluate()
        {
            EventBean theEvent = MakeEvent(10);
            EventBean[] events = new EventBean[] {theEvent};

            _node.Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(theEvent.Underlying, _node.Evaluate(new EvaluateParams(events, false, null)));
        }
    
        [Test]
        public void TestEqualsNode()
        {
            _node.Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.IsTrue(_node.EqualsNode(new ExprStreamUnderlyingNodeImpl("s0", false), false));
            Assert.IsFalse(_node.EqualsNode(new ExprStreamUnderlyingNodeImpl("xxx", false), false));
        }
    
        protected static EventBean MakeEvent(int intPrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return SupportEventBeanFactory.CreateObject(theEvent);
        }
    
        private void TryInvalidValidate(ExprStreamUnderlyingNode node)
        {
            try
            {
                node.Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
                Assert.Fail();
            }
            catch(ExprValidationException)
            {
                // expected
            }
        }
    }
}
