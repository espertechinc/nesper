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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientTimeControlEvent : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.AddEventType("SupportBean", typeof(SupportBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSendTimeSpan(epService);
            RunAssertionSendTimeSpanIsolated(epService);
            RunAssertionNextScheduledTime(epService);
        }
    
        private void RunAssertionSendTimeSpan(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select current_timestamp() as ct from pattern[every timer:interval(1.5 sec)]");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(3500));
            Assert.AreEqual(2, listener.NewDataList.Count);
            Assert.AreEqual(1500L, listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(3000L, listener.NewDataList[1][0].Get("ct"));
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4500));
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(4500L, listener.NewDataList[0][0].Get("ct"));
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(9000));
            Assert.AreEqual(3, listener.NewDataList.Count);
            Assert.AreEqual(6000L, listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(7500L, listener.NewDataList[1][0].Get("ct"));
            Assert.AreEqual(9000L, listener.NewDataList[2][0].Get("ct"));
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(0, listener.NewDataList.Count);
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(0, listener.NewDataList.Count);
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(10500L, listener.NewDataList[0][0].Get("ct"));
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(0, listener.NewDataList.Count);
    
            epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(14000, 200));
            Assert.AreEqual(14000, epService.EPRuntime.CurrentTime);
            Assert.AreEqual(2, listener.NewDataList.Count);
            Assert.AreEqual(12100L, listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(13700L, listener.NewDataList[1][0].Get("ct"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSendTimeSpanIsolated(EPServiceProvider epService) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            EPStatement stmtOne = isolated.EPAdministrator.CreateEPL("select current_timestamp() as ct from pattern[every timer:interval(1.5 sec)]", null, null);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(3500));
            Assert.AreEqual(2, listener.NewDataList.Count);
            Assert.AreEqual(1500L, listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(3000L, listener.NewDataList[1][0].Get("ct"));
            listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4500));
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(4500L, listener.NewDataList[0][0].Get("ct"));
            listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(9000));
            Assert.AreEqual(3, listener.NewDataList.Count);
            Assert.AreEqual(6000L, listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(7500L, listener.NewDataList[1][0].Get("ct"));
            Assert.AreEqual(9000L, listener.NewDataList[2][0].Get("ct"));
            listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(0, listener.NewDataList.Count);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(10499, isolated.EPRuntime.CurrentTime);
            Assert.AreEqual(0, listener.NewDataList.Count);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(10500L, listener.NewDataList[0][0].Get("ct"));
            listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(0, listener.NewDataList.Count);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(14000, 200));
            Assert.AreEqual(14000, isolated.EPRuntime.CurrentTime);
            Assert.AreEqual(2, listener.NewDataList.Count);
            Assert.AreEqual(12100L, listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(13700L, listener.NewDataList[1][0].Get("ct"));
    
            isolated.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNextScheduledTime(EPServiceProvider epService) {
    
            EPRuntimeSPI runtimeSPI = (EPRuntimeSPI) epService.EPRuntime;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            Assert.IsNull(epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new Object[0][]);
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select * from pattern[timer:interval(2 sec)]");
            Assert.AreEqual(2000L, (long) epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {stmtOne.Name, 2000L}});
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("@Name('s2') select * from pattern[timer:interval(150 msec)]");
            Assert.AreEqual(150L, (long) epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {"s2", 150L}, new object[] {stmtOne.Name, 2000L}});
    
            stmtTwo.Dispose();
            Assert.AreEqual(2000L, (long) epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {stmtOne.Name, 2000L}});
    
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL("select * from pattern[timer:interval(3 sec) and timer:interval(4 sec)]");
            Assert.AreEqual(2000L, (long) epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {stmtOne.Name, 2000L}, new object[] {stmtThree.Name, 3000L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2500));
            Assert.AreEqual(3000L, (long) epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {stmtThree.Name, 3000L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3500));
            Assert.AreEqual(4000L, (long) epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new object[][]{new object[] {stmtThree.Name, 4000L}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4500));
            Assert.AreEqual(null, epService.EPRuntime.NextScheduledTime);
            AssertSchedules(runtimeSPI.StatementNearestSchedules, new Object[0][]);
    
            // test isolated service
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("I1");
            EPRuntimeIsolatedSPI isolatedSPI = (EPRuntimeIsolatedSPI) isolated.EPRuntime;
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            Assert.IsNull(isolated.EPRuntime.NextScheduledTime);
            AssertSchedules(isolatedSPI.StatementNearestSchedules, new Object[0][]);
    
            EPStatement stmtFour = isolated.EPAdministrator.CreateEPL("select * from pattern[timer:interval(2 sec)]", null, null);
            Assert.AreEqual(2000L, (long) isolatedSPI.NextScheduledTime);
            AssertSchedules(isolatedSPI.StatementNearestSchedules, new object[][]{new object[] {stmtFour.Name, 2000L}});
    
            isolated.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertSchedules(IDictionary<string, long> schedules, object[][] expected) {
            ScopeTestHelper.AssertEquals(expected.Length, schedules.Count);
    
            var matchNumber = new HashSet<int?>();
            foreach (var entryObj in schedules) {
                var entry = entryObj;
                bool matchFound = false;
                for (int i = 0; i < expected.Length; i++) {
                    if (matchNumber.Contains(i)) {
                        continue;
                    }
                    if (expected[i][0].Equals(entry.Key)) {
                        matchFound = true;
                        matchNumber.Add(i);
#if false
                        if (expected[i][1] == null && entry.Value == null) {
                            continue;
                        }
#endif
                        if (!expected[i][1].Equals(entry.Value)) {
                            ScopeTestHelper.Fail("Failed to match value for key '" + entry.Key + "' expected '" + expected[i][i] + "' received '" + entry.Value + "'");
                        }
                    }
                }
                if (!matchFound) {
                    ScopeTestHelper.Fail("Failed to find key '" + entry.Key + "'");
                }
            }
        }
    }
} // end of namespace
