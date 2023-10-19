///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeAggregateGrouped
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithCriteriaByDotMethod(execs);
            WithIterateUnbound(execs);
            WithUnaggregatedHaving(execs);
            WithWildcard(execs);
            WithAggregationOverGroupedProps(execs);
            WithSumOneView(execs);
            WithSumJoin(execs);
            WithInsertInto(execs);
            WithMultikeyWArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithSumJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeSumJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSumOneView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeSumOneView());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationOverGroupedProps(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAggregationOverGroupedProps());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithUnaggregatedHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnaggregatedHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithIterateUnbound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeIterateUnbound());
            return execs;
        }

        public static IList<RegressionExecution> WithCriteriaByDotMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeCriteriaByDotMethod());
            return execs;
        }

        private class ResultSetQueryTypeMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select id, sum(value) as thesum from SupportEventWithIntArray group by array";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertIntArray(env, "E1", new int[] { 1, 2 }, 5, 5);

                env.Milestone(0);

                SendAssertIntArray(env, "E2", new int[] { 1, 2 }, 10, 15);
                SendAssertIntArray(env, "E3", new int[] { 1 }, 11, 11);
                SendAssertIntArray(env, "E4", new int[] { 1, 3 }, 12, 12);

                env.Milestone(1);

                SendAssertIntArray(env, "E5", new int[] { 1 }, 13, 24);
                SendAssertIntArray(env, "E6", new int[] { 1, 3 }, 15, 27);
                SendAssertIntArray(env, "E7", new int[] { 1, 2 }, 16, 31);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeCriteriaByDotMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select sb.getLongPrimitive() as c0, sum(intPrimitive) as c1 from SupportBean#length_batch(2) as sb group by sb.getTheString()";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendSupportBean(env, "E1", 10, 100L);

                env.Milestone(0);

                MakeSendSupportBean(env, "E1", 20, 200L);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "c0,c1".SplitCsv(),
                    new object[][] { new object[] { 100L, 30 }, new object[] { 200L, 30 } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeIterateUnbound : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@name('s0') @IterableUnbound select theString as c0, sum(intPrimitive) as c1 from SupportBean group by theString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 11));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeUnaggregatedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select theString from SupportBean group by theString having intPrimitive > 5";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 5));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 6));
                env.AssertEqualsNew("s0", "theString", "E1");

                env.SendEventBean(new SupportBean("E3", 7));
                env.AssertEqualsNew("s0", "theString", "E3");

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test no output limit
                var fields = "theString, intPrimitive, minval".SplitCsv();
                var epl =
                    "@name('s0') select *, min(intPrimitive) as minval from SupportBean#length(2) group by theString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, 10 });

                env.SendEventBean(new SupportBean("G1", 9));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 9, 9 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 11, 9 });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeAggregationOverGroupedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = "volume,symbol,price,mycount".SplitCsv();
                var epl = "@name('s0') select irstream volume,symbol,price,count(price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by symbol, price";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, SYMBOL_DELL, 1000, 10);
                env.AssertPropsNew("s0", fields, new object[] { 1000L, "DELL", 10.0, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1000L, "DELL", 10.0, 1L } });

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 900, 11);
                env.AssertPropsNew("s0", fields, new object[] { 900L, "DELL", 11.0, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { 1000L, "DELL", 10.0, 1L }, new object[] { 900L, "DELL", 11.0, 1L } });

                SendEvent(env, SYMBOL_DELL, 1500, 10);
                env.AssertPropsNew("s0", fields, new object[] { 1500L, "DELL", 10.0, 2L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 1000L, "DELL", 10.0, 2L }, new object[] { 900L, "DELL", 11.0, 1L },
                        new object[] { 1500L, "DELL", 10.0, 2L }
                    });

                env.Milestone(1);

                SendEvent(env, SYMBOL_IBM, 500, 5);
                env.AssertPropsNew("s0", fields, new object[] { 500L, "IBM", 5.0, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 1000L, "DELL", 10.0, 2L }, new object[] { 900L, "DELL", 11.0, 1L },
                        new object[] { 1500L, "DELL", 10.0, 2L }, new object[] { 500L, "IBM", 5.0, 1L }
                    });

                SendEvent(env, SYMBOL_IBM, 600, 5);
                env.AssertPropsNew("s0", fields, new object[] { 600L, "IBM", 5.0, 2L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 1000L, "DELL", 10.0, 2L }, new object[] { 900L, "DELL", 11.0, 1L },
                        new object[] { 1500L, "DELL", 10.0, 2L }, new object[] { 500L, "IBM", 5.0, 2L },
                        new object[] { 600L, "IBM", 5.0, 2L }
                    });

                env.Milestone(2);

                SendEvent(env, SYMBOL_IBM, 500, 5);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { 500L, "IBM", 5.0, 3L },
                    new object[] { 1000L, "DELL", 10.0, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 900L, "DELL", 11.0, 1L }, new object[] { 1500L, "DELL", 10.0, 1L },
                        new object[] { 500L, "IBM", 5.0, 3L }, new object[] { 600L, "IBM", 5.0, 3L },
                        new object[] { 500L, "IBM", 5.0, 3L }
                    });

                SendEvent(env, SYMBOL_IBM, 600, 5);
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { 600L, "IBM", 5.0, 4L },
                    new object[] { 900L, "DELL", 11.0, 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 1500L, "DELL", 10.0, 1L }, new object[] { 500L, "IBM", 5.0, 4L },
                        new object[] { 600L, "IBM", 5.0, 4L }, new object[] { 500L, "IBM", 5.0, 4L },
                        new object[] { 600L, "IBM", 5.0, 4L }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select irstream symbol, volume, sum(price) as mySum " +
                          "from SupportMarketDataBean#length(3) " +
                          "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                          "group by symbol";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select irstream symbol, volume, sum(price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                          "  and one.theString = two.symbol " +
                          "group by symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt =
                    "@name('s0') select symbol as symbol, avg(price) as average, sum(volume) as sumation from SupportMarketDataBean#length(3000)";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("IBM", 10D, 20000L, null));
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreEqual("IBM", eventBean.Get("symbol"));
                        Assert.AreEqual(10d, eventBean.Get("average"));
                        Assert.AreEqual(20000L, eventBean.Get("sumation"));
                    });

                // create insert into statements
                stmt =
                    "@name('s1') insert into StockAverages select symbol as symbol, avg(price) as average, sum(volume) as sumation " +
                    "from SupportMarketDataBean#length(3000);\n" +
                    "@name('s2') select * from StockAverages";
                env.CompileDeploy(stmt).AddListener("s1").AddListener("s2");

                // send event
                env.SendEventBean(new SupportMarketDataBean("IBM", 20D, 40000L, null));
                env.AssertEventNew(
                    "s0",
                    eventBean => {
                        Assert.AreEqual("IBM", eventBean.Get("symbol"));
                        Assert.AreEqual(15d, eventBean.Get("average"));
                        Assert.AreEqual(60000L, eventBean.Get("sumation"));
                    });

                env.AssertEventNew(
                    "s2",
                    eventBean => {
                        Assert.AreEqual("IBM", eventBean.Get("symbol"));
                        Assert.AreEqual(20d, eventBean.Get("average"));
                        Assert.AreEqual(40000L, eventBean.Get("sumation"));
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertionSum(RegressionEnvironment env)
        {
            var fields = new string[] { "symbol", "volume", "mySum" };
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("symbol"));
                    Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("volume"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                });

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            AssertEvents(env, SYMBOL_DELL, 10000, 51);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "DELL", 10000L, 51d }
                });

            env.Milestone(0);

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            AssertEvents(env, SYMBOL_DELL, 20000, 103);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "DELL", 10000L, 103d }, new object[] { "DELL", 20000L, 103d }
                });

            SendEvent(env, SYMBOL_IBM, 30000, 70);
            AssertEvents(env, SYMBOL_IBM, 30000, 70);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "DELL", 10000L, 103d }, new object[] { "DELL", 20000L, 103d },
                    new object[] { "IBM", 30000L, 70d }
                });

            env.Milestone(1);

            SendEvent(env, SYMBOL_IBM, 10000, 20);
            AssertEvents(env, SYMBOL_DELL, 10000, 52, SYMBOL_IBM, 10000, 90);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "DELL", 20000L, 52d }, new object[] { "IBM", 30000L, 90d },
                    new object[] { "IBM", 10000L, 90d }
                });

            SendEvent(env, SYMBOL_DELL, 40000, 45);
            AssertEvents(env, SYMBOL_DELL, 20000, 45, SYMBOL_DELL, 40000, 45);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "IBM", 10000L, 90d }, new object[] { "IBM", 30000L, 90d },
                    new object[] { "DELL", 40000L, 45d }
                });
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double sum)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    Assert.IsNull(oldData);
                    Assert.AreEqual(1, newData.Length);

                    Assert.AreEqual(symbol, newData[0].Get("symbol"));
                    Assert.AreEqual(volume, newData[0].Get("volume"));
                    Assert.AreEqual(sum, newData[0].Get("mySum"));

                    listener.Reset();
                });
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOld,
            long volumeOld,
            double sumOld,
            string symbolNew,
            long volumeNew,
            double sumNew)
        {
            env.AssertPropsIRPair(
                "s0",
                "symbol,volume,mySum".SplitCsv(),
                new object[] { symbolNew, volumeNew, sumNew },
                new object[] { symbolOld, volumeOld, sumOld });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        private static SupportBean MakeSendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendAssertIntArray(
            RegressionEnvironment env,
            string id,
            int[] array,
            int value,
            int expected)
        {
            var fields = "id,thesum".SplitCsv();
            env.SendEventBean(new SupportEventWithIntArray(id, array, value));
            env.AssertPropsNew("s0", fields, new object[] { id, expected });
        }
    }
} // end of namespace