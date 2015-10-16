///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestTimerIntervalObserver : SupportBeanConstants
    {
        [Test]
        public void TestOp()
        {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            CaseList testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            // The wait is done when 2 seconds passed
            testCase = new EventExpressionCase("timer:interval(1999 msec)");
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            String text = "select * from pattern [timer:interval(1.999)]";
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.TimerInterval(1.999d);
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(2 sec)");
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            // 3 seconds (>2001 microseconds) passed
            testCase = new EventExpressionCase("timer:interval(2.001)");
            testCase.Add("C1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(2999 milliseconds)");
            testCase.Add("C1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(3 seconds)");
            testCase.Add("C1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(3.001 seconds)");
            testCase.Add("B2");
            testCaseList.AddTest(testCase);
    
            // Try with an all ... repeated timer every 3 seconds
            testCase = new EventExpressionCase("every timer:interval(3.001 sec)");
            testCase.Add("B2");
            testCase.Add("F1");
            testCase.Add("D3");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:interval(5000 msec)");
            testCase.Add("A2");
            testCase.Add("B3");
            testCaseList.AddTest(testCase);
    
    
            testCase = new EventExpressionCase("timer:interval(3.999 second) -> b=" + EVENT_B_CLASS);
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(4 sec) -> b=" + EVENT_B_CLASS);
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(4.001 sec) -> b=" + EVENT_B_CLASS);
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(0) -> b=" + EVENT_B_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            // Try with an followed-by as a second argument
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> timer:interval(0.001)");
            testCase.Add("C1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> timer:interval(0)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> timer:interval(1 sec)");
            testCase.Add("C1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> timer:interval(1.001)");
            testCase.Add("B2", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            // Try in a 3-way followed by
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() -> timer:interval(6.000) -> d=" + EVENT_D_CLASS);
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + "() -> timer:interval(2.001) -> d=" + EVENT_D_CLASS + "())");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + "() -> timer:interval(2.000) -> d=" + EVENT_D_CLASS + "())");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            // Try with an "or"
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() or timer:interval(1.001)");
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() or timer:interval(2.001)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(Id='B3') or timer:interval(8.500)");
            testCase.Add("D2");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(8.500) or timer:interval(7.500)");
            testCase.Add("F1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(999999 msec) or g=" + EVENT_G_CLASS);
            testCase.Add("G1", "g", events.GetEvent("G1"));
            testCaseList.AddTest(testCase);
    
            // Try with an "and"
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() and timer:interval(4000 msec)");
            testCase.Add("B2", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() and timer:interval(4001 msec)");
            testCase.Add("A2", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(9999999 msec) and b=" + EVENT_B_CLASS);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(1 msec) and b=" + EVENT_B_CLASS + "(Id=\"B2\")");
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            // Try with an "within"
            testCase = new EventExpressionCase("timer:interval(3.000) where timer:within(2.000)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:interval(3.000) where timer:within (3.000)");
            testCaseList.AddTest(testCase);
    
            // Run all tests
            PatternTestHarness util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    
        /// <summary>
        /// As of release 1.6 this no longer updates listeners when the statement is
        /// started. The reason is that the dispatch view only gets attached after a pattern
        /// started, therefore ZeroDepthEventStream looses the theEvent. There should be no use case
        /// requiring this  testCase = new EventExpressionCase("not timer:interval(5000
        /// millisecond)"); testCase.Add(EventCollection.ON_START_EVENT_ID);
        /// testCaseList.AddTest(testCase);
        /// </summary>
    
        [Test]
        public void TestIntervalSpec()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select * from pattern [timer:interval(1 minute 2 seconds)]");
    
            SupportUpdateListener testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendTimer(62*1000 - 1, epService);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendTimer(62*1000, epService);
            Assert.IsTrue(testListener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIntervalSpecVariables()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            epService.EPAdministrator.CreateEPL("create variable double M=1");
            epService.EPAdministrator.CreateEPL("create variable double S=2");
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select * from pattern [timer:interval(M minute S seconds)]");
    
            SupportUpdateListener testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendTimer(62*1000 - 1, epService);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendTimer(62*1000, epService);
            Assert.IsTrue(testListener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIntervalSpecExpression()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            epService.EPAdministrator.CreateEPL("create variable double M=1");
            epService.EPAdministrator.CreateEPL("create variable double S=2");
            EPStatement statement = epService.EPAdministrator.CreateEPL("select * from pattern [timer:interval(M*60+S seconds)]");
    
            SupportUpdateListener testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendTimer(62*1000 - 1, epService);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendTimer(62*1000, epService);
            Assert.IsTrue(testListener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIntervalSpecExpressionWithProperty()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
    
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            EPStatement statement = epService.EPAdministrator.CreateEPL("select a.TheString as id from pattern [every a=SupportBean -> timer:interval(IntPrimitive seconds)]");
    
            SupportUpdateListener testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendTimer(10000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendTimer(11999, epService);
            Assert.IsFalse(testListener.IsInvoked);
            SendTimer(12000, epService);
            Assert.AreEqual("E2", testListener.AssertOneGetNewAndReset().Get("id"));
    
            SendTimer(12999, epService);
            Assert.IsFalse(testListener.IsInvoked);
            SendTimer(13000, epService);
            Assert.AreEqual("E1", testListener.AssertOneGetNewAndReset().Get("id"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIntervalSpecExpressionWithPropertyArray()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
    
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            EPStatement statement = epService.EPAdministrator.CreateEPL("select a[0].TheString as a0id, a[1].TheString as a1id from pattern [ [2] a=SupportBean -> timer:interval(a[0].IntPrimitive+a[1].IntPrimitive seconds)]");
    
            SupportUpdateListener testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendTimer(10000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendTimer(14999, epService);
            Assert.IsFalse(testListener.IsInvoked);
            SendTimer(15000, epService);
            EPAssertionUtil.AssertProps(testListener.AssertOneGetNewAndReset(), "a0id,a1id".Split(','), "E1,E2".Split(','));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIntervalSpecPreparedStmt()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(
                    "select * from pattern [timer:interval(? minute ? seconds)]");
    
            prepared.SetObject(1, 1);
            prepared.SetObject(2, 2);
            EPStatement statement = epService.EPAdministrator.Create(prepared);
    
            SupportUpdateListener testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendTimer(62*1000 - 1, epService);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendTimer(62*1000, epService);
            Assert.IsTrue(testListener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestMonthScoped()
        {
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SupportUpdateListener listener = new SupportUpdateListener();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName);}

            SendCurrentTime(epService, "2002-02-01T9:00:00.000");
            epService.EPAdministrator.CreateEPL("select * from pattern [timer:interval(1 month)]").Events += listener.Update;

            SendCurrentTimeWithMinus(epService, "2002-03-01T9:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);

            SendCurrentTime(epService, "2002-03-01T9:00:00.000");
            Assert.IsTrue(listener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }

        private void SendTimer(long timeInMSec, EPServiceProvider epService)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendCurrentTimeWithMinus(EPServiceProvider epService, String time, long minus)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }

        private void SendCurrentTime(EPServiceProvider epService, String time)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    }
}
