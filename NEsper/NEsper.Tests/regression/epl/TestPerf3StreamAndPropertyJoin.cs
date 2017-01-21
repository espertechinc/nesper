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
    public class TestPerf3StreamAndPropertyJoin 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        [SetUp]
        public void SetUp()
        {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            updateListener = null;
        }
    
        [Test]
        public void TestPerfAllProps()
        {
            // Statement where all streams are reachable from each other via properties
            String stmt = "select * from " +
                    typeof(SupportBean_A).FullName + "().win:length(1000000) s1," +
                    typeof(SupportBean_B).FullName + "().win:length(1000000) s2," +
                    typeof(SupportBean_C).FullName + "().win:length(1000000) s3" +
                " where s1.id=s2.id and s2.id=s3.id and s1.id=s3.id";
            TryJoinPerf3Streams(stmt);
        }
    
        [Test]
        public void TestPerfPartialProps()
        {
            // Statement where the s1 stream is not reachable by joining s2 to s3 and s3 to s1
            String stmt = "select * from " +
                    typeof(SupportBean_A).FullName + ".win:length(1000000) s1," +
                    typeof(SupportBean_B).FullName + ".win:length(1000000) s2," +
                    typeof(SupportBean_C).FullName + ".win:length(1000000) s3" +
                " where s1.id=s2.id and s2.id=s3.id";   // ==> therefore s1.id = s3.id
            TryJoinPerf3Streams(stmt);
        }
    
        [Test]
        public void TestPerfPartialStreams()
        {
            String methodName = ".testPerfPartialStreams";
    
            // Statement where the s1 stream is not reachable by joining s2 to s3 and s3 to s1
            String stmt = "select * from " +
                    typeof(SupportBean_A).FullName + "().win:length(1000000) s1," +
                    typeof(SupportBean_B).FullName + "().win:length(1000000) s2," +
                    typeof(SupportBean_C).FullName + "().win:length(1000000) s3" +
                " where s1.id=s2.id";   // ==> stream s3 no properties supplied, full s3 scan
            
            EPStatement joinView = epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += updateListener.Update;
    
            // preload s3 with just 1 event
            SendEvent(new SupportBean_C("GE_0"));
    
            // Send events for each stream
            log.Info(methodName + " Preloading events");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                SendEvent(new SupportBean_A("CSCO_" + i));
                SendEvent(new SupportBean_B("IBM_" + i));
            }
            log.Info(methodName + " Done preloading");
    
            long endTime = PerformanceObserver.MilliTime;
            log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay below 500, no index would be 4 sec plus
            Assert.IsTrue((endTime - startTime) < 500);
        }
    
        private void TryJoinPerf3Streams(String joinStatement)
        {
            String methodName = ".tryJoinPerf3Streams";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            // Send events for each stream
            log.Info(methodName + " Preloading events");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 100; i++)
            {
                SendEvent(new SupportBean_A("CSCO_" + i));
                SendEvent(new SupportBean_B("IBM_" + i));
                SendEvent(new SupportBean_C("GE_" + i));
            }
            log.Info(methodName + " Done preloading");
    
            long endTime = PerformanceObserver.MilliTime;
            log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay below 500, no index would be 4 sec plus
            Assert.IsTrue((endTime - startTime) < 500);
        }
    
        private void SendEvent(Object theEvent)
        {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
