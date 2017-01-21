///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;
using com.espertech.esper.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.stat
{
    [TestFixture]
	public class TestCorrelationViewFactory 
	{
	    private CorrelationViewFactory factory;
	    private ViewFactoryContext viewFactoryContext = new ViewFactoryContext(null, 1, null, null, false, -1, false);

        [SetUp]
	    public void SetUp()
	    {
	        factory = new CorrelationViewFactory();
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
	        factory.SetViewParameters(new ViewFactoryContext(null, 1, null, null, false, -1 , false), TestViewSupport.ToExprListMD(new object[] {"Price", "Volume"}));
	        factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
	        Assert.IsFalse(factory.CanReuse(new FirstElementView(null)));
	        EventType type = CorrelationView.CreateEventType(SupportStatementContextFactory.MakeContext(), null, 1);
	        Assert.IsFalse(factory.CanReuse(new CorrelationView(null, SupportStatementContextFactory.MakeAgentInstanceContext(), SupportExprNodeFactory.MakeIdentNodeMD("Volume"), SupportExprNodeFactory.MakeIdentNodeMD("Price"), type, null)));
	        Assert.IsFalse(factory.CanReuse(new CorrelationView(null, SupportStatementContextFactory.MakeAgentInstanceContext(), SupportExprNodeFactory.MakeIdentNodeMD("Feed"), SupportExprNodeFactory.MakeIdentNodeMD("Volume"), type, null)));
	        Assert.IsTrue(factory.CanReuse(new CorrelationView(null, SupportStatementContextFactory.MakeAgentInstanceContext(), SupportExprNodeFactory.MakeIdentNodeMD("Price"), SupportExprNodeFactory.MakeIdentNodeMD("Volume"), type, null)));
	    }

        [Test]
	    public void TestAttaches()
	    {
	        // Should attach to anything as long as the fields exists
	        EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));

	        factory.SetViewParameters(viewFactoryContext, TestViewSupport.ToExprListMD(new object[] {"Price", "Volume"}));
	        factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
	        Assert.AreEqual(typeof(double?), factory.EventType.GetPropertyType(ViewFieldEnum.CORRELATION__CORRELATION.GetName()));

	        try
	        {
	            factory.SetViewParameters(viewFactoryContext, TestViewSupport.ToExprListMD(new object[] {"Symbol", "Volume"}));
	            factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
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
	            factory.SetViewParameters(viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
	            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
	            Assert.Fail();
	        }
	        catch (ViewParameterException)
	        {
	            // expected
	        }
	    }

	    private void TryParameter(object[] parameters, string fieldNameX, string fieldNameY)
	    {
	        factory.SetViewParameters(viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
	        factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
	        CorrelationView view = (CorrelationView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
	        Assert.AreEqual(fieldNameX, ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(view.ExpressionX));
	        Assert.AreEqual(fieldNameY, ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(view.ExpressionY));
	    }
	}
} // end of namespace
