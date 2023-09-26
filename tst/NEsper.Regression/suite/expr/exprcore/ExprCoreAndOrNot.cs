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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreAndOrNot
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithAndOrNotCombined(execs);
            WithNotWithVariable(execs);
            WithAndOrNotNull(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAndOrNotNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAndOrNotNull());
            return execs;
        }

        public static IList<RegressionExecution> WithNotWithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreNotWithVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithAndOrNotCombined(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreAndOrNotCombined());
            return execs;
        }

        private class ExprCoreAndOrNotNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "cast(null, boolean) and cast(null, boolean)")
                    .WithExpression(fields[1], "boolPrimitive and boolBoxed")
                    .WithExpression(fields[2], "boolBoxed and boolPrimitive")
                    .WithExpression(fields[3], "boolPrimitive or boolBoxed")
                    .WithExpression(fields[4], "boolBoxed or boolPrimitive")
                    .WithExpression(fields[5], "not boolBoxed");
                builder.WithStatementConsumer(
                    stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(bool?)));
                builder.WithAssertion(MakeSB("E1", true, true)).Expect(fields, null, true, true, true, true, false);
                builder.WithAssertion(MakeSB("E2", false, false))
                    .Expect(fields, null, false, false, false, false, true);
                builder.WithAssertion(MakeSB("E3", false, null)).Expect(fields, null, false, false, null, null, null);
                builder.WithAssertion(MakeSB("E4", true, null)).Expect(fields, null, null, null, true, true, null);
                builder.WithAssertion(MakeSB("E5", true, false)).Expect(fields, null, false, false, true, true, true);
                builder.WithAssertion(MakeSB("E6", false, true)).Expect(fields, null, false, false, true, true, false);
                builder.Run(env);
                env.UndeployAll();
            }

            private SupportBean MakeSB(
                string theString,
                bool boolPrimitive,
                bool? boolBoxed)
            {
                var sb = new SupportBean(theString, 0);
                sb.BoolPrimitive = (boolPrimitive);
                sb.BoolBoxed = (boolBoxed);
                return sb;
            }
        }

        private class ExprCoreNotWithVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create variable string thing = \"Hello World\";" +
                    "@name('s0') select not thing.Contains(TheString) as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanAssert(env, "World", false);
                SendBeanAssert(env, "x", true);

                env.RuntimeSetVariable("s0", "thing", "5 x 5");

                SendBeanAssert(env, "World", true);
                SendBeanAssert(env, "x", false);

                env.UndeployAll();
            }
        }

        private class ExprCoreAndOrNotCombined : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(
                        fields,
                        "(IntPrimitive=1) or (IntPrimitive=2)",
                        "(IntPrimitive>0) and (IntPrimitive<3)",
                        "not(IntPrimitive=2)");
                builder.WithAssertion(new SupportBean("E1", 1)).Expect(fields, true, true, true);
                builder.WithAssertion(new SupportBean("E2", 2)).Expect(fields, true, true, false);
                builder.WithAssertion(new SupportBean("E3", 3)).Expect(fields, false, false, true);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private static void SendBeanAssert(
            RegressionEnvironment env,
            string theString,
            bool expected)
        {
            var bean = new SupportBean(theString, 0);
            env.SendEventBean(bean);
            env.AssertEqualsNew("s0", "c0", expected);
        }
    }
} // end of namespace