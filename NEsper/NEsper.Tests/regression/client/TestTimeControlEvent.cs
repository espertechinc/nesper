///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestTimeControlEvent 
    {
        private EPServiceProvider _epService;
        private EPRuntimeSPI _runtimeSpi;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            configuration.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
            configuration.AddEventType("SupportBean", typeof(SupportBean).FullName);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _runtimeSpi = (EPRuntimeSPI) _epService.EPRuntime;
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestSendTimeSpan()
        {
            var d = DateTime.Parse("2010-01-01 00:00:00.000");
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("select Current_timestamp() as ct from pattern[every timer:interval(1.5 sec)]");
            stmtOne.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(3500));
            Assert.AreEqual(2, _listener.NewDataList.Count);
            Assert.AreEqual(1500L, _listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(3000L, _listener.NewDataList[1][0].Get("ct"));
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4500));
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(4500L, _listener.NewDataList[0][0].Get("ct"));
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(9000));
            Assert.AreEqual(3, _listener.NewDataList.Count);
            Assert.AreEqual(6000L, _listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(7500L, _listener.NewDataList[1][0].Get("ct"));
            Assert.AreEqual(9000L, _listener.NewDataList[2][0].Get("ct"));
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(0, _listener.NewDataList.Count);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(0, _listener.NewDataList.Count);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(10500L, _listener.NewDataList[0][0].Get("ct"));
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(0, _listener.NewDataList.Count);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeSpanEvent(14000, 200));
            Assert.AreEqual(14000, _epService.EPRuntime.CurrentTime);
            Assert.AreEqual(2, _listener.NewDataList.Count);
            Assert.AreEqual(12100L, _listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(13700L, _listener.NewDataList[1][0].Get("ct"));
            _listener.Reset();
        }
    
        [Test]
        public void TestSendTimeSpanIsolated() {
    
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            EPStatement stmtOne = isolated.EPAdministrator.CreateEPL("select Current_timestamp() as ct from pattern[every timer:interval(1.5 sec)]", null, null);
            stmtOne.Events += _listener.Update;
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(3500));
            Assert.AreEqual(2, _listener.NewDataList.Count);
            Assert.AreEqual(1500L, _listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(3000L, _listener.NewDataList[1][0].Get("ct"));
            _listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(4500));
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(4500L, _listener.NewDataList[0][0].Get("ct"));
            _listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(9000));
            Assert.AreEqual(3, _listener.NewDataList.Count);
            Assert.AreEqual(6000L, _listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(7500L, _listener.NewDataList[1][0].Get("ct"));
            Assert.AreEqual(9000L, _listener.NewDataList[2][0].Get("ct"));
            _listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(0, _listener.NewDataList.Count);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10499));
            Assert.AreEqual(10499, isolated.EPRuntime.CurrentTime);
            Assert.AreEqual(0, _listener.NewDataList.Count);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(10500L, _listener.NewDataList[0][0].Get("ct"));
            _listener.Reset();
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(10500));
            Assert.AreEqual(0, _listener.NewDataList.Count);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeSpanEvent(14000, 200));
            Assert.AreEqual(14000, isolated.EPRuntime.CurrentTime);
            Assert.AreEqual(2, _listener.NewDataList.Count);
            Assert.AreEqual(12100L, _listener.NewDataList[0][0].Get("ct"));
            Assert.AreEqual(13700L, _listener.NewDataList[1][0].Get("ct"));
            _listener.Reset();
        }
    
        [Test]
        public void TestNextScheduledTime() {
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            Assert.IsNull(_epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[0][]);
    
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("select * from pattern[timer:interval(2 sec)]");
            Assert.AreEqual(2000L, (long) _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[][] { new Object[] {stmtOne.Name, 2000L}});
    
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("@Name('s2') select * from pattern[timer:interval(150 msec)]");
            Assert.AreEqual(150L, (long) _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[][] { new Object[] {"s2", 150L}, new Object[] {stmtOne.Name, 2000L}});
    
            stmtTwo.Dispose();
            Assert.AreEqual(2000L, (long) _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[][] { new Object[] {stmtOne.Name, 2000L}});
    
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL("select * from pattern[timer:interval(3 sec) and timer:interval(4 sec)]");
            Assert.AreEqual(2000L, (long) _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[][] { new Object[] {stmtOne.Name, 2000L}, new Object[] {stmtThree.Name, 3000L}});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2500));
            Assert.AreEqual(3000L, (long) _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[][] { new Object[] {stmtThree.Name, 3000L}});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3500));
            Assert.AreEqual(4000L, (long) _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[][] { new Object[] {stmtThree.Name, 4000L}});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(4500));
            Assert.AreEqual(null, _epService.EPRuntime.NextScheduledTime);
            AssertSchedules(_runtimeSpi.StatementNearestSchedules, new Object[0][]);
            
            // test isolated service
            EPServiceProviderIsolated isolated = _epService.GetEPServiceIsolated("I1");
            EPRuntimeIsolatedSPI isolatedSPI = (EPRuntimeIsolatedSPI) isolated.EPRuntime;
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            Assert.IsNull(isolated.EPRuntime.NextScheduledTime);
            AssertSchedules(isolatedSPI.StatementNearestSchedules, new Object[0][]);
    
            EPStatement stmtFour = isolated.EPAdministrator.CreateEPL("select * from pattern[timer:interval(2 sec)]", null, null);
            Assert.AreEqual(2000L, (long) isolatedSPI.NextScheduledTime);
            AssertSchedules(isolatedSPI.StatementNearestSchedules, new Object[][] { new Object[] {stmtFour.Name, 2000L}});
        }
    
        private void AssertSchedules(IDictionary<String, long> schedules, Object[][] expected) {
            ScopeTestHelper.AssertEquals(expected.Length, schedules.Count);

            ICollection<int> matchNumber = new HashSet<int>();
            foreach (KeyValuePair<string, long> entry in schedules) {
                bool matchFound = false;
                for (int i = 0; i < expected.Length; i++) {
                    if (matchNumber.Contains(i)) {
                        continue;
                    }
                    if (expected[i][0].Equals(entry.Key)) {
                        matchFound = true;
                        matchNumber.Add(i);
                        if (expected[i][1] == null && entry.Value == null) {
                            continue;
                        }
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
}
