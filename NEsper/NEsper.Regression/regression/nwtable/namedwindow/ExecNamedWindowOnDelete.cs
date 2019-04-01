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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOnDelete : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionFirstUnique(epService);
            RunAssertionStaggeredNamedWindow(epService);
            RunAssertionCoercionKeyMultiPropIndexes(epService);
            RunAssertionCoercionRangeMultiPropIndexes(epService);
            RunAssertionCoercionKeyAndRangeMultiPropIndexes(epService);
        }
    
        private void RunAssertionFirstUnique(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            var fields = new string[]{"TheString", "IntPrimitive"};
            string stmtTextCreateOne = "create window MyWindowFU#firstunique(TheString) as select * from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            epService.EPAdministrator.CreateEPL("insert into MyWindowFU select * from SupportBean");
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL("on SupportBean_A a delete from MyWindowFU where TheString=a.id");
            var listenerDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerDelete.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A", 2));
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A"));
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"A", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 3));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"A", 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A"));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
        }
    
        private void RunAssertionStaggeredNamedWindow(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionStaggered(epService, rep);
            }
        }
    
        private void TryAssertionStaggered(EPServiceProvider epService, EventRepresentationChoice outputType) {
    
            var fieldsOne = new string[]{"a1", "b1"};
            var fieldsTwo = new string[]{"a2", "b2"};
    
            // create window one
            string stmtTextCreateOne = outputType.GetAnnotationText() + " create window MyWindowSTAG#keepall as select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            var listenerWindow = new SupportUpdateListener();
            stmtCreateOne.Events += listenerWindow.Update;
            Assert.AreEqual(0, GetCount(epService, "MyWindowSTAG"));
            Assert.IsTrue(outputType.MatchesClass(stmtCreateOne.EventType.UnderlyingType));
    
            // create window two
            string stmtTextCreateTwo = outputType.GetAnnotationText() + " create window MyWindowSTAGTwo#keepall as select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName;
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            var listenerWindowTwo = new SupportUpdateListener();
            stmtCreateTwo.Events += listenerWindowTwo.Update;
            Assert.AreEqual(0, GetCount(epService, "MyWindowSTAGTwo"));
            Assert.IsTrue(outputType.MatchesClass(stmtCreateTwo.EventType.UnderlyingType));
    
            // create delete stmt
            string stmtTextDelete = "on MyWindowSTAG delete from MyWindowSTAGTwo where a1 = a2";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerDelete.Update;
            Assert.AreEqual(StatementType.ON_DELETE, ((EPStatementSPI) stmtDelete).StatementMetadata.StatementType);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowSTAG select TheString as a1, IntPrimitive as b1 from " + typeof(SupportBean).FullName + "(IntPrimitive > 0)";
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
            stmtTextInsert = "insert into MyWindowSTAGTwo select TheString as a2, IntPrimitive as b2 from " + typeof(SupportBean).FullName + "(IntPrimitive < 0)";
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendSupportBean(epService, "E1", -10);
            EPAssertionUtil.AssertProps(listenerWindowTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"E1", -10});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsTwo, new object[][]{new object[] {"E1", -10}});
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.AreEqual(1, GetCount(epService, "MyWindowSTAGTwo"));
    
            SendSupportBean(epService, "E2", 5);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E2", 5});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsOne, new object[][]{new object[] {"E2", 5}});
            Assert.IsFalse(listenerWindowTwo.IsInvoked);
            Assert.AreEqual(1, GetCount(epService, "MyWindowSTAG"));
    
            SendSupportBean(epService, "E3", -1);
            EPAssertionUtil.AssertProps(listenerWindowTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[]{"E3", -1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsTwo, new object[][]{new object[] {"E1", -10}, new object[] {"E3", -1}});
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.AreEqual(2, GetCount(epService, "MyWindowSTAGTwo"));
    
            SendSupportBean(epService, "E3", 1);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsOne, new object[]{"E3", 1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsOne, new object[][]{new object[] {"E2", 5}, new object[] {"E3", 1}});
            EPAssertionUtil.AssertProps(listenerWindowTwo.AssertOneGetOldAndReset(), fieldsTwo, new object[]{"E3", -1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsTwo, new object[][]{new object[] {"E1", -10}});
            Assert.AreEqual(2, GetCount(epService, "MyWindowSTAG"));
            Assert.AreEqual(1, GetCount(epService, "MyWindowSTAGTwo"));
    
            stmtDelete.Dispose();
            stmtCreateOne.Dispose();
            stmtCreateTwo.Dispose();
            listenerDelete.Reset();
            listenerWindow.Reset();
            listenerWindowTwo.Reset();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowSTAG", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowSTAGTwo", true);
        }
    
        private void RunAssertionCoercionKeyMultiPropIndexes(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowCK#keepall as select " +
                    "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            var deleteStatements = new List<EPStatement>();
            string stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='DB') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.DoubleBoxed";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(1, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='DP') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.DoublePrimitive";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(1, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='IB') as s0 delete from MyWindowCK where MyWindowCK.IntPrimitive = s0.IntBoxed";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='IPDP') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.IntPrimitive and win.DoublePrimitive = s0.DoublePrimitive";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='IPDP2') as s0 delete from MyWindowCK as win where win.DoublePrimitive = s0.DoublePrimitive and win.IntPrimitive = s0.IntPrimitive";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='IPDPIB') as s0 delete from MyWindowCK as win where win.DoublePrimitive = s0.DoublePrimitive and win.IntPrimitive = s0.IntPrimitive and win.IntBoxed = s0.IntBoxed";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='CAST') as s0 delete from MyWindowCK as win where win.IntBoxed = s0.IntPrimitive and win.DoublePrimitive = s0.DoubleBoxed and win.IntPrimitive = s0.IntBoxed";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCK").Length);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowCK select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed "
                    + "from " + typeof(SupportBean).FullName + "(TheString like 'E%')";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            SendSupportBean(epService, "E1", 1, 10, 100d, 1000d);
            SendSupportBean(epService, "E2", 2, 20, 200d, 2000d);
            SendSupportBean(epService, "E3", 3, 30, 300d, 3000d);
            SendSupportBean(epService, "E4", 4, 40, 400d, 4000d);
            listenerWindow.Reset();
    
            var fields = new string[]{"TheString"};
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
    
            SendSupportBean(epService, "DB", 0, 0, 0d, null);
            Assert.IsFalse(listenerWindow.IsInvoked);
            SendSupportBean(epService, "DB", 0, 0, 0d, 3d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E4"}});
    
            SendSupportBean(epService, "DP", 0, 0, 5d, null);
            Assert.IsFalse(listenerWindow.IsInvoked);
            SendSupportBean(epService, "DP", 0, 0, 4d, null);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            SendSupportBean(epService, "IB", 0, -1, 0d, null);
            Assert.IsFalse(listenerWindow.IsInvoked);
            SendSupportBean(epService, "IB", 0, 1, 0d, null);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}});
    
            SendSupportBean(epService, "E5", 5, 50, 500d, 5000d);
            SendSupportBean(epService, "E6", 6, 60, 600d, 6000d);
            SendSupportBean(epService, "E7", 7, 70, 700d, 7000d);
            listenerWindow.Reset();
    
            SendSupportBean(epService, "IPDP", 5, 0, 500d, null);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E6"}, new object[] {"E7"}});
    
            SendSupportBean(epService, "IPDP2", 6, 0, 600d, null);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E6"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E7"}});
    
            SendSupportBean(epService, "IPDPIB", 7, 70, 0d, null);
            Assert.IsFalse(listenerWindow.IsInvoked);
            SendSupportBean(epService, "IPDPIB", 7, 70, 700d, null);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E7"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}});
    
            SendSupportBean(epService, "E8", 8, 80, 800d, 8000d);
            listenerWindow.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E8"}});
    
            SendSupportBean(epService, "CAST", 80, 8, 0, 800d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E8"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2"}});
    
            foreach (EPStatement stmt in deleteStatements) {
                stmt.Dispose();
            }
            deleteStatements.Clear();
    
            // late delete on a filled window
            stmtTextDelete = "on " + typeof(SupportBean).FullName + "(TheString='LAST') as s0 delete from MyWindowCK as win where win.IntPrimitive = s0.IntPrimitive and win.DoublePrimitive = s0.DoublePrimitive";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            SendSupportBean(epService, "LAST", 2, 20, 200, 2000d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            foreach (EPStatement stmt in deleteStatements) {
                stmt.Dispose();
            }
    
            // test single-two-field index reuse
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create window WinOne#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_ST0 select * from WinOne where TheString = key0");
            Assert.AreEqual(1, GetNWMW(epService).GetNamedWindowIndexes("WinOne").Length);
    
            epService.EPAdministrator.CreateEPL("on SupportBean_ST0 select * from WinOne where TheString = key0 and IntPrimitive = p00");
            Assert.AreEqual(1, GetNWMW(epService).GetNamedWindowIndexes("WinOne").Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCoercionRangeMultiPropIndexes(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBeanTwo>();
    
            // create window
            string stmtTextCreate = "create window MyWindowCR#keepall as select " +
                    "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            string stmtText = "insert into MyWindowCR select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtText);
            var fields = new string[]{"TheString"};
    
            SendSupportBean(epService, "E1", 1, 10, 100d, 1000d);
            SendSupportBean(epService, "E2", 2, 20, 200d, 2000d);
            SendSupportBean(epService, "E3", 3, 30, 3d, 30d);
            SendSupportBean(epService, "E4", 4, 40, 4d, 40d);
            SendSupportBean(epService, "E5", 5, 50, 500d, 5000d);
            SendSupportBean(epService, "E6", 6, 60, 600d, 6000d);
            listenerWindow.Reset();
    
            var deleteStatements = new List<EPStatement>();
            string stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive between s2.DoublePrimitiveTwo and s2.DoubleBoxedTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(1, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
    
            SendSupportBeanTwo(epService, "T", 0, 0, 0d, null);
            Assert.IsFalse(listenerWindow.IsInvoked);
            SendSupportBeanTwo(epService, "T", 0, 0, -1d, 1d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1"});
    
            stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive between s2.IntPrimitiveTwo and s2.IntBoxedTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
    
            SendSupportBeanTwo(epService, "T", -2, 2, 0d, 0d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
    
            stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindowCR as win " +
                    "where win.IntPrimitive between s2.IntPrimitiveTwo and s2.IntBoxedTwo and win.DoublePrimitive between s2.IntPrimitiveTwo and s2.IntBoxedTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
    
            SendSupportBeanTwo(epService, "T", -3, 3, -3d, 3d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
    
            stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindowCR as win " +
                    "where win.DoublePrimitive between s2.IntPrimitiveTwo and s2.IntPrimitiveTwo and win.IntPrimitive between s2.IntPrimitiveTwo and s2.IntPrimitiveTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
    
            SendSupportBeanTwo(epService, "T", -4, 4, -4, 4d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E4"});
    
            stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive <= DoublePrimitiveTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
    
            SendSupportBeanTwo(epService, "T", 0, 0, 5, 1d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E5"});
    
            stmtTextDelete = "on SupportBeanTwo as s2 delete from MyWindowCR as win where win.IntPrimitive not between s2.IntPrimitiveTwo and s2.IntBoxedTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
    
            SendSupportBeanTwo(epService, "T", 100, 200, 0, 0d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E6"});
    
            // delete
            foreach (EPStatement stmt in deleteStatements) {
                stmt.Dispose();
            }
            deleteStatements.Clear();
            Assert.AreEqual(0, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCR").Length);
        }
    
        private void RunAssertionCoercionKeyAndRangeMultiPropIndexes(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBeanTwo>();
    
            // create window
            string stmtTextCreate = "create window MyWindowCKR#keepall as select " +
                    "TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            string stmtText = "insert into MyWindowCKR select TheString, IntPrimitive, IntBoxed, DoublePrimitive, DoubleBoxed from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtText);
            var fields = new string[]{"TheString"};
    
            SendSupportBean(epService, "E1", 1, 10, 100d, 1000d);
            SendSupportBean(epService, "E2", 2, 20, 200d, 2000d);
            SendSupportBean(epService, "E3", 3, 30, 300d, 3000d);
            SendSupportBean(epService, "E4", 4, 40, 400d, 4000d);
            listenerWindow.Reset();
    
            var deleteStatements = new List<EPStatement>();
            string stmtTextDelete = "on SupportBeanTwo delete from MyWindowCKR where TheString = stringTwo and IntPrimitive between DoublePrimitiveTwo and DoubleBoxedTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(1, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCKR").Length);
    
            SendSupportBeanTwo(epService, "T", 0, 0, 1d, 200d);
            Assert.IsFalse(listenerWindow.IsInvoked);
            SendSupportBeanTwo(epService, "E1", 0, 0, 1d, 200d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1"});
    
            stmtTextDelete = "on SupportBeanTwo delete from MyWindowCKR where TheString = stringTwo and IntPrimitive = IntPrimitiveTwo and IntBoxed between DoublePrimitiveTwo and DoubleBoxedTwo";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCKR").Length);
    
            SendSupportBeanTwo(epService, "E2", 2, 0, 19d, 21d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
    
            stmtTextDelete = "on SupportBeanTwo delete from MyWindowCKR where IntBoxed between DoubleBoxedTwo and DoublePrimitiveTwo and IntPrimitive = IntPrimitiveTwo and TheString = stringTwo ";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(2, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCKR").Length);
    
            SendSupportBeanTwo(epService, "E3", 3, 0, 29d, 34d);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
    
            stmtTextDelete = "on SupportBeanTwo delete from MyWindowCKR where IntBoxed between IntBoxedTwo and IntBoxedTwo and IntPrimitive = IntPrimitiveTwo and TheString = stringTwo ";
            deleteStatements.Add(epService.EPAdministrator.CreateEPL(stmtTextDelete));
            Assert.AreEqual(3, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCKR").Length);
    
            SendSupportBeanTwo(epService, "E4", 4, 40, 0d, null);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E4"});
    
            // delete
            foreach (EPStatement stmt in deleteStatements) {
                stmt.Dispose();
            }
            deleteStatements.Clear();
            Assert.AreEqual(0, GetNWMW(epService).GetNamedWindowIndexes("MyWindowCKR").Length);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, int? intBoxed,
                                     double doublePrimitive, double? doubleBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.DoublePrimitive = doublePrimitive;
            bean.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanTwo(EPServiceProvider epService, string theString, int intPrimitive, int? intBoxed,
                                        double doublePrimitive, double? doubleBoxed) {
            var bean = new SupportBeanTwo();
            bean.StringTwo = theString;
            bean.IntPrimitiveTwo = intPrimitive;
            bean.IntBoxedTwo = intBoxed;
            bean.DoublePrimitiveTwo = doublePrimitive;
            bean.DoubleBoxedTwo = doubleBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private long GetCount(EPServiceProvider epService, string windowName) {
            NamedWindowProcessor processor = GetNWMW(epService).GetProcessor(windowName);
            return processor.GetProcessorInstance(null).CountDataWindow;
        }
    
        private NamedWindowMgmtService GetNWMW(EPServiceProvider epService) {
            return ((EPServiceProviderSPI) epService).NamedWindowMgmtService;
        }
    }
} // end of namespace
