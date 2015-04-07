///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestVariablesTimer 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerSet;
    
        [SetUp]
        public void SetUp()
        {
            var config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = true;
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listenerSet = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listenerSet = null;
        }
    
        [Test]
        public void TestTimestamp()
        {
            _epService.EPAdministrator.Configuration.AddVariable("var1", typeof(long), "12");
            _epService.EPAdministrator.Configuration.AddVariable("var2", typeof(long), "2");
            _epService.EPAdministrator.Configuration.AddVariable("var3", typeof(long), null);
    
            long startTime = PerformanceObserver.MilliTime;        
            const string stmtTextSet = "on pattern [every timer:interval(100 milliseconds)] set var1 = current_timestamp, var2 = var1 + 1, var3 = var1 + var2";
            EPStatement stmtSet = _epService.EPAdministrator.CreateEPL(stmtTextSet);
            stmtSet.Events += _listenerSet.Update;
    
            Thread.Sleep(1000);
            stmtSet.Dispose();
    
            EventBean[] received = _listenerSet.GetNewDataListFlattened();
            Assert.IsTrue(received.Length >= 5, "received : " + received.Length);
    
            for (int i = 0; i < received.Length; i++)
            {
                long var1 = received[i].Get("var1").AsLong();
                long var2 = received[i].Get("var2").AsLong(); ;
                long var3 = received[i].Get("var3").AsLong(); ;
                Assert.IsTrue(var1 >= startTime);
                Assert.AreEqual(var1, var2 - 1);
                Assert.AreEqual(var3, var2 + var1);
            }
        }
    }
}
