///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpSelect
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAllTypes(execs);
            WithDocSamples(execs);
            WithInvalid(execs);
            WithIterateFinalMarker(execs);
            WithOutputRateLimit(execs);
            WithTimeWindowTriggered(execs);
            WithFromClauseJoinOrder(execs);
            WithSelectPerformance(execs);
            WithOuterJoinMultirow(execs);
            WithOpSelectWrapper(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOpSelectWrapper(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowOpSelectWrapper(false));
            execs.Add(new EPLDataflowOpSelectWrapper(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOuterJoinMultirow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowOuterJoinMultirow());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectPerformance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowSelectPerformance());
            return execs;
        }

        public static IList<RegressionExecution> WithFromClauseJoinOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowFromClauseJoinOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindowTriggered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowTimeWindowTriggered());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputRateLimit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowOutputRateLimit());
            return execs;
        }

        public static IList<RegressionExecution> WithIterateFinalMarker(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowIterateFinalMarker());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSamples(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowDocSamples());
            return execs;
        }

        public static IList<RegressionExecution> WithAllTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowAllTypes());
            return execs;
        }

        private class EPLDataflowDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var epl = "@name('flow') create dataflow MyDataFlow\n" +
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
                env.CompileDeploy(epl);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow");
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                TryInvalidCompileGraph(
                    env,
                    "insert into ABC select TheString from ME",
                    false,
                    "Failed to obtain operator 'Select': Insert-into clause is not supported");

                TryInvalidCompileGraph(
                    env,
                    "select irstream TheString from ME",
                    false,
                    "Failed to obtain operator 'Select': Selecting remove-stream is not supported");

                TryInvalidCompileGraph(
                    env,
                    "select TheString from pattern[SupportBean]",
                    false,
                    "Failed to obtain operator 'Select': From-clause must contain only streams and cannot contain patterns or other constructs");

                TryInvalidCompileGraph(
                    env,
                    "select TheString from DUMMY",
                    false,
                    "Failed to obtain operator 'Select': Failed to find stream 'DUMMY' among input ports, input ports are [\"ME\"]");

                TryInvalidCompileGraph(
                    env,
                    "select TheString from ME output every 10 seconds",
                    true,
                    "Failed to obtain operator 'Select': Output rate limiting is not supported with 'iterate'");

                TryInvalidCompileGraph(
                    env,
                    "select (select * from SupportBean#lastevent) from ME",
                    false,
                    "Failed to obtain operator 'Select': Subselects are not supported");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW, RegressionFlag.INVALIDITY);
            }
        }

        private class EPLDataflowIterateFinalMarker : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var graph = "@name('flow') create dataflow MySelect\n" +
                            "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                            "@Audit Select(instream_s0 as ALIAS) -> outstream {\n" +
                            "  select: (select TheString, sum(IntPrimitive) as sumInt from ALIAS group by TheString order by TheString asc),\n" +
                            "  iterate: true" +
                            "}\n" +
                            "DefaultSupportCaptureOp(outstream) {}\n";
                env.AdvanceTime(0);
                env.CompileDeploy(graph);

                var capture = new DefaultSupportCaptureOp(env.Container.LockManager());
                var operators = CollectionUtil.PopulateNameValueMap("DefaultSupportCaptureOp", capture);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(operators));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);
                var captive = instance.StartCaptive();

                var emitter = captive.Emitters.Get("emitterS0");
                emitter.Submit(new SupportBean("E3", 4));
                emitter.Submit(new SupportBean("E2", 3));
                emitter.Submit(new SupportBean("E1", 1));
                emitter.Submit(new SupportBean("E2", 2));
                emitter.Submit(new SupportBean("E1", 5));
                Assert.AreEqual(0, capture.Current.Length);

                emitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());

                EPAssertionUtil.AssertPropsPerRow(
                    env.Container,
                    capture.Current,
                    "TheString,sumInt".SplitCsv(),
                    new object[][] { new object[] { "E1", 6 }, new object[] { "E2", 5 }, new object[] { "E3", 4 } });

                instance.Cancel();
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowOutputRateLimit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var graph = "@name('flow') create dataflow MySelect\n" +
                            "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                            "Select(instream_s0) -> outstream {\n" +
                            "  select: (select sum(IntPrimitive) as sumInt from instream_s0 output snapshot every 1 minute)\n" +
                            "}\n" +
                            "DefaultSupportCaptureOp(outstream) {}\n";
                env.AdvanceTime(0);
                env.CompileDeploy(graph);

                var capture = new DefaultSupportCaptureOp(env.Container.LockManager());
                var operators = CollectionUtil.PopulateNameValueMap("DefaultSupportCaptureOp", capture);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(operators));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);
                var captive = instance.StartCaptive();
                var emitter = captive.Emitters.Get("emitterS0");

                env.AdvanceTime(5000);
                emitter.Submit(new SupportBean("E1", 5));
                emitter.Submit(new SupportBean("E2", 3));
                emitter.Submit(new SupportBean("E3", 6));
                Assert.AreEqual(0, capture.GetCurrentAndReset().Length);

                env.AdvanceTime(60000 + 5000);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "sumInt".SplitCsv(),
                    new object[] { 14 });

                emitter.Submit(new SupportBean("E4", 3));
                emitter.Submit(new SupportBean("E5", 6));
                Assert.AreEqual(0, capture.GetCurrentAndReset().Length);

                env.AdvanceTime(120000 + 5000);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "sumInt".SplitCsv(),
                    new object[] { 14 + 9 });

                instance.Cancel();

                emitter.Submit(new SupportBean("E5", 6));
                env.AdvanceTime(240000 + 5000);
                Assert.AreEqual(0, capture.GetCurrentAndReset().Length);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowTimeWindowTriggered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var graph = "@name('flow') create dataflow MySelect\n" +
                            "Emitter -> instream_s0<SupportBean>{name: 'emitterS0'}\n" +
                            "Select(instream_s0) -> outstream {\n" +
                            "  select: (select sum(IntPrimitive) as sumInt from instream_s0#time(1 minute))\n" +
                            "}\n" +
                            "DefaultSupportCaptureOp(outstream) {}\n";
                env.AdvanceTime(0);
                env.CompileDeploy(graph);

                var capture = new DefaultSupportCaptureOp(env.Container.LockManager());
                var operators = CollectionUtil.PopulateNameValueMap("DefaultSupportCaptureOp", capture);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(operators));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);
                var captive = instance.StartCaptive();

                env.AdvanceTime(5000);
                captive.Emitters.Get("emitterS0").Submit(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "sumInt".SplitCsv(),
                    new object[] { 2 });

                env.AdvanceTime(10000);
                captive.Emitters.Get("emitterS0").Submit(new SupportBean("E2", 5));
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "sumInt".SplitCsv(),
                    new object[] { 7 });

                env.AdvanceTime(65000);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "sumInt".SplitCsv(),
                    new object[] { 5 });

                instance.Cancel();
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowOuterJoinMultirow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var graph = "@name('flow') create dataflow MySelect\n" +
                            "Emitter -> instream_s0<SupportBean_S0>{name: 'emitterS0'}\n" +
                            "Emitter -> instream_s1<SupportBean_S1>{name: 'emitterS1'}\n" +
                            "Select(instream_s0 as S0, instream_s1 as S1) -> outstream {\n" +
                            "  select: (select P00, P10 from S0#keepall full outer join S1#keepall)\n" +
                            "}\n" +
                            "DefaultSupportCaptureOp(outstream) {}\n";
                env.CompileDeploy(graph);

                var capture = new DefaultSupportCaptureOp(env.Container.LockManager());
                var operators = CollectionUtil.PopulateNameValueMap("DefaultSupportCaptureOp", capture);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(operators));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);

                var captive = instance.StartCaptive();

                captive.Emitters.Get("emitterS0").Submit(new SupportBean_S0(1, "S0_1"));
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "P00,P11".SplitCsv(),
                    new object[] { "S0_1", null });

                instance.Cancel();
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowFromClauseJoinOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                TryAssertionJoinOrder(env, "from S2#lastevent as s2, S1#lastevent as s1, S0#lastevent as s0");
                TryAssertionJoinOrder(env, "from S0#lastevent as s0, S1#lastevent as s1, S2#lastevent as s2");
                TryAssertionJoinOrder(env, "from S1#lastevent as s1, S2#lastevent as s2, S0#lastevent as s0");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }

            private void TryAssertionJoinOrder(
                RegressionEnvironment env,
                string fromClause)
            {
                var graph = "@name('flow') create dataflow MySelect\n" +
                            "Emitter -> instream_s0<SupportBean_S0>{name: 'emitterS0'}\n" +
                            "Emitter -> instream_s1<SupportBean_S1>{name: 'emitterS1'}\n" +
                            "Emitter -> instream_s2<SupportBean_S2>{name: 'emitterS2'}\n" +
                            "Select(instream_s0 as S0, instream_s1 as S1, instream_s2 as S2) -> outstream {\n" +
                            "  select: (select s0.Id as s0id, s1.Id as s1id, s2.Id as s2id " +
                            fromClause +
                            ")\n" +
                            "}\n" +
                            "DefaultSupportCaptureOp(outstream) {}\n";
                env.CompileDeploy(graph);

                var capture = new DefaultSupportCaptureOp(env.Container.LockManager());
                var operators = CollectionUtil.PopulateNameValueMap("DefaultSupportCaptureOp", capture);

                var options =
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(
                        new DefaultSupportGraphOpProviderByOpName(operators));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);

                var captive = instance.StartCaptive();
                captive.Emitters.Get("emitterS0").Submit(new SupportBean_S0(1));
                captive.Emitters.Get("emitterS1").Submit(new SupportBean_S1(10));
                Assert.AreEqual(0, capture.Current.Length);

                captive.Emitters.Get("emitterS2").Submit(new SupportBean_S2(100));
                Assert.AreEqual(1, capture.Current.Length);
                EPAssertionUtil.AssertProps(
                    env.Container,
                    capture.GetCurrentAndReset()[0],
                    "s0id,s1id,s2id".SplitCsv(),
                    new object[] { 1, 10, 100 });

                instance.Cancel();

                captive.Emitters.Get("emitterS2").Submit(new SupportBean_S2(101));
                Assert.AreEqual(0, capture.Current.Length);

                env.UndeployAll();
            }
        }

        private class EPLDataflowAllTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                RunAssertionAllTypes(
                    env,
                    DefaultSupportGraphEventUtil.EVENTTYPENAME,
                    DefaultSupportGraphEventUtil.GetPONOEvents());
                RunAssertionAllTypes(env, "MyXMLEvent", DefaultSupportGraphEventUtil.GetXMLEvents());
                RunAssertionAllTypes(env, "MyOAEvent", DefaultSupportGraphEventUtil.GetOAEvents());
                RunAssertionAllTypes(env, "MyMapEvent", DefaultSupportGraphEventUtil.GetMapEvents());
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowOpSelectWrapper : RegressionExecution
        {
            private readonly bool wrapperWithAdditionalProps;

            public EPLDataflowOpSelectWrapper(bool wrapperWithAdditionalProps)
            {
                this.wrapperWithAdditionalProps = wrapperWithAdditionalProps;
            }

            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var epl = "@public @buseventtype create schema A(value int);\n" +
                          (wrapperWithAdditionalProps
                              ? "insert into B select 'a' as hello, * from A; \n"
                              : "insert into B select * from A; \n") +
                          "@name('flow') create dataflow OutputFlow\n" +
                          "  EventBusSource -> TheEvents<B> {}\n" +
                          "  Select(TheEvents) -> outstream {\n" +
                          "    select: (select * from TheEvents)\n" +
                          "  }\n" +
                          "  DefaultSupportCaptureOp(outstream) {}\n";

                env.CompileDeploy(epl);
                var capture = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options = new EPDataFlowInstantiationOptions();
                options.WithOperatorProvider(new DefaultSupportGraphOpProvider(capture));
                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "OutputFlow", options);
                instance.Start();

                env.SendEventMap(Collections.SingletonDataMap("value", 10), "A");
                object[] result;
                try {
                    result = capture.GetValue(1, TimeUnit.SECONDS);
                }
                catch (Exception e) {
                    throw new EPRuntimeException("Timeout: " + e.Message, e);
                }

                if (wrapperWithAdditionalProps) {
                    var underlying = (Pair<object, IDictionary<string, object>>)result[0];
                    Assert.AreEqual("a", underlying.Second.Get("hello"));
                    Assert.AreEqual(10, ((IDictionary<string, object>)underlying.First).Get("value"));
                }
                else {
                    EPAssertionUtil.AssertPropsPerRow(
                        env.Container,
                        result,
                        "value".SplitCsv(),
                        new object[][] { new object[] { 10 } });
                }

                instance.Cancel();

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "wrapperWithAdditionalProps=" +
                       wrapperWithAdditionalProps +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private static void RunAssertionAllTypes(
            RegressionEnvironment env,
            string typeName,
            object[] events)
        {
            var graph = "@name('flow') create dataflow MySelect\n" +
                        "DefaultSupportSourceOp -> instream<" +
                        typeName +
                        ">{}\n" +
                        "Select(instream as ME) -> outstream {select: (select MyString, sum(MyInt) as Total from ME)}\n" +
                        "DefaultSupportCaptureOp(outstream) {}";
            env.CompileDeploy(graph);

            var source = new DefaultSupportSourceOp(events);
            var capture = new DefaultSupportCaptureOp(2, env.Container.LockManager());
            var options = new EPDataFlowInstantiationOptions();
            options.WithOperatorProvider(new DefaultSupportGraphOpProvider(source, capture));
            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);

            instance.Run();

            var result = capture.GetAndReset()[0].ToArray();
            EPAssertionUtil.AssertPropsPerRow(
                env.Container,
                result,
                "MyString,Total".SplitCsv(),
                new object[][] {
                    new object[] { "one", 1 },
                    new object[] { "two", 3 }
                });

            instance.Cancel();

            env.UndeployAll();
        }

        private class EPLDataflowSelectPerformance : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                env.CompileDeploy(
                    "create objectarray schema MyEventOA(p0 string, p1 long);\n" +
                    "@name('flow') create dataflow MyDataFlowOne " +
                    "Emitter -> instream<MyEventOA> {name: 'E1'}" +
                    "Select(instream as ME) -> astream {select: (select p0, sum(p1) from ME)}");
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne");
                var emitter = df.StartCaptive().Emitters.Get("E1");
                var start = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1; i++) {
                    emitter.Submit(new object[] { "E1", 1L });
                }

                var end = PerformanceObserver.MilliTime;
                //Console.WriteLine("delta=" + (end - start) / 1000d);

                env.UndeployAll();
            }
        }

        private static void TryInvalidCompileGraph(
            RegressionEnvironment env,
            string select,
            bool iterate,
            string message)
        {
            var graph = "@name('flow') create dataflow MySelect\n" +
                        "DefaultSupportSourceOp -> instream<SupportBean>{}\n" +
                        "Select(instream as ME) -> outstream {select: (" +
                        select +
                        "), iterate: " +
                        iterate +
                        "}\n" +
                        "DefaultSupportCaptureOp(outstream) {}";
            env.TryInvalidCompile(graph, message);
        }
    }
} // end of namespace