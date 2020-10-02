///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterInnerJoin3Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFullJoinVariantThree(execs);
            WithFullJoinVariantTwo(execs);
            WithFullJoinVariantOne(execs);
            WithLeftJoinVariantThree(execs);
            WithLeftJoinVariantTwo(execs);
            WithRightJoinVariantOne(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRightJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRightJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftJoinVariantTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftJoinVariantThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftJoinVariantThree());
            return execs;
        }

        public static IList<RegressionExecution> WithFullJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithFullJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullJoinVariantTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithFullJoinVariantThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullJoinVariantThree());
            return execs;
        }

        private static void TryAssertionFull(
            RegressionEnvironment env,
            string expression)
        {
            var fields = new[] {"S0.Id", " S0.P00", " S1.Id", " S1.P10", " S2.Id", " S2.P20"};

            env.EplToModelCompileDeploy(expression).AddListener("s0");

            // s1, s2, s0
            env.SendEventBean(new SupportBean_S1(100, "A_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(200, "A_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, 100, "A_1", 200, "A_1"});

            env.SendEventBean(new SupportBean_S0(0, "A_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {0, "A_1", 100, "A_1", 200, "A_1"});

            // s1, s0, s2
            env.SendEventBean(new SupportBean_S1(103, "D_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(203, "D_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, 103, "D_1", 203, "D_1"});

            env.SendEventBean(new SupportBean_S0(3, "D_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {3, "D_1", 103, "D_1", 203, "D_1"});

            // s2, s1, s0
            env.SendEventBean(new SupportBean_S2(201, "B_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(101, "B_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, 101, "B_1", 201, "B_1"});

            env.SendEventBean(new SupportBean_S0(1, "B_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, "B_1", 101, "B_1", 201, "B_1"});

            // s2, s0, s1
            env.SendEventBean(new SupportBean_S2(202, "C_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(2, "C_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(102, "C_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {2, "C_1", 102, "C_1", 202, "C_1"});

            // s0, s1, s2
            env.SendEventBean(new SupportBean_S0(4, "E_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(104, "E_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(204, "E_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {4, "E_1", 104, "E_1", 204, "E_1"});

            // s0, s2, s1
            env.SendEventBean(new SupportBean_S0(5, "F_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(205, "F_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(105, "F_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {5, "F_1", 105, "F_1", 205, "F_1"});

            env.UndeployAll();
        }

        internal class EPLJoinFullJoinVariantThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S1#keepall as S1 inner join " +
                                    "SupportBean_S2#length(1000) as S2 on S1.P10 = S2.P20 " +
                                    "full outer join " +
                                    "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10";

                TryAssertionFull(env, joinStatement);
            }
        }

        internal class EPLJoinFullJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S2#length(1000) as S2 " +
                                    "inner join " +
                                    "SupportBean_S1#keepall as S1 on S1.P10 = S2.P20" +
                                    " full outer join " +
                                    "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10";

                TryAssertionFull(env, joinStatement);
            }
        }

        internal class EPLJoinFullJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S0#length(1000) as S0 " +
                                    "full outer join " +
                                    "SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10" +
                                    " inner join " +
                                    "SupportBean_S2#length(1000) as S2 on S1.P10 = S2.P20";

                TryAssertionFull(env, joinStatement);
            }
        }

        internal class EPLJoinLeftJoinVariantThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S1#keepall as S1 left outer join " +
                                    "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                                    "inner join " +
                                    "SupportBean_S2#length(1000) as S2 on S1.P10 = S2.P20";

                TryAssertionFull(env, joinStatement);
            }
        }

        internal class EPLJoinLeftJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S2#length(1000) as S2 " +
                                    "inner join " +
                                    "SupportBean_S1#keepall as S1 on S1.P10 = S2.P20" +
                                    " left outer join " +
                                    "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10";

                TryAssertionFull(env, joinStatement);
            }
        }

        internal class EPLJoinRightJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S0#length(1000) as S0 " +
                                    "right outer join " +
                                    "SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10" +
                                    " inner join " +
                                    "SupportBean_S2#length(1000) as S2 on S1.P10 = S2.P20";

                TryAssertionFull(env, joinStatement);
            }
        }
    }
} // end of namespace