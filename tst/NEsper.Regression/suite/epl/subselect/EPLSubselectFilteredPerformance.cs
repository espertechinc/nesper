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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectFilteredPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOneCriteria(execs);
            WithTwoCriteria(execs);
            WithJoin3CriteriaSceneOne(execs);
            WithJoin3CriteriaSceneTwo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithJoin3CriteriaSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceJoin3CriteriaSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithJoin3CriteriaSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceJoin3CriteriaSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoCriteria(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceTwoCriteria());
            return execs;
        }

        public static IList<RegressionExecution> WithOneCriteria(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceOneCriteria());
            return execs;
        }

        private class EPLSubselectPerformanceOneCriteria : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select P10 from SupportBean_S1#length(100000) where Id = s0.Id) as value from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
                    env.AssertEqualsNew("s0", "value", Convert.ToString(index));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private class EPLSubselectPerformanceTwoCriteria : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select P10 from SupportBean_S1#length(100000) where s0.Id = Id and P10 = s0.P00) as value from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
                    env.AssertEqualsNew("s0", "value", Convert.ToString(index));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private class EPLSubselectPerformanceJoin3CriteriaSceneOne : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select P00 from SupportBean_S0#length(100000) where P00 = s1.P10 and P01 = s2.P20 and P02 = s3.P30) as value " +
                    "from SupportBean_S1#length(100000) as s1, SupportBean_S2#length(100000) as s2, SupportBean_S3#length(100000) as s3 where s1.Id = s2.Id and s2.Id = s3.Id";
                TryPerfJoin3Criteria(env, stmtText);
            }
        }

        private class EPLSubselectPerformanceJoin3CriteriaSceneTwo : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select P00 from SupportBean_S0#length(100000) where P01 = s2.P20 and P00 = s1.P10 and P02 = s3.P30 and Id >= 0) as value " +
                    "from SupportBean_S3#length(100000) as s3, SupportBean_S1#length(100000) as s1, SupportBean_S2#length(100000) as s2 where s2.Id = s3.Id and s1.Id = s2.Id";
                TryPerfJoin3Criteria(env, stmtText);
            }
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
                env.AssertEqualsNew("s0", "value", Convert.ToString(index));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;

            Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
            env.UndeployAll();
        }
    }
} // end of namespace