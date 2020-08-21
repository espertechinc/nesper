///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.epl.SupportStaticMethodLib;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpBeaconSource
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowBeaconWithBeans());
            execs.Add(new EPLDataflowBeaconVariable());
            execs.Add(new EPLDataflowBeaconFields());
            execs.Add(new EPLDataflowBeaconNoType());
            return execs;
        }

        private static void RunAssertionFields(
            RegressionEnvironment env,
            EventRepresentationChoice representationEnum,
            bool eventbean)
        {
            EPDataFlowInstantiationOptions options;

            var path = new RegressionPath();
            var streamType = eventbean ? "EventBean<MyEvent>" : "MyEvent";
            
            env.CompileDeploy(
                representationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEvent>() +
                "create schema MyEvent(p0 string, p1 long, p2 double)", path);
            
            env.CompileDeploy(
                "@Name('flow') create dataflow MyDataFlowOne " +
                "" +
                "BeaconSource -> BeaconStream<" + streamType + "> " +
                "{" +
                "  iterations : 3," +
                "  p0 : 'abc'," +
                "  p1 : cast(Math.Round(Randomizer.Random() * 10) + 1, long)," +
                "  p2 : 1d," +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}",
                path);

            var future = new DefaultSupportCaptureOp(3, env.Container.LockManager());
            options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
            df.Start();
            object[] output;
            try {
                output = future.GetValue(2, TimeUnit.SECONDS);
            }
            catch (Exception t) {
                throw new EPException(t);
            }

            Assert.AreEqual(3, output.Length);
            for (var i = 0; i < 3; i++) {
                if (!eventbean) {
                    if (representationEnum.IsObjectArrayEvent()) {
                        var row = (object[]) output[i];
                        Assert.AreEqual("abc", row[0]);
                        var val = row[1].AsInt64();
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row[2]);
                    }
                    else if (representationEnum.IsMapEvent()) {
                        var row = (IDictionary<string, object>) output[i];
                        Assert.AreEqual("abc", row.Get("p0"));
                        var val = row.Get("p1").AsInt64();
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row.Get("p2"));
                    }
                    else {
                        var row = (GenericRecord) output[i];
                        Assert.AreEqual("abc", row.Get("p0"));
                        var val = row.Get("p1").AsInt64();
                        Assert.IsTrue(val >= 0 && val <= 11, "val=" + val);
                        Assert.AreEqual(1d, row.Get("p2"));
                    }
                }
                else {
                    var row = (EventBean) output[i];
                    Assert.AreEqual("abc", row.Get("p0"));
                }
            }

            env.UndeployAll();
        }

        private static object RunAssertionBeans(
            RegressionEnvironment env,
            string typeName)
        {
            env.CompileDeploy(
                "@Name('flow') create dataflow MyDataFlowOne " +
                "" +
                "BeaconSource -> BeaconStream<" +
                typeName +
                "> {" +
                "  Myfield : 'abc', iterations : 1" +
                "}" +
                "DefaultSupportCaptureOp(BeaconStream) {}");

            var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));
            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
            df.Start();
            var output = new object[0];
            try {
                output = future.GetValue(2, TimeUnit.SECONDS);
            }
            catch (Exception t) {
                throw new EPException(t);
            }

            Assert.AreEqual(1, output.Length);
            env.UndeployAll();
            return output[0];
        }

        public static string GenerateTagId()
        {
            return "";
        }

        internal class EPLDataflowBeaconWithBeans : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resultLegacy = (MyLegacyEvent) RunAssertionBeans(env, "MyLegacyEvent");
                Assert.AreEqual("abc", resultLegacy.Myfield);

#if NOT_APPLICABLE // Java can by-pass constructor-less objects (kind of)
                var resultNoDefCtor = (MyEventNoDefaultCtor) RunAssertionBeans(env, "MyEventNoDefaultCtor");
                Assert.AreEqual("abc", resultNoDefCtor.Myfield);
