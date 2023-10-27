///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitRowForAll
    {
        private const string CATEGORY = "Fully-Aggregated and Un-grouped";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if TEMPORARY
            With1NoneNoHavingNoJoin(execs);
            With2NoneNoHavingJoin(execs);
            With3NoneHavingNoJoin(execs);
            With4NoneHavingJoin(execs);
            With5DefaultNoHavingNoJoin(execs);
            With6DefaultNoHavingJoin(execs);
            With7DefaultHavingNoJoin(execs);
            With8DefaultHavingJoin(execs);
            With9AllNoHavingNoJoin(execs);
            With10AllNoHavingJoin(execs);
            With11AllHavingNoJoin(execs);
            With12AllHavingJoin(execs);
            With13LastNoHavingNoJoin(execs);
            With14LastNoHavingJoin(execs);
            With15LastHavingNoJoin(execs);
            With16LastHavingJoin(execs);
            With17FirstNoHavingNoJoin(execs);
            With18SnapshotNoHavingNoJoin(execs);
            WithOutputLastWithInsertInto(execs);
            WithAggAllHaving(execs);
            WithAggAllHavingJoin(execs);
            WithJoinSortWindow(execs);
            WithMaxTimeWindow(execs);
            WithTimeWindowOutputCountLast(execs);
            WithTimeBatchOutputCount(execs);
            WithLimitSnapshot(execs);
            WithLimitSnapshotJoin(execs);
            WithOutputSnapshotGetValue(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOutputSnapshotGetValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputSnapshotGetValue());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshotJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshotJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshot());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchOutputCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetTimeBatchOutputCount());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindowOutputCountLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetTimeWindowOutputCountLast());
            return execs;
        }

        public static IList<RegressionExecution> WithMaxTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetMaxTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinSortWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinSortWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithAggAllHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggAllHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithAggAllHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggAllHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputLastWithInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputLastWithInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> With18SnapshotNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With16LastHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet16LastHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With15LastHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet15LastHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With14LastNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet14LastNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With13LastNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet13LastNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With12AllHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet12AllHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With11AllHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet11AllHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With10AllNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet10AllNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With9AllNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet9AllNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With8DefaultHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet8DefaultHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With7DefaultHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet7DefaultHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With6DefaultNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet6DefaultNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With5DefaultNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet5DefaultNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With4NoneHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet4NoneHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With3NoneHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet3NoneHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With2NoneNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet2NoneNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With1NoneNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet1NoneNoHavingNoJoin());
            return execs;
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec)";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               " having sum(Price) > 100";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                               " having sum(Price) > 100";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "having sum(Price) > 100" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                               "having sum(Price) > 100" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet9AllNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion9AllNoHavingNoJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion9AllNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all", milestone);
        }

        internal class ResultSet10AllNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion10AllNoHavingJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion10AllNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all", milestone);
        }

        internal class ResultSet11AllHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion11AllHavingNoJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion11AllHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "having sum(Price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all", milestone);
        }

        internal class ResultSet12AllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion12AllHavingJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                           "having sum(Price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all", milestone);
        }

        internal class ResultSet13LastNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion13LastNoHavingNoJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion13LastNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last", milestone);
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion14LastNoHavingJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion14LastNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last", milestone);
        }

        internal class ResultSet15LastHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion15LastHavingNoJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "having sum(Price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        internal class ResultSet16LastHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion16LastHavingJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion16LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
                           "having sum(Price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        internal class ResultSet17FirstNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSetOutputLastWithInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionOuputLastWithInsertInto(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSetAggAllHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Volume) as result " +
                               "from SupportMarketDataBean#length(10) as two " +
                               "having sum(Volume) > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "result" };

                SendMDEvent(env, 20);
                SendMDEvent(env, -100);
                SendMDEvent(env, 0);
                SendMDEvent(env, 0);
                env.AssertListenerNotInvoked("s0");

                SendMDEvent(env, 0);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 20L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggAllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select sum(Volume) as result " +
                               "from SupportMarketDataBean#length(10) as one," +
                               "SupportBean#length(10) as two " +
