///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeOrderAndTimeToLive
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSceneTwo(execs);
            WithTTLTimeToLive(execs);
            WithTTLMonthScoped(execs);
            WithTTLTimeOrderRemoveStream(execs);
            WithTTLTimeOrder(execs);
            WithTTLGroupedWindow(execs);
            WithTTLInvalid(execs);
            WithTTLPreviousAndPriorSceneOne(execs);
            WithTTLPreviousAndPriorSceneTwo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTTLPreviousAndPriorSceneTwo(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLPreviousAndPriorSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLPreviousAndPriorSceneOne(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLPreviousAndPriorSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLGroupedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLGroupedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLTimeOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLTimeOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLTimeOrderRemoveStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLTimeOrderRemoveStream());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLTimeToLive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLTimeToLive());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderSceneOne());
            return execs;
        }

        internal class ViewTimeOrderSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "id".SplitCsv();
                env.AdvanceTime(1000);

                var text = "@name('s0') select irstream * from SupportBeanTimestamp#time_order(timestamp, 10 sec)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // 1st event
                env.AdvanceTime(1000);
                SendEvent(env, "E1", 3000);
                AssertIdReceived(env, "E1");

                env.Milestone(1);

                // 2nd event
                env.AdvanceTime(2000);
                SendEvent(env, "E2", 2000);
                AssertIdReceived(env, "E2");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E2" }, new object[] { "E1" } });

                env.Milestone(2);

                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E2" }, new object[] { "E1" } });

                // 3rd event
                env.AdvanceTime(3000);
                SendEvent(env, "E3", 3000);
                AssertIdReceived(env, "E3");

                env.Milestone(3);

                // 4th event
                env.AdvanceTime(4000);
                SendEvent(env, "E4", 2500);
                AssertIdReceived(env, "E4");

                env.Milestone(4);

                // Window pushes out event E2
                env.AdvanceTime(11999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(12000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E2" } });

                env.Milestone(5);

                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E4" }, new object[] { "E1" }, new object[] { "E3" } });

                // Window pushes out event E4
                env.AdvanceTime(12499);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(12500);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E4" } });

                env.Milestone(6);

                // Window pushes out event E1 and E3
                env.AdvanceTime(13000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E1" }, new object[] { "E3" } });

                env.Milestone(7);

                // E5
                env.AdvanceTime(14000);
                SendEvent(env, "E5", 14200);
                AssertIdReceived(env, "E5");

                env.Milestone(8);

                // E6
                env.AdvanceTime(14000);
                SendEvent(env, "E6", 14100);
                AssertIdReceived(env, "E6");

                env.Milestone(9);

                // E7
                env.AdvanceTime(15000);
                SendEvent(env, "E7", 15000);
                AssertIdReceived(env, "E7");

                env.Milestone(10);

                // E8
                env.AdvanceTime(15000);
                SendEvent(env, "E8", 14150);
                AssertIdReceived(env, "E8");

                env.Milestone(11);

                // Window pushes out events
                env.AdvanceTime(24500);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E6" }, new object[] { "E8" }, new object[] { "E5" } });

                env.Milestone(12);

                // Window pushes out events
                env.AdvanceTime(25000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E7" } });

                env.Milestone(13);

                // E9 is very old
                env.AdvanceTime(25000);
                SendEvent(env, "E9", 15000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E9" } },
                    new object[][] { new object[] { "E9" } });

                env.Milestone(14);

                // E10 at 26 sec
                env.AdvanceTime(26000);
                SendEvent(env, "E10", 26000);
                AssertIdReceived(env, "E10");

                env.Milestone(15);

                // E11 at 27 sec
                env.AdvanceTime(27000);
                SendEvent(env, "E11", 27000);
                AssertIdReceived(env, "E11");

                env.Milestone(16);

                // E12 and E13 at 25 sec
                env.AdvanceTime(28000);
                SendEvent(env, "E12", 25000);
                AssertIdReceived(env, "E12");
                SendEvent(env, "E13", 25000);
                AssertIdReceived(env, "E13");

                env.Milestone(17);

                // Window pushes out events
                env.AdvanceTime(35000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E12" }, new object[] { "E13" } });

                env.Milestone(18);

                // E10 at 26 sec
                env.AdvanceTime(35000);
                SendEvent(env, "E14", 26500);
                AssertIdReceived(env, "E14");

                env.Milestone(19);

                // Window pushes out events
                env.AdvanceTime(36000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E10" } });

                env.Milestone(20);
                // leaving 1 event in the window

                env.UndeployAll();
            }

            private void AssertIdReceived(
                RegressionEnvironment env,
                string expected)
            {
                env.AssertEqualsNew("s0", "id", expected);
            }
        }

        internal class ViewTimeOrderSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString,longPrimitive".SplitCsv();

                env.AdvanceTime(0);
                var epl = "@name('s0') select irstream * from SupportBean.ext:time_order(longPrimitive, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertPropsPerRowIterator("s0", fields, null);

                env.AdvanceTime(1000);
                SendSupportBeanWLong(env, "E1", 5000);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 5000L });

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1", 5000L } });
                env.AdvanceTime(2000);
                SendSupportBeanWLong(env, "E2", 4000);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 4000L });

                env.Milestone(2);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", 4000L }, new object[] { "E1", 5000L } });
                env.AdvanceTime(13999);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AdvanceTime(14000);
                env.AssertPropsOld("s0", fields, new object[] { "E2", 4000L });

                env.Milestone(4);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1", 5000L } });
                SendSupportBeanWLong(env, "E3", 5000);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 5000L });

                env.Milestone(5);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 5000L }, new object[] { "E3", 5000L } });
                env.AdvanceTime(14999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(15000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E1", 5000L }, new object[] { "E3", 5000L } });

                env.Milestone(6);

                SendSupportBeanWLong(env, "E4", 2500);
                env.AssertPropsIRPair("s0", fields, new object[] { "E4", 2500L }, new object[] { "E4", 2500L });

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                env.AdvanceTime(99999);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLTimeToLive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = "id".SplitCsv();
                var epl = "@name('s0') select irstream * from SupportBeanTimestamp#timetolive(timestamp)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                SendEvent(env, "E1", 1000);
                env.AssertPropsNew("s0", fields, new object[] { "E1" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(1);

                SendEvent(env, "E2", 500);
                env.AssertPropsNew("s0", fields, new object[] { "E2" });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E1" } });

                env.Milestone(2);

                env.AdvanceTime(499);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(500);
                env.AssertPropsOld("s0", fields, new object[] { "E2" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(3);

                SendEvent(env, "E3", 200);
                env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E3" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(4);

                SendEvent(env, "E4", 1200);
                env.AssertPropsNew("s0", fields, new object[] { "E4" });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E4" } });

                env.Milestone(5);

                SendEvent(env, "E5", 1000);
                env.AssertPropsNew("s0", fields, new object[] { "E5" });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E4" }, new object[] { "E5" } });

                env.AdvanceTime(999);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(1000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E1" }, new object[] { "E5" } });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E4" } });

                env.Milestone(6);

                env.AdvanceTime(1199);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(1200);
                env.AssertPropsOld("s0", fields, new object[] { "E4" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendEvent(env, "E6", 1200);
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E6" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                env.Milestone(7);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                env.CompileDeploy(
                        "@name('s0') select rstream * from SupportBeanTimestamp#time_order(timestamp, 1 month)")
                    .AddListener("s0");

                SendEvent(env, "E1", DateTimeParsingFunctions.ParseDefaultMSec("2002-02-01T09:00:00.000"));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.AssertListenerNotInvoked("s0");

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.AssertPropsPerRowLastNew("s0", "id".SplitCsv(), new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLTimeOrderRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "id".SplitCsv();
                SendTimer(env, 1000);
                var epl =
                    "insert rstream into OrderedStream select rstream id from SupportBeanTimestamp#time_order(timestamp, 10 sec);\n" +
                    "@name('s0') select * from OrderedStream";
                env.CompileDeploy(epl).AddListener("s0");

                // 1st event at 21 sec
                SendTimer(env, 21000);
                SendEvent(env, "E1", 21000);

                // 2nd event at 22 sec
                SendTimer(env, 22000);
                SendEvent(env, "E2", 22000);

                env.Milestone(0);

                // 3nd event at 28 sec
                SendTimer(env, 28000);
                SendEvent(env, "E3", 28000);

                // 4th event at 30 sec, however is 27 sec (old 3 sec)
                SendTimer(env, 30000);
                SendEvent(env, "E4", 27000);

                env.Milestone(1);

                // 5th event at 30 sec, however is 22 sec (old 8 sec)
                SendEvent(env, "E5", 22000);

                // flush one
                SendTimer(env, 30999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 31000);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E1" } }, null);

                // 6th event at 31 sec, however is 21 sec (old 10 sec)
                SendEvent(env, "E6", 21000);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E6" } }, null);

                // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
                SendEvent(env, "E7", 21300);

                // flush one
                SendTimer(env, 31299);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 31300);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E7" } }, null);

                // flush two
                SendTimer(env, 31999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 32000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E5" } },
                    null);

                // flush one
                SendTimer(env, 36999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 37000);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E4" } }, null);

                // rather old event
                SendEvent(env, "E8", 21000);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E8" } }, null);

                // 9-second old event for posting at 38 sec
                SendEvent(env, "E9", 28000);

                // flush two
                SendTimer(env, 37999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 38000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E9" } },
                    null);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLTimeOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "id".SplitCsv();
                SendTimer(env, 1000);

                var epl = "@name('s0') select irstream * from SupportBeanTimestamp#time_order(timestamp, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                SendTimer(env, 21000);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                env.Milestone(0);

                // 1st event at 21 sec
                SendEvent(env, "E1", 21000);
                AssertId(env, "E1");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E1" } });

                // 2nd event at 22 sec
                SendTimer(env, 22000);
                SendEvent(env, "E2", 22000);
                AssertId(env, "E2");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                // 3nd event at 28 sec
                SendTimer(env, 28000);
                SendEvent(env, "E3", 28000);
                AssertId(env, "E3");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(1);

                // 4th event at 30 sec, however is 27 sec (old 3 sec)
                SendTimer(env, 30000);
                SendEvent(env, "E4", 27000);
                AssertId(env, "E4");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E4" }, new object[] { "E3" } });

                // 5th event at 30 sec, however is 22 sec (old 8 sec)
                SendEvent(env, "E5", 22000);
                AssertId(env, "E5");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] {
                        new object[] { "E1" }, new object[] { "E2" }, new object[] { "E5" }, new object[] { "E4" },
                        new object[] { "E3" }
                    });

                // flush one
                SendTimer(env, 30999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 31000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E1" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][]
                        { new object[] { "E2" }, new object[] { "E5" }, new object[] { "E4" }, new object[] { "E3" } });

                // 6th event at 31 sec, however is 21 sec (old 10 sec)
                SendEvent(env, "E6", 21000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } },
                    new object[][] { new object[] { "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][]
                        { new object[] { "E2" }, new object[] { "E5" }, new object[] { "E4" }, new object[] { "E3" } });

                // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
                SendEvent(env, "E7", 21300);
                AssertId(env, "E7");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] {
                        new object[] { "E7" }, new object[] { "E2" }, new object[] { "E5" }, new object[] { "E4" },
                        new object[] { "E3" }
                    });

                // flush one
                SendTimer(env, 31299);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 31300);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E7" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][]
                        { new object[] { "E2" }, new object[] { "E5" }, new object[] { "E4" }, new object[] { "E3" } });

                // flush two
                SendTimer(env, 31999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 32000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E2" }, new object[] { "E5" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E4" }, new object[] { "E3" } });

                // flush one
                SendTimer(env, 36999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 37000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E4" } });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E3" } });

                // rather old event
                SendEvent(env, "E8", 21000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E8" } },
                    new object[][] { new object[] { "E8" } });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E3" } });

                // 9-second old event for posting at 38 sec
                SendEvent(env, "E9", 28000);
                AssertId(env, "E9");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E3" }, new object[] { "E9" } });

                // flush two
                SendTimer(env, 37999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 38000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E3" }, new object[] { "E9" } });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                // new event
                SendEvent(env, "E10", 38000);
                AssertId(env, "E10");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E10" } });

                // flush last
                SendTimer(env, 47999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 48000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E10" } });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                // last, in the future
                SendEvent(env, "E11", 70000);
                AssertId(env, "E11");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E11" } });

                SendTimer(env, 80000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E11" } });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                SendTimer(env, 100000);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLGroupedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "id".SplitCsv();
                SendTimer(env, 20000);
                var epl =
                    "@name('s0') select irstream * from SupportBeanTimestamp#groupwin(groupId)#time_order(timestamp, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");

                // 1st event is old
                SendEvent(env, "E1", "G1", 10000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" } },
                    new object[][] { new object[] { "E1" } });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                env.Milestone(0);

                // 2nd just fits
                SendEvent(env, "E2", "G2", 10001);
                AssertId(env, "E2");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E2" } });

                SendEvent(env, "E3", "G3", 20000);
                AssertId(env, "E3");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                SendEvent(env, "E4", "G2", 20000);
                AssertId(env, "E4");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E2" }, new object[] { "E4" }, new object[] { "E3" } });

                SendTimer(env, 20001);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E2" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E4" }, new object[] { "E3" } });

                env.Milestone(1);

                SendTimer(env, 22000);
                SendEvent(env, "E5", "G2", 19000);
                AssertId(env, "E5");
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E5" }, new object[] { "E4" }, new object[] { "E3" } });

                SendTimer(env, 29000);
                env.AssertPropsPerRowIRPair("s0", fields, null, new object[][] { new object[] { "E5" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "id" },
                    new object[][] { new object[] { "E4" }, new object[] { "E3" } });

                SendTimer(env, 30000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(2, listener.LastOldData.Length);
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            listener.LastOldData,
                            "id".SplitCsv(),
                            new object[][] { new object[] { "E4" }, new object[] { "E3" } });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, null);

                SendTimer(env, 100000);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportBeanTimestamp#time_order(bump, 10 sec)",
                    "Failed to validate data window declaration: Invalid parameter expression 0 for Time-Order view: Failed to validate view parameter expression 'bump': Property named 'bump' is not valid in any stream [");

                env.TryInvalidCompile(
                    "select * from SupportBeanTimestamp#time_order(10 sec)",
                    "Failed to validate data window declaration: Time-Order view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size [");

                env.TryInvalidCompile(
                    "select * from SupportBeanTimestamp#time_order(timestamp, abc)",
                    "Failed to validate data window declaration: Invalid parameter expression 1 for Time-Order view: Failed to validate view parameter expression 'abc': Property named 'abc' is not valid in any stream (did you mean 'id'?) [");
            }
        }

        internal class ViewTimeOrderTTLPreviousAndPriorSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var epl = "@name('s0') select irstream id, " +
                          " prev(0, id) as prevIdZero, " +
                          " prev(1, id) as prevIdOne, " +
                          " prior(1, id) as priorIdOne," +
                          " prevtail(0, id) as prevTailIdZero, " +
                          " prevtail(1, id) as prevTailIdOne, " +
                          " prevcount(id) as prevCountId, " +
                          " prevwindow(id) as prevWindowId " +
                          " from SupportBeanTimestamp#time_order(timestamp, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new string[]
                    { "id", "prevIdZero", "prevIdOne", "priorIdOne", "prevTailIdZero", "prevTailIdOne", "prevCountId" };

                SendTimer(env, 20000);
                SendEvent(env, "E1", 25000);
                AssertId(env, "E1");
                env.AssertPropsPerRowIterator("s0", new string[] { "id" }, new object[][] { new object[] { "E1" } });

                env.Milestone(0);

                SendEvent(env, "E2", 21000);
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual("E2", theEvent.Get("id"));
                        Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
                        Assert.AreEqual("E1", theEvent.Get("prevIdOne"));
                        Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
                        Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
                        Assert.AreEqual("E2", theEvent.Get("prevTailIdOne"));
                        Assert.AreEqual(2L, theEvent.Get("prevCountId"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])theEvent.Get("prevWindowId"),
                            new object[] { "E2", "E1" });
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", "E2", "E1", "E1", "E1", "E2", 2L },
                        new object[] { "E1", "E2", "E1", null, "E1", "E2", 2L }
                    });

                SendEvent(env, "E3", 22000);
                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        Assert.AreEqual("E3", theEvent.Get("id"));
                        Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
                        Assert.AreEqual("E3", theEvent.Get("prevIdOne"));
                        Assert.AreEqual("E2", theEvent.Get("priorIdOne"));
                        Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
                        Assert.AreEqual("E3", theEvent.Get("prevTailIdOne"));
                        Assert.AreEqual(3L, theEvent.Get("prevCountId"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])theEvent.Get("prevWindowId"),
                            new object[] { "E2", "E3", "E1" });
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", "E2", "E3", "E1", "E1", "E3", 3L },
                        new object[] { "E3", "E2", "E3", "E2", "E1", "E3", 3L },
                        new object[] { "E1", "E2", "E3", null, "E1", "E3", 3L }
                    });

                SendTimer(env, 31000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(1, listener.LastOldData.Length);
                        var theEvent = env.Listener("s0").LastOldData[0];
                        Assert.AreEqual("E2", theEvent.Get("id"));
                        Assert.IsNull(theEvent.Get("prevIdZero"));
                        Assert.IsNull(theEvent.Get("prevIdOne"));
                        Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
                        Assert.IsNull(theEvent.Get("prevTailIdZero"));
                        Assert.IsNull(theEvent.Get("prevTailIdOne"));
                        Assert.IsNull(theEvent.Get("prevCountId"));
                        Assert.IsNull(theEvent.Get("prevWindowId"));
                        env.Listener("s0").Reset();
                    });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E3", "E3", "E1", "E2", "E1", "E3", 2L },
                        new object[] { "E1", "E3", "E1", null, "E1", "E3", 2L }
                    });

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLPreviousAndPriorSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text = "@name('s0') select irstream id, " +
                           "prev(1, id) as prevId, " +
                           "prior(1, id) as priorId, " +
                           "prevtail(0, id) as prevtail, " +
                           "prevcount(id) as prevCountId, " +
                           "prevwindow(id) as prevWindowId " +
                           "from SupportBeanTimestamp#time_order(timestamp, 10 sec)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // event
                env.AdvanceTime(1000);
                SendEvent(env, "E1", 1000);
                env.AssertEventNew(
                    "s0",
                    @event => { AssertData(@event, "E1", null, null, "E1", 1L, new object[] { "E1" }); });

                env.Milestone(1);

                // event
                env.AdvanceTime(10000);
                SendEvent(env, "E2", 10000);
                env.AssertEventNew(
                    "s0",
                    @event => { AssertData(@event, "E2", "E2", "E1", "E2", 2L, new object[] { "E1", "E2" }); });

                env.Milestone(2);

                // event
                env.AdvanceTime(10500);
                SendEvent(env, "E3", 8000);
                env.AssertEventNew(
                    "s0",
                    @event => { AssertData(@event, "E3", "E3", "E2", "E2", 3L, new object[] { "E1", "E3", "E2" }); });

                env.Milestone(3);

                env.AdvanceTime(11000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        var oldData = listener.LastOldData;
                        AssertData(oldData[0], "E1", null, null, null, null, null);
                        listener.Reset();
                    });

                env.Milestone(4);

                // event
                env.AdvanceTime(12000);
                SendEvent(env, "E4", 7000);
                env.AssertEventNew(
                    "s0",
                    @event => { AssertData(@event, "E4", "E3", "E3", "E2", 3L, new object[] { "E4", "E3", "E2" }); });

                env.Milestone(5);

                env.AdvanceTime(16999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(17000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        var oldData = listener.LastOldData;
                        AssertData(oldData[0], "E4", null, "E3", null, null, null);
                        listener.Reset();
                    });

                env.Milestone(6);

                env.AdvanceTime(17999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(18000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        var oldData = listener.LastOldData;
                        AssertData(oldData[0], "E3", null, "E2", null, null, null);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        private static SupportBeanTimestamp SendEvent(
            RegressionEnvironment env,
            string id,
            string groupId,
            long timestamp)
        {
            var theEvent = new SupportBeanTimestamp(id, groupId, timestamp);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static SupportBeanTimestamp SendEvent(
            RegressionEnvironment env,
            string id,
            long timestamp)
        {
            var theEvent = new SupportBeanTimestamp(id, timestamp);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static void AssertData(
            EventBean @event,
            string id,
            string prevId,
            string priorId,
            string prevTailId,
            long? prevCountId,
            object[] prevWindowId)
        {
            Assert.AreEqual(id, @event.Get("id"));
            Assert.AreEqual(prevId, @event.Get("prevId"));
            Assert.AreEqual(priorId, @event.Get("priorId"));
            Assert.AreEqual(prevTailId, @event.Get("prevtail"));
            Assert.AreEqual(prevCountId, @event.Get("prevCountId"));
            EPAssertionUtil.AssertEqualsExactOrder(prevWindowId, (object[])@event.Get("prevWindowId"));
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

        private static void AssertId(
            RegressionEnvironment env,
            string expected)
        {
            env.AssertEqualsNew("s0", "id", expected);
        }
    }
} // end of namespace