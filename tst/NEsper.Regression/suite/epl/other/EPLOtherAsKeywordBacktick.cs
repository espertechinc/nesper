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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherAsKeywordBacktick
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithFAFUpdateDelete(execs);
            WithFromClause(execs);
            WithOnTrigger(execs);
            WithUpdateIStream(execs);
            WithnMergeAndUpdateAndSelect(execs);
            WithSubselect(execs);
            With(OnSelectProperty)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOnSelectProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherOnSelectProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithnMergeAndUpdateAndSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOthernMergeAndUpdateAndSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithUpdateIStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateIStream());
            return execs;
        }

        public static IList<RegressionExecution> WithOnTrigger(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherOnTrigger());
            return execs;
        }

        public static IList<RegressionExecution> WithFromClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFromClause());
            return execs;
        }

        public static IList<RegressionExecution> WithFAFUpdateDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherFAFUpdateDelete());
            return execs;
        }

        private class EPLOtherOnSelectProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "on OrderBean insert into ABC select * " +
                               "insert into DEF select `Order`.ReviewId from [Books][Reviews] `Order`";
                env.CompileDeploy(stmtText);
                env.UndeployAll();
            }
        }

        private class EPLOtherSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select `Order`.P00 from SupportBean_S0#lastevent as `Order`) as c0 from SupportBean_S1";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "A"));
                env.SendEventBean(new SupportBean_S1(2));
                env.AssertEqualsNew("s0", "c0", "A");

                env.UndeployAll();
            }
        }

        private class EPLOthernMergeAndUpdateAndSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindowMerge#keepall as (p0 string, p1 string)", path);
                env.CompileExecuteFAFNoResult("insert into MyWindowMerge select 'a' as p0, 'b' as p1", path);
                env.CompileDeploy(
                    "on SupportBean_S0 merge MyWindowMerge as `Order` when matched then update set `Order`.p1 = `Order`.p0",
                    path);
                env.CompileDeploy("on SupportBean_S1 update MyWindowMerge as `Order` set p0 = 'x'", path);

                AssertFAF(env, path, "MyWindowMerge", "a", "b");

                env.SendEventBean(new SupportBean_S0(1));
                AssertFAF(env, path, "MyWindowMerge", "a", "a");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(1, "x"));
                AssertFAF(env, path, "MyWindowMerge", "x", "a");

                env.CompileDeploy(
                        "@name('s0') on SupportBean select `Order`.p0 as c0 from MyWindowMerge as `Order`",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "c0", "x");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class EPLOtherFAFUpdateDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindowFAF#keepall as (p0 string, p1 string)", path);
                env.CompileExecuteFAF("insert into MyWindowFAF select 'a' as p0, 'b' as p1", path);
                AssertFAF(env, path, "MyWindowFAF", "a", "b");

                env.CompileExecuteFAF("update MyWindowFAF as `Order` set `Order`.p0 = `Order`.p1", path);
                AssertFAF(env, path, "MyWindowFAF", "b", "b");

                env.Milestone(0);

                env.CompileExecuteFAF("delete from MyWindowFAF as `Order` where `Order`.p0 = 'b'", path);
                ClassicAssert.AreEqual(0, env.CompileExecuteFAF("select * from MyWindowFAF", path).Array.Length);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class EPLOtherUpdateIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("update istream SupportBean_S0 as `Order` set P00=`Order`.P01");
                var epl = "@name('s0') select * from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "a", "x"));
                env.AssertEqualsNew("s0", "P00", "x");

                env.UndeployAll();
            }
        }

        private class EPLOtherOnTrigger : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create table MyTable(k1 string primary key, v1 string)", path);
                env.CompileExecuteFAFNoResult("insert into MyTable select 'x' as k1, 'y' as v1", path);
                env.CompileExecuteFAFNoResult("insert into MyTable select 'a' as k1, 'b' as v1", path);

                var epl = "@name('s0') on SupportBean_S0 as `Order` select v1 from MyTable where `Order`.P00 = k1";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "a"));
                env.AssertEqualsNew("s0", "v1", "b");

                env.UndeployAll();
            }
        }

        private class EPLOtherFromClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBean_S0#lastevent as `Order`, SupportBean_S1#lastevent as `select`";
                env.CompileDeploy(epl).AddListener("s0");

                var s0 = new SupportBean_S0(1, "S0_1");
                env.SendEventBean(s0);

                env.Milestone(0);

                var s1 = new SupportBean_S1(10, "S1_1");
                env.SendEventBean(s1);
                env.AssertPropsNew(
                    "s0",
                    "Order,select,Order.P00,select.P10".SplitCsv(),
                    new object[] { s0, s1, "S0_1", "S1_1" });

                env.UndeployAll();
            }
        }

        private static void AssertFAF(
            RegressionEnvironment env,
            RegressionPath path,
            string windowName,
            string p0,
            string p1)
        {
            EPAssertionUtil.AssertProps(
                env.CompileExecuteFAF("select * from " + windowName, path).Array[0],
                "p0,p1".SplitCsv(),
                new object[] { p0, p1 });
        }
    }
} // end of namespace