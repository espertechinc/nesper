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

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextWDeclaredExpression
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
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

        private static void TryAssertionExpression(RegressionEnvironment env)
        {
            var fields = new[] { "c0", "c1", "c2" };
            env.SendEventBean(new SupportBean("E1", -2));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] { "n", "xnx", "n" });

            env.SendEventBean(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] { "p", "xpx", "p" });
        }

        internal class ContextWDeclaredExpressionSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean",
                    path);
                env.CompileDeploy("create expression getLabelOne { context.label }", path);
                env.CompileDeploy("create expression getLabelTwo { 'x'||context.label||'x' }", path);

                env.CompileDeploy(
                        "@Name('s0') expression getLabelThree { context.label } " +
                        "context MyCtx " +
                        "select getLabelOne() as c0, getLabelTwo() as c1, getLabelThree() as c2 from SupportBean",
                        path)
                    .AddListener("s0");

                TryAssertionExpression(env);

                env.UndeployAll();
            }
        }

        internal class ContextWDeclaredExpressionAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean",
                    path);
                env.CompileDeploy("create expression getLabelOne alias for { context.label }", path);
                env.CompileDeploy("create expression getLabelTwo alias for { 'x'||context.label||'x' }", path);

                env.CompileDeploy(
                        "@Name('s0') expression getLabelThree alias for { context.label } " +
                        "context MyCtx " +
                        "select getLabelOne as c0, getLabelTwo as c1, getLabelThree as c2 from SupportBean",
                        path)
                    .AddListener("s0");

                TryAssertionExpression(env);

                env.UndeployAll();
            }
        }

        internal class ContextWDeclaredExpressionWFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var expr = "create expression THE_EXPRESSION alias for {TheString='x'}";
                env.CompileDeploy(expr, path);

                var context =
                    "create context context2 initiated @now and pattern[every(SupportBean(THE_EXPRESSION))] terminated after 10 minutes";
                env.CompileDeploy(context, path);

                var statement =
                    "@Name('s0') context context2 select * from pattern[e1=SupportBean(THE_EXPRESSION) -> e2=SupportBean(TheString='y')]";
                env.CompileDeploy(statement, path).AddListener("s0");

                env.SendEventBean(new SupportBean("x", 1));
                env.SendEventBean(new SupportBean("y", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] { "e1.IntPrimitive", "e2.IntPrimitive" },
                    new object[] { 1, 2 });

                env.UndeployAll();
            }
        }
    }
} // end of namespace