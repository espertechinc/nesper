///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.spec;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;
using com.espertech.esper.view.stat;
using com.espertech.esper.view.std;
using com.espertech.esper.view.window;

using NUnit.Framework;

namespace com.espertech.esper.view
{
    [TestFixture]
	public class TestViewServiceHelper 
	{
	    private readonly static Type TEST_CLASS = typeof(SupportBean);

	    private SupportSchemaNeutralView top;
	    private SupportSchemaNeutralView child_1;
	    private SupportSchemaNeutralView child_2;
	    private SupportSchemaNeutralView child_2_1;
	    private SupportSchemaNeutralView child_2_2;
	    private SupportSchemaNeutralView child_2_1_1;
	    private SupportSchemaNeutralView child_2_2_1;
	    private SupportSchemaNeutralView child_2_2_2;

	    private IContainer _container;

	    [SetUp]
	    public void SetUp()
	    {
	        _container = SupportContainer.Reset();
	        top = new SupportSchemaNeutralView("top");

	        child_1 = new SupportSchemaNeutralView("1");
	        child_2 = new SupportSchemaNeutralView("2");
	        top.AddView(child_1);
	        top.AddView(child_2);

	        child_2_1 = new SupportSchemaNeutralView("2_1");
	        child_2_2 = new SupportSchemaNeutralView("2_2");
	        child_2.AddView(child_2_1);
	        child_2.AddView(child_2_2);

	        child_2_1_1 = new SupportSchemaNeutralView("2_1_1");
	        child_2_2_1 = new SupportSchemaNeutralView("2_2_1");
	        child_2_2_2 = new SupportSchemaNeutralView("2_2_2");
	        child_2_1.AddView(child_2_1_1);
	        child_2_2.AddView(child_2_2_1);
	        child_2_2.AddView(child_2_2_2);
	    }

        [Test]
	    public void TestInstantiateChain()
	    {
	        SupportBeanClassView topView = new SupportBeanClassView(TEST_CLASS);
	        IList<ViewFactory> viewFactories = SupportViewSpecFactory.MakeFactoryListOne(topView.EventType);
	        AgentInstanceViewFactoryChainContext context = SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container);

	        // Check correct views created
	        IList<View> views = ViewServiceHelper.InstantiateChain(topView, viewFactories, context);

	        Assert.AreEqual(3, views.Count);
	        Assert.AreEqual(typeof(LengthWindowView), views[0].GetType());
	        Assert.AreEqual(typeof(UnivariateStatisticsView), views[1].GetType());
	        Assert.AreEqual(typeof(LastElementView), views[2].GetType());

	        // Check that the context is set
	        viewFactories = SupportViewSpecFactory.MakeFactoryListFive(topView.EventType);
	        views = ViewServiceHelper.InstantiateChain(topView, viewFactories, context);
	        TimeWindowView timeWindow = (TimeWindowView) views[0];
	    }

        [Test]
	    public void TestMatch()
	    {
	        SupportStreamImpl stream = new SupportStreamImpl(TEST_CLASS, 10);
	        IList<ViewFactory> viewFactories = SupportViewSpecFactory.MakeFactoryListOne(stream.EventType);
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);

