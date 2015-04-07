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

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestDataFlowOpBeaconSource
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddImport(typeof(MyMath));

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestBeaconWithBeans()
        {
            var legacy = new ConfigurationEventTypeLegacy();
            legacy.CodeGeneration = CodeGenerationEnum.DISABLED;
            _epService.EPAdministrator.Configuration.AddEventType("MyLegacyEvent", typeof(MyLegacyEvent).FullName, legacy);
            var resultLegacy = (MyLegacyEvent) RunAssertionBeans("MyLegacyEvent");
            Assert.AreEqual("abc", resultLegacy.Myfield);
    
#if NOT_SUPPORTED_IN_DOTNET
            _epService.EPAdministrator.Configuration.AddEventType("MyEventNoDefaultCtor", typeof(MyEventNoDefaultCtor));
            var resultNoDefCtor = (MyEventNoDefaultCtor) RunAssertionBeans("MyEventNoDefaultCtor");
            Assert.AreEqual("abc", resultNoDefCtor.Myfield);
#endif
        }
    
        private Object RunAssertionBeans(String typeName) {
            var stmtGraph = _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "" +
                    "BeaconSource -> BeaconStream<" + typeName + "> {" +
                    "  Myfield : 'abc', iterations : 1" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
    
            var future = new DefaultSupportCaptureOp(1);
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            var output = future.GetValue(TimeSpan.FromSeconds(2));
            Assert.AreEqual(1, output.Length);
            stmtGraph.Dispose();
            return output[0];
        }
    
        [Test]
        public void TestBeaconFields()
        {
            RunAssertionFields(EventRepresentationEnum.MAP, true);
            RunAssertionFields(EventRepresentationEnum.OBJECTARRAY, true);
            RunAssertionFields(EventRepresentationEnum.MAP, false);
            RunAssertionFields(EventRepresentationEnum.OBJECTARRAY, false);
    
            // test doc samples
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("GenerateTagId", GetType().FullName, "GenerateTagId");
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
                    "    tagId : GenerateTagId(),\n" +
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
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
    
            // test options-provided beacon field
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            var eplMinimal = "create dataflow MyGraph " +
                    "BeaconSource -> outstream<SupportBean> {iterations:1} " +
                    "EventBusSink(outstream) {}";
            _epService.EPAdministrator.CreateEPL(eplMinimal);
    
            var options = new EPDataFlowInstantiationOptions();
            options.AddParameterURI("BeaconSource/TheString", "E1");
            var instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options);
    
            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select * from SupportBean").Events += listener.Update;
            instance.Run();
            Thread.Sleep(200);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[]{"E1"});
    
            // invalid: no output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "DF1", "create dataflow DF1 BeaconSource {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'BeaconSource': BeaconSource operator requires one output stream but produces 0 streams");
        }
    
        private void RunAssertionFields(EventRepresentationEnum representationEnum, bool eventbean)
        {
            EPDataFlowInstantiationOptions options;
    
            _epService.EPAdministrator.CreateEPL("create " + representationEnum.GetOutputTypeCreateSchemaName() + " schema MyEvent(p0 string, p1 long, p2 double)");
            var stmtGraph = _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "" +
                    "BeaconSource -> BeaconStream<" + (eventbean ? "EventBean<MyEvent>" : "MyEvent") + "> {" +
                    "  iterations : 3," +
                    "  p0 : 'abc'," +
                    "  p1 : MyMath.Round(MyMath.Random() * 10) + 1," +
                    "  p2 : 1d," +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
    
            var future = new DefaultSupportCaptureOp(3);
            options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            Object[] output = future.GetValue(TimeSpan.FromSeconds(2));
            Assert.AreEqual(3, output.Length);
            for (var i = 0; i < 3; i++) {
    
                if (!eventbean) {
                    if (representationEnum.IsObjectArrayEvent()) {
                        var row = (Object[]) output[i];
                        Assert.AreEqual("abc", row[0]);
                        long val = row[1].AsLong();
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row[2]);
                    }
                    else {
                        var row = (DataMap) output[i];
                        Assert.AreEqual("abc", row.Get("p0"));
                        long val = row.Get("p1").AsLong();
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
            _epService.EPAdministrator.Configuration.RemoveEventType("MyEvent", true);
        }
    
        [Test]
        public void TestBeaconNoType()
        {
            EPDataFlowInstantiationOptions options;
            Object[] output;
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "BeaconSource -> BeaconStream {}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
    
            var countExpected = 10;
            var futureAtLeast = new DefaultSupportCaptureOp(countExpected);
            options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(futureAtLeast));
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
            output = futureAtLeast.GetValue(TimeSpan.FromSeconds(1));
            Assert.IsTrue(countExpected <= output.Length);
            df.Cancel();
    
            // BeaconSource with given number of iterations
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowTwo " +
                    "BeaconSource -> BeaconStream {" +
                    "  iterations: 5" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
    
            var futureExactTwo = new DefaultSupportCaptureOp(5);
            options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(futureExactTwo));
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowTwo", options).Start();
            output = futureExactTwo.GetValue(TimeSpan.FromSeconds(1));
            Assert.AreEqual(5, output.Length);
    
            // BeaconSource with delay
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowThree " +
                    "BeaconSource -> BeaconStream {" +
                    "  iterations: 2," +
                    "  initialDelay: 0.5" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
    
            var futureExactThree = new DefaultSupportCaptureOp(2);
            options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(futureExactThree));
            long start = Environment.TickCount;
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowThree", options).Start();
            output = futureExactThree.GetValue(TimeSpan.FromSeconds(1));
            long end = Environment.TickCount;
            Assert.AreEqual(2, output.Length);
            Assert.IsTrue(end - start > 490, "delta=" + (end - start));
    
            // BeaconSource with period
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowFour " +
                    "BeaconSource -> BeaconStream {" +
                    "  interval: 0.5" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
    
            var futureFour = new DefaultSupportCaptureOp(2);
            options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(futureFour));
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowFour", options).Start();
            output = futureFour.GetValue(TimeSpan.FromSeconds(1));
            Assert.AreEqual(2, output.Length);
    
            // test Beacon with define typed
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyTestOAType(p1 string)");
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowFive " +
                    "BeaconSource -> BeaconStream<MyTestOAType> {" +
                    "  interval: 0.5," +
                    "  p1 : 'abc'" +
                    "}");
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowFive");
        }
    
        public static String GenerateTagId()
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
                return (long)Math.Round(value);
            }
        }
        
        public class MyEventNoDefaultCtor
        {
            public MyEventNoDefaultCtor(String someOtherfield, int someOtherValue)
            {
            }

            public string Myfield { get; set; }
        }

        public class MyLegacyEvent
        {
            public string Myfield { get; set; }
        }
    }
}
