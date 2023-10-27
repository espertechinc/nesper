///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreCase
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSyntax1Sum(execs);
            WithSyntax1SumOM(execs);
            WithSyntax1SumCompile(execs);
            WithSyntax1WithElse(execs);
            WithSyntax1WithElseOM(execs);
            WithSyntax1WithElseCompile(execs);
            WithSyntax1Branches3(execs);
            WithSyntax2(execs);
            WithSyntax2StringsNBranches(execs);
            WithSyntax2NoElseWithNull(execs);
            WithSyntax1WithNull(execs);
            WithSyntax2WithNullOM(execs);
            WithSyntax2WithNullCompile(execs);
            WithSyntax2WithNull(execs);
            WithSyntax2WithNullBool(execs);
            WithSyntax2WithCoercion(execs);
            WithSyntax2WithinExpression(execs);
            WithSyntax2Sum(execs);
            WithSyntax2EnumChecks(execs);
            WithSyntax2EnumResult(execs);
            WithSyntax2NoAsName(execs);
            WithWithArrayResult(execs);
            WithWithTypeParameterizedProperty(execs);
            WithWithTypeParameterizedPropertyInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithTypeParameterizedPropertyInvalid(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseWithTypeParameterizedPropertyInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithWithTypeParameterizedProperty(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseWithTypeParameterizedProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithWithArrayResult(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseWithArrayResult());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2NoAsName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2NoAsName());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2EnumResult(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2EnumResult());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2EnumChecks(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2EnumChecks());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2Sum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2Sum());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2WithinExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2WithinExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2WithCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2WithCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2WithNullBool(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2WithNullBool());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2WithNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2WithNull());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2WithNullCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2WithNullCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2WithNullOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2WithNullOM());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1WithNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1WithNull());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2NoElseWithNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2NoElseWithNull());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2StringsNBranches(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2StringsNBranches());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax2());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1Branches3(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1Branches3());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1WithElseCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1WithElseCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1WithElseOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1WithElseOM());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1WithElse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1WithElse());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1SumCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1SumCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1SumOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1SumOM());
            return execs;
        }

        public static IList<RegressionExecution> WithSyntax1Sum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCaseSyntax1Sum());
            return execs;
        }

        private class ExprCoreCaseWithTypeParameterizedProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "IList<Integer>", "IList<Integer>", typeof(IList<int?>));
                RunAssertion(env, "IList<IList<Integer>>", "IList<IList<Integer>>", typeof(IList<IList<int?>>));
                RunAssertion(env, "IList<int[primitive]>", "IList<int[primitive]>", typeof(IList<int[]>));

                RunAssertion(env, "Integer[]", "String[]", typeof(object[]));

                RunAssertion(env, "IList<Integer>", "IList<Object>", typeof(IList<object>));
                RunAssertion(env, "ICollection<Integer>", "IList<Object>", typeof(object));

                RunAssertion(env, "ICollection<Integer>", "null", typeof(ICollection<int?>));
            }

            private void RunAssertion(
                RegressionEnvironment env,
                string typeOne,
                string typeTwo,
                Type expected)
            {
                var eplSyntaxOne = GetEPLSyntaxOne(typeOne, typeTwo);
                RunAssertionEPL(env, eplSyntaxOne, expected);

                var eplSyntaxTwo = GetEPLSyntaxTwo(typeOne, typeTwo);
                RunAssertionEPL(env, eplSyntaxTwo, expected);
            }

            private void RunAssertionEPL(
                RegressionEnvironment env,
                string epl,
                Type expected)
            {
                env.CompileDeploy(epl);
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(expected, statement.EventType.GetPropertyType("thecase")));
                env.UndeployAll();
            }

            private string GetEPLSyntaxOne(
                string typeOne,
                string typeTwo)
            {
                return "create schema MyEvent(switch boolean, fieldOne " +
                       typeOne +
                       ", fieldTwo " +
                       typeTwo +
                       ");\n" +
                       "@name('s0') select case when switch then fieldOne else fieldTwo end as thecase from MyEvent;\n";
            }

            private string GetEPLSyntaxTwo(
                string typeOne,
                string typeTwo)
            {
                return "create schema MyEvent(switch boolean, fieldOne " +
                       typeOne +
                       ", fieldTwo " +
                       typeTwo +
                       ");\n" +
                       "@name('s0') select case switch when true then fieldOne when false then fieldTwo end as thecase from MyEvent;\n";
            }
        }

        private class ExprCoreCaseWithTypeParameterizedPropertyInvalid : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }

            public void Run(RegressionEnvironment env)
            {
                RunInvalid(env, "int[primitive]", "String[]", "Cannot coerce to int[] type String[]");
                RunInvalid(env, "long[primitive]", "long[]", "Cannot coerce to long[] type Long[]");
                RunInvalid(env, "null", "null", "Null-type return value is not allowed");
            }

            private void RunInvalid(
                RegressionEnvironment env,
                string typeOne,
                string typeTwo,
                string detail)
            {
                var eplSyntaxOne = GetEPLSyntaxOne(typeOne, typeTwo);
                RunInvalidEPL(env, eplSyntaxOne, detail);

                var eplSyntaxTwo = GetEPLSyntaxTwo(typeOne, typeTwo);
                RunInvalidEPL(env, eplSyntaxTwo, detail);
            }

            private void RunInvalidEPL(
                RegressionEnvironment env,
                string epl,
                string detail)
            {
                try {
                    env.CompileWCheckedEx(epl);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    if (!ex.Message.Contains(detail)) {
                        Assert.AreEqual(detail, ex.Message);
                    }
                }
            }

            private string GetEPLSyntaxOne(
                string typeOne,
                string typeTwo)
            {
                return "create schema MyEvent(switch boolean, fieldOne " +
                       typeOne +
                       ", fieldTwo " +
                       typeTwo +
                       ");\n" +
                       "@name('s0') select case when switch then fieldOne else fieldTwo end as thecase from MyEvent;\n";
            }

            private string GetEPLSyntaxTwo(
                string typeOne,
                string typeTwo)
            {
                return "create schema MyEvent(switch boolean, fieldOne " +
                       typeOne +
                       ", fieldTwo " +
                       typeTwo +
                       ");\n" +
                       "@name('s0') select case switch when true then fieldOne when false then fieldTwo end as thecase from MyEvent;\n";
            }
        }

        private class ExprCoreCaseWithArrayResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select case when IntPrimitive = 1 then { 1, 2 } else { 1, 2 } end as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("c1"), new int?[] { 1, 2 }));

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax1Sum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Testing the two forms of the case expression
                // Furthermore the test checks the different when clauses and actions related.
                var epl = "@name('s0') select case " +
" when Symbol='GE' then Volume "+
" when Symbol='DELL' then sum(Price) "+
                          "end as p1 from SupportMarketDataBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(double?));

                RunCaseSyntax1Sum(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax1SumOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var copier = SerializableObjectCopier.GetInstance(env.Container);
                var model = new EPStatementObjectModel();
                model
                    .SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.CaseWhenThen()
                            .Add(Expressions.Eq("Symbol", "GE"), Expressions.Property("Volume"))
                            .Add(Expressions.Eq("Symbol", "DELL"), Expressions.Sum("Price")),
                        "p1");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportMarketDataBean))
                        .AddView("win", "length", Expressions.Constant(10)));
                model = copier.Copy(model);

                var epl = "select case" +
" when Symbol=\"GE\" then Volume"+
" when Symbol=\"DELL\" then sum(Price) "+
                          "end as p1 from SupportMarketDataBean.win:length(10)";

                Assert.AreEqual(epl, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                env.AssertStmtType("s0", "p1", typeof(double?));

                RunCaseSyntax1Sum(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax1SumCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case" +
" when Symbol=\"GE\" then Volume"+
" when Symbol=\"DELL\" then sum(Price) "+
                          "end as p1 from SupportMarketDataBean#length(10)";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                env.AssertStmtType("s0", "p1", typeof(double?));

                RunCaseSyntax1Sum(env);

                env.UndeployAll();
            }
        }

        private static void RunCaseSyntax1Sum(RegressionEnvironment env)
        {
            SendMarketDataEvent(env, "DELL", 10000, 50);
            env.AssertEqualsNew("s0", "p1", 50.0);

            SendMarketDataEvent(env, "DELL", 10000, 50);
            env.AssertEqualsNew("s0", "p1", 100.0);

            SendMarketDataEvent(env, "CSCO", 4000, 5);
            env.AssertEqualsNew("s0", "p1", null);

            SendMarketDataEvent(env, "GE", 20, 30);
            env.AssertEqualsNew("s0", "p1", 20.0);
        }

        private class ExprCoreCaseSyntax1WithElse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Adding to the EPL statement an else expression
                // when a CSCO ticker is sent the property for the else expression is selected
                var epl = "@name('s0') select case " +
" when Symbol='DELL' then 3 * Volume "+
                          " else Volume " +
                          "end as p1 from SupportMarketDataBean#length(3)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(long?));

                RunCaseSyntax1WithElse(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax1WithElseOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var copier = SerializableObjectCopier.GetInstance(env.Container);
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.CaseWhenThen()
                            .SetElse(Expressions.Property("Volume"))
                            .Add(
                                Expressions.Eq("Symbol", "DELL"),
                                Expressions.Multiply(Expressions.Property("Volume"), Expressions.Constant(3))),
                        "p1");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportMarketDataBean)).AddView("length", Expressions.Constant(10)));
                model = copier.Copy(model);

                var epl = "select case " +
"when Symbol=\"DELL\" then Volume*3 "+
                          "else Volume " +
                          "end as p1 from SupportMarketDataBean#length(10)";
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                env.AssertStmtType("s0", "p1", typeof(long?));

                RunCaseSyntax1WithElse(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax1WithElseCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case " +
"when Symbol=\"DELL\" then Volume*3 "+
                          "else Volume " +
                          "end as p1 from SupportMarketDataBean#length(10)";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                env.AssertStmtType("s0", "p1", typeof(long?));

                RunCaseSyntax1WithElse(env);

                env.UndeployAll();
            }
        }

        private static void RunCaseSyntax1WithElse(RegressionEnvironment env)
        {
            SendMarketDataEvent(env, "CSCO", 4000, 0);
            env.AssertEqualsNew("s0", "p1", 4000L);

            SendMarketDataEvent(env, "DELL", 20, 0);
            env.AssertEqualsNew("s0", "p1", 3 * 20L);
        }

        private class ExprCoreCaseSyntax1Branches3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportMarketDataBean")
                    .WithExpressions(
                        fields,
"case when (Symbol='GE') then Volume "+
" when (Symbol='DELL') then Volume / 2.0 "+
" when (Symbol='MSFT') then Volume / 3.0 "+
                        " end")
                    .WithStatementConsumer(
                        stmt => Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("c0")));

                builder.WithAssertion(MakeMarketDataEvent("DELL", 10000, 0)).Expect(fields, 10000 / 2.0);
                builder.WithAssertion(MakeMarketDataEvent("MSFT", 10000, 0)).Expect(fields, 10000 / 3.0);
                builder.WithAssertion(MakeMarketDataEvent("GE", 10000, 0)).Expect(fields, 10000.0);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(
                        fields,
                        "case IntPrimitive " +
                        " when LongPrimitive then (IntPrimitive + LongPrimitive) " +
                        " when DoublePrimitive then IntPrimitive * DoublePrimitive" +
                        " when FloatPrimitive then FloatPrimitive / DoublePrimitive " +
                        " else (IntPrimitive + LongPrimitive + FloatPrimitive + DoublePrimitive) end")
                    .WithStatementConsumer(
                        stmt => Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("c0")));

                // intPrimitive = longPrimitive
                // case result is intPrimitive + longPrimitive
                builder.WithAssertion(MakeSupportBeanEvent(2, 2L, 1.0f, 1.0)).Expect(fields, 4.0);

                // intPrimitive = doublePrimitive
                // case result is intPrimitive * doublePrimitive
                builder.WithAssertion(MakeSupportBeanEvent(5, 1L, 1.0f, 5.0)).Expect(fields, 25.0);

                // intPrimitive = floatPrimitive
                // case result is floatPrimitive / doublePrimitive
                builder.WithAssertion(MakeSupportBeanEvent(12, 1L, 12.0f, 4.0)).Expect(fields, 3.0);

                // all the properties of the event are different
                // The else part is computed: 1+2+3+4 = 10
                builder.WithAssertion(MakeSupportBeanEvent(1, 2L, 3.0f, 4.0)).Expect(fields, 10.0);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2StringsNBranches : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compat = typeof(CompatExtensions).FullName;

                // Test of the various coercion user cases.
                var epl = "@name('s0') select case IntPrimitive" +
                          $" when 1 then {compat}.RenderAny(BoolPrimitive) " +
                          $" when 2 then {compat}.RenderAny(BoolBoxed) " +
                          $" when 3 then {compat}.RenderAny(IntPrimitive) " +
                          $" when 4 then {compat}.RenderAny(IntBoxed)" +
                          $" when 5 then {compat}.RenderAny(LongPrimitive) " +
                          $" when 6 then {compat}.RenderAny(LongBoxed) " +
                          $" when 7 then {compat}.RenderAny(CharPrimitive) " +
                          $" when 8 then {compat}.RenderAny(CharBoxed) " +
                          $" when 9 then {compat}.RenderAny(ShortPrimitive) " +
                          $" when 10 then {compat}.RenderAny(ShortBoxed) " +
                          $" when 11 then {compat}.RenderAny(BytePrimitive) " +
                          $" when 12 then {compat}.RenderAny(ByteBoxed) " +
                          $" when 13 then {compat}.RenderAny(FloatPrimitive) " +
                          $" when 14 then {compat}.RenderAny(FloatBoxed) " +
                          $" when 15 then {compat}.RenderAny(DoublePrimitive) " +
                          $" when 16 then {compat}.RenderAny(DoubleBoxed) " +
                          $" when 17 then TheString " +
                          " else 'x' end as p1 " +
                          " from SupportBean#length(1)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(string));

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    1,
                    0,
                    0L,
                    0L,
                    '0',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "true");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    2,
                    0,
                    0L,
                    0L,
                    '0',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "false");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    3,
                    0,
                    0L,
                    0L,
                    '0',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "3");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    4,
                    4,
                    0L,
                    0L,
                    '0',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "4");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    5,
                    0,
                    5L,
                    0L,
                    '0',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "5");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    6,
                    0,
                    0L,
                    6L,
                    '0',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "6");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    7,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "A");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    8,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    0,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "a");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    9,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    0,
                    0,
                    0,
                    0.0f,
                    0.0f,
                    0.0,
                    0.0,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "9");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    10,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "10");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    11,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "11");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    12,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "12");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    13,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "13.0");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    14,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "14.0");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    15,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "15.0");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    16,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "16.0");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    17,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "testCoercion");

                SendSupportBeanEvent(
                    env,
                    true,
                    false,
                    -1,
                    0,
                    0L,
                    0L,
                    'A',
                    'a',
                    9,
                    10,
                    11,
                    12,
                    13.0f,
                    14.0f,
                    15.0,
                    16.0,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                AssertP1(env, "x");

                env.UndeployAll();
            }

            private void AssertP1(
                RegressionEnvironment env,
                string expected)
            {
                env.AssertListener(
                    "s0",
                    listener => {
                        var theEvent = listener.GetAndResetLastNewData()[0];
                        Assert.AreEqual(expected, theEvent.Get("p1"));
                    });
            }
        }

        private class ExprCoreCaseSyntax2NoElseWithNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case TheString " +
                          " when null then true " +
                          " when '' then false end as p1" +
                          " from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(bool?));

                SendSupportBeanEvent(env, "x");
                env.AssertEqualsNew("s0", "p1", null);

                SendSupportBeanEvent(env, "null");
                env.AssertEqualsNew("s0", "p1", null);

                SendSupportBeanEvent(env, null);
                env.AssertEqualsNew("s0", "p1", true);

                SendSupportBeanEvent(env, "");
                env.AssertEqualsNew("s0", "p1", false);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax1WithNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case " +
                          " when TheString is null then true " +
                          " when TheString = '' then false end as p1" +
                          " from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(bool?));

                SendSupportBeanEvent(env, "x");
                env.AssertEqualsNew("s0", "p1", null);

                SendSupportBeanEvent(env, "null");
                env.AssertEqualsNew("s0", "p1", null);

                SendSupportBeanEvent(env, null);
                env.AssertEqualsNew("s0", "p1", true);

                SendSupportBeanEvent(env, "");
                env.AssertEqualsNew("s0", "p1", false);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2WithNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select case IntPrimitive " +
                          "when 1 then null " +
                          "when 2 then 1.0d " +
                          "when 3 then null " +
                          "else 2 " +
                          "end as p1 from SupportBean#length(100)";

                var copier = SerializableObjectCopier.GetInstance(env.Container);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.CaseSwitch("IntPrimitive")
                            .SetElse(Expressions.Constant(2))
                            .Add(Expressions.Constant(1), Expressions.Constant(null))
                            .Add(Expressions.Constant(2), Expressions.Constant(1.0))
                            .Add(Expressions.Constant(3), Expressions.Constant(null)),
                        "p1");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportBean)).AddView("length", Expressions.Constant(100)));
                model = copier.Copy(model);

                Assert.AreEqual(epl, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);
                env.AssertStmtType("s0", "p1", typeof(double?));

                RunCaseSyntax2WithNull(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2WithNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case IntPrimitive " +
                          "when 1 then null " +
                          "when 2 then 1.0d " +
                          "when 3 then null " +
                          "else 2 " +
                          "end as p1 from SupportBean#length(100)";

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(double?));

                RunCaseSyntax2WithNull(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2WithNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case IntPrimitive " +
                          " when 1 then null " +
                          " when 2 then 1.0" +
                          " when 3 then null " +
                          " else 2 " +
                          " end as p1 from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(double?));

                RunCaseSyntax2WithNull(env);

                env.UndeployAll();
            }
        }

        private static void RunCaseSyntax2WithNull(RegressionEnvironment env)
        {
            SendSupportBeanEvent(env, 4);
            env.AssertEqualsNew("s0", "p1", 2.0);
            SendSupportBeanEvent(env, 1);
            env.AssertEqualsNew("s0", "p1", null);
            SendSupportBeanEvent(env, 2);
            env.AssertEqualsNew("s0", "p1", 1.0);
            SendSupportBeanEvent(env, 3);
            env.AssertEqualsNew("s0", "p1", null);
        }

        private class ExprCoreCaseSyntax2WithNullBool : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case BoolBoxed " +
                          " when null then 1 " +
                          " when true then 2l" +
                          " when false then 3 " +
                          " end as p1 from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(long?));

                SendSupportBeanEvent(env, null);
                env.AssertEqualsNew("s0", "p1", 1L);
                SendSupportBeanEvent(env, false);
                env.AssertEqualsNew("s0", "p1", 3L);
                SendSupportBeanEvent(env, true);
                env.AssertEqualsNew("s0", "p1", 2L);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2WithCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case IntPrimitive " +
                          " when 1.0 then null " +
                          " when 4/2.0 then 'x'" +
                          " end as p1 from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(string));

                SendSupportBeanEvent(env, 1);
                env.AssertEqualsNew("s0", "p1", null);

                SendSupportBeanEvent(env, 2);
                env.AssertEqualsNew("s0", "p1", "x");
                SendSupportBeanEvent(env, 3);
                env.AssertEqualsNew("s0", "p1", null);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2WithinExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select 2 * (case " +
                          " IntPrimitive when 1 then 2 " +
                          " when 2 then 3 " +
                          " else 10 end) as p1 " +
                          " from SupportBean#length(1)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(int?));

                SendSupportBeanEvent(env, 1);
                env.AssertEqualsNew("s0", "p1", 4);

                SendSupportBeanEvent(env, 2);
                env.AssertEqualsNew("s0", "p1", 6);

                SendSupportBeanEvent(env, 3);
                env.AssertEqualsNew("s0", "p1", 20);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2Sum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case IntPrimitive when 1 then sum(LongPrimitive) " +
                          " when 2 then sum(FloatPrimitive) " +
                          " else sum(IntPrimitive) end as p1 " +
                          " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(double?));

                SendSupportBeanEvent(env, 1, 10L, 3.0f, 4.0);
                env.AssertEqualsNew("s0", "p1", 10d);

                SendSupportBeanEvent(env, 1, 15L, 3.0f, 4.0);
                env.AssertEqualsNew("s0", "p1", 25d);

                SendSupportBeanEvent(env, 2, 1L, 3.0f, 4.0);
                env.AssertEqualsNew("s0", "p1", 9d);

                SendSupportBeanEvent(env, 2, 1L, 3.0f, 4.0);
                env.AssertEqualsNew("s0", "p1", 12d);

                SendSupportBeanEvent(env, 5, 1L, 1.0f, 1.0);
                env.AssertEqualsNew("s0", "p1", 11d);

                SendSupportBeanEvent(env, 5, 1L, 1.0f, 1.0);
                env.AssertEqualsNew("s0", "p1", 16d);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2EnumChecks : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case SupportEnum " +
                          " when " +
                          nameof(SupportEnum) +
                          ".getValueForEnum(0) then 1 " +
                          " when " +
                          nameof(SupportEnum) +
                          ".getValueForEnum(1) then 2 " +
                          " end as p1 " +
                          " from " +
                          nameof(SupportBeanWithEnum) +
                          "#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(int?));

                SendSupportBeanEvent(env, "a", SupportEnum.ENUM_VALUE_1);
                env.AssertEqualsNew("s0", "p1", 1);

                SendSupportBeanEvent(env, "b", SupportEnum.ENUM_VALUE_2);
                env.AssertEqualsNew("s0", "p1", 2);

                SendSupportBeanEvent(env, "c", SupportEnum.ENUM_VALUE_3);
                env.AssertEqualsNew("s0", "p1", null);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2EnumResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select case IntPrimitive * 2 " +
                          " when 2 then " +
                          nameof(SupportEnum) +
                          ".getValueForEnum(0) " +
                          " when 4 then " +
                          nameof(SupportEnum) +
                          ".getValueForEnum(1) " +
                          " else " +
                          nameof(SupportEnum) +
                          ".getValueForEnum(2) " +
                          " end as p1 " +
                          " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", "p1", typeof(SupportEnum));

                SendSupportBeanEvent(env, 1);
                env.AssertEqualsNew("s0", "p1", SupportEnum.ENUM_VALUE_1);

                SendSupportBeanEvent(env, 2);
                env.AssertEqualsNew("s0", "p1", SupportEnum.ENUM_VALUE_2);

                SendSupportBeanEvent(env, 3);
                env.AssertEqualsNew("s0", "p1", SupportEnum.ENUM_VALUE_3);

                env.UndeployAll();
            }
        }

        private class ExprCoreCaseSyntax2NoAsName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var caseSubExpr = "case IntPrimitive when 1 then 0 end";
                var epl = "@name('s0') select " +
                          caseSubExpr +
                          " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtType("s0", caseSubExpr, typeof(int?));

                SendSupportBeanEvent(env, 1);
                env.AssertEqualsNew("s0", caseSubExpr, 0);

                env.UndeployAll();
            }
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            bool b,
            bool? boolBoxed,
            int i,
            int? intBoxed,
            long l,
            long? longBoxed,
            char c,
            char? charBoxed,
            short s,
            short? shortBoxed,
            byte by,
            byte? byteBoxed,
            float f,
            float? floatBoxed,
            double d,
            double? doubleBoxed,
            string str,
            SupportEnum enumval)
        {
            var theEvent = new SupportBean();
            theEvent.BoolPrimitive = b;
            theEvent.BoolBoxed = boolBoxed;
            theEvent.IntPrimitive = i;
            theEvent.IntBoxed = intBoxed;
            theEvent.LongPrimitive = l;
            theEvent.LongBoxed = longBoxed;
            theEvent.CharPrimitive = c;
            theEvent.CharBoxed = charBoxed;
            theEvent.ShortPrimitive = s;
            theEvent.ShortBoxed = shortBoxed;
            theEvent.BytePrimitive = by;
            theEvent.ByteBoxed = byteBoxed;
            theEvent.FloatPrimitive = f;
            theEvent.FloatBoxed = floatBoxed;
            theEvent.DoublePrimitive = d;
            theEvent.DoubleBoxed = doubleBoxed;
            theEvent.TheString = str;
            theEvent.EnumValue = enumval;
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive,
            long longPrimitive,
            float floatPrimitive,
            double doublePrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.LongPrimitive = longPrimitive;
            theEvent.FloatPrimitive = floatPrimitive;
            theEvent.DoublePrimitive = doublePrimitive;
            env.SendEventBean(theEvent);
        }

        private static SupportBean MakeSupportBeanEvent(
            int intPrimitive,
            long longPrimitive,
            float floatPrimitive,
            double doublePrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.LongPrimitive = longPrimitive;
            theEvent.FloatPrimitive = floatPrimitive;
            theEvent.DoublePrimitive = doublePrimitive;
            return theEvent;
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            string theString)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            bool boolBoxed)
        {
            var theEvent = new SupportBean();
            theEvent.BoolBoxed = boolBoxed;
            env.SendEventBean(theEvent);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            string theString,
            SupportEnum supportEnum)
        {
            var theEvent = new SupportBeanWithEnum(theString, supportEnum);
            env.SendEventBean(theEvent);
        }

        private static void SendMarketDataEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = MakeMarketDataEvent(symbol, volume, price);
            env.SendEventBean(bean);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            long volume,
            double price)
        {
            return new SupportMarketDataBean(symbol, price, volume, null);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ExprCoreCase));
    }
} // end of namespace