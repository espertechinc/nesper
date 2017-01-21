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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinMultiKeyAndRange  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        private readonly int[][] _eventData = {
                new[] {1, 100},
                new[] {2, 100},
                new[] {1, 200},
                new[] {2, 200}
        };

        private SupportBean[] _eventsA;
        private SupportBean[] _eventsB;
    
        [SetUp]
        public void SetUp() {
            _eventsA = new SupportBean[_eventData.Length];
            _eventsB = new SupportBean[_eventData.Length];

            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestRangeNullAndDupAndInvalid() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanComplexProps", typeof(SupportBeanComplexProps));
    
            String eplOne = "select sb.* from SupportBean.win:keepall() sb, SupportBeanRange.std:lastevent() where IntBoxed between rangeStart and rangeEnd";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(eplOne);
            stmtOne.Events += _listener.Update;
    
            String eplTwo = "select sb.* from SupportBean.win:keepall() sb, SupportBeanRange.std:lastevent() where TheString = key and IntBoxed in [rangeStart: rangeEnd]";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(eplTwo);
            SupportUpdateListener listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            // null join lookups
            SendEvent(new SupportBeanRange("R1", "G", (int?) null, null));
            SendEvent(new SupportBeanRange("R2", "G", null, 10));
            SendEvent(new SupportBeanRange("R3", "G", 10, null));
            SendSupportBean("G", -1, null);
    
            // range invalid
            SendEvent(new SupportBeanRange("R4", "G", 10, 0));
            Assert.IsFalse(_listener.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            // duplicates
            Object eventOne = SendSupportBean("G", 100, 5);
            Object eventTwo = SendSupportBean("G", 101, 5);
            SendEvent(new SupportBeanRange("R4", "G", 0, 10));
            EventBean[] events = _listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));
            events = listenerTwo.GetAndResetLastNewData();
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));
    
            // test string compare
            String eplThree = "select sb.* from SupportBeanRange.win:keepall() sb, SupportBean.std:lastevent() where TheString in [rangeStartStr:rangeEndStr]";
            _epService.EPAdministrator.CreateEPL(eplThree);
    
            SendSupportBean("P", 1, 1);
            SendEvent(new SupportBeanRange("R5", "R5", "O", "Q"));
            Assert.IsTrue(_listener.IsInvoked);
    
        }
    
        [Test]
        public void TestMultiKeyed() {
    
            String eventClass = typeof(SupportBean).FullName;
    
            String joinStatement = "select * from " +
                    eventClass + "(TheString='A').win:length(3) as streamA," +
                    eventClass + "(TheString='B').win:length(3) as streamB" +
                    " where streamA.IntPrimitive = streamB.IntPrimitive " +
                    "and streamA.IntBoxed = streamB.IntBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;
    
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("streamA"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("streamB"));
            Assert.AreEqual(2, stmt.EventType.PropertyNames.Length);
    
            for (int i = 0; i < _eventData.Length; i++) {
                _eventsA[i] = new SupportBean();
                _eventsA[i].TheString = "A";
                _eventsA[i].IntPrimitive = _eventData[i][0];
                _eventsA[i].IntBoxed = _eventData[i][1];
    
                _eventsB[i] = new SupportBean();
                _eventsB[i].TheString = "B";
                _eventsB[i].IntPrimitive = _eventData[i][0];
                _eventsB[i].IntBoxed = _eventData[i][1];
            }
    
            SendEvent(_eventsA[0]);
            SendEvent(_eventsB[1]);
            SendEvent(_eventsB[2]);
            SendEvent(_eventsB[3]);
            Assert.IsNull(_listener.LastNewData);    // No events expected
        }
    
        private void SendEvent(Object theEvent) {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private SupportBean SendSupportBean(String stringValue, int intPrimitive, int? intBoxed) {
            SupportBean bean = new SupportBean(stringValue, intPrimitive);
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
