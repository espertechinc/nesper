///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethodJoinPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistInnerJoinPerformance());
            execs.Add(new EPLFromClauseMethod1Stream2HistOuterJoinPerformance());
            execs.Add(new EPLFromClauseMethod2Stream1HistTwoSidedEntryIdenticalIndex());
            execs.Add(new EPLFromClauseMethod2Stream1HistTwoSidedEntryMixedIndex());
            return execs;
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

        internal class EPLFromClauseMethod1Stream2HistInnerJoinPerformance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt#lastevent as s0, " +
                                 "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                                 "method:SupportJoinMethods.FetchVal('H1', 100) as h1 " +
                                 "where h0.index = P00 and h1.index = P00";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "id,valh0,valh1".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 5000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);

                    object[][] result = {
                        new object[] {"E1", "H0" + num, "H1" + num}
                    };
                    EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                env.UndeployAll();
                Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
            }
        }

        internal class EPLFromClauseMethod1Stream2HistOuterJoinPerformance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt#lastevent as s0 " +
                                 " left outer join " +
                                 "method:SupportJoinMethods.FetchVal('H0', 100) as h0 " +
                                 " on h0.index = P00 " +
                                 " left outer join " +
                                 "method:SupportJoinMethods.FetchVal('H1', 100) as h1 " +
                                 " on h1.index = P00";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "id,valh0,valh1".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 5000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);

                    object[][] result = {new object[] {"E1", "H0" + num, "H1" + num}};
                    EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                env.UndeployAll();
                Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
            }
        }

        internal class EPLFromClauseMethod2Stream1HistTwoSidedEntryIdenticalIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@Name('s0') select s0.Id as s0Id, s1.Id as s1Id, h0.val as valh0 " +
                                 "from SupportBeanInt(Id like 'E%')#lastevent as s0, " +
                                 "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                                 "SupportBeanInt(Id like 'F%')#lastevent as s1 " +
                                 "where h0.index = s0.P00 and h0.index = s1.P00";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "s0Id,s1Id,valh0".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 1000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);
                    SendBeanInt(env, "F1", num);

                    object[][] result = {new object[] {"E1", "F1", "H0" + num}};
                    EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);

                    // send reset events to avoid duplicate matches
                    SendBeanInt(env, "E1", 0);
                    SendBeanInt(env, "F1", 0);
                    env.Listener("s0").Reset();
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod2Stream1HistTwoSidedEntryMixedIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression =
                    "@Name('s0') select s0.Id as s0Id, s1.Id as s1Id, h0.val as valh0, h0.index as indexh0 from " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                    "SupportBeanInt(Id like 'H%')#lastevent as s1, " +
                    "SupportBeanInt(Id like 'E%')#lastevent as s0 " +
                    "where h0.index = s0.P00 and h0.val = s1.Id";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "s0Id,s1Id,valh0,indexh0".SplitCsv();
                var random = new Random();

                var start = PerformanceObserver.MilliTime;
                for (var i = 1; i < 1000; i++) {
                    var num = random.Next(98) + 1;
                    SendBeanInt(env, "E1", num);
                    SendBeanInt(env, "H0" + num, num);

                    object[][] result = {new object[] {"E1", "H0" + num, "H0" + num, num}};
                    EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);

                    // send reset events to avoid duplicate matches
                    SendBeanInt(env, "E1", 0);
                    SendBeanInt(env, "F1", 0);
                    env.Listener("s0").Reset();
                }

                var end = PerformanceObserver.MilliTime;
                var delta = end - start;
                env.UndeployAll();
                Assert.IsTrue(delta < 1000, "Delta to large, at " + delta + " msec");
            }
        }
    }
} // end of namespace