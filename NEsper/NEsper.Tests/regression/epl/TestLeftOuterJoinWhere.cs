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
    public class TestLeftOuterJoinWhere  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        private SupportBean_S0[] _eventsS0;
        private SupportBean_S1[] _eventsS1;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
    
            int count = 100;
            _eventsS0 = new SupportBean_S0[15];
            _eventsS1 = new SupportBean_S1[15];
            for (int i = 0; i < _eventsS0.Length; i++) {
                _eventsS0[i] = new SupportBean_S0(count++, Convert.ToString(i));
            }
            count = 200;
            for (int i = 0; i < _eventsS1.Length; i++) {
                _eventsS1[i] = new SupportBean_S1(count++, Convert.ToString(i));
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
            _eventsS0 = null;
            _eventsS1 = null;
        }
    
        [Test]
        public void TestWhereNotNullIs() {
            SetupStatement("where s1.p11 is not null");
            TryWhereNotNull();
        }
    
        [Test]
        public void TestWhereNotNullNE() {
            SetupStatement("where s1.p11 is not null");
            TryWhereNotNull();
        }
    
        [Test]
        public void TestWhereNullIs() {
            SetupStatement("where s1.p11 is null");
            TryWhereNull();
        }
    
        [Test]
        public void TestWhereNullEq() {
            SetupStatement("where s1.p11 is null");
            TryWhereNull();
        }
    
        [Test]
        public void TestWhereJoinOrNull() {
            SetupStatement("where s0.p01 = s1.p11 or s1.p11 is null");
    
            // Send S0[0] p01=a
            _eventsS0[0].P01 = "[a]";
            SendEvent(_eventsS0[0]);
            CompareEvent(_updateListener.AssertOneGetNewAndReset(), _eventsS0[0], null);
    
            // Send events to test the join for multiple rows incl. null value
            SupportBean_S1 s1_1 = new SupportBean_S1(1000, "5", "X");
            SupportBean_S1 s1_2 = new SupportBean_S1(1001, "5", "Y");
            SupportBean_S1 s1_3 = new SupportBean_S1(1002, "5", "X");
            SupportBean_S1 s1_4 = new SupportBean_S1(1003, "5", null);
            SupportBean_S0 s0 = new SupportBean_S0(1, "5", "X");
            SendEvent(new Object[]{s1_1, s1_2, s1_3, s1_4, s0});
    
            Assert.AreEqual(3, _updateListener.LastNewData.Length);
            Object[] received = new Object[3];
            for (int i = 0; i < 3; i++) {
                Assert.AreSame(s0, _updateListener.LastNewData[i].Get("s0"));
                received[i] = _updateListener.LastNewData[i].Get("s1");
            }
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{s1_1, s1_3, s1_4}, received);
        }
    
        [Test]
        public void TestWhereJoin() {
            SetupStatement("where s0.p01 = s1.p11");
    
            // Send S0[0] p01=a
            _eventsS0[0].P01 = "[a]";
            SendEvent(_eventsS0[0]);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            // Send S1[1] p11=b
            _eventsS1[1].P11 = "[b]";
            SendEvent(_eventsS1[1]);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            // Send S0[1] p01=c, no match expected
            _eventsS0[1].P01 = "[c]";
            SendEvent(_eventsS0[1]);
            Assert.IsFalse(_updateListener.IsInvoked);
    
            // Send S1[2] p11=d
            _eventsS1[2].P11 = "[d]";
            SendEvent(_eventsS1[2]);
            // Send S0[2] p01=d
            _eventsS0[2].P01 = "[d]";
            SendEvent(_eventsS0[2]);
            CompareEvent(_updateListener.AssertOneGetNewAndReset(), _eventsS0[2], _eventsS1[2]);
    
            // Send S1[3] and S0[3] with differing props, no match expected
            _eventsS1[3].P11 = "[e]";
            SendEvent(_eventsS1[3]);
            _eventsS0[3].P01 = "[e1]";
            SendEvent(_eventsS0[3]);
            Assert.IsFalse(_updateListener.IsInvoked);
        }
    
        public EPStatement SetupStatement(String whereClause) {
            String joinStatement = "select * from " +
                    typeof(SupportBean_S0).FullName + ".win:length(5) as s0 " +
                    "left outer join " +
                    typeof(SupportBean_S1).FullName + ".win:length(5) as s1" +
                    " on s0.P00 = s1.p10 " +
                    whereClause;
    
            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            outerJoinView.Events += _updateListener.Update;
            return outerJoinView;
        }
    
        [Test]
        public void TestEventType() {
            EPStatement outerJoinView = SetupStatement("");
            EventType type = outerJoinView.EventType;
            Assert.AreEqual(typeof(SupportBean_S0), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("s1"));
        }
    
        private void TryWhereNotNull() {
            SupportBean_S1 s1_1 = new SupportBean_S1(1000, "5", "X");
            SupportBean_S1 s1_2 = new SupportBean_S1(1001, "5", null);
            SupportBean_S1 s1_3 = new SupportBean_S1(1002, "6", null);
            SendEvent(new Object[]{s1_1, s1_2, s1_3});
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SupportBean_S0 s0 = new SupportBean_S0(1, "5", "X");
            SendEvent(s0);
            CompareEvent(_updateListener.AssertOneGetNewAndReset(), s0, s1_1);
        }
    
        private void TryWhereNull() {
            SupportBean_S1 s1_1 = new SupportBean_S1(1000, "5", "X");
            SupportBean_S1 s1_2 = new SupportBean_S1(1001, "5", null);
            SupportBean_S1 s1_3 = new SupportBean_S1(1002, "6", null);
            SendEvent(new Object[]{s1_1, s1_2, s1_3});
            Assert.IsFalse(_updateListener.IsInvoked);
    
            SupportBean_S0 s0 = new SupportBean_S0(1, "5", "X");
            SendEvent(s0);
            CompareEvent(_updateListener.AssertOneGetNewAndReset(), s0, s1_2);
        }
    
        private void CompareEvent(EventBean receivedEvent, SupportBean_S0 expectedS0, SupportBean_S1 expectedS1) {
            Assert.AreSame(expectedS0, receivedEvent.Get("s0"));
            Assert.AreSame(expectedS1, receivedEvent.Get("s1"));
        }
    
        private void SendEvent(Object[] events) {
            for (int i = 0; i < events.Length; i++) {
                SendEvent(events[i]);
            }
        }
    
        private void SendEvent(Object theEvent) {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
