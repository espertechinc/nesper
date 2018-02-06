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

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRollupOutputRate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
    
            RunAssertionOutputLast(epService, false, false);
            RunAssertionOutputLast(epService, false, true);
            RunAssertionOutputLast(epService, true, false);
            RunAssertionOutputLast(epService, true, true);
    
            RunAssertionOutputLastSorted(epService, false);
            RunAssertionOutputLastSorted(epService, true);
    
            RunAssertionOutputAll(epService, false, false);
            RunAssertionOutputAll(epService, false, true);
            RunAssertionOutputAll(epService, true, false);
            RunAssertionOutputAll(epService, true, true);
    
            RunAssertionOutputAllSorted(epService, false);
            RunAssertionOutputAllSorted(epService, true);
    
            RunAssertionOutputDefault(epService, false);
            RunAssertionOutputDefault(epService, true);
    
            RunAssertionOutputDefaultSorted(epService, false);
            RunAssertionOutputDefaultSorted(epService, true);
    
            RunAssertionOutputFirstHaving(epService, false);
            RunAssertionOutputFirstHaving(epService, true);
    
            RunAssertionOutputFirstSorted(epService, false);
            RunAssertionOutputFirstSorted(epService, true);
    
            RunAssertionOutputFirst(epService, false);
            RunAssertionOutputFirst(epService, true);
    
            RunAssertion3OutputLimitAll(epService, false);
            RunAssertion3OutputLimitAll(epService, true);
    
            RunAssertion4OutputLimitLast(epService, false);
            RunAssertion4OutputLimitLast(epService, true);
    
            RunAssertion1NoOutputLimit(epService);
            RunAssertion2OutputLimitDefault(epService);
            RunAssertion5OutputLimitFirst(epService);
            RunAssertion6OutputLimitSnapshot(epService);
        }
    
        private void RunAssertion1NoOutputLimit(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by Rollup(symbol)";
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult("NoOutputLimit", null, fields);
            expected.AddResultInsRem(200, 1, new object[][]{new object[] {"IBM", 25d}, new object[] {null, 25d}}, new object[][] {new object[] {"IBM", null}, new object[] {null, null}});
            expected.AddResultInsRem(800, 1, new object[][]{new object[] {"MSFT", 9d}, new object[] {null, 34d}}, new object[][] {new object[] {"MSFT", null}, new object[] {null, 25d}});
            expected.AddResultInsRem(1500, 1, new object[][]{new object[] {"IBM", 49d}, new object[] {null, 58d}}, new object[][] {new object[] {"IBM", 25d}, new object[] {null, 34d}});
            expected.AddResultInsRem(1500, 2, new object[][]{new object[] {"YAH", 1d}, new object[] {null, 59d}}, new object[][] {new object[] {"YAH", null}, new object[] {null, 58d}});
            expected.AddResultInsRem(2100, 1, new object[][]{new object[] {"IBM", 75d}, new object[] {null, 85d}}, new object[][] {new object[] {"IBM", 49d}, new object[] {null, 59d}});
            expected.AddResultInsRem(3500, 1, new object[][]{new object[] {"YAH", 3d}, new object[] {null, 87d}}, new object[][] {new object[] {"YAH", 1d}, new object[] {null, 85d}});
            expected.AddResultInsRem(4300, 1, new object[][]{new object[] {"IBM", 97d}, new object[] {null, 109d}}, new object[][] {new object[] {"IBM", 75d}, new object[] {null, 87d}});
            expected.AddResultInsRem(4900, 1, new object[][]{new object[] {"YAH", 6d}, new object[] {null, 112d}}, new object[][] {new object[] {"YAH", 3d}, new object[] {null, 109d}});
            expected.AddResultInsRem(5700, 0, new object[][]{new object[] {"IBM", 72d}, new object[] {null, 87d}}, new object[][] {new object[] {"IBM", 97d}, new object[] {null, 112d}});
            expected.AddResultInsRem(5900, 1, new object[][]{new object[] {"YAH", 7d}, new object[] {null, 88d}}, new object[][] {new object[] {"YAH", 6d}, new object[] {null, 87d}});
            expected.AddResultInsRem(6300, 0, new object[][]{new object[] {"MSFT", null}, new object[] {null, 79d}}, new object[][] {new object[] {"MSFT", 9d}, new object[] {null, 88d}});
            expected.AddResultInsRem(7000, 0, new object[][]{new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}}, new object[][] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 79d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertion2OutputLimitDefault(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by Rollup(symbol)" +
                    "output every 1 seconds";
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult("DefaultOutputLimit", null, fields);
            expected.AddResultInsRem(1200, 0,
                    new object[][]{new object[] {"IBM", 25d}, new object[] {null, 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34d}},
                    new object[][]{new object[] {"IBM", null}, new object[] {null, null}, new object[] {"MSFT", null}, new object[] {null, 25d}});
            expected.AddResultInsRem(2200, 0,
                    new object[][]{new object[] {"IBM", 49d}, new object[] {null, 58d}, new object[] {"YAH", 1d}, new object[] {null, 59d}, new object[] {"IBM", 75d}, new object[] {null, 85d}},
                    new object[][]{new object[] {"IBM", 25d}, new object[] {null, 34d}, new object[] {"YAH", null}, new object[] {null, 58d}, new object[] {"IBM", 49d}, new object[] {null, 59d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0,
                    new object[][]{new object[] {"YAH", 3d}, new object[] {null, 87d}},
                    new object[][]{new object[] {"YAH", 1d}, new object[] {null, 85d}});
            expected.AddResultInsRem(5200, 0,
                    new object[][]{new object[] {"IBM", 97d}, new object[] {null, 109d}, new object[] {"YAH", 6d}, new object[] {null, 112d}},
                    new object[][]{new object[] {"IBM", 75d}, new object[] {null, 87d}, new object[] {"YAH", 3d}, new object[] {null, 109d}});
            expected.AddResultInsRem(6200, 0,
                    new object[][]{new object[] {"IBM", 72d}, new object[] {null, 87d}, new object[] {"YAH", 7d}, new object[] {null, 88d}},
                    new object[][]{new object[] {"IBM", 97d}, new object[] {null, 112d}, new object[] {"YAH", 6d}, new object[] {null, 87d}});
            expected.AddResultInsRem(7200, 0,
                    new object[][]{new object[] {"MSFT", null}, new object[] {null, 79d}, new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}},
                    new object[][]{new object[] {"MSFT", 9d}, new object[] {null, 88d}, new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 79d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertion3OutputLimitAll(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            string stmtText = hint + "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by Rollup(symbol)" +
                    "output all every 1 seconds";
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
            expected.AddResultInsRem(1200, 0,
                    new object[][]{new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34d}},
                    new object[][]{new object[] {"IBM", null}, new object[] {"MSFT", null}, new object[] {null, null}});
            expected.AddResultInsRem(2200, 0,
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}},
                    new object[][]{new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {"YAH", null}, new object[] {null, 34d}});
            expected.AddResultInsRem(3200, 0,
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}},
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}});
            expected.AddResultInsRem(4200, 0,
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}, new object[] {null, 87d}},
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85d}});
            expected.AddResultInsRem(5200, 0,
                    new object[][]{new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}, new object[] {null, 112d}},
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}, new object[] {null, 87d}});
            expected.AddResultInsRem(6200, 0,
                    new object[][]{new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}, new object[] {null, 88d}},
                    new object[][]{new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}, new object[] {null, 112d}});
            expected.AddResultInsRem(7200, 0,
                    new object[][]{new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}, new object[] {null, 54d}},
                    new object[][]{new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}, new object[] {null, 88d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(true);
        }
    
        private void RunAssertion4OutputLimitLast(EPServiceProvider epService, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            string stmtText = hint + "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by Rollup(symbol)" +
                    "output last every 1 seconds";
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
            expected.AddResultInsRem(1200, 0,
                    new object[][]{new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34d}},
                    new object[][]{new object[] {"IBM", null}, new object[] {"MSFT", null}, new object[] {null, null}});
            expected.AddResultInsRem(2200, 0,
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"YAH", 1d}, new object[] {null, 85d}},
                    new object[][]{new object[] {"IBM", 25d}, new object[] {"YAH", null}, new object[] {null, 34d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0,
                    new object[][]{new object[] {"YAH", 3d}, new object[] {null, 87d}},
                    new object[][]{new object[] {"YAH", 1d}, new object[] {null, 85d}});
            expected.AddResultInsRem(5200, 0,
                    new object[][]{new object[] {"IBM", 97d}, new object[] {"YAH", 6d}, new object[] {null, 112d}},
                    new object[][]{new object[] {"IBM", 75d}, new object[] {"YAH", 3d}, new object[] {null, 87d}});
            expected.AddResultInsRem(6200, 0,
                    new object[][]{new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 88d}},
                    new object[][]{new object[] {"IBM", 97d}, new object[] {"YAH", 6d}, new object[] {null, 112d}});
            expected.AddResultInsRem(7200, 0,
                    new object[][]{new object[] {"MSFT", null}, new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}},
                    new object[][]{new object[] {"MSFT", 9d}, new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 88d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(true);
        }
    
        private void RunAssertion5OutputLimitFirst(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by Rollup(symbol)" +
                    "output first every 1 seconds";
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
            expected.AddResultInsRem(200, 1, new object[][]{new object[] {"IBM", 25d}, new object[] {null, 25d}}, new object[][] {new object[] {"IBM", null}, new object[] {null, null}});
            expected.AddResultInsRem(800, 1, new object[][]{new object[] {"MSFT", 9d}}, new object[][] {new object[] {"MSFT", null}});
            expected.AddResultInsRem(1500, 1, new object[][]{new object[] {"IBM", 49d}, new object[] {null, 58d}}, new object[][] {new object[] {"IBM", 25d}, new object[] {null, 34d}});
            expected.AddResultInsRem(1500, 2, new object[][]{new object[] {"YAH", 1d}}, new object[][] {new object[] {"YAH", null}});
            expected.AddResultInsRem(3500, 1, new object[][]{new object[] {"YAH", 3d}, new object[] {null, 87d}}, new object[][] {new object[] {"YAH", 1d}, new object[] {null, 85d}});
            expected.AddResultInsRem(4300, 1, new object[][]{new object[] {"IBM", 97d}}, new object[][] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(4900, 1, new object[][]{new object[] {"YAH", 6d}, new object[] {null, 112d}}, new object[][] {new object[] {"YAH", 3d}, new object[] {null, 109d}});
            expected.AddResultInsRem(5700, 0, new object[][]{new object[] {"IBM", 72d}}, new object[][] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(5900, 1, new object[][]{new object[] {"YAH", 7d}, new object[] {null, 88d}}, new object[][] {new object[] {"YAH", 6d}, new object[] {null, 87d}});
            expected.AddResultInsRem(6300, 0, new object[][]{new object[] {"MSFT", null}}, new object[][] {new object[] {"MSFT", 9d}});
            expected.AddResultInsRem(7000, 0, new object[][]{new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54d}}, new object[][] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 79d}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertion6OutputLimitSnapshot(EPServiceProvider epService) {
            string stmtText = "select symbol, sum(price) " +
                    "from MarketData#time(5.5 sec)" +
                    "group by Rollup(symbol)" +
                    "output snapshot every 1 seconds";
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"symbol", "sum(price)"};
            var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
            expected.AddResultInsert(1200, 0, new object[][]{new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {null, 34.0}});
            expected.AddResultInsert(2200, 0, new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85.0}});
            expected.AddResultInsert(3200, 0, new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}, new object[] {null, 85.0}});
            expected.AddResultInsert(4200, 0, new object[][]{new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}, new object[] {null, 87.0}});
            expected.AddResultInsert(5200, 0, new object[][]{new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}, new object[] {null, 112.0}});
            expected.AddResultInsert(6200, 0, new object[][]{new object[] {"MSFT", 9d}, new object[] {"IBM", 72d}, new object[] {"YAH", 7d}, new object[] {null, 88.0}});
            expected.AddResultInsert(7200, 0, new object[][]{new object[] {"IBM", 48d}, new object[] {"YAH", 6d}, new object[] {null, 54.0}});
    
            var execution = new ResultAssertExecution(epService, stmt, listener, expected);
            execution.Execute(false);
        }
    
        private void RunAssertionOutputFirstHaving(EPServiceProvider epService, bool join) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "having sum(LongPrimitive) > 100 " +
                    "output first every 1 second").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", null, 110L}, new object[] {null, null, 150L}},
                    new object[][]{new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", null, 170L}, new object[] {null, null, 210L}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}},
                    new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000)); // removes the first 3 events
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}},
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000)); // removes the second 2 events
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", null, 210L}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", null, 210L}, new object[] {null, null, 210L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 300L}},
                    new object[][]{new object[] {"E1", 1, 300L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000)); // removes the third 1 event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
                    new object[][]{new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}});
        }
    
        private void RunAssertionOutputFirst(EPServiceProvider epService, bool join) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output first every 1 second").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 10L}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L}},
                    new object[][]{new object[] {"E1", 1, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 2, 20L}},
                    new object[][]{new object[] {"E1", 2, null}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E2", 1, 40L}, new object[] {"E2", null, 40L}, new object[] {null, null, 100L}},
                    new object[][]{new object[] {"E2", 1, null}, new object[] {"E2", null, null}, new object[] {null, null, 60L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 2, 70L}, new object[] {"E1", null, 110L}},
                    new object[][]{new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}},
                    new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000)); // removes the first 3 events
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}},
                    new object[][]{new object[] {"E1", 1, 170L}, new object[] {"E1", 2, 70L}, new object[] {"E1", null, 240L}, new object[] {null, null, 280L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000)); // removes the second 2 events
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E2", 1, null}, new object[]{"E1", 2, null}, new object[]{"E2", null, null},
                        new object[]{"E1", null, 210L}, new object[]{null, null, 210L}},
                    new object[][]{
                        new object[]{"E2", 1, 40L}, new object[]{"E1", 2, 50L}, new object[]{"E2", null, 40L},
                        new object[]{"E1", null, 260L}, new object[]{null, null, 300L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 300L}},
                    new object[][]{new object[] {"E1", 1, 210L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000)); // removes the third 1 event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
                    new object[][]{new object[] {"E1", 1, 300L}, new object[] {"E1", null, 300L}, new object[] {null, null, 300L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputFirstSorted(EPServiceProvider epService, bool join) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output first every 1 second " +
                    "order by TheString, IntPrimitive").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 10L}, new object[] {"E1", null, 10L}, new object[] {"E1", 1, 10L}},
                    new object[][]{new object[] {null, null, null}, new object[] {"E1", null, null}, new object[] {"E1", 1, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 2, 20L}},
                    new object[][]{new object[] {"E1", 2, null}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 100L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
                    new object[][]{new object[] {null, null, 60L}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", null, 110L}, new object[] {"E1", 2, 70L}},
                    new object[][]{new object[] {"E1", null, 60L}, new object[] {"E1", 2, 20L}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}},
                    new object[][]{new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}});
    
            // pass 1 second
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 280L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 170L}},
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4000)); // removes the first 3 events
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}},
                    new object[][]{new object[] {null, null, 280L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 170L}, new object[] {"E1", 2, 70L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000)); // removes the second 2 events
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 2, null},
                        new object[]{"E2", null, null}, new object[]{"E2", 1, null}},
                    new object[][]{new object[] {null, null, 300L}, new object[] {"E1", null, 260L}, new object[] {"E1", 2, 50L},
                        new object[]{"E2", null, 40L}, new object[]{"E2", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 300L}},
                    new object[][]{new object[] {"E1", 1, 210L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000)); // removes the third 1 event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 240L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 240L}},
                    new object[][]{new object[] {null, null, 300L}, new object[] {"E1", null, 300L}, new object[] {"E1", 1, 300L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputDefault(EPServiceProvider epService, bool join) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output every 1 second").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E1", 1, 10L}, new object[]{"E1", null, 10L}, new object[]{null, null, 10L},
                        new object[]{"E1", 2, 20L}, new object[]{"E1", null, 30L}, new object[]{null, null, 30L},
                        new object[]{"E1", 1, 40L}, new object[]{"E1", null, 60L}, new object[]{null, null, 60L}},
                    new object[][]{
                        new object[]{"E1", 1, null}, new object[]{"E1", null, null}, new object[]{null, null, null},
                        new object[]{"E1", 2, null}, new object[]{"E1", null, 10L}, new object[]{null, null, 10L},
                        new object[]{"E1", 1, 10L}, new object[]{"E1", null, 30L}, new object[]{null, null, 30L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E2", 1, 40L}, new object[]{"E2", null, 40L}, new object[]{null, null, 100L},
                        new object[]{"E1", 2, 70L}, new object[]{"E1", null, 110L}, new object[]{null, null, 150L}},
                    new object[][]{
                        new object[]{"E2", 1, null}, new object[]{"E2", null, null}, new object[]{null, null, 60L},
                        new object[]{"E1", 2, 20L}, new object[]{"E1", null, 60L}, new object[]{null, null, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E1", 1, 100L}, new object[]{"E1", null, 170L}, new object[]{null, null, 210L}},
                    new object[][]{
                        new object[]{"E1", 1, 40L}, new object[]{"E1", null, 110L}, new object[]{null, null, 150L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E1", 1, 170L}, new object[]{"E1", null, 240L}, new object[]{null, null, 280L},
                        new object[]{"E1", 1, 130L}, new object[]{"E1", 2, 50L}, new object[]{"E1", null, 180L}, new object[]{null, null, 220L},
                    },
                    new object[][]{
                        new object[]{"E1", 1, 100L}, new object[]{"E1", null, 170L}, new object[]{null, null, 210L},
                        new object[]{"E1", 1, 170L}, new object[]{"E1", 2, 70L}, new object[]{"E1", null, 240L}, new object[]{null, null, 280L},
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E1", 1, 210L}, new object[]{"E1", null, 260L}, new object[]{null, null, 300L},
                        new object[]{"E2", 1, null}, new object[]{"E1", 2, null}, new object[]{"E2", null, null}, new object[]{"E1", null, 210L}, new object[]{null, null, 210L},
                    },
                    new object[][]{
                        new object[]{"E1", 1, 130L}, new object[]{"E1", null, 180L}, new object[]{null, null, 220L},
                        new object[]{"E2", 1, 40L}, new object[]{"E1", 2, 50L}, new object[]{"E2", null, 40L}, new object[]{"E1", null, 260L}, new object[]{null, null, 300L},
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{"E1", 1, 300L}, new object[]{"E1", null, 300L}, new object[]{null, null, 300L},
                        new object[]{"E1", 1, 240L}, new object[]{"E1", null, 240L}, new object[]{null, null, 240L}},
                    new object[][]{
                        new object[]{"E1", 1, 210L}, new object[]{"E1", null, 210L}, new object[]{null, null, 210L},
                        new object[]{"E1", 1, 300L}, new object[]{"E1", null, 300L}, new object[]{null, null, 300L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputDefaultSorted(EPServiceProvider epService, bool join) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output every 1 second " +
                    "order by TheString, IntPrimitive").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{null, null, 10L}, new object[]{null, null, 30L}, new object[]{null, null, 60L},
                        new object[]{"E1", null, 10L}, new object[]{"E1", null, 30L}, new object[]{"E1", null, 60L},
                        new object[]{"E1", 1, 10L}, new object[]{"E1", 1, 40L}, new object[]{"E1", 2, 20L}},
                    new object[][]{
                        new object[]{null, null, null}, new object[]{null, null, 10L}, new object[]{null, null, 30L},
                        new object[]{"E1", null, null}, new object[]{"E1", null, 10L}, new object[]{"E1", null, 30L},
                        new object[]{"E1", 1, null}, new object[]{"E1", 1, 10L}, new object[]{"E1", 2, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{null, null, 100L}, new object[]{null, null, 150L},
                        new object[]{"E1", null, 110L}, new object[]{"E1", 2, 70L},
                        new object[]{"E2", null, 40L}, new object[]{"E2", 1, 40L}},
                    new object[][]{
                        new object[]{null, null, 60L}, new object[]{null, null, 100L},
                        new object[]{"E1", null, 60L}, new object[]{"E1", 2, 20L},
                        new object[]{"E2", null, null}, new object[]{"E2", 1, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{null, null, 210L}, new object[]{"E1", null, 170L}, new object[]{"E1", 1, 100L}},
                    new object[][]{
                        new object[]{null, null, 150L}, new object[]{"E1", null, 110L}, new object[]{"E1", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{null, null, 280L}, new object[]{null, null, 220L},
                        new object[]{"E1", null, 240L}, new object[]{"E1", null, 180L},
                        new object[]{"E1", 1, 170L}, new object[]{"E1", 1, 130L}, new object[]{"E1", 2, 50L}},
                    new object[][]{
                        new object[]{null, null, 210L}, new object[]{null, null, 280L},
                        new object[]{"E1", null, 170L}, new object[]{"E1", null, 240L},
                        new object[]{"E1", 1, 100L}, new object[]{"E1", 1, 170L}, new object[]{"E1", 2, 70L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{null, null, 300L}, new object[]{null, null, 210L},
                        new object[]{"E1", null, 260L}, new object[]{"E1", null, 210L},
                        new object[]{"E1", 1, 210L}, new object[]{"E1", 2, null}, new object[]{"E2", null, null}, new object[]{"E2", 1, null}},
                    new object[][]{
                        new object[]{null, null, 220L}, new object[]{null, null, 300L},
                        new object[]{"E1", null, 180L}, new object[]{"E1", null, 260L},
                        new object[]{"E1", 1, 130L}, new object[]{"E1", 2, 50L}, new object[]{"E2", null, 40L}, new object[]{"E2", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{
                        new object[]{null, null, 300L}, new object[]{null, null, 240L},
                        new object[]{"E1", null, 300L}, new object[]{"E1", null, 240L},
                        new object[]{"E1", 1, 300L}, new object[]{"E1", 1, 240L}},
                    new object[][]{
                        new object[]{null, null, 210L}, new object[]{null, null, 300L},
                        new object[]{"E1", null, 210L}, new object[]{"E1", null, 300L},
                        new object[]{"E1", 1, 210L}, new object[]{"E1", 1, 300L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputAll(EPServiceProvider epService, bool join, bool hinted) {
    
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(hint + "@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output all every 1 second").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}},
                    new object[][]{new object[] {"E1", 1, null}, new object[] {"E1", 2, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {"E2", null, 40L}, new object[] {null, null, 150L}},
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E2", 1, null}, new object[] {"E1", null, 60L}, new object[] {"E2", null, null}, new object[] {null, null, 60L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 170L}, new object[] {"E2", null, 40L}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {"E2", null, 40L}, new object[] {null, null, 150L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 180L}, new object[] {"E2", null, 40L}, new object[] {null, null, 220L}},
                    new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 170L}, new object[] {"E2", null, 40L}, new object[] {null, null, 210L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", 1, null}, new object[] {"E1", null, 210L}, new object[] {"E2", null, null}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", 1, 40L}, new object[] {"E1", null, 180L}, new object[] {"E2", null, 40L}, new object[] {null, null, 220L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 240L}, new object[] {"E1", 2, null}, new object[] {"E2", 1, null}, new object[] {"E1", null, 240L}, new object[] {"E2", null, null}, new object[] {null, null, 240L}},
                    new object[][]{new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", 1, null}, new object[] {"E1", null, 210L}, new object[] {"E2", null, null}, new object[] {null, null, 210L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputAllSorted(EPServiceProvider epService, bool join) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output all every 1 second " +
                    "order by TheString, IntPrimitive").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}},
                    new object[][]{new object[] {null, null, null}, new object[] {"E1", null, null}, new object[] {"E1", 1, null}, new object[] {"E1", 2, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
                    new object[][]{new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
                    new object[][]{new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
                    new object[][]{new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 240L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 240L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputLast(EPServiceProvider epService, bool hinted, bool join) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(hint + "@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output last every 1 second").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}},
                    new object[][]{new object[] {"E1", 1, null}, new object[] {"E1", 2, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E2", 1, 40L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}},
                    new object[][]{new object[] {"E2", 1, null}, new object[] {"E1", 2, 20L}, new object[] {"E2", null, null}, new object[] {"E1", null, 60L}, new object[] {null, null, 60L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", 1, 40L}, new object[] {"E1", null, 110L}, new object[] {null, null, 150L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000)); // removes the first 3 events
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E1", null, 180L}, new object[] {null, null, 220L}},
                    new object[][]{new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}, new object[] {"E1", null, 170L}, new object[] {null, null, 210L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000)); // removes the second 2 events
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 210L}, new object[] {"E2", 1, null}, new object[] {"E1", 2, null}, new object[] {"E1", null, 210L}, new object[] {"E2", null, null}, new object[] {null, null, 210L}},
                    new object[][]{new object[] {"E1", 1, 130L}, new object[] {"E2", 1, 40L}, new object[] {"E1", 2, 50L}, new object[] {"E1", null, 180L}, new object[] {"E2", null, 40L}, new object[] {null, null, 220L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000)); // removes the third 1 event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1", 1, 240L}, new object[] {"E1", null, 240L}, new object[] {null, null, 240L}},
                    new object[][]{new object[] {"E1", 1, 210L}, new object[] {"E1", null, 210L}, new object[] {null, null, 210L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputLastSorted(EPServiceProvider epService, bool join) {
    
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#time(3.5 sec) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive) " +
                    "output last every 1 second " +
                    "order by TheString, IntPrimitive").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 30L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 1, 40L}, new object[] {"E1", 2, 20L}},
                    new object[][]{new object[] {null, null, null}, new object[] {"E1", null, null}, new object[] {"E1", 1, null}, new object[] {"E1", 2, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 40L));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 50L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 2, 70L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}},
                    new object[][]{new object[] {null, null, 60L}, new object[] {"E1", null, 60L}, new object[] {"E1", 2, 20L}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 60L));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}},
                    new object[][]{new object[] {null, null, 150L}, new object[] {"E1", null, 110L}, new object[] {"E1", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 70L));    // removes the first 3 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}},
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 170L}, new object[] {"E1", 1, 100L}, new object[] {"E1", 2, 70L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 80L));    // removes the second 2 events
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(5000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}, new object[] {"E1", 2, null}, new object[] {"E2", null, null}, new object[] {"E2", 1, null}},
                    new object[][]{new object[] {null, null, 220L}, new object[] {"E1", null, 180L}, new object[] {"E1", 1, 130L}, new object[] {"E1", 2, 50L}, new object[] {"E2", null, 40L}, new object[] {"E2", 1, 40L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 90L));    // removes the third 1 event
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(6000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {null, null, 240L}, new object[] {"E1", null, 240L}, new object[] {"E1", 1, 240L}},
                    new object[][]{new object[] {null, null, 210L}, new object[] {"E1", null, 210L}, new object[] {"E1", 1, 210L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            return sb;
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
