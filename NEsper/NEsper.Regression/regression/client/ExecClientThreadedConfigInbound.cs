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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientThreadedConfigInbound : RegressionExecution {
        public override void Configure(Configuration configuration) {
            SupportExceptionHandlerFactory.FactoryContexts.Clear();
            SupportExceptionHandlerFactory.Handlers.Clear();
            configuration.EngineDefaults.ExceptionHandling.HandlerFactories.Clear();
            configuration.EngineDefaults.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactory));
    
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = false;
            configuration.EngineDefaults.Threading.IsThreadPoolInbound = true;
            configuration.EngineDefaults.Threading.ThreadPoolInboundNumThreads = 4;
            configuration.EngineDefaults.Expression.IsUdfCache = false;
            configuration.AddEventType("MyMap", new Dictionary<string, object>());
            configuration.AddEventType<SupportBean>();
            configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
    
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            configuration.AddEventType("XMLType", xmlDOMEventTypeDesc);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionEventsProcessed(epService);
            RunAssertionExceptionHandler(epService);
        }
    
        private void RunAssertionExceptionHandler(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "throwException", GetType(), "ThrowException", ValueCacheEnum.DISABLED, FilterOptimizableEnum.ENABLED, true);
            string epl = "@Name('ABCName') select * from SupportBean(ThrowException())";
            epService.EPAdministrator.CreateEPL(epl);
    
            SupportExceptionHandlerFactory.SupportExceptionHandler handler = SupportExceptionHandlerFactory.Handlers[SupportExceptionHandlerFactory.Handlers.Count - 1];
            epService.EPRuntime.SendEvent(new SupportBean());
    
            int count = 0;
            while (true) {
                if (handler.InboundPoolContexts.Count == 1) {
                    break;
                }
                if (count++ < 100) {
                    Thread.Sleep(100);
                }
            }
    
            if (count >= 100) {
                Assert.Fail();
            }
        }
    
        private void RunAssertionEventsProcessed(EPServiceProvider epService) {
    
            var listenerOne = new SupportListenerTimerHRes();
            var listenerTwo = new SupportListenerTimerHRes();
            var listenerThree = new SupportListenerTimerHRes();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(100) from MyMap");
            stmtOne.Events += listenerOne.Update;
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(100) from SupportBean");
            stmtTwo.Events += listenerTwo.Update;
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL("select SupportStaticMethodLib.Sleep(100) from XMLType");
            stmtThree.Events += listenerThree.Update;
    
            EventSender senderOne = epService.EPRuntime.GetEventSender("MyMap");
            EventSender senderTwo = epService.EPRuntime.GetEventSender("SupportBean");
            EventSender senderThree = epService.EPRuntime.GetEventSender("XMLType");
    
            long start = PerformanceObserver.NanoTime;
            for (int i = 0; i < 2; i++) {
                epService.EPRuntime.SendEvent(new Dictionary<string, Object>(), "MyMap");
                senderOne.SendEvent(new Dictionary<string, Object>());
                epService.EPRuntime.SendEvent(new SupportBean());
                senderTwo.SendEvent(new SupportBean());
                epService.EPRuntime.SendEvent(SupportXML.GetDocument("<myevent/>"));
                senderThree.SendEvent(SupportXML.GetDocument("<myevent/>"));
            }
            long end = PerformanceObserver.NanoTime;
            long delta = (end - start) / 1000000;
            Assert.IsTrue(delta < 500);
    
            Thread.Sleep(1000);
            Assert.AreEqual(4, listenerOne.NewEvents.Count);
            Assert.AreEqual(4, listenerTwo.NewEvents.Count);
            Assert.AreEqual(4, listenerThree.NewEvents.Count);
    
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.AreEqual(0, spi.ThreadingService.InboundQueue.Count);
            Assert.IsNotNull(spi.ThreadingService.InboundThreadPool);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
            stmtThree.Dispose();
        }
    
        // Used by test
        public static bool ThrowException() {
            throw new EPRuntimeException("Intended for testing");
        }
    }
} // end of namespace
