///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil; // assertST0Id

// assertValuesArrayScalar

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumOrderBy
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEvents(execs);
            WithEventsPlus(execs);
            WithScalar(execs);
            WithScalarWithParam(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumOrderByInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarWithParam(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumOrderByScalarWithParam());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumOrderByScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsPlus(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumOrderByEventsPlus());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumOrderByEvents());
            return execs;
        }

        private class ExprEnumOrderByEventsPlus : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(
                    fields[0],
                    "contained.orderBy( (x, i) => case when i <= 2 then p00 else i-10 end)");
                builder.WithExpression(
                    fields[1],
                    "contained.orderByDesc( (x, i) => case when i <= 2 then p00 else i-10 end)");
                builder.WithExpression(
                    fields[2],
                    "contained.orderBy( (x, i, s) => case when s <= 2 then p00 else i-10 end)");
                builder.WithExpression(
                    fields[3],
                    "contained.orderByDesc( (x, i, s) => case when s <= 2 then p00 else i-10 end)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<SupportBean_ST0>)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"))
                    .Verify("c0", val => AssertST0Id(val, "E1,E2"))
                    .Verify("c1", val => AssertST0Id(val, "E2,E1"))
                    .Verify("c2", val => AssertST0Id(val, "E1,E2"))
                    .Verify("c3", val => AssertST0Id(val, "E2,E1"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3", "E4,4"))
                    .Verify("c0", val => AssertST0Id(val, "E4,E1,E2,E3"))
                    .Verify("c1", val => AssertST0Id(val, "E3,E2,E1,E4"))
                    .Verify("c2", val => AssertST0Id(val, "E1,E2,E3,E4"))
                    .Verify("c3", val => AssertST0Id(val, "E4,E3,E2,E1"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Verify("c0", val => AssertST0Id(val, ""))
                    .Verify("c1", val => AssertST0Id(val, ""))
                    .Verify("c2", val => AssertST0Id(val, ""))
                    .Verify("c3", val => AssertST0Id(val, ""));

                builder.Run(env);
            }
        }

        private class ExprEnumOrderByEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "contained.orderBy(x => p00)");
                builder.WithExpression(fields[1], "contained.orderBy(x => 10 - p00)");
                builder.WithExpression(fields[2], "contained.orderBy(x => 0)");
                builder.WithExpression(fields[3], "contained.orderByDesc(x => p00)");
                builder.WithExpression(fields[4], "contained.orderByDesc(x => 10 - p00)");
                builder.WithExpression(fields[5], "contained.orderByDesc(x => 0)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<SupportBean_ST0>)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"))
                    .Verify("c0", val => AssertST0Id(val, "E1,E2"))
                    .Verify("c1", val => AssertST0Id(val, "E2,E1"))
                    .Verify("c2", val => AssertST0Id(val, "E1,E2"))
                    .Verify("c3", val => AssertST0Id(val, "E2,E1"))
                    .Verify("c4", val => AssertST0Id(val, "E1,E2"))
                    .Verify("c5", val => AssertST0Id(val, "E1,E2"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"))
                    .Verify("c0", val => AssertST0Id(val, "E3,E4,E2,E1"))
                    .Verify("c1", val => AssertST0Id(val, "E2,E1,E3,E4"))
                    .Verify("c2", val => AssertST0Id(val, "E3,E2,E4,E1"))
                    .Verify("c3", val => AssertST0Id(val, "E2,E1,E3,E4"))
                    .Verify("c4", val => AssertST0Id(val, "E3,E4,E2,E1"))
                    .Verify("c5", val => AssertST0Id(val, "E3,E2,E4,E1"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Verify("c0", val => AssertST0Id(val, ""))
                    .Verify("c1", val => AssertST0Id(val, ""))
                    .Verify("c2", val => AssertST0Id(val, ""))
                    .Verify("c3", val => AssertST0Id(val, ""))
                    .Verify("c4", val => AssertST0Id(val, ""))
                    .Verify("c5", val => AssertST0Id(val, ""));

                builder.Run(env);
            }
        }

        private class ExprEnumOrderByScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.orderBy()");
                builder.WithExpression(fields[1], "strvals.orderByDesc()");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<string>)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E2", "E4", "E5"))
                    .Verify("c1", val => AssertValuesArrayScalar(val, "E5", "E4", "E2", "E1"));

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(builder, fields);
                builder.Run(env);
            }
        }

        private class ExprEnumOrderByScalarWithParam : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.orderBy(v => extractNum(v))");
                builder.WithExpression(fields[1], "strvals.orderByDesc(v => extractNum(v))");
                builder.WithExpression(
                    fields[2],
                    "strvals.orderBy( (v, i) => case when i <= 2 then extractNum(v) else i-10 end)");
                builder.WithExpression(
                    fields[3],
                    "strvals.orderByDesc( (v, i) => case when i <= 2 then extractNum(v) else i-10 end)");
                builder.WithExpression(
                    fields[4],
                    "strvals.orderBy( (v, i, s) => case when s <= 2 then extractNum(v) else i-10 end)");
                builder.WithExpression(
                    fields[5],
                    "strvals.orderByDesc( (v, i, s) => case when s <= 2 then extractNum(v) else i-10 end)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(
                        stmt.EventType,
                        fields,
                        typeof(ICollection<string>)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E2", "E4", "E5"))
                    .Verify("c1", val => AssertValuesArrayScalar(val, "E5", "E4", "E2", "E1"))
                    .Verify("c2", val => AssertValuesArrayScalar(val, "E4", "E1", "E2", "E5"))
                    .Verify("c3", val => AssertValuesArrayScalar(val, "E5", "E2", "E1", "E4"))
                    .Verify("c4", val => AssertValuesArrayScalar(val, "E2", "E1", "E5", "E4"))
                    .Verify("c5", val => AssertValuesArrayScalar(val, "E4", "E5", "E1", "E2"));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1"))
                    .Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E2"))
                    .Verify("c1", val => AssertValuesArrayScalar(val, "E2", "E1"))
                    .Verify("c2", val => AssertValuesArrayScalar(val, "E1", "E2"))
                    .Verify("c3", val => AssertValuesArrayScalar(val, "E2", "E1"))
                    .Verify("c4", val => AssertValuesArrayScalar(val, "E1", "E2"))
                    .Verify("c5", val => AssertValuesArrayScalar(val, "E2", "E1"));

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(builder, fields);

                builder.Run(env);
            }
        }

        private class ExprEnumOrderByInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select contained.orderBy() from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.orderBy()': Invalid input for built-in enumeration method 'orderBy' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean_ST0).FullName +
                    "'");

                epl = "select strvals.orderBy(v => null) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'strvals.orderBy()': Null-type is not allowed");
            }
        }
    }
} // end of namespace