#endif
            }
        }

        internal class EPLDataflowBeaconVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create Schema SomeEvent()", path);
                env.CompileDeploy("create variable int var_iterations=3", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "BeaconSource -> BeaconStream<SomeEvent> {" +
                    "  iterations : var_iterations" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}",
                    path);

                var future = new DefaultSupportCaptureOp(3, env.Container.LockManager());
                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                df.Start();
                object[] output;
                try {
                    output = future.GetValue(2, TimeUnit.SECONDS);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                Assert.AreEqual(3, output.Length);
                env.UndeployAll();
            }
        }

        internal class EPLDataflowBeaconFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in new[] {EventRepresentationChoice.AVRO}) {
                    RunAssertionFields(env, rep, true);
                    RunAssertionFields(env, rep, false);
                }

                // test doc samples
                var epl = "@Name('flow') create dataflow MyDataFlow\n" +
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
                          "  // BeaconSource that produces 10 object-array events populating the Price property \n" +
                          "  // with a random value.\n" +
                          "  BeaconSource -> stream.three {\n" +
                          "    iterations : 1,\n" +
                          "    interval : 10, // every 10 seconds\n" +
                          "    initialDelay : 5, // start after 5 seconds\n" +
                          "    price : Randomizer.Random() * 100,\n" +
                          "  }";
                env.CompileDeploy(epl);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow");
                env.UndeployAll();

                // test options-provided beacon field
                var eplMinimal = "@Name('flow') create dataflow MyGraph " +
                                 "BeaconSource -> outstream<SupportBean> {iterations:1} " +
                                 "EventBusSink(outstream) {}";
                env.CompileDeploy(eplMinimal);

                var options = new EPDataFlowInstantiationOptions();
                options.AddParameterURI("BeaconSource/TheString", "E1");
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyGraph", options);

                env.CompileDeploy("@Name('s0') select * from SupportBean").AddListener("s0");
                instance.Run();
                Sleep(200);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "TheString" },
                    new object[] {"E1"});

                // invalid: no output stream
                TryInvalidCompile(
                    env,
                    "create dataflow DF1 BeaconSource {}",
                    "Failed to obtain operator 'BeaconSource': BeaconSource operator requires one output stream but produces 0 streams");

                env.UndeployAll();
            }
        }

        internal class EPLDataflowBeaconNoType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EPDataFlowInstantiationOptions options;
                object[] output;

                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "BeaconSource -> BeaconStream {}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");

                var countExpected = 10;
                var futureAtLeast = new DefaultSupportCaptureOp(countExpected, env.Container.LockManager());
                options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(futureAtLeast));
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                df.Start();
                try {
                    output = futureAtLeast.GetValue(1, TimeUnit.SECONDS);
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                Assert.IsTrue(countExpected <= output.Length);
                df.Cancel();
                env.UndeployAll();

                // BeaconSource with given number of iterations
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowTwo " +
                    "BeaconSource -> BeaconStream {" +
                    "  iterations: 5" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");

                var futureExactTwo = new DefaultSupportCaptureOp(5, env.Container.LockManager());
                options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(futureExactTwo));
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowTwo", options).Start();
                try {
                    output = futureExactTwo.GetValue(1, TimeUnit.SECONDS);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                Assert.AreEqual(5, output.Length);
                env.UndeployAll();

                // BeaconSource with delay
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowThree " +
                    "BeaconSource -> BeaconStream {" +
                    "  iterations: 2," +
                    "  initialDelay: 0.5" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");

                var futureExactThree = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(futureExactThree));
                var start = PerformanceObserver.MilliTime;
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowThree", options).Start();
                try {
                    output = futureExactThree.GetValue(1, TimeUnit.SECONDS);
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                var end = PerformanceObserver.MilliTime;
                Assert.AreEqual(2, output.Length);
                Assert.IsTrue(end - start > 490, "delta=" + (end - start));
                env.UndeployAll();

                // BeaconSource with period
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowFour " +
                    "BeaconSource -> BeaconStream {" +
                    "  interval: 0.5" +
                    "}" +
                    "DefaultSupportCaptureOp(BeaconStream) {}");
                var futureFour = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(futureFour));
                var instance = env.Runtime.DataFlowService.Instantiate(
                    env.DeploymentId("flow"),
                    "MyDataFlowFour",
                    options);
                instance.Start();
                try {
                    output = futureFour.GetValue(2, TimeUnit.SECONDS);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                Assert.AreEqual(2, output.Length);
                instance.Cancel();
                env.UndeployAll();

                // test Beacon with define typed
                var path = new RegressionPath();
                env.CompileDeploy("create objectarray schema MyTestOAType(p1 string)", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowFive " +
                    "BeaconSource -> BeaconStream<MyTestOAType> {" +
                    "  interval: 0.5," +
                    "  p1 : 'abc'" +
                    "}",
                    path);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowFive");
                env.UndeployAll();
            }
        }

        public class MyEventNoDefaultCtor
        {
            public MyEventNoDefaultCtor(
                string someOtherfield,
                int someOtherValue)
            {
            }

            public string Myfield { get; set; }
        }

        [Serializable]
        public class MyLegacyEvent
        {
            public string Myfield { get; set; }
        }
        
        [Serializable]
        public class MyLocalJsonProvidedMyEvent
        {
            public string p0;
            public long p1;
            public double p2;
        }
    }
} // end of namespace