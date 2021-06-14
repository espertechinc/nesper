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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFilterNamedParameter
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFirstAggSODA(execs);
            WithMethodAggSQLAll(execs);
            WithMethodAggSQLMixedFilter(execs);
            WithMethodAggLeaving(execs);
            WithMethodAggNth(execs);
            WithMethodAggRateUnbound(execs);
            WithMethodAggRateBound(execs);
            WithAccessAggLinearBound(execs);
            WithAccessAggLinearUnbound(execs);
            WithAccessAggLinearWIndex(execs);
            WithAccessAggLinearBoundMixedFilter(execs);
            WithAccessAggSortedBound(execs);
            WithAccessAggSortedUnbound(execs);
            WithAccessAggSortedMulticriteria(execs);
            WithAuditAndReuse(execs);
            WithFilterNamedParamInvalid(execs);
            WithMethodPlugIn(execs);
            WithAccessAggPlugIn(execs);
            WithIntoTable(execs);
            WithIntoTableCountMinSketch(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithIntoTableCountMinSketch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateIntoTableCountMinSketch());
            return execs;
        }

        public static IList<RegressionExecution> WithIntoTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateIntoTable(false));
            execs.Add(new ResultSetAggregateIntoTable(true));
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggPlugIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggPlugIn());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodPlugIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodPlugIn());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterNamedParamInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFilterNamedParamInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithAuditAndReuse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAuditAndReuse());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggSortedMulticriteria(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggSortedMulticriteria());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggSortedUnbound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggSortedUnbound(false));
            execs.Add(new ResultSetAggregateAccessAggSortedUnbound(true));
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggSortedBound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggSortedBound(false));
            execs.Add(new ResultSetAggregateAccessAggSortedBound(true));
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggLinearBoundMixedFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggLinearBoundMixedFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggLinearWIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggLinearWIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggLinearUnbound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggLinearUnbound(false));
            execs.Add(new ResultSetAggregateAccessAggLinearUnbound(true));
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggLinearBound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAccessAggLinearBound(false));
            execs.Add(new ResultSetAggregateAccessAggLinearBound(true));
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAggRateBound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAggRateBound());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAggRateUnbound(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAggRateUnbound());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAggNth(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAggNth());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAggLeaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAggLeaving());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAggSQLMixedFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAggSQLMixedFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAggSQLAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAggSQLAll());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstAggSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstAggSODA(false));
            execs.Add(new ResultSetAggregateFirstAggSODA(true));
            return execs;
        }

        private static void SendEventAssertSQLFuncs(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            double doublePrimitive,
            object cAvedev,
            object cAvg,
            object cCount,
            object cMax,
            object cFmax,
            object cMaxever,
            object cFmaxever,
            object cMedian,
            object cMin,
            object cFmin,
            object cMinever,
            object cFminever,
            object cStddev,
            object cSum)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            env.SendEventBean(sb);
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(cAvedev, @event.Get("cAvedev"));
            Assert.AreEqual(cAvg, @event.Get("cAvg"));
            Assert.AreEqual(cCount, @event.Get("cCount"));
            Assert.AreEqual(cMax, @event.Get("cMax"));
            Assert.AreEqual(cFmax, @event.Get("cFmax"));
            Assert.AreEqual(cMaxever, @event.Get("cMaxever"));
            Assert.AreEqual(cFmaxever, @event.Get("cFmaxever"));
            Assert.AreEqual(cMedian, @event.Get("cMedian"));
            Assert.AreEqual(cMin, @event.Get("cMin"));
            Assert.AreEqual(cFmin, @event.Get("cFmin"));
            Assert.AreEqual(cMinever, @event.Get("cMinever"));
            Assert.AreEqual(cFminever, @event.Get("cFminever"));
            Assert.AreEqual(cStddev, @event.Get("cStddev"));
            Assert.AreEqual(cSum, @event.Get("cSum"));
        }

        private static void SendEventAssertEventsAsList(
            RegressionEnvironment env,
            string theString,
            string expected)
        {
            SendEvent(env, theString, 0);
            var value = env.Listener("s0").AssertOneGetNewAndReset().Get("c0").Unwrap<object>();
            Assert.AreEqual(expected, value.RenderAny());
        }

        private static void SendEventAssert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            string[] fields,
            object[] expected)
        {
            SendEvent(env, theString, intPrimitive);
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
        }

        private static void SendEventAssertIsolated(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            string[] fields,
            object[] expected)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            return SendEvent(env, theString, intPrimitive, -1);
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            env.SendEventBean(sb);
            return sb;
        }

        private static void SendEventAssertInfoTable(
            RegressionEnvironment env,
            object ta,
            object tb,
            object wa,
            object wb,
            object sa,
            object sb)
        {
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"ta", "tb", "wa", "wb", "sa", "sb"},
                new[] {ta, tb, wa, wb, sa, sb});
        }

        private static void SendEventAssertCount(
            RegressionEnvironment env,
            string p00,
            object expected)
        {
            env.SendEventBean(new SupportBean_S0(0, p00));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"c0"},
                new[] {expected});
        }

        private static void SendEventWLong(
            RegressionEnvironment env,
            string theString,
            long longPrimitive,
            int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        internal class ResultSetAggregateAccessAggPlugIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select eventsAsList(TheString, filter:TheString like 'A%') as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssertEventsAsList(env, "X1", "[]");
                SendEventAssertEventsAsList(env, "A1", "[SupportBean(\"A1\", 0)]");
                SendEventAssertEventsAsList(env, "A2", "[SupportBean(\"A1\", 0), SupportBean(\"A2\", 0)]");
                SendEventAssertEventsAsList(env, "X2", "[SupportBean(\"A1\", 0), SupportBean(\"A2\", 0)]");

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodPlugIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0"};
                var epl =
                    "@Name('s0') select concatMethodAgg(TheString, filter:TheString like 'A%') as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssert(
                    env,
                    "X1",
                    0,
                    fields,
                    new object[] {""});
                SendEventAssert(
                    env,
                    "A1",
                    0,
                    fields,
                    new object[] {"A1"});
                SendEventAssert(
                    env,
                    "A2",
                    0,
                    fields,
                    new object[] {"A1 A2"});
                SendEventAssert(
                    env,
                    "X2",
                    0,
                    fields,
                    new object[] {"A1 A2"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateIntoTableCountMinSketch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table WordCountTable(wordcms countMinSketch());\n" +
                          "into table WordCountTable select countMinSketchAdd(TheString, filter:IntPrimitive > 0) as wordcms from SupportBean;\n" +
                          "@Name('s0') select WordCountTable.wordcms.CountMinSketchFrequency(P00) as c0 from SupportBean_S0;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "hello", 0);
                SendEventAssertCount(env, "hello", 0L);

                SendEvent(env, "name", 1);
                SendEventAssertCount(env, "name", 1L);

                SendEvent(env, "name", 0);
                SendEventAssertCount(env, "name", 1L);

                SendEvent(env, "name", 1);
                SendEventAssertCount(env, "name", 2L);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAggRateBound : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"myrate", "myqtyrate"};
                var epl = "@Name('s0') select " +
                          "rate(LongPrimitive, filter:TheString like 'A%') as myrate, " +
                          "rate(LongPrimitive, IntPrimitive, filter:TheString like 'A%') as myqtyrate " +
                          "from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventWLong(env, "X1", 1000, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                SendEventWLong(env, "X2", 1200, 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.Milestone(0);

                SendEventWLong(env, "X2", 1300, 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                SendEventWLong(env, "A1", 1000, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.Milestone(1);

                SendEventWLong(env, "A2", 1200, 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                SendEventWLong(env, "A3", 1300, 0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                SendEventWLong(env, "A4", 1500, 14);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3 * 1000 / 500d, 14 * 1000 / 500d});

                env.Milestone(2);

                SendEventWLong(env, "A5", 2000, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3 * 1000 / 800d, 25 * 1000 / 800d});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAggRateUnbound : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = new[] {"c0"};
                var epl = "@Name('s0') select rate(1, filter:TheString like 'A%') as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssertIsolated(
                    env,
                    "X1",
                    0,
                    fields,
                    new object[] {null});
                SendEventAssertIsolated(
                    env,
                    "A1",
                    1,
                    fields,
                    new object[] {null});

                env.Milestone(0);

                env.AdvanceTime(1000);
                SendEventAssertIsolated(
                    env,
                    "X2",
                    2,
                    fields,
                    new object[] {null});
                SendEventAssertIsolated(
                    env,
                    "A2",
                    2,
                    fields,
                    new object[] {1.0});

                env.Milestone(1);

                SendEventAssertIsolated(
                    env,
                    "A3",
                    3,
                    fields,
                    new object[] {2.0});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAggNth : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0"};
                var epl = "@Name('s0') select nth(IntPrimitive, 1, filter:TheString like 'A%') as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssert(
                    env,
                    "X1",
                    0,
                    fields,
                    new object[] {null});
                SendEventAssert(
                    env,
                    "X2",
                    0,
                    fields,
                    new object[] {null});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "A3",
                    1,
                    fields,
                    new object[] {null});
                SendEventAssert(
                    env,
                    "A4",
                    2,
                    fields,
                    new object[] {1});

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "X3",
                    0,
                    fields,
                    new object[] {1});
                SendEventAssert(
                    env,
                    "A5",
                    3,
                    fields,
                    new object[] {2});

                env.Milestone(2);

                SendEventAssert(
                    env,
                    "X4",
                    0,
                    fields,
                    new object[] {2});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAggLeaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1"};
                var epl = "@Name('s0') select " +
                          "leaving(filter:IntPrimitive=1) as c0," +
                          "leaving(filter:IntPrimitive=2) as c1" +
                          " from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssert(
                    env,
                    "E1",
                    2,
                    fields,
                    new object[] {false, false});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "E2",
                    1,
                    fields,
                    new object[] {false, false});

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "E3",
                    3,
                    fields,
                    new object[] {false, true});

                env.Milestone(2);

                SendEventAssert(
                    env,
                    "E4",
                    4,
                    fields,
                    new object[] {true, true});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAuditAndReuse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "sum(IntPrimitive, filter:IntPrimitive=1) as c0, sum(IntPrimitive, filter:IntPrimitive=1) as c1, " +
                          "window(*, filter:IntPrimitive=1) as c2, window(*, filter:IntPrimitive=1) as c3 " +
                          " from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateFilterNamedParamInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid filter expression name parameter: multiple values
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sum(IntPrimitive, filter:(IntPrimitive, DoublePrimitive)) from SupportBean",
                    "Failed to validate select-clause expression 'sum(IntPrimitive,filter:(IntPrimiti...(55 chars)': Filter named parameter requires a single expression returning a boolean-typed value");

                // multiple filter expressions
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sum(IntPrimitive, IntPrimitive > 0, filter:IntPrimitive < 0) from SupportBean",
                    "Failed to validate select-clause expression 'sum(IntPrimitive,IntPrimitive>0,fil...(54 chars)': Only a single filter expression can be provided");

                // invalid filter expression name parameter: not returning boolean
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sum(IntPrimitive, filter:IntPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'sum(IntPrimitive,filter:IntPrimitive)': Filter named parameter requires a single expression returning a boolean-typed value");

                // create-table does not allow filters
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create table MyTable(totals sum(int, filter:true))",
                    "Failed to validate table-column expression 'sum(int,filter:true)': The 'group_by' and 'filter' parameter is not allowed in create-table statements");

                // invalid correlated subquery
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select (select sum(IntPrimitive, filter:S0.P00='a') from SupportBean) from SupportBean_S0 as S0",
                    "Failed to plan subquery number 1 querying SupportBean: Subselect aggregation functions cannot aggregate across correlated properties");
            }
        }

        internal class ResultSetAggregateIntoTable : RegressionExecution
        {
            private readonly bool join;

            public ResultSetAggregateIntoTable(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplTable =
                    "create table MyTable(\n" +
                    "totalA sum(int, true),\n" +
                    "totalB sum(int, true),\n" +
                    "winA window(*) @type(SupportBean),\n" +
                    "winB window(*) @type(SupportBean),\n" +
                    "sortedA sorted(IntPrimitive) @type(SupportBean),\n" +
                    "sortedB sorted(IntPrimitive) @type(SupportBean)" +
                    ")";
                env.CompileDeploy(eplTable, path);

                var eplInto = "into table MyTable select\n" +
                              "sum(IntPrimitive, filter: TheString like 'A%') as totalA,\n" +
                              "sum(IntPrimitive, filter: TheString like 'B%') as totalB,\n" +
                              "window(sb, filter: TheString like 'A%') as winA,\n" +
                              "window(sb, filter: TheString like 'B%') as winB,\n" +
                              "sorted(sb, filter: TheString like 'A%') as sortedA,\n" +
                              "sorted(sb, filter: TheString like 'B%') as sortedB\n" +
                              "from " +
                              (join ? "SupportBean_S1#lastevent, SupportBean#keepall as sb;\n" : "SupportBean as sb");
                env.CompileDeploy(eplInto, path);

                var eplSelect =
                    "@Name('s0') select MyTable.totalA as ta , MyTable.totalB as tb, MyTable.winA as wa, MyTable.winB as wb, MyTable.sortedA as sa, MyTable.sortedB as sb from SupportBean_S0";
                env.CompileDeploy(eplSelect, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S1(0));

                SendEvent(env, "X1", 1);
                SendEventAssertInfoTable(env, null, null, null, null, null, null);

                env.Milestone(0);

                var a1 = SendEvent(env, "A1", 1);
                SendEventAssertInfoTable(env, 1, null, new[] {a1}, null, new[] {a1}, null);

                env.Milestone(1);

                var b2 = SendEvent(env, "B2", 20);
                SendEventAssertInfoTable(env, 1, 20, new[] {a1}, new[] {b2}, new[] {a1}, new[] {b2});

                var a3 = SendEvent(env, "A3", 10);
                SendEventAssertInfoTable(env, 11, 20, new[] {a1, a3}, new[] {b2}, new[] {a1, a3}, new[] {b2});

                env.Milestone(2);

                var b4 = SendEvent(env, "B4", 2);
                SendEventAssertInfoTable(env, 11, 22, new[] {a1, a3}, new[] {b2, b4}, new[] {a1, a3}, new[] {b4, b2});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggLinearWIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2", "c3"};
                var epl = "@Name('s0') select " +
                          "first(IntPrimitive, 0, filter:TheString like 'A%') as c0," +
                          "first(IntPrimitive, 1, filter:TheString like 'A%') as c1," +
                          "last(IntPrimitive, 0, filter:TheString like 'A%') as c2," +
                          "last(IntPrimitive, 1, filter:TheString like 'A%') as c3" +
                          " from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssert(
                    env,
                    "B1",
                    1,
                    fields,
                    new object[] {null, null, null, null});
                SendEventAssert(
                    env,
                    "A2",
                    2,
                    fields,
                    new object[] {2, null, 2, null});
                SendEventAssert(
                    env,
                    "A3",
                    3,
                    fields,
                    new object[] {2, 3, 3, 2});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "A4",
                    4,
                    fields,
                    new object[] {2, 3, 4, 3});
                SendEventAssert(
                    env,
                    "B2",
                    2,
                    fields,
                    new object[] {3, 4, 4, 3});

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "B3",
                    3,
                    fields,
                    new object[] {4, null, 4, null});
                SendEventAssert(
                    env,
                    "B4",
                    4,
                    fields,
                    new object[] {null, null, null, null});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggSortedBound : RegressionExecution
        {
            private readonly bool join;

            public ResultSetAggregateAccessAggSortedBound(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"aMaxby", "aMinby", "aSorted", "bMaxby", "bMinby", "bSorted"};
                var epl = "@Name('s0') select " +
                          "maxby(IntPrimitive, filter:TheString like 'A%').TheString as aMaxby," +
                          "minby(IntPrimitive, filter:TheString like 'A%').TheString as aMinby," +
                          "sorted(IntPrimitive, filter:TheString like 'A%') as aSorted," +
                          "maxby(IntPrimitive, filter:TheString like 'B%').TheString as bMaxby," +
                          "minby(IntPrimitive, filter:TheString like 'B%').TheString as bMinby," +
                          "sorted(IntPrimitive, filter:TheString like 'B%') as bSorted" +
                          " from " +
                          (join ? "SupportBean_S1#lastevent, SupportBean#length(4)" : "SupportBean#length(4)");
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S1(0));

                env.Milestone(0);

                var b1 = SendEvent(env, "B1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, "B1", "B1", new[] {b1}});

                var a10 = SendEvent(env, "A10", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A10", "A10", new[] {a10}, "B1", "B1", new[] {b1}});

                env.Milestone(1);

                var b2 = SendEvent(env, "B2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A10", "A10", new[] {a10}, "B2", "B1", new[] {b1, b2}});

                var a5 = SendEvent(env, "A5", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A10", "A5", new[] {a5, a10}, "B2", "B1", new[] {b1, b2}});

                var a15 = SendEvent(env, "A15", 15);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A15", "A5", new[] {a5, a10, a15}, "B2", "B2", new[] {b2}});

                SendEvent(env, "X3", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A15", "A5", new[] {a5, a15}, "B2", "B2", new[] {b2}});

                env.Milestone(2);

                SendEvent(env, "X4", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A15", "A5", new[] {a5, a15}, null, null, null});

                SendEvent(env, "X5", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A15", "A15", new[] {a15}, null, null, null});

                SendEvent(env, "X6", 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggSortedMulticriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"aSorted", "bSorted"};
                var epl = "@Name('s0') select " +
                          "sorted(IntPrimitive, DoublePrimitive, filter:TheString like 'A%') as aSorted," +
                          "sorted(IntPrimitive, DoublePrimitive, filter:TheString like 'B%') as bSorted" +
                          " from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = SendEvent(env, "B1", 1, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, new[] {b1}});

                var a1 = SendEvent(env, "A1", 100, 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new[] {a1}, new[] {b1}});

                env.Milestone(0);

                var b2 = SendEvent(env, "B2", 1, 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new[] {a1}, new[] {b2, b1}});

                var a2 = SendEvent(env, "A2", 100, 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new[] {a1, a2}, new[] {b2, b1}});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggSortedUnbound : RegressionExecution
        {
            private readonly bool join;

            public ResultSetAggregateAccessAggSortedUnbound(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"aMaxby", "aMaxbyever", "aMinby", "aMinbyever"};
                var epl = "@Name('s0') select " +
                          "maxby(IntPrimitive, filter:TheString like 'A%').TheString as aMaxby," +
                          "maxbyever(IntPrimitive, filter:TheString like 'A%').TheString as aMaxbyever," +
                          "minby(IntPrimitive, filter:TheString like 'A%').TheString as aMinby," +
                          "minbyever(IntPrimitive, filter:TheString like 'A%').TheString as aMinbyever" +
                          " from " +
                          (join ? "SupportBean_S1#lastevent, SupportBean#keepall" : "SupportBean");
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S1(0));

                SendEventAssert(
                    env,
                    "B1",
                    1,
                    fields,
                    new object[] {null, null, null, null});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "A10",
                    10,
                    fields,
                    new object[] {"A10", "A10", "A10", "A10"});
                SendEventAssert(
                    env,
                    "A5",
                    5,
                    fields,
                    new object[] {"A10", "A10", "A5", "A5"});

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "A15",
                    15,
                    fields,
                    new object[] {"A15", "A15", "A5", "A5"});

                env.Milestone(2);

                SendEventAssert(
                    env,
                    "B1000",
                    1000,
                    fields,
                    new object[] {"A15", "A15", "A5", "A5"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggLinearBound : RegressionExecution
        {
            private readonly bool join;

            public ResultSetAggregateAccessAggLinearBound(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"aFirst", "aLast", "aWindow", "bFirst", "bLast", "bWindow"};
                var epl = "@Name('s0') select " +
                          "first(IntPrimitive, filter:TheString like 'A%') as aFirst," +
                          "last(IntPrimitive, filter:TheString like 'A%') as aLast," +
                          "window(IntPrimitive, filter:TheString like 'A%') as aWindow," +
                          "first(IntPrimitive, filter:TheString like 'B%') as bFirst," +
                          "last(IntPrimitive, filter:TheString like 'B%') as bLast," +
                          "window(IntPrimitive, filter:TheString like 'B%') as bWindow" +
                          " from " +
                          (join ? "SupportBean_S1#lastevent, SupportBean#length(5)" : "SupportBean#length(5)");
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S1(0));

                SendEventAssert(
                    env,
                    "X1",
                    1,
                    fields,
                    new object[] {null, null, null, null, null, null});
                SendEventAssert(
                    env,
                    "B2",
                    2,
                    fields,
                    new object[] {null, null, null, 2, 2, new[] {2}});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "B3",
                    3,
                    fields,
                    new object[] {null, null, null, 2, 3, new[] {2, 3}});
                SendEventAssert(
                    env,
                    "A4",
                    4,
                    fields,
                    new object[] {4, 4, new[] {4}, 2, 3, new[] {2, 3}});
                SendEventAssert(
                    env,
                    "B5",
                    5,
                    fields,
                    new object[] {4, 4, new[] {4}, 2, 5, new[] {2, 3, 5}});

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "A6",
                    6,
                    fields,
                    new object[] {4, 6, new[] {4, 6}, 2, 5, new[] {2, 3, 5}});
                SendEventAssert(
                    env,
                    "X2",
                    7,
                    fields,
                    new object[] {4, 6, new[] {4, 6}, 3, 5, new[] {3, 5}});
                SendEventAssert(
                    env,
                    "X3",
                    8,
                    fields,
                    new object[] {4, 6, new[] {4, 6}, 5, 5, new[] {5}});

                env.Milestone(2);

                SendEventAssert(
                    env,
                    "X4",
                    9,
                    fields,
                    new object[] {6, 6, new[] {6}, 5, 5, new[] {5}});
                SendEventAssert(
                    env,
                    "X5",
                    10,
                    fields,
                    new object[] {6, 6, new[] {6}, null, null, null});
                SendEventAssert(
                    env,
                    "X6",
                    11,
                    fields,
                    new object[] {null, null, null, null, null, null});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggLinearUnbound : RegressionExecution
        {
            private readonly bool join;

            public ResultSetAggregateAccessAggLinearUnbound(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"aFirst", "aFirstever", "aLast", "aLastever", "aCountever"};
                var epl = "@Name('s0') select " +
                          "first(IntPrimitive, filter:TheString like 'A%') as aFirst," +
                          "firstever(IntPrimitive, filter:TheString like 'A%') as aFirstever," +
                          "last(IntPrimitive, filter:TheString like 'A%') as aLast," +
                          "lastever(IntPrimitive, filter:TheString like 'A%') as aLastever," +
                          "countever(IntPrimitive, filter:TheString like 'A%') as aCountever" +
                          " from " +
                          (join ? "SupportBean_S1#lastevent, SupportBean#keepall" : "SupportBean");
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(0));

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "X0",
                    0,
                    fields,
                    new object[] {null, null, null, null, 0L});
                SendEventAssert(
                    env,
                    "A1",
                    1,
                    fields,
                    new object[] {1, 1, 1, 1, 1L});

                env.Milestone(2);

                SendEventAssert(
                    env,
                    "X2",
                    2,
                    fields,
                    new object[] {1, 1, 1, 1, 1L});
                SendEventAssert(
                    env,
                    "A3",
                    3,
                    fields,
                    new object[] {1, 1, 3, 3, 2L});

                env.Milestone(3);

                SendEventAssert(
                    env,
                    "X4",
                    4,
                    fields,
                    new object[] {1, 1, 3, 3, 2L});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateAccessAggLinearBoundMixedFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "@Name('s0') select " +
                          "window(sb, filter:TheString like 'A%') as c0," +
                          "window(sb) as c1," +
                          "window(filter:TheString like 'B%', sb) as c2" +
                          " from SupportBean#keepall as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var x1 = SendEvent(env, "X1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, new[] {x1}, null});

                var a2 = SendEvent(env, "A2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new[] {a2}, new[] {x1, a2}, null});

                env.Milestone(0);

                var b3 = SendEvent(env, "B3", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new[] {a2}, new[] {x1, a2, b3}, new[] {b3}});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAggSQLMixedFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "@Name('s0') select " +
                          "sum(IntPrimitive, filter:TheString like 'A%') as c0," +
                          "sum(IntPrimitive) as c1," +
                          "sum(filter:TheString like 'B%', IntPrimitive) as c2" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssert(
                    env,
                    "X1",
                    1,
                    fields,
                    new object[] {null, 1, null});
                SendEventAssert(
                    env,
                    "B2",
                    20,
                    fields,
                    new object[] {null, 1 + 20, 20});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "A3",
                    300,
                    fields,
                    new object[] {300, 1 + 20 + 300, 20});
                SendEventAssert(
                    env,
                    "X1",
                    2,
                    fields,
                    new object[] {300, 1 + 20 + 300 + 2, 20});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAggSQLAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "avedev(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cAvedev," +
                          "avg(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cAvg, " +
                          "count(*, filter:IntPrimitive between 1 and 3) as cCount, " +
                          "max(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMax, " +
                          "fmax(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFmax, " +
                          "maxever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMaxever, " +
                          "fmaxever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFmaxever, " +
                          "median(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMedian, " +
                          "min(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMin, " +
                          "fmin(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFmin, " +
                          "minever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cMinever, " +
                          "fminever(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cFminever, " +
                          "stddev(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cStddev, " +
                          "sum(DoublePrimitive, filter:IntPrimitive between 1 and 3) as cSum " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssertSQLFuncs(
                    env,
                    "E1",
                    0,
                    50,
                    null,
                    null,
                    0L,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);
                env.Milestone(0);

                SendEventAssertSQLFuncs(
                    env,
                    "E2",
                    2,
                    10,
                    0.0,
                    10d,
                    1L,
                    10d,
                    10d,
                    10d,
                    10d,
                    10.0,
                    10d,
                    10d,
                    10d,
                    10d,
                    null,
                    10d);

                env.Milestone(1);

                SendEventAssertSQLFuncs(
                    env,
                    "E3",
                    100,
                    10,
                    0.0,
                    10d,
                    1L,
                    10d,
                    10d,
                    10d,
                    10d,
                    10.0,
                    10d,
                    10d,
                    10d,
                    10d,
                    null,
                    10d);

                env.Milestone(2);

                SendEventAssertSQLFuncs(
                    env,
                    "E4",
                    1,
                    20,
                    5.0,
                    15d,
                    2L,
                    20d,
                    20d,
                    20d,
                    20d,
                    15.0,
                    10d,
                    10d,
                    10d,
                    10d,
                    7.0710678118654755,
                    30d);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateFirstAggSODA : RegressionExecution
        {
            private readonly bool soda;

            public ResultSetAggregateFirstAggSODA(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1"};
                var epl = "@Name('s0') select " +
                          "first(*,filter:IntPrimitive=1).TheString as c0, " +
                          "first(*,filter:IntPrimitive=2).TheString as c1" +
                          " from SupportBean#length(3)";
                env.CompileDeploy(soda, epl).AddListener("s0");

                SendEventAssert(
                    env,
                    "E1",
                    3,
                    fields,
                    new object[] {null, null});
                SendEventAssert(
                    env,
                    "E2",
                    2,
                    fields,
                    new object[] {null, "E2"});
                SendEventAssert(
                    env,
                    "E3",
                    1,
                    fields,
                    new object[] {"E3", "E2"});

                env.Milestone(0);

                SendEventAssert(
                    env,
                    "E4",
                    2,
                    fields,
                    new object[] {"E3", "E2"});
                SendEventAssert(
                    env,
                    "E5",
                    -1,
                    fields,
                    new object[] {"E3", "E4"});
                SendEventAssert(
                    env,
                    "E6",
                    -1,
                    fields,
                    new object[] {null, "E4"});

                env.Milestone(1);

                SendEventAssert(
                    env,
                    "E7",
                    -1,
                    fields,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }
    }
} // end of namespace