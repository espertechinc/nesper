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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAggregateExtRate  {
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        // rate implementation does not require a data window (may have one)
        // advantage: not retaining events, only timestamp data points
        // disadvantage: output rate limiting without snapshot may be less accurate rate
        [Test]
        public void TestRateDataNonWindowed()
        {
            SendTimer(0);
    
            const string epl = "select rate(10) as myrate from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertion();
    
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            Assert.AreEqual(epl, model.ToEPL());
            
            RunAssertion();
    
            TryInvalid("select rate() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'rate(*)': The rate aggregation function minimally requires a numeric constant or expression as a parameter. [select rate() from SupportBean]");
            TryInvalid("select rate(true) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'rate(true)': The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation [select rate(true) from SupportBean]");
        }
    
        [Test]
        public void TestRateDataWindowed()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
    
            var fields = "myrate,myqtyrate".Split(',');
            var viewExpr = "select RATE(LongPrimitive) as myrate, RATE(LongPrimitive, IntPrimitive) as myqtyrate from SupportBean.win:length(3)";
            var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;
    
            SendEvent(1000, 10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
    
            SendEvent(1200, 0);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
    
            SendEvent(1300, 0);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
    
            SendEvent(1500, 14);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3*1000/500d, 14*1000/500d});
    
            SendEvent(2000, 11);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3*1000/800d, 25*1000/800d});
    
            TryInvalid("select rate(LongPrimitive) as myrate from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'rate(LongPrimitive)': The rate aggregation function in the timestamp-property notation requires data windows [select rate(LongPrimitive) as myrate from SupportBean]");
            TryInvalid("select rate(current_timestamp) as myrate from SupportBean.win:time(20)",
                    "Error starting statement: Failed to validate select-clause expression 'rate(current_timestamp())': The rate aggregation function does not allow the current engine timestamp as a parameter [select rate(current_timestamp) as myrate from SupportBean.win:time(20)]");
            TryInvalid("select rate(TheString) as myrate from SupportBean.win:time(20)",
                    "Error starting statement: Failed to validate select-clause expression 'rate(TheString)': The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation [select rate(TheString) as myrate from SupportBean.win:time(20)]");
        }
    
        private void RunAssertion() {
            String[] fields = "myrate".Split(',');
    
            SendTimer(1000); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(1200); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(1600); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(1600); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(9000); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(9200); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(10999); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            SendTimer(11100); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {0.7});
    
            SendTimer(11101); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {0.8});
    
            SendTimer(11200); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {0.8});
    
            SendTimer(11600); SendEvent();
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {0.7});
        }
    
        private void TryInvalid(String epl, String message) {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent(long longPrimitive, int intPrimitive)
        {
            var bean = new SupportBean {LongPrimitive = longPrimitive, IntPrimitive = intPrimitive};
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent()
        {
            var bean = new SupportBean();
            _epService.EPRuntime.SendEvent(bean);
        }
    
        public class RateSendRunnable
        {
            private readonly EPRuntime _runtime;
    
            public RateSendRunnable(EPRuntime runtime) {
                _runtime = runtime;
            }
    
            public void Run()
            {
                var bean = new SupportBean {LongPrimitive = PerformanceObserver.MilliTime};
                _runtime.SendEvent(bean);
            }
        }
    }
}
