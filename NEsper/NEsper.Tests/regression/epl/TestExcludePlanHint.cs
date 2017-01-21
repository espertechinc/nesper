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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestExcludePlanHint : IndexBackingTableInfo
	{
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
	    {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
	    }

        [Test]
	    public void TestDocSample() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(AEvent));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(BEvent));

	        string[] hints = new string[] {
	                "@Hint('exclude_plan(true)')",
	                "@Hint('exclude_plan(opname=\"equals\")')",
	                "@Hint('exclude_plan(opname=\"equals\" and from_streamname=\"a\")')",
	                "@Hint('exclude_plan(opname=\"equals\" and from_streamname=\"b\")')",
	                "@Hint('exclude_plan(exprs[0]=\"aprop\")')"};
	        foreach (string hint in hints) {
	            _epService.EPAdministrator.CreateEPL("@Audit " + hint +
	                    "select * from AEvent.win:keepall() as a, BEvent.win:keepall() as b where aprop = bprop");
	        }

	        // test subquery
	        SupportQueryPlanIndexHook.Reset();
	        _epService.EPAdministrator.CreateEPL(INDEX_CALLBACK_HOOK + "@Hint('exclude_plan(true)') select (select * from S0.std:unique(p00) as s0 where s1.p10 = p00) from S1 as s1");
	        QueryPlanIndexDescSubquery subq = SupportQueryPlanIndexHook.GetAndResetSubqueries()[0];
	        Assert.AreEqual(typeof(SubordFullTableScanLookupStrategyFactory).Name, subq.TableLookupStrategy);

	        // test named window
	        _epService.EPAdministrator.CreateEPL("create window S0Window.win:keepall() as S0");
	        _epService.EPAdministrator.CreateEPL(INDEX_CALLBACK_HOOK + "@Hint('exclude_plan(true)') on S1 as s1 select * from S0Window as s0 where s1.p10 = s0.p00");
	        QueryPlanIndexDescOnExpr onExpr = SupportQueryPlanIndexHook.GetAndResetOnExpr();
	        Assert.AreEqual(typeof(SubordWMatchExprLookupStrategyFactoryAllFiltered).Name, onExpr.StrategyName);
	    }

        [Test]
	    public void TestJoin() {
	        string epl = "select * from S0.win:keepall() as s0, S1.win:keepall() as s1 ";
	        QueryPlan planFullTableScan = SupportQueryPlanBuilder.Start(2)
	                .SetIndexFullTableScan(0, "i0")
	                .SetIndexFullTableScan(1, "i1")
	                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i1")))
	                .SetLookupPlanInner(1, new FullTableScanLookupPlan(1, 0, GetIndexKey("i0"))).Get();

	        // test "any"
	        string excludeAny = "@Hint('exclude_plan(true)')";
	        RunAssertionJoin(epl, planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 = p10", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 = 'abc'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 = (p10 || 'A')", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p10 = 'abc'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 > p10", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 > 'A'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p10 > 'A'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p10 > 'A'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 > (p10 || 'A')", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 between p10 and p11", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 between 'a' and p11", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 between 'a' and 'c'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 between p10 and 'c'", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 in (p10, p11)", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 in ('a', p11)", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p00 in ('a', 'b')", planFullTableScan);
	        RunAssertionJoin(excludeAny + epl + " where p10 in (p00, p01)", planFullTableScan);

	        // test EQUALS
	        QueryPlan planEquals = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(0, "i1", "p00")
	                .SetIndexFullTableScan(1, "i2")
	                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
	                .SetLookupPlanInner(1, new IndexedTableLookupPlanSingle(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeKeyed("p10"))).Get();
	        string eplWithWhereEquals = epl + " where p00 = p10";
	        RunAssertionJoin("@Hint('exclude_plan(from_streamnum=0)')" + eplWithWhereEquals, planEquals);
	        RunAssertionJoin("@Hint('exclude_plan(from_streamname=\"s0\")')" + eplWithWhereEquals, planEquals);
	        RunAssertionJoin("@Hint('exclude_plan(from_streamname=\"s0\")') @Hint('exclude_plan(from_streamname=\"s1\")')" + eplWithWhereEquals, planFullTableScan);
	        RunAssertionJoin("@Hint('exclude_plan(from_streamname=\"s0\")') @Hint('exclude_plan(from_streamname=\"s1\")')" + eplWithWhereEquals, planFullTableScan);
	        RunAssertionJoin("@Hint('exclude_plan(to_streamname=\"s1\")')" + eplWithWhereEquals, planEquals);
	        RunAssertionJoin("@Hint('exclude_plan(to_streamname=\"s0\")') @Hint('exclude_plan(to_streamname=\"s1\")')" + eplWithWhereEquals, planFullTableScan);
	        RunAssertionJoin("@Hint('exclude_plan(from_streamnum=0 and to_streamnum =  1)')" + eplWithWhereEquals, planEquals);
	        RunAssertionJoin("@Hint('exclude_plan(to_streamnum=1)')" + eplWithWhereEquals, planEquals);
	        RunAssertionJoin("@Hint('exclude_plan(to_streamnum = 1, from_streamnum = 0)')" + eplWithWhereEquals, planEquals);
	        RunAssertionJoin("@Hint('exclude_plan(opname=\"equals\")')" + eplWithWhereEquals, planFullTableScan);
	        RunAssertionJoin("@Hint('exclude_plan(exprs.anyOf(v=> v=\"p00\"))')" + eplWithWhereEquals, planFullTableScan);
	        RunAssertionJoin("@Hint('exclude_plan(\"p10\" in (exprs))')" + eplWithWhereEquals, planFullTableScan);

	        // test greater (relop)
	        QueryPlan planGreater = SupportQueryPlanBuilder.Start(2)
	                .AddIndexBtreeSingle(0, "i1", "p00")
	                .SetIndexFullTableScan(1, "i2")
	                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
	                .SetLookupPlanInner(1, new SortedTableLookupPlan(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeRangeLess("p10"))).Get();
	        string eplWithWhereGreater = epl + " where p00 > p10";
	        RunAssertionJoin("@Hint('exclude_plan(from_streamnum=0)')" + eplWithWhereGreater, planGreater);
	        RunAssertionJoin("@Hint('exclude_plan(opname=\"relop\")')" + eplWithWhereGreater, planFullTableScan);

	        // test range (relop)
	        QueryPlan planRange = SupportQueryPlanBuilder.Start(2)
	                .AddIndexBtreeSingle(0, "i1", "p00")
	                .SetIndexFullTableScan(1, "i2")
	                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
	                .SetLookupPlanInner(1, new SortedTableLookupPlan(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeRangeIn("p10", "p11"))).Get();
	        string eplWithWhereRange = epl + " where p00 between p10 and p11";
	        RunAssertionJoin("@Hint('exclude_plan(from_streamnum=0)')" + eplWithWhereRange, planRange);
	        RunAssertionJoin("@Hint('exclude_plan(opname=\"relop\")')" + eplWithWhereRange, planFullTableScan);

	        // test in (relop)
	        QueryPlan planIn = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(0, "i1", "p00")
	                .SetIndexFullTableScan(1, "i2")
	                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("i2")))
	                .SetLookupPlanInner(1, new InKeywordTableLookupPlanSingleIdx(1, 0, GetIndexKey("i1"), SupportExprNodeFactory.MakeIdentExprNodes("p10", "p11"))).Get();
	        string eplWithIn = epl + " where p00 in (p10, p11)";
	        RunAssertionJoin("@Hint('exclude_plan(from_streamnum=0)')" + eplWithIn, planIn);
	        RunAssertionJoin("@Hint('exclude_plan(opname=\"inkw\")')" + eplWithIn, planFullTableScan);
	    }

        [Test]
	    public void TestInvalid() {
	        string epl = "select * from S0 unidirectional, S1.win:keepall()";
	        // no params
	        TryInvalid("@Hint('exclude_plan') " + epl,
	                "Failed to process statement annotations: Hint 'EXCLUDE_PLAN' requires additional parameters in parentheses [@Hint('exclude_plan') select * from S0 unidirectional, S1.win:keepall()]");

	        // empty parameter allowed, to be filled in
	        _epService.EPAdministrator.CreateEPL("@Hint('exclude_plan()') " + epl);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        // invalid return type
	        TryInvalid("@Hint('exclude_plan(1)') " + epl,
	                "Error starting statement: Expression provided for hint EXCLUDE_PLAN must return a boolean value [@Hint('exclude_plan(1)') select * from S0 unidirectional, S1.win:keepall()]");

	        // invalid expression
	        TryInvalid("@Hint('exclude_plan(dummy = 1)') " + epl,
	                "Error starting statement: Failed to validate hint expression 'dummy=1': Property named 'dummy' is not valid in any stream [@Hint('exclude_plan(dummy = 1)') select * from S0 unidirectional, S1.win:keepall()]");
	    }

	    private void TryInvalid(string epl, string expected) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(expected, ex.Message);
	        }
	    }

	    private void RunAssertionJoin(string epl, QueryPlan expectedPlan)
	    {
	        SupportQueryPlanIndexHook.Reset();
	        epl = INDEX_CALLBACK_HOOK + epl;
	        _epService.EPAdministrator.CreateEPL(epl);

	        QueryPlan actualPlan = SupportQueryPlanIndexHook.AssertJoinAndReset();
	        SupportQueryPlanIndexHelper.CompareQueryPlans(expectedPlan, actualPlan);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        public class AEvent
        {
            public string Aprop { get; private set; }
            public AEvent(string aprop)
            {
	            this.Aprop = aprop;
	        }
	    }

        public class BEvent
        {
            public string Bprop { get; private set; }
            public BEvent(string bprop)
            {
	            this.Bprop = bprop;
	        }
	    }

	    private static TableLookupIndexReqKey GetIndexKey(string name)
        {
	        return new TableLookupIndexReqKey(name);
	    }
	}
} // end of namespace
