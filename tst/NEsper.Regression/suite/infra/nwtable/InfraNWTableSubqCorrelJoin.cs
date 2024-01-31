///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqCorrelJoin
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithJoinAssertion(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithJoinAssertion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            // named window
            execs.Add(new InfraNWTableSubqCorrelJoinAssertion(true, false));
            execs.Add(new InfraNWTableSubqCorrelJoinAssertion(true, true));
            // table
            execs.Add(new InfraNWTableSubqCorrelJoinAssertion(false, false));
            return execs;
        }

        private class InfraNWTableSubqCorrelJoinAssertion : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly bool enableIndexShareCreate;

            public InfraNWTableSubqCorrelJoinAssertion(
                bool namedWindow,
                bool enableIndexShareCreate)
            {
                this.namedWindow = namedWindow;
                this.enableIndexShareCreate = enableIndexShareCreate;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = namedWindow
                    ? "@Public create window MyInfra#unique(TheString) as select * from SupportBean"
                    : "@Public create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
                if (enableIndexShareCreate) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyInfra select TheString, IntPrimitive from SupportBean", path);

                var consumeEpl =
                    "@name('s0') select (select IntPrimitive from MyInfra where TheString = s1.P10) as val from SupportBean_S0#lastevent as s0, SupportBean_S1#lastevent as s1";
                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                var fields = "val".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E3", 30));

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(1, "E2"));
                env.AssertPropsNew("s0", fields, new object[] { 20 });

                env.SendEventBean(new SupportBean_S0(1, "E3"));
                env.AssertPropsNew("s0", fields, new object[] { 20 });

                env.SendEventBean(new SupportBean_S1(1, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { 10 });

                env.SendEventBean(new SupportBean_S1(1, "E3"));
                env.AssertPropsNew("s0", fields, new object[] { 30 });

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       ", enableIndexShareCreate=" +
                       enableIndexShareCreate +
                       '}';
            }
        }
    }
} // end of namespace