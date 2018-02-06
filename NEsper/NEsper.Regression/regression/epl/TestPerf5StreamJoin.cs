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
using com.espertech.esper.compat.logging;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf5StreamJoin 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _updateListener = null;
        }
    
        [Test]
        public void TestPerfAllProps()
        {
            String statement = "select * from " +
                typeof(SupportBean_S0).FullName + "#length(100000) as s0," +
                typeof(SupportBean_S1).FullName + "#length(100000) as s1," +
                typeof(SupportBean_S2).FullName + "#length(100000) as s2," +
                typeof(SupportBean_S3).FullName + "#length(100000) as s3," +
                typeof(SupportBean_S4).FullName + "#length(100000) as s4" +
                " where s0.P00 = s1.p10 " +
                   "and s1.p10 = s2.p20 " +
                   "and s2.p20 = s3.p30 " +
                   "and s3.p30 = s4.p40 ";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(statement);
            joinView.Events += _updateListener.Update;
    
            log.Info(".testPerfAllProps Preloading events");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                SendEvents(new[] {0,0,0,0,0}, new[] {"s0"+i, "s1"+i, "s2"+i, "s3"+i, "s4"+i});
            }
    
            long endTime = PerformanceObserver.MilliTime;
            log.Info(".testPerfAllProps delta=" + (endTime - startTime));
            Assert.IsTrue((endTime - startTime) < 1500);
    
            // test if join returns data
            Assert.IsNull(_updateListener.LastNewData);
            var propertyValues = new[] {"x", "x", "x", "x", "x"};
            var ids = new[] { 1, 2, 3, 4, 5 };
            SendEvents(ids, propertyValues);
            AssertEventsReceived(ids);
        }
    
        private void AssertEventsReceived(int[] expectedIds)
        {
            Assert.AreEqual(1, _updateListener.LastNewData.Length);
            Assert.IsNull(_updateListener.LastOldData);
            EventBean theEvent = _updateListener.LastNewData[0];
            Assert.AreEqual(expectedIds[0], ((SupportBean_S0) theEvent.Get("s0")).Id);
            Assert.AreEqual(expectedIds[1], ((SupportBean_S1) theEvent.Get("s1")).Id);
            Assert.AreEqual(expectedIds[2], ((SupportBean_S2) theEvent.Get("s2")).Id);
            Assert.AreEqual(expectedIds[3], ((SupportBean_S3) theEvent.Get("s3")).Id);
            Assert.AreEqual(expectedIds[4], ((SupportBean_S4) theEvent.Get("s4")).Id);
        }
    
        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvents(int[] ids, String[] propertyValues)
        {
            SendEvent(new SupportBean_S0(ids[0], propertyValues[0]));
            SendEvent(new SupportBean_S1(ids[1], propertyValues[1]));
            SendEvent(new SupportBean_S2(ids[2], propertyValues[2]));
            SendEvent(new SupportBean_S3(ids[3], propertyValues[3]));
            SendEvent(new SupportBean_S4(ids[4], propertyValues[4]));
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