	        // No views under stream, no matches
            Pair<Viewable, IList<View>> result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);
	        Assert.AreEqual(stream, result.First);
	        Assert.AreEqual(3, viewFactories.Count);
	        Assert.AreEqual(0, result.Second.Count);

	        // One top view under the stream that doesn't match
	        SupportBeanClassView testView = new SupportBeanClassView(TEST_CLASS);
	        stream.AddView(new FirstElementView(null));
            result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);

	        Assert.AreEqual(stream, result.First);
	        Assert.AreEqual(3, viewFactories.Count);
	        Assert.AreEqual(0, result.Second.Count);

	        // Another top view under the stream that doesn't matche again
	        testView = new SupportBeanClassView(TEST_CLASS);
	        stream.AddView(new LengthWindowView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), null, 999, null));
            result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);

	        Assert.AreEqual(stream, result.First);
	        Assert.AreEqual(3, viewFactories.Count);
	        Assert.AreEqual(0, result.Second.Count);

	        // One top view under the stream that does actually match
	        LengthWindowView myLengthWindowView = new LengthWindowView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), null, 1000, null);
	        stream.AddView(myLengthWindowView);
            result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);

	        Assert.AreEqual(myLengthWindowView, result.First);
	        Assert.AreEqual(2, viewFactories.Count);
	        Assert.AreEqual(1, result.Second.Count);
	        Assert.AreEqual(myLengthWindowView, result.Second[0]);

	        // One child view under the top view that does not match
	        testView = new SupportBeanClassView(TEST_CLASS);
	        viewFactories = SupportViewSpecFactory.MakeFactoryListOne(stream.EventType);
	        EventType type = UnivariateStatisticsView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
	        UnivariateStatisticsViewFactory factory = new UnivariateStatisticsViewFactory();
	        factory.EventType = type;
	        factory.FieldExpression = SupportExprNodeFactory.MakeIdentNodeBean("LongBoxed");
	        myLengthWindowView.AddView(new UnivariateStatisticsView(factory, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)));
            result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);
	        Assert.AreEqual(1, result.Second.Count);
	        Assert.AreEqual(myLengthWindowView, result.Second[0]);
	        Assert.AreEqual(myLengthWindowView, result.First);
	        Assert.AreEqual(2, viewFactories.Count);

	        // Add child view under the top view that does match
	        viewFactories = SupportViewSpecFactory.MakeFactoryListOne(stream.EventType);
	        UnivariateStatisticsViewFactory factoryTwo = new UnivariateStatisticsViewFactory();
	        factoryTwo.EventType = type;
	        factoryTwo.FieldExpression = SupportExprNodeFactory.MakeIdentNodeBean("IntPrimitive");
	        UnivariateStatisticsView myUnivarView = new UnivariateStatisticsView(factoryTwo, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
	        myLengthWindowView.AddView(myUnivarView);
            result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);

	        Assert.AreEqual(myUnivarView, result.First);
	        Assert.AreEqual(1, viewFactories.Count);

	        // Add ultimate child view under the child view that does match
	        viewFactories = SupportViewSpecFactory.MakeFactoryListOne(stream.EventType);
	        LastElementView lastElementView = new LastElementView(null);
	        myUnivarView.AddView(lastElementView);
            result = ViewServiceHelper.MatchExistingViews(stream, viewFactories, agentInstanceContext);

	        Assert.AreEqual(lastElementView, result.First);
	        Assert.AreEqual(0, viewFactories.Count);
	    }

        [Test]
	    public void TestAddMergeViews()
	    {
	        IList<ViewSpec> specOne = SupportViewSpecFactory.MakeSpecListOne();

	        ViewServiceHelper.AddMergeViews(specOne);
	        Assert.AreEqual(3, specOne.Count);

	        IList<ViewSpec> specFour = SupportViewSpecFactory.MakeSpecListTwo();
	        ViewServiceHelper.AddMergeViews(specFour);
	        Assert.AreEqual(3, specFour.Count);
	        Assert.AreEqual("merge", specFour[2].ObjectName);
	        Assert.AreEqual(specFour[0].ObjectParameters.Count, specFour[1].ObjectParameters.Count);
	    }

        [Test]
	    public void TestRemoveChainLeafView()
	    {
	        // Remove a non-leaf, expect no removals
	        IList<View> removedViews = ViewServiceHelper.RemoveChainLeafView(top, child_2_2);
	        Assert.AreEqual(0, removedViews.Count);
	        Assert.AreEqual(2, child_2.Views.Length);

	        // Remove the whole tree child-by-child
	        removedViews = ViewServiceHelper.RemoveChainLeafView(top, child_2_2_2);
	        Assert.AreEqual(1, removedViews.Count);
	        Assert.AreEqual(child_2_2_2, removedViews[0]);
	        Assert.AreEqual(2, child_2.Views.Length);

	        removedViews = ViewServiceHelper.RemoveChainLeafView(top, child_2_2_1);
	        Assert.AreEqual(2, removedViews.Count);
	        Assert.AreEqual(child_2_2_1, removedViews[0]);
	        Assert.AreEqual(child_2_2, removedViews[1]);
	        Assert.AreEqual(1, child_2.Views.Length);

	        removedViews = ViewServiceHelper.RemoveChainLeafView(top, child_1);
	        Assert.AreEqual(1, removedViews.Count);
	        Assert.AreEqual(child_1, removedViews[0]);

	        removedViews = ViewServiceHelper.RemoveChainLeafView(top, child_2_1_1);
	        Assert.AreEqual(3, removedViews.Count);
	        Assert.AreEqual(child_2_1_1, removedViews[0]);
	        Assert.AreEqual(child_2_1, removedViews[1]);
	        Assert.AreEqual(child_2, removedViews[2]);

	        Assert.AreEqual(0, child_2.Views.Length);
	        Assert.AreEqual(0, top.Views.Length);
	    }
	}
} // end of namespace
