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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreLikeRegexp
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreLikeWConstants());
            executions.Add(new ExprCoreLikeWExprs());
            executions.Add(new ExprCoreRegexpWConstants());
            executions.Add(new ExprCoreRegexpWExprs());
            executions.Add(new ExprCoreLikeRegexStringAndNull());
            executions.Add(new ExprCoreLikeRegExInvalid());
            executions.Add(new ExprCoreLikeRegexEscapedChar());
            executions.Add(new ExprCoreLikeRegexStringAndNullOM());
            executions.Add(new ExprCoreRegexStringAndNullCompile());
            executions.Add(new ExprCoreLikeRegexNumericAndNull());
            return executions;
        }

        private static void RunLikeRegexStringAndNull(RegressionEnvironment env)
        {
            SendS0Event(env, -1, "a", "b", "c", "d");
            AssertReceived(
                env,
                new[] {new object[] {"r1", false}, new object[] {"r2", false}, new object[] {"r3", false}});

            SendS0Event(env, -1, null, "b", null, "d");
            AssertReceived(
                env,
                new[] {new object[] {"r1", null}, new object[] {"r2", null}, new object[] {"r3", null}});

            SendS0Event(env, -1, "a", null, "c", null);
            AssertReceived(
                env,
                new[] {new object[] {"r1", null}, new object[] {"r2", null}, new object[] {"r3", null}});

            SendS0Event(env, -1, null, null, null, null);
            AssertReceived(
                env,
                new[] {new object[] {"r1", null}, new object[] {"r2", null}, new object[] {"r3", null}});

            SendS0Event(env, -1, "abcdef", "%de_", "a", "[a-c]");
            AssertReceived(
                env,
                new[] {new object[] {"r1", true}, new object[] {"r2", true}, new object[] {"r3", true}});

            SendS0Event(env, -1, "abcdef", "b%de_", "d", "[a-c]");
            AssertReceived(
                env,
                new[] {new object[] {"r1", false}, new object[] {"r2", false}, new object[] {"r3", false}});

            SendS0Event(env, -1, "!adex", "!%de_", "", ".");
            AssertReceived(
                env,
                new[] {new object[] {"r1", true}, new object[] {"r2", false}, new object[] {"r3", false}});

            SendS0Event(env, -1, "%dex", "!%de_", "a", ".");
            AssertReceived(
                env,
                new[] {new object[] {"r1", false}, new object[] {"r2", true}, new object[] {"r3", true}});
        }

        private static void SendS0Event(
            RegressionEnvironment env,
            int id,
            string p00,
            string p01,
            string p02,
            string p03)
        {
            var bean = new SupportBean_S0(id, p00, p01, p02, p03);
            env.SendEventBean(bean);
        }

        private static void AssertReceived(
            RegressionEnvironment env,
            object[][] objects)
        {
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            foreach (var @object in objects) {
                var key = (string) @object[0];
                var result = @object[1];
                Assert.AreEqual(result, theEvent.Get(key), "key=" + key + " result=" + result);
            }
        }

        private static void TryInvalidExpr(
            RegressionEnvironment env,
            string expr)
        {
            var statement = "select " + expr + " from " + typeof(SupportBean).Name;
            TryInvalidCompile(env, statement, "skip");
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int? intBoxed,
            double? doubleBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            env.SendEventBean(bean);
        }

        internal class ExprCoreLikeWConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select TheString like 'A%' as c0, IntPrimitive like '1%' as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("Bxx", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {false, false});

                env.SendEventBean(new SupportBean("Ayyy", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreLikeWExprs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select p00 like p01 as c0, id like p02 as c1 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                SendS0Event(env, 413, "%XXaXX", "%a%", "%1%", null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {true, true});

                SendS0Event(env, 413, "%XXcXX", "%b%", "%2%", null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {false, false});

                SendS0Event(env, 413, "%XXcXX", "%c%", "%3%", null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreRegexpWConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select TheString regexp '.*Jack.*' as c0, IntPrimitive regexp '.*1.*' as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("Joe", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {false, false});

                env.SendEventBean(new SupportBean("TheJackWhite", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreRegexpWExprs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select p00 regexp p01 as c0, id regexp p02 as c1 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                SendS0Event(env, 413, "XXAXX", ".*A.*", ".*1.*", null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {true, true});

                SendS0Event(env, 413, "XXaXX", ".*B.*", ".*2.*", null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {false, false});

                SendS0Event(env, 413, "XXCXX", ".*C.*", ".*3.*", null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1".SplitCsv(),
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreLikeRegexStringAndNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select p00 like p01 as r1, " +
                          " p00 like p01 escape \"!\" as r2," +
                          " p02 regexp p03 as r3 " +
                          " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                RunLikeRegexStringAndNull(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreLikeRegExInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidExpr(env, "IntPrimitive like 'a' escape null");
                TryInvalidExpr(env, "IntPrimitive like boolPrimitive");
                TryInvalidExpr(env, "boolPrimitive like string");
                TryInvalidExpr(env, "string like string escape IntPrimitive");

                TryInvalidExpr(env, "IntPrimitive regexp doublePrimitve");
                TryInvalidExpr(env, "IntPrimitive regexp boolPrimitive");
                TryInvalidExpr(env, "boolPrimitive regexp string");
                TryInvalidExpr(env, "string regexp IntPrimitive");

                TryInvalidCompile(
                    env,
                    "select TheString regexp \"*any*\" from SupportBean",
                    "Failed to validate select-clause expression 'theString regexp \"*any*\"': Error compiling regex pattern '*any*': Dangling meta character '*' near index 0");
            }
        }

        internal class ExprCoreLikeRegexEscapedChar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select p00 regexp '\\\\w*-ABC' as result from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(-1, "TBT-ABC"));
                Assert.IsTrue((bool) env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.SendEventBean(new SupportBean_S0(-1, "TBT-BC"));
                Assert.IsFalse((bool) env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreLikeRegexStringAndNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select p00 like p01 as r1, " +
                               "p00 like p01 escape \"!\" as r2, " +
                               "p02 regexp p03 as r3 " +
                               "from SupportBean_S0";

                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.Like(Expressions.Property("p00"), Expressions.Property("p01")), "r1")
                    .Add(
                        Expressions.Like(
                            Expressions.Property("p00"),
                            Expressions.Property("p01"),
                            Expressions.Constant("!")),
                        "r2")
                    .Add(Expressions.Regexp(Expressions.Property("p02"), Expressions.Property("p03")), "r3");

                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model = env.CopyMayFail(model);
                Assert.AreEqual(stmtText, model.ToEPL());

                var compiled = env.Compile(model, new CompilerArguments(env.Configuration));
                env.Deploy(compiled).AddListener("s0").Milestone(0);

                RunLikeRegexStringAndNull(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreRegexStringAndNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select p00 like p01 as r1, " +
                          "p00 like p01 escape \"!\" as r2, " +
                          "p02 regexp p03 as r3 " +
                          "from SupportBean_S0";

                var model = env.EplToModel(epl);
                model = env.CopyMayFail(model);
                Assert.AreEqual(epl, model.ToEPL());

                var compiled = env.Compile(model, new CompilerArguments(env.Configuration));
                env.Deploy(compiled).AddListener("s0").Milestone(0);

                RunLikeRegexStringAndNull(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreLikeRegexNumericAndNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select intBoxed like '%01%' as r1, " +
                          " doubleBoxed regexp '[0-9][0-9].[0-9]' as r2 " +
                          " from " +
                          typeof(SupportBean).Name;

                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBeanEvent(env, 101, 1.1);
                AssertReceived(
                    env,
                    new[] {new object[] {"r1", true}, new object[] {"r2", false}});

                SendSupportBeanEvent(env, 102, 11d);
                AssertReceived(
                    env,
                    new[] {new object[] {"r1", false}, new object[] {"r2", true}});

                SendSupportBeanEvent(env, null, null);
                AssertReceived(
                    env,
                    new[] {new object[] {"r1", null}, new object[] {"r2", null}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace