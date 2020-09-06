///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
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
            execs.Add(new EPLSubselectHavingNoAggNoFilterNoWhere());
            execs.Add(new EPLSubselectHavingNoAggWWhere());
            execs.Add(new EPLSubselectHavingNoAggWFilterWWhere());
            execs.Add(new EPLSubselectSameEventCompile());
            execs.Add(new EPLSubselectSameEventOM());
            execs.Add(new EPLSubselectSameEvent());
            execs.Add(new EPLSubselectSelectSceneOne());
            execs.Add(new EPLSubselectSelectWildcard());
            execs.Add(new EPLSubselectSelectWildcardNoName());
            execs.Add(new EPLSubselectWhereConstant());
            execs.Add(new EPLSubselectWherePrevious());
            execs.Add(new EPLSubselectWherePreviousOM());
            execs.Add(new EPLSubselectWherePreviousCompile());
            execs.Add(new EPLSubselectSelectWithWhereJoined());
            execs.Add(new EPLSubselectSelectWhereJoined2Streams());
            execs.Add(new EPLSubselectSelectWhereJoined3Streams());
            execs.Add(new EPLSubselectSelectWhereJoined3SceneTwo());
            execs.Add(new EPLSubselectSelectWhereJoined4Coercion());
            execs.Add(new EPLSubselectSelectWhereJoined4BackCoercion());
            execs.Add(new EPLSubselectSelectWithWhere2Subqery());
            execs.Add(new EPLSubselectJoinFilteredOne());
            execs.Add(new EPLSubselectJoinFilteredTwo());
            execs.Add(new EPLSubselectSubselectMixMax());
            execs.Add(new EPLSubselectSubselectPrior());
            execs.Add(new EPLSubselectWhereClauseMultikeyWArrayPrimitive());
            execs.Add(new EPLSubselectWhereClauseMultikeyWArray2Field());
            execs.Add(new EPLSubselectWhereClauseMultikeyWArrayComposite());
            return execs;
        }

        private static void TrySelectWhereJoined4CoercionBack(
            RegressionEnvironment env,
            AtomicLong milestone,
            string stmtText)
        {
            env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

            SendBean(env, "A", 1, 10, 200, 3000); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "B", 1, 10, 200, 3000);
            SendBean(env, "C", 1, 10, 200, 3000);
            Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -1, 11, 201, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "A", 2, 201, 0, 0);
            SendBean(env, "B", 2, 0, 0, 201);
            SendBean(env, "C", 2, 0, 11, 0);
            Assert.AreEqual(-1, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -2, 12, 202, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "A", 3, 202, 0, 0);
            SendBean(env, "B", 3, 0, 0, 202);
            SendBean(env, "C", 3, 0, -1, 0);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -3, 13, 203, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "A", 4, 203, 0, 0);
            SendBean(env, "B", 4, 0, 0, 203.0001);
            SendBean(env, "C", 4, 0, 13, 0);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -4, 14, 204, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "A", 5, 205, 0, 0);
            SendBean(env, "B", 5, 0, 0, 204);
            SendBean(env, "C", 5, 0, 14, 0);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            env.UndeployAll();
        }

        private static void TrySelectWhereJoined4Coercion(
            RegressionEnvironment env,
            AtomicLong milestone,
            string stmtText)
        {
            env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

            SendBean(env, "A", 1, 10, 200, 3000); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "B", 1, 10, 200, 3000);
            SendBean(env, "C", 1, 10, 200, 3000);
            Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -2, 11, 0, 3001);
            SendBean(env, "A", 2, 0, 11, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "B", 2, 0, 0, 11);
            SendBean(env, "C", 2, 3001, 0, 0);
            Assert.AreEqual(-2, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -3, 12, 0, 3002);
            SendBean(env, "A", 3, 0, 12, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "B", 3, 0, 0, 12);
            SendBean(env, "C", 3, 3003, 0, 0);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -4, 11, 0, 3003);
            SendBean(env, "A", 4, 0, 0, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "B", 4, 0, 0, 11);
            SendBean(env, "C", 4, 3003, 0, 0);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            SendBean(env, "S", -5, 14, 0, 3004);
            SendBean(env, "A", 5, 0, 14, 0); // IntPrimitive, IntBoxed, LongBoxed, DoubleBoxed
            SendBean(env, "B", 5, 0, 0, 11);
            SendBean(env, "C", 5, 3004, 0, 0);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

            env.UndeployAll();
        }

        private static void TryJoinFiltered(
            RegressionEnvironment env,
            string stmtText)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");

            env.SendEventBean(new SupportBean_S0(0, "X"));
            env.SendEventBean(new SupportBean_S1(0, "Y"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(1, "ab"));
            env.SendEventBean(new SupportBean_S0(1, "a"));
            env.SendEventBean(new SupportBean_S1(1, "b"));
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(1, theEvent.Get("S0Id"));
            Assert.AreEqual(1, theEvent.Get("S1Id"));
            Assert.AreEqual("ab", theEvent.Get("S2P20"));
            Assert.AreEqual(null, theEvent.Get("S2P20Prior"));
            Assert.AreEqual(null, theEvent.Get("S2P20Prev"));

            env.SendEventBean(new SupportBean_S2(2, "qx"));
            env.SendEventBean(new SupportBean_S0(2, "q"));
            env.SendEventBean(new SupportBean_S1(2, "x"));
            theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(2, theEvent.Get("S0Id"));
            Assert.AreEqual(2, theEvent.Get("S1Id"));
            Assert.AreEqual("qx", theEvent.Get("S2P20"));
            Assert.AreEqual("ab", theEvent.Get("S2P20Prior"));
            Assert.AreEqual("ab", theEvent.Get("S2P20Prev"));

            env.UndeployAll();
        }

        private static void RunWherePrevious(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S1(1));
            env.SendEventBean(new SupportBean_S0(0));
            Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));

            env.SendEventBean(new SupportBean_S1(2));
            env.SendEventBean(new SupportBean_S0(2));
            Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));

            env.SendEventBean(new SupportBean_S1(3));
            env.SendEventBean(new SupportBean_S0(3));
            Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));
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
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
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
            String id,
            int[] ints,
            int value)
        {
            env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints).WithValue(value));
        }

        private static void SendIntArrayAndAssert(
            RegressionEnvironment env,
            String id,
            int[] array,
            int value,
            String expected)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array, value));
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));
        }

        private static void SendIntArray(
            RegressionEnvironment env,
            String id,
            int[] array)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array));
        }

        private static void SendManyArray (RegressionEnvironment env, String id, int[] ints) {
            env.SendEventBean (new SupportEventWithManyArray (id).WithIntOne (ints));
        }

        private static void SendIntArrayAndAssert(
            RegressionEnvironment env,
            String id,
            int[] array,
            String expected)
        {
            env.SendEventBean(new SupportEventWithIntArray(id, array));
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));
        }

        private static void SendManyArrayAndAssert(
            RegressionEnvironment env,
            String id,
            int[] intOne,
            int[] intTwo,
            String expected)
        {
            env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(intOne).WithIntTwo(intTwo));
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));
        }

        internal class EPLSubselectWhereClauseMultikeyWArrayComposite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select (select Id from SupportEventWithManyArray#keepall as sm " +
                             "where sm.IntOne = se.Array and sm.Value > se.Value) as Value from SupportEventWithIntArray as se";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "MA1", new int[] {1, 2}, 100);
                SendManyArray(env, "MA2", new int[] {1, 2}, 200);
                SendManyArray(env, "MA3", new int[] {1}, 300);
                SendManyArray(env, "MA4", new int[] {1, 2}, 400);

                env.Milestone(0);

                SendIntArrayAndAssert(env, "IA2", new int[] {1, 2}, 250, "MA4");
                SendIntArrayAndAssert(env, "IA3", new int[] {1, 2}, 0, null);
                SendIntArrayAndAssert(env, "IA4", new int[] {1}, 299, "MA3");
                SendIntArrayAndAssert(env, "IA5", new int[] {1, 2}, 500, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWhereClauseMultikeyWArray2Field : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select (select Id from SupportEventWithManyArray#keepall as sm " +
                             "where sm.IntOne = se.Array and sm.Value = se.Value) as Value from SupportEventWithIntArray as se";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "MA1", new int[] {1, 2}, 10);
                SendManyArray(env, "MA2", new int[] {1, 2}, 11);
                SendManyArray(env, "MA3", new int[] {1}, 12);

                env.Milestone(0);

                SendIntArrayAndAssert(env, "IA1", new int[] {1}, 12, "MA3");
                SendIntArrayAndAssert(env, "IA2", new int[] {1, 2}, 11, "MA2");
                SendIntArrayAndAssert(env, "IA3", new int[] {1, 2}, 10, "MA1");
                SendIntArrayAndAssert(env, "IA4", new int[] {1}, 10, null);
                SendIntArrayAndAssert(env, "IA5", new int[] {1, 2}, 12, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWhereClauseMultikeyWArrayPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "@Name('s0') select (select Id from SupportEventWithManyArray#keepall as sm where sm.IntOne = se.Array) as Value from SupportEventWithIntArray as se";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "MA1", new int[] {1, 2});
                SendIntArrayAndAssert(env, "IA1", new int[] {1, 2}, "MA1");

                SendManyArray(env, "MA2", new int[] {1, 2});
                SendManyArray(env, "MA3", new int[] {1});
                SendManyArray(env, "MA4", new int[] { });
                SendManyArray(env, "MA5", null);

                env.Milestone(0);

                SendIntArrayAndAssert(env, "IA2", new int[] { }, "MA4");
                SendIntArrayAndAssert(env, "IA3", new int[] {1}, "MA3");
                SendIntArrayAndAssert(env, "IA4", null, "MA5");
                SendIntArrayAndAssert(env, "IA5", new int[] {1, 2}, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSameEventCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S1";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, result.Get("events1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSameEventOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.CreateWildcard();
                subquery.FromClause =
                    FromClause.Create(
                        FilterStream.Create("SupportBean_S1")
                            .AddView(View.Create("length", Expressions.Constant(1000))));

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S1"));
                model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "events1");
                model = env.CopyMayFail(model);

                var stmtText = "select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S1";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, result.Get("events1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSameEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S1";
                env.CompileDeploy(stmtText).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, result.Get("events1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select * from SupportBean_S1#length(1000)) as events1 from SupportBean_S0";
                env.CompileDeploy(stmtText).AddListener("s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("events1"));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.SendEventBean(new SupportBean_S0(0));
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, result.Get("events1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWildcardNoName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select (select * from SupportBean_S1#length(1000)) from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                var type = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("subselect_1"));

                object theEvent = new SupportBean_S1(-1, "Y");
                env.SendEventBean(theEvent);
                env.SendEventBean(new SupportBean_S0(0));
                var result = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreSame(theEvent, result.Get("subselect_1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWhereConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // single-column constant
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1#length(1000) where P10='X') as ids1 from SupportBean_S0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S1(-1, "Y"));
                env.SendEventBean(new SupportBean_S0(0));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.SendEventBean(new SupportBean_S1(1, "X"));
                env.SendEventBean(new SupportBean_S1(2, "Y"));
                env.SendEventBean(new SupportBean_S1(3, "Z"));

                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.SendEventBean(new SupportBean_S1(2, "X"));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));
                env.UndeployAll();

                // two-column constant
                stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1#length(1000) where P10='X' and P11='Y') as ids1 from SupportBean_S0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S1(1, "X", "Y"));
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));
                env.UndeployAll();

                // single range
                stmtText =
                    "@Name('s0') select (select TheString from SupportBean#lastevent where IntPrimitive between 10 and 20) as ids1 from SupportBean_S0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("E1", 15));
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWherePrevious : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select prev(1, Id) from SupportBean_S1#length(1000) where Id=S0.Id) as Value from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                RunWherePrevious(env);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectWherePreviousOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subquery = new EPStatementObjectModel();
                subquery.SelectClause = SelectClause.Create().Add(Expressions.Previous(1, "Id"));
                subquery.FromClause =
                    FromClause.Create(
                        FilterStream.Create("SupportBean_S1")
                            .AddView(View.Create("length", Expressions.Constant(1000))));
                subquery.WhereClause = Expressions.EqProperty("Id", "S0.Id");

                var model = new EPStatementObjectModel();
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0", "S0"));
                model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "Value");
                model = env.CopyMayFail(model);

                var stmtText =
                    "select (select prev(1,Id) from SupportBean_S1#length(1000) where Id=S0.Id) as Value from SupportBean_S0 as S0";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0").Milestone(0);

                RunWherePrevious(env);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectWherePreviousCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select prev(1,Id) from SupportBean_S1#length(1000) where Id=S0.Id) as Value from SupportBean_S0 as S0";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0").Milestone(0);

                RunWherePrevious(env);

                env.UndeployAll();
            }
        }

        public class EPLSubselectSelectSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream S0.Price as S0Price, " +
                           " (select Price from SupportMarketDataBean(Symbol='S1')#length(10) S1" +
                           " where S0.Volume = S1.Volume) as S1Price " +
                           " from  SupportMarketDataBean(Symbol='S0')#length(2) S0";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S0", 100, 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new[] {"S0Price", "S1Price"},
                    new[] {new object[] {100.0, null}});
                Assert.AreEqual(0, env.Listener("s0").OldDataListFlattened.Length);
                env.Listener("s0").Reset();

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S1", -10, 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S0", 200, 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new[] {"S0Price", "S1Price"},
                    new[] {new object[] {200.0, -10.0}});
                Assert.AreEqual(0, env.Listener("s0").OldDataListFlattened.Length);
                env.Listener("s0").Reset();

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", -20, 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S0", 300, 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new[] {"S0Price", "S1Price"},
                    new[] {new object[] {300.0, -20.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new[] {"S0Price", "S1Price"},
                    new[] {new object[] {100.0, null}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWithWhereJoined : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S1#length(1000) where P10=S0.P00) as ids1 from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(0));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.SendEventBean(new SupportBean_S1(1, "X"));
                env.SendEventBean(new SupportBean_S1(2, "Y"));
                env.SendEventBean(new SupportBean_S1(3, "Z"));

                env.SendEventBean(new SupportBean_S0(0));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.SendEventBean(new SupportBean_S0(0, "X"));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));
                env.SendEventBean(new SupportBean_S0(0, "Y"));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));
                env.SendEventBean(new SupportBean_S0(0, "Z"));
                Assert.AreEqual(3, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));
                env.SendEventBean(new SupportBean_S0(0, "A"));
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("ids1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWhereJoined2Streams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S0#length(1000) where P00=S1.P10 and P00=S2.P20) as ids0 from SupportBean_S1#keepall as S1, SupportBean_S2#keepall as S2 where S1.Id = S2.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10, "s0_1"));
                env.SendEventBean(new SupportBean_S2(10, "s0_1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S0(99, "s0_1"));
                env.SendEventBean(new SupportBean_S1(11, "s0_1"));
                env.SendEventBean(new SupportBean_S2(11, "s0_1"));
                Assert.AreEqual(99, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWhereJoined3Streams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S0#length(1000) where P00=S1.P10 and P00=S3.P30) as ids0 " +
                    "from SupportBean_S1#keepall as S1, SupportBean_S2#keepall as S2, SupportBean_S3#keepall as S3 where S1.Id = S2.Id and S2.Id = S3.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10, "s0_1"));
                env.SendEventBean(new SupportBean_S2(10, "s0_1"));
                env.SendEventBean(new SupportBean_S3(10, "s0_1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S0(99, "s0_1"));
                env.SendEventBean(new SupportBean_S1(11, "s0_1"));
                env.SendEventBean(new SupportBean_S2(11, "xxx"));
                env.SendEventBean(new SupportBean_S3(11, "s0_1"));
                Assert.AreEqual(99, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S0(98, "s0_2"));
                env.SendEventBean(new SupportBean_S1(12, "s0_x"));
                env.SendEventBean(new SupportBean_S2(12, "s0_2"));
                env.SendEventBean(new SupportBean_S3(12, "s0_1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S1(13, "s0_2"));
                env.SendEventBean(new SupportBean_S2(13, "s0_2"));
                env.SendEventBean(new SupportBean_S3(13, "s0_x"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S1(14, "s0_2"));
                env.SendEventBean(new SupportBean_S2(14, "xx"));
                env.SendEventBean(new SupportBean_S3(14, "s0_2"));
                Assert.AreEqual(98, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWhereJoined3SceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select (select Id from SupportBean_S0#length(1000) where P00=S1.P10 and P00=S3.P30 and P00=S2.P20) as ids0 " +
                    "from SupportBean_S1#keepall as S1, SupportBean_S2#keepall as S2, SupportBean_S3#keepall as S3 where S1.Id = S2.Id and S2.Id = S3.Id";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S1(10, "s0_1"));
                env.SendEventBean(new SupportBean_S2(10, "s0_1"));
                env.SendEventBean(new SupportBean_S3(10, "s0_1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S0(99, "s0_1"));
                env.SendEventBean(new SupportBean_S1(11, "s0_1"));
                env.SendEventBean(new SupportBean_S2(11, "xxx"));
                env.SendEventBean(new SupportBean_S3(11, "s0_1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S0(98, "s0_2"));
                env.SendEventBean(new SupportBean_S1(12, "s0_x"));
                env.SendEventBean(new SupportBean_S2(12, "s0_2"));
                env.SendEventBean(new SupportBean_S3(12, "s0_1"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S1(13, "s0_2"));
                env.SendEventBean(new SupportBean_S2(13, "s0_2"));
                env.SendEventBean(new SupportBean_S3(13, "s0_x"));
                Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.SendEventBean(new SupportBean_S1(14, "s0_2"));
                env.SendEventBean(new SupportBean_S2(14, "s0_2"));
                env.SendEventBean(new SupportBean_S3(14, "s0_2"));
                Assert.AreEqual(98, env.Listener("s0").AssertOneGetNewAndReset().Get("ids0"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSelectWhereJoined4Coercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtText = "@Name('s0') select " +
                               "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                               "  where IntBoxed=S1.LongBoxed and " +
                               "IntBoxed=S2.DoubleBoxed and " +
                               "DoubleBoxed=S3.IntBoxed" +
                               ") as ids0 from " +
                               "SupportBean(TheString='A')#keepall as S1, " +
                               "SupportBean(TheString='B')#keepall as S2, " +
                               "SupportBean(TheString='C')#keepall as S3 " +
                               "where S1.IntPrimitive = S2.IntPrimitive and S2.IntPrimitive = S3.IntPrimitive";
                TrySelectWhereJoined4Coercion(env, milestone, stmtText);

                stmtText = "@Name('s0') select " +
                           "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                           "  where DoubleBoxed=S3.IntBoxed and " +
                           "IntBoxed=S2.DoubleBoxed and " +
                           "IntBoxed=S1.LongBoxed" +
                           ") as ids0 from " +
                           "SupportBean(TheString='A')#keepall as S1, " +
                           "SupportBean(TheString='B')#keepall as S2, " +
                           "SupportBean(TheString='C')#keepall as S3 " +
                           "where S1.IntPrimitive = S2.IntPrimitive and S2.IntPrimitive = S3.IntPrimitive";
                TrySelectWhereJoined4Coercion(env, milestone, stmtText);

                stmtText = "@Name('s0') select " +
                           "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                           "  where DoubleBoxed=S3.IntBoxed and " +
                           "IntBoxed=S1.LongBoxed and " +
                           "IntBoxed=S2.DoubleBoxed" +
                           ") as ids0 from " +
                           "SupportBean(TheString='A')#keepall as S1, " +
                           "SupportBean(TheString='B')#keepall as S2, " +
                           "SupportBean(TheString='C')#keepall as S3 " +
                           "where S1.IntPrimitive = S2.IntPrimitive and S2.IntPrimitive = S3.IntPrimitive";
                TrySelectWhereJoined4Coercion(env, milestone, stmtText);
            }
        }

        internal class EPLSubselectSelectWhereJoined4BackCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtText = "@Name('s0') select " +
                               "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                               "  where LongBoxed=S1.IntBoxed and " +
                               "LongBoxed=S2.DoubleBoxed and " +
                               "IntBoxed=S3.LongBoxed" +
                               ") as ids0 from " +
                               "SupportBean(TheString='A')#keepall as S1, " +
                               "SupportBean(TheString='B')#keepall as S2, " +
                               "SupportBean(TheString='C')#keepall as S3 " +
                               "where S1.IntPrimitive = S2.IntPrimitive and S2.IntPrimitive = S3.IntPrimitive";
                TrySelectWhereJoined4CoercionBack(env, milestone, stmtText);

                stmtText = "@Name('s0') select " +
                           "(select IntPrimitive from SupportBean(TheString='S')#length(1000) " +
                           "  where LongBoxed=S2.DoubleBoxed and " +
                           "IntBoxed=S3.LongBoxed and " +
                           "LongBoxed=S1.IntBoxed " +
                           ") as ids0 from " +
                           "SupportBean(TheString='A')#keepall as S1, " +
                           "SupportBean(TheString='B')#keepall as S2, " +
                           "SupportBean(TheString='C')#keepall as S3 " +
                           "where S1.IntPrimitive = S2.IntPrimitive and S2.IntPrimitive = S3.IntPrimitive";
                TrySelectWhereJoined4CoercionBack(env, milestone, stmtText);
            }
        }

        internal class EPLSubselectSelectWithWhere2Subqery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Id from SupportBean_S0 as S0 where " +
                               " Id = (select Id from SupportBean_S1#length(1000) where S0.Id = Id) or Id = (select Id from SupportBean_S2#length(1000) where S0.Id = Id)";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(1));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.SendEventBean(new SupportBean_S2(2));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.SendEventBean(new SupportBean_S0(3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(3));
                env.SendEventBean(new SupportBean_S0(3));
                Assert.AreEqual(3, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectJoinFilteredOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id as S0Id, S1.Id as S1Id, " +
                               "(select P20 from SupportBean_S2#length(1000) where Id=S0.Id) as S2P20, " +
                               "(select prior(1, P20) from SupportBean_S2#length(1000) where Id=S0.Id) as S2P20Prior, " +
                               "(select prev(1, P20) from SupportBean_S2#length(10) where Id=S0.Id) as S2P20Prev " +
                               "from SupportBean_S0#keepall as S0, SupportBean_S1#keepall as S1 " +
                               "where S0.Id = S1.Id and P00||P10 = (select P20 from SupportBean_S2#length(1000) where Id=S0.Id)";
                TryJoinFiltered(env, stmtText);
            }
        }

        internal class EPLSubselectJoinFilteredTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id as S0Id, S1.Id as S1Id, " +
                               "(select P20 from SupportBean_S2#length(1000) where Id=S0.Id) as S2P20, " +
                               "(select prior(1, P20) from SupportBean_S2#length(1000) where Id=S0.Id) as S2P20Prior, " +
                               "(select prev(1, P20) from SupportBean_S2#length(10) where Id=S0.Id) as S2P20Prev " +
                               "from SupportBean_S0#keepall as S0, SupportBean_S1#keepall as S1 " +
                               "where S0.Id = S1.Id and (select S0.P00||S1.P10 = P20 from SupportBean_S2#length(1000) where Id=S0.Id)";
                TryJoinFiltered(env, stmtText);
            }
        }

        internal class EPLSubselectSubselectPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "insert into Pair " +
                          "select * from SupportSensorEvent(Device='A')#lastevent as a, SupportSensorEvent(Device='B')#lastevent as b " +
                          "where a.Type = b.Type;\n" +
                          "" +
                          "insert into PairDuplicatesRemoved select * from Pair(1=2);\n" +
                          "" +
                          "@Name('s0') insert into PairDuplicatesRemoved " +
                          "select * from Pair " +
                          "where a.Id != coalesce((select a.Id from PairDuplicatesRemoved#lastevent), -1)" +
                          "  and b.Id != coalesce((select b.Id from PairDuplicatesRemoved#lastevent), -1);\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportSensorEvent(1, "Temperature", "A", 51, 94.5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportSensorEvent(2, "Temperature", "A", 57, 95.5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportSensorEvent(3, "HumIdity", "B", 29, 67.5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportSensorEvent(4, "Temperature", "B", 55, 88.0));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(2, theEvent.Get("a.Id"));
                Assert.AreEqual(4, theEvent.Get("b.Id"));

                env.SendEventBean(new SupportSensorEvent(5, "Temperature", "B", 65, 85.0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportSensorEvent(6, "Temperature", "B", 49, 87.0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportSensorEvent(7, "Temperature", "A", 51, 99.5));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(7, theEvent.Get("a.Id"));
                Assert.AreEqual(6, theEvent.Get("b.Id"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSubselectMixMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtTextOne =
                    "@Name('s0') select " +
                    " (select * from SupportSensorEvent#sort(1, Measurement desc)) as high, " +
                    " (select * from SupportSensorEvent#sort(1, Measurement asc)) as low " +
                    " from SupportSensorEvent";
                env.CompileDeployAddListenerMileZero(stmtTextOne, "s0");

                env.SendEventBean(new SupportSensorEvent(1, "Temp", "Dev1", 68.0, 96.5));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
                Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);

                env.SendEventBean(new SupportSensorEvent(2, "Temp", "Dev2", 70.0, 98.5));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(70.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
                Assert.AreEqual(68.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);

                env.SendEventBean(new SupportSensorEvent(3, "Temp", "Dev2", 65.0, 99.5));
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(70.0, ((SupportSensorEvent) theEvent.Get("high")).Measurement);
                Assert.AreEqual(65.0, ((SupportSensorEvent) theEvent.Get("low")).Measurement);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectHavingNoAggWFilterWWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select IntPrimitive from SupportBean(IntPrimitive < 20) #keepall where IntPrimitive > 15 having TheString = 'ID1') as c0 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendS0AndAssert(env, null);
                SendSBAndS0Assert(env, "ID2", 10, null);
                SendSBAndS0Assert(env, "ID1", 11, null);
                SendSBAndS0Assert(env, "ID1", 20, null);
                SendSBAndS0Assert(env, "ID1", 19, 19);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectHavingNoAggWWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select IntPrimitive from SupportBean#keepall where IntPrimitive > 15 having TheString = 'ID1') as c0 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendS0AndAssert(env, null);
                SendSBAndS0Assert(env, "ID2", 10, null);
                SendSBAndS0Assert(env, "ID1", 11, null);
                SendSBAndS0Assert(env, "ID1", 20, 20);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectHavingNoAggNoFilterNoWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select IntPrimitive from SupportBean#keepall having TheString = 'ID1') as c0 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendS0AndAssert(env, null);
                SendSBAndS0Assert(env, "ID2", 10, null);
                SendSBAndS0Assert(env, "ID1", 11, 11);

                env.UndeployAll();
            }
        }
    }
} // end of namespace