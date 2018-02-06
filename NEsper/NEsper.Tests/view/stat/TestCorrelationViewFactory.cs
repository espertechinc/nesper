///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.stat
{
    [TestFixture]
	public class TestCorrelationViewFactory 
	{
	    private CorrelationViewFactory _factory;
	    private readonly ViewFactoryContext _viewFactoryContext = new ViewFactoryContext(null, 1, null, null, false, -1, false);
	    private IContainer _container;

	    [SetUp]
	    public void SetUp()
	    {
	        _container = SupportContainer.Reset();
	        _factory = new CorrelationViewFactory();
	    }

        [Test]
	    public void TestSetParameters()
	    {
	        TryParameter(new object[] {"Price", "Volume"}, "Price", "Volume");

	        TryInvalidParameter(new object[] {"Symbol", 1.1d});
	        TryInvalidParameter(new object[] {1.1d, "Symbol"});
	        TryInvalidParameter(new object[] {1.1d});
	        TryInvalidParameter(new object[] {"Symbol", "Symbol", "Symbol"});
	        TryInvalidParameter(new object[] {new string[] {"Symbol", "Feed"}});
	    }

        [Test]
	    public void TestCanReuse()
	    {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(new ViewFactoryContext(null, 1, null, null, false, -1, false), TestViewSupport.ToExprListMD(new object[]{"Price", "Volume"}));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            EventType type = CorrelationView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            Assert.IsFalse(_factory.CanReuse(new CorrelationView(null, SupportStatementContextFactory.MakeAgentInstanceContext(_container), SupportExprNodeFactory.MakeIdentNodeMD("Volume"), SupportExprNodeFactory.MakeIdentNodeMD("Price"), type, null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new CorrelationView(null, SupportStatementContextFactory.MakeAgentInstanceContext(_container), SupportExprNodeFactory.MakeIdentNodeMD("Feed"), SupportExprNodeFactory.MakeIdentNodeMD("Volume"), type, null), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new CorrelationView(null, SupportStatementContextFactory.MakeAgentInstanceContext(_container), SupportExprNodeFactory.MakeIdentNodeMD("Price"), SupportExprNodeFactory.MakeIdentNodeMD("Volume"), type, null), agentInstanceContext));
	    }

        [Test]
	    public void TestAttaches()
	    {
	        // Should attach to anything as long as the fields exists
	        EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));

	        _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new object[] {"Price", "Volume"}));
	        _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
	        Assert.AreEqual(typeof(double?), _factory.EventType.GetPropertyType(ViewFieldEnum.CORRELATION__CORRELATION.GetName()));

	        try
	        {
	            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new object[] {"Symbol", "Volume"}));
	            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
	        }
	        catch (ViewParameterException)
	        {
	            // expected;
	        }
	    }

	    private void TryInvalidParameter(object[] parameters)
	    {
	        try
	        {
	            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
	            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
	            Assert.Fail();
	        }
	        catch (ViewParameterException)
	        {
	            // expected
	        }
	    }

	    private void TryParameter(object[] parameters, string fieldNameX, string fieldNameY)
	    {
	        _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
	        _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
	        CorrelationView view = (CorrelationView) _factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
	        Assert.AreEqual(fieldNameX, ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(view.ExpressionX));
	        Assert.AreEqual(fieldNameY, ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(view.ExpressionY));
	    }
	}
} // end of namespace
