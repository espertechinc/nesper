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
    public class TestPerfSubselectFiltered 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
            config.AddEventType("S3", typeof(SupportBean_S3));
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
        public void TestPerformanceOneCriteria()
        {
            String stmtText = "select (select P10 from S1#length(100000) where Id = s0.Id) as value from S0 as s0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(i)));
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i++)
            {
                int index = 5000 + i % 1000;
                _epService.EPRuntime.SendEvent(new SupportBean_S0(index, Convert.ToString(index)));
                Assert.AreEqual(Convert.ToString(index), _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerformanceTwoCriteria()
        {
            String stmtText = "select (select P10 from S1#length(100000) where s0.Id = Id and P10 = s0.P00) as value from S0 as s0";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(i)));
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i++)
            {
                int index = 5000 + i % 1000;
                _epService.EPRuntime.SendEvent(new SupportBean_S0(index, Convert.ToString(index)));
                Assert.AreEqual(Convert.ToString(index), _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;

            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerformanceJoin3CriteriaSceneOne()
        {
            String stmtText = "select (select P00 from S0#length(100000) where P00 = s1.P10 and P01 = s2.P20 and P02 = s3.P30) as value " +
                    "from S1#length(100000) as s1, S2#length(100000) as s2, S3#length(100000) as s3 where s1.Id = s2.Id and s2.Id = s3.Id";
            TryPerfJoin3Criteria(stmtText);
        }
    
        [Test]
        public void TestPerformanceJoin3CriteriaSceneTwo()
        {
            String stmtText = "select (select P00 from S0#length(100000) where P01 = s2.P20 and P00 = s1.P10 and P02 = s3.P30 and Id >= 0) as value " +
                    "from S3#length(100000) as s3, S1#length(100000) as s1, S2#length(100000) as s2 where s2.Id = s3.Id and s1.Id = s2.Id";
            TryPerfJoin3Criteria(stmtText);
        }
    
        private void TryPerfJoin3Criteria(String stmtText)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(i, Convert.ToString(i), Convert.ToString(i + 1), Convert.ToString(i + 2)));
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = i;
                _epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(index)));
                _epService.EPRuntime.SendEvent(new SupportBean_S2(i, Convert.ToString(index + 1)));
                _epService.EPRuntime.SendEvent(new SupportBean_S3(i, Convert.ToString(index + 2)));
                Assert.AreEqual(Convert.ToString(index), _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    }
}
