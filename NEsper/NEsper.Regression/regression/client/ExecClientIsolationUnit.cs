///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientIsolationUnit : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMovePattern(epService);
            RunAssertionInvalid(epService);
            RunAssertionIsolateFilter(epService);
            RunAssertionDestroy(epService);
            RunAssertionIsolatedSchedule(epService);
            RunAssertionInsertInto(epService);
            RunAssertionIsolateMultiple(epService);
            RunAssertionStartStop(epService);
            RunAssertionNamedWindowTakeCreate(epService);
            RunAssertionNamedWindowTimeCatchup(epService);
            RunAssertionCurrentTimestamp(epService);
            RunAssertionUpdate(epService);
            RunAssertionSuspend(epService);
            RunAssertionCreateStmt(epService);
            RunAssertionSubscriberNamedWindowConsumerIterate(epService);
            RunAssertionEventSender(epService);
        }
    
        private void RunAssertionMovePattern(EPServiceProvider epService) {
            EPServiceProviderIsolated isolatedService = epService.GetEPServiceIsolated("Isolated");
            EPStatement stmt = isolatedService.EPAdministrator.CreateEPL("select * from pattern [every (a=SupportBean -> b=SupportBean(TheString=a.TheString)) where timer:within(1 day)]", "TestStatement", null);
            isolatedService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.CurrentTimeMillis + 1000));
            isolatedService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            isolatedService.EPAdministrator.RemoveStatement(stmt);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsTrue(listener.IsInvokedAndReset());
            stmt.Dispose();
            isolatedService.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('A') select * from SupportBean");
            EPServiceProviderIsolated unitOne = epService.GetEPServiceIsolated("i1");
            EPServiceProviderIsolated unitTwo = epService.GetEPServiceIsolated("i2");
            unitOne.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_INTERNAL));
            unitOne.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
    
            unitOne.EPAdministrator.AddStatement(stmt);
            try {
                unitTwo.EPAdministrator.AddStatement(stmt);
                Assert.Fail();
            } catch (EPServiceIsolationException ex) {
                Assert.AreEqual("Statement named 'A' already in service isolation under 'i1'", ex.Message);
            }
    
            try {
                unitTwo.EPAdministrator.RemoveStatement(stmt);
                Assert.Fail();
            } catch (EPServiceIsolationException ex) {
                Assert.AreEqual("Statement named 'A' not in this service isolation but under service isolation 'A'", ex.Message);
            }
    
            unitOne.EPAdministrator.RemoveStatement(stmt);
    
            try {
                unitOne.EPAdministrator.RemoveStatement(stmt);
                Assert.Fail();
            } catch (EPServiceIsolationException ex) {
                Assert.AreEqual("Statement named 'A' is not currently in service isolation", ex.Message);
            }
    
            try {
                unitTwo.EPAdministrator.RemoveStatement(new EPStatement[]{null});
                Assert.Fail();
            } catch (EPServiceIsolationException ex) {
                Assert.AreEqual("Illegal argument, a null value was provided in the statement list", ex.Message);
            }
    
            try {
                unitTwo.EPAdministrator.AddStatement(new EPStatement[]{null});
                Assert.Fail();
            } catch (EPServiceIsolationException ex) {
                Assert.AreEqual("Illegal argument, a null value was provided in the statement list", ex.Message);
            }
    
            stmt.Dispose();
            unitOne.Dispose();
            unitTwo.Dispose();
        }
    
        private void RunAssertionIsolateFilter(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from pattern [every a=SupportBean -> b=SupportBean(TheString=a.TheString)]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"i1"}, epService.EPServiceIsolatedNames);
    
            // send fake to wrong place
            unit.EPRuntime.SendEvent(new SupportBean("E1", -1));
    
            unit.EPAdministrator.AddStatement(stmt);
    
            // send to 'wrong' engine
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // send to 'right' engine
            unit.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,a.IntPrimitive,b.IntPrimitive".Split(','), new object[]{"E1", 1, 3});
    
            // send second pair, and a fake to the wrong place
            unit.EPRuntime.SendEvent(new SupportBean("E2", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
    
            unit.EPAdministrator.RemoveStatement(stmt);
    
            // send to 'wrong' engine
            unit.EPRuntime.SendEvent(new SupportBean("E2", 5));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            // send to 'right' engine
            epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,a.IntPrimitive,b.IntPrimitive".Split(','), new object[]{"E2", 4, 6});
    
            epService.EPAdministrator.DestroyAllStatements();
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"i1"}, epService.EPServiceIsolatedNames);
            epService.GetEPServiceIsolated("i1").Dispose();
            EPAssertionUtil.AssertEqualsAnyOrder(new string[0], epService.EPServiceIsolatedNames);
        }
    
        private void RunAssertionDestroy(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Name('A') select * from SupportBean", null, null);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
    
            EPStatement stmtTwo = unit.EPAdministrator.CreateEPL("@Name('B') select * from SupportBean", null, null);
            stmtTwo.Events += listener.Update;
            unit.EPAdministrator.AddStatement(stmtOne);
    
            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(2, listener.GetNewDataListFlattened().Length);
            listener.Reset();
    
            unit.Dispose();
    
            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(0, listener.GetNewDataListFlattened().Length);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(2, listener.GetNewDataListFlattened().Length);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionIsolatedSchedule(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            SendTimerUnisolated(epService, 100000);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from pattern [every a=SupportBean -> timer:interval(10)]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimerUnisolated(epService, 105000);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            SendTimerIso(0, unit);
            unit.EPAdministrator.AddStatement(stmt);
    
            SendTimerIso(9999, unit);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerIso(10000, unit);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new object[]{"E1"});
    
            SendTimerIso(11000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E2", 1));
    
            SendTimerUnisolated(epService, 120000);
            Assert.IsFalse(listener.IsInvoked);
    
            unit.EPAdministrator.RemoveStatement(stmt);
    
            SendTimerUnisolated(epService, 129999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerUnisolated(epService, 130000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new object[]{"E2"});
    
            SendTimerIso(30000, unit);
            Assert.IsFalse(listener.IsInvoked);
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInsertInto(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyStream select * from SupportBean");
            var listenerInsert = new SupportUpdateListener();
            stmtInsert.Events += listenerInsert.Update;
    
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyStream");
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
    
            // unit takes "insert"
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            unit.EPAdministrator.AddStatement(stmtInsert);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            Assert.IsFalse(listenerInsert.GetAndClearIsInvoked());
    
            unit.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            Assert.IsFalse(listenerInsert.GetAndClearIsInvoked());    // is there a remaining event that gets flushed with the last one?
    
            // unit returns insert
            unit.EPAdministrator.RemoveStatement(stmtInsert);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E4"});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E4"});
    
            unit.EPRuntime.SendEvent(new SupportBean("E5", 5));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            Assert.IsFalse(listenerInsert.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E6"});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E6"});
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIsolateMultiple(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            var fields = new string[]{"TheString", "sumi"};
            int count = 4;
            var listeners = new SupportUpdateListener[count];
            for (int i = 0; i < count; i++) {
                string epl = "@Name('S" + i + "') select TheString, sum(IntPrimitive) as sumi from SupportBean(TheString='" + i + "')#time(10)";
                listeners[i] = new SupportUpdateListener();
                epService.EPAdministrator.CreateEPL(epl).Events += listeners[i].Update;
            }
    
            var statements = new EPStatement[2];
            statements[0] = epService.EPAdministrator.GetStatement("S0");
            statements[1] = epService.EPAdministrator.GetStatement("S2");
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            unit.EPAdministrator.AddStatement(statements);
    
            // send to unisolated
            for (int i = 0; i < count; i++) {
                epService.EPRuntime.SendEvent(new SupportBean(Convert.ToString(i), i));
            }
            Assert.IsFalse(listeners[0].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new object[]{"1", 1});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new object[]{"3", 3});
    
            // send to isolated
            for (int i = 0; i < count; i++) {
                unit.EPRuntime.SendEvent(new SupportBean(Convert.ToString(i), i));
            }
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[3].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new object[]{"0", 0});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, new object[]{"2", 2});
    
            unit.EPRuntime.SendEvent(new SupportBean(Convert.ToString(2), 2));
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, new object[]{"2", 4});
    
            // return
            unit.EPAdministrator.RemoveStatement(statements);
    
            // send to unisolated
            for (int i = 0; i < count; i++) {
                epService.EPRuntime.SendEvent(new SupportBean(Convert.ToString(i), i));
            }
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new object[]{"0", 0});
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new object[]{"1", 2});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, new object[]{"2", 6});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new object[]{"3", 6});
    
            // send to isolated
            for (int i = 0; i < count; i++) {
                unit.EPRuntime.SendEvent(new SupportBean(Convert.ToString(i), i));
                Assert.IsFalse(listeners[i].IsInvoked);
            }
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStartStop(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            var fields = new string[]{"TheString"};
            string epl = "select TheString from SupportBean#time(60)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            unit.EPAdministrator.AddStatement(stmt);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}});
    
            stmt.Stop();
    
            unit.EPAdministrator.RemoveStatement(stmt);
    
            stmt.Start();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E4", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3"}});
    
            unit.EPAdministrator.AddStatement(stmt);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3"}, new object[] {"E6"}});
    
            unit.EPAdministrator.RemoveStatement(stmt);
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E8", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3"}, new object[] {"E6"}, new object[] {"E7"}});
    
            stmt.Stop();
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowTakeCreate(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            var fields = new string[]{"TheString"};
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("@Name('create') create window MyWindow#keepall as SupportBean");
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL("@Name('delete') on SupportBean_A delete from MyWindow where TheString = id");
            EPStatement stmtConsume = epService.EPAdministrator.CreateEPL("@Name('consume') select irstream * from MyWindow");
            var listener = new SupportUpdateListener();
            stmtConsume.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            unit.EPAdministrator.AddStatement(epService.EPAdministrator.GetStatement("create"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
            Assert.IsFalse(listener.IsInvoked);
    
            unit.EPAdministrator.AddStatement(stmtInsert);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E5"}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});    // yes receives it
    
            // Note: Named window is global across isolation units: they are a relation and not a stream.
    
            // The insert-into to a named window is a stream that can be isolated from the named window.
            // The streams of on-select and on-delete can be isolated, however they select or change the named window even if that is isolated.
            // Consumers to a named window always receive all changes to a named window (regardless of whether the consuming statement is isolated or not), even if the window itself was isolated.
            //
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E5"}});
    
            unit.EPAdministrator.AddStatement(stmtDelete);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E5"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E5"}});
    
            unit.EPRuntime.SendEvent(new SupportBean_A("E5"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, null);
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowTimeCatchup(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            SendTimerUnisolated(epService, 100000);
            var fields = new string[]{"TheString"};
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("@Name('create') create window MyWindow#time(10) as SupportBean");
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            SendTimerIso(0, unit);
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmtCreate, stmtInsert});
    
            SendTimerIso(1000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            SendTimerIso(2000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendTimerIso(9000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E3", 3));
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
            unit.EPAdministrator.RemoveStatement(new EPStatement[]{stmtCreate});
    
            SendTimerUnisolated(epService, 101000);    // equivalent to 10000  (E3 is 1 seconds old)
    
            SendTimerUnisolated(epService, 102000);    // equivalent to 11000  (E3 is 2 seconds old)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
    
            SendTimerUnisolated(epService, 103000);    // equivalent to 12000  (E3 is 3 seconds old)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E3"}});
    
            SendTimerUnisolated(epService, 109000);    // equivalent to 18000 (E3 is 9 seconds old)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E3"}});
    
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmtCreate});
    
            SendTimerIso(9999, unit);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E3"}});
    
            SendTimerIso(10000, unit);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, null);
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCurrentTimestamp(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            SendTimerUnisolated(epService, 5000);
            var fields = new string[]{"ct"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select current_timestamp() as ct from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5000L});
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            SendTimerIso(100000, unit);
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmt});
    
            unit.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100000L});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            stmt = epService.EPAdministrator.CreateEPL("select TheString as ct from SupportBean where current_timestamp() >= 10000");
            stmt.Events += listener.Update;
    
            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerUnisolated(epService, 10000);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            unit.EPAdministrator.AddStatement(stmt);
    
            unit.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select TheString as ct from SupportBean where current_timestamp() >= 120000");
            stmt.Events += listener.Update;
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmt});
    
            unit.EPRuntime.SendEvent(new SupportBean("E3", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerIso(120000, unit);
    
            unit.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUpdate(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            SendTimerUnisolated(epService, 5000);
            var fields = new string[]{"TheString"};
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into NewStream select * from SupportBean");
            EPStatement stmtUpd = epService.EPAdministrator.CreateEPL("update istream NewStream set TheString = 'X'");
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select * from NewStream");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"X"});
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmtSelect});
    
            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(listener.IsInvoked);
    
            /// <summary>
            /// Update statements apply to a stream even if the statement is not isolated.
            /// </summary>
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmtInsert});
            unit.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"X"});
    
            unit.EPAdministrator.AddStatement(stmtUpd);
            unit.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"X"});
    
            stmtUpd.Stop();
    
            unit.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSuspend(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            SendTimerUnisolated(epService, 1000);
            var fields = new string[]{"TheString"};
            string epl = "select irstream TheString from SupportBean#time(10)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimerUnisolated(epService, 2000);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
    
            SendTimerUnisolated(epService, 3000);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            SendTimerUnisolated(epService, 7000);
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
    
            SendTimerUnisolated(epService, 8000);
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select 'x' as TheString from pattern [timer:interval(10)]");
            stmtTwo.Events += listener.Update;
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            unit.EPAdministrator.AddStatement(new EPStatement[]{stmt, stmtTwo});
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{stmt.Name, stmtTwo.Name}, unit.EPAdministrator.StatementNames);
            Assert.AreEqual("i1", stmt.ServiceIsolated);
            Assert.AreEqual("i1", stmt.ServiceIsolated);
    
            listener.Reset();
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            SendTimerUnisolated(epService, 15000);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            unit.EPAdministrator.RemoveStatement(new EPStatement[]{stmt, stmtTwo});
            EPAssertionUtil.AssertEqualsAnyOrder(new string[0], unit.EPAdministrator.StatementNames);
            Assert.IsNull(stmt.ServiceIsolated);
            Assert.IsNull(stmt.ServiceIsolated);
    
            SendTimerUnisolated(epService, 18999);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            SendTimerUnisolated(epService, 19000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
    
            SendTimerUnisolated(epService, 23999);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3"}});
    
            SendTimerUnisolated(epService, 24000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendTimerUnisolated(epService, 25000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"x"});
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCreateStmt(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("i1");
            SendTimerUnisolated(epService, 100000);
            SendTimerIso(1000, unit);
    
            var fields = new string[]{"ct"};
            EPStatement stmt = unit.EPAdministrator.CreateEPL("select current_timestamp() as ct from pattern[every timer:interval(10)]", null, null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimerIso(10999, unit);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerIso(11000, unit);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11000L});
    
            SendTimerIso(15000, unit);
    
            unit.EPAdministrator.RemoveStatement(stmt);
    
            SendTimerIso(21000, unit);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimerUnisolated(epService, 106000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{106000L});
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubscriberNamedWindowConsumerIterate(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EPServiceProviderIsolated isolatedService = epService.GetEPServiceIsolated("isolatedStmts");
            isolatedService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.CurrentTimeMillis));
    
            var subscriber = new SupportSubscriber();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmtOne.Subscriber = subscriber;
    
            EPStatement stmtTwo = isolatedService.EPAdministrator.CreateEPL("select * from MyWindow", null, null);
            isolatedService.EPAdministrator.AddStatement(stmtOne);
    
            IEnumerator<EventBean> iter = stmtTwo.GetEnumerator();
            while (iter.MoveNext()) {
                EventBean theEvent = iter.Current;
                isolatedService.EPRuntime.SendEvent(theEvent.Underlying);
            }
    
            Assert.IsTrue(subscriber.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventSender(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecClientIsolationUnit))) {
                return;
            }
    
            EPServiceProviderIsolated unit = epService.GetEPServiceIsolated("other1");
            EventSender sender = unit.EPRuntime.GetEventSender("SupportBean");
            var listener = new SupportUpdateListener();
            unit.EPAdministrator.CreateEPL("select * from SupportBean", null, null).Events += listener.Update;
            sender.SendEvent(new SupportBean());
            Assert.IsTrue(listener.IsInvoked);
    
            unit.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendTimerUnisolated(EPServiceProvider epService, long millis) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(millis));
        }
    
        private void SendTimerIso(long millis, EPServiceProviderIsolated unit) {
            unit.EPRuntime.SendEvent(new CurrentTimeEvent(millis));
        }
    }
} // end of namespace
