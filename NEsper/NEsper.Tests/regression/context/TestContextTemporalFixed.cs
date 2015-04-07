///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextTemporalFixed
    {

        private EPServiceProvider _epService;
        private EPServiceProviderSPI _spi;

        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _spi = (EPServiceProviderSPI)_epService;
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestContextPartitionSelection()
        {
            String[] fields = "c0,c1,c2,c3".Split(',');
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL("create context MyCtx as start SupportBean_S0 s0 end SupportBean_S1(Id=s0.Id)");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, context.s0.P00 as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean.win:keepall() group by TheString");

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 101));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            Object[][] expected = new Object[][]
                                      {
                                          new Object[] {0, "S0_1", "E1", 6}, 
                                          new Object[] {0, "S0_1", "E2", 10}, 
                                          new Object[] {0, "S0_1", "E3", 201}
                                      };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expected);

            // test iterator targeted by context partition id
            SupportSelectorById selectorById = new SupportSelectorById(0.AsSingleton());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selectorById), stmt.GetSafeEnumerator(selectorById), fields, expected);

            // test iterator targeted by property on triggering event
            SupportSelectorFilteredInitTerm filtered = new SupportSelectorFilteredInitTerm("S0_1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, expected);
            filtered = new SupportSelectorFilteredInitTerm("S0_2");
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());

            // test always-false filter - compare context partition info
            filtered = new SupportSelectorFilteredInitTerm(null);
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] { 1000L }, filtered.ContextsStartTimes);
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] { "S0_1" }, filtered.P00PropertyValues);

            try
            {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented
                {
                    ProcPartitionKeys = () => null
                });
                Assert.Fail();
            }
            catch (InvalidContextPartitionSelector ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById] interfaces but received com."), "message: " + ex.Message);
            }
        }

        [Test]
        public void TestFilterStartedFilterEndedCorrelatedOutputSnapshot()
        {
            _epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "start SupportBean_S0 as s0 " +
                    "end SupportBean_S1(p10 = s0.P00) as s1");

            String[] fields = "c1,c2,c3".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.Id as c1, context.s1.Id as c2, sum(IntPrimitive) as c3 " +
                    "from SupportBean.win:keepall() output snapshot when terminated");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "G1"));    // starts it
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "GX"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "G1"));  // terminate
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { 100, 200, 5 });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "G2"));    // starts new one
            _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "G3"));    // ignored

            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));

            _epService.EPRuntime.SendEvent(new SupportBean_S1(201, "G2"));  // terminate
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { 101, 201, 15 });
        }

        [Test]
        public void TestFilterStartedPatternEndedCorrelated()
        {
            _epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "start SupportBean_S0 as s0 " +
                    "end pattern [SupportBean_S1(p10 = s0.P00)]");

            String[] fields = "c1,c2".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.P00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean.win:keepall()");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "G1"));    // starts it
            Assert.IsFalse(listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "G1", 2 });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "GX"));  // false terminate
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "G1", 5 });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "G1"));  // actual terminate
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "G2"));    // starts second

            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "G2", 6 });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(101, null));    // false terminate
            _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "GY"));    // false terminate

            _epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "G2", 13 });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(300, "G2"));  // actual terminate
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            Assert.IsFalse(listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "G3"));    // starts third
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "G3"));    // terminate third

            _epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            Assert.IsFalse(listener.IsInvoked);
        }

        [Test]
        public void TestStartAfterEndAfter()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as start after 5 sec end after 10 sec");

            String[] fields = "c1,c2,c3".Split(',');
            String[] fieldsShort = "c3".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.startTime as c1, context.endTime as c2, sum(IntPrimitive) as c3 " +
                    "from SupportBean.win:keepall()");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimeEvent("2002-05-1 8:00:05.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            DateTimeHelper.ParseDefaultMSec("2002-05-1 8:00:05.000"),
                                            DateTimeHelper.ParseDefaultMSec("2002-05-1 8:00:15.000"), 2
                                        });

            SendTimeEvent("2002-05-1 8:00:14.999");

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsShort, new Object[] { 5 });

            SendTimeEvent("2002-05-1 8:00:15.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);

            SendTimeEvent("2002-05-1 8:00:20.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{
                DateTimeHelper.ParseDefaultMSec("2002-05-1 8:00:20.000"),
                DateTimeHelper.ParseDefaultMSec("2002-05-1 8:00:30.000"), 5});

            SendTimeEvent("2002-05-1 8:00:30.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            Assert.IsFalse(listener.IsInvoked);

            // try variable
            _epService.EPAdministrator.CreateEPL("create variable int var_start = 10");
            _epService.EPAdministrator.CreateEPL("create variable int var_end = 20");
            _epService.EPAdministrator.CreateEPL("create context FrequentlyContext as start after var_start sec end after var_end sec");
        }

        [Test]
        public void TestFilterStartedFilterEndedOutputSnapshot()
        {
            _epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as start SupportBean_S0 as s0 end SupportBean_S1 as s1");

            String[] fields = "c1,c2".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.P00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean.win:keepall() output snapshot when terminated");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));    // starts it
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            // terminate
            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "S1_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_1", 5 });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(201, "S1_2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "S0_2"));    // starts it
            _epService.EPRuntime.SendEvent(new SupportBean_S1(201, "S1_3"));    // ends it
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_2", null });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(103, "S0_3"));    // starts it
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 6));           // some more data
            _epService.EPRuntime.SendEvent(new SupportBean_S0(104, "S0_4"));    // ignored
            _epService.EPRuntime.SendEvent(new SupportBean_S1(201, "S1_3"));    // ends it
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_3", 6 });

            statement.Dispose();
        }

        [Test]
        public void TestPatternStartedPatternEnded()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context EveryNowAndThen as " +
                    "start pattern [s0=SupportBean_S0 -> timer:interval(1 sec)] " +
                    "end pattern [s1=SupportBean_S1 -> timer:interval(1 sec)]");

            String[] fields = "c1,c2".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context EveryNowAndThen select context.s0.P00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean.win:keepall()");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "S0_1"));    // starts it
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimeEvent("2002-05-1 8:00:01.000"); // 1 second passes

            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_1", 4 });

            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_1", 9 });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "S0_2"));    // ignored
            SendTimeEvent("2002-05-1 8:00:03.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_1", 15 });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "S1_1"));    // ignored

            _epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_1", 22 });

            SendTimeEvent("2002-05-1 8:00:04.000"); // terminates

            _epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "S1_2"));    // ignored
            SendTimeEvent("2002-05-1 8:00:10.000");
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(103, "S0_3"));    // new instance
            SendTimeEvent("2002-05-1 8:00:11.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E10", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "S0_3", 10 });

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestContextCreateDestroy()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context EverySecond as start (*, *, *, *, *, *) end (*, *, *, *, *, *)");

            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context EverySecond select * from SupportBean");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            SendTimeEvent("2002-05-1 8:00:00.999");
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            SendTimeEvent("2002-05-1 8:00:01.000");
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            long start = DateTimeHelper.ParseDefaultMSec("2002-05-1 8:00:01.999");
            for (int i = 0; i < 10; i++)
            {
                SendTimeEvent(start);

                SendEventAndAssert(listener, false);

                start += 1;
                SendTimeEvent(start);

                SendEventAndAssert(listener, true);

                start += 999;
                SendTimeEvent(start);

                SendEventAndAssert(listener, true);

                start += 1;
                SendTimeEvent(start);

                SendEventAndAssert(listener, false);

                start += 999;
            }
        }

        [Test]
        public void TestDBHistorical()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            String[] fields = "s1.mychar".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            String stmtText = "context NineToFive select * from SupportBean_S0 as s0, sql:MyDB ['select * from mytesttable where ${Id} = mytesttable.mybigint'] as s1";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-1 9:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "Y" });

            // now gone
            SendTimeEvent("2002-05-1 17:00:00.000");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-2 9:00:00.000");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "X" });
        }

        [Test]
        public void TestPrevPriorAndAggregation()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            String[] fields = "col1,col2,col3,col4,col5".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatementSPI statement = (EPStatementSPI)_epService.EPAdministrator.CreateEPL("context NineToFive " +
                    "select prev(TheString) as col1, prevwindow(sb) as col2, prevtail(TheString) as col3, prior(1, TheString) as col4, sum(IntPrimitive) as col5 " +
                    "from SupportBean.win:keepall() as sb");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-1 9:00:00.000");
            SupportBean event1 = new SupportBean("E1", 1);
            _epService.EPRuntime.SendEvent(event1);
            Object[][] expected = new Object[][] { new Object[] { null, new SupportBean[] { event1 }, "E1", null, 1 } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);

            SupportBean event2 = new SupportBean("E2", 2);
            _epService.EPRuntime.SendEvent(event2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", new SupportBean[] { event2, event1 }, "E1", "E1", 3 });

            // now gone
            SendTimeEvent("2002-05-1 17:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, null);

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);

            // now started
            SendTimeEvent("2002-05-2 9:00:00.000");

            SupportBean event3 = new SupportBean("E3", 9);
            _epService.EPRuntime.SendEvent(event3);
            expected = new Object[][] { new Object[] { null, new SupportBean[] { event3 }, "E3", null, 9 } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, expected);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, expected);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1, 0, 3, 1);
        }

        [Test]
        public void TestJoin()
        {
            SendTimeEvent("2002-05-01 08:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            String[] fields = "col1,col2,col3,col4".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context NineToFive " +
                    "select sb.TheString as col1, sb.IntPrimitive as col2, s0.Id as col3, s0.P00 as col4 " +
                    "from SupportBean.win:keepall() as sb full outer join SupportBean_S0.win:keepall() as s0 on p00 = TheString");
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-01 09:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null, 1, "E1" });

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 5, 1, "E1" });

            // now gone
            SendTimeEvent("2002-05-01 17:00:00.000");

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-02 09:00:00.000");

            SendTimeEvent("2002-05-01 09:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 4, null, null });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 4, 2, "E1" });
        }

        [Test]
        public void TestPatternWithTime()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement statement = _epService.EPAdministrator.CreateEPL("context NineToFive select * from pattern[every timer:interval(10 sec)]");
            statement.Events += listener.Update;
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);   // from the context

            // now started
            SendTimeEvent("2002-05-1 9:00:00.000");
            Assert.AreEqual(2, _spi.SchedulingService.ScheduleHandleCount);   // context + pattern
            Assert.IsFalse(listener.IsInvoked);

            SendTimeEvent("2002-05-1 9:00:10.000");
            Assert.IsTrue(listener.IsInvoked);

            // now gone
            SendTimeEvent("2002-05-1 17:00:00.000");
            listener.Reset();   // it is not well defined whether the listener does get fired or not
            Assert.AreEqual(1, _spi.SchedulingService.ScheduleHandleCount);   // from the context

            // now started
            SendTimeEvent("2002-05-2 9:00:00.000");
            Assert.AreEqual(2, _spi.SchedulingService.ScheduleHandleCount);   // context + pattern
            Assert.IsFalse(listener.IsInvoked);

            SendTimeEvent("2002-05-2 9:00:10.000");
            Assert.IsTrue(listener.IsInvoked);
        }

        [Test]
        public void TestSubselect()
        {
            FilterServiceSPI filterSPI = (FilterServiceSPI)_spi.FilterService;

            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            String[] fields = "TheString,col".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatementSPI statement = (EPStatementSPI)_epService.EPAdministrator.CreateEPL("context NineToFive select TheString, (select p00 from SupportBean_S0.std:lastevent()) as col from SupportBean");
            statement.Events += listener.Update;
            Assert.AreEqual(0, filterSPI.FilterCountApprox);   // from the context

            // now started
            SendTimeEvent("2002-05-1 9:00:00.000");
            Assert.AreEqual(2, filterSPI.FilterCountApprox);   // from the context

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", null });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "S01"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", "S01" });

            // now gone
            SendTimeEvent("2002-05-1 17:00:00.000");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);   // from the context

            _epService.EPRuntime.SendEvent(new SupportBean("Ex", 0));
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-2 9:00:00.000");
            Assert.AreEqual(2, filterSPI.FilterCountApprox);   // from the context
            Assert.IsFalse(listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E3", null });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(12, "S02"));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E4", "S02" });
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 1, 1, 0, 0);

            // now gone
            SendTimeEvent("2002-05-2 17:00:00.000");
            Assert.AreEqual(0, filterSPI.FilterCountApprox);   // from the context

            _epService.EPRuntime.SendEvent(new SupportBean("Ey", 0));
            Assert.IsFalse(listener.IsInvoked);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
        }

        [Test]
        public void TestNWSameContextOnExpr()
        {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("MakeBean", GetType().FullName, "SingleRowPluginMakeBean");
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            // no started yet
            String[] fields = "TheString,IntPrimitive".Split(',');
            SupportUpdateListener listener = new SupportUpdateListener();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("context NineToFive create window MyWindow.win:keepall() as SupportBean");
            stmt.Events += listener.Update;

            _epService.EPAdministrator.CreateEPL("context NineToFive insert into MyWindow select * from SupportBean");

            _epService.EPAdministrator.CreateEPL("context NineToFive " +
                    "on SupportBean_S0 s0 merge MyWindow mw where mw.TheString = s0.P00 " +
                    "when matched then Update set IntPrimitive = s0.Id " +
                    "when not matched then insert select MakeBean(Id, p00)");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);

            // now started
            SendTimeEvent("2002-05-1 9:00:00.000");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 1 });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 2 });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E1"));
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new Object[] { "E1", 3 });
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new Object[] { "E1", 1 });
            listener.Reset();

            // now gone
            SendTimeEvent("2002-05-1 17:00:00.000");

            // no longer updated
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.IsInvoked);

            // now started again but empty
            SendTimeEvent("2002-05-2 9:00:00.000");

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 1 });
        }

        [Test]
        public void TestNWFireAndForget()
        {
            SendTimeEvent("2002-05-1 8:00:00.000");
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");

            // no started yet
            _epService.EPAdministrator.CreateEPL("context NineToFive create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("context NineToFive insert into MyWindow select * from SupportBean");

            // not queryable
            TryInvalidNWQuery();

            // now started
            SendTimeEvent("2002-05-1 9:00:00.000");
            TryNWQuery(0);

            // now not empty
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(1, _epService.EPRuntime.ExecuteQuery("select * from MyWindow").Array.Length);

            // now gone
            SendTimeEvent("2002-05-1 17:00:00.000");

            // no longer queryable
            TryInvalidNWQuery();
            _epService.EPRuntime.SendEvent(new SupportBean());

            // now started again but empty
            SendTimeEvent("2002-05-2 9:00:00.000");
            TryNWQuery(0);

            // fill some data
            _epService.EPRuntime.SendEvent(new SupportBean());
            _epService.EPRuntime.SendEvent(new SupportBean());
            SendTimeEvent("2002-05-2 9:10:00.000");
            TryNWQuery(2);
        }

        private void TryInvalidNWQuery()
        {
            try
            {
                _epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            }
            catch (EPException ex)
            {
                String expected = "Error executing statement: Named window 'MyWindow' is associated to context 'NineToFive' that is not available for querying without context partition selector, use the ExecuteQuery(epl, selector) method instead [select * from MyWindow]";
                Assert.AreEqual(expected, ex.Message);
            }
        }

        private void TryNWQuery(int numRows)
        {
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            Assert.AreEqual(numRows, result.Array.Length);
        }

        [Test]
        public void TestStartTurnedOff()
        {
            SendTimeEvent("2002-05-01 08:00:00.000");
            String contextEPL = "@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
            EPStatement stmtContext = _epService.EPAdministrator.CreateEPL("@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
            AssertContextEventType(stmtContext.EventType);
            SupportUpdateListener contextListener = new SupportUpdateListener();
            stmtContext.Events += contextListener.Update;
            stmtContext.Subscriber = new MiniSubscriber();

            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context NineToFive " +
                    "select * from SupportBean");
            stmtOne.Events += new SupportUpdateListener().Update;

            SendTimeAndAssert("2002-05-01 08:59:30.000", false, 1);
            SendTimeAndAssert("2002-05-01 08:59:59.999", false, 1);
            SendTimeAndAssert("2002-05-01 09:00:00.000", true, 1);

            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('B') context NineToFive select * from SupportBean");
            stmtTwo.Events += new SupportUpdateListener().Update;

            SendTimeAndAssert("2002-05-01 16:59:59.000", true, 2);
            SendTimeAndAssert("2002-05-01 17:00:00.000", false, 2);

            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL("@Name('C') context NineToFive select * from SupportBean");
            stmtThree.Events += new SupportUpdateListener().Update;

            SendTimeAndAssert("2002-05-02 08:59:59.999", false, 3);
            SendTimeAndAssert("2002-05-02 09:00:00.000", true, 3);
            SendTimeAndAssert("2002-05-02 16:59:59.000", true, 3);
            SendTimeAndAssert("2002-05-02 17:00:00.000", false, 3);

            Assert.IsFalse(contextListener.IsInvoked);

            _epService.EPAdministrator.DestroyAllStatements();

            // test SODA
            SendTimeEvent("2002-05-03 16:59:59.000");
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(contextEPL);
            Assert.AreEqual(contextEPL, model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(contextEPL, stmt.Text);

            // test built-in properties
            EPStatement stmtLast = _epService.EPAdministrator.CreateEPL("@Name('A') context NineToFive " +
                    "select context.name as c1, context.startTime as c2, context.endTime as c3, TheString as c4 from SupportBean");
            SupportUpdateListener listener = new SupportUpdateListener();
            stmtLast.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("NineToFive", theEvent.Get("c1"));
            Assert.AreEqual("2002-05-03 16:59:59.000", theEvent.Get("c2").AsLong().Print());
            Assert.AreEqual("2002-05-03 17:00:00.000", theEvent.Get("c3").AsLong().Print());
            Assert.AreEqual("E1", theEvent.Get("c4"));
        }

        [Test]
        public void TestStartTurnedOn()
        {

            ContextManagementService ctxMgmtService = _spi.ContextManagementService;
            Assert.AreEqual(0, ctxMgmtService.ContextCount);

            SendTimeEvent("2002-05-1 9:15:00.000");
            EPStatement stmtContext = _epService.EPAdministrator.CreateEPL("@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
            Assert.AreEqual(1, ctxMgmtService.ContextCount);

            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("@Name('A') context NineToFive select * from SupportBean");
            stmtOne.Events += new SupportUpdateListener().Update;

            SendTimeAndAssert("2002-05-1 9:16:00.000", true, 1);
            SendTimeAndAssert("2002-05-1 16:59:59.000", true, 1);
            SendTimeAndAssert("2002-05-1 17:00:00.000", false, 1);

            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('B') context NineToFive select * from SupportBean");
            stmtTwo.Events += new SupportUpdateListener().Update;

            SendTimeAndAssert("2002-05-2 8:59:59.999", false, 2);
            SendTimeAndAssert("2002-05-2 9:15:00.000", true, 2);
            SendTimeAndAssert("2002-05-2 16:59:59.000", true, 2);
            SendTimeAndAssert("2002-05-2 17:00:00.000", false, 2);

            // destroy context before stmts
            stmtContext.Dispose();
            Assert.AreEqual(1, ctxMgmtService.ContextCount);

            stmtTwo.Dispose();
            stmtOne.Dispose();

            // context gone too
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
        }

        private void AssertContextEventType(EventType eventType)
        {
            Assert.AreEqual(0, eventType.PropertyNames.Length);
            Assert.AreEqual("anonymous_EventType_Context_NineToFive", eventType.Name);
        }

        private void SendTimeAndAssert(String time, bool isInvoked, int countStatements)
        {
            SendTimeEvent(time);
            _epService.EPRuntime.SendEvent(new SupportBean());

            IList<string> statements = _epService.EPAdministrator.StatementNames;
            Assert.AreEqual(countStatements + 1, statements.Count);

            for (int i = 0; i < statements.Count; i++)
            {
                EPStatement stmt = _epService.EPAdministrator.GetStatement(statements[i]);
                if (stmt.Name.Equals("context"))
                {
                    continue;
                }
                //SupportUpdateListener listener = (SupportUpdateListener) stmt.UpdateListeners.Next();
                //Assert.AreEqual(isInvoked, listener.GetAndClearIsInvoked(), "Failed for statement " + stmt.Name);
            }
        }

        private void SendTimeEvent(String time)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time)));
        }

        private void SendTimeEvent(long time)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(time));
        }

        private void SendEventAndAssert(SupportUpdateListener listener, bool expected)
        {
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(expected, listener.IsInvoked);
            listener.Reset();
        }

        public static SupportBean SingleRowPluginMakeBean(int id, String p00)
        {
            return new SupportBean(p00, id);
        }

        public class MiniSubscriber
        {
            public static void Update()
            {
                // no action
            }
        }
    }
}
