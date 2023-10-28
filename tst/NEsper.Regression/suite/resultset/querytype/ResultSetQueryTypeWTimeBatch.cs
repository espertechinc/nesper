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
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithRowForAllNoJoin(execs);
            WithRowForAllJoin(execs);
            WithRowPerEventNoJoin(execs);
            WithRowPerEventJoin(execs);
            WithRowPerGroupNoJoin(execs);
            WithRowPerGroupJoin(execs);
            WithAggrGroupedNoJoin(execs);
            WithAggrGroupedJoin(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAggrGroupedJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchAggrGroupedJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithAggrGroupedNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchAggrGroupedNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerGroupJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerGroupNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerEventJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowPerEventNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAllJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowForAllJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAllNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeTimeBatchRowForAllNoJoin());
            return execs;
        }

        private class ResultSetQueryTypeTimeBatchRowForAllNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select irstream sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec)";
                env.CompileDeploy(stmtText).AddListener("s0");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], 45d);
                    });

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], 20d);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(1, oldEvents.Length);
                        AssertEvent(oldEvents[0], 45d);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchRowForAllJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select irstream sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec) as S0, SupportBean#keepall as S1 where S0.Symbol = S1.TheString";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendSupportEvent(env, "DELL");
                SendSupportEvent(env, "IBM");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], 45d);
                    });

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], 20d);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(1, oldEvents.Length);
                        AssertEvent(oldEvents[0], 45d);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchRowPerEventNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec)";
                env.CompileDeploy(stmtText).AddListener("s0");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(3, newEvents.Length);
                        AssertEvent(newEvents[0], "DELL", 45d);
                        AssertEvent(newEvents[1], "IBM", 45d);
                        AssertEvent(newEvents[2], "DELL", 45d);
                    });

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], "IBM", 20d);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(3, oldEvents.Length);
                        AssertEvent(oldEvents[0], "DELL", 20d);
                        AssertEvent(oldEvents[1], "IBM", 20d);
                        AssertEvent(oldEvents[2], "DELL", 20d);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchRowPerEventJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec) as S0, SupportBean#keepall as S1 where S0.Symbol = S1.TheString";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendSupportEvent(env, "DELL");
                SendSupportEvent(env, "IBM");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(3, newEvents.Length);
                        AssertEvent(newEvents[0], "DELL", 45d);
                        AssertEvent(newEvents[1], "IBM", 45d);
                        AssertEvent(newEvents[2], "DELL", 45d);
                    });

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], "IBM", 20d);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(3, oldEvents.Length);
                        AssertEvent(oldEvents[0], "DELL", 20d);
                        AssertEvent(oldEvents[1], "IBM", 20d);
                        AssertEvent(oldEvents[2], "DELL", 20d);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchRowPerGroupNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean#time_batch(1 sec) group by Symbol order by Symbol asc";
                env.CompileDeploy(stmtText).AddListener("s0");

                // send first batch
                SendMDEvent(env, "DELL", 10, 0L);
                SendMDEvent(env, "IBM", 15, 0L);
                SendMDEvent(env, "DELL", 20, 0L);
                SendTimer(env, 1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(2, newEvents.Length);
                        AssertEvent(newEvents[0], "DELL", 30d);
                        AssertEvent(newEvents[1], "IBM", 15d);
                    });

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(2, newEvents.Length);
                        AssertEvent(newEvents[0], "DELL", null);
                        AssertEvent(newEvents[1], "IBM", 20d);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(2, oldEvents.Length);
                        AssertEvent(oldEvents[0], "DELL", 30d);
                        AssertEvent(oldEvents[1], "IBM", 15d);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchRowPerGroupJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText = "@name('s0') select irstream Symbol, sum(Price) as sumPrice " +
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

                var fields = "Symbol,sumPrice".SplitCsv();
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "DELL", 30d }, new object[] { "IBM", 15d } });

                // send second batch
                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);

                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            listener.LastNewData,
                            fields,
                            new object[][] { new object[] { "DELL", null }, new object[] { "IBM", 20d } });
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            listener.GetAndResetLastOldData(),
                            fields,
                            new object[][] { new object[] { "DELL", 30d }, new object[] { "IBM", 15d } });
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchAggrGroupedNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText =
                    "@name('s0') select irstream Symbol, sum(Price) as sumPrice, Volume from SupportMarketDataBean#time_batch(1 sec) group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendMDEvent(env, "DELL", 10, 200L);
                SendMDEvent(env, "IBM", 15, 500L);
                SendMDEvent(env, "DELL", 20, 250L);

                SendTimer(env, 1000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(3, newEvents.Length);
                        AssertEvent(newEvents[0], "DELL", 30d, 200L);
                        AssertEvent(newEvents[1], "IBM", 15d, 500L);
                        AssertEvent(newEvents[2], "DELL", 30d, 250L);
                    });

                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], "IBM", 20d, 600L);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(3, oldEvents.Length);
                        AssertEvent(oldEvents[0], "DELL", null, 200L);
                        AssertEvent(oldEvents[1], "IBM", 20d, 500L);
                        AssertEvent(oldEvents[2], "DELL", null, 250L);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeTimeBatchAggrGroupedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var stmtText = "@name('s0') select irstream Symbol, sum(Price) as sumPrice, Volume " +
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
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(3, newEvents.Length);
                        AssertEvent(newEvents[0], "DELL", 30d, 200L);
                        AssertEvent(newEvents[1], "IBM", 15d, 500L);
                        AssertEvent(newEvents[2], "DELL", 30d, 250L);
                    });

                SendMDEvent(env, "IBM", 20, 600L);
                SendTimer(env, 2000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.LastNewData;
                        Assert.AreEqual(1, newEvents.Length);
                        AssertEvent(newEvents[0], "IBM", 20d, 600L);

                        var oldEvents = listener.LastOldData;
                        Assert.AreEqual(3, oldEvents.Length);
                        AssertEvent(oldEvents[0], "DELL", null, 200L);
                        AssertEvent(oldEvents[1], "IBM", 20d, 500L);
                        AssertEvent(oldEvents[2], "DELL", null, 250L);
                    });

                env.UndeployAll();
            }
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
    }
} // end of namespace