///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextWDeclaredExpression
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithAlias(execs);
            WithWFilter(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextWDeclaredExpressionWFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextWDeclaredExpressionAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextWDeclaredExpressionSimple());
            return execs;
        }

        private class ContextWDeclaredExpressionSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean",
                    path);
                env.CompileDeploy("@public create expression getLabelOne { context.label }", path);
                env.CompileDeploy("@public create expression getLabelTwo { 'x'||context.label||'x' }", path);

                env.CompileDeploy(
                        "@public @name('s0') expression getLabelThree { context.label } " +
                        "context MyCtx " +
                        "select getLabelOne() as c0, getLabelTwo() as c1, getLabelThree() as c2 from SupportBean",
                        path)
                    .AddListener("s0");

                TryAssertionExpression(env);

                env.UndeployAll();
            }
        }

        private class ContextWDeclaredExpressionAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean",
                    path);
                env.CompileDeploy("@public create expression getLabelOne alias for { context.label }", path);
                env.CompileDeploy("@public create expression getLabelTwo alias for { 'x'||context.label||'x' }", path);

                env.CompileDeploy(
                        "@name('s0') expression getLabelThree alias for { context.label } " +
                        "context MyCtx " +
                        "select getLabelOne as c0, getLabelTwo as c1, getLabelThree as c2 from SupportBean",
                        path)
                    .AddListener("s0");

                TryAssertionExpression(env);

                env.UndeployAll();
            }
        }

        private class ContextWDeclaredExpressionWFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var expr = "@public create expression THE_EXPRESSION alias for {TheString='x'}";
                env.CompileDeploy(expr, path);

                var context =
                    "@public create context context2 initiated @now and pattern[every(SupportBean(THE_EXPRESSION))] terminated after 10 minutes";
                env.CompileDeploy(context, path);

                var statement =
                    "@name('s0') context context2 select * from pattern[e1=SupportBean(THE_EXPRESSION) -> e2=SupportBean(TheString='y')]";
                env.CompileDeploy(statement, path).AddListener("s0");

                env.SendEventBean(new SupportBean("x", 1));
                env.SendEventBean(new SupportBean("y", 2));
                env.AssertPropsNew("s0", "e1.IntPrimitive,e2.IntPrimitive".SplitCsv(), new object[] { 1, 2 });

                env.UndeployAll();
            }
        }

        private static void TryAssertionExpression(RegressionEnvironment env)
        {
            var fields = "c0,c1,c2".SplitCsv();
            env.SendEventBean(new SupportBean("E1", -2));
            env.AssertPropsNew("s0", fields, new object[] { "n", "xnx", "n" });

            env.SendEventBean(new SupportBean("E2", 1));
            env.AssertPropsNew("s0", fields, new object[] { "p", "xpx", "p" });
        }
    }
} // end of namespace