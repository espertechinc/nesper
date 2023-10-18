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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertTypes

// assertTypesAllSame

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumSumOf
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumSumEvents());
            execs.Add(new ExprEnumSumEventsPlus());
            execs.Add(new ExprEnumSumScalar());
            execs.Add(new ExprEnumSumScalarStringValue());
            execs.Add(new ExprEnumSumInvalid());
            execs.Add(new ExprEnumSumArray());
            return execs;
        }

        private class ExprEnumSumArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean");
                builder.WithExpression(fields[0], "{1d, 2d}.sumOf()");
                builder.WithExpression(fields[1], "{BigInteger.valueOf(1), BigInteger.valueOf(2)}.sumOf()");
                builder.WithExpression(fields[2], "{1L, 2L}.sumOf()");
                builder.WithExpression(fields[3], "{1L, 2L, null}.sumOf()");

                builder.WithAssertion(new SupportBean()).Expect(fields, 3d, new BigInteger(3), 3L, 3L);

                builder.Run(env);
            }
        }

        private class ExprEnumSumEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_Container");
                builder.WithExpression(fields[0], "beans.sumOf(x => intBoxed)");
                builder.WithExpression(fields[1], "beans.sumOf(x => doubleBoxed)");
                builder.WithExpression(fields[2], "beans.sumOf(x => longBoxed)");
                builder.WithExpression(fields[3], "beans.sumOf(x => bigDecimal)");
                builder.WithExpression(fields[4], "beans.sumOf(x => bigInteger)");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] {
                            typeof(int?), typeof(double?), typeof(long?), typeof(decimal?), typeof(BigInteger?)
                        }));

                builder.WithAssertion(new SupportBean_Container(null)).Expect(fields, null, null, null, null, null);

                builder.WithAssertion(new SupportBean_Container(EmptyList<SupportBean>.Instance))
                    .Expect(fields, null, null, null, null, null);

                IList<SupportBean> listOne = new List<SupportBean>(Arrays.AsList(Make(2, 3d, 4L, 5, 6)));
                builder.WithAssertion(new SupportBean_Container(listOne))
                    .Expect(fields, 2, 3d, 4L, 5m, BigInteger.Parse("6"));

                IList<SupportBean> listTwo =
                    new List<SupportBean>(Arrays.AsList(Make(2, 3d, 4L, 5, 6), Make(4, 6d, 8L, 10, 12)));
                builder.WithAssertion(new SupportBean_Container(listTwo))
                    .Expect(fields, 2 + 4, 3d + 6d, 4L + 8L, (decimal)(5 + 10), BigInteger.Parse("18"));

                builder.Run(env);
            }
        }

        private class ExprEnumSumEventsPlus : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_Container");
                builder.WithExpression(fields[0], "beans.sumOf(x => intBoxed)");
                builder.WithExpression(fields[1], "beans.sumOf( (x, i) => intBoxed + i*10)");
                builder.WithExpression(fields[2], "beans.sumOf( (x, i, s) => intBoxed + i*10 + s*100)");
                builder.WithExpression(fields[3], "beans.sumOf( (x, i) => case when i = 1 then null else 1 end)");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                builder.WithAssertion(new SupportBean_Container(null)).Expect(fields, null, null, null, null);

                builder.WithAssertion(new SupportBean_Container(EmptyList<SupportBean>.Instance))
                    .Expect(fields, null, null, null, null);

                IList<SupportBean> listOne = new List<SupportBean>(Arrays.AsList(MakeSB("E1", 10)));
                builder.WithAssertion(new SupportBean_Container(listOne)).Expect(fields, 10, 10, 110, 1);

                IList<SupportBean> listTwo = new List<SupportBean>(Arrays.AsList(MakeSB("E1", 10), MakeSB("E2", 11)));
                builder.WithAssertion(new SupportBean_Container(listTwo)).Expect(fields, 21, 31, 431, 1);

                builder.Run(env);
            }

            private SupportBean MakeSB(
                string theString,
                int intBoxed)
            {
                var bean = new SupportBean(theString, intBoxed);
                bean.IntBoxed = intBoxed;
                return bean;
            }
        }

        private class ExprEnumSumScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "intvals.sumOf()");
                builder.WithExpression(fields[1], "bdvals.sumOf()");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(stmt.EventType, fields, new Type[] { typeof(int?), typeof(decimal?) }));

                builder.WithAssertion(SupportCollection.MakeNumeric("1,4,5"))
                    .Expect(fields, 1 + 4 + 5, (decimal)(1 + 4 + 5));

                builder.WithAssertion(SupportCollection.MakeNumeric("3,4")).Expect(fields, 3 + 4, (decimal)(3 + 4));

                builder.WithAssertion(SupportCollection.MakeNumeric("3")).Expect(fields, 3, (decimal)(3));

                builder.WithAssertion(SupportCollection.MakeNumeric("")).Expect(fields, null, null);

                builder.WithAssertion(SupportCollection.MakeNumeric(null)).Expect(fields, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumSumScalarStringValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.sumOf(v => extractNum(v))");
                builder.WithExpression(fields[1], "strvals.sumOf(v => extractDecimal(v))");
                builder.WithExpression(fields[2], "strvals.sumOf( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[3], "strvals.sumOf( (v, i, s) => extractNum(v) + i*10 + s*100)");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] { typeof(int?), typeof(decimal?), typeof(int?), typeof(int?) }));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
                    .Expect(fields, 2 + 1 + 5 + 4, (decimal)(2 + 1 + 5 + 4), 2 + 11 + 25 + 34, 402 + 411 + 425 + 434);

                builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, 1, 1m, 1, 101);

                builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumSumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select beans.sumof() from SupportBean_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'beans.sumof()': Invalid input for built-in enumeration method 'sumof' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '");

                epl = "select strvals.sumOf(v => null) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'strvals.sumOf()': Failed to validate enumeration method 'sumOf', expected a non-null result for expression parameter 0 but received a null-typed expression");
            }
        }

        private static SupportBean Make(
            int? intBoxed,
            double? doubleBoxed,
            long? longBoxed,
            decimal? decimalBoxed,
            int bigInteger)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            bean.BigInteger = new BigInteger(bigInteger);
            return bean;
        }
    }
} // end of namespace