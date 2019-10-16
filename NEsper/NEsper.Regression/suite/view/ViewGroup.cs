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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewGroup
    {
        private const string SYMBOL_CISCO = "CSCO.O";
        private const string SYMBOL_IBM = "IBM.N";
        private const string SYMBOL_GE = "GE.N";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewGroupObjectArrayEvent());
            execs.Add(new ViewGroupStats());
            execs.Add(new ViewGroupReclaimTimeWindow());
            execs.Add(new ViewGroupReclaimAgedHint());
            execs.Add(new ViewGroupCorrel());
            execs.Add(new ViewGroupLinest());
            execs.Add(new ViewGroupMultiProperty());
            execs.Add(new ViewGroupInvalid());
            execs.Add(new ViewGroupLengthWinWeightAvg());
            execs.Add(new ViewGroupReclaimWithFlipTime(5000L));
            execs.Add(new ViewGroupTimeBatch());
            execs.Add(new ViewGroupTimeAccum());
            execs.Add(new ViewGroupTimeOrder());
            execs.Add(new ViewGroupTimeLengthBatch());
            execs.Add(new ViewGroupLengthWin());
            execs.Add(new ViewGroupLengthBatch());
            execs.Add(new ViewGroupTimeWin());
            execs.Add(new ViewGroupExpressionGrouped());
            execs.Add(new ViewGroupExpressionBatch());
            return execs;
        }

        private static void AssertCount(
            EPStatement stmt,
            long count)
        {
            Assert.AreEqual(count, EPAssertionUtil.EnumeratorCount(stmt.GetEnumerator()));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            SendEvent(env, symbol, price, -1);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price,
            long volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, volume, "");
            env.SendEventBean(theEvent);
        }

        private static IList<IDictionary<string, object>> MakeMap(
            string symbol,
            double average)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();

            result.Put("Symbol", symbol);
            result.Put("average", average);

            var vec = new List<IDictionary<string, object>>();
            vec.Add(result);

            return vec;
        }

        private static void SendProductNew(
            RegressionEnvironment env,
            string product,
            int size)
        {
            IDictionary<string, object> theEvent = new Dictionary<string, object>();
            theEvent.Put("product", product);
            theEvent.Put("productsize", size);
            env.SendEventMap(theEvent, "Product");
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void PopulateMap(
            IDictionary<string, object> map,
            string symbol,
            string feed,
            long volume,
            long size)
        {
            map.Put("Symbol", symbol);
            map.Put("Feed", feed);
            map.Put("Volume", volume);
            map.Put("size", size);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            string feed,
            long volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, feed);
            env.SendEventBean(theEvent);
        }

        private static SupportBeanTimestamp SendEventTS(
            RegressionEnvironment env,
            string id,
            string groupId,
            long timestamp)
        {
            var bean = new SupportBeanTimestamp(id, groupId, timestamp);
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price)
        {
            return new SupportMarketDataBean(symbol, price, 0L, null);
        }

        internal class ViewGroupInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select * from SupportBean#groupwin(TheString)#length(1)#groupwin(TheString)#uni(IntPrimitive)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate data window declaration: Multiple groupwin-declarations are not supported");

                epl = "select avg(Price), Symbol from SupportMarketDataBean#length(100)#groupwin(Symbol)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate data window declaration: Invalid use of the 'groupwin' view, the view requires one or more child views to group, or consider using the group-by clause");

                epl = "select * from SupportBean#keepall#groupwin(TheString)#length(2)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate data window declaration: The 'groupwin' declaration must occur in the first position");

                epl = "select * from SupportBean#groupwin(TheString)#length(2)#merge(TheString)#keepall";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate data window declaration: The 'merge' declaration cannot be used in conjunction with multiple data windows");
            }
        }

        internal class ViewGroupMultiProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var symbolMsft = "MSFT";
                var symbolGe = "GE";
                var feedInfo = "INFO";
                var feedReu = "REU";

                // Listen to all ticks
                var epl = "@Name('s0') select irstream datapoints as size, Symbol, Feed, Volume " +
                          "from SupportMarketDataBean#groupwin(Symbol, Feed, Volume)#uni(Price) order by Symbol, Feed, Volume";
                env.CompileDeploy(epl).AddListener("s0");

                var mapList = new List<IDictionary<string, object>>();

                // Set up a map of expected values

                var expectedValues = new IDictionary<string, object>[10];
                for (var i = 0; i < expectedValues.Length; i++) {
                    expectedValues[i] = new Dictionary<string, object>();
                }

                // Send one event, check results
                SendEvent(env, symbolGe, feedInfo, 1);

                PopulateMap(expectedValues[0], symbolGe, feedInfo, 1L, 0);
                mapList.Add(expectedValues[0]);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").LastOldData, mapList);
                PopulateMap(expectedValues[0], symbolGe, feedInfo, 1L, 1);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").LastNewData, mapList);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), mapList);

                env.Milestone(0);

                // Send a couple of events
                SendEvent(env, symbolGe, feedInfo, 1);
                SendEvent(env, symbolGe, feedInfo, 2);
                SendEvent(env, symbolGe, feedInfo, 1);

                env.Milestone(1);

                SendEvent(env, symbolGe, feedReu, 99);
                SendEvent(env, symbolMsft, feedInfo, 100);

                PopulateMap(expectedValues[1], symbolMsft, feedInfo, 100, 0);
                mapList.Clear();
                mapList.Add(expectedValues[1]);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").LastOldData, mapList);
                PopulateMap(expectedValues[1], symbolMsft, feedInfo, 100, 1);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").LastNewData, mapList);

                PopulateMap(expectedValues[0], symbolGe, feedInfo, 1, 3);
                PopulateMap(expectedValues[2], symbolGe, feedInfo, 2, 1);
                PopulateMap(expectedValues[3], symbolGe, feedReu, 99, 1);
                mapList.Clear();
                mapList.Add(expectedValues[0]);
                mapList.Add(expectedValues[2]);
                mapList.Add(expectedValues[3]);
                mapList.Add(expectedValues[1]);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), mapList);

                env.UndeployAll();
            }
        }

        internal class ViewGroupExpressionBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl = "@Name('create_var') create variable long ENGINE_TIME;\n" +
                          "@Name('runtime_time_update') on pattern[every timer:interval(10 seconds)] set ENGINE_TIME = current_timestamp();\n" +
                          "@Name('out_null') select window(*) from SupportBean#groupwin(TheString)#expr_batch(oldest_timestamp.plus(9 seconds) < ENGINE_TIME);";
                env.CompileDeploy(epl).AddListener("out_null");

                env.AdvanceTime(5000);
                env.AdvanceTime(10000);
                env.AdvanceTime(11000);

                Assert.IsFalse(env.Listener("out_null").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewGroupObjectArrayEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "P1","sp2" };
                var epl = "@Name('s0') select P1,sum(P2) as sp2 from OAEventStringInt#groupwin(P1)#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventObjectArray(new object[] {"A", 10}, "OAEventStringInt");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 10});

                env.SendEventObjectArray(new object[] {"B", 11}, "OAEventStringInt");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 21});

                env.Milestone(0);

                env.SendEventObjectArray(new object[] {"A", 12}, "OAEventStringInt");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 33});

                env.SendEventObjectArray(new object[] {"A", 13}, "OAEventStringInt");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 36});

                env.UndeployAll();
            }
        }

        internal class ViewGroupReclaimTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var epl = "@Name('s0') @Hint('reclaim_group_aged=30,reclaim_group_freq=5') " +
                          "select LongPrimitive, count(*) from SupportBean#groupwin(TheString)#time(3000000)";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    var theEvent = new SupportBean(Convert.ToString(i), i);
                    env.SendEventBean(theEvent);
                }

                Assert.AreEqual(10, SupportScheduleHelper.ScheduleCount(env.Statement("s0")));

                env.Milestone(0);

                env.AdvanceTime(1000000);
                env.SendEventBean(new SupportBean("E1", 1));

                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCount(env.Statement("s0")));

                env.UndeployAll();

                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
            }
        }

        internal class ViewGroupReclaimAgedHint : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl = "@Name('s0') @Hint('reclaim_group_aged=5,reclaim_group_freq=1') " +
                          "select * from SupportBean#groupwin(TheString)#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                var maxEventsPerSlot = 1000;
                for (var timeSlot = 0; timeSlot < 10; timeSlot++) {
                    env.AdvanceTime(timeSlot * 1000 + 1);

                    for (var i = 0; i < maxEventsPerSlot; i++) {
                        env.SendEventBean(new SupportBean("E" + timeSlot, 0));
                    }
                }

                env.Milestone(0);

                var iterator = EPAssertionUtil.EnumeratorToArray(env.Statement("s0").GetEnumerator());
                Assert.IsTrue(iterator.Length <= 6 * maxEventsPerSlot);

                env.SendEventBean(new SupportBean("E0", 1));

                env.Milestone(1);

                iterator = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                Assert.AreEqual(6 * maxEventsPerSlot + 1, iterator.Length);

                env.UndeployAll();
            }
        }

        internal class ViewGroupStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;
                var filter = "select * from SupportMarketDataBean";

                epl = "@Name('PriceLast3Stats')" +
                      filter +
                      "#groupwin(Symbol)#length(3)#uni(Price) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("PriceLast3Stats");

                epl = "@Name('VolumeLast3Stats')" +
                      filter +
                      "#groupwin(Symbol)#length(3)#uni(Volume) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("VolumeLast3Stats");

                epl = "@Name('PriceAllStats')" + filter + "#groupwin(Symbol)#uni(Price) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("PriceAllStats");

                epl = "@Name('VolumeAllStats')" + filter + "#groupwin(Symbol)#uni(Volume) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("VolumeAllStats");

                IList<IDictionary<string, object>> expectedList = new List<IDictionary<string, object>>();
                for (var i = 0; i < 3; i++) {
                    expectedList.Add(new Dictionary<string, object>());
                }

                SendEvent(env, SYMBOL_CISCO, 25, 50000);
                SendEvent(env, SYMBOL_CISCO, 26, 60000);
                SendEvent(env, SYMBOL_IBM, 10, 8000);
                SendEvent(env, SYMBOL_IBM, 10.5, 8200);
                SendEvent(env, SYMBOL_GE, 88, 1000);

                EPAssertionUtil.AssertPropsPerRow(env.Listener("PriceLast3Stats").LastNewData, MakeMap(SYMBOL_GE, 88));
                EPAssertionUtil.AssertPropsPerRow(env.Listener("PriceAllStats").LastNewData, MakeMap(SYMBOL_GE, 88));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeLast3Stats").LastNewData,
                    MakeMap(SYMBOL_GE, 1000));
                EPAssertionUtil.AssertPropsPerRow(env.Listener("VolumeAllStats").LastNewData, MakeMap(SYMBOL_GE, 1000));

                SendEvent(env, SYMBOL_CISCO, 27, 70000);
                SendEvent(env, SYMBOL_CISCO, 28, 80000);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("PriceAllStats").LastNewData,
                    MakeMap(SYMBOL_CISCO, 26.5d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeAllStats").LastNewData,
                    MakeMap(SYMBOL_CISCO, 65000d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("PriceLast3Stats").LastNewData,
                    MakeMap(SYMBOL_CISCO, 27d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeLast3Stats").LastNewData,
                    MakeMap(SYMBOL_CISCO, 70000d));

                SendEvent(env, SYMBOL_IBM, 11, 8700);
                SendEvent(env, SYMBOL_IBM, 12, 8900);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("PriceAllStats").LastNewData,
                    MakeMap(SYMBOL_IBM, 10.875d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeAllStats").LastNewData,
                    MakeMap(SYMBOL_IBM, 8450d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("PriceLast3Stats").LastNewData,
                    MakeMap(SYMBOL_IBM, 11d + 1 / 6d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeLast3Stats").LastNewData,
                    MakeMap(SYMBOL_IBM, 8600d));

                SendEvent(env, SYMBOL_GE, 85.5, 950);
                SendEvent(env, SYMBOL_GE, 85.75, 900);
                SendEvent(env, SYMBOL_GE, 89, 1250);
                SendEvent(env, SYMBOL_GE, 86, 1200);
                SendEvent(env, SYMBOL_GE, 85, 1150);

                var averageGE = (88d + 85.5d + 85.75d + 89d + 86d + 85d) / 6d;
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("PriceAllStats").LastNewData,
                    MakeMap(SYMBOL_GE, averageGE));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeAllStats").LastNewData,
                    MakeMap(SYMBOL_GE, 1075d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("PriceLast3Stats").LastNewData,
                    MakeMap(SYMBOL_GE, 86d + 2d / 3d));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("VolumeLast3Stats").LastNewData,
                    MakeMap(SYMBOL_GE, 1200d));

                // Check iterator results
                expectedList[0].Put("Symbol", SYMBOL_CISCO);
                expectedList[0].Put("average", 26.5d);
                expectedList[1].Put("Symbol", SYMBOL_GE);
                expectedList[1].Put("average", averageGE);
                expectedList[2].Put("Symbol", SYMBOL_IBM);
                expectedList[2].Put("average", 10.875d);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("PriceAllStats"), expectedList);

                expectedList[0].Put("Symbol", SYMBOL_CISCO);
                expectedList[0].Put("average", 27d);
                expectedList[1].Put("Symbol", SYMBOL_GE);
                expectedList[1].Put("average", 86d + 2d / 3d);
                expectedList[2].Put("Symbol", SYMBOL_IBM);
                expectedList[2].Put("average", 11d + 1 / 6d);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("PriceLast3Stats"), expectedList);

                env.UndeployAll();
            }
        }

        internal class ViewGroupExpressionGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select irstream * from SupportBeanTimestamp#groupwin(Timestamp.getDayOfWeek())#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(
                    new SupportBeanTimestamp(
                        "E1",
                        DateTimeParsingFunctions.ParseDefaultMSec("2002-01-01T09:0:00.000")));
                env.SendEventBean(
                    new SupportBeanTimestamp(
                        "E2",
                        DateTimeParsingFunctions.ParseDefaultMSec("2002-01-08T09:0:00.000")));
                env.SendEventBean(
                    new SupportBeanTimestamp(
                        "E3",
                        DateTimeParsingFunctions.ParseDefaultMSec("2002-01-15T09:0:00.000")));
                Assert.AreEqual(1, env.Listener("s0").DataListsFlattened.Second.Length);

                env.UndeployAll();
            }
        }

        internal class ViewGroupCorrel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // further math tests can be found in the view unit test
                var epl =
                    "@Name('s0') select * from SupportMarketDataBean#groupwin(Symbol)#length(1000000)#correl(Price, Volume, Feed)";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("correlation"));

                string[] fields = {"Symbol", "correlation", "Feed"};

                env.SendEventBean(new SupportMarketDataBean("ABC", 10.0, 1000L, "f1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"ABC", double.NaN, "f1"});

                env.SendEventBean(new SupportMarketDataBean("DEF", 1.0, 2L, "f2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"DEF", double.NaN, "f2"});

                env.Milestone(0);

                env.SendEventBean(new SupportMarketDataBean("DEF", 2.0, 4L, "f3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"DEF", 1.0, "f3"});

                env.SendEventBean(new SupportMarketDataBean("ABC", 20.0, 2000L, "f4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"ABC", 1.0, "f4"});

                env.UndeployAll();
            }
        }

        internal class ViewGroupLinest : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // further math tests can be found in the view unit test
                var epl =
                    "@Name('s0') select * from SupportMarketDataBean#groupwin(Symbol)#length(1000000)#linest(Price, Volume, Feed)";
                env.CompileDeploy(epl).AddListener("s0");

                var statement = env.Statement("s0");
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("slope"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YIntercept"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XAverage"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XStandardDeviationPop"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XStandardDeviationSample"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XSum"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XVariance"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YAverage"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YStandardDeviationPop"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YStandardDeviationSample"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YSum"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YVariance"));
                Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("dataPoints"));
                Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("n"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumX"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumXSq"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumXY"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumY"));
                Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumYSq"));

                string[] fields = {"Symbol", "slope", "YIntercept", "Feed"};

                env.SendEventBean(new SupportMarketDataBean("ABC", 10.0, 50000L, "f1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"ABC", double.NaN, double.NaN, "f1"});

                env.Milestone(0);

                env.SendEventBean(new SupportMarketDataBean("DEF", 1.0, 1L, "f2"));
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    fields,
                    new object[] {"DEF", double.NaN, double.NaN, "f2"});
                Assert.AreEqual(1d, theEvent.Get("XAverage"));
                Assert.AreEqual(0d, theEvent.Get("XStandardDeviationPop"));
                Assert.AreEqual(double.NaN, theEvent.Get("XStandardDeviationSample"));
                Assert.AreEqual(1d, theEvent.Get("XSum"));
                Assert.AreEqual(double.NaN, theEvent.Get("XVariance"));
                Assert.AreEqual(1d, theEvent.Get("YAverage"));
                Assert.AreEqual(0d, theEvent.Get("YStandardDeviationPop"));
                Assert.AreEqual(double.NaN, theEvent.Get("YStandardDeviationSample"));
                Assert.AreEqual(1d, theEvent.Get("YSum"));
                Assert.AreEqual(double.NaN, theEvent.Get("YVariance"));
                Assert.AreEqual(1L, theEvent.Get("dataPoints"));
                Assert.AreEqual(1L, theEvent.Get("n"));
                Assert.AreEqual(1d, theEvent.Get("sumX"));
                Assert.AreEqual(1d, theEvent.Get("sumXSq"));
                Assert.AreEqual(1d, theEvent.Get("sumXY"));
                Assert.AreEqual(1d, theEvent.Get("sumY"));
                Assert.AreEqual(1d, theEvent.Get("sumYSq"));
                // above computed values tested in more detail in RegressionBean test

                env.SendEventBean(new SupportMarketDataBean("DEF", 2.0, 2L, "f3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"DEF", 1.0, 0.0, "f3"});

                env.SendEventBean(new SupportMarketDataBean("ABC", 11.0, 50100L, "f4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"ABC", 100.0, 49000.0, "f4"});

                env.UndeployAll();
            }
        }

        public class ViewGroupLengthWinWeightAvg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var useGroup = true;
                if (useGroup) {
                    // 0.69 sec for 100k
                    var stmtString =
                        "@Name('s0') select * from SupportSensorEvent#groupwin(Type)#length(10000000)#weighted_avg(Measurement, Confidence)";
                    env.CompileDeploy(stmtString).AddListener("s0");
                }
                else {
                    // 0.53 sec for 100k
                    for (var i = 0; i < 10; i++) {
                        var stmtString = "SELECT * FROM SupportSensorEvent(type='A" +
                                         i +
                                         "')#length(1000000)#weighted_avg(measurement,confidence)";
                        env.CompileDeploy(stmtString).AddListener("s0");
                    }
                }

                // prime
                for (var i = 0; i < 100; i++) {
                    env.SendEventBean(new SupportSensorEvent(0, "A", "1", i, i));
                }

                // measure
                long numEvents = 10000;
                var startTime = PerformanceObserver.NanoTime;
                for (var i = 0; i < numEvents; i++) {
                    //int modulo = i % 10;
                    var modulo = 1;
                    var type = "A" + modulo;
                    env.SendEventBean(new SupportSensorEvent(0, type, "1", i, i));

                    if (i % 1000 == 0) {
                        //System.out.println("Send " + i + " events");
                        env.Listener("s0");
                    }
                }

                var endTime = PerformanceObserver.NanoTime;
                var delta = (endTime - startTime) / 1000d / 1000d / 1000d;
                // System.out.println("delta=" + delta);
#if PRODUCTION_TESTING
                Assert.That(delta, Is.LessThan(1.0));
#else
                Assert.That(delta, Is.LessThan(5.0));
#endif

                env.UndeployAll();
            }
        }

        public class ViewGroupReclaimWithFlipTime : RegressionExecution
        {
            private readonly long flipTime;

            public ViewGroupReclaimWithFlipTime(long flipTime)
            {
                this.flipTime = flipTime;
            }

            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var epl =
                    "@Name('s0') @Hint('reclaim_group_aged=1,reclaim_group_freq=5') select * from SupportBean#groupwin(TheString)#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                AssertCount(env.Statement("s0"), 1);

                env.AdvanceTime(flipTime - 1);
                env.SendEventBean(new SupportBean("E2", 0));
                AssertCount(env.Statement("s0"), 2);

                env.Milestone(0);

                env.AdvanceTime(flipTime);
                env.SendEventBean(new SupportBean("E3", 0));
                AssertCount(env.Statement("s0"), 2);

                env.UndeployAll();
            }
        }

        public class ViewGroupTimeAccum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text =
                    "@Name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time_accum(10 sec)";
                env.CompileDeploy(text).AddListener("s0");

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                Assert.AreEqual(10d, env.Listener("s0").AssertOneGetNewAndReset().Get("Price"));

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                Assert.AreEqual(20d, env.Listener("s0").AssertOneGetNewAndReset().Get("Price"));

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                Assert.AreEqual(30d, env.Listener("s0").AssertOneGetNewAndReset().Get("Price"));
                env.Milestone(3);

                env.AdvanceTime(15000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {10d}, new object[] {20d}});
                env.Listener("s0").Reset();

                env.AdvanceTime(20000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {30d}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewGroupTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text =
                    "@Name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time_batch(10 sec)";
                env.CompileDeploy(text).AddListener("s0");

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.AdvanceTime(10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(11000);
                Assert.IsNull(env.Listener("s0").LastOldData);
                var newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Price"},
                    new[] {new object[] {10d}, new object[] {20d}});
                env.Listener("s0").Reset();

                env.Milestone(4);

                env.AdvanceTime(20000);
                Assert.IsNull(env.Listener("s0").LastOldData);
                newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Price"},
                    new[] {new object[] {30d}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                env.AdvanceTime(21000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {10d}, new object[] {20d}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewGroupTimeOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text =
                    "@Name('s0') select irstream * from SupportBeanTimestamp#groupwin(GroupId)#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // 1st event
                env.AdvanceTime(1000);
                SendEventTS(env, "E1", "G1", 3000);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(1);

                // 2nd event
                env.AdvanceTime(2000);
                SendEventTS(env, "E2", "G2", 2000);
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(2);

                // 3rd event
                env.AdvanceTime(3000);
                SendEventTS(env, "E3", "G2", 3000);
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(3);

                // 4th event
                env.AdvanceTime(4000);
                SendEventTS(env, "E4", "G1", 2500);
                Assert.AreEqual("E4", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(4);

                // Window pushes out event E2
                env.AdvanceTime(11999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(12000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                // Window pushes out event E4
                env.AdvanceTime(12499);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(12500);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E4"}});
                env.Listener("s0").Reset();

                env.Milestone(6);

                env.UndeployAll();
            }
        }

        public class ViewGroupTimeLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text =
                    "@Name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time_length_batch(10 sec, 100)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.AdvanceTime(10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(11000);
                Assert.IsNull(env.Listener("s0").LastOldData);
                var newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Price"},
                    new[] {new object[] {10d}, new object[] {20d}});
                env.Listener("s0").Reset();

                env.Milestone(4);

                env.AdvanceTime(20000);
                Assert.IsNull(env.Listener("s0").LastOldData);
                newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Price"},
                    new[] {new object[] {30d}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                env.AdvanceTime(21000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {10d}, new object[] {20d}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewGroupLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#length_batch(3) order by Symbol asc";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeMarketDataEvent("S1", 1));

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", 20));

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S2", 21));

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"Price"},
                    new[] {new object[] {1.0}, new object[] {2.0}, new object[] {20.0}, new object[] {21.0}});

                env.SendEventBean(MakeMarketDataEvent("S2", 22));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Price" },
                    new[] {new object[] {20.0}, new object[] {21.0}, new object[] {22.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Symbol" },
                    new[] {new object[] {"S2"}, new object[] {"S2"}, new object[] {"S2"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("S2", 23));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                env.SendEventBean(MakeMarketDataEvent("S1", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Price" },
                    new[] {new object[] {1.0}, new object[] {2.0}, new object[] {3.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Symbol" },
                    new[] {new object[] {"S1"}, new object[] {"S1"}, new object[] {"S1"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(7);

                env.SendEventBean(MakeMarketDataEvent("S2", 24));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(8);

                env.SendEventBean(MakeMarketDataEvent("S2", 25));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Price" },
                    new[] {new object[] {23.0}, new object[] {24.0}, new object[] {25.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Symbol" },
                    new[] {new object[] {"S2"}, new object[] {"S2"}, new object[] {"S2"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new [] { "Price" },
                    new[] {new object[] {20.0}, new object[] {21.0}, new object[] {22.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new [] { "Symbol" },
                    new[] {new object[] {"S2"}, new object[] {"S2"}, new object[] {"S2"}});
                env.Listener("s0").Reset();

                env.Milestone(9);

                env.SendEventBean(MakeMarketDataEvent("S1", 4));
                env.SendEventBean(MakeMarketDataEvent("S1", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(10);

                env.SendEventBean(MakeMarketDataEvent("S1", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Price" },
                    new[] {new object[] {4.0}, new object[] {5.0}, new object[] {6.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    new [] { "Symbol" },
                    new[] {new object[] {"S1"}, new object[] {"S1"}, new object[] {"S1"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new [] { "Price" },
                    new[] {new object[] {1.0}, new object[] {2.0}, new object[] {3.0}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    new [] { "Symbol" },
                    new[] {new object[] {"S1"}, new object[] {"S1"}, new object[] {"S1"}});
                env.Listener("s0").Reset();

                env.Milestone(11);

                env.UndeployAll();
            }
        }

        public class ViewGroupTimeWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text = "@Name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time(10 sec)";
                env.CompileDeploy(text).AddListener("s0");

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                Assert.AreEqual(10d, env.Listener("s0").AssertOneGetNewAndReset().Get("Price"));

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                Assert.AreEqual(20d, env.Listener("s0").AssertOneGetNewAndReset().Get("Price"));

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                Assert.AreEqual(30d, env.Listener("s0").AssertOneGetNewAndReset().Get("Price"));

                env.Milestone(3);

                env.AdvanceTime(10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(11000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {10d}});
                env.Listener("s0").Reset();

                env.Milestone(4);

                env.AdvanceTime(15000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {20d}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                env.AdvanceTime(20000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Price"},
                    new[] {new object[] {30d}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewGroupLengthWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };

                env.Milestone(0);

                var epl =
                    "@Name('s0') select irstream TheString as c0,IntPrimitive as c1 from SupportBean#groupwin(TheString)#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}});
                SendSupportBean(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});

                env.Milestone(3);

                SendSupportBean(env, "E1", 2);
                SendSupportBean(env, "E2", 21);
                SendSupportBean(env, "E2", 22);
                SendSupportBean(env, "E1", 3);
                Assert.AreEqual(0, env.Listener("s0").OldDataListFlattened.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {"E1", 2}, new object[] {"E2", 21}, new object[] {"E2", 22}, new object[] {"E1", 3}
                    });
                env.Listener("s0").Reset();

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E1", 3}, new object[] {"E2", 20},
                        new object[] {"E2", 21}, new object[] {"E2", 22}
                    });
                SendSupportBean(env, "E2", 23);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E2", 23},
                    new object[] {"E2", 20});

                env.Milestone(5);

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E1", 3}, new object[] {"E2", 23},
                        new object[] {"E2", 21}, new object[] {"E2", 22}
                    });
                SendSupportBean(env, "E1", -1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E1", -1},
                    new object[] {"E1", 1});

                env.UndeployAll();
            }
        }
    }
} // end of namespace