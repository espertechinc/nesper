///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestRowLimit
    {
		private EPServiceProvider _epService;
		private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        config.AddEventType("SupportBeanNumeric", typeof(SupportBeanNumeric));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestLimitOneWithOrderOptimization() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));

	        // batch-window assertions
	        var eplWithBatchSingleKey = "select TheString from SupportBean.win:length_batch(10) order by TheString limit 1";
	        RunAssertionLimitOneSingleKeySortBatch(eplWithBatchSingleKey);

	        var eplWithBatchMultiKey = "select TheString, IntPrimitive from SupportBean.win:length_batch(5) order by TheString asc, IntPrimitive desc limit 1";
	        RunAssertionLimitOneMultiKeySortBatch(eplWithBatchMultiKey);

	        // context output-when-terminated assertions
	        _epService.EPAdministrator.CreateEPL("create context StartS0EndS1 as start SupportBean_S0 end SupportBean_S1");

	        var eplContextSingleKey = "context StartS0EndS1 " +
	                "select TheString from SupportBean.win:keepall() " +
	                "output snapshot when terminated " +
	                "order by TheString limit 1";
	        RunAssertionLimitOneSingleKeySortBatch(eplContextSingleKey);

	        var eplContextMultiKey = "context StartS0EndS1 " +
	                "select TheString, IntPrimitive from SupportBean.win:keepall() " +
	                "output snapshot when terminated " +
	                "order by TheString asc, IntPrimitive desc limit 1";
	        RunAssertionLimitOneMultiKeySortBatch(eplContextMultiKey);
	    }

	    private void RunAssertionLimitOneMultiKeySortBatch(string epl) {
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendSBSequenceAndAssert("F", 10, new object[][] { new object[] {"F", 10},  new object[] {"X", 8},  new object[] {"F", 8},  new object[] {"G", 10},  new object[] {"X", 1}});
	        SendSBSequenceAndAssert("G", 12, new object[][] { new object[] {"X", 10},  new object[] {"G", 12},  new object[] {"H", 100},  new object[] {"G", 10},  new object[] {"X", 1}});
	        SendSBSequenceAndAssert("G", 11, new object[][] { new object[] {"G", 10},  new object[] {"G", 8},  new object[] {"G", 8},  new object[] {"G", 10},  new object[] {"G", 11}});

	        stmt.Dispose();
	    }

	    private void RunAssertionLimitOneSingleKeySortBatch(string epl) {
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendSBSequenceAndAssert("A", new string[] {"F", "Q", "R", "T", "M", "T", "A", "I", "P", "B"});
	        SendSBSequenceAndAssert("B", new string[] {"P", "Q", "P", "T", "P", "T", "P", "P", "P", "B"});
	        SendSBSequenceAndAssert("C", new string[] {"C", "P", "Q", "P", "T", "P", "T", "P", "P", "P", "X"});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestBatchNoOffsetNoOrder()
		{
	        var statementString = "select irstream * from SupportBean.win:length_batch(3) limit 1";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);

	        RunAssertion(stmt);
	    }

        [Test]
	    public void TestLengthOffsetVariable()
		{
	        _epService.EPAdministrator.CreateEPL("create variable int myrows = 2");
	        _epService.EPAdministrator.CreateEPL("create variable int myoffset = 1");
	        _epService.EPAdministrator.CreateEPL("on SupportBeanNumeric set myrows = intOne, myoffset = intTwo");

	        var statementString = "select * from SupportBean.win:length(5) output every 5 events limit myoffset, myrows";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);
	        RunAssertionVariable(stmt);
	        stmt.Dispose();
	        _listener.Reset();
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));

	        statementString = "select * from SupportBean.win:length(5) output every 5 events limit myrows offset myoffset";
	        stmt = _epService.EPAdministrator.CreateEPL(statementString);
	        RunAssertionVariable(stmt);
	        stmt.Dispose();
	        _listener.Reset();
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));

	        var model = _epService.EPAdministrator.CompileEPL(statementString);
	        Assert.AreEqual(statementString, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model);
	        RunAssertionVariable(stmt);
	    }

        [Test]
	    public void TestOrderBy()
		{
	        var statementString = "select * from SupportBean.win:length(5) output every 5 events order by IntPrimitive limit 2 offset 2";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);

	        var fields = "TheString".Split(',');
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E1", 90);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E2", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E3", 60);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"}});

	        SendEvent("E4", 99);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"},  new object[] {"E4"}});
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("E5", 6);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E3"},  new object[] {"E1"}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E3"},  new object[] {"E1"}});
	    }

	    private void RunAssertionVariable(EPStatement stmt)
	    {
	        var fields = "TheString".Split(',');
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E1", 1);
	        SendEvent("E2", 2);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2"}});

	        SendEvent("E3", 3);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2"},  new object[] {"E3"}});

	        SendEvent("E4", 4);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2"},  new object[] {"E3"}});
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("E5", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2"},  new object[] {"E3"}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E2"},  new object[] {"E3"}});

	        SendEvent("E6", 6);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E3"},  new object[] {"E4"}});
	        Assert.IsFalse(_listener.IsInvoked);

	        // change variable values
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 3));
	        SendEvent("E7", 7);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E6"},  new object[] {"E7"}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 0));
	        SendEvent("E8", 8);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E4"},  new object[] {"E5"},  new object[] {"E6"},  new object[] {"E7"},  new object[] {"E8"}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(10, 0));
	        SendEvent("E9", 9);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E5"},  new object[] {"E6"},  new object[] {"E7"},  new object[] {"E8"},  new object[] {"E9"}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(6, 3));
	        SendEvent("E10", 10);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E9"},  new object[] {"E10"}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E9"},  new object[] {"E10"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 1));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E7"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E7"},  new object[] {"E8"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 2));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E8"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(6, 6));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 4));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E10"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric((int?) null, null));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E6"},  new object[] {"E7"},  new object[] {"E8"},  new object[] {"E9"},  new object[] {"E10"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, 2));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E8"},  new object[] {"E9"},  new object[] {"E10"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, null));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E6"},  new object[] {"E7"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 4));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E10"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 0));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E6"},  new object[] {"E7"},  new object[] {"E8"},  new object[] {"E9"},  new object[] {"E10"}});

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
	    }

        [Test]
	    public void TestBatchOffsetNoOrderOM()
		{
	        var model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.CreateWildcard();
	        model.SelectClause.StreamSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
	        model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("win", "length_batch", Expressions.Constant(3)));
	        model.RowLimitClause = RowLimitClause.Create(1);

	        var statementString = "select irstream * from SupportBean.win:length_batch(3) limit 1";
	        Assert.AreEqual(statementString, model.ToEPL());
	        var stmt = _epService.EPAdministrator.Create(model);
	        RunAssertion(stmt);
	        stmt.Dispose();
	        _listener.Reset();

	        model = _epService.EPAdministrator.CompileEPL(statementString);
	        Assert.AreEqual(statementString, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model);
	        RunAssertion(stmt);
	    }

        [Test]
	    public void TestFullyGroupedOrdered()
		{
	        var statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) group by TheString order by sum(IntPrimitive) limit 2";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);

	        var fields = "TheString,mysum".Split(',');
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E1", 90);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 90}});

	        SendEvent("E2", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2", 5},  new object[] {"E1", 90}});

	        SendEvent("E3", 60);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2", 5},  new object[] {"E3", 60}});

	        SendEvent("E3", 40);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E2", 5},  new object[] {"E1", 90}});

	        SendEvent("E2", 1000);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 90},  new object[] {"E3", 100}});
	    }

        [Test]
	    public void TestEventPerRowUnGrouped()
		{
	        SendTimer(1000);
	        var statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) output every 10 seconds order by TheString desc limit 2";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);

	        var fields = "TheString,mysum".Split(',');
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E1", 10);
	        SendEvent("E2", 5);
	        SendEvent("E3", 20);
	        SendEvent("E4", 30);

	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E4", 65},  new object[] {"E3", 35}});
	    }

        [Test]
	    public void TestGroupedSnapshot()
		{
	        SendTimer(1000);
	        var statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) group by TheString output snapshot every 10 seconds order by sum(IntPrimitive) desc limit 2";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);

	        var fields = "TheString,mysum".Split(',');
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E1", 10);
	        SendEvent("E2", 5);
	        SendEvent("E3", 20);
	        SendEvent("E1", 30);

	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 40},  new object[] {"E3", 20}});
	    }

        [Test]
	    public void TestGroupedSnapshotNegativeRowcount()
		{
	        SendTimer(1000);
	        var statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) group by TheString output snapshot every 10 seconds order by sum(IntPrimitive) desc limit -1 offset 1";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementString);

	        var fields = "TheString,mysum".Split(',');
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E1", 10);
	        SendEvent("E2", 5);
	        SendEvent("E3", 20);
	        SendEvent("E1", 30);

	        SendTimer(11000);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E3", 20},  new object[] {"E2", 5}});
	    }

        [Test]
	    public void TestInvalid()
	    {
	        _epService.EPAdministrator.CreateEPL("create variable string myrows = 'abc'");
	        TryInvalid("select * from SupportBean limit myrows",
	                   "Error starting statement: Limit clause requires a variable of numeric type [select * from SupportBean limit myrows]");
	        TryInvalid("select * from SupportBean limit 1, myrows",
	                   "Error starting statement: Limit clause requires a variable of numeric type [select * from SupportBean limit 1, myrows]");
	        TryInvalid("select * from SupportBean limit dummy",
	                   "Error starting statement: Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit dummy]");
	        TryInvalid("select * from SupportBean limit 1,dummy",
	                   "Error starting statement: Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit 1,dummy]");
	    }

	    private void SendTimer(long timeInMSec)
	    {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }

	    private void RunAssertion(EPStatement stmt)
	    {
	        var fields = "TheString".Split(',');
	        stmt.AddListener(_listener);
	        SendEvent("E1", 1);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"}});

	        SendEvent("E2", 2);
	        Assert.IsFalse(_listener.IsInvoked);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"}});

	        SendEvent("E3", 3);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1"}});
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendEvent("E4", 4);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E4"}});

	        SendEvent("E5", 5);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E4"}});

	        SendEvent("E6", 6);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E4"}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new object[][]{ new object[] {"E1"}});
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
	    }

	    private void TryInvalid(string expression, string expected)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(expression);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual(expected, ex.Message);
	        }
	    }

	    private void SendEvent(string theString, int intPrimitive)
	    {
	        _epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
	    }

	    private void SendSBSequenceAndAssert(string expected, string[] theStrings)
        {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        foreach (var TheString in theStrings) {
	            SendEvent(TheString, 0);
	        }
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{expected});
	    }

	    private void SendSBSequenceAndAssert(string expectedString, int expectedInt, IEnumerable<object[]> rows)
        {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        foreach (var row in rows) {
	            SendEvent(row[0].ToString(), row[1].AsInt());
	        }
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString,IntPrimitive".Split(','), new object[]{expectedString, expectedInt});
	    }
	}
} // end of namespace
