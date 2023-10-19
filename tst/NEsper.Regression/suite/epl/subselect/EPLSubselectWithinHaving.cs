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

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectWithinHaving
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithGroupBy(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectHavingSubselectWithGroupBy(true));
            execs.Add(new EPLSubselectHavingSubselectWithGroupBy(false));
            return execs;
        }

        private class EPLSubselectHavingSubselectWithGroupBy : RegressionExecution
        {
            private readonly bool namedWindow;

            public EPLSubselectHavingSubselectWithGroupBy(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "@public create window MyInfra#unique(key) as SupportMaxAmountEvent"
                    : "@public create table MyInfra(key string primary key, maxAmount double)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfra select * from SupportMaxAmountEvent", path);

                var stmtText = "@name('s0') select theString as c0, sum(intPrimitive) as c1 " +
                               "from SupportBean#groupwin(theString)#length(2) as sb " +
                               "group by theString " +
                               "having sum(intPrimitive) > (select maxAmount from MyInfra as mw where sb.theString = mw.key)";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                var fields = "c0,c1".SplitCsv();

                // set some amounts
                env.SendEventBean(new SupportMaxAmountEvent("G1", 10));
                env.SendEventBean(new SupportMaxAmountEvent("G2", 20));
                env.SendEventBean(new SupportMaxAmountEvent("G3", 30));

                // send some events
                env.SendEventBean(new SupportBean("G1", 5));
                env.SendEventBean(new SupportBean("G2", 19));
                env.SendEventBean(new SupportBean("G3", 28));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 21 });

                env.SendEventBean(new SupportBean("G2", 18));
                env.SendEventBean(new SupportBean("G1", 4));
                env.SendEventBean(new SupportBean("G3", 2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G3", 29));
                env.AssertPropsNew("s0", fields, new object[] { "G3", 31 });

                env.SendEventBean(new SupportBean("G3", 4));
                env.AssertPropsNew("s0", fields, new object[] { "G3", 33 });

                env.SendEventBean(new SupportBean("G1", 6));
                env.SendEventBean(new SupportBean("G2", 2));
                env.SendEventBean(new SupportBean("G3", 26));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G1", 99));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 105 });

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 100 });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }
    }
} // end of namespace