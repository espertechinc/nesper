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
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitRowPerEvent
    {
        private const string EVENT_NAME = nameof(SupportMarketDataBean);
        private const string JOIN_KEY = "KEY";
        private const string CATEGORY = "Aggregated and Un-grouped";

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
            With11AllHavingNoJoinHinted(execs);
            With12AllHavingJoin(execs);
            With13LastNoHavingNoJoin(execs);
            With14LastNoHavingJoin(execs);
            With15LastHavingNoJoin(execs);
            With16LastHavingJoin(execs);
            With17FirstNoHavingNoJoinIStreamOnly(execs);
            With17FirstNoHavingNoJoinIRStream(execs);
            With18SnapshotNoHavingNoJoin(execs);
            WithHaving(execs);
            WithHavingJoin(execs);
            WithMaxTimeWindow(execs);
            WithLimitSnapshot(execs);
            WithLimitSnapshotJoin(execs);
            WithJoinSortWindow(execs);
            WithRowPerEventNoJoinLast(execs);
            WithRowPerEventJoinAll(execs);
            WithRowPerEventJoinLast(execs);
            WithTime(execs);
            WithCount(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetCount());
            return execs;
        }

        public static IList<RegressionExecution> WithTime(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetTime());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventNoJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventNoJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinSortWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinSortWindow());
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

        public static IList<RegressionExecution> WithMaxTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetMaxTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHaving());
            return execs;
        }

        public static IList<RegressionExecution> With18SnapshotNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingNoJoinIRStream(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingNoJoinIRStream());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingNoJoinIStreamOnly(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingNoJoinIStreamOnly());
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

        public static IList<RegressionExecution> With11AllHavingNoJoinHinted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet11AllHavingNoJoinHinted());
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

        private class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        private class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        private class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               " having sum(price) > 100";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        private class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               " having sum(price) > 100";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        private class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        private class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        private class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "having sum(price) > 100" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        private class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "having sum(price) > 100" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        private class ResultSet9AllNoHavingNoJoin : RegressionExecution
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
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all", milestone);
        }

        private class ResultSet10AllNoHavingJoin : RegressionExecution
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
            SupportOutputLimitOpt hint,
            AtomicLong milestone)
        {
            var stmtText = hint.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all", milestone);
        }

        private class ResultSet11AllHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "having sum(price) > 100" +
                               "output all every 1 seconds";
                TryAssertion78(env, stmtText, "all", new AtomicLong());
            }
        }

        private class ResultSet11AllHavingNoJoinHinted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion11AllHavingNoJoinHinted(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion11AllHavingNoJoinHinted(
            RegressionEnvironment env,
            SupportOutputLimitOpt hint,
            AtomicLong milestone)
        {
            var stmtText = hint.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "having sum(price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all", milestone);
        }

        private class ResultSet12AllHavingJoin : RegressionExecution
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
            SupportOutputLimitOpt hint,
            AtomicLong milestone)
        {
            var stmtText = hint.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "having sum(price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all", milestone);
        }

        private class ResultSet13LastNoHavingNoJoin : RegressionExecution
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
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last", milestone);
        }

        private class ResultSet14LastNoHavingJoin : RegressionExecution
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
            SupportOutputLimitOpt hint,
            AtomicLong milestone)
        {
            var stmtText = hint.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last", milestone);
        }

        private class ResultSet15LastHavingNoJoin : RegressionExecution
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
            SupportOutputLimitOpt hint,
            AtomicLong milestone)
        {
            var stmtText = hint.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "having sum(price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        private class ResultSet16LastHavingJoin : RegressionExecution
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
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "having sum(price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        private class ResultSet17FirstNoHavingNoJoinIStreamOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IStreamOnly(env, stmtText, "first", new AtomicLong());
            }
        }

        private class ResultSet17FirstNoHavingNoJoinIRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select irstream symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IRStream(env, stmtText, "first", new AtomicLong());
            }
        }

        private class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "first", new AtomicLong());
            }
        }

        private class ResultSetHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select symbol, avg(price) as avgPrice " +
                          "from SupportMarketDataBean#time(3 sec) " +
                          "having avg(price) > 10" +
                          "output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionHaving(env);
            }
        }

        private class ResultSetHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select symbol, avg(price) as avgPrice " +
                          "from SupportMarketDataBean#time(3 sec) as md, " +
                          "SupportBean#keepall as s where s.theString = md.symbol " +
                          "having avg(price) > 10" +
                          "output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("SYM1", -1));

                TryAssertionHaving(env);
            }
        }

        private class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream volume, max(price) as maxVol" +
                          " from SupportMarketDataBean#time(1 sec) " +
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
                        Assert.AreEqual(null, result.Second[1].Get("maxVol"));
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@name('s0') select symbol, sum(price) as sumprice from SupportMarketDataBean" +
                                 "#time(10 seconds) output snapshot every 1 seconds order by symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 16);
                SendEvent(env, "MSFT", 14);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                var fields = new string[] { "symbol", "sumprice" };
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ABC", 50d }, new object[] { "IBM", 50d }, new object[] { "MSFT", 50d } },
                    null);

                SendTimer(env, 1500);
                SendEvent(env, "YAH", 18);
                SendEvent(env, "s4", 30);

                SendTimer(env, 10000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "ABC", 98d }, new object[] { "IBM", 98d }, new object[] { "MSFT", 98d },
                        new object[] { "YAH", 98d }, new object[] { "s4", 98d }
                    },
                    null);

                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "YAH", 48d }, new object[] { "s4", 48d } });

                SendTimer(env, 12000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                SendTimer(env, 13000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.UndeployAll();
            }
        }

        private class ResultSetLimitSnapshotJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt =
                    "@name('s0') select irstream symbol, sum(price) as sumprice from SupportMarketDataBean" +
                    "#time(10 seconds) as m, SupportBean" +
                    "#keepall as s where s.theString = m.symbol output snapshot every 1 seconds order by symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                env.SendEventBean(new SupportBean("ABC", 1));
                env.SendEventBean(new SupportBean("IBM", 2));
                env.SendEventBean(new SupportBean("MSFT", 3));
                env.SendEventBean(new SupportBean("YAH", 4));
                env.SendEventBean(new SupportBean("s4", 5));

                SendEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 16);
                SendEvent(env, "MSFT", 14);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                var fields = new string[] { "symbol", "sumprice" };
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ABC", 50d }, new object[] { "IBM", 50d }, new object[] { "MSFT", 50d } });

                SendTimer(env, 1500);
                SendEvent(env, "YAH", 18);
                SendEvent(env, "s4", 30);

                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "ABC", 98d }, new object[] { "IBM", 98d }, new object[] { "MSFT", 98d },
                        new object[] { "YAH", 98d }, new object[] { "s4", 98d }
                    });

                SendTimer(env, 10500);
                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "YAH", 48d }, new object[] { "s4", 48d } });

                SendTimer(env, 11500);
                SendTimer(env, 12000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                SendTimer(env, 13000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.UndeployAll();
            }
        }

        private class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream volume, max(price) as maxVol" +
                          " from SupportMarketDataBean#sort(1, volume desc) as s0," +
                          "SupportBean#keepall as s1 " +
                          "output every 1 seconds";
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
                        Assert.AreEqual(1, result.Second.Length);
                        Assert.AreEqual(2.0, result.Second[0].Get("maxVol"));
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventNoJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionRowPerEventNoJoinLast(env, outputLimitOpt);
                }
            }

            private static void TryAssertionRowPerEventNoJoinLast(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@name('s0') select longBoxed, sum(longBoxed) as result " +
                          "from SupportBean#length(3) " +
                          "having sum(longBoxed) > 0 " +
                          "output last every 2 events";

                CreateStmtAndListenerNoJoin(env, epl);
                TryAssertLastSum(env);

                epl = opt.GetHint() +
                      "@name('s0') select longBoxed, sum(longBoxed) as result " +
                      "from SupportBean#length(3) " +
                      "output last every 2 events";
                CreateStmtAndListenerNoJoin(env, epl);
                TryAssertLastSum(env);
            }
        }

        private class ResultSetRowPerEventJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionRowPerEventJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionRowPerEventJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@name('s0') select longBoxed, sum(longBoxed) as result " +
                          "from SupportBeanString#length(3) as one, " +
                          "SupportBean#length(3) as two " +
                          "having sum(longBoxed) > 0 " +
                          "output all every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertAllSum(env);

                epl = opt.GetHint() +
                      "@name('s0') select longBoxed, sum(longBoxed) as result " +
                      "from SupportBeanString#length(3) as one, " +
                      "SupportBean#length(3) as two " +
                      "output every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertAllSum(env);

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select longBoxed, sum(longBoxed) as result " +
                          "from SupportBeanString#length(3) as one, " +
                          "SupportBean#length(3) as two " +
                          "having sum(longBoxed) > 0 " +
                          "output last every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertLastSum(env);

                epl = "@name('s0') select longBoxed, sum(longBoxed) as result " +
                      "from SupportBeanString#length(3) as one, " +
                      "SupportBean#length(3) as two " +
                      "output last every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertLastSum(env);

                env.UndeployAll();
            }
        }

        private class ResultSetTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Set the clock to 0
                var currentTime = new AtomicLong();
                SendTimeEventRelative(env, 0, currentTime);

                // Create the EPL statement and add a listener
                var epl = "@name('s0') select symbol, sum(volume) from " +
                          EVENT_NAME +
                          "#length(5) output first every 3 seconds";
                env.CompileDeploy(epl).AddListener("s0");
                env.ListenerReset("s0");

                // Send the first event of the batch; should be output
                SendMarketDataEvent(env, 10L);
                AssertEvent(env, 10L);

                // Send another event, not the first, for aggregation
                // update only, no output
                SendMarketDataEvent(env, 20L);
                env.AssertListenerNotInvoked("s0");

                // Update time
                SendTimeEventRelative(env, 3000, currentTime);
                env.AssertListenerNotInvoked("s0");

                // Send first event of the next batch, should be output.
                // The aggregate value is computed over all events
                // received: 10 + 20 + 30 = 60
                SendMarketDataEvent(env, 30L);
                AssertEvent(env, 60L);

                // Send the next event of the batch, no output
                SendMarketDataEvent(env, 40L);
                env.AssertListenerNotInvoked("s0");

                // Update time
                SendTimeEventRelative(env, 3000, currentTime);
                env.AssertListenerNotInvoked("s0");

                // Send first event of third batch
                SendMarketDataEvent(env, 1L);
                AssertEvent(env, 101L);

                // Update time
                SendTimeEventRelative(env, 3000, currentTime);
                env.AssertListenerNotInvoked("s0");

                // Update time: no first event this batch, so a callback
                // is made at the end of the interval
                SendTimeEventRelative(env, 3000, currentTime);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ResultSetCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Create the EPL statement and add a listener
                var statementText = "@name('s0') select symbol, sum(volume) from " +
                                    EVENT_NAME +
                                    "#length(5) output first every 3 events";
                env.CompileDeploy(statementText).AddListener("s0");
                env.ListenerReset("s0");

                // Send the first event of the batch, should be output
                SendEventLong(env, 10L);
                AssertEvent(env, 10L);

                // Send the second event of the batch, not output, used
                // for updating the aggregate value only
                SendEventLong(env, 20L);
                env.AssertListenerNotInvoked("s0");

                // Send the third event of the batch, still not output,
                // but should reset the batch
                SendEventLong(env, 30L);
                env.AssertListenerNotInvoked("s0");

                // First event, next batch, aggregate value should be
                // 10 + 20 + 30 + 40 = 100
                SendEventLong(env, 40L);
                AssertEvent(env, 100L);

                // Next event again not output
                SendEventLong(env, 50L);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void SendEventLong(
            RegressionEnvironment env,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean("DELL", 0.0, volume, null));
        }

        private static void CreateStmtAndListenerNoJoin(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
        }

        private static void TryAssertAllSum(RegressionEnvironment env)
        {
            // send an event
            SendEvent(env, 1);

            // check no update
            env.AssertListenerNotInvoked("s0");

            // send another event
            SendEvent(env, 2);

            // check update, all events present
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(2, listener.LastNewData.Length);
                    Assert.AreEqual(1L, listener.LastNewData[0].Get("longBoxed"));
                    Assert.AreEqual(1L, listener.LastNewData[0].Get("result"));
                    Assert.AreEqual(2L, listener.LastNewData[1].Get("longBoxed"));
                    Assert.AreEqual(3L, listener.LastNewData[1].Get("result"));
                    Assert.IsNull(listener.LastOldData);
                });

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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 25d } });
            expected.AddResultInsert(800, 1, new object[][] { new object[] { "MSFT", 34d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 58d } });
            expected.AddResultInsert(1500, 2, new object[][] { new object[] { "YAH", 59d } });
            expected.AddResultInsert(2100, 1, new object[][] { new object[] { "IBM", 85d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 87d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 109d } });
            expected.AddResultInsert(4900, 1, new object[][] { new object[] { "YAH", 112d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 87d } });
            expected.AddResultInsert(5900, 1, new object[][] { new object[] { "YAH", 88d } });
            expected.AddResultRemove(6300, 0, new object[][] { new object[] { "MSFT", 79d } });
            expected.AddResultRemove(
                7000,
                0,
                new object[][] { new object[] { "IBM", 54d }, new object[] { "YAH", 54d } });

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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 109d } });
            expected.AddResultInsert(4900, 1, new object[][] { new object[] { "YAH", 112d } });

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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][] { new object[] { "MSFT", 34d } });
            expected.AddResultInsert(2200, 0, new object[][] { new object[] { "IBM", 85d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "YAH", 87d } });
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "YAH", 112d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "YAH", 88d } },
                new object[][] { new object[] { "IBM", 87d } });
            expected.AddResultRemove(7200, 0, new object[][] { new object[] { "YAH", 54d } });

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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "YAH", 112d } });
            expected.AddResultInsRem(6200, 0, null, null);
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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 109d }, new object[] { "YAH", 112d } },
                null);
            expected.AddResultInsRem(6200, 0, null, null);
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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 34d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][]
                    { new object[] { "IBM", 58d }, new object[] { "YAH", 59d }, new object[] { "IBM", 85d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "YAH", 87d } });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] { new object[] { "IBM", 109d }, new object[] { "YAH", 112d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "YAH", 88d } },
                new object[][] { new object[] { "IBM", 87d } });
            expected.AddResultRemove(
                7200,
                0,
                new object[][]
                    { new object[] { "MSFT", 79d }, new object[] { "IBM", 54d }, new object[] { "YAH", 54d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion17IStreamOnly(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 25d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 58d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 87d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 109d } });
            expected.AddResultInsert(5900, 1, new object[][] { new object[] { "YAH", 88d } });

            var execution = new ResultAssertExecution(
                stmtText,
                env,
                expected,
                ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion17IRStream(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 25d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 58d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 87d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 109d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 87d } });
            expected.AddResultRemove(6300, 0, new object[][] { new object[] { "MSFT", 79d } });

            var execution = new ResultAssertExecution(
                stmtText,
                env,
                expected,
                ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
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

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 34d }, new object[] { "MSFT", 34d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 85d }, new object[] { "MSFT", 85d }, new object[] { "IBM", 85d },
                    new object[] { "YAH", 85d }, new object[] { "IBM", 85d }
                });
            expected.AddResultInsert(
                3200,
                0,
                new object[][] {
                    new object[] { "IBM", 85d }, new object[] { "MSFT", 85d }, new object[] { "IBM", 85d },
                    new object[] { "YAH", 85d }, new object[] { "IBM", 85d }
                });
            expected.AddResultInsert(
                4200,
                0,
                new object[][] {
                    new object[] { "IBM", 87d }, new object[] { "MSFT", 87d }, new object[] { "IBM", 87d },
                    new object[] { "YAH", 87d }, new object[] { "IBM", 87d }, new object[] { "YAH", 87d }
                });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] {
                    new object[] { "IBM", 112d }, new object[] { "MSFT", 112d }, new object[] { "IBM", 112d },
                    new object[] { "YAH", 112d }, new object[] { "IBM", 112d }, new object[] { "YAH", 112d },
                    new object[] { "IBM", 112d }, new object[] { "YAH", 112d }
                });
            expected.AddResultInsert(
                6200,
                0,
                new object[][] {
                    new object[] { "MSFT", 88d }, new object[] { "IBM", 88d }, new object[] { "YAH", 88d },
                    new object[] { "IBM", 88d }, new object[] { "YAH", 88d }, new object[] { "IBM", 88d },
                    new object[] { "YAH", 88d }, new object[] { "YAH", 88d }
                });
            expected.AddResultInsert(
                7200,
                0,
                new object[][] {
                    new object[] { "IBM", 54d }, new object[] { "YAH", 54d }, new object[] { "IBM", 54d },
                    new object[] { "YAH", 54d }, new object[] { "YAH", 54d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertionHaving(RegressionEnvironment env)
        {
            SendEvent(env, "SYM1", 10d);
            SendEvent(env, "SYM1", 11d);
            SendEvent(env, "SYM1", 9);

            SendTimer(env, 1000);
            var fields = "symbol,avgPrice".SplitCsv();
            env.AssertPropsNew("s0", fields, new object[] { "SYM1", 10.5 });

            SendEvent(env, "SYM1", 13d);
            SendEvent(env, "SYM1", 10d);
            SendEvent(env, "SYM1", 9);
            SendTimer(env, 2000);

            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(3, listener.LastNewData.Length);
                    Assert.IsNull(listener.LastOldData);
                    EPAssertionUtil.AssertPropsPerRow(
                        listener.LastNewData,
                        fields,
                        new object[][] {
                            new object[] { "SYM1", 43 / 4.0 }, new object[] { "SYM1", 53.0 / 5.0 },
                            new object[] { "SYM1", 62 / 6.0 }
                        });
                });

            env.UndeployAll();
        }

        private static void TryAssertLastSum(RegressionEnvironment env)
        {
            // send an event
            SendEvent(env, 1);

            // check no update
            env.AssertListenerNotInvoked("s0");

            // send another event
            SendEvent(env, 2);

            // check update, all events present
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(1, listener.LastNewData.Length);
                    Assert.AreEqual(2L, listener.LastNewData[0].Get("longBoxed"));
                    Assert.AreEqual(3L, listener.LastNewData[0].Get("result"));
                    Assert.IsNull(listener.LastOldData);
                });

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0);
        }

        private static void SendMarketDataEvent(
            RegressionEnvironment env,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean("SYM1", 0, volume, null));
        }

        private static void SendTimeEventRelative(
            RegressionEnvironment env,
            int timeIncrement,
            AtomicLong currentTime)
        {
            currentTime.IncrementAndGet(timeIncrement);
            env.AdvanceTime(currentTime.Get());
        }

        private static void CreateStmtAndListenerJoin(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBeanString(JOIN_KEY));
        }

        private static void AssertEvent(
            RegressionEnvironment env,
            long volume)
        {
            env.AssertListener(
                "s0",
                listener => {
                    Assert.IsTrue(listener.LastNewData != null);
                    Assert.AreEqual(1, listener.LastNewData.Length);
                    Assert.AreEqual(volume, listener.LastNewData[0].Get("sum(volume)"));
                    listener.Reset();
                });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
        }
    }
} // end of namespace