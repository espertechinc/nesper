///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            configuration.Common.AddEventType("MyOA", Array.Empty<string>(), Array.Empty<object>());

            var xmlDOMEventTypeDesc = new ConfigurationCommonEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            configuration.Common.AddEventType("XMLType", xmlDOMEventTypeDesc);

            configuration.Compiler.AddPlugInSingleRowFunction(
                "throwException",
                GetType(),
                "ThrowException",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED,
                true);
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.RUNTIMEOPS);
        }

        public void Run(RegressionEnvironment env)
        {
            RunAssertionEventsProcessed(env);
            RunAssertionExceptionHandler(env);
        }

        private void RunAssertionExceptionHandler(RegressionEnvironment env)
        {
            var epl = "@name('ABCName') select * from SupportBean(throwException())";
            env.CompileDeploy(epl);

            var handler = SupportExceptionHandlerFactory.Handlers[^1];
            env.SendEventBean(new SupportBean());

            var count = 0;
            while (true) {
                if (handler.InboundPoolContexts.Count == 1) {
                    break;
                }

                if (count++ < 100) {
                    Thread.Sleep(100);
                }

                if (count >= 100) {
                    Assert.Fail();
                }
            }

            env.UndeployAll();
        }

        private void RunAssertionEventsProcessed(RegressionEnvironment env)
        {
            var listenerMap = new SupportListenerTimerHRes();
            var listenerBean = new SupportListenerTimerHRes();
            var listenerXML = new SupportListenerTimerHRes();
            var listenerOA = new SupportListenerTimerHRes();
            var listenerJson = new SupportListenerTimerHRes();
            env.CompileDeploy("@name('s0') select SupportStaticMethodLib.Sleep(100) from MyMap")
                .Statement("s0")
                .AddListener(listenerMap);
            env.CompileDeploy("@name('s1') select SupportStaticMethodLib.Sleep(100) from SupportBean")
                .Statement("s1")
                .AddListener(listenerBean);
            env.CompileDeploy("@name('s2') select SupportStaticMethodLib.Sleep(100) from XMLType")
                .Statement("s2")
                .AddListener(listenerXML);
            env.CompileDeploy("@name('s3') select SupportStaticMethodLib.Sleep(100) from MyOA")
                .Statement("s3")
                .AddListener(listenerOA);
            env.CompileDeploy(
                    "@public @buseventtype create json schema JsonEvent();\n" +
                    "@name('s4') select SupportStaticMethodLib.Sleep(100) from JsonEvent")
                .Statement("s4")
                .AddListener(listenerJson);

            var senderMap = env.EventService.GetEventSender("MyMap");
            var senderBean = env.EventService.GetEventSender("SupportBean");
            var senderXML = env.EventService.GetEventSender("XMLType");
            var senderOA = env.EventService.GetEventSender("MyOA");
            var senderJson = env.EventService.GetEventSender("JsonEvent");

            var start = PerformanceObserver.MicroTime;
            for (var i = 0; i < 2; i++) {
                env.SendEventMap(new Dictionary<string, object>(), "MyMap");
                senderMap.SendEvent(new Dictionary<string, object>());
                env.SendEventBean(new SupportBean());
                senderBean.SendEvent(new SupportBean());
                env.SendEventXMLDOM(SupportXML.GetDocument("<myevent/>"), "XMLType");
                senderXML.SendEvent(SupportXML.GetDocument("<myevent/>"));
                env.SendEventObjectArray(Array.Empty<object>(), "MyOA");
                senderOA.SendEvent(Array.Empty<object>());
                env.SendEventJson("{}", "JsonEvent");
                senderJson.SendEvent("{}");
            }

            var end = PerformanceObserver.MicroTime;
            var delta = (end - start) / 1000;
            ClassicAssert.Less(delta, 500);

            Thread.Sleep(1000);

            foreach (var listener in Arrays.AsList(listenerMap, listenerBean, listenerXML, listenerOA, listenerJson)) {
                ClassicAssert.AreEqual(4, listener.NewEvents.Count);
            }

            var spi = (EPRuntimeSPI)env.Runtime;
            ClassicAssert.AreEqual(0, spi.ServicesContext.ThreadingService.InboundQueue.Count);
            ClassicAssert.IsNotNull(spi.ServicesContext.ThreadingService.InboundThreadPool);

            env.UndeployAll();
        }

        // Used by test
        public static bool ThrowException()
        {
            throw new EPRuntimeException("Intended for testing");
        }
    }
} // end of namespace