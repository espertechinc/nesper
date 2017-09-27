///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestInKeywordQueryPlan : IndexBackingTableInfo
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();

	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
	        _epService.EPAdministrator.Configuration.AddEventType("S2", typeof(SupportBean_S2));
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	    }

        [TearDown]
	    public void TearDown() {
	        _listener = null;
	    }

        [Test]
	    public void TestNotIn()
	    {
	        SupportQueryPlanIndexHook.Reset();
	        var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
	                "where p00 not in (p10, p11)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
	        Assert.AreEqual("null", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));
	    }

        [Test]
	    public void TestMultiIdxMultipleInAndMultirow()
	    {
	        // assert join
	        SupportQueryPlanIndexHook.Reset();
	        var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
	                "where p00 in (p10, p11) and p01 in (p12, p13)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
	        Assert.AreEqual("[p10][p11]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

	        RunAssertionMultiIdx();
	        _epService.EPAdministrator.DestroyAllStatements();

	        // assert named window
	        _epService.EPAdministrator.CreateEPL("create window S1Window#keepall as S1");
	        _epService.EPAdministrator.CreateEPL("insert into S1Window select * from S1");

	        var eplNamedWindow = INDEX_CALLBACK_HOOK + "on S0 as s0 select * from S1Window as s1 " +
	                "where P00 in (P10, p11) and P01 in (P12, P13)";
	        var stmtNamedWindow = _epService.EPAdministrator.CreateEPL(eplNamedWindow);
	        stmtNamedWindow.AddListener(_listener);

	        var onExprNamedWindow = SupportQueryPlanIndexHook.AssertOnExprAndReset();
	        Assert.AreEqual(typeof(SubordInKeywordMultiTableLookupStrategyFactory).Name, onExprNamedWindow.TableLookupStrategy);

	        RunAssertionMultiIdx();

	        // assert table
	        _epService.EPAdministrator.CreateEPL("create table S1Table(Id int primary key, P10 string primary key, P11 string primary key, P12 string primary key, P13 string primary key)");
	        _epService.EPAdministrator.CreateEPL("insert into S1Table select * from S1");
	        _epService.EPAdministrator.CreateEPL("create index S1Idx1 on S1Table(P10)");
	        _epService.EPAdministrator.CreateEPL("create index S1Idx2 on S1Table(P11)");
	        _epService.EPAdministrator.CreateEPL("create index S1Idx3 on S1Table(P12)");
	        _epService.EPAdministrator.CreateEPL("create index S1Idx4 on S1Table(P13)");

	        var eplTable = INDEX_CALLBACK_HOOK + "on S0 as s0 select * from S1Table as s1 " +
	                "where P00 in (P10, P11) and P01 in (P12, P13)";
	        var stmtTable = _epService.EPAdministrator.CreateEPL(eplTable);
	        stmtTable.AddListener(_listener);

	        var onExprTable = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(typeof(SubordInKeywordMultiTableLookupStrategyFactory).Name, onExprTable.TableLookupStrategy);

	        RunAssertionMultiIdx();
	    }

        [Test]
	    public void TestMultiIdxSubquery() {

	        var epl = INDEX_CALLBACK_HOOK + "select s0.id as c0," +
	                "(select * from S1#keepall as s1 " +
	                "  where s0.p00 in (s1.p10, s1.p11) and s0.p01 in (s1.p12, s1.p13))" +
	                ".selectFrom(a=>S1.id) as c1 " +
	                "from S0 as s0";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(typeof(SubordInKeywordMultiTableLookupStrategyFactory).Name, subquery.TableLookupStrategy);

	        // single row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "a", "b", "c", "d"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "x"));
	        AssertSubqueryC0C1(1, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x", "c"));
	        AssertSubqueryC0C1(2, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "a", "c"));
	        AssertSubqueryC0C1(3, new int?[] {101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "b", "d"));
	        AssertSubqueryC0C1(4, new int?[] {101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "a", "d"));
	        AssertSubqueryC0C1(5, new int?[] {101});

	        // 2-row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "a1", "a", "d1", "d"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "a", "x"));
	        AssertSubqueryC0C1(10, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "x", "c"));
	        AssertSubqueryC0C1(11, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(12, "a", "c"));
	        AssertSubqueryC0C1(12, new int?[]{101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(13, "a", "d"));
	        AssertSubqueryC0C1(13, new int?[]{101, 102});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(14, "a1", "d"));
	        AssertSubqueryC0C1(14, new int?[]{102});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(15, "a", "d1"));
	        AssertSubqueryC0C1(15, new int?[]{102});

	        // 3-row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(103, "a", "a2", "d", "d2"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "a", "c"));
	        AssertSubqueryC0C1(20, new int?[]{101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(21, "a", "d"));
	        AssertSubqueryC0C1(21, new int?[]{101, 102, 103});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(22, "a2", "d"));
	        AssertSubqueryC0C1(22, new int?[]{103});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(23, "a", "d2"));
	        AssertSubqueryC0C1(23, new int?[]{103});

	        // test coercion absence - types the same
	        var eplCoercion = INDEX_CALLBACK_HOOK + "select *," +
	                "(select * from S0#keepall as s0 where sb.LongPrimitive in (id)) from SupportBean as sb";
	        _epService.EPAdministrator.CreateEPL(eplCoercion);
	        var subqueryCoercion = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(typeof(SubordFullTableScanLookupStrategyFactory).Name, subqueryCoercion.TableLookupStrategy);
	    }

        [Test]
	    public void TestSingleIdxMultipleInAndMultirow() {
	        // assert join
	        SupportQueryPlanIndexHook.Reset();
	        var epl = INDEX_CALLBACK_HOOK + "select * from S0#keepall as s0, S1 as s1 unidirectional " +
                    "where P00 in (P10, P11) and P01 in (P12, P13)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[0].Items;
	        Assert.AreEqual("[P00]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

	        RunAssertionSingleIdx();
	        _epService.EPAdministrator.DestroyAllStatements();

	        // assert named window
	        _epService.EPAdministrator.CreateEPL("create window S0Window#keepall as S0");
	        _epService.EPAdministrator.CreateEPL("insert into S0Window select * from S0");

	        var eplNamedWindow = INDEX_CALLBACK_HOOK + "on S1 as s1 select * from S0Window as s0 " +
	                "where P00 in (P10, P11) and P01 in (P12, P13)";
	        var stmtNamedWindow = _epService.EPAdministrator.CreateEPL(eplNamedWindow);
	        stmtNamedWindow.AddListener(_listener);

	        var onExprNamedWindow = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(typeof(SubordInKeywordSingleTableLookupStrategyFactory).Name, onExprNamedWindow.TableLookupStrategy);

	        RunAssertionSingleIdx();

	        // assert table
	        _epService.EPAdministrator.CreateEPL("create table S0Table(Id int primary key, P00 string primary key, P01 string primary key, P02 string primary key, P03 string primary key)");
	        _epService.EPAdministrator.CreateEPL("insert into S0Table select * from S0");
	        _epService.EPAdministrator.CreateEPL("create index S0Idx1 on S0Table(P00)");
	        _epService.EPAdministrator.CreateEPL("create index S0Idx2 on S0Table(P01)");

	        var eplTable = INDEX_CALLBACK_HOOK + "on S1 as s1 select * from S0Table as s0 " +
	                "where P00 in (P10, P11) and P01 in (P12, P13)";
	        var stmtTable = _epService.EPAdministrator.CreateEPL(eplTable);
	        stmtTable.AddListener(_listener);

	        var onExprTable = SupportQueryPlanIndexHook.AssertOnExprAndReset();
            Assert.AreEqual(typeof(SubordInKeywordSingleTableLookupStrategyFactory).Name, onExprTable.TableLookupStrategy);

	        RunAssertionSingleIdx();
	    }

        [Test]
	    public void TestSingleIdxSubquery() {
	        SupportQueryPlanIndexHook.Reset();
	        var epl = INDEX_CALLBACK_HOOK + "select s1.id as c0," +
	                "(select * from S0#keepall as s0 " +
	                "  where s0.p00 in (s1.p10, s1.p11) and s0.p01 in (s1.p12, s1.p13))" +
	                ".selectFrom(a=>S0.id) as c1 " +
	                " from S1 as s1";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var subquery = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(typeof(SubordInKeywordSingleTableLookupStrategyFactory).Name, subquery.TableLookupStrategy);

	        // single row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "a", "c"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "a1", "b", "c", "d"));
	        AssertSubqueryC0C1(1, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "a", "b", "x", "d"));
	        AssertSubqueryC0C1(2, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "a", "b", "c", "d"));
	        AssertSubqueryC0C1(3, new int?[] {100});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "x", "a", "x", "c"));
	        AssertSubqueryC0C1(4, new int?[] {100});

	        // 2-rows available tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "a", "d"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "a1", "b", "c", "d"));
	        AssertSubqueryC0C1(10, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "a", "b", "x", "c1"));
	        AssertSubqueryC0C1(11, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "a", "b", "c", "d"));
	        AssertSubqueryC0C1(12, new int?[] {100, 101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "x", "a", "x", "c"));
	        AssertSubqueryC0C1(13, new int?[] {100});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(14, "x", "a", "d", "x"));
	        AssertSubqueryC0C1(14, new int?[] {101});

	        // 3-rows available tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "b", "c"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "a1", "b", "c1", "d"));
	        AssertSubqueryC0C1(20, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(21, "a", "b", "x", "c1"));
	        AssertSubqueryC0C1(21, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(22, "a", "b", "c", "d"));
	        AssertSubqueryC0C1(22, new int?[] {100, 101, 102});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(23, "b", "a", "x", "c"));
	        AssertSubqueryC0C1(23, new int?[] {100, 102});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(24, "b", "a", "d", "c"));
	        AssertSubqueryC0C1(24, new int?[] {100, 101, 102});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(25, "b", "x", "x", "c"));
	        AssertSubqueryC0C1(25, new int?[] {102});

	        // test coercion absence - types the same
	        var eplCoercion = INDEX_CALLBACK_HOOK + "select *," +
	                "(select * from SupportBean#keepall as sb where sb.LongPrimitive in (s0.id)) from S0 as s0";
	        _epService.EPAdministrator.CreateEPL(eplCoercion);
	        var subqueryCoercion = SupportQueryPlanIndexHook.AssertSubqueryAndReset();
            Assert.AreEqual(typeof(SubordFullTableScanLookupStrategyFactory).Name, subqueryCoercion.TableLookupStrategy);
	    }

	    private void RunAssertionSingleIdx()
	    {
	        var fields = "s0.Id,s1.Id".Split(',');

	        // single row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "a", "c"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a1", "b", "c", "d"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a", "b", "x", "d"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "a", "b", "c", "d"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 1 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "x", "a", "x", "c"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 2 } });

	        // 2-rows available tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "a", "d"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a1", "b", "c", "d"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a", "b", "x", "c1"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "a", "b", "c", "d"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 10 }, new object[] { 101, 10 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "x", "a", "x", "c"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 11 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "x", "a", "d", "x"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 101, 12 } });

	        // 3-rows available tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(102, "b", "c"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a1", "b", "c1", "d"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "a", "b", "x", "c1"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "a", "b", "c", "d"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 20 }, new object[] { 101, 20 }, new object[] { 102, 20 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(21, "b", "a", "x", "c"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 21 }, new object[] { 102, 21 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(22, "b", "a", "d", "c"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 100, 22 }, new object[] { 101, 22 }, new object[] { 102, 22 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(23, "b", "x", "x", "c"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 102, 23 } });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionMultiIdx() {
	        var fields = "s0.Id,s1.Id".Split(',');

	        // single row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "a", "b", "c", "d"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "a", "x"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "x", "c"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "c"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, 101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "b", "d"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, 101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "a", "d"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3, 101});

	        // 2-row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "a1", "a", "d1", "d"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "a", "x"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "x", "c"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "a", "c"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "a", "d"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 11, 101 }, new object[] { 11, 102 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(12, "a1", "d"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 12, 102 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(13, "a", "d1"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 13, 102 } });

	        // 3-row tests
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(103, "a", "a2", "d", "d2"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "a", "c"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20, 101});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(21, "a", "d"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 21, 101 }, new object[] { 21, 102 }, new object[] { 21, 103 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(22, "a2", "d"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 22, 103 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(23, "a", "d2"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 23, 103 } });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestSingleIdxConstants()
	    {
	        SupportQueryPlanIndexHook.Reset();
	        var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
	                "where p10 in ('a', 'b')";
	        var fields = "s0.id,s1.id".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
	        Assert.AreEqual("[p10]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "x"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "a"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 1, 101 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "b"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 2, 101 }, new object[] { 2, 102 } });
	    }

        [Test]
	    public void TestMultiIdxConstants()
	    {
	        SupportQueryPlanIndexHook.Reset();
	        var epl = INDEX_CALLBACK_HOOK + "select * from S0 as s0 unidirectional, S1#keepall as s1 " +
	                "where 'a' in (p10, p11)";
	        var fields = "s0.id,s1.id".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var items = SupportQueryPlanIndexHook.AssertJoinAndReset().IndexSpecs[1].Items;
	        Assert.AreEqual("[p10][p11]", SupportQueryPlanIndexHelper.GetIndexedExpressions(items));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "x", "y"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "x", "a"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 1, 101 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "b", "a"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 2, 101 }, new object[] { 2, 102 } });
	    }

        [Test]
	    public void TestQueryPlan3Stream()
	    {
	        var epl = "select * from S0 as s0 unidirectional, S1#keepall, S2#keepall ";

	        // 3-stream join with in-multiindex directional
	        var planInMidx = new InKeywordTableLookupPlanMultiIdx(0, 1, GetIndexKeys("i1a", "i1b"), SupportExprNodeFactory.MakeIdentExprNode("p00"));
	        RunAssertion(epl + " where p00 in (p10, p11)",
	                SupportQueryPlanBuilder.Start(3)
	                        .AddIndexHashSingleNonUnique(1, "i1a", "p10")
	                        .AddIndexHashSingleNonUnique(1, "i1b", "p11")
	                        .SetIndexFullTableScan(2, "i2")
	                        .SetLookupPlanInstruction(0, "s0", new LookupInstructionPlan[]{
	                                new LookupInstructionPlan(0, "s0", new int[] {1},
	                                        new TableLookupPlan[] {planInMidx}, null, new bool[3]),
	                                new LookupInstructionPlan(0, "s0", new int[] {2},
	                                        new TableLookupPlan[] {new FullTableScanLookupPlan(1, 2, GetIndexKey("i2"))}, null, new bool[3])
	                        })
	                        .Get());

	        var planInMidxMulitiSrc = new InKeywordTableLookupPlanMultiIdx(0, 1, GetIndexKeys("i1", "i2"), SupportExprNodeFactory.MakeIdentExprNode("p00"));
	        RunAssertion(epl + " where p00 in (p10, p20)",
	                SupportQueryPlanBuilder.Start(3)
	                        .SetIndexFullTableScan(1, "i1")
	                        .SetIndexFullTableScan(2, "i2")
	                        .SetLookupPlanInstruction(0, "s0", new LookupInstructionPlan[]{
	                                new LookupInstructionPlan(0, "s0", new int[] {1},
	                                        new TableLookupPlan[] {new FullTableScanLookupPlan(0, 1, GetIndexKey("i1"))}, null, new bool[3]),
	                                new LookupInstructionPlan(0, "s0", new int[] {2},
	                                        new TableLookupPlan[] {new FullTableScanLookupPlan(1, 2, GetIndexKey("i2"))}, null, new bool[3])
	                        })
	                        .Get());

	        // 3-stream join with in-singleindex directional
	        var planInSidx = new InKeywordTableLookupPlanSingleIdx(0, 1, GetIndexKey("i1"), SupportExprNodeFactory.MakeIdentExprNodes("p00", "p01"));
	        RunAssertion(epl + " where p10 in (p00, p01)", GetSingleIndexPlan(planInSidx));

	        // 3-stream join with in-singleindex multi-sourced
	        var planInSingleMultiSrc = new InKeywordTableLookupPlanSingleIdx(0, 1, GetIndexKey("i1"), SupportExprNodeFactory.MakeIdentExprNodes("p00"));
	        RunAssertion(epl + " where p10 in (p00, p20)", GetSingleIndexPlan(planInSingleMultiSrc));
	    }

        [Test]
	    public void TestQueryPlan2Stream()
	    {
	        var epl = "select * from S0 as s0 unidirectional, S1#keepall ";
	        var fullTableScan = SupportQueryPlanBuilder.Start(2)
	                .SetIndexFullTableScan(1, "a")
	                .SetLookupPlanInner(0, new FullTableScanLookupPlan(0, 1, GetIndexKey("a"))).Get();

	        // 2-stream unidirectional joins
	        RunAssertion(epl, fullTableScan);

	        var planEquals = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p10")
	                .SetLookupPlanInner(0, new IndexedTableLookupPlanSingle(0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeKeyed("p00"))).Get();
	        RunAssertion(epl + "where p00 = p10", planEquals);
	        RunAssertion(epl + "where p00 = p10 and p00 in (p11, p12, p13)", planEquals);

	        var planInMultiInner = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p11")
	                .AddIndexHashSingleNonUnique(1, "b", "p12")
	                .SetLookupPlanInner(0, new InKeywordTableLookupPlanMultiIdx(0, 1, GetIndexKeys("a", "b"), SupportExprNodeFactory.MakeIdentExprNode("p00"))).Get();
	        RunAssertion(epl + "where p00 in (p11, p12)", planInMultiInner);
	        RunAssertion(epl + "where p00 = p11 or p00 = p12", planInMultiInner);

	        var planInMultiOuter = SupportQueryPlanBuilder.Start(planInMultiInner)
	                .SetLookupPlanOuter(0, new InKeywordTableLookupPlanMultiIdx(0, 1, GetIndexKeys("a", "b"), SupportExprNodeFactory.MakeIdentExprNode("p00"))).Get();
	        var eplOuterJoin = "select * from S0 as s0 unidirectional full outer join S1#keepall ";
	        RunAssertion(eplOuterJoin + "where p00 in (p11, p12)", planInMultiOuter);

	        var planInMultiWConst = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p11")
	                .AddIndexHashSingleNonUnique(1, "b", "p12")
	                .SetLookupPlanInner(0, new InKeywordTableLookupPlanMultiIdx(0, 1, GetIndexKeys("a", "b"), SupportExprNodeFactory.MakeConstExprNode("A"))).Get();
	        RunAssertion(epl + "where 'A' in (p11, p12)", planInMultiWConst);
	        RunAssertion(epl + "where 'A' = p11 or 'A' = p12", planInMultiWConst);

	        var planInMultiWAddConst = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p12")
	                .SetLookupPlanInner(0, new InKeywordTableLookupPlanMultiIdx(0, 1, GetIndexKeys("a"), SupportExprNodeFactory.MakeConstExprNode("A"))).Get();
	        RunAssertion(epl + "where 'A' in ('B', p12)", planInMultiWAddConst);
	        RunAssertion(epl + "where 'A' in ('B', 'C')", fullTableScan);

	        var planInSingle = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p10")
	                .SetLookupPlanInner(0, new InKeywordTableLookupPlanSingleIdx(0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeIdentExprNodes("p00", "p01"))).Get();
	        RunAssertion(epl + "where p10 in (p00, p01)", planInSingle);

	        var planInSingleWConst = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p10")
	                .SetLookupPlanInner(0, new InKeywordTableLookupPlanSingleIdx(0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeConstAndIdentNode("A", "p01"))).Get();
	        RunAssertion(epl + "where p10 in ('A', p01)", planInSingleWConst);

	        var planInSingleJustConst = SupportQueryPlanBuilder.Start(2)
	                .AddIndexHashSingleNonUnique(1, "a", "p10")
	                .SetLookupPlanInner(0, new InKeywordTableLookupPlanSingleIdx(0, 1, GetIndexKey("a"), SupportExprNodeFactory.MakeConstAndConstNode("A", "B"))).Get();
	        RunAssertion(epl + "where p10 in ('A', 'B')", planInSingleJustConst);
	    }

	    private void RunAssertion(string epl, QueryPlan expectedPlan)
	    {
	        SupportQueryPlanIndexHook.Reset();
	        epl = INDEX_CALLBACK_HOOK + epl;
	        _epService.EPAdministrator.CreateEPL(epl);

	        var actualPlan = SupportQueryPlanIndexHook.AssertJoinAndReset();
	        SupportQueryPlanIndexHelper.CompareQueryPlans(expectedPlan, actualPlan);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void AssertSubqueryC0C1(int c0, int?[] c1) {
	        var @event = _listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(c0, @event.Get("c0"));
	        var c1Coll = @event.Get("c1").Unwrap<int?>();
	        EPAssertionUtil.AssertEqualsAnyOrder(c1, c1Coll ?? null);
	    }

	    private QueryPlan GetSingleIndexPlan(InKeywordTableLookupPlanSingleIdx plan) {
	        return SupportQueryPlanBuilder.Start(3)
	                .AddIndexHashSingleNonUnique(1, "i1", "p10")
	                .SetIndexFullTableScan(2, "i2")
	                .SetLookupPlanInstruction(0, "s0", new LookupInstructionPlan[]{
	                        new LookupInstructionPlan(0, "s0", new int[] {1},
	                                new TableLookupPlan[] {plan}, null, new bool[3]),
	                        new LookupInstructionPlan(0, "s0", new int[] {2},
	                                new TableLookupPlan[] {new FullTableScanLookupPlan(1, 2, GetIndexKey("i2"))}, null, new bool[3])
	                })
	                .Get();
	    }

	    private static TableLookupIndexReqKey[] GetIndexKeys(params string[] names) {
	        var keys = new TableLookupIndexReqKey[names.Length];
	        for (var i = 0; i < names.Length; i++) {
	            keys[i] = new TableLookupIndexReqKey(names[i]);
	        }
	        return keys;
	    }

	    private static TableLookupIndexReqKey GetIndexKey(string name) {
	        return new TableLookupIndexReqKey(name);
	    }
	}
} // end of namespace
