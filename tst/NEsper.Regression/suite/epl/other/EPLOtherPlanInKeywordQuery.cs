///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.queryplanouter;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherPlanInKeywordQuery : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithNotIn(execs);
            WithMultiIdxMultipleInAndMultirow(execs);
            WithMultiIdxSubquery(execs);
            WithSingleIdxMultipleInAndMultirow(execs);
            WithSingleIdxSubquery(execs);
            WithSingleIdxConstants(execs);
            WithMultiIdxConstants(execs);
            WithQueryPlan3Stream(execs);
            With(QueryPlan2Stream)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithQueryPlan2Stream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherQueryPlan2Stream());
            return execs;
        }

        public static IList<RegressionExecution> WithQueryPlan3Stream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherQueryPlan3Stream());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiIdxConstants(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherMultiIdxConstants());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleIdxConstants(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSingleIdxConstants());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleIdxSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSingleIdxSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleIdxMultipleInAndMultirow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSingleIdxMultipleInAndMultirow());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiIdxSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherMultiIdxSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiIdxMultipleInAndMultirow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherMultiIdxMultipleInAndMultirow());
            return execs;
        }

        public static IList<RegressionExecution> WithNotIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherNotIn());
            return execs;
        }

        private class EPLOtherNotIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportQueryPlanIndexHook.Reset();
                var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select * from SupportBean_S0 as s0 unidirectional, SupportBean_S1#keepall as s1 " +
                          "where P00 not in (P10, P11)";
                env.CompileDeploy(epl);

                var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
                Assert.AreEqual("[]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLOtherMultiIdxMultipleInAndMultirow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // assert join
                SupportQueryPlanIndexHook.Reset();
                var epl = "@name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select * from SupportBean_S0 as s0 unidirectional, SupportBean_S1#keepall as s1 " +
                          "where P00 in (P10, P11) and P01 in (P12, P13)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertThat(
                    () => {
                        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
                        Assert.AreEqual("[\"P10\"][\"P11\"]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));
                    });

                TryAssertionMultiIdx(env);
                env.UndeployAll();

                // assert named window
                var path = new RegressionPath();
                env.CompileDeploy("@public create window S1Window#keepall as SupportBean_S1", path);
                env.CompileDeploy("insert into S1Window select * from SupportBean_S1", path);

                var eplNamedWindow = "@name('s0') " +
                                     IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                                     "on SupportBean_S0 as s0 select * from S1Window as s1 " +
                                     "where P00 in (P10, P11) and P01 in (P12, P13)";
                env.CompileDeploy(eplNamedWindow, path).AddListener("s0");

                env.AssertThat(
                    () => {
                        var onExprNamedWindow = SupportQueryPlanIndexHook.AssertOnExprAndReset();
                        Assert.AreEqual(
                            nameof(SubordInKeywordMultiTableLookupStrategyFactoryForge),
                            onExprNamedWindow.TableLookupStrategy);
                    });

                TryAssertionMultiIdx(env);

                // assert table
                path.Clear();
                env.CompileDeploy(
                    "@public create table S1Table(Id int primary key, P10 string primary key, P11 string primary key, P12 string primary key, P13 string primary key)",
                    path);
                env.CompileDeploy("insert into S1Table select * from SupportBean_S1", path);
                env.CompileDeploy("create index S1Idx1 on S1Table(P10)", path);
                env.CompileDeploy("create index S1Idx2 on S1Table(P11)", path);
                env.CompileDeploy("create index S1Idx3 on S1Table(P12)", path);
                env.CompileDeploy("create index S1Idx4 on S1Table(P13)", path);

                var eplTable = "@name('s0') " +
                               IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                               "on SupportBean_S0 as s0 select * from S1Table as s1 " +
                               "where P00 in (P10, P11) and P01 in (P12, P13)";
                env.CompileDeploy(eplTable, path).AddListener("s0");

                env.AssertThat(
                    () => {
                        var onExprTable = SupportQueryPlanIndexHook.AssertOnExprAndReset();
                        Assert.AreEqual(
                            nameof(SubordInKeywordMultiTableLookupStrategyFactoryForge),
                            onExprTable.TableLookupStrategy);
                    });

                TryAssertionMultiIdx(env);

                env.UndeployAll();
            }
        }

        private class EPLOtherMultiIdxSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select s0.Id as c0," +
                          "(select * from SupportBean_S1#keepall as s1 " +
                          "  where s0.P00 in (s1.P10, SupportBean_S1.P11) and s0.P01 in (s1.P12, SupportBean_S1.P13))" +
                          ".selectFrom(a=>SupportBean_S1.Id) as c1 " +
                          "from SupportBean_S0 as s0";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertThat(
                    () => {
                        var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
                        Assert.AreEqual(
                            nameof(SubordInKeywordMultiTableLookupStrategyFactoryForge),
                            subquery.TableLookupStrategy);
                    });

                // single row tests
                env.SendEventBean(new SupportBean_S1(101, "a", "b", "c", "d"));

                env.SendEventBean(new SupportBean_S0(1, "a", "x"));
                AssertSubqueryC0C1(env, 1, null);

                env.SendEventBean(new SupportBean_S0(2, "x", "c"));
                AssertSubqueryC0C1(env, 2, null);

                env.SendEventBean(new SupportBean_S0(3, "a", "c"));
                AssertSubqueryC0C1(env, 3, new int?[] { 101 });

                env.SendEventBean(new SupportBean_S0(4, "b", "d"));
                AssertSubqueryC0C1(env, 4, new int?[] { 101 });

                env.SendEventBean(new SupportBean_S0(5, "a", "d"));
                AssertSubqueryC0C1(env, 5, new int?[] { 101 });

                // 2-row tests
                env.SendEventBean(new SupportBean_S1(102, "a1", "a", "d1", "d"));

                env.SendEventBean(new SupportBean_S0(10, "a", "x"));
                AssertSubqueryC0C1(env, 10, null);

                env.SendEventBean(new SupportBean_S0(11, "x", "c"));
                AssertSubqueryC0C1(env, 11, null);

                env.SendEventBean(new SupportBean_S0(12, "a", "c"));
                AssertSubqueryC0C1(env, 12, new int?[] { 101 });

                env.SendEventBean(new SupportBean_S0(13, "a", "d"));
                AssertSubqueryC0C1(env, 13, new int?[] { 101, 102 });

                env.SendEventBean(new SupportBean_S0(14, "a1", "d"));
                AssertSubqueryC0C1(env, 14, new int?[] { 102 });

                env.SendEventBean(new SupportBean_S0(15, "a", "d1"));
                AssertSubqueryC0C1(env, 15, new int?[] { 102 });

                // 3-row tests
                env.SendEventBean(new SupportBean_S1(103, "a", "a2", "d", "d2"));

                env.SendEventBean(new SupportBean_S0(20, "a", "c"));
                AssertSubqueryC0C1(env, 20, new int?[] { 101 });

                env.SendEventBean(new SupportBean_S0(21, "a", "d"));
                AssertSubqueryC0C1(env, 21, new int?[] { 101, 102, 103 });

                env.SendEventBean(new SupportBean_S0(22, "a2", "d"));
                AssertSubqueryC0C1(env, 22, new int?[] { 103 });

                env.SendEventBean(new SupportBean_S0(23, "a", "d2"));
                AssertSubqueryC0C1(env, 23, new int?[] { 103 });

                env.UndeployAll();

                // test coercion absence - types the same
                var eplCoercion = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                                  "select *," +
                                  "(select * from SupportBean_S0#keepall as s0 where sb.LongPrimitive in (Id)) from SupportBean as sb";
                env.CompileDeploy(eplCoercion);
                env.AssertThat(
                    () => {
                        var subqueryCoercion = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
                        Assert.AreEqual(
                            nameof(SubordFullTableScanLookupStrategyFactoryForge),
                            subqueryCoercion.TableLookupStrategy);
                    });
                env.UndeployAll();
            }
        }

        private class EPLOtherSingleIdxMultipleInAndMultirow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // assert join
                SupportQueryPlanIndexHook.Reset();
                var epl = "@name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select * from SupportBean_S0#keepall as s0, SupportBean_S1 as s1 unidirectional " +
                          "where P00 in (P10, P11) and P01 in (P12, P13)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertThat(
                    () => {
                        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[0].Items;
                        Assert.AreEqual("[\"P00\"]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));
                    });

                TryAssertionSingleIdx(env);
                env.UndeployAll();

                // assert named window
                var path = new RegressionPath();
                env.CompileDeploy("@public create window S0Window#keepall as SupportBean_S0", path);
                env.CompileDeploy("insert into S0Window select * from SupportBean_S0", path);

                var eplNamedWindow = "@name('s0') " +
                                     IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                                     "on SupportBean_S1 as s1 select * from S0Window as s0 " +
                                     "where P00 in (P10, P11) and P01 in (P12, P13)";
                env.CompileDeploy(eplNamedWindow, path).AddListener("s0");

                env.AssertThat(
                    () => {
                        var onExprNamedWindow = SupportQueryPlanIndexHook.AssertOnExprAndReset();
                        Assert.AreEqual(
                            nameof(SubordInKeywordSingleTableLookupStrategyFactoryForge),
                            onExprNamedWindow.TableLookupStrategy);
                    });

                TryAssertionSingleIdx(env);

                // assert table
                path.Clear();
                env.CompileDeploy(
                    "@public create table S0Table(Id int primary key, P00 string primary key, P01 string primary key, P02 string primary key, P03 string primary key)",
                    path);
                env.CompileDeploy("insert into S0Table select * from SupportBean_S0", path);
                env.CompileDeploy("create index S0Idx1 on S0Table(P00)", path);
                env.CompileDeploy("create index S0Idx2 on S0Table(P01)", path);

                var eplTable = "@name('s0') " +
                               IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                               "on SupportBean_S1 as s1 select * from S0Table as s0 " +
                               "where P00 in (P10, P11) and P01 in (P12, P13)";
                env.CompileDeploy(eplTable, path).AddListener("s0");

                env.AssertThat(
                    () => {
                        var onExprTable = SupportQueryPlanIndexHook.AssertOnExprAndReset();
                        Assert.AreEqual(
                            nameof(SubordInKeywordSingleTableLookupStrategyFactoryForge),
                            onExprTable.TableLookupStrategy);
                    });

                TryAssertionSingleIdx(env);

                env.UndeployAll();
            }
        }

        private class EPLOtherSingleIdxSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportQueryPlanIndexHook.Reset();
                var epl = "@name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select s1.Id as c0," +
                          "(select * from SupportBean_S0#keepall as s0 " +
                          "  where s0.P00 in (s1.P10, SupportBean_S1.P11) and s0.P01 in (s1.P12, SupportBean_S1.P13))" +
                          ".selectFrom(a=>SupportBean_S0.Id) as c1 " +
                          " from SupportBean_S1 as s1";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertThat(
                    () => {
                        var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
                        Assert.AreEqual(
                            nameof(SubordInKeywordSingleTableLookupStrategyFactoryForge),
                            subquery.TableLookupStrategy);
                    });

                // single row tests
                env.SendEventBean(new SupportBean_S0(100, "a", "c"));

                env.SendEventBean(new SupportBean_S1(1, "a1", "b", "c", "d"));
                AssertSubqueryC0C1(env, 1, null);

                env.SendEventBean(new SupportBean_S1(2, "a", "b", "x", "d"));
                AssertSubqueryC0C1(env, 2, null);

                env.SendEventBean(new SupportBean_S1(3, "a", "b", "c", "d"));
                AssertSubqueryC0C1(env, 3, new int?[] { 100 });

                env.SendEventBean(new SupportBean_S1(4, "x", "a", "x", "c"));
                AssertSubqueryC0C1(env, 4, new int?[] { 100 });

                // 2-rows available tests
                env.SendEventBean(new SupportBean_S0(101, "a", "d"));

                env.SendEventBean(new SupportBean_S1(10, "a1", "b", "c", "d"));
                AssertSubqueryC0C1(env, 10, null);

                env.SendEventBean(new SupportBean_S1(11, "a", "b", "x", "c1"));
                AssertSubqueryC0C1(env, 11, null);

                env.SendEventBean(new SupportBean_S1(12, "a", "b", "c", "d"));
                AssertSubqueryC0C1(env, 12, new int?[] { 100, 101 });

                env.SendEventBean(new SupportBean_S1(13, "x", "a", "x", "c"));
                AssertSubqueryC0C1(env, 13, new int?[] { 100 });

                env.SendEventBean(new SupportBean_S1(14, "x", "a", "d", "x"));
                AssertSubqueryC0C1(env, 14, new int?[] { 101 });

                // 3-rows available tests
                env.SendEventBean(new SupportBean_S0(102, "b", "c"));

                env.SendEventBean(new SupportBean_S1(20, "a1", "b", "c1", "d"));
                AssertSubqueryC0C1(env, 20, null);

                env.SendEventBean(new SupportBean_S1(21, "a", "b", "x", "c1"));
                AssertSubqueryC0C1(env, 21, null);

                env.SendEventBean(new SupportBean_S1(22, "a", "b", "c", "d"));
                AssertSubqueryC0C1(env, 22, new int?[] { 100, 101, 102 });

                env.SendEventBean(new SupportBean_S1(23, "b", "a", "x", "c"));
                AssertSubqueryC0C1(env, 23, new int?[] { 100, 102 });

                env.SendEventBean(new SupportBean_S1(24, "b", "a", "d", "c"));
                AssertSubqueryC0C1(env, 24, new int?[] { 100, 101, 102 });

                env.SendEventBean(new SupportBean_S1(25, "b", "x", "x", "c"));
                AssertSubqueryC0C1(env, 25, new int?[] { 102 });

                env.UndeployAll();

                // test coercion absence - types the same
                var eplCoercion = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                                  "select *," +
                                  "(select * from SupportBean#keepall as sb where sb.LongPrimitive in (s0.Id)) from SupportBean_S0 as s0";
                env.CompileDeploy(eplCoercion);
                env.AssertThat(
                    () => {
                        var subqueryCoercion = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
                        Assert.AreEqual(
                            nameof(SubordFullTableScanLookupStrategyFactoryForge),
                            subqueryCoercion.TableLookupStrategy);
                    });
                env.UndeployAll();
            }
        }

        private class EPLOtherSingleIdxConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportQueryPlanIndexHook.Reset();
                var epl = "@name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select * from SupportBean_S0 as s0 unidirectional, SupportBean_S1#keepall as s1 " +
                          "where P10 in ('a', 'b')";
                var fields = "s0.Id,s1.Id".SplitCsv();
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertThat(
                    () => {
                        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
                        Assert.AreEqual("[\"P10\"]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));
                    });

                env.SendEventBean(new SupportBean_S1(100, "x"));
                env.SendEventBean(new SupportBean_S1(101, "a"));

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsPerRowLastNewAnyOrder("s0", fields, new object[][] { new object[] { 1, 101 } });

                env.SendEventBean(new SupportBean_S1(102, "b"));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 2, 101 }, new object[] { 2, 102 } });

                env.UndeployAll();
            }
        }

        private class EPLOtherMultiIdxConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportQueryPlanIndexHook.Reset();
                var epl = "@name('s0') " +
                          IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          "select * from SupportBean_S0 as s0 unidirectional, SupportBean_S1#keepall as s1 " +
                          "where 'a' in (P10, P11)";
                var fields = "s0.Id,s1.Id".SplitCsv();
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertThat(
                    () => {
                        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
                        Assert.AreEqual("[\"P10\"][\"P11\"]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));
                    });

                env.SendEventBean(new SupportBean_S1(100, "x", "y"));
                env.SendEventBean(new SupportBean_S1(101, "x", "a"));

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsPerRowLastNewAnyOrder("s0", fields, new object[][] { new object[] { 1, 101 } });

                env.SendEventBean(new SupportBean_S1(102, "b", "a"));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 2, 101 }, new object[] { 2, 102 } });

                env.UndeployAll();
            }
        }

        private class EPLOtherQueryPlan3Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var types = new EventType[3];
                var epl =
                    "@name('s0') select * from SupportBean_S0 as s0 unidirectional, SupportBean_S1#keepall, SupportBean_S2#keepall ";

                // 3-stream join with in-multiindex directional
                var planInMidx = new InKeywordTableLookupPlanMultiIdxForge(
                    0,
                    1,
                    false,
                    types,
                    GetIndexKeys("i1a", "i1b"),
                    SupportExprNodeFactory.MakeIdentExprNode("P00"));
                TryAssertion(
                    env,
                    epl + " where P00 in (P10, P11)",
                    SupportQueryPlanBuilder.Start(3)
                        .AddIndexHashSingleNonUnique(1, "i1a", "P10")
                        .AddIndexHashSingleNonUnique(1, "i1b", "P11")
                        .SetIndexFullTableScan(2, "i2")
                        .SetLookupPlanInstruction(
                            0,
                            "s0",
                            new LookupInstructionPlanForge[] {
                                new LookupInstructionPlanForge(
                                    0,
                                    "s0",
                                    new int[] { 1 },
                                    new TableLookupPlanForge[] { planInMidx },
                                    null,
                                    new bool[3]),
                                new LookupInstructionPlanForge(
                                    0,
                                    "s0",
                                    new int[] { 2 },
                                    new TableLookupPlanForge[]
                                        { new FullTableScanLookupPlanForge(1, 2, false, types, GetIndexKey("i2")) },
                                    null,
                                    new bool[3])
                            })
                        .Get());

                var planInMidxMulitiSrc = new InKeywordTableLookupPlanMultiIdxForge(
                    0,
                    1,
                    false,
                    types,
                    GetIndexKeys("i1", "i2"),
                    SupportExprNodeFactory.MakeIdentExprNode("P00"));
                TryAssertion(
                    env,
                    epl + " where P00 in (P10, P20)",
                    SupportQueryPlanBuilder.Start(3)
                        .SetIndexFullTableScan(1, "i1")
                        .SetIndexFullTableScan(2, "i2")
                        .SetLookupPlanInstruction(
                            0,
                            "s0",
                            new LookupInstructionPlanForge[] {
                                new LookupInstructionPlanForge(
                                    0,
                                    "s0",
                                    new int[] { 1 },
                                    new TableLookupPlanForge[]
                                        { new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("i1")) },
                                    null,
                                    new bool[3]),
                                new LookupInstructionPlanForge(
                                    0,
                                    "s0",
                                    new int[] { 2 },
                                    new TableLookupPlanForge[]
                                        { new FullTableScanLookupPlanForge(1, 2, false, types, GetIndexKey("i2")) },
                                    null,
                                    new bool[3])
                            })
                        .Get());

                // 3-stream join with in-singleindex directional
                var planInSidx = new InKeywordTableLookupPlanSingleIdxForge(
                    0,
                    1,
                    false,
                    types,
                    GetIndexKey("i1"),
                    SupportExprNodeFactory.MakeIdentExprNodes("P00", "P01"));
                TryAssertion(env, epl + " where P10 in (P00, P01)", GetSingleIndexPlan(types, planInSidx));

                // 3-stream join with in-singleindex multi-sourced
                var planInSingleMultiSrc = new InKeywordTableLookupPlanSingleIdxForge(
                    0,
                    1,
                    false,
                    types,
                    GetIndexKey("i1"),
                    SupportExprNodeFactory.MakeIdentExprNodes("P00"));
                TryAssertion(env, epl + " where P10 in (P00, P20)", GetSingleIndexPlan(types, planInSingleMultiSrc));
            }
        }

        private class EPLOtherQueryPlan2Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var types = new EventType[2];
                var epl = "select * from SupportBean_S0 as s0 unidirectional, SupportBean_S1#keepall ";
                var fullTableScan = SupportQueryPlanBuilder.Start(2)
                    .SetIndexFullTableScan(1, "a")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("a")))
                    .Get();

                // 2-stream unidirectional joins
                TryAssertion(env, epl, fullTableScan);

                var planEquals = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P10")
                    .SetLookupPlanInner(
                        0,
                        new IndexedTableLookupPlanHashedOnlyForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKey("a"),
                            new QueryGraphValueEntryHashKeyedForge[] { SupportExprNodeFactory.MakeKeyed("P00") },
                            null,
                            null,
                            null))
                    .Get();
                TryAssertion(env, epl + "where P00 = P10", planEquals);
                TryAssertion(env, epl + "where P00 = P10 and P00 in (P11, P12, P13)", planEquals);

                var planInMultiInner = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P11")
                    .AddIndexHashSingleNonUnique(1, "b", "P12")
                    .SetLookupPlanInner(
                        0,
                        new InKeywordTableLookupPlanMultiIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKeys("a", "b"),
                            SupportExprNodeFactory.MakeIdentExprNode("P00")))
                    .Get();
                TryAssertion(env, epl + "where P00 in (P11, P12)", planInMultiInner);
                TryAssertion(env, epl + "where P00 = P11 or P00 = P12", planInMultiInner);

                var planInMultiOuter = SupportQueryPlanBuilder.Start(planInMultiInner)
                    .SetLookupPlanOuter(
                        0,
                        new InKeywordTableLookupPlanMultiIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKeys("a", "b"),
                            SupportExprNodeFactory.MakeIdentExprNode("P00")))
                    .Get();
                var eplOuterJoin =
                    "select * from SupportBean_S0 as s0 unidirectional full outer join SupportBean_S1#keepall ";
                TryAssertion(env, eplOuterJoin + "where P00 in (P11, P12)", planInMultiOuter);

                var planInMultiWConst = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P11")
                    .AddIndexHashSingleNonUnique(1, "b", "P12")
                    .SetLookupPlanInner(
                        0,
                        new InKeywordTableLookupPlanMultiIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKeys("a", "b"),
                            SupportExprNodeFactory.MakeConstExprNode("A")))
                    .Get();
                TryAssertion(env, epl + "where 'A' in (P11, P12)", planInMultiWConst);
                TryAssertion(env, epl + "where 'A' = P11 or 'A' = P12", planInMultiWConst);

                var planInMultiWAddConst = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P12")
                    .SetLookupPlanInner(
                        0,
                        new InKeywordTableLookupPlanMultiIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKeys("a"),
                            SupportExprNodeFactory.MakeConstExprNode("A")))
                    .Get();
                TryAssertion(env, epl + "where 'A' in ('B', P12)", planInMultiWAddConst);
                TryAssertion(env, epl + "where 'A' in ('B', 'C')", fullTableScan);

                var planInSingle = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P10")
                    .SetLookupPlanInner(
                        0,
                        new InKeywordTableLookupPlanSingleIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKey("a"),
                            SupportExprNodeFactory.MakeIdentExprNodes("P00", "P01")))
                    .Get();
                TryAssertion(env, epl + "where P10 in (P00, P01)", planInSingle);

                var planInSingleWConst = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P10")
                    .SetLookupPlanInner(
                        0,
                        new InKeywordTableLookupPlanSingleIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKey("a"),
                            SupportExprNodeFactory.MakeConstAndIdentNode("A", "P01")))
                    .Get();
                TryAssertion(env, epl + "where P10 in ('A', P01)", planInSingleWConst);

                var planInSingleJustConst = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(1, "a", "P10")
                    .SetLookupPlanInner(
                        0,
                        new InKeywordTableLookupPlanSingleIdxForge(
                            0,
                            1,
                            false,
                            types,
                            GetIndexKey("a"),
                            SupportExprNodeFactory.MakeConstAndConstNode("A", "B")))
                    .Get();
                TryAssertion(env, epl + "where P10 in ('A', 'B')", planInSingleJustConst);
            }
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string epl,
            QueryPlanForge expectedPlan)
        {
            SupportQueryPlanIndexHook.Reset();
            epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + epl;
            env.CompileDeploy(epl);

            env.AssertThat(
                () => {
                    var actualPlan = SupportQueryPlanIndexHook.AssertJoinAndReset();
                    SupportQueryPlanIndexHelper.CompareQueryPlans(expectedPlan, actualPlan);
                });

            env.UndeployAll();
        }

        private static void AssertSubqueryC0C1(
            RegressionEnvironment env,
            int c0,
            int?[] c1)
        {
            env.AssertEventNew(
                "s0",
                @event => {
                    Assert.AreEqual(c0, @event.Get("c0"));
                    var c1Coll = @event.Get("c1");
                    EPAssertionUtil.AssertEqualsAnyOrder(c1, c1Coll?.Unwrap<int?>());
                });
        }

        private static QueryPlanForge GetSingleIndexPlan(
            EventType[] types,
            InKeywordTableLookupPlanSingleIdxForge plan)
        {
            return SupportQueryPlanBuilder.Start(3)
                .AddIndexHashSingleNonUnique(1, "i1", "P10")
                .SetIndexFullTableScan(2, "i2")
                .SetLookupPlanInstruction(
                    0,
                    "s0",
                    new LookupInstructionPlanForge[] {
                        new LookupInstructionPlanForge(
                            0,
                            "s0",
                            new int[] { 1 },
                            new TableLookupPlanForge[] { plan },
                            null,
                            new bool[3]),
                        new LookupInstructionPlanForge(
                            0,
                            "s0",
                            new int[] { 2 },
                            new TableLookupPlanForge[]
                                { new FullTableScanLookupPlanForge(1, 2, false, types, GetIndexKey("i2")) },
                            null,
                            new bool[3])
                    })
                .Get();
        }

        private static TableLookupIndexReqKey[] GetIndexKeys(params string[] names)
        {
            var keys = new TableLookupIndexReqKey[names.Length];
            for (var i = 0; i < names.Length; i++) {
                keys[i] = new TableLookupIndexReqKey(names[i], null);
            }

            return keys;
        }

        private static void TryAssertionMultiIdx(RegressionEnvironment env)
        {
            var fields = "s0.Id,s1.Id".SplitCsv();

            // single row tests
            env.SendEventBean(new SupportBean_S1(101, "a", "b", "c", "d"));

            env.SendEventBean(new SupportBean_S0(0, "a", "x"));
            env.SendEventBean(new SupportBean_S0(0, "x", "c"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(1, "a", "c"));
            env.AssertPropsNew("s0", fields, new object[] { 1, 101 });

            env.SendEventBean(new SupportBean_S0(2, "b", "d"));
            env.AssertPropsNew("s0", fields, new object[] { 2, 101 });

            env.SendEventBean(new SupportBean_S0(3, "a", "d"));
            env.AssertPropsNew("s0", fields, new object[] { 3, 101 });

            // 2-row tests
            env.SendEventBean(new SupportBean_S1(102, "a1", "a", "d1", "d"));

            env.SendEventBean(new SupportBean_S0(0, "a", "x"));
            env.SendEventBean(new SupportBean_S0(0, "x", "c"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(10, "a", "c"));
            env.AssertPropsNew("s0", fields, new object[] { 10, 101 });

            env.SendEventBean(new SupportBean_S0(11, "a", "d"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { 11, 101 }, new object[] { 11, 102 } });

            env.SendEventBean(new SupportBean_S0(12, "a1", "d"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 12, 102 } });

            env.SendEventBean(new SupportBean_S0(13, "a", "d1"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 13, 102 } });

            // 3-row tests
            env.SendEventBean(new SupportBean_S1(103, "a", "a2", "d", "d2"));

            env.SendEventBean(new SupportBean_S0(20, "a", "c"));
            env.AssertPropsNew("s0", fields, new object[] { 20, 101 });

            env.SendEventBean(new SupportBean_S0(21, "a", "d"));
            env.AssertPropsPerRowLastNewAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { 21, 101 }, new object[] { 21, 102 }, new object[] { 21, 103 } });

            env.SendEventBean(new SupportBean_S0(22, "a2", "d"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 22, 103 } });

            env.SendEventBean(new SupportBean_S0(23, "a", "d2"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 23, 103 } });

            env.UndeployAll();
        }

        private static void TryAssertionSingleIdx(RegressionEnvironment env)
        {
            var fields = "s0.Id,s1.Id".SplitCsv();

            // single row tests
            env.SendEventBean(new SupportBean_S0(100, "a", "c"));

            env.SendEventBean(new SupportBean_S1(0, "a1", "b", "c", "d"));
            env.SendEventBean(new SupportBean_S1(0, "a", "b", "x", "d"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(1, "a", "b", "c", "d"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 100, 1 } });

            env.SendEventBean(new SupportBean_S1(2, "x", "a", "x", "c"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 100, 2 } });

            // 2-rows available tests
            env.SendEventBean(new SupportBean_S0(101, "a", "d"));

            env.SendEventBean(new SupportBean_S1(0, "a1", "b", "c", "d"));
            env.SendEventBean(new SupportBean_S1(0, "a", "b", "x", "c1"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(10, "a", "b", "c", "d"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { 100, 10 }, new object[] { 101, 10 } });

            env.SendEventBean(new SupportBean_S1(11, "x", "a", "x", "c"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 100, 11 } });

            env.SendEventBean(new SupportBean_S1(12, "x", "a", "d", "x"));
            env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 101, 12 } });

            // 3-rows available tests
            env.SendEventBean(new SupportBean_S0(102, "b", "c"));

            env.SendEventBean(new SupportBean_S1(0, "a1", "b", "c1", "d"));
            env.SendEventBean(new SupportBean_S1(0, "a", "b", "x", "c1"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(20, "a", "b", "c", "d"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { 100, 20 }, new object[] { 101, 20 }, new object[] { 102, 20 } });

            env.SendEventBean(new SupportBean_S1(21, "b", "a", "x", "c"));
            env.AssertPropsPerRowLastNewAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { 100, 21 }, new object[] { 102, 21 } });

            env.SendEventBean(new SupportBean_S1(22, "b", "a", "d", "c"));
            env.AssertPropsPerRowLastNewAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { 100, 22 }, new object[] { 101, 22 }, new object[] { 102, 22 } });

            env.SendEventBean(new SupportBean_S1(23, "b", "x", "x", "c"));
            env.AssertPropsPerRowLastNewAnyOrder("s0", fields, new object[][] { new object[] { 102, 23 } });

            env.UndeployAll();
        }

        private static TableLookupIndexReqKey GetIndexKey(string name)
        {
            return new TableLookupIndexReqKey(name, null);
        }
    }
} // end of namespace