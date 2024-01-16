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
using com.espertech.esper.common.@internal.util;

using NUnit.Framework;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using static com.espertech.esper.regressionlib.framework.RegressionFlag;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewGroup
    {
        private const string SYMBOL_CISCO = "CSCO.O";
        private const string SYMBOL_IBM = "IBM.N";
        private const string SYMBOL_GE = "GE.N";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithObjectArrayEvent(execs);
            WithStats(execs);
            WithReclaimTimeWindow(execs);
            WithReclaimAgedHint(execs);
            WithCorrel(execs);
            WithLinest(execs);
            WithMultiProperty(execs);
            WithInvalid(execs);
            WithLengthWinWeightAvg(execs);
            WithReclaimWithFlipTime(execs);
            WithTimeBatch(execs);
            WithTimeAccum(execs);
            WithTimeOrder(execs);
            WithTimeLengthBatch(execs);
            WithLengthWin(execs);
            WithLengthBatch(execs);
            WithTimeWin(execs);
            WithExpressionGrouped(execs);
            WithExpressionBatch(execs);
            WithEscapedPropertyText(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithEscapedPropertyText(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupEscapedPropertyText());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupExpressionBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupExpressionGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupTimeWin());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupLengthBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupLengthWin());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeLengthBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupTimeLengthBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupTimeOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeAccum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupTimeAccum());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupTimeBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithReclaimWithFlipTime(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupReclaimWithFlipTime(5000L));
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWinWeightAvg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupLengthWinWeightAvg());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupMultiProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithLinest(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupLinest());
            return execs;
        }

        public static IList<RegressionExecution> WithCorrel(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupCorrel());
            return execs;
        }

        public static IList<RegressionExecution> WithReclaimAgedHint(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupReclaimAgedHint());
            return execs;
        }

        public static IList<RegressionExecution> WithReclaimTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupReclaimTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithStats(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupStats());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectArrayEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewGroupObjectArrayEvent());
            return execs;
        }

        internal class ViewGroupEscapedPropertyText : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    $"create schema event as {typeof(EventWithTags).MaskTypeName()};\n\n" +
                    $"insert into stream1\nselect Name, Tags from event;\n\n" +
                    $"select Name, Tags('a\\.b') from stream1.std:groupwin(Name, Tags('a\\.b')).win:length(10)\nhaving count(1) >= 5;\n";
                env.CompileDeploy(epl).UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(SERDEREQUIRED);
            }
        }

        internal class ViewGroupInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select * from SupportBean#groupwin(TheString)#length(1)#groupwin(TheString)#uni(IntPrimitive)";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate data window declaration: Multiple groupwin-declarations are not supported");

                epl = "select avg(Price), Symbol from SupportMarketDataBean#length(100)#groupwin(Symbol)";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate data window declaration: Invalid use of the 'groupwin' view, the view requires one or more child views to group, or consider using the group-by clause");

                epl = "select * from SupportBean#keepall#groupwin(TheString)#length(2)";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate data window declaration: The 'groupwin' declaration must occur in the first position");

                epl = "select * from SupportBean#groupwin(TheString)#length(2)#merge(TheString)#keepall";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate data window declaration: The 'merge' declaration cannot be used in conjunction with multiple data windows");

#if false
                epl = "create schema MyEvent(somefield null);\n" +
                      "select * from MyEvent#groupwin(somefield)#length(2)";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate data window declaration: Group-window received a null-typed criteria expression");
