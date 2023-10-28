///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

// BIGDECIMAL
// DOUBLEBOXED
using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertTypes

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumAverage
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAverageEvents(execs);
            WithAverageScalar(execs);
            WithAverageScalarMore(execs);
            WithAverageInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAverageInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAverageInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithAverageScalarMore(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAverageScalarMore());
            return execs;
        }

        public static IList<RegressionExecution> WithAverageScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAverageScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithAverageEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAverageEvents());
            return execs;
        }

        private class ExprEnumAverageEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_Container");
                builder.WithExpression(fields[0], "beans.Average(x => IntBoxed)");
                builder.WithExpression(fields[1], "beans.Average(x => DoubleBoxed)");
                builder.WithExpression(fields[2], "beans.Average(x => LongBoxed)");
                builder.WithExpression(fields[3], "beans.Average(x => bigDecimal)");
                builder.WithExpression(fields[4], "beans.Average( (x, i) => IntBoxed + i*10)");
                builder.WithExpression(fields[5], "beans.Average( (x, i) => bigDecimal + i*10)");
                builder.WithExpression(fields[6], "beans.Average( (x, i, s) => IntBoxed + i*10 + s*100)");
                builder.WithExpression(fields[7], "beans.Average( (x, i, s) => bigDecimal + i*10 + s*100)");
                builder.WithExpression(fields[8], "beans.Average( (x, i, s) => case when i = 1 then null else 2 end)");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] {
                            typeof(double?), typeof(double?), typeof(double?), typeof(decimal?),
                            typeof(double?), typeof(decimal?), typeof(double?), typeof(decimal?),
                            typeof(double?)
                        }));

                builder.WithAssertion(new SupportBean_Container(null))
                    .Expect(fields, null, null, null, null, null, null, null, null, null);

                builder.WithAssertion(new SupportBean_Container(EmptyList<SupportBean>.Instance))
                    .Expect(fields, null, null, null, null, null, null, null, null, null);

                IList<SupportBean> listOne = new List<SupportBean>(Arrays.AsList(Make(2, 3d, 4L, 5)));
                builder.WithAssertion(new SupportBean_Container(listOne))
                    .Expect(fields, 2d, 3d, 4d, 5.0m, 2d, 5.0m, 102d, 105.0m, 2d);

                IList<SupportBean> listTwo =
                    new List<SupportBean>(Arrays.AsList(Make(2, 3d, 4L, 5), Make(4, 6d, 8L, 10)));
                builder.WithAssertion(new SupportBean_Container(listTwo))
                    .Expect(
                        fields,
                        (2 + 4) / 2d,
                        (3d + 6d) / 2d,
                        (4L + 8L) / 2d,
                        (5 + 10) / 2m,
                        (2 + 14) / 2d,
                        (5 + 20) / 2m,
                        (202 + 214) / 2d,
                        (205 + 220) / 2m,
                        2d);

                builder.Run(env);
            }
        }

        private class ExprEnumAverageScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "intvals.Average()");
                builder.WithExpression(fields[1], "bdvals.Average()");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(stmt.EventType, fields, new Type[] { typeof(double?), typeof(decimal?) }));

                builder.WithAssertion(SupportCollection.MakeNumeric("1,2,3")).Expect(fields, 2d, 2m);

                builder.WithAssertion(SupportCollection.MakeNumeric("1,null,3")).Expect(fields, 2d, 2m);

                builder.WithAssertion(SupportCollection.MakeNumeric("4")).Expect(fields, 4d, 4m);

                builder.Run(env);
            }
        }

        private class ExprEnumAverageScalarMore : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.Average(v => extractNum(v))");
                builder.WithExpression(fields[1], "Strvals.Average(v => extractDecimal(v))");
                builder.WithExpression(fields[2], "Strvals.Average( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[3], "Strvals.Average( (v, i) => extractDecimal(v) + i*10)");
                builder.WithExpression(fields[4], "Strvals.Average( (v, i, s) => extractNum(v) + i*10 + s*100)");
                builder.WithExpression(fields[5], "Strvals.Average( (v, i, s) => extractDecimal(v) + i*10 + s*100)");
                builder.WithExpression(
                    fields[6],
                    "Strvals.Average( (v, i, s) => case when i = 1 then null else 2 end)");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] {
                            typeof(double?), typeof(decimal?), typeof(double?), typeof(decimal?),
                            typeof(double?), typeof(decimal?), typeof(double?)
                        }));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
                    .Expect(
                        fields,
                        (2 + 1 + 5 + 4) / 4d,
                        ((2 + 1 + 5 + 4) / 4m),
                        (2 + 11 + 25 + 34) / 4d,
                        ((2 + 11 + 25 + 34) / 4m),
                        (402 + 411 + 425 + 434) / 4d,
                        ((402 + 411 + 425 + 434) / 4m),
                        2d);

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Expect(fields, 1d, 1m, 1d, 1m, 101d, 101m, 2d);

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Expect(fields, null, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Expect(fields, null, null, null, null, null, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumAverageInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select Strvals.Average() from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Strvals.Average()': Invalid input for built-in enumeration method 'Average' and 0-parameter footprint, expecting collection of numeric values as input, received java.util.Collection<String> [select Strvals.Average() from SupportCollection]");

                epl = "select beans.Average() from SupportBean_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'beans.Average()': Invalid input for built-in enumeration method 'Average' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean).FullName +
                    "'");

                epl = "select Strvals.Average(v => null) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Strvals.Average()': Failed to validate enumeration method 'Average', expected a non-null result for expression parameter 0 but received a null-typed expression");
            }
        }

        private static SupportBean Make(
            int? intBoxed,
            double? doubleBoxed,
            long? longBoxed,
            decimal? decimalBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            return bean;
        }
    }
} // end of namespace