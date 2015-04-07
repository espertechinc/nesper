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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeBatch 
    {
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

        [Test]
        public void TestMonthScoped()
        {
            var epAdministrator = _epService.EPAdministrator;
            epAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime("2002-02-01T9:00:00.000");
            epAdministrator.CreateEPL("select * from SupportBean.win:time_batch(1 month)").Events += _listener.Update;

            var epRuntime = _epService.EPRuntime;
            epRuntime.SendEvent(new SupportBean("E1", 1));
            SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendCurrentTime("2002-03-01T9:00:00.000");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[] {"E1"});

            epRuntime.SendEvent(new SupportBean("E2", 1));
            SendCurrentTimeWithMinus("2002-04-01T9:00:00.000", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendCurrentTime("2002-04-01T9:00:00.000");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[] {"E2"});

            epRuntime.SendEvent(new SupportBean("E3", 1));
            SendCurrentTime("2002-05-01T9:00:00.000");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[] {"E3"});
        }
    
        [Test]
        public void TestStartEagerForceUpdate()
        {
            SendTimer(1000);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:time_batch(1, \"START_EAGER,FORCE_UPDATE\")");
            stmt.Events += _listener.Update;
    
            SendTimer(1999);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
            
            SendTimer(2000);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendTimer(2999);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendTimer(3000);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendTimer(4000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[] { "E1" });
    
            SendTimer(5000);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), "TheString".Split(','), new Object[] { "E1" });
    
            SendTimer(5999);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendTimer(6000);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendTimer(7000);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
        }
    
        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendCurrentTime(String time)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time)));
        }

        private void SendCurrentTimeWithMinus(String time, long minus)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time) - minus));
        }
    }
}
