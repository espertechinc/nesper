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

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFirstEverLastEver
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFirstLastEver(execs);
            WithFirstLastInvalid(execs);
            WithOnDelete(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOnDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastEver(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastEver(true));
            execs.Add(new ResultSetAggregateFirstLastEver(false));
            return execs;
        }

        private class ResultSetAggregateFirstLastInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select countever(distinct intPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'countever(distinct intPrimitive)': Aggregation function 'countever' does now allow distinct [");
            }
        }

        private class ResultSetAggregateOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "firsteverstring,lasteverstring,counteverall".SplitCsv();
                var epl = "create window MyWindow#keepall as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_A delete from MyWindow where theString = id;\n" +
                          "@name('s0') select firstever(theString) as firsteverstring, " +
                          "lastever(theString) as lasteverstring," +
                          "countever(*) as counteverall from MyWindow";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 1L });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E2", 2L });

                env.SendEventBean(new SupportBean("E3", 30));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E3", 3L });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E3", 3L });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_A("E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E3", 3L });

                env.SendEventBean(new SupportBean_A("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E3", 3L });

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
                var epl = "@Audit @Name('s0') select " +
                          "firstever(theString) as firsteverstring, " +
                          "lastever(theString) as lasteverstring, " +
                          "first(theString) as firststring, " +
                          "last(theString) as laststring, " +
                          "countever(*) as cntstar, " +
                          "countever(intBoxed) as cntexpr, " +
                          "countever(*,boolPrimitive) as cntstarfiltered, " +
                          "countever(intBoxed,boolPrimitive) as cntexprfiltered " +
                          "from SupportBean.win:length(2)";
                env.CompileDeploy(soda, epl).AddListener("s0");

                var fields =
                    "firsteverstring,lasteverstring,firststring,laststring,cntstar,cntexpr,cntstarfiltered,cntexprfiltered"
                        .SplitCsv();

                env.Milestone(0);

                MakeSendBean(env, "E1", 10, 100, true);
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", "E1", "E1", 1L, 1L, 1L, 1L });

                env.Milestone(1);

                MakeSendBean(env, "E2", 11, null, true);
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E2", "E1", "E2", 2L, 1L, 2L, 1L });

                env.Milestone(2);

                MakeSendBean(env, "E3", 12, 120, false);
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E3", "E2", "E3", 3L, 2L, 2L, 1L });

                env.Milestone(3);

                MakeSendBean(env, "E4", 13, 130, true);
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E4", "E3", "E4", 4L, 3L, 3L, 2L });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
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
    }
} // end of namespace