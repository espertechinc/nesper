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
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeExceptionHandler
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With(RuntimeExHandlerInvalidAgg)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithRuntimeExHandlerInvalidAgg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeExHandlerInvalidAgg());
            return execs;
        }

        private class ClientRuntimeExHandlerInvalidAgg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('ABCName') select myinvalidagg() from SupportBean";
                env.CompileDeploy(epl);

                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (EPException) {
                    /* expected */
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        public class ClientRuntimeExHandlerGetContext
        {
            public void Run(Configuration configuration)
            {
                SupportExceptionHandlerFactory.FactoryContexts.Clear();
                SupportExceptionHandlerFactory.Handlers.Clear();
                configuration.Runtime.ExceptionHandling.HandlerFactories.Clear();
                configuration.Runtime.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactory));
                configuration.Runtime.ExceptionHandling.AddClass(typeof(SupportExceptionHandlerFactory));
                configuration.Common.AddEventType(typeof(SupportBean));
                configuration.Compiler.AddPlugInAggregationFunctionForge(
                    "myinvalidagg",
                    typeof(SupportInvalidAggregationFunctionForge));

                var runtimeProvider = new EPRuntimeProvider();
                var runtime = runtimeProvider.GetRuntimeInstance(
                    typeof(ClientRuntimeExHandlerGetContext).FullName,
                    configuration);

                SupportExceptionHandlerFactory.FactoryContexts.Clear();
                SupportExceptionHandlerFactory.Handlers.Clear();
                runtime.Initialize();

                var epl = "@name('ABCName') select myinvalidagg() from SupportBean";
                EPDeployment deployment;
                try {
                    var compiled = EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(configuration));
                    deployment = runtime.DeploymentService.Deploy(compiled);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                var contexts = SupportExceptionHandlerFactory.FactoryContexts;
                ClassicAssert.AreEqual(2, contexts.Count);
                ClassicAssert.AreEqual(runtime.URI, contexts[0].RuntimeURI);
                ClassicAssert.AreEqual(runtime.URI, contexts[1].RuntimeURI);

                var handlerOne = SupportExceptionHandlerFactory.Handlers[0];
                var handlerTwo = SupportExceptionHandlerFactory.Handlers[1];
                runtime.EventService.SendEventBean(new SupportBean(), "SupportBean");

                ClassicAssert.AreEqual(1, handlerOne.Contexts.Count);
                ClassicAssert.AreEqual(1, handlerTwo.Contexts.Count);
                var ehc = handlerOne.Contexts[0];
                ClassicAssert.AreEqual(runtime.URI, ehc.RuntimeURI);
                ClassicAssert.AreEqual(epl, ehc.Epl);
                ClassicAssert.AreEqual(deployment.DeploymentId, ehc.DeploymentId);
                ClassicAssert.AreEqual("ABCName", ehc.StatementName);
                ClassicAssert.AreEqual("Sample exception", ehc.Exception.Message);
                ClassicAssert.IsNotNull(ehc.CurrentEvent);

                runtime.Destroy();
            }
        }

        public class ClientRuntimeExceptionHandlerNoHandler
        {
            public void Run(Configuration configuration)
            {
                configuration.Runtime.ExceptionHandling.HandlerFactories.Clear();
                configuration.Compiler.AddPlugInAggregationFunctionForge(
                    "myinvalidagg",
                    typeof(SupportInvalidAggregationFunctionForge));
                configuration.Common.AddEventType(typeof(SupportBean));

                var runtimeProvider = new EPRuntimeProvider();
                var runtime = runtimeProvider.GetRuntimeInstance(
                    nameof(ClientRuntimeExceptionHandlerNoHandler),
                    configuration);

                var epl = "@name('ABCName') select myinvalidagg() from SupportBean";
                EPDeployment deployment;
                try {
                    var compiled = EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(configuration));
                    deployment = runtime.DeploymentService.Deploy(compiled);
                }
                catch (Exception t) {
                    throw new EPException(t);
                }

                runtime.EventService.SendEventBean(new SupportBean(), "SupportBean");

                runtime.Destroy();
            }
        }
    }
} // end of namespace