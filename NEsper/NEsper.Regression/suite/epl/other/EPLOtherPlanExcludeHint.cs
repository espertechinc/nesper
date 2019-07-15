///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupsubord;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherPlanExcludeHint : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherDocSample());
            execs.Add(new EPLOtherJoin());
            execs.Add(new EPLOtherInvalid());
            return execs;
        }

        private static void TryAssertionJoin(
            RegressionEnvironment env,
            string epl,
            QueryPlanForge expectedPlan)
        {
            SupportQueryPlanIndexHook.Reset();
            epl = INDEX_CALLBACK_HOOK + epl;
            env.CompileDeploy(epl);

            var actualPlan = SupportQueryPlanIndexHook.AssertJoinAndReset();
            SupportQueryPlanIndexHelper.CompareQueryPlans(expectedPlan, actualPlan);

            env.UndeployAll();
        }

        private static TableLookupIndexReqKey GetIndexKey(string name)
        {
            return new TableLookupIndexReqKey(name, null);
        }

        internal class EPLOtherDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var schema =
                    "create schema AEvent as " +
                    typeof(AEvent).Name +
                    ";\n" +
                    "create schema BEvent as " +
                    typeof(BEvent).Name +
                    ";\n";
                var path = new RegressionPath();
                env.CompileDeploy(schema, path);

                string[] hints = {
                    "@hint('exclude_plan(true)')",
                    "@hint('exclude_plan(opname=\"equals\")')",
                    "@hint('exclude_plan(opname=\"equals\" and from_streamname=\"a\")')",
                    "@hint('exclude_plan(opname=\"equals\" and from_streamname=\"b\")')",
                    "@hint('exclude_plan(exprs[0]=\"aprop\")')"
                };
                foreach (var hint in hints) {
                    env.CompileDeploy(
                        "@Audit " +
                        hint +
                        "select * from AEvent#keepall as a, BEvent#keepall as b where aprop = bprop",
                        path);
                }

                // test subquery
                SupportQueryPlanIndexHook.Reset();
                env.CompileDeploy(
                    INDEX_CALLBACK_HOOK +
                    "@hint('exclude_plan(true)') select (select * from SupportBean_S0#unique(p00) as s0 where s1.p10 = p00) from SupportBean_S1 as s1",
                    path);
                var subq = SupportQueryPlanIndexHook.GetAndResetSubqueries()[0];
                Assert.AreEqual(typeof(SubordFullTableScanLookupStrategyFactoryForge).Name, subq.TableLookupStrategy);

                // test named window
                env.CompileDeploy("create window S0Window#keepall as SupportBean_S0", path);
                env.CompileDeploy(
                    INDEX_CALLBACK_HOOK +
                    "@hint('exclude_plan(true)') on SupportBean_S1 as s1 select * from S0Window as s0 where s1.p10 = s0.p00",
                    path);
                var onExpr = SupportQueryPlanIndexHook.GetAndResetOnExpr();
                Assert.AreEqual(typeof(SubordWMatchExprLookupStrategyAllFilteredForge).Name, onExpr.StrategyName);

                env.UndeployAll();
            }
        }

        internal class EPLOtherJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EventType[] types = {
                    env.Runtime.EventTypeService.GetEventTypePreconfigured("SupportBean_S0"),
                    env.Runtime.EventTypeService.GetEventTypePreconfigured("SupportBean_S1")
                };
                var epl = "select * from SupportBean_S0#keepall as s0, SupportBean_S1#keepall as s1 ";
                var planFullTableScan = SupportQueryPlanBuilder.Start(2)
                    .SetIndexFullTableScan(0, "i0")
                    .SetIndexFullTableScan(1, "i1")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("i1")))
                    .SetLookupPlanInner(1, new FullTableScanLookupPlanForge(1, 0, false, types, GetIndexKey("i0")))
                    .Get();

                // test "any"
                var excludeAny = "@hint('exclude_plan(true)')";
                TryAssertionJoin(env, epl, planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 = p10", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 = 'abc'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 = (p10 || 'A')", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p10 = 'abc'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 > p10", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 > 'A'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p10 > 'A'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p10 > 'A'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 > (p10 || 'A')", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 between p10 and p11", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 between 'a' and p11", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 between 'a' and 'c'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 between p10 and 'c'", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 in (p10, p11)", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 in ('a', p11)", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p00 in ('a', 'b')", planFullTableScan);
                TryAssertionJoin(env, excludeAny + epl + " where p10 in (p00, p01)", planFullTableScan);

                // test EQUALS
                var planEquals = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(0, "i1", "p00")
                    .SetIndexFullTableScan(1, "i2")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("i2")))
                    .SetLookupPlanInner(
                        1,
                        new IndexedTableLookupPlanHashedOnlyForge(
                            1,
                            0,
                            false,
                            types,
                            GetIndexKey("i1"),
                            new[] {SupportExprNodeFactory.MakeKeyed("p10")},
                            null,
                            null))
                    .Get();
                var eplWithWhereEquals = epl + " where p00 = p10";
                TryAssertionJoin(env, "@hint('exclude_plan(from_streamnum=0)')" + eplWithWhereEquals, planEquals);
                TryAssertionJoin(env, "@hint('exclude_plan(from_streamname=\"s0\")')" + eplWithWhereEquals, planEquals);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(from_streamname=\"s0\")') @hint('exclude_plan(from_streamname=\"s1\")')" +
                    eplWithWhereEquals,
                    planFullTableScan);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(from_streamname=\"s0\")') @hint('exclude_plan(from_streamname=\"s1\")')" +
                    eplWithWhereEquals,
                    planFullTableScan);
                TryAssertionJoin(env, "@hint('exclude_plan(to_streamname=\"s1\")')" + eplWithWhereEquals, planEquals);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(to_streamname=\"s0\")') @hint('exclude_plan(to_streamname=\"s1\")')" +
                    eplWithWhereEquals,
                    planFullTableScan);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(from_streamnum=0 and to_streamnum =  1)')" + eplWithWhereEquals,
                    planEquals);
                TryAssertionJoin(env, "@hint('exclude_plan(to_streamnum=1)')" + eplWithWhereEquals, planEquals);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(to_streamnum = 1, from_streamnum = 0)')" + eplWithWhereEquals,
                    planEquals);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(opname=\"equals\")')" + eplWithWhereEquals,
                    planFullTableScan);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(exprs.anyOf(v=> v=\"p00\"))')" + eplWithWhereEquals,
                    planFullTableScan);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(\"p10\" in (exprs))')" + eplWithWhereEquals,
                    planFullTableScan);

                // test greater (relop)
                var planGreater = SupportQueryPlanBuilder.Start(2)
                    .AddIndexBtreeSingle(0, "i1", "p00")
                    .SetIndexFullTableScan(1, "i2")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("i2")))
                    .SetLookupPlanInner(
                        1,
                        new SortedTableLookupPlanForge(
                            1,
                            0,
                            false,
                            types,
                            GetIndexKey("i1"),
                            SupportExprNodeFactory.MakeRangeLess("p10"),
                            null))
                    .Get();
                var eplWithWhereGreater = epl + " where p00 > p10";
                TryAssertionJoin(env, "@hint('exclude_plan(from_streamnum=0)')" + eplWithWhereGreater, planGreater);
                TryAssertionJoin(
                    env,
                    "@hint('exclude_plan(opname=\"relop\")')" + eplWithWhereGreater,
                    planFullTableScan);

                // test range (relop)
                var planRange = SupportQueryPlanBuilder.Start(2)
                    .AddIndexBtreeSingle(0, "i1", "p00")
                    .SetIndexFullTableScan(1, "i2")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("i2")))
                    .SetLookupPlanInner(
                        1,
                        new SortedTableLookupPlanForge(
                            1,
                            0,
                            false,
                            types,
                            GetIndexKey("i1"),
                            SupportExprNodeFactory.MakeRangeIn("p10", "p11"),
                            null))
                    .Get();
                var eplWithWhereRange = epl + " where p00 between p10 and p11";
                TryAssertionJoin(env, "@hint('exclude_plan(from_streamnum=0)')" + eplWithWhereRange, planRange);
                TryAssertionJoin(env, "@hint('exclude_plan(opname=\"relop\")')" + eplWithWhereRange, planFullTableScan);

                // test in (relop)
                var planIn = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(0, "i1", "p00")
                    .SetIndexFullTableScan(1, "i2")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlanForge(0, 1, false, types, GetIndexKey("i2")))
                    .SetLookupPlanInner(
                        1,
                        new InKeywordTableLookupPlanSingleIdxForge(
                            1,
                            0,
                            false,
                            types,
                            GetIndexKey("i1"),
                            SupportExprNodeFactory.MakeIdentExprNodes("p10", "p11")))
                    .Get();
                var eplWithIn = epl + " where p00 in (p10, p11)";
                TryAssertionJoin(env, "@hint('exclude_plan(from_streamnum=0)')" + eplWithIn, planIn);
                TryAssertionJoin(env, "@hint('exclude_plan(opname=\"inkw\")')" + eplWithIn, planFullTableScan);

                env.UndeployAll();
            }
        }

        internal class EPLOtherInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select * from SupportBean_S0 unIdirectional, SupportBean_S1#keepall";
                // no params
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "@hint('exclude_plan') " + epl,
                    "Failed to process statement annotations: Hint 'EXCLUDE_PLAN' requires additional parameters in parentheses");

                // empty parameter allowed, to be filled in
                env.CompileDeploy("@hint('exclude_plan()') " + epl);
                env.SendEventBean(new SupportBean_S0(1));

                // invalid return type
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "@hint('exclude_plan(1)') " + epl,
                    "Expression provIded for hint EXCLUDE_PLAN must return a boolean value");

                // invalid expression
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "@hint('exclude_plan(dummy = 1)') " + epl,
                    "Failed to valIdate hint expression 'dummy=1': Property named 'dummy' is not valId in any stream");

                env.UndeployAll();
            }
        }

        public class AEvent
        {
            private AEvent(string aprop)
            {
                Aprop = aprop;
            }

            public string Aprop { get; }
        }

        public class BEvent
        {
            private BEvent(string bprop)
            {
                Bprop = bprop;
            }

            public string Bprop { get; }
        }
    }
} // end of namespace