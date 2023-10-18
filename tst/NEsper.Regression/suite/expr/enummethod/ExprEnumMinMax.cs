///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

// INTEGERBOXED
// STRING
using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertTypes

// assertTypesAllSame

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMinMax
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxEvents());
            execs.Add(new ExprEnumMinMaxScalar());
            execs.Add(new ExprEnumMinMaxScalarWithPredicate());
            execs.Add(new ExprEnumMinMaxScalarChain());
            execs.Add(new ExprEnumInvalid());
            return execs;
        }

        private class ExprEnumMinMaxScalarChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportEventWithLongArray");
                builder.WithExpression(fields[0], "coll.max().minus(1 minute) >= coll.min()");

                builder.WithAssertion(
                        new SupportEventWithLongArray("E1", new long[] { 150000, 140000, 200000, 190000 }))
                    .Expect(fields, true);

                builder.WithAssertion(
                        new SupportEventWithLongArray("E2", new long[] { 150000, 139999, 200000, 190000 }))
                    .Expect(fields, true);

                builder.Run(env);
            }
        }

        private class ExprEnumMinMaxScalarWithPredicate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.min(v => extractNum(v))");
                builder.WithExpression(fields[1], "strvals.max(v => extractNum(v))");
                builder.WithExpression(fields[2], "strvals.min(v => v)");
                builder.WithExpression(fields[3], "strvals.max(v => v)");
                builder.WithExpression(fields[4], "strvals.min( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[5], "strvals.max( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[6], "strvals.min( (v, i, s) => extractNum(v) + i*10 + s*100)");
                builder.WithExpression(fields[7], "strvals.max( (v, i, s) => extractNum(v) + i*10 + s*100)");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new Type[] {
                            typeof(int?), typeof(int?), typeof(string), typeof(string),
                            typeof(int?), typeof(int?), typeof(int?), typeof(int?)
                        }));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
                    .Expect(fields, 1, 5, "E1", "E5", 2, 34, 402, 434);

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Expect(fields, 1, 1, "E1", "E1", 1, 1, 101, 101);

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Expect(fields, null, null, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Expect(fields, null, null, null, null, null, null, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumMinMaxEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "contained.min(x => p00)");
                builder.WithExpression(fields[1], "contained.max(x => p00)");
                builder.WithExpression(fields[2], "contained.min( (x, i) => p00 + i*10)");
                builder.WithExpression(fields[3], "contained.max( (x, i) => p00 + i*10)");
                builder.WithExpression(fields[4], "contained.min( (x, i, s) => p00 + i*10 + s*100)");
                builder.WithExpression(fields[5], "contained.max( (x, i, s) => p00 + i*10 + s*100)");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"))
                    .Expect(fields, 2, 12, 12, 22, 312, 322);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12", "E2,0", "E2,2"))
                    .Expect(fields, 0, 12, 10, 22, 310, 322);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Expect(fields, null, null, null, null, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumMinMaxScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.min()");
                builder.WithExpression(fields[1], "strvals.max()");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(string)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4")).Expect(fields, "E1", "E5");

                builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, "E1", "E1");

                builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null);

                builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select contained.min() from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.min()': Invalid input for built-in enumeration method 'min' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean_ST0).FullName +
                    "'");

                epl = "select contained.min(x => null) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'contained.min()': Null-type is not allowed");
            }
        }

        public class MyService
        {
            public static int ExtractNum(string arg)
            {
                return int.Parse(arg.Substring(1));
            }

            public static decimal? ExtractDecimal(string arg)
            {
                return decimal.Parse(arg.Substring(1));
            }
        }

        public class MyEvent
        {
            private MyEvent myevent;

            public MyEvent GetMyevent()
            {
                return myevent;
            }
        }
    }
} // end of namespace