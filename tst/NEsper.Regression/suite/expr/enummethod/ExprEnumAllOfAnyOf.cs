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


namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumAllOfAnyOf
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithEvents(execs);
            WithScalar(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAllOfAnyOfInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAllOfAnyOfScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumAllOfAnyOfEvents());
            return execs;
        }

        internal class ExprEnumAllOfAnyOfInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;
                epl = "select Contained.allOf(x => 1) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Contained.allOf()': Failed to validate enumeration method 'allOf', expected a boolean-type result for expression parameter 0 but received int");

                epl = "select Contained.anyOf(x => 1) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Contained.anyOf()': Failed to validate enumeration method 'anyOf', expected a boolean-type result for expression parameter 0 but received int");

                epl = "select Contained.anyOf(x => null) from SupportBean_ST0_Container";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'Contained.anyOf()': Failed to validate enumeration method 'anyOf', expected a non-null result for expression parameter 0 but received a null-typed expression");
            }
        }

        internal class ExprEnumAllOfAnyOfEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.allof(v => P00 = 7)");
                builder.WithExpression(fields[1], "Contained.anyof(v => P00 = 7)");
                builder.WithExpression(fields[2], "Contained.allof((v, i) => P00 = (7 + i*10))");
                builder.WithExpression(fields[3], "Contained.anyof((v, i) => P00 = (7 + i*10))");
                builder.WithExpression(fields[4], "Contained.allof((v, i, s) => P00 = (7 + i*10 + s*100))");
                builder.WithExpression(fields[5], "Contained.anyof((v, i, s) => P00 = (7 + i*10 + s*100))");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(bool?)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,7", "E3,2"))
                    .Expect(fields, false, true, false, false, false, false);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,7", "E2,7", "E3,7"))
                    .Expect(fields, true, true, false, true, false, false);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,0", "E2,0", "E3,0"))
                    .Expect(fields, false, false, false, false, false, false);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Expect(fields, true, false, true, false, true, false);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,327"))
                    .Expect(fields, false, false, false, false, false, true);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,307", "E2,317", "E3,327"))
                    .Expect(fields, false, false, false, false, true, true);

                builder.Run(env);
            }
        }

        internal class ExprEnumAllOfAnyOfScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.allof(v => v='A')");
                builder.WithExpression(fields[1], "Strvals.anyof(v => v='A')");
                builder.WithExpression(fields[2], "Strvals.allof((v, i) => (v='A' and i < 2) or (v='C' and i >= 2))");
                builder.WithExpression(fields[3], "Strvals.anyof((v, i) => (v='A' and i < 2) or (v='C' and i >= 2))");
                builder.WithExpression(
                    fields[4],
                    "Strvals.allof((v, i, s) => (v='A' and i < s - 2) or (v='C' and i >= s - 2))");
                builder.WithExpression(
                    fields[5],
                    "Strvals.anyof((v, i, s) => (v='A' and i < s - 2) or (v='C' and i >= s - 2))");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(bool?)));

                builder.WithAssertion(SupportCollection.MakeString("B,A,C"))
                    .Expect(fields, false, true, false, true, false, true);

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("A,A"))
                    .Expect(fields, true, true, true, true, false, false);

                builder.WithAssertion(SupportCollection.MakeString("B"))
                    .Expect(fields, false, false, false, false, false, false);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Expect(fields, true, false, true, false, true, false);

                builder.WithAssertion(SupportCollection.MakeString("B,B,B"))
                    .Expect(fields, false, false, false, false, false, false);

                builder.WithAssertion(SupportCollection.MakeString("A,A,C,C"))
                    .Expect(fields, false, true, true, true, true, true);

                builder.Run(env);
            }
        }
    }
} // end of namespace