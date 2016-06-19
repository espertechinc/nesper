///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateLocalGroupBy 
	{
	    public string PLAN_CALLBACK_HOOK = "@Hook(type=" + typeof(HookType).FullName + ".INTERNAL_AGGLOCALLEVEL,hook='" + typeof(SupportAggLevelPlanHook).FullName + "')";

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();

	        foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }
	        SupportAggLevelPlanHook.GetAndReset();

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() 
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestInvalid()
        {
	        // not valid with count-min-sketch
	        SupportMessageAssertUtil.TryInvalid(_epService, "create table MyTable(approx countMinSketch(group_by:TheString) @type(SupportBean))",
	                "Error starting statement: Failed to validate table-column expression 'countMinSketch(group_by:TheString)': Count-min-sketch aggregation function 'countMinSketch'  expects either no parameter or a single json parameter object");

	        // not allowed with tables
	        SupportMessageAssertUtil.TryInvalid(_epService, "create table MyTable(col sum(int, group_by:TheString) @type(SupportBean))",
	                "Error starting statement: Failed to validate table-column expression 'sum(int,group_by:TheString)': The 'group_by' parameter is not allowed in create-table statements");

	        // invalid named parameter
	        SupportMessageAssertUtil.TryInvalid(_epService, "select sum(IntPrimitive, xxx:TheString) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'sum(IntPrimitive,xxx:TheString)': Invalid named parameter 'xxx' (did you mean 'group_by'?) [");

	        // invalid group-by expression
	        SupportMessageAssertUtil.TryInvalid(_epService, "select sum(IntPrimitive, group_by:sum(IntPrimitive)) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'sum(IntPrimitive,group_by:sum(intPr...(44 chars)': Group-by expressions cannot contain aggregate functions");

	        // other functions don't accept this named parameter
	        SupportMessageAssertUtil.TryInvalid(_epService, "select coalesce(0, 1, group_by:TheString) from SupportBean",
	                "Incorrect syntax near ':' at line 1 column 30");
	        SupportMessageAssertUtil.TryInvalid(_epService, "select " + typeof(SupportStaticMethodLib).Name + ".staticMethod(group_by:IntPrimitive) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'com.espertech.esper.support.epl.Sup...(90 chars)': Named parameters are not allowed");

	        // not allowed in combination with roll-up
	        SupportMessageAssertUtil.TryInvalid(_epService, "select sum(IntPrimitive, group_by:TheString) from SupportBean group by rollup(TheString)",
	                "Error starting statement: Roll-up and group-by parameters cannot be combined ");

	        // not allowed in combination with into-table
	        _epService.EPAdministrator.CreateEPL("create table mytable (thesum sum(int))");
	        SupportMessageAssertUtil.TryInvalid(_epService, "into table mytable select sum(IntPrimitive, group_by:TheString) as thesum from SupportBean",
	                "Error starting statement: Into-table and group-by parameters cannot be combined");

	        // not allowed for match-rezognize measure clauses
	        var eplMatchRecog = "select * from SupportBean match_recognize (" +
	                "  measures count(B.IntPrimitive, group_by:B.TheString) pattern (A B* C))";
	        SupportMessageAssertUtil.TryInvalid(_epService, eplMatchRecog,
	                "Error starting statement: Match-recognize does not allow aggregation functions to specify a group-by");

	        // disallow subqueries to specify their own local group-by
	        var eplSubq = "select (select sum(IntPrimitive, group_by:TheString) from SupportBean.win:keepall()) from SupportBean_S0";
	        SupportMessageAssertUtil.TryInvalid(_epService, eplSubq,
	                "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect aggregations functions cannot specify a group-by");
	    }

        [Test]
	    public void TestUngroupedAndLocalSyntax() {
	        RunAssertionUngroupedAggSQLStandard();
	        RunAssertionUngroupedAggEvent();
	        RunAssertionUngroupedAggAdditionalAndPlugin();
	        RunAssertionUngroupedAggIterator();
	        RunAssertionUngroupedParenSODA(false);
	        RunAssertionUngroupedParenSODA(true);
	        RunAssertionColNameRendering();
	        RunAssertionUngroupedSameKey();
	        RunAssertionUngroupedRowRemove();
	        RunAssertionUngroupedHaving();
	        RunAssertionUngroupedOrderBy();
	        RunAssertionUngroupedUnidirectionalJoin();
	        RunAssertionEnumMethods(true);
	    }

        [Test]
	    public void TestGrouped() {
	        RunAssertionGroupedSolutionPattern();
	        RunAssertionGroupedMultiLevelMethod();
	        RunAssertionGroupedMultiLevelAccess();
	        RunAssertionGroupedMultiLevelNoDefaultLvl();
	        RunAssertionGroupedSameKey();
	        RunAssertionGroupedRowRemove();
	        RunAssertionGroupedOnSelect();
	        RunAssertionEnumMethods(false);
	    }

        [Test]
	    public void TestPlanning() {
	        AssertNoPlan("select sum(group_by:(),IntPrimitive) as c0 from SupportBean");
	        AssertNoPlan("select sum(group_by:(TheString),IntPrimitive) as c0 from SupportBean group by TheString");
	        AssertNoPlan("select sum(group_by:(TheString, IntPrimitive),LongPrimitive) as c0 from SupportBean group by TheString, IntPrimitive");
	        AssertNoPlan("select sum(group_by:(IntPrimitive, TheString),LongPrimitive) as c0 from SupportBean group by TheString, IntPrimitive");

	        // provide column count stays at 1
	        AssertCountColsAndLevels("select sum(group_by:(TheString),IntPrimitive) as c0, sum(group_by:(TheString),IntPrimitive) as c1 from SupportBean",
	                1, 1);

	        // prove order of group-by expressions does not matter
	        AssertCountColsAndLevels("select sum(group_by:(IntPrimitive, TheString),LongPrimitive) as c0, sum(LongPrimitive, group_by:(TheString, IntPrimitive)) as c1 from SupportBean",
	                1, 1);

	        // prove the number of levels stays the same even when group-by expressions vary
	        AssertCountColsAndLevels("select sum(group_by:(IntPrimitive, TheString),LongPrimitive) as c0, count(*, group_by:(TheString, IntPrimitive)) as c1 from SupportBean",
	                2, 1);

	        // prove there is one shared state factory
	        var theEpl = PLAN_CALLBACK_HOOK + "select window(*, group_by:TheString), last(*, group_by:TheString) from SupportBean.win:length(2)";
	        _epService.EPAdministrator.CreateEPL(theEpl);
	        Pair<AggregationGroupByLocalGroupDesc,AggregationLocalGroupByPlan> plan = SupportAggLevelPlanHook.GetAndReset();
	        Assert.AreEqual(1, plan.Second.AllLevels.Length);
	        Assert.AreEqual(1, plan.Second.AllLevels[0].StateFactories.Length);
	    }

        [Test]
	    public void TestFullyVersusNotFullyAgg() {
	        var colsC0 = "c0".Split(',');

	        // full-aggregated and un-grouped (row for all)
	        RunAssertionAggAndFullyAgg("select sum(group_by:(),IntPrimitive) as c0 from SupportBean",
                listener => EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), colsC0, new object[] {60}));

	        // aggregated and un-grouped (row for event)
	        RunAssertionAggAndFullyAgg("select sum(group_by:TheString, IntPrimitive) as c0 from SupportBean.win:keepall()",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), colsC0, new object[][] { new object[] { 10 }, new object[] { 50 }, new object[] { 50 } }));

	        // fully aggregated and grouped (row for group)
	        RunAssertionAggAndFullyAgg("select sum(IntPrimitive, group_by:()) as c0, sum(group_by:TheString, IntPrimitive) as c1, TheString " +
	                                   "from SupportBean group by TheString",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), "TheString,c0,c1".Split(','), new object[][] { new object[] { "E1", 60, 10 }, new object[] { "E2", 60, 50 } }));

	        // aggregated and grouped (row for event)
	        RunAssertionAggAndFullyAgg("select sum(LongPrimitive, group_by:()) as c0," +
	                                   " sum(LongPrimitive, group_by:TheString) as c1, " +
	                                   " sum(LongPrimitive, group_by:IntPrimitive) as c2, " +
	                                   " TheString " +
	                                   "from SupportBean.win:keepall() group by TheString",
	             listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(),
                                    "TheString,c0,c1,c2".Split(','), new object[][] { new object[] { "E1", 600L, 100L, 100L }, new object[] { "E2", 600L, 500L, 200L }, new object[] { "E2", 600L, 500L, 300L } }));
	    }

	    private void RunAssertionUngroupedRowRemove() {
	        var cols = "TheString,IntPrimitive,c0,c1".Split(',');
	        var epl = "create window MyWindow.win:keepall() as SupportBean;\n" +
	                     "insert into MyWindow select * from SupportBean;\n" +
	                     "on SupportBean_S0 delete from MyWindow where p00 = TheString and id = IntPrimitive;\n" +
	                     "on SupportBean_S1 delete from MyWindow;\n" +
	                     "@name('out') select TheString, IntPrimitive, sum(LongPrimitive) as c0, " +
	                     "  sum(LongPrimitive, group_by:TheString) as c1 from MyWindow;\n";
	        var result = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("out").AddListener(_listener);

	        MakeSendEvent("E1", 10, 101);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 10, 101L, 101L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1")); // delete event {"E1", 10}
	        Assert.IsFalse(_listener.IsInvoked);

	        MakeSendEvent("E1", 20, 102);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 20, 102L, 102L});

	        MakeSendEvent("E2", 30, 103);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E2", 30, 102+103L, 103L});

	        MakeSendEvent("E1", 40, 104);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 40, 102+103+104L, 102+104L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(40, "E1")); // delete event {"E1", 40}
	        Assert.IsFalse(_listener.IsInvoked);

	        MakeSendEvent("E1", 50, 105);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 50, 102+103+105L, 102+105L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(-1)); // delete all
	        Assert.IsFalse(_listener.IsInvoked);

	        MakeSendEvent("E1", 60, 106);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 60, 106L, 106L});

	        _epService.EPAdministrator.DeploymentAdmin.Undeploy(result.DeploymentId);
	    }

	    private void RunAssertionGroupedRowRemove() {
	        var cols = "TheString,IntPrimitive,c0,c1".Split(',');
	        var epl = "create window MyWindow.win:keepall() as SupportBean;\n" +
	                "insert into MyWindow select * from SupportBean;\n" +
	                "on SupportBean_S0 delete from MyWindow where p00 = TheString and id = IntPrimitive;\n" +
	                "on SupportBean_S1 delete from MyWindow;\n" +
	                "@name('out') select TheString, IntPrimitive, sum(LongPrimitive) as c0, " +
	                "  sum(LongPrimitive, group_by:TheString) as c1 " +
	                "  from MyWindow group by TheString, IntPrimitive;\n";
	        var result = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("out").AddListener(_listener);

	        MakeSendEvent("E1", 10, 101);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 10, 101L, 101L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1")); // delete event {"E1", 10}
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 10, null, null});

	        MakeSendEvent("E1", 20, 102);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 20, 102L, 102L});

	        MakeSendEvent("E2", 30, 103);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E2", 30, 103L, 103L});

	        MakeSendEvent("E1", 40, 104);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 40, 104L, 102+104L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(40, "E1")); // delete event {"E1", 40}
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 40, null, 102L});

	        MakeSendEvent("E1", 50, 105);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 50, 105L, 102+105L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(-1)); // delete all
	        _listener.Reset();

	        MakeSendEvent("E1", 60, 106);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {"E1", 60, 106L, 106L});

	        _epService.EPAdministrator.DeploymentAdmin.Undeploy(result.DeploymentId);
	    }

	    private void RunAssertionGroupedMultiLevelMethod() {
	        SendTime(0);
	        var fields = "TheString,IntPrimitive,c0,c1,c2,c3,c4".Split(',');
	        var epl = "select" +
	                "   TheString, IntPrimitive," +
	                "   sum(LongPrimitive, group_by:(IntPrimitive, TheString)) as c0," +
	                "   sum(LongPrimitive) as c1," +
	                "   sum(LongPrimitive, group_by:(TheString)) as c2," +
	                "   sum(LongPrimitive, group_by:(IntPrimitive)) as c3," +
	                "   sum(LongPrimitive, group_by:()) as c4" +
	                " from SupportBean" +
	                " group by TheString, IntPrimitive" +
	                " output snapshot every 10 seconds";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        MakeSendEvent("E1", 10, 100);
	        MakeSendEvent("E1", 20, 202);
	        MakeSendEvent("E2", 10, 303);
	        MakeSendEvent("E1", 10, 404);
	        MakeSendEvent("E2", 10, 505);
	        SendTime(10000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"E1", 10, 504L, 504L, 706L, 1312L, 1514L}, new object[] {"E1", 20, 202L, 202L, 706L, 202L, 1514L}, new object[] {"E2", 10, 808L, 808L, 808L, 1312L, 1514L}});

	        MakeSendEvent("E1", 10, 1);
	        SendTime(20000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"E1", 10, 505L, 505L, 707L, 1313L, 1515L}, new object[] {"E1", 20, 202L, 202L, 707L, 202L, 1515L}, new object[] {"E2", 10, 808L, 808L, 808L, 1313L, 1515L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionGroupedMultiLevelAccess() {
	        SendTime(0);
	        var fields = "TheString,IntPrimitive,c0,c1,c2,c3,c4".Split(',');
	        var epl = "select" +
	                "   TheString, IntPrimitive," +
	                "   window(*, group_by:(IntPrimitive, TheString)) as c0," +
	                "   window(*) as c1," +
	                "   window(*, group_by:TheString) as c2," +
	                "   window(*, group_by:IntPrimitive) as c3," +
	                "   window(*, group_by:()) as c4" +
	                " from SupportBean.win:keepall()" +
	                " group by TheString, IntPrimitive" +
	                " output snapshot every 10 seconds" +
	                " order by TheString, IntPrimitive";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        var b1 = MakeSendEvent("E1", 10, 100);
	        var b2 = MakeSendEvent("E1", 20, 202);
	        var b3 = MakeSendEvent("E2", 10, 303);
	        var b4 = MakeSendEvent("E1", 10, 404);
	        var b5 = MakeSendEvent("E2", 10, 505);
	        SendTime(10000);

	        var all = new object[]{b1, b2, b3, b4, b5};
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields,
	            new object[] {"E1", 10, new object[] {b1, b4}, new object[] {b1, b4},   new object[] {b1, b2, b4},
	            new object[] {b1, b3, b4, b5}, all});
	        EPAssertionUtil.AssertProps(_listener.LastNewData[1], fields,
	            new object[] {"E1", 20, new object[] {b2},     new object[] {b2},       new object[] {b1, b2, b4},
	            new object[] {b2}, all});
	        EPAssertionUtil.AssertProps(_listener.LastNewData[2], fields,
	            new object[] {"E2", 10, new object[] {b3, b5}, new object[] {b3, b5},   new object[] {b3, b5},
	            new object[] {b1, b3, b4, b5}, all});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionGroupedMultiLevelNoDefaultLvl() {
	        SendTime(0);
	        var fields = "TheString,IntPrimitive,c0,c1,c2".Split(',');
	        var epl = "select" +
	                "   TheString, IntPrimitive," +
	                "   sum(LongPrimitive, group_by:(TheString)) as c0," +
	                "   sum(LongPrimitive, group_by:(IntPrimitive)) as c1," +
	                "   sum(LongPrimitive, group_by:()) as c2" +
	                " from SupportBean" +
	                " group by TheString, IntPrimitive" +
	                " output snapshot every 10 seconds";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        MakeSendEvent("E1", 10, 100);
	        MakeSendEvent("E1", 20, 202);
	        MakeSendEvent("E2", 10, 303);
	        MakeSendEvent("E1", 10, 404);
	        MakeSendEvent("E2", 10, 505);
	        SendTime(10000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"E1", 10, 706L, 1312L, 1514L}, new object[] {"E1", 20, 706L, 202L, 1514L}, new object[] {"E2", 10, 808L, 1312L, 1514L}});

	        MakeSendEvent("E1", 10, 1);
	        SendTime(20000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"E1", 10, 707L, 1313L, 1515L}, new object[] {"E1", 20, 707L, 202L, 1515L}, new object[] {"E2", 10, 808L, 1313L, 1515L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionGroupedSolutionPattern() {
	        SendTime(0);
	        var fields = "TheString,pct".Split(',');
	        var epl = "select TheString, count(*) / count(*, group_by:()) as pct" +
	                " from SupportBean.win:time(30 sec)" +
	                " group by TheString" +
	                " output snapshot every 10 seconds";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        SendEventMany("A", "B", "C", "B", "B", "C");
	        SendTime(10000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"A", 1/6d}, new object[] {"B", 3/6d}, new object[] {"C", 2/6d}});

	        SendEventMany("A", "B", "B", "B", "B", "A");
	        SendTime(20000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"A", 3/12d}, new object[] {"B", 7/12d}, new object[] {"C", 2/12d}});

	        SendEventMany("C", "A", "A", "A", "B", "A");
	        SendTime(30000);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"A", 6/12d}, new object[] {"B", 5/12d}, new object[] {"C", 1/12d}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionAggAndFullyAgg(string selected, MyAssertion assertion) {
	        var epl = "create context StartS0EndS1 start SupportBean_S0 end SupportBean_S1;" +
	                     "@name('out') context StartS0EndS1 " +
	                      selected +
	                     " output snapshot when terminated;";
	        var deployed = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("out").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        MakeSendEvent("E1", 10, 100);
	        MakeSendEvent("E2", 20, 200);
	        MakeSendEvent("E2", 30, 300);
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));

	        assertion.Invoke(_listener);

	        // try an empty batch
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1));

	        _epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
	    }

	    private void RunAssertionUngroupedParenSODA(bool soda)
        {
	        var cols = "c0,c1,c2,c3,c4".Split(',');
	        var epl = "select LongPrimitive, " +
	                "sum(LongPrimitive) as c0, " +
	                "sum(group_by:(),LongPrimitive) as c1, " +
	                "sum(LongPrimitive,group_by:()) as c2, " +
	                "sum(LongPrimitive,group_by:TheString) as c3, " +
	                "sum(LongPrimitive,group_by:(TheString,IntPrimitive)) as c4" +
	                " from SupportBean";
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl).AddListener(_listener);

	        MakeSendEvent("E1", 1, 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {10L, 10L, 10L, 10L, 10L});

	        MakeSendEvent("E1", 2, 11);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {21L, 21L, 21L, 21L, 11L});

	        MakeSendEvent("E2", 1, 12);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {33L, 33L, 33L, 12L, 12L});

	        MakeSendEvent("E2", 2, 13);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {46L, 46L, 46L, 25L, 13L});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedAggAdditionalAndPlugin()
        {
	        _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("concatstring", typeof(MyConcatAggregationFunctionFactory).FullName);
	        var mfAggConfig = new ConfigurationPlugInAggregationMultiFunction(SupportAggMFFuncExtensions.GetFunctionNames(), typeof(SupportAggMFFactory).FullName);
	        _epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(mfAggConfig);

	        var cols = "c0,c1,c2,c3,c4,c5,c8,c9,c10,c11,c12,c13".Split(',');
	        var epl = "select IntPrimitive, " +
	                " countever(*, IntPrimitive>0, group_by:(TheString)) as c0," +
	                " countever(*, IntPrimitive>0, group_by:()) as c1," +
	                " countever(*, group_by:(TheString)) as c2," +
	                " countever(*, group_by:()) as c3," +
	                " concatstring(Integer.toString(IntPrimitive), group_by:(TheString)) as c4," +
	                " concatstring(Integer.toString(IntPrimitive), group_by:()) as c5," +
	                " sc(IntPrimitive, group_by:(TheString)) as c6," +
	                " sc(IntPrimitive, group_by:()) as c7," +
	                " leaving(group_by:(TheString)) as c8," +
	                " leaving(group_by:()) as c9," +
	                " rate(3, group_by:(TheString)) as c10," +
	                " rate(3, group_by:()) as c11," +
	                " nth(IntPrimitive, 1, group_by:(TheString)) as c12," +
	                " nth(IntPrimitive, 1, group_by:()) as c13" +
	                " from SupportBean as sb";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        MakeSendEvent("E1", 10);
	        AssertScalarColl(_listener.LastNewData[0], new int?[]{10}, new int?[]{10});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[]{1L, 1L, 1L, 1L, "10", "10", false, false,
	                null, null, null, null});

	        MakeSendEvent("E2", 20);
	        AssertScalarColl(_listener.LastNewData[0], new int?[]{20}, new int?[]{10, 20});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {1L, 2L, 1L, 2L, "20", "10 20", false, false,
	                null, null, null, 10});

	        MakeSendEvent("E1", -1);
	        AssertScalarColl(_listener.LastNewData[0], new int?[]{10, -1}, new int?[]{10, 20, -1});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {1L, 2L, 2L, 3L, "10 -1", "10 20 -1", false, false,
	                null, null, 10, 20});

	        MakeSendEvent("E2", 30);
	        AssertScalarColl(_listener.LastNewData[0], new int?[]{20, 30}, new int?[]{10, 20, -1, 30});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {2L, 3L, 2L, 4L, "20 30", "10 20 -1 30", false, false,
	                null, null, 20, -1});

	        // plug-in aggregation function can also take other parameters
	        _epService.EPAdministrator.CreateEPL("select sc(IntPrimitive, dummy:1)," +
	                "concatstring(Integer.toString(IntPrimitive), dummy2:(1,2,3)) from SupportBean");

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedAggEvent() {
	        var cols = "first0,first1,last0,last1,window0,window1,maxby0,maxby1,minby0,minby1,sorted0,sorted1,maxbyever0,maxbyever1,minbyever0,minbyever1,firstever0,firstever1,lastever0,lastever1".Split(',');
	        var epl = "select IntPrimitive as c0, " +
	                " first(sb, group_by:(TheString)) as first0," +
	                " first(sb, group_by:()) as first1," +
	                " last(sb, group_by:(TheString)) as last0," +
	                " last(sb, group_by:()) as last1," +
	                " window(sb, group_by:(TheString)) as window0," +
	                " window(sb, group_by:()) as window1," +
	                " maxby(IntPrimitive, group_by:(TheString)) as maxby0," +
	                " maxby(IntPrimitive, group_by:()) as maxby1," +
	                " minby(IntPrimitive, group_by:(TheString)) as minby0," +
	                " minby(IntPrimitive, group_by:()) as minby1," +
	                " sorted(IntPrimitive, group_by:(TheString)) as sorted0," +
	                " sorted(IntPrimitive, group_by:()) as sorted1," +
	                " maxbyever(IntPrimitive, group_by:(TheString)) as maxbyever0," +
	                " maxbyever(IntPrimitive, group_by:()) as maxbyever1," +
	                " minbyever(IntPrimitive, group_by:(TheString)) as minbyever0," +
	                " minbyever(IntPrimitive, group_by:()) as minbyever1," +
	                " firstever(sb, group_by:(TheString)) as firstever0," +
	                " firstever(sb, group_by:()) as firstever1," +
	                " lastever(sb, group_by:(TheString)) as lastever0," +
	                " lastever(sb, group_by:()) as lastever1" +
	                " from SupportBean.win:length(3) as sb";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        var b1 = MakeSendEvent("E1", 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[]{b1, b1, b1, b1, new object[]{b1}, new object[]{b1},
	                b1, b1, b1, b1, new object[]{b1}, new object[]{b1}, b1, b1, b1, b1,
	                b1, b1, b1, b1});

	        var b2 = MakeSendEvent("E2", 20);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[]{b2, b1, b2, b2, new object[]{b2}, new object[]{b1, b2},
	                b2, b2, b2, b1, new object[]{b2}, new object[]{b1, b2}, b2, b2, b2, b1,
	                b2, b1, b2, b2});

	        var b3 = MakeSendEvent("E1", 15);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[]{b1, b1, b3, b3, new object[]{b1, b3}, new object[]{b1, b2, b3},
	                b3, b2, b1, b1, new object[]{b1, b3}, new object[]{b1, b3, b2}, b3, b2, b1, b1,
	                b1, b1, b3, b3});

	        var b4 = MakeSendEvent("E3", 16);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[]{b4, b2, b4, b4, new object[]{b4}, new object[]{b2, b3, b4},
	                b4, b2, b4, b3, new object[]{b4}, new object[]{b3, b4, b2}, b4, b2, b4, b1,
	                b4, b1, b4, b4});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedAggSQLStandard() {
	        var fields = "c0,sum0,sum1,avedev0,avg0,max0,fmax0,min0,fmin0,maxever0,fmaxever0,minever0,fminever0,median0,stddev0".Split(',');
	        var epl = "select IntPrimitive as c0, " +
	                "sum(IntPrimitive, group_by:()) as sum0, " +
	                "sum(IntPrimitive, group_by:(TheString)) as sum1," +
	                "avedev(IntPrimitive, group_by:(TheString)) as avedev0," +
	                "avg(IntPrimitive, group_by:(TheString)) as avg0," +
	                "max(IntPrimitive, group_by:(TheString)) as max0," +
	                "fmax(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fmax0," +
	                "min(IntPrimitive, group_by:(TheString)) as min0," +
	                "fmin(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fmin0," +
	                "maxever(IntPrimitive, group_by:(TheString)) as maxever0," +
	                "fmaxever(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fmaxever0," +
	                "minever(IntPrimitive, group_by:(TheString)) as minever0," +
	                "fminever(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fminever0," +
	                "median(IntPrimitive, group_by:(TheString)) as median0," +
	                "Math.round(coalesce(stddev(IntPrimitive, group_by:(TheString)), 0)) as stddev0" +
	                " from SupportBean.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10, 10,
	                0.0d, 10d, 10, 10, 10, 10, 10, 10, 10, 10, 10.0, 0L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20, 10 + 20, 20,
	                0.0d, 20d, 20, 20, 20, 20, 20, 20, 20, 20, 20.0, 0L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{30, 10 + 20 + 30, 10 + 30,
	                10.0d, 20d, 30, 30, 10, 10, 30, 30, 10, 10, 20.0, 14L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 40));
	        var expected = new object[] {40, 10+20+30+40, 20+40,
	                10.0d, 30d, 40, 40, 20, 20, 40, 40, 20, 20, 30.0, 14L};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedSameKey() {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent (d1 String, d2 String, val int)");
	        var epl = "select sum(val, group_by: d1) as c0, sum(val, group_by: d2) as c1 from MyEvent";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);
	        var cols = "c0,c1".Split(',');

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "E1", 10}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {10, 10});

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "E2", 11}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {21, 11});

	        _epService.EPRuntime.SendEvent(new object[] {"E2", "E1", 12}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {12, 22});

	        _epService.EPRuntime.SendEvent(new object[] {"E3", "E1", 13}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {13, 35});

	        _epService.EPRuntime.SendEvent(new object[] {"E3", "E3", 14}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {27, 14});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionGroupedSameKey() {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent (g1 String, d1 String, d2 String, val int)");
	        var epl = "select sum(val) as c0, sum(val, group_by: d1) as c1, sum(val, group_by: d2) as c2 from MyEvent group by g1";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);
	        var cols = "c0,c1,c2".Split(',');

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "E1", "E1", 10}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {10, 10, 10});

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "E1", "E2", 11}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {21, 21, 11});

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "E2", "E1", 12}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {33, 12, 22});

	        _epService.EPRuntime.SendEvent(new object[] {"X", "E1", "E1", 13}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {13, 10+11+13, 10+12+13});

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "E2", "E3", 14}, "MyEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), cols, new object[] {10+11+12+14, 12+14, 14});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedAggIterator() {
	        var fields = "c0,sum0,sum1".Split(',');
	        var epl = "select IntPrimitive as c0, " +
	                "sum(IntPrimitive, group_by:()) as sum0, " +
	                "sum(IntPrimitive, group_by:(TheString)) as sum1 " +
	                " from SupportBean.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {10, 10, 10}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] { 10, 30, 10 }, new object[] { 20, 30, 20 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] { 10, 60, 40 }, new object[] { 20, 60, 20 }, new object[] { 30, 60, 40 } });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedHaving()
        {
	        var epl = "select * from SupportBean having sum(IntPrimitive, group_by:TheString) > 100";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        MakeSendEvent("E1", 95);
	        MakeSendEvent("E2", 10);
	        Assert.IsFalse(_listener.IsInvoked);

	        MakeSendEvent("E1", 10);
	        Assert.IsTrue(_listener.IsInvoked);
	        _listener.Reset();

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedOrderBy()
        {
	        var epl = "create context StartS0EndS1 start SupportBean_S0 end SupportBean_S1;" +
	                "@name('out') context StartS0EndS1 select TheString, sum(IntPrimitive, group_by:TheString) as c0 " +
	                " from SupportBean.win:keepall() " +
	                " output snapshot when terminated" +
	                " order by sum(IntPrimitive, group_by:TheString)" +
	                ";";
	        var deployed = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("out").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        MakeSendEvent("E1", 10);
	        MakeSendEvent("E2", 20);
	        MakeSendEvent("E1", 30);
	        MakeSendEvent("E3", 40);
	        MakeSendEvent("E2", 50);
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));

	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString,c0".Split(','), new object[][] {
	                new object[] {"E1", 40}, new object[] {"E1", 40}, new object[] {"E3", 40}, new object[] {"E2", 70}, new object[] {"E2", 70}});

	        // try an empty batch
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1));

	        _epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
	    }

	    private void RunAssertionGroupedOnSelect() {
	        var epl = "create window MyWindow.win:keepall() as SupportBean;" +
	                "insert into MyWindow select * from SupportBean;" +
	                "@name('out') on SupportBean_S0 select TheString, sum(IntPrimitive) as c0, sum(IntPrimitive, group_by:()) as c1" +
	                " from MyWindow group by TheString;";
	        var deployed = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
	        _epService.EPAdministrator.GetStatement("out").AddListener(_listener);

	        MakeSendEvent("E1", 10);
	        MakeSendEvent("E2", 20);
	        MakeSendEvent("E1", 30);
	        MakeSendEvent("E3", 40);
	        MakeSendEvent("E2", 50);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "TheString,c0,c1".Split(','), new object[][]{
	                new object[] {"E1", 40, 150}, new object[] {"E2", 70, 150}, new object[] {"E3", 40, 150}});

	        MakeSendEvent("E1", 60);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "TheString,c0,c1".Split(','), new object[][]{
	                new object[] {"E1", 100, 210}, new object[] {"E2", 70, 210}, new object[] {"E3", 40, 210}});

	        _epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
	    }

	    private void RunAssertionUngroupedUnidirectionalJoin() {
	        var epl = "select TheString, sum(IntPrimitive, group_by:TheString) as c0 from SupportBean.win:keepall(), SupportBean_S0 unidirectional";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        MakeSendEvent("E1", 10);
	        MakeSendEvent("E2", 20);
	        MakeSendEvent("E1", 30);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "TheString,c0".Split(','),
                    new object[][] { new object[] { "E1", 40 }, new object[] { "E1", 40 }, new object[] { "E2", 20 } });

	        MakeSendEvent("E1", 40);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "TheString,c0".Split(','),
                    new object[][] { new object[] { "E1", 80 }, new object[] { "E1", 80 }, new object[] { "E1", 80 }, new object[] { "E2", 20 } });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionEnumMethods(bool grouped) {
	        var epl =
	                "select" +
	                " window(*, group_by:()).firstOf() as c0," +
	                " window(*, group_by:TheString).firstOf() as c1," +
	                " window(IntPrimitive, group_by:()).firstOf() as c2," +
	                " window(IntPrimitive, group_by:TheString).firstOf() as c3," +
	                " first(*, group_by:()).IntPrimitive as c4," +
	                " first(*, group_by:TheString).IntPrimitive as c5 " +
	                " from SupportBean.win:keepall()" +
	                (grouped ? "group by TheString, IntPrimitive" : "");
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        var b1 = MakeSendEvent("E1", 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4,c5".Split(','),
	                new object[] {b1, b1, 10, 10, 10, 10});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void SendTime(long msec) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
	    }

	    private void SendEventMany(params string[] theString) {
	        foreach (string value in theString) {
	            SendEvent(value);
	        }
	    }

	    private void SendEvent(string theString) {
	        _epService.EPRuntime.SendEvent(new SupportBean(theString, 0));
	    }

	    private SupportBean MakeSendEvent(string theString, int intPrimitive) {
	        var b = new SupportBean(theString, intPrimitive);
	        _epService.EPRuntime.SendEvent(b);
	        return b;
	    }

	    private SupportBean MakeSendEvent(string theString, int intPrimitive, long longPrimitive) {
	        var b = new SupportBean(theString, intPrimitive);
	        b.LongPrimitive = longPrimitive;
	        _epService.EPRuntime.SendEvent(b);
	        return b;
	    }

        private delegate void MyAssertion(SupportUpdateListener listener);

	    private void AssertCountColsAndLevels(string epl, int colCount, int lvlCount) {
	        var theEpl = PLAN_CALLBACK_HOOK + epl;
	        _epService.EPAdministrator.CreateEPL(theEpl);
	        Pair<AggregationGroupByLocalGroupDesc,AggregationLocalGroupByPlan> plan = SupportAggLevelPlanHook.GetAndReset();
	        Assert.AreEqual(colCount, plan.First.NumColumns);
	        Assert.AreEqual(lvlCount, plan.First.Levels.Length);
	    }

	    private void AssertNoPlan(string epl) {
	        var theEpl = PLAN_CALLBACK_HOOK + epl;
	        _epService.EPAdministrator.CreateEPL(theEpl);
	        Assert.IsNull(SupportAggLevelPlanHook.GetAndReset());
	    }

	    private void RunAssertionColNameRendering() {
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
	                "count(*, group_by:(TheString, IntPrimitive)), " +
	                "count(group_by:TheString, *) " +
	                "from SupportBean");
	        Assert.AreEqual("count(*,group_by:(TheString,IntPrimitive))", stmt.EventType.PropertyNames[0]);
	        Assert.AreEqual("count(group_by:TheString,*)", stmt.EventType.PropertyNames[1]);
	    }

	    private void AssertScalarColl(EventBean eventBean, int?[] expectedC6, int?[] expectedC7)
	    {
	        var c6 = eventBean.Get("c6").UnwrapIntoArray<int?>();
	        var c7 = eventBean.Get("c7").UnwrapIntoArray<int?>();
	        EPAssertionUtil.AssertEqualsExactOrder(expectedC6, c6);
	        EPAssertionUtil.AssertEqualsExactOrder(expectedC7, c7);
	    }
	}
} // end of namespace
