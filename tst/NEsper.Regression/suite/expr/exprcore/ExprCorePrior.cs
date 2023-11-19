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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCorePrior
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithBoundedMultiple(execs);
            WithExtTimedWindow(execs);
            WithTimeBatchWindow(execs);
            WithNoDataWindowWhere(execs);
            WithLengthWindowWhere(execs);
            WithStreamAndVariable(execs);
            WithUnbound(execs);
            WithUnboundSceneOne(execs);
            WithUnboundSceneTwo(execs);
            WithBoundedSingle(execs);
            WithLongRunningSingle(execs);
            WithLongRunningUnbound(execs);
            WithLongRunningMultiple(execs);
            WithTimewindowStats(execs);
            WithTimeWindow(execs);
            WithLengthWindow(execs);
            WithLengthWindowSceneTwo(execs);
            WithSortWindow(execs);
            WithTimeBatchWindowJoin(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchWindowJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorTimeBatchWindowJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSortWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorSortWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindowSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorLengthWindowSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorLengthWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimewindowStats(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorTimewindowStats());
            return execs;
        }

        public static IList<RegressionExecution> WithLongRunningMultiple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorLongRunningMultiple());
            return execs;
        }

        public static IList<RegressionExecution> WithLongRunningUnbound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorLongRunningUnbound());
            return execs;
        }

        public static IList<RegressionExecution> WithLongRunningSingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorLongRunningSingle());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundedSingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorBoundedSingle());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorUnboundSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorUnboundSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithUnbound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorUnbound());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamAndVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorStreamAndVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindowWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorLengthWindowWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithNoDataWindowWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorNoDataWindowWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorTimeBatchWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithExtTimedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorExtTimedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundedMultiple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCorePriorBoundedMultiple());
            return execs;
        }

        internal class ExprCorePriorUnboundSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select prior(1, Symbol) as prior1 from SupportMarketDataBean";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeMarketDataEvent("E0"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "prior1" },
                    new object[][] { new object[] { null } });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "prior1" },
                    new object[][] { new object[] { "E0" } });

                env.Milestone(2);

                for (var i = 2; i < 9; i++) {
                    env.SendEventBean(MakeMarketDataEvent("E" + i));
                    env.AssertPropsPerRowNewFlattened(
                        "s0",
                        new string[] { "prior1" },
                        new object[][] { new object[] { "E" + (i - 1) } });

                    if (i % 3 == 0) {
                        env.Milestone(i + 1);
                    }
                }

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorUnboundSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();

                var epl =
                    "@name('s0') select TheString as c0, prior(1, IntPrimitive) as c1, prior(2, IntPrimitive) as c2 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", null, null });

                env.Milestone(2);

                SendSupportBean(env, "E2", 11);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 10, null });

                env.Milestone(3);

                SendSupportBean(env, "E3", 12);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 11, 10 });

                env.Milestone(4);

                env.Milestone(5);

                SendSupportBean(env, "E4", 13);
                env.AssertPropsNew("s0", fields, new object[] { "E4", 12, 11 });

                SendSupportBean(env, "E5", 14);
                env.AssertPropsNew("s0", fields, new object[] { "E5", 13, 12 });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorBoundedMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl =
                    "@name('s0') select irstream TheString as c0, prior(1, IntPrimitive) as c1, prior(2, IntPrimitive) as c2 from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", null, null });

                env.Milestone(2);

                SendSupportBean(env, "E2", 11);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 10, null });

                env.Milestone(3);

                SendSupportBean(env, "E3", 12);
                env.AssertPropsIRPair("s0", fields, new object[] { "E3", 11, 10 }, new object[] { "E1", null, null });

                env.Milestone(4);

                env.Milestone(5);

                SendSupportBean(env, "E4", 13);
                env.AssertPropsIRPair("s0", fields, new object[] { "E4", 12, 11 }, new object[] { "E2", 10, null });

                SendSupportBean(env, "E5", 14);
                env.AssertPropsIRPair("s0", fields, new object[] { "E5", 13, 12 }, new object[] { "E3", 11, 10 });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorBoundedSingle : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();

                env.Milestone(0);

                var epl =
                    "@name('s0') select irstream TheString as c0, prior(1, IntPrimitive) as c1 from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", null });

                env.Milestone(2);

                SendSupportBean(env, "E2", 11);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 10 });

                env.Milestone(3);

                SendSupportBean(env, "E3", 12);
                env.AssertPropsIRPair("s0", fields, new object[] { "E3", 11 }, new object[] { "E1", null });

                env.Milestone(4);

                env.Milestone(5);

                SendSupportBean(env, "E4", 13);
                env.AssertPropsIRPair("s0", fields, new object[] { "E4", 12 }, new object[] { "E2", 10 });

                SendSupportBean(env, "E5", 14);
                env.AssertPropsIRPair("s0", fields, new object[] { "E5", 13 }, new object[] { "E3", 11 });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorTimewindowStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') SELECT prior(1, average) as value FROM SupportBean()#time(5 minutes)#uni(IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "value", null);

                env.SendEventBean(new SupportBean("E1", 4));
                env.AssertEqualsNew("s0", "value", 1.0);

                env.SendEventBean(new SupportBean("E1", 5));
                env.AssertEqualsNew("s0", "value", 2.5d);

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorStreamAndVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                TryAssertionPriorStreamAndVariable(env, path, "1", milestone);

                // try variable
                TryAssertionPriorStreamAndVariable(env, path, "NUM_PRIOR", milestone);

                // must be a constant-value expression
                env.CompileDeploy("@public create variable int NUM_PRIOR_NONCONST = 1", path);
                env.TryInvalidCompile(
                    path,
                    "@name('s0') select prior(NUM_PRIOR_NONCONST, s0) as result from SupportBean_S0#length(2) as s0",
                    "Failed to validate select-clause expression 'prior(NUM_PRIOR_NONCONST,s0)': Prior function requires a constant-value integer-typed index expression as the first parameter");

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol as currSymbol, " +
                          " prior(2, Symbol) as priorSymbol, " +
                          " prior(2, Price) as priorPrice " +
                          "from SupportMarketDataBean#time(1 min)";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("priorSymbol"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("priorPrice"));
                    });

                SendTimer(env, 0);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "D1", 1);
                AssertNewEvents(env, "D1", null, null);

                SendTimer(env, 1000);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "D2", 2);
                AssertNewEvents(env, "D2", null, null);

                SendTimer(env, 2000);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "D3", 3);
                AssertNewEvents(env, "D3", "D1", 1d);

                SendTimer(env, 3000);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "D4", 4);
                AssertNewEvents(env, "D4", "D2", 2d);

                SendTimer(env, 4000);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "D5", 5);
                AssertNewEvents(env, "D5", "D3", 3d);

                SendTimer(env, 30000);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "D6", 6);
                AssertNewEvents(env, "D6", "D4", 4d);

                SendTimer(env, 60000);
                AssertOldEvents(env, "D1", null, null);
                SendTimer(env, 61000);
                AssertOldEvents(env, "D2", null, null);
                SendTimer(env, 62000);
                AssertOldEvents(env, "D3", "D1", 1d);
                SendTimer(env, 63000);
                AssertOldEvents(env, "D4", "D2", 2d);
                SendTimer(env, 64000);
                AssertOldEvents(env, "D5", "D3", 3d);
                SendTimer(env, 90000);
                AssertOldEvents(env, "D6", "D4", 4d);

                SendMarketEvent(env, "D7", 7);
                AssertNewEvents(env, "D7", "D5", 5d);
                SendMarketEvent(env, "D8", 8);
                SendMarketEvent(env, "D9", 9);
                SendMarketEvent(env, "D10", 10);
                SendMarketEvent(env, "D11", 11);
                env.ListenerReset("s0");

                // release batch
                SendTimer(env, 150000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldData = listener.LastOldData;
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(5, oldData.Length);
                        AssertEvent(oldData[0], "D7", "D5", 5d);
                        AssertEvent(oldData[1], "D8", "D6", 6d);
                        AssertEvent(oldData[2], "D9", "D7", 7d);
                        AssertEvent(oldData[3], "D10", "D8", 8d);
                        AssertEvent(oldData[4], "D11", "D9", 9d);
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorExtTimedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol as currSymbol, " +
                          " prior(2, Symbol) as priorSymbol, " +
                          " prior(3, Price) as priorPrice " +
                          "from SupportMarketDataBean#ext_timed(Volume, 1 min) ";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("priorSymbol"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("priorPrice"));
                    });

                SendMarketEvent(env, "D1", 1, 0);
                AssertNewEvents(env, "D1", null, null);

                SendMarketEvent(env, "D2", 2, 1000);
                AssertNewEvents(env, "D2", null, null);

                SendMarketEvent(env, "D3", 3, 3000);
                AssertNewEvents(env, "D3", "D1", null);

                SendMarketEvent(env, "D4", 4, 4000);
                AssertNewEvents(env, "D4", "D2", 1d);

                SendMarketEvent(env, "D5", 5, 5000);
                AssertNewEvents(env, "D5", "D3", 2d);

                SendMarketEvent(env, "D6", 6, 30000);
                AssertNewEvents(env, "D6", "D4", 3d);

                SendMarketEvent(env, "D7", 7, 60000);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertEvent(listener.LastNewData[0], "D7", "D5", 4d);
                        AssertEvent(listener.LastOldData[0], "D1", null, null);
                        listener.Reset();
                    });

                SendMarketEvent(env, "D8", 8, 61000);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertEvent(listener.LastNewData[0], "D8", "D6", 5d);
                        AssertEvent(listener.LastOldData[0], "D2", null, null);
                        listener.Reset();
                    });

                SendMarketEvent(env, "D9", 9, 63000);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertEvent(listener.LastNewData[0], "D9", "D7", 6d);
                        AssertEvent(listener.LastOldData[0], "D3", "D1", null);
                        listener.Reset();
                    });

                SendMarketEvent(env, "D10", 10, 64000);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertEvent(listener.LastNewData[0], "D10", "D8", 7d);
                        AssertEvent(listener.LastOldData[0], "D4", "D2", 1d);
                        listener.Reset();
                    });

                SendMarketEvent(env, "D10", 10, 150000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldData = listener.LastOldData;
                        Assert.AreEqual(6, oldData.Length);
                        AssertEvent(oldData[0], "D5", "D3", 2d);
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorTimeBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol as currSymbol, " +
                          " prior(3, Symbol) as priorSymbol, " +
                          " prior(2, Price) as priorPrice " +
                          "from SupportMarketDataBean#time_batch(1 min) ";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("priorSymbol"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("priorPrice"));
                    });

                SendTimer(env, 0);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "A", 1);
                SendMarketEvent(env, "B", 2);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 60000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(2, listener.LastNewData.Length);
                        AssertEvent(listener.LastNewData[0], "A", null, null);
                        AssertEvent(listener.LastNewData[1], "B", null, null);
                        Assert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });

                SendTimer(env, 80000);
                SendMarketEvent(env, "C", 3);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 120000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(1, listener.LastNewData.Length);
                        AssertEvent(listener.LastNewData[0], "C", null, 1d);
                        Assert.AreEqual(2, listener.LastOldData.Length);
                        AssertEvent(listener.LastOldData[0], "A", null, null);
                        listener.Reset();
                    });

                SendTimer(env, 300000);
                SendMarketEvent(env, "D", 4);
                SendMarketEvent(env, "E", 5);
                SendMarketEvent(env, "F", 6);
                SendMarketEvent(env, "G", 7);
                SendTimer(env, 360000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(4, listener.LastNewData.Length);
                        AssertEvent(listener.LastNewData[0], "D", "A", 2d);
                        AssertEvent(listener.LastNewData[1], "E", "B", 3d);
                        AssertEvent(listener.LastNewData[2], "F", "C", 4d);
                        AssertEvent(listener.LastNewData[3], "G", "D", 5d);
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorUnbound : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol as currSymbol, " +
                          " prior(3, Symbol) as priorSymbol, " +
                          " prior(2, Price) as priorPrice " +
                          "from SupportMarketDataBean";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("priorSymbol"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("priorPrice"));
                    });

                SendMarketEvent(env, "A", 1);
                AssertNewEvents(env, "A", null, null);

                env.Milestone(1);

                SendMarketEvent(env, "B", 2);
                AssertNewEvents(env, "B", null, null);

                env.Milestone(2);

                SendMarketEvent(env, "C", 3);
                AssertNewEvents(env, "C", null, 1d);

                env.Milestone(3);

                SendMarketEvent(env, "D", 4);
                AssertNewEvents(env, "D", "A", 2d);

                env.Milestone(4);

                SendMarketEvent(env, "E", 5);
                AssertNewEvents(env, "E", "B", 3d);

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorNoDataWindowWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportMarketDataBean where prior(1, Price) = 100";
                env.CompileDeploy(text).AddListener("s0");

                SendMarketEvent(env, "IBM", 75);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "IBM", 100);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "IBM", 120);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorLongRunningSingle : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol as currSymbol, " +
                          " prior(3, Symbol) as prior0Symbol " +
                          "from SupportMarketDataBean#sort(3, Symbol)";
                env.CompileDeploy(epl).AddListener("s0");

                var random = new Random();
                // 200000 is a better number for a memory test, however for short unit tests this is 2000
                for (var i = 0; i < 2000; i++) {
                    if (i % 10000 == 0) {
                        //Console.WriteLine(i);
                    }

                    SendMarketEvent(env, random.Next().ToString(), 4);

                    if (i % 1000 == 0) {
                        env.ListenerReset("s0");
                    }
                }

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorLongRunningUnbound : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol as currSymbol, " +
                          " prior(3, Symbol) as prior0Symbol " +
                          "from SupportMarketDataBean";
                env.CompileDeploy(epl).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                var random = new Random();
                // 200000 is a better number for a memory test, however for short unit tests this is 2000
                for (var i = 0; i < 2000; i++) {
                    if (i % 10000 == 0) {
                        //Console.WriteLine(i);
                    }

                    SendMarketEvent(env, random.Next().ToString(), 4);

                    if (i % 1000 == 0) {
                        env.ListenerReset("s0");
                    }
                }

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorLongRunningMultiple : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol as currSymbol, " +
                          " prior(3, Symbol) as prior0Symbol, " +
                          " prior(2, Symbol) as prior1Symbol, " +
                          " prior(1, Symbol) as prior2Symbol, " +
                          " prior(0, Symbol) as prior3Symbol, " +
                          " prior(0, Price) as prior0Price, " +
                          " prior(1, Price) as prior1Price, " +
                          " prior(2, Price) as prior2Price, " +
                          " prior(3, Price) as prior3Price " +
                          "from SupportMarketDataBean#sort(3, Symbol)";
                env.CompileDeploy(epl).AddListener("s0");

                var random = new Random();
                // 200000 is a better number for a memory test, however for short unit tests this is 2000
                for (var i = 0; i < 2000; i++) {
                    if (i % 10000 == 0) {
                        //Console.WriteLine(i);
                    }

                    SendMarketEvent(env, random.Next().ToString(), 4);

                    if (i % 1000 == 0) {
                        env.ListenerReset("s0");
                    }
                }

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol as currSymbol, " +
                          "prior(0, Symbol) as prior0Symbol, " +
                          "prior(1, Symbol) as prior1Symbol, " +
                          "prior(2, Symbol) as prior2Symbol, " +
                          "prior(3, Symbol) as prior3Symbol, " +
                          "prior(0, Price) as prior0Price, " +
                          "prior(1, Price) as prior1Price, " +
                          "prior(2, Price) as prior2Price, " +
                          "prior(3, Price) as prior3Price " +
                          "from SupportMarketDataBean#length(3) ";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("prior0Symbol"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("prior0Price"));
                    });

                SendMarketEvent(env, "A", 1);
                AssertNewEvents(env, "A", "A", 1d, null, null, null, null, null, null);
                SendMarketEvent(env, "B", 2);
                AssertNewEvents(env, "B", "B", 2d, "A", 1d, null, null, null, null);
                SendMarketEvent(env, "C", 3);
                AssertNewEvents(env, "C", "C", 3d, "B", 2d, "A", 1d, null, null);

                SendMarketEvent(env, "D", 4);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvent = listener.LastNewData[0];
                        var oldEvent = listener.LastOldData[0];
                        AssertEventProps(env, newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);
                        AssertEventProps(env, oldEvent, "A", "A", 1d, null, null, null, null, null, null);
                    });

                SendMarketEvent(env, "E", 5);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvent = listener.LastNewData[0];
                        var oldEvent = listener.LastOldData[0];
                        AssertEventProps(env, newEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
                        AssertEventProps(env, oldEvent, "B", "B", 2d, "A", 1d, null, null, null, null);
                    });

                SendMarketEvent(env, "F", 6);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvent = listener.LastNewData[0];
                        var oldEvent = listener.LastOldData[0];
                        AssertEventProps(env, newEvent, "F", "F", 6d, "E", 5d, "D", 4d, "C", 3d);
                        AssertEventProps(env, oldEvent, "C", "C", 3d, "B", 2d, "A", 1d, null, null);
                    });

                SendMarketEvent(env, "G", 7);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvent = listener.LastNewData[0];
                        var oldEvent = listener.LastOldData[0];
                        AssertEventProps(env, newEvent, "G", "G", 7d, "F", 6d, "E", 5d, "D", 4d);
                        AssertEventProps(env, oldEvent, "D", "D", 4d, "C", 3d, "B", 2d, "A", 1d);
                    });

                SendMarketEvent(env, "G", 8);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldEvent = listener.LastOldData[0];
                        AssertEventProps(env, oldEvent, "E", "E", 5d, "D", 4d, "C", 3d, "B", 2d);
                    });

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorLengthWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var text =
                    "@name('s0') select prior(1, Symbol) as prior1, prior(2, Symbol) as prior2 from SupportMarketDataBean#length(3)";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeMarketDataEvent("E0"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "prior1", "prior2" },
                    new object[][] { new object[] { null, null } });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "prior1", "prior2" },
                    new object[][] { new object[] { "E0", null } });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    new string[] { "prior1", "prior2" },
                    new object[][] { new object[] { "E1", "E0" } });

                env.MilestoneInc(milestone);

                for (var i = 3; i < 9; i++) {
                    env.SendEventBean(MakeMarketDataEvent("E" + i));
                    env.AssertPropsPerRowNewFlattened(
                        "s0",
                        new string[] { "prior1", "prior2" },
                        new object[][] { new object[] { "E" + (i - 1), "E" + (i - 2) } });

                    if (i % 3 == 0) {
                        env.MilestoneInc(milestone);
                    }
                }

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorLengthWindowWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select prior(2, Symbol) as currSymbol " +
                          "from SupportMarketDataBean#length(1) " +
                          "where prior(2, Price) > 100";
                env.CompileDeploy(epl).AddListener("s0");

                SendMarketEvent(env, "A", 1);
                SendMarketEvent(env, "B", 130);
                SendMarketEvent(env, "C", 10);
                env.AssertListenerNotInvoked("s0");
                SendMarketEvent(env, "D", 5);
                env.AssertEqualsNew("s0", "currSymbol", "B");

                env.UndeployAll();
            }
        }

        internal class ExprCorePriorSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select irstream Symbol as currSymbol, " +
                          " prior(0, Symbol) as prior0Symbol, " +
                          " prior(1, Symbol) as prior1Symbol, " +
                          " prior(2, Symbol) as prior2Symbol, " +
                          " prior(3, Symbol) as prior3Symbol, " +
                          " prior(0, Price) as prior0Price, " +
                          " prior(1, Price) as prior1Price, " +
                          " prior(2, Price) as prior2Price, " +
                          " prior(3, Price) as prior3Price " +
                          "from SupportMarketDataBean#sort(3, Symbol)";
                TryPriorSortWindow(env, epl, milestone);

                epl = "@name('s0') select irstream Symbol as currSymbol, " +
                      " prior(3, Symbol) as prior3Symbol, " +
                      " prior(1, Symbol) as prior1Symbol, " +
                      " prior(2, Symbol) as prior2Symbol, " +
                      " prior(0, Symbol) as prior0Symbol, " +
                      " prior(2, Price) as prior2Price, " +
                      " prior(1, Price) as prior1Price, " +
                      " prior(0, Price) as prior0Price, " +
                      " prior(3, Price) as prior3Price " +
                      "from SupportMarketDataBean#sort(3, Symbol)";
                TryPriorSortWindow(env, epl, milestone);
            }
        }

        internal class ExprCorePriorTimeBatchWindowJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select TheString as currSymbol, " +
                          "prior(2, Symbol) as priorSymbol, " +
                          "prior(1, Price) as priorPrice " +
                          "from SupportBean#keepall, SupportMarketDataBean#time_batch(1 min)";
                env.CompileDeploy(epl).AddListener("s0");

                // assert select result type
                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("priorSymbol"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("priorPrice"));
                    });

                SendTimer(env, 0);
                env.AssertListenerNotInvoked("s0");

                SendMarketEvent(env, "A", 1);
                SendMarketEvent(env, "B", 2);
                SendBeanEvent(env, "X1");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 60000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(2, listener.LastNewData.Length);
                        AssertEvent(listener.LastNewData[0], "X1", null, null);
                        AssertEvent(listener.LastNewData[1], "X1", null, 1d);
                        Assert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });

                SendMarketEvent(env, "C1", 11);
                SendMarketEvent(env, "C2", 12);
                SendMarketEvent(env, "C3", 13);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 120000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(3, listener.LastNewData.Length);
                        AssertEvent(listener.LastNewData[0], "X1", "A", 2d);
                        AssertEvent(listener.LastNewData[1], "X1", "B", 11d);
                        AssertEvent(listener.LastNewData[2], "X1", "C1", 12d);
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertionPriorStreamAndVariable(
            RegressionEnvironment env,
            RegressionPath path,
            string priorIndex,
            AtomicLong milestone)
        {
            var text = "create constant variable int NUM_PRIOR = 1;\n @name('s0') select prior(" +
                       priorIndex +
                       ", s0) as result from SupportBean_S0#length(2) as s0";
            env.CompileDeploy(text, path).AddListener("s0");

            var e1 = new SupportBean_S0(3);
            env.SendEventBean(e1);
            env.AssertEqualsNew("s0", "result", null);

            env.Milestone(milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean_S0(3));
            env.AssertEqualsNew("s0", "result", e1);
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(SupportBean_S0), statement.EventType.GetPropertyType("result")));

            env.UndeployAll();
            path.Clear();
        }

        private static void TryPriorSortWindow(
            RegressionEnvironment env,
            string epl,
            AtomicLong milestone)
        {
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            SendMarketEvent(env, "COX", 30);
            AssertNewEvents(env, "COX", "COX", 30d, null, null, null, null, null, null);

            SendMarketEvent(env, "IBM", 45);
            AssertNewEvents(env, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);

            SendMarketEvent(env, "MSFT", 33);
            AssertNewEvents(env, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);

            SendMarketEvent(env, "XXX", 55);
            env.AssertListener(
                "s0",
                listener => {
                    var newEvent = listener.LastNewData[0];
                    var oldEvent = listener.LastOldData[0];
                    AssertEventProps(env, newEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);
                    AssertEventProps(env, oldEvent, "XXX", "XXX", 55d, "MSFT", 33d, "IBM", 45d, "COX", 30d);
                });

            SendMarketEvent(env, "BOO", 20);
            env.AssertListener(
                "s0",
                listener => {
                    var newEvent = listener.LastNewData[0];
                    var oldEvent = listener.LastOldData[0];
                    AssertEventProps(env, newEvent, "BOO", "BOO", 20d, "XXX", 55d, "MSFT", 33d, "IBM", 45d);
                    AssertEventProps(env, oldEvent, "MSFT", "MSFT", 33d, "IBM", 45d, "COX", 30d, null, null);
                });

            SendMarketEvent(env, "DOR", 1);
            env.AssertListener(
                "s0",
                listener => {
                    var newEvent = listener.LastNewData[0];
                    var oldEvent = listener.LastOldData[0];
                    AssertEventProps(env, newEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);
                    AssertEventProps(env, oldEvent, "IBM", "IBM", 45d, "COX", 30d, null, null, null, null);
                });

            SendMarketEvent(env, "AAA", 2);
            env.AssertListener(
                "s0",
                listener => {
                    var newEvent = listener.LastNewData[0];
                    var oldEvent = listener.LastOldData[0];
                    AssertEventProps(env, newEvent, "AAA", "AAA", 2d, "DOR", 1d, "BOO", 20d, "XXX", 55d);
                    AssertEventProps(env, oldEvent, "DOR", "DOR", 1d, "BOO", 20d, "XXX", 55d, "MSFT", 33d);
                });

            SendMarketEvent(env, "AAB", 2);
            env.AssertListener(
                "s0",
                listener => {
                    var oldEvent = listener.LastOldData[0];
                    AssertEventProps(env, oldEvent, "COX", "COX", 30d, null, null, null, null, null, null);
                    listener.Reset();
                });

            env.UndeployAll();
        }

        private static void AssertNewEvents(
            RegressionEnvironment env,
            string currSymbol,
            string priorSymbol,
            double? priorPrice)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    Assert.IsNull(oldData);
                    Assert.AreEqual(1, newData.Length);

                    AssertEvent(newData[0], currSymbol, priorSymbol, priorPrice);

                    listener.Reset();
                });
        }

        private static void AssertEvent(
            EventBean eventBean,
            string currSymbol,
            string priorSymbol,
            double? priorPrice)
        {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(priorSymbol, eventBean.Get("priorSymbol"));
            Assert.AreEqual(priorPrice, eventBean.Get("priorPrice"));
        }

        private static void AssertNewEvents(
            RegressionEnvironment env,
            string currSymbol,
            string prior0Symbol,
            double? prior0Price,
            string prior1Symbol,
            double? prior1Price,
            string prior2Symbol,
            double? prior2Price,
            string prior3Symbol,
            double? prior3Price)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    Assert.IsNull(oldData);
                    Assert.AreEqual(1, newData.Length);
                    AssertEventProps(
                        env,
                        newData[0],
                        currSymbol,
                        prior0Symbol,
                        prior0Price,
                        prior1Symbol,
                        prior1Price,
                        prior2Symbol,
                        prior2Price,
                        prior3Symbol,
                        prior3Price);

                    listener.Reset();
                });
        }

        private static void AssertEventProps(
            RegressionEnvironment env,
            EventBean eventBean,
            string currSymbol,
            string prior0Symbol,
            double? prior0Price,
            string prior1Symbol,
            double? prior1Price,
            string prior2Symbol,
            double? prior2Price,
            string prior3Symbol,
            double? prior3Price)
        {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(prior0Symbol, eventBean.Get("prior0Symbol"));
            Assert.AreEqual(prior0Price, eventBean.Get("prior0Price"));
            Assert.AreEqual(prior1Symbol, eventBean.Get("prior1Symbol"));
            Assert.AreEqual(prior1Price, eventBean.Get("prior1Price"));
            Assert.AreEqual(prior2Symbol, eventBean.Get("prior2Symbol"));
            Assert.AreEqual(prior2Price, eventBean.Get("prior2Price"));
            Assert.AreEqual(prior3Symbol, eventBean.Get("prior3Symbol"));
            Assert.AreEqual(prior3Price, eventBean.Get("prior3Price"));

            env.ListenerReset("s0");
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendMarketEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
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

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void AssertOldEvents(
            RegressionEnvironment env,
            string currSymbol,
            string priorSymbol,
            double? priorPrice)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    Assert.IsNull(newData);
                    Assert.AreEqual(1, oldData.Length);

                    Assert.AreEqual(currSymbol, oldData[0].Get("currSymbol"));
                    Assert.AreEqual(priorSymbol, oldData[0].Get("priorSymbol"));
                    Assert.AreEqual(priorPrice, oldData[0].Get("priorPrice"));

                    listener.Reset();
                });
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, "");
        }
    }
} // end of namespace