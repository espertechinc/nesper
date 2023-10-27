///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectUnfiltered
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithUnfilteredExpression(execs);
            WithUnfilteredUnlimitedStream(execs);
            WithUnfilteredLengthWindow(execs);
            WithUnfilteredAsAfterSubselect(execs);
            WithUnfilteredWithAsWithinSubselect(execs);
            WithUnfilteredNoAs(execs);
            WithUnfilteredLastEvent(execs);
            WithStartStopStatement(execs);
            WithSelfSubselect(execs);
            WithComputedResult(execs);
            WithFilterInside(execs);
            WithWhereClauseWithExpression(execs);
            WithCustomFunction(execs);
            WithUnfilteredStreamPriorOM(execs);
            WithUnfilteredStreamPriorCompile(execs);
            WithTwoSubqSelect(execs);
            WithWhereClauseReturningTrue(execs);
            WithJoinUnfiltered(execs);
            WithInvalidSubselect(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInvalidSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinUnfiltered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectJoinUnfiltered());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseReturningTrue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseReturningTrue());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoSubqSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectTwoSubqSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredStreamPriorCompile(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredStreamPriorCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredStreamPriorOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredStreamPriorOM());
            return execs;
        }

        public static IList<RegressionExecution> WithCustomFunction(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectCustomFunction());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseWithExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseWithExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterInside(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectFilterInside());
            return execs;
        }

        public static IList<RegressionExecution> WithComputedResult(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectComputedResult());
            return execs;
        }

        public static IList<RegressionExecution> WithSelfSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelfSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopStatement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectStartStopStatement());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredLastEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredLastEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredNoAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredNoAs());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredWithAsWithinSubselect(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredWithAsWithinSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredAsAfterSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredAsAfterSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredLengthWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredLengthWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredUnlimitedStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredUnlimitedStream());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredExpression());
            return execs;
        }

        private class EPLSubselectSelfSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "insert into MyCount select count(*) as cnt from SupportBean_S0;\n" +
                          "@name('s0') select (select cnt from MyCount#lastevent) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "value", null);

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "value", 1L);

                env.UndeployAll();
            }
        }

        private class EPLSubselectStartStopStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 where (select true from SupportBean_S1#length(1000))";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.UndeployAll();
                env.SendEventBean(new SupportBean_S0(2));

                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertEqualsNew("s0", "Id", 3);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class EPLSubselectWhereClauseReturningTrue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 where (select true from SupportBean_S1#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.UndeployAll();
            }
        }

        private class EPLSubselectWhereClauseWithExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id from SupportBean_S0 where (select P10='X' from SupportBean_S1#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(10, "X"));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "Id", 0);

                env.UndeployAll();
            }
        }

        private class EPLSubselectJoinUnfiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select id from SupportBean_S3#length(1000)) as idS3, (select Id from SupportBean_S4#length(1000)) as idS4 from SupportBean_S0#keepall as s0, SupportBean_S1#keepall as s1 where s0.Id = s1.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("idS3"));
                        Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("idS4"));
                    });

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(0));
                env.SendEventBean(new SupportBean_S1(0));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(null, theEvent.Get("idS3"));
                        Assert.AreEqual(null, theEvent.Get("idS4"));
                    });

                // send one event
                env.SendEventBean(new SupportBean_S3(-1));
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(1));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(-1, theEvent.Get("idS3"));
                        Assert.AreEqual(null, theEvent.Get("idS4"));
                    });

                // send one event
                env.SendEventBean(new SupportBean_S4(-2));
                env.SendEventBean(new SupportBean_S0(2));
                env.SendEventBean(new SupportBean_S1(2));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(-1, theEvent.Get("idS3"));
                        Assert.AreEqual(-2, theEvent.Get("idS4"));
                    });

                // send second event
                env.SendEventBean(new SupportBean_S4(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.SendEventBean(new SupportBean_S1(3));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(-1, theEvent.Get("idS3"));
                        Assert.AreEqual(null, theEvent.Get("idS4"));
                    });

                env.SendEventBean(new SupportBean_S3(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.SendEventBean(new SupportBean_S1(3));
                env.AssertListener(
                    "s0",
                    listener => {
                        var events = listener.NewDataListFlattened;
                        Assert.AreEqual(3, events.Length);
                        for (var i = 0; i < events.Length; i++) {
                            Assert.AreEqual(null, events[i].Get("idS3"));
                            Assert.AreEqual(null, events[i].Get("idS4"));
                        }
                    });

                env.UndeployAll();
            }
        }

        private class EPLSubselectInvalidSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select (select Id from SupportBean_S1) from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window (applies to correlated or non-fully-aggregated subqueries) [");

                env.TryInvalidCompile(
                    "select (select dummy from SupportBean_S1#lastevent) as idS1 from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Failed to validate select-clause expression 'dummy': Property named 'dummy' is not valid in any stream [select (select dummy from SupportBean_S1#lastevent) as idS1 from SupportBean_S0]");

                env.TryInvalidCompile(
                    "select (select (select id from SupportBean_S1#lastevent) Id from SupportBean_S1#lastevent) as idS1 from SupportBean_S0",
                    "Invalid nested subquery, subquery-within-subquery is not supported [select (select (select id from SupportBean_S1#lastevent) Id from SupportBean_S1#lastevent) as idS1 from SupportBean_S0]");

                env.TryInvalidCompile(
                    "select (select id from SupportBean_S1#lastevent where (sum(Id) = 5)) as idS1 from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Aggregation functions are not supported within subquery filters, consider using a having-clause or insert-into instead [select (select id from SupportBean_S1#lastevent where (sum(Id) = 5)) as idS1 from SupportBean_S0]");

                env.TryInvalidCompile(
                    "select * from SupportBean_S0(id=5 and (select Id from SupportBean_S1))",
                    "Failed to validate subquery number 1 querying SupportBean_S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from SupportBean_S0(id=5 and (select Id from SupportBean_S1))]");

                env.TryInvalidCompile(
                    "select * from SupportBean_S0 group by id + (select Id from SupportBean_S1)",
                    "Subselects not allowed within group-by [select * from SupportBean_S0 group by id + (select Id from SupportBean_S1)]");

                env.TryInvalidCompile(
"select * from SupportBean_S0 Order by (select Id from SupportBean_S1) asc",
"Subselects not allowed within Order-by clause [select * from SupportBean_S0 Order by (select Id from SupportBean_S1) asc]");

                env.TryInvalidCompile(
                    "select (select Id from SupportBean_S1#lastevent where 'a') from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subselect filter expression must return a boolean value [select (select Id from SupportBean_S1#lastevent where 'a') from SupportBean_S0]");

                env.TryInvalidCompile(
                    "select (select id from SupportBean_S1#lastevent where Id = P00) from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Failed to validate filter expression 'id=P00': Property named 'P00' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [select (select id from SupportBean_S1#lastevent where Id = P00) from SupportBean_S0]");

                env.TryInvalidCompile(
                    "select Id in (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean_S1: Implicit conversion from datatype '" +
                    typeof(SupportBean_S1).FullName +
                    "' to 'Integer' is not allowed [select Id in (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0]");
            }
        }

        private class EPLSubselectUnfilteredStreamPriorOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.Create().Add(Expressions.Prior(0, "Id"));
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView("length", Expressions.Constant(1000)));

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "idS1");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model = env.CopyMayFail(model);

                var stmtText =
                    "select (select prior(0,Id) from SupportBean_S1#length(1000)) as idS1 from SupportBean_S0";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                RunUnfilteredStreamPrior(env);
                env.UndeployAll();
            }
        }

        private class EPLSubselectUnfilteredStreamPriorCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select prior(0,Id) from SupportBean_S1#length(1000)) as idS1 from SupportBean_S0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");
                RunUnfilteredStreamPrior(env);
                env.UndeployAll();
            }
        }

        private class EPLSubselectCustomFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select (select " +
                               typeof(SupportStaticMethodLib).FullName +
                               ".MinusOne(Id) from SupportBean_S1#length(1000)) as idS1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("idS1")));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "idS1", null);

                // test one event
                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "idS1", 9d);

                // resend event
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "idS1", 9d);

                env.UndeployAll();
            }
        }

        private class EPLSubselectComputedResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select 100*(select Id from SupportBean_S1#length(1000)) as idS1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("idS1")));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "idS1", null);

                // test one event
                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "idS1", 1000);

                // resend event
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "idS1", 1000);

                env.UndeployAll();
            }
        }

        private class EPLSubselectFilterInside : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S1(P10='A')#length(1000)) as idS1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(1, "X"));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "idS1", null);

                env.SendEventBean(new SupportBean_S1(1, "A"));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "idS1", 1);

                env.UndeployAll();
            }
        }

        private class EPLSubselectUnfilteredUnlimitedStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S1#length(1000)) as idS1 from SupportBean_S0";
                TryAssertMultiRowUnfiltered(env, stmtText, "idS1");
            }
        }

        private class EPLSubselectUnfilteredLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S1#length(2)) as idS1 from SupportBean_S0";
                TryAssertMultiRowUnfiltered(env, stmtText, "idS1");
            }
        }

        private class EPLSubselectUnfilteredAsAfterSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S1#lastevent) as idS1 from SupportBean_S0";
                TryAssertSingleRowUnfiltered(env, stmtText, "idS1");
            }
        }

        private class EPLSubselectUnfilteredWithAsWithinSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id as myId from SupportBean_S1#lastevent) from SupportBean_S0";
                TryAssertSingleRowUnfiltered(env, stmtText, "myId");
            }
        }

        private class EPLSubselectUnfilteredNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select (select Id from SupportBean_S1#lastevent) from SupportBean_S0";
                TryAssertSingleRowUnfiltered(env, stmtText, "Id");
            }
        }

        public class EPLSubselectUnfilteredLastEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString,col".SplitCsv();
                var epl =
                    "@name('s0') select TheString, (select P00 from SupportBean_S0#lastevent()) as col from SupportBean";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1", null });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(11, "S01"));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "S01" });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s0", fields, new object[] { "E3", "S01" });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(12, "S02"));
                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertPropsNew("s0", fields, new object[] { "E4", "S02" });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUnfilteredExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select P10 || P11 from SupportBean_S1#lastevent) as value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("value")));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "value", null);

                // test one event
                env.SendEventBean(new SupportBean_S1(-1, "a", "b"));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "value", "ab");

                env.UndeployAll();
            }
        }

        private class EPLSubselectTwoSubqSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "idS1_0,idS1_1".SplitCsv();
                var stmtText = "@name('s0') select (select Id+1 as myId from SupportBean_S1#lastevent) as idS1_0, " +
                               "(select Id+2 as myId from SupportBean_S1#lastevent) as idS1_1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("idS1_0"));
                        Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("idS1_1"));
                    });

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew("s0", fields, new object[] { null, null });

                // test one event
                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew("s0", fields, new object[] { 11, 12 });

                // resend event
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertPropsNew("s0", fields, new object[] { 11, 12 });

                // test second event
                env.SendEventBean(new SupportBean_S1(999));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertPropsNew("s0", fields, new object[] { 1000, 1001 });

                env.UndeployAll();
            }
        }

        private static void TryAssertSingleRowUnfiltered(
            RegressionEnvironment env,
            string stmtText,
            string columnName)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            // check type
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType(columnName)));

            // test no event, should return null
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEqualsNew("s0", columnName, null);

            // test one event
            env.SendEventBean(new SupportBean_S1(10));
            env.SendEventBean(new SupportBean_S0(1));
            env.AssertEqualsNew("s0", columnName, 10);

            // resend event
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", columnName, 10);

            // test second event
            env.SendEventBean(new SupportBean_S1(999));
            env.SendEventBean(new SupportBean_S0(3));
            env.AssertEqualsNew("s0", columnName, 999);

            env.UndeployAll();
        }

        private static void RunUnfilteredStreamPrior(RegressionEnvironment env)
        {
            // check type
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("idS1")));

            // test no event, should return null
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEqualsNew("s0", "idS1", null);

            // test one event
            env.SendEventBean(new SupportBean_S1(10));
            env.SendEventBean(new SupportBean_S0(1));
            env.AssertEqualsNew("s0", "idS1", 10);

            // resend event
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", "idS1", 10);

            // test second event
            env.SendEventBean(new SupportBean_S0(3));
            env.AssertEqualsNew("s0", "idS1", 10);
        }

        private static void TryAssertMultiRowUnfiltered(
            RegressionEnvironment env,
            string stmtText,
            string columnName)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            // check type
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType(columnName)));

            // test no event, should return null
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEqualsNew("s0", columnName, null);

            // test one event
            env.SendEventBean(new SupportBean_S1(10));
            env.SendEventBean(new SupportBean_S0(1));
            env.AssertEqualsNew("s0", columnName, 10);

            // resend event
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", columnName, 10);

            // test second event
            env.SendEventBean(new SupportBean_S1(999));
            env.SendEventBean(new SupportBean_S0(3));
            env.AssertEqualsNew("s0", columnName, null);

            env.UndeployAll();
        }
    }
} // end of namespace