///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;


namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewExternallyTimedBatched
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithedNoReference(execs);
            WithedWithRefTime(execs);
            WithRefWithPrev(execs);
            WithMonthScoped(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedBatchMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithRefWithPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedBatchRefWithPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithedWithRefTime(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedBatchedWithRefTime());
            return execs;
        }

        public static IList<RegressionExecution> WithedNoReference(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedBatchedNoReference());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExternallyTimedBatchSceneOne());
            return execs;
        }

        public class ViewExternallyTimedBatchSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var epl =
                    "@name('s0') select irstream theString as c0 from SupportBean#ext_timed_batch(longPrimitive, 10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBeanWLong(env, "E1", 1000); // reference point is 1000, every batch is 11000/21000/31000...
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendSupportBeanWLong(env, "E2", 5000);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                SendSupportBeanWLong(env, "E3", 11000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } },
                    null);

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E3" } });
                SendSupportBeanWLong(env, "E4", 0);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });
                SendSupportBeanWLong(env, "E5", 21000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(5);
                env.Milestone(6);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E5" } });
                SendSupportBeanWLong(env, "E6", 31000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" } },
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });
                SendSupportBeanWLong(env, "E7", 41000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } },
                    new object[][] { new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        private class ViewExternallyTimedBatchedNoReference : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "id".SplitCsv();
                var epl =
                    "@name('s0') select irstream * from SupportEventIdWithTimestamp#ext_timed_batch(mytimestamp, 1 minute)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E1", "8:00:00.000"));
                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E2", "8:00:30.000"));

                env.Milestone(1);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E3", "8:00:59.999"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E4", "8:01:00.000"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } },
                    null);

                env.Milestone(3);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E5", "8:01:02.000"));
                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E6", "8:01:05.000"));

                env.Milestone(4);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E7", "8:02:00.000"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(5);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E8", "8:03:59.000"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E9", "8:03:59.000"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(6);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E10", "8:04:00.000"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E8" }, new object[] { "E9" } },
                    new object[][] { new object[] { "E7" } });

                env.Milestone(7);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E11", "8:06:30.000"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E10" } },
                    new object[][] { new object[] { "E8" }, new object[] { "E9" } });

                env.Milestone(8);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E12", "8:06:59.999"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(9);

                env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E13", "8:07:00.001"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E11" }, new object[] { "E12" } },
                    new object[][] { new object[] { "E10" } });

                env.UndeployAll();
            }
        }

        private class ViewExternallyTimedBatchedWithRefTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl =
                    "@name('s0') select irstream * from SupportEventIdWithTimestamp#ext_timed_batch(mytimestamp, 1 minute, 5000)";
                TryAssertionWithRefTime(env, epl, milestone);

                epl =
                    "@name('s0') select irstream * from SupportEventIdWithTimestamp#ext_timed_batch(mytimestamp, 1 minute, 65000)";
                TryAssertionWithRefTime(env, epl, milestone);
            }
        }

        public class ViewExternallyTimedBatchRefWithPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields =
                    "currSymbol,prev0Symbol,prev0Price,prev1Symbol,prev1Price,prev2Symbol,prev2Price,prevTail0Symbol,prevTail0Price,prevTail1Symbol,prevTail1Price,prevCountPrice,prevWindowPrice"
                        .SplitCsv();
                var epl = "@name('s0') select irstream symbol as currSymbol, " +
                          "prev(0, symbol) as prev0Symbol, " +
                          "prev(0, price) as prev0Price, " +
                          "prev(1, symbol) as prev1Symbol, " +
                          "prev(1, price) as prev1Price, " +
                          "prev(2, symbol) as prev2Symbol, " +
                          "prev(2, price) as prev2Price," +
                          "prevtail(0, symbol) as prevTail0Symbol, " +
                          "prevtail(0, price) as prevTail0Price, " +
                          "prevtail(1, symbol) as prevTail1Symbol, " +
                          "prevtail(1, price) as prevTail1Price, " +
                          "prevcount(price) as prevCountPrice, " +
                          "prevwindow(price) as prevWindowPrice " +
                          "from SupportMarketDataBean#ext_timed_batch(volume, 10, 0L) ";
                env.CompileDeploy(epl).AddListener("s0");

                SendMarketEvent(env, "A", 1, 1000);

                env.Milestone(0);

                SendMarketEvent(env, "B", 2, 1001);

                env.Milestone(1);

                SendMarketEvent(env, "C", 3, 1002);

                env.Milestone(2);

                SendMarketEvent(env, "D", 4, 10000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] {
                        new object[]
                            { "A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d") },
                        new object[]
                            { "B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d") },
                        new object[] { "C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d") }
                    },
                    null);

                env.Milestone(3);

                SendMarketEvent(env, "E", 5, 20000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] {
                        new object[]
                            { "D", "D", 4d, null, null, null, null, "D", 4d, null, null, 1L, SplitDoubles("4d") },
                    },
                    new object[][] {
                        new object[] { "A", null, null, null, null, null, null, null, null, null, null, null, null },
                        new object[] { "B", null, null, null, null, null, null, null, null, null, null, null, null },
                        new object[] { "C", null, null, null, null, null, null, null, null, null, null, null, null },
                    }
                );

                env.UndeployAll();
            }
        }

        private static void TryAssertionWithRefTime(
            RegressionEnvironment env,
            string epl,
            AtomicLong milestone)
        {
            var fields = "id".SplitCsv();
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E1", "8:00:00.000"));
            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E2", "8:00:04.999"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E3", "8:00:05.000"));
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E1" }, new object[] { "E2" } },
                null);

            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E4", "8:00:04.000"));
            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E5", "7:00:00.000"));
            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E6", "8:01:04.999"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E7", "8:01:05.000"));
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][]
                    { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                new object[][] { new object[] { "E1" }, new object[] { "E2" } });

            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E8", "8:03:55.000"));
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E7" } },
                new object[][]
                    { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E9", "0:00:00.000"));
            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E10", "8:04:04.999"));
            env.SendEventBean(SupportEventIdWithTimestamp.MakeTime("E11", "8:04:05.000"));
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E8" }, new object[] { "E9" }, new object[] { "E10" } },
                new object[][] { new object[] { "E7" } });

            env.UndeployAll();
        }

        private class ViewExternallyTimedBatchMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean#ext_timed_batch(longPrimitive, 1 month)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendExtTimeEvent(env, DateTimeParsingFunctions.ParseDefaultMSec("2002-02-01T09:00:00.000"), "E1");
                SendExtTimeEvent(env, DateTimeParsingFunctions.ParseDefaultMSec("2002-03-01T09:00:00.000") - 1, "E2");
                env.AssertListenerNotInvoked("s0");

                SendExtTimeEvent(env, DateTimeParsingFunctions.ParseDefaultMSec("2002-03-01T09:00:00.000"), "E3");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "theString".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        private static void SendMarketEvent(
            RegressionEnvironment env,
            string symbol,
            double price,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        private static object[] SplitDoubles(string doubleList)
        {
            var doubles = doubleList.SplitCsv();
            var result = new object[doubles.Length];
            for (var i = 0; i < result.Length; i++) {
                result[i] = double.Parse(doubles[i]);
            }

            return result;
        }

        private static void SendSupportBeanWLong(
            RegressionEnvironment env,
            string @string,
            long longPrimitive)
        {
            var sb = new SupportBean(@string, 0);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
        }

        private static void SendExtTimeEvent(
            RegressionEnvironment env,
            long longPrimitive,
            string theString)
        {
            var theEvent = new SupportBean(theString, 0);
            theEvent.LongPrimitive = longPrimitive;
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace