///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectFilteredPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceOneCriteria());
            execs.Add(new EPLSubselectPerformanceTwoCriteria());
            execs.Add(new EPLSubselectPerformanceJoin3CriteriaSceneOne());
            execs.Add(new EPLSubselectPerformanceJoin3CriteriaSceneTwo());
            return execs;
        }

        private static void TryPerfJoin3Criteria(
            RegressionEnvironment env,
            string stmtText)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            // preload with 10k events
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(
                    new SupportBean_S0(i, Convert.ToString(i), Convert.ToString(i + 1), Convert.ToString(i + 2)));
            }

            var startTime = PerformanceObserver.MilliTime;
            for (var index = 0; index < 5000; index++) {
                env.SendEventBean(new SupportBean_S1(index, Convert.ToString(index)));
                env.SendEventBean(new SupportBean_S2(index, Convert.ToString(index + 1)));
                env.SendEventBean(new SupportBean_S3(index, Convert.ToString(index + 2)));
                Assert.AreEqual(Convert.ToString(index), env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;

            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
            env.UndeployAll();
        }

        internal class EPLSubselectPerformanceOneCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select P10 from SupportBean_S1#length(100000) where Id = s0.Id) as value from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
                    Assert.AreEqual(Convert.ToString(index), env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectPerformanceTwoCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select P10 from SupportBean_S1#length(100000) where s0.Id = Id and P10 = s0.P00) as value from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
                    Assert.AreEqual(Convert.ToString(index), env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectPerformanceJoin3CriteriaSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select P00 from SupportBean_S0#length(100000) where P00 = s1.P10 and P01 = s2.P20 and P02 = s3.P30) as value " +
                    "from SupportBean_S1#length(100000) as s1, SupportBean_S2#length(100000) as s2, SupportBean_S3#length(100000) as s3 where s1.Id = s2.Id and s2.Id = s3.Id";
                TryPerfJoin3Criteria(env, stmtText);
            }
        }

        internal class EPLSubselectPerformanceJoin3CriteriaSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select P00 from SupportBean_S0#length(100000) where P01 = s2.P20 and P00 = s1.P10 and P02 = s3.P30 and Id >= 0) as value " +
                    "from SupportBean_S3#length(100000) as s3, SupportBean_S1#length(100000) as s1, SupportBean_S2#length(100000) as s2 where s2.Id = s3.Id and s1.Id = s2.Id";
                TryPerfJoin3Criteria(env, stmtText);
            }
        }
    }
} // end of namespace