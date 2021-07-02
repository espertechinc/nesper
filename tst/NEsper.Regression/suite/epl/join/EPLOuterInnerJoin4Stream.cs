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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterInnerJoin4Stream
    {
        private static readonly string[] FIELDS =
            new[] {"S0.Id", " S0.P00", " S1.Id", " S1.P10", " S2.Id", " S2.P20", " S3.Id", " S3.P30"};

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFullMiddleJoinVariantTwo(execs);
            WithFullMiddleJoinVariantOne(execs);
            WithFullSidedJoinVariantTwo(execs);
            WithFullSidedJoinVariantOne(execs);
            WithStarJoinVariantTwo(execs);
            WithStarJoinVariantOne(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithStarJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinStarJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithStarJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinStarJoinVariantTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithFullSidedJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinFullSidedJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithFullSidedJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinFullSidedJoinVariantTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithFullMiddleJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinFullMiddleJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithFullMiddleJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinFullMiddleJoinVariantTwo());
            return execs;
        }

        private static void TryAssertionMiddle(
            RegressionEnvironment env,
            string expression)
        {
            var fields = new[] {"S0.Id", " S0.P00", " S1.Id", " S1.P10", " S2.Id", " S2.P20", " S3.Id", " S3.P30"};

            env.CompileDeployAddListenerMileZero(expression, "s0");

            // s0, s1, s2, s3
            env.SendEventBean(new SupportBean_S0(0, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(100, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(200, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {0, "A", 100, "A", 200, "A", 300, "A"});

            // s0, s2, s3, s1
            env.SendEventBean(new SupportBean_S0(1, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(201, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(301, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, "B", 101, "B", 201, "B", 301, "B"});

            // s2, s3, s1, s0
            env.SendEventBean(new SupportBean_S2(202, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(302, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(102, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {2, "C", 102, "C", 202, "C", 302, "C"});

            // s1, s2, s0, s3
            env.SendEventBean(new SupportBean_S1(103, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(203, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(3, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {3, "D", 103, "D", 203, "D", 303, "D"});

            env.UndeployAll();
        }

        private static void TryAssertionSided(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeployAddListenerMileZero(expression, "s0");

            // s0, s1, s2, s3
            env.SendEventBean(new SupportBean_S0(0, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(100, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {0, "A", 100, "A", null, null, null, null});

            env.SendEventBean(new SupportBean_S2(200, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {0, "A", 100, "A", 200, "A", null, null});

            env.SendEventBean(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {0, "A", 100, "A", 200, "A", 300, "A"});

            // s0, s2, s3, s1
            env.SendEventBean(new SupportBean_S0(1, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(201, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(301, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {1, "B", 101, "B", 201, "B", 301, "B"});

            // s2, s3, s1, s0
            env.SendEventBean(new SupportBean_S2(202, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(302, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(102, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {2, "C", 102, "C", 202, "C", 302, "C"});

            // s1, s2, s0, s3
            env.SendEventBean(new SupportBean_S1(103, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(203, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(3, "D"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {3, "D", 103, "D", 203, "D", null, null});

            env.SendEventBean(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {3, "D", 103, "D", 203, "D", 303, "D"});

            env.UndeployAll();
        }

        private static void TryAssertionStar(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeployAddListenerMileZero(expression, "s0");

            // s0, s1, s2, s3
            env.SendEventBean(new SupportBean_S0(0, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(100, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(200, "A"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {0, "A", 100, "A", 200, "A", 300, "A"});

            // s0, s2, s3, s1
            env.SendEventBean(new SupportBean_S0(1, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(201, "B"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(301, "B"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {1, "B", null, null, 201, "B", 301, "B"});

            env.SendEventBean(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {1, "B", 101, "B", 201, "B", 301, "B"});

            // s2, s3, s1, s0
            env.SendEventBean(new SupportBean_S2(202, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(302, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(102, "C"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {2, "C", 102, "C", 202, "C", 302, "C"});

            // s1, s2, s0, s3
            env.SendEventBean(new SupportBean_S1(103, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(203, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(3, "D"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {3, "D", 103, "D", 203, "D", 303, "D"});

            // s3, s0, s1, s2
            env.SendEventBean(new SupportBean_S3(304, "E"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(4, "E"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {4, "E", null, null, null, null, 304, "E"});

            env.SendEventBean(new SupportBean_S1(104, "E"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {4, "E", 104, "E", null, null, 304, "E"});

            env.SendEventBean(new SupportBean_S2(204, "E"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {4, "E", 104, "E", 204, "E", 304, "E"});

            env.UndeployAll();
        }

        internal class EPLJoinFullMiddleJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from SupportBean_S3#keepall S3 " +
                                    " inner join SupportBean_S2#keepall S2 on S3.P30 = S2.P20 " +
                                    " full outer join SupportBean_S1#keepall S1 on S2.P20 = S1.P10 " +
                                    " inner join SupportBean_S0#keepall S0 on S1.P10 = S0.P00";

                TryAssertionMiddle(env, joinStatement);
            }
        }

        internal class EPLJoinFullMiddleJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from SupportBean_S0#keepall S0 " +
                                    " inner join SupportBean_S1#keepall S1 on S0.P00 = S1.P10 " +
                                    " full outer join SupportBean_S2#keepall S2 on S1.P10 = S2.P20 " +
                                    " inner join SupportBean_S3#keepall S3 on S2.P20 = S3.P30";

                TryAssertionMiddle(env, joinStatement);
            }
        }

        internal class EPLJoinFullSidedJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from SupportBean_S3#keepall S3 " +
                                    " full outer join SupportBean_S2#keepall S2 on S3.P30 = S2.P20 " +
                                    " full outer join SupportBean_S1#keepall S1 on S2.P20 = S1.P10 " +
                                    " inner join SupportBean_S0#keepall S0 on S1.P10 = S0.P00";

                TryAssertionSided(env, joinStatement);
            }
        }

        internal class EPLJoinFullSidedJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from SupportBean_S0#keepall S0 " +
                                    " inner join SupportBean_S1#keepall S1 on S0.P00 = S1.P10 " +
                                    " full outer join SupportBean_S2#keepall S2 on S1.P10 = S2.P20 " +
                                    " full outer join SupportBean_S3#keepall S3 on S2.P20 = S3.P30";

                TryAssertionSided(env, joinStatement);
            }
        }

        internal class EPLJoinStarJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from SupportBean_S0#keepall S0 " +
                                    " left outer join SupportBean_S1#keepall S1 on S0.P00 = S1.P10 " +
                                    " full outer join SupportBean_S2#keepall S2 on S0.P00 = S2.P20 " +
                                    " inner join SupportBean_S3#keepall S3 on S0.P00 = S3.P30";

                TryAssertionStar(env, joinStatement);
            }
        }

        internal class EPLJoinStarJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from SupportBean_S3#keepall S3 " +
                                    " inner join SupportBean_S0#keepall S0 on S0.P00 = S3.P30 " +
                                    " full outer join SupportBean_S2#keepall S2 on S0.P00 = S2.P20 " +
                                    " left outer join SupportBean_S1#keepall S1 on S1.P10 = S0.P00";

                TryAssertionStar(env, joinStatement);
            }
        }
    }
} // end of namespace