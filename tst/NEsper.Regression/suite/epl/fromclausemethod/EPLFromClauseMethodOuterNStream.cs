///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;


namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethodOuterNStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With1Stream2HistStarSubordinateLeftRight(execs);
            With1Stream2HistStarSubordinateInner(execs);
            With1Stream2HistForwardSubordinate(execs);
            With1Stream3HistForwardSubordinate(execs);
            With1Stream3HistForwardSubordinateChain(execs);
            WithInvalid(execs);
            With2Stream1HistStarSubordinateLeftRight(execs);
            With1Stream2HistStarNoSubordinateLeftRight(execs);
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistStarNoSubordinateLeftRight(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistStarNoSubordinateLeftRight());
            return execs;
        }

        public static IList<RegressionExecution> With2Stream1HistStarSubordinateLeftRight(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2Stream1HistStarSubordinateLeftRight());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodInvalid());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream3HistForwardSubordinateChain(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream3HistForwardSubordinateChain());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream3HistForwardSubordinate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream3HistForwardSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistForwardSubordinate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistForwardSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistStarSubordinateInner(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistStarSubordinateInner());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistStarSubordinateLeftRight(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistStarSubordinateLeftRight());
            return execs;
        }

        private class EPLFromClauseMethod1Stream2HistStarSubordinateLeftRight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;
                var milestone = new AtomicLong();

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H0', P00, P04) as h0 " +
                             " on s0.P02 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H1', P01, P05) as h1 " +
                             " on s0.P03 = h1.index" +
                             " Order by valh0, valh1";
                TryAssertionOne(env, expression, milestone);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.FetchValMultiRow('H1', P01, P05) as h1 " +
                             " right outer join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s0.P03 = h1.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H0', P00, P04) as h0 " +
                             " on s0.P02 = h0.index" +
                             " Order by valh0, valh1";
                TryAssertionOne(env, expression, milestone);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.FetchValMultiRow('H0', P00, P04) as h0 " +
                             " right outer join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s0.P02 = h0.index" +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H1', P01, P05) as h1 " +
                             " on s0.P03 = h1.index " +
                             " Order by valh0, valh1";
                TryAssertionOne(env, expression, milestone);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.FetchValMultiRow('H0', P00, P04) as h0 " +
                             " full outer join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s0.P02 = h0.index" +
                             " full outer join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H1', P01, P05) as h1 " +
                             " on s0.P03 = h1.index " +
                             " Order by valh0, valh1";
                TryAssertionOne(env, expression, milestone);
            }
        }

        private class EPLFromClauseMethod1Stream2HistStarSubordinateInner : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " inner join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H0', P00, P04) as h0 " +
                             " on s0.P02 = h0.index " +
                             " inner join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H1', P01, P05) as h1 " +
                             " on s0.P03 = h1.index" +
                             " Order by valh0, valh1";
                TryAssertionTwo(env, expression);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.FetchValMultiRow('H0', P00, P04) as h0 " +
                             " inner join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s0.P02 = h0.index " +
                             " inner join " +
                             "method:SupportJoinMethods.FetchValMultiRow('H1', P01, P05) as h1 " +
                             " on s0.P03 = h1.index" +
                             " Order by valh0, valh1";
                TryAssertionTwo(env, expression);
            }
        }

        private class EPLFromClauseMethod1Stream2HistForwardSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt(Id like 'E%')#lastevent as s0 " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0 " +
                             " on s0.P02 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             " on h0.index = h1.index" +
                             " Order by valh0, valh1";
                TryAssertionThree(env, expression);
            }

            private static void TryAssertionThree(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 0, 0, 1);
                var result = new object[][] { new object[] { "E1", null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E2", 0, 1, 1);
                result = new object[][] { new object[] { "E2", null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E3", 1, 0, 1);
                result = new object[][] { new object[] { "E3", "H01", null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E4", 1, 1, 1);
                result = new object[][] { new object[] { "E4", "H01", "H11" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E5", 4, 4, 2);
                result = new object[][] { new object[] { "E5", "H02", "H12" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream3HistForwardSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt(Id like 'E%')#lastevent as s0 " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0 " +
                             " on s0.P03 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             " on h0.index = h1.index" +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H2', P02) as h2 " +
                             " on h1.index = h2.index" +
                             " Order by valh0, valh1, valh2";
                TryAssertionFour(env, expression);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0 " +
                             " right outer join " +
                             "SupportBeanInt(Id like 'E%')#lastevent as s0 " +
                             " on s0.P03 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             " on h0.index = h1.index" +
                             " full outer join " +
                             "method:SupportJoinMethods.FetchVal('H2', P02) as h2 " +
                             " on h1.index = h2.index" +
                             " Order by valh0, valh1, valh2";
                TryAssertionFour(env, expression);
            }

            private static void TryAssertionFour(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1,valh2".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 0, 0, 0, 1);
                var result = new object[][] { new object[] { "E1", null, null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E2", 0, 1, 1, 1);
                result = new object[][] { new object[] { "E2", null, null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E3", 1, 1, 1, 1);
                result = new object[][] { new object[] { "E3", "H01", "H11", "H21" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E4", 1, 0, 1, 1);
                result = new object[][] { new object[] { "E4", "H01", null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E5", 4, 4, 4, 2);
                result = new object[][] { new object[] { "E5", "H02", "H12", "H22" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream3HistForwardSubordinateChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt(Id like 'E%')#lastevent as s0 " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal(s0.Id || '-H0', P00) as h0 " +
                             " on s0.P03 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal(h0.val || '-H1', P01) as h1 " +
                             " on h0.index = h1.index" +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal(h1.val || '-H2', P02) as h2 " +
                             " on h1.index = h2.index" +
                             " Order by valh0, valh1, valh2";
                TryAssertionFive(env, expression);
            }

            private static void TryAssertionFive(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1,valh2".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 0, 0, 0, 1);
                var result = new object[][] { new object[] { "E1", null, null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E2", 0, 1, 1, 1);
                result = new object[][] { new object[] { "E2", null, null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E3", 1, 1, 1, 1);
                result = new object[][] { new object[] { "E3", "E3-H01", "E3-H01-H11", "E3-H01-H11-H21" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E4", 1, 0, 1, 1);
                result = new object[][] { new object[] { "E4", "E4-H01", null, null } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                SendBeanInt(env, "E5", 4, 4, 4, 2);
                result = new object[][] { new object[] { "E5", "E5-H02", "E5-H02-H12", "E5-H02-H12-H22" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;
                // Invalid dependency order: a historical depends on it's own outer join child or descendant
                //              S0
                //      H0  (depends H1)
                //      H1
                expression = "@name('s0') select * from " +
                             "SupportBeanInt#lastevent as s0 " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal(h1.val, 1) as h0 " +
                             " on s0.P00 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.FetchVal('H1', 1) as h1 " +
                             " on h0.index = h1.index";
                env.TryInvalidCompile(
                    expression,
                    "Historical stream 1 parameter dependency originating in stream 2 cannot or may not be satisfied by the join");

                // Optimization conflict : required streams are always executed before optional streams
                //              S0
                //  full outer join H0 to S0
                //  left outer join H1 to S0 (H1 depends on H0)
                expression = "@name('s0') select * from " +
                             "SupportBeanInt#lastevent as s0 " +
                             " full outer join " +
                             "method:SupportJoinMethods.FetchVal('x', 1) as h0 " +
                             " on s0.P00 = h0.index " +
                             " left outer join " +
                             "method:SupportJoinMethods.fetchVal(h0.val, 1) as h1 " +
                             " on s0.P00 = h1.index";
                env.TryInvalidCompile(
                    expression,
                    "Historical stream 2 parameter dependency originating in stream 1 cannot or may not be satisfied by the join");
            }
        }

        private class EPLFromClauseMethod2Stream1HistStarSubordinateLeftRight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                //   S1 -> S0 -> H0
                expression = "@name('s0') select s0.Id as s0id, s1.Id as s1id, h0.val as valh0 from " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " left outer join " +
                             "method:SupportJoinMethods.fetchVal(s0.Id || 'H0', s0.P00) as h0 " +
                             " on s0.P01 = h0.index " +
                             " right outer join " +
                             "SupportBeanInt(Id like 'F%')#keepall as s1 " +
                             " on s1.P01 = s0.P01";
                TryAssertionSix(env, expression);

                expression = "@name('s0') select s0.Id as s0id, s1.Id as s1id, h0.val as valh0 from " +
                             "SupportBeanInt(Id like 'F%')#keepall as s1 " +
                             " left outer join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s1.P01 = s0.P01" +
                             " left outer join " +
                             "method:SupportJoinMethods.fetchVal(s0.Id || 'H0', s0.P00) as h0 " +
                             " on s0.P01 = h0.index ";
                TryAssertionSix(env, expression);

                expression = "@name('s0') select s0.Id as s0id, s1.Id as s1id, h0.val as valh0 from " +
                             "method:SupportJoinMethods.fetchVal(s0.Id || 'H0', s0.P00) as h0 " +
                             " right outer join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s0.P01 = h0.index " +
                             " right outer join " +
                             "SupportBeanInt(Id like 'F%')#keepall as s1 " +
                             " on s1.P01 = s0.P01";
                TryAssertionSix(env, expression);
            }

            private static void TryAssertionSix(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "s0id,s1id,valh0".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 1, 1);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "F1", 1, 1);
                var resultOne = new object[][] { new object[] { "E1", "F1", "E1H01" } };
                env.AssertPropsPerRowLastNew("s0", fields, resultOne);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultOne);

                SendBeanInt(env, "F2", 2, 2);
                var resultTwo = new object[][] { new object[] { null, "F2", null } };
                env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

                SendBeanInt(env, "E2", 2, 2);
                var resultThree = new object[][] { new object[] { "E2", "F2", "E2H02" } };
                env.AssertPropsPerRowLastNew("s0", fields, resultThree);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree));

                SendBeanInt(env, "F3", 3, 3);
                var resultFour = new object[][] { new object[] { null, "F3", null } };
                env.AssertPropsPerRowLastNew("s0", fields, resultFour);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree, resultFour));

                SendBeanInt(env, "E3", 0, 3);
                var resultFive = new object[][] { new object[] { "E3", "F3", null } };
                env.AssertPropsPerRowLastNew("s0", fields, resultFive);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree, resultFive));

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream2HistStarNoSubordinateLeftRight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as s0id, h0.val as valh0, h1.val as valh1 from " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " right outer join " +
                             "method:SupportJoinMethods.fetchVal('H0', 2) as h0 " +
                             " on s0.P00 = h0.index " +
                             " right outer join " +
                             "method:SupportJoinMethods.fetchVal('H1', 2) as h1 " +
                             " on s0.P00 = h1.index";
                TryAssertionSeven(env, expression);

                expression = "@name('s0') select s0.Id as s0id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.fetchVal('H1', 2) as h1 " +
                             " left outer join " +
                             "SupportBeanInt(Id like 'E%')#keepall as s0 " +
                             " on s0.P00 = h1.index" +
                             " right outer join " +
                             "method:SupportJoinMethods.fetchVal('H0', 2) as h0 " +
                             " on s0.P00 = h0.index ";
                TryAssertionSeven(env, expression);
            }
        }

        private static void TryAssertionSeven(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeploy(expression).AddListener("s0");

            var fields = "s0id,valh0,valh1".SplitCsv();
            var resultOne = new object[][] {
                new object[] { null, "H01", null }, new object[] { null, "H02", null },
                new object[] { null, null, "H11" }, new object[] { null, null, "H12" }
            };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultOne);

            SendBeanInt(env, "E1", 0);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultOne);

            SendBeanInt(env, "E2", 2);
            var resultTwo = new object[][] { new object[] { "E2", "H02", "H12" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
            var resultIt = new object[][] {
                new object[] { null, "H01", null }, new object[] { null, null, "H11" },
                new object[] { "E2", "H02", "H12" }
            };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultIt);

            SendBeanInt(env, "E3", 1);
            resultTwo = new object[][] { new object[] { "E3", "H01", "H11" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
            resultIt = new object[][] { new object[] { "E3", "H01", "H11" }, new object[] { "E2", "H02", "H12" } };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultIt);

            SendBeanInt(env, "E4", 1);
            resultTwo = new object[][] { new object[] { "E4", "H01", "H11" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
            resultIt = new object[][] {
                new object[] { "E3", "H01", "H11" }, new object[] { "E4", "H01", "H11" },
                new object[] { "E2", "H02", "H12" }
            };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultIt);

            env.UndeployAll();
        }

        private static void TryAssertionOne(
            RegressionEnvironment env,
            string expression,
            AtomicLong milestone)
        {
            env.CompileDeploy(expression).AddListener("s0");

            var fields = "Id,valh0,valh1".SplitCsv();
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendBeanInt(env, "E1", 0, 0, 0, 0, 1, 1);
            var resultOne = new object[][] { new object[] { "E1", null, null } };
            env.AssertPropsPerRowLastNew("s0", fields, resultOne);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultOne);

            SendBeanInt(env, "E2", 1, 1, 1, 1, 1, 1);
            var resultTwo = new object[][] { new object[] { "E2", "H01_0", "H11_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

            env.MilestoneInc(milestone);

            SendBeanInt(env, "E3", 5, 5, 3, 4, 1, 1);
            var resultThree = new object[][] { new object[] { "E3", "H03_0", "H14_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultThree);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree));

            SendBeanInt(env, "E4", 0, 5, 3, 4, 1, 1);
            var resultFour = new object[][] { new object[] { "E4", null, "H14_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultFour);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour));

            SendBeanInt(env, "E5", 2, 0, 2, 1, 1, 1);
            var resultFive = new object[][] { new object[] { "E5", "H02_0", null } };
            env.AssertPropsPerRowLastNew("s0", fields, resultFive);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive));

            // set 2 rows for H0
            SendBeanInt(env, "E6", 2, 2, 2, 2, 2, 1);
            var resultSix = new object[][]
                { new object[] { "E6", "H02_0", "H12_0" }, new object[] { "E6", "H02_1", "H12_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultSix);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(
                    resultOne,
                    resultTwo,
                    resultThree,
                    resultFour,
                    resultFive,
                    resultSix));

            SendBeanInt(env, "E7", 10, 10, 4, 5, 1, 2);
            var resultSeven = new object[][]
                { new object[] { "E7", "H04_0", "H15_0" }, new object[] { "E7", "H04_0", "H15_1" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultSeven);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(
                    resultOne,
                    resultTwo,
                    resultThree,
                    resultFour,
                    resultFive,
                    resultSix,
                    resultSeven));

            env.UndeployAll();
        }

        private static void TryAssertionTwo(
            RegressionEnvironment env,
            string expression)
        {
            env.CompileDeploy(expression).AddListener("s0");

            var fields = "Id,valh0,valh1".SplitCsv();
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendBeanInt(env, "E1", 0, 0, 0, 0, 1, 1);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendBeanInt(env, "E2", 1, 1, 1, 1, 1, 1);
            var resultTwo = new object[][] { new object[] { "E2", "H01_0", "H11_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo));

            SendBeanInt(env, "E3", 5, 5, 3, 4, 1, 1);
            var resultThree = new object[][] { new object[] { "E3", "H03_0", "H14_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultThree);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));

            SendBeanInt(env, "E4", 0, 5, 3, 4, 1, 1);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));

            SendBeanInt(env, "E5", 2, 0, 2, 1, 1, 1);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));

            // set 2 rows for H0
            SendBeanInt(env, "E6", 2, 2, 2, 2, 2, 1);
            var resultSix = new object[][]
                { new object[] { "E6", "H02_0", "H12_0" }, new object[] { "E6", "H02_1", "H12_0" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultSix);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree, resultSix));

            SendBeanInt(env, "E7", 10, 10, 4, 5, 1, 2);
            var resultSeven = new object[][]
                { new object[] { "E7", "H04_0", "H15_0" }, new object[] { "E7", "H04_0", "H15_1" } };
            env.AssertPropsPerRowLastNew("s0", fields, resultSeven);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree, resultSix, resultSeven));

            env.UndeployAll();
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            string id,
            int p00,
            int p01,
            int p02,
            int p03,
            int p04,
            int p05)
        {
            env.SendEventBean(new SupportBeanInt(id, p00, p01, p02, p03, p04, p05));
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            string id,
            int p00,
            int p01,
            int p02,
            int p03)
        {
            SendBeanInt(env, id, p00, p01, p02, p03, -1, -1);
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            string id,
            int p00,
            int p01,
            int p02)
        {
            SendBeanInt(env, id, p00, p01, p02, -1);
        }

        private static void SendBeanInt(
            RegressionEnvironment env,
            string id,
            int p00,
            int p01)
        {
            SendBeanInt(env, id, p00, p01, -1, -1);
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