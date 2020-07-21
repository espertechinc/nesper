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

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamAndPropertyPerformance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoin2StreamAndPropertyPerfRemoveStream());
            execs.Add(new EPLJoin2StreamAndPropertyPerf2Properties());
            execs.Add(new EPLJoin2StreamAndPropertyPerf3Properties());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static object MakeSupportEvent(
            string id,
            long longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = id;
            bean.LongBoxed = longBoxed;
            return bean;
        }

        private static object MakeMarketEvent(
            string id,
            long volume)
        {
            return new SupportMarketDataBean(id, 0, volume, "");
        }

        internal class EPLJoin2StreamAndPropertyPerfRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MyStaticEval.CountCalled = 0;
                MyStaticEval.WaitTimeMSec = 0;
                env.AdvanceTime(0);

                var epl = "@name('s0') select * from " +
                          " SupportBean#time(1) as sb, " +
                          " SupportBean_S0#keepall as S0 " +
                          " where myStaticEvaluator(sb.TheString, S0.P00)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S0(1, "x"));
                Assert.AreEqual(0, MyStaticEval.CountCalled);

                env.SendEventBean(new SupportBean("y", 10));
                Assert.AreEqual(1, MyStaticEval.CountCalled);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                // this would be observed as hanging if there was remove-stream evaluation
                MyStaticEval.WaitTimeMSec = 10000000;
                env.AdvanceTime(100000);

                env.UndeployAll();
            }
        }

        internal class EPLJoin2StreamAndPropertyPerf2Properties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var methodName = ".testPerformanceJoinNoResults";

                var epl = "@name('s0') select * from " +
                          "SupportMarketDataBean#length(1000000)," +
                          "SupportBean#length(1000000)" +
                          " where Symbol=TheString and Volume=LongBoxed";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // Send events for each stream
                log.Info(methodName + " Preloading events");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    SendEvent(env, MakeMarketEvent("IBM_" + i, 1));
                    SendEvent(env, MakeSupportEvent("CSCO_" + i, 2));
                }

                log.Info(methodName + " Done preloading");

                var endTime = PerformanceObserver.MilliTime;
                log.Info(methodName + " delta=" + (endTime - startTime));

                // Stay at 250, belwo 500ms
                Assert.IsTrue(endTime - startTime < 500);
                env.UndeployAll();
            }
        }

        internal class EPLJoin2StreamAndPropertyPerf3Properties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var methodName = ".testPerformanceJoinNoResults";

                var epl = "@name('s0') select * from " +
                          "SupportMarketDataBean()#length(1000000)," +
                          "SupportBean#length(1000000)" +
                          " where Symbol=TheString and Volume=LongBoxed and DoublePrimitive=Price";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // Send events for each stream
                log.Info("{0} Preloading events", methodName);
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    SendEvent(env, MakeMarketEvent("IBM_" + i, 1));
                    SendEvent(env, MakeSupportEvent("CSCO_" + i, 2));
                }

                log.Info("{0} Done preloading", methodName);

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                log.Info("{0} delta={1}", methodName, delta);

                // Stay at 250, below 500ms
                Assert.That(endTime - startTime, Is.LessThan(500));
                env.UndeployAll();
            }
        }

        public class MyStaticEval
        {
            public static int CountCalled { get; set; }

            public static long WaitTimeMSec { get; set; }

            public static bool MyStaticEvaluator(
                string a,
                string b)
            {
                try {
                    Thread.Sleep((int) WaitTimeMSec);
                    CountCalled++;
                }
                catch (ThreadInterruptedException) {
                    return false;
                }

                return true;
            }
        }
    }
} // end of namespace