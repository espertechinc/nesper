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
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestSubscriberPerf  {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            String pkg = typeof(SupportBean).Namespace;
            config.AddEventTypeAutoName(pkg);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        [Test]
        public void TestPerformanceSyntheticUndelivered() {
            int NUM_LOOP = 100000;
            _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)");
    
            long start = Environment.TickCount;
            for (int i = 0; i < NUM_LOOP; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1000 + i));
            }
            long end = Environment.TickCount;
            // Console.WriteLine("delta=" + (end - start));
        }
    
        [Test]
        public void TestPerformanceSynthetic() {
            const int NUM_LOOP = 100000;
            var stmt = _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)");
            var results = new List<Object[]>();
    
            stmt.Events += (sender, args) =>
            {
                var stringValue = (String)args.NewEvents[0].Get("TheString");
                var val = args.NewEvents[0].Get("IntPrimitive").AsInt();
                results.Add(new Object[] { stringValue, val });
            };
    
            long start = Environment.TickCount;
            for (int i = 0; i < NUM_LOOP; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 1000 + i));
            }
            long end = Environment.TickCount;
    
            Assert.AreEqual(NUM_LOOP, results.Count);
            for (int i = 0; i < NUM_LOOP; i++) {
                EPAssertionUtil.AssertEqualsAnyOrder(results[i], new Object[]{"E1", 1000 + i});
            }
            // Console.WriteLine("delta=" + (end - start));
        }
    }
}
