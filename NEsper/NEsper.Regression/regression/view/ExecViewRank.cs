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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    // Ranked-Window tests.
    //   - retains the last event per unique key as long as within rank
    //   - retains the newest event for a given rank: same-rank new events push out old events within the same rank when overflowing.
    //
    // Related to the ranked data window is the following:
    // ext:Sort(10, p00)                            Maintain the top 10 events sorted by p00-value
    // std:Groupwin(p00)#Sort(10, p01 desc)     For each p00-value maintain the top 10 events sorted by p01 desc
    // SupportBean#unique(string)#Sort(3, intPrimitive)  Intersection NOT applicable because E1-3, E2-2, E3-1 then E2-4 causes E2-2 to go out of window
    // ... order by p00 desc limit 8 offset 2       This can rank, however it may retain too data (such as count per word); also cannot use window(*) on rank data
    // - it is a data window because it retains events, works with 'prev' (its sorted), works with 'window(*)', is iterable
    // - is is not an aggregation (regular or data window) because aggregations don't decide how many events to retain
    public class ExecViewRank : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            RunAssertionPrevAndGroupWin(epService);
            RunAssertionMultiexpression(epService);
            RunAssertionRemoveStream(epService);
            RunAssertionRanked(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionPrevAndGroupWin(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select Prevwindow(ev) as win, Prev(0, ev) as prev0, Prev(1, ev) as prev1, Prev(2, ev) as prev2, Prev(3, ev) as prev3, Prev(4, ev) as prev4 " +
                    "from SupportBean#Rank(theString, 3, intPrimitive) as ev");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 0L));
            AssertWindowAggAndPrev(listener, new Object[][]{new object[] {"E1", 100, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 99, 0L));
            AssertWindowAggAndPrev(listener, new Object[][]{new object[] {"E2", 99, 0L}, new object[] {"E1", 100, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 98, 1L));
            AssertWindowAggAndPrev(listener, new Object[][]{new object[] {"E1", 98, 1L}, new object[] {"E2", 99, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 98, 0L));
            AssertWindowAggAndPrev(listener, new Object[][]{new object[] {"E1", 98, 1L}, new object[] {"E3", 98, 0L}, new object[] {"E2", 99, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 97, 1L));
            AssertWindowAggAndPrev(listener, new Object[][]{new object[] {"E2", 97, 1L}, new object[] {"E1", 98, 1L}, new object[] {"E3", 98, 0L}});
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#Groupwin(theString)#Rank(intPrimitive, 2, doublePrimitive) as ev");
            stmt.AddListener(listener);
    
            string[] fields = "theString,intPrimitive,longPrimitive,doublePrimitive".Split(',');
            epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 0L, 1d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 100, 0L, 1d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 0L, 1d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 100, 0L, 2d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 100, 0L, 2d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 0L, 1d}, new object[] {"E2", 100, 0L, 2d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 200, 0L, 0.5d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 200, 0L, 0.5d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 200, 0L, 0.5d}, new object[] {"E1", 100, 0L, 1d}, new object[] {"E2", 100, 0L, 2d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 200, 0L, 2.5d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 200, 0L, 2.5d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 200, 0L, 0.5d}, new object[] {"E1", 100, 0L, 1d}, new object[] {"E2", 100, 0L, 2d}, new object[] {"E2", 200, 0L, 2.5d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 300, 0L, 0.1d));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E1", 300, 0L, 0.1d}, new Object[]{"E1", 100, 0L, 1d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 300, 0L, 0.1d}, new object[] {"E1", 200, 0L, 0.5d}, new object[] {"E2", 100, 0L, 2d}, new object[] {"E2", 200, 0L, 2.5d}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiexpression(EPServiceProvider epService) {
            string[] fields = "theString,intPrimitive,longPrimitive,doublePrimitive".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#Rank(theString, intPrimitive, 3, longPrimitive, doublePrimitive)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 1L, 10d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 100, 1L, 10d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 1L, 10d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 200, 1L, 9d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 200, 1L, 9d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 200, 1L, 9d}, new object[] {"E1", 100, 1L, 10d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 150, 1L, 11d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 150, 1L, 11d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 200, 1L, 9d}, new object[] {"E1", 100, 1L, 10d}, new object[] {"E1", 150, 1L, 11d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 1L, 8d));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E1", 100, 1L, 8d}, new Object[]{"E1", 100, 1L, 10d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 1L, 8d}, new object[] {"E1", 200, 1L, 9d}, new object[] {"E1", 150, 1L, 11d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 300, 2L, 7d));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E2", 300, 2L, 7d}, new Object[]{"E2", 300, 2L, 7d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 1L, 8d}, new object[] {"E1", 200, 1L, 9d}, new object[] {"E1", 150, 1L, 11d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 300, 1L, 8.5d));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E3", 300, 1L, 8.5d}, new Object[]{"E1", 150, 1L, 11d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 1L, 8d}, new object[] {"E3", 300, 1L, 8.5d}, new object[] {"E1", 200, 1L, 9d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E4", 400, 1L, 9d));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E4", 400, 1L, 9d}, new Object[]{"E1", 200, 1L, 9d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 100, 1L, 8d}, new object[] {"E3", 300, 1L, 8.5d}, new object[] {"E4", 400, 1L, 9d}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRemoveStream(EPServiceProvider epService) {
            string[] fields = "theString,intPrimitive,longPrimitive".Split(',');
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create window MyWindow#Rank(theString, 3, intPrimitive asc) as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmtListen = epService.EPAdministrator.CreateEPL("select irstream * from MyWindow");
            var listener = new SupportUpdateListener();
            stmtListen.AddListener(listener);
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow mw where theString = id");
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 10, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 50, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 10, 0L}, new object[] {"E2", 50, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 5, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 5, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E3", 5, 0L}, new object[] {"E1", 10, 0L}, new object[] {"E2", 50, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E4", 5, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E4", 5, 0L}, new Object[]{"E2", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E3", 5, 0L}, new object[] {"E4", 5, 0L}, new object[] {"E1", 10, 0L}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new Object[]{"E3", 5, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E4", 5, 0L}, new object[] {"E1", 10, 0L}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new Object[]{"E4", 5, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 10, 0L}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new Object[]{"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[0][]);
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 100, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 100, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E3", 100, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 101, 1L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E3", 101, 1L}, new Object[]{"E3", 100, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]{new object[] {"E3", 101, 1L}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new Object[]{"E3", 101, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[0][]);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionRanked(EPServiceProvider epService) {
            string[] fields = "theString,intPrimitive,longPrimitive".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#Rank(theString, 4, intPrimitive desc)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            // sorting-related testing
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 10, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 30, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 30, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 30, 0L}, new object[] {"E1", 10, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 50, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E1", 50, 0L}, new Object[]{"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 50, 0L}, new object[] {"E2", 30, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 40, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", 40, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 50, 0L}, new object[] {"E3", 40, 0L}, new object[] {"E2", 30, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 45, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E2", 45, 0L}, new Object[]{"E2", 30, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E1", 50, 0L}, new object[] {"E2", 45, 0L}, new object[] {"E3", 40, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 43, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E1", 43, 0L}, new Object[]{"E1", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E3", 40, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 50, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E3", 50, 0L}, new Object[]{"E3", 40, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E3", 50, 0L}, new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 10, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E3", 10, 0L}, new Object[]{"E3", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E3", 10, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E4", 43, 0L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E4", 43, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E4", 43, 0L}, new object[] {"E3", 10, 0L}});
    
            // in-place replacement
            epService.EPRuntime.SendEvent(MakeEvent("E4", 43, 1L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E4", 43, 1L}, new Object[]{"E4", 43, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E4", 43, 1L}, new object[] {"E3", 10, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 45, 1L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E2", 45, 1L}, new Object[]{"E2", 45, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 1L}, new object[] {"E1", 43, 0L}, new object[] {"E4", 43, 1L}, new object[] {"E3", 10, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 43, 1L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E1", 43, 1L}, new Object[]{"E1", 43, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L}, new object[] {"E3", 10, 0L}});
    
            // out-of-space: pushing out the back end
            epService.EPRuntime.SendEvent(MakeEvent("E5", 10, 2L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E5", 10, 2L}, new Object[]{"E3", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L}, new object[] {"E5", 10, 2L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E5", 11, 3L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E5", 11, 3L}, new Object[]{"E5", 10, 2L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L}, new object[] {"E5", 11, 3L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E6", 43, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E6", 43, 0L}, new Object[]{"E5", 11, 3L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L}, new object[] {"E6", 43, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E7", 50, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E7", 50, 0L}, new Object[]{"E4", 43, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E7", 50, 0L}, new object[] {"E2", 45, 1L}, new object[] {"E1", 43, 1L}, new object[] {"E6", 43, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E8", 45, 0L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E8", 45, 0L}, new Object[]{"E1", 43, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E7", 50, 0L}, new object[] {"E2", 45, 1L}, new object[] {"E8", 45, 0L}, new object[] {"E6", 43, 0L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E8", 46, 1L));
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new Object[]{"E8", 46, 1L}, new Object[]{"E8", 45, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new object[] {"E7", 50, 0L}, new object[] {"E8", 46, 1L}, new object[] {"E2", 45, 1L}, new object[] {"E6", 43, 0L}});
    
            stmt.Dispose();
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            return bean;
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select * from SupportBean#Rank(1, intPrimitive desc)",
                    "Error starting statement: Error attaching view to event stream: Rank view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys [select * from SupportBean#Rank(1, intPrimitive desc)]");
    
            TryInvalid(epService, "select * from SupportBean#Rank(1, intPrimitive, theString desc)",
                    "Error starting statement: Error attaching view to event stream: Failed to find unique value expressions that are expected to occur before the numeric size parameter [select * from SupportBean#Rank(1, intPrimitive, theString desc)]");
    
            TryInvalid(epService, "select * from SupportBean#Rank(theString, intPrimitive, 1)",
                    "Error starting statement: Error attaching view to event stream: Failed to find sort key expressions after the numeric size parameter [select * from SupportBean#Rank(theString, intPrimitive, 1)]");
    
            TryInvalid(epService, "select * from SupportBean#Rank(theString, intPrimitive, theString desc)",
                    "Error starting statement: Error attaching view to event stream: Failed to find constant value for the numeric size parameter [select * from SupportBean#Rank(theString, intPrimitive, theString desc)]");
    
            TryInvalid(epService, "select * from SupportBean#Rank(theString, 1, 1, intPrimitive, theString desc)",
                    "Error starting statement: Error attaching view to event stream: Invalid view parameter expression 2 for Rank view, the expression returns a constant result value, are you sure? [select * from SupportBean#Rank(theString, 1, 1, intPrimitive, theString desc)]");
    
            TryInvalid(epService, "select * from SupportBean#Rank(theString, intPrimitive, 1, intPrimitive, 1, theString desc)",
                    "Error starting statement: Error attaching view to event stream: Invalid view parameter expression 4 for Rank view, the expression returns a constant result value, are you sure? [select * from SupportBean#Rank(theString, intPrimitive, 1, intPrimitive, 1, theString desc)]");
        }
    
        private void AssertWindowAggAndPrev(SupportUpdateListener listener, Object[][] expected) {
            string[] fields = "theString,intPrimitive,longPrimitive".Split(',');
            EventBean @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertPropsPerRow((Object[]) @event.Get("win"), fields, expected);
            for (int i = 0; i < 5; i++) {
                Object prevValue = @event.Get("prev" + i);
                if (prevValue == null && expected.Length <= i) {
                    continue;
                }
                EPAssertionUtil.AssertProps(prevValue, fields, expected[i]);
            }
        }
    }
} // end of namespace
