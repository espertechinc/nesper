///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLPlanExcludeHint : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
    
            RunAssertionDocSample(epService);
            RunAssertionJoin(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionDocSample(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(AEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(BEvent));
    
            var hints = new string[]{
                    "@Hint('Exclude_plan(true)')",
                    "@Hint('Exclude_plan(opname=\"equals\")')",
                    "@Hint('Exclude_plan(opname=\"equals\" and from_streamname=\"a\")')",
                    "@Hint('Exclude_plan(opname=\"equals\" and from_streamname=\"b\")')",
                    "@Hint('Exclude_plan(exprs[0]=\"aprop\")')"};
            foreach (string hint in hints) {
                epService.EPAdministrator.CreateEPL("@Audit " + hint +
                        "select * from AEvent#keepall as a, BEvent#keepall as b where aprop = bprop");
            }
    
            // test subquery
            SupportQueryPlanIndexHook.Reset();
            epService.EPAdministrator.CreateEPL(INDEX_CALLBACK_HOOK + "@Hint('Exclude_plan(true)') select (select * from S0#unique(p00) as s0 where s1.p10 = p00) from S1 as s1");
            QueryPlanIndexDescSubquery subq = SupportQueryPlanIndexHook.GetAndResetSubqueries()[0];
            Assert.AreEqual(typeof(SubordFullTableScanLookupStrategyFactory).Name, subq.TableLookupStrategy);
    
            // test named window
            epService.EPAdministrator.CreateEPL("create window S0Window#keepall as S0");
            epService.EPAdministrator.CreateEPL(INDEX_CALLBACK_HOOK + "@Hint('Exclude_plan(true)') on S1 as s1 select * from S0Window as s0 where s1.p10 = s0.p00");
            QueryPlanIndexDescOnExpr onExpr = SupportQueryPlanIndexHook.GetAndResetOnExpr();
            Assert.AreEqual(typeof(SubordWMatchExprLookupStrategyFactoryAllFiltered).Name, onExpr.StrategyName);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionJoin(EPServiceProvider epService) {
            string epl = "select * from S0#keepall as s0, S1#keepall as s1 ";
            QueryPlan planFullTableScan = SupportQueryPlanBuilder.Start(2)
                .SetIndexFullTableScan(0, "i0")
                .SetIndexFullTableScan(1, "i1")
                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i1")))
                .SetLookupPlanInner(1, new FullTableScanLookupPlan(1, 0, GetIndexKey("i0")))
                .Get();
    
            // test "any"
            string excludeAny = "@Hint('Exclude_plan(true)')";
            TryAssertionJoin(epService, epl, planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 = p10", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 = 'abc'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 = (p10 || 'A')", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p10 = 'abc'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 > p10", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 > 'A'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p10 > 'A'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p10 > 'A'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 > (p10 || 'A')", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 between p10 and p11", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 between 'a' and p11", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 between 'a' and 'c'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 between p10 and 'c'", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 in (p10, p11)", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 in ('a', p11)", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p00 in ('a', 'b')", planFullTableScan);
            TryAssertionJoin(epService, excludeAny + epl + " where p10 in (p00, p01)", planFullTableScan);
    
            // test EQUALS
            QueryPlan planEquals = SupportQueryPlanBuilder.Start(2)
                .AddIndexHashSingleNonUnique(0, "i1", "p00")
                .SetIndexFullTableScan(1, "i2")
                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
                .SetLookupPlanInner(1, new IndexedTableLookupPlanSingle(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeKeyed("p10")))
                .Get();
            string eplWithWhereEquals = epl + " where p00 = p10";
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamnum=0)')" + eplWithWhereEquals, planEquals);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamname=\"s0\")')" + eplWithWhereEquals, planEquals);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamname=\"s0\")') @Hint('Exclude_plan(from_streamname=\"s1\")')" + eplWithWhereEquals, planFullTableScan);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamname=\"s0\")') @Hint('Exclude_plan(from_streamname=\"s1\")')" + eplWithWhereEquals, planFullTableScan);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(to_streamname=\"s1\")')" + eplWithWhereEquals, planEquals);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(to_streamname=\"s0\")') @Hint('Exclude_plan(to_streamname=\"s1\")')" + eplWithWhereEquals, planFullTableScan);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamnum=0 and to_streamnum =  1)')" + eplWithWhereEquals, planEquals);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(to_streamnum=1)')" + eplWithWhereEquals, planEquals);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(to_streamnum = 1, from_streamnum = 0)')" + eplWithWhereEquals, planEquals);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(opname=\"equals\")')" + eplWithWhereEquals, planFullTableScan);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(exprs.anyOf(v=> v=\"p00\"))')" + eplWithWhereEquals, planFullTableScan);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(\"p10\" in (exprs))')" + eplWithWhereEquals, planFullTableScan);
    
            // test greater (relop)
            QueryPlan planGreater = SupportQueryPlanBuilder.Start(2)
                .AddIndexBtreeSingle(0, "i1", "p00")
                .SetIndexFullTableScan(1, "i2")
                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
                .SetLookupPlanInner(1, new SortedTableLookupPlan(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeRangeLess("p10")))
                .Get();
            string eplWithWhereGreater = epl + " where p00 > p10";
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamnum=0)')" + eplWithWhereGreater, planGreater);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(opname=\"relop\")')" + eplWithWhereGreater, planFullTableScan);
    
            // test range (relop)
            QueryPlan planRange = SupportQueryPlanBuilder.Start(2)
                    .AddIndexBtreeSingle(0, "i1", "p00")
                    .SetIndexFullTableScan(1, "i2")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
                    .SetLookupPlanInner(1, new SortedTableLookupPlan(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeRangeIn("p10", "p11")))
                    .Get();
            string eplWithWhereRange = epl + " where p00 between p10 and p11";
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamnum=0)')" + eplWithWhereRange, planRange);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(opname=\"relop\")')" + eplWithWhereRange, planFullTableScan);
    
            // test in (relop)
            QueryPlan planIn = SupportQueryPlanBuilder.Start(2)
                    .AddIndexHashSingleNonUnique(0, "i1", "p00")
                    .SetIndexFullTableScan(1, "i2")
                    .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
                    .SetLookupPlanInner(1, new InKeywordTableLookupPlanSingleIdx(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeIdentExprNodes("p10", "p11")))
                    .Get();
            string eplWithIn = epl + " where p00 in (p10, p11)";
            TryAssertionJoin(epService, "@Hint('Exclude_plan(from_streamnum=0)')" + eplWithIn, planIn);
            TryAssertionJoin(epService, "@Hint('Exclude_plan(opname=\"inkw\")')" + eplWithIn, planFullTableScan);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl = "select * from S0 unidirectional, S1#keepall";
            // no params
            TryInvalid(epService, "@Hint('exclude_plan') " + epl,
                    "Failed to process statement annotations: Hint 'EXCLUDE_PLAN' requires additional parameters in parentheses [@Hint('exclude_plan') select * from S0 unidirectional, S1#keepall]");
    
            // empty parameter allowed, to be filled in
            epService.EPAdministrator.CreateEPL("@Hint('Exclude_plan()') " + epl);
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            // invalid return type
            TryInvalid(epService, "@Hint('Exclude_plan(1)') " + epl,
                    "Error starting statement: Expression provided for hint EXCLUDE_PLAN must return a boolean value [@Hint('Exclude_plan(1)') select * from S0 unidirectional, S1#keepall]");
    
            // invalid expression
            TryInvalid(epService, "@Hint('Exclude_plan(dummy = 1)') " + epl,
                    "Error starting statement: Failed to validate hint expression 'dummy=1': Property named 'dummy' is not valid in any stream [@Hint('Exclude_plan(dummy = 1)') select * from S0 unidirectional, S1#keepall]");
        }
    
        private void TryAssertionJoin(EPServiceProvider epService, string epl, QueryPlan expectedPlan) {
            SupportQueryPlanIndexHook.Reset();
            epl = INDEX_CALLBACK_HOOK + epl;
            epService.EPAdministrator.CreateEPL(epl);
    
            QueryPlan actualPlan = SupportQueryPlanIndexHook.AssertJoinAndReset();
            SupportQueryPlanIndexHelper.CompareQueryPlans(expectedPlan, actualPlan);
    
            epService.EPAdministrator.DestroyAllStatements();
        }

        public class AEvent
        {
            public string Aprop { get; }
            public AEvent(string aprop)
            {
                Aprop = aprop;
            }
        }

        public class BEvent
        {
            public string Bprop { get; }
            public BEvent(string bprop)
            {
                Bprop = bprop;
            }
        }

        private static TableLookupIndexReqKey GetIndexKey(string name)
        {
            return new TableLookupIndexReqKey(name);
        }
    }
} // end of namespace
