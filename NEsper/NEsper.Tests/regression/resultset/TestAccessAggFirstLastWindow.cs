///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAccessAggFirstLastWindow
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        config.AddEventType<SupportBean_A>();
	        config.AddEventType<SupportBean_B>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNoParamChainedAndProperty() {
	        _epService.EPAdministrator.Configuration.AddEventType("ChainEvent", typeof(ChainEvent));
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("select first().property as val0, first().myMethod() as val1, window() as val2 from ChainEvent.std:lastevent()");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new ChainEvent("p1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[]{"p1", "abc"});
	    }

        [Test]
	    public void TestLastMaxMixedOnSelect() {
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(TheString like 'A%')");

	        string epl = "on SupportBean(TheString like 'B%') select last(mw.IntPrimitive) as li, max(mw.IntPrimitive) as mi from MyWindow mw";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        string[] fields = "li,mi".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10});

	        for (int i = 11; i < 20; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean("A1", i));
	            _epService.EPRuntime.SendEvent(new SupportBean("Bx", -1));
	            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{i, i});
	        }

	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, 19});

	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("B1", -1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, 19});
	    }

        [Test]
	    public void TestPrevNthIndexedFirstLast() {
	        string epl = "select " +
	                "prev(IntPrimitive, 0) as p0, " +
	                "prev(IntPrimitive, 1) as p1, " +
	                "prev(IntPrimitive, 2) as p2, " +
	                "nth(IntPrimitive, 0) as n0, " +
	                "nth(IntPrimitive, 1) as n1, " +
	                "nth(IntPrimitive, 2) as n2, " +
	                "last(IntPrimitive, 0) as l1, " +
	                "last(IntPrimitive, 1) as l2, " +
	                "last(IntPrimitive, 2) as l3 " +
	                "from SupportBean.win:length(3)";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        string[] fields = "p0,p1,p2,n0,n1,n2,l1,l2,l3".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, null, null, 10, null, null, 10, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{11, 10, null, 11, 10, null, 11, 10, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{12, 11, 10, 12, 11, 10, 12, 11, 10});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 13));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{13, 12, 11, 13, 12, 11, 13, 12, 11});
	    }

        [Test]
	    public void TestFirstLastIndexed() {
	        string epl = "select " +
	                "first(IntPrimitive, 0) as f0, " +
	                "first(IntPrimitive, 1) as f1, " +
	                "first(IntPrimitive, 2) as f2, " +
	                "first(IntPrimitive, 3) as f3, " +
	                "last(IntPrimitive, 0) as l0, " +
	                "last(IntPrimitive, 1) as l1, " +
	                "last(IntPrimitive, 2) as l2, " +
	                "last(IntPrimitive, 3) as l3 " +
	                "from SupportBean.win:length(3)";

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionFirstLastIndexed();

	        // test join
	        stmt.Dispose();
	        epl += ", SupportBean_A.std:lastevent()";
	        stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));

	        RunAssertionFirstLastIndexed();

	        // test variable
	        stmt.Dispose();
	        _epService.EPAdministrator.CreateEPL("create variable int indexvar = 2");
	        epl = "select " +
	                "first(IntPrimitive, indexvar) as f0 " +
	                "from SupportBean.win:keepall()";

	        stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        string[] fields = "f0".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{12});

	        _epService.EPRuntime.SetVariableValue("indexvar", 0);
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10});
	        stmt.Dispose();

	        // test as part of function
	        _epService.EPAdministrator.CreateEPL("select Math.abs(last(IntPrimitive)) from SupportBean");
	    }

	    private void RunAssertionFirstLastIndexed() {
	        string[] fields = "f0,f1,f2,f3,l0,l1,l2,l3".Split(',');
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, null, null, null, 10, null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 11, null, null, 11, 10, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 11, 12, null, 12, 11, 10, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 13));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{11, 12, 13, null, 13, 12, 11, null});
	    }

        [Test]
	    public void TestInvalid() {
	        TryInvalid("select window(distinct IntPrimitive) from SupportBean",
	                   "Incorrect syntax near '(' ('distinct' is a reserved keyword) at line 1 column 13 near reserved keyword 'distinct' [");

	        TryInvalid("select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean.std:lastevent() sa, SupportBean.std:lastevent() sb",
	                   "Error starting statement: Failed to validate select-clause expression 'window(sa.IntPrimitive+sb.IntPrimitive)': The 'window' aggregation function requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead [select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean.std:lastevent() sa, SupportBean.std:lastevent() sb]");

	        TryInvalid("select last(*) from SupportBean.std:lastevent() sa, SupportBean.std:lastevent() sb",
	                   "Error starting statement: Failed to validate select-clause expression 'last(*)': The 'last' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select last(*) from SupportBean.std:lastevent() sa, SupportBean.std:lastevent() sb]");

	        TryInvalid("select TheString, (select first(*) from SupportBean.std:lastevent() sa) from SupportBean.std:lastevent() sb",
	                   "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Failed to validate select-clause expression 'first(*)': The 'first' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select TheString, (select first(*) from SupportBean.std:lastevent() sa) from SupportBean.std:lastevent() sb]");

	        TryInvalid("select window(x.*) from SupportBean.std:lastevent()",
	                   "Error starting statement: Failed to validate select-clause expression 'window(x.*)': Stream by name 'x' could not be found among all streams [select window(x.*) from SupportBean.std:lastevent()]");

	        TryInvalid("select window(*) from SupportBean x",
	                   "Error starting statement: Failed to validate select-clause expression 'window(*)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(*) from SupportBean x]");
	        TryInvalid("select window(x.*) from SupportBean x",
	                   "Error starting statement: Failed to validate select-clause expression 'window(x.*)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.*) from SupportBean x]");
	        TryInvalid("select window(x.IntPrimitive) from SupportBean x",
	                   "Error starting statement: Failed to validate select-clause expression 'window(x.IntPrimitive)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.IntPrimitive) from SupportBean x]");

	        TryInvalid("select window(x.IntPrimitive, 10) from SupportBean.win:keepall() x",
	                   "Error starting statement: Failed to validate select-clause expression 'window(x.IntPrimitive,10)': The 'window' aggregation function does not accept an index expression; Use 'first' or 'last' instead [");

	        TryInvalid("select first(x.*, 10d) from SupportBean.std:lastevent() as x",
	                   "Error starting statement: Failed to validate select-clause expression 'first(x.*,10.0)': The 'first' aggregation function requires an index expression that returns an integer value [select first(x.*, 10d) from SupportBean.std:lastevent() as x]");
	    }

        [Test]
	    public void TestSubquery() {
	        string epl = "select id, (select window(sb.*) from SupportBean.win:length(2) as sb) as w from SupportBean_A";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        string[] fields = "id,w".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", null});

	        SupportBean beanOne = SendEvent(_epService, "E1", 0, 1);
	        _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A2", new object[]{beanOne}});

	        SupportBean beanTwo = SendEvent(_epService, "E2", 0, 1);
	        _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A3", new object[]{beanOne, beanTwo}});

	        SupportBean beanThree = SendEvent(_epService, "E2", 0, 1);
	        _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A4", new object[]{beanTwo, beanThree}});
	    }

        [Test]
	    public void TestMethodAndAccessTogether() {
	        string epl = "select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean.win:length(2) as sa";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        string[] fields = "si,wi".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, IntArray(1)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3, IntArray(1, 2)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5, IntArray(2, 3)});

	        stmt.Dispose();
	        epl = "select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean.win:keepall() as sa group by TheString";
	        stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, IntArray(1)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, IntArray(2)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5, IntArray(2, 3)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5, IntArray(1, 4)});
	    }

        [Test]
	    public void TestOutputRateLimiting() {
	        string epl = "select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean.win:keepall() as sa output every 2 events";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        string[] fields = "si,wi".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{
	                new object[]{1, IntArray(1)},
	                new object[]{3, IntArray(1, 2)},
	        });

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{
	                new object[]{6, IntArray(1, 2, 3)},
	                new object[]{10, IntArray(1, 2, 3, 4)},
	        });
	    }

        [Test]
	    public void TestTypeAndColNameAndEquivalency() {
	        _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).Name);

	        string epl = "select " +
	                "first(sa.DoublePrimitive + sa.IntPrimitive), " +
	                "first(sa.IntPrimitive), " +
	                "window(sa.*), " +
	                "last(*) from SupportBean.win:length(2) as sa";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        object[][] rows = new object[][] {
	                new object[]{"first(sa.DoublePrimitive+sa.IntPrimitive)", typeof(double?)},
	                new object[]{"first(sa.IntPrimitive)", typeof(int)},
	                new object[]{"window(sa.*)", typeof(SupportBean[])},
	                new object[]{"last(*)", typeof(SupportBean)},
	                };
	        for (int i = 0; i < rows.Length; i++) {
	            EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
	            Assert.AreEqual(rows[i][0], prop.PropertyName);
	            Assert.AreEqual(rows[i][1], prop.PropertyType);
	        }

	        stmt.Dispose();
	        epl = "select " +
	                "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
	                "first(sa.IntPrimitive) as f2, " +
	                "window(sa.*) as w1, " +
	                "last(*) as l1 " +
	                "from SupportBean.win:length(2) as sa";
	        stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionType(false);

	        stmt.Dispose();

	        epl = "select " +
	                "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
	                "first(sa.IntPrimitive) as f2, " +
	                "window(sa.*) as w1, " +
	                "last(*) as l1 " +
	                "from SupportBean.win:length(2) as sa " +
	                "having SupportStaticMethodLib.alwaysTrue({first(sa.DoublePrimitive + sa.IntPrimitive), " +
	                "first(sa.IntPrimitive), window(sa.*), last(*)})";
	        stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionType(true);
	    }

	    private void RunAssertionType(bool isCheckStatic) {
	        string[] fields = "f1,f2,w1,l1".Split(',');

	        SupportBean beanOne = SendEvent(_epService, "E1", 10d, 100);
	        object[] expected = new object[] {110d, 100, new object[] {beanOne}, beanOne};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
	        if (isCheckStatic) {
	            object[] parameters = SupportStaticMethodLib.Invocations[0];
	            SupportStaticMethodLib.Invocations.Clear();
	            EPAssertionUtil.AssertEqualsExactOrder(expected, parameters);
	        }
	    }

        [Test]
	    public void TestJoin2Access() {
	        string epl = "select " +
	                "sa.id as ast, " +
	                "sb.id as bst, " +
	                "first(sa.id) as fas, " +
	                "window(sa.id) as was, " +
	                "last(sa.id) as las, " +
	                "first(sb.id) as fbs, " +
	                "window(sb.id) as wbs, " +
	                "last(sb.id) as lbs " +
	                "from SupportBean_A.win:length(2) as sa, SupportBean_B.win:length(2) as sb " +
	                "order by ast, bst";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        string[] fields = "ast,bst,fas,was,las,fbs,wbs,lbs".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A1", "B1", "A1", Split("A1"), "A1", "B1", Split("B1"), "B1"});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{
	                        new object[]{"A2", "B1", "A1", Split("A1,A2"), "A2", "B1", Split("B1"), "B1"}
	                });

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{
	                        new object[]{"A3", "B1", "A2", Split("A2,A3"), "A3", "B1", Split("B1"), "B1"}
	                });

	        _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{
	                        new object[]{"A2", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2"},
	                        new object[]{"A3", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2"}
	                });

	        _epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{
	                        new object[]{"A2", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3"},
	                        new object[]{"A3", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3"}
	                });

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
	                new object[][]{
	                        new object[]{"A4", "B2", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3"},
	                        new object[]{"A4", "B3", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3"}
	                });
	    }

        [Test]
	    public void TestOuterJoin1Access() {
	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	        _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
	        string epl = "select " +
	                "sa.id as aid, " +
	                "sb.id as bid, " +
	                "first(sb.p10) as fb, " +
	                "window(sb.p10) as wb, " +
	                "last(sb.p10) as lb " +
	                "from S0.win:keepall() as sa " +
	                "left outer join " +
	                "S1.win:keepall() as sb " +
	                "on sa.id = sb.id";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        string[] fields = "aid,bid,fb,wb,lb".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1, null, null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1, 1, "A", Split("A"), "A"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{2, 2, "A", Split("A,B"), "B"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "C"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{3, 3, "A", Split("A,B,C"), "C"});
	    }

        [Test]
	    public void TestBatchWindow()
	    {
	        string epl = "select irstream " +
	                "first(TheString) as fs, " +
	                "window(TheString) as ws, " +
	                "last(TheString) as ls " +
	                "from SupportBean.win:length_batch(2) as sb";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        string[] fields = "fs,ws,ls".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new object[]{null, null, null});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new object[]{"E1", Split("E1,E2"), "E2"});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new object[]{"E1", Split("E1,E2"), "E2"});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new object[]{"E3", Split("E3,E4"), "E4"});
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetOld(), fields, new object[]{"E3", Split("E3,E4"), "E4"});
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new object[]{"E5", Split("E5,E6"), "E6"});
	        _listener.Reset();
	    }

        [Test]
	    public void TestBatchWindowGrouped()
	    {
	        string epl = "select " +
	                "TheString, " +
	                "first(IntPrimitive) as fi, " +
	                "window(IntPrimitive) as wi, " +
	                "last(IntPrimitive) as li " +
	                "from SupportBean.win:length_batch(6) as sb group by TheString order by TheString asc";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        string[] fields = "TheString,fi,wi,li".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{
	                new object[]{"E1", 10, IntArray(10, 11, 12), 12},
	                new object[]{"E2", 20, IntArray(20), 20},
	                new object[]{"E3", 30, IntArray(30, 31), 31}
	        });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 14));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 15));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 16));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 17));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 18));
	        EventBean[] result = _listener.GetAndResetLastNewData();
	        EPAssertionUtil.AssertPropsPerRow(result, fields, new object[][]{
	                new object[]{"E1", 13, IntArray(13, 14, 15, 16, 17, 18), 18},
	                new object[]{"E2", null, null, null},
	                new object[]{"E3", null, null, null}
	        });
	    }

        [Test]
	    public void TestLateInitialize()
	    {
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));

	        string[] fields = "firststring,windowstring,laststring".Split(',');
	        string epl = "select " +
	                "first(TheString) as firststring, " +
	                "window(TheString) as windowstring, " +
	                "last(TheString) as laststring " +
	                "from MyWindow";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", Split("E1,E2,E3"), "E3"});
	    }

        [Test]
	    public void TestOnDelete()
	    {
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow where TheString = id");

	        string[] fields = "firststring,windowstring,laststring".Split(',');
	        string epl = "select " +
	                "first(TheString) as firststring, " +
	                "window(TheString) as windowstring, " +
	                "last(TheString) as laststring " +
	                "from MyWindow";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", Split("E1"), "E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", Split("E1,E2"), "E2"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", Split("E1,E2,E3"), "E3"});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", Split("E1,E3"), "E3"});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", Split("E1"), "E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 40));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", Split("E4"), "E4"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 50));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", Split("E4,E5"), "E5"});

	        _epService.EPRuntime.SendEvent(new SupportBean_A("E4"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E5", Split("E5"), "E5"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 60));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E5", Split("E5,E6"), "E6"});
	    }

        [Test]
	    public void TestOnDemandQuery()
	    {
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));

	        EPOnDemandPreparedQuery q = _epService.EPRuntime.PrepareQuery("select first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindow as s");
	        EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "f,w,l".Split(','),
                    new object[][] { new object[] { 10, IntArray(10, 20, 30, 31, 11, 12), 12 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 13));
	        EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "f,w,l".Split(','),
                    new object[][] { new object[] { 10, IntArray(10, 20, 30, 31, 11, 12, 13), 13 } });

	        q = _epService.EPRuntime.PrepareQuery("select TheString as s, first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindow as s group by TheString order by TheString asc");
	        object[][] expected = new object[][] {
	                        new object[]{"E1", 10, IntArray(10, 11, 12, 13), 13},
	                        new object[]{"E2", 20, IntArray(20), 20},
	                        new object[]{"E3", 30, IntArray(30, 31), 31}
	                };
	        EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".Split(','), expected);
	        EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".Split(','), expected);
	    }

        [Test]
	    public void TestStar()
	    {
	        string epl = "select " +
	                    "first(*) as firststar, " +
	                    "first(sb.*) as firststarsb, " +
	                    "last(*) as laststar, " +
	                    "last(sb.*) as laststarsb, " +
	                    "window(*) as windowstar, " +
	                    "window(sb.*) as windowstarsb " +
	                    "from SupportBean.win:length(2) as sb";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionStar();
	        stmt.Dispose();

	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(epl, model.ToEPL());

	        RunAssertionStar();
	    }

	    private void RunAssertionStar() {
	        string[] fields = "firststar,firststarsb,laststar,laststarsb,windowstar,windowstarsb".Split(',');

	        object beanE1 = new SupportBean("E1", 10);
	        _epService.EPRuntime.SendEvent(beanE1);
	        object[] window = new object[] {beanE1};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{beanE1, beanE1, beanE1, beanE1, window, window});

	        object beanE2 = new SupportBean("E2", 20);
	        _epService.EPRuntime.SendEvent(beanE2);
	        window = new object[] {beanE1, beanE2};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{beanE1, beanE1, beanE2, beanE2, window, window});

	        object beanE3 = new SupportBean("E3", 30);
	        _epService.EPRuntime.SendEvent(beanE3);
	        window = new object[] {beanE2, beanE3};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{beanE2, beanE2, beanE3, beanE3, window, window});
	    }

        [Test]
	    public void TestUnboundedStream()
	    {
	        string epl = "select " +
	                "first(TheString) as f1, " +
	                "first(sb.*) as f2, " +
	                "first(*) as f3, " +
	                "last(TheString) as l1, " +
	                "last(sb.*) as l2, " +
	                "last(*) as l3 " +
	                "from SupportBean as sb";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        string[] fields = "f1,f2,f3,l1,l2,l3".Split(',');

	        SupportBean beanOne = SendEvent(_epService, "E1", 1d, 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", beanOne, beanOne, "E1", beanOne, beanOne});

	        SupportBean beanTwo = SendEvent(_epService, "E2", 2d, 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", beanOne, beanOne, "E2", beanTwo, beanTwo});

	        SupportBean beanThree = SendEvent(_epService, "E3", 3d, 3);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", beanOne, beanOne, "E3", beanThree, beanThree});
	    }

        [Test]
	    public void TestWindowedUnGrouped()
	    {
	        string epl = "select " +
	                "first(TheString) as firststring, " +
	                "last(TheString) as laststring, " +
	                "first(IntPrimitive) as firstint, " +
	                "last(IntPrimitive) as lastint, " +
	                "window(IntPrimitive) as allint " +
	                "from SupportBean.win:length(2)";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionUngrouped();

	        stmt.Dispose();

	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(epl, model.ToEPL());

	        RunAssertionUngrouped();

	        stmt.Dispose();

	        // test null-value provided
	        EPStatement stmtWNull = _epService.EPAdministrator.CreateEPL("select window(intBoxed).take(10) from SupportBean.win:length(2)");
	        stmtWNull.AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	    }

        [Test]
	    public void TestWindowedGrouped()
	    {
	        string epl = "select " +
	                "TheString, " +
	                "first(TheString) as firststring, " +
	                "last(TheString) as laststring, " +
	                "first(IntPrimitive) as firstint, " +
	                "last(IntPrimitive) as lastint, " +
	                "window(IntPrimitive) as allint " +
	                "from SupportBean.win:length(5) " +
	                "group by TheString order by TheString";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionGrouped();

	        stmt.Dispose();

	        // SODA
	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(epl, model.ToEPL());

	        RunAssertionGrouped();

	        // test hints
	        stmt.Dispose();
	        string newEPL = "@Hint('disable_reclaim_group') " + epl;
	        stmt = _epService.EPAdministrator.CreateEPL(newEPL);
	        stmt.AddListener(_listener);
	        RunAssertionGrouped();

	        // test hints
	        stmt.Dispose();
	        newEPL = "@Hint('reclaim_group_aged=10,reclaim_group_freq=5') " + epl;
	        stmt = _epService.EPAdministrator.CreateEPL(newEPL);
	        stmt.AddListener(_listener);
	        RunAssertionGrouped();

	        // test SODA indexes
	        string eplFirstLast = "select " +
	                "last(IntPrimitive), " +
	                "last(IntPrimitive, 1), " +
	                "first(IntPrimitive), " +
	                "first(IntPrimitive, 1) " +
	                "from SupportBean.win:length(3)";
	        EPStatementObjectModel modelFirstLast = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, modelFirstLast.ToEPL());
	    }

	    private void RunAssertionGrouped() {
	        string[] fields = "TheString,firststring,firstint,laststring,lastint,allint".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10, "E1", 10, new int[]{10}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 11, "E2", 11, new int[]{11}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 12));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 10, "E1", 12, new int[]{10, 12}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 13));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 11, "E2", 13, new int[]{11, 13}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 14));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E2", 11, "E2", 14, new int[]{11, 13, 14}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 15));  // push out E1/10
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 12, "E1", 15, new int[]{12, 15}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 16));  // push out E2/11 --> 2 events
	        EventBean[] received = _listener.GetAndResetLastNewData();
	        EPAssertionUtil.AssertPropsPerRow(received, fields,
	                new object[][]{
	                        new object[]{"E1", "E1", 12, "E1", 16, new int[]{12, 15, 16}},
	                        new object[]{"E2", "E2", 13, "E2", 14, new int[]{13, 14}}
	                });
	    }

	    private void RunAssertionUngrouped() {
	        string[] fields = "firststring,firstint,laststring,lastint,allint".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E1", 10, new int[]{10}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E2", 11, new int[]{10, 11}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 11, "E3", 12, new int[]{11, 12}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 13));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 12, "E4", 13, new int[]{12, 13}});
	    }

	    private object Split(string s)
	    {
	        if (s == null) {
	            return new object[0];
	        }
	        return s.Split(',');
	    }

	    private int[] IntArray(params int[] value)
	    {
	        if (value == null) {
	            return new int[0];
	        }
	        return value;
	    }

	    private SupportBean SendEvent(EPServiceProvider epService, string theString, double doublePrimitive, int intPrimitive) {
	        SupportBean bean = new SupportBean(theString, intPrimitive);
	        bean.DoublePrimitive = doublePrimitive;
	        epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

	    private void TryInvalid(string epl, string message) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, message);
	        }
	    }

        public class ChainEvent
        {
            public ChainEvent(string property)
            {
                Property = property;
            }

            public string Property { get; private set; }

            public string MyMethod()
            {
                return "abc";
            }
        }
    }
} // end of namespace
