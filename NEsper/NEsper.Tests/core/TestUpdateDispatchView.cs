///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;
using com.espertech.esper.dispatch;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.supportunit.core;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.core
{
    [TestFixture]
    public class TestUpdateDispatchView 
    {
        private UpdateDispatchViewBlockingWait _updateDispatchView;
        private SupportUpdateListener _listenerOne;
        private SupportUpdateListener _listenerTwo;
        private DispatchService _dispatchService;
        private StatementResultServiceImpl _statementResultService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            MetricReportingPath.IsMetricsEnabled = false;

            _listenerOne = new SupportUpdateListener();
            _listenerTwo = new SupportUpdateListener();
    
            var listenerSet = new EPStatementListenerSet();
            listenerSet.Events.Add(_listenerOne.Update);
            listenerSet.Events.Add(_listenerTwo.Update);
    
            _dispatchService = new DispatchServiceImpl(
                _container.ThreadLocalManager());

            _statementResultService = new StatementResultServiceImpl(
                "name", null, null,
                new ThreadingServiceImpl(new ConfigurationEngineDefaults.ThreadingConfig()),
                _container.ThreadLocalManager());
            _statementResultService.SetUpdateListeners(listenerSet, false);
            _statementResultService.SetSelectClause(
                new Type[1], new String[1], false, 
                new ExprEvaluator[1], new SupportExprEvaluatorContext(_container, null));
            _statementResultService.SetContext(new SupportEPStatementSPI(), null, false, false, false, false, null);

            _updateDispatchView = new UpdateDispatchViewBlockingWait(
                _statementResultService, _dispatchService, 1000, _container.ThreadLocalManager());
        }
    
        [Test]
        public void TestUpdateOnceAndDispatch()
        {
            EventBean[] oldData = MakeEvents("old");
            EventBean[] newData = MakeEvents("new");
            _updateDispatchView.NewResult(new UniformPair<EventBean[]>(newData, oldData));
    
            Assert.IsFalse(_listenerOne.IsInvoked || _listenerTwo.IsInvoked);
            _dispatchService.Dispatch();
            Assert.IsTrue(_listenerOne.IsInvoked && _listenerTwo.IsInvoked);
            Assert.IsTrue(_listenerOne.LastNewData[0] == newData[0]);
            Assert.IsTrue(_listenerTwo.LastOldData[0] == oldData[0]);
        }
    
        [Test]
        public void TestUpdateTwiceAndDispatch()
        {
            EventBean[] oldDataOne = MakeEvents("old1");
            EventBean[] newDataOne = MakeEvents("new1");
            _updateDispatchView.NewResult(new UniformPair<EventBean[]>(newDataOne, oldDataOne));
    
            EventBean[] oldDataTwo = MakeEvents("old2");
            EventBean[] newDataTwo = MakeEvents("new2");
            _updateDispatchView.NewResult(new UniformPair<EventBean[]>(newDataTwo, oldDataTwo));
    
            Assert.IsFalse(_listenerOne.IsInvoked || _listenerTwo.IsInvoked);
            _dispatchService.Dispatch();
            Assert.IsTrue(_listenerOne.IsInvoked && _listenerTwo.IsInvoked);
            Assert.IsTrue(_listenerOne.LastNewData[1] == newDataTwo[0]);
            Assert.IsTrue(_listenerTwo.LastOldData[1] == oldDataTwo[0]);
        }
    
        private static EventBean[] MakeEvents(String text)
        {
            return new[] { SupportEventBeanFactory.CreateObject(text) };
        }
    }
}
