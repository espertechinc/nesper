///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreCase
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreCaseSyntax1Sum());
            executions.Add(new ExprCoreCaseSyntax1SumOM());
            executions.Add(new ExprCoreCaseSyntax1SumCompile());
            executions.Add(new ExprCoreCaseSyntax1WithElse());
            executions.Add(new ExprCoreCaseSyntax1WithElseOM());
            executions.Add(new ExprCoreCaseSyntax1WithElseCompile());
            executions.Add(new ExprCoreCaseSyntax1Branches3());
            executions.Add(new ExprCoreCaseSyntax2());
            executions.Add(new ExprCoreCaseSyntax2StringsNBranches());
            executions.Add(new ExprCoreCaseSyntax2NoElseWithNull());
            executions.Add(new ExprCoreCaseSyntax1WithNull());
            executions.Add(new ExprCoreCaseSyntax2WithNullOM());
            executions.Add(new ExprCoreCaseSyntax2WithNullCompile());
            executions.Add(new ExprCoreCaseSyntax2WithNull());
            executions.Add(new ExprCoreCaseSyntax2WithNullBool());
            executions.Add(new ExprCoreCaseSyntax2WithCoercion());
            executions.Add(new ExprCoreCaseSyntax2WithinExpression());
            executions.Add(new ExprCoreCaseSyntax2Sum());
            executions.Add(new ExprCoreCaseSyntax2EnumChecks());
            executions.Add(new ExprCoreCaseSyntax2EnumResult());
            executions.Add(new ExprCoreCaseSyntax2NoAsName());
            executions.Add(new ExprCoreCaseWithArrayResult());
            return executions;
        }

        private static void RunCaseSyntax1Sum(RegressionEnvironment env)
        {
            SendMarketDataEvent(env, "DELL", 10000, 50);
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(50.0, theEvent.Get("P1"));

            SendMarketDataEvent(env, "DELL", 10000, 50);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(100.0, theEvent.Get("P1"));

            SendMarketDataEvent(env, "CSCO", 4000, 5);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("P1"));

            SendMarketDataEvent(env, "GE", 20, 30);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(20.0, theEvent.Get("P1"));
        }

        private static void RunCaseSyntax1WithElse(RegressionEnvironment env)
        {
            SendMarketDataEvent(env, "CSCO", 4000, 0);
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(4000L, theEvent.Get("P1"));

            SendMarketDataEvent(env, "DELL", 20, 0);
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(3 * 20L, theEvent.Get("P1"));
        }

        private static void RunCaseSyntax2WithNull(RegressionEnvironment env)
        {
            SendSupportBeanEvent(env, 4);
            Assert.AreEqual(2.0, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
            SendSupportBeanEvent(env, 1);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
            SendSupportBeanEvent(env, 2);
            Assert.AreEqual(1.0, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
            SendSupportBeanEvent(env, 3);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
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
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        internal class ExprCoreCaseWithArrayResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select case when IntPrimitive = 1 then { 1, 2 } else { 1, 2 } end as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertEqualsExactOrder(
                    (int?[]) env.Listener("s0").AssertOneGetNewAndReset().Get("c1"),
                    new int?[] {1, 2});

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1Sum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Testing the two forms of the case expression
                // Furthermore the test checks the different when clauses and actions related.
                var epl = "@Name('s0') select case " +
                          " when Symbol='GE' then Volume " +
                          " when Symbol='DELL' then sum(Price) " +
                          "end as P1 from SupportMarketDataBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax1Sum(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1SumOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.CaseWhenThen()
                            .Add(Expressions.Eq("Symbol", "GE"), Expressions.Property("Volume"))
                            .Add(Expressions.Eq("Symbol", "DELL"), Expressions.Sum("Price")),
                        "P1");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportMarketDataBean).Name)
                        .AddView("win", "length", Expressions.Constant(10)));
                model = env.CopyMayFail(model);

                var epl = "select case" +
                          " when Symbol=\"GE\" then Volume" +
                          " when Symbol=\"DELL\" then sum(Price) " +
                          "end as P1 from SupportMarketDataBean.win:length(10)";

                Assert.AreEqual(epl, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax1Sum(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1SumCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case" +
                          " when Symbol=\"GE\" then Volume" +
                          " when Symbol=\"DELL\" then sum(Price) " +
                          "end as P1 from SupportMarketDataBean#length(10)";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax1Sum(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1WithElse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Adding to the EPL statement an else expression
                // when a CSCO ticker is sent the property for the else expression is selected
                var epl = "@Name('s0') select case " +
                          " when Symbol='DELL' then 3 * Volume " +
                          " else Volume " +
                          "end as P1 from SupportMarketDataBean#length(3)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax1WithElse(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1WithElseOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.CaseWhenThen()
                            .SetElse(Expressions.Property("Volume"))
                            .Add(
                                Expressions.Eq("Symbol", "DELL"),
                                Expressions.Multiply(Expressions.Property("Volume"), Expressions.Constant(3))),
                        "P1");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportMarketDataBean).Name)
                        .AddView("length", Expressions.Constant(10)));
                model = env.CopyMayFail(model);

                var epl = "select case " +
                          "when Symbol=\"DELL\" then Volume*3 " +
                          "else Volume " +
                          "end as P1 from SupportMarketDataBean#length(10)";
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax1WithElse(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1WithElseCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case " +
                          "when Symbol=\"DELL\" then Volume*3 " +
                          "else Volume " +
                          "end as P1 from SupportMarketDataBean#length(10)";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax1WithElse(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1Branches3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Same test but the where clause doesn't match any of the condition of the case expresssion
                var epl = "@Name('s0') select case " +
                          " when (Symbol='GE') then Volume " +
                          " when (Symbol='DELL') then Volume / 2.0 " +
                          " when (Symbol='MSFT') then Volume / 3.0 " +
                          " end as P1 from " +
                          typeof(SupportMarketDataBean).Name;
                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendMarketDataEvent(env, "DELL", 10000, 0);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(10000 / 2.0, theEvent.Get("P1"));

                SendMarketDataEvent(env, "MSFT", 10000, 0);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(10000 / 3.0, theEvent.Get("P1"));

                SendMarketDataEvent(env, "GE", 10000, 0);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(10000.0, theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case IntPrimitive " +
                          " when LongPrimitive then (IntPrimitive + LongPrimitive) " +
                          " when DoublePrimitive then IntPrimitive * DoublePrimitive" +
                          " when FloatPrimitive then FloatPrimitive / DoublePrimitive " +
                          " else (IntPrimitive + LongPrimitive + FloatPrimitive + DoublePrimitive) end as P1 " +
                          " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                // intPrimitive = longPrimitive
                // case result is IntPrimitive + longPrimitive
                SendSupportBeanEvent(env, 2, 2L, 1.0f, 1.0);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(4.0, theEvent.Get("P1"));
                // intPrimitive = doublePrimitive
                // case result is IntPrimitive * doublePrimitive
                SendSupportBeanEvent(env, 5, 1L, 1.0f, 5.0);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(25.0, theEvent.Get("P1"));
                // IntPrimitive = floatPrimitive
                // case result is floatPrimitive / doublePrimitive
                SendSupportBeanEvent(env, 12, 1L, 12.0f, 4.0);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(3.0, theEvent.Get("P1"));
                // all the properties of the event are different
                // The else part is computed: 1+2+3+4 = 10
                SendSupportBeanEvent(env, 1, 2L, 3.0f, 4.0);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(10.0, theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2StringsNBranches : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test of the various coercion user cases.
                var epl = "@Name('s0') select case IntPrimitive" +
                          " when 1 then Convert.ToString(BoolPrimitive) " +
                          " when 2 then Convert.ToString(BoolBoxed) " +
                          " when 3 then Convert.ToString(IntPrimitive) " +
                          " when 4 then Convert.ToString(IntBoxed)" +
                          " when 5 then Convert.ToString(LongPrimitive) " +
                          " when 6 then Convert.ToString(LongBoxed) " +
                          " when 7 then Convert.ToString(CharPrimitive) " +
                          " when 8 then Convert.ToString(CharBoxed) " +
                          " when 9 then Convert.ToString(ShortPrimitive) " +
                          " when 10 then Convert.ToString(ShortBoxed) " +
                          " when 11 then Convert.ToString(BytePrimitive) " +
                          " when 12 then Convert.ToString(ByteBoxed) " +
                          " when 13 then Convert.ToString(FloatPrimitive) " +
                          " when 14 then Convert.ToString(FloatBoxed) " +
                          " when 15 then Convert.ToString(DoublePrimitive) " +
                          " when 16 then Convert.ToString(DoubleBoxed) " +
                          " when 17 then TheString " +
                          " else 'x' end as P1 " +
                          " from SupportBean#length(1)";

                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("True", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("False", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("3", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("4", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("5", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("6", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("A", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("a", theEvent.Get("P1"));

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
                    0,
                    0.0,
                    0.0d,
                    null,
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("9", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("10", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("11", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("12", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("13", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("14", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("15", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("16", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("testCoercion", theEvent.Get("P1"));

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
                    14,
                    15.0,
                    16.0d,
                    "testCoercion",
                    SupportEnum.ENUM_VALUE_1);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("x", theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2NoElseWithNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case TheString " +
                          " when null then true " +
                          " when '' then false end as P1" +
                          " from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, "x");
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                SendSupportBeanEvent(env, "null");
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                SendSupportBeanEvent(env, null);
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                SendSupportBeanEvent(env, "");
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax1WithNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case " +
                          " when TheString is null then true " +
                          " when TheString = '' then false end as P1" +
                          " from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, "x");
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                SendSupportBeanEvent(env, "null");
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                SendSupportBeanEvent(env, null);
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                SendSupportBeanEvent(env, "");
                Assert.AreEqual(false, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2WithNullOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select case IntPrimitive " +
                          "when 1 then null " +
                          "when 2 then 1.0d " +
                          "when 3 then null " +
                          "else 2 " +
                          "end as P1 from SupportBean#length(100)";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.CaseSwitch("IntPrimitive")
                            .SetElse(Expressions.Constant(2))
                            .Add(Expressions.Constant(1), Expressions.Constant(null))
                            .Add(Expressions.Constant(2), Expressions.Constant(1.0))
                            .Add(Expressions.Constant(3), Expressions.Constant(null)),
                        "P1");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportBean).Name).AddView("length", Expressions.Constant(100)));
                model = env.CopyMayFail(model);

                Assert.AreEqual(epl, model.ToEPL());
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax2WithNull(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2WithNullCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case IntPrimitive " +
                          "when 1 then null " +
                          "when 2 then 1.0d " +
                          "when 3 then null " +
                          "else 2 " +
                          "end as P1 from SupportBean#length(100)";

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax2WithNull(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2WithNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case IntPrimitive " +
                          " when 1 then null " +
                          " when 2 then 1.0" +
                          " when 3 then null " +
                          " else 2 " +
                          " end as P1 from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                RunCaseSyntax2WithNull(env);

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2WithNullBool : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case BoolBoxed " +
                          " when null then 1 " +
                          " when true then 2l" +
                          " when false then 3 " +
                          " end as P1 from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, null);
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
                SendSupportBeanEvent(env, false);
                Assert.AreEqual(3L, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
                SendSupportBeanEvent(env, true);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2WithCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case IntPrimitive " +
                          " when 1.0 then null " +
                          " when 4/2.0 then 'x'" +
                          " end as P1 from SupportBean#length(100)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, 1);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
                SendSupportBeanEvent(env, 2);
                Assert.AreEqual("x", env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));
                SendSupportBeanEvent(env, 3);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2WithinExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select 2 * (case IntPrimitive" +
                          " when 1 then 2 " +
                          " when 2 then 3 " +
                          " else 10 end) as P1 " +
                          " from SupportBean#length(1)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, 1);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(4, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 2);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(6, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 3);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(20, theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2Sum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case IntPrimitive" +
                          " when 1 then sum(LongPrimitive) " +
                          " when 2 then sum(FloatPrimitive) " +
                          " else sum(IntPrimitive) end as P1 " +
                          " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, 1, 10L, 3.0f, 4.0);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(10d, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 1, 15L, 3.0f, 4.0);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(25d, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 2, 1L, 3.0f, 4.0);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(9d, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 2, 1L, 3.0f, 4.0);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(12.0d, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 5, 1L, 1.0f, 1.0);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(11.0d, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 5, 1L, 1.0f, 1.0);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(16d, theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2EnumChecks : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case SupportEnum " +
                          " when " + typeof(SupportEnumHelper).FullName + ".GetValueForEnum(0) then 1 " +
                          " when " + typeof(SupportEnumHelper).FullName + ".GetValueForEnum(1) then 2 " +
                          " end as P1 " +
                          " from " + typeof(SupportBeanWithEnum).Name + "#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, "a", SupportEnum.ENUM_VALUE_1);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(1, theEvent.Get("P1"));

                SendSupportBeanEvent(env, "b", SupportEnum.ENUM_VALUE_2);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(2, theEvent.Get("P1"));

                SendSupportBeanEvent(env, "c", SupportEnum.ENUM_VALUE_3);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(null, theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2EnumResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select case IntPrimitive * 2 " +
                          " when 2 then " + typeof(SupportEnumHelper).FullName + ".GetValueForEnum(0) " +
                          " when 4 then " + typeof(SupportEnumHelper).FullName + ".GetValueForEnum(1) " +
                          " else " + typeof(SupportEnumHelper).FullName + ".GetValueForEnum(2) " +
                          " end as P1 " +
                          " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(SupportEnum?), env.Statement("s0").EventType.GetPropertyType("P1"));

                SendSupportBeanEvent(env, 1);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(SupportEnum.ENUM_VALUE_1, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 2);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(SupportEnum.ENUM_VALUE_2, theEvent.Get("P1"));

                SendSupportBeanEvent(env, 3);
                theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(SupportEnum.ENUM_VALUE_3, theEvent.Get("P1"));

                env.UndeployAll();
            }
        }

        internal class ExprCoreCaseSyntax2NoAsName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var caseSubExpr = "case IntPrimitive when 1 then 0 end";
                var epl = "@Name('s0') select " + caseSubExpr + " from SupportBean#length(10)";

                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(caseSubExpr));

                SendSupportBeanEvent(env, 1);
                var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual(0, theEvent.Get(caseSubExpr));

                env.UndeployAll();
            }
        }
    }
} // end of namespace