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

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectAllAnySomeExpr
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectRelationalOpAll());
            execs.Add(new EPLSubselectRelationalOpNullOrNoRows());
            execs.Add(new EPLSubselectRelationalOpSome());
            execs.Add(new EPLSubselectEqualsNotEqualsAll());
            execs.Add(new EPLSubselectEqualsAnyOrSome());
            execs.Add(new EPLSubselectEqualsInNullOrNoRows());
            execs.Add(new EPLSubselectInvalid());
            return execs;
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

        internal class EPLSubselectRelationalOpAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var stmtText = "@Name('s0') select " +
                               "IntPrimitive > all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as g, " +
                               "IntPrimitive >= all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as ge, " +
                               "IntPrimitive < all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as l, " +
                               "IntPrimitive <= all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as le " +
                               "from SupportBean(TheString like \"E%\")";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, true});

                env.SendEventBean(new SupportBean("S1", 1));

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, true});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("S2", 2));

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("E4", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, false});

                env.SendEventBean(new SupportBean("E5", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, true});

                env.SendEventBean(new SupportBean("E6", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true});

                env.UndeployAll();

                TryInvalidCompile(
                    env,
                    "select intArr > all (select IntPrimitive from SupportBean#keepall) from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr > all (select IntPrimitive from SupportBean#keepall) from SupportBeanArrayCollMap]");

                // test OM
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, true});
                env.UndeployAll();
            }
        }

        internal class EPLSubselectRelationalOpNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "vall,vany".SplitCsv();
                var stmtText = "@Name('s0') select " +
                               "IntBoxed >= all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as vall, " +
                               "IntBoxed >= any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as vany " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // subs is empty
                // select  null >= all (select val from subs), null >= any (select val from subs)
                SendEvent(env, "E1", null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                // select  1 >= all (select val from subs), 1 >= any (select val from subs)
                SendEvent(env, "E2", 1, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false});

                // subs is {null}
                SendEvent(env, "S1", null, null);

                SendEvent(env, "E3", null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                SendEvent(env, "E4", 1, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                // subs is {null, 1}
                SendEvent(env, "S2", null, 1d);

                SendEvent(env, "E5", null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                SendEvent(env, "E6", 1, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true});

                SendEvent(env, "E7", 0, null);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    fields,
                    new object[] {false, false});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectRelationalOpSome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "g,ge,l,le".SplitCsv();
                var stmtText = "@Name('s0') select " +
                               "IntPrimitive > any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as g, " +
                               "IntPrimitive >= any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as ge, " +
                               "IntPrimitive < any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as l, " +
                               "IntPrimitive <= any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as le " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, false});

                env.SendEventBean(new SupportBean("S1", 1));

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false, true});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("E2a", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true});

                env.SendEventBean(new SupportBean("S2", 2));

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("E4", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, true});

                env.SendEventBean(new SupportBean("E5", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, true, true});

                env.SendEventBean(new SupportBean("E6", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectEqualsNotEqualsAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eq,neq,sqlneq,nneq".SplitCsv();
                var stmtText = "@Name('s0') select " +
                               "IntPrimitive=all(select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as eq, " +
                               "IntPrimitive != all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as neq, " +
                               "IntPrimitive <> all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as sqlneq, " +
                               "not IntPrimitive = all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as nneq " +
                               " from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, false});

                env.SendEventBean(new SupportBean("S1", 11));

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false, false, false});

                env.SendEventBean(new SupportBean("E3", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, true, true});

                env.SendEventBean(new SupportBean("S1", 12));

                env.SendEventBean(new SupportBean("E4", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, true});

                env.SendEventBean(new SupportBean("E5", 14));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, true, true});

                env.UndeployAll();
            }
        } // Test "value = SOME (subselect)" which is the same as "value IN (subselect)"

        internal class EPLSubselectEqualsAnyOrSome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "r1,r2,r3,r4".SplitCsv();
                var stmtText = "@Name('s0') select " +
                               "IntPrimitive = SOME (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r1, " +
                               "IntPrimitive = ANY (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r2, " +
                               "IntPrimitive != SOME (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r3, " +
                               "IntPrimitive <> ANY (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r4 " +
                               "from SupportBean(TheString like 'E%')";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false, false});

                env.SendEventBean(new SupportBean("S1", 11));
                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, false, false});

                env.SendEventBean(new SupportBean("E3", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true});

                env.SendEventBean(new SupportBean("S2", 12));
                env.SendEventBean(new SupportBean("E4", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true, true});

                env.SendEventBean(new SupportBean("E5", 13));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, true, true});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectEqualsInNullOrNoRows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "eall,eany,neall,neany,isin".SplitCsv();
                var stmtText = "@Name('s0') select " +
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
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false, true, false, false});

                // select  1 = all (select val from subs), 1 = any (select val from subs), 1 != all (select val from subs), 1 != any (select val from subs), 1 in (select val from subs)
                SendEvent(env, "E2", 1, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, false, true, false, false});

                // subs is {null}
                SendEvent(env, "S1", null, null);

                SendEvent(env, "E3", null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});
                SendEvent(env, "E4", 1, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                // subs is {null, 1}
                SendEvent(env, "S2", null, 1d);

                SendEvent(env, "E5", null, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});
                SendEvent(env, "E6", 1, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, true, false, null, true});
                SendEvent(env, "E7", 0, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, null, null, true, null});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select intArr = all (select IntPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr = all (select IntPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap]");
            }
        }
    }
} // end of namespace