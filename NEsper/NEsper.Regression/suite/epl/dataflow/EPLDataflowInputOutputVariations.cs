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
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowInputOutputVariations
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowLargeNumOpsDataFlow());
            execs.Add(new EPLDataflowFanInOut());
            execs.Add(new EPLDataflowFactorial());
            return execs;
        }

        internal class EPLDataflowLargeNumOpsDataFlow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (env.IsHA) {
                    return;
                }

                var epl = "@Name('flow') create dataflow MyGraph \n" +
                          "" +
                          "create objectarray schema SchemaOne (p1 string),\n" +
                          "\n" +
                          "BeaconSource -> InStream<SchemaOne> {p1:'A1', iterations:1}\n" +
                          "Select(InStream) -> out_1 { select: (select p1 from InStream) }\n" +
                          "Select(out_1) -> out_2 { select: (select p1 from out_1) }\n" +
                          "Select(out_2) -> out_3 { select: (select p1 from out_2) }\n" +
                          "Select(out_3) -> out_4 { select: (select p1 from out_3) }\n" +
                          "Select(out_4) -> out_5 { select: (select p1 from out_4) }\n" +
                          "Select(out_5) -> out_6 { select: (select p1 from out_5) }\n" +
                          "Select(out_6) -> out_7 { select: (select p1 from out_6) }\n" +
                          "Select(out_7) -> out_8 { select: (select p1 from out_7) }\n" +
                          "Select(out_8) -> out_9 { select: (select p1 from out_8) }\n" +
                          "Select(out_9) -> out_10 { select: (select p1 from out_9) }\n" +
                          "Select(out_10) -> out_11 { select: (select p1 from out_10) }\n" +
                          "Select(out_11) -> out_12 { select: (select p1 from out_11) }\n" +
                          "Select(out_12) -> out_13 { select: (select p1 from out_12) }\n" +
                          "Select(out_13) -> out_14 { select: (select p1 from out_13) }\n" +
                          "Select(out_14) -> out_15 { select: (select p1 from out_14) }\n" +
                          "Select(out_15) -> out_16 { select: (select p1 from out_15) }\n" +
                          "Select(out_16) -> out_17 { select: (select p1 from out_16) }\n" +
                          "\n" +
                          "DefaultSupportCaptureOp(out_17) {}\n";
                env.CompileDeploy(epl);

                var futureOneA = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                IDictionary<string, object> operators = new Dictionary<string, object>();
                operators.Put("DefaultSupportCaptureOp", futureOneA);

                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyGraph", options).Start();

                object[] result;
                try {
                    result = futureOneA.GetValue(3, TimeUnit.SECONDS);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                EPAssertionUtil.AssertEqualsAnyOrder(new[] {new object[] {"A1"}}, result);

                env.UndeployAll();
            }
        }

        internal class EPLDataflowFanInOut : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('flow') create dataflow MultiInMultiOutGraph \n" +
                          "" +
                          "create objectarray schema SchemaOne (p1 string),\n" +
                          "create objectarray schema SchemaTwo (p2 int),\n" +
                          "\n" +
                          "BeaconSource -> InOne<SchemaOne> {p1:'A1', iterations:1}\n" +
                          "BeaconSource -> InTwo<SchemaOne> {p1:'A2', iterations:1}\n" +
                          "\n" +
                          "BeaconSource -> InThree<SchemaTwo> {p2:10, iterations:1}\n" +
                          "BeaconSource -> InFour<SchemaTwo> {p2:20, iterations:1}\n" +
                          "MyCustomOp((InOne, InTwo) as S0, (InThree, InFour) as S1) -> OutOne<SchemaTwo>, OutTwo<SchemaOne>{}\n" +
                          "\n" +
                          "DefaultSupportCaptureOp(OutOne) { name : 'SupportOpCountFutureOneA' }\n" +
                          "DefaultSupportCaptureOp(OutOne) { name : 'SupportOpCountFutureOneB' }\n" +
                          "DefaultSupportCaptureOp(OutTwo) { name : 'SupportOpCountFutureTwoA' }\n" +
                          "DefaultSupportCaptureOp(OutTwo) { name : 'SupportOpCountFutureTwoB' }\n";
                env.CompileDeploy(epl);

                var futureOneA = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                var futureOneB = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                var futureTwoA = new DefaultSupportCaptureOp(2, env.Container.LockManager());
                var futureTwoB = new DefaultSupportCaptureOp(2, env.Container.LockManager());

                IDictionary<string, object> operators = new Dictionary<string, object>();
                operators.Put("SupportOpCountFutureOneA", futureOneA);
                operators.Put("SupportOpCountFutureOneB", futureOneB);
                operators.Put("SupportOpCountFutureTwoA", futureTwoA);
                operators.Put("SupportOpCountFutureTwoB", futureTwoB);

                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProviderByOpName(operators));

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MultiInMultiOutGraph", options)
                    .Start();

                try {
                    EPAssertionUtil.AssertEqualsAnyOrder(
                        new[] {new object[] {"S1-10"}, new object[] {"S1-20"}},
                        futureOneA.GetValue(3, TimeUnit.SECONDS));
                    EPAssertionUtil.AssertEqualsAnyOrder(
                        new[] {new object[] {"S1-10"}, new object[] {"S1-20"}},
                        futureOneB.GetValue(3, TimeUnit.SECONDS));
                    EPAssertionUtil.AssertEqualsAnyOrder(
                        new[] {new object[] {"S0-A1"}, new object[] {"S0-A2"}},
                        futureTwoA.GetValue(3, TimeUnit.SECONDS));
                    EPAssertionUtil.AssertEqualsAnyOrder(
                        new[] {new object[] {"S0-A1"}, new object[] {"S0-A2"}},
                        futureTwoB.GetValue(3, TimeUnit.SECONDS));
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                env.UndeployAll();
            }
        }

        internal class EPLDataflowFactorial : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('flow') create dataflow FactorialGraph \n" +
                          "" +
                          "create objectarray schema InputSchema (number int),\n" +
                          "create objectarray schema TempSchema (current int, temp long),\n" +
                          "create objectarray schema FinalSchema (result long),\n" +
                          "\n" +
                          "BeaconSource -> InputData<InputSchema> {number:5, iterations:1}\n" +
                          "\n" +
                          "MyFactorialOp(InputData as Input, TempResult as Temp) -> TempResult<TempSchema>, FinalResult<FinalSchema>{}\n" +
                          "\n" +
                          "DefaultSupportCaptureOp(FinalResult) {}\n";
                env.CompileDeploy(epl);

                var future = new DefaultSupportCaptureOp(1, env.Container.LockManager());
                var options = new EPDataFlowInstantiationOptions()
                    .WithOperatorProvider(new DefaultSupportGraphOpProvider(future));

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "FactorialGraph", options).Start();

                object[] result;
                try {
                    result = future.GetValue(3, TimeUnit.SECONDS);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                Assert.AreEqual(1, result.Length);
                Assert.AreEqual((long) 5 * 4 * 3 * 2, ((object[]) result[0])[0]);

                env.UndeployAll();
            }
        }

        public class MyFactorialOp : DataFlowOperatorForge,
            DataFlowOperatorFactory,
            DataFlowOperator
        {
            [DataFlowContext] private EPDataFlowEmitter graphContext;

            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new MyFactorialOp();
            }

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return NewInstance(typeof(MyFactorialOp));
            }

            public void OnInput(int number)
            {
                graphContext.SubmitPort(
                    0,
                    new object[] {number, (long) number});
            }

            public void OnTemp(
                int current,
                long temp)
            {
                if (current == 1) {
                    graphContext.SubmitPort(
                        1,
                        new object[] {temp}); // we are done
                }
                else {
                    current--;
                    var result = temp * current;
                    graphContext.SubmitPort(
                        0,
                        new object[] {current, result});
                }
            }
        }

        public class MyCustomOp : DataFlowOperatorForge,
            DataFlowOperatorFactory,
            DataFlowOperator
        {
            [DataFlowContext] private EPDataFlowEmitter graphContext;

            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new MyCustomOp();
            }

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return NewInstance(typeof(MyCustomOp));
            }

            public void OnS0(string value)
            {
                var output = "S0-" + value;
                graphContext.SubmitPort(
                    1,
                    new object[] {output});
            }

            public void OnS1(int value)
            {
                var output = "S1-" + value;
                graphContext.SubmitPort(
                    0,
                    new object[] {output});
            }
        }
    }
} // end of namespace