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
    public class EPLFromClauseMethodNStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod1Stream2HistStarSubordinateCartesianLast());
            execs.Add(new EPLFromClauseMethod1Stream2HistStarSubordinateJoinedKeepall());
            execs.Add(new EPLFromClauseMethod1Stream2HistForwardSubordinate());
            execs.Add(new EPLFromClauseMethod1Stream3HistStarSubordinateCartesianLast());
            execs.Add(new EPLFromClauseMethod1Stream3HistForwardSubordinate());
            execs.Add(new EPLFromClauseMethod1Stream3HistChainSubordinate());
            execs.Add(new EPLFromClauseMethod2Stream2HistStarSubordinate());
            execs.Add(new EPLFromClauseMethod3Stream1HistSubordinate());
            execs.Add(new EPLFromClauseMethod3HistPureNoSubordinate());
            execs.Add(new EPLFromClauseMethod3Hist1Subordinate());
            execs.Add(new EPLFromClauseMethod3Hist2SubordinateChain());
            execs.Add(new EPLFromClauseMethod3Stream1HistStreamNWTwice());
            return execs;
        }

        private static void TryAssertionSeven(
            RegressionEnvironment env,
            string expression,
            AtomicLong milestone)
        {
            env.CompileDeploy(expression).AddListener("s0");

            var fields = new [] { "valh0","valh1","valh2" };

            SendBeanInt(env, "S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {"H01", "H01-H11", "H01-H11-H21"}});

            SendBeanInt(env, "S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

            env.MilestoneInc(milestone);

            SendBeanInt(env, "S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

            SendBeanInt(env, "S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {"H01", "H01-H11", "H01-H11-H21"}, new object[] {"H01", "H01-H11", "H01-H11-H22"}});

            SendBeanInt(env, "S04", 2, 2, 1);
            object[][] result = {
                new object[] {"H01", "H01-H11", "H01-H11-H21"}, new object[] {"H02", "H02-H11", "H02-H11-H21"},
                new object[] {"H01", "H01-H12", "H01-H12-H21"},
                new object[] {"H02", "H02-H12", "H02-H12-H21"}
            };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, result);

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

        internal class EPLFromClauseMethod3Stream1HistStreamNWTwice : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window AllTrades#keepall as SupportTradeEventWithSide", path);
                env.CompileDeploy("insert into AllTrades select * from SupportTradeEventWithSide", path);

                var epl = "@name('s0') select us, them, corr.Correlation as crl " +
                          "from AllTrades as us, AllTrades as them," +
                          "method:" +
                          typeof(EPLFromClauseMethodNStream).FullName +
                          ".ComputeCorrelation(us, them) as corr\n" +
                          "where us.Side != them.Side and corr.Correlation > 0";
                env.CompileDeploy(epl, path).AddListener("s0");

                var one = new SupportTradeEventWithSide("T1", "B");
                env.SendEventBean(one);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                var two = new SupportTradeEventWithSide("T2", "S");
                env.SendEventBean(two);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new [] { "us","them","crl" },
                    new[] {new object[] {one, two, 1}, new object[] {two, one, 1}});
                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod1Stream2HistStarSubordinateCartesianLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt#lastevent as S0, " +
                                 "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                                 "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                                 "order by h0.val, h1.val";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "Id","valh0","valh1" };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E1", 1, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11"}});

                SendBeanInt(env, "E2", 2, 0);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                env.Milestone(0);

                SendBeanInt(env, "E3", 0, 1);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E3", 2, 2);
                object[][] result = {
                    new object[] {"E3", "H01", "H11"}, new object[] {"E3", "H01", "H12"},
                    new object[] {"E3", "H02", "H11"}, new object[] {"E3", "H02", "H12"}
                };
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, result);

                env.Milestone(0);

                SendBeanInt(env, "E4", 2, 1);
                result = new[] {new object[] {"E4", "H01", "H11"}, new object[] {"E4", "H02", "H11"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, result);

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod1Stream2HistStarSubordinateJoinedKeepall : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt#keepall as S0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             "where h0.index = h1.index and h0.index = P02";
                TryAssertionOne(env, expression);

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1   from " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "SupportBeanInt#keepall as S0 " +
                             "where h0.index = h1.index and h0.index = P02";
                TryAssertionOne(env, expression);
            }

            private static void TryAssertionOne(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "Id","valh0","valh1" };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E1", 20, 20, 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "H03", "H13"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H03", "H13"}});

                SendBeanInt(env, "E2", 20, 20, 21);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H03", "H13"}});

                SendBeanInt(env, "E3", 4, 4, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E3", "H02", "H12"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H03", "H13"}, new object[] {"E3", "H02", "H12"}});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod1Stream2HistForwardSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;
                var milestone = new AtomicLong();

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1 " +
                             "from SupportBeanInt#keepall as S0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal(h0.val, P01) as h1 " +
                             "order by h0.val, h1.val";
                TryAssertionTwo(env, expression, milestone);

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1 from " +
                             "method:SupportJoinMethods.FetchVal(h0.val, P01) as h1, " +
                             "SupportBeanInt#keepall as S0, " +
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

                var fields = new [] { "Id","valh0","valh1" };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E1", 1, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "H01", "H011"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H011"}});

                SendBeanInt(env, "E2", 0, 1);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H011"}});

                env.MilestoneInc(milestone);

                SendBeanInt(env, "E3", 1, 0);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H011"}});

                SendBeanInt(env, "E4", 2, 2);
                object[][] result = {
                    new object[] {"E4", "H01", "H011"}, new object[] {"E4", "H01", "H012"},
                    new object[] {"E4", "H02", "H021"}, new object[] {"E4", "H02", "H022"}
                };
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(
                        result,
                        new[] {new object[] {"E1", "H01", "H011"}}));

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod1Stream3HistStarSubordinateCartesianLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt#lastevent as S0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal('H2', P02) as h2 " +
                             "order by h0.val, h1.val, h2.val";
                TryAssertionThree(env, expression);

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal('H2', P02) as h2, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "SupportBeanInt#lastevent as S0 " +
                             "order by h0.val, h1.val, h2.val";
                TryAssertionThree(env, expression);
            }

            private static void TryAssertionThree(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "Id","valh0","valh1","valh2" };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E1", 1, 1, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11", "H21"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11", "H21"}});

                SendBeanInt(env, "E2", 1, 1, 2);
                object[][] result =
                    {new object[] {"E2", "H01", "H11", "H21"}, new object[] {"E2", "H01", "H11", "H22"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, result);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, result);

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod1Stream3HistForwardSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt#keepall as S0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'H2', P02) as h2 " +
                             " where h0.index = h1.index and h1.index = h2.index and h2.index = P03";
                TryAssertionFour(env, expression);

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'H2', P02) as h2, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "SupportBeanInt#keepall as S0, " +
                             "method:SupportJoinMethods.FetchVal('H1', P01) as h1 " +
                             " where h0.index = h1.index and h1.index = h2.index and h2.index = P03";
                TryAssertionFour(env, expression);
            }

            private static void TryAssertionFour(
                RegressionEnvironment env,
                string expression)
            {
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "Id","valh0","valh1","valh2" };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E1", 2, 2, 2, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11", "H01H21"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11", "H01H21"}});

                SendBeanInt(env, "E2", 4, 4, 4, 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", "H03", "H13", "H03H23"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", "H01", "H11", "H01H21"}, new object[] {"E2", "H03", "H13", "H03H23"}});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod1Stream3HistChainSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string expression;

                expression = "@name('s0') select S0.Id as Id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                             "from SupportBeanInt#keepall as S0, " +
                             "method:SupportJoinMethods.FetchVal('H0', P00) as h0, " +
                             "method:SupportJoinMethods.FetchVal(h0.val||'H1', P01) as h1, " +
                             "method:SupportJoinMethods.FetchVal(h1.val||'H2', P02) as h2 " +
                             " where h0.index = h1.index and h1.index = h2.index and h2.index = P03";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "Id","valh0","valh1","valh2" };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "E2", 4, 4, 4, 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", "H03", "H03H13", "H03H13H23"}});

                SendBeanInt(env, "E2", 4, 4, 4, 5);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);

                env.Milestone(0);

                SendBeanInt(env, "E2", 4, 4, 0, 1);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2", "H03", "H03H13", "H03H13H23"}});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod2Stream2HistStarSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select S0.Id as ids0, S1.Id as ids1, h0.val as valh0, h1.val as valh1 " +
                                 "from SupportBeanInt(Id like 'S0%')#keepall as S0, " +
                                 "SupportBeanInt(Id like 'S1%')#lastevent as S1, " +
                                 "method:SupportJoinMethods.FetchVal(S0.Id||'H1', S0.P00) as h0, " +
                                 "method:SupportJoinMethods.FetchVal(S1.Id||'H2', S1.P00) as h1 " +
                                 "order by S0.Id asc";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "ids0","ids1","valh0","valh1" };
                SendBeanInt(env, "S00", 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanInt(env, "S10", 1);
                object[][] resultOne = {new object[] {"S00", "S10", "S00H11", "S10H21"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, resultOne);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, resultOne);

                SendBeanInt(env, "S01", 1);
                object[][] resultTwo = {new object[] {"S01", "S10", "S01H11", "S10H21"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, resultTwo);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

                env.Milestone(0);

                SendBeanInt(env, "S11", 1);
                object[][] resultThree =
                    {new object[] {"S00", "S11", "S00H11", "S11H21"}, new object[] {"S01", "S11", "S01H11", "S11H21"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, resultThree);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultThree));

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod3Stream1HistSubordinate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var expression = "@name('s0') select S0.Id as ids0, S1.Id as ids1, S2.Id as ids2, h0.val as valh0 " +
                                 "from SupportBeanInt(Id like 'S0%')#keepall as S0, " +
                                 "SupportBeanInt(Id like 'S1%')#lastevent as S1, " +
                                 "SupportBeanInt(Id like 'S2%')#lastevent as S2, " +
                                 "method:SupportJoinMethods.FetchVal(S1.Id||S2.Id||'H1', S0.P00) as h0 " +
                                 "order by S0.Id, S1.Id, S2.Id, h0.val";
                env.CompileDeploy(expression).AddListener("s0");

                var fields = new [] { "ids0","ids1","ids2","valh0" };
                SendBeanInt(env, "S00", 2);
                SendBeanInt(env, "S10", 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanInt(env, "S20", 1);
                object[][] resultOne =
                    {new object[] {"S00", "S10", "S20", "S10S20H11"}, new object[] {"S00", "S10", "S20", "S10S20H12"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, resultOne);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, resultOne);

                SendBeanInt(env, "S01", 1);
                object[][] resultTwo = {new object[] {"S01", "S10", "S20", "S10S20H11"}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, resultTwo);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

                env.Milestone(0);

                SendBeanInt(env, "S21", 1);
                object[][] resultThree = {
                    new object[] {"S00", "S10", "S21", "S10S21H11"}, new object[] {"S00", "S10", "S21", "S10S21H12"},
                    new object[] {"S01", "S10", "S21", "S10S21H11"}
                };
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, resultThree);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    EPAssertionUtil.ConcatenateArray2Dim(resultThree));

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod3HistPureNoSubordinate : RegressionExecution
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

                var fields = new [] { "valh0","valh1","valh2" };

                SendBeanInt(env, "S00", 1, 1, 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"H01", "H11", "H21"}});

                SendBeanInt(env, "S01", 0, 1, 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                env.MilestoneInc(milestone);

                SendBeanInt(env, "S02", 1, 1, 0);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "S03", 1, 1, 2);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"H01", "H11", "H21"}, new object[] {"H01", "H11", "H22"}});

                SendBeanInt(env, "S04", 2, 2, 1);
                object[][] result = {
                    new object[] {"H01", "H11", "H21"}, new object[] {"H02", "H11", "H21"},
                    new object[] {"H01", "H12", "H21"}, new object[] {"H02", "H12", "H21"}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, result);

                env.UndeployModuleContaining("s0");
            }
        }

        internal class EPLFromClauseMethod3Hist1Subordinate : RegressionExecution
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

                var fields = new [] { "valh0","valh1","valh2" };

                SendBeanInt(env, "S00", 1, 1, 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"H01", "H11", "H01-H21"}});

                SendBeanInt(env, "S01", 0, 1, 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "S02", 1, 1, 0);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendBeanInt(env, "S03", 1, 1, 2);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"H01", "H11", "H01-H21"}, new object[] {"H01", "H11", "H01-H22"}});

                SendBeanInt(env, "S04", 2, 2, 1);
                object[][] result = {
                    new object[] {"H01", "H11", "H01-H21"}, new object[] {"H02", "H11", "H02-H21"},
                    new object[] {"H01", "H12", "H01-H21"}, new object[] {"H02", "H12", "H02-H21"}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, result);

                env.UndeployModuleContaining("s0");
            }
        }

        internal class EPLFromClauseMethod3Hist2SubordinateChain : RegressionExecution
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

        [Serializable]
        public class ComputeCorrelationResult
        {
            public ComputeCorrelationResult(int correlation)
            {
                Correlation = correlation;
            }

            public int Correlation { get; }
        }
    }
} // end of namespace