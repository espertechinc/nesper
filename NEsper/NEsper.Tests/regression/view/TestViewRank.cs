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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


// Ranked-Window tests.
//   - retains the last event per unique key as long as within rank
//   - retains the newest event for a given rank: same-rank new events push out old events within the same rank when overflowing.
//
// Related to the ranked data window is the following:
// ext:sort(10, p00)                            Maintain the top 10 events sorted by p00-value
// std:groupwin(p00).ext:sort(10, p01 desc)     For each p00-value maintain the top 10 events sorted by p01 desc
// SupportBean.std:unique(string).ext:sort(3, intPrimitive)  Intersection NOT applicable because E1-3, E2-2, E3-1 then E2-4 causes E2-2 to go out of window
// ... order by p00 desc limit 8 offset 2       This can rank, however it may retain too data (such as count per word); also cannot use Window(*) on rank data
// - it is a data window because it retains events, works with 'prev' (its sorted), works with 'window(*)', is iterable
// - is is not an aggregation (regular or data window) because aggregations don't decide how many events to retain
namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewRank
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestPrevAndGroupWin() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select prevwindow(ev) as win, prev(0, ev) as prev0, prev(1, ev) as prev1, prev(2, ev) as prev2, prev(3, ev) as prev3, prev(4, ev) as prev4 " +
                    "from SupportBean.ext:rank(TheString, 3, IntPrimitive) as ev");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 0L));
            AssertWindowAggAndPrev(new Object[][]  { new Object[] {"E1", 100, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 99, 0L));
            AssertWindowAggAndPrev(new Object[][]  { new Object[] {"E2", 99, 0L}, new Object[] {"E1", 100, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 98, 1L));
            AssertWindowAggAndPrev(new Object[][]  { new Object[] {"E1", 98, 1L}, new Object[] {"E2", 99, 0L}, });
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 98, 0L));
            AssertWindowAggAndPrev(new Object[][]  { new Object[] {"E1", 98, 1L}, new Object[] {"E3", 98, 0L}, new Object[] {"E2", 99, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 97, 1L));
            AssertWindowAggAndPrev(new Object[][]  { new Object[] {"E2", 97, 1L}, new Object[] {"E1", 98, 1L}, new Object[] {"E3", 98, 0L}});
            stmt.Dispose();

            stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.std:groupwin(TheString).ext:rank(IntPrimitive, 2, DoublePrimitive) as ev");
            stmt.Events += _listener.Update;

            String[] fields = "TheString,IntPrimitive,LongPrimitive,DoublePrimitive".Split(',');
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 0L, 1d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 100, 0L, 1d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 0L, 1d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 100, 0L, 2d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 100, 0L, 2d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 0L, 1d}, new Object[] {"E2", 100, 0L, 2d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 200, 0L, 0.5d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 200, 0L, 0.5d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 200, 0L, 0.5d}, new Object[] {"E1", 100, 0L, 1d}, new Object[] {"E2", 100, 0L, 2d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 200, 0L, 2.5d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 200, 0L, 2.5d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 200, 0L, 0.5d}, new Object[] {"E1", 100, 0L, 1d}, new Object[] {"E2", 100, 0L, 2d}, new Object[] {"E2", 200, 0L, 2.5d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 300, 0L, 0.1d));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E1", 300, 0L, 0.1d}, new Object[] {"E1", 100, 0L, 1d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 300, 0L, 0.1d}, new Object[] {"E1", 200, 0L, 0.5d}, new Object[] {"E2", 100, 0L, 2d}, new Object[] {"E2", 200, 0L, 2.5d}});
        }
    
        private void AssertWindowAggAndPrev(Object[][] expected) {
            String[] fields = "TheString,IntPrimitive,LongPrimitive".Split(',');
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertPropsPerRow((Object[]) theEvent.Get("win"), fields, expected);
            for (int i = 0; i < 5; i++) {
                Object prevValue = theEvent.Get("prev" + i);
                if (prevValue == null && expected.Length <= i) {
                    continue;
                }
                EPAssertionUtil.AssertProps(prevValue, fields, expected[i]);
            }
        }
    
        [Test]
        public void TestMultiexpression() {
            String[] fields = "TheString,IntPrimitive,LongPrimitive,DoublePrimitive".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.ext:rank(TheString, IntPrimitive, 3, LongPrimitive, DoublePrimitive)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 1L, 10d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 100, 1L, 10d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 1L, 10d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 200, 1L, 9d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 200, 1L, 9d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 200, 1L, 9d}, new Object[] {"E1", 100, 1L, 10d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 150, 1L, 11d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 150, 1L, 11d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 200, 1L, 9d}, new Object[] {"E1", 100, 1L, 10d}, new Object[] {"E1", 150, 1L, 11d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 100, 1L, 8d));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E1", 100, 1L, 8d}, new Object[] {"E1", 100, 1L, 10d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 1L, 8d}, new Object[] {"E1", 200, 1L, 9d}, new Object[] {"E1", 150, 1L, 11d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 300, 2L, 7d));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E2", 300, 2L, 7d}, new Object[] {"E2", 300, 2L, 7d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 1L, 8d}, new Object[] {"E1", 200, 1L, 9d}, new Object[] {"E1", 150, 1L, 11d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 300, 1L, 8.5d));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E3", 300, 1L, 8.5d}, new Object[] {"E1", 150, 1L, 11d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 1L, 8d}, new Object[] {"E3", 300, 1L, 8.5d}, new Object[] {"E1", 200, 1L, 9d}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E4", 400, 1L, 9d));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E4", 400, 1L, 9d}, new Object[] {"E1", 200, 1L, 9d});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 100, 1L, 8d}, new Object[] {"E3", 300, 1L, 8.5d}, new Object[] {"E4", 400, 1L, 9d}});
        }
    
        [Test]
        public void TestRemoveStream() {
            String[] fields = "TheString,IntPrimitive,LongPrimitive".Split(',');
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL("create window MyWindow.ext:rank(TheString, 3, IntPrimitive asc) as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmtListen = _epService.EPAdministrator.CreateEPL("select irstream * from MyWindow");
            stmtListen.Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow mw where TheString = id");
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 50, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 10, 0L}, new Object[] {"E2", 50, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 5, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 5, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E3", 5, 0L}, new Object[] {"E1", 10, 0L}, new Object[] {"E2", 50, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E4", 5, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E4", 5, 0L}, new Object[] {"E2", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E3", 5, 0L}, new Object[] {"E4", 5, 0L}, new Object[] {"E1", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 5, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E4", 5, 0L}, new Object[] {"E1", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E4", 5, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[0][]);
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 100, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 100, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E3", 100, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 101, 1L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E3", 101, 1L}, new Object[] {"E3", 100, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[][]  { new Object[] {"E3", 101, 1L}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E3", 101, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[0][]);
        }
    
        [Test]
        public void TestRanked()
        {
            String[] fields = "TheString,IntPrimitive,LongPrimitive".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.ext:rank(TheString, 4, IntPrimitive desc)");
            stmt.Events += _listener.Update;
    
            // sorting-related testing
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 30, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 30, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E2", 30, 0L}, new Object[] {"E1", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 50, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E1", 50, 0L}, new Object[] {"E1", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 50, 0L}, new Object[] {"E2", 30, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 40, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 40, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 50, 0L}, new Object[] {"E3", 40, 0L}, new Object[] {"E2", 30, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 45, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E2", 45, 0L}, new Object[] {"E2", 30, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E1", 50, 0L}, new Object[] {"E2", 45, 0L}, new Object[] {"E3", 40, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 43, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E1", 43, 0L}, new Object[] {"E1", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E2", 45, 0L}, new Object[] {"E1", 43, 0L}, new Object[] {"E3", 40, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 50, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E3", 50, 0L}, new Object[] {"E3", 40, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E3", 50, 0L}, new Object[] {"E2", 45, 0L}, new Object[] {"E1", 43, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 10, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[]{"E3", 10, 0L}, new Object[]{"E3", 50, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 0L}, new Object[] {"E1", 43, 0L}, new Object[] {"E3", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E4", 43, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E4", 43, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]  { new Object[] {"E2", 45, 0L}, new Object[] {"E1", 43, 0L}, new Object[] {"E4", 43, 0L}, new Object[] {"E3", 10, 0L}});
    
            // in-place replacement
            _epService.EPRuntime.SendEvent(MakeEvent("E4", 43, 1L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E4", 43, 1L}, new Object[] {"E4", 43, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 0L}, new Object[] {"E1", 43, 0L}, new Object[] {"E4", 43, 1L}, new Object[] {"E3", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E2", 45, 1L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E2", 45, 1L}, new Object[] {"E2", 45, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 1L}, new Object[] {"E1", 43, 0L}, new Object[] {"E4", 43, 1L}, new Object[] {"E3", 10, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E1", 43, 1L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E1", 43, 1L}, new Object[] {"E1", 43, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 1L}, new Object[] {"E4", 43, 1L}, new Object[] {"E1", 43, 1L}, new Object[] {"E3", 10, 0L}});
    
            // out-of-space: pushing out the back end
            _epService.EPRuntime.SendEvent(MakeEvent("E5", 10, 2L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E5", 10, 2L}, new Object[] {"E3", 10, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 1L}, new Object[] {"E4", 43, 1L}, new Object[] {"E1", 43, 1L}, new Object[] {"E5", 10, 2L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E5", 11, 3L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E5", 11, 3L}, new Object[] {"E5", 10, 2L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 1L}, new Object[] {"E4", 43, 1L}, new Object[] {"E1", 43, 1L}, new Object[] {"E5", 11, 3L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E6", 43, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E6", 43, 0L}, new Object[] {"E5", 11, 3L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E2", 45, 1L}, new Object[] {"E4", 43, 1L}, new Object[] {"E1", 43, 1L}, new Object[] {"E6", 43, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E7", 50, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E7", 50, 0L}, new Object[] {"E4", 43, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E7", 50, 0L}, new Object[] {"E2", 45, 1L}, new Object[] {"E1", 43, 1L}, new Object[] {"E6", 43, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E8", 45, 0L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E8", 45, 0L}, new Object[] {"E1", 43, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E7", 50, 0L}, new Object[] {"E2", 45, 1L}, new Object[] {"E8", 45, 0L}, new Object[] {"E6", 43, 0L}});
    
            _epService.EPRuntime.SendEvent(MakeEvent("E8", 46, 1L));
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), fields, new Object[] {"E8", 46, 1L}, new Object[] {"E8", 45, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] {new Object[] {"E7", 50, 0L}, new Object[] {"E8", 46, 1L}, new Object[] {"E2", 45, 1L}, new Object[] {"E6", 43, 0L}});
        }
        
        private SupportBean MakeEvent(String theString, int intPrimitive, long longPrimitive) {
            return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
        }
    
        private SupportBean MakeEvent(String theString, int intPrimitive, long longPrimitive, double doublePrimitive) {
            SupportBean bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            return bean;
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from SupportBean.ext:rank(1, IntPrimitive desc)",
                       "Error starting statement: Error attaching view to event stream: Rank view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys [select * from SupportBean.ext:rank(1, IntPrimitive desc)]");
    
            TryInvalid("select * from SupportBean.ext:rank(1, IntPrimitive, TheString desc)",
                       "Error starting statement: Error attaching view to event stream: Failed to find unique value expressions that are expected to occur before the numeric size parameter [select * from SupportBean.ext:rank(1, IntPrimitive, TheString desc)]");
    
            TryInvalid("select * from SupportBean.ext:rank(TheString, IntPrimitive, 1)",
                       "Error starting statement: Error attaching view to event stream: Failed to find sort key expressions after the numeric size parameter [select * from SupportBean.ext:rank(TheString, IntPrimitive, 1)]");
    
            TryInvalid("select * from SupportBean.ext:rank(TheString, IntPrimitive, TheString desc)",
                       "Error starting statement: Error attaching view to event stream: Failed to find constant value for the numeric size parameter [select * from SupportBean.ext:rank(TheString, IntPrimitive, TheString desc)]");

            TryInvalid("select * from SupportBean.ext:rank(TheString, 1, 1, IntPrimitive, TheString desc)",
                        "Error starting statement: Error attaching view to event stream: Invalid view parameter expression 2 for Rank view, the expression returns a constant result value, are you sure? [select * from SupportBean.ext:rank(TheString, 1, 1, IntPrimitive, TheString desc)]");

            TryInvalid("select * from SupportBean.ext:rank(TheString, IntPrimitive, 1, IntPrimitive, 1, TheString desc)",
                       "Error starting statement: Error attaching view to event stream: Invalid view parameter expression 4 for Rank view, the expression returns a constant result value, are you sure? [select * from SupportBean.ext:rank(TheString, IntPrimitive, 1, IntPrimitive, 1, TheString desc)]");
        }
    
        private void TryInvalid(String epl, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
