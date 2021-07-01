///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportCompileDeployUtil
    {
        public static void ThreadSleep(int time)
        {
            try {
                Thread.Sleep(time);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        public static void ExecutorAwait(
            IExecutorService executor,
            int num,
            TimeUnit unit)
        {
            try {
                executor.AwaitTermination(TimeUnitHelper.ToTimeSpan(num, unit));
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        public static void ExecutorAwait(
            IExecutorService executor,
            TimeSpan timeout)
        {
            try {
                executor.AwaitTermination(timeout);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }
        }

        public static void AssertFutures<T>(IList<IFuture<T>> futures)
        {
            try {
                futures.ForEach(
                    future => {
                        Assert.That(
                            future.GetValue(TimeSpan.FromSeconds(10)),
                            Is.True);
                    });
            }
            catch (Exception t) {
                throw new EPException(t);
            }
        }

        public static EPDeployment CompileDeploy(
            EPRuntime runtime,
            string epl)
        {
            try {
                var configuration = runtime.ConfigurationDeepCopy;
                var args = new CompilerArguments(configuration);
                args.Path.Add(runtime.RuntimePath);
                var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
                return runtime.DeploymentService.Deploy(compiled);
            }
            catch (Exception ex) {
                throw new EPException(ex);
            }
        }

        public static EPDeployment CompileDeploy(
            string epl,
            EPRuntime runtime,
            Configuration configuration)
        {
            var compiled = Compile(epl, configuration, new RegressionPath());
            try {
                return runtime.DeploymentService.Deploy(compiled);
            }
            catch (Exception t) {
                throw new EPException(t);
            }
        }

        public static EPCompiled Compile(
            string epl,
            Configuration configuration,
            RegressionPath path)
        {
            try {
                return EPCompilerProvider.Compiler
                    .Compile(
                        epl,
                        new CompilerArguments(configuration)
                            .SetPath(new CompilerPath().AddAll(path.Compileds))
                            .SetOptions(
                                new CompilerOptions()
                                    .SetAccessModifierContext(ctx => NameAccessModifier.PUBLIC)
                                    .SetAccessModifierEventType(ctx => NameAccessModifier.PUBLIC)
                            ));
            }
            catch (Exception t) {
                throw new EPException(t);
            }
        }

        public static EPDeployment Deploy(
            EPCompiled compiledStmt,
            EPRuntime runtime)
        {
            try {
                return runtime.DeploymentService.Deploy(compiledStmt);
            }
            catch (EPDeployException e) {
                throw new EPException(e);
            }
        }

        public static void DeployAddListener(
            EPCompiled compiledStmt,
            string stmtName,
            UpdateListener listener,
            EPRuntime runtime)
        {
            try {
                var deployed = runtime.DeploymentService.Deploy(
                    compiledStmt,
                    new DeploymentOptions().WithStatementNameRuntime(ctx => stmtName));
                if (deployed.Statements.Length != 1) {
                    throw new UnsupportedOperationException("This method is designed for a single statement");
                }

                deployed.Statements[0].AddListener(listener);
            }
            catch (EPDeployException e) {
                throw new EPException(e);
            }
        }

        public static void CompileDeployAddListener(
            string epl,
            string stmtName,
            UpdateListener listener,
            EPRuntime runtime,
            Configuration configuration)
        {
            var compiled = Compile(epl, configuration, new RegressionPath());
            DeployAddListener(compiled, stmtName, listener, runtime);
        }

        public static void ThreadJoin(Thread t)
        {
            try {
                t.Join();
            }
            catch (ThreadInterruptedException ex) {
                throw new EPException(ex);
            }
        }
    }
} // end of namespace