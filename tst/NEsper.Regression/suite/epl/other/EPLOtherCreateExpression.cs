///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherCreateExpression
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithInvalid(execs);
            WithParseSpecialAndMixedExprAndScript(execs);
            WithExprAndScriptLifecycleAndFilter(execs);
            WithScriptUse(execs);
            With(ExpressionUse)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionUse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherExpressionUse());
            return execs;
        }

        public static IList<RegressionExecution> WithScriptUse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherScriptUse());
            return execs;
        }

        public static IList<RegressionExecution> WithExprAndScriptLifecycleAndFilter(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherExprAndScriptLifecycleAndFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithParseSpecialAndMixedExprAndScript(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherParseSpecialAndMixedExprAndScript());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalid());
            return execs;
        }

        private class EPLOtherInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('s0') @public create expression E1 {''}", path);
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(
                            StatementType.CREATE_EXPRESSION,
                            statement.GetProperty(StatementProperty.STATEMENTTYPE));
                        Assert.AreEqual("E1", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
                    });

                env.TryInvalidCompile(
                    path,
                    "create expression E1 {''}",
                    "Expression 'E1' has already been declared [create expression E1 {''}]");

                env.CompileDeploy("@public create expression int js:abc(p1, p2) [p1*p2]", path);
                env.TryInvalidCompile(
                    path,
                    "create expression int js:abc(a, a) [p1*p2]",
                    "Script 'abc' that takes the same number of parameters has already been declared [create expression int js:abc(a, a) [p1*p2]]");

                env.UndeployAll();
            }
        }

        private class EPLOtherParseSpecialAndMixedExprAndScript : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create expression string js:myscript(p1) [return \"--\"+p1+\"--\"]", path);
                env.CompileDeploy("@public create expression myexpr {sb => '--'||TheString||'--'}", path);

                // test mapped property syntax
                var eplMapped = "@name('s0') select myscript('x') as c0, myexpr(sb) as c1 from SupportBean as sb";
                env.CompileDeploy(eplMapped, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "--x--", "--E1--" });
                env.UndeployModuleContaining("s0");

                // test expression chained syntax
                var eplExpr = "" +
                              "@public create expression scalarfilter {s => " +
                              "   Strvals.where(y => y != 'E1') " +
                              "}";
                env.CompileDeploy(eplExpr, path);
                var eplSelect =
                    "@name('s0') select scalarfilter(t).where(x => x != 'E2') as val1 from SupportCollection as t";
                env.CompileDeploy(eplSelect, path).AddListener("s0");
                AssertStatelessStmt(env, "s0", true);
                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env, "val1", "E3", "E4");
                env.UndeployAll();

                // test script chained synax
                var eplScript = "@public create expression " +
                                typeof(SupportBean).FullName +
                                " js:callIt() [ new " +
                                typeof(SupportBean).FullName +
                                "('E1', 10); ]";
                env.CompileDeploy(eplScript, path);
                env.CompileDeploy(
                        "@name('s0') select callIt() as val0, callIt().TheString as val1 from SupportBean as sb",
                        path)
                    .AddListener("s0");
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew(
                    "s0",
                    "val0.TheString,val0.IntPrimitive,val1".SplitCsv(),
                    new object[] { "E1", 10, "E1" });

                env.UndeployAll();
            }
        }

        private class EPLOtherScriptUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create expression int js:abc(p1, p2) [return p1*p2*10]", path);
                env.CompileDeploy("@public create expression int js:abc(p1) [return p1*10]", path);

                var epl =
                    "@name('s0') select abc(IntPrimitive, DoublePrimitive) as c0, abc(IntPrimitive) as c1 from SupportBean";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(MakeBean("E1", 10, 3.5));
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { 350, 100 });

                env.UndeployAll();

                // test SODA
                var eplExpr = "@name('expr') @public create expression somescript(i1) ['a']";
                var modelExpr = env.EplToModel(eplExpr);
                Assert.AreEqual(eplExpr, modelExpr.ToEPL());
                env.CompileDeploy(modelExpr, path);
                env.AssertStatement(
                    "expr",
                    statement => Assert.AreEqual(eplExpr, statement.GetProperty(StatementProperty.EPL)));

                var eplSelect = "@name('select') select somescript(1) from SupportBean";
                var modelSelect = env.EplToModel(eplSelect);
                Assert.AreEqual(eplSelect, modelSelect.ToEPL());
                env.CompileDeploy(modelSelect, path);
                env.AssertStatement(
                    "select",
                    statement => Assert.AreEqual(eplSelect, statement.GetProperty(StatementProperty.EPL)));

                env.UndeployAll();
            }
        }

        private class EPLOtherExpressionUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create expression TwoPi {Math.PI * 2}", path);
                env.CompileDeploy("@public create expression factorPi {sb => Math.PI * IntPrimitive}", path);

                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "TwoPi() as c0," +
                          "(select TwoPi() from SupportBean_S0#lastevent) as c1," +
                          "factorPi(sb) as c2 " +
                          "from SupportBean sb";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean("E1", 3)); // factor is 3
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { Math.PI * 2, Math.PI * 2, Math.PI * 3 });

                env.UndeployModuleContaining("s0");

                // test local expression override
                env.CompileDeploy(
                        "@name('s0') expression TwoPi {Math.PI * 10} select TwoPi() as c0 from SupportBean",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { Math.PI * 10 });

                // test SODA
                var eplExpr =
                    "@name('expr') @public create expression JoinMultiplication {(s1,s2) => s1.IntPrimitive*s2.Id}";
                var modelExpr = env.EplToModel(eplExpr);
                Assert.AreEqual(eplExpr, modelExpr.ToEPL());
                env.CompileDeploy(modelExpr, path);
                env.AssertStatement(
                    "expr",
                    statement => Assert.AreEqual(eplExpr, statement.GetProperty(StatementProperty.EPL)));

                // test SODA and join and 2-stream parameter
                var eplJoin =
                    "@name('join') select JoinMultiplication(sb,s0) from SupportBean#lastevent as sb, SupportBean_S0#lastevent as s0";
                var modelJoin = env.EplToModel(eplJoin);
                Assert.AreEqual(eplJoin, modelJoin.ToEPL());
                env.CompileDeploy(modelJoin, path);
                env.AssertStatement(
                    "join",
                    statement => Assert.AreEqual(eplJoin, statement.GetProperty(StatementProperty.EPL)));
                env.UndeployAll();

                // test subquery against named window and table defined in declared expression
                TryAssertionTestExpressionUse(env, true);
                TryAssertionTestExpressionUse(env, false);

                env.UndeployAll();
            }

            private static void TryAssertionTestExpressionUse(
                RegressionEnvironment env,
                bool namedWindow)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create expression myexpr {(select IntPrimitive from MyInfra)}", path);
                var eplCreate = namedWindow
                    ? "@public create window MyInfra#keepall as SupportBean"
                    : "@public create table MyInfra(TheString string, IntPrimitive int)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("insert into MyInfra select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("@name('s0') select myexpr() as c0 from SupportBean_S0", path).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("E1", 100));
                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { 100 });

                env.UndeployAll();
            }
        }

        private class EPLOtherExprAndScriptLifecycleAndFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // expression assertion
                TryAssertionLifecycleAndFilter(
                    env,
                    "@public create expression MyFilter {sb => IntPrimitive = 1}",
                    "select * from SupportBean(MyFilter(sb)) as sb",
                    "@public create expression MyFilter {sb => IntPrimitive = 2}");

                // script assertion
                TryAssertionLifecycleAndFilter(
                    env,
                    "@public create expression boolean js:MyFilter(IntPrimitive) [return IntPrimitive==1]",
                    "select * from SupportBean(MyFilter(IntPrimitive)) as sb",
                    "@public create expression boolean js:MyFilter(IntPrimitive) [return IntPrimitive==2]");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private static void TryAssertionLifecycleAndFilter(
            RegressionEnvironment env,
            string expressionBefore,
            string selector,
            string expressionAfter)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@name('expr-one') " + expressionBefore, path);
            env.CompileDeploy("@name('s1') " + selector, path).AddListener("s1");

            env.SendEventBean(new SupportBean("E1", 0));
            env.AssertListenerNotInvoked("s1");
            env.SendEventBean(new SupportBean("E2", 1));
            env.AssertListenerInvoked("s1");

            var listenerS1 = env.Listener("s1");
            path.Clear();
            env.UndeployAll();

            env.CompileDeploy("@name('expr-two') " + expressionAfter, path);
            env.CompileDeploy("@name('s2') " + selector, path).AddListener("s2");

            env.SendEventBean(new SupportBean("E3", 0));
            Assert.IsFalse(listenerS1.GetAndClearIsInvoked() || env.Listener("s2").GetAndClearIsInvoked());

            env.Milestone(0);

            env.SendEventBean(new SupportBean("E4", 1));
            Assert.IsFalse(listenerS1.GetAndClearIsInvoked());
            Assert.IsFalse(env.Listener("s2").GetAndClearIsInvoked());
            env.SendEventBean(new SupportBean("E4", 2));
            Assert.IsFalse(listenerS1.GetAndClearIsInvoked());
            Assert.IsTrue(env.Listener("s2").GetAndClearIsInvoked());

            env.UndeployAll();
        }

        private static SupportBean MakeBean(
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean();
            sb.IntPrimitive = intPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    }
} // end of namespace