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

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    using Map = IDictionary<string, object>;

    public class ExecDataflowOpBeaconSource : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            base.Configure(configuration);
            configuration.AddImport(typeof(MyMath));
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionBeaconWithBeans(epService);
            RunAssertionVariable(epService);
            RunAssertionBeaconFields(epService);
            RunAssertionBeaconNoType(epService);
        }

        private void RunAssertionBeaconWithBeans(EPServiceProvider epService)
        {

            var legacy = new ConfigurationEventTypeLegacy();
            legacy.CodeGeneration = CodeGenerationEnum.DISABLED;
            epService.EPAdministrator.Configuration.AddEventType(
                "MyLegacyEvent", typeof(MyLegacyEvent).AssemblyQualifiedName, legacy);
            var resultLegacy = (MyLegacyEvent) RunAssertionBeans(epService, "MyLegacyEvent");
            Assert.AreEqual("abc", resultLegacy.Myfield);

#if NOT_SUPPORTED_IN_DOTNET
            epService.EPAdministrator.Configuration.AddEventType("MyEventNoDefaultCtor", typeof(MyEventNoDefaultCtor));
            var resultNoDefCtor = (MyEventNoDefaultCtor) RunAssertionBeans(epService, "MyEventNoDefaultCtor");
            Assert.AreEqual("abc", resultNoDefCtor.Myfield);
#endif
        }

        private void RunAssertionVariable(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create Schema SomeEvent()");
            epService.EPAdministrator.CreateEPL("create variable int var_iterations=3");
            var stmtGraph = epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream<SomeEvent> {" +
                "  iterations : var_iterations" +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var future = new DefaultSupportCaptureOp(3, SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            object[] output = future.GetValue(2, TimeUnit.SECONDS);
            Assert.AreEqual(3, output.Length);
            stmtGraph.Dispose();
        }

        private Object RunAssertionBeans(EPServiceProvider epService, string typeName)
        {
            var stmtGraph = epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream<" + typeName + "> {" +
                "  Myfield : 'abc', iterations : 1" +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var future = new DefaultSupportCaptureOp(1, SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            object[] output = future.GetValue(2, TimeUnit.SECONDS);
            Assert.AreEqual(1, output.Length);
            stmtGraph.Dispose();
            return output[0];
        }

        private void RunAssertionBeaconFields(EPServiceProvider epService)
        {
            EnumHelper.ForEach<EventRepresentationChoice>(
                rep =>
                {
                    RunAssertionFields(epService, rep, true);
                    RunAssertionFields(epService, rep, false);
                });

#if NOT_SEEMS_INCORRECT
            foreach (var rep in new EventRepresentationChoice[]{EventRepresentationChoice.AVRO}) {
                RunAssertionFields(epService, rep, true);
                RunAssertionFields(epService, rep, false);
            }
#endif

            // test doc samples
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "generateTagId", GetType(), "GenerateTagId");
            var epl = "create dataflow MyDataFlow\n" +
                      "  create schema SampleSchema(tagId string, locX double, locY double)," +
                      "  " +
                      "  // BeaconSource that produces empty object-array events without delay or interval\n" +
                      "  // until cancelled.\n" +
                      "  BeaconSource -> stream.one {}\n" +
                      "  \n" +
                      "  // BeaconSource that produces one RFIDSchema event populating event properties\n" +
                      "  // from a user-defined function \"generateTagId\" and values.\n" +
                      "  BeaconSource -> stream.two<SampleSchema> {\n" +
                      "    iterations : 1,\n" +
                      "    tagId : generateTagId(),\n" +
                      "    locX : 10,\n" +
                      "    locY : 20 \n" +
                      "  }\n" +
                      "  \n" +
                      "  // BeaconSource that produces 10 object-array events populating the price property \n" +
                      "  // with a random value.\n" +
                      "  BeaconSource -> stream.three {\n" +
                      "    iterations : 1,\n" +
                      "    interval : 10, // every 10 seconds\n" +
                      "    initialDelay : 5, // start after 5 seconds\n" +
                      "    price : MyMath.Random() * 100,\n" +
                      "  }";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");

            // test options-provided beacon field
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var eplMinimal = "create dataflow MyGraph " +
                             "BeaconSource -> outstream<SupportBean> {iterations:1} " +
                             "EventBusSink(outstream) {}";
            epService.EPAdministrator.CreateEPL(eplMinimal);

            var options = new EPDataFlowInstantiationOptions();
            options.AddParameterURI("BeaconSource/TheString", "E1");
            var instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options);

            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean").Events += listener.Update;
            instance.Run();
            Thread.Sleep(200);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[] {"E1"});

            // invalid: no output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(
                epService, "DF1", "create dataflow DF1 BeaconSource {}",
                "Failed to instantiate data flow 'DF1': Failed initialization for operator 'BeaconSource': BeaconSource operator requires one output stream but produces 0 streams");
        }

        private void RunAssertionFields(
            EPServiceProvider epService, EventRepresentationChoice representationEnum, bool eventbean)
        {
            EPDataFlowInstantiationOptions options;

            epService.EPAdministrator.CreateEPL(
                "create " + representationEnum.GetOutputTypeCreateSchemaName() +
                " schema MyEvent(p0 string, p1 long, p2 double)");
            var stmtGraph = epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "" +
                "BeaconSource -> BeaconStream<" + (eventbean ? "EventBean<MyEvent>" : "MyEvent") + "> {" +
                "  iterations : 3," +
                "  p0 : 'abc'," +
                "  p1 : MyMath.Round(MyMath.Random() * 10) + 1," +
                "  p2 : 1d," +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var future = new DefaultSupportCaptureOp(3, SupportContainer.Instance.LockManager());
            options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            var output = future.GetValue(2, TimeUnit.SECONDS);
            Assert.AreEqual(3, output.Length);
            for (var i = 0; i < 3; i++) {

                if (!eventbean) {
                    if (representationEnum.IsObjectArrayEvent()) {
                        var row = (object[]) output[i];
                        Assert.AreEqual("abc", row[0]);
                        var val = (long) row[1];
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row[2]);
                    }
                    else if (representationEnum.IsMapEvent()) {
                        Map row = (Map) output[i];
                        Assert.AreEqual("abc", row.Get("p0"));
                        var val = (long) row.Get("p1");
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row.Get("p2"));
                    }
                    else {
                        var row = (GenericRecord) output[i];
                        Assert.AreEqual("abc", row.Get("p0"));
                        var val = (long) row.Get("p1");
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row.Get("p2"));
                    }
                }
                else {
                    var row = (EventBean) output[i];
                    Assert.AreEqual("abc", row.Get("p0"));
                }
            }

            stmtGraph.Dispose();
            epService.EPAdministrator.Configuration.RemoveEventType("MyEvent", true);
        }

        private void RunAssertionBeaconNoType(EPServiceProvider epService)
        {
            EPDataFlowInstantiationOptions options;
            object[] output;

            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowOne " +
                "BeaconSource -> BeaconStream {}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var countExpected = 10;
            var futureAtLeast = new DefaultSupportCaptureOp(countExpected, SupportContainer.Instance.LockManager());
            options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(futureAtLeast));
            var df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            output = futureAtLeast.GetValue(1, TimeUnit.SECONDS);
            Assert.IsTrue(countExpected <= output.Length);
            df.Cancel();

            // BeaconSource with given number of iterations
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowTwo " +
                "BeaconSource -> BeaconStream {" +
                "  iterations: 5" +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var futureExactTwo = new DefaultSupportCaptureOp(5, SupportContainer.Instance.LockManager());
            options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(futureExactTwo));
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowTwo", options).Start();
            output = futureExactTwo.GetValue(1, TimeUnit.SECONDS);
            Assert.AreEqual(5, output.Length);

            // BeaconSource with delay
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowThree " +
                "BeaconSource -> BeaconStream {" +
                "  iterations: 2," +
                "  initialDelay: 0.5" +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var futureExactThree = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(futureExactThree));
            var start = PerformanceObserver.MilliTime;
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowThree", options).Start();
            output = futureExactThree.GetValue(1, TimeUnit.SECONDS);
            var end = PerformanceObserver.MilliTime;
            Assert.AreEqual(2, output.Length);
            Assert.IsTrue(end - start > 490, "delta=" + (end - start));

            // BeaconSource with period
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowFour " +
                "BeaconSource -> BeaconStream {" +
                "  interval: 0.5" +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var futureFour = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            options = new EPDataFlowInstantiationOptions()
                .OperatorProvider(new DefaultSupportGraphOpProvider(futureFour));
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowFour", options).Start();
            output = futureFour.GetValue(1, TimeUnit.SECONDS);
            Assert.AreEqual(2, output.Length);

            // test Beacon with define typed
            epService.EPAdministrator.CreateEPL("create objectarray schema MyTestOAType(p1 string)");
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyDataFlowFive " +
                "BeaconSource -> BeaconStream<MyTestOAType> {" +
                "  interval: 0.5," +
                "  p1 : 'abc'" +
                "}");
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowFive");
        }

        public static string GenerateTagId()
        {
            return "";
        }

        public class MyMath
        {
            public static readonly Random RandomInstance = new Random();

            public static double Random()
            {
                return RandomInstance.NextDouble();
            }

            public static long Round(double value)
            {
                return (long) Math.Round(value);
            }
        }

        public class MyEventNoDefaultCtor
        {
            public string Myfield { get; set; }
            public MyEventNoDefaultCtor(string someOtherfield, int someOtherValue) { }
        }

        public class MyLegacyEvent
        {
            public string Myfield { get; set; }
        }
    }
} // end of namespace
