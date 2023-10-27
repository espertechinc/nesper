///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreMath
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithDouble(execs);
            WithLong(execs);
            WithFloat(execs);
            WithIntWNull(execs);
            WithDecimal(execs);
            WithDecimalConv(execs);
            WithBigInt(execs);
            WithBigIntConv(execs);
            WithShortAndByteArithmetic(execs);
            With(Modulo)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithModulo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathModulo());
            return execs;
        }

        public static IList<RegressionExecution> WithShortAndByteArithmetic(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathShortAndByteArithmetic());
            return execs;
        }

        public static IList<RegressionExecution> WithBigIntConv(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathBigIntConv());
            return execs;
        }

        public static IList<RegressionExecution> WithBigInt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathBigInt());
            return execs;
        }

        public static IList<RegressionExecution> WithDecimalConv(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathDecimalConv());
            return execs;
        }

        public static IList<RegressionExecution> WithDecimal(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathDecimal());
            return execs;
        }

        public static IList<RegressionExecution> WithIntWNull(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathIntWNull());
            return execs;
        }

        public static IList<RegressionExecution> WithFloat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathFloat());
            return execs;
        }

        public static IList<RegressionExecution> WithLong(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathLong());
            return execs;
        }

        public static IList<RegressionExecution> WithDouble(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathDouble());
            return execs;
        }

        private static SupportBean MakeBoxedEvent(
            int intPrimitive,
            long? longBoxed,
            int? intBoxed)
        {
            var bean = new SupportBean("E", intPrimitive);
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            return bean;
        }

        private static SupportBean MakeEvent(
            int intPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            return bean;
        }

        private static void AssertTypes(
            EPStatement stmt,
            string[] fields,
            params Type[] types)
        {
            Assert.AreEqual(fields.Length, types.Length);
            for (var i = 0; i < fields.Length; i++) {
                Assert.AreEqual(types[i], stmt.EventType.GetPropertyType(fields[i]), "failed for " + i);
            }
        }

        private class ExprCoreMathDouble : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean").WithExpressions(
                        fields,
                        "10d+5d",
                        "10d-5d",
                        "10d*5d",
                        "10d/5d",
                        "10d%4d")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(double?),
                            typeof(double?),
                            typeof(double?),
                            typeof(double?),
                            typeof(double?)));
                builder.WithAssertion(new SupportBean()).Expect(fields, 15d, 5d, 50d, 2d, 2d);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathLong : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(fields, "10L+5L", "10L-5L", "10L*5L", "10L/5L")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(long?),
                            typeof(long?),
                            typeof(long?),
                            typeof(double?)));
                builder.WithAssertion(new SupportBean()).Expect(fields, 15L, 5L, 50L, 2d);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathFloat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean").WithExpressions(
                        fields,
                        "10f+5f",
                        "10f-5f",
                        "10f*5f",
                        "10f/5f",
                        "10f%4f")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(float?),
                            typeof(float?),
                            typeof(float?),
                            typeof(double?),
                            typeof(float?)));
                builder.WithAssertion(new SupportBean()).Expect(fields, 15f, 5f, 50f, 2d, 2f);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathIntWNull : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "IntPrimitive/IntBoxed")
                    .WithExpression(fields[1], "IntPrimitive*IntBoxed")
                    .WithExpression(fields[2], "IntPrimitive+IntBoxed")
                    .WithExpression(fields[3], "IntPrimitive-IntBoxed")
                    .WithExpression(fields[4], "IntBoxed/IntPrimitive")
                    .WithExpression(fields[5], "IntBoxed*IntPrimitive")
                    .WithExpression(fields[6], "IntBoxed+IntPrimitive")
                    .WithExpression(fields[7], "IntBoxed-IntPrimitive")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(double?),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?),
                            typeof(double?),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?)));

                builder.WithAssertion(MakeEvent(100, 3))
                    .Expect(fields, 100 / 3d, 300, 103, 97, 3 / 100d, 300, 103, -97);
                builder.WithAssertion(MakeEvent(100, null))
                    .Expect(fields, null, null, null, null, null, null, null, null);
                builder.WithAssertion(MakeEvent(100, 0))
                    .Expect(fields, double.PositiveInfinity, 0, 100, 100, 0d, 0, 100, -100);
                builder.WithAssertion(MakeEvent(-5, 0))
                    .Expect(fields, double.NegativeInfinity, 0, -5, -5, -0d, 0, -5, 5);

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathDecimalConv : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean").WithExpression(fields[0], "10+5.0m")
                    .WithExpression(fields[1], "10-5.0m")
                    .WithExpression(fields[2], "10*5.0m")
                    .WithExpression(fields[3], "10/5.0m")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(decimal?),
                            typeof(decimal?),
                            typeof(decimal?),
                            typeof(decimal?)));
                builder.WithAssertion(new SupportBean()).Expect(fields, 15.0m, 5.0m, 50.0m, 2.0m);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathBigIntConv : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bigInteger = typeof(BigIntegerHelper).FullName;
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], $"10+{bigInteger}.ValueOf(5)")
                    .WithExpression(fields[1], $"10-{bigInteger}.ValueOf(5)")
                    .WithExpression(fields[2], $"10*{bigInteger}.ValueOf(5)")
                    .WithExpression(fields[3], $"10/{bigInteger}.ValueOf(5)")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(BigInteger?),
                            typeof(BigInteger?),
                            typeof(BigInteger?),
                            typeof(double?)));

                builder.WithAssertion(new SupportBean())
                    .Expect(fields, new BigInteger(15), new BigInteger(5), new BigInteger(50), 2d);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathBigInt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bigInteger = typeof(BigIntegerHelper).FullName;
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean").WithExpression(
                        fields[0],
                        $"{bigInteger}.ValueOf(10)+{bigInteger}.ValueOf(5)")
                    .WithExpression(fields[1], $"{bigInteger}.ValueOf(10)-{bigInteger}.ValueOf(5)")
                    .WithExpression(fields[2], $"{bigInteger}.ValueOf(10)*{bigInteger}.ValueOf(5)")
                    .WithExpression(fields[3], $"{bigInteger}.ValueOf(10)/{bigInteger}.ValueOf(5)")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(BigInteger?),
                            typeof(BigInteger?),
                            typeof(BigInteger?),
                            typeof(double?)));
                builder.WithAssertion(new SupportBean())
                    .Expect(fields, new BigInteger(15), new BigInteger(5), new BigInteger(50), 2d);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathDecimal : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean").WithExpression(fields[0], "10.0m+5.0m")
                    .WithExpression(fields[1], "10.0m-5.0m")
                    .WithExpression(fields[2], "10.0m*5.0m")
                    .WithExpression(fields[3], "10.0m/5.0m")
                    .WithStatementConsumer(
                        stmt => AssertTypes(
                            stmt,
                            fields,
                            typeof(decimal?),
                            typeof(decimal?),
                            typeof(decimal?),
                            typeof(decimal?)));
                builder.WithAssertion(new SupportBean()).Expect(fields, 15.0m, 5.0m, 50.0m, 2.0m);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathShortAndByteArithmetic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "ShortPrimitive + ShortBoxed")
                    .WithExpression(fields[1], "BytePrimitive + ByteBoxed ")
                    .WithExpression(fields[2], "ShortPrimitive - ShortBoxed")
                    .WithExpression(fields[3], "BytePrimitive - ByteBoxed ")
                    .WithExpression(fields[4], "ShortPrimitive * ShortBoxed")
                    .WithExpression(fields[5], "BytePrimitive * ByteBoxed ")
                    .WithExpression(fields[6], "ShortPrimitive / ShortBoxed")
                    .WithExpression(fields[7], "BytePrimitive / ByteBoxed")
                    .WithExpression(fields[8], "ShortPrimitive + LongPrimitive")
                    .WithExpression(fields[9], "BytePrimitive + LongPrimitive");
                Consumer<EPStatement> typeVerifier = stmt => {
                        foreach (var field in fields) {
                            var expected = typeof(int?);
                            if (field.Equals("c6") || field.Equals("c7")) {
                                expected = typeof(double?);
                            }

                            if (field.Equals("c8") || field.Equals("c9")) {
                                expected = typeof(long?);
                            }

                            Assert.AreEqual(expected, stmt.EventType.GetPropertyType(field), "for field " + field);
                        }
                    }
                    ;
                builder.WithStatementConsumer(typeVerifier);
                var bean = new SupportBean();
                bean.ShortPrimitive = 5;
                bean.ShortBoxed = 6;
                bean.BytePrimitive = 4;
                bean.ByteBoxed = 2;
                bean.LongPrimitive = 10;
                env.SendEventBean(bean);
                builder.WithAssertion(bean).Expect(fields, 11, 6, -1, 2, 30, 8, 5d / 6d, 2d, 15L, 14L);
                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreMathModulo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean").WithExpressions(
                    fields,
                    "LongBoxed % IntBoxed",
                    "IntPrimitive % IntBoxed");
                builder.WithAssertion(MakeBoxedEvent(5, 1L, 1)).Expect(fields, 0L, 0);
                builder.WithAssertion(MakeBoxedEvent(5, 2L, 3)).Expect(fields, 2L, 2);
                builder.Run(env);
                env.UndeployAll();
            }
        }
    }
} // end of namespace