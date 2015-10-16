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
    public class TestViewLengthBatch  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportBean[] _events;
    
        [SetUp]
        public void SetUp() {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _events = new SupportBean[100];
            for (int i = 0; i < _events.Length; i++) {
                _events[i] = new SupportBean();
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestLengthBatchSize2() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + ".win:length_batch(2)");
            stmt.Events += _listener.Update;
    
            SendEvent(_events[0]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[0]}, stmt.GetEnumerator());
    
            SendEvent(_events[1]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[0], _events[1]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[2]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[2]}, stmt.GetEnumerator());
    
            SendEvent(_events[3]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[2], _events[3]}, new SupportBean[]{_events[0], _events[1]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[4]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[4]}, stmt.GetEnumerator());
    
            SendEvent(_events[5]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[4], _events[5]}, new SupportBean[]{_events[2], _events[3]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
        }
    
        [Test]
        public void TestLengthBatchSize1() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + ".win:length_batch(1)");
            stmt.Events += _listener.Update;
    
            SendEvent(_events[0]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[0]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[1]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[1]}, new SupportBean[]{_events[0]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[2]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[2]}, new SupportBean[]{_events[1]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
        }
    
        [Test]
        public void TestLengthBatchSize3() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + ".win:length_batch(3)");
            stmt.Events += _listener.Update;
    
            SendEvent(_events[0]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[0]}, stmt.GetEnumerator());
    
            SendEvent(_events[1]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[0], _events[1]}, stmt.GetEnumerator());
    
            SendEvent(_events[2]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[0], _events[1], _events[2]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[3]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[3]}, stmt.GetEnumerator());
    
            SendEvent(_events[4]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new SupportBean[]{_events[3], _events[4]}, stmt.GetEnumerator());
    
            SendEvent(_events[5]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[3], _events[4], _events[5]}, new SupportBean[]{_events[0], _events[1], _events[2]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
        }
    
        [Test]
        public void TestLengthBatchSize3And2Staggered() {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName + ".win:length_batch(3).win:length_batch(2)");
            stmt.Events += _listener.Update;
    
            SendEvent(_events[0]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[1]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[2]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[0], _events[1], _events[2]}, null);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[3]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[4]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
    
            SendEvent(_events[5]);
            EPAssertionUtil.AssertUnderlyingPerRow(_listener.AssertInvokedAndReset(), new SupportBean[]{_events[3], _events[4], _events[5]}, new SupportBean[]{_events[0], _events[1], _events[2]});
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(null, stmt.GetEnumerator());
        }
    
        [Test]
        public void TestInvalid() {
            try {
                _epService.EPAdministrator.CreateEPL(
                        "select * from " + typeof(SupportMarketDataBean).FullName + ".win:length_batch(0)");
                Assert.Fail();
            } catch (Exception ex) {
                // expected
            }
        }
    
        private void SendEvent(SupportBean theEvent) {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
