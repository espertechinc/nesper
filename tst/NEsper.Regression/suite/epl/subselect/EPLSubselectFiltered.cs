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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectFiltered
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithHavingNoAggNoFilterNoWhere(execs);
            WithHavingNoAggWWhere(execs);
            WithHavingNoAggWFilterWWhere(execs);
            WithSameEventCompile(execs);
            WithSameEventOM(execs);
            WithSameEvent(execs);
            WithSelectSceneOne(execs);
            WithSelectWildcard(execs);
            WithSelectWildcardNoName(execs);
            WithWhereConstant(execs);
            WithWherePrevious(execs);
            WithWherePreviousOM(execs);
            WithWherePreviousCompile(execs);
            WithSelectWithWhereJoined(execs);
            WithSelectWhereJoined2Streams(execs);
            WithSelectWhereJoined3Streams(execs);
            WithSelectWhereJoined3SceneTwo(execs);
            WithSelectWhereJoined4Coercion(execs);
            WithSelectWhereJoined4BackCoercion(execs);
            WithSelectWithWhere2Subqery(execs);
            WithJoinFilteredOne(execs);
            WithJoinFilteredTwo(execs);
            WithSubselectMixMax(execs);
            WithSubselectPrior(execs);
            WithWhereClauseMultikeyWArrayPrimitive(execs);
            WithWhereClauseMultikeyWArray2Field(execs);
            WithWhereClauseMultikeyWArrayComposite(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseMultikeyWArrayComposite(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseMultikeyWArrayComposite());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseMultikeyWArray2Field(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseMultikeyWArray2Field());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseMultikeyWArrayPrimitive(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereClauseMultikeyWArrayPrimitive());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselectPrior(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSubselectPrior());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselectMixMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSubselectMixMax());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinFilteredTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectJoinFilteredTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinFilteredOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectJoinFilteredOne());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWithWhere2Subqery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWithWhere2Subqery());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWhereJoined4BackCoercion(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWhereJoined4BackCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWhereJoined4Coercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWhereJoined4Coercion());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWhereJoined3SceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWhereJoined3SceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWhereJoined3Streams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWhereJoined3Streams());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWhereJoined2Streams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWhereJoined2Streams());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWithWhereJoined(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWithWhereJoined());
            return execs;
        }

        public static IList<RegressionExecution> WithWherePreviousCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWherePreviousCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithWherePreviousOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWherePreviousOM());
            return execs;
        }

        public static IList<RegressionExecution> WithWherePrevious(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWherePrevious());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereConstant(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectWhereConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWildcardNoName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWildcardNoName());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSelectSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithSameEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSameEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithSameEventOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSameEventOM());
            return execs;
        }

        public static IList<RegressionExecution> WithSameEventCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSameEventCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithHavingNoAggWFilterWWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectHavingNoAggWFilterWWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithHavingNoAggWWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectHavingNoAggWWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithHavingNoAggNoFilterNoWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectHavingNoAggNoFilterNoWhere());
            return execs;
        }

        private class EPLSubselectWhereClauseMultikeyWArrayComposite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select (select Id from SupportEventWithManyArray#keepall as sm " +
                          "where sm.intOne = se.array and sm.value > se.value) as value from SupportEventWithIntArray as se";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "MA1", new int[] { 1, 2 }, 100);
                SendManyArray(env, "MA2", new int[] { 1, 2 }, 200);
                SendManyArray(env, "MA3", new int[] { 1 }, 300);
                SendManyArray(env, "MA4", new int[] { 1, 2 }, 400);

                env.Milestone(0);

                SendIntArrayAndAssert(env, "IA2", new int[] { 1, 2 }, 250, "MA4");
                SendIntArrayAndAssert(env, "IA3", new int[] { 1, 2 }, 0, null);
                SendIntArrayAndAssert(env, "IA4", new int[] { 1 }, 299, "MA3");
                SendIntArrayAndAssert(env, "IA5", new int[] { 1, 2 }, 500, null);

                env.UndeployAll();
            }
        }

        private class EPLSubselectWhereClauseMultikeyWArray2Field : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select (select Id from SupportEventWithManyArray#keepall as sm " +
                          "where sm.intOne = se.array and sm.value = se.value) as value from SupportEventWithIntArray as se";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "MA1", new int[] { 1, 2 }, 10);
                SendManyArray(env, "MA2", new int[] { 1, 2 }, 11);
                SendManyArray(env, "MA3", new int[] { 1 }, 12);

                env.Milestone(0);

                SendIntArrayAndAssert(env, "IA1", new int[] { 1 }, 12, "MA3");
                SendIntArrayAndAssert(env, "IA2", new int[] { 1, 2 }, 11, "MA2");
                SendIntArrayAndAssert(env, "IA3", new int[] { 1, 2 }, 10, "MA1");
                SendIntArrayAndAssert(env, "IA4", new int[] { 1 }, 10, null);
                SendIntArrayAndAssert(env, "IA5", new int[] { 1, 2 }, 12, null);

                env.UndeployAll();
            }
        }

        private class EPLSubselectWhereClauseMultikeyWArrayPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select Id from SupportEventWithManyArray#keepall as sm where sm.intOne = se.array) as value from SupportEventWithIntArray as se";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "MA1", new int[] { 1, 2 });
                SendIntArrayAndAssert(env, "IA1", new int[] { 1, 2 }, "MA1");

                SendManyArray(env, "MA2", new int[] { 1, 2 });
                SendManyArray(env, "MA3", new int[] { 1 });
                SendManyArray(env, "MA4", new int[] { });
                SendManyArray(env, "MA5", null);

                env.Milestone(0);

                SendIntArrayAndAssert(env, "IA2", new int[] { }, "MA4");
                SendIntArrayAndAssert(env, "IA3", new int[] { 1 }, "MA3");
                SendIntArrayAndAssert(env, "IA4", null, "MA5");
                SendIntArrayAndAssert(env, "IA5", new int[] { 1, 2 }, null);

                env.UndeployAll();
            }
        }

        private class EPLSubselectSameEventCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S1";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        typeof(SupportBean_S1),
                        statement.EventType.GetPropertyType("events1")));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("events1")));

                env.UndeployAll();
            }
        }

        private class EPLSubselectSameEventOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.CreateWildcard();
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView(View.Create("length", Expressions.Constant(1000))));

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S1"));
                model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "events1");
                model = env.CopyMayFail(model);

                var stmtText = "select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S1";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        typeof(SupportBean_S1),
                        statement.EventType.GetPropertyType("events1")));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("events1")));

                env.UndeployAll();
            }
        }

        private class EPLSubselectSameEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S1";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        typeof(SupportBean_S1),
                        statement.EventType.GetPropertyType("events1")));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("events1")));

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S0";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        typeof(SupportBean_S1),
                        statement.EventType.GetPropertyType("events1")));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.SendEventBean(new SupportBean_S0(0));

                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("events1")));

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWildcardNoName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select (select * from SupportBean_S1#length(1000)) from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        typeof(SupportBean_S1),
                        statement.EventType.GetPropertyType("subselect_1")));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.SendEventBean(new SupportBean_S0(0));

                env.AssertEventNew("s0", @event => Assert.AreSame(theEvent, @event.Get("subselect_1")));

                env.UndeployAll();
            }
        }

        private class EPLSubselectWhereConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // single-column constant
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S1#length(1000) where P10='X') as ids1 from SupportBean_S0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S1(-1, "Y"));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "ids1", null);

                env.SendEventBean(new SupportBean_S1(1, "X"));
                env.SendEventBean(new SupportBean_S1(2, "Y"));
                env.SendEventBean(new SupportBean_S1(3, "Z"));

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "ids1", 1);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "ids1", 1);

                env.SendEventBean(new SupportBean_S1(2, "X"));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "ids1", null);
                env.UndeployAll();

                // two-column constant
                stmtText =
                    "@name('s0') select (select Id from SupportBean_S1#length(1000) where P10='X' and P11='Y') as ids1 from SupportBean_S0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S1(1, "X", "Y"));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "ids1", 1);
                env.UndeployAll();

                // single range
                stmtText =
                    "@name('s0') select (select TheString from SupportBean#lastevent where IntPrimitive between 10 and 20) as ids1 from SupportBean_S0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("E1", 15));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "ids1", "E1");

                env.UndeployAll();
            }
        }

        private class EPLSubselectWherePrevious : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select prev(1, id) from SupportBean_S1#length(1000) where Id=s0.Id) as value from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                RunWherePrevious(env);
                env.UndeployAll();
            }
        }

        private class EPLSubselectWherePreviousOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.Create().Add(Expressions.Previous(1, "Id"));
                subquery.FromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S1").AddView(View.Create("length", Expressions.Constant(1000))));
                subquery.WhereClause = Expressions.EqProperty("Id", "s0.Id");

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0", "s0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "value");
                model = env.CopyMayFail(model);

                var stmtText =
                    "select (select prev(1,id) from SupportBean_S1#length(1000) where Id=s0.Id) as value from SupportBean_S0 as s0";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                RunWherePrevious(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectWherePreviousCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select prev(1,id) from SupportBean_S1#length(1000) where Id=s0.Id) as value from SupportBean_S0 as s0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0").Milestone(0);

                RunWherePrevious(env);

                env.UndeployAll();
            }
        }

        public class EPLSubselectSelectSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "s0price", "s1price" };
                var text = "@name('s0') select irstream s0.Price as s0price, " +
" (select Price from SupportMarketDataBean(Symbol='S1')#length(10) s1"+
                           " where s0.Volume = s1.Volume) as s1price " +
" from  SupportMarketDataBean(Symbol='S0')#length(2) s0";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S0", 100, 1));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { 100.0, null } },
                    null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S1", -10, 2));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S0", 200, 2));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { 200.0, -10.0 } },
                    null);

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", -20, 3));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S0", 300, 3));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { 300.0, -20.0 } },
                    new object[][] { new object[] { 100.0, null } });

                env.Milestone(5);

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWithWhereJoined : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S1#length(1000) where P10=s0.P00) as ids1 from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "ids1", null);

                env.SendEventBean(new SupportBean_S1(1, "X"));
                env.SendEventBean(new SupportBean_S1(2, "Y"));
                env.SendEventBean(new SupportBean_S1(3, "Z"));

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "ids1", null);

                env.SendEventBean(new SupportBean_S0(0, "X"));
                env.AssertEqualsNew("s0", "ids1", 1);
                env.SendEventBean(new SupportBean_S0(0, "Y"));
                env.AssertEqualsNew("s0", "ids1", 2);
                env.SendEventBean(new SupportBean_S0(0, "Z"));
                env.AssertEqualsNew("s0", "ids1", 3);
                env.SendEventBean(new SupportBean_S0(0, "A"));
                env.AssertEqualsNew("s0", "ids1", null);

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWhereJoined2Streams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S0#length(1000) where P00=s1.P10 and P00=s2.P20) as ids0 from SupportBean_S1#keepall as s1, SupportBean_S2#keepall as s2 where s1.Id = s2.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10, "s0_1"));
                env.SendEventBean(new SupportBean_S2(10, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S0(99, "s0_1"));
                env.SendEventBean(new SupportBean_S1(11, "s0_1"));
                env.SendEventBean(new SupportBean_S2(11, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", 99);

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWhereJoined3Streams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S0#length(1000) where P00=s1.P10 and P00=s3.P30) as ids0 " +
                    "from SupportBean_S1#keepall as s1, SupportBean_S2#keepall as s2, SupportBean_S3#keepall as s3 where s1.Id = s2.Id and s2.Id = s3.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10, "s0_1"));
                env.SendEventBean(new SupportBean_S2(10, "s0_1"));
                env.SendEventBean(new SupportBean_S3(10, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S0(99, "s0_1"));
                env.SendEventBean(new SupportBean_S1(11, "s0_1"));
                env.SendEventBean(new SupportBean_S2(11, "xxx"));
                env.SendEventBean(new SupportBean_S3(11, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", 99);

                env.SendEventBean(new SupportBean_S0(98, "s0_2"));
                env.SendEventBean(new SupportBean_S1(12, "s0_x"));
                env.SendEventBean(new SupportBean_S2(12, "s0_2"));
                env.SendEventBean(new SupportBean_S3(12, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S1(13, "s0_2"));
                env.SendEventBean(new SupportBean_S2(13, "s0_2"));
                env.SendEventBean(new SupportBean_S3(13, "s0_x"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S1(14, "s0_2"));
                env.SendEventBean(new SupportBean_S2(14, "xx"));
                env.SendEventBean(new SupportBean_S3(14, "s0_2"));
                env.AssertEqualsNew("s0", "ids0", 98);

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWhereJoined3SceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select (select Id from SupportBean_S0#length(1000) where P00=s1.P10 and P00=s3.P30 and P00=s2.P20) as ids0 " +
                    "from SupportBean_S1#keepall as s1, SupportBean_S2#keepall as s2, SupportBean_S3#keepall as s3 where s1.Id = s2.Id and s2.Id = s3.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10, "s0_1"));
                env.SendEventBean(new SupportBean_S2(10, "s0_1"));
                env.SendEventBean(new SupportBean_S3(10, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S0(99, "s0_1"));
                env.SendEventBean(new SupportBean_S1(11, "s0_1"));
                env.SendEventBean(new SupportBean_S2(11, "xxx"));
                env.SendEventBean(new SupportBean_S3(11, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S0(98, "s0_2"));
                env.SendEventBean(new SupportBean_S1(12, "s0_x"));
                env.SendEventBean(new SupportBean_S2(12, "s0_2"));
                env.SendEventBean(new SupportBean_S3(12, "s0_1"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S1(13, "s0_2"));
                env.SendEventBean(new SupportBean_S2(13, "s0_2"));
                env.SendEventBean(new SupportBean_S3(13, "s0_x"));
                env.AssertEqualsNew("s0", "ids0", null);

                env.SendEventBean(new SupportBean_S1(14, "s0_2"));
                env.SendEventBean(new SupportBean_S2(14, "s0_2"));
                env.SendEventBean(new SupportBean_S3(14, "s0_2"));
                env.AssertEqualsNew("s0", "ids0", 98);

                env.UndeployAll();
            }
        }

        private class EPLSubselectSelectWhereJoined4Coercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtText = "@name('s0') select " +
                               "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                               "  where IntBoxed=s1.LongBoxed and " +
                               "IntBoxed=s2.DoubleBoxed and " +
                               "DoubleBoxed=s3.IntBoxed" +
                               ") as ids0 from " +
                               "SupportBean(TheString='A')#keepall as s1, " +
                               "SupportBean(TheString='B')#keepall as s2, " +
                               "SupportBean(TheString='C')#keepall as s3 " +
                               "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
                TrySelectWhereJoined4Coercion(env, milestone, stmtText);

                stmtText = "@name('s0') select " +
                           "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                           "  where DoubleBoxed=s3.IntBoxed and " +
                           "IntBoxed=s2.DoubleBoxed and " +
                           "IntBoxed=s1.LongBoxed" +
                           ") as ids0 from " +
                           "SupportBean(TheString='A')#keepall as s1, " +
                           "SupportBean(TheString='B')#keepall as s2, " +
                           "SupportBean(TheString='C')#keepall as s3 " +
                           "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
                TrySelectWhereJoined4Coercion(env, milestone, stmtText);

                stmtText = "@name('s0') select " +
                           "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                           "  where DoubleBoxed=s3.IntBoxed and " +
                           "IntBoxed=s1.LongBoxed and " +
                           "IntBoxed=s2.DoubleBoxed" +
                           ") as ids0 from " +
                           "SupportBean(TheString='A')#keepall as s1, " +
                           "SupportBean(TheString='B')#keepall as s2, " +
                           "SupportBean(TheString='C')#keepall as s3 " +
                           "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
                TrySelectWhereJoined4Coercion(env, milestone, stmtText);
            }
        }

        private class EPLSubselectSelectWhereJoined4BackCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtText = "@name('s0') select " +
                               "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                               "  where LongBoxed=s1.IntBoxed and " +
                               "LongBoxed=s2.DoubleBoxed and " +
                               "IntBoxed=s3.LongBoxed" +
                               ") as ids0 from " +
                               "SupportBean(TheString='A')#keepall as s1, " +
                               "SupportBean(TheString='B')#keepall as s2, " +
                               "SupportBean(TheString='C')#keepall as s3 " +
                               "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
                TrySelectWhereJoined4CoercionBack(env, milestone, stmtText);

                stmtText = "@name('s0') select " +
                           "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                           "  where LongBoxed=s2.DoubleBoxed and " +
                           "IntBoxed=s3.LongBoxed and " +
                           "LongBoxed=s1.IntBoxed " +
                           ") as ids0 from " +
                           "SupportBean(TheString='A')#keepall as s1, " +
                           "SupportBean(TheString='B')#keepall as s2, " +
                           "SupportBean(TheString='C')#keepall as s3 " +
                           "where s1.IntPrimitive = s2.IntPrimitive and s2.IntPrimitive = s3.IntPrimitive";
                TrySelectWhereJoined4CoercionBack(env, milestone, stmtText);
            }
        }

        private class EPLSubselectSelectWithWhere2Subqery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Id from SupportBean_S0 as s0 where " +
                               " id = (select id from SupportBean_S1#length(1000) where s0.Id = id) or id = (select id from SupportBean_S2#length(1000) where s0.Id = Id)";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(1));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "Id", 1);

                env.SendEventBean(new SupportBean_S2(2));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("s0", "Id", 2);

                env.SendEventBean(new SupportBean_S0(3));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(3));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertEqualsNew("s0", "Id", 3);

                env.UndeployAll();
            }
        }

        private class EPLSubselectJoinFilteredOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s0.Id as s0id, s1.Id as s1id, " +
                               "(select P20 from SupportBean_S2#length(1000) where Id=s0.Id) as s2p20, " +
                               "(select prior(1, P20) from SupportBean_S2#length(1000) where Id=s0.Id) as s2p20Prior, " +
                               "(select prev(1, P20) from SupportBean_S2#length(10) where Id=s0.Id) as s2p20Prev " +
                               "from SupportBean_S0#keepall as s0, SupportBean_S1#keepall as s1 " +
                               "where s0.Id = s1.Id and P00||P10 = (select P20 from SupportBean_S2#length(1000) where Id=s0.Id)";
                TryJoinFiltered(env, stmtText);
            }
        }

        private class EPLSubselectJoinFilteredTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select s0.Id as s0id, s1.Id as s1id, " +
                               "(select P20 from SupportBean_S2#length(1000) where Id=s0.Id) as s2p20, " +
                               "(select prior(1, P20) from SupportBean_S2#length(1000) where Id=s0.Id) as s2p20Prior, " +
                               "(select prev(1, P20) from SupportBean_S2#length(10) where Id=s0.Id) as s2p20Prev " +
                               "from SupportBean_S0#keepall as s0, SupportBean_S1#keepall as s1 " +
                               "where s0.Id = s1.Id and (select s0.P00||s1.P10 = P20 from SupportBean_S2#length(1000) where Id=s0.Id)";
                TryJoinFiltered(env, stmtText);
            }
        }

        private class EPLSubselectSubselectPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "insert into Pair " +
                          "select * from SupportSensorEvent(device='A')#lastevent as a, SupportSensorEvent(device='B')#lastevent as b " +
                          "where a.type = b.type;\n" +
                          "" +
                          "insert into PairDuplicatesRemoved select * from Pair(1=2);\n" +
                          "" +
                          "@name('s0') insert into PairDuplicatesRemoved " +
                          "select * from Pair " +
                          "where a.Id != coalesce((select a.Id from PairDuplicatesRemoved#lastevent), -1)" +
                          "  and b.Id != coalesce((select b.Id from PairDuplicatesRemoved#lastevent), -1);\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportSensorEvent(1, "Temperature", "A", 51, 94.5));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportSensorEvent(2, "Temperature", "A", 57, 95.5));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportSensorEvent(3, "Humidity", "B", 29, 67.5));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportSensorEvent(4, "Temperature", "B", 55, 88.0));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(2, theEvent.Get("a.Id"));
                        Assert.AreEqual(4, theEvent.Get("b.Id"));
                    });

                env.SendEventBean(new SupportSensorEvent(5, "Temperature", "B", 65, 85.0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportSensorEvent(6, "Temperature", "B", 49, 87.0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportSensorEvent(7, "Temperature", "A", 51, 99.5));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual(7, theEvent.Get("a.Id"));
                        Assert.AreEqual(6, theEvent.Get("b.Id"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLSubselectSubselectMixMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextOne =
                    "@name('s0') select " +
                    " (select * from SupportSensorEvent#sort(1, measurement desc)) as high, " +
                    " (select * from SupportSensorEvent#sort(1, measurement asc)) as low " +
                    " from SupportSensorEvent";
                env.CompileDeployAddListenerMileZero(stmtTextOne, "s0");

                env.SendEventBean(new SupportSensorEvent(1, "Temp", "Dev1", 68.0, 96.5));
                AssertHighLow(env, 68, 68);

                env.SendEventBean(new SupportSensorEvent(2, "Temp", "Dev2", 70.0, 98.5));
                AssertHighLow(env, 70, 68);

                env.SendEventBean(new SupportSensorEvent(3, "Temp", "Dev2", 65.0, 99.5));
                AssertHighLow(env, 70, 65);

                env.UndeployAll();
            }

            private void AssertHighLow(
                RegressionEnvironment env,
                double highExpected,
                double lowExpected)
            {
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        var high = ((SupportSensorEvent)theEvent.Get("high")).Measurement;
                        var low = ((SupportSensorEvent)theEvent.Get("low")).Measurement;
                        Assert.AreEqual(highExpected, high, 0d);
                        Assert.AreEqual(lowExpected, low, 0d);
                    });
            }
        }

        private class EPLSubselectHavingNoAggWFilterWWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select IntPrimitive from SupportBean(IntPrimitive < 20) #keepall where IntPrimitive > 15 having TheString = 'ID1') as c0 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendS0AndAssert(env, null);
                SendSBAndS0Assert(env, "ID2", 10, null);
                SendSBAndS0Assert(env, "ID1", 11, null);
                SendSBAndS0Assert(env, "ID1", 20, null);
                SendSBAndS0Assert(env, "ID1", 19, 19);

                env.UndeployAll();
            }
        }

        private class EPLSubselectHavingNoAggWWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select IntPrimitive from SupportBean#keepall where IntPrimitive > 15 having TheString = 'ID1') as c0 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendS0AndAssert(env, null);
                SendSBAndS0Assert(env, "ID2", 10, null);
                SendSBAndS0Assert(env, "ID1", 11, null);
                SendSBAndS0Assert(env, "ID1", 20, 20);

                env.UndeployAll();
            }
        }

        private class EPLSubselectHavingNoAggNoFilterNoWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select (select IntPrimitive from SupportBean#keepall having TheString = 'ID1') as c0 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendS0AndAssert(env, null);
                SendSBAndS0Assert(env, "ID2", 10, null);
                SendSBAndS0Assert(env, "ID1", 11, 11);

                env.UndeployAll();
            }
        }

        private static void TrySelectWhereJoined4CoercionBack(
            RegressionEnvironment env,
            AtomicLong milestone,
            string stmtText)
        {
            env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

            SendBean(env, "A", 1, 10, 200, 3000); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "B", 1, 10, 200, 3000);
            SendBean(env, "C", 1, 10, 200, 3000);
            env.AssertEqualsNew("s0", "ids0", null);

            SendBean(env, "S", -1, 11, 201, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "A", 2, 201, 0, 0);
            SendBean(env, "B", 2, 0, 0, 201);
            SendBean(env, "C", 2, 0, 11, 0);
            env.AssertEqualsNew("s0", "ids0", -1);

            SendBean(env, "S", -2, 12, 202, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "A", 3, 202, 0, 0);
            SendBean(env, "B", 3, 0, 0, 202);
            SendBean(env, "C", 3, 0, -1, 0);
            env.AssertEqualsNew("s0", "ids0", null);

            SendBean(env, "S", -3, 13, 203, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "A", 4, 203, 0, 0);
            SendBean(env, "B", 4, 0, 0, 203.0001);
            SendBean(env, "C", 4, 0, 13, 0);
            env.AssertEqualsNew("s0", "ids0", null);

            SendBean(env, "S", -4, 14, 204, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "A", 5, 205, 0, 0);
            SendBean(env, "B", 5, 0, 0, 204);
            SendBean(env, "C", 5, 0, 14, 0);
            env.AssertEqualsNew("s0", "ids0", null);

            env.UndeployAll();
        }

        private static void TrySelectWhereJoined4Coercion(
            RegressionEnvironment env,
            AtomicLong milestone,
            string stmtText)
        {
            env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

            SendBean(env, "A", 1, 10, 200, 3000); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "B", 1, 10, 200, 3000);
            SendBean(env, "C", 1, 10, 200, 3000);
            env.AssertEqualsNew("s0", "ids0", null);

            SendBean(env, "S", -2, 11, 0, 3001);
            SendBean(env, "A", 2, 0, 11, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "B", 2, 0, 0, 11);
            SendBean(env, "C", 2, 3001, 0, 0);
            env.AssertEqualsNew("s0", "ids0", -2);

            SendBean(env, "S", -3, 12, 0, 3002);
            SendBean(env, "A", 3, 0, 12, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "B", 3, 0, 0, 12);
            SendBean(env, "C", 3, 3003, 0, 0);
            env.AssertEqualsNew("s0", "ids0", null);

            SendBean(env, "S", -4, 11, 0, 3003);
            SendBean(env, "A", 4, 0, 0, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "B", 4, 0, 0, 11);
            SendBean(env, "C", 4, 3003, 0, 0);
            env.AssertEqualsNew("s0", "ids0", null);

            SendBean(env, "S", -5, 14, 0, 3004);
            SendBean(env, "A", 5, 0, 14, 0); // intPrimitive, intBoxed, longBoxed, doubleBoxed
            SendBean(env, "B", 5, 0, 0, 11);
            SendBean(env, "C", 5, 3004, 0, 0);
            env.AssertEqualsNew("s0", "ids0", null);

            env.UndeployAll();
        }

        private static void TryJoinFiltered(
            RegressionEnvironment env,
            string stmtText)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            env.SendEventBean(new SupportBean_S0(0, "X"));
            env.SendEventBean(new SupportBean_S1(0, "Y"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S2(1, "ab"));
            env.SendEventBean(new SupportBean_S0(1, "a"));
            env.SendEventBean(new SupportBean_S1(1, "b"));
            env.AssertEventNew(
                "s0",
                theEvent => {
                    Assert.AreEqual(1, theEvent.Get("s0id"));
                    Assert.AreEqual(1, theEvent.Get("s1id"));
                    Assert.AreEqual("ab", theEvent.Get("s2p20"));
                    Assert.AreEqual(null, theEvent.Get("s2p20Prior"));
                    Assert.AreEqual(null, theEvent.Get("s2p20Prev"));
                });

            env.SendEventBean(new SupportBean_S2(2, "qx"));
            env.SendEventBean(new SupportBean_S0(2, "q"));
            env.SendEventBean(new SupportBean_S1(2, "x"));
            env.AssertEventNew(
                "s0",
                theEvent => {
                    Assert.AreEqual(2, theEvent.Get("s0id"));
                    Assert.AreEqual(2, theEvent.Get("s1id"));
                    Assert.AreEqual("qx", theEvent.Get("s2p20"));
                    Assert.AreEqual("ab", theEvent.Get("s2p20Prior"));
                    Assert.AreEqual("ab", theEvent.Get("s2p20Prev"));
                });

            env.UndeployAll();
        }

        private static void RunWherePrevious(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S1(1));
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEqualsNew("s0", "value", null);

            env.SendEventBean(new SupportBean_S1(2));
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertEqualsNew("s0", "value", 1);

            env.SendEventBean(new SupportBean_S1(3));
            env.SendEventBean(new SupportBean_S0(3));
            env.AssertEqualsNew("s0", "value", 2);
        }

        private static void SendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int intBoxed,
            long longBoxed,
            double doubleBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            env.SendEventBean(bean);
        }

        private static void SendSBAndS0Assert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? expected)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            SendS0AndAssert(env, expected);
        }

        private static void SendS0AndAssert(
            RegressionEnvironment env,
            int? expected)
        {
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEqualsNew("s0", "c0", expected);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price,
            long volume)
        {
            return new SupportMarketDataBean(symbol, price, volume, null);
        }

        private static void SendManyArray(
            RegressionEnvironment env,
            string id,
            int[] ints,
            int value)
        {
            env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints).WithValue(value));
        }

        private static void SendIntArrayAndAssert(
            RegressionEnvironment env,
            string id,
            int[] array,
            int value,
            string expected)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array, value));
            env.AssertEqualsNew("s0", "value", expected);
        }

        private static void SendIntArray(
            RegressionEnvironment env,
            string id,
            int[] array)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array));
        }

        private static void SendManyArray(
            RegressionEnvironment env,
            string id,
            int[] ints)
        {
            env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints));
        }

        private static void SendIntArrayAndAssert(
            RegressionEnvironment env,
            string id,
            int[] array,
            string expected)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array));
            env.AssertEqualsNew("s0", "value", expected);
        }
    }
} // end of namespace