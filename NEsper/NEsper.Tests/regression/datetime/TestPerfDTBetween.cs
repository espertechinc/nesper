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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestPerfDTBetween
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestPerf() {
    
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).FullName);
            _epService.EPAdministrator.Configuration.AddEventType("SupportDateTime", typeof(SupportDateTime).FullName);
    
            _epService.EPAdministrator.CreateEPL("create window AWindow.win:keepall() as A");
            _epService.EPAdministrator.CreateEPL("insert into AWindow select * from A");
    
            // preload
            for (int i = 0; i < 10000; i++) {
                _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A" + i, "2002-05-30 9:00:00.000", 100));
            }
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("AEarlier", "2002-05-30 8:00:00.000", 100));
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("ALater", "2002-05-30 10:00:00.000", 100));
    
            String epl = "select a.key as c0 from SupportDateTime unidirectional, AWindow as a where msecdate.Between(msecdateStart, msecdateEnd, false, true)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            // query
            long startTime = Environment.TickCount;
            for (int i = 0; i < 1000; i++) {
                _epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30 8:00:00.050"));
                Assert.AreEqual("AEarlier", _listener.AssertOneGetNewAndReset().Get("c0"));
            }
            long endTime = Environment.TickCount;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 500, "Delta=" + delta / 1000d);
    
            _epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30 10:00:00.050"));
            Assert.AreEqual("ALater", _listener.AssertOneGetNewAndReset().Get("c0"));
    
            stmt.Dispose();
        }
    }
}
