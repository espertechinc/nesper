///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowOpEventBusSink : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionAllTypes(epService);
            RunAssertionBeacon(epService);
            RunAssertionSendEventDynamicType(epService);
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService) {
            DefaultSupportGraphEventUtil.AddTypeConfiguration((EPServiceProviderSPI) epService);
    
            RunAssertionAllTypes(epService, "MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
            RunAssertionAllTypes(epService, "MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionAllTypes(epService, "MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionAllTypes(epService, "MyEvent", DefaultSupportGraphEventUtil.PonoEvents);
    
            // invalid: output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 EventBusSink -> s1 {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EventBusSink': EventBusSink operator does not provide an output stream");
    
            epService.EPAdministrator.CreateEPL("create schema SampleSchema(tagId string, locX double, locY double)");
            string docSmple = "create dataflow MyDataFlow\n" +
                    "BeaconSource -> instream<SampleSchema> {} // produces sample stream to\n" +
                    "//demonstrate below\n" +
                    "// Send SampleSchema events produced by beacon to the event bus.\n" +
                    "EventBusSink(instream) {}\n" +
                    "\n" +
                    "// Send SampleSchema events produced by beacon to the event bus.\n" +
                    "// With collector that performs transformation.\n" +
                    "EventBusSink(instream) {\n" +
                    "collector : {\n" +
                    "class : '" + typeof(MyTransformToEventBus).FullName + "'\n" +
                    "}\n" +
                    "}";
            epService.EPAdministrator.CreateEPL(docSmple);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService, string typeName, object[] events) {
            string graph = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> instream<" + typeName + ">{}" +
                    "EventBusSink(instream) {}";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from " + typeName);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var source = new DefaultSupportSourceOp(events);
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(source));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options);
            instance.Run();
    
            EPAssertionUtil.AssertPropsPerRow(listener.GetNewDataListFlattened(), "myDouble,myInt,myString".Split(','), new object[][]{new object[] {1.1d, 1, "one"}, new object[] {2.2d, 2, "two"}});
            listener.Reset();
    
            stmtGraph.Dispose();
        }
    
        private void RunAssertionBeacon(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventBeacon(p0 string, p1 long)");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyEventBeacon");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "" +
                    "BeaconSource -> BeaconStream<MyEventBeacon> {" +
                    "  iterations : 3," +
                    "  p0 : 'abc'," +
                    "  p1 : 1," +
                    "}" +
                    "EventBusSink(BeaconStream) {}");
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", null).Start();
            listener.WaitForInvocation(3000, 3);
            EventBean[] events = listener.GetNewDataListFlattened();
    
            for (int i = 0; i < 3; i++) {
                Assert.AreEqual("abc", events[i].Get("p0"));
                long val = (long) events[i].Get("p1");
                Assert.IsTrue(val > 0 && val < 10);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSendEventDynamicType(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventOne(type string, p0 int, p1 string)");
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventTwo(type string, f0 string, f1 int)");
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from MyEventOne").Events += listener.Update;
            var listenerTwo = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from MyEventTwo").Events += listenerTwo.Update;
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow " +
                    "MyObjectArrayGraphSource -> OutStream<?> {}" +
                    "EventBusSink(OutStream) {" +
                    "  collector : {" +
                    "    class: '" + typeof(MyTransformToEventBus).FullName + "'" +
                    "  }" +
                    "}");
    
            var source = new MyObjectArrayGraphSource(Collections.List(
                    new object[]{"type1", 100, "abc"},
                    new object[]{"type2", "GE", -1}
            ).GetEnumerator());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(source));
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options).Start();
    
            listener.WaitForInvocation(3000, 1);
            listenerTwo.WaitForInvocation(3000, 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "p0,p1".Split(','), new object[]{100, "abc"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "f0,f1".Split(','), new object[]{"GE", -1});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class MyTransformToEventBus : EPDataFlowEventCollector {
    
            public void Collect(EPDataFlowEventCollectorContext context) {
                object[] eventObj = (object[]) context.Event;
                if (eventObj[0].Equals("type1")) {
                    context.EventBusCollector.SendEvent(eventObj, "MyEventOne");
                } else {
                    context.EventBusCollector.SendEvent(eventObj, "MyEventTwo");
                }
            }
        }
    }
} // end of namespace
