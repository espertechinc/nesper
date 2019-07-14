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

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreEqualsIs
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreEqualsIsCoercion());
            executions.Add(new ExprCoreEqualsIsCoercionSameType());
            return executions;
        }

        private static void MakeSendBean(
            RegressionEnvironment env,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        internal class ExprCoreEqualsIsCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select IntPrimitive=longPrimitive as c0, IntPrimitive is longPrimitive as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = "c0,c1".SplitCsv();

                MakeSendBean(env, 1, 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});

                MakeSendBean(env, 1, 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.UndeployAll();
            }
        }

        internal class ExprCoreEqualsIsCoercionSameType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select p00 = p01 as c0, id = id as c1, p02 is not null as c2 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = "c0,c1,c2".SplitCsv();

                env.SendEventBean(new SupportBean_S0(1, "a", "a", "a"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true});

                env.SendEventBean(new SupportBean_S0(1, "a", "b", null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, true, false});

                env.UndeployAll();
            }
        }
    }
} // end of namespace