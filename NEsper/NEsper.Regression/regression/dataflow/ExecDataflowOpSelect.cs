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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowOpSelect : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionDocSamples(epService);
            RunAssertionInvalid(epService);
            RunAssertionIterateFinalMarker(epService);
            RunAssertionOutputRateLimit(epService);
            RunAssertionTimeWindowTriggered(epService);
            RunAssertionOuterJoinMultirow(epService);
            RunAssertionFromClauseJoinOrder(epService);
            RunAssertionAllTypes(epService);
            RunAssertionSelectPerformance(epService);
        }
    
        private void RunAssertionDocSamples(EPServiceProvider epService) {
            string epl = "create dataflow MyDataFlow\n" +
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
                    "  // Output only when the final marker arrives\n" +
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
                    "                 from instream#lastevent as a, secondstream#lastevent as b)\n" +
                    "  }\n" +
                    "  \n" +
                    "  // A join with multiple input streams and using aliases.\n" +
                    "  @Audit Select(instream as S1, secondstream as S2) -> outstream {\n" +
                    "    select: (select a.tagId, b.tagId \n" +
                    "                 from S1#lastevent as a, S2#lastevent as b)\n" +
                    "  }";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp).Namespace);
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            TryInvalidInstantiate(epService, "insert into ABC select TheString from ME", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Insert-into clause is not supported");
    
            TryInvalidInstantiate(epService, "select irstream TheString from ME", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Selecting remove-stream is not supported");
    
            TryInvalidInstantiate(epService, "select TheString from pattern[SupportBean]", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': From-clause must contain only streams and cannot contain patterns or other constructs");
    
            TryInvalidInstantiate(epService, "select TheString from DUMMY", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Failed to find stream 'DUMMY' among input ports, input ports are [ME]");
    
            TryInvalidInstantiate(epService, "select TheString from ME output every 10 seconds", true,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Output rate limiting is not supported with 'iterate'");
    
            TryInvalidInstantiate(epService, "select (select * from SupportBean#lastevent) from ME", false,
                    "Failed to instantiate data flow 'MySelect': Failed validation for operator 'Select': Subselects are not supported");
        }
    
        private void TryInvalidInstantiate(EPServiceProvider epService, string select, bool iterate, string message) {
            string graph = "create dataflow MySelect\n" +
                    "DefaultSupportSourceOp -> instream<SupportBean>{}\n" +
                    "Select(instream as ME) -> outstream {select: (" + select + "), iterate: " + iterate + "}\n" +
                    "DefaultSupportCaptureOp(outstream) {}";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);
    
            try {
                epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect");
                Assert.Fail();
            } catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual(message, ex.Message);
            } finally {
                stmtGraph.Dispose();
            }
        }
    
        private void RunAssertionIterateFinalMarker(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                    "@Audit Select(instream_s0 as ALIAS) -> outstream {\n" +
                    "  select: (select TheString, sum(IntPrimitive) as sumInt from ALIAS group by TheString order by TheString asc),\n" +
                    "  iterate: true" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL(graph);
    
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            IDictionary<string, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);
    
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
    
            Emitter emitter = captive.Emitters.Get("emitterS0");
            emitter.Submit(new SupportBean("E3", 4));
            emitter.Submit(new SupportBean("E2", 3));
            emitter.Submit(new SupportBean("E1", 1));
            emitter.Submit(new SupportBean("E2", 2));
            emitter.Submit(new SupportBean("E1", 5));
            Assert.AreEqual(0, capture.Current.Length);

            emitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            EPAssertionUtil.AssertPropsPerRow(
                epService.Container, capture.Current, 
                "TheString,sumInt".Split(','), 
                new[] {new object[] {"E1", 6}, new object[] {"E2", 5}, new object[] {"E3", 4}});
    
            instance.Cancel();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOutputRateLimit(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                    "Select(instream_s0) -> outstream {\n" +
                    "  select: (select sum(IntPrimitive) as sumInt from instream_s0 output snapshot every 1 minute)\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL(graph);
    
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            IDictionary<string, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);
    
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
            Emitter emitter = captive.Emitters.Get("emitterS0");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            emitter.Submit(new SupportBean("E1", 5));
            emitter.Submit(new SupportBean("E2", 3));
            emitter.Submit(new SupportBean("E3", 6));
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(60000 + 5000));
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "sumInt".Split(','), new object[]{14});
    
            emitter.Submit(new SupportBean("E4", 3));
            emitter.Submit(new SupportBean("E5", 6));
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(120000 + 5000));
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "sumInt".Split(','), new object[]{14 + 9});
    
            instance.Cancel();
    
            emitter.Submit(new SupportBean("E5", 6));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(240000 + 5000));
            Assert.AreEqual(0, capture.GetCurrentAndReset().Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeWindowTriggered(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                    "Select(instream_s0) -> outstream {\n" +
                    "  select: (select sum(IntPrimitive) as sumInt from instream_s0#time(1 minute))\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL(graph);
    
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            IDictionary<string, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);
    
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            captive.Emitters.Get("emitterS0").Submit(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "sumInt".Split(','), new object[]{2});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            captive.Emitters.Get("emitterS0").Submit(new SupportBean("E2", 5));
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "sumInt".Split(','), new object[]{7});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(65000));
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "sumInt".Split(','), new object[]{5});
    
            instance.Cancel();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOuterJoinMultirow(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
    
            string graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean_S0>{name: 'emitterS0'}\n" +
                    "Emitter -> instream_s1<SupportBean_S1>{name: 'emitterS1'}\n" +
                    "Select(instream_s0 as S0, instream_s1 as S1) -> outstream {\n" +
                    "  select: (select p00, p10 from S0#keepall full outer join S1#keepall)\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            epService.EPAdministrator.CreateEPL(graph);
    
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            IDictionary<string, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);
    
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
    
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
    
            captive.Emitters.Get("emitterS0").Submit(new SupportBean_S0(1, "S0_1"));
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "p00,p11".Split(','), new object[]{"S0_1", null});
    
            instance.Cancel();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFromClauseJoinOrder(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S2));
    
            TryAssertionJoinOrder(epService, "from S2#lastevent as s2, S1#lastevent as s1, S0#lastevent as s0");
            TryAssertionJoinOrder(epService, "from S0#lastevent as s0, S1#lastevent as s1, S2#lastevent as s2");
            TryAssertionJoinOrder(epService, "from S1#lastevent as s1, S2#lastevent as s2, S0#lastevent as s0");
        }
    
        private void TryAssertionJoinOrder(EPServiceProvider epService, string fromClause) {
            string graph = "create dataflow MySelect\n" +
                    "Emitter -> instream_s0<SupportBean_S0>{name: 'emitterS0'}\n" +
                    "Emitter -> instream_s1<SupportBean_S1>{name: 'emitterS1'}\n" +
                    "Emitter -> instream_s2<SupportBean_S2>{name: 'emitterS2'}\n" +
                    "Select(instream_s0 as S0, instream_s1 as S1, instream_s2 as S2) -> outstream {\n" +
                    "  select: (select s0.id as s0id, s1.id as s1id, s2.id as s2id " + fromClause + ")\n" +
                    "}\n" +
                    "CaptureOp(outstream) {}\n";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);
    
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            IDictionary<string, Object> operators = CollectionUtil.PopulateNameValueMap("CaptureOp", capture);
    
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
    
            EPDataFlowInstanceCaptive captive = instance.StartCaptive();
            captive.Emitters.Get("emitterS0").Submit(new SupportBean_S0(1));
            captive.Emitters.Get("emitterS1").Submit(new SupportBean_S1(10));
            Assert.AreEqual(0, capture.Current.Length);
    
            captive.Emitters.Get("emitterS2").Submit(new SupportBean_S2(100));
            Assert.AreEqual(1, capture.Current.Length);
            EPAssertionUtil.AssertProps(
                epService.Container, capture.GetCurrentAndReset()[0], "s0id,s1id,s2id".Split(','), new object[]{1, 10, 100});
    
            instance.Cancel();
    
            captive.Emitters.Get("emitterS2").Submit(new SupportBean_S2(101));
            Assert.AreEqual(0, capture.Current.Length);
    
            stmtGraph.Dispose();
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService) {
            DefaultSupportGraphEventUtil.AddTypeConfiguration((EPServiceProviderSPI) epService);
    
            RunAssertionAllTypes(epService, "MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
            RunAssertionAllTypes(epService, "MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionAllTypes(epService, "MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionAllTypes(epService, "MyEvent", DefaultSupportGraphEventUtil.PonoEvents);
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService, string typeName, object[] events) {
            string graph = "create dataflow MySelect\n" +
                    "DefaultSupportSourceOp -> instream<" + typeName + ">{}\n" +
                    "Select(instream as ME) -> outstream {select: (select myString, sum(myInt) as total from ME)}\n" +
                    "DefaultSupportCaptureOp(outstream) {}";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);
    
            var source = new DefaultSupportSourceOp(events);
            var capture = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions();
            options.OperatorProvider(new DefaultSupportGraphOpProvider(source, capture));
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MySelect", options);
    
            instance.Run();
    
            object[] result = capture.GetAndReset()[0].ToArray();
            EPAssertionUtil.AssertPropsPerRow(
                epService.Container, result, "myString,total".Split(','), new object[][]{new object[] {"one", 1}, new object[] {"two", 3}});
    
            instance.Cancel();
    
            stmtGraph.Dispose();
        }
    
        private void RunAssertionSelectPerformance(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventOA(p0 string, p1 long)");
    
            /*
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0, sum(p1) from MyEvent");
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000000; i++) {
                epService.EPRuntime.SendEvent(new object[] {"E1", 1L}, "MyEvent");
            }
            long end = PerformanceObserver.MilliTime;
            Log.Info("delta=" + (end - start) / 1000d);
            Log.Info(stmt.First().Get("sum(p1)"));
            */
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "Emitter -> instream<MyEventOA> {name: 'E1'}" +
                    "Select(instream as ME) -> astream {select: (select p0, sum(p1) from ME)}");
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne");
            Emitter emitter = df.StartCaptive().Emitters.Get("E1");
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1; i++) {
                emitter.Submit(new object[]{"E1", 1L});
            }
            long end = PerformanceObserver.MilliTime;
            //Log.Info("delta=" + (end - start) / 1000d);
        }
    
    }
} // end of namespace
