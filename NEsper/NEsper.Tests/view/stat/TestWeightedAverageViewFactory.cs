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
    public class TestWeightedAverageViewFactory 
    {
        private WeightedAverageViewFactory _factory;
        private readonly ViewFactoryContext _viewFactoryContext = new ViewFactoryContext(null, 1, null, null, false, -1, false);
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new WeightedAverageViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {"Price", "Volume"}, "Price", "Volume");

            TryInvalidParameter(new Object[] { "Symbol", 1.1d });
            TryInvalidParameter(new Object[] {1.1d, "Feed"});
            TryInvalidParameter(new Object[] {1.1d});
            TryInvalidParameter(new Object[] { "Feed", "Symbol", "Feed" });
            TryInvalidParameter(new Object[] {new[] {"Volume", "Price"}});
        }
    
        [Test]
        public void TestAttaches()
        {
            // Should attach to anything as long as the fields exists
            EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
    
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] {"Price", "Volume"}));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.AreEqual(typeof(double?), _factory.EventType.GetPropertyType(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName()));
    
            try
            {
                _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] { "Symbol", "Feed" }));
                _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected;
            }
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] { "Price", "Volume" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            EventType type = WeightedAverageView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            WeightedAverageViewFactory factoryTwo = new WeightedAverageViewFactory();
            factoryTwo.FieldNameX = SupportExprNodeFactory.MakeIdentNodeMD("Price");
            factoryTwo.EventType = type;
            factoryTwo.FieldNameWeight = SupportExprNodeFactory.MakeIdentNodeMD("Price");
            Assert.IsFalse(_factory.CanReuse(new WeightedAverageView(factoryTwo, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)), agentInstanceContext));
            factoryTwo.FieldNameWeight = SupportExprNodeFactory.MakeIdentNodeMD("Volume");
            Assert.IsTrue(_factory.CanReuse(new WeightedAverageView(factoryTwo, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)), agentInstanceContext));
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
    
        private void TryParameter(Object[] @params, String fieldNameX, String fieldNameW)
        {
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(@params));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            WeightedAverageView view = (WeightedAverageView) _factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(fieldNameX, view.FieldNameX.ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual(fieldNameW, view.FieldNameWeight.ToExpressionStringMinPrecedenceSafe());
        }
    }
}
