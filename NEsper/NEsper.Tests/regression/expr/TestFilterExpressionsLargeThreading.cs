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

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestFilterExpressionsLargeThreading
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            _listener = new SupportUpdateListener();
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportEvent", typeof(SupportTradeEvent));
            config.EngineDefaults.ExecutionConfig.ThreadingProfile = ConfigurationEngineDefaults.ThreadingProfile.LARGE;
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestNullBooleanExpr() {
            String stmtOneText = "every event1=SupportEvent(userId like '123%')";
            EPStatement statement = _epService.EPAdministrator.CreatePattern(stmtOneText);
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportTradeEvent(1, null, 1001));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportTradeEvent(2, "1234", 1001));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("event1.id"));
        }
    }
}
