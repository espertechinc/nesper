///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestSubselectAggregatedSingleValue
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportBean", typeof(SupportBean));
	        config.AddEventType("S0", typeof(SupportBean_S0));
	        config.AddEventType("S1", typeof(SupportBean_S1));
	        config.AddEventType("MarketData", typeof(SupportMarketDataBean));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestAggregatedSingleValue() {
	        RunAssertionUngroupedUncorrelatedInSelect();
	        RunAssertionUngroupedUncorrelatedTwoAggStopStart();
	        RunAssertionUngroupedUncorrelatedNoDataWindow();
	        RunAssertionUngroupedUncorrelatedWHaving();
	        RunAssertionUngroupedUncorrelatedInWhereClause();
	        RunAssertionUngroupedUncorrelatedInSelectClause();
	        RunAssertionUngroupedUncorrelatedFiltered();
	        RunAssertionUngroupedUncorrelatedWWhereClause();
	        RunAssertionUngroupedCorrelated();
	        RunAssertionUngroupedCorrelatedInWhereClause();
	        RunAssertionUngroupedCorrelatedWHaving();
	        RunAssertionUngroupedCorrelationInsideHaving();
	        RunAssertionUngroupedTableWHaving();
	        RunAssertionGroupedUncorrelatedWHaving();
	        RunAssertionGroupedCorrelatedWHaving();
	        RunAssertionGroupedTableWHaving();
	        RunAssertionGroupedCorrelationInsideHaving();
	    }

        [Test]
	    public void TestInvalid() {
	        string stmtText;

	        SupportMessageAssertUtil.TryInvalid(_epService, "", "Unexpected end-of-input []");

	        stmtText = "select (select sum(s0.id) from S1#length(3) as s1) as value from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect aggregation functions cannot aggregate across correlated properties");

	        stmtText = "select (select s1.id + sum(s1.id) from S1#length(3) as s1) as value from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect properties must all be within aggregation functions");

	        stmtText = "select (select sum(s0.id + s1.id) from S1#length(3) as s1) as value from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect aggregation functions cannot aggregate across correlated properties");

	        // having-clause cannot aggregate over properties from other streams
	        stmtText = "select (select last(theString) from SupportBean#keepall having sum(s0.p00) = 1) as c0 from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Failed to validate having-clause expression '(sum(s0.p00))=1': Implicit conversion from datatype 'String' to numeric is not allowed for aggregation function 'sum' [");

	        // having-clause properties must be aggregated
	        stmtText = "select (select last(theString) from SupportBean#keepall having sum(intPrimitive) = intPrimitive) as c0 from S0 as s0";
	        SupportMessageAssertUtil.TryInvalid(_epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect having-clause requires that all properties are under aggregation, consider using the 'first' aggregation function instead");

	        // having-clause not returning boolean
	        stmtText = "select (select last(theString) from SupportBean#keepall having sum(intPrimitive)) as c0 from S0";
	        SupportMessageAssertUtil.TryInvalid(_epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect having-clause expression must return a boolean value ");
	    }

	    private void RunAssertionGroupedCorrelationInsideHaving() {
	        string epl = "select (select theString from SupportBean#keepall group by theString having sum(intPrimitive) = s0.id) as c0 from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendSB("E1", 100);
	        SendSB("E2", 5);
	        SendSB("E3", 20);
	        SendEventS0Assert(1, null);
	        SendEventS0Assert(5, "E2");

	        SendSB("E2", 3);
	        SendEventS0Assert(5, null);
	        SendEventS0Assert(8, "E2");
	        SendEventS0Assert(20, "E3");

            stmt.Dispose();
	    }

	    private void RunAssertionUngroupedCorrelationInsideHaving() {
	        string epl = "select (select last(theString) from SupportBean#keepall having sum(intPrimitive) = s0.id) as c0 from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendSB("E1", 100);
	        SendEventS0Assert(1, null);
	        SendEventS0Assert(100, "E1");

	        SendSB("E2", 5);
	        SendEventS0Assert(100, null);
	        SendEventS0Assert(105, "E2");

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedTableWHaving() {
	        _epService.EPAdministrator.CreateEPL("create table MyTableWith2Keys(k1 string primary key, k2 string primary key, total sum(int))");
	        _epService.EPAdministrator.CreateEPL("into table MyTableWith2Keys select p10 as k1, p11 as k2, sum(id) as total from S1 group by p10, p11");

	        string epl = "select (select sum(total) from MyTableWith2Keys group by k1 having sum(total) > 100) as c0 from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEventS1(50, "G1", "S1");
	        SendEventS1(50, "G1", "S2");
	        SendEventS1(50, "G2", "S1");
	        SendEventS1(50, "G2", "S2");
	        SendEventS0Assert(null);

	        SendEventS1(1, "G2", "S3");
	        SendEventS0Assert(101);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUngroupedTableWHaving() {
	        _epService.EPAdministrator.CreateEPL("create table MyTable(total sum(int))");
	        _epService.EPAdministrator.CreateEPL("into table MyTable select sum(intPrimitive) as total from SupportBean");

	        string epl = "select (select sum(total) from MyTable having sum(total) > 100) as c0 from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEventS0Assert(null);

	        SendSB("E1", 50);
	        SendEventS0Assert(null);

	        SendSB("E2", 55);
	        SendEventS0Assert(105);

	        SendSB("E3", -5);
	        SendEventS0Assert(null);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionGroupedCorrelatedWHaving() {
	        string epl = "select (select sum(intPrimitive) from SupportBean#keepall where s0.id = intPrimitive group by theString having sum(intPrimitive) > 10) as c0 from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEventS0Assert(10, null);

	        SendSB("G1", 10);
	        SendSB("G2", 10);
	        SendSB("G2", 2);
	        SendSB("G1", 9);
	        SendEventS0Assert(null);

	        SendSB("G2", 10);
	        SendEventS0Assert(10, 20);

	        SendSB("G1", 10);
	        SendEventS0Assert(10, null);

	        stmt.Dispose();
	    }

	    private void RunAssertionGroupedUncorrelatedWHaving() {
	        string epl = "select (select sum(intPrimitive) from SupportBean#keepall group by theString having sum(intPrimitive) > 10) as c0 from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEventS0Assert(null);

	        SendSB("G1", 10);
	        SendSB("G2", 9);
	        SendEventS0Assert(null);

	        SendSB("G2", 2);
	        SendEventS0Assert(11);

	        SendSB("G1", 3);
	        SendEventS0Assert(null);

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedCorrelatedWHaving() {
	        string epl = "select (select sum(intPrimitive) from SupportBean#keepall where theString = s0.p00 having sum(intPrimitive) > 10) as c0 from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEventS0Assert("G1", null);

	        SendSB("G1", 10);
	        SendEventS0Assert("G1", null);

	        SendSB("G2", 11);
	        SendEventS0Assert("G1", null);
	        SendEventS0Assert("G2", 11);

	        SendSB("G1", 12);
	        SendEventS0Assert("G1", 22);

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedFiltered() {
	        string stmtText = "select (select sum(id) from S1(id < 0)#length(3)) as value from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        RunAssertionSumFilter();

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedWWhereClause() {
	        string stmtText = "select (select sum(id) from S1#length(3) where id < 0) as value from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        RunAssertionSumFilter();

	        stmt.Dispose();
	    }

	    private void RunAssertionCorrAggWhereGreater() {
	        string[] fields = "p00".SplitCsv();

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("T1", 10));

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "T1"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "T1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T1"});

	        _epService.EPRuntime.SendEvent(new SupportBean("T1", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(21, "T1"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(22, "T1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T1"});
	    }

	    private void RunAssertionSumFilter() {
	        SendEventS0(1);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(1);
	        SendEventS0(2);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(0);
	        SendEventS0(3);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(-1);
	        SendEventS0(4);
	        Assert.AreEqual(-1, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(-3);
	        SendEventS0(5);
	        Assert.AreEqual(-4, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(-5);
	        SendEventS0(6);
	        Assert.AreEqual(-9, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(-2);   // note event leaving window
	        SendEventS0(6);
	        Assert.AreEqual(-10, _listener.AssertOneGetNewAndReset().Get("value"));
	    }

	    private void RunAssertionUngroupedUncorrelatedNoDataWindow() {
	        string stmtText = "select p00 as c0, (select sum(intPrimitive) from SupportBean) as c1 from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        string[] fields = "c0,c1".SplitCsv();

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", null});

	        _epService.EPRuntime.SendEvent(new SupportBean("", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 10});

	        _epService.EPRuntime.SendEvent(new SupportBean("", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E3", 30});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedWHaving() {
	        string[] fields = "c0,c1".SplitCsv();
	        string epl = "select *, " +
	                     "(select sum(intPrimitive) from SupportBean#keepall having sum(intPrimitive) > 100) as c0," +
	                     "exists (select sum(intPrimitive) from SupportBean#keepall having sum(intPrimitive) > 100) as c1 " +
	                     "from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEventS0Assert(fields, new object[] {null, false});
	        SendSB("E1", 10);
	        SendEventS0Assert(fields, new object[] {null, false});
	        SendSB("E1", 91);
	        SendEventS0Assert(fields, new object[] {101, true});
	        SendSB("E1", 2);
	        SendEventS0Assert(fields, new object[] {103, true});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedCorrelated() {
	        string stmtText = "select p00, " +
	                          "(select sum(intPrimitive) from SupportBean#keepall where theString = s0.p00) as sump00 " +
	                          "from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        string[] fields = "p00,sump00".SplitCsv();

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T1", null});

	        _epService.EPRuntime.SendEvent(new SupportBean("T1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T1", 10});

	        _epService.EPRuntime.SendEvent(new SupportBean("T1", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T1", 21});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "T2"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T2", null});

	        _epService.EPRuntime.SendEvent(new SupportBean("T2", -2));
	        _epService.EPRuntime.SendEvent(new SupportBean("T2", -7));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "T2"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"T2", -9});
	        stmt.Dispose();

	        // test distinct
	        fields = "theString,c0,c1,c2,c3".SplitCsv();
	        string viewExpr = "select theString, " +
	                          "(select count(sb.intPrimitive) from SupportBean()#keepall as sb where bean.theString = sb.theString) as c0, " +
	                          "(select count(distinct sb.intPrimitive) from SupportBean()#keepall as sb where bean.theString = sb.theString) as c1, " +
	                          "(select count(sb.intPrimitive, true) from SupportBean()#keepall as sb where bean.theString = sb.theString) as c2, " +
	                          "(select count(distinct sb.intPrimitive, true) from SupportBean()#keepall as sb where bean.theString = sb.theString) as c3 " +
	                          "from SupportBean as bean";
	        stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1L, 1L, 1L, 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 1L, 1L, 1L, 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2L, 2L, 2L, 2L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 3L, 2L, 3L, 2L});

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedCorrelatedInWhereClause() {
	        string stmtText = "select p00 from S0 as s0 where id > " +
	                          "(select sum(intPrimitive) from SupportBean#keepall where theString = s0.p00)";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertionCorrAggWhereGreater();
	        stmt.Dispose();

	        stmtText = "select p00 from S0 as s0 where id > " +
	                   "(select sum(intPrimitive) from SupportBean#keepall where theString||'X' = s0.p00||'X')";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        RunAssertionCorrAggWhereGreater();
	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedInWhereClause() {
	        string stmtText = "select * from MarketData " +
	                          "where price > (select max(price) from MarketData(symbol='GOOG')#lastevent) ";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendEventMD("GOOG", 1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEventMD("GOOG", 2);
	        Assert.IsFalse(_listener.IsInvoked);

	        object theEvent = SendEventMD("IBM", 3);
	        Assert.AreEqual(theEvent, _listener.AssertOneGetNewAndReset().Underlying);

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedInSelectClause() {
	        string stmtText = "select (select s0.id + max(s1.id) from S1#length(3) as s1) as value from S0 as s0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendEventS0(1);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(100);
	        SendEventS0(2);
	        Assert.AreEqual(102, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(30);
	        SendEventS0(3);
	        Assert.AreEqual(103, _listener.AssertOneGetNewAndReset().Get("value"));

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedInSelect() {
	        string stmtText = "select (select max(id) from S1#length(3)) as value from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendEventS0(1);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(100);
	        SendEventS0(2);
	        Assert.AreEqual(100, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(200);
	        SendEventS0(3);
	        Assert.AreEqual(200, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(190);
	        SendEventS0(4);
	        Assert.AreEqual(200, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(180);
	        SendEventS0(5);
	        Assert.AreEqual(200, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(170);   // note event leaving window
	        SendEventS0(6);
	        Assert.AreEqual(190, _listener.AssertOneGetNewAndReset().Get("value"));

	        stmt.Dispose();
	    }

	    private void RunAssertionUngroupedUncorrelatedTwoAggStopStart() {
	        string stmtText = "select (select avg(id) + max(id) from S1#length(3)) as value from S0";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendEventS0(1);
	        Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(100);
	        SendEventS0(2);
	        Assert.AreEqual(200.0, _listener.AssertOneGetNewAndReset().Get("value"));

	        SendEventS1(200);
	        SendEventS0(3);
	        Assert.AreEqual(350.0, _listener.AssertOneGetNewAndReset().Get("value"));

	        stmt.Stop();
	        SendEventS1(10000);
	        SendEventS0(4);
	        Assert.IsFalse(_listener.IsInvoked);
	        stmt.Start();

	        SendEventS1(10);
	        SendEventS0(5);
	        Assert.AreEqual(20.0, _listener.AssertOneGetNewAndReset().Get("value"));

	        stmt.Dispose();
	    }

	    private void SendEventS0(int id) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(id));
	    }

	    private void SendEventS0(int id, string p00) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(id, p00));
	    }

	    private void SendEventS1(int id, string p10, string p11) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(id, p10, p11));
	    }

	    private void SendEventS1(int id) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(id));
	    }

	    private object SendEventMD(string symbol, double price) {
	        object theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
	        _epService.EPRuntime.SendEvent(theEvent);
	        return theEvent;
	    }

	    private void SendSB(string theString, int intPrimitive) {
	        _epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
	    }

	    private void SendEventS0Assert(object expected) {
	        SendEventS0Assert(0, expected);
	    }

	    private void SendEventS0Assert(int id, object expected) {
	        SendEventS0(id, null);
	        Assert.AreEqual(expected, _listener.AssertOneGetNewAndReset().Get("c0"));
	    }

	    private void SendEventS0Assert(string p00, object expected) {
	        SendEventS0(0, p00);
	        Assert.AreEqual(expected, _listener.AssertOneGetNewAndReset().Get("c0"));
	    }

	    private void SendEventS0Assert(string[] fields, object[] expected) {
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
	    }
	}
} // end of namespace
