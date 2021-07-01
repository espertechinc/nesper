///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectIn
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInSelect(execs);
            WithInSelectOM(execs);
            WithInSelectCompile(execs);
            WithInSelectWhere(execs);
            WithInSelectWhereExpressions(execs);
            WithInFilterCriteria(execs);
            WithInWildcard(execs);
            WithInNullable(execs);
            WithInNullableCoercion(execs);
            WithInNullRow(execs);
            WithInSingleIndex(execs);
            WithInMultiIndex(execs);
            WithNotInNullRow(execs);
            WithNotInSelect(execs);
            WithNotInNullableCoercion(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNotInNullableCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNotInNullableCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithNotInSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNotInSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithNotInNullRow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectNotInNullRow());
            return execs;
        }

        public static IList<RegressionExecution> WithInMultiIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInMultiIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithInSingleIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSingleIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithInNullRow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInNullRow());
            return execs;
        }

        public static IList<RegressionExecution> WithInNullableCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInNullableCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithInNullable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInNullable());
            return execs;
        }

        public static IList<RegressionExecution> WithInWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithInFilterCriteria(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInFilterCriteria());
            return execs;
        }

        public static IList<RegressionExecution> WithInSelectWhereExpressions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSelectWhereExpressions());
            return execs;
        }

        public static IList<RegressionExecution> WithInSelectWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSelectWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithInSelectCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSelectCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithInSelectOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSelectOM());
            return execs;
        }

        public static IList<RegressionExecution> WithInSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSelect());
            return execs;
        }

        private static void RunTestInSelect(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(2));
            Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            env.SendEventBean(new SupportBean_S1(-1));
            env.SendEventBean(new SupportBean_S0(2));
            Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            env.SendEventBean(new SupportBean_S0(-1));
            Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            env.SendEventBean(new SupportBean_S1(5));
            env.SendEventBean(new SupportBean_S0(4));
            Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            env.SendEventBean(new SupportBean_S0(5));
            Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
        }

        private static void SendBean(
            RegressionEnvironment env,
            string theString,
            int? intBoxed,
            long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }

        internal class EPLSubselectInSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id in (select Id from SupportBean_S1#length(1000)) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                AssertStatelessStmt(env, "s0", false);

                RunTestInSelect(env);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInSelectOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.Create("Id");
                subquery.FromClause =
                    FromClause.Create(
                        FilterStream.Create("SupportBean_S1")
                            .AddView(View.Create("length", Expressions.Constant(1000))));

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.SubqueryIn("Id", subquery), "value");
                model = env.CopyMayFail(model);

                var stmtText = "select Id in (select Id from SupportBean_S1#length(1000)) as value from SupportBean_S0";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                RunTestInSelect(env);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInSelectCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id in (select Id from SupportBean_S1#length(1000)) as value from SupportBean_S0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                RunTestInSelect(env);

                env.UndeployAll();
            }
        }

        public class EPLSubselectInFilterCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Id"};
                var text = "@Name('s0') select Id from SupportBean_S0(Id in (select Id from SupportBean_S1#length(2)))";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(10));

                env.SendEventBean(new SupportBean_S0(10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});
                env.SendEventBean(new SupportBean_S0(11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});
                env.SendEventBean(new SupportBean_S0(11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(11));
                env.SendEventBean(new SupportBean_S0(11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11});

                env.SendEventBean(new SupportBean_S1(12)); //pushing 10 out

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean_S0(11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11});
                env.SendEventBean(new SupportBean_S0(12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInSelectWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id in (select Id from SupportBean_S1#length(1000) where Id > 0) as value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S0(-1));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S1(5));
                env.SendEventBean(new SupportBean_S0(4));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S0(5));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInSelectWhereExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select 3*Id in (select 2*Id from SupportBean_S1#length(1000)) as value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S0(-1));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S1(6));
                env.SendEventBean(new SupportBean_S0(4));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select S0.AnyObject in (select * from SupportBean_S1#length(1000)) as value from SupportBeanArrayCollMap S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                var s1 = new SupportBean_S1(100);
                var arrayBean = new SupportBeanArrayCollMap(s1);
                env.SendEventBean(s1);
                env.SendEventBean(arrayBean);
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                var s2 = new SupportBean_S2(100);
                arrayBean.AnyObject = s2;
                env.SendEventBean(s2);
                env.SendEventBean(arrayBean);
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInNullable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id from SupportBean_S0 as S0 where P00 in (select P10 from SupportBean_S1#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(1, "a"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(2, null));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(-1, "A"));
                env.SendEventBean(new SupportBean_S0(3, null));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(4, "A"));
                Assert.AreEqual(4, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.SendEventBean(new SupportBean_S1(-2, null));
                env.SendEventBean(new SupportBean_S0(5, null));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInNullableCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select LongBoxed from SupportBean(TheString='A') as S0 " +
                               "where LongBoxed in " +
                               "(select IntBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "A", 0, 0L);
                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "B", null, null);

                SendBean(env, "A", 0, 0L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "B", 99, null);

                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendBean(env, "A", null, 99L);
                Assert.AreEqual(99L, env.Listener("s0").AssertOneGetNewAndReset().Get("LongBoxed"));

                SendBean(env, "B", 98, null);

                SendBean(env, "A", null, 98L);
                Assert.AreEqual(98L, env.Listener("s0").AssertOneGetNewAndReset().Get("LongBoxed"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInNullRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select IntBoxed from SupportBean(TheString='A') as S0 " +
                               "where IntBoxed in " +
                               "(select LongBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "B", 1, 1L);

                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "A", 1, 1L);
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("IntBoxed"));

                SendBean(env, "B", null, null);

                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "A", 1, 1L);
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("IntBoxed"));

                env.UndeployAll();
            }
        }

        public class EPLSubselectInSingleIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select P00 from SupportBean_S0#keepall() as S0 where S0.P01 in (S1.P10, S1.P11)) as c0 from SupportBean_S1 as S1";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "v" + i, "P00_" + i));
                }

                env.Milestone(0);

                for (var i = 0; i < 5; i++) {
                    var index = i + 4;
                    env.SendEventBean(new SupportBean_S1(index, "x", "P00_" + index));
                    Assert.AreEqual("v" + index, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                }

                env.UndeployAll();
            }
        }

        public class EPLSubselectInMultiIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select P00 from SupportBean_S0#keepall() as S0 where S1.P11 in (S0.P00, S0.P01)) as c0 from SupportBean_S1 as S1";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "v" + i, "P00_" + i));
                }

                env.Milestone(0);

                for (var i = 0; i < 5; i++) {
                    var index = i + 4;
                    env.SendEventBean(new SupportBean_S1(index, "x", "P00_" + index));
                    Assert.AreEqual("v" + index, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                }

                env.UndeployAll();
            }
        }

        internal class EPLSubselectNotInNullRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select IntBoxed from SupportBean(TheString='A') as S0 " +
                               "where IntBoxed not in " +
                               "(select LongBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "B", 1, 1L);

                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "A", 1, 1L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "B", null, null);

                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "A", 1, 1L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectNotInSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select not Id in (select Id from SupportBean_S1#length(1000)) as value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S0(-1));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S1(5));
                env.SendEventBean(new SupportBean_S0(4));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S0(5));
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectNotInNullableCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select LongBoxed from SupportBean(TheString='A') as S0 " +
                               "where LongBoxed not in " +
                               "(select IntBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "A", 0, 0L);
                Assert.AreEqual(0L, env.Listener("s0").AssertOneGetNewAndReset().Get("LongBoxed"));

                SendBean(env, "A", null, null);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("LongBoxed"));

                SendBean(env, "B", null, null);

                SendBean(env, "A", 1, 1L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "B", 99, null);

                SendBean(env, "A", null, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendBean(env, "A", null, 99L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "B", 98, null);

                SendBean(env, "A", null, 98L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBean(env, "A", null, 97L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "@Name('s0') select IntArr in (select IntPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }
        }
    }
} // end of namespace