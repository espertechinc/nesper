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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreLikeRegexp
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithLikeWConstants(execs);
            WithLikeWExprs(execs);
            WithRegexpWConstants(execs);
            WithRegexpWExprs(execs);
            WithLikeRegexStringAndNull(execs);
            WithLikeRegexInvalid(execs);
            WithLikeRegexEscapedChar(execs);
            WithLikeRegexStringAndNullOM(execs);
            WithRegexStringAndNullCompile(execs);
            WithLikeRegexNumericAndNull(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLikeRegexNumericAndNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeRegexNumericAndNull());
            return execs;
        }

        public static IList<RegressionExecution> WithRegexStringAndNullCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRegexStringAndNullCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithLikeRegexStringAndNullOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeRegexStringAndNullOM());
            return execs;
        }

        public static IList<RegressionExecution> WithLikeRegexEscapedChar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeRegexEscapedChar());
            return execs;
        }

        public static IList<RegressionExecution> WithLikeRegexInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeRegexInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithLikeRegexStringAndNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeRegexStringAndNull());
            return execs;
        }

        public static IList<RegressionExecution> WithRegexpWExprs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRegexpWExprs());
            return execs;
        }

        public static IList<RegressionExecution> WithRegexpWConstants(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreRegexpWConstants());
            return execs;
        }

        public static IList<RegressionExecution> WithLikeWExprs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeWExprs());
            return execs;
        }

        public static IList<RegressionExecution> WithLikeWConstants(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreLikeWConstants());
            return execs;
        }

        private class ExprCoreLikeWConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(fields, "TheString like 'A%'", "IntPrimitive like '1%'");
                builder.WithAssertion(new SupportBean("Bxx", 0)).Expect(fields, false, false);
                builder.WithAssertion(new SupportBean("Ayyy", 100)).Expect(fields, true, true);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreLikeWExprs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_S0")
                    .WithExpressions(fields, "P00 like P01", "Id like P02");

                builder.WithAssertion(new SupportBean_S0(413, "%XXaXX", "%a%", "%1%", null)).Expect(fields, true, true);
                builder.WithAssertion(new SupportBean_S0(413, "%XXcXX", "%b%", "%2%", null))
                    .Expect(fields, false, false);
                builder.WithAssertion(new SupportBean_S0(413, "%XXcXX", "%c%", "%3%", null)).Expect(fields, true, true);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreRegexpWConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(fields, "TheString regexp '.*Jack.*'", "IntPrimitive regexp '.*1.*'");

                builder.WithAssertion(new SupportBean("Joe", 0)).Expect(fields, false, false);
                builder.WithAssertion(new SupportBean("TheJackWhite", 100)).Expect(fields, true, true);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreRegexpWExprs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_S0")
                    .WithExpressions(fields, "P00 regexp P01", "Id regexp P02");

                builder.WithAssertion(new SupportBean_S0(413, "XXAXX", ".*A.*", ".*1.*", null))
                    .Expect(fields, true, true);
                builder.WithAssertion(new SupportBean_S0(413, "XXaXX", ".*B.*", ".*2.*", null))
                    .Expect(fields, false, false);
                builder.WithAssertion(new SupportBean_S0(413, "XXCXX", ".*C.*", ".*3.*", null))
                    .Expect(fields, true, true);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreLikeRegexStringAndNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select P00 like P01 as r1, " +
                          " P00 like P01 escape \"!\" as r2," +
                          " P02 regexp P03 as r3 " +
                          " from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                RunLikeRegexStringAndNull(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreLikeRegexInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidExpr(env, "IntPrimitive like 'a' escape null");
                TryInvalidExpr(env, "IntPrimitive like BoolPrimitive");
                TryInvalidExpr(env, "BoolPrimitive like string");
                TryInvalidExpr(env, "string like string escape IntPrimitive");

                TryInvalidExpr(env, "IntPrimitive regexp doublePrimitve");
                TryInvalidExpr(env, "IntPrimitive regexp BoolPrimitive");
                TryInvalidExpr(env, "BoolPrimitive regexp string");
                TryInvalidExpr(env, "string regexp IntPrimitive");

                env.TryInvalidCompile(
                    "select TheString regexp \"*any*\" from SupportBean",
                    "Failed to validate select-clause expression 'TheString regexp \"*any*\"': " +
                    "Failed to compile regex pattern '*any*': " +
#if NET7_0_OR_GREATER
                    "Invalid pattern '*any*' at offset 1. Quantifier '*' following nothing."
#elif NETCOREAPP3_0_OR_GREATER
                    "Invalid pattern '*any*' at offset 1. Quantifier {x,y} following nothing."
#else
					"parsing \"*any*\" - Quantifier {x,y} following nothing."
#endif
                );
            }
        }

        private class ExprCoreLikeRegexEscapedChar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_S0")
                    .WithExpressions(fields, "P00 regexp '\\\\w*-ABC'");

                builder.WithAssertion(new SupportBean_S0(-1, "TBT-ABC")).Expect(fields, true);
                builder.WithAssertion(new SupportBean_S0(-1, "TBT-BC")).Expect(fields, false);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreLikeRegexStringAndNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select P00 like P01 as r1, " +
                               "P00 like P01 escape \"!\" as r2, " +
                               "P02 regexp P03 as r3 " +
                               "from SupportBean_S0";

                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.Like(Expressions.Property("P00"), Expressions.Property("P01")), "r1")
                    .Add(
                        Expressions.Like(
                            Expressions.Property("P00"),
                            Expressions.Property("P01"),
                            Expressions.Constant("!")),
                        "r2")
                    .Add(Expressions.Regexp(Expressions.Property("P02"), Expressions.Property("P03")), "r3");

                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
                Assert.AreEqual(stmtText, model.ToEPL());

                var compiled = env.Compile(model, new CompilerArguments(env.Configuration));
                env.Deploy(compiled).AddListener("s0").Milestone(0);

                RunLikeRegexStringAndNull(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreRegexStringAndNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select P00 like P01 as r1, " +
                          "P00 like P01 escape \"!\" as r2, " +
                          "P02 regexp P03 as r3 " +
                          "from SupportBean_S0";

                var model = env.EplToModel(epl);
                model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
                Assert.AreEqual(epl, model.ToEPL());

                var compiled = env.Compile(model, new CompilerArguments(env.Configuration));
                env.Deploy(compiled).AddListener("s0").Milestone(0);

                RunLikeRegexStringAndNull(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreLikeRegexNumericAndNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(fields, "IntBoxed like '%01%'", "DoubleBoxed regexp '[0-9][0-9].[0-9]d'");

                builder.WithAssertion(MakeSupportBeanEvent(101, 1.1)).Expect(fields, true, false);
                builder.WithAssertion(MakeSupportBeanEvent(102, 11d)).Expect(fields, false, true);
                builder.WithAssertion(MakeSupportBeanEvent(null, null)).Expect(fields, null, null);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private static void RunLikeRegexStringAndNull(RegressionEnvironment env)
        {
            SendS0Event(env, -1, "a", "b", "c", "d");
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", false },
                    new object[] { "r2", false },
                    new object[] { "r3", false }
                });

            SendS0Event(env, -1, null, "b", null, "d");
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", null },
                    new object[] { "r2", null },
                    new object[] { "r3", null }
                });

            SendS0Event(env, -1, "a", null, "c", null);
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", null },
                    new object[] { "r2", null },
                    new object[] { "r3", null }
                });

            SendS0Event(env, -1, null, null, null, null);
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", null },
                    new object[] { "r2", null },
                    new object[] { "r3", null }
                });

            SendS0Event(env, -1, "abcdef", "%de_", "a", "[a-c]");
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", true },
                    new object[] { "r2", true },
                    new object[] { "r3", true }
                });

            SendS0Event(env, -1, "abcdef", "b%de_", "d", "[a-c]");
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", false },
                    new object[] { "r2", false },
                    new object[] { "r3", false }
                });

            SendS0Event(env, -1, "!adex", "!%de_", "", ".");
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", true },
                    new object[] { "r2", false },
                    new object[] { "r3", false }
                });

            SendS0Event(env, -1, "%dex", "!%de_", "a", ".");
            AssertReceived(
                env,
                new[] {
                    new object[] { "r1", false },
                    new object[] { "r2", true },
                    new object[] { "r3", true }
                });
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
            env.AssertEventNew(
                "s0",
                theEvent => {
                    foreach (var @object in objects) {
                        var key = (string)@object[0];
                        var result = @object[1];
                        Assert.AreEqual(result, theEvent.Get(key), "key=" + key + " result=" + result);
                    }
                });
        }

        private static void TryInvalidExpr(
            RegressionEnvironment env,
            string expr)
        {
            var statement = "select " + expr + " from SupportBean";
            env.TryInvalidCompile(statement, "skip");
        }

        private static SupportBean MakeSupportBeanEvent(
            int? intBoxed,
            double? doubleBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            return bean;
        }
    }
} // end of namespace