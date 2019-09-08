///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIStartCaptive : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = new [] { "p0","p1" };

            env.CompileDeploy(
                "@Name('flow') create dataflow MyDataFlow " +
                "Emitter -> outstream<MyOAEventType> {name:'src1'}" +
                "DefaultSupportCaptureOp(outstream) {}");

            var captureOp = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions();
            options.WithOperatorProvider(new DefaultSupportGraphOpProvider(captureOp));

            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow", options);
            var captiveStart = instance.StartCaptive();
            Assert.AreEqual(0, captiveStart.Runnables.Count);
            Assert.AreEqual(1, captiveStart.Emitters.Count);
            var emitter = captiveStart.Emitters.Get("src1");
            Assert.AreEqual(EPDataFlowState.RUNNING, instance.State);

            emitter.Submit(new object[] {"E1", 10});
            EPAssertionUtil.AssertPropsPerRow(
                captureOp.Current.UnwrapIntoArray<EventBean>(),
                fields,
                new[] {new object[] {"E1", 10}});

            emitter.Submit(new object[] {"E2", 20});
            EPAssertionUtil.AssertPropsPerRow(
                captureOp.Current.UnwrapIntoArray<EventBean>(),
                fields,
                new[] {new object[] {"E1", 10}, new object[] {"E2", 20}});

            emitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            EPAssertionUtil.AssertPropsPerRow(
                captureOp.Current.UnwrapIntoArray<EventBean>(),
                fields,
                new object[0][]);
            EPAssertionUtil.AssertPropsPerRow(
                captureOp.GetAndReset()[0].UnwrapIntoArray<EventBean>(),
                fields,
                new[] {new object[] {"E1", 10}, new object[] {"E2", 20}});

            emitter.Submit(new object[] {"E3", 30});
            EPAssertionUtil.AssertPropsPerRow(
                captureOp.Current.UnwrapIntoArray<EventBean>(),
                fields,
                new[] {new object[] {"E3", 30}});

            // stays running until cancelled (no transition to complete)
            Assert.AreEqual(EPDataFlowState.RUNNING, instance.State);

            instance.Cancel();
            Assert.AreEqual(EPDataFlowState.CANCELLED, instance.State);

            env.UndeployAll();

            // test doc sample
            var epl = "@Name('flow') create dataflow HelloWorldDataFlow\n" +
                      "  create schema SampleSchema(text string),\t// sample type\t\t\n" +
                      "\t\n" +
                      "  Emitter -> helloworld.stream<SampleSchema> { name: 'myemitter' }\n" +
                      "  LogSink(helloworld.stream) {}";
            env.CompileDeploy(epl);
            env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "HelloWorldDataFlow");

            env.UndeployAll();
        }
    }
} // end of namespace