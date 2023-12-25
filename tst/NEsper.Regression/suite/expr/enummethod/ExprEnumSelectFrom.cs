///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertTypes

// assertTypesAllSame

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumSelectFrom
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEventsPlain(execs);
            WithEventsWIndexWSize(execs);
            WithEventsWithNew(execs);
            WithScalarPlain(execs);
            WithScalarWIndexWSize(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithScalarWIndexWSize(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSelectFromScalarWIndexWSize());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarPlain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSelectFromScalarPlain());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsWithNew(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSelectFromEventsWithNew());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsWIndexWSize(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSelectFromEventsWIndexWSize());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsPlain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumSelectFromEventsPlain());
            return execs;
        }

        private class ExprEnumSelectFromScalarWIndexWSize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.selectFrom( (v, i) => v || '_' || Convert.ToString(i))");
                builder.WithExpression(
                    fields[1],
                    "Strvals.selectFrom( (v, i, s) => v || '_' || Convert.ToString(i) || '_' || Convert.ToString(s))");

                builder.WithStatementConsumer(
                    stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<string>)));

                builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3"))
                    .Verify(
                        fields[0],
                        value => LambdaAssertionUtil.AssertValuesArrayScalar(value, "E1_0", "E2_1", "E3_2"))
                    .Verify(
                        fields[1],
                        value => LambdaAssertionUtil.AssertValuesArrayScalar(value, "E1_0_3", "E2_1_3", "E3_2_3"));

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Verify(fields[0], value => LambdaAssertionUtil.AssertValuesArrayScalar(value))
                    .Verify(fields[1], value => LambdaAssertionUtil.AssertValuesArrayScalar(value));

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Verify(fields[0], value => LambdaAssertionUtil.AssertValuesArrayScalar(value, "E1_0"))
                    .Verify(fields[1], value => LambdaAssertionUtil.AssertValuesArrayScalar(value, "E1_0_1"));

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Verify(fields[0], Assert.IsNull)
                    .Verify(fields[1], Assert.IsNull);

                builder.Run(env);
            }
        }

        private class ExprEnumSelectFromEventsWIndexWSize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.selectFrom( (v, i) => new {v0=v.Id,v1=i})");
                builder.WithExpression(fields[1], "Contained.selectFrom( (v, i, s) => new {v0=v.Id,v1=i + 100*s})");

                builder.WithStatementConsumer(
                    stmt => AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<IDictionary<string, object>>)));

                builder.WithAssertion(SupportBean_ST0_Container.Make3Value("E1,12,0", "E2,11,0", "E3,2,0"))
                    .Verify(
                        fields[0],
                        value => AssertRows(
                            value,
                            new object[][]
                                { new object[] { "E1", 0 }, new object[] { "E2", 1 }, new object[] { "E3", 2 } }))
                    .Verify(
                        fields[1],
                        value => AssertRows(
                            value,
                            new object[][] {
                                new object[] { "E1", 300 }, new object[] { "E2", 301 }, new object[] { "E3", 302 }
                            }));

                builder.WithAssertion(SupportBean_ST0_Container.Make3Value("E4,0,1"))
                    .Verify(fields[0], value => AssertRows(value, new object[][] { new object[] { "E4", 0 } }))
                    .Verify(fields[1], value => AssertRows(value, new object[][] { new object[] { "E4", 100 } }));

                builder.WithAssertion(SupportBean_ST0_Container.Make3ValueNull())
                    .Verify(fields[0], value => AssertRows(value, null))
                    .Verify(fields[1], value => AssertRows(value, null));

                builder.WithAssertion(SupportBean_ST0_Container.Make3Value())
                    .Verify(fields[0], value => AssertRows(value, Array.Empty<object[]>()))
                    .Verify(fields[1], value => AssertRows(value, Array.Empty<object[]>()));

                builder.Run(env);
            }

            private void AssertRows(
                object value,
                object[][] expected)
            {
                EPAssertionUtil.AssertPropsPerRow(ToMapArray(value), "v0,v1".SplitCsv(), expected);
            }
        }

        private class ExprEnumSelectFromEventsWithNew : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var field = "c0";
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(field, "Contained.selectFrom(x => new {c0 = Id||'x', c1 = Key0||'y'})");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(stmt.EventType, field, typeof(ICollection<IDictionary<string, object>>)));

                builder.WithAssertion(SupportBean_ST0_Container.Make3Value("E1,12,0", "E2,11,0", "E3,2,0"))
                    .Verify(
                        field,
                        value => AssertRows(
                            value,
                            new object[][] {
                                new object[] { "E1x", "12y" }, new object[] { "E2x", "11y" },
                                new object[] { "E3x", "2y" }
                            }));

                builder.WithAssertion(SupportBean_ST0_Container.Make3Value("E4,0,1"))
                    .Verify(field, value => AssertRows(value, new object[][] { new object[] { "E4x", "0y" } }));

                builder.WithAssertion(SupportBean_ST0_Container.Make3ValueNull())
                    .Verify(field, value => AssertRows(value, null));

                builder.WithAssertion(SupportBean_ST0_Container.Make3Value())
                    .Verify(field, value => AssertRows(value, Array.Empty<object[]>()));

                builder.Run(env);
            }

            private void AssertRows(
                object value,
                object[][] expected)
            {
                EPAssertionUtil.AssertPropsPerRow(ToMapArray(value), "c0,c1".SplitCsv(), expected);
            }
        }

        private class ExprEnumSelectFromEventsPlain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.selectFrom(x => Id)");
                builder.WithExpression(fields[1], "Contained.selectFrom(x => null)");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] {
                            typeof(ICollection<string>),
                            typeof(ICollection<object>)
                        }));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E3,2"))
                    .Verify(fields[0], value => LambdaAssertionUtil.AssertValuesArrayScalar(value, "E1", "E2", "E3"))
                    .Verify(fields[1], value => LambdaAssertionUtil.AssertValuesArrayScalar(value));

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Verify(fields[0], Assert.IsNull)
                    .Verify(fields[1], Assert.IsNull);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Verify(fields[0], value => LambdaAssertionUtil.AssertValuesArrayScalar(value))
                    .Verify(fields[1], value => LambdaAssertionUtil.AssertValuesArrayScalar(value));

                builder.Run(env);
            }
        }

        private class ExprEnumSelectFromScalarPlain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var field = "c0";
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(field, "Strvals.selectFrom(v => extractNum(v))");

                builder.WithStatementConsumer(stmt => AssertTypes(stmt.EventType, field, typeof(ICollection<int?>)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
                    .Verify(field, value => LambdaAssertionUtil.AssertValuesArrayScalar(value, 2, 1, 5, 4));

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Verify(field, value => LambdaAssertionUtil.AssertValuesArrayScalar(value, 1));

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Verify(field, Assert.IsNull);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Verify(field, value => LambdaAssertionUtil.AssertValuesArrayScalar(value));

                builder.Run(env);
            }
        }

        private static IDictionary<string, object>[] ToMapArray(object result)
        {
            return result.UnwrapIntoArray<IDictionary<string, object>>(true);
        }
    }
} // end of namespace