///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumNamedWindowPerformance
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestNamedWindowQualified()
        {
            _epService.EPAdministrator.CreateEPL("create window Win.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into Win select * from SupportBean");
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("K" + i % 100, i));
            }
            
            RunAssertiomReuse();
    
            RunAssertiomSubquery();
        }
    
        private void RunAssertiomSubquery()
        {
            // test expression reuse
            String epl =    "expression q {" +
                            "  x => (select * from Win where IntPrimitive = x.P00)" +
                            "}" +
                            "select " +
                            "q(st0).Where(x => TheString = key0) as val0, " +
                            "q(st0).Where(x => TheString = key0) as val1, " +
                            "q(st0).Where(x => TheString = key0) as val2, " +
                            "q(st0).Where(x => TheString = key0) as val3, " +
                            "q(st0).Where(x => TheString = key0) as val4, " +
                            "q(st0).Where(x => TheString = key0) as val5, " +
                            "q(st0).Where(x => TheString = key0) as val6, " +
                            "q(st0).Where(x => TheString = key0) as val7, " +
                            "q(st0).Where(x => TheString = key0) as val8, " +
                            "q(st0).Where(x => TheString = key0) as val9 " +
                            "from SupportBean_ST0 st0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID", "K50", 1050));
                EventBean theEvent = _listener.AssertOneGetNewAndReset();
                for (int j = 0; j < 10; j++) {
                    var coll = (ICollection<object>) theEvent.Get("val" + j);
                    Assert.AreEqual(1, coll.Count);
                    var bean = (SupportBean) coll.First();
                    Assert.AreEqual("K50", bean.TheString);
                    Assert.AreEqual(1050, bean.IntPrimitive);
                }
            }
            long delta = PerformanceObserver.MilliTime - start;
            Assert.That(delta, Is.LessThan(1000), "Delta = " + delta);
    
            stmt.Dispose();
        }
    
        private void RunAssertiomReuse()
        {
            // test expression reuse
            String epl =    "expression q {" +
                            "  x => Win(TheString = x.key0).Where(y => IntPrimitive = x.P00)" +
                            "}" +
                            "select " +
                            "q(st0) as val0, " +
                            "q(st0) as val1, " +
                            "q(st0) as val2, " +
                            "q(st0) as val3, " +
                            "q(st0) as val4, " +
                            "q(st0) as val5, " +
                            "q(st0) as val6, " +
                            "q(st0) as val7, " +
                            "q(st0) as val8, " +
                            "q(st0) as val9 " +
                    "from SupportBean_ST0 st0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID", "K50", 1050));
                EventBean theEvent = _listener.AssertOneGetNewAndReset();
                for (int j = 0; j < 10; j++)
                {
                    var coll = (ICollection<object>) theEvent.Get("val" + j);
                    Assert.AreEqual(1, coll.Count);
                    var bean = (SupportBean) coll.First();
                    Assert.AreEqual("K50", bean.TheString);
                    Assert.AreEqual(1050, bean.IntPrimitive);
                }
            }
            long delta = PerformanceObserver.MilliTime - start;
            Assert.That(delta, Is.LessThan(1000), "Delta = " + delta);
    
            // This will create a single dispatch
            // epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            stmt.Dispose();
        }
    }
}
