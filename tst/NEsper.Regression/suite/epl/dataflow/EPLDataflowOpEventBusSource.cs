///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.support.epl.SupportStaticMethodLib; // sleep
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpEventBusSource
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAllTypes(execs);
            WithSchemaObjectArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSchemaObjectArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowSchemaObjectArray());
            return execs;
        }

        public static IList<RegressionExecution> WithAllTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowAllTypes());
            return execs;
        }

        private class EPLDataflowAllTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionAllTypes(
                    env,
                    DefaultSupportGraphEventUtil.EVENTTYPENAME,
                    DefaultSupportGraphEventUtil.GetPONOEventsSendable());
                RunAssertionAllTypes(env, "MyMapEvent", DefaultSupportGraphEventUtil.GetMapEventsSendable());
                RunAssertionAllTypes(env, "MyXMLEvent", DefaultSupportGraphEventUtil.GetXMLEventsSendable());
                RunAssertionAllTypes(env, "MyOAEvent", DefaultSupportGraphEventUtil.GetOAEventsSendable());

                // invalid: no output stream
                env.TryInvalidCompile(
                    "create dataflow DF1 EventBusSource {}",
                    "Failed to obtain operator 'EventBusSource': EventBusSource operator requires one output stream but produces 0 streams");

                // invalid: type not found
                env.TryInvalidCompile(
                    "create dataflow DF1 EventBusSource -> ABC {}",
                    "Failed to obtain operator 'EventBusSource': EventBusSource operator requires an event type declated for the output stream");

                // test doc samples
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema SampleSchema(tagId string, locX double, locY double)", path);
                var epl = "@name('flow') create dataflow MyDataFlow\n" +
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
                          "      class : '" +
                          typeof(MyDummyCollector).FullName +
                          "'\n" +
                          "    },\n" +
                          "  }";
                env.CompileDeploy(epl, path);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private static void RunAssertionAllTypes(
            RegressionEnvironment env,
            string typeName,
            SendableEvent[] events)
        {
            env.CompileDeploy(
                "@name('flow') create dataflow MyDataFlowOne " +
                "EventBusSource -> ReceivedStream<" +
                typeName +
                "> {} " +
                "DefaultSupportCaptureOp(ReceivedStream) {}");

            var future = new DefaultSupportCaptureOp<object>(env.Container.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));
            var eventService = (EventServiceSendEventCommon)env.EventService;

            events[0].Send(eventService);
            Assert.AreEqual(0, future.Current.Length);

            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

            events[0].Send(eventService);
            Assert.AreEqual(0, future.Current.Length);

            df.Start();

            // send events
            for (var i = 0; i < events.Length; i++) {
                events[i].Send(eventService);
            }

            // assert
            future.WaitForInvocation(200, events.Length);
            var rows = future.GetCurrentAndReset();
            Assert.AreEqual(events.Length, rows.Length);
            for (var i = 0; i < events.Length; i++) {
                Assert.AreSame(events[i].Underlying, rows[i]);
            }

            df.Cancel();

            events[0].Send(eventService);
            Sleep(50);
            Assert.AreEqual(0, future.Current.Length);

            env.UndeployAll();
        }

        private class EPLDataflowSchemaObjectArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var compiled = env.Compile(
                    "@public @buseventtype create objectarray schema MyEventOA(p0 string, p1 long)");
                env.Deploy(compiled);
                path.Add(compiled);

                RunAssertionOA(env, path, false);
                RunAssertionOA(env, path, true);

                // test collector
                env.CompileDeploy(
                    "@name('flow') create dataflow MyDataFlowOne " +
                    "EventBusSource -> ReceivedStream<MyEventOA> {filter: p0 like 'A%'} " +
                    "DefaultSupportCaptureOp(ReceivedStream) {}",
                    path);

                var collector = new MyCollector();
                var future = new DefaultSupportCaptureOp<object>(env.Container.LockManager());
                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(future))
                    .WithParameterProvider(
                        new DefaultSupportGraphParamProvider(Collections.SingletonDataMap("collector", collector)));

                var instance = env.Runtime.DataFlowService.Instantiate(
                    env.DeploymentId("flow"),
                    "MyDataFlowOne",
                    options);
                instance.Start();

                env.SendEventObjectArray(new object[] { "B", 100L }, "MyEventOA");
                Sleep(50);
                Assert.IsNull(collector.Last);

                env.SendEventObjectArray(new object[] { "A", 101L }, "MyEventOA");
                future.WaitForInvocation(100, 1);
                Assert.IsNotNull(collector.Last.Emitter);
                Assert.AreEqual("MyEventOA", collector.Last.Event.EventType.Name);
                Assert.AreEqual(false, collector.Last.IsSubmitEventBean);

                instance.Cancel();

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private static void RunAssertionOA(
            RegressionEnvironment env,
            RegressionPath path,
            bool underlying)
        {
            env.CompileDeploy(
                "@name('flow') create dataflow MyDataFlowOne " +
                "EventBusSource -> ReceivedStream<" +
                (underlying ? "MyEventOA" : "EventBean<MyEventOA>") +
                "> {} " +
                "DefaultSupportCaptureOp(ReceivedStream) {}",
                path);

            var future = new DefaultSupportCaptureOp<object>(1, env.Container.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));

            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
            instance.Start();

            env.SendEventObjectArray(new object[] { "abc", 100L }, "MyEventOA");
            var rows = Array.Empty<object>();
            try {
                rows = future.GetValue(1, TimeUnit.SECONDS);
            }
            catch (Exception t) {
                throw new EPRuntimeException(t);
            }

            Assert.AreEqual(1, rows.Length);
            if (underlying) {
                EPAssertionUtil.AssertEqualsExactOrder((object[])rows[0], new object[] { "abc", 100L });
            }
            else {
                EPAssertionUtil.AssertProps((EventBean)rows[0], "p0,p1".SplitCsv(), new object[] { "abc", 100L });
            }

            instance.Cancel();
            env.UndeployModuleContaining("flow");
        }

        public class MyCollector : EPDataFlowEventBeanCollector
        {
            private EPDataFlowEventBeanCollectorContext last;

            public void Collect(EPDataFlowEventBeanCollectorContext context)
            {
                this.last = context;
                context.Emitter.Submit(context.Event);
            }

            public EPDataFlowEventBeanCollectorContext Last => last;
        }

        public class MyDummyCollector : EPDataFlowEventBeanCollector
        {
            public void Collect(EPDataFlowEventBeanCollectorContext context)
            {
            }
        }
    }
} // end of namespace