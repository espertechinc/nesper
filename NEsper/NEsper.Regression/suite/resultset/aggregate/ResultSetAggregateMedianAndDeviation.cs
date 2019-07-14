///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateMedianAndDeviation
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateStmt());
            execs.Add(new ResultSetAggregateStmtJoinOM());
            execs.Add(new ResultSetAggregateStmtJoin());
            execs.Add(new ResultSetAggregateStmt());
            return execs;
        }

        private static void TryAssertionStmt(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myMedian"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myDistMedian"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myStdev"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myAvedev"));

            SendEvent(env, SYMBOL_DELL, 10);
            AssertEvents(
                env,
                SYMBOL_DELL,
                null,
                null,
                null,
                null,
                10d,
                10d,
                null,
                0d);

            env.MilestoneInc(milestone);

            SendEvent(env, SYMBOL_DELL, 20);
            AssertEvents(
                env,
                SYMBOL_DELL,
                10d,
                10d,
                null,
                0d,
                15d,
                15d,
                7.071067812d,
                5d);

            SendEvent(env, SYMBOL_DELL, 20);
            AssertEvents(
                env,
                SYMBOL_DELL,
                15d,
                15d,
                7.071067812d,
                5d,
                20d,
                15d,
                5.773502692,
                4.444444444444444);

            env.MilestoneInc(milestone);

            SendEvent(env, SYMBOL_DELL, 90);
            AssertEvents(
                env,
                SYMBOL_DELL,
                20d,
                15d,
                5.773502692,
                4.444444444444444,
                20d,
                20d,
                36.96845502d,
                27.5d);

            SendEvent(env, SYMBOL_DELL, 5);
            AssertEvents(
                env,
                SYMBOL_DELL,
                20d,
                20d,
                36.96845502d,
                27.5d,
                20d,
                15d,
                34.71310992d,
                24.4d);

            SendEvent(env, SYMBOL_DELL, 90);
            AssertEvents(
                env,
                SYMBOL_DELL,
                20d,
                15d,
                34.71310992d,
                24.4d,
                20d,
                20d,
                41.53311931d,
                36d);

            env.MilestoneInc(milestone);

            SendEvent(env, SYMBOL_DELL, 30);
            AssertEvents(
                env,
                SYMBOL_DELL,
                20d,
                20d,
                41.53311931d,
                36d,
                30d,
                25d,
                40.24922359d,
                34.4d);
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            double? oldMedian,
            double? oldDistMedian,
            double? oldStdev,
            double? oldAvedev,
            double? newMedian,
            double? newDistMedian,
            double? newStdev,
            double? newAvedev
        )
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
            Assert.AreEqual(oldMedian, oldData[0].Get("myMedian"), "oldData.myMedian wrong");
            Assert.AreEqual(oldDistMedian, oldData[0].Get("myDistMedian"), "oldData.myDistMedian wrong");
            Assert.AreEqual(oldAvedev, oldData[0].Get("myAvedev"), "oldData.myAvedev wrong");

            var oldStdevResult = (double?) oldData[0].Get("myStdev");
            if (oldStdevResult == null) {
                Assert.IsNull(oldStdev);
            }
            else {
                Assert.AreEqual(
                    Math.Round(oldStdev.Value * 1000),
                    Math.Round(oldStdevResult.Value * 1000),
                    "oldData.myStdev wrong");
            }

            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(newMedian, newData[0].Get("myMedian"), "newData.myMedian wrong");
            Assert.AreEqual(newDistMedian, newData[0].Get("myDistMedian"), "newData.myDistMedian wrong");
            Assert.AreEqual(newAvedev, newData[0].Get("myAvedev"), "newData.myAvedev wrong");

            var newStdevResult = (double?) newData[0].Get("myStdev");
            if (newStdevResult == null) {
                Assert.IsNull(newStdev);
            }
            else {
                Assert.AreEqual(
                    Math.Round(newStdev.Value * 1000),
                    Math.Round(newStdevResult.Value * 1000),
                    "newData.myStdev wrong");
            }

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetAggregateStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@Name('s0') select irstream symbol," +
                          "median(all price) as myMedian," +
                          "median(distinct price) as myDistMedian," +
                          "stddev(all price) as myStdev," +
                          "avedev(all price) as myAvedev " +
                          "from SupportMarketDataBean#length(5) " +
                          "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                          "group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionStmt(env, milestone);

                // Test NaN sensitivity
                env.UndeployAll();

                epl = "@Name('s0') select stddev(price) as val from SupportMarketDataBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "A", double.NaN);
                SendEvent(env, "B", double.NaN);
                SendEvent(env, "C", double.NaN);

                env.MilestoneInc(milestone);

                SendEvent(env, "D", 1d);
                SendEvent(env, "E", 2d);
                env.Listener("s0").Reset();

                env.MilestoneInc(milestone);

                SendEvent(env, "F", 3d);
                var result = env.Listener("s0").AssertOneGetNewAndReset().Get("val").AsDouble();
                Assert.IsTrue(double.IsNaN(result));

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateStmtJoinOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("symbol")
                    .Add(Expressions.Median("price"), "myMedian")
                    .Add(Expressions.MedianDistinct("price"), "myDistMedian")
                    .Add(Expressions.Stddev("price"), "myStdev")
                    .Add(Expressions.Avedev("price"), "myAvedev")
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);

                var fromClause = FromClause.Create(
                    FilterStream
                        .Create(typeof(SupportBeanString).Name, "one")
                        .AddView(View.Create("length", Expressions.Constant(100))),
                    FilterStream
                        .Create(typeof(SupportMarketDataBean).Name, "two")
                        .AddView(View.Create("length", Expressions.Constant(5))));
                model.FromClause = fromClause;
                model.WhereClause = Expressions.And()
                    .Add(
                        Expressions.Or()
                            .Add(Expressions.Eq("symbol", "DELL"))
                            .Add(Expressions.Eq("symbol", "IBM"))
                            .Add(Expressions.Eq("symbol", "GE"))
                    )
                    .Add(Expressions.EqProperty("one.TheString", "two.symbol"));
                model.GroupByClause = GroupByClause.Create("symbol");
                model = env.CopyMayFail(model);

                var epl = "select irstream symbol, " +
                          "median(price) as myMedian, " +
                          "median(distinct price) as myDistMedian, " +
                          "stddev(price) as myStdev, " +
                          "avedev(price) as myAvedev " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where (symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\") " +
                          "and one.TheString=two.symbol " +
                          "group by Symbol";
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionStmt(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateStmtJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream symbol," +
                          "median(price) as myMedian," +
                          "median(distinct price) as myDistMedian," +
                          "stddev(price) as myStdev," +
                          "avedev(price) as myAvedev " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                          "       and one.TheString = two.symbol " +
                          "group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionStmt(env, new AtomicLong());

                env.UndeployAll();
            }
        }
    }
} // end of namespace