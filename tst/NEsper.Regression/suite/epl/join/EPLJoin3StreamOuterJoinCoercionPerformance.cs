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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin3StreamOuterJoinCoercionPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSceneTwo(execs);
            WithSceneThree(execs);
            WithRange(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3wayRange());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneThree());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneOne());
            return execs;
        }

        private class EPLJoinPerfCoercion3waySceneOne : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                               "SupportBean(TheString='A')#length(1000000) s1 " +
                               " left outer join " +
                               "SupportBean(TheString='B')#length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                               " left outer join " +
                               "SupportBean(TheString='C')#length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
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
                    env.AssertEventNew(
                        "s0",
                        theEvent => {
                            Assert.AreEqual(index, theEvent.Get("v1"));
                            Assert.AreEqual((long)index, theEvent.Get("v2"));
                            Assert.AreEqual((double)index, theEvent.Get("v3"));
                        });
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private class EPLJoinPerfCoercion3waySceneTwo : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                               "SupportBean(TheString='A')#length(1000000) s1 " +
                               " left outer join " +
                               "SupportBean(TheString='B')#length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                               " left outer join " +
                               "SupportBean(TheString='C')#length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "B", 0, i, 0);
                    SendEvent(env, "A", i, 0, 0);
                }

                env.ListenerReset("s0");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "C", 0, 0, index);
                    env.AssertEventNew(
                        "s0",
                        theEvent => {
                            Assert.AreEqual(index, theEvent.Get("v1"));
                            Assert.AreEqual((long)index, theEvent.Get("v2"));
                            Assert.AreEqual((double)index, theEvent.Get("v3"));
                        });
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private class EPLJoinPerfCoercion3waySceneThree : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                               "SupportBean(TheString='A')#length(1000000) s1 " +
                               " left outer join " +
                               "SupportBean(TheString='B')#length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                               " left outer join " +
                               "SupportBean(TheString='C')#length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "A", i, 0, 0);
                    SendEvent(env, "C", 0, 0, i);
                }

                env.ListenerReset("s0");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "B", 0, index, 0);
                    env.AssertEventNew(
                        "s0",
                        theEvent => {
                            Assert.AreEqual(index, theEvent.Get("v1"));
                            Assert.AreEqual((long)index, theEvent.Get("v2"));
                            Assert.AreEqual((double)index, theEvent.Get("v3"));
                        });
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private class EPLJoinPerfCoercion3wayRange : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select * from " +
                    "SupportBeanRange#keepall sbr " +
                    " left outer join " +
                    "SupportBean_ST0#keepall s0 on s0.Key0=sbr.Key" +
                    " left outer join " +
                    "SupportBean_ST1#keepall s1 on s1.Key1=s0.Key0" +
                    " where s0.P00 between sbr.RangeStartLong and sbr.RangeEndLong";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                log.Info("Preload");
                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_ST1($"ST1_{i}", "K", i));
                }

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_ST0($"ST0_{i}", "K", i));
                }

                log.Info("Preload done");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 100; i++) {
                    long index = 5000 + i;
                    env.SendEventBean(SupportBeanRange.MakeLong("R", "K", index, index + 2));
                    env.AssertListener("s0", listener => Assert.AreEqual(30, listener.GetAndResetLastNewData().Length));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                env.SendEventBean(new SupportBean_ST0("ST0X", "K", 5000));
                env.AssertListener("s0", listener => Assert.AreEqual(10, listener.GetAndResetLastNewData().Length));

                env.SendEventBean(new SupportBean_ST1("ST1X", "K", 5004));
                env.AssertListener("s0", listener => Assert.AreEqual(301, listener.GetAndResetLastNewData().Length));

                Assert.That(delta, Is.LessThan(500), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
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

        private static readonly ILog log = LogManager.GetLogger(typeof(EPLJoin3StreamOuterJoinCoercionPerformance));
    }
} // end of namespace