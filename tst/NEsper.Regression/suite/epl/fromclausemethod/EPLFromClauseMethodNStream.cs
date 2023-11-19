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
    public class EPLFromClauseMethodNStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With1Stream2HistStarSubordinateCartesianLast(execs);
            With1Stream2HistStarSubordinateJoinedKeepall(execs);
            With1Stream2HistForwardSubordinate(execs);
            With1Stream3HistStarSubordinateCartesianLast(execs);
            With1Stream3HistForwardSubordinate(execs);
            With1Stream3HistChainSubordinate(execs);
            With2Stream2HistStarSubordinate(execs);
            With3Stream1HistSubordinate(execs);
            With3HistPureNoSubordinate(execs);
            With3Hist1Subordinate(execs);
            With3Hist2SubordinateChain(execs);
            With3Stream1HistStreamNWTwice(execs);
            return execs;
        }

        public static IList<RegressionExecution> With3Stream1HistStreamNWTwice(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod3Stream1HistStreamNWTwice());
            return execs;
        }

        public static IList<RegressionExecution> With3Hist2SubordinateChain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod3Hist2SubordinateChain());
            return execs;
        }

        public static IList<RegressionExecution> With3Hist1Subordinate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod3Hist1Subordinate());
            return execs;
        }

        public static IList<RegressionExecution> With3HistPureNoSubordinate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod3HistPureNoSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With3Stream1HistSubordinate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod3Stream1HistSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With2Stream2HistStarSubordinate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2Stream2HistStarSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream3HistChainSubordinate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream3HistChainSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream3HistForwardSubordinate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream3HistForwardSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream3HistStarSubordinateCartesianLast(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream3HistStarSubordinateCartesianLast());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistForwardSubordinate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistForwardSubordinate());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistStarSubordinateJoinedKeepall(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistStarSubordinateJoinedKeepall());
            return execs;
        }

        public static IList<RegressionExecution> With1Stream2HistStarSubordinateCartesianLast(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistStarSubordinateCartesianLast());
            return execs;
        }

        private class EPLFromClauseMethod3Stream1HistStreamNWTwice : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Public create window AllTrades#keepall as SupportTradeEventWithSide", path);
                env.CompileDeploy("insert into AllTrades select * from SupportTradeEventWithSide", path);

                var epl = "@name('s0') select us, them, corr.correlation as crl " +
                          "from AllTrades as us, AllTrades as them," +
                          "method:" +
                          typeof(EPLFromClauseMethodNStream).FullName +
                          ".computeCorrelation(us, them) as corr\n" +
                          "where us.side != them.side and corr.correlation > 0";
                env.CompileDeploy(epl, path).AddListener("s0");

                var one = new SupportTradeEventWithSide("T1", "B");
                env.SendEventBean(one);
                env.AssertListenerNotInvoked("s0");

                var two = new SupportTradeEventWithSide("T2", "S");
                env.SendEventBean(two);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    "us,them,crl".SplitCsv(),
                    new object[][] { new object[] { one, two, 1 }, new object[] { two, one, 1 } });
                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream2HistStarSubordinateCartesianLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt#lastevent as s0, " +
                                 "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                                 "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                                 "order by h0.val, h1.val";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 1, 1);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1", "H01", "H11" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H11" } });

                SendBeanInt(env, "E2", 2, 0);
                env.AssertPropsPerRowLastNew("s0", fields, null);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                env.Milestone(0);

                SendBeanInt(env, "E3", 0, 1);
                env.AssertPropsPerRowLastNew("s0", fields, null);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E3", 2, 2);
                var result = new object[][] {
                    new object[] { "E3", "H01", "H11" }, new object[] { "E3", "H01", "H12" },
                    new object[] { "E3", "H02", "H11" }, new object[] { "E3", "H02", "H12" }
                };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.Milestone(1);

                SendBeanInt(env, "E4", 2, 1);
                result = new object[][] { new object[] { "E4", "H01", "H11" }, new object[] { "E4", "H02", "H11" } };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream2HistStarSubordinateJoinedKeepall : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt#keepall as s0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             "where h0.index = h1.index and h0.index = P02";
                TryAssertionOne(env, expression);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1   from " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "SupportBeanInt#keepall as s0 " +
                             "where h0.index = h1.index and h0.index = P02";
                TryAssertionOne(env, expression);
            }

            private static void TryAssertionOne(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 20, 20, 3);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1", "H03", "H13" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H03", "H13" } });

                SendBeanInt(env, "E2", 20, 20, 21);
                env.AssertPropsPerRowLastNew("s0", fields, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H03", "H13" } });

                SendBeanInt(env, "E3", 4, 4, 2);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E3", "H02", "H12" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H03", "H13" }, new object[] { "E3", "H02", "H12" } });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream2HistForwardSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;
                var milestone = new AtomicLong();

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt#keepall as s0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal(h0.val, P01) as h1 " +
                             "order by h0.val, h1.val";
                TryAssertionTwo(env, expression, milestone);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.FetchVal(h0.val, P01) as h1, " +
                             "SupportBeanInt#keepall as s0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0 " +
                             "order by h0.val, h1.val";
                TryAssertionTwo(env, expression, milestone);
            }

            private static void TryAssertionTwo(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 1, 1);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1", "H01", "H011" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H011" } });

                SendBeanInt(env, "E2", 0, 1);
                env.AssertPropsPerRowLastNew("s0", fields, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H011" } });

                env.MilestoneInc(milestone);

                SendBeanInt(env, "E3", 1, 0);
                env.AssertPropsPerRowLastNew("s0", fields, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H011" } });

                SendBeanInt(env, "E4", 2, 2);
                object[][] result = {
                    new object[] { "E4", "H01", "H011" }, new object[] { "E4", "H01", "H012" },
                    new object[] { "E4", "H02", "H021" }, new object[] { "E4", "H02", "H022" }
                };
                env.AssertPropsPerRowLastNew("s0", fields, result);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(
                        result,
                        new object[][] { new object[] { "E1", "H01", "H011" } }));

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream3HistStarSubordinateCartesianLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt#lastevent as s0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal('H2', P02) as h2 " +
                             "order by h0.val, h1.val, h2.val";
                TryAssertionThree(env, expression);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H2', P02) as h2, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "SupportBeanInt#lastevent as s0 " +
                             "order by h0.val, h1.val, h2.val";
                TryAssertionThree(env, expression);
            }

            private static void TryAssertionThree(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1,valh2".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 1, 1, 1);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H11", "H21" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H11", "H21" } });

                SendBeanInt(env, "E2", 1, 1, 2);
                var result = new object[][]
                    { new object[] { "E2", "H01", "H11", "H21" }, new object[] { "E2", "H01", "H11", "H22" } };
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
                             "from SupportBeanInt#keepall as s0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'H2', P02) as h2 " +
                             " where h0.index = h1.index and h1.index = h2.index and h2.index = P03";
                TryAssertionFour(env, expression);

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'H2', P02) as h2, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "SupportBeanInt#keepall as s0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             " where h0.index = h1.index and h1.index = h2.index and h2.index = P03";
                TryAssertionFour(env, expression);
            }

            private static void TryAssertionFour(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1,valh2".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E1", 2, 2, 2, 1);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H11", "H01H21" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "H01", "H11", "H01H21" } });

                SendBeanInt(env, "E2", 4, 4, 4, 3);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "H03", "H13", "H03H23" } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", "H01", "H11", "H01H21" }, new object[] { "E2", "H03", "H13", "H03H23" }
                    });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod1Stream3HistChainSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select s0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt#keepall as s0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal(h1.val||'H2', P02) as h2 " +
                             " where h0.index = h1.index and h1.index = h2.index and h2.index = P03";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "Id,valh0,valh1,valh2".SplitCsv();
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "E2", 4, 4, 4, 3);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "H03", "H03H13", "H03H13H23" } });

                SendBeanInt(env, "E2", 4, 4, 4, 5);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.Milestone(0);

                SendBeanInt(env, "E2", 4, 4, 0, 1);
                env.AssertPropsPerRowLastNew("s0", fields, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "H03", "H03H13", "H03H13H23" } });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod2Stream2HistStarSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select s0.Id as ids0, s1.Id as ids1, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt(Id like 'S0%')#keepall as s0, " +
                                 "SupportBeanInt(Id like 'S1%')#lastevent as s1, " +
                                 "method:SupportJoinMethods.FetchVal(s0.Id||'H1', s0.P00) as h0, " +
                                 "method:SupportJoinMethods.FetchVal(s1.Id||'H2', s1.P00) as h1 " +
                                 "order by s0.Id asc";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "ids0,ids1,valh0,valh1".SplitCsv();
                SendBeanInt(env, "S00", 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);
                env.AssertListenerNotInvoked("s0");

                SendBeanInt(env, "S10", 1);
                var resultOne = new object[][] { new object[] { "S00", "S10", "S00H11", "S10H21" } };
                env.AssertPropsPerRowLastNew("s0", fields, resultOne);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultOne);

                SendBeanInt(env, "S01", 1);
                var resultTwo = new object[][] { new object[] { "S01", "S10", "S01H11", "S10H21" } };
                env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

                env.Milestone(0);

                SendBeanInt(env, "S11", 1);
                var resultThree = new object[][] {
                    new object[] { "S00", "S11", "S00H11", "S11H21" }, new object[] { "S01", "S11", "S01H11", "S11H21" }
                };
                env.AssertPropsPerRowLastNew("s0", fields, resultThree);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, EPAssertionUtil.ConcatenateArray2Dim(resultThree));

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod3Stream1HistSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select s0.Id as ids0, s1.Id as ids1, s2.Id as ids2, h0.val as valh0 " +
                                 "from SupportBeanInt(Id like 'S0%')#keepall as s0, " +
                                 "SupportBeanInt(Id like 'S1%')#lastevent as s1, " +
                                 "SupportBeanInt(Id like 'S2%')#lastevent as s2, " +
                                 "method:SupportJoinMethods.FetchVal(s1.Id||s2.Id||'H1', s0.P00) as h0 " +
                                 "order by s0.Id, s1.Id, s2.Id, h0.val";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "ids0,ids1,ids2,valh0".SplitCsv();
                SendBeanInt(env, "S00", 2);
                SendBeanInt(env, "S10", 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);
                env.AssertListenerNotInvoked("s0");

                SendBeanInt(env, "S20", 1);
                var resultOne = new object[][] {
                    new object[] { "S00", "S10", "S20", "S10S20H11" }, new object[] { "S00", "S10", "S20", "S10S20H12" }
                };
                env.AssertPropsPerRowLastNew("s0", fields, resultOne);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, resultOne);

                SendBeanInt(env, "S01", 1);
                var resultTwo = new object[][] { new object[] { "S01", "S10", "S20", "S10S20H11" } };
                env.AssertPropsPerRowLastNew("s0", fields, resultTwo);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

                env.Milestone(0);

                SendBeanInt(env, "S21", 1);
                var resultThree = new object[][] {
                    new object[] { "S00", "S10", "S21", "S10S21H11" },
                    new object[] { "S00", "S10", "S21", "S10S21H12" }, new object[] { "S01", "S10", "S21", "S10S21H11" }
                };
                env.AssertPropsPerRowLastNew("s0", fields, resultThree);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, EPAssertionUtil.ConcatenateArray2Dim(resultThree));

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod3HistPureNoSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("on SupportBeanInt set var1=P00, var2=P01, var3=P02, var4=P03");
                var milestone = new AtomicLong();

                string expression;
                expression = "@name('s0') select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                             "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                             "method:SupportJoinMethods.FetchVal('H2', var3) as h2";
                TryAssertionFive(env, expression, milestone);

                expression = "@name('s0') select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H2', var3) as h2," +
                             "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                             "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
                TryAssertionFive(env, expression, milestone);

                env.UndeployAll();
            }

            private static void TryAssertionFive(
                RegressionEnvironment env,
                string expression,
                AtomicLong milestone)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "valh0,valh1,valh2".SplitCsv();

                SendBeanInt(env, "S00", 1, 1, 1);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "H01", "H11", "H21" } });

                SendBeanInt(env, "S01", 0, 1, 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                env.MilestoneInc(milestone);

                SendBeanInt(env, "S02", 1, 1, 0);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "S03", 1, 1, 2);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "H01", "H11", "H21" }, new object[] { "H01", "H11", "H22" } });

                SendBeanInt(env, "S04", 2, 2, 1);
                var result = new object[][] {
                    new object[] { "H01", "H11", "H21" }, new object[] { "H02", "H11", "H21" },
                    new object[] { "H01", "H12", "H21" }, new object[] { "H02", "H12", "H21" }
                };
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.UndeployModuleContaining("s0");
            }
        }

        private class EPLFromClauseMethod3Hist1Subordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("on SupportBeanInt set var1=P00, var2=P01, var3=P02, var4=P03");

                string expression;
                expression = "@name('s0') select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                             "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                             "method:SupportJoinMethods.FetchVal(h0.val||'-H2', var3) as h2";
                TryAssertionSix(env, expression);

                expression = "@name('s0') select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'-H2', var3) as h2," +
                             "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                             "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
                TryAssertionSix(env, expression);

                env.UndeployAll();
            }

            private static void TryAssertionSix(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = "valh0,valh1,valh2".SplitCsv();

                SendBeanInt(env, "S00", 1, 1, 1);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "H01", "H11", "H01-H21" } });

                SendBeanInt(env, "S01", 0, 1, 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "S02", 1, 1, 0);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendBeanInt(env, "S03", 1, 1, 2);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "H01", "H11", "H01-H21" }, new object[] { "H01", "H11", "H01-H22" } });

                SendBeanInt(env, "S04", 2, 2, 1);
                var result = new object[][] {
                    new object[] { "H01", "H11", "H01-H21" }, new object[] { "H02", "H11", "H02-H21" },
                    new object[] { "H01", "H12", "H01-H21" }, new object[] { "H02", "H12", "H02-H21" }
                };
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

                env.UndeployModuleContaining("s0");
            }
        }

        private class EPLFromClauseMethod3Hist2SubordinateChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("on SupportBeanInt set var1=P00, var2=P01, var3=P02, var4=P03");
                var milestone = new AtomicLong();

                string expression;
                expression = "@name('s0') select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                             "method:SupportJoinMethods.FetchVal(h0.val||'-H1', var2) as h1," +
                             "method:SupportJoinMethods.FetchVal(h1.val||'-H2', var3) as h2";
                TryAssertionSeven(env, expression, milestone);

                expression = "@name('s0') select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal(h1.val||'-H2', var3) as h2," +
                             "method:SupportJoinMethods.FetchVal(h0.val||'-H1', var2) as h1," +
                             "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
                TryAssertionSeven(env, expression, milestone);

                env.UndeployAll();
            }
        }

        private static void TryAssertionSeven(
            RegressionEnvironment env,
            string expression,
            AtomicLong milestone)
        {
            env.CompileDeploy(expression).AddListener("s0");

            var fields = "valh0,valh1,valh2".SplitCsv();

            SendBeanInt(env, "S00", 1, 1, 1);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "H01", "H01-H11", "H01-H11-H21" } });

            SendBeanInt(env, "S01", 0, 1, 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            env.MilestoneInc(milestone);

            SendBeanInt(env, "S02", 1, 1, 0);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendBeanInt(env, "S03", 1, 1, 2);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "H01", "H01-H11", "H01-H11-H21" }, new object[] { "H01", "H01-H11", "H01-H11-H22" }
                });

            SendBeanInt(env, "S04", 2, 2, 1);
            var result = new object[][] {
                new object[] { "H01", "H01-H11", "H01-H11-H21" }, new object[] { "H02", "H02-H11", "H02-H11-H21" },
                new object[] { "H01", "H01-H12", "H01-H12-H21" }, new object[] { "H02", "H02-H12", "H02-H12-H21" }
            };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, result);

            env.UndeployModuleContaining("s0");
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

        public static ComputeCorrelationResult ComputeCorrelation(
            SupportTradeEventWithSide us,
            SupportTradeEventWithSide them)
        {
            return new ComputeCorrelationResult(us != null && them != null ? 1 : 0);
        }

        public class ComputeCorrelationResult
        {
            private readonly int correlation;

            public ComputeCorrelationResult(int correlation)
            {
                this.correlation = correlation;
            }

            public int GetCorrelation()
            {
                return correlation;
            }
        }
    }
} // end of namespace