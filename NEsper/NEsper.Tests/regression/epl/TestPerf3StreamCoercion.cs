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
using com.espertech.esper.compat.logging;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf3StreamCoercion 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _listener = new SupportUpdateListener();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestPerfCoercion3waySceneOne()
        {
            String stmtText = "select s1.IntBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) s1," +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) s2," +
                    typeof(SupportBean).FullName + "(TheString='C').win:length(1000000) s3" +
                " where s1.IntBoxed=s2.LongBoxed and s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                SendEvent("B", 0, i, 0);
                SendEvent("C", 0, 0, i);
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                SendEvent("A", index, 0, 0);
                Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;

            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            stmt.Dispose();
        }
    
        [Test]
        public void TestPerfCoercion3waySceneTwo()
        {
            String stmtText = "select s1.IntBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) s1," +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) s2," +
                    typeof(SupportBean).FullName + "(TheString='C').win:length(1000000) s3" +
                " where s1.IntBoxed=s2.LongBoxed and s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                SendEvent("A", i, 0, 0);
                SendEvent("B", 0, i, 0);
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                SendEvent("C", 0, 0, index);
                Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            stmt.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerfCoercion3waySceneThree()
        {
            String stmtText = "select s1.IntBoxed as value from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) s1," +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) s2," +
                    typeof(SupportBean).FullName + "(TheString='C').win:length(1000000) s3" +
                " where s1.IntBoxed=s2.LongBoxed and s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                SendEvent("A", i, 0, 0);
                SendEvent("C", 0, 0, i);
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                SendEvent("B", 0, index, 0);
                Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            stmt.Dispose();
            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        private void SendEvent(string stringValue, int intBoxed, long longBoxed, double doubleBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
