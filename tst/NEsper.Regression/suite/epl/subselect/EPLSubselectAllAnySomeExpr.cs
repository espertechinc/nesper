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


namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectAllAnySomeExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRelationalOpAll(execs);
            WithRelationalOpNullOrNoRows(execs);
            WithRelationalOpSome(execs);
            WithEqualsNotEqualsAll(execs);
            WithEqualsAnyOrSome(execs);
            WithEqualsInNullOrNoRows(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsInNullOrNoRows(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectEqualsInNullOrNoRows());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsAnyOrSome(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectEqualsAnyOrSome());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsNotEqualsAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectEqualsNotEqualsAll());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpSome(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectRelationalOpSome());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpNullOrNoRows(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectRelationalOpNullOrNoRows());
            return execs;
        }

        public static IList<RegressionExecution> WithRelationalOpAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectRelationalOpAll());
            return execs;
        }

        private class EPLSubselectRelationalOpAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var stmtText = "@name('s0') select " +
                               "IntPrimitive > all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as g, " +
                               "IntPrimitive >= all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as ge, " +
                               "IntPrimitive < all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as l, " +
                               "IntPrimitive <= all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as le " +
                               "from SupportBean(TheString like \"E%\")";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, true });

                env.SendEventBean(new SupportBean("S1", 1));

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, true });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false });

                env.SendEventBean(new SupportBean("S2", 2));

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false });

                env.SendEventBean(new SupportBean("E4", 2));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, false });

                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, true });

                env.SendEventBean(new SupportBean("E6", 0));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true });

                env.UndeployAll();

                env.TryInvalidCompile(
                    "select IntArr > all (select IntPrimitive from SupportBean#keepall) from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords [select IntArr > all (select IntPrimitive from SupportBean#keepall) from SupportBeanArrayCollMap]");

                // test OM
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, true });
                env.UndeployAll();
            }
        }

        private class EPLSubselectRelationalOpNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "vall,vany".SplitCsv();
                var stmtText = "@name('s0') select " +
                               "IntBoxed >= all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as vall, " +
                               "IntBoxed >= any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as vany " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // subs is empty
                // select  null >= all (select val from subs), null >= any (select val from subs)
                SendEvent(env, "E1", null, null);
                env.AssertPropsNew("s0", fields, new object[] { true, false });

                // select  1 >= all (select val from subs), 1 >= any (select val from subs)
                SendEvent(env, "E2", 1, null);
                env.AssertPropsNew("s0", fields, new object[] { true, false });

                // subs is {null}
                SendEvent(env, "S1", null, null);

                SendEvent(env, "E3", null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                SendEvent(env, "E4", 1, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });

                // subs is {null, 1}
                SendEvent(env, "S2", null, 1d);

                SendEvent(env, "E5", null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                SendEvent(env, "E6", 1, null);
                env.AssertPropsNew("s0", fields, new object[] { null, true });

                SendEvent(env, "E7", 0, null);
                env.AssertPropsNew("s0", fields, new object[] { false, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectRelationalOpSome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var stmtText = "@name('s0') select " +
                               "IntPrimitive > any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as g, " +
                               "IntPrimitive >= any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as ge, " +
                               "IntPrimitive < any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as l, " +
                               "IntPrimitive <= any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as le " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, false });

                env.SendEventBean(new SupportBean("S1", 1));

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, true, false, true });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false });

                env.SendEventBean(new SupportBean("E2a", 0));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true });

                env.SendEventBean(new SupportBean("S2", 2));

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false });

                env.SendEventBean(new SupportBean("E4", 2));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, true });

                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertPropsNew("s0", fields, new object[] { false, true, true, true });

                env.SendEventBean(new SupportBean("E6", 0));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectEqualsNotEqualsAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eq,neq,sqlneq,nneq".SplitCsv();
                var stmtText = "@name('s0') select " +
                               "IntPrimitive=all(select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as eq, " +
                               "IntPrimitive != all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as neq, " +
                               "IntPrimitive <> all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as sqlneq, " +
                               "not IntPrimitive = all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as nneq " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, false });

                env.SendEventBean(new SupportBean("S1", 11));

                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertPropsNew("s0", fields, new object[] { true, false, false, false });

                env.SendEventBean(new SupportBean("E3", 10));
                env.AssertPropsNew("s0", fields, new object[] { false, true, true, true });

                env.SendEventBean(new SupportBean("S1", 12));

                env.SendEventBean(new SupportBean("E4", 11));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, true });

                env.SendEventBean(new SupportBean("E5", 14));
                env.AssertPropsNew("s0", fields, new object[] { false, true, true, true });

                env.UndeployAll();
            }
        } // Test "value = SOME (subselect)" which is the same as "value IN (subselect)"

        private class EPLSubselectEqualsAnyOrSome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "r1,r2,r3,r4".SplitCsv();
                var stmtText = "@name('s0') select " +
                               "IntPrimitive = SOME (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r1, " +
                               "IntPrimitive = ANY (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r2, " +
                               "IntPrimitive != SOME (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r3, " +
                               "IntPrimitive <> ANY (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r4 " +
                               "from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, false });

                env.SendEventBean(new SupportBean("S1", 11));
                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertPropsNew("s0", fields, new object[] { true, true, false, false });

                env.SendEventBean(new SupportBean("E3", 12));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true });

                env.SendEventBean(new SupportBean("S2", 12));
                env.SendEventBean(new SupportBean("E4", 12));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, true });

                env.SendEventBean(new SupportBean("E5", 13));
                env.AssertPropsNew("s0", fields, new object[] { false, false, true, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectEqualsInNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eall,eany,neall,neany,isin".SplitCsv();
                var stmtText = "@name('s0') select " +
                               "IntBoxed = all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as eall, " +
                               "IntBoxed = any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as eany, " +
                               "IntBoxed != all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as neall, " +
                               "IntBoxed != any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as neany, " +
                               "IntBoxed in (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as isin " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // subs is empty
                // select  null = all (select val from subs), null = any (select val from subs), null != all (select val from subs), null != any (select val from subs), null in (select val from subs)
                SendEvent(env, "E1", null, null);
                env.AssertPropsNew("s0", fields, new object[] { true, false, true, false, false });

                // select  1 = all (select val from subs), 1 = any (select val from subs), 1 != all (select val from subs), 1 != any (select val from subs), 1 in (select val from subs)
                SendEvent(env, "E2", 1, null);
                env.AssertPropsNew("s0", fields, new object[] { true, false, true, false, false });

                // subs is {null}
                SendEvent(env, "S1", null, null);

                SendEvent(env, "E3", null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });
                SendEvent(env, "E4", 1, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });

                // subs is {null, 1}
                SendEvent(env, "S2", null, 1d);

                SendEvent(env, "E5", null, null);
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null });
                SendEvent(env, "E6", 1, null);
                env.AssertPropsNew("s0", fields, new object[] { null, true, false, null, true });
                SendEvent(env, "E7", 0, null);
                env.AssertPropsNew("s0", fields, new object[] { false, null, null, true, null });

                env.UndeployAll();
            }
        }

        private class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select IntArr = all (select IntPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords [select IntArr = all (select IntPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap]");
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int? intBoxed,
            double? doubleBoxed)
        {
            var bean = new SupportBean(theString, -1);
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            env.SendEventBean(bean);
        }
    }
} // end of namespace