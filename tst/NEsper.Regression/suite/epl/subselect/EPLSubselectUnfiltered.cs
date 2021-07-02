///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
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
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectInvalidSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinUnfiltered(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectJoinUnfiltered());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseReturningTrue(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseReturningTrue());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoSubqSelect(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectTwoSubqSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredStreamPriorCompile(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredStreamPriorCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredStreamPriorOM(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredStreamPriorOM());
            return execs;
        }

        public static IList<RegressionExecution> WithCustomFunction(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectCustomFunction());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseWithExpression(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseWithExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterInside(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectFilterInside());
            return execs;
        }

        public static IList<RegressionExecution> WithComputedResult(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectComputedResult());
            return execs;
        }

        public static IList<RegressionExecution> WithSelfSubselect(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelfSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopStatement(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectStartStopStatement());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredLastEvent(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredLastEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredNoAs(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredNoAs());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredWithAsWithinSubselect(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredWithAsWithinSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredAsAfterSubselect(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredAsAfterSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredLengthWindow(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredLengthWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredUnlimitedStream(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredUnlimitedStream());
            return execs;
        }

        public static IList<RegressionExecution> WithUnfilteredExpression(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectUnfilteredExpression());
            return execs;
        }

        private static void TryAssertSingleRowUnfiltered(
            RegressionEnvironment env,
            string stmtText,
            string columnName)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            // check type
            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(columnName));

            // test no event, should return null
            env.SendEventBean(new SupportBean_S0(0));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            // test one event
            env.SendEventBean(new SupportBean_S1(10));
            env.SendEventBean(new SupportBean_S0(1));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            // resend event
            env.SendEventBean(new SupportBean_S0(2));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            // test second event
            env.SendEventBean(new SupportBean_S1(999));
            env.SendEventBean(new SupportBean_S0(3));
            Assert.AreEqual(999, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            env.UndeployAll();
        }

        private static void RunUnfilteredStreamPrior(RegressionEnvironment env)
        {
            // check type
            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("IdS1"));

            // test no event, should return null
            env.SendEventBean(new SupportBean_S0(0));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

            // test one event
            env.SendEventBean(new SupportBean_S1(10));
            env.SendEventBean(new SupportBean_S0(1));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

            // resend event
            env.SendEventBean(new SupportBean_S0(2));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

            // test second event
            env.SendEventBean(new SupportBean_S0(3));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));
        }

        private static void TryAssertMultiRowUnfiltered(
            RegressionEnvironment env,
            string stmtText,
            string columnName)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            // check type
            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(columnName));

            // test no event, should return null
            env.SendEventBean(new SupportBean_S0(0));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            // test one event
            env.SendEventBean(new SupportBean_S1(10));
            env.SendEventBean(new SupportBean_S0(1));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            // resend event
            env.SendEventBean(new SupportBean_S0(2));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            // test second event
            env.SendEventBean(new SupportBean_S1(999));
            env.SendEventBean(new SupportBean_S0(3));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get(columnName));

            env.UndeployAll();
        }

        internal class EPLSubselectSelfSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "insert into MyCount select count(*) as cnt from SupportBean_S0;\n" +
                          "@Name('s0') select (select cnt from MyCount#lastevent) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectStartStopStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id from SupportBean_S0 where (select true from SupportBean_S1#length(1000))";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                var listener = env.Listener("s0");
                env.UndeployAll();
                env.SendEventBean(new SupportBean_S0(2));
                Assert.IsFalse(listener.IsInvoked);

                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                env.SendEventBean(new SupportBean_S0(2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(3));
                Assert.AreEqual(3, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWhereClauseReturningTrue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id from SupportBean_S0 where (select true from SupportBean_S1#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWhereClauseWithExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select Id from SupportBean_S0 where (select P10='X' from SupportBean_S1#length(1000))";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(10, "X"));
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(0, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectJoinUnfiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select" +
                    " (select Id from SupportBean_S3#length(1000)) as idS3," +
                    " (select Id from SupportBean_S4#length(1000)) as idS4 from " +
                    " SupportBean_S0#keepall as S0," +
                    " SupportBean_S1#keepall as S1 " +
                    " where S0.Id = S1.Id";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("idS3"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("idS4"));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(0));
                env.SendEventBean(new SupportBean_S1(0));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(null, theEvent.Get("idS3"));
                Assert.AreEqual(null, theEvent.Get("idS4"));

                // send one event
                env.SendEventBean(new SupportBean_S3(-1));
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(1));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(-1, theEvent.Get("idS3"));
                Assert.AreEqual(null, theEvent.Get("idS4"));

                // send one event
                env.SendEventBean(new SupportBean_S4(-2));
                env.SendEventBean(new SupportBean_S0(2));
                env.SendEventBean(new SupportBean_S1(2));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(-1, theEvent.Get("idS3"));
                Assert.AreEqual(-2, theEvent.Get("idS4"));

                // send second event
                env.SendEventBean(new SupportBean_S4(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.SendEventBean(new SupportBean_S1(3));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(-1, theEvent.Get("idS3"));
                Assert.AreEqual(null, theEvent.Get("idS4"));

                env.SendEventBean(new SupportBean_S3(-2));
                env.SendEventBean(new SupportBean_S0(3));
                env.SendEventBean(new SupportBean_S1(3));
                var events = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(3, events.Length);
                for (var i = 0; i < events.Length; i++) {
                    Assert.AreEqual(null, events[i].Get("idS3"));
                    Assert.AreEqual(null, events[i].Get("idS4"));
                }

                env.UndeployAll();
            }
        }

        internal class EPLSubselectInvalidSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select Id from SupportBean_S1) from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window (applies to correlated or non-fully-aggregated subqueries) [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select dummy from SupportBean_S1#lastevent) as IdS1 from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Failed to validate select-clause expression 'dummy': Property named 'dummy' is not valid in any stream [select (select dummy from SupportBean_S1#lastevent) as IdS1 from SupportBean_S0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select (select Id from SupportBean_S1#lastevent) Id from SupportBean_S1#lastevent) as IdS1 from SupportBean_S0",
                    "Invalid nested subquery, subquery-within-subquery is not supported [select (select (select Id from SupportBean_S1#lastevent) Id from SupportBean_S1#lastevent) as IdS1 from SupportBean_S0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select Id from SupportBean_S1#lastevent where (sum(Id) = 5)) as IdS1 from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Aggregation functions are not supported within subquery filters, consider using a having-clause or insert-into instead [select (select Id from SupportBean_S1#lastevent where (sum(Id) = 5)) as IdS1 from SupportBean_S0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean_S0(Id=5 and (select Id from SupportBean_S1))",
                    "Failed to validate subquery number 1 querying SupportBean_S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from SupportBean_S0(Id=5 and (select Id from SupportBean_S1))]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean_S0 group by Id + (select Id from SupportBean_S1)",
                    "Subselects not allowed within group-by [select * from SupportBean_S0 group by Id + (select Id from SupportBean_S1)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean_S0 order by (select Id from SupportBean_S1) asc",
                    "Subselects not allowed within order-by clause [select * from SupportBean_S0 order by (select Id from SupportBean_S1) asc]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select Id from SupportBean_S1#lastevent where 'a') from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subselect filter expression must return a boolean value [select (select Id from SupportBean_S1#lastevent where 'a') from SupportBean_S0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select Id from SupportBean_S1#lastevent where Id = P00) from SupportBean_S0",
                    "Failed to plan subquery number 1 querying SupportBean_S1: Failed to validate filter expression 'Id=P00': Property named 'P00' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [select (select Id from SupportBean_S1#lastevent where Id = P00) from SupportBean_S0]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select Id in (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0",
                    "Failed to validate select-clause expression subquery number 1 querying SupportBean_S1: Implicit conversion from datatype '" +
                    typeof(SupportBean_S1).CleanName() +
                    "' to '" +
                    typeof(int?).CleanName() +
                    "' is not allowed [select Id in (select * from SupportBean_S1#length(1000)) as value from SupportBean_S0]");
            }
        }

        internal class EPLSubselectUnfilteredStreamPriorOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.Create().Add(Expressions.Prior(0, "Id"));
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView("length", Expressions.Constant(1000)));

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "IdS1");
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
                model = env.CopyMayFail(model);

                var stmtText =
                    "select (select prior(0,Id) from SupportBean_S1#length(1000)) as IdS1 from SupportBean_S0";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                RunUnfilteredStreamPrior(env);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectUnfilteredStreamPriorCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select prior(0,Id) from SupportBean_S1#length(1000)) as IdS1 from SupportBean_S0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");
                RunUnfilteredStreamPrior(env);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectCustomFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select (select " +
                               typeof(SupportStaticMethodLib).FullName +
                               ".MinusOne(Id) from SupportBean_S1#length(1000)) as IdS1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("IdS1"));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                // test one event
                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(9d, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                // resend event
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(9d, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectComputedResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select 100*(select Id from SupportBean_S1#length(1000)) as IdS1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("IdS1"));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                // test one event
                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(1000, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                // resend event
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(1000, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectFilterInside : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1(P10='A')#length(1000)) as IdS1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(1, "X"));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                env.SendEventBean(new SupportBean_S1(1, "A"));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("IdS1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUnfilteredUnlimitedStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1#length(1000)) as IdS1 from SupportBean_S0";
                TryAssertMultiRowUnfiltered(env, stmtText, "IdS1");
            }
        }

        internal class EPLSubselectUnfilteredLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1#length(2)) as IdS1 from SupportBean_S0";
                TryAssertMultiRowUnfiltered(env, stmtText, "IdS1");
            }
        }

        internal class EPLSubselectUnfilteredAsAfterSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1#lastevent) as IdS1 from SupportBean_S0";
                TryAssertSingleRowUnfiltered(env, stmtText, "IdS1");
            }
        }

        internal class EPLSubselectUnfilteredWithAsWithinSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id as myId from SupportBean_S1#lastevent) from SupportBean_S0";
                TryAssertSingleRowUnfiltered(env, stmtText, "myId");
            }
        }

        internal class EPLSubselectUnfilteredNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select (select Id from SupportBean_S1#lastevent) from SupportBean_S0";
                TryAssertSingleRowUnfiltered(env, stmtText, "Id");
            }
        }

        public class EPLSubselectUnfilteredLastEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString", "col"};
                var epl =
                    "@Name('s0') select TheString, (select P00 from SupportBean_S0#lastevent()) as col from SupportBean";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", null});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(11, "S01"));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "S01"});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", "S01"});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(12, "S02"));
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", "S02"});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUnfilteredExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select P10 || P11 from SupportBean_S1#lastevent) as value from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("value"));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(1));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(null, theEvent.Get("value"));

                // test one event
                env.SendEventBean(new SupportBean_S1(-1, "a", "b"));
                env.SendEventBean(new SupportBean_S0(1));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("ab", theEvent.Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectTwoSubqSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select (select Id+1 as myId from SupportBean_S1#lastevent) as idS1_0, " +
                               "(select Id+2 as myId from SupportBean_S1#lastevent) as idS1_1 from SupportBean_S0";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // check type
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("idS1_0"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("idS1_1"));

                // test no event, should return null
                env.SendEventBean(new SupportBean_S0(1));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(null, theEvent.Get("idS1_0"));
                Assert.AreEqual(null, theEvent.Get("idS1_1"));

                // test one event
                env.SendEventBean(new SupportBean_S1(10));
                env.SendEventBean(new SupportBean_S0(1));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(11, theEvent.Get("idS1_0"));
                Assert.AreEqual(12, theEvent.Get("idS1_1"));

                // resend event
                env.SendEventBean(new SupportBean_S0(2));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(11, theEvent.Get("idS1_0"));
                Assert.AreEqual(12, theEvent.Get("idS1_1"));

                // test second event
                env.SendEventBean(new SupportBean_S1(999));
                env.SendEventBean(new SupportBean_S0(3));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(1000, theEvent.Get("idS1_0"));
                Assert.AreEqual(1001, theEvent.Get("idS1_1"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace