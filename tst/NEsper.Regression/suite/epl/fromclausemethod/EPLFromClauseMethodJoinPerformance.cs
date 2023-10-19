///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethodJoinPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With1Stream2HistInnerJoinPerformance(execs);
            With1Stream2HistOuterJoinPerformance(execs);
            With2Stream1HistTwoSidedEntryIdenticalIndex(execs);
            With2Stream1HistTwoSidedEntryMixedIndex(execs);
            return execs;
        }

        public static IList<RegressionExecution> With2Stream1HistTwoSidedEntryMixedIndex(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2Stream1HistTwoSidedEntryMixedIndex());
            return execs;
        }

        public static IList<RegressionExecution> With2Stream1HistTwoSidedEntryIdenticalIndex(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2Stream1HistTwoSidedEntryIdenticalIndex());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistOuterJoinPerformance(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistOuterJoinPerformance());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistInnerJoinPerformance(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistInnerJoinPerformance());
            return execs;
        }

        private class EPLFromClauseMethod1Stream2HistInnerJoinPerformance : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt#lastevent as s0, " +
                                 "method:SupportJoinMethods.fetchVal('H0', 100) as h0, " +
                                 "method:SupportJoinMethods.fetchVal('H1', 100) as h1 " +
                                 "where h0.index = p00 and h1.index = p00";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "id,valh0,valh1".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 5000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);

                    var result = new object[][] { new object[] { "E1", "H0" + num, "H1" + num } };
                    env.AssertPropsPerRowLastNew("s0", fields, result);
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
            }
        }

        private class EPLFromClauseMethod1Stream2HistOuterJoinPerformance : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt#lastevent as s0 " +
                                 " left outer join " +
                                 "method:SupportJoinMethods.fetchVal('H0', 100) as h0 " +
                                 " on h0.index = p00 " +
                                 " left outer join " +
                                 "method:SupportJoinMethods.fetchVal('H1', 100) as h1 " +
                                 " on h1.index = p00";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "id,valh0,valh1".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 5000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);

                    var result = new object[][] { new object[] { "E1", "H0" + num, "H1" + num } };
                    env.AssertPropsPerRowLastNew("s0", fields, result);
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
            }
        }

        private class EPLFromClauseMethod2Stream1HistTwoSidedEntryIdenticalIndex : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select s0.id as s0id, s1.id as s1id, h0.val as valh0 " +
                                 "from SupportBeanInt(id like 'E%')#lastevent as s0, " +
                                 "method:SupportJoinMethods.fetchVal('H0', 100) as h0, " +
                                 "SupportBeanInt(id like 'F%')#lastevent as s1 " +
                                 "where h0.index = s0.p00 and h0.index = s1.p00";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "s0id,s1id,valh0".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 1000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);
                    SendBeanInt(env, "F1", num);

                    var result = new object[][] { new object[] { "E1", "F1", "H0" + num } };
                    env.AssertPropsPerRowLastNew("s0", fields, result);

                    // send reset events to avoid duplicate matches
                    SendBeanInt(env, "E1", 0);
                    SendBeanInt(env, "F1", 0);
                    env.ListenerReset("s0");
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod2Stream1HistTwoSidedEntryMixedIndex : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var expression =
                    "@name('s0') select s0.id as s0id, s1.id as s1id, h0.val as valh0, h0.index as indexh0 from " +
                    "method:SupportJoinMethods.fetchVal('H0', 100) as h0, " +
                    "SupportBeanInt(id like 'H%')#lastevent as s1, " +
                    "SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    "where h0.index = s0.p00 and h0.val = s1.id";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "s0id,s1id,valh0,indexh0".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 1000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);
                    SendBeanInt(env, "H0" + num, num);

                    var result = new object[][] { new object[] { "E1", "H0" + num, "H0" + num, num } };
                    env.AssertPropsPerRowLastNew("s0", fields, result);

                    // send reset events to avoid duplicate matches
                    SendBeanInt(env, "E1", 0);
                    SendBeanInt(env, "F1", 0);
                    env.ListenerReset("s0");
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
            }
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            string id,
            int p00,
            int p01,
            int p02,
            int p03)
        {
            env.SendEventBean(new SupportBeanInt(id, p00, p01, p02, p03, -1, -1));
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            string id,
            int p00)
        {
            SendBeanInt(env, id, p00, -1, -1, -1);
        }
    }
} // end of namespace