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
using com.espertech.esper.compat;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitSimple : RegressionExecution
    {
        private const string JOIN_KEY = "KEY";
        private const string CATEGORY = "Un-aggregated and Un-grouped";

        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
            configuration.AddEventType<SupportBean>();
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertion1NoneNoHavingNoJoin(epService);
            RunAssertion2NoneNoHavingJoin(epService);
            RunAssertion3NoneHavingNoJoin(epService);
            RunAssertion4NoneHavingJoin(epService);
            RunAssertion5DefaultNoHavingNoJoin(epService);
            RunAssertion6DefaultNoHavingJoin(epService);
            RunAssertion7DefaultHavingNoJoin(epService);
            RunAssertion8DefaultHavingJoin(epService);
            RunAssertion9AllNoHavingNoJoin(epService);
            RunAssertion9AllNoHavingNoJoinHinted(epService);
            RunAssertion10AllNoHavingJoin(epService);
            RunAssertion10AllNoHavingJoinHinted(epService);
            RunAssertion11AllHavingNoJoin(epService);
            RunAssertion11AllHavingNoJoinHinted(epService);
            RunAssertion12AllHavingJoin(epService);
            RunAssertion12AllHavingJoinHinted(epService);
            RunAssertion13LastNoHavingNoJoin(epService);
            RunAssertion14LastNoHavingJoin(epService);
            RunAssertion15LastHavingNoJoin(epService);
            RunAssertion16LastHavingJoin(epService);
            RunAssertion17FirstNoHavingNoJoinIStream(epService);
            RunAssertion17FirstNoHavingJoinIStream(epService);
            RunAssertion17FirstNoHavingNoJoinIRStream(epService);
            RunAssertion17FirstNoHavingJoinIRStream(epService);
            RunAssertion18SnapshotNoHavingNoJoin(epService);
            RunAssertionOutputFirstUnidirectionalJoinNamedWindow(epService);
            RunAssertionOutputEveryTimePeriod(epService);
            RunAssertionOutputEveryTimePeriodVariable(epService);
            RunAssertionAggAllHaving(epService);
            RunAssertionAggAllHavingJoin(epService);
            RunAssertionIterator(epService);
            RunAssertionLimitEventJoin(epService);
            RunAssertionLimitTime(epService);
            RunAssertionTimeBatchOutputEvents(epService);
            RunAssertionSimpleNoJoinAll(epService);
            RunAssertionSimpleNoJoinLast(epService);
            RunAssertionSimpleJoinAll(epService);
            RunAssertionSimpleJoinLast(epService);
            RunAssertionLimitEventSimple(epService);
            RunAssertionLimitSnapshot(epService);
            RunAssertionFirstSimpleHavingAndNoHaving(epService);
            RunAssertionLimitSnapshotJoin(epService);
            RunAssertionSnapshotMonthScoped(epService);
            RunAssertionFirstMonthScoped(epService);
        }

        private void RunAssertion1NoneNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec)";
            TryAssertion12(epService, stmtText, "none");
        }

        private void RunAssertion2NoneNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol";
            TryAssertion12(epService, stmtText, "none");
        }

        private void RunAssertion3NoneHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           " having price > 10";
            TryAssertion34(epService, stmtText, "none");
        }

        private void RunAssertion4NoneHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           " having price > 10";
            TryAssertion34(epService, stmtText, "none");
        }

        private void RunAssertion5DefaultNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }

        private void RunAssertion6DefaultNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }

        private void RunAssertion7DefaultHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) \n" +
                           "having price > 10" +
                           "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }

        private void RunAssertion8DefaultHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "having price > 10" +
                           "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }

        private void RunAssertion9AllNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }

        private void RunAssertion9AllNoHavingNoJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }

        private void RunAssertion10AllNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }

        private void RunAssertion10AllNoHavingJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }

        private void RunAssertion11AllHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "having price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }

        private void RunAssertion11AllHavingNoJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "having price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }

        private void RunAssertion12AllHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "having price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }

        private void RunAssertion12AllHavingJoinHinted(EPServiceProvider epService)
        {
            var stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "having price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }

        private void RunAssertion13LastNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec)" +
                           "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }

        private void RunAssertion14LastNoHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }

        private void RunAssertion15LastHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec)" +
                           "having price > 10 " +
                           "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }

        private void RunAssertion16LastHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "having price > 10 " +
                           "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }

        private void RunAssertion17FirstNoHavingNoJoinIStream(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "output first every 1 seconds";
            TryAssertion17IStream(epService, stmtText, "first");
        }

        private void RunAssertion17FirstNoHavingJoinIStream(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec)," +
                           "SupportBean#keepall where TheString=symbol " +
                           "output first every 1 seconds";
            TryAssertion17IStream(epService, stmtText, "first");
        }

        private void RunAssertion17FirstNoHavingNoJoinIRStream(EPServiceProvider epService)
        {
            var stmtText = "select irstream symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "output first every 1 seconds";
            TryAssertion17IRStream(epService, stmtText, "first");
        }

        private void RunAssertion17FirstNoHavingJoinIRStream(EPServiceProvider epService)
        {
            var stmtText = "select irstream symbol, volume, price " +
                           "from MarketData#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=symbol " +
                           "output first every 1 seconds";
            TryAssertion17IRStream(epService, stmtText, "first");
        }

        private void RunAssertion18SnapshotNoHavingNoJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume, price " +
                           "from MarketData#time(5.5 sec) " +
                           "output snapshot every 1 seconds";
            TryAssertion18(epService, stmtText, "first");
        }

        private void RunAssertionOutputFirstUnidirectionalJoinNamedWindow(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

            var fields = "c0,c1".Split(',');
            var epl =
                "create window MyWindow#keepall as SupportBean_S0;\n" +
                "insert into MyWindow select * from SupportBean_S0;\n" +
                "@Name('join') select myWindow.id as c0, s1.id as c1\n" +
                "from SupportBean_S1 as s1 unidirectional, MyWindow as myWindow\n" +
                "where myWindow.p00 = s1.p10\n" +
                "output first every 1 minutes;";
            var result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("join").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "a"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "b"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1000, "b"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {20, 1000});

            epService.EPRuntime.SendEvent(new SupportBean_S1(1001, "b"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1002, "a"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new CurrentTimeEvent(60 * 1000));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1003, "a"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {10, 1003});

            epService.EPRuntime.SendEvent(new SupportBean_S1(1004, "a"));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new CurrentTimeEvent(120 * 1000));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1005, "a"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {10, 1005});

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }

        private void RunAssertionOutputEveryTimePeriod(EPServiceProvider epService)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));

            var stmtText =
                "select symbol from MarketData#keepall output snapshot every 1 day 2 hours 3 minutes 4 seconds 5 milliseconds";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendMDEvent(epService, "E1", 0);

            long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
            var deltaMSec = deltaSec * 1000 + 5 + 2000;
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec - 1));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("symbol"));

            stmt.Dispose();
        }

        private void RunAssertionOutputEveryTimePeriodVariable(EPServiceProvider epService)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            epService.EPAdministrator.Configuration.AddVariable("D", typeof(int), 1);
            epService.EPAdministrator.Configuration.AddVariable("H", typeof(int), 2);
            epService.EPAdministrator.Configuration.AddVariable("M", typeof(int), 3);
            epService.EPAdministrator.Configuration.AddVariable("S", typeof(int), 4);
            epService.EPAdministrator.Configuration.AddVariable("MS", typeof(int), 5);

            var stmtText =
                "select symbol from MarketData#keepall output snapshot every D days H hours M minutes S seconds MS milliseconds";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendMDEvent(epService, "E1", 0);

            long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
            var deltaMSec = deltaSec * 1000 + 5 + 2000;
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec - 1));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("symbol"));

            // test statement model
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());

            stmt.Dispose();
        }

        private void TryAssertion34(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new[] {"symbol", "volume", "price"};

            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(1500, 1, new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(2100, 1, new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsert(4300, 1, new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultRemove(5700, 0, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(7000, 0, new[] {new object[] {"IBM", 150L, 24d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion15_16(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);

            expected.AddResultInsert(1200, 0, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(2200, 0, new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsRem(6200, 0, null, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(7200, 0, new[] {new object[] {"IBM", 150L, 24d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion12(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(800, 1, new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(1500, 1, new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(1500, 2, new[] {new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(2100, 1, new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsert(3500, 1, new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(4300, 1, new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsert(4900, 1, new[] {new object[] {"YAH", 11500L, 3d}});
            expected.AddResultRemove(5700, 0, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(5900, 1, new[] {new object[] {"YAH", 10500L, 1d}});
            expected.AddResultRemove(6300, 0, new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultRemove(
                7000, 0, new[] {new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion13_14(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(2200, 0, new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(5200, 0, new[] {new object[] {"YAH", 11500L, 3d}});
            expected.AddResultInsRem(
                6200, 0, new[] {new object[] {"YAH", 10500L, 1d}}, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(7200, 0, new[] {new object[] {"YAH", 10000L, 1d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion78(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(2200, 0, new[] {new object[] {"IBM", 150L, 24d}, new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsRem(6200, 0, null, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(7200, 0, new[] {new object[] {"IBM", 150L, 24d}});

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion56(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200, 0, new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200, 0,
                new[]
                {
                    new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}
                });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(
                5200, 0, new[] {new object[] {"IBM", 150L, 22d}, new object[] {"YAH", 11500L, 3d}});
            expected.AddResultInsRem(
                6200, 0, new[] {new object[] {"YAH", 10500L, 1d}}, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                7200, 0,
                new[]
                {
                    new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d}
                });

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void TryAssertion17IStream(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(1500, 1, new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(3500, 1, new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(4300, 1, new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsert(5900, 1, new[] {new object[] {"YAH", 10500L, 1.0d}});

            var execution = new ResultAssertExecution(
                epService, stmt, listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false);
        }

        private void TryAssertion17IRStream(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(1500, 1, new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(3500, 1, new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(4300, 1, new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultRemove(5700, 0, new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(6300, 0, new[] {new object[] {"MSFT", 5000L, 9d}});

            var execution = new ResultAssertExecution(
                epService, stmt, listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false);
        }

        private void TryAssertion18(EPServiceProvider epService, string stmtText, string outputLimit)
        {
            SendTimer(epService, 0);
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var fields = new[] {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200, 0, new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200, 0,
                new[]
                {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}
                });
            expected.AddResultInsert(
                3200, 0,
                new[]
                {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}
                });
            expected.AddResultInsert(
                4200, 0,
                new[]
                {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d}
                });
            expected.AddResultInsert(
                5200, 0,
                new[]
                {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d},
                    new object[] {"IBM", 150L, 22d}, new object[] {"YAH", 11500L, 3d}
                });
            expected.AddResultInsert(
                6200, 0,
                new[]
                {
                    new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d},
                    new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d}, new object[] {"IBM", 150L, 22d},
                    new object[] {"YAH", 11500L, 3d}, new object[] {"YAH", 10500L, 1d}
                });
            expected.AddResultInsert(
                7200, 0,
                new[]
                {
                    new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d}, new object[] {"IBM", 150L, 22d},
                    new object[] {"YAH", 11500L, 3d}, new object[] {"YAH", 10500L, 1d}
                });

            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }

        private void RunAssertionAggAllHaving(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume " +
                           "from " + typeof(SupportMarketDataBean).FullName + "#length(10) as two " +
                           "having volume > 0 " +
                           "output every 5 events";

            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new[] {"symbol", "volume"};

            SendMDEvent(epService, "S0", 20);
            SendMDEvent(epService, "IBM", -1);
            SendMDEvent(epService, "MSFT", -2);
            SendMDEvent(epService, "YAH", 10);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent(epService, "IBM", 0);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new[] {new object[] {"S0", 20L}, new object[] {"YAH", 10L}});

            stmt.Dispose();
        }

        private void RunAssertionAggAllHavingJoin(EPServiceProvider epService)
        {
            var stmtText = "select symbol, volume " +
                           "from " + typeof(SupportMarketDataBean).FullName + "#length(10) as one," +
                           typeof(SupportBean).FullName + "#length(10) as two " +
                           "where one.symbol=two.TheString " +
                           "having volume > 0 " +
                           "output every 5 events";

            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new[] {"symbol", "volume"};
            epService.EPRuntime.SendEvent(new SupportBean("S0", 0));
            epService.EPRuntime.SendEvent(new SupportBean("IBM", 0));
            epService.EPRuntime.SendEvent(new SupportBean("MSFT", 0));
            epService.EPRuntime.SendEvent(new SupportBean("YAH", 0));

            SendMDEvent(epService, "S0", 20);
            SendMDEvent(epService, "IBM", -1);
            SendMDEvent(epService, "MSFT", -2);
            SendMDEvent(epService, "YAH", 10);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent(epService, "IBM", 0);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new[] {new object[] {"S0", 20L}, new object[] {"YAH", 10L}});

            stmt.Dispose();
        }

        private void RunAssertionIterator(EPServiceProvider epService)
        {
            var fields = new[] {"symbol", "price"};
            var statementString = "select symbol, TheString, price from " +
                                  typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                                  typeof(SupportBeanString).FullName + "#length(100) as two " +
                                  "where one.symbol = two.TheString " +
                                  "output every 3 events";
            var statement = epService.EPAdministrator.CreateEPL(statementString);
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));

            // Output limit clause ignored when iterating, for both joins and no-join
            SendEvent(epService, "CAT", 50);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new object[] {"CAT", 50d}});

            SendEvent(epService, "CAT", 60);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                statement.GetEnumerator(), fields, new[] {new object[] {"CAT", 50d}, new object[] {"CAT", 60d}});

            SendEvent(epService, "IBM", 70);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                statement.GetEnumerator(), fields,
                new[] {new object[] {"CAT", 50d}, new object[] {"CAT", 60d}, new object[] {"IBM", 70d}});

            SendEvent(epService, "IBM", 90);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                statement.GetEnumerator(), fields,
                new[]
                {
                    new object[] {"CAT", 50d}, new object[] {"CAT", 60d}, new object[] {"IBM", 70d},
                    new object[] {"IBM", 90d}
                });

            statement.Dispose();
        }

        private void RunAssertionLimitEventJoin(EPServiceProvider epService)
        {
            var eventName1 = typeof(SupportBean).FullName;
            var eventName2 = typeof(SupportBean_A).FullName;
            var joinStatement =
                "select * from " +
                eventName1 + "#length(5) as event1," +
                eventName2 + "#length(5) as event2" +
                " where event1.TheString = event2.id";
            var outputStmt1 = joinStatement + " output every 1 events";
            var outputStmt3 = joinStatement + " output every 3 events";

            var fireEvery1 = epService.EPAdministrator.CreateEPL(outputStmt1);
            var fireEvery3 = epService.EPAdministrator.CreateEPL(outputStmt3);

            var updateListener1 = new SupportUpdateListener();
            fireEvery1.Events += updateListener1.Update;
            var updateListener3 = new SupportUpdateListener();
            fireEvery3.Events += updateListener3.Update;

            // send event 1
            SendJoinEvents(epService, "IBM");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
            Assert.IsNull(updateListener3.LastNewData);
            Assert.IsNull(updateListener3.LastOldData);

            // send event 2
            SendJoinEvents(epService, "MSFT");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
            Assert.IsNull(updateListener3.LastNewData);
            Assert.IsNull(updateListener3.LastOldData);

            // send event 3
            SendJoinEvents(epService, "YAH");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsTrue(updateListener3.GetAndClearIsInvoked());
            Assert.AreEqual(3, updateListener3.LastNewData.Length);
            Assert.IsNull(updateListener3.LastOldData);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionLimitTime(EPServiceProvider epService)
        {
            var eventName = typeof(SupportBean).FullName;
            var selectStatement = "select * from " + eventName + "#length(5)";

            // test integer seconds
            var statementString1 = selectStatement +
                                   " output every 3 seconds";
            TimeCallback(epService, statementString1, 3000);

            // test fractional seconds
            var statementString2 = selectStatement +
                                   " output every 3.3 seconds";
            TimeCallback(epService, statementString2, 3300);

            // test integer minutes
            var statementString3 = selectStatement +
                                   " output every 2 minutes";
            TimeCallback(epService, statementString3, 120000);

            // test fractional minutes
            var statementString4 =
                "select * from " +
                eventName + "#length(5)" +
                " output every .05 minutes";
            TimeCallback(epService, statementString4, 3000);
        }

        private void RunAssertionTimeBatchOutputEvents(EPServiceProvider epService)
        {
            var stmtText = "select * from " + typeof(SupportBean).FullName +
                           "#time_batch(10 seconds) output every 10 seconds";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendTimer(epService, 0);
            SendTimer(epService, 10000);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 20000);
            Assert.IsFalse(listener.IsInvoked);

            SendEvent(epService, "e1");
            SendTimer(epService, 30000);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 40000);
            var newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(1, newEvents.Length);
            Assert.AreEqual("e1", newEvents[0].Get("TheString"));
            listener.Reset();

            SendTimer(epService, 50000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            SendTimer(epService, 60000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            SendTimer(epService, 70000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            SendEvent(epService, "e2");
            SendEvent(epService, "e3");
            SendTimer(epService, 80000);
            newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual("e2", newEvents[0].Get("TheString"));
            Assert.AreEqual("e3", newEvents[1].Get("TheString"));

            SendTimer(epService, 90000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            stmt.Dispose();
        }

        private void RunAssertionSimpleNoJoinAll(EPServiceProvider epService)
        {
            TryAssertionSimpleNoJoinAll(epService, false);
            TryAssertionSimpleNoJoinAll(epService, true);
        }

        private void TryAssertionSimpleNoJoinAll(EPServiceProvider epService, bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt')" : "";
            var epl = hint + "select LongBoxed " +
                      "from " + typeof(SupportBean).FullName + "#length(3) " +
                      "output all every 2 events";

            TryAssertAll(epService, CreateStmtAndListenerNoJoin(epService, epl));

            epl = hint + "select LongBoxed " +
                  "from " + typeof(SupportBean).FullName + "#length(3) " +
                  "output every 2 events";

            TryAssertAll(epService, CreateStmtAndListenerNoJoin(epService, epl));

            epl = hint + "select * " +
                  "from " + typeof(SupportBean).FullName + "#length(3) " +
                  "output every 2 events";

            TryAssertAll(epService, CreateStmtAndListenerNoJoin(epService, epl));

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSimpleNoJoinLast(EPServiceProvider epService)
        {
            var epl = "select LongBoxed " +
                      "from " + typeof(SupportBean).FullName + "#length(3) " +
                      "output last every 2 events";

            TryAssertLast(epService, CreateStmtAndListenerNoJoin(epService, epl));

            epl = "select * " +
                  "from " + typeof(SupportBean).FullName + "#length(3) " +
                  "output last every 2 events";

            TryAssertLast(epService, CreateStmtAndListenerNoJoin(epService, epl));

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSimpleJoinAll(EPServiceProvider epService)
        {
            TryAssertionSimpleJoinAll(epService, false);
            TryAssertionSimpleJoinAll(epService, true);
        }

        private void TryAssertionSimpleJoinAll(EPServiceProvider epService, bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt')" : "";
            var epl = hint + "select LongBoxed  " +
                      "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                      typeof(SupportBean).FullName + "#length(3) as two " +
                      "output all every 2 events";

            TryAssertAll(epService, CreateStmtAndListenerJoin(epService, epl));
            epService.EPAdministrator.DestroyAllStatements();
        }

        private SupportUpdateListener CreateStmtAndListenerNoJoin(EPServiceProvider epService, string epl)
        {
            var updateListener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += updateListener.Update;
            return updateListener;
        }

        private void TryAssertAll(EPServiceProvider epService, SupportUpdateListener updateListener)
        {
            // send an event
            SendEvent(epService, 1);

            // check no update
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // send another event
            SendEvent(epService, 2);

            // check update, all events present
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(2, updateListener.LastNewData.Length);
            Assert.AreEqual(1L, updateListener.LastNewData[0].Get("LongBoxed"));
            Assert.AreEqual(2L, updateListener.LastNewData[1].Get("LongBoxed"));
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(EPServiceProvider epService, long longBoxed)
        {
            SendEvent(epService, longBoxed, 0, 0);
        }

        private void RunAssertionSimpleJoinLast(EPServiceProvider epService)
        {
            var epl = "select LongBoxed " +
                      "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                      typeof(SupportBean).FullName + "#length(3) as two " +
                      "output last every 2 events";

            TryAssertLast(epService, CreateStmtAndListenerJoin(epService, epl));
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionLimitEventSimple(EPServiceProvider epService)
        {
            var updateListener1 = new SupportUpdateListener();
            var updateListener2 = new SupportUpdateListener();
            var updateListener3 = new SupportUpdateListener();

            var eventName = typeof(SupportBean).FullName;
            var selectStmt = "select * from " + eventName + "#length(5)";
            var statement1 = selectStmt +
                             " output every 1 events";
            var statement2 = selectStmt +
                             " output every 2 events";
            var statement3 = selectStmt +
                             " output every 3 events";

            var rateLimitStmt1 = epService.EPAdministrator.CreateEPL(statement1);
            rateLimitStmt1.Events += updateListener1.Update;
            var rateLimitStmt2 = epService.EPAdministrator.CreateEPL(statement2);
            rateLimitStmt2.Events += updateListener2.Update;
            var rateLimitStmt3 = epService.EPAdministrator.CreateEPL(statement3);
            rateLimitStmt3.Events += updateListener3.Update;

            // send event 1
            SendEvent(epService, "IBM");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener2.GetAndClearIsInvoked());
            Assert.IsNull(updateListener2.LastNewData);
            Assert.IsNull(updateListener2.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
            Assert.IsNull(updateListener3.LastNewData);
            Assert.IsNull(updateListener3.LastOldData);

            // send event 2
            SendEvent(epService, "MSFT");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsTrue(updateListener2.GetAndClearIsInvoked());
            Assert.AreEqual(2, updateListener2.LastNewData.Length);
            Assert.IsNull(updateListener2.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());

            // send event 3
            SendEvent(epService, "YAH");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener2.GetAndClearIsInvoked());

            Assert.IsTrue(updateListener3.GetAndClearIsInvoked());
            Assert.AreEqual(3, updateListener3.LastNewData.Length);
            Assert.IsNull(updateListener3.LastOldData);
        }

        private void RunAssertionLimitSnapshot(EPServiceProvider epService)
        {
            var listener = new SupportUpdateListener();

            SendTimer(epService, 0);
            var selectStmt = "select * from " + typeof(SupportBean).FullName +
                             "#time(10) output snapshot every 3 events";

            var stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += listener.Update;

            SendTimer(epService, 1000);
            SendEvent(epService, "IBM");
            SendEvent(epService, "MSFT");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(epService, 2000);
            SendEvent(epService, "YAH");
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[] {new object[] {"IBM"}, new object[] {"MSFT"}, new object[] {"YAH"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 3000);
            SendEvent(epService, "s4");
            SendEvent(epService, "s5");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(epService, 10000);
            SendEvent(epService, "s6");
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[]
                {
                    new object[] {"IBM"}, new object[] {"MSFT"}, new object[] {"YAH"}, new object[] {"s4"},
                    new object[] {"s5"}, new object[] {"s6"}
                });
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 11000);
            SendEvent(epService, "s7");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent(epService, "s8");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent(epService, "s9");
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[]
                {
                    new object[] {"YAH"}, new object[] {"s4"}, new object[] {"s5"}, new object[] {"s6"},
                    new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}
                });
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 14000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[] {new object[] {"s6"}, new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent(epService, "s10");
            SendEvent(epService, "s11");
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(epService, 23000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"}, new[] {new object[] {"s10"}, new object[] {"s11"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent(epService, "s12");
            Assert.IsFalse(listener.IsInvoked);

            stmt.Dispose();
        }

        private void RunAssertionFirstSimpleHavingAndNoHaving(EPServiceProvider epService)
        {
            TryAssertionFirstSimpleHavingAndNoHaving(epService, "");
            TryAssertionFirstSimpleHavingAndNoHaving(epService, "having IntPrimitive != 0");
        }

        private void TryAssertionFirstSimpleHavingAndNoHaving(EPServiceProvider epService, string having)
        {
            var epl = "select TheString from SupportBean " + having + " output first every 3 events";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[] {"E1"});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.IsInvoked);

            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[] {"E4"});

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.IsInvoked);

            stmt.Dispose();
        }

        private void RunAssertionLimitSnapshotJoin(EPServiceProvider epService)
        {
            var listener = new SupportUpdateListener();

            SendTimer(epService, 0);
            var selectStmt = "select TheString from " + typeof(SupportBean).FullName + "#time(10) as s," +
                             typeof(SupportMarketDataBean).FullName +
                             "#keepall as m where s.TheString = m.symbol output snapshot every 3 events order by symbol asc";

            var stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += listener.Update;

            foreach (var symbol in "s0,s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11".Split(','))
            {
                epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, 0, 0L, ""));
            }

            SendTimer(epService, 1000);
            SendEvent(epService, "s0");
            SendEvent(epService, "s1");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(epService, 2000);
            SendEvent(epService, "s2");
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[] {new object[] {"s0"}, new object[] {"s1"}, new object[] {"s2"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 3000);
            SendEvent(epService, "s4");
            SendEvent(epService, "s5");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(epService, 10000);
            SendEvent(epService, "s6");
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[]
                {
                    new object[] {"s0"}, new object[] {"s1"}, new object[] {"s2"}, new object[] {"s4"},
                    new object[] {"s5"}, new object[] {"s6"}
                });
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 11000);
            SendEvent(epService, "s7");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent(epService, "s8");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent(epService, "s9");
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[]
                {
                    new object[] {"s2"}, new object[] {"s4"}, new object[] {"s5"}, new object[] {"s6"},
                    new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}
                });
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(epService, 14000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"},
                new[] {new object[] {"s6"}, new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent(epService, "s10");
            SendEvent(epService, "s11");
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(epService, 23000);
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, new[] {"TheString"}, new[] {new object[] {"s10"}, new object[] {"s11"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent(epService, "s12");
            Assert.IsFalse(listener.IsInvoked);

            stmt.Dispose();
        }

        private void RunAssertionSnapshotMonthScoped(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean#lastevent output snapshot every 1 month")
                .Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), "TheString".Split(','), new[] {new object[] {"E1"}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFirstMonthScoped(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean#lastevent output first every 1 month")
                .Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), "TheString".Split(','), new[] {new object[] {"E4"}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private SupportUpdateListener CreateStmtAndListenerJoin(EPServiceProvider epService, string epl)
        {
            var updateListener = new SupportUpdateListener();
            var view = epService.EPAdministrator.CreateEPL(epl);
            view.Events += updateListener.Update;
            epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
            return updateListener;
        }

        private void TryAssertLast(EPServiceProvider epService, SupportUpdateListener updateListener)
        {
            // send an event
            SendEvent(epService, 1);

            // check no update
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // send another event
            SendEvent(epService, 2);

            // check update, only the last event present
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreEqual(2L, updateListener.LastNewData[0].Get("LongBoxed"));
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendTimer(EPServiceProvider epService, long time)
        {
            var theEvent = new CurrentTimeEvent(time);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendEvent(EPServiceProvider epService, string s)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = 0.0;
            bean.IntPrimitive = 0;
            bean.IntBoxed = 0;
            epService.EPRuntime.SendEvent(bean);
        }

        private void TimeCallback(EPServiceProvider epService, string statementString, int timeToCallback)
        {
            // set the clock to 0
            var currentTime = new AtomicLong();
            SendTimeEvent(epService, 0, currentTime);

            // create the EPL statement and add a listener
            var statement = epService.EPAdministrator.CreateEPL(statementString);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
            updateListener.Reset();

            // send an event
            SendEvent(epService, "IBM");

            // check that the listener hasn't been updated
            SendTimeEvent(epService, timeToCallback - 1, currentTime);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(epService, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);

            // send another event
            SendEvent(epService, "MSFT");

            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(epService, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);

            // don't send an event
            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(epService, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.IsNull(updateListener.LastNewData);
            Assert.IsNull(updateListener.LastOldData);

            // don't send an event
            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(epService, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.IsNull(updateListener.LastNewData);
            Assert.IsNull(updateListener.LastOldData);

            // send several events
            SendEvent(epService, "YAH");
            SendEvent(epService, "s4");
            SendEvent(epService, "s5");

            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(epService, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(3, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendTimeEvent(EPServiceProvider epService, int timeIncrement, AtomicLong currentTime)
        {
            currentTime.IncrementAndGet(timeIncrement);
            var theEvent = new CurrentTimeEvent(currentTime.Get());
            epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendJoinEvents(EPServiceProvider epService, string s)
        {
            var event1 = new SupportBean();
            event1.TheString = s;
            event1.DoubleBoxed = 0.0;
            event1.IntPrimitive = 0;
            event1.IntBoxed = 0;

            var event2 = new SupportBean_A(s);

            epService.EPRuntime.SendEvent(event1);
            epService.EPRuntime.SendEvent(event2);
        }

        private void SendMDEvent(EPServiceProvider epService, string symbol, long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(EPServiceProvider epService, string symbol, double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }

        private void SendCurrentTime(EPServiceProvider epService, string time)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }

        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace