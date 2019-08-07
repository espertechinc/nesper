///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeThreadedConfigInbound : RegressionExecutionWithConfigure
    {
        public bool EnableHATest => true;
        public bool HAWithCOnly => false;

        public void Configure(Configuration configuration)
        {
            SupportExceptionHandlerFactory.FactoryContexts.Clear();
            SupportExceptionHandlerFactory.Handlers.Clear();
            configuration.Runtime.ExceptionHandling.HandlerFactories.Clear();
            configuration.Runtime.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactory));

            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Runtime.Threading.IsThreadPoolInbound = true;
            configuration.Runtime.Threading.ThreadPoolInboundNumThreads = 4;
            configuration.Compiler.Expression.UdfCache = false;
            configuration.Common.AddEventType("MyMap", new Dictionary<string, object>());
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            var xmlDOMEventTypeDesc = new ConfigurationCommonEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            configuration.Common.AddEventType("XMLType", xmlDOMEventTypeDesc);

            configuration.Compiler.AddPlugInSingleRowFunction(
                "throwException",
                GetType(),
                "ThrowException",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED,
                true);
        }

        public void Run(RegressionEnvironment env)
        {
            RunAssertionEventsProcessed(env);
            RunAssertionExceptionHandler(env);
        }

        private void RunAssertionExceptionHandler(RegressionEnvironment env)
        {
            var epl = "@Name('ABCName') select * from SupportBean(throwException())";
            env.CompileDeploy(epl);

            var handler = SupportExceptionHandlerFactory
                .Handlers[SupportExceptionHandlerFactory.Handlers.Count - 1];
            env.SendEventBean(new SupportBean());

            var count = 0;
            while (true) {
                if (handler.InboundPoolContexts.Count == 1) {
                    break;
                }

                if (count++ < 100) {
                    try {
                        Thread.Sleep(100);
                    }
                    catch (ThreadInterruptedException e) {
                        throw new EPException(e);
                    }
                }

                if (count >= 100) {
                    Assert.Fail();
                }
            }

            env.UndeployAll();
        }

        private void RunAssertionEventsProcessed(RegressionEnvironment env)
        {
            var listenerOne = new SupportListenerTimerHRes();
            var listenerTwo = new SupportListenerTimerHRes();
            var listenerThree = new SupportListenerTimerHRes();
            env.CompileDeploy("@Name('s0') select SupportStaticMethodLib.Sleep(100) from MyMap")
                .Statement("s0")
                .AddListener(listenerOne);
            env.CompileDeploy("@Name('s1') select SupportStaticMethodLib.Sleep(100) from SupportBean")
                .Statement("s1")
                .AddListener(listenerTwo);
            env.CompileDeploy("@Name('s2') select SupportStaticMethodLib.Sleep(100) from XMLType")
                .Statement("s2")
                .AddListener(listenerThree);

            var senderOne = env.EventService.GetEventSender("MyMap");
            var senderTwo = env.EventService.GetEventSender("SupportBean");
            var senderThree = env.EventService.GetEventSender("XMLType");

            var start = PerformanceObserver.NanoTime;
            for (var i = 0; i < 2; i++) {
                env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                senderOne.SendEvent(new Dictionary<string, object>());
                env.SendEventBean(new SupportBean());
                senderTwo.SendEvent(new SupportBean());
                env.SendEventXMLDOM(SupportXML.GetDocument("<myevent/>"), "XMLType");
                senderThree.SendEvent(SupportXML.GetDocument("<myevent/>"));
            }

            var end = PerformanceObserver.NanoTime;
            var delta = (end - start) / 1000000;
            Assert.IsTrue(delta < 500);

            try {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }

            Assert.AreEqual(4, listenerOne.NewEvents.Count);
            Assert.AreEqual(4, listenerTwo.NewEvents.Count);
            Assert.AreEqual(4, listenerThree.NewEvents.Count);

            var spi = (EPRuntimeSPI) env.Runtime;
            Assert.AreEqual(0, spi.ServicesContext.ThreadingService.InboundQueue.Count);
            Assert.IsNotNull(spi.ServicesContext.ThreadingService.InboundThreadPool);

            env.UndeployAll();
        }

        // Used by test
        public static bool ThrowException()
        {
            throw new EPException("Intended for testing");
        }
    }
} // end of namespace