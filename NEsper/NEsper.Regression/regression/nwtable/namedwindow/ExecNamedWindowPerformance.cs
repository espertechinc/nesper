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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
    
            RunAssertionOnSelectInKeywordPerformance(epService);
            RunAssertionOnSelectEqualsAndRangePerformance(epService);
            RunAssertionDeletePerformance(epService);
            RunAssertionDeletePerformanceCoercion(epService);
            RunAssertionDeletePerformanceTwoDeleters(epService);
            RunAssertionDeletePerformanceIndexReuse(epService);
        }
    
        private void RunAssertionOnSelectInKeywordPerformance(EPServiceProvider epService) {
            // create window
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
    
            int maxRows = 10000;   // for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, "p00_" + i));
            }
    
            string eplSingleIdx = "on SupportBean_S1 select sum(mw.id) as sumi from MyWindow mw where p00 in (p10, p11)";
            RunOnDemandAssertion(epService, eplSingleIdx, 1, new SupportBean_S1(0, "x", "p00_6523"), 6523);
    
            string eplMultiIndex = "on SupportBean_S1 select sum(mw.id) as sumi from MyWindow mw where p10 in (p00, p01)";
            RunOnDemandAssertion(epService, eplMultiIndex, 2, new SupportBean_S1(0, "p00_6524"), 6524);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        private void RunAssertionOnSelectEqualsAndRangePerformance(EPServiceProvider epService) {
            // create window one
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                var bean = new SupportBean((i < 5000) ? "A" : "B", i);
                bean.LongPrimitive = i;
                bean.LongBoxed = (i + 1);
                epService.EPRuntime.SendEvent(bean);
            }
            epService.EPRuntime.SendEvent(new SupportBean("B", 100));
    
            string eplIdx1One = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow where IntPrimitive = sbr.rangeStart";
            RunOnDemandAssertion(epService, eplIdx1One, 1, new SupportBeanRange("R", 5501, 0), 5501);
    
            string eplIdx1Two = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow where IntPrimitive between sbr.rangeStart and sbr.rangeEnd";
            RunOnDemandAssertion(epService, eplIdx1Two, 1, new SupportBeanRange("R", 5501, 5503), 5501 + 5502 + 5503);
    
            string eplIdx1Three = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow where TheString = key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd";
            RunOnDemandAssertion(epService, eplIdx1Three, 1, new SupportBeanRange("R", "A", 4998, 5503), 4998 + 4999);
    
            string eplIdx1Four = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow " +
                    "where TheString = key and LongPrimitive = rangeStart and IntPrimitive between rangeStart and rangeEnd " +
                    "and LongBoxed between rangeStart and rangeEnd";
            RunOnDemandAssertion(epService, eplIdx1Four, 1, new SupportBeanRange("R", "A", 4998, 5503), 4998);
    
            string eplIdx1Five = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow " +
                    "where IntPrimitive between rangeStart and rangeEnd " +
                    "and LongBoxed between rangeStart and rangeEnd";
            RunOnDemandAssertion(epService, eplIdx1Five, 1, new SupportBeanRange("R", "A", 4998, 5001), 4998 + 4999 + 5000);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        private void RunOnDemandAssertion(EPServiceProvider epService, string epl, int numIndexes, Object theEvent, int? expected) {
            Assert.AreEqual(0, ((EPServiceProviderSPI) epService).NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(numIndexes, ((EPServiceProviderSPI) epService).NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
    
            long start = PerformanceObserver.MilliTime;
            int loops = 1000;
    
            for (int i = 0; i < loops; i++) {
                epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("sumi"));
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 1000, "delta=" + delta);
    
            stmt.Dispose();
            Assert.AreEqual(0, ((EPServiceProviderSPI) epService).NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
        }
    
        private void RunAssertionDeletePerformance(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow where id = a";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            for (int i = 0; i < 50000; i++) {
                SendSupportBean(epService, "S" + i, i);
            }
    
            // delete rows
            var listener = new SupportUpdateListener();
            stmtCreate.Events += listener.Update;

            var delta = PerformanceObserver.TimeMillis(
                () => {
                    for (int i = 0; i < 10000; i++) {
                        SendSupportBean_A(epService, "S" + i);
                    }
                });

            Assert.IsTrue(delta < 100, "Delta=" + delta);
    
            // assert they are deleted
            Assert.AreEqual(50000 - 10000, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
            Assert.AreEqual(10000, listener.OldDataList.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        private void RunAssertionDeletePerformanceCoercion(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where b = price";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            for (int i = 0; i < 50000; i++) {
                SendSupportBean(epService, "S" + i, (long) i);
            }
    
            // delete rows
            var listener = new SupportUpdateListener();
            stmtCreate.Events += listener.Update;
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                SendMarketBean(epService, "S" + i, i);
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 500, "Delta=" + delta);
    
            // assert they are deleted
            Assert.AreEqual(50000 - 10000, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
            Assert.AreEqual(10000, listener.OldDataList.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        private void RunAssertionDeletePerformanceTwoDeleters(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecNamedWindowPerformance))) {
                return;
            }
    
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt one
            string stmtTextDeleteOne = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where b = price";
            epService.EPAdministrator.CreateEPL(stmtTextDeleteOne);
    
            // create delete stmt two
            string stmtTextDeleteTwo = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow where id = a";
            epService.EPAdministrator.CreateEPL(stmtTextDeleteTwo);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            for (int i = 0; i < 20000; i++) {
                SendSupportBean(epService, "S" + i, (long) i);
            }
    
            // delete all rows
            var listener = new SupportUpdateListener();
            stmtCreate.Events += listener.Update;
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                SendMarketBean(epService, "S" + i, i);
                SendSupportBean_A(epService, "S" + (i + 10000));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 1500, "Delta=" + delta);
    
            // assert they are all deleted
            Assert.AreEqual(0, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
            Assert.AreEqual(20000, listener.OldDataList.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        private void RunAssertionDeletePerformanceIndexReuse(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt
            var statements = new EPStatement[50];
            for (int i = 0; i < statements.Length; i++) {
                string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where b = price";
                statements[i] = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            }
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                SendSupportBean(epService, "S" + i, (long) i);
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 1000, "Delta=" + delta);
            Assert.AreEqual(10000, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
    
            // destroy all
            foreach (EPStatement statement in statements) {
                statement.Dispose();
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    
        private void SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, long longPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
