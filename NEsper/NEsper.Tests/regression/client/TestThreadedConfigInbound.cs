///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.regression.events;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestThreadedConfigInbound 
    {
        [Test]
        public void TestFastShutdown()
        {
            Configuration config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsThreadPoolInbound = true;
            config.EngineDefaults.ThreadingConfig.ThreadPoolInboundNumThreads = 2;
            config.AddEventType<MyEvent>();
            config.AddPlugInSingleRowFunction("sleepaLittle", GetType().FullName, "SleepaLittle");
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();

            EPStatement stmt = epService.EPAdministrator.CreateEPL("select sleepaLittle(100) from MyEvent");
            stmt.Subscriber = new MySubscriber();
            for (int i = 0; i < 10000; i++)
            {
                epService.EPRuntime.SendEvent(new MyEvent());
            }
            epService.Dispose();
        }

        [Test]
        public void TestOp()
        {
            Configuration config = new Configuration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            config.EngineDefaults.ThreadingConfig.IsThreadPoolInbound = true;
            config.EngineDefaults.ThreadingConfig.ThreadPoolInboundNumThreads = 4;
            config.EngineDefaults.ExpressionConfig.IsUdfCache = false;
            config.AddEventType("MyMap", new Dictionary<String, Object>());
            config.AddEventType<SupportBean>();
            config.AddImport(typeof(SupportStaticMethodLib).FullName);
    
            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            config.AddEventType("XMLType", xmlDOMEventTypeDesc);
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            SupportListenerTimerHRes listenerOne = new SupportListenerTimerHRes();
            SupportListenerTimerHRes listenerTwo = new SupportListenerTimerHRes();
            SupportListenerTimerHRes listenerThree = new SupportListenerTimerHRes();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(100) from MyMap");
            stmtOne.Events += listenerOne.Update;
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(100) from SupportBean");
            stmtTwo.Events += listenerTwo.Update;
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(100) from XMLType");
            stmtThree.Events += listenerThree.Update;
    
            EventSender senderOne = epService.EPRuntime.GetEventSender("MyMap");
            EventSender senderTwo = epService.EPRuntime.GetEventSender("SupportBean");
            EventSender senderThree = epService.EPRuntime.GetEventSender("XMLType");

            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 2; i++)
                    {
                        epService.EPRuntime.SendEvent(new Dictionary<String, Object>(), "MyMap");
                        senderOne.SendEvent(new Dictionary<String, Object>());
                        epService.EPRuntime.SendEvent(new SupportBean());
                        senderTwo.SendEvent(new SupportBean());
                        epService.EPRuntime.SendEvent(SupportXML.GetDocument("<myevent/>"));
                        senderThree.SendEvent(SupportXML.GetDocument("<myevent/>"));
                    }
                });

            Assert.LessOrEqual(delta, 100);
    
            Thread.Sleep(1000);
            Assert.AreEqual(4, listenerOne.NewEvents.Count);
            Assert.AreEqual(4, listenerTwo.NewEvents.Count);
            Assert.AreEqual(4, listenerThree.NewEvents.Count);
    
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.AreEqual(0, spi.ThreadingService.InboundQueue.Count);
            Assert.NotNull(spi.ThreadingService.InboundThreadPool);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
            stmtThree.Dispose();
    
            epService.Dispose();
        }

        public static void SleepaLittle(long time)
        {
            try
            {
                Thread.Sleep((int) time);
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        public class MySubscriber
        {
            public void Update(Object[] args)
            {
            }
        }

        public class MyEvent
        {
        }
    }
}
