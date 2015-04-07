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

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTDocSamples
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
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
        public void TestInput()
        {
            IDictionary<String, Object> meta = new Dictionary<String, Object>();
            meta.Put("timeTaken", typeof(DateTime));
            _epService.EPAdministrator.Configuration.AddEventType("RFIDEvent", meta);
    
            _epService.EPAdministrator.CreateEPL("select timeTaken.Format() as timeTakenStr from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.Get('month') as timeTakenMonth from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.GetMonthOfYear() as timeTakenMonth from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.Minus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.Minus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.Plus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.Plus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.RoundCeiling('min') as timeTakenRounded from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.RoundFloor('min') as timeTakenRounded from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.Set('month', 3) as timeTakenMonth from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.WithDate(2002, 4, 30) as timeTakenDated from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.WithMax('sec') as timeTakenMaxSec from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.ToCalendar() as timeTakenCal from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.ToDate() as timeTakenDate from RFIDEvent");
            _epService.EPAdministrator.CreateEPL("select timeTaken.ToMillisec() as timeTakenLong from RFIDEvent");
    
            // test pattern use
            ConfigurationEventTypeLegacy leg = new ConfigurationEventTypeLegacy();
            leg.StartTimestampPropertyName = "MsecdateStart";
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).FullName, leg);
            _epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB).FullName, leg);

            TryRun("a.MsecdateStart.After(b)", "2002-05-30 09:00:00.000", "2002-05-30 08:59:59.999", true);
            TryRun("a.After(b.MsecdateStart)", "2002-05-30 09:00:00.000", "2002-05-30 08:59:59.999", true);
            TryRun("a.After(b)", "2002-05-30 09:00:00.000", "2002-05-30 08:59:59.999", true);
            TryRun("a.After(b)", "2002-05-30 08:59:59.999", "2002-05-30 09:00:00.000", false);
        }
    
        private void TryRun(String condition, String tsa, String tsb, bool isInvoked) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from pattern [a=A -> b=B] as abc where " + condition);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", tsa, 0));
            _epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("E2", tsb, 0));
            Assert.AreEqual(isInvoked, _listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        public class MyEvent
        {
            public String Get()
            {
                return "abc";
            }
        }
    }
}
