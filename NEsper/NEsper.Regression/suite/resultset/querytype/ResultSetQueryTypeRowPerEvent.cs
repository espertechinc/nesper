///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowPerEvent
    {
        private const string JOIN_KEY = "KEY";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerEventSumOneView());
            execs.Add(new ResultSetQueryTypeRowPerEventSumJoin());
            execs.Add(new ResultSetQueryTypeAggregatedSelectTriggerEvent());
            execs.Add(new ResultSetQueryTypeAggregatedSelectUnaggregatedHaving());
            execs.Add(new ResultSetQueryTypeSumAvgWithWhere());
            execs.Add(new ResultSetQueryTypeRowPerEventDistinct());
            execs.Add(new ResultSetQueryTypeRowPerEventDistinctNullable());
            return execs;
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            long? volume,
            string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, volume, null);
        }

        private static void AssertPostedNew(
            RegressionEnvironment env,
            double? newAvg,
            long? newSum)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual("IBM stats", newData[0].Get("title"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));

            env.Listener("s0").Reset();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed,
            AtomicLong eventCount)
        {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            bean.LongPrimitive = eventCount.IncrementAndGet();
            env.SendEventBean(bean);
        }

        private static void SendMarketDataEvent(
            RegressionEnvironment env,
            string symbol,
            long? volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            AtomicLong eventCount,
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0, eventCount);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int intBoxed)
        {
            var theEvent = new SupportBean(theString, intPrimitive);
            theEvent.IntBoxed = intBoxed;
            env.SendEventBean(theEvent);
        }

        private static void TryAssert(RegressionEnvironment env)
        {
            string[] fields = {"LongPrimitive", "mySum"};

            // assert select result type
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);
            var eventCount = new AtomicLong();

            SendEvent(env, eventCount, 10);
            Assert.AreEqual(10L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {1L, 10L}});

            env.Milestone(1);

            SendEvent(env, eventCount, 15);
            Assert.AreEqual(25L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {1L, 25L}, new object[] {2L, 25L}});

            env.Milestone(2);

            SendEvent(env, eventCount, -5);
            Assert.AreEqual(20L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(env.Listener("s0").LastOldData);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {1L, 20L}, new object[] {2L, 20L}, new object[] {3L, 20L}});

            env.Milestone(3);

            SendEvent(env, eventCount, -2);
            Assert.AreEqual(8L, env.Listener("s0").LastOldData[0].Get("mySum"));
            Assert.AreEqual(8L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {4L, 8L}, new object[] {2L, 8L}, new object[] {3L, 8L}});

            env.Milestone(4);

            SendEvent(env, eventCount, 100);
            Assert.AreEqual(93L, env.Listener("s0").LastOldData[0].Get("mySum"));
            Assert.AreEqual(93L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {4L, 93L}, new object[] {5L, 93L}, new object[] {3L, 93L}});

            env.Milestone(5);

            SendEvent(env, eventCount, 1000);
            Assert.AreEqual(1098L, env.Listener("s0").LastOldData[0].Get("mySum"));
            Assert.AreEqual(1098L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {4L, 1098L}, new object[] {5L, 1098L}, new object[] {6L, 1098L}});
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            long volSum)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(volSum, newData[0].Get("volSum"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOld,
            long volSumOld,
            string symbolNew,
            long volSumNew)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
            Assert.AreEqual(volSumOld, oldData[0].Get("volSum"));

            Assert.AreEqual(symbolNew, newData[0].Get("Symbol"));
            Assert.AreEqual(volSumNew, newData[0].Get("volSum"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetQueryTypeRowPerEventSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream longPrimitive, sum(longBoxed) as mySum " +
                          "from SupportBean#length(3)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssert(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerEventSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream longPrimitive, sum(longBoxed) as mySum " +
                          "from SupportBeanString#length(3) as one, SupportBean#length(3) as two " +
                          "where one.TheString = two.TheString";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBeanString(JOIN_KEY));

                TryAssert(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregatedSelectTriggerEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select window(s0.*) as rows, sb " +
                          "from SupportBean#keepall as sb, SupportBean_S0#keepall as s0 " +
                          "where sb.TheString = s0.p00";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "K1", "V1"));
                env.SendEventBean(new SupportBean_S0(2, "K1", "V2"));

                env.Milestone(0);

                // test SB-direction
                var b1 = new SupportBean("K1", 0);
                env.SendEventBean(b1);
                var events = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(2, events.Length);
                foreach (var eventX in events) {
                    Assert.AreEqual(b1, eventX.Get("sb"));
                    Assert.AreEqual(2, ((SupportBean_S0[]) eventX.Get("rows")).Length);
                }

                // test S0-direction
                env.SendEventBean(new SupportBean_S0(1, "K1", "V3"));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(b1, @event.Get("sb"));
                Assert.AreEqual(3, ((SupportBean_S0[]) @event.Get("rows")).Length);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregatedSelectUnaggregatedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // ESPER-571
                var epl =
                    "@Name('s0') select max(IntPrimitive) as val from SupportBean#time(1) having max(IntPrimitive) > intBoxed";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 10, 1);
                Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                SendEvent(env, "E2", 10, 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendEvent(env, "E3", 15, 11);
                Assert.AreEqual(15, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                SendEvent(env, "E4", 20, 11);
                Assert.AreEqual(20, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.Milestone(1);

                SendEvent(env, "E5", 25, 25);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumAvgWithWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select 'IBM stats' as title, volume, avg(volume) as myAvg, sum(volume) as mySum " +
                    "from SupportMarketDataBean#length(3)" +
                    "where symbol='IBM'";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendMarketDataEvent(env, "GE", 10L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMarketDataEvent(env, "IBM", 20L);
                AssertPostedNew(env, 20d, 20L);

                SendMarketDataEvent(env, "XXX", 10000L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendMarketDataEvent(env, "IBM", 30L);
                AssertPostedNew(env, 25d, 50L);

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowPerEventDistinct : RegressionExecution
        {
            private const string SYMBOL_DELL = "DELL";
            private const string SYMBOL_IBM = "IBM";

            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@Name('s0') select irstream symbol, sum(distinct volume) as volSum " +
                          "from SupportMarketDataBean#length(3) ";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("volSum"));

                SendEvent(env, SYMBOL_DELL, 10000);
                AssertEvents(env, SYMBOL_DELL, 10000);

                SendEvent(env, SYMBOL_DELL, 10000);
                AssertEvents(env, SYMBOL_DELL, 10000); // still 10k since summing distinct volumes

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 20000);
                AssertEvents(env, SYMBOL_DELL, 30000);

                SendEvent(env, SYMBOL_IBM, 1000);
                AssertEvents(env, SYMBOL_DELL, 31000, SYMBOL_IBM, 31000);

                SendEvent(env, SYMBOL_IBM, 1000);
                AssertEvents(env, SYMBOL_DELL, 21000, SYMBOL_IBM, 21000);

                SendEvent(env, SYMBOL_IBM, 1000);
                AssertEvents(env, SYMBOL_DELL, 1000, SYMBOL_IBM, 1000);

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeRowPerEventDistinctNullable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream " +
                          "avg(distinct volume) as avgVolume, count(distinct symbol) as countDistinctSymbol " +
                          "from SupportMarketDataBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent(100L, "ONE"));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"avgVolume", 100d},
                            new object[] {"countDistinctSymbol", 1L}
                        },
                        new[] {
                            new object[] {"avgVolume", null},
                            new object[] {"countDistinctSymbol", 0L}
                        });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent(null, null));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"avgVolume", 100d},
                            new object[] {"countDistinctSymbol", 1L}
                        },
                        new[] {
                            new object[] {"avgVolume", 100d},
                            new object[] {"countDistinctSymbol", 1L}
                        });

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent(null, "Two"));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"avgVolume", 100d},
                            new object[] {"countDistinctSymbol", 2L}
                        },
                        new[] {
                            new object[] {"avgVolume", 100d},
                            new object[] {"countDistinctSymbol", 1L}
                        });

                env.Milestone(3);

                env.UndeployAll();
            }
        }
    }
} // end of namespace