"where one.Symbol=two.TheString "+
                               "having sum(Volume) > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "result" };
                env.SendEventBean(new SupportBean("S0", 0));

                SendMDEvent(env, 20);
                SendMDEvent(env, -100);
                SendMDEvent(env, 0);
                SendMDEvent(env, 0);
                env.AssertListenerNotInvoked("s0");

                SendMDEvent(env, 0);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 20L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream max(Price) as maxVol" +
                          " from SupportMarketDataBean#sort(1,Volume desc) as s0, " +
"SupportBean#keepall as s1 where s1.TheString=s0.Symbol "+
                          "output every 1.0d seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("JOIN_KEY", -1));

                SendEvent(env, "JOIN_KEY", 1d);
                SendEvent(env, "JOIN_KEY", 2d);
                env.ListenerReset("s0");

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                env.AssertListener(
                    "s0",
                    listener => {
                        var result = listener.DataListsFlattened;
                        Assert.AreEqual(2, result.First.Length);
                        Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
                        Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
                        Assert.AreEqual(2, result.Second.Length);
                        Assert.AreEqual(null, result.Second[0].Get("maxVol"));
                        Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));
                    });

                // statement object model test
                var model = env.EplToModel(epl);
                env.CopyMayFail(model);
                Assert.AreEqual(epl, model.ToEPL());

                env.UndeployAll();
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream max(Price) as maxVol" +
                          " from SupportMarketDataBean#time(1.1 sec) " +
                          "output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "SYM1", 1d);
                SendEvent(env, "SYM1", 2d);
                env.ListenerReset("s0");

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                env.AssertListener(
                    "s0",
                    listener => {
                        var result = listener.DataListsFlattened;
                        Assert.AreEqual(2, result.First.Length);
                        Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
                        Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
                        Assert.AreEqual(2, result.Second.Length);
                        Assert.AreEqual(null, result.Second[0].Get("maxVol"));
                        Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetTimeWindowOutputCountLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select count(*) as cnt from SupportBean#time(10 seconds) output every 10 seconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendTimer(env, 0);
                SendTimer(env, 10000);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 20000);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "e1");
                SendTimer(env, 30000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.GetAndResetLastNewData();
                        Assert.AreEqual(2, newEvents.Length);
                        Assert.AreEqual(1L, newEvents[0].Get("cnt"));
                        Assert.AreEqual(0L, newEvents[1].Get("cnt"));
                    });

                SendTimer(env, 31000);

                SendEvent(env, "e2");
                SendEvent(env, "e3");
                SendTimer(env, 40000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.GetAndResetLastNewData();
                        Assert.AreEqual(2, newEvents.Length);
                        Assert.AreEqual(1L, newEvents[0].Get("cnt"));
                        Assert.AreEqual(2L, newEvents[1].Get("cnt"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetTimeBatchOutputCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select count(*) as cnt from SupportBean#time_batch(10 seconds) output every 10 seconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendTimer(env, 0);
                SendTimer(env, 10000);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 20000);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "e1");
                SendTimer(env, 30000);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 40000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.GetAndResetLastNewData();
                        Assert.AreEqual(2, newEvents.Length);
                        // output limiting starts 10 seconds after, therefore the old batch was posted already and the cnt is zero
                        Assert.AreEqual(1L, newEvents[0].Get("cnt"));
                        Assert.AreEqual(0L, newEvents[1].Get("cnt"));
                    });

                SendTimer(env, 50000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newData = listener.LastNewData;
                        Assert.AreEqual(0L, newData[0].Get("cnt"));
                        listener.Reset();
                    });

                SendEvent(env, "e2");
                SendEvent(env, "e3");
                SendTimer(env, 60000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.GetAndResetLastNewData();
                        Assert.AreEqual(1, newEvents.Length);
                        Assert.AreEqual(2L, newEvents[0].Get("cnt"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "cnt" };
                SendTimer(env, 0);
                var selectStmt =
                    "@name('s0') select count(*) as cnt from SupportBean#time(10 seconds) where IntPrimitive > 0 output snapshot every 1 seconds";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendEvent(env, "s0", 1);

                SendTimer(env, 500);
                SendEvent(env, "s1", 1);
                SendEvent(env, "s2", -1);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 2L } });

                SendTimer(env, 1500);
                SendEvent(env, "s4", 2);
                SendEvent(env, "s5", 3);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 2000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 4L } });

                SendEvent(env, "s5", 4);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 9000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 5L } });

                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 4L } });

                SendTimer(env, 10999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 3L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "cnt" };
                SendTimer(env, 0);
                var selectStmt = "@name('s0') select count(*) as cnt from " +
                                 "SupportBean#time(10 seconds) as s, " +
"SupportMarketDataBean#keepall as m where m.Symbol = s.TheString and IntPrimitive > 0 output snapshot every 1 seconds";
                env.CompileDeploy(selectStmt).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("s0", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s1", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s2", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s4", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s5", 0, 0L, ""));

                SendEvent(env, "s0", 1);

                SendTimer(env, 500);
                SendEvent(env, "s1", 1);
                SendEvent(env, "s2", -1);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 2L } });

                SendTimer(env, 1500);
                SendEvent(env, "s4", 2);
                SendEvent(env, "s5", 3);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 2000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 4L } });

                SendEvent(env, "s5", 4);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 9000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 5L } });

                // The execution of the join is after the snapshot, as joins are internal dispatch
                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 5L } });

                SendTimer(env, 10999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 3L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputSnapshotGetValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionOutputSnapshotGetValue(env, true);
                TryAssertionOutputSnapshotGetValue(env, false);
            }
        }

        private static void TryAssertionOutputSnapshotGetValue(
            RegressionEnvironment env,
            bool join)
        {
            var epl = "@name('s0') select customagg(IntPrimitive) as c0 from SupportBean" +
                      (join ? "#keepall, SupportBean_S0#lastevent" : "") +
                      " output snapshot every 3 events";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1));

            SupportInvocationCountFunction.ResetGetValueInvocationCount();

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));
            env.AssertThat(() => Assert.AreEqual(0, SupportInvocationCountFunction.GetValueInvocationCount));

            env.SendEventBean(new SupportBean("E3", 30));
            env.AssertEqualsNew("s0", "c0", 60);
            env.AssertThat(() => Assert.AreEqual(1, SupportInvocationCountFunction.GetValueInvocationCount));

            env.SendEventBean(new SupportBean("E3", 40));
            env.SendEventBean(new SupportBean("E4", 50));
            env.SendEventBean(new SupportBean("E5", 60));
            env.AssertEqualsNew("s0", "c0", 210);
            env.AssertThat(() => Assert.AreEqual(2, SupportInvocationCountFunction.GetValueInvocationCount));

            env.UndeployAll();
        }

        private static void TryAssertionOuputLastWithInsertInto(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
                      "insert into MyStream select sum(IntPrimitive) as thesum from SupportBean#keepall " +
                      "output last every 2 events;\n" +
                      "@name('s0') select * from MyStream;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));
            env.AssertPropsNew("s0", "thesum".SplitCsv(), new object[] { 30 });

            env.UndeployAll();
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new object[][] { new object[] { 25d } },
                new object[][] { new object[] { null } });
            expected.AddResultInsRem(
                800,
                1,
                new object[][] { new object[] { 34d } },
                new object[][] { new object[] { 25d } });
            expected.AddResultInsRem(
                1500,
                1,
                new object[][] { new object[] { 58d } },
                new object[][] { new object[] { 34d } });
            expected.AddResultInsRem(
                1500,
                2,
                new object[][] { new object[] { 59d } },
                new object[][] { new object[] { 58d } });
            expected.AddResultInsRem(
                2100,
                1,
                new object[][] { new object[] { 85d } },
                new object[][] { new object[] { 59d } });
            expected.AddResultInsRem(
                3500,
                1,
                new object[][] { new object[] { 87d } },
                new object[][] { new object[] { 85d } });
            expected.AddResultInsRem(
                4300,
                1,
                new object[][] { new object[] { 109d } },
                new object[][] { new object[] { 87d } });
            expected.AddResultInsRem(
                4900,
                1,
                new object[][] { new object[] { 112d } },
                new object[][] { new object[] { 109d } });
            expected.AddResultInsRem(
                5700,
                0,
                new object[][] { new object[] { 87d } },
                new object[][] { new object[] { 112d } });
            expected.AddResultInsRem(
                5900,
                1,
                new object[][] { new object[] { 88d } },
                new object[][] { new object[] { 87d } });
            expected.AddResultInsRem(
                6300,
                0,
                new object[][] { new object[] { 79d } },
                new object[][] { new object[] { 88d } });
            expected.AddResultInsRem(
                7000,
                0,
                new object[][] { new object[] { 54d } },
                new object[][] { new object[] { 79d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion34(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(4300, 1, new object[][] { new object[] { 109d } }, null);
            expected.AddResultInsRem(
                4900,
                1,
                new object[][] { new object[] { 112d } },
                new object[][] { new object[] { 109d } });
            expected.AddResultInsRem(5700, 0, null, new object[][] { new object[] { 112d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion13_14(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { 34d } },
                new object[][] { new object[] { null } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] { new object[] { 85d } },
                new object[][] { new object[] { 34d } });
            expected.AddResultInsRem(
                3200,
                0,
                new object[][] { new object[] { 85d } },
                new object[][] { new object[] { 85d } });
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { 87d } },
                new object[][] { new object[] { 85d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { 112d } },
                new object[][] { new object[] { 87d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { 88d } },
                new object[][] { new object[] { 112d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][] { new object[] { 54d } },
                new object[][] { new object[] { 88d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion15_16(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { 112d } },
                new object[][] { new object[] { 109d } });
            expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] { 112d } });
            expected.AddResultInsRem(7200, 0, null, null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion78(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { 109d }, new object[] { 112d } },
                new object[][] { new object[] { 109d } });
            expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] { 112d } });
            expected.AddResultInsRem(7200, 0, null, null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion56(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { 25d }, new object[] { 34d } },
                new object[][] { new object[] { null }, new object[] { 25d } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] { new object[] { 58d }, new object[] { 59d }, new object[] { 85d } },
                new object[][] { new object[] { 34d }, new object[] { 58d }, new object[] { 59d } });
            expected.AddResultInsRem(
                3200,
                0,
                new object[][] { new object[] { 85d } },
                new object[][] { new object[] { 85d } });
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { 87d } },
                new object[][] { new object[] { 85d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { 109d }, new object[] { 112d } },
                new object[][] { new object[] { 87d }, new object[] { 109d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { 87d }, new object[] { 88d } },
                new object[][] { new object[] { 112d }, new object[] { 87d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][] { new object[] { 79d }, new object[] { 54d } },
                new object[][] { new object[] { 88d }, new object[] { 79d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion17(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new object[][] { new object[] { 25d } },
                new object[][] { new object[] { null } });
            expected.AddResultInsRem(
                1500,
                1,
                new object[][] { new object[] { 58d } },
                new object[][] { new object[] { 34d } });
            expected.AddResultInsRem(
                3500,
                1,
                new object[][] { new object[] { 87d } },
                new object[][] { new object[] { 85d } });
            expected.AddResultInsRem(
                4300,
                1,
                new object[][] { new object[] { 109d } },
                new object[][] { new object[] { 87d } });
            expected.AddResultInsRem(
                5700,
                0,
                new object[][] { new object[] { 87d } },
                new object[][] { new object[] { 112d } });
            expected.AddResultInsRem(
                6300,
                0,
                new object[][] { new object[] { 79d } },
                new object[][] { new object[] { 88d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion18(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new object[][] { new object[] { 34d } }, null);
            expected.AddResultInsRem(2200, 0, new object[][] { new object[] { 85d } }, null);
            expected.AddResultInsRem(3200, 0, new object[][] { new object[] { 85d } }, null);
            expected.AddResultInsRem(4200, 0, new object[][] { new object[] { 87d } }, null);
            expected.AddResultInsRem(5200, 0, new object[][] { new object[] { 112d } }, null);
            expected.AddResultInsRem(6200, 0, new object[][] { new object[] { 88d } }, null);
            expected.AddResultInsRem(7200, 0, new object[][] { new object[] { 54d } }, null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = 0.0;
            bean.IntPrimitive = 0;
            bean.IntBoxed = 0;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            long volume)
        {
            var bean = new SupportMarketDataBean("S0", 0, volume, null);
            env.SendEventBean(bean);
        }
    }
} // end of namespace