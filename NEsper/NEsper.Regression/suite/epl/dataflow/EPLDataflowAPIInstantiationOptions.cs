///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIInstantiationOptions
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowParameterInjectionCallback());
            execs.Add(new EPLDataflowOperatorInjectionCallback());
            return execs;
        }

        internal class EPLDataflowParameterInjectionCallback : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne MyOp => outstream<SomeType> {propOne:'abc', propThree:'xyz'}",
                    path);

                var options = new EPDataFlowInstantiationOptions();
                var myParameterProvider = new MyParameterProvider(Collections.SingletonDataMap("propTwo", "def"));
                options.WithParameterProvider(myParameterProvider);

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);
                var myOp = MyOp.GetAndClearInstances()[0];
                Assert.AreEqual("abc", myOp.PropOne);
                Assert.AreEqual("def", myOp.PropTwo);

                Assert.AreEqual(3, myParameterProvider.contextMap.Count);
                Assert.IsNotNull(myParameterProvider.contextMap.Get("propOne"));

                var context = myParameterProvider.contextMap.Get("propTwo");
                Assert.AreEqual("propTwo", context.ParameterName);
                Assert.AreEqual("MyOp", context.OperatorName);
                Assert.AreSame(myOp.Factory, context.Factory);
                Assert.AreEqual(0, context.OperatorNum);
                Assert.AreEqual("MyDataFlowOne", context.DataFlowName);

                context = myParameterProvider.contextMap.Get("propThree");
                Assert.AreEqual("propThree", context.ParameterName);
                Assert.AreEqual("MyOp", context.OperatorName);
                Assert.AreSame(myOp.Factory, context.Factory);
                Assert.AreEqual(0, context.OperatorNum);

                env.UndeployAll();
            }
        }

        internal class EPLDataflowOperatorInjectionCallback : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SomeType ()", path);
                env.CompileDeploy(
                    "@Name('flow') create dataflow MyDataFlowOne MyOp => outstream<SomeType> {propOne:'abc', propThree:'xyz'}",
                    path);

                var myOperatorProvider = new MyOperatorProvider();
                var options = new EPDataFlowInstantiationOptions();
                options.WithOperatorProvider(myOperatorProvider);

                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlowOne", options);

                Assert.AreEqual(1, myOperatorProvider.contextMap.Count);
                var context = myOperatorProvider.contextMap.Get("MyOp");
                Assert.AreEqual("MyOp", context.OperatorName);
                Assert.AreEqual("MyDataFlowOne", context.DataFlowName);

                env.UndeployAll();
            }
        }

        public class MyOpForge : DataFlowOperatorForge
        {
            [DataFlowOpParameter] private ExprNode propOne;

            [DataFlowOpParameter] private ExprNode propThree;

            [DataFlowOpParameter] private ExprNode propTwo;

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return new SAIFFInitializeBuilder(typeof(MyOpFactory), GetType(), "myop", parent, symbols, classScope)
                    .Exprnode("propOne", propOne)
                    .Exprnode("propTwo", propTwo)
                    .Exprnode("propThree", propThree)
                    .Build();
            }
        }

        public class MyOpFactory : DataFlowOperatorFactory
        {
            [DataFlowOpParameter] private ExprEvaluator propOne;

            [DataFlowOpParameter] private ExprEvaluator propThree;

            [DataFlowOpParameter] private ExprEvaluator propTwo;

            public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
            {
            }

            public DataFlowOperator Operator(DataFlowOpInitializeContext context)
            {
                var propOneText = DataFlowParameterResolution.ResolveStringOptional("propOne", propOne, context);
                var propTwoText = DataFlowParameterResolution.ResolveStringOptional("propTwo", propTwo, context);
                var propThreeText = DataFlowParameterResolution.ResolveStringOptional("propThree", propThree, context);
                return new MyOp(this, propOneText, propTwoText, propThreeText);
            }

            public void SetPropOne(ExprEvaluator propOne)
            {
                this.propOne = propOne;
            }

            public void SetPropTwo(ExprEvaluator propTwo)
            {
                this.propTwo = propTwo;
            }

            public void SetPropThree(ExprEvaluator propThree)
            {
                this.propThree = propThree;
            }
        }

        public class MyOp : DataFlowSourceOperator
        {
            private static readonly IList<MyOp> INSTANCES = new List<MyOp>();

            public MyOp(
                MyOpFactory factory,
                string propOne,
                string propTwo,
                string propThree)
            {
                Factory = factory;
                PropOne = propOne;
                PropTwo = propTwo;
                PropThree = propThree;
                INSTANCES.Add(this);
            }

            public MyOpFactory Factory { get; }

            public string PropOne { get; }

            public string PropTwo { get; }

            public string PropThree { get; }

            public void Next()
            {
            }

            public void Open(DataFlowOpOpenContext openContext)
            {
            }

            public void Close(DataFlowOpCloseContext openContext)
            {
            }

            public static IList<MyOp> GetAndClearInstances()
            {
                IList<MyOp> ops = new List<MyOp>(INSTANCES);
                INSTANCES.Clear();
                return ops;
            }
        }

        public class MyParameterProvider : EPDataFlowOperatorParameterProvider
        {
            internal readonly IDictionary<string, EPDataFlowOperatorParameterProviderContext> contextMap =
                new Dictionary<string, EPDataFlowOperatorParameterProviderContext>();

            internal readonly IDictionary<string, object> values;

            public MyParameterProvider(IDictionary<string, object> values)
            {
                this.values = values;
            }

            public object Provide(EPDataFlowOperatorParameterProviderContext context)
            {
                contextMap.Put(context.ParameterName, context);
                return values.Get(context.ParameterName);
            }
        }

        public class MyOperatorProvider : EPDataFlowOperatorProvider
        {
            internal readonly IDictionary<string, EPDataFlowOperatorProviderContext> contextMap =
                new Dictionary<string, EPDataFlowOperatorProviderContext>();

            public object Provide(EPDataFlowOperatorProviderContext context)
            {
                contextMap.Put(context.OperatorName, context);
                return new MyOp(null, "test", "test", "test");
            }
        }
    }
} // end of namespace