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

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMinMax
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMinMaxEvents(execs);
            WithMinMaxScalar(execs);
            WithMinMaxScalarWithPredicate(execs);
            WithMinMaxScalarChain(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMaxScalarChain(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxScalarChain());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMaxScalarWithPredicate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxScalarWithPredicate());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMaxScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithMinMaxEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxEvents());
            return execs;
        }

        private class ExprEnumMinMaxScalarChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportEventWithLongArray");
                builder.WithExpression(fields[0], "Coll.max().minus(1 minute) >= Coll.min()");

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
                builder.WithExpression(fields[0], "Strvals.min(v => extractNum(v))");
                builder.WithExpression(fields[1], "Strvals.max(v => extractNum(v))");
                builder.WithExpression(fields[2], "Strvals.min(v => v)");
                builder.WithExpression(fields[3], "Strvals.max(v => v)");
                builder.WithExpression(fields[4], "Strvals.min( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[5], "Strvals.max( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[6], "Strvals.min( (v, i, s) => extractNum(v) + i*10 + s*100)");
                builder.WithExpression(fields[7], "Strvals.max( (v, i, s) => extractNum(v) + i*10 + s*100)");

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
                builder.WithExpression(fields[0], "Contained.min(x => P00)");
                builder.WithExpression(fields[1], "Contained.max(x => P00)");
                builder.WithExpression(fields[2], "Contained.min( (x, i) => P00 + i*10)");
                builder.WithExpression(fields[3], "Contained.max( (x, i) => P00 + i*10)");
                builder.WithExpression(fields[4], "Contained.min( (x, i, s) => P00 + i*10 + s*100)");
                builder.WithExpression(fields[5], "Contained.max( (x, i, s) => P00 + i*10 + s*100)");

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
                builder.WithExpression(fields[0], "Strvals.min()");
                builder.WithExpression(fields[1], "Strvals.max()");

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

                epl = "select Contained.min() from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Contained.min()': Invalid input for built-in enumeration method 'min' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean_ST0).FullName +
                    "'");

                epl = "select Contained.min(x => null) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Contained.min()': Null-type is not allowed");
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
            public MyEvent Myevent { get; }
        }
    }
} // end of namespace