///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitGroupByEventPerGroup : RegressionExecution
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private const string CATEGORY = "Fully-Aggregated and Grouped";

        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
            configuration.AddEventType<SupportBean>();
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionLastNoDataWindow(epService);
            RunAssertionOutputFirstHavingJoinNoJoin(epService);
            RunAssertionOutputFirstCrontab(epService);
            RunAssertionOutputFirstWhenThen(epService);
            RunAssertionOutputFirstEveryNEvents(epService);
            RunAssertionWildcardEventPerGroup(epService);
            RunAssertion1NoneNoHavingNoJoin(epService);
            RunAssertion2NoneNoHavingJoin(epService);
            RunAssertion3NoneHavingNoJoin(epService);
            RunAssertion4NoneHavingJoin(epService);
            RunAssertion5DefaultNoHavingNoJoin(epService);
            RunAssertion6DefaultNoHavingJoin(epService);
            RunAssertion7DefaultHavingNoJoin(epService);
            RunAssertion8DefaultHavingJoin(epService);
            RunAssertion9AllNoHavingNoJoin(epService);
            RunAssertion10AllNoHavingJoin(epService);
            RunAssertion11AllHavingNoJoin(epService);
            RunAssertion11AllHavingNoJoinHinted(epService);
            RunAssertion12AllHavingJoin(epService);
            RunAssertion12AllHavingJoinHinted(epService);
            RunAssertion13LastNoHavingNoJoin(epService);
            RunAssertion14LastNoHavingJoin(epService);
            RunAssertion15LastHavingNoJoin(epService);
            RunAssertion15LastHavingNoJoinHinted(epService);
            RunAssertion16LastHavingJoin(epService);
            RunAssertion16LastHavingJoinHinted(epService);
            RunAssertion17FirstNoHavingNoJoin(epService);
            RunAssertion17FirstNoHavingJoin(epService);
            RunAssertion18SnapshotNoHavingNoJoin(epService);
            RunAssertion18SnapshotNoHavingJoin(epService);
            RunAssertionJoinSortWindow(epService);
            RunAssertionLimitSnapshot(epService);
            RunAssertionLimitSnapshotLimit(epService);
            RunAssertionGroupBy_All(epService);
            RunAssertionGroupBy_Default(epService);
            RunAssertionMaxTimeWindow(epService);
            RunAssertionNoJoinLast(epService);
            RunAssertionNoOutputClauseView(epService);
            RunAssertionNoOutputClauseJoin(epService);
            RunAssertionNoJoinAll(epService);
            RunAssertionJoinLast(epService);
            RunAssertionJoinAll(epService);
        }

        private void RunAssertionLastNoDataWindow(EPServiceProvider epService)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var epl =
                "select TheString, IntPrimitive as intp from SupportBean group by TheString output last every 1 seconds order by TheString asc";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));

            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), new[] {"TheString", "intp"},
                new[] {new object[] {"E1", 3}, new object[] {"E2", 21}, new object[] {"E3", 31}});

            epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 33));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));

            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), new[] {"TheString", "intp"},
                new[] {new object[] {"E1", 5}, new object[] {"E3", 33}});

            stmt.Dispose();
        }

        private void RunAssertionOutputFirstHavingJoinNoJoin(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));

            var stmtText =
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
            TryOutputFirstHaving(epService, stmtText);

            var stmtTextJoin =
                "select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.TheString " +
                "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
            TryOutputFirstHaving(epService, stmtTextJoin);

            var stmtTextOrder =
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
            TryOutputFirstHaving(epService, stmtTextOrder);

            var stmtTextOrderJoin =
                "select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.TheString " +
                "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
            TryOutputFirstHaving(epService, stmtTextOrderJoin);
        }

        private void TryOutputFirstHaving(EPServiceProvider epService, string statementText)
        {
            var fields = "TheString,value".Split(',');
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            var stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));

            SendBeanEvent(epService, "E1", 10);
            SendBeanEvent(epService, "E2", 15);
            SendBeanEvent(epService, "E1", 10);
            SendBeanEvent(epService, "E2", 5);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanEvent(epService, "E2", 5);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 25});

            SendBeanEvent(epService, "E2", -6); // to 19, does not count toward condition
            SendBeanEvent(epService, "E2", 2); // to 21, counts toward condition
            Assert.IsFalse(listener.IsInvoked);
            SendBeanEvent(epService, "E2", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 22});

            SendBeanEvent(epService, "E2", 1); // to 23, counts toward condition
            Assert.IsFalse(listener.IsInvoked);
            SendBeanEvent(epService, "E2", 1); // to 24
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 24});

            SendBeanEvent(epService, "E2", -10); // to 14
            SendBeanEvent(epService, "E2", 10); // to 24, counts toward condition
            Assert.IsFalse(listener.IsInvoked);
            SendBeanEvent(epService, "E2", 0); // to 24, counts toward condition
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 24});

            SendBeanEvent(epService, "E2", -10); // to 14
            SendBeanEvent(epService, "E2", 1); // to 15
            SendBeanEvent(epService, "E2", 5); // to 20
            SendBeanEvent(epService, "E2", 0); // to 20
            SendBeanEvent(epService, "E2", 1); // to 21    // counts
            Assert.IsFalse(listener.IsInvoked);

            SendBeanEvent(epService, "E2", 0); // to 21
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 21});

            // remove events
            SendMDEvent(epService, "E2", 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 21});

            // remove events
            SendMDEvent(epService, "E2", -10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 41});

            // remove events
            SendMDEvent(epService, "E2", -6); // since there is 3*-10 we output the next one
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 47});

            SendMDEvent(epService, "E2", 2);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionOutputFirstCrontab(EPServiceProvider epService)
        {
            SendTimer(epService, 0);
            var fields = "TheString,value".Split(',');
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first at (*/2, *, *, *, *)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendBeanEvent(epService, "E1", 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 10});

            SendTimer(epService, 2 * 60 * 1000 - 1);
            SendBeanEvent(epService, "E1", 11);
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(epService, 2 * 60 * 1000);
            SendBeanEvent(epService, "E1", 12);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 33});

            SendBeanEvent(epService, "E2", 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 20});

            SendBeanEvent(epService, "E2", 21);
            SendTimer(epService, 4 * 60 * 1000 - 1);
            SendBeanEvent(epService, "E2", 22);
            SendBeanEvent(epService, "E1", 13);
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(epService, 4 * 60 * 1000);
            SendBeanEvent(epService, "E2", 23);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 86});
            SendBeanEvent(epService, "E1", 14);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 60});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionOutputFirstWhenThen(EPServiceProvider epService)
        {
            var fields = "TheString,value".Split(',');
            epService.EPAdministrator.Configuration.AddVariable("varoutone", typeof(bool), false);
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first when varoutone then set varoutone = false");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendBeanEvent(epService, "E1", 10);
            SendBeanEvent(epService, "E1", 11);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SetVariableValue("varoutone", true);
            SendBeanEvent(epService, "E1", 12);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 33});
            Assert.AreEqual(false, epService.EPRuntime.GetVariableValue("varoutone"));

            epService.EPRuntime.SetVariableValue("varoutone", true);
            SendBeanEvent(epService, "E2", 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 20});
            Assert.AreEqual(false, epService.EPRuntime.GetVariableValue("varoutone"));

            SendBeanEvent(epService, "E1", 13);
            SendBeanEvent(epService, "E2", 21);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionOutputFirstEveryNEvents(EPServiceProvider epService)
        {
            var fields = "TheString,value".Split(',');
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every 3 events");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendBeanEvent(epService, "E1", 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 10});

            SendBeanEvent(epService, "E1", 12);
            SendBeanEvent(epService, "E1", 11);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanEvent(epService, "E1", 13);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 46});

            SendMDEvent(epService, "S1", 12);
            SendMDEvent(epService, "S1", 11);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent(epService, "S1", 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 13});

            SendBeanEvent(epService, "E1", 14);
            SendBeanEvent(epService, "E1", 15);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanEvent(epService, "E2", 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 20});

            // test variable
            epService.EPAdministrator.CreateEPL("create variable int myvar = 1");
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every myvar events");
            stmt.Events += listener.Update;

            SendBeanEvent(epService, "E3", 10);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E3", 10}});

            SendBeanEvent(epService, "E1", 5);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1", 47}});

            epService.EPRuntime.SetVariableValue("myvar", 2);

            SendBeanEvent(epService, "E1", 6);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanEvent(epService, "E1", 7);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1", 60}});

            SendBeanEvent(epService, "E1", 1);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new[] {new object[] {"E1", 62}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionWildcardEventPerGroup(EPServiceProvider epService)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select * from SupportBean group by TheString output last every 3 events order by TheString asc");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("IBM", 10));
            epService.EPRuntime.SendEvent(new SupportBean("ATT", 11));
            epService.EPRuntime.SendEvent(new SupportBean("IBM", 100));

            var events = listener.GetNewDataListFlattened();
            listener.Reset();
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual("ATT", events[0].Get("TheString"));
            Assert.AreEqual(11, events[0].Get("IntPrimitive"));
            Assert.AreEqual("IBM", events[1].Get("TheString"));
            Assert.AreEqual(100, events[1].Get("IntPrimitive"));
            stmt.Dispose();

            // All means each event
            stmt = epService.EPAdministrator.CreateEPL(
                "select * from SupportBean group by TheString output all every 3 events");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("IBM", 10));
            epService.EPRuntime.SendEvent(new SupportBean("ATT", 11));
            epService.EPRuntime.SendEvent(new SupportBean("IBM", 100));

            events = listener.GetNewDataListFlattened();
            Assert.AreEqual(3, events.Length);
            Assert.AreEqual("IBM", events[0].Get("TheString"));
            Assert.AreEqual(10, events[0].Get("IntPrimitive"));
            Assert.AreEqual("ATT", events[1].Get("TheString"));
            Assert.AreEqual(11, events[1].Get("IntPrimitive"));
            Assert.AreEqual("IBM", events[2].Get("TheString"));
            Assert.AreEqual(100, events[2].Get("IntPrimitive"));

            stmt.Dispose();
        }

        private void RunAssertion1NoneNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec)" +
                           "group by symbol " +
                           "order by symbol asc";
            TryAssertion12(epService, stmtText, "none");
        }

        private void RunAssertion2NoneNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "order by symbol asc";
            TryAssertion12(epService, stmtText, "none");
        }

        private void RunAssertion3NoneHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           " having sum(price) > 50";
            TryAssertion34(epService, stmtText, "none");
        }

        private void RunAssertion4NoneHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50";
            TryAssertion34(epService, stmtText, "none");
        }

        private void RunAssertion5DefaultNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           "output every 1 seconds order by symbol asc";
            TryAssertion56(epService, stmtText, "default");
        }

        private void RunAssertion6DefaultNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "output every 1 seconds order by symbol asc";
            TryAssertion56(epService, stmtText, "default");
        }

        private void RunAssertion7DefaultHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) \n" +
                           "group by symbol " +
                           "having sum(price) > 50" +
                           "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }

        private void RunAssertion8DefaultHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50" +
                           "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }

        private void RunAssertion9AllNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           "output all every 1 seconds " +
                           "order by symbol";
            TryAssertion9_10(epService, stmtText, "all");
        }

        private void RunAssertion10AllNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "output all every 1 seconds " +
                           "order by symbol";
            TryAssertion9_10(epService, stmtText, "all");
        }

        private void RunAssertion11AllHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }

        private void RunAssertion11AllHavingNoJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }

        private void RunAssertion12AllHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }

        private void RunAssertion12AllHavingJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }

        private void RunAssertion13LastNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec)" +
                           "group by symbol " +
                           "output last every 1 seconds " +
                           "order by symbol";
            TryAssertion13_14(epService, stmtText, "last");
        }

        private void RunAssertion14LastNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "output last every 1 seconds " +
                           "order by symbol";
            TryAssertion13_14(epService, stmtText, "last");
        }

        private void RunAssertion15LastHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec)" +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }

        private void RunAssertion15LastHavingNoJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec)" +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }

        private void RunAssertion16LastHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }

        private void RunAssertion16LastHavingJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }

        private void RunAssertion17FirstNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           "output first every 1 seconds";
            TryAssertion17(epService, stmtText, "first");
        }

        private void RunAssertion17FirstNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "output first every 1 seconds";
            TryAssertion17(epService, stmtText, "first");
        }

        private void RunAssertion18SnapshotNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec) " +
                           "group by symbol " +
                           "output snapshot every 1 seconds " +
                           "order by symbol";
            TryAssertion18(epService, stmtText, "snapshot");
        }

        private void RunAssertion18SnapshotNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, sum(price) " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "group by symbol " +
                           "output snapshot every 1 seconds " +
                           "order by symbol";
            TryAssertion18(epService, stmtText, "snapshot");
        }

        private void TryAssertion12(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(200, 1, new[] {new object[] {"IBM", 25d}}, new[] {new object[] {"IBM", null}});
            expected.AddResultInsRem(800, 1, new[] {new object[] {"MSFT", 9d}}, new[] {new object[] {"MSFT", null}});
            expected.AddResultInsRem(1500, 1, new[] {new object[] {"IBM", 49d}}, new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsRem(1500, 2, new[] {new object[] {"YAH", 1d}}, new[] {new object[] {"YAH", null}});
            expected.AddResultInsRem(2100, 1, new[] {new object[] {"IBM", 75d}}, new[] {new object[] {"IBM", 49d}});
            expected.AddResultInsRem(3500, 1, new[] {new object[] {"YAH", 3d}}, new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(4300, 1, new[] {new object[] {"IBM", 97d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(4900, 1, new[] {new object[] {"YAH", 6d}}, new[] {new object[] {"YAH", 3d}});
            expected.AddResultInsRem(5700, 0, new[] {new object[] {"IBM", 72d}}, new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(5900, 1, new[] {new object[] {"YAH", 7d}}, new[] {new object[] {"YAH", 6d}});
            expected.AddResultInsRem(6300, 0, new[] {new object[] {"MSFT", null}}, new[] {new object[] {"MSFT", 9d}});
            expected.AddResultInsRem(
                7000, 0, new[] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion34(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(2100, 1, new[] {new object[] {"IBM", 75d}}, null);
            expected.AddResultInsRem(4300, 1, new[] {new object[] {"IBM", 97d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(5700, 0, new[] {new object[] {"IBM", 72d}}, new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(7000, 0, null, new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion13_14(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200, 0, new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}},
                new[] {new object[] {"IBM", null}, new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                2200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 25d}, new object[] {"YAH", null}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, new[] {new object[] {"YAH", 3d}}, new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                5200, 0, new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                6200, 0, new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}},
                new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                7200, 0, new[] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion15_16(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new[] {new object[] {"IBM", 75d}}, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new[] {new object[] {"IBM", 97d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(6200, 0, new[] {new object[] {"IBM", 72d}}, new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(7200, 0, null, new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion78(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new[] {new object[] {"IBM", 75d}}, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new[] {new object[] {"IBM", 97d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(6200, 0, new[] {new object[] {"IBM", 72d}}, new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(7200, 0, null, new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion56(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200, 0, new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}},
                new[] {new object[] {"IBM", null}, new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                2200, 0, new[] {new object[] {"IBM", 49d}, new object[] {"IBM", 75d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 25d}, new object[] {"IBM", 49d}, new object[] {"YAH", null}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, new[] {new object[] {"YAH", 3d}}, new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                5200, 0, new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                6200, 0, new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}},
                new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                7200, 0, new[] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion9_10(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200, 0, new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}},
                new[] {new object[] {"IBM", null}, new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                2200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {"YAH", null}});
            expected.AddResultInsRem(
                3200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                4200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                5200, 0, new[] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                6200, 0, new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}},
                new[] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                7200, 0, new[] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion11_12(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new[] {new object[] {"IBM", 75d}}, null);
            expected.AddResultInsRem(3200, 0, new[] {new object[] {"IBM", 75d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(4200, 0, new[] {new object[] {"IBM", 75d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(5200, 0, new[] {new object[] {"IBM", 97d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(6200, 0, new[] {new object[] {"IBM", 72d}}, new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(7200, 0, null, new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion17(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(200, 1, new[] {new object[] {"IBM", 25d}}, new[] {new object[] {"IBM", null}});
            expected.AddResultInsRem(800, 1, new[] {new object[] {"MSFT", 9d}}, new[] {new object[] {"MSFT", null}});
            expected.AddResultInsRem(1500, 1, new[] {new object[] {"IBM", 49d}}, new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsRem(1500, 2, new[] {new object[] {"YAH", 1d}}, new[] {new object[] {"YAH", null}});
            expected.AddResultInsRem(3500, 1, new[] {new object[] {"YAH", 3d}}, new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(4300, 1, new[] {new object[] {"IBM", 97d}}, new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(4900, 1, new[] {new object[] {"YAH", 6d}}, new[] {new object[] {"YAH", 3d}});
            expected.AddResultInsRem(5700, 0, new[] {new object[] {"IBM", 72d}}, new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(5900, 1, new[] {new object[] {"YAH", 7d}}, new[] {new object[] {"YAH", 6d}});
            expected.AddResultInsRem(6300, 0, new[] {new object[] {"MSFT", null}}, new[] {new object[] {"MSFT", 9d}});
            expected.AddResultInsRem(
                7000, 0, new[] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion18(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}});
            expected.AddResultInsert(
                2200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsert(
                3200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsert(
                4200, 0, new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}});
            expected.AddResultInsert(
                5200, 0, new[] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}});
            expected.AddResultInsert(
                6200, 0, new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});
            expected.AddResultInsert(7200, 0, new[] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void RunAssertionJoinSortWindow(EPServiceProvider epService)
        {
            SendTimer(epService, 0);

            var fields = "symbol,maxVol".Split(',');
            var epl = "select irstream symbol, max(price) as maxVol" +
                      " from " + typeof(SupportMarketDataBean).FullName + "#sort(1, volume desc) as s0," +
                      typeof(SupportBean).FullName + "#keepall as s1 " +
                      "group by symbol output every 1 seconds";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));

            SendMDEvent(epService, "JOIN_KEY", 1d);
            SendMDEvent(epService, "JOIN_KEY", 2d);
            listener.Reset();

            // moves all events out of the window,
            SendTimer(epService, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            var result = listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            EPAssertionUtil.AssertPropsPerRow(
                result.First, fields, new[] {new object[] {"JOIN_KEY", 1.0}, new object[] {"JOIN_KEY", 2.0}});
            Assert.AreEqual(2, result.Second.Length);
            EPAssertionUtil.AssertPropsPerRow(
                result.Second, fields, new[] {new object[] {"JOIN_KEY", null}, new object[] {"JOIN_KEY", 1.0}});

            stmt.Dispose();
        }

        private void RunAssertionLimitSnapshot(EPServiceProvider epService)
        {
            SendTimer(epService, 0);
            var selectStmt = "select symbol, min(price) as minprice from " + typeof(SupportMarketDataBean).FullName +
                             "#time(10 seconds) group by symbol output snapshot every 1 seconds order by symbol asc";

            var stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendMDEvent(epService, "ABC", 20);

            SendTimer(epService, 500);
            SendMDEvent(epService, "IBM", 16);
            SendMDEvent(epService, "ABC", 14);
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(epService, 1000);
            var fields = new[] {"symbol", "minprice"};
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 1500);
            SendMDEvent(epService, "IBM", 18);
            SendMDEvent(epService, "MSFT", 30);

            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields,
                new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}, new object[] {"MSFT", 30d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new[] {new object[] {"IBM", 18d}, new object[] {"MSFT", 30d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 12000);
            Assert.IsTrue(listener.IsInvoked);
            Assert.IsNull(listener.LastNewData);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            stmt.Dispose();
        }

        private void RunAssertionLimitSnapshotLimit(EPServiceProvider epService)
        {
            SendTimer(epService, 0);
            var selectStmt = "select symbol, min(price) as minprice from " + typeof(SupportMarketDataBean).FullName +
                             "#time(10 seconds) as m, " +
                             typeof(SupportBean).FullName + "#keepall as s where s.TheString = m.symbol " +
                             "group by symbol output snapshot every 1 seconds order by symbol asc";

            var stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            foreach (var theString in "ABC,IBM,MSFT".Split(','))
            {
                epService.EPRuntime.SendEvent(new SupportBean(theString, 1));
            }

            SendMDEvent(epService, "ABC", 20);

            SendTimer(epService, 500);
            SendMDEvent(epService, "IBM", 16);
            SendMDEvent(epService, "ABC", 14);
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(epService, 1000);
            var fields = new[] {"symbol", "minprice"};
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 1500);
            SendMDEvent(epService, "IBM", 18);
            SendMDEvent(epService, "MSFT", 30);

            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields,
                new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}, new object[] {"MSFT", 30d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 10500);
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new[] {new object[] {"IBM", 18d}, new object[] {"MSFT", 30d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 11500);
            SendTimer(epService, 12000);
            Assert.IsTrue(listener.IsInvoked);
            Assert.IsNull(listener.LastNewData);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            stmt.Dispose();
        }

        private void RunAssertionGroupBy_All(EPServiceProvider epService)
        {
            var fields = "symbol,sum(price)".Split(',');
            var eventName = typeof(SupportMarketDataBean).FullName;
            var statementString = "select irstream symbol, sum(price) from " + eventName +
                                  "#length(5) group by symbol output all every 5 events";
            var statement = epService.EPAdministrator.CreateEPL(statementString);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;

            // send some events and check that only the most recent
            // ones are kept
            SendMDEvent(epService, "IBM", 1D);
            SendMDEvent(epService, "IBM", 2D);
            SendMDEvent(epService, "HP", 1D);
            SendMDEvent(epService, "IBM", 3D);
            SendMDEvent(epService, "MAC", 1D);

            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            var newData = updateListener.LastNewData;
            Assert.AreEqual(3, newData.Length);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                newData, fields, new[]
                {
                    new object[]{"IBM", 6d},
                    new object[]{"HP", 1d},
                    new object[]{"MAC", 1d}
                });
            var oldData = updateListener.LastOldData;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                oldData, fields, new[]
                {
                    new object[]{"IBM", null},
                    new object[]{"HP", null},
                    new object[]{"MAC", null}
                });

            statement.Dispose();
        }

        private void RunAssertionGroupBy_Default(EPServiceProvider epService)
        {
            var fields = "symbol,sum(price)".Split(',');
            var eventName = typeof(SupportMarketDataBean).FullName;
            var statementString = "select irstream symbol, sum(price) from " + eventName +
                                  "#length(5) group by symbol output every 5 events";
            var statement = epService.EPAdministrator.CreateEPL(statementString);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;

            // send some events and check that only the most recent
            // ones are kept
            SendMDEvent(epService, "IBM", 1D);
            SendMDEvent(epService, "IBM", 2D);
            SendMDEvent(epService, "HP", 1D);
            SendMDEvent(epService, "IBM", 3D);
            SendMDEvent(epService, "MAC", 1D);

            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            var newData = updateListener.LastNewData;
            var oldData = updateListener.LastOldData;
            Assert.AreEqual(5, newData.Length);
            Assert.AreEqual(5, oldData.Length);
            EPAssertionUtil.AssertPropsPerRow(
                newData, fields, new[]
                {
                    new object[]{"IBM", 1d},
                    new object[]{"IBM", 3d},
                    new object[]{"HP", 1d},
                    new object[]{"IBM", 6d},
                    new object[]{"MAC", 1d}
                });
            EPAssertionUtil.AssertPropsPerRow(
                oldData, fields, new[]
                {
                    new object[]{"IBM", null},
                    new object[]{"IBM", 1d},
                    new object[]{"HP", null},
                    new object[]{"IBM", 3d},
                    new object[]{"MAC", null}
                });

            statement.Dispose();
        }

        private void RunAssertionMaxTimeWindow(EPServiceProvider epService)
        {
            SendTimer(epService, 0);

            var fields = "symbol,maxVol".Split(',');
            var epl = "select irstream symbol, max(price) as maxVol" +
                      " from " + typeof(SupportMarketDataBean).FullName + "#time(1 sec) " +
                      "group by symbol output every 1 seconds";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendMDEvent(epService, "SYM1", 1d);
            SendMDEvent(epService, "SYM1", 2d);
            listener.Reset();

            // moves all events out of the window,
            SendTimer(epService, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            var result = listener.GetDataListsFlattened();
            Assert.AreEqual(3, result.First.Length);
            EPAssertionUtil.AssertPropsPerRow(
                result.First, fields,
                new[] {new object[] {"SYM1", 1.0}, new object[] {"SYM1", 2.0}, new object[] {"SYM1", null}});
            Assert.AreEqual(3, result.Second.Length);
            EPAssertionUtil.AssertPropsPerRow(
                result.Second, fields,
                new[] {new object[] {"SYM1", null}, new object[] {"SYM1", 1.0}, new object[] {"SYM1", 2.0}});

            stmt.Dispose();
        }

        private void RunAssertionNoJoinLast(EPServiceProvider epService)
        {
            TryAssertionNoJoinLast(epService, true);
            TryAssertionNoJoinLast(epService, false);
        }

        private void TryAssertionNoJoinLast(EPServiceProvider epService, bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            var epl = hint + "select irstream symbol," +
                      "sum(price) as mySum," +
                      "avg(price) as myAvg " +
                      "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                      "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                      "group by symbol " +
                      "output last every 2 events";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryAssertionLast(epService, stmt, listener);
            stmt.Dispose();
        }

        private void RunAssertionNoOutputClauseView(EPServiceProvider epService)
        {
            var epl = "select irstream symbol," +
                      "sum(price) as mySum," +
                      "avg(price) as myAvg " +
                      "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                      "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                      "group by symbol";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            TryAssertionSingle(epService, stmt, listener);

            stmt.Dispose();
        }

        private void RunAssertionNoOutputClauseJoin(EPServiceProvider epService)
        {
            var epl = "select irstream symbol," +
                      "sum(price) as mySum," +
                      "avg(price) as myAvg " +
                      "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                      typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                      "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                      "       and one.TheString = two.symbol " +
                      "group by symbol";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            TryAssertionSingle(epService, stmt, listener);

            stmt.Dispose();
        }

        private void RunAssertionNoJoinAll(EPServiceProvider epService)
        {
            TryAssertionNoJoinAll(epService, false);
            TryAssertionNoJoinAll(epService, true);
        }

        private void TryAssertionNoJoinAll(EPServiceProvider epService, bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            var epl = hint + "select irstream symbol," +
                      "sum(price) as mySum," +
                      "avg(price) as myAvg " +
                      "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                      "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                      "group by symbol " +
                      "output all every 2 events";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            TryAssertionAll(epService, stmt, listener);

            stmt.Dispose();
        }

        private void RunAssertionJoinLast(EPServiceProvider epService)
        {
            TryAssertionJoinLast(epService, true);
            TryAssertionJoinLast(epService, false);
        }

        private void TryAssertionJoinLast(EPServiceProvider epService, bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            var epl = hint + "select irstream symbol," +
                      "sum(price) as mySum," +
                      "avg(price) as myAvg " +
                      "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                      typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                      "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                      "       and one.TheString = two.symbol " +
                      "group by symbol " +
                      "output last every 2 events";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            TryAssertionLast(epService, stmt, listener);

            stmt.Dispose();
        }

        private void RunAssertionJoinAll(EPServiceProvider epService)
        {
            TryAssertionJoinAll(epService, false);
            TryAssertionJoinAll(epService, true);
        }

        private void TryAssertionJoinAll(EPServiceProvider epService, bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            var epl = hint + "select irstream symbol," +
                      "sum(price) as mySum," +
                      "avg(price) as myAvg " +
                      "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                      typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                      "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                      "       and one.TheString = two.symbol " +
                      "group by symbol " +
                      "output all every 2 events";

            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            TryAssertionAll(epService, stmt, listener);

            stmt.Dispose();
        }

        private void TryAssertionLast(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myAvg"));

            SendMDEvent(epService, SYMBOL_DELL, 10);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent(epService, SYMBOL_DELL, 20);
            AssertEvent(
                listener, SYMBOL_DELL,
                null, null,
                30d, 15d);
            listener.Reset();

            SendMDEvent(epService, SYMBOL_DELL, 100);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent(epService, SYMBOL_DELL, 50);
            AssertEvent(
                listener, SYMBOL_DELL,
                30d, 15d,
                170d, 170 / 3d);
        }

        private void TryAssertionSingle(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("mySum").GetBoxedType());
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("myAvg").GetBoxedType());

            SendMDEvent(epService, SYMBOL_DELL, 10);
            Assert.IsTrue(listener.IsInvoked);
            AssertEvent(
                listener, SYMBOL_DELL,
                null, null,
                10d, 10d);

            SendMDEvent(epService, SYMBOL_IBM, 20);
            Assert.IsTrue(listener.IsInvoked);
            AssertEvent(
                listener, SYMBOL_IBM,
                null, null,
                20d, 20d);
        }

        private void TryAssertionAll(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myAvg"));

            SendMDEvent(epService, SYMBOL_IBM, 70);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent(epService, SYMBOL_DELL, 10);
            AssertEvents(
                listener, SYMBOL_IBM,
                null, null,
                70d, 70d,
                SYMBOL_DELL,
                null, null,
                10d, 10d);
            listener.Reset();

            SendMDEvent(epService, SYMBOL_DELL, 20);
            Assert.IsFalse(listener.IsInvoked);


            SendMDEvent(epService, SYMBOL_DELL, 100);
            AssertEvents(
                listener, SYMBOL_IBM,
                70d, 70d,
                70d, 70d,
                SYMBOL_DELL,
                10d, 10d,
                130d, 130d / 3d);
        }

        private void AssertEvent(
            SupportUpdateListener listener, string symbol,
            double? oldSum, double? oldAvg,
            double? newSum, double? newAvg)
        {
            var oldData = listener.LastOldData;
            var newData = listener.LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
            Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
            Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");

            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }

        private void AssertEvents(
            SupportUpdateListener listener, string symbolOne,
            double? oldSumOne, double? oldAvgOne,
            double newSumOne, double newAvgOne,
            string symbolTwo,
            double? oldSumTwo, double? oldAvgTwo,
            double newSumTwo, double newAvgTwo)
        {
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.GetAndResetDataListsFlattened(),
                "mySum,myAvg".Split(','),
                new[] {new object[] {newSumOne, newAvgOne}, new object[] {newSumTwo, newAvgTwo}},
                new[] {new object[] {oldSumOne, oldAvgOne}, new object[] {oldSumTwo, oldAvgTwo}});
        }

        private void SendMDEvent(EPServiceProvider epService, string symbol, double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendBeanEvent(EPServiceProvider epService, string theString, int intPrimitive)
        {
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
        }

        private void SendTimer(EPServiceProvider epService, long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace