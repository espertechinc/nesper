///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestExternallyTimedWindowViewFactory 
    {
        private ExternallyTimedWindowViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
            _factory = new ExternallyTimedWindowViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {"LongPrimitive", 2d}, "LongPrimitive", 2000);
            TryParameter(new Object[] {"LongPrimitive", 10L}, "LongPrimitive", 10000);
            TryParameter(new Object[] {"LongPrimitive", 11}, "LongPrimitive", 11000);
            TryParameter(new Object[] {"LongPrimitive", 2.2}, "LongPrimitive", 2200);
    
            TryInvalidParameter(new Object[] {"a"});
        }
    
        [Test]
        public void TestCanReuse()
        {
            EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
    
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {"LongBoxed", 1000}));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsFalse(_factory.CanReuse(new ExternallyTimedWindowView(_factory, SupportExprNodeFactory.MakeIdentNodeBean("LongPrimitive"), null, new ExprTimePeriodEvalDeltaConstMsec(1000), null, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext())));
            Assert.IsFalse(_factory.CanReuse(new ExternallyTimedWindowView(_factory, SupportExprNodeFactory.MakeIdentNodeBean("LongBoxed"), null, new ExprTimePeriodEvalDeltaConstMsec(999), null, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext())));
            Assert.IsTrue(_factory.CanReuse(new ExternallyTimedWindowView(_factory, SupportExprNodeFactory.MakeIdentNodeBean("LongBoxed"), null, new ExprTimePeriodEvalDeltaConstMsec(1000000), null, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext())));
        }
    
        [Test]
        public void TestInvalid()
        {
            EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
    
            try
            {
                _factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {50, 20}));
                _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
    
            try
            {
                _factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {"TheString", 20}));
                _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
    
            _factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {"LongPrimitive", 20}));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
    
            Assert.AreSame(parentType, _factory.EventType);
        }
    
        private void TryInvalidParameter(Object[] param)
        {
            try
            {
                ExternallyTimedWindowViewFactory factory = new ExternallyTimedWindowViewFactory();
                factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {param}));
                factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] @params, String fieldName, long msec)
        {
            ExternallyTimedWindowViewFactory factory = new ExternallyTimedWindowViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(@params));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(), null, null);
            ExternallyTimedWindowView view = (ExternallyTimedWindowView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.AreEqual(fieldName, view.TimestampExpression.ToExpressionStringMinPrecedenceSafe());
            Assert.IsTrue(new ExprTimePeriodEvalDeltaConstMsec(msec).EqualsTimePeriod(view.TimeDeltaComputation));
        }
    }
}
