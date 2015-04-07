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

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAggregateRowPerEvent
    {
        private const String JOIN_KEY = "KEY";
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
        private int _eventCount;
    
        [SetUp]
        public void SetUp() {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _eventCount = 0;
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestAggregatedSelectTriggerEvent()
        {
            var configuration = _epService.EPAdministrator.Configuration;

            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            String epl = "select window(s0.*) as rows, sb " +
                    "from SupportBean.win:keepall() as sb, SupportBean_S0.win:keepall() as s0 " +
                    "where sb.theString = s0.p00";
            _epService.EPAdministrator.CreateEPL(epl).AddListener(_testListener);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "K1", "V1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "K1", "V2"));

            // test SB-direction
            SupportBean b1 = new SupportBean("K1", 0);
            _epService.EPRuntime.SendEvent(b1);
            EventBean[] events = _testListener.GetAndResetLastNewData();
            Assert.AreEqual(2, events.Length);
            foreach (EventBean @eventX in events) {
                Assert.AreEqual(b1, @eventX.Get("sb"));
                Assert.AreEqual(2, ((SupportBean_S0[]) @eventX.Get("rows")).Length);
            }

            // test S0-direction
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "K1", "V3"));
            EventBean @event = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(b1, @event.Get("sb"));
            Assert.AreEqual(3, ((SupportBean_S0[]) @event.Get("rows")).Length);
        }

        [Test]
        public void TestAggregatedSelectUnaggregatedHaving() {
            // ESPER-571
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            String epl = "select max(IntPrimitive) as val from SupportBean.win:time(1) having max(IntPrimitive) > IntBoxed";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _testListener.Update;
    
            SendEvent("E1", 10, 1);
            Assert.AreEqual(10, _testListener.AssertOneGetNewAndReset().Get("val"));
    
            SendEvent("E2", 10, 11);
            Assert.IsFalse(_testListener.IsInvoked);
    
            SendEvent("E3", 15, 11);
            Assert.AreEqual(15, _testListener.AssertOneGetNewAndReset().Get("val"));
    
            SendEvent("E4", 20, 11);
            Assert.AreEqual(20, _testListener.AssertOneGetNewAndReset().Get("val"));
    
            SendEvent("E5", 25, 25);
            Assert.IsFalse(_testListener.IsInvoked);
        }
    
        [Test]
        public void TestSumOneView() {
            String viewExpr = "select irstream LongPrimitive, sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBean).FullName + ".win:length(3)";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            RunAssert(selectTestView);
        }
    
        [Test]
        public void TestSumJoin() {
            String viewExpr = "select irstream LongPrimitive, sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
                    typeof(SupportBean).FullName + ".win:length(3) as two " +
                    "where one.TheString = two.TheString";
    
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
    
            RunAssert(selectTestView);
        }
    
        [Test]
        public void TestSumAvgWithWhere() {
            String viewExpr = "select 'IBM stats' as title, Volume, avg(Volume) as myAvg, sum(Volume) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3)" +
                    "where symbol='IBM'";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            SendMarketDataEvent("GE", 10L);
            Assert.IsFalse(_testListener.IsInvoked);
    
            SendMarketDataEvent("IBM", 20L);
            AssertPostedNew(20d, 20L);
    
            SendMarketDataEvent("XXX", 10000L);
            Assert.IsFalse(_testListener.IsInvoked);
    
            SendMarketDataEvent("IBM", 30L);
            AssertPostedNew(25d, 50L);
        }
    
        private void AssertPostedNew(double? newAvg, long? newSum) {
            EventBean[] oldData = _testListener.LastOldData;
            EventBean[] newData = _testListener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual("IBM stats", newData[0].Get("title"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
    
            _testListener.Reset();
        }
    
        private void RunAssert(EPStatement selectTestView) {
            String[] fields = new String[]{"LongPrimitive", "mySum"};
    
            // assert select result type
            Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, null);
    
            SendEvent(10);
            Assert.AreEqual(10L, _testListener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{new Object[] {1L, 10L}});
    
            SendEvent(15);
            Assert.AreEqual(25L, _testListener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{new Object[] {1L, 25L}, new Object[] {2L, 25L}});
    
            SendEvent(-5);
            Assert.AreEqual(20L, _testListener.GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(_testListener.LastOldData);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{new Object[] {1L, 20L}, new Object[] {2L, 20L}, new Object[] {3L, 20L}});
    
            SendEvent(-2);
            Assert.AreEqual(8L, _testListener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(8L, _testListener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{new Object[] {4L, 8L}, new Object[] {2L, 8L}, new Object[] {3L, 8L}});
    
            SendEvent(100);
            Assert.AreEqual(93L, _testListener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(93L, _testListener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{new Object[] {4L, 93L}, new Object[] {5L, 93L}, new Object[] {3L, 93L}});
    
            SendEvent(1000);
            Assert.AreEqual(1098L, _testListener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(1098L, _testListener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{new Object[] {4L, 1098L}, new Object[] {5L, 1098L}, new Object[] {6L, 1098L}});
        }
    
        private void SendEvent(long longBoxed, int intBoxed, short shortBoxed) {
            SupportBean bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            bean.LongPrimitive = ++_eventCount;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketDataEvent(String symbol, long? volume) {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, volume, null);
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(long longBoxed) {
            SendEvent(longBoxed, 0, (short) 0);
        }
    
        private void SendEvent(String stringValue, int intPrimitive, int intBoxed) {
            SupportBean theEvent = new SupportBean(stringValue, intPrimitive);
            theEvent.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
