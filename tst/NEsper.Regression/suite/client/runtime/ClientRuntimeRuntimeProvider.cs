///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeRuntimeProvider
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withk(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withk(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ClientRuntimeObtainEngineWideRWLock());
            return execs;
        }

        internal class ClientRuntimeObtainEngineWideRWLock : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                using (env.Runtime.RuntimeInstanceWideLock.WriteLock.Acquire()) {
                    // some action here
                }
            }
        }

        public class ClientRuntimeRuntimeStateChange
        {
            public void Run(Configuration config)
            {
                var listener = new SupportRuntimeStateListener();
                var runtime = EPRuntimeProvider.GetRuntime(GetType().Name + "__listenerstatechange", config);
                runtime.AddRuntimeStateListener(listener);
                runtime.Destroy();
                Assert.AreSame(runtime, listener.AssertOneGetAndResetDestroyedEvents());

                runtime.Initialize();
                Assert.AreSame(runtime, listener.AssertOneGetAndResetInitializedEvents());

                runtime.RemoveAllRuntimeStateListeners();
                runtime.Initialize();
                Assert.IsTrue(listener.InitializedEvents.IsEmpty());

                runtime.AddRuntimeStateListener(listener);
                var listenerTwo = new SupportRuntimeStateListener();
                runtime.AddRuntimeStateListener(listenerTwo);
                runtime.Initialize();
                Assert.AreSame(runtime, listener.AssertOneGetAndResetInitializedEvents());
                Assert.AreSame(runtime, listenerTwo.AssertOneGetAndResetInitializedEvents());

                Assert.IsTrue(runtime.RemoveRuntimeStateListener(listener));
                runtime.Initialize();
                Assert.AreSame(runtime, listenerTwo.AssertOneGetAndResetInitializedEvents());
                Assert.IsTrue(listener.InitializedEvents.IsEmpty());

                runtime.Destroy();
            }
        }

        public class ClientRuntimeRuntimeDestroy
        {
            public void Run(Configuration config)
            {
                var uriOne = GetType().Name + "_1";
                var runtimeOne = EPRuntimeProvider.GetRuntime(uriOne, config);
                var uriTwo = GetType().Name + "_2";
                var runtimeTwo = EPRuntimeProvider.GetRuntime(uriTwo, config);
                EPAssertionUtil.AssertContains(EPRuntimeProvider.RuntimeURIs, uriOne, uriTwo);
                Assert.IsNotNull(EPRuntimeProvider.GetExistingRuntime(uriOne));
                Assert.IsNotNull(EPRuntimeProvider.GetExistingRuntime(uriTwo));

                config.Common.AddEventType(typeof(SupportBean));
                EPCompiled compiled;
                try {
                    compiled = EPCompilerProvider.Compiler.Compile(
                        "select * from SupportBean",
                        new CompilerArguments(config));
                }
                catch (EPCompileException e) {
                    throw new EPException(e);
                }

                var adminOne = runtimeOne.DeploymentService;
                runtimeOne.Destroy();
                EPAssertionUtil.AssertNotContains(EPRuntimeProvider.RuntimeURIs, uriOne);
                EPAssertionUtil.AssertContains(EPRuntimeProvider.RuntimeURIs, uriTwo);
                Assert.IsNull(EPRuntimeProvider.GetExistingRuntime(uriOne));
                Assert.IsTrue(runtimeOne.IsDestroyed);
                Assert.IsFalse(runtimeTwo.IsDestroyed);

                var stageTwo = runtimeTwo.StageService;
                runtimeTwo.Destroy();
                EPAssertionUtil.AssertNotContains(EPRuntimeProvider.RuntimeURIs, uriOne, uriTwo);
                Assert.IsNull(EPRuntimeProvider.GetExistingRuntime(uriTwo));
                Assert.IsTrue(runtimeOne.IsDestroyed);
                Assert.IsTrue(runtimeTwo.IsDestroyed);

                Assert.That(
                    () => adminOne.Rollout(Collections.SingletonList(new EPDeploymentRolloutCompiled(compiled))),
                    Throws.InstanceOf<EPRuntimeDestroyedException>());

                Assert.That(
                    () => adminOne.Deploy(compiled),
                    Throws.InstanceOf<EPRuntimeDestroyedException>());

                EPAssertionUtil.AssertNotContains(EPRuntimeProvider.RuntimeURIs, uriTwo);

                TryAssertDestroyed(() => DoNothing(runtimeTwo.EventService));
                TryAssertDestroyed(() => DoNothing(runtimeTwo.DeploymentService));
                TryAssertDestroyed(() => DoNothing(runtimeTwo.StageService));
                TryAssertDestroyed(() => stageTwo.GetStage("x"));
                TryAssertDestroyed(() => stageTwo.GetExistingStage("x"));
                TryAssertDestroyed(() => DoNothing(stageTwo.StageURIs));
            }

            public static void DoNothing(object value)
            {
            }
        }

        public class ClientRuntimeMicrosecondInvalid
        {
            public void Run(Configuration config)
            {
                config.Runtime.Threading.IsInternalTimerEnabled = true;
                config.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;

                try {
                    EPRuntimeProvider.GetRuntime(GetType().Name, config).Initialize();
                    Assert.Fail();
                }
                catch (ConfigurationException ex) {
                    SupportMessageAssertUtil.AssertMessage(ex, "Internal timer requires millisecond time resolution");
                }

                config.Runtime.Threading.IsInternalTimerEnabled = false;
                var runtime = EPRuntimeProvider.GetRuntime(GetType().Name, config);

                try {
                    runtime.EventService.ClockInternal();
                    Assert.Fail();
                }
                catch (EPException ex) {
                    SupportMessageAssertUtil.AssertMessage(ex, "Internal timer requires millisecond time resolution");
                }

                runtime.Destroy();
            }
        }

        private static void TryAssertDestroyed(Runnable r)
        {
            Assert.That(() => r.Invoke(), Throws.Exception.InstanceOf<EPRuntimeDestroyedException>());
        }
    }
} // end of namespace