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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

// INTEGERBOXED

// STRING

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumAggregate
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEvents(execs);
            WithScalar(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAggregateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAggregateScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAggregateEvents());
            return execs;
        }

        private class ExprEnumAggregateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid incompatible params
                epl = "select contained.aggregate(0, (result, item) => result || ',') from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.aggregate(0,)': Failed to validate enumeration method 'aggregate' parameter 1: Failed to validate declared expression body expression 'result||\",\"': Implicit conversion from datatype 'Integer' to string is not allowed");

                // null-init-value for aggregate
                epl = "select contained.aggregate(null, (result, item) => result) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.aggregate(null,)': Initialization value is null-typed");
            }
        }

        private class ExprEnumAggregateEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "contained.aggregate(0, (result, item) => result + item.p00)");
                builder.WithExpression(
                    fields[1],
                    "contained.aggregate('', (result, item) => result || ', ' || item.id)");
                builder.WithExpression(
                    fields[2],
                    "contained.aggregate('', (result, item) => result || (case when result='' then '' else ',' end) || item.id)");
                builder.WithExpression(
                    fields[3],
                    "contained.aggregate(0, (result, item, i) => result + item.p00 + i*10)");
                builder.WithExpression(
                    fields[4],
                    "contained.aggregate(0, (result, item, i, s) => result + item.p00 + i*10 + s*100)");
                builder.WithExpression(fields[5], "contained.aggregate(0, (result, item) => null)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] {
                            typeof(int?),
                            typeof(string),
                            typeof(string),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?)
                        }));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"))
                    .Expect(fields, 25, ", E1, E2, E2", "E1,E2,E2", 12 + 21 + 22, 312 + 321 + 322, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Expect(fields, 0, "", "", 0, 0, 0);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12"))
                    .Expect(fields, 12, ", E1", "E1", 12, 112, null);

                builder.Run(env);
            }
        }

        private class ExprEnumAggregateScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.aggregate('', (result, item) => result || '+' || item)");
                builder.WithExpression(
                    fields[1],
                    "strvals.aggregate('', (result, item, i) => result || '+' || item || '_' || Integer.toString(i))");
                builder.WithExpression(
                    fields[2],
                    "strvals.aggregate('', (result, item, i, s) => result || '+' || item || '_' || Integer.toString(i) || '_' || Integer.toString(s))");
                builder.WithExpression(fields[3], "strvals.aggregate('', (result, item, i, s) => null)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(string)));

                builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3"))
                    .Expect(fields, "+E1+E2+E3", "+E1_0+E2_1+E3_2", "+E1_0_3+E2_1_3+E3_2_3", null);

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Expect(fields, "+E1", "+E1_0", "+E1_0_1", null);

                builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, "", "", "", "");

                builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null, null);

                builder.Run(env);
            }
        }
    }
} // end of namespace