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
    public class ResultSetQueryTypeAggregateGrouped
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeCriteriaByDotMethod());
            execs.Add(new ResultSetQueryTypeIterateUnbound());
            execs.Add(new ResultSetQueryTypeUnaggregatedHaving());
            execs.Add(new ResultSetQueryTypeWildcard());
            execs.Add(new ResultSetQueryTypeAggregationOverGroupedProps());
            execs.Add(new ResultSetQueryTypeSumOneView());
            execs.Add(new ResultSetQueryTypeSumJoin());
            execs.Add(new ResultSetQueryTypeInsertInto());
            execs.Add(new ResultSetQueryTypeMultikeyWArray());
            return execs;
        }

        private static void TryAssertionSum(RegressionEnvironment env)
        {
            string[] fields = {"Symbol", "Volume", "mySum"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);

            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("Volume"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mySum"));

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            AssertEvents(env, SYMBOL_DELL, 10000, 51);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {
                    new object[] {"DELL", 10000L, 51d}
                });

            env.Milestone(0);

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            AssertEvents(env, SYMBOL_DELL, 20000, 103);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {
                    new object[] {"DELL", 10000L, 103d}, new object[] {"DELL", 20000L, 103d}
                });

            SendEvent(env, SYMBOL_IBM, 30000, 70);
            AssertEvents(env, SYMBOL_IBM, 30000, 70);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {
                    new object[] {"DELL", 10000L, 103d}, new object[] {"DELL", 20000L, 103d},
                    new object[] {"IBM", 30000L, 70d}
                });

            env.Milestone(1);

            SendEvent(env, SYMBOL_IBM, 10000, 20);
            AssertEvents(env, SYMBOL_DELL, 10000, 52, SYMBOL_IBM, 10000, 90);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {
                    new object[] {"DELL", 20000L, 52d}, new object[] {"IBM", 30000L, 90d},
                    new object[] {"IBM", 10000L, 90d}
                });

            SendEvent(env, SYMBOL_DELL, 40000, 45);
            AssertEvents(env, SYMBOL_DELL, 20000, 45, SYMBOL_DELL, 40000, 45);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {
                    new object[] {"IBM", 10000L, 90d}, new object[] {"IBM", 30000L, 90d},
                    new object[] {"DELL", 40000L, 45d}
                });
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double sum)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(volume, newData[0].Get("Volume"));
            Assert.AreEqual(sum, newData[0].Get("mySum"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
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
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
            Assert.AreEqual(volumeOld, oldData[0].Get("Volume"));
            Assert.AreEqual(sumOld, oldData[0].Get("mySum"));

            Assert.AreEqual(symbolNew, newData[0].Get("Symbol"));
            Assert.AreEqual(volumeNew, newData[0].Get("Volume"));
            Assert.AreEqual(sumNew, newData[0].Get("mySum"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
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
            var fields = new[] {"id", "thesum"};
            env.SendEventBean(new SupportEventWithIntArray(id, array, value));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {id, expected});
        }

        internal class ResultSetQueryTypeMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@name('s0') select id, sum(value) as thesum from SupportEventWithIntArray group by array";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertIntArray(env, "E1", new int[] {1, 2}, 5, 5);

                env.Milestone(0);

                SendAssertIntArray(env, "E2", new int[] {1, 2}, 10, 15);
                SendAssertIntArray(env, "E3", new int[] {1}, 11, 11);
                SendAssertIntArray(env, "E4", new int[] {1, 3}, 12, 12);

                env.Milestone(1);

                SendAssertIntArray(env, "E5", new int[] {1}, 13, 24);
                SendAssertIntArray(env, "E6", new int[] {1, 3}, 15, 27);
                SendAssertIntArray(env, "E7", new int[] {1, 2}, 16, 31);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeCriteriaByDotMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select sb.GetLongPrimitive() as c0, sum(IntPrimitive) as c1 from SupportBean#length_batch(2) as sb group by sb.GetTheString()";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendSupportBean(env, "E1", 10, 100L);

                env.Milestone(0);

                MakeSendSupportBean(env, "E1", 20, 200L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new [] { "c0", "c1" },
                    new[] {new object[] {100L, 30}, new object[] {200L, 30}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeIterateUnbound : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl =
                    "@name('s0') @IterableUnbound select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10}, new object[] {"E2", 20}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 21}, new object[] {"E2", 20}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnaggregatedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select TheString from SupportBean group by TheString having IntPrimitive > 5";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 6));
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));

                env.SendEventBean(new SupportBean("E3", 7));
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("TheString"));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test no output limit
                var fields = new [] { "TheString"," IntPrimitive"," minval" };
                var epl =
                    "@name('s0') select *, min(IntPrimitive) as minval from SupportBean#length(2) group by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, 10});

                env.SendEventBean(new SupportBean("G1", 9));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 9, 9});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 11, 9});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregationOverGroupedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = new [] { "Volume","Symbol","Price","mycount" };
                var epl = "@name('s0') select irstream Volume,Symbol,Price,count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Symbol, Price";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, SYMBOL_DELL, 1000, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1000L, "DELL", 10.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1000L, "DELL", 10.0, 1L}});

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 900, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {900L, "DELL", 11.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1000L, "DELL", 10.0, 1L}, new object[] {900L, "DELL", 11.0, 1L}});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_DELL, 1500, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1500L, "DELL", 10.0, 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {1000L, "DELL", 10.0, 2L}, new object[] {900L, "DELL", 11.0, 1L},
                        new object[] {1500L, "DELL", 10.0, 2L}
                    });
                env.Listener("s0").Reset();

                env.Milestone(1);

                SendEvent(env, SYMBOL_IBM, 500, 5);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {500L, "IBM", 5.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {1000L, "DELL", 10.0, 2L}, new object[] {900L, "DELL", 11.0, 1L},
                        new object[] {1500L, "DELL", 10.0, 2L}, new object[] {500L, "IBM", 5.0, 1L}
                    });
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 600, 5);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {600L, "IBM", 5.0, 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {1000L, "DELL", 10.0, 2L}, new object[] {900L, "DELL", 11.0, 1L},
                        new object[] {1500L, "DELL", 10.0, 2L}, new object[] {500L, "IBM", 5.0, 2L},
                        new object[] {600L, "IBM", 5.0, 2L}
                    });
                env.Listener("s0").Reset();

                env.Milestone(2);

                SendEvent(env, SYMBOL_IBM, 500, 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {500L, "IBM", 5.0, 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {1000L, "DELL", 10.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {900L, "DELL", 11.0, 1L}, new object[] {1500L, "DELL", 10.0, 1L},
                        new object[] {500L, "IBM", 5.0, 3L}, new object[] {600L, "IBM", 5.0, 3L},
                        new object[] {500L, "IBM", 5.0, 3L}
                    });
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 600, 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {600L, "IBM", 5.0, 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {900L, "DELL", 11.0, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {1500L, "DELL", 10.0, 1L}, new object[] {500L, "IBM", 5.0, 4L},
                        new object[] {600L, "IBM", 5.0, 4L}, new object[] {500L, "IBM", 5.0, 4L},
                        new object[] {600L, "IBM", 5.0, 4L}
                    });
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select irstream Symbol, Volume, sum(Price) as mySum " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select irstream Symbol, Volume, sum(Price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt =
                    "@name('s0') select Symbol as Symbol, avg(Price) as average, sum(Volume) as sumation from SupportMarketDataBean#length(3000)";
                env.CompileDeploy(stmt).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("IBM", 10D, 20000L, null));
                var eventBean = env.Listener("s0").LastNewData[0];
                Assert.AreEqual("IBM", eventBean.Get("Symbol"));
                Assert.AreEqual(10d, eventBean.Get("average"));
                Assert.AreEqual(20000L, eventBean.Get("sumation"));

                // create insert into statements
                stmt =
                    "@name('s1') insert into StockAverages select Symbol as Symbol, avg(Price) as average, sum(Volume) as sumation " +
                    "from SupportMarketDataBean#length(3000);\n" +
                    "@name('s2') select * from StockAverages";
                env.CompileDeploy(stmt).AddListener("s1").AddListener("s2");

                // send event
                env.SendEventBean(new SupportMarketDataBean("IBM", 20D, 40000L, null));
                eventBean = env.Listener("s0").LastNewData[0];
                Assert.AreEqual("IBM", eventBean.Get("Symbol"));
                Assert.AreEqual(15d, eventBean.Get("average"));
                Assert.AreEqual(60000L, eventBean.Get("sumation"));

                Assert.AreEqual(1, env.Listener("s2").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s2").LastNewData.Length);
                eventBean = env.Listener("s2").LastNewData[0];
                Assert.AreEqual("IBM", eventBean.Get("Symbol"));
                Assert.AreEqual(20d, eventBean.Get("average"));
                Assert.AreEqual(40000L, eventBean.Get("sumation"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace