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

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMinMaxBy
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithEvents(execs);
            WithScalar(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxByScalar());
            return execs;
        }

        public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxByEvents());
            return execs;
        }

        internal class ExprEnumMinMaxByEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.minBy(x => P00)");
                builder.WithExpression(fields[1], "Contained.maxBy(x => P00)");
                builder.WithExpression(fields[2], "Contained.minBy(x => P00).Id");
                builder.WithExpression(fields[3], "Contained.maxBy(x => P00).P00");
                builder.WithExpression(fields[4], "Contained.minBy( (x, i) => case when i < 1 then P00 else P00*10 end).P00");
                builder.WithExpression(fields[5], "Contained.maxBy( (x, i) => case when i < 1 then P00 else P00*10 end).P00");
                builder.WithExpression(fields[6], "Contained.minBy( (x, i, s) => case when i < 1 and s > 2 then P00 else P00*10 end).P00");
                builder.WithExpression(fields[7], "Contained.maxBy( (x, i, s) => case when i < 1 and s > 2 then P00 else P00*10 end).P00");

                builder.WithStatementConsumer(
                    stmt => AssertTypes(
                        stmt.EventType,
                        fields,
                        new[] {
                            typeof(SupportBean_ST0),
                            typeof(SupportBean_ST0),
                            typeof(String),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?),
                            typeof(int?)
                        }));

                var beanOne = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2");
                builder.WithAssertion(beanOne).Expect(fields, beanOne.Contained[2], beanOne.Contained[0], "E2", 12, 12, 11, 12, 11);

                var beanTwo = SupportBean_ST0_Container.Make2Value("E1,12");
                builder.WithAssertion(beanTwo).Expect(fields, beanTwo.Contained[0], beanTwo.Contained[0], "E1", 12, 12, 12, 12, 12);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value(null)).Expect(fields, null, null, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value()).Expect(fields, null, null, null, null, null, null, null, null);

                var beanThree = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11");
                builder.WithAssertion(beanThree).Expect(fields, beanThree.Contained[1], beanThree.Contained[0], "E2", 12, 12, 11, 11, 12);

                builder.Run(env);
            }
        }

        internal class ExprEnumMinMaxByScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.minBy(v => extractNum(v))");
                builder.WithExpression(fields[1], "Strvals.maxBy(v => extractNum(v))");
                builder.WithExpression(fields[2], "Strvals.minBy( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[3], "Strvals.maxBy( (v, i) => extractNum(v) + i*10)");
                builder.WithExpression(fields[4], "Strvals.minBy( (v, i, s) => extractNum(v) + (case when s > 2 then i*10 else 0 end))");
                builder.WithExpression(fields[5], "Strvals.maxBy( (v, i, s) => extractNum(v) + (case when s > 2 then i*10 else 0 end))");

                builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(String)));

                builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4")).Expect(fields, "E1", "E5", "E2", "E4", "E2", "E4");

                builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, "E1", "E1", "E1", "E1", "E1", "E1");

                builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("E8,E2")).Expect(fields, "E2", "E8", "E8", "E2", "E2", "E8");

                builder.Run(env);
            }
        }
    }
} // end of namespace