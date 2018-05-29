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
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitGroupByEventPerRow : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
        private const string CATEGORY = "Aggregated and Grouped";
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionUnaggregatedOutputFirst(epService);
            RunAssertionOutputFirstHavingJoinNoJoin(epService);
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
            RunAssertionHaving(epService);
            RunAssertionHavingJoin(epService);
            RunAssertionJoinSortWindow(epService);
            RunAssertionLimitSnapshot(epService);
            RunAssertionLimitSnapshotJoin(epService);
            RunAssertionMaxTimeWindow(epService);
            RunAssertionNoJoinLast(epService);
            RunAssertionNoOutputClauseView(epService);
            RunAssertionNoJoinDefault(epService);
            RunAssertionJoinDefault(epService);
            RunAssertionNoJoinAll(epService);
            RunAssertionJoinAll(epService);
            RunAssertionJoinLast(epService);
        }
    
        private void RunAssertionUnaggregatedOutputFirst(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string[] fields = "TheString,IntPrimitive".Split(',');
            string epl = "select * from SupportBean\n" +
                    "     group by TheString\n" +
                    "     output first every 10 seconds";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 3});
    
            SendTimer(epService, 5000);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 4});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 10000);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 6));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 7});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 8));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 9});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputFirstHavingJoinNoJoin(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            string stmtText = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
            TryOutputFirstHaving(epService, stmtText);
    
            string stmtTextJoin = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
            TryOutputFirstHaving(epService, stmtTextJoin);
    
            string stmtTextOrder = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
            TryOutputFirstHaving(epService, stmtTextOrder);
    
            string stmtTextOrderJoin = "select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
            TryOutputFirstHaving(epService, stmtTextOrderJoin);
        }
    
        private void TryOutputFirstHaving(EPServiceProvider epService, string statementText) {
            string[] fields = "TheString,LongPrimitive,value".Split(',');
            string[] fieldsLimited = "TheString,value".Split(',');
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
    
            SendBeanEvent(epService, "E1", 101, 10);
            SendBeanEvent(epService, "E2", 102, 15);
            SendBeanEvent(epService, "E1", 103, 10);
            SendBeanEvent(epService, "E2", 104, 5);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, "E2", 105, 5);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 105L, 25});
    
            SendBeanEvent(epService, "E2", 106, -6);    // to 19, does not count toward condition
            SendBeanEvent(epService, "E2", 107, 2);    // to 21, counts toward condition
            Assert.IsFalse(listener.IsInvoked);
            SendBeanEvent(epService, "E2", 108, 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 108L, 22});
    
            SendBeanEvent(epService, "E2", 109, 1);    // to 23, counts toward condition
            Assert.IsFalse(listener.IsInvoked);
            SendBeanEvent(epService, "E2", 110, 1);     // to 24
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 110L, 24});
    
            SendBeanEvent(epService, "E2", 111, -10);    // to 14
            SendBeanEvent(epService, "E2", 112, 10);    // to 24, counts toward condition
            Assert.IsFalse(listener.IsInvoked);
            SendBeanEvent(epService, "E2", 113, 0);    // to 24, counts toward condition
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 113L, 24});
    
            SendBeanEvent(epService, "E2", 114, -10);    // to 14
            SendBeanEvent(epService, "E2", 115, 1);     // to 15
            SendBeanEvent(epService, "E2", 116, 5);     // to 20
            SendBeanEvent(epService, "E2", 117, 0);     // to 20
            SendBeanEvent(epService, "E2", 118, 1);     // to 21    // counts
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, "E2", 119, 0);    // to 21
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 119L, 21});
    
            // remove events
            SendMDEvent(epService, "E2", 0);   // remove 113, 117, 119 (any order of delete!)
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLimited, new object[]{"E2", 21});
    
            // remove events
            SendMDEvent(epService, "E2", -10); // remove 111, 114
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLimited, new object[]{"E2", 41});
    
            // remove events
            SendMDEvent(epService, "E2", -6);  // since there is 3*0 we output the next one
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsLimited, new object[]{"E2", 47});
    
            SendMDEvent(epService, "E2", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion1NoneNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by symbol";
            TryAssertion12(epService, stmtText, "none");
        }
    
        private void RunAssertion2NoneNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol";
            TryAssertion12(epService, stmtText, "none");
        }
    
        private void RunAssertion3NoneHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    " having sum(price) > 50";
            TryAssertion34(epService, stmtText, "none");
        }
    
        private void RunAssertion4NoneHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "having sum(price) > 50";
            TryAssertion34(epService, stmtText, "none");
        }
    
        private void RunAssertion5DefaultNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }
    
        private void RunAssertion6DefaultNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "output every 1 seconds";
            TryAssertion56(epService, stmtText, "default");
        }
    
        private void RunAssertion7DefaultHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) \n" +
                    "group by symbol " +
                    "having sum(price) > 50" +
                    "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }
    
        private void RunAssertion8DefaultHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "having sum(price) > 50" +
                    "output every 1 seconds";
            TryAssertion78(epService, stmtText, "default");
        }
    
        private void RunAssertion9AllNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    "output all every 1 seconds " +
                    "order by symbol";
            TryAssertion9_10(epService, stmtText, "all");
        }
    
        private void RunAssertion10AllNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "output all every 1 seconds " +
                    "order by symbol";
            TryAssertion9_10(epService, stmtText, "all");
        }
    
        private void RunAssertion11AllHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }
    
        private void RunAssertion11AllHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }
    
        private void RunAssertion12AllHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }
    
        private void RunAssertion12AllHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output all every 1 seconds";
            TryAssertion11_12(epService, stmtText, "all");
        }
    
        private void RunAssertion13LastNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by symbol " +
                    "output last every 1 seconds " +
                    "order by symbol";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion14LastNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "output last every 1 seconds " +
                    "order by symbol";
            TryAssertion13_14(epService, stmtText, "last");
        }
    
        private void RunAssertion15LastHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion15LastHavingNoJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion16LastHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion16LastHavingJoinHinted(EPServiceProvider epService) {
            string stmtText = "@Hint('enable_outputlimit_opt') select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "having sum(price) > 50 " +
                    "output last every 1 seconds";
            TryAssertion15_16(epService, stmtText, "last");
        }
    
        private void RunAssertion17FirstNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    "output first every 1 seconds";
            TryAssertion17(epService, stmtText, "first");
        }
    
        private void RunAssertion17FirstNoHavingJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec), " +
                    "SupportBean#keepall where TheString=symbol " +
                    "group by symbol " +
                    "output first every 1 seconds";
            TryAssertion17(epService, stmtText, "first");
        }
    
        private void RunAssertion18SnapshotNoHavingNoJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, sum(price) " +
                    "from MarketData#time(5.5 sec) " +
                    "group by symbol " +
                    "output snapshot every 1 seconds";
            TryAssertion18(epService, stmtText, "snapshot");
        }
    
        private void TryAssertion12(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][]{new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(800, 1, new object[][]{new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(1500, 1, new object[][]{new object[] {"IBM", 150L, 49d}});
            expected.AddResultInsert(1500, 2, new object[][]{new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(2100, 1, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(3500, 1, new object[][]{new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsert(4900, 1, new object[][]{new object[] {"YAH", 11500L, 6d}});
            expected.AddResultRemove(5700, 0, new object[][]{new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsert(5900, 1, new object[][]{new object[] {"YAH", 10500L, 7d}});
            expected.AddResultRemove(6300, 0, new object[][]{new object[] {"MSFT", 5000L, null}});
            expected.AddResultRemove(7000, 0, new object[][]{new object[] {"IBM", 150L, 48d}, new object[] {"YAH", 10000L, 6d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion34(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(2100, 1, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 150L, 97d}});
            expected.AddResultRemove(5700, 0, new object[][]{new object[] {"IBM", 100L, 72d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion13_14(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 155L, 75d}, new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsRem(6200, 0, new object[][]{new object[] {"YAH", 10500L, 7d}}, new object[][] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultRemove(7200, 0, new object[][]{new object[] {"IBM", 150L, 48d}, new object[] {"MSFT", 5000L, null}, new object[] {"YAH", 10000L, 6d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion15_16(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsRem(6200, 0, null, new object[][]{new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion78(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsRem(6200, 0, null, new object[][]{new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion56(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 150L, 49d}, new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsRem(6200, 0, new object[][]{new object[] {"YAH", 10500L, 7d}}, new object[][] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultRemove(7200, 0, new object[][]{new object[] {"MSFT", 5000L, null}, new object[] {"IBM", 150L, 48d}, new object[] {"YAH", 10000L, 6d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion9_10(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 150L, 49d}, new object[] {"IBM", 155L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(3200, 0, new object[][]{new object[] {"IBM", 155L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"IBM", 155L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 150L, 97d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsRem(6200, 0, new object[][]{new object[] {"IBM", 150L, 72d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 10500L, 7d}}, new object[][] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, new object[][]{new object[] {"IBM", 150L, 48d}, new object[] {"MSFT", 5000L, null}, new object[] {"YAH", 10500L, 6d}}, new object[][] {new object[] {"IBM", 150L, 48d}, new object[] {"MSFT", 5000L, null}, new object[] {"YAH", 10000L, 6d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion11_12(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(3200, 0, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsRem(6200, 0, new object[][]{new object[] {"IBM", 150L, 72d}}, new object[][] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion17(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][]{new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(800, 1, new object[][]{new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(1500, 1, new object[][]{new object[] {"IBM", 150L, 49d}});
            expected.AddResultInsert(1500, 2, new object[][]{new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(3500, 1, new object[][]{new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(4300, 1, new object[][]{new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsert(4900, 1, new object[][]{new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsert(5700, 0, new object[][]{new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsert(5900, 1, new object[][]{new object[] {"YAH", 10500L, 7d}});
            expected.AddResultInsert(6300, 0, new object[][]{new object[] {"MSFT", 5000L, null}});
            expected.AddResultInsert(7000, 0, new object[][]{new object[] {"IBM", 150L, 48d}, new object[] {"YAH", 10000L, 6d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void TryAssertion18(EPServiceProvider epService, string stmtText, string outputLimit) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "volume", "sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 100L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 75d}, new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(3200, 0, new object[][]{new object[] {"IBM", 100L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 75d}, new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"IBM", 100L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 75d}, new object[] {"YAH", 10000L, 3d}, new object[] {"IBM", 155L, 75d}, new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 100L, 97d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 10000L, 6d}, new object[] {"IBM", 155L, 97d}, new object[] {"YAH", 11000L, 6d}, new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsert(6200, 0, new object[][]{new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 72d}, new object[] {"YAH", 10000L, 7d}, new object[] {"IBM", 155L, 72d}, new object[] {"YAH", 11000L, 7d}, new object[] {"IBM", 150L, 72d}, new object[] {"YAH", 11500L, 7d}, new object[] {"YAH", 10500L, 7d}});
            expected.AddResultInsert(7200, 0, new object[][]{new object[] {"IBM", 155L, 48d}, new object[] {"YAH", 11000L, 6d}, new object[] {"IBM", 150L, 48d}, new object[] {"YAH", 11500L, 6d}, new object[] {"YAH", 10500L, 6d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertionHaving(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream symbol, volume, sum(price) as sumprice" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#time(10 sec) " +
                    "group by symbol " +
                    "having sum(price) >= 10 " +
                    "output every 3 events";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionHavingDefault(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionHavingJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream symbol, volume, sum(price) as sumprice" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#time(10 sec) as s0," +
                    typeof(SupportBean).FullName + "#keepall as s1 " +
                    "where s0.symbol = s1.TheString " +
                    "group by symbol " +
                    "having sum(price) >= 10 " +
                    "output every 3 events";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("IBM", 0));
    
            TryAssertionHavingDefault(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSortWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream symbol, volume, max(price) as maxVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#sort(1, volume) as s0," +
                    typeof(SupportBean).FullName + "#keepall as s1 where s1.TheString = s0.symbol " +
                    "group by symbol output every 1 seconds";
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
    
        private void RunAssertionLimitSnapshot(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string selectStmt = "select symbol, volume, sum(price) as sumprice from " + typeof(SupportMarketDataBean).FullName +
                    "#time(10 seconds) group by symbol output snapshot every 1 seconds";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendEvent(epService, "s0", 1, 20);
    
            SendTimer(epService, 500);
            SendEvent(epService, "IBM", 2, 16);
            SendEvent(epService, "s0", 3, 14);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 1000);
            var fields = new string[]{"symbol", "volume", "sumprice"};
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"s0", 1L, 34d}, new object[] {"IBM", 2L, 16d}, new object[] {"s0", 3L, 34d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 1500);
            SendEvent(epService, "MSFT", 4, 18);
            SendEvent(epService, "IBM", 5, 30);
    
            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"s0", 1L, 34d}, new object[] {"IBM", 2L, 46d}, new object[] {"s0", 3L, 34d}, new object[] {"MSFT", 4L, 18d}, new object[] {"IBM", 5L, 46d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"MSFT", 4L, 18d}, new object[] {"IBM", 5L, 30d}});
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
            string selectStmt = "select symbol, volume, sum(price) as sumprice from " + typeof(SupportMarketDataBean).FullName +
                    "#time(10 seconds) as m, " + typeof(SupportBean).FullName +
                    "#keepall as s where s.TheString = m.symbol group by symbol output snapshot every 1 seconds order by symbol, volume asc";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(selectStmt);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("ABC", 1));
            epService.EPRuntime.SendEvent(new SupportBean("IBM", 2));
            epService.EPRuntime.SendEvent(new SupportBean("MSFT", 3));
    
            SendEvent(epService, "ABC", 1, 20);
    
            SendTimer(epService, 500);
            SendEvent(epService, "IBM", 2, 16);
            SendEvent(epService, "ABC", 3, 14);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 1000);
            var fields = new string[]{"symbol", "volume", "sumprice"};
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"ABC", 1L, 34d}, new object[] {"ABC", 3L, 34d}, new object[] {"IBM", 2L, 16d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 1500);
            SendEvent(epService, "MSFT", 4, 18);
            SendEvent(epService, "IBM", 5, 30);
    
            SendTimer(epService, 10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"ABC", 1L, 34d}, new object[] {"ABC", 3L, 34d}, new object[] {"IBM", 2L, 46d}, new object[] {"IBM", 5L, 46d}, new object[] {"MSFT", 4L, 18d}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 10500);
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"IBM", 5L, 30d}, new object[] {"MSFT", 4L, 18d}});
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
    
        private void RunAssertionMaxTimeWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string epl = "select irstream symbol, " +
                    "volume, max(price) as maxVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#time(1 sec) " +
                    "group by symbol output every 1 seconds";
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
    
            stmt.Dispose();
        }
    
        private void RunAssertionNoJoinLast(EPServiceProvider epService) {
            TryAssertionNoJoinLast(epService, true);
            TryAssertionNoJoinLast(epService, false);
        }
    
        private void TryAssertionNoJoinLast(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
    
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = hint +
                    "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol " +
                    "output last every 2 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionLast(epService, listener);
    
            stmt.Dispose();
            listener.Reset();
        }
    
        private void AssertEvent(SupportUpdateListener listener, string symbol, double? mySum, long volume) {
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(mySum, newData[0].Get("mySum"));
            Assert.AreEqual(volume, newData[0].Get("volume"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void TryAssertionSingle(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("volume"));
    
            SendEvent(epService, SYMBOL_DELL, 10, 100);
            Assert.IsTrue(listener.IsInvoked);
            AssertEvent(listener, SYMBOL_DELL, 100d, 10L);
    
            SendEvent(epService, SYMBOL_IBM, 15, 50);
            AssertEvent(listener, SYMBOL_IBM, 50d, 15L);
        }
    
        private void RunAssertionNoOutputClauseView(EPServiceProvider epService) {
            string epl = "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSingle(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNoJoinDefault(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol " +
                    "output every 2 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionDefault(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinDefault(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "  and one.TheString = two.symbol " +
                    "group by symbol " +
                    "output every 2 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
    
            TryAssertionDefault(epService, stmt, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNoJoinAll(EPServiceProvider epService) {
            TryAssertionNoJoinAll(epService, false);
            TryAssertionNoJoinAll(epService, true);
        }
    
        private void TryAssertionNoJoinAll(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
    
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = hint + "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol " +
                    "output all every 2 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionAll(epService, stmt, listener);
    
            stmt.Dispose();
            listener.Reset();
        }
    
        private void RunAssertionJoinAll(EPServiceProvider epService) {
            TryAssertionJoinAll(epService, false);
            TryAssertionJoinAll(epService, true);
        }
    
        private void TryAssertionJoinAll(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
    
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = hint + "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "  and one.TheString = two.symbol " +
                    "group by symbol " +
                    "output all every 2 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
    
            TryAssertionAll(epService, stmt, listener);
    
            stmt.Dispose();
            listener.Reset();
        }
    
        private void RunAssertionJoinLast(EPServiceProvider epService) {
            TryAssertionJoinLast(epService, true);
            TryAssertionJoinLast(epService, false);
        }
    
        private void TryAssertionJoinLast(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
    
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = hint +
                    "select symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "  and one.TheString = two.symbol " +
                    "group by symbol " +
                    "output last every 2 events";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
    
            TryAssertionLast(epService, listener);
    
            stmt.Dispose();
        }
    
        private void TryAssertionHavingDefault(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEvent(epService, "IBM", 1, 5);
            SendEvent(epService, "IBM", 2, 6);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "IBM", 3, -3);
            string[] fields = "symbol,volume,sumprice".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"IBM", 2L, 11.0});
    
            SendTimer(epService, 5000);
            SendEvent(epService, "IBM", 4, 10);
            SendEvent(epService, "IBM", 5, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "IBM", 6, 1);
            Assert.AreEqual(3, listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"IBM", 4L, 18.0});
            EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[]{"IBM", 5L, 18.0});
            EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[]{"IBM", 6L, 19.0});
            listener.Reset();
    
            SendTimer(epService, 11000);
            Assert.AreEqual(3, listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"IBM", 1L, 11.0});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[]{"IBM", 2L, 11.0});
            listener.Reset();
        }
    
        private void TryAssertionDefault(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("volume").GetBoxedType());
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("mySum").GetBoxedType());
    
            SendEvent(epService, SYMBOL_IBM, 500, 20);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, SYMBOL_DELL, 10000, 51);
            string[] fields = "symbol,volume,mySum".Split(',');
            UniformPair<EventBean[]> events = listener.GetDataListsFlattened();
            if (events.First[0].Get("symbol").Equals(SYMBOL_IBM)) {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_IBM, 500L, 20.0}, new object[] {SYMBOL_DELL, 10000L, 51.0}});
            } else {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_DELL, 10000L, 51.0}, new object[] {SYMBOL_IBM, 500L, 20.0}});
            }
            Assert.IsNull(listener.LastOldData);
    
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 20000, 52);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, SYMBOL_DELL, 40000, 45);
            events = listener.GetDataListsFlattened();
            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                    new object[][]{new object[] {SYMBOL_DELL, 20000L, 51.0 + 52.0}, new object[] {SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0}});
            Assert.IsNull(listener.LastOldData);
        }
    
        private void TryAssertionAll(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("volume").GetBoxedType());
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("mySum").GetBoxedType());
    
            SendEvent(epService, SYMBOL_IBM, 500, 20);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, SYMBOL_DELL, 10000, 51);
            string[] fields = "symbol,volume,mySum".Split(',');
            UniformPair<EventBean[]> events = listener.GetDataListsFlattened();
            if (events.First[0].Get("symbol").Equals(SYMBOL_IBM)) {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_IBM, 500L, 20.0}, new object[] {SYMBOL_DELL, 10000L, 51.0}});
            } else {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_DELL, 10000L, 51.0}, new object[] {SYMBOL_IBM, 500L, 20.0}});
            }
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 20000, 52);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, SYMBOL_DELL, 40000, 45);
            events = listener.GetDataListsFlattened();
            if (events.First[0].Get("symbol").Equals(SYMBOL_IBM)) {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_IBM, 500L, 20.0}, new object[] {SYMBOL_DELL, 20000L, 51.0 + 52.0}, new object[] {SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0}});
            } else {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_DELL, 20000L, 51.0 + 52.0}, new object[] {SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0}, new object[] {SYMBOL_IBM, 500L, 20.0}});
            }
            Assert.IsNull(listener.LastOldData);
        }
    
        private void TryAssertionLast(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "symbol,volume,mySum".Split(',');
            SendEvent(epService, SYMBOL_DELL, 10000, 51);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, SYMBOL_DELL, 20000, 52);
            UniformPair<EventBean[]> events = listener.GetDataListsFlattened();
            EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                    new object[][]{new object[] {SYMBOL_DELL, 20000L, 103.0}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 30000, 70);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, SYMBOL_IBM, 10000, 20);
            events = listener.GetDataListsFlattened();
            if (events.First[0].Get("symbol").Equals(SYMBOL_DELL)) {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_DELL, 30000L, 173.0}, new object[] {SYMBOL_IBM, 10000L, 20.0}});
            } else {
                EPAssertionUtil.AssertPropsPerRow(events.First, fields,
                        new object[][]{new object[] {SYMBOL_IBM, 10000L, 20.0}, new object[] {SYMBOL_DELL, 30000L, 173.0}});
            }
            Assert.IsNull(listener.LastOldData);
        }
    
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString, long longPrimitive, int intPrimitive) {
            var b = new SupportBean();
            b.TheString = theString;
            b.LongPrimitive = longPrimitive;
            b.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(b);
        }
    
        private void SendMDEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
