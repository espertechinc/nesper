///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.stat
{
    [TestFixture]
    public class TestUnivariateStatisticsViewFactory 
    {
        private UnivariateStatisticsViewFactory _factory;
        private readonly ViewFactoryContext _viewFactoryContext = new ViewFactoryContext(null, 1, null, null, false, -1, false);
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new UnivariateStatisticsViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {"Price"}, "Price");
    
            TryInvalidParameter(new Object[] {});
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[]{ "Price" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            EventType type = UnivariateStatisticsView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            UnivariateStatisticsViewFactory factoryOne = new UnivariateStatisticsViewFactory();
            factoryOne.EventType = type;
            factoryOne.FieldExpression = SupportExprNodeFactory.MakeIdentNodeMD("Volume");
            UnivariateStatisticsViewFactory factoryTwo = new UnivariateStatisticsViewFactory();
            factoryTwo.EventType = type;
            factoryTwo.FieldExpression = SupportExprNodeFactory.MakeIdentNodeMD("Price");
            Assert.IsFalse(_factory.CanReuse(new UnivariateStatisticsView(factoryOne, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new UnivariateStatisticsView(factoryTwo, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)), agentInstanceContext));
        }
    
        [Test]
        public void TestAttaches()
        {
            // Should attach to anything as long as the fields exists
            EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
    
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] {"Price"}));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.AreEqual(typeof(double?), _factory.EventType.GetPropertyType(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE.GetName()));
    
            try
            {
                _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] {"Symbol"}));
                _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected;
            }
        }
    
        private void TryInvalidParameter(Object[] @params)
        {
            try
            {
                _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(@params));
                _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] @params, String fieldName)
        {
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(@params));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            UnivariateStatisticsView view = (UnivariateStatisticsView) _factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(fieldName, view.FieldExpression.ToExpressionStringMinPrecedenceSafe());
        }
    }
}
