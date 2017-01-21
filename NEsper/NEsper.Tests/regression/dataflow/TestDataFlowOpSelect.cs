///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestDataFlowOpSelect
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
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
        public void TestDocSamples()
        {
            String epl = "create dataflow MyDataFlow\n" +
                    "  create schema SampleSchema(tagId string, locX double),\t// sample type\t\t\t\n" +
                    "  BeaconSource -> instream<SampleSchema> {}  // sample stream\n" +
                    "  BeaconSource -> secondstream<SampleSchema> {}  // sample stream\n" +
                    "  \n" +
                    "  // Simple continuous count of events\n" +
                    "  Select(instream) -> outstream {\n" +
                    "    select: (select count(*) from instream)\n" +
                    "  }\n" +
                    "  \n" +
                    "  // Demonstrate use of alias\n" +
                    "  Select(instream as myalias) -> outstream {\n" +
                    "    select: (select count(*) from myalias)\n" +
                    "  }\n" +
                    "  \n" +
                    "  // Output only when the readonly marker arrives\n" +
                    "  Select(instream as myalias) -> outstream {\n" +
                    "    select: (select count(*) from myalias),\n" +
                    "    iterate: true\n" +
                    "  }\n" +
                    "\n" +
                    "  // Same input port for the two sample streams.\n" +
                    "  Select( (instream, secondstream) as myalias) -> outstream {\n" +
                    "    select: (select count(*) from myalias)\n" +
                    "  }\n" +
                    "\n" +
                    "  // A join with multiple input streams,\n" +
                    "  // joining the last event per stream forming pairs\n" +
                    "  Select(instream, secondstream) -> outstream {\n" +
                    "    select: (select a.tagId, b.tagId \n" +
                    "                 from instream.std:lastevent() as a, secondstream.std:lastevent() as b)\n" +
                    "  }\n" +
                    "  \n" +
                    "  // A join with multiple input streams and using aliases.\n" +
                    "  @Audit Select(instream as S1, secondstream as S2) -> outstream {\n" +
                    "    select: (select a.tagId, b.tagId \n" +
                    "                 from S1.std:lastevent() as a, S2.std:lastevent() as b)\n" +
                    "  }";
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
        }

        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddNamespaceImport<DefaultSupportSourceOp>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            TryInvalidInstantiate("insert into ABC select TheString from ME", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Insert-into clause is not supported");

            TryInvalidInstantiate("select irstream TheString from ME", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Selecting remove-stream is not supported");

            TryInvalidInstantiate("select TheString from pattern[SupportBean]", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': From-clause must contain only streams and cannot contain patterns or other constructs");

            TryInvalidInstantiate("select TheString from DUMMY", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Failed to find stream 'DUMMY' among input ports, input ports are [ME]");

            TryInvalidInstantiate("select TheString from ME output every 10 seconds", true,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Output rate limiting is not supported with 'iterate'");

            TryInvalidInstantiate("select (select * from SupportBean.std:lastevent()) from ME", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Subselects are not supported");
        }

        private void TryInvalidInstantiate(String select, bool iterate, String message)
        {
            String graph = "create dataflow MySelect\n" +
                    "DefaultSupportSourceOp -> instream<SupportBean>{}\n" +
                    "Select(instream as ME) -> outstream {select: (" + select + "), iterate: " + iterate + "}\n" +
                    "DefaultSupportCaptureOp(outstream) {}";
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL(graph);

            try
            {
                _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect");
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
            finally
            {
                stmtGraph.Dispose();
            }
        }

        [Test]
        public void TestIterateFinalMarker()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            String graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                    "@Audit Select(instream_s0 as ALIAS) -> outstream {\n" +
                    "  select: (select TheString, sum(IntPrimitive) as SumInt from ALIAS group by TheString order by TheString asc),\n" +
                    "  iterate: true" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL(graph);

            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            IDictionary<String, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();

            Emitter emitter = captive.Emitters.Get("emitterS0");
            emitter.Submit(new SupportBean("E3", 4));
            emitter.Submit(new SupportBean("E2", 3));
            emitter.Submit(new SupportBean("E1", 1));
            emitter.Submit(new SupportBean("E2", 2));
            emitter.Submit(new SupportBean("E1", 5));
            Assert.AreEqual(0, capture.GetCurrent().Length);

            emitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            EPAssertionUtil.AssertPropsPerRow(capture.GetCurrent(), "TheString,SumInt".Split(','), new Object[][] { new Object[] { "E1", 6 }, new Object[] { "E2", 5 }, new Object[] { "E3", 4 } });

            instance.Cancel();
        }

        [Test]
        public void TestOutputRateLimit()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            String graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                    "Select(instream_s0) -> outstream {\n" +
                    "  select: (select sum(IntPrimitive) as sumInt from instream_s0 output snapshot every 1 minute)\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL(graph);

            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            IDictionary<String, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
            Emitter emitter = captive.Emitters.Get("emitterS0");

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            emitter.Submit(new SupportBean("E1", 5));
            emitter.Submit(new SupportBean("E2", 3));
            emitter.Submit(new SupportBean("E3", 6));
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(60000 + 5000));
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "sumInt".Split(','), new Object[] { 14 });

            emitter.Submit(new SupportBean("E4", 3));
            emitter.Submit(new SupportBean("E5", 6));
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000 + 5000));
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "sumInt".Split(','), new Object[] { 14 + 9 });

            instance.Cancel();

            emitter.Submit(new SupportBean("E5", 6));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(240000 + 5000));
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);
        }

        [Test]
        public void TestTimeWindowTriggered()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            String graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                    "Select(instream_s0) -> outstream {\n" +
                    "  select: (select sum(IntPrimitive) as sumInt from instream_s0.win:time(1 minute))\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL(graph);

            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            IDictionary<String, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            captive.Emitters.Get("emitterS0").Submit(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "sumInt".Split(','), new Object[] { 2 });

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            captive.Emitters.Get("emitterS0").Submit(new SupportBean("E2", 5));
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "sumInt".Split(','), new Object[] { 7 });

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(65000));
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "sumInt".Split(','), new Object[] { 5 });

            instance.Cancel();
        }

        [Test]
        public void TestOuterJoinMultirow()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));

            String graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean_S0>{name: 'emitterS0'}\n" +
                    "Emitter -> instream_s1<SupportBean_S1>{name: 'emitterS1'}\n" +
                    "Select(instream_s0 as S0, instream_s1 as S1) -> outstream {\n" +
                    "  select: (select p00, p10 from S0.win:keepall() full outer join S1.win:keepall())\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            _epService.EPAdministrator.CreateEPL(graph);

            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            IDictionary<String, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);

            EPDataFlowInstanceCaptive captive = instance.StartCaptive();

            captive.Emitters.Get("emitterS0").Submit(new SupportBean_S0(1, "S0_1"));
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "p00,p11".Split(','), new Object[] { "S0_1", null });

            instance.Cancel();
        }

        [Test]
        public void TestFromClauseJoinOrder()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S2));

            RunAssertionJoinOrder("from S2.std:lastevent() as s2, S1.std:lastevent() as s1, S0.std:lastevent() as s0");
            RunAssertionJoinOrder("from S0.std:lastevent() as s0, S1.std:lastevent() as s1, S2.std:lastevent() as s2");
            RunAssertionJoinOrder("from S1.std:lastevent() as s1, S2.std:lastevent() as s2, S0.std:lastevent() as s0");
        }

        public void RunAssertionJoinOrder(String fromClause)
        {
            String graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean_S0>{name: 'emitterS0'}\n" +
                    "Emitter -> instream_s1<SupportBean_S1>{name: 'emitterS1'}\n" +
                    "Emitter -> instream_s2<SupportBean_S2>{name: 'emitterS2'}\n" +
                    "Select(instream_s0 as S0, instream_s1 as S1, instream_s2 as S2) -> outstream {\n" +
                    "  select: (select s0.id as s0id, s1.id as s1id, s2.id as s2id " + fromClause + ")\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL(graph);

            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            IDictionary<String, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);

            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);

            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
            captive.Emitters.Get("emitterS0").Submit(new SupportBean_S0(1));
            captive.Emitters.Get("emitterS1").Submit(new SupportBean_S1(10));
            Assert.AreEqual(0, capture.GetCurrent().Length);

            captive.Emitters.Get("emitterS2").Submit(new SupportBean_S2(100));
            Assert.AreEqual(1, capture.GetCurrent().Length);
            EPAssertionUtil.AssertProps(capture.GetCurrentAndReset()[0], "s0id,s1id,s2id".Split(','), new Object[] { 1, 10, 100 });

            instance.Cancel();

            captive.Emitters.Get("emitterS2").Submit(new SupportBean_S2(101));
            Assert.AreEqual(0, capture.GetCurrent().Length);

            stmtGraph.Dispose();
        }

        [Test]
        public void TestAllTypes()
        {
            DefaultSupportGraphEventUtil.AddTypeConfiguration(_epService);

            RunAssertionAllTypes("MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
            RunAssertionAllTypes("MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionAllTypes("MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionAllTypes("MyEvent", DefaultSupportGraphEventUtil.PONOEvents);
        }

        private void RunAssertionAllTypes(String typeName, Object[] events)
        {
            String graph = "create dataflow MySelect\n" +
                    "DefaultSupportSourceOp -> instream<" + typeName + ">{}\n" +
                    "Select(instream as ME) -> outstream {select: (select myString, sum(myInt) as total from ME)}\n" +
                    "DefaultSupportCaptureOp(outstream) {}";
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL(graph);

            DefaultSupportSourceOp source = new DefaultSupportSourceOp(events);
            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp(2);
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(source, capture));
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);

            instance.Run();

            Object[] result = capture.GetAndReset()[0].ToArray();
            EPAssertionUtil.AssertPropsPerRow(result, "myString,total".Split(','), new Object[][] { new Object[] { "one", 1 }, new Object[] { "two", 3 } });

            instance.Cancel();

            stmtGraph.Dispose();
        }

        [Test]
        public void TestSelectPerformance()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(p0 string, p1 long)");

            /*
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0, sum(p1) from MyEvent");
    
            long start = Environment.TickCount;
            for (int i = 0; i < 1000000; i++) {
                epService.EPRuntime.SendEvent(new Object[] {"E1", 1L}, "MyEvent");
            }
            long end = Environment.TickCount;
            Console.WriteLine("delta=" + (end - start) / 1000d);
            Console.WriteLine(stmt.GetEnumerator().Next().Get("sum(p1)"));
            */

            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "Emitter -> instream<MyEvent> {name: 'E1'}" +
                    "Select(instream as ME) -> astream {select: (select p0, sum(p1) from ME)}");
            EPDataFlowInstance df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne");
            Emitter emitter = df.StartCaptive().Emitters.Get("E1");
            long start = Environment.TickCount;
            for (int i = 0; i < 1; i++)
            {
                emitter.Submit(new Object[] { "E1", 1L });
            }
            long end = Environment.TickCount;
            //Console.WriteLine("delta=" + (end - start) / 1000d);
        }

    }
}
