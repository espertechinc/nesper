///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAccessAggMinMaxBySorted
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        config.AddEventType("S0", typeof(SupportBean_S0));
	        config.AddEventType("S1", typeof(SupportBean_S1));
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
	    public void TestGroupedSortedMinMax() {
	        var epl = "select " +
	                "window(*) as c0, " +
	                "sorted(IntPrimitive desc) as c1, " +
	                "sorted(IntPrimitive asc) as c2, " +
	                "maxby(IntPrimitive) as c3, " +
	                "minby(IntPrimitive) as c4, " +
	                "maxbyever(IntPrimitive) as c5, " +
	                "minbyever(IntPrimitive) as c6 " +
	                "from SupportBean#groupwin(LongPrimitive)#length(3) " +
	                "group by LongPrimitive";
	        var stmtPlain = _epService.EPAdministrator.CreateEPL(epl);
	        stmtPlain.AddListener(_listener);

	        RunAssertionGroupedSortedMinMax();
	        stmtPlain.Dispose();

	        // test SODA
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        var stmtSoda = _epService.EPAdministrator.Create(model);
	        stmtSoda.AddListener(_listener);
	        Assert.AreEqual(epl, stmtSoda.Text);
	        RunAssertionGroupedSortedMinMax();
            stmtSoda.Dispose();

	        // test join
	        var eplJoin = "select " +
	                "window(sb.*) as c0, " +
	                "sorted(IntPrimitive desc) as c1, " +
	                "sorted(IntPrimitive asc) as c2, " +
	                "maxby(IntPrimitive) as c3, " +
	                "minby(IntPrimitive) as c4, " +
	                "maxbyever(IntPrimitive) as c5, " +
	                "minbyever(IntPrimitive) as c6 " +
	                "from S0#lastevent, SupportBean#groupwin(LongPrimitive)#length(3) as sb " +
	                "group by LongPrimitive";
	        var stmtJoin = _epService.EPAdministrator.CreateEPL(eplJoin);
	        stmtJoin.AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "p00"));
	        RunAssertionGroupedSortedMinMax();
            stmtJoin.Dispose();

	        // test join multirow
	        var fields = "c0".Split(',');
	        var joinMultirow = "select sorted(IntPrimitive desc) as c0 from S0#keepall, SupportBean#length(2)";
	        var stmtJoinMultirow = _epService.EPAdministrator.CreateEPL(joinMultirow);
	        stmtJoinMultirow.AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S2"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S3"));

	        var eventOne = new SupportBean("E1", 1);
	        _epService.EPRuntime.SendEvent(eventOne);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{new object[]{eventOne}});

	        var eventTwo = new SupportBean("E2", 2);
	        _epService.EPRuntime.SendEvent(eventTwo);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{new object[]{eventTwo, eventOne}});

	        var eventThree = new SupportBean("E3", 0);
	        _epService.EPRuntime.SendEvent(eventThree);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{new object[]{eventTwo, eventThree}});
	    }

	    private void RunAssertionGroupedSortedMinMax() {

	        var fields = "c0,c1,c2,c3,c4,c5,c6".Split(',');
	        var eventOne = MakeEvent("E1", 1, 1);
	        _epService.EPRuntime.SendEvent(eventOne);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventOne},
	                        new object[] {eventOne},
	                        new object[] {eventOne},
	                        eventOne, eventOne, eventOne, eventOne});

	        var eventTwo = MakeEvent("E2", 2, 1);
	        _epService.EPRuntime.SendEvent(eventTwo);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventOne, eventTwo},
	                        new object[] {eventTwo, eventOne},
	                        new object[] {eventOne, eventTwo},
	                        eventTwo, eventOne, eventTwo, eventOne});

	        var eventThree = MakeEvent("E3", 0, 1);
	        _epService.EPRuntime.SendEvent(eventThree);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventOne, eventTwo, eventThree},
	                        new object[] {eventTwo, eventOne, eventThree},
	                        new object[] {eventThree, eventOne, eventTwo},
	                        eventTwo, eventThree, eventTwo, eventThree});

	        var eventFour = MakeEvent("E4", 3, 1);   // pushes out E1
	        _epService.EPRuntime.SendEvent(eventFour);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventTwo, eventThree, eventFour},
	                        new object[] {eventFour, eventTwo, eventThree},
	                        new object[] {eventThree, eventTwo, eventFour},
	                        eventFour, eventThree, eventFour, eventThree});

	        var eventFive = MakeEvent("E5", -1, 2);   // group 2
	        _epService.EPRuntime.SendEvent(eventFive);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventFive},
	                        new object[] {eventFive},
	                        new object[] {eventFive},
	                        eventFive, eventFive, eventFive, eventFive});

	        var eventSix = MakeEvent("E6", -1, 1);   // pushes out E2
	        _epService.EPRuntime.SendEvent(eventSix);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventThree, eventFour, eventSix},
	                        new object[] {eventFour, eventThree, eventSix},
	                        new object[] {eventSix, eventThree, eventFour},
	                        eventFour, eventSix, eventFour, eventSix});

	        var eventSeven = MakeEvent("E7", 2, 2);   // group 2
	        _epService.EPRuntime.SendEvent(eventSeven);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{
	                        new object[] {eventFive, eventSeven},
	                        new object[] {eventSeven, eventFive},
	                        new object[] {eventFive, eventSeven},
	                        eventSeven, eventFive, eventSeven, eventFive});

	    }

        [Test]
	    public void TestMinByMaxByOverWindow() {
	        var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".Split(',');
	        var epl = "select " +
	                "maxbyever(LongPrimitive) as c0, " +
	                "minbyever(LongPrimitive) as c1, " +
	                "maxby(LongPrimitive).LongPrimitive as c2, " +
	                "maxby(LongPrimitive).TheString as c3, " +
	                "maxby(LongPrimitive).IntPrimitive as c4, " +
	                "maxby(LongPrimitive) as c5, " +
	                "minby(LongPrimitive).LongPrimitive as c6, " +
	                "minby(LongPrimitive).TheString as c7, " +
	                "minby(LongPrimitive).IntPrimitive as c8, " +
	                "minby(LongPrimitive) as c9 " +
	                "from SupportBean#length(5)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        var eventOne = MakeEvent("E1", 1, 10);
	        _epService.EPRuntime.SendEvent(eventOne);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{eventOne, eventOne, 10L, "E1", 1, eventOne, 10L, "E1", 1, eventOne});

	        var eventTwo = MakeEvent("E2", 2, 20);
	        _epService.EPRuntime.SendEvent(eventTwo);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{eventTwo, eventOne, 20L, "E2", 2, eventTwo, 10L, "E1", 1, eventOne});

	        var eventThree = MakeEvent("E3", 3, 5);
	        _epService.EPRuntime.SendEvent(eventThree);
	        var resultThree = new object[]{eventTwo, eventThree, 20L, "E2", 2, eventTwo, 5L, "E3", 3, eventThree};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultThree);

	        var eventFour = MakeEvent("E4", 4, 5);
	        _epService.EPRuntime.SendEvent(eventFour); // same as E3
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultThree);

	        var eventFive = MakeEvent("E5", 5, 20);
	        _epService.EPRuntime.SendEvent(eventFive); // same as E2
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultThree);

	        var eventSix = MakeEvent("E6", 6, 10);
	        _epService.EPRuntime.SendEvent(eventSix); // expires E1
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultThree);

	        var eventSeven = MakeEvent("E7", 7, 20);
	        _epService.EPRuntime.SendEvent(eventSeven); // expires E2
	        var resultSeven = new object[]{eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E3", 3, eventThree};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultSeven);

	        _epService.EPRuntime.SendEvent(MakeEvent("E8", 8, 20)); // expires E3
	        var resultEight = new object[]{eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E4", 4, eventFour};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultEight);

	        _epService.EPRuntime.SendEvent(MakeEvent("E9", 9, 19)); // expires E4
	        var resultNine = new object[]{eventTwo, eventThree, 20L, "E5", 5, eventFive, 10L, "E6", 6, eventSix};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultNine);

	        _epService.EPRuntime.SendEvent(MakeEvent("E10", 10, 12)); // expires E5
	        var resultTen = new object[]{eventTwo, eventThree, 20L, "E7", 7, eventSeven, 10L, "E6", 6, eventSix};
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, resultTen);
	    }

        [Test]
	    public void TestNoAlias() {
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
	                "maxby(IntPrimitive).TheString, " +
	                "minby(IntPrimitive)," +
	                "maxbyever(IntPrimitive).TheString, " +
	                "minbyever(IntPrimitive)," +
	                "sorted(IntPrimitive asc, TheString desc)" +
	                " from SupportBean#time(10)");

	        var props = stmt.EventType.PropertyDescriptors;
	        Assert.AreEqual("maxby(IntPrimitive).TheString()", props[0].PropertyName);
	        Assert.AreEqual("minby(IntPrimitive)", props[1].PropertyName);
	        Assert.AreEqual("maxbyever(IntPrimitive).TheString()", props[2].PropertyName);
	        Assert.AreEqual("minbyever(IntPrimitive)", props[3].PropertyName);
	        Assert.AreEqual("sorted(IntPrimitive,TheString desc)", props[4].PropertyName);
	    }

        [Test]
	    public void TestMultipleOverlappingCategories() {
	        var fields = "c0,c1,c2,c3,c4,c5,c6,c7".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
	                "maxbyever(IntPrimitive).LongPrimitive as c0," +
	                "maxbyever(TheString).LongPrimitive as c1," +
	                "minbyever(IntPrimitive).LongPrimitive as c2," +
	                "minbyever(TheString).LongPrimitive as c3," +
	                "maxby(IntPrimitive).LongPrimitive as c4," +
	                "maxby(TheString).LongPrimitive as c5," +
	                "minby(IntPrimitive).LongPrimitive as c6," +
	                "minby(TheString).LongPrimitive as c7 " +
	                "from SupportBean#keepall");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("C", 10, 1L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L});

	        _epService.EPRuntime.SendEvent(MakeEvent("P", 5, 2L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});

	        _epService.EPRuntime.SendEvent(MakeEvent("G", 7, 3L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});

	        _epService.EPRuntime.SendEvent(MakeEvent("A", 7, 4L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1L, 2L, 2L, 4L, 1L, 2L, 2L, 4L});

	        _epService.EPRuntime.SendEvent(MakeEvent("G", 1, 5L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1L, 2L, 5L, 4L, 1L, 2L, 5L, 4L});

	        _epService.EPRuntime.SendEvent(MakeEvent("X", 7, 6L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{1L, 6L, 5L, 4L, 1L, 6L, 5L, 4L});

	        _epService.EPRuntime.SendEvent(MakeEvent("G", 100, 7L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{7L, 6L, 5L, 4L, 7L, 6L, 5L, 4L});

	        _epService.EPRuntime.SendEvent(MakeEvent("Z", 1000, 8L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
	                new object[]{8L, 8L, 5L, 4L, 8L, 8L, 5L, 4L});
	    }

        [Test]
	    public void TestMultipleCriteria() {
	        // test sorted multiple criteria
	        var fields = "c0,c1,c2,c3".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
	                "sorted(TheString desc, IntPrimitive desc) as c0," +
	                "sorted(TheString, IntPrimitive) as c1," +
	                "sorted(TheString asc, IntPrimitive asc) as c2," +
	                "sorted(TheString desc, IntPrimitive asc) as c3 " +
	                "from SupportBean#keepall");
	        stmt.AddListener(_listener);

	        var eventOne = new SupportBean("C", 10);
	        _epService.EPRuntime.SendEvent(eventOne);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[][] {
	                new object[] {eventOne},
	                new object[] {eventOne},
	                new object[] {eventOne},
	                new object[] {eventOne}});

	        var eventTwo = new SupportBean("D", 20);
	        _epService.EPRuntime.SendEvent(eventTwo);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[][] {
	                new object[] {eventTwo, eventOne},
	                new object[] {eventOne, eventTwo},
	                new object[] {eventOne, eventTwo},
	                new object[] {eventTwo, eventOne}});

	        var eventThree = new SupportBean("C", 15);
	        _epService.EPRuntime.SendEvent(eventThree);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[][] {
	                new object[] {eventTwo, eventThree, eventOne},
	                new object[] {eventOne, eventThree, eventTwo},
	                new object[] {eventOne, eventThree, eventTwo},
	                new object[] {eventTwo, eventOne, eventThree}});

	        var eventFour = new SupportBean("D", 19);
	        _epService.EPRuntime.SendEvent(eventFour);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[][] {
	                new object[] {eventTwo, eventFour, eventThree, eventOne},
	                new object[] {eventOne, eventThree, eventFour, eventTwo},
	                new object[] {eventOne, eventThree, eventFour, eventTwo},
	                new object[] {eventFour, eventTwo, eventOne, eventThree}});

            stmt.Dispose();

	        // test min/max
	        var fieldsTwo = "c0,c1,c2,c3,c4,c5,c6,c7".Split(',');
	        var stmtTwo = _epService.EPAdministrator.CreateEPL("select " +
	                "maxbyever(IntPrimitive, TheString).LongPrimitive as c0," +
	                "minbyever(IntPrimitive, TheString).LongPrimitive as c1," +
	                "maxbyever(TheString, IntPrimitive).LongPrimitive as c2," +
	                "minbyever(TheString, IntPrimitive).LongPrimitive as c3," +
	                "maxby(IntPrimitive, TheString).LongPrimitive as c4," +
	                "minby(IntPrimitive, TheString).LongPrimitive as c5," +
	                "maxby(TheString, IntPrimitive).LongPrimitive as c6," +
	                "minby(TheString, IntPrimitive).LongPrimitive as c7 " +
	                "from SupportBean#keepall");
	        stmtTwo.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("C", 10, 1L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo,
	                new object[]{1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L});

	        _epService.EPRuntime.SendEvent(MakeEvent("P", 5, 2L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo,
	                new object[]{1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});

	        _epService.EPRuntime.SendEvent(MakeEvent("C", 9, 3L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo,
	                new object[]{1L, 2L, 2L, 3L, 1L, 2L, 2L, 3L});

	        _epService.EPRuntime.SendEvent(MakeEvent("C", 11, 4L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo,
	                new object[]{4L, 2L, 2L, 3L, 4L, 2L, 2L, 3L});

	        _epService.EPRuntime.SendEvent(MakeEvent("X", 11, 5L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo,
	                new object[]{5L, 2L, 5L, 3L, 5L, 2L, 5L, 3L});

	        _epService.EPRuntime.SendEvent(MakeEvent("X", 0, 6L));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTwo,
	                new object[]{5L, 6L, 5L, 3L, 5L, 6L, 5L, 3L});
	    }

        [Test]
	    public void TestNoDataWindow() {
	        var fields = "c0,c1,c2,c3".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
	                "maxbyever(IntPrimitive).TheString as c0, " +
	                "minbyever(IntPrimitive).TheString as c1, " +
	                "maxby(IntPrimitive).TheString as c2, " +
	                "minby(IntPrimitive).TheString as c3 " +
	                "from SupportBean");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", "E1", "E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E1", "E2", "E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E3", "E2", "E3"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "E3", "E4", "E3"});
	    }

        [Test]
	    public void TestInvalid() {
	        TryInvalid("select maxBy(p00||p10) from S0#lastevent, S1#lastevent",
	                "Error starting statement: Failed to validate select-clause expression 'maxby(p00||p10)': The 'maxby' aggregation function requires that any parameter expressions evaluate properties of the same stream [select maxBy(p00||p10) from S0#lastevent, S1#lastevent]");

	        TryInvalid("select sorted(p00) from S0",
	                "Error starting statement: Failed to validate select-clause expression 'sorted(p00)': The 'sorted' aggregation function requires that a data window is declared for the stream [select sorted(p00) from S0]");
	    }

	    private void TryInvalid(string epl, string message)
        {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive)
        {
            var @event = new SupportBean(theString, intPrimitive);
	        @event.LongPrimitive = longPrimitive;
	        return @event;
	    }
	}
} // end of namespace
