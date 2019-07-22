///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeWTimeBatch
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowForAllNoJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchRowForAllJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerEventNoJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerEventJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerGroupNoJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerGroupJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchAggrGroupedNoJoin());
            execs.Add(new ResultSetQueryTypeTimeBatchAggrGroupedJoin());
            return execs;
        }

        private static void SendSupportEvent(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, -1));
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            double price,
            long? volume)
        {
            env.SendEventBean(new SupportMarketDataBean(symbol, price, volume, null));
        }

        private static void AssertEvent(
            EventBean theEvent,
            string symbol,
            double? sumPrice,
            long? volume)
        {
            Assert.AreEqual(symbol, theEvent.Get("Symbol"));
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
            Assert.AreEqual(volume, theEvent.Get("Volume"));
        }

        private static void AssertEvent(
            EventBean theEvent,
            string symbol,
            double? sumPrice)
        {
            Assert.AreEqual(symbol, theEvent.Get("Symbol"));
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
        }

        private static void AssertEvent(
            EventBean theEvent,
            double? sumPrice)
        {
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
        }

        internal class ResultSetQueryTypeTimeBatchRowForAllNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select irstream sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec)";
                env.CompileDeploy(stmtText).AddListener("s0");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], 45d);

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], 20d);

                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(1, oldEvents.Length);
                AssertEvent(oldEvents[0], 45d);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchRowForAllJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select irstream sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec) as S0, SupportBean#keepall as S1 where S0.Symbol = S1.TheString";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendSupportEvent(env, "DELL");
                SendSupportEvent(env, "IBM");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], 45d);

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], 20d);

                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(1, oldEvents.Length);
                AssertEvent(oldEvents[0], 45d);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchRowPerEventNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec)";
                env.CompileDeploy(stmtText).AddListener("s0");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(3, newEvents.Length);
                AssertEvent(newEvents[0], "DELL", 45d);
                AssertEvent(newEvents[1], "IBM", 45d);
                AssertEvent(newEvents[2], "DELL", 45d);

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], "IBM", 20d);

                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(3, oldEvents.Length);
                AssertEvent(oldEvents[0], "DELL", 20d);
                AssertEvent(oldEvents[1], "IBM", 20d);
                AssertEvent(oldEvents[2], "DELL", 20d);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchRowPerEventJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec) as S0, SupportBean#keepall as S1 where S0.Symbol = S1.TheString";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendSupportEvent(env, "DELL");
                SendSupportEvent(env, "IBM");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(3, newEvents.Length);
                AssertEvent(newEvents[0], "DELL", 45d);
                AssertEvent(newEvents[1], "IBM", 45d);
                AssertEvent(newEvents[2], "DELL", 45d);

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], "IBM", 20d);

                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(3, oldEvents.Length);
                AssertEvent(oldEvents[0], "DELL", 20d);
                AssertEvent(oldEvents[1], "IBM", 20d);
                AssertEvent(oldEvents[2], "DELL", 20d);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchRowPerGroupNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec) group by Symbol order by Symbol asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(2, newEvents.Length);
                AssertEvent(newEvents[0], "DELL", 30d);
                AssertEvent(newEvents[1], "IBM", 15d);

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(2, newEvents.Length);
                AssertEvent(newEvents[0], "DELL", null);
                AssertEvent(newEvents[1], "IBM", 20d);

                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(2, oldEvents.Length);
                AssertEvent(oldEvents[0], "DELL", 30d);
                AssertEvent(oldEvents[1], "IBM", 15d);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchRowPerGroupJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText = "@Name('s0') select irstream Symbol, sum(Price) as sumPrice " +
                               " from SupportMarketDataBean#time_batch(1 sec) as S0, SupportBean#keepall as S1" +
                               " where S0.Symbol = S1.TheString " +
                               " group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendSupportEvent(env, "DELL");
                SendSupportEvent(env, "IBM");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                var fields = "symbol,sumPrice".SplitCsv();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"DELL", 30d}, new object[] {"IBM", 15d}});

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"DELL", null}, new object[] {"IBM", 20d}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"DELL", 30d}, new object[] {"IBM", 15d}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchAggrGroupedNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@Name('s0') select irstream Symbol, sum(Price) as sumPrice, Volume from SupportMarketDataBean#time_batch(1 sec) group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendMDEvent(env, "DELL", 10, 200L);
                SendMDEvent(env, "IBM", 15, 500L);
                SendMDEvent(env, "DELL", 20, 250L);

                SendTimer(env, 1000);
                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(3, newEvents.Length);
                AssertEvent(newEvents[0], "DELL", 30d, 200L);
                AssertEvent(newEvents[1], "IBM", 15d, 500L);
                AssertEvent(newEvents[2], "DELL", 30d, 250L);

                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);
                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], "IBM", 20d, 600L);
                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(3, oldEvents.Length);
                AssertEvent(oldEvents[0], "DELL", null, 200L);
                AssertEvent(oldEvents[1], "IBM", 20d, 500L);
                AssertEvent(oldEvents[2], "DELL", null, 250L);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeTimeBatchAggrGroupedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText = "@Name('s0') select irstream Symbol, sum(Price) as sumPrice, Volume " +
                               "from SupportMarketDataBean#time_batch(1 sec) as S0, SupportBean#keepall as S1" +
                               " where S0.Symbol = S1.TheString " +
                               " group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendSupportEvent(env, "DELL");
                SendSupportEvent(env, "IBM");

                SendMDEvent(env, "DELL", 10, 200L);
                SendMDEvent(env, "IBM", 15, 500L);
                SendMDEvent(env, "DELL", 20, 250L);

                SendTimer(env, 1000);
                var newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(3, newEvents.Length);
                AssertEvent(newEvents[0], "DELL", 30d, 200L);
                AssertEvent(newEvents[1], "IBM", 15d, 500L);
                AssertEvent(newEvents[2], "DELL", 30d, 250L);

                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);
                newEvents = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newEvents.Length);
                AssertEvent(newEvents[0], "IBM", 20d, 600L);
                var oldEvents = env.Listener("s0").LastOldData;
                Assert.AreEqual(3, oldEvents.Length);
                AssertEvent(oldEvents[0], "DELL", null, 200L);
                AssertEvent(oldEvents[1], "IBM", 20d, 500L);
                AssertEvent(oldEvents[2], "DELL", null, 250L);

                env.UndeployAll();
            }
        }
    }
} // end of namespace