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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    /// <summary>
    /// This test uses unique and sort views to obtain from a set of market data events 
    /// the 3 currently most expensive stocks and their symbols. The unique view plays 
    /// the role of filtering only the most recent events and making prior events for a
    /// symbol 'old' data to the sort view, which removes these prior events for a symbol
    /// from the sorted window.
    /// </summary>
    [TestFixture]
    public class TestViewUniqueSorted  {
        private const String SYMBOL_CSCO = "CSCO.O";
        private const String SYMBOL_IBM = "IBM.Count";
        private const String SYMBOL_MSFT = "MSFT.O";
        private const String SYMBOL_C = "C.Count";
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp() {
            _testListener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestExpressionParameter() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.std:unique(Math.Abs(IntPrimitive))");
            SendEvent("E1", 10);
            SendEvent("E2", -10);
            SendEvent("E3", -5);
            SendEvent("E4", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "TheString".Split(','), new Object[][]{new Object[] {"E2"}, new Object[] {"E4"}});
        }
    
        [Test]
        public void TestWindowStats() {
            // Get the top 3 volumes for each symbol
            EPStatement top3Prices = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportMarketDataBean).FullName +
                            ".std:unique(symbol).ext:sort(3, Price desc)");
            top3Prices.Events += _testListener.Update;
    
            _testListener.Reset();
    
            Object[] beans = new Object[10];
    
            beans[0] = MakeEvent(SYMBOL_CSCO, 50);
            _epService.EPRuntime.SendEvent(beans[0]);
    
            Object[] result = ToObjectArray(top3Prices.GetEnumerator());
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{beans[0]}, result);
            Assert.IsTrue(_testListener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) null, _testListener.LastOldData);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{beans[0]}, new Object[]{_testListener.LastNewData[0].Underlying}
            );
            _testListener.Reset();
    
            beans[1] = MakeEvent(SYMBOL_CSCO, 20);
            beans[2] = MakeEvent(SYMBOL_IBM, 50);
            beans[3] = MakeEvent(SYMBOL_MSFT, 40);
            beans[4] = MakeEvent(SYMBOL_C, 100);
            beans[5] = MakeEvent(SYMBOL_IBM, 10);
    
            _epService.EPRuntime.SendEvent(beans[1]);
            _epService.EPRuntime.SendEvent(beans[2]);
            _epService.EPRuntime.SendEvent(beans[3]);
            _epService.EPRuntime.SendEvent(beans[4]);
            _epService.EPRuntime.SendEvent(beans[5]);
    
            result = ToObjectArray(top3Prices.GetEnumerator());
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{beans[4], beans[3], beans[5]}, result);
    
            beans[6] = MakeEvent(SYMBOL_CSCO, 110);
            beans[7] = MakeEvent(SYMBOL_C, 30);
            beans[8] = MakeEvent(SYMBOL_CSCO, 30);
    
            _epService.EPRuntime.SendEvent(beans[6]);
            _epService.EPRuntime.SendEvent(beans[7]);
            _epService.EPRuntime.SendEvent(beans[8]);
    
            result = ToObjectArray(top3Prices.GetEnumerator());
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{beans[3], beans[8], beans[7]}, result);
        }
    
        [Test]
        public void TestSensorPerEvent() {
            String stmtString =
                    "SELECT irstream * " +
                            "FROM\n " +
                            typeof(SupportSensorEvent).FullName + ".std:groupwin(type).win:time(1 hour).std:unique(device).ext:sort(1, measurement desc) as high ";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtString);
            stmt.Events += _testListener.Update;
    
            EPRuntime runtime = _epService.EPRuntime;
    
            SupportSensorEvent eventOne = new SupportSensorEvent(1, "Temperature", "Device1", 5.0, 96.5);
            runtime.SendEvent(eventOne);
            EPAssertionUtil.AssertUnderlyingPerRow(_testListener.AssertInvokedAndReset(), new Object[]{eventOne}, null);
    
            SupportSensorEvent eventTwo = new SupportSensorEvent(2, "Temperature", "Device2", 7.0, 98.5);
            runtime.SendEvent(eventTwo);
            EPAssertionUtil.AssertUnderlyingPerRow(_testListener.AssertInvokedAndReset(), new Object[]{eventTwo}, new Object[]{eventOne});
    
            SupportSensorEvent eventThree = new SupportSensorEvent(3, "Temperature", "Device2", 4.0, 99.5);
            runtime.SendEvent(eventThree);
            EPAssertionUtil.AssertUnderlyingPerRow(_testListener.AssertInvokedAndReset(), new Object[]{eventThree}, new Object[]{eventTwo});
    
            IEnumerator<EventBean> it = stmt.GetEnumerator();
            SupportSensorEvent theEvent = (SupportSensorEvent) it.Advance().Underlying;
            Assert.AreEqual(3, theEvent.Id);
        }
    
        [Test]
        public void TestReuseUnique() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.std:unique(IntBoxed)");
            stmt.Events += _testListener.Update;
    
            SupportBean beanOne = new SupportBean("E1", 1);
            _epService.EPRuntime.SendEvent(beanOne);
            _testListener.Reset();
    
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.std:unique(IntBoxed)");
            SupportUpdateListener testListenerTwo = new SupportUpdateListener();
            stmtTwo.Events += testListenerTwo.Update;
            stmt.Start(); // no effect
    
            SupportBean beanTwo = new SupportBean("E2", 2);
            _epService.EPRuntime.SendEvent(beanTwo);
    
            Assert.AreSame(beanTwo, _testListener.LastNewData[0].Underlying);
            Assert.AreSame(beanOne, _testListener.LastOldData[0].Underlying);
            Assert.AreSame(beanTwo, testListenerTwo.LastNewData[0].Underlying);
            Assert.IsNull(testListenerTwo.LastOldData);
        }
    
        private Object MakeEvent(String symbol, double price) {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            return theEvent;
        }
    
        private void SendEvent(String stringValue, int intPrimitive) {
            _epService.EPRuntime.SendEvent(new SupportBean(stringValue, intPrimitive));
        }
    
        private Object[] ToObjectArray(IEnumerator<EventBean> it) {
            List<Object> result = new List<Object>();
            for (; it.MoveNext();) {
                EventBean theEvent = it.Current;
                result.Add(theEvent.Underlying);
            }
            return result.ToArray();
        }
    }
}
