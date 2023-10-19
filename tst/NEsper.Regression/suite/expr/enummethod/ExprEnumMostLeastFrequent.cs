///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertTypesAllSame

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMostLeastFrequent
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEvents(execs);
            WithScalarNoParam(execs);
            WithScalar(execs);
            WithuentInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithuentInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMostLeastFrequentInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMostLeastFreqScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarNoParam(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMostLeastFreqScalarNoParam());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMostLeastFreqEvents());
            return execs;
        }

        private class ExprEnumMostLeastFreqEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "contained.mostFrequent(x => p00)");
                builder.WithExpression(fields[1], "contained.leastFrequent(x => p00)");
                builder.WithExpression(fields[2], "contained.mostFrequent( (x, i) => p00 + i*2)");
                builder.WithExpression(fields[3], "contained.leastFrequent( (x, i) => p00 + i*2)");
                builder.WithExpression(fields[4], "contained.mostFrequent( (x, i, s) => p00 + i*2 + s*4)");
                builder.WithExpression(fields[5], "contained.leastFrequent( (x, i, s) => p00 + i*2 + s*4)");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                var bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2", "E3,12");
                builder.WithAssertion(bean).Expect(fields, 12, 11, 12, 12, 28, 28);

                bean = SupportBean_ST0_Container.Make2Value("E1,12");
                builder.WithAssertion(bean).Expect(fields, 12, 12, 12, 12, 16, 16);

                bean = SupportBean_ST0_Container.Make2Value(
                    "E1,12",
                    "E2,11",
                    "E2,2",
                    "E3,12",
                    "E1,12",
                    "E2,11",
                    "E3,11");
                builder.WithAssertion(bean).Expect(fields, 12, 2, 12, 12, 40, 40);

                bean = SupportBean_ST0_Container.Make2Value(
                    "E2,11",
                    "E1,12",
                    "E2,15",
                    "E3,12",
                    "E1,12",
                    "E2,11",
                    "E3,11");
                builder.WithAssertion(bean).Expect(fields, 11, 15, 11, 11, 39, 39);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Expect(fields, null, null, null, null, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumMostLeastFreqScalarNoParam : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.mostFrequent()");
                builder.WithExpression(fields[1], "strvals.leastFrequent()");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(string)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3"))
                    .Expect(fields, "E3", "E4");

                builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, "E1", "E1");

                builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null);

                builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumMostLeastFreqScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "strvals.mostFrequent(v => extractNum(v))");
                builder.WithExpression(fields[1], "strvals.leastFrequent(v => extractNum(v))");
                builder.WithExpression(fields[2], "strvals.mostFrequent( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[3], "strvals.leastFrequent( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[4], "strvals.mostFrequent( (v, i, s) => extractNum(v) + i*10 + s*100)");
                builder.WithExpression(fields[5], "strvals.leastFrequent( (v, i, s) => extractNum(v) + i*10 + s*100)");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3"))
                    .Expect(fields, 3, 4, 2, 2, 802, 802);

                builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, 1, 1, 1, 1, 101, 101);

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Expect(fields, null, null, null, null, null, null);

                builder.Run(env);
            }
        }

        private class ExprEnumMostLeastFrequentInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select strvals.mostFrequent(v => null) from SupportCollection";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'strvals.mostFrequent()': Null-type is not allowed");
            }
        }
    }
} // end of namespace