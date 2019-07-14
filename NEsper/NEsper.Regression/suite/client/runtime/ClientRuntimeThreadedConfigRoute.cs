///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeThreadedConfigRoute : RegressionExecutionWithConfigure
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = true;
            configuration.Compiler.Expression.UdfCache = false;
            configuration.Runtime.Threading.IsThreadPoolRouteExec = true;
            configuration.Runtime.Threading.ThreadPoolRouteExecNumThreads = 5;
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
            configuration.Common.AddImportNamespace(typeof(SupportStaticMethodLib).Name);
        }

        public bool EnableHATest => false;
        public bool HAWithCOnly => false;

        public void Run(RegressionEnvironment env)
        {
            log.Debug("Creating statements");
            var countStatements = 100;
            var listener = new SupportListenerTimerHRes();
            var compiled = env.Compile("select SupportStaticMethodLib.sleep(10) from SupportBean");
            for (var i = 0; i < countStatements; i++) {
                var stmtName = "s" + i;
                env.Deploy(compiled, new DeploymentOptions().WithStatementNameRuntime(ctx => stmtName));
                env.Statement(stmtName).AddListener(listener);
            }

            log.Info("Sending trigger event");
            var start = PerformanceObserver.NanoTime;
            env.SendEventBean(new SupportBean());
            var end = PerformanceObserver.NanoTime;
            var delta = (end - start) / 1000000;
            Assert.IsTrue(delta < 100, "Delta is " + delta);

            try {
                Thread.Sleep(2000);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }

            Assert.AreEqual(100, listener.NewEvents.Count);
            listener.NewEvents.Clear();

            // destroy all statements
            env.UndeployAll();

            env.CompileDeploy("@Name('s0') select SupportStaticMethodLib.sleep(10) from SupportBean, SupportBean");
            env.Statement("s0").AddListener(listener);
            env.SendEventBean(new SupportBean());
            try {
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }

            Assert.AreEqual(1, listener.NewEvents.Count);

            env.UndeployAll();
        }
    }
} // end of namespace