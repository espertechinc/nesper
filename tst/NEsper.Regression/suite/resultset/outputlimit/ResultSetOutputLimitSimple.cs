///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitSimple
    {
        private const string JOIN_KEY = "KEY";
        private const string CATEGORY = "Un-aggregated and Un-grouped";

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
            With17FirstNoHavingNoJoinIStream(execs);
            With17FirstNoHavingJoinIStream(execs);
            With17FirstNoHavingNoJoinIRStream(execs);
            With17FirstNoHavingJoinIRStream(execs);
            With18SnapshotNoHavingNoJoin(execs);
            WithOutputEveryTimePeriod(execs);
            WithOutputEveryTimePeriodVariable(execs);
            WithAggAllHaving(execs);
            WithAggAllHavingJoin(execs);
            WithIterator(execs);
            WithLimitEventJoin(execs);
            WithLimitTime(execs);
            WithTimeBatchOutputEvents(execs);
            WithSimpleNoJoinAll(execs);
            WithSimpleNoJoinLast(execs);
            WithSimpleJoinAll(execs);
            WithSimpleJoinLast(execs);
            WithLimitEventSimple(execs);
            WithLimitSnapshot(execs);
            WithFirstSimpleHavingAndNoHaving(execs);
            WithLimitSnapshotJoin(execs);
            WithSnapshotMonthScoped(execs);
            WithFirstMonthScoped(execs);
            WithOutputFirstUnidirectionalJoinNamedWindow(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstUnidirectionalJoinNamedWindow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstUnidirectionalJoinNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetFirstMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithSnapshotMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSnapshotMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshotJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshotJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstSimpleHavingAndNoHaving(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetFirstSimpleHavingAndNoHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshot());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitEventSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitEventSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSimpleJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSimpleJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleNoJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSimpleNoJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleNoJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSimpleNoJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchOutputEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetTimeBatchOutputEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitTime(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitTime());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitEventJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitEventJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetIterator());
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

        public static IList<RegressionExecution> WithOutputEveryTimePeriodVariable(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputEveryTimePeriodVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputEveryTimePeriod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputEveryTimePeriod());
            return execs;
        }

        public static IList<RegressionExecution> With18SnapshotNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingJoinIRStream(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingJoinIRStream());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingNoJoinIRStream(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingNoJoinIRStream());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingJoinIStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingJoinIStream());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingNoJoinIStream(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingNoJoinIStream());
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
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec)";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               " having Price > 10";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               " having Price > 10";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "having Price > 10" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "having Price > 10" +
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
                           "@name('s0') select Symbol, Volume, Price " +
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
                           "@name('s0') select Symbol, Volume, Price " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
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
                           "@name('s0') select Symbol, Volume, Price " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "having Price > 10" +
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
                           "@name('s0') select Symbol, Volume, Price " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "having Price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all", milestone);
        }

        internal class ResultSet13LastNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "output last every 1 seconds";
                TryAssertion13_14(env, stmtText, "last", new AtomicLong());
            }
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "output last every 1 seconds";
                TryAssertion13_14(env, stmtText, "last", new AtomicLong());
            }
        }

        internal class ResultSet15LastHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "having Price > 10 " +
                               "output last every 1 seconds";
                TryAssertion15_16(env, stmtText, "last", new AtomicLong());
            }
        }

        internal class ResultSet16LastHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "having Price > 10 " +
                               "output last every 1 seconds";
                TryAssertion15_16(env, stmtText, "last", new AtomicLong());
            }
        }

        internal class ResultSet17FirstNoHavingNoJoinIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IStream(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet17FirstNoHavingJoinIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec)," +
                               "SupportBean#keepall where TheString=Symbol " +
                               "output first every 1 seconds";
                TryAssertion17IStream(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet17FirstNoHavingNoJoinIRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select irstream Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IRStream(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet17FirstNoHavingJoinIRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select irstream Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "output first every 1 seconds";
                TryAssertion17IRStream(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume, Price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSetOutputFirstUnidirectionalJoinNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = "c0,c1".SplitCsv();
                var epl =
                    "create window MyWindow#keepall as SupportBean_S0;\n" +
                    "insert into MyWindow select * from SupportBean_S0;\n" +
                    "@name('s0') select myWindow.Id as c0, s1.Id as c1\n" +
                    "from SupportBean_S1 as s1 unidirectional, MyWindow as myWindow\n" +
                    "where myWindow.P00 = s1.P10\n" +
                    "output first every 1 minutes;";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "a"));
                env.SendEventBean(new SupportBean_S0(20, "b"));
                env.SendEventBean(new SupportBean_S1(1000, "b"));
                env.AssertPropsNew("s0", fields, new object[] { 20, 1000 });

                env.SendEventBean(new SupportBean_S1(1001, "b"));
                env.SendEventBean(new SupportBean_S1(1002, "a"));
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(60 * 1000);
                env.SendEventBean(new SupportBean_S1(1003, "a"));
                env.AssertPropsNew("s0", fields, new object[] { 10, 1003 });

                env.SendEventBean(new SupportBean_S1(1004, "a"));
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(120 * 1000);
                env.SendEventBean(new SupportBean_S1(1005, "a"));
                env.AssertPropsNew("s0", fields, new object[] { 10, 1005 });

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputEveryTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(2000);

                var stmtText =
                    "@name('s0') select Symbol from SupportMarketDataBean#keepall output snapshot every 1 day 2 hours 3 minutes 4 seconds 5 milliseconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendMDEvent(env, "E1", 0);

                long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
                var deltaMSec = deltaSec * 1000 + 5 + 2000;
                env.AdvanceTime(deltaMSec - 1);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(deltaMSec);
                env.AssertEqualsNew("s0", "Symbol", "E1");

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputEveryTimePeriodVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(2000);

                var stmtText =
                    "@name('s0') select Symbol from SupportMarketDataBean#keepall output snapshot every D days H hours M minutes S seconds MS milliseconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendMDEvent(env, "E1", 0);

                long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
                var deltaMSec = deltaSec * 1000 + 5 + 2000;
                env.AdvanceTime(deltaMSec - 1);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(deltaMSec);
                env.AssertEqualsNew("s0", "Symbol", "E1");

                // test statement model
                var model = env.EplToModel(stmtText);
                ClassicAssert.AreEqual(stmtText, model.ToEPL());

                env.UndeployAll();
            }
        }

        internal class ResultSetAggAllHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume " +
                               "from SupportMarketDataBean#length(10) as two " +
                               "having Volume > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "Symbol", "Volume" };

                SendMDEvent(env, "S0", 20);
                SendMDEvent(env, "IBM", -1);
                SendMDEvent(env, "MSFT", -2);
                SendMDEvent(env, "YAH", 10);
                env.AssertListenerNotInvoked("s0");

                SendMDEvent(env, "IBM", 0);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S0", 20L }, new object[] { "YAH", 10L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggAllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, Volume " +
                               "from SupportMarketDataBean#length(10) as one," +
                               "SupportBean#length(10) as two " +
                               "where one.Symbol=two.TheString " +
                               "having Volume > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "Symbol", "Volume" };
                env.SendEventBean(new SupportBean("S0", 0));
                env.SendEventBean(new SupportBean("IBM", 0));
                env.SendEventBean(new SupportBean("MSFT", 0));
                env.SendEventBean(new SupportBean("YAH", 0));

                SendMDEvent(env, "S0", 20);
                SendMDEvent(env, "IBM", -1);
                SendMDEvent(env, "MSFT", -2);
                SendMDEvent(env, "YAH", 10);
                env.AssertListenerNotInvoked("s0");

                SendMDEvent(env, "IBM", 0);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S0", 20L }, new object[] { "YAH", 10L } });

                env.UndeployAll();
            }
        }

        internal class ResultSetIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "Price" };
                var epl = "@name('s0') select Symbol, TheString, Price from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "output every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));

                // Output limit clause ignored when iterating, for both joins and no-join
                SendEvent(env, "CAT", 50);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "CAT", 50d } });

                SendEvent(env, "CAT", 60);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "CAT", 50d }, new object[] { "CAT", 60d } });

                SendEvent(env, "IBM", 70);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "CAT", 50d }, new object[] { "CAT", 60d }, new object[] { "IBM", 70d } });

                SendEvent(env, "IBM", 90);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 50d }, new object[] { "CAT", 60d }, new object[] { "IBM", 70d },
                        new object[] { "IBM", 90d }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitEventJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement =
                    "select * from SupportBean#length(5) as event1," +
                    "SupportBean_A#length(5) as event2" +
                    " where event1.TheString = event2.Id";
                var outputStmt1 = joinStatement + " output every 1 events";
                var outputStmt3 = joinStatement + " output every 3 events";

                env.CompileDeploy("@name('s1') " + outputStmt1).AddListener("s1");
                env.CompileDeploy("@name('s3') " + outputStmt3).AddListener("s3");

                // send event 1
                SendJoinEvents(env, "IBM");

                env.AssertListener(
                    "s1",
                    listener => {
                        ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                        ClassicAssert.IsNull(listener.LastOldData);
                    });
                env.AssertListenerNotInvoked("s3");

                // send event 2
                SendJoinEvents(env, "MSFT");

                env.AssertListenerInvoked("s1");
                env.AssertListenerNotInvoked("s3");

                // send event 3
                SendJoinEvents(env, "YAH");

                env.AssertListener(
                    "s1",
                    listener => {
                        ClassicAssert.IsTrue(listener.IsInvoked);
                        ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                        ClassicAssert.IsNull(listener.LastOldData);
                    });
                env.AssertListener(
                    "s3",
                    listener => {
                        ClassicAssert.IsTrue(listener.GetAndClearIsInvoked());
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        ClassicAssert.IsNull(listener.LastOldData);
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var selectStatement = "@name('s0') select * from SupportBean#length(5)";

                // test integer seconds
                var statementString1 = selectStatement +
                                       " output every 3 seconds";
                TimeCallback(env, statementString1, 3000);

                // test fractional seconds
                var statementString2 = selectStatement +
                                       " output every 3.3 seconds";
                TimeCallback(env, statementString2, 3300);

                // test integer minutes
                var statementString3 = selectStatement +
                                       " output every 2 minutes";
                TimeCallback(env, statementString3, 120000);

                // test fractional minutes
                var statementString4 =
                    "@name('s0') select * from SupportBean#length(5) output every .05 minutes";
                TimeCallback(env, statementString4, 3000);
            }
        }

        internal class ResultSetTimeBatchOutputEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from SupportBean#time_batch(10 seconds) output every 10 seconds";
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
                        ClassicAssert.AreEqual(1, newEvents.Length);
                        ClassicAssert.AreEqual("e1", newEvents[0].Get("TheString"));
                        listener.Reset();
                    });

                SendTimer(env, 50000);
                env.AssertListenerInvoked("s0");

                SendTimer(env, 60000);
                env.AssertListenerInvoked("s0");

                SendTimer(env, 70000);
                env.AssertListenerInvoked("s0");

                SendEvent(env, "e2");
                SendEvent(env, "e3");
                SendTimer(env, 80000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newEvents = listener.GetAndResetLastNewData();
                        ClassicAssert.AreEqual(2, newEvents.Length);
                        ClassicAssert.AreEqual("e2", newEvents[0].Get("TheString"));
                        ClassicAssert.AreEqual("e3", newEvents[1].Get("TheString"));
                    });

                SendTimer(env, 90000);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetSimpleNoJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionSimpleNoJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionSimpleNoJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@name('s0') select LongBoxed " +
                          "from SupportBean#length(3) " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertAll(env);

                epl = opt.GetHint() +
                      "@name('s0') select LongBoxed " +
                      "from SupportBean#length(3) " +
                      "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertAll(env);

                epl = opt.GetHint() +
                      "@name('s0') select * " +
                      "from SupportBean#length(3) " +
                      "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertAll(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetSimpleNoJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select LongBoxed " +
                          "from SupportBean#length(3) " +
                          "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertLast(env);

                epl = "@name('s0') select * " +
                      "from SupportBean#length(3) " +
                      "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertLast(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetSimpleJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionSimpleJoinAll(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSetSimpleJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select LongBoxed " +
                          "from SupportBeanString#length(3) as one, " +
                          "SupportBean#length(3) as two " +
                          "output last every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertLast(env);
                env.UndeployAll();
            }
        }

        internal class ResultSetLimitEventSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var selectStmt = "select * from SupportBean#length(5)";
                var statement1 = "@name('s0') " + selectStmt + " output every 1 events";
                var statement2 = "@name('s1') " + selectStmt + " output every 2 events";
                var statement3 = "@name('s2') " + selectStmt + " output every 3 events";

                env.CompileDeploy(statement1).AddListener("s0");
                env.CompileDeploy(statement2).AddListener("s1");
                env.CompileDeploy(statement3).AddListener("s2");

                // send event 1
                SendEvent(env, "IBM");

                env.AssertListenerInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerNotInvoked("s2");

                // send event 2
                SendEvent(env, "MSFT");

                env.AssertListenerInvoked("s0");
                env.AssertListener(
                    "s1",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastNewData.Length);
                        ClassicAssert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });
                env.AssertListenerNotInvoked("s2");

                // send event 3
                SendEvent(env, "YAH");

                env.AssertListenerInvoked("s0");
                env.AssertListenerNotInvoked("s1");
                env.AssertListener(
                    "s2",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        ClassicAssert.IsNull(listener.LastOldData);
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                SendTimer(env, 0);
                var selectStmt = "@name('s0') select * from SupportBean#time(10) output snapshot every 3 events";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendTimer(env, 1000);
                SendEvent(env, "IBM");
                SendEvent(env, "MSFT");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 2000);
                SendEvent(env, "YAH");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "IBM" }, new object[] { "MSFT" }, new object[] { "YAH" } });

                SendTimer(env, 3000);
                SendEvent(env, "s4");
                SendEvent(env, "s5");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 10000);
                SendEvent(env, "s6");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "IBM" }, new object[] { "MSFT" }, new object[] { "YAH" }, new object[] { "s4" },
                        new object[] { "s5" }, new object[] { "s6" }
                    });

                SendTimer(env, 11000);
                SendEvent(env, "s7");
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "s8");
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "s9");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "YAH" }, new object[] { "s4" }, new object[] { "s5" }, new object[] { "s6" },
                        new object[] { "s7" }, new object[] { "s8" }, new object[] { "s9" }
                    });

                SendTimer(env, 14000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "s6" }, new object[] { "s7" }, new object[] { "s8" }, new object[] { "s9" } });

                SendEvent(env, "s10");
                SendEvent(env, "s11");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 23000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "s10" }, new object[] { "s11" } });

                SendEvent(env, "s12");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetFirstSimpleHavingAndNoHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionFirstSimpleHavingAndNoHaving(env, "");
                TryAssertionFirstSimpleHavingAndNoHaving(env, "having IntPrimitive != 0");
            }

            private static void TryAssertionFirstSimpleHavingAndNoHaving(
                RegressionEnvironment env,
                string having)
            {
                var epl = "@name('s0') select TheString from SupportBean " + having + " output first every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", "TheString".SplitCsv(), new object[] { "E1" });

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertPropsNew("s0", "TheString".SplitCsv(), new object[] { "E4" });

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                SendTimer(env, 0);
                var selectStmt = "@name('s0') select TheString from SupportBean#time(10) as s," +
                                 "SupportMarketDataBean#keepall as m where s.TheString = m.Symbol output snapshot every 3 events order by Symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                foreach (var symbol in "s0,s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11".SplitCsv()) {
                    env.SendEventBean(new SupportMarketDataBean(symbol, 0, 0L, ""));
                }

                SendTimer(env, 1000);
                SendEvent(env, "s0");
                SendEvent(env, "s1");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 2000);
                SendEvent(env, "s2");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "s0" }, new object[] { "s1" }, new object[] { "s2" } });

                SendTimer(env, 3000);
                SendEvent(env, "s4");
                SendEvent(env, "s5");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 10000);
                SendEvent(env, "s6");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "s0" }, new object[] { "s1" }, new object[] { "s2" }, new object[] { "s4" },
                        new object[] { "s5" }, new object[] { "s6" }
                    });

                SendTimer(env, 11000);
                SendEvent(env, "s7");
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "s8");
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "s9");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "s2" }, new object[] { "s4" }, new object[] { "s5" }, new object[] { "s6" },
                        new object[] { "s7" }, new object[] { "s8" }, new object[] { "s9" }
                    });

                SendTimer(env, 14000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "s6" }, new object[] { "s7" }, new object[] { "s8" }, new object[] { "s9" } });

                SendEvent(env, "s10");
                SendEvent(env, "s11");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 23000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "s10" }, new object[] { "s11" } });

                SendEvent(env, "s12");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetSnapshotMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                var epl = "@name('s0') select * from SupportBean#lastevent output snapshot every 1 month";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.AssertListenerNotInvoked("s0");

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.AssertPropsPerRowLastNew("s0", "TheString".SplitCsv(), new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        internal class ResultSetFirstMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                var epl = "@name('s0') select * from SupportBean#lastevent output first every 1 month";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean("E2", 2));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertListenerNotInvoked("s0");

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertPropsPerRowLastNew("s0", "TheString".SplitCsv(), new object[][] { new object[] { "E4" } });

                env.UndeployAll();
            }
        }

        private static void CreateStmtAndListenerJoin(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBeanString(JOIN_KEY));
        }

        private static void TryAssertLast(RegressionEnvironment env)
        {
            // send an event
            SendEvent(env, 1);

            // check no update
            env.AssertListenerNotInvoked("s0");

            // send another event
            SendEvent(env, 2);

            // check update, only the last event present
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                    ClassicAssert.AreEqual(2L, listener.LastNewData[0].Get("LongBoxed"));
                    ClassicAssert.IsNull(listener.LastOldData);
                });
            env.UndeployAll();
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
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
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0);
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

        private static void TimeCallback(
            RegressionEnvironment env,
            string epl,
            int timeToCallback)
        {
            // set the clock to 0
            var currentTime = new AtomicLong();
            SendTimeEvent(env, 0, currentTime);

            // create the EPL statement and add a listener
            env.CompileDeploy(epl).AddListener("s0");

            // send an event
            SendEvent(env, "IBM");

            // check that the listener hasn't been updated
            SendTimeEvent(env, timeToCallback - 1, currentTime);
            env.AssertListenerNotInvoked("s0");

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsTrue(listener.IsInvoked);
                    ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                    ClassicAssert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            // send another event
            SendEvent(env, "MSFT");

            // check that the listener hasn't been updated
            env.AssertListenerNotInvoked("s0");

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsTrue(listener.IsInvoked);
                    ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                    ClassicAssert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            // don't send an event
            // check that the listener hasn't been updated
            env.AssertListenerNotInvoked("s0");

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsTrue(listener.IsInvoked);
                    ClassicAssert.IsNull(listener.LastNewData);
                    ClassicAssert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            // don't send an event
            // check that the listener hasn't been updated
            env.AssertListenerNotInvoked("s0");

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsTrue(listener.IsInvoked);
                    ClassicAssert.IsNull(listener.LastNewData);
                    ClassicAssert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            // send several events
            SendEvent(env, "YAH");
            SendEvent(env, "s4");
            SendEvent(env, "s5");

            // check that the listener hasn't been updated
            env.AssertListenerNotInvoked("s0");

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsTrue(listener.IsInvoked);
                    ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                    ClassicAssert.IsNull(listener.LastOldData);
                });

            env.UndeployAll();
        }

        private static void TryAssertionSimpleJoinAll(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
                      "@name('s0') select LongBoxed  " +
                      "from SupportBeanString#length(3) as one, " +
                      "SupportBean#length(3) as two " +
                      "output all every 2 events";

            CreateStmtAndListenerJoin(env, epl);
            TryAssertAll(env);

            env.UndeployAll();
        }

        private static void TryAssertAll(RegressionEnvironment env)
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
                    ClassicAssert.AreEqual(2, listener.LastNewData.Length);
                    ClassicAssert.AreEqual(1L, listener.LastNewData[0].Get("LongBoxed"));
                    ClassicAssert.AreEqual(2L, listener.LastNewData[1].Get("LongBoxed"));
                    ClassicAssert.IsNull(listener.LastOldData);
                });

            env.UndeployAll();
        }

        private static void TryAssertion34(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "Symbol", "Volume", "Price" };

            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 150L, 24d } });
            expected.AddResultInsert(2100, 1, new object[][] { new object[] { "IBM", 155L, 26d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 22d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultRemove(7000, 0, new object[][] { new object[] { "IBM", 150L, 24d } });

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

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);

            expected.AddResultInsert(1200, 0, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(2200, 0, new object[][] { new object[] { "IBM", 155L, 26d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "IBM", 150L, 22d } });
            expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultRemove(7200, 0, new object[][] { new object[] { "IBM", 150L, 24d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(800, 1, new object[][] { new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 150L, 24d } });
            expected.AddResultInsert(1500, 2, new object[][] { new object[] { "YAH", 10000L, 1d } });
            expected.AddResultInsert(2100, 1, new object[][] { new object[] { "IBM", 155L, 26d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 11000L, 2d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 22d } });
            expected.AddResultInsert(4900, 1, new object[][] { new object[] { "YAH", 11500L, 3d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(5900, 1, new object[][] { new object[] { "YAH", 10500L, 1d } });
            expected.AddResultRemove(6300, 0, new object[][] { new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultRemove(
                7000,
                0,
                new object[][] { new object[] { "IBM", 150L, 24d }, new object[] { "YAH", 10000L, 1d } });

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

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][] { new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(2200, 0, new object[][] { new object[] { "IBM", 155L, 26d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "YAH", 11000L, 2d } });
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "YAH", 11500L, 3d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "YAH", 10500L, 1d } },
                new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultRemove(7200, 0, new object[][] { new object[] { "YAH", 10000L, 1d } });

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

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] { new object[] { "IBM", 150L, 24d }, new object[] { "IBM", 155L, 26d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "IBM", 150L, 22d } });
            expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultRemove(7200, 0, new object[][] { new object[] { "IBM", 150L, 24d } });

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

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 24d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 26d }
                });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "YAH", 11000L, 2d } });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] { new object[] { "IBM", 150L, 22d }, new object[] { "YAH", 11500L, 3d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "YAH", 10500L, 1d } },
                new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultRemove(
                7200,
                0,
                new object[][] {
                    new object[] { "MSFT", 5000L, 9d }, new object[] { "IBM", 150L, 24d },
                    new object[] { "YAH", 10000L, 1d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion17IStream(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 150L, 24d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 11000L, 2d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 22d } });
            expected.AddResultInsert(5900, 1, new object[][] { new object[] { "YAH", 10500L, 1.0d } });

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

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 150L, 24d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 11000L, 2d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 22d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultRemove(6300, 0, new object[][] { new object[] { "MSFT", 5000L, 9d } });

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

            var fields = new string[] { "Symbol", "Volume", "Price" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 24d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 26d }
                });
            expected.AddResultInsert(
                3200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 24d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 26d }
                });
            expected.AddResultInsert(
                4200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 24d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 26d }, new object[] { "YAH", 11000L, 2d }
                });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 24d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 26d }, new object[] { "YAH", 11000L, 2d },
                    new object[] { "IBM", 150L, 22d }, new object[] { "YAH", 11500L, 3d }
                });
            expected.AddResultInsert(
                6200,
                0,
                new object[][] {
                    new object[] { "MSFT", 5000L, 9d }, new object[] { "IBM", 150L, 24d },
                    new object[] { "YAH", 10000L, 1d }, new object[] { "IBM", 155L, 26d },
                    new object[] { "YAH", 11000L, 2d }, new object[] { "IBM", 150L, 22d },
                    new object[] { "YAH", 11500L, 3d }, new object[] { "YAH", 10500L, 1d }
                });
            expected.AddResultInsert(
                7200,
                0,
                new object[][] {
                    new object[] { "IBM", 155L, 26d }, new object[] { "YAH", 11000L, 2d },
                    new object[] { "IBM", 150L, 22d }, new object[] { "YAH", 11500L, 3d },
                    new object[] { "YAH", 10500L, 1d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            int timeIncrement,
            AtomicLong currentTime)
        {
            currentTime.IncrementAndGet(timeIncrement);
            env.AdvanceTime(currentTime.Get());
        }

        private static void SendJoinEvents(
            RegressionEnvironment env,
            string s)
        {
            var event1 = new SupportBean();
            event1.TheString = s;
            event1.DoubleBoxed = 0.0;
            event1.IntPrimitive = 0;
            event1.IntBoxed = 0;

            var event2 = new SupportBean_A(s);

            env.SendEventBean(event1);
            env.SendEventBean(event2);
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
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
    }
} // end of namespace