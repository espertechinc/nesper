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

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;


namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateCountSum
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithCountSimple(execs);
            WithCountPlusStar(execs);
            WithCountHaving(execs);
            WithSumHaving(execs);
            WithCountOneViewOM(execs);
            WithGroupByCountNestedAggregationAvg(execs);
            WithCountOneViewCompile(execs);
            WithCountOneView(execs);
            WithCountJoin(execs);
            WithCountDistinctGrouped(execs);
            WithSumNamedWindowRemoveGroup(execs);
            WithCountDistinctMultikeyWArray(execs);
            WithCountSumInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCountSumInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountSumInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithCountDistinctMultikeyWArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountDistinctMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithSumNamedWindowRemoveGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSumNamedWindowRemoveGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithCountDistinctGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountDistinctGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithCountJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithCountOneView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountOneView());
            return execs;
        }

        public static IList<RegressionExecution> WithCountOneViewCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountOneViewCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupByCountNestedAggregationAvg(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateGroupByCountNestedAggregationAvg());
            return execs;
        }

        public static IList<RegressionExecution> WithCountOneViewOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountOneViewOM());
            return execs;
        }

        public static IList<RegressionExecution> WithSumHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSumHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithCountHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithCountPlusStar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountPlusStar());
            return execs;
        }

        public static IList<RegressionExecution> WithCountSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountSimple());
            return execs;
        }

        private class ResultSetAggregateCountSumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                var message =
                    "Failed to validate select-clause expression 'XXX': Implicit conversion from datatype 'null' to numeric is not allowed for aggregation function '";
                epl = "select avg(null) from SupportBean";
                env.TryInvalidCompile(epl, message.Replace("XXX", "avg(null)"));
                epl = "select avg(distinct null) from SupportBean";
                env.TryInvalidCompile(epl, message.Replace("XXX", "avg(distinct null)"));
                epl = "select median(null) from SupportBean";
                env.TryInvalidCompile(epl, message.Replace("XXX", "median(null)"));
                epl = "select sum(null) from SupportBean";
                env.TryInvalidCompile(epl, message.Replace("XXX", "sum(null)"));
                epl = "select stddev(null) from SupportBean";
                env.TryInvalidCompile(epl, message.Replace("XXX", "stddev(null)"));
            }
        }

        private class ResultSetAggregateCountDistinctMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select count(distinct intOne) as c0, count(distinct {intOne, intTwo}) as c1 from SupportEventWithManyArray#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArrayAssert(env, new int[] { 1, 2 }, new int[] { 1 }, 1, 1);
                SendManyArrayAssert(env, new int[] { 1, 2 }, new int[] { 1 }, 1, 1);
                SendManyArrayAssert(env, new int[] { 1, 3 }, new int[] { 1 }, 2, 2);

                env.Milestone(0);

                SendManyArrayAssert(env, new int[] { 1, 4 }, new int[] { 1 }, 3, 3);
                SendManyArrayAssert(env, new int[] { 1, 3 }, new int[] { 2 }, 2, 3);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountPlusStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test for ESPER-118
                var fields = "Symbol,cnt".SplitCsv();
                var statementText = "@name('s0') select *, count(*) as cnt from SupportMarketDataBean";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env, "S0", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "S0", 1L });

                SendEvent(env, "S1", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "S1", 2L });

                env.Milestone(0);

                SendEvent(env, "S2", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "S2", 3L });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@name('s0') select count(*) as cnt from SupportMarketDataBean#time(1)";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env, "DELL", 1L);
                env.AssertEqualsNew("s0", "cnt", 1L);

                SendEvent(env, "DELL", 1L);
                env.AssertEqualsNew("s0", "cnt", 2L);

                env.Milestone(0);

                SendEvent(env, "DELL", 1L);
                env.AssertEqualsNew("s0", "cnt", 3L);

                // test invalid distinct
                env.TryInvalidCompile(
                    "select count(distinct *) from SupportMarketDataBean",
                    "Failed to validate select-clause expression 'count(distinct *)': Invalid use of the 'distinct' keyword with count and wildcard");

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select irstream sum(IntPrimitive) as mysum from SupportBean having sum(IntPrimitive) = 2";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env);
                env.AssertListenerNotInvoked("s0");
                SendEvent(env);
                env.AssertEqualsNew("s0", "mysum", 2);

                env.Milestone(0);

                SendEvent(env);
                env.AssertEqualsOld("s0", "mysum", 2);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateSumHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@name('s0') select irstream count(*) as mysum from SupportBean having count(*) = 2";
                env.CompileDeploy(statementText).AddListener("s0");

                SendEvent(env);
                env.AssertListenerNotInvoked("s0");
                SendEvent(env);
                env.AssertEqualsNew("s0", "mysum", 2L);

                env.Milestone(0);

                SendEvent(env);
                env.AssertEqualsOld("s0", "mysum", 2L);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountOneViewOM : RegressionExecution
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
                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportMarketDataBean)).AddView("length", Expressions.Constant(3)));
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
                          " from SupportMarketDataBean#length(3) " +
                          "where Symbol=\"DELL\" or Symbol=\"IBM\" or Symbol=\"GE\" " +
                          "group by Symbol";
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionCount(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateGroupByCountNestedAggregationAvg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-328
                var epl =
                    "@name('s0') select Symbol, count(*) as cnt, avg(count(*)) as val from SupportMarketDataBean#length(3)" +
                    "group by Symbol order by Symbol asc";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, SYMBOL_DELL, 50L);
                env.AssertPropsNew("s0", "Symbol,cnt,val".SplitCsv(), new object[] { "DELL", 1L, 1d });

                SendEvent(env, SYMBOL_DELL, 51L);
                env.AssertPropsNew("s0", "Symbol,cnt,val".SplitCsv(), new object[] { "DELL", 2L, 1.5d });

                env.Milestone(1);

                SendEvent(env, SYMBOL_DELL, 52L);
                env.AssertPropsNew("s0", "Symbol,cnt,val".SplitCsv(), new object[] { "DELL", 3L, 2d });

                SendEvent(env, "IBM", 52L);
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    "Symbol,cnt,val".SplitCsv(),
                    new object[][] { new object[] { "DELL", 2L, 2d }, new object[] { "IBM", 1L, 1d } });

                env.Milestone(2);

                SendEvent(env, SYMBOL_DELL, 53L);
                env.AssertPropsNew("s0", "Symbol,cnt,val".SplitCsv(), new object[] { "DELL", 2L, 2.5d });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountOneViewCompile : RegressionExecution
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

                TryAssertionCount(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select irstream Symbol, " +
                          "count(*) as countAll," +
                          "count(distinct Volume) as countDistVol," +
                          "count(all Volume) as countVol" +
                          " from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";

                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                TryAssertionCount(env, milestone);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
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

                env.MilestoneInc(milestone);

                TryAssertionCount(env, milestone);

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
                var fields = "TheString,mysum".SplitCsv();
                var epl = "create window MyWindow.win:keepall() as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_A a delete from MyWindow w where w.TheString = a.Id;\n" +
                          "on SupportBean_B delete from MyWindow;\n" +
                          "@name('s0') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A", 100));
                env.AssertPropsNew("s0", fields, new object[] { "A", 100 });

                env.SendEventBean(new SupportBean("B", 20));
                env.AssertPropsNew("s0", fields, new object[] { "B", 20 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 101));
                env.AssertPropsNew("s0", fields, new object[] { "A", 201 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 21));
                env.AssertPropsNew("s0", fields, new object[] { "B", 41 });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 201 }, new object[] { "B", 41 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_A("A"));
                env.AssertPropsNew("s0", fields, new object[] { "A", null });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "B", 41 } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A", 102));
                env.AssertPropsNew("s0", fields, new object[] { "A", 102 });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 102 }, new object[] { "B", 41 } });

                env.Milestone(4);

                env.SendEventBean(new SupportBean_A("B"));
                env.AssertPropsNew("s0", fields, new object[] { "B", null });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "A", 102 } });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("B", 22));
                env.AssertPropsNew("s0", fields, new object[] { "B", 22 });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 102 }, new object[] { "B", 22 } });

                env.UndeployAll();
            }
        }

        private static void TryAssertionCount(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("countAll"));
                    Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("countDistVol"));
                    Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("countVol"));
                });

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

            env.MilestoneInc(milestone);

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

            env.MilestoneInc(milestone);

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
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;
                    listener.Reset();

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
                });
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
            env.SendEventBean(new SupportEventWithManyArray("Id").WithIntOne(intOne).WithIntTwo(intTwo));
            env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { expectedC0, expectedC1 });
        }
    }
} // end of namespace