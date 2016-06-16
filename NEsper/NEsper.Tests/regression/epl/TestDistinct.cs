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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestDistinct 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName); }
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_N>();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestWildcardJoinPattern() {
	        var epl = "select distinct * from " +
	                "SupportBean(IntPrimitive=0) as fooB unidirectional " +
	                "inner join " +
	                "pattern [" +
	                "every-distinct(fooA.TheString) fooA=SupportBean(IntPrimitive=1)" +
	                "->" +
	                "every-distinct(wooA.TheString) wooA=SupportBean(IntPrimitive=2)" +
	                " where timer:within(1 hour)" +
	                "].win:time(1 hour) as fooWooPair " +
	                "on fooB.LongPrimitive = fooWooPair.fooA.LongPrimitive";

	        var subs = new SupportSubscriber();
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        SendEvent("E1", 1, 10L);
	        SendEvent("E1", 2, 10L);

	        SendEvent("E2", 1, 10L);
	        SendEvent("E2", 2, 10L);

	        SendEvent("E3", 1, 10L);
	        SendEvent("E3", 2, 10L);

	        SendEvent("Query", 0, 10L);
	        Assert.IsTrue(_listener.IsInvoked);
	    }

	    private void SendEvent(string theString, int intPrimitive, long longPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	    }

        [Test]
	    public void TestOnDemandAndOnSelect()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        var query = "select distinct TheString, IntPrimitive from MyWindow order by TheString, IntPrimitive";
	        var result = _epService.EPRuntime.ExecuteQuery(query);
	        EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E1", 2},  new object[] {"E2", 2}});

	        var stmt = _epService.EPAdministrator.CreateEPL("on SupportBean_A select distinct TheString, IntPrimitive from MyWindow order by TheString, IntPrimitive asc");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("x"));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E1", 2},  new object[] {"E2", 2}});
	    }

        [Test]
	    public void TestSubquery()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString in (select distinct id from SupportBean_A.win:keepall())");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 2});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 3});
	    }

	    // Since the "this" property will always be unique, this test verifies that condition
        [Test]
	    public void TestBeanEventWildcardThisProperty()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "select distinct * from SupportBean.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2},  new object[] {"E1", 1}});
	    }

        [Test]
	    public void TestBeanEventWildcardSODA()
	    {
	        var fields = new string[] {"id"};
	        var statementText = "select distinct * from SupportBean_A.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"}});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"},  new object[] {"E2"}});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1"},  new object[] {"E2"}});

	        var model = _epService.EPAdministrator.CompileEPL(statementText);
	        Assert.AreEqual(statementText, model.ToEPL());

	        model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.CreateWildcard().Distinct(true);
	        model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_A"));
	        Assert.AreEqual("select distinct * from SupportBean_A", model.ToEPL());
	    }

        [Test]
	    public void TestBeanEventWildcardPlusCols()
	    {
	        var fields = new string[] {"IntPrimitive", "val1", "val2"};
	        var statementText = "select distinct *, IntBoxed%5 as val1, IntBoxed as val2 from SupportBean_N.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_N(1, 8));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] { 1, 3, 8 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_N(1, 3));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] { 1, 3, 8 }, new object[] { 1, 3, 3 } });

	        _epService.EPRuntime.SendEvent(new SupportBean_N(1, 8));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] { 1, 3, 8 }, new object[] { 1, 3, 3 } });
	    }

        [Test]
	    public void TestMapEventWildcard()
	    {
	        IDictionary<string, object> def = new Dictionary<string, object>();
	        def.Put("k1", typeof(string));
	        def.Put("v1", typeof(int));
	        _epService.EPAdministrator.Configuration.AddEventType("MyMapType", def);

	        var fields = new string[] {"k1", "v1"};
	        var statementText = "select distinct * from MyMapType.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        SendMapEvent("E1", 1);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});

	        SendMapEvent("E2", 2);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});

	        SendMapEvent("E1", 1);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});
	    }

        [Test]
	    public void TestOutputSimpleColumn()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "select distinct TheString, IntPrimitive from SupportBean.win:keepall()";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        RunAssertionSimpleColumn(stmt, fields);
	        stmt.Dispose();

	        // test join
	        statementText = "select distinct TheString, IntPrimitive from SupportBean.win:keepall() a, SupportBean_A.win:keepall() b where a.TheString = b.id";
	        stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
	        RunAssertionSimpleColumn(stmt, fields);
	    }

        [Test]
	    public void TestOutputLimitEveryColumn()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "@IterableUnbound select distinct TheString, IntPrimitive from SupportBean output every 3 events";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        RunAssertionOutputEvery(stmt, fields);
	        stmt.Dispose();

	        // test join
	        statementText = "select distinct TheString, IntPrimitive from SupportBean.std:lastevent() a, SupportBean_A.win:keepall() b where a.TheString = b.id output every 3 events";
	        stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
	        RunAssertionOutputEvery(stmt, fields);
	    }

        [Test]
	    public void TestOutputRateSnapshotColumn()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "select distinct TheString, IntPrimitive from SupportBean.win:keepall() output snapshot every 3 events order by TheString asc";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        RunAssertionSnapshotColumn(stmt, fields);
	        stmt.Dispose();

	        statementText = "select distinct TheString, IntPrimitive from SupportBean.win:keepall() a, SupportBean_A.win:keepall() b where a.TheString = b.id output snapshot every 3 events order by TheString asc";
	        stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
	        RunAssertionSnapshotColumn(stmt, fields);
	    }

        [Test]
	    public void TestBatchWindow()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "select distinct TheString, IntPrimitive from SupportBean.win:length_batch(3)";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E2", 2},  new object[] {"E1", 1}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{ new object[] {"E2", 3}});

	        stmt.Dispose();

	        // test batch window with aggregation
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var fieldsTwo = new string[] {"c1", "c2"};
	        var epl = "insert into ABC select distinct TheString as c1, first(IntPrimitive) as c2 from SupportBean.win:time_batch(1 second)";
	        var stmtTwo = _epService.EPAdministrator.CreateEPL(epl);
	        stmtTwo.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fieldsTwo, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 1}});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestBatchWindowJoin()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "select distinct TheString, IntPrimitive from SupportBean.win:length_batch(3) a, SupportBean_A.win:keepall() b where a.TheString = b.id";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E2", 2},  new object[] {"E1", 1}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E2", 3}});
	    }

        [Test]
	    public void TestBatchWindowInsertInto()
	    {
	        var fields = new string[] {"TheString", "IntPrimitive"};
	        var statementText = "insert into MyStream select distinct TheString, IntPrimitive from SupportBean.win:length_batch(3)";
	        _epService.EPAdministrator.CreateEPL(statementText);

	        statementText = "select * from MyStream";
	        var stmt = _epService.EPAdministrator.CreateEPL(statementText);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.GetNewDataListFlattened()[0], fields, new object[] {"E2", 2});
	        EPAssertionUtil.AssertProps(_listener.GetNewDataListFlattened()[1], fields, new object[] {"E3", 3});
	    }

	    private void RunAssertionOutputEvery(EPStatement stmt, string[] fields)
	    {
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E2", 2},  new object[] {"E1", 1}});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E2", 3}});
	        _listener.Reset();
	    }

	    private void RunAssertionSimpleColumn(EPStatement stmt, string[] fields)
	    {
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 1}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 1},  new object[] {"E1", 2}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 2});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 1},  new object[] {"E1", 2},  new object[] {"E2", 2}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 1},  new object[] {"E1", 2},  new object[] {"E2", 2}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", 2});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 1},  new object[] {"E1", 2},  new object[] {"E2", 2}});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});
	    }

	    private void RunAssertionSnapshotColumn(EPStatement stmt, string[] fields)
	    {
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1}});
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2}});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2},  new object[] {"E3", 3}});
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]{ new object[] {"E1", 1},  new object[] {"E2", 2},  new object[] {"E3", 3}});
	        _listener.Reset();
	    }

	    private void SendMapEvent(string s, int i)
	    {
	        IDictionary<string, object> def = new Dictionary<string, object>();
	        def.Put("k1", s);
	        def.Put("v1", i);
	        _epService.EPRuntime.SendEvent(def, "MyMapType");
	    }
	}
} // end of namespace
