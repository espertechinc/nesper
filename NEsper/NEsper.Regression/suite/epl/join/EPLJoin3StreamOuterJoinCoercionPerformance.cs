///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin3StreamOuterJoinCoercionPerformance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneOne());
            execs.Add(new EPLJoinPerfCoercion3waySceneTwo());
            execs.Add(new EPLJoinPerfCoercion3waySceneThree());
            execs.Add(new EPLJoinPerfCoercion3wayRange());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intBoxed,
            long longBoxed,
            double doubleBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            env.SendEventBean(bean);
        }

        internal class EPLJoinPerfCoercion3waySceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select s1.IntBoxed as v1, s2.longBoxed as v2, s3.doubleBoxed as v3 from " +
                               "SupportBean(theString='A')#length(1000000) s1 " +
                               " left outer join " +
                               "SupportBean(theString='B')#length(1000000) s2 on s1.intBoxed=s2.longBoxed " +
                               " left outer join " +
                               "SupportBean(theString='C')#length(1000000) s3 on s1.intBoxed=s3.doubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "B", 0, i, 0);
                    SendEvent(env, "C", 0, 0, i);
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "A", index, 0, 0);
                    var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                    Assert.AreEqual(index, theEvent.Get("v1"));
                    Assert.AreEqual((long) index, theEvent.Get("v2"));
                    Assert.AreEqual((double) index, theEvent.Get("v3"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLJoinPerfCoercion3waySceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select s1.IntBoxed as v1, s2.longBoxed as v2, s3.doubleBoxed as v3 from " +
                               "SupportBean(theString='A')#length(1000000) s1 " +
                               " left outer join " +
                               "SupportBean(theString='B')#length(1000000) s2 on s1.intBoxed=s2.longBoxed " +
                               " left outer join " +
                               "SupportBean(theString='C')#length(1000000) s3 on s1.intBoxed=s3.doubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "B", 0, i, 0);
                    SendEvent(env, "A", i, 0, 0);
                }

                env.Listener("s0").Reset();
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "C", 0, 0, index);
                    var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                    Assert.AreEqual(index, theEvent.Get("v1"));
                    Assert.AreEqual((long) index, theEvent.Get("v2"));
                    Assert.AreEqual((double) index, theEvent.Get("v3"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLJoinPerfCoercion3waySceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select s1.IntBoxed as v1, s2.longBoxed as v2, s3.doubleBoxed as v3 from " +
                               "SupportBean(theString='A')#length(1000000) s1 " +
                               " left outer join " +
                               "SupportBean(theString='B')#length(1000000) s2 on s1.intBoxed=s2.longBoxed " +
                               " left outer join " +
                               "SupportBean(theString='C')#length(1000000) s3 on s1.intBoxed=s3.doubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "A", i, 0, 0);
                    SendEvent(env, "C", 0, 0, i);
                }

                env.Listener("s0").Reset();
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "B", 0, index, 0);
                    var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                    Assert.AreEqual(index, theEvent.Get("v1"));
                    Assert.AreEqual((long) index, theEvent.Get("v2"));
                    Assert.AreEqual((double) index, theEvent.Get("v3"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLJoinPerfCoercion3wayRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from " +
                               "SupportBeanRange#keepall sbr " +
                               " left outer join " +
                               "SupportBean_ST0#keepall s0 on s0.key0=sbr.key" +
                               " left outer join " +
                               "SupportBean_ST1#keepall s1 on s1.key1=s0.key0" +
                               " where s0.p00 between sbr.rangeStartLong and sbr.rangeEndLong";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                log.Info("Preload");
                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_ST1("ST1_" + i, "K", i));
                }

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_ST0("ST0_" + i, "K", i));
                }

                log.Info("Preload done");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 100; i++) {
                    long index = 5000 + i;
                    env.SendEventBean(SupportBeanRange.MakeLong("R", "K", index, index + 2));
                    Assert.AreEqual(30, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                env.SendEventBean(new SupportBean_ST0("ST0X", "K", 5000));
                Assert.AreEqual(10, env.Listener("s0").GetAndResetLastNewData().Length);

                env.SendEventBean(new SupportBean_ST1("ST1X", "K", 5004));
                Assert.AreEqual(301, env.Listener("s0").GetAndResetLastNewData().Length);

                Assert.IsTrue(delta < 500, "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }
    }
} // end of namespace