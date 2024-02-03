///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumFirstLastOf
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithScalar(execs);
            WithEventProperty(execs);
            WithEvent(execs);
            With(EventWithPredicate)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithEventWithPredicate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumFirstLastEventWithPredicate());
            return execs;
        }

        public static IList<RegressionExecution> WithEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumFirstLastEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithEventProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumFirstLastEventProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprEnumFirstLastScalar());
            return execs;
        }

        internal class ExprEnumFirstLastScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                var builder = new SupportEvalBuilder("SupportCollection");
                builder.WithExpression(fields[0], "Strvals.firstOf()");
                builder.WithExpression(fields[1], "Strvals.lastOf()");
                builder.WithExpression(fields[2], "Strvals.firstOf(x => x like '%1%')");
                builder.WithExpression(fields[3], "Strvals.lastOf(x => x like '%1%')");
                builder.WithExpression(fields[4], "Strvals.firstOf((x, i) => x like '%1%' and i >= 1)");
                builder.WithExpression(fields[5], "Strvals.lastOf((x, i) => x like '%1%' and i >= 1)");
                builder.WithExpression(fields[6], "Strvals.firstOf((x, i, s) => x like '%1%' and i >= 1 and s > 2)");
                builder.WithExpression(fields[7], "Strvals.lastOf((x, i, s) => x like '%1%' and i >= 1 and s > 2)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(string)));

                builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3"))
                    .Expect(fields, "E1", "E3", "E1", "E1", null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("E1"))
                    .Expect(fields, "E1", "E1", "E1", "E1", null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("E2,E3,E4"))
                    .Expect(fields, "E2", "E4", null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString(""))
                    .Expect(fields, null, null, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString(null))
                    .Expect(fields, null, null, null, null, null, null, null, null);

                builder.WithAssertion(SupportCollection.MakeString("E5,E2,E3,A1,B1"))
                    .Expect(fields, "E5", "B1", "A1", "B1", "A1", "B1", "A1", "B1");

                builder.WithAssertion(SupportCollection.MakeString("A1,B1,E5,E2,E3"))
                    .Expect(fields, "A1", "E3", "A1", "B1", "B1", "B1", "B1", "B1");

                builder.WithAssertion(SupportCollection.MakeString("A1,B1"))
                    .Expect(fields, "A1", "B1", "A1", "B1", "B1", "B1", null, null);

                builder.Run(env);
            }
        }

        internal class ExprEnumFirstLastEventProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.firstOf().P00");
                builder.WithExpression(fields[1], "Contained.lastOf().P00");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,3"))
                    .Expect(fields, 1, 3);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1")).Expect(fields, 1, 1);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull()).Expect(fields, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value()).Expect(fields, null, null);

                builder.Run(env);
            }
        }

        internal class ExprEnumFirstLastEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.firstOf()");
                builder.WithExpression(fields[1], "Contained.lastOf()");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(SupportBean_ST0)));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E3,9", "E2,9"))
                    .Verify("c0", value => AssertId(value, "E1"))
                    .Verify("c1", value => AssertId(value, "E2"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E2,2"))
                    .Verify("c0", value => AssertId(value, "E2"))
                    .Verify("c1", value => AssertId(value, "E2"));

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull()).Expect(fields, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value()).Expect(fields, null, null);

                builder.Run(env);
            }
        }

        internal class ExprEnumFirstLastEventWithPredicate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
                builder.WithExpression(fields[0], "Contained.firstOf(x => P00 = 9)");
                builder.WithExpression(fields[1], "Contained.lastOf(x => P00 = 9)");
                builder.WithExpression(fields[2], "Contained.firstOf( (x, i) => P00 = 9 and i >= 1)");
                builder.WithExpression(fields[3], "Contained.lastOf( (x, i) => P00 = 9 and i >= 1)");
                builder.WithExpression(fields[4], "Contained.firstOf( (x, i, s) => P00 = 9 and i >= 1 and s > 2)");
                builder.WithExpression(fields[5], "Contained.lastOf((x, i, s) => P00 = 9 and i >= 1 and s > 2)");

                builder.WithStatementConsumer(
                    stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(SupportBean_ST0)));

                var beanOne = SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9");
                builder.WithAssertion(beanOne)
                    .Expect(
                        fields,
                        beanOne.Contained[1],
                        beanOne.Contained[2],
                        beanOne.Contained[1],
                        beanOne.Contained[2],
                        beanOne.Contained[1],
                        beanOne.Contained[2]);

                builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
                    .Expect(fields, null, null, null, null, null, null);

                builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E2,1"))
                    .Expect(fields, null, null, null, null, null, null);

                var beanTwo = SupportBean_ST0_Container.Make2Value("E1,1", "E2,9");
                builder.WithAssertion(beanTwo)
                    .Expect(
                        fields,
                        beanTwo.Contained[1],
                        beanTwo.Contained[1],
                        beanTwo.Contained[1],
                        beanTwo.Contained[1],
                        null,
                        null);

                var beanThree = SupportBean_ST0_Container.Make2Value("E2,9", "E1,1");
                builder.WithAssertion(beanThree)
                    .Expect(
                        fields,
                        beanThree.Contained[0],
                        beanThree.Contained[0],
                        null,
                        null,
                        null,
                        null);

                builder.Run(env);
            }
        }

        private static void AssertId(
            object value,
            string id)
        {
            var result = (SupportBean_ST0)value;
            ClassicAssert.AreEqual(id, result.Id);
        }
    }
} // end of namespace