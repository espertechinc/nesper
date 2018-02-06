///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumNestedPerformance
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddImport(typeof(LocationReportFactory));
            config.AddEventType("Bean", typeof(SupportBean_ST0_Container));
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
        public void TestPerfNestedUncorrelated() {
    
            List<SupportBean_ST0> list = new List<SupportBean_ST0>();
            for (int i = 0; i < 10000; i++) {
                list.Add(new SupportBean_ST0("E1", 1000));
            }
            SupportBean_ST0 minEvent = new SupportBean_ST0("E2", 5);
            list.Add(minEvent);
            SupportBean_ST0_Container theEvent = new SupportBean_ST0_Container(list);
    
            // the "contained.min" inner lambda only depends on values within "contained" (a stream's value)
            // and not on the particular "x".
            String eplFragment = "select contained.Where(x => x.P00 = contained.min(y => y.P00)) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
    
            long start = Environment.TickCount;
            _epService.EPRuntime.SendEvent(theEvent);
            long delta = Environment.TickCount - start;
            Assert.IsTrue(delta < 100, "delta=" + delta);

            ICollection<object> result = (ICollection<object>)_listener.AssertOneGetNewAndReset().Get("val");
            EPAssertionUtil.AssertEqualsExactOrder(new Object[]{minEvent}, result.ToArray());
        }
    }
}
