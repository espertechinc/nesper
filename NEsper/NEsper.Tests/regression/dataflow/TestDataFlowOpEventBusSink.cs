///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestDataFlowOpEventBusSink
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _listenerTwo;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listenerTwo = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _listenerTwo = null;
        }
    
        [Test]
        public void TestAllTypes()
        {
            DefaultSupportGraphEventUtil.AddTypeConfiguration(_epService);
    
            RunAssertionAllTypes("MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
            RunAssertionAllTypes("MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionAllTypes("MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionAllTypes("MyEvent", DefaultSupportGraphEventUtil.PONOEvents);
    
            // invalid: output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "DF1", "create dataflow DF1 EventBusSink -> s1 {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EventBusSink': EventBusSink operator does not provide an output stream");
    
            _epService.EPAdministrator.CreateEPL("create schema SampleSchema(tagId string, locX double, locY double)");
            String docSmple = "create dataflow MyDataFlow\n" +
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
            _epService.EPAdministrator.CreateEPL(docSmple);
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
        }
    
        private void RunAssertionAllTypes(String typeName, Object[] events)
        {
            String graph = "create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> instream<" + typeName + ">{}" +
                    "EventBusSink(instream) {}";
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL(graph);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeName);
            stmt.Events += _listener.Update;
    
            DefaultSupportSourceOp source = new DefaultSupportSourceOp(events);
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(source));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options);
            instance.Run();

            EPAssertionUtil.AssertPropsPerRow(
                _listener.GetNewDataListFlattened(),
                "myDouble,myInt,myString".Split(','),
                new Object[][]
                {
                    new Object[] { 1.1d, 1, "one" },
                    new Object[] { 2.2d, 2, "two" }
                });
            _listener.Reset();
    
            stmtGraph.Dispose();
        }
    
        [Test]
        public void TestBeacon()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(p0 string, p1 long)");
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyEvent");
            stmt.Events += _listener.Update;
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "" +
                    "BeaconSource -> BeaconStream<MyEvent> {" +
                    "  iterations : 3," +
                    "  p0 : 'abc'," +
                    "  p1 : 1," +
                    "}" +
                    "EventBusSink(BeaconStream) {}");
    
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", null).Start();
            _listener.WaitForInvocation(3000, 3);
            EventBean[] events = _listener.GetNewDataListFlattened();
    
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual("abc", events[i].Get("p0"));
                var val = (long?) events[i].Get("p1");
                Assert.IsTrue(val > 0 && val < 10);
            }
        }
    
        [Test]
        public void TestSendEventDynamicType()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEventOne(type string, p0 int, p1 string)");
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEventTwo(type string, f0 string, f1 int)");
    
            _epService.EPAdministrator.CreateEPL("select * from MyEventOne").Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("select * from MyEventTwo").Events += _listenerTwo.Update;
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlow " +
                    "MyObjectArrayGraphSource -> OutStream<?> {}" +
                    "EventBusSink(OutStream) {" +
                    "  collector : {" +
                    "    class: '" + typeof(MyTransformToEventBus).FullName + "'" +
                    "  }" +
                    "}");
    
            MyObjectArrayGraphSource source = new MyObjectArrayGraphSource(Collections.List(
                    new Object[]{"type1", 100, "abc"},
                    new Object[]{"type2", "GE", -1}
            ).GetEnumerator());
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(source));
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow", options).Start();
    
            _listener.WaitForInvocation(3000, 1);
            _listenerTwo.WaitForInvocation(3000, 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "p0,p1".Split(','), new Object[]{100, "abc"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), "f0,f1".Split(','), new Object[] {"GE", -1});
        }
    
        public class MyTransformToEventBus : EPDataFlowEventCollector
        {
            public void Collect(EPDataFlowEventCollectorContext context) {
                var eventObj = (Object[]) context.Event;
                if (eventObj[0].Equals("type1")) 
                {
                    context.EventBusCollector.SendEvent(eventObj, "MyEventOne");
                }
                else 
                {
                    context.EventBusCollector.SendEvent(eventObj, "MyEventTwo");
                }
            }
        }
    }
}
