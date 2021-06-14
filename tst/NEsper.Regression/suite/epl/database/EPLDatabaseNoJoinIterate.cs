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

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseNoJoinIterate
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLDatabaseExpressionPoll());
            execs.Add(new EPLDatabaseVariablesPoll());
            return execs;
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            bool boolPrimitive,
            int intPrimitive,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.BoolPrimitive = boolPrimitive;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }

        internal class EPLDatabaseExpressionPoll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable boolean queryvar_bool", path);
                env.CompileDeploy("create variable int queryvar_int", path);
                env.CompileDeploy("create variable int lower", path);
                env.CompileDeploy("create variable int upper", path);
                env.CompileDeploy(
                    "on SupportBean set queryvar_int=IntPrimitive, queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed",
                    path);

                // Test int and singlerow
                var stmtText =
                    "@Name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where ${queryvar_int -2} = mytesttable.myBigint']";
                env.CompileDeploy(stmtText, path).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), new[] {"myint"}, null);

                SendSupportBeanEvent(env, 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new[] {"myint"},
                    new[] {new object[] {30}});

                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployModuleContaining("s0");

                // Test multi-parameter and multi-row
                stmtText =
                    "@Name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where mytesttable.myBigint between ${queryvar_int-2} and ${queryvar_int+2}'] order by myint";
                env.CompileDeploy(stmtText, path);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new[] {"myint"},
                    new[] {
                        new object[] {30}, new object[] {40}, new object[] {50}, new object[] {60}, new object[] {70}
                    });
                env.UndeployAll();

                // Test substitution parameters
                TryInvalidCompile(
                    env,
                    "@Name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where mytesttable.myBigint between ${?} and ${queryvar_int+?}'] order by myint",
                    "EPL substitution parameters are not allowed in SQL ${...} expressions, consider using a variable instead");
            }
        }

        internal class EPLDatabaseVariablesPoll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable boolean queryvar_bool", path);
                env.CompileDeploy("create variable int queryvar_int", path);
                env.CompileDeploy("create variable int lower", path);
                env.CompileDeploy("create variable int upper", path);
                env.CompileDeploy(
                    "on SupportBean set queryvar_int=IntPrimitive, queryvar_bool=BoolPrimitive, lower=IntPrimitive,upper=IntBoxed",
                    path);

                // Test int and singlerow
                var stmtText =
                    "@Name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where ${queryvar_int} = mytesttable.myBigint']";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), new[] {"myint"}, null);

                SendSupportBeanEvent(env, 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new[] {"myint"},
                    new[] {new object[] {50}});

                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployModuleContaining("s0");

                // Test boolean and multirow
                stmtText =
                    "@Name('s0') select * from sql:MyDBWithTxnIso1WithReadOnly ['select myBigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by myBigint']";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                string[] fields = {"myBigint", "mybool"};
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendSupportBeanEvent(env, true, 10, 40);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1L, true}, new object[] {4L, true}});

                SendSupportBeanEvent(env, false, 30, 80);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {3L, false}, new object[] {5L, false}, new object[] {6L, false}});

                SendSupportBeanEvent(env, true, 20, 30);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendSupportBeanEvent(env, true, 20, 60);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {4L, true}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace