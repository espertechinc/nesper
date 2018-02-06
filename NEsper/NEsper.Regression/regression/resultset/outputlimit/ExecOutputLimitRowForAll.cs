///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitRowForAll : RegressionExecution {
        private const string CATEGORY = "Fully-Aggregated and Un-grouped";
    
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
            RunAssertion17FirstNoHavingNoJoin(epService);
            RunAssertion18SnapshotNoHavingNoJoin(epService);
            RunAssertionOuputLastWithInsertInto(epService);
            RunAssertionAggAllHaving(epService);
            RunAssertionAggAllHavingJoin(epService);
            RunAssertionJoinSortWindow(epService);
            RunAssertionMaxTimeWindow(epService);
            RunAssertionTimeWindowOutputCountLast(epService);
            RunAssertionTimeBatchOutputCount(epService);
            RunAssertionLimitSnapshot(epService);
            RunAssertionLimitSnapshotJoin(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionOutputSnapshotGetValue(epService);
            }
        }
    
        private void RunAssertion1NoneNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec)";
            TryAssertion12(epService, stmtText, "none");
        }
    
        private void RunAssertion2NoneNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol";
            TryAssertion12(epService, stmtText, "none");
        }
    
        private void RunAssertion3NoneHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    " having sum(price) > 100";
            TryAssertion34(epService, stmtText, "none");
        }
    
        private void RunAssertion4NoneHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    " having sum(price) > 100";
            TryAssertion34(epService, stmtText, "none");
        }
    
        private void RunAssertion5DefaultNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }
    
        private void RunAssertion6DefaultNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }
    
        private void RunAssertion7DefaultHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) \n" +
                    "having sum(price) > 100" +
                    "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }
    
        private void RunAssertion8DefaultHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100" +
                    "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }
    
        private void RunAssertion9AllNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion9AllNoHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion10AllNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion10AllNoHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output all every 1 seconds";
            TryAssertion56(epService, stmtText, "all");
        }
    
        private void RunAssertion11AllHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion11AllHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion12AllHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion12AllHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100" +
                    "output all every 1 seconds";
            TryAssertion78(epService, stmtText, "all");
        }
    
        private void RunAssertion13LastNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion13LastNoHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion14LastNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion14LastNoHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "output last every 1 seconds";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion15LastHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion15LastHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion16LastHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion16LastHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "having sum(price) > 100 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion17FirstNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output first every 1 seconds";
            TryAssertion17(epService, stmtText, "first");
        }
    
        private void RunAssertion18SnapshotNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "output snapshot every 1 seconds";
            TryAssertion18(epService, stmtText, "first");
        }
    
        private void RunAssertionOuputLastWithInsertInto(EPServiceProvider epService) {
            TryAssertionOuputLastWithInsertInto(epService, false);
            TryAssertionOuputLastWithInsertInto(epService, true);
        }
    
        private void TryAssertionOuputLastWithInsertInto(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            string eplInsert = hint + "insert into MyStream select sum(IntPrimitive) as thesum from SupportBean#keepall " +
                    "output last every 2 events";
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(eplInsert);
    
            EPStatement stmtListen = epService.EPAdministrator.CreateEPL("select * from MyStream");
            var listener = new SupportUpdateListener();
            stmtListen.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "thesum".Split(','), new object[]{30});
    
            stmtInsert.Dispose();
            stmtListen.Dispose();
        }
    
        private void TryAssertion12(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(200, 1, new[] {new object[] {25d}}, new[] {new object[] {null}});
            expected.AddResultInsRem(800, 1, new[] {new object[] {34d}}, new[] {new object[] {25d}});
            expected.AddResultInsRem(1500, 1, new[] {new object[] {58d}}, new[] {new object[] {34d}});
            expected.AddResultInsRem(1500, 2, new[] {new object[] {59d}}, new[] {new object[] {58d}});
            expected.AddResultInsRem(2100, 1, new[] {new object[] {85d}}, new[] {new object[] {59d}});
            expected.AddResultInsRem(3500, 1, new[] {new object[] {87d}}, new[] {new object[] {85d}});
            expected.AddResultInsRem(4300, 1, new[] {new object[] {109d}}, new[] {new object[] {87d}});
            expected.AddResultInsRem(4900, 1, new[] {new object[] {112d}}, new[] {new object[] {109d}});
            expected.AddResultInsRem(5700, 0, new[] {new object[] {87d}}, new[] {new object[] {112d}});
            expected.AddResultInsRem(5900, 1, new[] {new object[] {88d}}, new[] {new object[] {87d}});
            expected.AddResultInsRem(6300, 0, new[] {new object[] {79d}}, new[] {new object[] {88d}});
            expected.AddResultInsRem(7000, 0, new[] {new object[] {54d}}, new[] {new object[] {79d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion34(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(4300, 1, new[] {new object[] {109d}}, null);
            expected.AddResultInsRem(4900, 1, new[] {new object[] {112d}}, new[] {new object[] {109d}});
            expected.AddResultInsRem(5700, 0, null, new[] {new object[] {112d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion13_14(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new[] {new object[] {34d}}, new[] {new object[] {null}});
            expected.AddResultInsRem(2200, 0, new[] {new object[] {85d}}, new[] {new object[] {34d}});
            expected.AddResultInsRem(3200, 0, new[] {new object[] {85d}}, new[] {new object[] {85d}});
            expected.AddResultInsRem(4200, 0, new[] {new object[] {87d}}, new[] {new object[] {85d}});
            expected.AddResultInsRem(5200, 0, new[] {new object[] {112d}}, new[] {new object[] {87d}});
            expected.AddResultInsRem(6200, 0, new[] {new object[] {88d}}, new[] {new object[] {112d}});
            expected.AddResultInsRem(7200, 0, new[] {new object[] {54d}}, new[] {new object[] {88d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion15_16(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new[] {new object[] {112d}}, new[] {new object[] {109d}});
            expected.AddResultInsRem(6200, 0, null, new[] {new object[] {112d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion78(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new[] {new object[] {109d}, new object[] {112d}}, new[] {new object[] {109d}});
            expected.AddResultInsRem(6200, 0, null, new[] {new object[] {112d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion56(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new[] {new object[] {25d}, new object[] {34d}}, new[] {new object[] {null}, new object[] {25d}});
            expected.AddResultInsRem(2200, 0, new[] {new object[] {58d}, new object[] {59d}, new object[] {85d}}, new[] {new object[] {34d}, new object[] {58d}, new object[] {59d}});
            expected.AddResultInsRem(3200, 0, new[] {new object[] {85d}}, new[] {new object[] {85d}});
            expected.AddResultInsRem(4200, 0, new[] {new object[] {87d}}, new[] {new object[] {85d}});
            expected.AddResultInsRem(5200, 0, new[] {new object[] {109d}, new object[] {112d}}, new[] {new object[] {87d}, new object[] {109d}});
            expected.AddResultInsRem(6200, 0, new[] {new object[] {87d}, new object[] {88d}}, new[] {new object[] {112d}, new object[] {87d}});
            expected.AddResultInsRem(7200, 0, new[] {new object[] {79d}, new object[] {54d}}, new[] {new object[] {88d}, new object[] {79d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion17(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(200, 1, new[] {new object[] {25d}}, new[] {new object[] {null}});
            expected.AddResultInsRem(1500, 1, new[] {new object[] {58d}}, new[] {new object[] {34d}});
            expected.AddResultInsRem(3500, 1, new[] {new object[] {87d}}, new[] {new object[] {85d}});
            expected.AddResultInsRem(4300, 1, new[] {new object[] {109d}}, new[] {new object[] {87d}});
            expected.AddResultInsRem(5700, 0, new[] {new object[] {87d}}, new[] {new object[] {112d}});
            expected.AddResultInsRem(6300, 0, new[] {new object[] {79d}}, new[] {new object[] {88d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion18(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new[]{"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new[] {new object[] {34d}}, null);
            expected.AddResultInsRem(2200, 0, new[] {new object[] {85d}}, null);
            expected.AddResultInsRem(3200, 0, new[] {new object[] {85d}}, null);
            expected.AddResultInsRem(4200, 0, new[] {new object[] {87d}}, null);
            expected.AddResultInsRem(5200, 0, new[] {new object[] {112d}}, null);
            expected.AddResultInsRem(6200, 0, new[] {new object[] {88d}}, null);
            expected.AddResultInsRem(7200, 0, new[] {new object[] {54d}}, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertionAggAllHaving(EPServiceProvider epService) {
            string stmtText = "select sum(volume) as result " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(10) as two " +
                    "having sum(volume) > 0 " +
                    "output every 5 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new[]{"result"};
    
            SendMDEvent(epService, 20);
            SendMDEvent(epService, -100);
            SendMDEvent(epService, 0);
            SendMDEvent(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMDEvent(epService, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new[] {new object[] {20L}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new[] {new object[] {20L}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggAllHavingJoin(EPServiceProvider epService) {
            string stmtText = "select sum(volume) as result " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(10) as one," +
                    typeof(SupportBean).FullName + "#length(10) as two " +
                    "where one.symbol=two.TheString " +
                    "having sum(volume) > 0 " +
                    "output every 5 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new[]{"result"};
            epService.EPRuntime.SendEvent(new SupportBean("S0", 0));
    
            SendMDEvent(epService, 20);
            SendMDEvent(epService, -100);
            SendMDEvent(epService, 0);
            SendMDEvent(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMDEvent(epService, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new[] {new object[] {20L}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new[] {new object[] {20L}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSortWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream max(price) as maxVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#sort(1,volume desc) as s0, " +
                    typeof(SupportBean).FullName + "#keepall as s1 where s1.TheString=s0.symbol " +
                    "output every 1.0d seconds";
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
            Assert.AreEqual(2, result.Second.Length);
            Assert.AreEqual(null, result.Second[0].Get("maxVol"));
            Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));
    
            // statement object model test
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            stmt.Dispose();
        }
    
        private void RunAssertionMaxTimeWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream max(price) as maxVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#time(1.1 sec) " +
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
            Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeWindowOutputCountLast(EPServiceProvider epService) {
            string stmtText = "select count(*) as cnt from " + typeof(SupportBean).FullName + "#time(10 seconds) output every 10 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 0);
            SendTimer(epService, 10000);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 20000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "e1");
            SendTimer(epService, 30000);
            EventBean[] newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual(1L, newEvents[0].Get("cnt"));
            Assert.AreEqual(0L, newEvents[1].Get("cnt"));
    
            SendTimer(epService, 31000);
    
            SendEvent(epService, "e2");
            SendEvent(epService, "e3");
            SendTimer(epService, 40000);
            newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual(1L, newEvents[0].Get("cnt"));
            Assert.AreEqual(2L, newEvents[1].Get("cnt"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchOutputCount(EPServiceProvider epService) {
            string stmtText = "select count(*) as cnt from " + typeof(SupportBean).FullName + "#time_batch(10 seconds) output every 10 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
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
            EventBean[] newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            // output limiting starts 10 seconds after, therefore the old batch was posted already and the cnt is zero
            Assert.AreEqual(1L, newEvents[0].Get("cnt"));
            Assert.AreEqual(0L, newEvents[1].Get("cnt"));
    
            SendTimer(epService, 50000);
            EventBean[] newData = listener.LastNewData;
            Assert.AreEqual(0L, newData[0].Get("cnt"));
            listener.Reset();
    
            SendEvent(epService, "e2");
            SendEvent(epService, "e3");
            SendTimer(epService, 60000);
            newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(1, newEvents.Length);
            Assert.AreEqual(2L, newEvents[0].Get("cnt"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionLimitSnapshot(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
    
            SendTimer(epService, 0);
            string selectStmt = "select count(*) as cnt from " + typeof(SupportBean).FullName + "#time(10 seconds) where IntPrimitive > 0 output snapshot every 1 seconds";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += listener.Update;
            SendEvent(epService, "s0", 1);
    
            SendTimer(epService, 500);
            SendEvent(epService, "s1", 1);
            SendEvent(epService, "s2", -1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 1000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {2L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 1500);
            SendEvent(epService, "s4", 2);
            SendEvent(epService, "s5", 3);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 2000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {4L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendEvent(epService, "s5", 4);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 9000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {5L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {4L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {3L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionLimitSnapshotJoin(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
    
            SendTimer(epService, 0);
            string selectStmt = "select count(*) as cnt from " +
                    typeof(SupportBean).FullName + "#time(10 seconds) as s, " +
                    typeof(SupportMarketDataBean).FullName + "#keepall as m where m.symbol = s.TheString and IntPrimitive > 0 output snapshot every 1 seconds";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("s0", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("s1", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("s2", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("s4", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("s5", 0, 0L, ""));
    
            SendEvent(epService, "s0", 1);
    
            SendTimer(epService, 500);
            SendEvent(epService, "s1", 1);
            SendEvent(epService, "s2", -1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 1000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {2L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 1500);
            SendEvent(epService, "s4", 2);
            SendEvent(epService, "s5", 3);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 2000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {4L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendEvent(epService, "s5", 4);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 9000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {5L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            // The execution of the join is after the snapshot, as joins are internal dispatch
            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {5L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new[]{"cnt"}, new[] {new object[] {3L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputSnapshotGetValue(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("customagg", typeof(MyContextAggFuncFactory));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            TryAssertionOutputSnapshotGetValue(epService, true);
            TryAssertionOutputSnapshotGetValue(epService, false);
        }
    
        private void TryAssertionOutputSnapshotGetValue(EPServiceProvider epService, bool join) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select Customagg(IntPrimitive) as c0 from SupportBean" +
                            (join ? "#keepall, SupportBean_S0#lastevent" : "") +
                            " output snapshot every 3 events");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            MyContextAggFunc.ResetGetValueInvocationCount();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.AreEqual(0, MyContextAggFunc.GetValueInvocationCount);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            Assert.AreEqual(60, listener.AssertOneGetNewAndReset().Get("c0"));
            Assert.AreEqual(1, MyContextAggFunc.GetValueInvocationCount);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 40));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 50));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 60));
            Assert.AreEqual(210, listener.AssertOneGetNewAndReset().Get("c0"));
            Assert.AreEqual(2, MyContextAggFunc.GetValueInvocationCount);
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string s) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = 0.0;
            bean.IntPrimitive = 0;
            bean.IntBoxed = 0;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string s, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(EPServiceProvider epService, long time) {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMDEvent(EPServiceProvider epService, long volume) {
            var bean = new SupportMarketDataBean("S0", 0, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }

        public class MyContextAggFuncFactory : AggregationFunctionFactory
        {
            public string FunctionName
            {
                set { }
            }

            public void Validate(AggregationValidationContext validationContext)
            {
            }

            public AggregationMethod NewAggregator()
            {
                return new MyContextAggFunc();
            }

            public Type ValueType
            {
                get { return typeof(int); }
            }
        }

        public class MyContextAggFunc : AggregationMethod
        {
            private static long getValueInvocationCount = 0;

            public static long GetValueInvocationCount
            {
                get { return getValueInvocationCount; }
            }

            public static void ResetGetValueInvocationCount()
            {
                getValueInvocationCount = 0;
            }

            private int sum;

            public void Enter(object value)
            {
                int amount = value.AsInt();
                sum += amount;
            }

            public void Leave(object value)
            {

            }

            public object Value
            {
                get
                {
                    getValueInvocationCount++;
                    return sum;
                }
            }

            public void Clear()
            {
            }
        }
    }
} // end of namespace
