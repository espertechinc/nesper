///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.dataflow;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowTypes
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
WithBeanType(execs);
WithMapType(execs);
            return execs;
        }
public static IList<RegressionExecution> WithMapType(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowMapType());
    return execs;
}public static IList<RegressionExecution> WithBeanType(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLDataflowBeanType());
    return execs;
}
        private static IDictionary<string, object> MakeMap(
            string p0,
            int p1)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("P0", p0);
            map.Put("P1", p1);
            return map;
        }

        internal class EPLDataflowBeanType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SupportBean> {}" +
                    "MySupportBeanOutputOp(outstream) {}" +
                    "SupportGenericOutputOpWPort(outstream) {}");

                var source = new DefaultSupportSourceOp(new object[] {new SupportBean("E1", 1)});
                var outputOne = new MySupportBeanOutputOp();
                var outputTwo = new SupportGenericOutputOpWPort();
                var options =
                    new EPDataFlowInstantiationOptions()
                        .WithOperatorProvider(new DefaultSupportGraphOpProvider(source, outputOne, outputTwo));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                dfOne.Run();

                SupportBean.Compare(
                    outputOne.GetAndReset().ToArray(),
                    new [] { "TheString","IntPrimitive" },
                    new[] {
                        new object[] {"E1", 1}
                    });
                var received = outputTwo.GetAndReset();
                SupportBean.Compare(
                    received.First.ToArray(),
                    new [] { "TheString","IntPrimitive" },
                    new[] {
                        new object[] {"E1", 1}
                    });
                EPAssertionUtil.AssertEqualsExactOrder(new int?[] {0}, received.Second.ToArray());

                env.UndeployAll();
            }
        }

        internal class EPLDataflowMapType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create map schema MyMap (p0 String, p1 int)", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<MyMap> {}" +
                    "MyMapOutputOp(outstream) {}" +
                    "DefaultSupportCaptureOp(outstream) {}",
                    path);

                var source = new DefaultSupportSourceOp(new object[] {MakeMap("E1", 1)});
                var outputOne = new MyMapOutputOp();
                var outputTwo = new DefaultSupportCaptureOp(env.Container.LockManager());
                var options =
                    new EPDataFlowInstantiationOptions()
                        .WithOperatorProvider(new DefaultSupportGraphOpProvider(source, outputOne, outputTwo));
                var dfOne = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                dfOne.Run();

                EPAssertionUtil.AssertPropsPerRow(
                    outputOne.GetAndReset().ToArray(),
                    new[] {"P0", "P1"},
                    new[] {
                        new object[] {"E1", 1}
                    });

                EPAssertionUtil.AssertPropsPerRow(
                    env.Container,
                    outputTwo.GetAndReset()[0].UnwrapIntoArray<object>(),
                    new[] {"P0", "P1"},
                    new[] {
                        new object[] {"E1", 1}
                    });

                env.UndeployAll();
            }
        }

        public class MySupportBeanOutputOp : DataFlowOperatorForge,
            DataFlowOperatorFactory,
            DataFlowOperator
        {
            private IList<SupportBean> received = new List<SupportBean>();

            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new MySupportBeanOutputOp();
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
                return NewInstance(typeof(MySupportBeanOutputOp));
            }

            public void OnInput(SupportBean @event)
            {
                lock (this) {
                    received.Add(@event);
                }
            }

            public IList<SupportBean> GetAndReset()
            {
                lock (this) {
                    var result = received;
                    received = new List<SupportBean>();
                    return result;
                }
            }
        }

        public class MyMapOutputOp : DataFlowOperatorForge,
            DataFlowOperatorFactory,
            DataFlowOperator
        {
            private IList<IDictionary<string, object>> received = new List<IDictionary<string, object>>();

            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new MyMapOutputOp();
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
                return NewInstance(typeof(MyMapOutputOp));
            }

            internal IList<IDictionary<string, object>> GetAndReset()
            {
                lock (this) {
                    var result = received;
                    received = new List<IDictionary<string, object>>();
                    return result;
                }
            }

            public void OnInput(IDictionary<string, object> @event)
            {
                lock (this) {
                    received.Add(@event);
                }
            }
        }
    }
} // end of namespace