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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestDataWindowIntersectExpiry 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestUniqueAndFirstLength()
        {
            Init(false);
    
            var epl = "select irstream TheString, IntPrimitive from SupportBean.win:firstlength(3).std:unique(TheString)";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertionUniqueAndFirstLength(stmt);
    
            stmt.Dispose();
            _listener.Reset();
            
            epl = "select irstream TheString, IntPrimitive from SupportBean.std:unique(TheString).win:firstlength(3)";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertionUniqueAndFirstLength(stmt);
        }
    
        [Test]
        public void TestFirstUniqueAndFirstLength()
        {
            Init(false);
    
            var epl = "select irstream TheString, IntPrimitive from SupportBean.std:firstunique(TheString).win:firstlength(3)";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertionFirstUniqueAndLength(stmt);
    
            stmt.Dispose();
            epl = "select irstream TheString, IntPrimitive from SupportBean.win:firstlength(3).std:firstunique(TheString)";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertionFirstUniqueAndLength(stmt);
        }
    
        [Test]
        public void TestFirstUniqueAndLengthOnDelete()
        {
            Init(false);
    
            var nwstmt = _epService.EPAdministrator.CreateEPL("create window MyWindow.std:firstunique(TheString).win:firstlength(3) as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindow where TheString = p00");
    
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream * from MyWindow");
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"TheString", "IntPrimitive"};
    
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1});
    
            SendEvent("E1", 99);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2});
            
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E1", 1});
    
            SendEvent("E1", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 3});
    
            SendEvent("E1", 99);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}, new Object[] {"E3", 3}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3});
    
            SendEvent("E3", 98);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}, new Object[] {"E3", 3}});
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestBatchWindow()
        {
            Init(false);
            EPStatement stmt;
    
            // test window
            stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.win:length_batch(3).std:unique(IntPrimitive) order by TheString asc");
            stmt.Events += _listener.Update;
            RunAssertionUniqueAndBatch(stmt);
            stmt.Dispose();
    
            stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.std:unique(IntPrimitive).win:length_batch(3) order by TheString asc");
            stmt.Events += _listener.Update;
            RunAssertionUniqueAndBatch(stmt);
            stmt.Dispose();
    
            // test aggregation with window
            stmt = _epService.EPAdministrator.CreateEPL("select Count(*) as c0, Sum(IntPrimitive) as c1 from SupportBean.std:unique(TheString).win:length_batch(3)");
            stmt.Events += _listener.Update;
            RunAssertionUniqueBatchAggreation();
            stmt.Dispose();
    
            stmt = _epService.EPAdministrator.CreateEPL("select Count(*) as c0, Sum(IntPrimitive) as c1 from SupportBean.win:length_batch(3).std:unique(TheString)");
            stmt.Events += _listener.Update;
            RunAssertionUniqueBatchAggreation();
            stmt.Dispose();
    
            // test first-unique
            stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.std:firstunique(TheString).win:length_batch(3)");
            stmt.Events += _listener.Update;
            RunAssertionLengthBatchAndFirstUnique();
            stmt.Dispose();
    
            stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:length_batch(3).std:firstunique(TheString)");
            stmt.Events += _listener.Update;
            RunAssertionLengthBatchAndFirstUnique();
            stmt.Dispose();
    
            // test time-based expiry
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.std:unique(TheString).win:time_batch(1)");
            stmt.Events += _listener.Update;
            RunAssertionTimeBatchAndUnique(0);
            stmt.Dispose();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(100000));
            stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time_batch(1).std:unique(TheString)");
            stmt.Events += _listener.Update;
            RunAssertionTimeBatchAndUnique(100000);
            stmt.Dispose();
    
            try {
                stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:time_batch(1).win:length_batch(10)");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot combined multiple batch data windows into an intersection [select * from SupportBean.win:time_batch(1).win:length_batch(10)]", ex.Message);
            }
        }
    
        [Test]
        public void TestIntersectAndDerivedValue()
        {
            Init(false);
            var fields = new String[] {"total"};
    
            var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.std:unique(IntPrimitive).std:unique(IntBoxed).stat:uni(DoublePrimitive)");
            stmt.Events += _listener.Update;
    
            SendEvent("E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(100d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {100d});
    
            SendEvent("E2", 2, 20, 50d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(150d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {150d});
    
            SendEvent("E3", 1, 20, 20d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(20d));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {20d});
        }
    
        [Test]
        public void TestIntersectGroupBy()
        {
            Init(false);
            var fields = new String[] {"TheString"};
    
            var text = "select irstream TheString from SupportBean.std:groupwin(IntPrimitive).win:length(2).std:unique(IntBoxed) retain-intersection";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            SendEvent("E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2"});
    
            SendEvent("E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3"});
    
            SendEvent("E4", 1, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E1"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E4"});
            _listener.Reset();
    
            SendEvent("E5", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E2"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E5"});
            _listener.Reset();
    
            SendEvent("E6", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E3"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E6"});
            _listener.Reset();
    
            SendEvent("E7", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E4"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E7"});
            _listener.Reset();
    
            SendEvent("E8", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E6", "E7", "E8"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E5"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E8"});
            _listener.Reset();
    
            // another combination
            _epService.EPAdministrator.CreateEPL("select * from SupportBean.std:groupwin(TheString).win:time(.0083 sec).std:firstevent()");
        }
    
        [Test]
        public void TestIntersectSubselect()
        {
            Init(false);
    
            var text = "select * from SupportBean_S0 where p00 in (select TheString from SupportBean.win:length(2).std:unique(IntPrimitive) retain-intersection)";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            SendEvent("E1", 1);
            SendEvent("E2", 2);
            SendEvent("E3", 3); // throws out E1
            SendEvent("E4", 2); // throws out E2
            SendEvent("E5", 1); // throws out E3
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E2"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E3"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E4"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestIntersectThreeUnique()
        {
            Init(false);
            var fields = new String[] {"TheString"};
    
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.std:unique(IntPrimitive).std:unique(IntBoxed).std:unique(DoublePrimitive) retain-intersection");
            stmt.Events += _listener.Update;
    
            SendEvent("E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            SendEvent("E2", 2, 10, 200d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E1"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E2"});
            _listener.Reset();
    
            SendEvent("E3", 2, 20, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E2"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E3"});
            _listener.Reset();
    
            SendEvent("E4", 1, 30, 300d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E4"});
    
            SendEvent("E5", 3, 40, 400d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E5"});
    
            SendEvent("E6", 3, 40, 300d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E6"));
            Object[] result = {_listener.LastOldData[0].Get("TheString"), _listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new String[] {"E4", "E5"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E6"});
            _listener.Reset();
        }
    
        [Test]
        public void TestIntersectPattern()
        {
            Init(false);
            var fields = new String[] {"TheString"};
    
            var text = "select irstream a.p00||b.p10 as TheString from pattern [every a=SupportBean_S0 -> b=SupportBean_S1].std:unique(a.id).std:unique(b.id) retain-intersection";
            var stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1E2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E4"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1E2", "E3E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3E4"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E6"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3E4", "E5E6"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E1E2"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E5E6"});
        }
    
        [Test]
        public void TestIntersectTwoUnique()
        {
            Init(false);
            var fields = new String[] {"TheString"};
    
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.std:unique(IntPrimitive).std:unique(IntBoxed) retain-intersection");
            stmt.Events += _listener.Update;
    
            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            SendEvent("E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E1"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E2"});
            _listener.Reset();
    
            SendEvent("E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3"});
    
            SendEvent("E4", 3, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E3"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E4"});
            _listener.Reset();
    
            SendEvent("E5", 2, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E2"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E5"});
            _listener.Reset();
    
            SendEvent("E6", 3, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5", "E6"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E4"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E6"});
            _listener.Reset();
    
            SendEvent("E7", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E7"));
            Assert.AreEqual(2, _listener.LastOldData.Length);
            Object[] result = {_listener.LastOldData[0].Get("TheString"), _listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new String[] {"E5", "E6"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E7"});
            _listener.Reset();
    
            SendEvent("E8", 4, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E7", "E8"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E8"});
    
            SendEvent("E9", 3, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E8", "E9"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E7"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E9"});
            _listener.Reset();
    
            SendEvent("E10", 2, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E8", "E10"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E9"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E10"});
            _listener.Reset();
        }
    
        [Test]
        public void TestIntersectSorted()
        {
            Init(false);
            var fields = new String[] {"TheString"};
    
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.ext:sort(2, IntPrimitive).ext:sort(2, IntBoxed) retain-intersection");
            stmt.Events += _listener.Update;
    
            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            SendEvent("E2", 2, 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2"});
    
            SendEvent("E3", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3"));
            Object[] result = {_listener.LastOldData[0].Get("TheString"), _listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new String[] {"E1", "E2"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E3"});
            _listener.Reset();
    
            SendEvent("E4", -1, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E4"});
    
            SendEvent("E5", 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E5"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E5"});
            _listener.Reset();
    
            SendEvent("E6", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E6"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E3"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E6"});
            _listener.Reset();
        }
    
        [Test]
        public void TestIntersectTimeWin()
        {
            Init(false);
    
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.std:unique(IntPrimitive).win:time(10 sec) retain-intersection");
            stmt.Events += _listener.Update;
    
            RunAssertionTimeWinUnique(stmt);
        }
    
        [Test]
        public void TestIntersectTimeWinReversed()
        {
            Init(false);
    
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.win:time(10 sec).std:unique(IntPrimitive) retain-intersection");
            stmt.Events += _listener.Update;
    
            RunAssertionTimeWinUnique(stmt);
        }
    
        [Test]
        public void TestIntersectTimeWinSODA()
        {
            Init(false);
    
            SendTimer(0);
            var stmtText = "select irstream TheString from SupportBean.win:time(10 seconds).std:unique(IntPrimitive) retain-intersection";
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            var stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            RunAssertionTimeWinUnique(stmt);
        }
    
        [Test]
        public void TestIntersectTimeWinNamedWindow()
        {
            Init(false);
    
            SendTimer(0);
            var stmtWindow = _epService.EPAdministrator.CreateEPL("create window MyWindow.win:time(10 sec).std:unique(IntPrimitive) retain-intersection as select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindow where IntBoxed = id");
            stmtWindow.Events += _listener.Update;
    
            RunAssertionTimeWinUnique(stmtWindow);
        }
    
        [Test]
        public void TestIntersectTimeWinNamedWindowDelete()
        {
            Init(false);
    
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL("create window MyWindow.win:time(10 sec).std:unique(IntPrimitive) retain-intersection as select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindow where IntBoxed = id");
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"TheString"};
    
            SendTimer(1000);
            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            SendTimer(2000);
            SendEvent("E2", 2, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
    
            SendTimer(3000);
            SendEvent("E3", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3"});
            SendEvent("E4", 3, 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E3"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E4"});
            _listener.Reset();
    
            SendTimer(4000);
            SendEvent("E5", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4", "E5"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E5"});
            SendEvent("E6", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4", "E6"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E5"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E6"});
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4", "E6"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(50));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E6"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4"));
    
            SendTimer(10999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(11000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E1"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4"));
    
            SendTimer(12999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(13000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr());
    
            SendTimer(10000000);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        private void RunAssertionTimeWinUnique(EPStatement stmt)
        {
            var fields = new String[] {"TheString"};
    
            SendTimer(1000);
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            SendTimer(2000);
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2"});
    
            SendTimer(3000);
            SendEvent("E3", 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E1"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E3"});
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3"));
    
            SendTimer(4000);
            SendEvent("E4", 3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E4"));
            SendEvent("E5", 3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new Object[] {"E4"});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new Object[] {"E5"});
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E5"));
    
            SendTimer(11999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(12000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3","E5"));
    
            SendTimer(12999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(13000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5"));
    
            SendTimer(13999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(14000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[] {"E5"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr());
        }
    
        private void SendEvent(String theString, int intPrimitive, int intBoxed, double doublePrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.DoublePrimitive = doublePrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(String theString, int intPrimitive, int intBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(String theString, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private Object[][] ToArr(params Object[] values)
        {
            var arr = new Object[values.Length][];
            for (var i = 0; i < values.Length; i++)
            {
                arr[i] = new Object[] {values[i]};
            }
            return arr;
        }
    
        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void Init(bool isAllowMultipleDataWindows)
        {
            _listener = new SupportUpdateListener();
    
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = isAllowMultipleDataWindows;
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S1>();
        }
    
        private void RunAssertionUniqueBatchAggreation() {
            var fields = "c0,c1".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 11));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3L, 10+11+12});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 13));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 14));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 15));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3L, 13+14+15});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 16));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 17));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 18));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3L, 16+17+18});
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 19));
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 21));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 22));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 23));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3L, 20+22+23});
        }
    
        private void RunAssertionUniqueAndBatch(EPStatement stmt) {
            var fields = new String[] {"TheString"};
    
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
            Assert.IsNull(_listener.GetAndResetLastOldData());
    
            SendEvent("E4", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E5", 4); // throws out E5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E6", 4); // throws out E6
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E6"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E7", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E6", "E7"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E8", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] {new Object[] {"E6"}, new Object[] {"E7"}, new Object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new Object[][] {new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
            _listener.Reset();
    
            SendEvent("E8", 7);
            SendEvent("E9", 9);
            SendEvent("E9", 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E8", "E9"));
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E10", 11);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] {new Object[] {"E10"}, new Object[] {"E8"}, new Object[] {"E9"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new Object[][] {new Object[] {"E6"}, new Object[] {"E7"}, new Object[] {"E8"}});
            _listener.Reset();
        }
    
        private void RunAssertionUniqueAndFirstLength(EPStatement stmt)
        {
            var fields = new String[] {"TheString", "IntPrimitive"};
    
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1});
    
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2});
    
            SendEvent("E1", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] {"E1", 3});
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] {"E1", 1});
            _listener.Reset();
    
            SendEvent("E3", 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}, new Object[] {"E3", 30}});
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] {"E3", 30});
            _listener.Reset();
    
            SendEvent("E4", 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 3}, new Object[] {"E2", 2}, new Object[] {"E3", 30}});
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        private void RunAssertionFirstUniqueAndLength(EPStatement stmt) {
    
            var fields = new String[] {"TheString", "IntPrimitive"};
    
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1});
    
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", 2});
    
            SendEvent("E2", 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}});
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}, new Object[] {"E3", 3}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3", 3});
    
            SendEvent("E4", 4);
            SendEvent("E4", 5);
            SendEvent("E5", 5);
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}, new Object[] {"E3", 3}});
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        private void RunAssertionTimeBatchAndUnique(long startTime) {
            var fields = "TheString,IntPrimitive".Split(',');
            _listener.Reset();
    
            SendEvent("E1", 1);
            SendEvent("E2", 2);
            SendEvent("E1", 3);
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 1000));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {"E2", 2}, new Object[] {"E1", 3}});
            Assert.IsNull(_listener.GetAndResetLastOldData());
    
            SendEvent("E3", 3);
            SendEvent("E3", 4);
            SendEvent("E3", 5);
            SendEvent("E4", 6);
            SendEvent("E3", 7);
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 2000));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {"E4", 6}, new Object[] {"E3", 7}});
            Assert.IsNull(_listener.GetAndResetLastOldData());
        }
    
        private void RunAssertionLengthBatchAndFirstUnique() {
            var fields = "TheString,IntPrimitive".Split(',');
    
            SendEvent("E1", 1);
            SendEvent("E2", 2);
            SendEvent("E1", 3);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E3", 4);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}, new Object[] {"E3", 4}});
            Assert.IsNull(_listener.GetAndResetLastOldData());
    
            SendEvent("E1", 5);
            SendEvent("E4", 7);
            SendEvent("E1", 6);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E5", 9);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {"E1", 5}, new Object[] {"E4", 7}, new Object[] {"E5", 9}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastOldData(), fields, new Object[][] { new Object[] {"E1", 1}, new Object[] {"E2", 2}, new Object[] {"E3", 4}});
            _listener.Reset();
        }
    }
}
