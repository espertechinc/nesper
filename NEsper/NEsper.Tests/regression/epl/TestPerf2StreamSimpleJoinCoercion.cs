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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf2StreamSimpleJoinCoercion 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _epService.Initialize();
        }
    
        [Test]
        public void TestPerformanceCoercionForward()
        {
            String stmt = "select A.LongBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) as A," +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) as B" +
                " where A.LongBoxed=B.IntPrimitive";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmt);
            statement.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(MakeSupportEvent("A", 0, i));
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                _epService.EPRuntime.SendEvent(MakeSupportEvent("B", index, 0));
                Assert.AreEqual((long)index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            statement.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerformanceCoercionBack()
        {
            String stmt = "select A.IntPrimitive as value from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) as A," +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) as B" +
                " where A.IntPrimitive=B.LongBoxed";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmt);
            statement.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(MakeSupportEvent("A", i, 0));
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                _epService.EPRuntime.SendEvent(MakeSupportEvent("B", 0, index));
                Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            statement.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        private Object MakeSupportEvent(string stringValue, int intPrimitive, long longBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            bean.LongBoxed = longBoxed;
            return bean;
        }
    }
}
