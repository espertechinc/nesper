///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf2StreamAndPropertyJoin 
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
        public void TestPerfRemoveStream()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myStaticEvaluator", typeof(MyStaticEval).FullName, "MyStaticEvaluator");
    
            MyStaticEval.CountCalled = 0;
            MyStaticEval.WaitTimeMSec = 0;
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            String joinStatement = "select * from SupportBean#time(1) as sb, " +
                    " SupportBean_S0#keepall as s0 " +
                    " where myStaticEvaluator(sb.TheString, s0.p00)";
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x"));
            Assert.AreEqual(0, MyStaticEval.CountCalled);
    
            _epService.EPRuntime.SendEvent(new SupportBean("y", 10));
            Assert.AreEqual(1, MyStaticEval.CountCalled);
            Assert.IsTrue(_updateListener.IsInvoked);
    
            // this would be observed as hanging if there was remove-stream evaluation
            MyStaticEval.WaitTimeMSec = 10000000;
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(100000));
        }
    
        [Test]
        public void TestPerf2Properties()
        {
            String methodName = ".testPerformanceJoinNoResults";
    
            String joinStatement = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(1000000)," +
                    typeof(SupportBean).FullName + "#length(1000000)" +
                " where symbol=TheString and volume=LongBoxed";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = Environment.TickCount;
            for (int i = 0; i < 1000; i++)
            {
                SendEvent(MakeMarketEvent("IBM_" + i, 1));
                SendEvent(MakeSupportEvent("CSCO_" + i, 2));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = Environment.TickCount;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay at 250, belwo 500ms
            Assert.IsTrue((endTime - startTime) < 500);
        }
    
        [Test]
        public void TestPerf3Properties()
        {
            String methodName = ".testPerformanceJoinNoResults";
    
            String joinStatement = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "()#length(1000000)," +
                    typeof(SupportBean).FullName + "#length(1000000)" +
                " where symbol=TheString and volume=LongBoxed and DoublePrimitive=price";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = Environment.TickCount;
            for (int i = 0; i < 1000; i++)
            {
                SendEvent(MakeMarketEvent("IBM_" + i, 1));
                SendEvent(MakeSupportEvent("CSCO_" + i, 2));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = Environment.TickCount;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay at 250, belwo 500ms
            Assert.IsTrue((endTime - startTime) < 500);
        }
    
        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private Object MakeSupportEvent(String id, long longBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = id;
            bean.LongBoxed = longBoxed;
            return bean;
        }
    
        private Object MakeMarketEvent(String id, long volume)
        {
            return new SupportMarketDataBean(id, 0, (long) volume, "");
        }
    
        public class MyStaticEval
        {
            static MyStaticEval()
            {
                CountCalled = 0;
            }

            public static int CountCalled { get; set; }

            public static int WaitTimeMSec { get; set; }

            public static bool MyStaticEvaluator(String a, String b) {
                try {
                    Thread.Sleep(WaitTimeMSec);
                    CountCalled++;
                }
                catch (ThreadInterruptedException ex) {
                    return false;
                }
                return true;
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
