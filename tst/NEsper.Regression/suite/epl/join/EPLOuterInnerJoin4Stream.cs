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
using com.espertech.esper.regressionlib.support.bean;


namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterInnerJoin4Stream
    {
        private static readonly string[] FIELDS =
            "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".SplitCsv();

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
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinStarJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithStarJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinStarJoinVariantTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithFullSidedJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullSidedJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithFullSidedJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullSidedJoinVariantTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithFullMiddleJoinVariantOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullMiddleJoinVariantOne());
            return execs;
        }

        public static IList<RegressionExecution> WithFullMiddleJoinVariantTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullMiddleJoinVariantTwo());
            return execs;
        }

        private class EPLJoinFullMiddleJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from SupportBean_S3#keepall s3 " +
                                    " inner join SupportBean_S2#keepall s2 on s3.p30 = s2.p20 " +
                                    " full outer join SupportBean_S1#keepall s1 on s2.p20 = s1.p10 " +
                                    " inner join SupportBean_S0#keepall s0 on s1.p10 = s0.p00";

                TryAssertionMiddle(env, joinStatement);
            }
        }

        private class EPLJoinFullMiddleJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from SupportBean_S0#keepall s0 " +
                                    " inner join SupportBean_S1#keepall s1 on s0.p00 = s1.p10 " +
                                    " full outer join SupportBean_S2#keepall s2 on s1.p10 = s2.p20 " +
                                    " inner join SupportBean_S3#keepall s3 on s2.p20 = s3.p30";

                TryAssertionMiddle(env, joinStatement);
            }
        }

        private class EPLJoinFullSidedJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from SupportBean_S3#keepall s3 " +
                                    " full outer join SupportBean_S2#keepall s2 on s3.p30 = s2.p20 " +
                                    " full outer join SupportBean_S1#keepall s1 on s2.p20 = s1.p10 " +
                                    " inner join SupportBean_S0#keepall s0 on s1.p10 = s0.p00";

                TryAssertionSided(env, joinStatement);
            }
        }

        private class EPLJoinFullSidedJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from SupportBean_S0#keepall s0 " +
                                    " inner join SupportBean_S1#keepall s1 on s0.p00 = s1.p10 " +
                                    " full outer join SupportBean_S2#keepall s2 on s1.p10 = s2.p20 " +
                                    " full outer join SupportBean_S3#keepall s3 on s2.p20 = s3.p30";

                TryAssertionSided(env, joinStatement);
            }
        }

        private class EPLJoinStarJoinVariantTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from SupportBean_S0#keepall s0 " +
                                    " left outer join SupportBean_S1#keepall s1 on s0.p00 = s1.p10 " +
                                    " full outer join SupportBean_S2#keepall s2 on s0.p00 = s2.p20 " +
                                    " inner join SupportBean_S3#keepall s3 on s0.p00 = s3.p30";

                TryAssertionStar(env, joinStatement);
            }
        }

        private class EPLJoinStarJoinVariantOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from SupportBean_S3#keepall s3 " +
                                    " inner join SupportBean_S0#keepall s0 on s0.p00 = s3.p30 " +
                                    " full outer join SupportBean_S2#keepall s2 on s0.p00 = s2.p20 " +
                                    " left outer join SupportBean_S1#keepall s1 on s1.p10 = s0.p00";

                TryAssertionStar(env, joinStatement);
            }
        }

        private static void TryAssertionMiddle(
            RegressionEnvironment env,
            string expression)
        {
            var fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".SplitCsv();

            env.CompileDeployAddListenerMileZero(expression, "s0");

            // s0, s1, s2, s3
            env.SendEventBean(new SupportBean_S0(0, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(100, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(200, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(300, "A"));
            env.AssertPropsNew("s0", fields, new object[] { 0, "A", 100, "A", 200, "A", 300, "A" });

            // s0, s2, s3, s1
            env.SendEventBean(new SupportBean_S0(1, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(201, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(301, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(101, "B"));
            env.AssertPropsNew("s0", fields, new object[] { 1, "B", 101, "B", 201, "B", 301, "B" });

            // s2, s3, s1, s0
            env.SendEventBean(new SupportBean_S2(202, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(302, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(102, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(2, "C"));
            env.AssertPropsNew("s0", fields, new object[] { 2, "C", 102, "C", 202, "C", 302, "C" });

            // s1, s2, s0, s3
            env.SendEventBean(new SupportBean_S1(103, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(203, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(3, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(303, "D"));
            env.AssertPropsNew("s0", fields, new object[] { 3, "D", 103, "D", 203, "D", 303, "D" });

            env.UndeployAll();
        }

        private static void TryAssertionSided(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeployAddListenerMileZero(expression, "s0");

            // s0, s1, s2, s3
            env.SendEventBean(new SupportBean_S0(0, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(100, "A"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 0, "A", 100, "A", null, null, null, null });

            env.SendEventBean(new SupportBean_S2(200, "A"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 0, "A", 100, "A", 200, "A", null, null });

            env.SendEventBean(new SupportBean_S3(300, "A"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 0, "A", 100, "A", 200, "A", 300, "A" });

            // s0, s2, s3, s1
            env.SendEventBean(new SupportBean_S0(1, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(201, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(301, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(101, "B"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 1, "B", 101, "B", 201, "B", 301, "B" });

            // s2, s3, s1, s0
            env.SendEventBean(new SupportBean_S2(202, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(302, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(102, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(2, "C"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 2, "C", 102, "C", 202, "C", 302, "C" });

            // s1, s2, s0, s3
            env.SendEventBean(new SupportBean_S1(103, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(203, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(3, "D"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 3, "D", 103, "D", 203, "D", null, null });

            env.SendEventBean(new SupportBean_S3(303, "D"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 3, "D", 103, "D", 203, "D", 303, "D" });

            env.UndeployAll();
        }

        private static void TryAssertionStar(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeployAddListenerMileZero(expression, "s0");

            // s0, s1, s2, s3
            env.SendEventBean(new SupportBean_S0(0, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(100, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(200, "A"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(300, "A"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 0, "A", 100, "A", 200, "A", 300, "A" });

            // s0, s2, s3, s1
            env.SendEventBean(new SupportBean_S0(1, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(201, "B"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(301, "B"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 1, "B", null, null, 201, "B", 301, "B" });

            env.SendEventBean(new SupportBean_S1(101, "B"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 1, "B", 101, "B", 201, "B", 301, "B" });

            // s2, s3, s1, s0
            env.SendEventBean(new SupportBean_S2(202, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(302, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(102, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(2, "C"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 2, "C", 102, "C", 202, "C", 302, "C" });

            // s1, s2, s0, s3
            env.SendEventBean(new SupportBean_S1(103, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(203, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(3, "D"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S3(303, "D"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 3, "D", 103, "D", 203, "D", 303, "D" });

            // s3, s0, s1, s2
            env.SendEventBean(new SupportBean_S3(304, "E"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(4, "E"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 4, "E", null, null, null, null, 304, "E" });

            env.SendEventBean(new SupportBean_S1(104, "E"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 4, "E", 104, "E", null, null, 304, "E" });

            env.SendEventBean(new SupportBean_S2(204, "E"));
            env.AssertPropsNew("s0", FIELDS, new object[] { 4, "E", 104, "E", 204, "E", 304, "E" });

            env.UndeployAll();
        }
    }
} // end of namespace