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
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowOpEventBusSource : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionAllTypes(epService);
            RunAssertionSchemaObjectArray(epService);
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService) {
            DefaultSupportGraphEventUtil.AddTypeConfiguration((EPServiceProviderSPI) epService);
    
            RunAssertionAllTypes(epService, "MyMapEvent", DefaultSupportGraphEventUtil.MapEventsSendable);
            RunAssertionAllTypes(epService, "MyXMLEvent", DefaultSupportGraphEventUtil.XMLEventsSendable);
            RunAssertionAllTypes(epService, "MyOAEvent", DefaultSupportGraphEventUtil.OAEventsSendable);
            RunAssertionAllTypes(epService, "MyEvent", DefaultSupportGraphEventUtil.PonoEventsSendable);
    
            // invalid: no output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 EventBusSource {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EventBusSource': EventBusSource operator requires one output stream but produces 0 streams");
    
            // invalid: type not found
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 EventBusSource -> ABC {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EventBusSource': EventBusSource operator requires an event type declated for the output stream");
    
            // test doc samples
            epService.EPAdministrator.CreateEPL("create schema SampleSchema(tagId string, locX double, locY double)");
            string epl = "create dataflow MyDataFlow\n" +
                    "\n" +
                    "  // Receive all SampleSchema events from the event bus.\n" +
                    "  // No transformation.\n" +
                    "  EventBusSource -> stream.one<SampleSchema> {}\n" +
                    "  \n" +
                    "  // Receive all SampleSchema events with tag id '001' from the event bus.\n" +
                    "  // No transformation.\n" +
                    "  EventBusSource -> stream.one<SampleSchema> {\n" +
                    "    filter : tagId = '001'\n" +
                    "  }\n" +
                    "\n" +
                    "  // Receive all SampleSchema events from the event bus.\n" +
                    "  // With collector that performs transformation.\n" +
                    "  EventBusSource -> stream.two<SampleSchema> {\n" +
                    "    collector : {\n" +
                    "      class : '" + typeof(MyDummyCollector).FullName + "'\n" +
                    "    },\n" +
                    "  }";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService, string typeName, SendableEvent[] events) {
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<" + typeName + "> {} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");
    
            var future = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future));
    
            events[0].Send(epService.EPRuntime);
            Assert.AreEqual(0, future.Current.Length);
    
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
    
            events[0].Send(epService.EPRuntime);
            Assert.AreEqual(0, future.Current.Length);
    
            df.Start();
    
            // send events
            for (int i = 0; i < events.Length; i++) {
                events[i].Send(epService.EPRuntime);
            }
    
            // assert
            future.WaitForInvocation(200, events.Length);
            object[] rows = future.GetCurrentAndReset();
            Assert.AreEqual(events.Length, rows.Length);
            for (int i = 0; i < events.Length; i++) {
                Assert.AreSame(events[i].Underlying, rows[i]);
            }
    
            df.Cancel();
    
            events[0].Send(epService.EPRuntime);
            Thread.Sleep(50);
            Assert.AreEqual(0, future.Current.Length);
    
            stmtGraph.Dispose();
        }
    
        private void RunAssertionSchemaObjectArray(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventOA(p0 string, p1 long)");
    
            RunAssertionOA(epService, false);
            RunAssertionOA(epService, true);
    
            // test collector
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<MyEventOA> {filter: p0 like 'A%'} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");
    
            var collector = new MyCollector();
            var future = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(future))
                .ParameterProvider(new DefaultSupportGraphParamProvider(
                    Collections.SingletonDataMap("collector", collector)));
    
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            instance.Start();
    
            epService.EPRuntime.SendEvent(new object[]{"B", 100L}, "MyEventOA");
            Thread.Sleep(50);
            Assert.IsNull(collector.Last);
    
            epService.EPRuntime.SendEvent(new object[]{"A", 101L}, "MyEventOA");
            future.WaitForInvocation(100, 1);
            Assert.IsNotNull(collector.Last.Emitter);
            Assert.AreEqual("MyEventOA", collector.Last.Event.EventType.Name);
            Assert.AreEqual(false, collector.Last.IsSubmitEventBean);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOA(EPServiceProvider epService, bool underlying) {
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<" + (underlying ? "MyEventOA" : "EventBean<MyEventOA>") + "> {} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");
    
            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future));
    
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            instance.Start();
    
            epService.EPRuntime.SendEvent(new object[]{"abc", 100L}, "MyEventOA");
            object[] rows = future.GetValue(1, TimeUnit.SECONDS);
            Assert.AreEqual(1, rows.Length);
            if (underlying) {
                EPAssertionUtil.AssertEqualsExactOrder((object[]) rows[0], new object[]{"abc", 100L});
            } else {
                EPAssertionUtil.AssertProps((EventBean) rows[0], "p0,p1".Split(','), new object[]{"abc", 100L});
            }
    
            instance.Cancel();
            stmtGraph.Dispose();
        }
    
        public class MyCollector : EPDataFlowEventBeanCollector {
            public void Collect(EPDataFlowEventBeanCollectorContext context) {
                Last = context;
                context.Emitter.Submit(context.Event);
            }

            public EPDataFlowEventBeanCollectorContext Last { get; private set; }
        }
    
        public class MyDummyCollector : EPDataFlowEventBeanCollector {
            public void Collect(EPDataFlowEventBeanCollectorContext context) {
    
            }
        }
    }
} // end of namespace
