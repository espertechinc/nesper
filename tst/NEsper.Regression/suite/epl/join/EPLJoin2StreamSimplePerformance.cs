///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamSimplePerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPerformanceJoinNoResults(execs);
            WithJoinPerformanceStreamA(execs);
            WithJoinPerformanceStreamB(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithJoinPerformanceStreamB(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinPerformanceStreamB());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinPerformanceStreamA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinPerformanceStreamA());
            return execs;
        }

        public static IList<RegressionExecution> WithPerformanceJoinNoResults(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerformanceJoinNoResults());
            return execs;
        }

        private class EPLJoinPerformanceJoinNoResults : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env);
                var methodName = ".testPerformanceJoinNoResults";

                // Send events for each stream
                log.Info($"{methodName} Preloading events");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    SendEvent(env, MakeMarketEvent($"IBM_{i}"));
                    SendEvent(env, MakeSupportEvent($"CSCO_{i}"));
                }

                log.Info($"{methodName} Done preloading");

                var endTime = PerformanceObserver.MilliTime;
                log.Info($"{methodName} delta={(endTime - startTime)}");

                // Stay below 50 ms
                Assert.IsTrue((endTime - startTime) < 500);
                env.UndeployAll();
            }
        }

        private class EPLJoinJoinPerformanceStreamA : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env);
                var methodName = ".testJoinPerformanceStreamA";

                // Send 100k events
                log.Info($"{methodName} Preloading events");
                for (var i = 0; i < 50000; i++) {
                    SendEvent(env, MakeMarketEvent($"IBM_{i}"));
                }

                log.Info($"{methodName} Done preloading");

                var startTime = PerformanceObserver.MilliTime;
                SendEvent(env, MakeSupportEvent("IBM_10"));
                var endTime = PerformanceObserver.MilliTime;
                log.Info($"{methodName} delta={(endTime - startTime)}");

                env.AssertListener("s0", listener => Assert.AreEqual(1, listener.LastNewData.Length));
                // Stay below 50 ms
                Assert.IsTrue((endTime - startTime) < 50);
                env.UndeployAll();
            }
        }

        private class EPLJoinJoinPerformanceStreamB : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var methodName = ".testJoinPerformanceStreamB";
                SetupStatement(env);

                // Send 100k events
                log.Info($"{methodName} Preloading events");
                for (var i = 0; i < 50000; i++) {
                    SendEvent(env, MakeSupportEvent($"IBM_{i}"));
                }

                log.Info($"{methodName} Done preloading");

                var startTime = PerformanceObserver.MilliTime;

                env.ListenerReset("s0");
                SendEvent(env, MakeMarketEvent($"IBM_{10}"));

                var endTime = PerformanceObserver.MilliTime;
                log.Info($"{methodName} delta={(endTime - startTime)}");

                env.AssertListener("s0", listener => Assert.AreEqual(1, listener.LastNewData.Length));
                // Stay below 50 ms
                Assert.IsTrue((endTime - startTime) < 25);
                env.UndeployAll();
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static object MakeSupportEvent(string id)
        {
            var bean = new SupportBean();
            bean.TheString = id;
            return bean;
        }

        private static object MakeMarketEvent(string id)
        {
            return new SupportMarketDataBean(id, 0, 0, "");
        }

        private static void SetupStatement(RegressionEnvironment env)
        {
            var epl = "@name('s0') select * from " +
                      "SupportMarketDataBean#length(1000000)," +
                      "SupportBean#length(1000000)" +
                      " where symbol=theString";
            env.CompileDeployAddListenerMileZero(epl, "s0");
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(EPLJoin2StreamSimplePerformance));
    }
} // end of namespace