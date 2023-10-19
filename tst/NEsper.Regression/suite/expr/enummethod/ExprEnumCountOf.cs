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
    public class ExprEnumCountOf
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithEvents(execs);
            With(Scalar)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumCountOfScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumCountOfEvents());
            return execs;
        }

        internal class ExprEnumCountOfEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.countof()");
                builder.WithExpression(fields[1], "Contained.countof(x => x.P00 = 9)");
                builder.WithExpression(fields[2], "Contained.countof((x, i) => x.P00 + i = 10)");
                builder.WithExpression(fields[3], "Contained.countof((x, i, s) => x.P00 + i + s = 100)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9"))
                    .Expect(fields, 3, 2, 1, 0);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value()).Expect(fields, 0, 0, 0, 0);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,9")).Expect(fields, 1, 1, 0, 0);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1")).Expect(fields, 1, 0, 0, 0);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,10", "E2,9")).Expect(fields, 2, 1, 2, 0);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,98", "E2,97"))
                    .Expect(fields, 2, 0, 0, 2);

                builder.Run(env);
            }
        }

        internal class ExprEnumCountOfScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.countof()");
                builder.WithExpression(fields[1], "Strvals.countof(x => x = 'E1')");
                builder.WithExpression(fields[2], "Strvals.countof((x, i) => x = 'E1' and i >= 1)");
                builder.WithExpression(fields[3], "Strvals.countof((x, i, s) => x = 'E1' and i >= 1 and s > 2)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                builder.WithAssertion(SupportCollection.MakeString("E1,E2")).Expect(fields, 2, 1, 0, 0);

                builder.WithAssertion(SupportCollection.MakeString("E1,E2,E1,E3")).Expect(fields, 4, 2, 1, 1);

                builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, 1, 1, 0, 0);

                builder.WithAssertion(SupportCollection.MakeString("E1,E1")).Expect(fields, 2, 2, 1, 0);

                builder.Run(env);
            }
        }
    }
} // end of namespace