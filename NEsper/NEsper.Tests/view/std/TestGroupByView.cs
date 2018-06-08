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
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestGroupByView 
    {
        private GroupByView _myGroupByView;
        private SupportBeanClassView _ultimateChildView;
        private StatementContext _statementContext;
        private AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _statementContext = SupportStatementContextFactory.MakeContext(_container);
            _agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container);
    
            var expressions = SupportExprNodeFactory.MakeIdentNodesMD("Symbol");
            _myGroupByView = new GroupByViewImpl(_agentInstanceContext, expressions, ExprNodeUtility.GetEvaluators(expressions));
    
            var childView = new SupportBeanClassView(typeof(SupportMarketDataBean));

            var myMergeView = new MergeView(_agentInstanceContext, SupportExprNodeFactory.MakeIdentNodesMD("Symbol"), SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), false);
    
            _ultimateChildView = new SupportBeanClassView(typeof(SupportMarketDataBean));
    
            // This is the view hierarchy
            _myGroupByView.AddView(childView);
            childView.AddView(myMergeView);
            myMergeView.AddView(_ultimateChildView);
    
            SupportBeanClassView.Instances.Clear();
        }
    
        [Test]
        public void TestViewPush()
        {
            // Reset instance lists of child views
            SupportBeanClassView.Instances.Clear();
            SupportMapView.Instances.Clear();
    
            // Set up a feed for the view under test - it will have a depth of 3 trades
            var stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 3);
            stream.AddView(_myGroupByView);
    
            var tradeBeans = new EventBean[10];
    
            // Send an IBM symbol event
            tradeBeans[0] = MakeTradeBean("IBM", 70);
            stream.Insert(tradeBeans[0]);
    
            // Expect 1 new beanclass view instance and check its data
            Assert.AreEqual(1, SupportBeanClassView.Instances.Count);
            var child_1 = SupportBeanClassView.Instances[0];
            SupportViewDataChecker.CheckOldData(child_1, null);
            SupportViewDataChecker.CheckNewData(child_1, new EventBean[] { tradeBeans[0] });
    
            // Check the data of the ultimate receiver
            SupportViewDataChecker.CheckOldData(_ultimateChildView, null);
            SupportViewDataChecker.CheckNewData(_ultimateChildView, new EventBean[] {tradeBeans[0]});
        }
    
        [Test]
        public void TestUpdate()
        {
            // Set up a feed for the view under test - it will have a depth of 3 trades
            var stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 3);
            stream.AddView(_myGroupByView);
    
            // Send old a new events
            var newEvents = new EventBean[] { MakeTradeBean("IBM", 70), MakeTradeBean("GE", 10) };
            var oldEvents = new EventBean[] { MakeTradeBean("IBM", 65), MakeTradeBean("GE", 9) };
            _myGroupByView.Update(newEvents, oldEvents);
    
            Assert.AreEqual(2, SupportBeanClassView.Instances.Count);
            var child_1 = SupportBeanClassView.Instances[0];
            var child_2 = SupportBeanClassView.Instances[1];
            SupportViewDataChecker.CheckOldData(child_1, new EventBean[] { oldEvents[0] });
            SupportViewDataChecker.CheckNewData(child_1, new EventBean[] { newEvents[0] });
            SupportViewDataChecker.CheckOldData(child_2, new EventBean[] { oldEvents[1] });
            SupportViewDataChecker.CheckNewData(child_2, new EventBean[] { newEvents[1] });
    
            newEvents = new EventBean[] { MakeTradeBean("IBM", 71), MakeTradeBean("GE", 11) };
            oldEvents = new EventBean[] { MakeTradeBean("IBM", 70), MakeTradeBean("GE", 10) };
            _myGroupByView.Update(newEvents, oldEvents);
    
            SupportViewDataChecker.CheckOldData(child_1, new EventBean[] { oldEvents[0] });
            SupportViewDataChecker.CheckNewData(child_1, new EventBean[] { newEvents[0] });
            SupportViewDataChecker.CheckOldData(child_2, new EventBean[] { oldEvents[1] });
            SupportViewDataChecker.CheckNewData(child_2, new EventBean[] { newEvents[1] });
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                _myGroupByView.GetEnumerator();
                Assert.IsTrue(false);
            }
            catch (UnsupportedOperationException)
            {
                // Expected exception
            }
        }
    
        [Test]
        public void TestMakeSubviews()
        {
            EventStream eventStream = new SupportStreamImpl(typeof(SupportMarketDataBean), 4);
            var expressions = SupportExprNodeFactory.MakeIdentNodesMD("Symbol");
            GroupByView groupView = new GroupByViewImpl(_agentInstanceContext, expressions, ExprNodeUtility.GetEvaluators(expressions));
            eventStream.AddView(groupView);
    
            var groupByValue = new Object[] {"IBM"};
    
            // Invalid for no child nodes
            try
            {
                GroupByViewImpl.MakeSubViews(groupView, "Symbol".Split(','), groupByValue, _agentInstanceContext);
                Assert.IsTrue(false);
            }
            catch (EPException)
            {
                // Expected exception
            }
    
            // Invalid for child node is a merge node - doesn't make sense to group and merge only
            var mergeViewOne = new MergeView(_agentInstanceContext, SupportExprNodeFactory.MakeIdentNodesMD("Symbol"), null, false);
            groupView.AddView(mergeViewOne);
            try
            {
                GroupByViewImpl.MakeSubViews(groupView, "Symbol".Split(','), groupByValue, _agentInstanceContext);
                Assert.IsTrue(false);
            }
            catch (EPException)
            {
                // Expected exception
            }
    
            // Add a size view parent of merge view
            groupView = new GroupByViewImpl(_agentInstanceContext, expressions, ExprNodeUtility.GetEvaluators(expressions));
    
            var firstElementView_1 = new FirstElementView(null);
    
            groupView.AddView(firstElementView_1);
            groupView.Parent = eventStream;
            mergeViewOne = new MergeView(_agentInstanceContext, SupportExprNodeFactory.MakeIdentNodesMD("Symbol"), null, false);
            firstElementView_1.AddView(mergeViewOne);
    
            var subViews = GroupByViewImpl.MakeSubViews(groupView, "Symbol".Split(','), groupByValue, _agentInstanceContext);
    
            Assert.IsTrue(subViews is FirstElementView);
            Assert.IsTrue(subViews != firstElementView_1);
    
            var firstEleView = (FirstElementView) subViews;
            Assert.AreEqual(1, firstEleView.Views.Length);
            Assert.IsTrue(firstEleView.Views[0] is AddPropertyValueOptionalView);

            var md = (AddPropertyValueOptionalView)firstEleView.Views[0];
            Assert.AreEqual(1, md.Views.Length);
            Assert.IsTrue(md.Views[0] == mergeViewOne);
        }
    
        private EventBean MakeTradeBean(String symbol, int price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
