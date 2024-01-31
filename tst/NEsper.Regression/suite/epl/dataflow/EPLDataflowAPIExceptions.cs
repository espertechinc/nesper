///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.regressionlib.support.epl.SupportStaticMethodLib;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIExceptions : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            MyExceptionHandler.Contexts.Clear();

            // test exception by graph source
            env.CompileDeploy(
                "@name('flow') create dataflow MyDataFlow DefaultSupportSourceOp -> outstream<SupportBean> {}");

            var op = new DefaultSupportSourceOp(new object[] { new EPRuntimeException("My-Exception-Is-Here") });
            var options = new EPDataFlowInstantiationOptions();
            options.WithOperatorProvider(new DefaultSupportGraphOpProvider(op));
            var handler = new MyExceptionHandler();
            options.WithExceptionHandler(handler);
            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow", options);

            df.Start();
            Sleep(100);
            Sleep(10000);
            ClassicAssert.AreEqual(EPDataFlowState.COMPLETE, df.State);

            ClassicAssert.AreEqual(1, MyExceptionHandler.Contexts.Count);
            var context = MyExceptionHandler.Contexts[0];
            ClassicAssert.AreEqual("MyDataFlow", context.DataFlowName);
            ClassicAssert.AreEqual("DefaultSupportSourceOp", context.OperatorName);
            ClassicAssert.AreEqual(0, context.OperatorNumber);
            ClassicAssert.AreEqual("DefaultSupportSourceOp#0() -> outstream<SupportBean>", context.OperatorPrettyPrint);
            ClassicAssert.AreEqual(
                "Support-graph-source generated exception: My-Exception-Is-Here",
                context.Exception.Message);
            df.Cancel();
            env.UndeployModuleContaining("flow");
            MyExceptionHandler.Contexts.Clear();

            // test exception by operator
            env.CompileDeploy(
                "@name('flow') create dataflow MyDataFlow DefaultSupportSourceOp -> outstream<SupportBean> {}" +
                "MyExceptionOp(outstream) {}");

            var opTwo = new DefaultSupportSourceOp(new object[] { new SupportBean("E1", 1) });
            var optionsTwo = new EPDataFlowInstantiationOptions();
            optionsTwo.WithOperatorProvider(new DefaultSupportGraphOpProvider(opTwo));
            var handlerTwo = new MyExceptionHandler();
            optionsTwo.WithExceptionHandler(handlerTwo);
            var dfTwo = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow", optionsTwo);

            dfTwo.Start();
            Sleep(100);

            ClassicAssert.AreEqual(1, MyExceptionHandler.Contexts.Count);
            var contextTwo = MyExceptionHandler.Contexts[0];
            ClassicAssert.AreEqual("MyDataFlow", contextTwo.DataFlowName);
            ClassicAssert.AreEqual("MyExceptionOp", contextTwo.OperatorName);
            ClassicAssert.AreEqual(1, contextTwo.OperatorNumber);
            ClassicAssert.AreEqual("MyExceptionOp#1(outstream)", contextTwo.OperatorPrettyPrint);
            ClassicAssert.AreEqual("Operator-thrown-exception", contextTwo.Exception.Message);
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.DATAFLOW);
        }

        public class MyExceptionHandler : EPDataFlowExceptionHandler
        {
            public static IList<EPDataFlowExceptionContext> Contexts { get; set; } =
                new List<EPDataFlowExceptionContext>();

            public void Handle(EPDataFlowExceptionContext context)
            {
                Contexts.Add(context);
            }
        }

        public class MyExceptionOpForge : DataFlowOperatorForge
        {
            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return NewInstance(typeof(MyExceptionOpFactory));
            }
        }

        public class MyExceptionOpFactory : DataFlowOperatorFactory
        {
            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                return new MyExceptionOp();
            }
        }

        public class MyExceptionOp : DataFlowOperator
        {
            public void OnInput(SupportBean bean)
            {
                throw new EPException("Operator-thrown-exception");
            }
        }
    }
} // end of namespace