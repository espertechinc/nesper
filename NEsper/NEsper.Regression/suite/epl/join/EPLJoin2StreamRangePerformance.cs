///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public partial class EPLJoin2StreamRangePerformance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithKeyAndRangeOuterJoin(execs);
            WithRelationalOp(execs);
            WithKeyAndRange(execs);
            WithKeyAndRangeInverted(execs);
            WithUnidirectionalRelOp(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUnidirectionalRelOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfUnidirectionalRelOp());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRangeInverted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfKeyAndRangeInverted());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfKeyAndRange());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfRelationalOp());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRangeOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerfKeyAndRangeOuterJoin());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env, RegressionPath path, AtomicLong milestone, string epl, int numLoops, AssertionCallback assertionCallback)
        {
            var fields = new[]{"mini", "maxi"};
            env.CompileDeploy("@Name('s0')" + epl, path).AddListener("s0").MilestoneInc(milestone);
            // Send range query events
            log.Info("Querying");
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < numLoops; i++)
            {
                //if (i % 10 == 0) {
                //    log.info("At loop #" + i);
                //}
                env.SendEventBean(assertionCallback.GetEvent(i));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, assertionCallback.GetExpectedValue(i));
            }

            log.Info("Done Querying");
            var endTime = PerformanceObserver.MilliTime;
            log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 1500);
            env.UndeployModuleContaining("s0");
        }

        internal class EPLJoinPerfKeyAndRangeOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window SBR#keepall as SupportBeanRange;\n" + "@Name('I1') insert into SBR select * from SupportBeanRange;\n" + "create window SB#keepall as SupportBean;\n" + "@Name('I2') insert into SB select * from SupportBean;\n";
                env.CompileDeploy(epl, path).Milestone(0);
                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 10000; i++)
                {
                    env.SendEventBean(new SupportBean("G", i));
                    env.SendEventBean(new SupportBeanRange("R", "G", i - 1, i + 2));
                }

                log.Info("Done preloading");
                // create
                var eplQuery = "@Name('s0') select * " + "from SB sb " + "full outer join " + "SBR sbr " + "on TheString = Key " + "where IntPrimitive between RangeStart and RangeEnd";
                env.CompileDeploy(eplQuery, path).AddListener("s0").Milestone(1);
                // Repeat
                log.Info("Querying");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++)
                {
                    env.SendEventBean(new SupportBean("G", 9990));
                    Assert.AreEqual(4, env.Listener("s0").GetAndResetLastNewData().Length);
                    env.SendEventBean(new SupportBeanRange("R", "G", 4, 10));
                    Assert.AreEqual(7, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                log.Info("Done Querying");
                var endTime = PerformanceObserver.MilliTime;
                log.Info("delta=" + (endTime - startTime));
                env.UndeployAll();
                Assert.That(endTime - startTime, Is.LessThan(500));
            }
        }

        internal class EPLJoinPerfRelationalOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window SBR#keepall as SupportBeanRange;\n" + "@Name('I1') insert into SBR select * from SupportBeanRange;\n" + "create window SB#keepall as SupportBean;\n" + "@Name('I2') insert into SB select * from SupportBean";
                env.CompileDeploy(epl, path).Milestone(0);
                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 10000; i++)
                {
                    env.SendEventBean(new SupportBean("E" + i, i));
                    env.SendEventBean(new SupportBeanRange("E", i, -1));
                }

                log.Info("Done preloading");
                // start query
                var eplQuery = "@Name('s0') select * from SBR a, SB b where a.RangeStart < b.IntPrimitive";
                env.CompileDeploy(eplQuery, path).AddListener("s0").Milestone(1);
                // Repeat
                log.Info("Querying");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++)
                {
                    env.SendEventBean(new SupportBean("B", 10));
                    Assert.AreEqual(10, env.Listener("s0").GetAndResetLastNewData().Length);
                    env.SendEventBean(new SupportBeanRange("R", 9990, -1));
                    Assert.AreEqual(9, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                log.Info("Done Querying");
                var endTime = PerformanceObserver.MilliTime;
                log.Info("delta=" + (endTime - startTime));
                env.UndeployAll();
                Assert.IsTrue(endTime - startTime < 500);
            }
        }

        internal class EPLJoinPerfKeyAndRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window SBR#keepall as SupportBeanRange;\n" + "@Name('I1') insert into SBR select * from SupportBeanRange;\n" + "create window SB#keepall as SupportBean;\n" + "@Name('I2') insert into SB select * from SupportBean;\n";
                env.CompileDeploy(epl, path).Milestone(0);
                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 100; i++)
                {
                    for (var j = 0; j < 100; j++)
                    {
                        env.SendEventBean(new SupportBean(Convert.ToString(i), j));
                        env.SendEventBean(new SupportBeanRange("R", Convert.ToString(i), j - 1, j + 1));
                    }
                }

                log.Info("Done preloading");
                // start query
                var eplQuery = "@Name('s0') select * from SBR sbr, SB sb where sbr.Key = sb.TheString and sb.IntPrimitive between sbr.RangeStart and sbr.RangeEnd";
                env.CompileDeploy(eplQuery, path).AddListener("s0").Milestone(1);
                // repeat
                log.Info("Querying");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++)
                {
                    env.SendEventBean(new SupportBean("55", 10));
                    Assert.AreEqual(3, env.Listener("s0").GetAndResetLastNewData().Length);
                    env.SendEventBean(new SupportBeanRange("R", "56", 12, 20));
                    Assert.AreEqual(9, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                log.Info("Done Querying");
                var endTime = PerformanceObserver.MilliTime;
                log.Info("delta=" + (endTime - startTime));
                // test no event found
                env.SendEventBean(new SupportBeanRange("R", "56", 2000, 3000));
                env.SendEventBean(new SupportBeanRange("R", "X", 2000, 3000));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsTrue(endTime - startTime < 1500, "delta=" + (endTime - startTime));
                // delete all events
                env.CompileDeploy("on SupportBean delete from SBR;\n" + "on SupportBean delete from SB;\n", path);
                env.SendEventBean(new SupportBean("D", -1));
                env.UndeployAll();
            }
        }

        internal class EPLJoinPerfKeyAndRangeInverted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window SB#keepall as SupportBean;\n" + "@Name('I2') insert into SB select * from SupportBean";
                env.CompileDeploy(epl, path).Milestone(0);
                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 10000; i++)
                {
                    env.SendEventBean(new SupportBean("E", i));
                }

                log.Info("Done preloading");
                // start query
                var eplQuery = "@Name('s0') select * from SupportBeanRange#lastevent sbr, SB sb where sbr.Key = sb.TheString and sb.IntPrimitive not in [sbr.RangeStart:sbr.RangeEnd]";
                env.CompileDeploy(eplQuery, path).AddListener("s0").Milestone(1);
                // repeat
                log.Info("Querying");
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++)
                {
                    env.SendEventBean(new SupportBeanRange("R", "E", 5, 9995));
                    Assert.AreEqual(9, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                log.Info("Done Querying");
                var endTime = PerformanceObserver.MilliTime;
                log.Info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);
                env.UndeployAll();
            }
        }

        internal class EPLJoinPerfUnidirectionalRelOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                var epl = "create window SB#keepall as SupportBean;\n" + "@Name('I') insert into SB select * from SupportBean;\n";
                env.CompileDeploy(epl, path).Milestone(0);
                // Preload
                log.Info("Preloading events");
                for (var i = 0; i < 100000; i++)
                {
                    env.SendEventBean(new SupportBean("E" + i, i));
                }

                log.Info("Done preloading");
                // Test range
                var rangeEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive between r.RangeStart and r.RangeEnd";
                var rangeEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " + "where a.IntPrimitive between r.RangeStart and r.RangeEnd";
                var rangeEplThree = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange#lastevent r, SB a " + "where a.IntPrimitive between r.RangeStart and r.RangeEnd";
                var rangeEplFour = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange#lastevent r " + "where a.IntPrimitive between r.RangeStart and r.RangeEnd";
                var rangeEplFive = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a\n" + "where a.IntPrimitive >= r.RangeStart and a.IntPrimitive <= r.RangeEnd";
                var rangeEplSix = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive <= r.RangeEnd and a.IntPrimitive >= r.RangeStart";
                AssertionCallback rangeCallback = new ProxyAssertionCallback{ProcGetEvent = iteration => new SupportBeanRange("E", iteration + 50000, iteration + 50100), ProcGetExpectedValue = iteration => new object[]{50000 + iteration, 50100 + iteration}};
                TryAssertion(env, path, milestone, rangeEplOne, 100, rangeCallback);
                TryAssertion(env, path, milestone, rangeEplTwo, 100, rangeCallback);
                TryAssertion(env, path, milestone, rangeEplThree, 100, rangeCallback);
                TryAssertion(env, path, milestone, rangeEplFour, 100, rangeCallback);
                TryAssertion(env, path, milestone, rangeEplFive, 100, rangeCallback);
                TryAssertion(env, path, milestone, rangeEplSix, 100, rangeCallback);
                // Test Greater-Equals
                var geEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive >= r.RangeStart and a.IntPrimitive <= 99200";
                var geEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " + "where a.IntPrimitive >= r.RangeStart and a.IntPrimitive <= 99200";
                AssertionCallback geCallback = new ProxyAssertionCallback{ProcGetEvent = iteration => new SupportBeanRange("E", iteration + 99000, null), ProcGetExpectedValue = iteration => new object[]{99000 + iteration, 99200}};
                TryAssertion(env, path, milestone, geEplOne, 100, geCallback);
                TryAssertion(env, path, milestone, geEplTwo, 100, geCallback);
                // Test Greater-Then
                var gtEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive > r.RangeStart and a.IntPrimitive <= 99200";
                var gtEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " + "where a.IntPrimitive > r.RangeStart and a.IntPrimitive <= 99200";
                var gtEplThree = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange#lastevent r, SB a " + "where a.IntPrimitive > r.RangeStart and a.IntPrimitive <= 99200";
                var gtEplFour = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange#lastevent r " + "where a.IntPrimitive > r.RangeStart and a.IntPrimitive <= 99200";
                AssertionCallback gtCallback = new ProxyAssertionCallback{ProcGetEvent = iteration =>
                {
                    return new SupportBeanRange("E", iteration + 99000, null);
                }

                , ProcGetExpectedValue = iteration =>
                {
                    return new object[]{99001 + iteration, 99200};
                }
                };
                TryAssertion(env, path, milestone, gtEplOne, 100, gtCallback);
                TryAssertion(env, path, milestone, gtEplTwo, 100, gtCallback);
                TryAssertion(env, path, milestone, gtEplThree, 100, gtCallback);
                TryAssertion(env, path, milestone, gtEplFour, 100, gtCallback);
                // Test Less-Then
                var ltEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive < r.RangeStart and a.IntPrimitive > 100";
                var ltEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " + "where a.IntPrimitive < r.RangeStart and a.IntPrimitive > 100";
                AssertionCallback ltCallback = new ProxyAssertionCallback{ProcGetEvent = iteration =>
                {
                    return new SupportBeanRange("E", iteration + 500, null);
                }

                , ProcGetExpectedValue = iteration =>
                {
                    return new object[]{101, 499 + iteration};
                }
                };
                TryAssertion(env, path, milestone, ltEplOne, 100, ltCallback);
                TryAssertion(env, path, milestone, ltEplTwo, 100, ltCallback);
                // Test Less-Equals
                var leEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive <= r.RangeStart and a.IntPrimitive > 100";
                var leEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " + "where a.IntPrimitive <= r.RangeStart and a.IntPrimitive > 100";
                AssertionCallback leCallback = new ProxyAssertionCallback{ProcGetEvent = iteration => new SupportBeanRange("E", iteration + 500, null), ProcGetExpectedValue = iteration => new object[]{101, 500 + iteration}};
                TryAssertion(env, path, milestone, leEplOne, 100, leCallback);
                TryAssertion(env, path, milestone, leEplTwo, 100, leCallback);
                // Test open range
                var openEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive > r.RangeStart and a.IntPrimitive < r.RangeEnd";
                var openEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive in (r.RangeStart:r.RangeEnd)";
                AssertionCallback openCallback = new ProxyAssertionCallback{ProcGetEvent = iteration =>
                {
                    return new SupportBeanRange("E", iteration + 3, iteration + 7);
                }

                , ProcGetExpectedValue = iteration =>
                {
                    return new object[]{iteration + 4, iteration + 6};
                }
                };
                TryAssertion(env, path, milestone, openEplOne, 100, openCallback);
                TryAssertion(env, path, milestone, openEplTwo, 100, openCallback);
                // Test half-open range
                var hopenEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive >= r.RangeStart and a.IntPrimitive < r.RangeEnd";
                var hopenEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive in [r.RangeStart:r.RangeEnd)";
                AssertionCallback halfOpenCallback = new ProxyAssertionCallback{ProcGetEvent = iteration =>
                {
                    return new SupportBeanRange("E", iteration + 3, iteration + 7);
                }

                , ProcGetExpectedValue = iteration =>
                {
                    return new object[]{iteration + 3, iteration + 6};
                }
                };
                TryAssertion(env, path, milestone, hopenEplOne, 100, halfOpenCallback);
                TryAssertion(env, path, milestone, hopenEplTwo, 100, halfOpenCallback);
                // Test half-closed range
                var hclosedEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive > r.RangeStart and a.IntPrimitive <= r.RangeEnd";
                var hclosedEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive in (r.RangeStart:r.RangeEnd]";
                AssertionCallback halfClosedCallback = new ProxyAssertionCallback{ProcGetEvent = iteration =>
                {
                    return new SupportBeanRange("E", iteration + 3, iteration + 7);
                }

                , ProcGetExpectedValue = iteration =>
                {
                    return new object[]{iteration + 4, iteration + 7};
                }
                };
                TryAssertion(env, path, milestone, hclosedEplOne, 100, halfClosedCallback);
                TryAssertion(env, path, milestone, hclosedEplTwo, 100, halfClosedCallback);
                // Test inverted closed range
                var invertedClosedEPLOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive not in [r.RangeStart:r.RangeEnd]";
                var invertedClosedEPLTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive not between r.RangeStart and r.RangeEnd";
                AssertionCallback invertedClosedCallback = new ProxyAssertionCallback{ProcGetEvent = iteration =>
                {
                    return new SupportBeanRange("E", 20, 99990);
                }

                , ProcGetExpectedValue = iteration =>
                {
                    return new object[]{0, 99999};
                }
                };
                TryAssertion(env, path, milestone, invertedClosedEPLOne, 100, invertedClosedCallback);
                TryAssertion(env, path, milestone, invertedClosedEPLTwo, 100, invertedClosedCallback);
                // Test inverted open range
                var invertedOpenEPLOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " + "where a.IntPrimitive not in (r.RangeStart:r.RangeEnd)";
                TryAssertion(env, path, milestone, invertedOpenEPLOne, 100, invertedClosedCallback);
                env.UndeployAll();
            }
        }
    }
} // end of namespace
