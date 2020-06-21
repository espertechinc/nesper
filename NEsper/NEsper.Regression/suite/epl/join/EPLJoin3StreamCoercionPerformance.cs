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
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin3StreamCoercionPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOne(execs);
            WithTwo(execs);
            WithThree(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneThree());
            return execs;
        }

        public static IList<RegressionExecution> WithTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfCoercion3waySceneOne());
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
                var stmtText = "@Name('s0') select S1.IntBoxed as value from " +
                               "SupportBean(TheString='A')#length(1000000) S1," +
                               "SupportBean(TheString='B')#length(1000000) S2," +
                               "SupportBean(TheString='C')#length(1000000) S3" +
                               " where S1.IntBoxed=S2.LongBoxed and S1.IntBoxed=S3.DoubleBoxed";
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
                    Assert.AreEqual(index, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLJoinPerfCoercion3waySceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S1.IntBoxed as value from " +
                               "SupportBean(TheString='A')#length(1000000) S1," +
                               "SupportBean(TheString='B')#length(1000000) S2," +
                               "SupportBean(TheString='C')#length(1000000) S3" +
                               " where S1.IntBoxed=S2.LongBoxed and S1.IntBoxed=S3.DoubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "A", i, 0, 0);
                    SendEvent(env, "B", 0, i, 0);
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "C", 0, 0, index);
                    Assert.AreEqual(index, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
            }
        }

        internal class EPLJoinPerfCoercion3waySceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S1.IntBoxed as value from " +
                               "SupportBean(TheString='A')#length(1000000) S1," +
                               "SupportBean(TheString='B')#length(1000000) S2," +
                               "SupportBean(TheString='C')#length(1000000) S3" +
                               " where S1.IntBoxed=S2.LongBoxed and S1.IntBoxed=S3.DoubleBoxed";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload
                for (var i = 0; i < 10000; i++) {
                    SendEvent(env, "A", i, 0, 0);
                    SendEvent(env, "C", 0, 0, i);
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    SendEvent(env, "B", 0, index, 0);
                    Assert.AreEqual(index, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
            }
        }
    }
} // end of namespace