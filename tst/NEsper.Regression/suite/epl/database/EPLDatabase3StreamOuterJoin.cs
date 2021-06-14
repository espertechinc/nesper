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

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabase3StreamOuterJoin
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInnerJoinLeftS0());
            execs.Add(new EPLDatabaseOuterJoinLeftS0());
            return execs;
        }

        internal class EPLDatabaseInnerJoinLeftS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportBean#lastevent sb" +
                               " inner join " +
                               " SupportBeanTwo#lastevent sbt" +
                               " on sb.TheString = sbt.stringTwo " +
                               " inner join " +
                               " sql:MyDBWithRetain ['select myint from mytesttable'] as S1 " +
                               "  on S1.myint = sbt.IntPrimitiveTwo";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBeanTwo("T1", 2));
                env.SendEventBean(new SupportBean("T1", -1));

                env.SendEventBean(new SupportBeanTwo("T2", 30));
                env.SendEventBean(new SupportBean("T2", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "sb.TheString","sbt.stringTwo","S1.myint" },
                    new object[] {"T2", "T2", 30});

                env.SendEventBean(new SupportBean("T3", -1));
                env.SendEventBean(new SupportBeanTwo("T3", 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "sb.TheString","sbt.stringTwo","S1.myint" },
                    new object[] {"T3", "T3", 40});

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseOuterJoinLeftS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportBean#lastevent sb" +
                               " left outer join " +
                               " SupportBeanTwo#lastevent sbt" +
                               " on sb.TheString = sbt.stringTwo " +
                               " left outer join " +
                               " sql:MyDBWithRetain ['select myint from mytesttable'] as S1 " +
                               "  on S1.myint = sbt.IntPrimitiveTwo";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBeanTwo("T1", 2));
                env.SendEventBean(new SupportBean("T1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "sb.TheString","sbt.stringTwo","S1.myint" },
                    new object[] {"T1", "T1", null});

                env.SendEventBean(new SupportBeanTwo("T2", 30));
                env.SendEventBean(new SupportBean("T2", -2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "sb.TheString","sbt.stringTwo","S1.myint" },
                    new object[] {"T2", "T2", 30});

                env.SendEventBean(new SupportBean("T3", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "sb.TheString","sbt.stringTwo","S1.myint" },
                    new object[] {"T3", null, null});

                env.SendEventBean(new SupportBeanTwo("T3", 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "sb.TheString","sbt.stringTwo","S1.myint" },
                    new object[] {"T3", "T3", 40});

                env.UndeployAll();
            }
        }
    }
} // end of namespace