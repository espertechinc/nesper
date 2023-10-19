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

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabase3StreamOuterJoin
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInnerJoinLeftS0(execs);
            WithOuterJoinLeftS0(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOuterJoinLeftS0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseOuterJoinLeftS0());
            return execs;
        }

        public static IList<RegressionExecution> WithInnerJoinLeftS0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInnerJoinLeftS0());
            return execs;
        }

        private class EPLDatabaseInnerJoinLeftS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from SupportBean#lastevent sb" +
                               " inner join " +
                               " SupportBeanTwo#lastevent sbt" +
                               " on sb.theString = sbt.stringTwo " +
                               " inner join " +
                               " sql:MyDBWithRetain ['select myint from mytesttable'] as s1 " +
                               "  on s1.myint = sbt.intPrimitiveTwo";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBeanTwo("T1", 2));
                env.SendEventBean(new SupportBean("T1", -1));

                env.SendEventBean(new SupportBeanTwo("T2", 30));
                env.SendEventBean(new SupportBean("T2", -1));
                env.AssertPropsNew(
                    "s0",
                    "sb.theString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] { "T2", "T2", 30 });

                env.SendEventBean(new SupportBean("T3", -1));
                env.SendEventBean(new SupportBeanTwo("T3", 40));
                env.AssertPropsNew(
                    "s0",
                    "sb.theString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] { "T3", "T3", 40 });

                env.UndeployAll();
            }
        }

        private class EPLDatabaseOuterJoinLeftS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from SupportBean#lastevent sb" +
                               " left outer join " +
                               " SupportBeanTwo#lastevent sbt" +
                               " on sb.theString = sbt.stringTwo " +
                               " left outer join " +
                               " sql:MyDBWithRetain ['select myint from mytesttable'] as s1 " +
                               "  on s1.myint = sbt.intPrimitiveTwo";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBeanTwo("T1", 2));
                env.SendEventBean(new SupportBean("T1", 3));
                env.AssertPropsNew(
                    "s0",
                    "sb.theString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] { "T1", "T1", null });

                env.SendEventBean(new SupportBeanTwo("T2", 30));
                env.SendEventBean(new SupportBean("T2", -2));
                env.AssertPropsNew(
                    "s0",
                    "sb.theString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] { "T2", "T2", 30 });

                env.SendEventBean(new SupportBean("T3", -1));
                env.AssertPropsNew(
                    "s0",
                    "sb.theString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] { "T3", null, null });

                env.SendEventBean(new SupportBeanTwo("T3", 40));
                env.AssertPropsNew(
                    "s0",
                    "sb.theString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] { "T3", "T3", 40 });

                env.UndeployAll();
            }
        }
    }
} // end of namespace