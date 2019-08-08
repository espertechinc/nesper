///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeHaving
    {
        private const string SYMBOL_DELL = "DELL";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeHavingWildcardSelect());
            execs.Add(new ResultSetQueryTypeStatementOM());
            execs.Add(new ResultSetQueryTypeStatement());
            execs.Add(new ResultSetQueryTypeStatementJoin());
            execs.Add(new ResultSetQueryTypeSumHavingNoAggregatedProp());
            execs.Add(new ResultSetQueryTypeNoAggregationJoinHaving());
            execs.Add(new ResultSetQueryTypeNoAggregationJoinWhere());
            execs.Add(new ResultSetQueryTypeSubstreamSelectHaving());
            execs.Add(new ResultSetQueryTypeHavingSum());
            execs.Add(new ResultSetQueryTypeHavingSumIStream());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("Price"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("avgPrice"));

            SendEvent(env, SYMBOL_DELL, 10);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, SYMBOL_DELL, 5);
            AssertNewEvents(env, SYMBOL_DELL, 5d, 7.5d);

            env.Milestone(0);

            SendEvent(env, SYMBOL_DELL, 15);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, SYMBOL_DELL, 8); // avg = (10 + 5 + 15 + 8) / 4 = 38/4=9.5
            AssertNewEvents(env, SYMBOL_DELL, 8d, 9.5d);

            SendEvent(env, SYMBOL_DELL, 10); // avg = (10 + 5 + 15 + 8 + 10) / 5 = 48/5=9.5
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(1);

            SendEvent(env, SYMBOL_DELL, 6); // avg = (5 + 15 + 8 + 10 + 6) / 5 = 44/5=8.8
            // no old event posted, old event falls above current avg price
            AssertNewEvents(env, SYMBOL_DELL, 6d, 8.8d);

            SendEvent(env, SYMBOL_DELL, 12); // avg = (15 + 8 + 10 + 6 + 12) / 5 = 51/5=10.2
            AssertOldEvents(env, SYMBOL_DELL, 5d, 10.2d);
        }

        private static void AssertNewEvents(
            RegressionEnvironment env,
            string symbol,
            double? newPrice,
            double? newAvgPrice
        )
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(newPrice, newData[0].Get("Price"));
            Assert.AreEqual(newAvgPrice, newData[0].Get("avgPrice"));

            env.Listener("s0").Reset();
        }

        private static void AssertOldEvents(
            RegressionEnvironment env,
            string symbol,
            double? oldPrice,
            double? oldAvgPrice
        )
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);

            Assert.AreEqual(symbol, oldData[0].Get("Symbol"));
            Assert.AreEqual(oldPrice, oldData[0].Get("Price"));
            Assert.AreEqual(oldAvgPrice, oldData[0].Get("avgPrice"));

            env.Listener("s0").Reset();
        }

        private static void RunNoAggregationJoin(
            RegressionEnvironment env,
            string filterClause)
        {
            var epl =
                "@Name('s0') select irstream a.Price as aPrice, b.Price as bPrice, Math.Max(a.Price, b.Price) - Math.min(a.Price, b.Price) as spread " +
                "from SupportMarketDataBean(Symbol='SYM1')#length(1) as a, " +
                "SupportMarketDataBean(Symbol='SYM2')#length(1) as b " +
                filterClause +
                " Math.Max(a.Price, b.Price) - Math.min(a.Price, b.Price) >= 1.4";
            env.CompileDeploy(epl).AddListener("s0");

            SendPriceEvent(env, "SYM1", 20);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(0);

            SendPriceEvent(env, "SYM2", 10);
            AssertNewSpreadEvent(env, 20, 10, 10);

            SendPriceEvent(env, "SYM2", 20);
            AssertOldSpreadEvent(env, 20, 10, 10);

            env.Milestone(1);

            SendPriceEvent(env, "SYM2", 20);
            SendPriceEvent(env, "SYM2", 20);
            SendPriceEvent(env, "SYM1", 20);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendPriceEvent(env, "SYM1", 18.7);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendPriceEvent(env, "SYM2", 20);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(2);

            SendPriceEvent(env, "SYM1", 18.5);
            AssertNewSpreadEvent(env, 18.5, 20, 1.5d);

            SendPriceEvent(env, "SYM2", 16);
            AssertOldNewSpreadEvent(env, 18.5, 20, 1.5d, 18.5, 16, 2.5d);

            env.Milestone(3);

            SendPriceEvent(env, "SYM1", 12);
            AssertOldNewSpreadEvent(env, 18.5, 16, 2.5d, 12, 16, 4);

            env.UndeployAll();
        }

        private static void AssertOldNewSpreadEvent(
            RegressionEnvironment env,
            double oldaprice,
            double oldbprice,
            double oldspread,
            double newaprice,
            double newbprice,
            double newspread)
        {
            Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
            Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
            Assert.AreEqual(1, env.Listener("s0").NewDataList.Count); // since event null is put into the list
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);

            var oldEvent = env.Listener("s0").LastOldData[0];
            var newEvent = env.Listener("s0").LastNewData[0];

            CompareSpreadEvent(oldEvent, oldaprice, oldbprice, oldspread);
            CompareSpreadEvent(newEvent, newaprice, newbprice, newspread);

            env.Listener("s0").Reset();
        }

        private static void AssertOldSpreadEvent(
            RegressionEnvironment env,
            double aprice,
            double bprice,
            double spread)
        {
            var listener = env.Listener("s0");
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual(1, listener.NewDataList.Count); // since event null is put into the list
            Assert.IsNull(listener.LastNewData);

            var theEvent = listener.LastOldData[0];

            CompareSpreadEvent(theEvent, aprice, bprice, spread);
            listener.Reset();
        }

        private static void AssertNewSpreadEvent(
            RegressionEnvironment env,
            double aprice,
            double bprice,
            double spread)
        {
            var listener = env.Listener("s0");
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.IsNull(listener.LastOldData);

            var theEvent = listener.LastNewData[0];
            CompareSpreadEvent(theEvent, aprice, bprice, spread);
            listener.Reset();
        }

        private static void CompareSpreadEvent(
            EventBean theEvent,
            double aprice,
            double bprice,
            double spread)
        {
            Assert.AreEqual(aprice, theEvent.Get("aPrice"));
            Assert.AreEqual(bprice, theEvent.Get("bPrice"));
            Assert.AreEqual(spread, theEvent.Get("spread"));
        }

        private static void SendPriceEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            env.SendEventBean(new SupportMarketDataBean(symbol, price, -1L, null));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetQueryTypeHavingWildcardSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * " +
                          "from SupportBean#length_batch(2) " +
                          "where IntPrimitive>0 " +
                          "having count(*)=2";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 1));
                env.SendEventBean(new SupportBean("E4", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 0));
                env.SendEventBean(new SupportBean("E4", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeStatementOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Symbol", "Price")
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add(Expressions.Avg("Price"), "avgPrice");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportMarketDataBean).Name).AddView("length", Expressions.Constant(5)));
                model.HavingClause = Expressions.Lt(Expressions.Property("Price"), Expressions.Avg("Price"));
                model = env.CopyMayFail(model);

                var epl = "select irstream Symbol, Price, avg(Price) as avgPrice " +
                          "from SupportMarketDataBean#length(5) " +
                          "having Price<avg(Price)";
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, Price, avg(Price) as avgPrice " +
                          "from SupportMarketDataBean#length(5) " +
                          "having Price < avg(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeStatementJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, Price, avg(Price) as avgPrice " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where one.TheString = two.Symbol " +
                          "having Price < avg(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumHavingNoAggregatedProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, Price, avg(Price) as avgPrice " +
                          "from SupportMarketDataBean#length(5) as two " +
                          "having Volume < avg(Price)";
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        internal class ResultSetQueryTypeNoAggregationJoinHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunNoAggregationJoin(env, "having");
            }
        }

        internal class ResultSetQueryTypeNoAggregationJoinWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunNoAggregationJoin(env, "where");
            }
        }

        internal class ResultSetQueryTypeSubstreamSelectHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') insert into MyStream select quote.* from SupportBean#length(14) quote having avg(IntPrimitive) >= 3\n";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean("abc", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("abc", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("abc", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("abc", 5));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeHavingSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select irstream sum(myEvent.IntPrimitive) as mysum from pattern [every myEvent=SupportBean] having sum(myEvent.IntPrimitive) = 2";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, 1);
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("mysum"));

                env.Milestone(0);

                SendEvent(env, 1);
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetOldAndReset().Get("mysum"));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeHavingSumIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select istream sum(myEvent.IntPrimitive) as mysum from pattern [every myEvent=SupportBean" +
                    "] having sum(myEvent.IntPrimitive) = 2";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendEvent(env, 1);
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("mysum"));

                SendEvent(env, 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace