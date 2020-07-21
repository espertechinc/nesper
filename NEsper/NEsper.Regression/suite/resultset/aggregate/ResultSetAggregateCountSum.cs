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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateCountSum
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountSimple());
            execs.Add(new ResultSetAggregateCountPlusStar());
            execs.Add(new ResultSetAggregateCountHaving());
            execs.Add(new ResultSetAggregateSumHaving());
            execs.Add(new ResultSetAggregateCountOneViewOM());
            execs.Add(new ResultSetAggregateGroupByCountNestedAggregationAvg());
            execs.Add(new ResultSetAggregateCountOneViewCompile());
            execs.Add(new ResultSetAggregateCountOneView());
            execs.Add(new ResultSetAggregateCountJoin());
            execs.Add(new ResultSetAggregateCountDistinctGrouped());
            execs.Add(new ResultSetAggregateSumNamedWindowRemoveGroup());
            execs.Add(new ResultSetAggregateCountDistinctMultikeyWArray());
            return execs;
        }

        private static void TryAssertionCount(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("countAll"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("countDistVol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("countVol"));

            SendEvent(env, SYMBOL_DELL, 50L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                0L,
                0L,
                0L,
                SYMBOL_DELL,
                1L,
                1L,
                1L
            );

            SendEvent(env, SYMBOL_DELL, null);
            AssertEvents(
                env,
                SYMBOL_DELL,
                1L,
                1L,
                1L,
                SYMBOL_DELL,
                2L,
                1L,
                1L
            );

            SendEvent(env, SYMBOL_DELL, 25L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                2L,
                1L,
                1L,
                SYMBOL_DELL,
                3L,
                2L,
                2L
            );

            SendEvent(env, SYMBOL_DELL, 25L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                3L,
                2L,
                2L,
                SYMBOL_DELL,
                3L,
                1L,
                2L
            );

            SendEvent(env, SYMBOL_DELL, 25L);
            AssertEvents(
                env,
                SYMBOL_DELL,
                3L,
                1L,
                2L,
                SYMBOL_DELL,
                3L,
                1L,
                3L
            );

            SendEvent(env, SYMBOL_IBM, 1L);
            SendEvent(env, SYMBOL_IBM, null);
            SendEvent(env, SYMBOL_IBM, null);
            SendEvent(env, SYMBOL_IBM, null);
            AssertEvents(
                env,
                SYMBOL_IBM,
                3L,
                1L,
                1L,
                SYMBOL_IBM,
                3L,
                0L,
                0L
            );
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOld,
            long? countAllOld,
            long? countDistVolOld,
            long? countVolOld,
            string symbolNew,
            long? countAllNew,
            long? countDistVolNew,
            long? countVolNew)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
            Assert.AreEqual(countAllOld, oldData[0].Get("countAll"));
            Assert.AreEqual(countDistVolOld, oldData[0].Get("countDistVol"));
            Assert.AreEqual(countVolOld, oldData[0].Get("countVol"));

            Assert.AreEqual(symbolNew, newData[0].Get("Symbol"));
            Assert.AreEqual(countAllNew, newData[0].Get("countAll"));
            Assert.AreEqual(countDistVolNew, newData[0].Get("countDistVol"));
            Assert.AreEqual(countVolNew, newData[0].Get("countVol"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long? volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "f1");
            env.SendEventBean(bean);
        }

        private static void SendEvent(RegressionEnvironment env)
        {
            var bean = new SupportBean("", 1);
            env.SendEventBean(bean);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price)
        {
            return new SupportMarketDataBean(symbol, price, 0L, null);
        }

        private static void SendManyArrayAssert(
            RegressionEnvironment env,
            int[] intOne,
            int[] intTwo,
            long expectedC0,
            long expectedC1)
        {
            env.SendEventBean(new SupportEventWithManyArray("id").WithIntOne(intOne).WithIntTwo(intTwo));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1".SplitCsv(), new object[] {expectedC0, expectedC1});
        }

        internal class ResultSetAggregateCountDistinctMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@name('s0') select count(distinct intOne) as c0, count(distinct {intOne, intTwo}) as c1 from SupportEventWithManyArray#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArrayAssert(env, new int[] {1, 2}, new int[] {1}, 1, 1);
                SendManyArrayAssert(env, new int[] {1, 2}, new int[] {1}, 1, 1);
                SendManyArrayAssert(env, new int[] {1, 3}, new int[] {1}, 2, 2);

                env.Milestone(0);

                SendManyArrayAssert(env, new int[] {1, 4}, new int[] {1}, 3, 3);
                SendManyArrayAssert(env, new int[] {1, 3}, new int[] {2}, 2, 3);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountPlusStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for ESPER-118
                var statementText = "@name('s0') select *, count(*) as cnt from SupportMarketDataBean";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env, "S0", 1L);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual(1L, env.Listener("s0").LastNewData[0].Get("cnt"));
                Assert.AreEqual("S0", env.Listener("s0").LastNewData[0].Get("Symbol"));

                SendEvent(env, "S1", 1L);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual(2L, env.Listener("s0").LastNewData[0].Get("cnt"));
                Assert.AreEqual("S1", env.Listener("s0").LastNewData[0].Get("Symbol"));

                SendEvent(env, "S2", 1L);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual(3L, env.Listener("s0").LastNewData[0].Get("cnt"));
                Assert.AreEqual("S2", env.Listener("s0").LastNewData[0].Get("Symbol"));

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select count(*) as cnt from SupportMarketDataBean#time(1)";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env, "DELL", 1L);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual(1L, env.Listener("s0").LastNewData[0].Get("cnt"));

                SendEvent(env, "DELL", 1L);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual(2L, env.Listener("s0").LastNewData[0].Get("cnt"));

                SendEvent(env, "DELL", 1L);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual(3L, env.Listener("s0").LastNewData[0].Get("cnt"));

                // test invalid distinct
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select count(distinct *) from SupportMarketDataBean",
                    "Failed to validate select-clause expression 'count(distinct *)': Invalid use of the 'distinct' keyword with count and wildcard");

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select irstream sum(IntPrimitive) as mysum from SupportBean having sum(IntPrimitive) = 2";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendEvent(env);
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("mysum"));
                SendEvent(env);
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetOldAndReset().Get("mysum"));

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateSumHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select irstream count(*) as mysum from SupportBean having count(*) = 2";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                SendEvent(env);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("mysum"));
                SendEvent(env);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetOldAndReset().Get("mysum"));

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountOneViewOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add("Symbol")
                    .Add(Expressions.CountStar(), "countAll")
                    .Add(Expressions.CountDistinct("Volume"), "countDistVol")
                    .Add(Expressions.Count("Volume"), "countVol");
                model.FromClause = FromClause
                    .Create(
                        FilterStream.Create(typeof(SupportMarketDataBean).Name)
                            .AddView("length", Expressions.Constant(3)));
                model.WhereClause = Expressions.Or()
                    .Add(Expressions.Eq("Symbol", "DELL"))
                    .Add(Expressions.Eq("Symbol", "IBM"))
                    .Add(Expressions.Eq("Symbol", "GE"));
                model.GroupByClause = GroupByClause.Create("Symbol");
                model = env.CopyMayFail(model);

                var epl = "select irstream Symbol, " +
                          "count(*) as countAll, " +
                          "count(distinct Volume) as countDistVol, " +
                          "count(Volume) as countVol" +
                          " from " + typeof(SupportMarketDataBean).Name + "#length(3) " +
                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
                          "group by Symbol";
                Assert.That(model.ToEPL(), Is.EqualTo(epl));

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionCount(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateGroupByCountNestedAggregationAvg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-328
                var epl =
                    "@name('s0') select Symbol, count(*) as cnt, avg(count(*)) as val from SupportMarketDataBean#length(3)" +
                    "group by Symbol order by Symbol asc";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, SYMBOL_DELL, 50L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "Symbol","cnt","val" },
                    new object[] {"DELL", 1L, 1d});

                SendEvent(env, SYMBOL_DELL, 51L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "Symbol","cnt","val" },
                    new object[] {"DELL", 2L, 1.5d});

                SendEvent(env, SYMBOL_DELL, 52L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "Symbol","cnt","val" },
                    new object[] {"DELL", 3L, 2d});

                SendEvent(env, "IBM", 52L);
                var events = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertProps(
                    events[0],
                    new [] { "Symbol","cnt","val" },
                    new object[] {"DELL", 2L, 2d});
                EPAssertionUtil.AssertProps(
                    events[1],
                    new [] { "Symbol","cnt","val" },
                    new object[] {"IBM", 1L, 1d});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_DELL, 53L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "Symbol","cnt","val" },
                    new object[] {"DELL", 2L, 2.5d});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountOneViewCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol, " +
                          "count(*) as countAll, " +
                          "count(distinct Volume) as countDistVol, " +
                          "count(Volume) as countVol" +
                          " from SupportMarketDataBean#length(3) " +
                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
                          "group by Symbol";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionCount(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol, " +
                          "count(*) as countAll," +
                          "count(distinct Volume) as countDistVol," +
                          "count(all Volume) as countVol" +
                          " from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionCount(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateCountJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol, " +
                          "count(*) as countAll," +
                          "count(distinct Volume) as countDistVol," +
                          "count(Volume) as countVol " +
                          " from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                env.Milestone(0);

                TryAssertionCount(env);

                env.UndeployAll();
            }
        }

        public class ResultSetAggregateCountDistinctGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol, count(distinct Price) as countDistinctPrice " +
                          "from SupportMarketDataBean group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent("ONE", 100));

                env.UndeployAll();
            }
        }

        public class ResultSetAggregateSumNamedWindowRemoveGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","mysum" };
                var epl = "create window MyWindow.win:keepall() as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_A a delete from MyWindow w where w.TheString = a.Id;\n" +
                          "on SupportBean_B delete from MyWindow;\n" +
                          "@name('s0') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 100});

                env.SendEventBean(new SupportBean("B", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 20});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 101));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 201});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 21));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 41});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 201}, new object[] {"B", 41}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_A("A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", null});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"B", 41}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A", 102));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 102});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 102}, new object[] {"B", 41}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_A("B"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", null});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 102}});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("B", 22));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 22});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 102}, new object[] {"B", 22}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace