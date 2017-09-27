///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestSubselectAggregatedMultirowAndColumn 
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportBean", typeof(SupportBean));
	        config.AddEventType("S0", typeof(SupportBean_S0));
	        config.AddEventType("S1", typeof(SupportBean_S1));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestAggregatedMulticolumn() {
	        RunAssertionMultirowGroupedNoDataWindowUncorrelated();
	        RunAssertionMultirowGroupedNamedWindowSubqueryIndexShared();
	        RunAssertionMultirowGroupedUncorrelatedIteratorAndExpressionDef();
	        RunAssertionMultirowGroupedCorrelatedWithEnumMethod();
	        RunAssertionMultirowGroupedUncorrelatedWithEnumerationMethod();
	        RunAssertionMultirowGroupedCorrelatedWHaving();

	        RunAssertionMulticolumnGroupedUncorrelatedUnfiltered();
	        RunAssertionMulticolumnGroupedContextPartitioned();
	        RunAssertionMulticolumnGroupedWHaving();
	    }

        [Test]
	    public void TestInvalid() {
	        string epl;

	        // not fully aggregated
	        epl = "select (select theString, sum(longPrimitive) from SupportBean#keepall group by intPrimitive) from S0";
	        SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires non-aggregated properties in the select-clause to also appear in the group-by clause [select (select theString, sum(longPrimitive) from SupportBean#keepall group by intPrimitive) from S0]");

	        // correlated group-by not allowed
	        epl = "select (select theString, sum(longPrimitive) from SupportBean#keepall group by theString, s0.id) from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (property 'id' is not) [select (select theString, sum(longPrimitive) from SupportBean#keepall group by theString, s0.id) from S0 as s0]");
	        epl = "select (select theString, sum(longPrimitive) from SupportBean#keepall group by theString, s0.get_P00()) from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (expression 's0.get_P00()' against stream 1 is not)");

	        // aggregations not allowed in group-by
	        epl = "select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by sum(intPrimitive)) from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have an aggregation function [select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by sum(intPrimitive)) from S0 as s0]");

	        // "prev" not allowed in group-by
	        epl = "select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by prev(1, intPrimitive)) from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have a function that requires view resources (prior, prev) [select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by prev(1, intPrimitive)) from S0 as s0]");
	    }

	    private void RunAssertionMulticolumnGroupedWHaving() {
	        var fields = "c0,c1".SplitCsv();
	        var epl = "select (select theString as c0, sum(intPrimitive) as c1 from SupportBean#keepall group by theString having sum(intPrimitive) > 10) as subq from S0";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendSBEventAndTrigger("E1", 10);
	        AssertMapFieldAndReset("subq", _listener, fields, null);

	        SendSBEventAndTrigger("E2", 5);
	        AssertMapFieldAndReset("subq", _listener, fields, null);

	        SendSBEventAndTrigger("E2", 6);
	        AssertMapFieldAndReset("subq", _listener, fields, new object[] {"E2", 11});

	        SendSBEventAndTrigger("E1", 1);
	        AssertMapFieldAndReset("subq", _listener, fields, null);
	    }

	    private void RunAssertionMulticolumnGroupedContextPartitioned() {
	        var fieldName = "subq";
	        var fields = "c0,c1".SplitCsv();

	        _epService.EPAdministrator.CreateEPL(
	            "create context MyCtx partition by theString from SupportBean, p00 from S0");

	        var stmtText = "context MyCtx select " +
	                          "(select theString as c0, sum(intPrimitive) as c1 " +
	                          " from SupportBean#keepall " +
	                          " group by theString) as subq " +
	                          "from S0 as s0";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P1"));
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"P1", 100});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "P2"));
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("P2", 200));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "P2"));
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"P2", 200});

	        _epService.EPRuntime.SendEvent(new SupportBean("P2", 205));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "P2"));
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"P2", 405});

	        stmt.Dispose();
	    }

	    private void RunAssertionMulticolumnGroupedUncorrelatedUnfiltered() {
	        var fieldName = "subq";
	        var fields = "c0,c1".SplitCsv();
	        var eplNoDelete = "select " +
	                             "(select theString as c0, sum(intPrimitive) as c1 " +
	                             "from SupportBean#keepall " +
	                             "group by theString) as subq " +
	                             "from S0 as s0";
	        var stmtNoDelete = _epService.EPAdministrator.CreateEPL(eplNoDelete);
	        stmtNoDelete.AddListener(_listener);
	        RunAssertionNoDelete(fieldName, fields);
	        stmtNoDelete.Dispose();

	        // try SODA
	        var model = _epService.EPAdministrator.CompileEPL(eplNoDelete);
	        Assert.AreEqual(eplNoDelete, model.ToEPL());
	        stmtNoDelete = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(stmtNoDelete.Text, eplNoDelete);
	        stmtNoDelete.AddListener(_listener);
	        RunAssertionNoDelete(fieldName, fields);
	        stmtNoDelete.Dispose();

	        // test named window with delete/remove
	        _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on S1 delete from MyWindow where id = intPrimitive");
	        var stmtDelete = _epService.EPAdministrator.CreateEPL("@Hint('disable_reclaim_group') select (select theString as c0, sum(intPrimitive) as c1 " +
	                                 " from MyWindow group by theString) as subq from S0 as s0");
	        stmtDelete.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);

	        SendSBEventAndTrigger("E1", 10);
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E1", 10});

	        SendS1EventAndTrigger(10);     // delete 10
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);

	        SendSBEventAndTrigger("E2", 20);
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E2", 20});

	        SendSBEventAndTrigger("E2", 21);
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E2", 41});

	        SendSBEventAndTrigger("E1", 30);
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);

	        SendS1EventAndTrigger(30);     // delete 30
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E2", 41});

	        SendS1EventAndTrigger(20);     // delete 20
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E2", 21});

	        SendSBEventAndTrigger("E1", 31);    // two groups
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);

	        SendS1EventAndTrigger(21);     // delete 21
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E1", 31});
	        stmtDelete.Dispose();

	        // test multiple group-by criteria
	        var fieldsMultiGroup = "c0,c1,c2,c3,c4".SplitCsv();
	        var eplMultiGroup = "select " +
	                               "(select theString as c0, intPrimitive as c1, theString||'x' as c2, " +
	                               "    intPrimitive * 1000 as c3, sum(longPrimitive) as c4 " +
	                               " from SupportBean#keepall " +
	                               " group by theString, intPrimitive) as subq " +
	                               "from S0 as s0";
	        var stmtMultiGroup = _epService.EPAdministrator.CreateEPL(eplMultiGroup);
	        stmtMultiGroup.AddListener(_listener);

	        SendSBEventAndTrigger("G1", 1, 100L);
	        AssertMapFieldAndReset(fieldName, _listener, fieldsMultiGroup, new object[] {"G1", 1, "G1x", 1000, 100L});

	        SendSBEventAndTrigger("G1", 1, 101L);
	        AssertMapFieldAndReset(fieldName, _listener, fieldsMultiGroup, new object[] {"G1", 1, "G1x", 1000, 201L});

	        SendSBEventAndTrigger("G2", 1, 200L);
	        AssertMapFieldAndReset(fieldName, _listener, fieldsMultiGroup, null);

	        stmtMultiGroup.Dispose();
	    }

	    private void RunAssertionMultirowGroupedCorrelatedWHaving() {
	        var fieldName = "subq";
	        var fields = "c0,c1".SplitCsv();

	        var eplEnumCorrelated = "select " +
	                                   "(select theString as c0, sum(intPrimitive) as c1 " +
	                                   " from SupportBean#keepall " +
	                                   " where intPrimitive = s0.id " +
	                                   " group by theString" +
	                                   " having sum(intPrimitive) > 10).take(100) as subq " +
	                                   "from S0 as s0";
	        var stmtEnumUnfiltered = _epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
	        stmtEnumUnfiltered.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E2", 20}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 20}, new object[]{"E2", 20}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 55));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 20}, new object[]{"E2", 20}});

	        stmtEnumUnfiltered.Dispose();
	    }

	    private void RunAssertionMultirowGroupedCorrelatedWithEnumMethod() {
	        var fieldName = "subq";
	        var fields = "c0,c1".SplitCsv();

	        var eplEnumCorrelated = "select " +
	                                   "(select theString as c0, sum(intPrimitive) as c1 " +
	                                   " from SupportBean#keepall " +
	                                   " where intPrimitive = s0.id " +
	                                   " group by theString).take(100) as subq " +
	                                   "from S0 as s0";
	        var stmtEnumUnfiltered = _epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
	        stmtEnumUnfiltered.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 10}});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(11));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 20}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(100));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E2", 100}});

	        stmtEnumUnfiltered.Dispose();
	    }

	    private void RunAssertionNoDelete(string fieldName, string[] fields) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);

	        SendSBEventAndTrigger("E1", 10);
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E1", 10});

	        SendSBEventAndTrigger("E1", 20);
	        AssertMapFieldAndReset(fieldName, _listener, fields, new object[] {"E1", 30});

	        // second group - this returns null as subquerys cannot return multiple rows (unless enumerated) (sql standard)
	        SendSBEventAndTrigger("E2", 5);
	        AssertMapFieldAndReset(fieldName, _listener, fields, null);
	    }

	    private void RunAssertionMultirowGroupedNamedWindowSubqueryIndexShared() {
	        // test uncorrelated
	        _epService.EPAdministrator.CreateEPL("@Hint('enable_window_subquery_indexshare')" +
	                "create window SBWindow#keepall as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into SBWindow select * from SupportBean");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 20));

	        var stmtUncorrelated = _epService.EPAdministrator.CreateEPL("select " +
	                                       "(select theString as c0, sum(intPrimitive) as c1 from SBWindow group by theString).take(10) as e1 from S0");
	        stmtUncorrelated.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapMultiRow("e1", _listener.AssertOneGetNewAndReset(), "c0", "c0,c1".SplitCsv(), new object[][] {new object[]{"E1", 30}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
	        AssertMapMultiRow("e1", _listener.AssertOneGetNewAndReset(), "c0", "c0,c1".SplitCsv(), new object[][] {new object[]{"E1", 30}, new object[]{"E2", 200}});
	        stmtUncorrelated.Dispose();

	        // test correlated
	        var stmtCorrelated = _epService.EPAdministrator.CreateEPL("select " +
	                                     "(select theString as c0, sum(intPrimitive) as c1 from SBWindow where theString = s0.p00 group by theString).take(10) as e1 from S0 as s0");
	        stmtCorrelated.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
	        AssertMapMultiRow("e1", _listener.AssertOneGetNewAndReset(), "c0", "c0,c1".SplitCsv(), new object[][] {new object[]{"E1", 30}});

	        stmtCorrelated.Dispose();
	    }

	    private void RunAssertionMultirowGroupedNoDataWindowUncorrelated() {
	        var stmtText = "select (select theString as c0, sum(intPrimitive) as c1 from SupportBean group by theString).take(10) as subq from S0";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        var fields = "c0,c1".SplitCsv();

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
	        TestSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", _listener.AssertOneGetNewAndReset(), "c0", fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
	        TestSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", _listener.AssertOneGetNewAndReset(), "c0", fields, new object[][] {new object[]{"G1", 10}});

	        _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
	        TestSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", _listener.AssertOneGetNewAndReset(), "c0", fields, new object[][] {new object[]{"G1", 10}, new object[]{"G2", 20}});

	        stmt.Dispose();
	    }

	    private void RunAssertionMultirowGroupedUncorrelatedIteratorAndExpressionDef() {
	        var fields = "c0,c1".SplitCsv();
	        var epl = "expression getGroups {" +
	                     "(select theString as c0, sum(intPrimitive) as c1 " +
	                     "  from SupportBean#keepall group by theString)" +
	                     "}" +
	                     "select getGroups() as e1, getGroups().take(10) as e2 from S0#lastevent()";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendSBEventAndTrigger("E1", 20);
	        foreach (var @event in new EventBean[] {_listener.AssertOneGetNew(), stmt.First()}) {
	            AssertMapField("e1", @event, fields, new object[] {"E1", 20});
	            AssertMapMultiRow("e2", @event, "c0", fields, new object[][] {new object[]{"E1", 20}});
	        }
	        _listener.Reset();

	        SendSBEventAndTrigger("E2", 30);
	        foreach (var @event in new EventBean[] {_listener.AssertOneGetNew(), stmt.First()}) {
	            AssertMapField("e1", @event, fields, null);
	            AssertMapMultiRow("e2", @event, "c0", fields, new object[][] {new object[]{"E1", 20}, new object[]{"E2", 30}});
	        }
	        _listener.Reset();

	        stmt.Dispose();
	    }

	    private void RunAssertionMultirowGroupedUncorrelatedWithEnumerationMethod() {
	        var fieldName = "subq";
	        var fields = "c0,c1".SplitCsv();

	        // test unfiltered
	        var eplEnumUnfiltered = "select " +
	                                   "(select theString as c0, sum(intPrimitive) as c1 " +
	                                   " from SupportBean#keepall " +
	                                   " group by theString).take(100) as subq " +
	                                   "from S0 as s0";
	        var stmtEnumUnfiltered = _epService.EPAdministrator.CreateEPL(eplEnumUnfiltered);
	        stmtEnumUnfiltered.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        SendSBEventAndTrigger("E1", 10);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 10}});

	        SendSBEventAndTrigger("E1", 20);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 30}});

	        SendSBEventAndTrigger("E2", 100);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 30}, new object[]{"E2", 100}});

	        SendSBEventAndTrigger("E3", 2000);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 30}, new object[]{"E2", 100}, new object[]{"E3", 2000}});
	        stmtEnumUnfiltered.Dispose();

	        // test filtered
	        var eplEnumFiltered = "select " +
	                                 "(select theString as c0, sum(intPrimitive) as c1 " +
	                                 " from SupportBean#keepall " +
	                                 " where intPrimitive > 100 " +
	                                 " group by theString).take(100) as subq " +
	                                 "from S0 as s0";
	        var stmtEnumFiltered = _epService.EPAdministrator.CreateEPL(eplEnumFiltered);
	        stmtEnumFiltered.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        SendSBEventAndTrigger("E1", 10);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, null);

	        SendSBEventAndTrigger("E1", 200);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 200}});

	        SendSBEventAndTrigger("E1", 11);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 200}});

	        SendSBEventAndTrigger("E1", 201);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 401}});

	        SendSBEventAndTrigger("E2", 300);
	        AssertMapMultiRowAndReset(fieldName, _listener, "c0", fields, new object[][] {new object[]{"E1", 401}, new object[]{"E2", 300}});

	        stmtEnumFiltered.Dispose();
	    }

	    private void SendSBEventAndTrigger(string theString, int intPrimitive) {
	        SendSBEventAndTrigger(theString, intPrimitive, 0);
	    }

	    private void SendSBEventAndTrigger(string theString, int intPrimitive, long longPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	    }

	    private void SendS1EventAndTrigger(int id) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(id, "x"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	    }

	    private void AssertMapFieldAndReset(string fieldName, SupportUpdateListener listener, string[] names, object[] values) {
	        AssertMapField(fieldName, listener.AssertOneGetNew(), names, values);
	        listener.Reset();
	    }

	    private void AssertMapMultiRowAndReset(string fieldName, SupportUpdateListener listener, string sortKey, string[] names, object[][] values) {
	        AssertMapMultiRow(fieldName, listener.AssertOneGetNew(), sortKey, names, values);
	        listener.Reset();
	    }

	    private void AssertMapField(string fieldName, EventBean @event, string[] names, object[] values) {
	        var subq = (IDictionary<string, object>) @event.Get(fieldName);
	        if (values == null && subq == null) {
	            return;
	        }
	        EPAssertionUtil.AssertPropsMap(subq, names, values);
	    }

	    protected static void AssertMapMultiRow(string fieldName, EventBean @event, string sortKey, string[] names, object[][] values)
	    {
	        var subq = @event.Get(fieldName).UnwrapIntoList<IDictionary<string, object>>();
	        if (values == null && subq == null) {
	            return;
	        }
	        var maps = subq.ToArray();
	        maps.SortInPlace((o1, o2) => ((IComparable) o1.Get(sortKey)).CompareTo(o2.Get(sortKey)));
	        EPAssertionUtil.AssertPropsPerRow(maps, names, values);
	    }
	}
} // end of namespace
