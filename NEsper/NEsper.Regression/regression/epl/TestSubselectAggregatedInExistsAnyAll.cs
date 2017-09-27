///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestSubselectAggregatedInExistsAnyAll
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
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
	    public void TestAggregatedInExistsAnyAll() {
	        foreach (Type clazz in Collections.List(typeof(SupportBean), typeof(SupportValueEvent), typeof(SupportIdAndValueEvent))) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }
	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));

	        RunAssertionInSimple();
	        RunAssertionExistsSimple();

	        RunAssertionUngroupedWOHavingWRelOpAllAnySome();
	        RunAssertionUngroupedWOHavingWEqualsAllAnySome();
	        RunAssertionUngroupedWOHavingWIn();
	        RunAssertionUngroupedWOHavingWExists();

	        RunAssertionUngroupedWHavingWRelOpAllAnySome();
	        RunAssertionUngroupedWHavingWEqualsAllAnySome();
	        RunAssertionUngroupedWHavingWIn();
	        RunAssertionUngroupedWHavingWExists();

	        RunAssertionGroupedWOHavingWRelOpAllAnySome();
	        RunAssertionGroupedWOHavingWEqualsAllAnySome();
	        RunAssertionGroupedWOHavingWIn();
	        RunAssertionGroupedWOHavingWExists();

	        RunAssertionGroupedWHavingWRelOpAllAnySome();
	        RunAssertionGroupedWHavingWEqualsAllAnySome();
	        RunAssertionGroupedWHavingWIn();
	        RunAssertionGroupedWHavingWExists();
	    }

	    private void RunAssertionUngroupedWHavingWIn() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select value in (select sum(intPrimitive) from SupportBean#keepall having last(theString) != 'E1') as c0," +
	                     "value not in (select sum(intPrimitive) from SupportBean#keepall having last(theString) != 'E1') as c1 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        SendVEAndAssert(fields, 10, new object[] {true, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
	        SendVEAndAssert(fields, 10, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", -1));
	        SendVEAndAssert(fields, 10, new object[] {true, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWHavingWIn() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select value in (select sum(intPrimitive) from SupportBean#keepall group by theString having last(theString) != 'E1') as c0," +
	                     "value not in (select sum(intPrimitive) from SupportBean#keepall group by theString having last(theString) != 'E1') as c1 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        SendVEAndAssert(fields, 10, new object[] {true, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWOHavingWIn() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select value in (select sum(intPrimitive) from SupportBean#keepall group by theString) as c0," +
	                     "value not in (select sum(intPrimitive) from SupportBean#keepall group by theString) as c1 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        SendVEAndAssert(fields, 10, new object[] {false, true});
	        SendVEAndAssert(fields, 11, new object[] {true, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedWOHavingWIn() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select value in (select sum(intPrimitive) from SupportBean#keepall) as c0," +
	                     "value not in (select sum(intPrimitive) from SupportBean#keepall) as c1 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {true, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        SendVEAndAssert(fields, 10, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", -1));
	        SendVEAndAssert(fields, 10, new object[] {true, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWOHavingWRelOpAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value < all (select sum(intPrimitive) from SupportBean#keepall group by theString) as c0, " +
	                     "value < any (select sum(intPrimitive) from SupportBean#keepall group by theString) as c1, " +
	                     "value < some (select sum(intPrimitive) from SupportBean#keepall group by theString) as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {true, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
	        SendVEAndAssert(fields, 10, new object[] {false, true, true});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWHavingWRelOpAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value < all (select sum(intPrimitive) from SupportBean#keepall group by theString having last(theString) not in ('E1', 'E3')) as c0, " +
	                     "value < any (select sum(intPrimitive) from SupportBean#keepall group by theString having last(theString) not in ('E1', 'E3')) as c1, " +
	                     "value < some (select sum(intPrimitive) from SupportBean#keepall group by theString having last(theString) not in ('E1', 'E3')) as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {true, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
	        SendVEAndAssert(fields, 10, new object[] {true, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 9));
	        SendVEAndAssert(fields, 10, new object[] {false, true, true});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWOHavingWEqualsAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value = all (select sum(intPrimitive) from SupportBean#keepall group by theString) as c0, " +
	                     "value = any (select sum(intPrimitive) from SupportBean#keepall group by theString) as c1, " +
	                     "value = some (select sum(intPrimitive) from SupportBean#keepall group by theString) as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {true, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        SendVEAndAssert(fields, 10, new object[] {false, true, true});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedWOHavingWEqualsAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value = all (select sum(intPrimitive) from SupportBean#keepall) as c0, " +
	                     "value = any (select sum(intPrimitive) from SupportBean#keepall) as c1, " +
	                     "value = some (select sum(intPrimitive) from SupportBean#keepall) as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        SendVEAndAssert(fields, 10, new object[] {false, false, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedWHavingWEqualsAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value = all (select sum(intPrimitive) from SupportBean#keepall having last(theString) != 'E1') as c0, " +
	                     "value = any (select sum(intPrimitive) from SupportBean#keepall having last(theString) != 'E1') as c1, " +
	                     "value = some (select sum(intPrimitive) from SupportBean#keepall having last(theString) != 'E1') as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
	        SendVEAndAssert(fields, 10, new object[] {false, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWHavingWEqualsAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value = all (select sum(intPrimitive) from SupportBean#keepall group by theString having first(theString) != 'E1') as c0, " +
	                     "value = any (select sum(intPrimitive) from SupportBean#keepall group by theString having first(theString) != 'E1') as c1, " +
	                     "value = some (select sum(intPrimitive) from SupportBean#keepall group by theString having first(theString) != 'E1') as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {true, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        SendVEAndAssert(fields, 10, new object[] {true, false, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
	        SendVEAndAssert(fields, 10, new object[] {false, true, true});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedWHavingWExists() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select exists (select sum(intPrimitive) from SupportBean having sum(intPrimitive) < 15) as c0," +
	                     "not exists (select sum(intPrimitive) from SupportBean  having sum(intPrimitive) < 15) as c1 from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        SendVEAndAssert(fields, new object[] {true, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
	        SendVEAndAssert(fields, new object[] {false, true});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedWOHavingWExists() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select exists (select sum(intPrimitive) from SupportBean) as c0," +
	                     "not exists (select sum(intPrimitive) from SupportBean) as c1 from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, new object[] {true, false});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        SendVEAndAssert(fields, new object[] {true, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedWOHavingWExists() {
	        EPStatement stmtNamedWindow = _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (key string, anint int)");
	        EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyWindow(key, anint) select id, value from SupportIdAndValueEvent");

	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select exists (select sum(anint) from MyWindow group by key) as c0," +
	                     "not exists (select sum(anint) from MyWindow group by key) as c1 from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportIdAndValueEvent("E1", 19));
	        SendVEAndAssert(fields, new object[] {true, false});

	        _epService.EPRuntime.ExecuteQuery("delete from MyWindow");

	        SendVEAndAssert(fields, new object[] {false, true});

	        stmt.Dispose();
	        stmtNamedWindow.Dispose();
            stmtInsert.Dispose();
	    }

	    private void RunAssertionGroupedWHavingWExists() {
	        EPStatement stmtNamedWindow = _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (key string, anint int)");
	        EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyWindow(key, anint) select id, value from SupportIdAndValueEvent");

	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select exists (select sum(anint) from MyWindow group by key having sum(anint) < 15) as c0," +
	                     "not exists (select sum(anint) from MyWindow group by key having sum(anint) < 15) as c1 from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportIdAndValueEvent("E1", 19));
	        SendVEAndAssert(fields, new object[] {false, true});

	        _epService.EPRuntime.SendEvent(new SupportIdAndValueEvent("E2", 12));
	        SendVEAndAssert(fields, new object[] {true, false});

	        _epService.EPRuntime.ExecuteQuery("delete from MyWindow");

	        SendVEAndAssert(fields, new object[] {false, true});

	        stmt.Dispose();
            stmtNamedWindow.Dispose();
            stmtInsert.Dispose();
	    }

	    private void RunAssertionUngroupedWHavingWRelOpAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value < all (select sum(intPrimitive) from SupportBean#keepall having last(theString) not in ('E1', 'E3')) as c0, " +
	                     "value < any (select sum(intPrimitive) from SupportBean#keepall having last(theString) not in ('E1', 'E3')) as c1, " +
	                     "value < some (select sum(intPrimitive) from SupportBean#keepall having last(theString) not in ('E1', 'E3')) as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", -1000));
	        SendVEAndAssert(fields, 10, new object[] {false, false, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedWOHavingWRelOpAllAnySome() {
	        string[] fields = "c0,c1,c2".SplitCsv();
	        string epl = "select " +
	                     "value < all (select sum(intPrimitive) from SupportBean#keepall) as c0, " +
	                     "value < any (select sum(intPrimitive) from SupportBean#keepall) as c1, " +
	                     "value < some (select sum(intPrimitive) from SupportBean#keepall) as c2 " +
	                     "from SupportValueEvent";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendVEAndAssert(fields, 10, new object[] {null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        SendVEAndAssert(fields, 10, new object[] {true, true, true});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", -1000));
	        SendVEAndAssert(fields, 10, new object[] {false, false, false});

	        stmt.Dispose();
	    }

	    private void RunAssertionExistsSimple() {
	        string stmtText = "select id from S0 where exists (select max(id) from S1#length(3))";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendEventS0(1);
	        Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("id"));

	        SendEventS1(100);
	        SendEventS0(2);
	        Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("id"));
	    }

	    private void RunAssertionInSimple() {
	        string stmtText = "select id from S0 where id in (select max(id) from S1#length(2))";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendEventS0(1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEventS1(100);
	        SendEventS0(2);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEventS0(100);
	        Assert.AreEqual(100, _listener.AssertOneGetNewAndReset().Get("id"));

	        SendEventS0(200);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEventS1(-1);
	        SendEventS1(-1);
	        SendEventS0(-1);
	        Assert.AreEqual(-1, _listener.AssertOneGetNewAndReset().Get("id"));
	    }

	    private void SendVEAndAssert(string[] fields, int value, object[] expected) {
	        _epService.EPRuntime.SendEvent(new SupportValueEvent(value));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
	    }

	    private void SendVEAndAssert(string[] fields, object[] expected) {
	        _epService.EPRuntime.SendEvent(new SupportValueEvent(-1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
	    }

	    private void SendEventS0(int id) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(id));
	    }

	    private void SendEventS1(int id) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(id));
	    }
	}
} // end of namespace
