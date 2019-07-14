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

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFirstEverLastEver
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastEver(true));
            execs.Add(new ResultSetAggregateFirstLastEver(false));
            execs.Add(new ResultSetAggregateFirstLastInvalid());
            execs.Add(new ResultSetAggregateOnDelete());
            return execs;
        }

        private static void MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed,
            bool boolPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.IntBoxed = intBoxed;
            sb.BoolPrimitive = boolPrimitive;
            env.SendEventBean(sb);
        }

        internal class ResultSetAggregateFirstLastInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select countever(distinct IntPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'countever(distinct IntPrimitive)': Aggregation function 'countever' does now allow distinct [");
            }
        }

        internal class ResultSetAggregateOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "firsteverstring,lasteverstring,counteverall".SplitCsv();
                var epl = "create window MyWindow#keepall as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_A delete from MyWindow where theString = id;\n" +
                          "@Name('s0') select firstever(TheString) as firsteverstring, " +
                          "lastever(TheString) as lasteverstring," +
                          "countever(*) as counteverall from MyWindow";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", 1L});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E2", 2L});

                env.SendEventBean(new SupportBean("E3", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3", 3L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3", 3L});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_A("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3", 3L});

                env.SendEventBean(new SupportBean_A("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3", 3L});

                env.UndeployAll();
            }
        }

        public class ResultSetAggregateFirstLastEver : RegressionExecution
        {
            private readonly bool soda;

            public ResultSetAggregateFirstLastEver(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "firstever(TheString) as firsteverstring, " +
                          "lastever(TheString) as lasteverstring, " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "countever(*) as cntstar, " +
                          "countever(intBoxed) as cntexpr, " +
                          "countever(*,BoolPrimitive) as cntstarfiltered, " +
                          "countever(intBoxed,BoolPrimitive) as cntexprfiltered " +
                          "from SupportBean.win:length(2)";
                env.CompileDeploy(soda, epl).AddListener("s0");

                var fields =
                    "firsteverstring,lasteverstring,firststring,laststring,cntstar,cntexpr,cntstarfiltered,cntexprfiltered"
                        .SplitCsv();

                env.Milestone(0);

                MakeSendBean(env, "E1", 10, 100, true);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", "E1", "E1", 1L, 1L, 1L, 1L});

                env.Milestone(1);

                MakeSendBean(env, "E2", 11, null, true);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E2", "E1", "E2", 2L, 1L, 2L, 1L});

                env.Milestone(2);

                MakeSendBean(env, "E3", 12, 120, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3", "E2", "E3", 3L, 2L, 2L, 1L});

                env.Milestone(3);

                MakeSendBean(env, "E4", 13, 130, true);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E4", "E3", "E4", 4L, 3L, 3L, 2L});

                env.UndeployAll();
            }
        }
    }
} // end of namespace