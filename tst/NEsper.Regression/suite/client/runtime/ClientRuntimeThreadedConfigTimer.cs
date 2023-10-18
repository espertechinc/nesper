///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeThreadedConfigTimer : RegressionExecutionWithConfigure
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool EnableHATest => false;
        public bool HAWithCOnly => false;

        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Compiler.Expression.UdfCache = false;
            configuration.Runtime.Threading.IsThreadPoolTimerExec = true;
            configuration.Runtime.Threading.ThreadPoolTimerExecNumThreads = 5;
            configuration.Common.AddEventType("MyMap", new Dictionary<string, object>());
            configuration.Common.AddImportNamespace(typeof(SupportStaticMethodLib));
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.OBSERVEROPS);
        }
        
        public void Run(RegressionEnvironment env)
        {
            SendTimer(0, env);

            log.Debug("Creating statements");
            var countStatements = 100;
            var listener = new SupportListenerTimerHRes();
            var compiled = env.Compile(
                "select SupportStaticMethodLib.Sleep(10) from pattern[every MyMap -> timer:interval(1)]");
            for (var i = 0; i < countStatements; i++) {
                var stmtName = "s" + i;
                env.Deploy(compiled, new DeploymentOptions().WithStatementNameRuntime(ctx => stmtName));
                env.Statement(stmtName).AddListener(listener);
            }

            log.Info("Sending trigger event");
            env.SendEventMap(new Dictionary<string, object>(), "MyMap");

            var start = PerformanceObserver.NanoTime;
            SendTimer(1000, env);
            var end = PerformanceObserver.NanoTime;
            var delta = (end - start) / 1000000;
            Assert.That(delta, Is.LessThan(100), "Delta is " + delta);

            // wait for delivery
            while (true) {
                var countDelivered = listener.NewEvents.Count;
                if (countDelivered == countStatements) {
                    break;
                }

                log.Info("Delivered " + countDelivered + ", waiting for more");
                try {
                    Thread.Sleep(200);
                }
                catch (ThreadInterruptedException e) {
                    throw new EPException(e);
                }
            }

            Assert.AreEqual(100, listener.NewEvents.Count);
            // analyze result
            //List<Pair<Long, EventBean[]>> events = listener.getNewEvents();
            //OccuranceResult result = OccuranceAnalyzer.analyze(events, new long[] {100 * 1000 * 1000L, 10*1000 * 1000L});
            //log.info(result);
        }

        private void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
        }
    }
} // end of namespace