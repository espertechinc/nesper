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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherAsKeywordBacktick
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherFAFUpdateDelete());
            execs.Add(new EPLOtherFromClause());
            execs.Add(new EPLOtherOnTrigger());
            execs.Add(new EPLOtherUpdateIStream());
            execs.Add(new EPLOthernMergeAndUpdateAndSelect());
            execs.Add(new EPLOtherSubselect());
            execs.Add(new EPLOtherOnSelectProperty());
            return execs;
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
                new object[] {p0, p1});
        }

        internal class EPLOtherOnSelectProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "on OrderBean insert into ABC select * " +
                               "insert into DEF select `order`.reviewId from [books][reviews] `order`";
                env.CompileDeploy(stmtText);
                env.UndeployAll();
            }
        }

        internal class EPLOtherSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select `order`.P00 from SupportBean_S0#lastevent as `order`) as c0 from SupportBean_S1";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "A"));
                env.SendEventBean(new SupportBean_S1(2));
                Assert.AreEqual("A", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class EPLOthernMergeAndUpdateAndSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindowMerge#keepall as (p0 string, p1 string)", path);
                env.CompileExecuteFAF("insert into MyWindowMerge select 'a' as p0, 'b' as p1", path);
                env.CompileDeploy(
                    "on SupportBean_S0 merge MyWindowMerge as `order` when matched then update set `order`.p1 = `order`.p0",
                    path);
                env.CompileDeploy("on SupportBean_S1 update MyWindowMerge as `order` set p0 = 'x'", path);

                AssertFAF(env, path, "MyWindowMerge", "a", "b");

                env.SendEventBean(new SupportBean_S0(1));
                AssertFAF(env, path, "MyWindowMerge", "a", "a");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(1, "x"));
                AssertFAF(env, path, "MyWindowMerge", "x", "a");

                env.CompileDeploy(
                        "@Name('s0') on SupportBean select `order`.p0 as c0 from MyWindowMerge as `order`",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.AreEqual("x", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherFAFUpdateDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindowFAF#keepall as (p0 string, p1 string)", path);
                env.CompileExecuteFAF("insert into MyWindowFAF select 'a' as p0, 'b' as p1", path);
                AssertFAF(env, path, "MyWindowFAF", "a", "b");

                env.CompileExecuteFAF("update MyWindowFAF as `order` set `order`.p0 = `order`.p1", path);
                AssertFAF(env, path, "MyWindowFAF", "b", "b");

                env.Milestone(0);

                env.CompileExecuteFAF("delete from MyWindowFAF as `order` where `order`.p0 = 'b'", path);
                Assert.AreEqual(0, env.CompileExecuteFAF("select * from MyWindowFAF", path).Array.Length);

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("update istream SupportBean_S0 as `order` set P00=`order`.P01");
                var epl = "@Name('s0') select * from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "a", "x"));
                Assert.AreEqual("x", env.Listener("s0").AssertOneGetNewAndReset().Get("P00"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherOnTrigger : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTable(k1 string primary key, v1 string)", path);
                env.CompileExecuteFAF("insert into MyTable select 'x' as k1, 'y' as v1", path);
                env.CompileExecuteFAF("insert into MyTable select 'a' as k1, 'b' as v1", path);

                var epl = "@Name('s0') on SupportBean_S0 as `order` select v1 from MyTable where `order`.P00 = k1";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "a"));
                Assert.AreEqual("b", env.Listener("s0").AssertOneGetNewAndReset().Get("v1"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherFromClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from SupportBean_S0#lastevent as `order`, SupportBean_S1#lastevent as `select`";
                env.CompileDeploy(epl).AddListener("s0");

                var s0 = new SupportBean_S0(1, "S0_1");
                env.SendEventBean(s0);

                env.Milestone(0);

                var s1 = new SupportBean_S1(10, "S1_1");
                env.SendEventBean(s1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "order,select,order.P00,select.P10".SplitCsv(),
                    new object[] {s0, s1, "S0_1", "S1_1"});

                env.UndeployAll();
            }
        }
    }
} // end of namespace