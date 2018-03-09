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
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitAggregateAll : RegressionExecution
    {
        private static readonly string EVENT_NAME = typeof(SupportMarketDataBean).FullName;
        private const string JOIN_KEY = "KEY";
        private const string CATEGORY = "Aggregated and Un-grouped";
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
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
            RunAssertion13LastNoHavingNoJoinHinted(epService);
            RunAssertion14LastNoHavingJoin(epService);
            RunAssertion14LastNoHavingJoinHinted(epService);
            RunAssertion15LastHavingNoJoin(epService);
            RunAssertion15LastHavingNoJoinHinted(epService);
            RunAssertion16LastHavingJoin(epService);
            RunAssertion16LastHavingJoinHinted(epService);
            RunAssertion17FirstNoHavingNoJoinIStreamOnly(epService);
            RunAssertion17FirstNoHavingNoJoinIRStream(epService);
            RunAssertion18SnapshotNoHavingNoJoin(epService);
            RunAssertionHaving(epService);
            RunAssertionHavingJoin(epService);
            RunAssertionMaxTimeWindow(epService);
            RunAssertionLimitSnapshot(epService);
            RunAssertionLimitSnapshotJoin(epService);
            RunAssertionJoinSortWindow(epService);
            RunAssertionAggregateAllNoJoinLast(epService);
            RunAssertionAggregateAllJoinAll(epService);
            RunAssertionAggregateAllJoinLast(epService);
            RunAssertionTime(epService);
            RunAssertionCount(epService);
        }
    
        private void RunAssertion1NoneNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)";
            TryAssertion12(epService, stmtText, "none");
        }
    
        private void RunAssertion2NoneNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol";
            TryAssertion12(epService, stmtText, "none");
        }
    
        private void RunAssertion3NoneHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    " having sum(price) > 100";
            TryAssertion34(epService, stmtText, "none");
        }
    
        private void RunAssertion4NoneHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    " having sum(price) > 100";
            TryAssertion34(epService, stmtText, "none");
        }
    
        private void RunAssertion5DefaultNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }
    
        private void RunAssertion6DefaultNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }
    
        private void RunAssertion7DefaultHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) \n" +
                    "having sum(price) > 100" +
                    "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }
    
        private void RunAssertion8DefaultHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100" +
                    "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }
    
        private void RunAssertion9AllNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion9AllNoHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion10AllNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion10AllNoHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion11AllHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion11AllHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion12AllHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion12AllHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion13LastNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion13LastNoHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion14LastNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion14LastNoHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion15LastHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion15LastHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion16LastHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion16LastHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion17FirstNoHavingNoJoinIStreamOnly(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output first every 1 seconds";
            TryAssertion17IStreamOnly(epService, stmtText, "first");
        }
    
        private void RunAssertion17FirstNoHavingNoJoinIRStream(EPServiceProvider epService) {
            string stmtText = "select irstream symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output first every 1 seconds";
            TryAssertion17IRStream(epService, stmtText, "first");
        }
    
        private void RunAssertion18SnapshotNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output snapshot every 1 seconds";
            TryAssertion18(epService, stmtText, "first");
        }
    
        private void TryAssertion12(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][]{new object[] {"IBM", 25d}});
            expected.AddResultInsert(800, 1, new object[][]{new object[] {"MSFT", 34d}});
            expected.AddResultInsert(1500, 1, new object[][]{new object[] {"IBM", 58d}});
            expected.AddResultInsert(1500, 2, new object[][]{new object[] {"YAH", 59d}});
            expected.AddResultInsert(2100, 1, new object[][]{new object[] {"IBM", 85d}});
            expected.AddResultInsert(3500, 1, new object[][]{new object[] {"YAH", 87d}});
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 109d}});
            expected.AddResultInsert(4900, 1, new object[][]{new object[] {"YAH", 112d}});
            expected.AddResultRemove(5700, 0, new object[][]{new object[] {"IBM", 87d}});
            expected.AddResultInsert(5900, 1, new object[][]{new object[] {"YAH", 88d}});
            expected.AddResultRemove(6300, 0, new object[][]{new object[] {"MSFT", 79d}});
            expected.AddResultRemove(7000, 0, new object[][]{new object[] {"IBM", 54d}, new object[] {"YAH", 54d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion34(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 109d}});
            expected.AddResultInsert(4900, 1, new object[][]{new object[] {"YAH", 112d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion13_14(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"MSFT", 34d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 85d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"YAH", 87d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"YAH", 112d}});
            expected.AddResultInsRem(6200, 0, new object[][]{new object[] {"YAH", 88d}}, new object[][] {new object[] {"IBM", 87d}});
            expected.AddResultRemove(7200, 0, new object[][]{new object[] {"YAH", 54d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion15_16(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"YAH", 112d}});
            expected.AddResultInsRem(6200, 0, null, null);
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion78(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new object[][]{new object[] {"IBM", 109d}, new object[] {"YAH", 112d}}, null);
            expected.AddResultInsRem(6200, 0, null, null);
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion56(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 25d}, new object[] {"MSFT", 34d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 58d}, new object[] {"YAH", 59d}, new object[] {"IBM", 85d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"YAH", 87d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 109d}, new object[] {"YAH", 112d}});
            expected.AddResultInsRem(6200, 0, new object[][]{new object[] {"YAH", 88d}}, new object[][] {new object[] {"IBM", 87d}});
            expected.AddResultRemove(7200, 0, new object[][]{new object[] {"MSFT", 79d}, new object[] {"IBM", 54d}, new object[] {"YAH", 54d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion17IStreamOnly(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][]{new object[] {"IBM", 25d}});
            expected.AddResultInsert(1500, 1, new object[][]{new object[] {"IBM", 58d}});
            expected.AddResultInsert(3500, 1, new object[][]{new object[] {"YAH", 87d}});
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 109d}});
            expected.AddResultInsert(5900, 1, new object[][]{new object[] {"YAH", 88d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false);
        }
    
        private void TryAssertion17IRStream(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][]{new object[] {"IBM", 25d}});
            expected.AddResultInsert(1500, 1, new object[][]{new object[] {"IBM", 58d}});
            expected.AddResultInsert(3500, 1, new object[][]{new object[] {"YAH", 87d}});
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 109d}});
            expected.AddResultRemove(5700, 0, new object[][]{new object[] {"IBM", 87d}});
            expected.AddResultRemove(6300, 0, new object[][]{new object[] {"MSFT", 79d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false);
        }
    
        private void TryAssertion18(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 34d}, new object[] {"MSFT", 34d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 85d}, new object[] {"MSFT", 85d}, new object[] {"IBM", 85d}, new object[] {"YAH", 85d}, new object[] {"IBM", 85d}});
            expected.AddResultInsert(3200, 0, new object[][]{new object[] {"IBM", 85d}, new object[] {"MSFT", 85d}, new object[] {"IBM", 85d}, new object[] {"YAH", 85d}, new object[] {"IBM", 85d}});
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"IBM", 87d}, new object[] {"MSFT", 87d}, new object[] {"IBM", 87d}, new object[] {"YAH", 87d}, new object[] {"IBM", 87d}, new object[] {"YAH", 87d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 112d}, new object[] {"MSFT", 112d}, new object[] {"IBM", 112d}, new object[] {"YAH", 112d}, new object[] {"IBM", 112d}, new object[] {"YAH", 112d}, new object[] {"IBM", 112d}, new object[] {"YAH", 112d}});
            expected.AddResultInsert(6200, 0, new object[][]{new object[] {"MSFT", 88d}, new object[] {"IBM", 88d}, new object[] {"YAH", 88d}, new object[] {"IBM", 88d}, new object[] {"YAH", 88d}, new object[] {"IBM", 88d}, new object[] {"YAH", 88d}, new object[] {"YAH", 88d}});
            expected.AddResultInsert(7200, 0, new object[][]{new object[] {"IBM", 54d}, new object[] {"YAH", 54d}, new object[] {"IBM", 54d}, new object[] {"YAH", 54d}, new object[] {"YAH", 54d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertionHaving(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select symbol, avg(price) as avgPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time(3 sec) " +
                    "having avg(price) > 10" +
                    "output every 1 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionHaving(epService, listener);
        }
    
        private void RunAssertionHavingJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select symbol, avg(price) as avgPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time(3 sec) as md, " +
                    typeof(SupportBean).FullName + "#keepall as s where s.TheString = md.symbol " +
                    "having avg(price) > 10" +
                    "output every 1 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("SYM1", -1));
    
            TryAssertionHaving(epService, listener);
        }
    
        private void TryAssertionHaving(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEvent(epService, "SYM1", 10d);
            SendEvent(epService, "SYM1", 11d);
            SendEvent(epService, "SYM1", 9);
    
            SendTimer(epService, 1000);
            string[] fields = "symbol,avgPrice".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"SYM1", 10.5});
    
            SendEvent(epService, "SYM1", 13d);
            SendEvent(epService, "SYM1", 10d);
            SendEvent(epService, "SYM1", 9);
            SendTimer(epService, 2000);
    
            Assert.AreEqual(3, listener.LastNewData.Length);
            Assert.IsNull(listener.LastOldData);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"SYM1", 43 / 4.0}, new object[] {"SYM1", 53.0 / 5.0}, new object[] {"SYM1", 62 / 6.0}});
        }
    
        private void RunAssertionMaxTimeWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream volume, max(price) as maxVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#time(1 sec) " +
                    "output every 1 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "SYM1", 1d);
            SendEvent(epService, "SYM1", 2d);
            listener.Reset();
    
            // moves all events out of the window,
            SendTimer(epService, 1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            UniformPair<EventBean[]> result = listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
            Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
            Assert.AreEqual(2, result.Second.Length);
            Assert.AreEqual(null, result.Second[0].Get("maxVol"));
            Assert.AreEqual(null, result.Second[1].Get("maxVol"));
        }
    
        private void RunAssertionLimitSnapshot(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string selectStmt = "select symbol, sum(price) as sumprice from " + typeof(SupportMarketDataBean).FullName +
                    "#time(10 seconds) output snapshot every 1 seconds order by symbol asc";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendEvent(epService, "ABC", 20);
    
            SendTimer(epService, 500);
            SendEvent(epService, "IBM", 16);
            SendEvent(epService, "MSFT", 14);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 1000);
            var fields = new string[]{"symbol", "sumprice"};
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"ABC", 50d}, new object[] {"IBM", 50d}, new object[] {"MSFT", 50d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 1500);
            SendEvent(epService, "YAH", 18);
            SendEvent(epService, "s4", 30);
    
            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"ABC", 98d}, new object[] {"IBM", 98d}, new object[] {"MSFT", 98d}, new object[] {"YAH", 98d}, new object[] {"s4", 98d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"YAH", 48d}, new object[] {"s4", 48d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 12000);
            Assert.IsTrue(listener.IsInvoked);
            Assert.IsNull(listener.LastNewData);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 13000);
            Assert.IsTrue(listener.IsInvoked);
            Assert.IsNull(listener.LastNewData);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionLimitSnapshotJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string selectStmt = "select symbol, sum(price) as sumprice from " + typeof(SupportMarketDataBean).FullName +
                    "#time(10 seconds) as m, " + typeof(SupportBean).FullName +
                    "#keepall as s where s.TheString = m.symbol output snapshot every 1 seconds order by symbol asc";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("ABC", 1));
            epService.EPRuntime.SendEvent(new SupportBean("IBM", 2));
            epService.EPRuntime.SendEvent(new SupportBean("MSFT", 3));
            epService.EPRuntime.SendEvent(new SupportBean("YAH", 4));
            epService.EPRuntime.SendEvent(new SupportBean("s4", 5));
    
            SendEvent(epService, "ABC", 20);
    
            SendTimer(epService, 500);
            SendEvent(epService, "IBM", 16);
            SendEvent(epService, "MSFT", 14);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 1000);
            var fields = new string[]{"symbol", "sumprice"};
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"ABC", 50d}, new object[] {"IBM", 50d}, new object[] {"MSFT", 50d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 1500);
            SendEvent(epService, "YAH", 18);
            SendEvent(epService, "s4", 30);
    
            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"ABC", 98d}, new object[] {"IBM", 98d}, new object[] {"MSFT", 98d}, new object[] {"YAH", 98d}, new object[] {"s4", 98d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 10500);
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"YAH", 48d}, new object[] {"s4", 48d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 11500);
            SendTimer(epService, 12000);
            Assert.IsTrue(listener.IsInvoked);
            Assert.IsNull(listener.LastNewData);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 13000);
            Assert.IsTrue(listener.IsInvoked);
            Assert.IsNull(listener.LastNewData);
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSortWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream volume, max(price) as maxVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#sort(1, volume desc) as s0," +
                    typeof(SupportBean).FullName + "#keepall as s1 " +
                    "output every 1 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));
    
            SendEvent(epService, "JOIN_KEY", 1d);
            SendEvent(epService, "JOIN_KEY", 2d);
            listener.Reset();
    
            // moves all events out of the window,
            SendTimer(epService, 1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            UniformPair<EventBean[]> result = listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
            Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
            Assert.AreEqual(1, result.Second.Length);
            Assert.AreEqual(2.0, result.Second[0].Get("maxVol"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregateAllNoJoinLast(EPServiceProvider epService) {
            TryAssertionAggregateAllNoJoinLast(epService, true);
            TryAssertionAggregateAllNoJoinLast(epService, false);
        }
    
        private void TryAssertionAggregateAllNoJoinLast(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
    
            string epl = hint + "select LongBoxed, sum(LongBoxed) as result " +
                    "from " + typeof(SupportBean).FullName + "#length(3) " +
                    "having sum(LongBoxed) > 0 " +
                    "output last every 2 events";
    
            TryAssertLastSum(epService, CreateStmtAndListenerNoJoin(epService, epl));
    
            epl = hint + "select LongBoxed, sum(LongBoxed) as result " +
                    "from " + typeof(SupportBean).FullName + "#length(3) " +
                    "output last every 2 events";
            TryAssertLastSum(epService, CreateStmtAndListenerNoJoin(epService, epl));
        }
    
        private void RunAssertionAggregateAllJoinAll(EPServiceProvider epService) {
            TryAssertionAggregateAllJoinAll(epService, true);
            TryAssertionAggregateAllJoinAll(epService, false);
        }
    
        private void TryAssertionAggregateAllJoinAll(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
    
            string epl = hint + "select LongBoxed, sum(LongBoxed) as result " +
                    "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                    typeof(SupportBean).FullName + "#length(3) as two " +
                    "having sum(LongBoxed) > 0 " +
                    "output all every 2 events";
    
            TryAssertAllSum(epService, CreateStmtAndListenerJoin(epService, epl));
    
            epl = hint + "select LongBoxed, sum(LongBoxed) as result " +
                    "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                    typeof(SupportBean).FullName + "#length(3) as two " +
                    "output every 2 events";
    
            TryAssertAllSum(epService, CreateStmtAndListenerJoin(epService, epl));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAggregateAllJoinLast(EPServiceProvider epService) {
            string epl = "select LongBoxed, sum(LongBoxed) as result " +
                    "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                    typeof(SupportBean).FullName + "#length(3) as two " +
                    "having sum(LongBoxed) > 0 " +
                    "output last every 2 events";
    
            TryAssertLastSum(epService, CreateStmtAndListenerJoin(epService, epl));
    
            epl = "select LongBoxed, sum(LongBoxed) as result " +
                    "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                    typeof(SupportBean).FullName + "#length(3) as two " +
                    "output last every 2 events";
    
            TryAssertLastSum(epService, CreateStmtAndListenerJoin(epService, epl));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTime(EPServiceProvider epService) {
            // Set the clock to 0
            var currentTime = new AtomicLong();
            SendTimeEventRelative(epService, 0, currentTime);
    
            // Create the EPL statement and add a listener
            string statementText = "select symbol, sum(volume) from " + EVENT_NAME + "#length(5) output first every 3 seconds";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
            updateListener.Reset();
    
            // Send the first event of the batch; should be output
            SendMarketDataEvent(epService, 10L);
            AssertEvent(updateListener, 10L);
    
            // Send another event, not the first, for aggregation
            // update only, no output
            SendMarketDataEvent(epService, 20L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // Update time
            SendTimeEventRelative(epService, 3000, currentTime);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // Send first event of the next batch, should be output.
            // The aggregate value is computed over all events
            // received: 10 + 20 + 30 = 60
            SendMarketDataEvent(epService, 30L);
            AssertEvent(updateListener, 60L);
    
            // Send the next event of the batch, no output
            SendMarketDataEvent(epService, 40L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // Update time
            SendTimeEventRelative(epService, 3000, currentTime);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // Send first event of third batch
            SendMarketDataEvent(epService, 1L);
            AssertEvent(updateListener, 101L);
    
            // Update time
            SendTimeEventRelative(epService, 3000, currentTime);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // Update time: no first event this batch, so a callback
            // is made at the end of the interval
            SendTimeEventRelative(epService, 3000, currentTime);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            statement.Dispose();
        }
    
        private void RunAssertionCount(EPServiceProvider epService) {
            // Create the EPL statement and add a listener
            string statementText = "select symbol, sum(volume) from " + EVENT_NAME + "#length(5) output first every 3 events";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
            updateListener.Reset();
    
            // Send the first event of the batch, should be output
            SendEventLong(epService, 10L);
            AssertEvent(updateListener, 10L);
    
            // Send the second event of the batch, not output, used
            // for updating the aggregate value only
            SendEventLong(epService, 20L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // Send the third event of the batch, still not output,
            // but should reset the batch
            SendEventLong(epService, 30L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // First event, next batch, aggregate value should be
            // 10 + 20 + 30 + 40 = 100
            SendEventLong(epService, 40L);
            AssertEvent(updateListener, 100L);
    
            // Next event again not output
            SendEventLong(epService, 50L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            statement.Dispose();
        }
    
        private void SendEventLong(EPServiceProvider epService, long volume) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("DELL", 0.0, volume, null));
        }
    
        private SupportUpdateListener CreateStmtAndListenerNoJoin(EPServiceProvider epService, string epl) {
            var updateListener = new SupportUpdateListener();
            EPStatement view = epService.EPAdministrator.CreateEPL(epl);
            view.Events += updateListener.Update;
    
            return updateListener;
        }
    
        private void TryAssertAllSum(EPServiceProvider epService, SupportUpdateListener updateListener) {
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
            Assert.AreEqual(1L, updateListener.LastNewData[0].Get("result"));
            Assert.AreEqual(2L, updateListener.LastNewData[1].Get("LongBoxed"));
            Assert.AreEqual(3L, updateListener.LastNewData[1].Get("result"));
            Assert.IsNull(updateListener.LastOldData);
        }
    
        private void TryAssertLastSum(EPServiceProvider epService, SupportUpdateListener updateListener) {
            // send an event
            SendEvent(epService, 1);
    
            // check no update
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // send another event
            SendEvent(epService, 2);
    
            // check update, all events present
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreEqual(2L, updateListener.LastNewData[0].Get("LongBoxed"));
            Assert.AreEqual(3L, updateListener.LastNewData[0].Get("result"));
            Assert.IsNull(updateListener.LastOldData);
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed) {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed) {
            SendEvent(epService, longBoxed, 0, (short) 0);
        }
    
        private void SendMarketDataEvent(EPServiceProvider epService, long volume) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM1", 0, volume, null));
        }
    
        private void SendTimeEventRelative(EPServiceProvider epService, int timeIncrement, AtomicLong currentTime) {
            currentTime.IncrementAndGet(timeIncrement);
            var theEvent = new CurrentTimeEvent(currentTime.Get());
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private SupportUpdateListener CreateStmtAndListenerJoin(EPServiceProvider epService, string epl) {
            var updateListener = new SupportUpdateListener();
            EPStatement view = epService.EPAdministrator.CreateEPL(epl);
            view.Events += updateListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
    
            return updateListener;
        }
    
        private void AssertEvent(SupportUpdateListener updateListener, long volume) {
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.IsTrue(updateListener.LastNewData != null);
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreEqual(volume, updateListener.LastNewData[0].Get("sum(volume)"));
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(EPServiceProvider epService, long time) {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
