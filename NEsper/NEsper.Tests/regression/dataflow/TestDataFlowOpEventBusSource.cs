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
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestDataFlowOpEventBusSource  {
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestAllTypes() {
            DefaultSupportGraphEventUtil.AddTypeConfiguration(_epService);
    
            RunAssertionAllTypes("MyMapEvent", DefaultSupportGraphEventUtil.MapEventsSendable);
            RunAssertionAllTypes("MyXMLEvent", DefaultSupportGraphEventUtil.XMLEventsSendable);
            RunAssertionAllTypes("MyOAEvent", DefaultSupportGraphEventUtil.OAEventsSendable);
            RunAssertionAllTypes("MyEvent", DefaultSupportGraphEventUtil.PONOEventsSendable);
    
            // invalid: no output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "DF1", "create dataflow DF1 EventBusSource {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EventBusSource': EventBusSource operator requires one output stream but produces 0 streams");
    
            // invalid: type not found
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "DF1", "create dataflow DF1 EventBusSource -> ABC {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EventBusSource': EventBusSource operator requires an event type declated for the output stream");
    
            // test doc samples
            _epService.EPAdministrator.CreateEPL("create schema SampleSchema(tagId string, locX double, locY double)");
            String epl = "create dataflow MyDataFlow\n" +
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
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
        }
    
        private void RunAssertionAllTypes(String typeName, SendableEvent[] events) {
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<" + typeName + "> {} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");
    
            DefaultSupportCaptureOp future = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future));
    
            events[0].Send(_epService.EPRuntime);
            Assert.AreEqual(0, future.GetCurrent().Length);
    
            EPDataFlowInstance df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
    
            events[0].Send(_epService.EPRuntime);
            Assert.AreEqual(0, future.GetCurrent().Length);
    
            df.Start();
    
            // send events
            for (int i = 0; i < events.Length; i++) {
                events[i].Send(_epService.EPRuntime);
            }
    
            // assert
            future.WaitForInvocation(200, events.Length);
            Object[] rows = future.GetCurrentAndReset();
            Assert.AreEqual(events.Length, rows.Length);
            for (int i = 0; i < events.Length; i++) {
                Assert.AreSame(events[i].Underlying, rows[i]);
            }
    
            df.Cancel();
    
            events[0].Send(_epService.EPRuntime);
            Thread.Sleep(50);
            Assert.AreEqual(0, future.GetCurrent().Length);
    
            stmtGraph.Dispose();
        }
    
        [Test]
        public void TestSchemaObjectArray() {
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(p0 string, p1 long)");
    
            RunAssertionOA(false);
            RunAssertionOA(true);
    
            // test collector
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<MyEvent> {filter: p0 like 'A%'} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");
    
            MyCollector collector = new MyCollector();
            DefaultSupportCaptureOp future = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future))
                    .ParameterProvider(new DefaultSupportGraphParamProvider(Collections.SingletonDataMap("collector", collector)));
    
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            instance.Start();
    
            _epService.EPRuntime.SendEvent(new Object[] {"B", 100L}, "MyEvent");
            Thread.Sleep(50);
            Assert.IsNull(collector.GetLast());
    
            _epService.EPRuntime.SendEvent(new Object[] {"A", 101L}, "MyEvent");
            future.WaitForInvocation(100, 1);
            Assert.NotNull(collector.GetLast().Emitter);
            Assert.AreEqual("MyEvent", collector.GetLast().Event.EventType.Name);
            Assert.AreEqual(false, collector.GetLast().IsSubmitEventBean);
        }
    
        private void RunAssertionOA(bool underlying) {
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<" + (underlying ? "MyEvent" : "EventBean<MyEvent>") + "> {} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}");
    
            DefaultSupportCaptureOp future = new DefaultSupportCaptureOp(1);
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(future));
    
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            instance.Start();
    
            _epService.EPRuntime.SendEvent(new Object[] {"abc", 100L}, "MyEvent");
            Object[] rows = future.GetValue(TimeSpan.FromSeconds(1));
            Assert.AreEqual(1, rows.Length);
            if (underlying) {
                EPAssertionUtil.AssertEqualsExactOrder((Object[]) rows[0], new Object[] {"abc", 100L});
            }
            else {
                EPAssertionUtil.AssertProps((EventBean) rows[0], "p0,p1".Split(','), new Object[] {"abc", 100L});
            }
    
            instance.Cancel();
            stmtGraph.Dispose();
        }
    
        public class MyCollector : EPDataFlowEventBeanCollector {
            private EPDataFlowEventBeanCollectorContext _last;
    
            public void Collect(EPDataFlowEventBeanCollectorContext context) {
                _last = context;
                context.Emitter.Submit(context.Event);
            }
    
            public EPDataFlowEventBeanCollectorContext GetLast() {
                return _last;
            }
        }
    
        public class MyDummyCollector : EPDataFlowEventBeanCollector {
            public void Collect(EPDataFlowEventBeanCollectorContext context) {
    
            }
        }
    }
}
