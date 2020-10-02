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
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.support.util.IndexBackingTableInfo;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherCreateIndex
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOneModule(execs);
            WithThreeModule(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithThreeModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherCreateIndexPathThreeModule());
            return execs;
        }

        public static IList<RegressionExecution> WithOneModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherCreateIndexPathOneModule());
            return execs;
        }

        internal class EPLOtherCreateIndexPathOneModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindow#keepall as (p0 string, p1 int);\n" +
                          "create unique index MyIndex on MyWindow(p0);\n" +
                          INDEX_CALLBACK_HOOK +
                          "@Name('s0') on SupportBean_S0 as S0 select p0,p1 from MyWindow as win where win.p0 = S0.P00;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIndex",
                    "unique hash={p0(string)} btree={} advanced={}");

                env.CompileExecuteFAF("insert into MyWindow select 'a' as p0, 1 as p1", path);

                env.SendEventBean(new SupportBean_S0(1, "a"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"p0", "p1"},
                    new object[] {"a", 1});

                env.UndeployAll();
            }
        }

        internal class EPLOtherCreateIndexPathThreeModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as (p0 string, p1 int);", path);
                env.CompileDeploy("create unique index MyIndex on MyWindow(p0);", path);
                env.CompileDeploy(
                    INDEX_CALLBACK_HOOK +
                    "@Name('s0') on SupportBean_S0 as S0 select p0, p1 from MyWindow as win where win.p0 = S0.P00;",
                    path);
                env.AddListener("s0");

                SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                    "MyIndex",
                    "unique hash={p0(string)} btree={} advanced={}");

                env.CompileExecuteFAF("insert into MyWindow select 'a' as p0, 1 as p1", path);

                env.SendEventBean(new SupportBean_S0(1, "a"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"p0", "p1"},
                    new object[] {"a", 1});

                env.UndeployAll();
            }
        }
    }
} // end of namespace