///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;


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

        private class EPLSubselectInSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id in (select Id from SupportBean_S1#length(1000)) as Value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                SupportAdminUtil.AssertStatelessStmt(env, "s0", false);

                RunTestInSelect(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInSelectOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.Create("Id");
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView(View.Create("length", Expressions.Constant(1000))));

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.SubqueryIn("Id", subquery), "Value");
                model = env.CopyMayFail(model);

                var stmtText = "select Id in (select Id from SupportBean_S1#length(1000)) as Value from SupportBean_S0";
                ClassicAssert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                RunTestInSelect(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInSelectCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id in (select Id from SupportBean_S1#length(1000)) as Value from SupportBean_S0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                RunTestInSelect(env);

                env.UndeployAll();
            }
        }

        public class EPLSubselectInFilterCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Id" };
                var text = "@name('s0') select Id from SupportBean_S0(Id in (select Id from SupportBean_S1#length(2)))";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(10));

                env.SendEventBean(new SupportBean_S0(10));
                env.AssertPropsNew("s0", fields, new object[] { 10 });
                env.SendEventBean(new SupportBean_S0(11));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10));
                env.AssertPropsNew("s0", fields, new object[] { 10 });
                env.SendEventBean(new SupportBean_S0(11));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(11));
                env.SendEventBean(new SupportBean_S0(11));
                env.AssertPropsNew("s0", fields, new object[] { 11 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(11));
                env.AssertPropsNew("s0", fields, new object[] { 11 });

                env.SendEventBean(new SupportBean_S1(12)); //pushing 10 out

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(10));
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(new SupportBean_S0(11));
                env.AssertPropsNew("s0", fields, new object[] { 11 });
                env.SendEventBean(new SupportBean_S0(12));
                env.AssertPropsNew("s0", fields, new object[] { 12 });

                env.UndeployAll();
            }
        }

        private class EPLSubselectInSelectWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id in (select Id from SupportBean_S1#length(1000) where Id > 0) as Value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S1(5));
                env.SendEventBean(new SupportBean_S0(4));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S0(5));
                env.AssertEqualsNew("s0", "Value", true);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInSelectWhereExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select 3*Id in (select 2*Id from SupportBean_S1#length(1000)) as Value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S1(6));
                env.SendEventBean(new SupportBean_S0(4));
                env.AssertEqualsNew("s0", "Value", true);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select s0.AnyObject in (select * from SupportBean_S1#length(1000)) as Value from SupportBeanArrayCollMap s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                var s1 = new SupportBean_S1(100);
                var arrayBean = new SupportBeanArrayCollMap(s1);
                env.SendEventBean(s1);
                env.SendEventBean(arrayBean);
                env.AssertEqualsNew("s0", "Value", true);

                var s2 = new SupportBean_S2(100);
                arrayBean.AnyObject = s2;
                env.SendEventBean(s2);
                env.SendEventBean(arrayBean);
                env.AssertEqualsNew("s0", "Value", false);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInNullable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 as s0 where P00 in (select P10 from SupportBean_S1#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(1, "a"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(2, null));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(-1, "A"));
                env.SendEventBean(new SupportBean_S0(3, null));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(4, "A"));
                env.AssertEqualsNew("s0", "Id", 4);

                env.SendEventBean(new SupportBean_S1(-2, null));
                env.SendEventBean(new SupportBean_S0(5, null));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectInNullableCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select LongBoxed from SupportBean(TheString='A') as s0 " +
                               "where LongBoxed in " +
                               "(select IntBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "A", 0, 0L);
                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "B", null, null);

                SendBean(env, "A", 0, 0L);
                env.AssertListenerNotInvoked("s0");
                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "B", 99, null);

                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");
                SendBean(env, "A", null, 99L);
                env.AssertEqualsNew("s0", "LongBoxed", 99L);

                SendBean(env, "B", 98, null);

                SendBean(env, "A", null, 98L);
                env.AssertEqualsNew("s0", "LongBoxed", 98L);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInNullRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select IntBoxed from SupportBean(TheString='A') as s0 " +
                               "where IntBoxed in " +
                               "(select LongBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "B", 1, 1L);

                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "A", 1, 1L);
                env.AssertEqualsNew("s0", "IntBoxed", 1);

                SendBean(env, "B", null, null);

                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "A", 1, 1L);
                env.AssertEqualsNew("s0", "IntBoxed", 1);

                env.UndeployAll();
            }
        }

        public class EPLSubselectInSingleIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select P00 from SupportBean_S0#keepall() as s0 where s0.P01 in (s1.P10, s1.P11)) as c0 from SupportBean_S1 as s1";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "v" + i, "p00_" + i));
                }

                env.Milestone(0);

                for (var i = 0; i < 5; i++) {
                    var index = i + 4;
                    env.SendEventBean(new SupportBean_S1(index, "x", "p00_" + index));
                    env.AssertEqualsNew("s0", "c0", "v" + index);
                }

                env.UndeployAll();
            }
        }

        public class EPLSubselectInMultiIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select P00 from SupportBean_S0#keepall() as s0 where s1.P11 in (s0.P00, s0.P01)) as c0 from SupportBean_S1 as s1";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "v" + i, "p00_" + i));
                }

                env.Milestone(0);

                for (var i = 0; i < 5; i++) {
                    var index = i + 4;
                    env.SendEventBean(new SupportBean_S1(index, "x", "p00_" + index));
                    env.AssertEqualsNew("s0", "c0", "v" + index);
                }

                env.UndeployAll();
            }
        }

        private class EPLSubselectNotInNullRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select IntBoxed from SupportBean(TheString='A') as s0 " +
                               "where IntBoxed not in " +
                               "(select LongBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "B", 1, 1L);

                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "A", 1, 1L);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "B", null, null);

                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "A", 1, 1L);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectNotInSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select not Id in (select Id from SupportBean_S1#length(1000)) as Value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Value", true);

                env.SendEventBean(new SupportBean_S1(-1));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Value", true);

                env.SendEventBean(new SupportBean_S0(-1));
                env.AssertEqualsNew("s0", "Value", false);

                env.SendEventBean(new SupportBean_S1(5));
                env.SendEventBean(new SupportBean_S0(4));
                env.AssertEqualsNew("s0", "Value", true);

                env.SendEventBean(new SupportBean_S0(5));
                env.AssertEqualsNew("s0", "Value", false);

                env.UndeployAll();
            }
        }

        private class EPLSubselectNotInNullableCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select LongBoxed from SupportBean(TheString='A') as s0 " +
                               "where LongBoxed not in " +
                               "(select IntBoxed from SupportBean(TheString='B')#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendBean(env, "A", 0, 0L);
                env.AssertEqualsNew("s0", "LongBoxed", 0L);

                SendBean(env, "A", null, null);
                env.AssertEqualsNew("s0", "LongBoxed", null);

                SendBean(env, "B", null, null);

                SendBean(env, "A", 1, 1L);
                env.AssertListenerNotInvoked("s0");
                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "B", 99, null);

                SendBean(env, "A", null, null);
                env.AssertListenerNotInvoked("s0");
                SendBean(env, "A", null, 99L);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "B", 98, null);

                SendBean(env, "A", null, 98L);
                env.AssertListenerNotInvoked("s0");

                SendBean(env, "A", null, 97L);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void RunTestInSelect(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", "Value", false);

            env.SendEventBean(new SupportBean_S1(-1));
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", "Value", false);

            env.SendEventBean(new SupportBean_S0(-1));
            env.AssertEqualsNew("s0", "Value", true);

            env.SendEventBean(new SupportBean_S1(5));
            env.SendEventBean(new SupportBean_S0(4));
            env.AssertEqualsNew("s0", "Value", false);

            env.SendEventBean(new SupportBean_S0(5));
            env.AssertEqualsNew("s0", "Value", true);
        }

        private class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "@name('s0') select IntArr in (select IntPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords");
            }
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
    }
} // end of namespace