///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.dataflow;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIOpLifecycle
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithTypeEvent(execs);
            WithFlowGraphSource(execs);
            With(FlowGraphOperator)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithFlowGraphOperator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowFlowGraphOperator());
            return execs;
        }

        public static IList<RegressionExecution> WithFlowGraphSource(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowFlowGraphSource());
            return execs;
        }

        public static IList<RegressionExecution> WithTypeEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowTypeEvent());
            return execs;
        }

        internal class EPLDataflowTypeEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Compile(
                    "create schema MySchema(key string, value int);\n" +
                    "@name('flow') create dataflow MyDataFlowOne MyCaptureOutputPortOp -> outstream<EventBean<MySchema>> {}");
                ClassicAssert.AreEqual("MySchema", MyCaptureOutputPortOpForge.Port.OptionalDeclaredType.EventType.Name);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        internal class EPLDataflowFlowGraphSource : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportGraphSource.GetAndResetLifecycle();

                var compiled = env.Compile(
                    "@name('flow') create dataflow MyDataFlow @Name('Goodie') @Audit SupportGraphSource -> outstream<SupportBean> {propOne:'abc'}");
                var events = SupportGraphSourceForge.GetAndResetLifecycle();
                ClassicAssert.AreEqual(3, events.Count);
                ClassicAssert.AreEqual("instantiated", events[0]);
                ClassicAssert.AreEqual("SetPropOne=abc", events[1]);
                var forgeCtx = (DataFlowOpForgeInitializeContext)events[2];
                ClassicAssert.AreEqual(0, forgeCtx.InputPorts.Count);
                ClassicAssert.AreEqual(1, forgeCtx.OutputPorts.Count);
                ClassicAssert.AreEqual("outstream", forgeCtx.OutputPorts[0].StreamName);
                ClassicAssert.AreEqual("SupportBean", forgeCtx.OutputPorts[0].OptionalDeclaredType.EventType.Name);
                ClassicAssert.AreEqual(2, forgeCtx.OperatorAnnotations.Length);
                ClassicAssert.AreEqual("Goodie", ((NameAttribute)forgeCtx.OperatorAnnotations[0]).Value);
                ClassicAssert.IsNotNull((AuditAttribute)forgeCtx.OperatorAnnotations[1]);
                ClassicAssert.AreEqual("MyDataFlow", forgeCtx.DataflowName);
                ClassicAssert.AreEqual(0, forgeCtx.OperatorNumber);

                env.Deploy(compiled);
                events = SupportGraphSourceFactory.GetAndResetLifecycle();
                ClassicAssert.AreEqual(3, events.Count);
                ClassicAssert.AreEqual("instantiated", events[0]);
                ClassicAssert.AreEqual("SetPropOne=abc", events[1]);
                var factoryCtx = (DataFlowOpFactoryInitializeContext)events[2];
                ClassicAssert.AreEqual("MyDataFlow", factoryCtx.DataFlowName);
                ClassicAssert.AreEqual(0, factoryCtx.OperatorNumber);
                ClassicAssert.IsNotNull(factoryCtx.StatementContext);

                // instantiate
                var options = new EPDataFlowInstantiationOptions()
                    .WithDataFlowInstanceId("id1")
                    .WithDataFlowInstanceUserObject("myobject");
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow", options);
                events = SupportGraphSourceFactory.GetAndResetLifecycle();
                ClassicAssert.AreEqual(1, events.Count);
                var opCtx = (DataFlowOpInitializeContext)events[0];
                ClassicAssert.AreEqual("MyDataFlow", opCtx.DataFlowName);
                ClassicAssert.AreEqual("id1", opCtx.DataFlowInstanceId);
                ClassicAssert.IsNotNull(opCtx.AgentInstanceContext);
                ClassicAssert.AreEqual("myobject", opCtx.DataflowInstanceUserObject);
                ClassicAssert.AreEqual(0, opCtx.OperatorNumber);
                ClassicAssert.AreEqual("SupportGraphSource", opCtx.OperatorName);

                events = SupportGraphSource.GetAndResetLifecycle();
                ClassicAssert.AreEqual(1, events.Count);
                ClassicAssert.AreEqual("instantiated", events[0]); // instantiated

                // run
                df.Run();

                events = SupportGraphSource.GetAndResetLifecycle();
                ClassicAssert.AreEqual(5, events.Count);
                ClassicAssert.IsTrue(events[0] is DataFlowOpOpenContext); // called open (GraphSource only)
                ClassicAssert.AreEqual("next(numrows=0)", events[1]);
                ClassicAssert.AreEqual("next(numrows=1)", events[2]);
                ClassicAssert.AreEqual("next(numrows=2)", events[3]);
                ClassicAssert.IsTrue(events[4] is DataFlowOpCloseContext); // called close (GraphSource only)

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        internal class EPLDataflowFlowGraphOperator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportGraphSource.GetAndResetLifecycle();

                env.CompileDeploy(
                    "@name('flow') create dataflow MyDataFlow MyLineFeedSource -> outstream {} SupportOperator(outstream) {propOne:'abc'}");
                ClassicAssert.AreEqual(0, SupportOperator.GetAndResetLifecycle().Count);

                // instantiate
                var src = new MyLineFeedSource(Arrays.AsList("abc", "def").GetEnumerator());
                var
                    options = new EPDataFlowInstantiationOptions()
                        .WithOperatorProvider(new DefaultSupportGraphOpProvider(src));
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow", options);

                var events = SupportOperator.GetAndResetLifecycle();
                ClassicAssert.AreEqual(1, events.Count);
                ClassicAssert.AreEqual("instantiated", events[0]); // instantiated

                // run
                df.Run();

                events = SupportOperator.GetAndResetLifecycle();
                ClassicAssert.AreEqual(4, events.Count);
                ClassicAssert.IsTrue(events[0] is DataFlowOpOpenContext); // called open (GraphSource only)
                ClassicAssert.AreEqual("abc", ((object[])events[1])[0]);
                ClassicAssert.AreEqual("def", ((object[])events[2])[0]);
                ClassicAssert.IsTrue(events[3] is DataFlowOpCloseContext); // called close (GraphSource only)

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        public class SupportGraphSourceForge : DataFlowOperatorForge
        {
            private static IList<object> lifecycle = new List<object>();
            private string _propOne;

            public SupportGraphSourceForge()
            {
                lifecycle.Add("instantiated");
            }

            public string PropOne {
                get => _propOne;
                set {
                    lifecycle.Add("SetPropOne=" + value);
                    _propOne = value;
                }
            }

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                lifecycle.Add(context);
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return new SAIFFInitializeBuilder(
                        typeof(SupportGraphSourceFactory),
                        GetType(),
                        "s",
                        parent,
                        symbols,
                        classScope)
                    .Constant("propOne", PropOne)
                    .Build();
            }

            public static IList<object> GetAndResetLifecycle()
            {
                IList<object> copy = new List<object>(lifecycle);
                lifecycle = new List<object>();
                return copy;
            }
        }

        public class SupportGraphSourceFactory : DataFlowOperatorFactory
        {
            private static IList<object> lifecycle = new List<object>();
            private string _propOne;

            public SupportGraphSourceFactory()
            {
                lifecycle.Add("instantiated");
            }

            public string PropOne {
                get => _propOne;
                set {
                    lifecycle.Add("SetPropOne=" + value);
                    _propOne = value;
                }
            }

            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
                lifecycle.Add(context);
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                lifecycle.Add(context);
                return new SupportGraphSource();
            }

            public static IList<object> GetAndResetLifecycle()
            {
                IList<object> copy = new List<object>(lifecycle);
                lifecycle = new List<object>();
                return copy;
            }
        }

        public class SupportGraphSource : DataFlowSourceOperator
        {
            private static IList<object> lifecycle = new List<object>();

            [DataFlowContext] private EPDataFlowEmitter graphContext;

            private int numrows;

            public SupportGraphSource()
            {
                lifecycle.Add("instantiated");
            }

            public void Open(DataFlowOpOpenContext openContext)
            {
                lifecycle.Add(openContext);
            }

            public void Close(DataFlowOpCloseContext closeContext)
            {
                lifecycle.Add(closeContext);
            }

            public void Next()
            {
                lifecycle.Add("next(numrows=" + numrows + ")");
                if (numrows < 2) {
                    numrows++;
                    graphContext.Submit("E" + numrows);
                }
                else {
                    graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                }
            }

            public static IList<object> GetAndResetLifecycle()
            {
                IList<object> copy = new List<object>(lifecycle);
                lifecycle = new List<object>();
                return copy;
            }

            public EPDataFlowEmitter GraphContext {
                set {
                    lifecycle.Add(value);
                    this.graphContext = value;
                }
            }
        }

        public class SupportOperator : DataFlowOperatorLifecycle
        {
            private static IList<object> lifecycle = new List<object>();

            [DataFlowContext] private EPDataFlowEmitter graphContext;

            public SupportOperator()
            {
                lifecycle.Add("instantiated");
            }

            public void Open(DataFlowOpOpenContext openContext)
            {
                lifecycle.Add(openContext);
            }

            public void Close(DataFlowOpCloseContext closeContext)
            {
                lifecycle.Add(closeContext);
            }


            public static IList<object> GetAndResetLifecycle()
            {
                IList<object> copy = new List<object>(lifecycle);
                lifecycle = new List<object>();
                return copy;
            }

            public EPDataFlowEmitter GraphContext {
                get { return this.graphContext; }
                set {
                    lifecycle.Add(value);
                    this.graphContext = value;
                }
            }

            public void OnInput(object abc)
            {
                lifecycle.Add(abc);
            }
        }

        public class SupportOperatorForge : DataFlowOperatorForge
        {
            [DataFlowOpParameter] private string propOne;

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return NewInstance(typeof(SupportOperatorFactory));
            }
        }

        public class SupportOperatorFactory : DataFlowOperatorFactory
        {
            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new SupportOperator();
            }
        }

        public class MyCaptureOutputPortOpForge : DataFlowOperatorForge
        {
            private static DataFlowOpOutputPort port;

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                port = context.OutputPorts[0];
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
#if TYPE_ERASURE_BUG
                return NewInstance("System.Object");
#else
                return NewInstance<VoidDataFlowOperatorFactory>();
#endif
            }

            public static DataFlowOpOutputPort Port {
                get { return port; }
            }
        }
    }
} // end of namespace