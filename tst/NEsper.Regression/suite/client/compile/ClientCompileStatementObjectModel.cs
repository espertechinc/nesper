///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileStatementObjectModel
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithCreateFromOM(execs);
            WithCreateFromOMComplete(execs);
            WithEPLtoOMtoStmt(execs);
            WithPrecedenceExpressions(execs);
            With(PrecedencePatterns)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithPrecedencePatterns(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSODAPrecedencePatterns());
            return execs;
        }

        public static IList<RegressionExecution> WithPrecedenceExpressions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSODAPrecedenceExpressions());
            return execs;
        }

        public static IList<RegressionExecution> WithEPLtoOMtoStmt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSODAEPLtoOMtoStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateFromOMComplete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSODACreateFromOMComplete());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateFromOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSODACreateFromOM());
            return execs;
        }

        // This is a simple EPL only.
        // Each OM/SODA Api is tested in it's respective unit test (i.e. TestInsertInto), including toEPL()
        //
        private class ClientCompileSODACreateFromOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CopyMayFail(model);

                env.CompileDeploy(model).AddListener("s0");

                var theEvent = new SupportBean();
                env.SendEventBean(theEvent);
                env.AssertEventNew("s0", @event => Assert.AreEqual(theEvent, @event.Underlying));

                env.UndeployAll();
            }
        }

        private class ClientCompileSODACreateFromOMComplete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.WithInsertInto(InsertIntoClause.Create("ReadyStreamAvg", "line", "avgAge"));
                model.WithSelectClause(SelectClause.Create().Add("line").Add(Expressions.Avg("age"), "avgAge"));
                var filter = Filter.Create(typeof(SupportBean).FullName, Expressions.In("line", 1, 8, 10));
                model.WithFromClause(
                    FromClause.Create(FilterStream.Create(filter, "RS").AddView("time", Expressions.Constant(10))));
                model.WithWhereClause(Expressions.IsNotNull("waverId"));
                model.WithGroupByClause(GroupByClause.Create("line"));
                model.WithHavingClause(Expressions.Lt(Expressions.Avg("age"), Expressions.Constant(0)));
                model.WithOutputLimitClause(
                    OutputLimitClause.Create(Expressions.TimePeriod(null, null, null, 10, null)));
                model.WithOrderByClause(OrderByClause.Create("line"));

                Assert.AreEqual(
                    "insert into ReadyStreamAvg(line, avgAge) select line, avg(age) as avgAge from " +
                    typeof(SupportBean).CleanName() +
                    "(line in (1,8,10))#time(10) as RS where waverId is not null group by line having avg(age)<0 output every 10.0d seconds order by line",
                    model.ToEPL());
                env.CopyMayFail(model);
            }
        }

        private class ClientCompileSODAEPLtoOMtoStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select * from SupportBean";
                var model = env.EplToModel(stmtText);
                env.CopyMayFail(model);
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                env.CompileDeploy(model).AddListener("s0");

                object theEvent = new SupportBean();
                env.SendEventBean(theEvent);

                env.AssertEventNew("s0", @event => Assert.AreEqual(theEvent, @event.Underlying));
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        "@name('s0') " + stmtText,
                        statement.GetProperty(StatementProperty.EPL)));

                env.UndeployAll();
            }
        }

        private class ClientCompileSODAPrecedenceExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[][] testdata = {
                    new[] { "1+2*3", null, "ArithmaticExpression" },
                    new[] { "1+(2*3)", "1+2*3", "ArithmaticExpression" },
                    new[] { "2-2/3-4", null, "ArithmaticExpression" },
                    new[] { "2-(2/3)-4", "2-2/3-4", "ArithmaticExpression" },
                    new[] { "1+2 in (4,5)", null, "InExpression" },
                    new[] { "(1+2) in (4,5)", "1+2 in (4,5)", "InExpression" },
                    new[] { "true and false or true", null, "Disjunction" },
                    new[] { "(true and false) or true", "true and false or true", "Disjunction" },
                    new[] { "true and (false or true)", null, "Conjunction" },
                    new[] { "true and (((false or true)))", "true and (false or true)", "Conjunction" },
                    new[] { "true and (((false or true)))", "true and (false or true)", "Conjunction" },
                    new[] { "false or false and true or false", null, "Disjunction" },
                    new[] { "false or (false and true) or false", "false or false and true or false", "Disjunction" },
                    new[] { "\"a\"||\"b\"=\"ab\"", null, "RelationalOpExpression" },
                    new[] { "(\"a\"||\"b\")=\"ab\"", "\"a\"||\"b\"=\"ab\"", "RelationalOpExpression" },
                };

                foreach (var aTestdata in testdata) {
                    var epl = "select * from System.Object where " + aTestdata[0];
                    var expected = aTestdata[1];
                    var expressionLowestPrecedenceClass = aTestdata[2];

                    var modelBefore = env.EplToModel(epl);
                    var eplAfter = modelBefore.ToEPL();

                    if (expected == null) {
                        Assert.AreEqual(epl, eplAfter);
                    }
                    else {
                        var expectedEPL = "select * from System.Object where " + expected;
                        Assert.AreEqual(expectedEPL, eplAfter);
                    }

                    // get where clause root expression of both models
                    var modelAfter = env.EplToModel(eplAfter);
                    Assert.AreEqual(modelAfter.WhereClause.GetType(), modelBefore.WhereClause.GetType());
                    Assert.AreEqual(expressionLowestPrecedenceClass, modelAfter.WhereClause.GetType().Name);
                }
            }
        }

        private class ClientCompileSODAPrecedencePatterns : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[][] testdata = {
                    new[] { "A or B and C", null, "PatternOrExpr" },
                    new[] { "(A or B) and C", null, "PatternAndExpr" },
                    new[] { "(A or B) and C", null, "PatternAndExpr" },
                    new[] { "every A or every B", null, "PatternOrExpr" },
                    new[] { "B -> D or A", null, "PatternFollowedByExpr" },
                    new[] { "every A and not B", null, "PatternAndExpr" },
                    new[] { "every A and not B", null, "PatternAndExpr" },
                    new[] { "every A -> B", null, "PatternFollowedByExpr" },
                    new[] { "A where timer:within(10)", null, "PatternGuardExpr" },
                    new[] { "every (A and B)", null, "PatternEveryExpr" },
                    new[] { "every A where timer:within(10)", null, "PatternEveryExpr" },
                    new[] { "A or B until C", null, "PatternOrExpr" },
                    new[] { "A or (B until C)", "A or B until C", "PatternOrExpr" },
                    new[] { "every (every A)", null, "PatternEveryExpr" },
                    new[] { "(A until B) until C", null, "PatternMatchUntilExpr" },
                };

                foreach (var aTestdata in testdata) {
                    var epl = "select * from pattern [" + aTestdata[0] + "]";
                    var expected = aTestdata[1];
                    var expressionLowestPrecedenceClass = aTestdata[2];
                    var failText = "Failed for [" + aTestdata[0] + "]";

                    var modelBefore = env.EplToModel(epl);
                    var eplAfter = modelBefore.ToEPL();

                    if (expected == null) {
                        Assert.AreEqual(epl, eplAfter, failText);
                    }
                    else {
                        var expectedEPL = "select * from pattern [" + expected + "]";
                        Assert.AreEqual(expectedEPL, eplAfter, failText);
                    }

                    // get where clause root expression of both models
                    var modelAfter = env.EplToModel(eplAfter);
                    Assert.AreEqual(
                        GetPatternRootExpr(modelAfter).GetType(),
                        GetPatternRootExpr(modelBefore).GetType(),
                        failText);
                    Assert.AreEqual(
                        expressionLowestPrecedenceClass,
                        GetPatternRootExpr(modelAfter).GetType().Name,
                        failText);
                }

                env.UndeployAll();
            }

            private PatternExpr GetPatternRootExpr(EPStatementObjectModel model)
            {
                var patternStream = (PatternStream)model.FromClause.Streams[0];
                return patternStream.Expression;
            }
        }
    }
} // end of namespace