#endif
            }
        }

        internal class ViewGroupMultiProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Symbol,Feed,Volume,size".SplitCsv();
                var symbolMsft = "MSFT";
                var symbolGe = "GE";
                var feedInfo = "INFO";
                var feedReu = "REU";

                // Listen to all ticks
                var epl = "@name('s0') select irstream datapoints as size, Symbol, Feed, Volume " +
                          "from SupportMarketDataBean#groupwin(Symbol, Feed, Volume)#uni(Price) order by Symbol, Feed, Volume";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, symbolGe, feedInfo, 1);
                var row0GEOld = new object[] { symbolGe, feedInfo, 1L, 0L };
                var row0GENew = new object[] { symbolGe, feedInfo, 1L, 1L };
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { row0GENew }, new object[][] { row0GEOld });

                env.Milestone(0);

                // Send a couple of events
                SendEvent(env, symbolGe, feedInfo, 1);
                SendEvent(env, symbolGe, feedInfo, 2);
                SendEvent(env, symbolGe, feedInfo, 1);
                env.ListenerReset("s0");

                env.Milestone(1);

                SendEvent(env, symbolGe, feedReu, 99);
                SendEvent(env, symbolMsft, feedInfo, 100);

                var row1GENew = new object[] { symbolGe, feedReu, 99L, 1L };
                var row1GEOld = new object[] { symbolGe, feedReu, 99L, 0L };
                var row1MSOld = new object[] { symbolMsft, feedInfo, 100L, 0L };
                var row1MSNew = new object[] { symbolMsft, feedInfo, 100L, 1L };
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { row1GENew, row1MSNew },
                    new object[][] { row1GEOld, row1MSOld });

                row0GENew = new object[] { symbolGe, feedInfo, 1L, 3L };
                var row2GENew = new object[] { symbolGe, feedInfo, 2L, 1L };
                var row3GENew = new object[] { symbolGe, feedReu, 99L, 1L };
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { row0GENew, row2GENew, row3GENew, row1MSNew });

                env.UndeployAll();
            }
        }

        internal class ViewGroupExpressionBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl = "@name('create_var') create variable long ENGINE_TIME;\n" +
                          "@name('runtime_time_update') on pattern[every timer:interval(10 seconds)] set ENGINE_TIME = current_timestamp();\n" +
                          "@name('out_null') select window(*) from SupportBean#groupwin(TheString)#expr_batch(oldest_timestamp.plus(9 seconds) < ENGINE_TIME);";
                env.CompileDeploy(epl).AddListener("out_null");

                env.AdvanceTime(5000);
                env.AdvanceTime(10000);
                env.AdvanceTime(11000);

                env.AssertListenerNotInvoked("out_null");

                env.UndeployAll();
            }
        }

        internal class ViewGroupObjectArrayEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "P1,sp2".SplitCsv();
                var epl = "@name('s0') select P1,sum(P2) as sp2 from OAEventStringInt#groupwin(P1)#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventObjectArray(new object[] { "A", 10 }, "OAEventStringInt");
                env.AssertPropsNew("s0", fields, new object[] { "A", 10 });

                env.SendEventObjectArray(new object[] { "B", 11 }, "OAEventStringInt");
                env.AssertPropsNew("s0", fields, new object[] { "B", 21 });

                env.Milestone(0);

                env.SendEventObjectArray(new object[] { "A", 12 }, "OAEventStringInt");
                env.AssertPropsNew("s0", fields, new object[] { "A", 33 });

                env.SendEventObjectArray(new object[] { "A", 13 }, "OAEventStringInt");
                env.AssertPropsNew("s0", fields, new object[] { "A", 36 });

                env.UndeployAll();
            }
        }

        internal class ViewGroupReclaimTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var epl = "@name('s0') @Hint('reclaim_group_aged=30,reclaim_group_freq=5') " +
                          "select LongPrimitive, count(*) from SupportBean#groupwin(TheString)#time(3000000)";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 10; i++) {
                    var theEvent = new SupportBean(i.ToString(), i);
                    env.SendEventBean(theEvent);
                }

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(10, SupportScheduleHelper.ScheduleCount(statement)));

                env.Milestone(0);

                env.AdvanceTime(1000000);
                env.SendEventBean(new SupportBean("E1", 1));

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(1, SupportScheduleHelper.ScheduleCount(statement)));

                env.UndeployAll();

                env.AssertRuntime(rt => Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(rt)));
            }
        }

        internal class ViewGroupReclaimAgedHint : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl = "@name('s0') @Hint('reclaim_group_aged=5,reclaim_group_freq=1') " +
                          "select * from SupportBean#groupwin(TheString)#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                var maxEventsPerSlot = 1000;
                for (var timeSlot = 0; timeSlot < 10; timeSlot++) {
                    env.AdvanceTime(timeSlot * 1000 + 1);

                    for (var i = 0; i < maxEventsPerSlot; i++) {
                        env.SendEventBean(new SupportBean($"E{timeSlot}", 0));
                    }
                }

                env.Milestone(0);

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var events = EPAssertionUtil.EnumeratorToArray(iterator);
                        Assert.IsTrue(events.Length <= 6 * maxEventsPerSlot);
                    });

                env.SendEventBean(new SupportBean("E0", 1));

                env.Milestone(1);

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var events = EPAssertionUtil.EnumeratorToArray(iterator);
                        Assert.AreEqual(6 * maxEventsPerSlot + 1, events.Length);
                    });

                env.UndeployAll();
            }
        }

        internal class ViewGroupStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;
                var filter = "select * from SupportMarketDataBean";

                epl = $"@name('priceLast3Stats'){filter}#groupwin(Symbol)#length(3)#uni(Price) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("priceLast3Stats");

                epl = $"@name('volumeLast3Stats'){filter}#groupwin(Symbol)#length(3)#uni(Volume) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("volumeLast3Stats");

                epl = $"@name('priceAllStats'){filter}#groupwin(Symbol)#uni(Price) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("priceAllStats");

                epl = $"@name('volumeAllStats'){filter}#groupwin(Symbol)#uni(Volume) order by Symbol asc";
                env.CompileDeploy(epl).AddListener("volumeAllStats");

                var expectedList = new List<IDictionary<string, object>>();
                for (var i = 0; i < 3; i++) {
                    expectedList.Add(new Dictionary<string, object>());
                }

                SendEvent(env, SYMBOL_CISCO, 25, 50000);
                SendEvent(env, SYMBOL_CISCO, 26, 60000);
                SendEvent(env, SYMBOL_IBM, 10, 8000);
                SendEvent(env, SYMBOL_IBM, 10.5, 8200);
                SendEvent(env, SYMBOL_GE, 88, 1000);

                AssertLastNewRow(env, "priceLast3Stats", SYMBOL_GE, 88);
                AssertLastNewRow(env, "priceAllStats", SYMBOL_GE, 88);
                AssertLastNewRow(env, "volumeLast3Stats", SYMBOL_GE, 1000);
                AssertLastNewRow(env, "volumeAllStats", SYMBOL_GE, 1000);

                SendEvent(env, SYMBOL_CISCO, 27, 70000);
                SendEvent(env, SYMBOL_CISCO, 28, 80000);

                AssertLastNewRow(env, "priceAllStats", SYMBOL_CISCO, 26.5d);
                AssertLastNewRow(env, "volumeAllStats", SYMBOL_CISCO, 65000d);
                AssertLastNewRow(env, "priceLast3Stats", SYMBOL_CISCO, 27d);
                AssertLastNewRow(env, "volumeLast3Stats", SYMBOL_CISCO, 70000d);

                SendEvent(env, SYMBOL_IBM, 11, 8700);
                SendEvent(env, SYMBOL_IBM, 12, 8900);

                AssertLastNewRow(env, "priceAllStats", SYMBOL_IBM, 10.875d);
                AssertLastNewRow(env, "volumeAllStats", SYMBOL_IBM, 8450d);
                AssertLastNewRow(env, "priceLast3Stats", SYMBOL_IBM, 11d + 1 / 6d);
                AssertLastNewRow(env, "volumeLast3Stats", SYMBOL_IBM, 8600d);

                SendEvent(env, SYMBOL_GE, 85.5, 950);
                SendEvent(env, SYMBOL_GE, 85.75, 900);
                SendEvent(env, SYMBOL_GE, 89, 1250);
                SendEvent(env, SYMBOL_GE, 86, 1200);
                SendEvent(env, SYMBOL_GE, 85, 1150);

                var averageGE = (88d + 85.5d + 85.75d + 89d + 86d + 85d) / 6d;
                AssertLastNewRow(env, "priceAllStats", SYMBOL_GE, averageGE);
                AssertLastNewRow(env, "volumeAllStats", SYMBOL_GE, 1075d);
                AssertLastNewRow(env, "priceLast3Stats", SYMBOL_GE, 86d + 2d / 3d);
                AssertLastNewRow(env, "volumeLast3Stats", SYMBOL_GE, 1200d);

                // Check iterator results
                var fields = new string[] { "Symbol", "average" };
                env.AssertPropsPerRowIterator(
                    "priceAllStats",
                    fields,
                    new object[][] {
                        new object[] { SYMBOL_CISCO, 26.5d }, new object[] { SYMBOL_GE, averageGE },
                        new object[] { SYMBOL_IBM, 10.875d }
                    });
                env.AssertPropsPerRowIterator(
                    "priceLast3Stats",
                    fields,
                    new object[][] {
                        new object[] { SYMBOL_CISCO, 27d }, new object[] { SYMBOL_GE, 86d + 2d / 3d },
                        new object[] { SYMBOL_IBM, 11d + 1 / 6d }
                    });

                env.UndeployAll();
            }
        }

        internal class ViewGroupExpressionGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select irstream * from SupportBeanTimestamp#groupwin(Timestamp.getDayOfWeek())#length(2)";
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
                env.AssertListener(
                    "s0",
                    listener => { Assert.AreEqual(1, listener.DataListsFlattened.Second.Length); });

                env.UndeployAll();
            }
        }

        internal class ViewGroupCorrel : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // further math tests can be found in the view unit test
                var epl =
                    "@name('s0') select * from SupportMarketDataBean#groupwin(Symbol)#length(1000000)#correl(Price, Volume, Feed)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("correlation"));
                    });

                var fields = new string[] { "Symbol", "correlation", "Feed" };

                env.SendEventBean(new SupportMarketDataBean("ABC", 10.0, 1000L, "f1"));
                env.AssertPropsNew("s0", fields, new object[] { "ABC", double.NaN, "f1" });

                env.SendEventBean(new SupportMarketDataBean("DEF", 1.0, 2L, "f2"));
                env.AssertPropsNew("s0", fields, new object[] { "DEF", double.NaN, "f2" });

                env.Milestone(0);

                env.SendEventBean(new SupportMarketDataBean("DEF", 2.0, 4L, "f3"));
                env.AssertPropsNew("s0", fields, new object[] { "DEF", 1.0, "f3" });

                env.SendEventBean(new SupportMarketDataBean("ABC", 20.0, 2000L, "f4"));
                env.AssertPropsNew("s0", fields, new object[] { "ABC", 1.0, "f4" });

                env.UndeployAll();
            }
        }

        internal class ViewGroupLinest : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // further math tests can be found in the view unit test
                var epl =
                    "@name('s0') select * from SupportMarketDataBean#groupwin(Symbol)#length(1000000)#linest(Price, Volume, Feed)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
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
                    });

                var fields = new string[] { "Symbol", "slope", "YIntercept", "Feed" };

                env.SendEventBean(new SupportMarketDataBean("ABC", 10.0, 50000L, "f1"));
                env.AssertPropsNew("s0", fields, new object[] { "ABC", double.NaN, double.NaN, "f1" });

                env.Milestone(0);

                env.SendEventBean(new SupportMarketDataBean("DEF", 1.0, 1L, "f2"));
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        EPAssertionUtil.AssertProps(
                            theEvent,
                            fields,
                            new object[] { "DEF", double.NaN, double.NaN, "f2" });
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
                    });

                env.SendEventBean(new SupportMarketDataBean("DEF", 2.0, 2L, "f3"));
                env.AssertPropsNew("s0", fields, new object[] { "DEF", 1.0, 0.0, "f3" });

                env.SendEventBean(new SupportMarketDataBean("ABC", 11.0, 50100L, "f4"));
                env.AssertPropsNew("s0", fields, new object[] { "ABC", 100.0, 49000.0, "f4" });

                env.UndeployAll();
            }
        }

        internal class ViewGroupLengthWinWeightAvg : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(EXCLUDEWHENINSTRUMENTED, PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var useGroup = true;
                if (useGroup) {
                    // 0.69 sec for 100k
                    var stmtString =
                        "@name('s0') select * from SupportSensorEvent#groupwin(Type)#length(10000000)#weighted_avg(Measurement, Confidence)";
                    env.CompileDeploy(stmtString).AddListener("s0");
                }
                else {
                    // 0.53 sec for 100k
                    for (var i = 0; i < 10; i++) {
                        var stmtString =
                            $"SELECT * FROM SupportSensorEvent(Type='A{i}')#length(1000000)#weighted_avg(Measurement,Confidence)";
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
                    var type = $"A{modulo}";
                    env.SendEventBean(new SupportSensorEvent(0, type, "1", i, i));
                }

                var endTime = PerformanceObserver.NanoTime;
                var delta = (endTime - startTime) / 1000d / 1000d / 1000d;
                // Console.WriteLine("delta=" + delta);
                Assert.Less(delta, 1);

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
                    "@name('s0') @Hint('reclaim_group_aged=1,reclaim_group_freq=5') select * from SupportBean#groupwin(TheString)#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertStatement("s0", statement => { AssertCount(statement, 1); });

                env.AdvanceTime(flipTime - 1);
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertStatement("s0", statement => { AssertCount(statement, 2); });

                env.Milestone(0);

                env.AdvanceTime(flipTime);
                env.SendEventBean(new SupportBean("E3", 0));
                env.AssertStatement("s0", statement => { AssertCount(statement, 2); });

                env.UndeployAll();
            }
        }

        internal class ViewGroupTimeAccum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Price".SplitCsv();
                env.AdvanceTime(1000);

                var text =
                    "@name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time_accum(10 sec)";
                env.CompileDeploy(text).AddListener("s0");

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                AssertPrice(env, 10d);

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                AssertPrice(env, 20d);

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                AssertPrice(env, 30d);

                env.Milestone(3);

                env.AdvanceTime(15000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { 10d }, new object[] { 20d } });

                env.AdvanceTime(20000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { 30d } });

                env.UndeployAll();
            }
        }

        internal class ViewGroupTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Price".SplitCsv();
                env.AdvanceTime(1000);

                var text =
                    "@name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time_batch(10 sec)";
                env.CompileDeploy(text).AddListener("s0");

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AdvanceTime(10999);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(11000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { 10d }, new object[] { 20d } },
                    null);

                env.Milestone(4);

                env.AdvanceTime(20000);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { 30d } }, null);

                env.Milestone(5);

                env.AdvanceTime(21000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { 10d }, new object[] { 20d } });

                env.UndeployAll();
            }
        }

        internal class ViewGroupTimeOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Id".SplitCsv();
                env.AdvanceTime(1000);

                var text =
                    "@name('s0') select irstream * from SupportBeanTimestamp#groupwin(GroupId)#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // 1st event
                env.AdvanceTime(1000);
                SendEventTS(env, "E1", "G1", 3000);
                AssertId(env, "E1");

                env.Milestone(1);

                // 2nd event
                env.AdvanceTime(2000);
                SendEventTS(env, "E2", "G2", 2000);
                AssertId(env, "E2");

                env.Milestone(2);

                // 3rd event
                env.AdvanceTime(3000);
                SendEventTS(env, "E3", "G2", 3000);
                AssertId(env, "E3");

                env.Milestone(3);

                // 4th event
                env.AdvanceTime(4000);
                SendEventTS(env, "E4", "G1", 2500);
                AssertId(env, "E4");

                env.Milestone(4);

                // Window pushes out event E2
                env.AdvanceTime(11999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(12000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E2" } });

                env.Milestone(5);

                // Window pushes out event E4
                env.AdvanceTime(12499);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(12500);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E4" } });

                env.Milestone(6);

                env.UndeployAll();
            }
        }

        internal class ViewGroupTimeLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Price".SplitCsv();
                env.AdvanceTime(1000);

                var text =
                    "@name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time_length_batch(10 sec, 100)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AdvanceTime(10999);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(11000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { 10d }, new object[] { 20d } },
                    null);

                env.Milestone(4);

                env.AdvanceTime(20000);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { 30d } }, null);

                env.Milestone(5);

                env.AdvanceTime(21000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { 10d }, new object[] { 20d } });

                env.UndeployAll();
            }
        }

        internal class ViewGroupLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Symbol,Price".SplitCsv();
                var text =
                    "@name('s0') select irstream * from SupportMarketDataBean#groupwin(Symbol)#length_batch(3) order by Symbol asc";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeMarketDataEvent("S1", 1));

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", 20));

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S2", 21));

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", 2));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "Price" },
                    new object[][]
                        { new object[] { 1.0 }, new object[] { 2.0 }, new object[] { 20.0 }, new object[] { 21.0 } });

                env.SendEventBean(MakeMarketDataEvent("S2", 22));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "S2", 20.0 }, new object[] { "S2", 21.0 }, new object[] { "S2", 22.0 } },
                    null);

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("S2", 23));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(6);

                env.SendEventBean(MakeMarketDataEvent("S1", 3));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "S1", 1.0 }, new object[] { "S1", 2.0 }, new object[] { "S1", 3.0 } },
                    null);

                env.Milestone(7);

                env.SendEventBean(MakeMarketDataEvent("S2", 24));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(8);

                env.SendEventBean(MakeMarketDataEvent("S2", 25));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "S2", 23.0 }, new object[] { "S2", 24.0 }, new object[] { "S2", 25.0 } },
                    new object[][]
                        { new object[] { "S2", 20.0 }, new object[] { "S2", 21.0 }, new object[] { "S2", 22.0 } });

                env.Milestone(9);

                env.SendEventBean(MakeMarketDataEvent("S1", 4));
                env.SendEventBean(MakeMarketDataEvent("S1", 5));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(10);

                env.SendEventBean(MakeMarketDataEvent("S1", 6));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "S1", 4.0 }, new object[] { "S1", 5.0 }, new object[] { "S1", 6.0 } },
                    new object[][]
                        { new object[] { "S1", 1.0 }, new object[] { "S1", 2.0 }, new object[] { "S1", 3.0 } });

                env.Milestone(11);

                env.UndeployAll();
            }
        }

        internal class ViewGroupTimeWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Price".SplitCsv();
                env.AdvanceTime(1000);

                var text = "@name('s0') select irstream * from  SupportMarketDataBean#groupwin(Symbol)#time(10 sec)";
                env.CompileDeploy(text).AddListener("s0");

                // 1st event S1 group
                env.AdvanceTime(1000);
                SendEvent(env, "S1", 10);
                AssertPrice(env, 10d);

                env.Milestone(1);

                // 2nd event S1 group
                env.AdvanceTime(5000);
                SendEvent(env, "S1", 20);
                AssertPrice(env, 20d);

                env.Milestone(2);

                // 1st event S2 group
                env.AdvanceTime(10000);
                SendEvent(env, "S2", 30);
                AssertPrice(env, 30d);

                env.Milestone(3);

                env.AdvanceTime(10999);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(11000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { 10d } });

                env.Milestone(4);

                env.AdvanceTime(15000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { 20d } });

                env.Milestone(5);

                env.AdvanceTime(20000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { 30d } });

                env.UndeployAll();
            }
        }

        internal class ViewGroupLengthWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();

                env.Milestone(0);

                var epl =
                    "@name('s0') select irstream TheString as c0,IntPrimitive as c1 from SupportBean#groupwin(TheString)#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                SendSupportBean(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });

                env.Milestone(3);

                SendSupportBean(env, "E1", 2);
                SendSupportBean(env, "E2", 21);
                SendSupportBean(env, "E2", 22);
                SendSupportBean(env, "E1", 3);
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 2 }, new object[] { "E2", 21 }, new object[] { "E2", 22 },
                        new object[] { "E1", 3 }
                    });

                env.Milestone(4);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1 }, new object[] { "E1", 2 }, new object[] { "E1", 3 },
                        new object[] { "E2", 20 }, new object[] { "E2", 21 }, new object[] { "E2", 22 }
                    });
                SendSupportBean(env, "E2", 23);
                env.AssertPropsIRPair("s0", fields, new object[] { "E2", 23 }, new object[] { "E2", 20 });

                env.Milestone(5);

                env.Milestone(6);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1 }, new object[] { "E1", 2 }, new object[] { "E1", 3 },
                        new object[] { "E2", 23 }, new object[] { "E2", 21 }, new object[] { "E2", 22 }
                    });
                SendSupportBean(env, "E1", -1);
                env.AssertPropsIRPair("s0", fields, new object[] { "E1", -1 }, new object[] { "E1", 1 });

                env.UndeployAll();
            }
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

        private static void AssertLastNewRow(
            RegressionEnvironment env,
            string statementName,
            string symbol,
            double average)
        {
            var fields = "Symbol,average".SplitCsv();
            env.AssertListener(
                statementName,
                listener => EPAssertionUtil.AssertPropsPerRow(
                    listener.LastNewData,
                    fields,
                    new object[][] { new object[] { symbol, average } }));
        }

        private static void AssertId(
            RegressionEnvironment env,
            string expected)
        {
            env.AssertEqualsNew("s0", "Id", expected);
        }

        private static void AssertPrice(
            RegressionEnvironment env,
            double expected)
        {
            env.AssertEqualsNew("s0", "Price", expected);
        }

        public class EventWithTags
        {
            private string name;
            private IDictionary<string, string> tags;

            public string GetName()
            {
                return name;
            }

            public void SetName(string name)
            {
                this.name = name;
            }

            public IDictionary<string, string> GetTags()
            {
                return tags;
            }

            public void SetTags(IDictionary<string, string> tags)
            {
                this.tags = tags;
            }
        }
    }
} // end of namespace