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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfSubselectCorrelatedAggregation 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();        
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestPerformanceCorrelatedAggregation() {
            String stmtText = "select p00, " +
                    "(select sum(IntPrimitive) from SupportBean#keepall where TheString = s0.P00) as sump00 " +
                    "from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            String[] fields = "p00,sump00".Split(',');
    
            // preload
            int max = 50000;
            for (int i = 0; i < max; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("T" + i, -i));
                _epService.EPRuntime.SendEvent(new SupportBean("T" + i, 10));
            }
    
            // excercise
            long start = PerformanceObserver.MilliTime;
            Random random = new Random();
            for (int i = 0; i < 10000; i++) {
                int index = random.Next(max);
                _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "T" + index));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T" + index, -index + 10});
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
    
            //Console.Out.WriteLine("delta=" + delta);
            Assert.IsTrue(delta < 500, "Failed perf test, delta=" + delta);
        }
    }
}
