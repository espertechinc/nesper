///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin3StreamRangePerformance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithKeyAndRange(execs);
            WithRangeOnly(execs);
            WithUnidirectionalKeyAndRange(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUnidirectionalKeyAndRange(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinPerf3StreamUnidirectionalKeyAndRange());
            return execs;
        }

        public static IList<RegressionExecution> WithRangeOnly(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinPerf3StreamRangeOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRange(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinPerf3StreamKeyAndRange());
            return execs;
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            env.CompileDeploy(epl, path).AddListener("s0");

            // Repeat
            log.Info("Querying");
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(new SupportBeanRange("R", "G", 100, 101));
                Assert.AreEqual(4, env.Listener("s0").GetAndResetLastNewData().Length);
            }

            log.Info("Done Querying");
            var endTime = PerformanceObserver.MilliTime;
            log.Info("delta=" + (endTime - startTime));

            Assert.That(endTime - startTime, Is.LessThan(500));
            env.UndeployModuleContaining("s0");
        }

        /// <summary>
        ///     This join algorithm profits from merge join cartesian indicated via @hint.
        /// </summary>
        internal class EPLJoinPerf3StreamKeyAndRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window ST0#keepall as SupportBean_ST0;\n" +
                          "@Name('I1') insert into ST0 select * from SupportBean_ST0;\n" +
                          "create window ST1#keepall as SupportBean_ST1;\n" +
                          "@Name('I2') insert into ST1 select * from SupportBean_ST1;\n";
                env.CompileDeploy(epl, path).Milestone(0);

                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_ST0("ST0", "G", i));
                    env.SendEventBean(new SupportBean_ST1("ST1", "G", i));
                }

                log.Info("Done preloading");

                var eplQuery = "@Name('s0') @Hint('PREFER_MERGE_JOIN') select * from SupportBeanRange#lastevent a " +
                               "inner join ST0 st0 on st0.Key0 = a.Key " +
                               "inner join ST1 st1 on st1.Key1 = a.Key " +
                               "where " +
                               "st0.P00 between RangeStart and RangeEnd and st1.P10 between RangeStart and RangeEnd";
                TryAssertion(env, path, eplQuery);

                eplQuery =
                    "@Name('s0') @Hint('PREFER_MERGE_JOIN') select * from SupportBeanRange#lastevent a, ST0 st0, ST1 st1 " +
                    "where st0.Key0 = a.Key and st1.Key1 = a.Key and " +
                    "st0.P00 between RangeStart and RangeEnd and st1.P10 between RangeStart and RangeEnd";
                TryAssertion(env, path, eplQuery);

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     This join algorithm uses merge join cartesian (not nested iteration).
        /// </summary>
        internal class EPLJoinPerf3StreamRangeOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window ST0#keepall as SupportBean_ST0;\n" +
                          "@Name('I1') insert into ST0 select * from SupportBean_ST0;\n" +
                          "create window ST1#keepall as SupportBean_ST1;\n" +
                          "@Name('I2') insert into ST1 select * from SupportBean_ST1;\n";
                env.CompileDeploy(epl, path).Milestone(0);

                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_ST0("ST0", "ST0", i));
                    env.SendEventBean(new SupportBean_ST1("ST1", "ST1", i));
                }

                log.Info("Done preloading");

                var eplQuery = "@Name('s0') select * from SupportBeanRange#lastevent a, ST0 st0, ST1 st1 " +
                               "where st0.P00 between RangeStart and RangeEnd and st1.P10 between RangeStart and RangeEnd";
                env.CompileDeploy(eplQuery, path).AddListener("s0").Milestone(1);

                // Repeat
                log.Info("Querying");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "R", 100, 101));
                    Assert.AreEqual(4, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                log.Info("Done Querying");
                var endTime = PerformanceObserver.MilliTime;
                log.Info("delta=" + (endTime - startTime));

                Assert.IsTrue(endTime - startTime < 1000);
                env.UndeployAll();
            }
        }

        /// <summary>
        ///     This join algorithm profits from nested iteration execution.
        /// </summary>
        internal class EPLJoinPerf3StreamUnidirectionalKeyAndRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window SBR#keepall as SupportBeanRange;\n" +
                          "@Name('I1') insert into SBR select * from SupportBeanRange;\n" +
                          "create window ST1#keepall as SupportBean_ST1;\n" +
                          "@Name('I2') insert into ST1 select * from SupportBean_ST1;\n";
                env.CompileDeploy(epl, path).Milestone(0);

                // Preload
                log.Info("Preloading events");
                env.SendEventBean(new SupportBeanRange("ST1", "G", 4000, 4004));
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_ST1("ST1", "G", i));
                }

                log.Info("Done preloading");

                var eplQuery = "@Name('s0') select * from SupportBean_ST0 st0 unidirectional, SBR a, ST1 st1 " +
                               "where st0.Key0 = a.Key and st1.Key1 = a.Key and " +
                               "st1.P10 between RangeStart and RangeEnd";
                env.CompileDeploy(eplQuery, path).AddListener("s0").Milestone(1);

                // Repeat
                log.Info("Querying");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 500; i++) {
                    env.SendEventBean(new SupportBean_ST0("ST0", "G", -1));
                    Assert.AreEqual(5, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                log.Info("Done Querying");
                var delta = PerformanceObserver.MilliTime - startTime;
                log.Info("delta=" + delta);

                // This works best with a nested iteration join (and not a cardinal join)
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                env.UndeployAll();
            }
        }
    }
} // end of namespace