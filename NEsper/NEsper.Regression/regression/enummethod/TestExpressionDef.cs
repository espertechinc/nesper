///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
	public class TestExpressionDef
    {
        private readonly string NEWLINE = Environment.NewLine;

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        config.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
	        config.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
	        config.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
	        config.AddEventType("SupportCollection", typeof(SupportCollection));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNestedExpressionMultiSubquery() {
	        var fields = "c0".Split(',');
	        _epService.EPAdministrator.CreateEPL("create expression F1 { (select IntPrimitive from SupportBean#lastevent)}");
	        _epService.EPAdministrator.CreateEPL("create expression F2 { param => (select a.IntPrimitive from SupportBean#unique(TheString) as a where a.TheString = param.TheString) }");
	        _epService.EPAdministrator.CreateEPL("create expression F3 { s => F1()+F2(s) }");
	        _epService.EPAdministrator.CreateEPL("select F3(myevent) as c0 from SupportBean as myevent").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {20});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {22});
	    }

        [Test]
	    public void TestWildcardAndPattern() {
	        var eplNonJoin =
	                "expression abc { x => IntPrimitive } " +
	                "expression def { (x, y) => x.IntPrimitive * y.IntPrimitive }" +
	                "select abc(*) as c0, def(*, *) as c1 from SupportBean";
	        _epService.EPAdministrator.CreateEPL(eplNonJoin).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0, c1".Split(','), new object[]{2, 4});
	        _epService.EPAdministrator.DestroyAllStatements();

	        var eplPattern = "expression abc { x => IntPrimitive * 2} " +
	                "select * from pattern [a=SupportBean -> b=SupportBean(IntPrimitive = abc(a))]";
	        _epService.EPAdministrator.CreateEPL(eplPattern).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "a.TheString, b.TheString".Split(','), new object[]{"E1", "E2"});
	    }

        [Test]
	    public void TestSequenceAndNested() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
	        _epService.EPAdministrator.CreateEPL("create window WindowOne#keepall as (col1 string, col2 string)");
	        _epService.EPAdministrator.CreateEPL("insert into WindowOne select P00 as col1, P01 as col2 from SupportBean_S0");

	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S1>();
	        _epService.EPAdministrator.CreateEPL("create window WindowTwo#keepall as (col1 string, col2 string)");
	        _epService.EPAdministrator.CreateEPL("insert into WindowTwo select P10 as col1, P11 as col2 from SupportBean_S1");

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "B1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A", "B2"));

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A", "B1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A", "B2"));

	        var epl =
	                "@Audit('exprdef') " +
	                "expression last2X {\n" +
	                "  p => WindowOne(WindowOne.col1 = p.TheString).takeLast(2)\n" +
	                "} " +
	                "expression last2Y {\n" +
	                "  p => WindowTwo(WindowTwo.col1 = p.TheString).takeLast(2).selectFrom(q => q.col2)\n" +
	                "} " +
	                "select last2X(sb).selectFrom(a => a.col2).sequenceEqual(last2Y(sb)) as val from SupportBean as sb";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
	        Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("val"));
	    }

        [Test]
	    public void TestCaseNewMultiReturnNoElse() {

	        var fieldsInner = "col1,col2".Split(',');
	        var epl = "expression gettotal {" +
	                " x => case " +
	                "  when TheString = 'A' then new { col1 = 'X', col2 = 10 } " +
	                "  when TheString = 'B' then new { col1 = 'Y', col2 = 20 } " +
	                "end" +
	                "} " +
	                "insert into OtherStream select gettotal(sb) as val0 from SupportBean sb";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(IDictionary<string,object>), stmt.EventType.GetPropertyType("val0"));

	        var listenerTwo = new SupportUpdateListener();
	        _epService.EPAdministrator.CreateEPL("select val0.col1 as c1, val0.col2 as c2 from OtherStream").AddListener(listenerTwo);
	        var fieldsConsume = "c1,c2".Split(',');

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[] { null, null });
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsConsume, new object[]{null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("A", 2));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[] { "X", 10 });
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsConsume, new object[]{"X", 10});

	        _epService.EPRuntime.SendEvent(new SupportBean("B", 3));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("val0"), fieldsInner, new object[] { "Y", 20 });
	        EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsConsume, new object[]{"Y", 20});
	    }

        [Test]
	    public void TestAnnotationOrder() {
	        var epl = "expression scalar {1} @Name('test') select scalar() from SupportBean_ST0";
	        RunAssertionAnnotation(epl);

	        epl = "@Name('test') expression scalar {1} select scalar() from SupportBean_ST0";
	        RunAssertionAnnotation(epl);
	    }

	    private void RunAssertionAnnotation(string epl) {
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("scalar()"));
	        Assert.AreEqual("test", stmt.Name);

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "scalar()".Split(','), new object[]{1});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestSubqueryMultiresult() {
	        var eplOne = "" +
	                "expression maxi {" +
	                " (select max(IntPrimitive) from SupportBean#keepall)" +
	                "} " +
	                "expression mini {" +
	                " (select min(IntPrimitive) from SupportBean#keepall)" +
	                "} " +
	                "select p00/maxi() as val0, p00/mini() as val1 " +
	                "from SupportBean_ST0#lastevent";
	        RunAssertionMultiResult(eplOne);

	        var eplTwo = "" +
	                "expression subq {" +
	                " (select max(IntPrimitive) as maxi, min(IntPrimitive) as mini from SupportBean#keepall)" +
	                "} " +
	                "select p00/subq().maxi as val0, p00/subq().mini as val1 " +
	                "from SupportBean_ST0#lastevent";
	        RunAssertionMultiResult(eplTwo);

	        var eplTwoAlias = "" +
	                "expression subq alias for " +
	                " { (select max(IntPrimitive) as maxi, min(IntPrimitive) as mini from SupportBean#keepall) }" +
	                " " +
	                "select p00/subq().maxi as val0, p00/subq().mini as val1 " +
	                "from SupportBean_ST0#lastevent";
	        RunAssertionMultiResult(eplTwoAlias);
	    }

	    private void RunAssertionMultiResult(string epl) {
	        var fields = new string[] { "val0","val1"};

	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 2));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2 / 10d, 2 / 5d});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{4 / 20d, 4 / 2d});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestSubqueryCross() {
	        var eplDeclare = "expression subq {" +
	                " (x, y) => (select TheString from SupportBean#keepall where TheString = x.id and IntPrimitive = y.p10)" +
	                "} " +
	                "select subq(one, two) as val1 " +
	                "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
	        RunAssertionSubqueryCross(eplDeclare);

	        var eplAlias = "expression subq alias for { (select TheString from SupportBean#keepall where TheString = one.id and IntPrimitive = two.p10) }" +
	                "select subq as val1 " +
	                "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
	        RunAssertionSubqueryCross(eplAlias);
	    }

	    private void RunAssertionSubqueryCross(string epl)
	    {
	        var fields = new string[] { "val1" };
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(string)});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        _epService.EPRuntime.SendEvent(new SupportBean("ST0", 20));

	        _epService.EPRuntime.SendEvent(new SupportBean_ST1("x", 20));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"ST0"});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestSubqueryJoinSameField() {
	        var eplDeclare = "" +
	                "expression subq {" +
	                " x => (select IntPrimitive from SupportBean#keepall where TheString = x.pcommon)" +   // a common field
	                "} " +
	                "select subq(one) as val1, subq(two) as val2 " +
	                "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
	        RunAssertionSubqueryJoinSameField(eplDeclare);

	        var eplAlias = "" +
	                "expression subq alias for {(select IntPrimitive from SupportBean#keepall where TheString = pcommon) }" +
	                "select subq as val1, subq as val2 " +
	                "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
	        TryInvalid(eplAlias,
	                "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Failed to validate filter expression 'TheString=pcommon': Property named 'pcommon' is ambiguous as is valid for more then one stream [expression subq alias for {(select IntPrimitive from SupportBean#keepall where TheString = pcommon) }select subq as val1, subq as val2 from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two]");
	    }

	    private void RunAssertionSubqueryJoinSameField(string epl)
	    {
	        var fields = new string[] { "val1", "val2"};
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?)});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", 0, "E0"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, 10});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0, "E0"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestSubqueryCorrelated() {
	        var eplDeclare = "expression subqOne {" +
	                " x => (select id from SupportBean_ST0#keepall where p00 = x.IntPrimitive)" +
	                "} " +
	                "select TheString as val0, subqOne(t) as val1 from SupportBean as t";
	        RunAssertionSubqueryCorrelated(eplDeclare);

	        var eplAlias = "expression subqOne alias for {(select id from SupportBean_ST0#keepall where p00 = t.IntPrimitive)} " +
	                "select TheString as val0, subqOne(t) as val1 from SupportBean as t";
	        RunAssertionSubqueryCorrelated(eplAlias);
	    }

	    private void RunAssertionSubqueryCorrelated(string epl) {
	        var fields = new string[] { "val0", "val1"};
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(string), typeof(string)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E0", null});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 100));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 99));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "ST0"});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST1", 100));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", null});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestSubqueryUncorrelated() {
	        var eplDeclare = "expression subqOne {(select id from SupportBean_ST0#lastevent)} " +
	                "select TheString as val0, subqOne() as val1 from SupportBean as t";
	        RunAssertionSubqueryUncorrelated(eplDeclare);

	        var eplAlias = "expression subqOne alias for {(select id from SupportBean_ST0#lastevent)} " +
	                "select TheString as val0, subqOne as val1 from SupportBean as t";
	        RunAssertionSubqueryUncorrelated(eplAlias);
	    }

	    private void RunAssertionSubqueryUncorrelated(string epl)
        {
	        var fields = new string[] { "val0", "val1"};
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(string), typeof(string)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E0", null});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 99));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "ST0"});

	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST1", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "ST1"});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestSubqueryNamedWindowUncorrelated()
        {
	        var eplDeclare = "expression subqnamedwin { MyWindow.where(x => x.val1 > 10).orderBy(x => x.val0) } " +
	                "select subqnamedwin() as c0, subqnamedwin().where(x => x.val1 < 100) as c1 from SupportBean_ST0 as t";
	        RunAssertionSubqueryNamedWindowUncorrelated(eplDeclare);

	        var eplAlias = "expression subqnamedwin alias for {MyWindow.where(x => x.val1 > 10).orderBy(x => x.val0)}" +
	                "select subqnamedwin as c0, subqnamedwin.where(x => x.val1 < 100) as c1 from SupportBean_ST0";
	        RunAssertionSubqueryNamedWindowUncorrelated(eplAlias);
	    }

	    private void RunAssertionSubqueryNamedWindowUncorrelated(string epl)
        {
	        var fieldsSelected = "c0,c1".Split(',');
	        var fieldsInside = "val0".Split(',');

	        _epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create window MyWindow#keepall as (val0 string, val1 int)");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow (val0, val1) select TheString, IntPrimitive from SupportBean");

	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fieldsSelected, new Type[]{typeof(ICollection<DataMap>), typeof(ICollection<DataMap>)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID0", 0));
	        EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) _listener.AssertOneGetNew().Get("c0")), fieldsInside, null);
	        EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) _listener.AssertOneGetNew().Get("c1")), fieldsInside, null);
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>)_listener.AssertOneGetNew().Get("c0")), fieldsInside, new object[][] { new object[] { "E1" } });
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>)_listener.AssertOneGetNew().Get("c1")), fieldsInside, new object[][] { new object[] { "E1" } });
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 500));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID2", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>)_listener.AssertOneGetNew().Get("c0")), fieldsInside, new object[][] { new object[] { "E1" }, new object[] { "E2" } });
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>)_listener.AssertOneGetNew().Get("c1")), fieldsInside, new object[][] { new object[] { "E1" } });
	        _listener.Reset();

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestSubqueryNamedWindowCorrelated() {

	        var epl =    "expression subqnamedwin {" +
	                        "  x => MyWindow(val0 = x.key0).where(y => val1 > 10)" +
	                        "} " +
	                        "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
	        RunAssertionSubqNWCorrelated(epl);

	        // more or less prefixes
	        epl =           "expression subqnamedwin {" +
	                        "  x => MyWindow(val0 = x.key0).where(y => y.val1 > 10)" +
	                        "} " +
	                        "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
	        RunAssertionSubqNWCorrelated(epl);

	        // with property-explicit stream name
	        epl =    "expression subqnamedwin {" +
	                        "  x => MyWindow(MyWindow.val0 = x.key0).where(y => y.val1 > 10)" +
	                        "} " +
	                        "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
	        RunAssertionSubqNWCorrelated(epl);

	        // with alias
	        epl =   "expression subqnamedwin alias for {MyWindow(MyWindow.val0 = t.key0).where(y => y.val1 > 10)}" +
	                "select subqnamedwin as c0 from SupportBean_ST0 as t";
	        RunAssertionSubqNWCorrelated(epl);

	        // test ambiguous property names
	        _epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create window MyWindowTwo#keepall as (id string, p00 int)");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindowTwo (id, p00) select TheString, IntPrimitive from SupportBean");
	        epl =    "expression subqnamedwin {" +
	                        "  x => MyWindowTwo(MyWindowTwo.id = x.id).where(y => y.p00 > 10)" +
	                        "} " +
	                        "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
	        _epService.EPAdministrator.CreateEPL(epl);
	    }

	    private void RunAssertionSubqNWCorrelated(string epl) {
	        var fieldSelected = "c0".Split(',');
	        var fieldInside = "val0".Split(',');

	        _epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create window MyWindow#keepall as (val0 string, val1 int)");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow (val0, val1) select TheString, IntPrimitive from SupportBean");
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fieldSelected, new Type[]{typeof(ICollection<IDictionary<string, object>>)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID0", "x", 0));
	        EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) _listener.AssertOneGetNew().Get("c0")), fieldInside, null);
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID1", "x", 0));
	        EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>) _listener.AssertOneGetNew().Get("c0")), fieldInside, null);
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID2", "E2", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>)_listener.AssertOneGetNew().Get("c0")), fieldInside, new object[][] { new object[] { "E2" } });
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 13));
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", "E3", 0));
            EPAssertionUtil.AssertPropsPerRow(ToArrayMap((ICollection<object>)_listener.AssertOneGetNew().Get("c0")), fieldInside, new object[][] { new object[] { "E3" } });
	        _listener.Reset();

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestAggregationNoAccess() {
	        var fields = new string[] { "val1", "val2", "val3", "val4"};
	        var epl = "" +
	                "expression sumA {x => " +
	                "   sum(x.IntPrimitive) " +
	                "} " +
	                "expression sumB {x => " +
	                "   sum(x.IntBoxed) " +
	                "} " +
	                "expression countC {" +
	                "   count(*) " +
	                "} " +
	                "select sumA(t) as val1, sumB(t) as val2, sumA(t)/sumB(t) as val3, countC() as val4 from SupportBean as t";

	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?), typeof(double?), typeof(long?)});

	        _epService.EPRuntime.SendEvent(GetSupportBean(5, 6));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5, 6, 5 / 6d, 1L});

	        _epService.EPRuntime.SendEvent(GetSupportBean(8, 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5 + 8, 6 + 10, (5 + 8) / (6d + 10d), 2L});
	    }

        [Test]
	    public void TestSplitStream() {
	        var epl =  "expression myLittleExpression { event => false }" +
	                      "on SupportBean as myEvent " +
	                      " insert into ABC select * where myLittleExpression(myEvent)" +
	                      " insert into DEF select * where not myLittleExpression(myEvent)";
	        _epService.EPAdministrator.CreateEPL(epl);

	        _epService.EPAdministrator.CreateEPL("select * from DEF").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean());
	        Assert.IsTrue(_listener.IsInvoked);
	    }

        [Test]
	    public void TestAggregationAccess() {
	        var eplDeclare = "expression wb {s => window(*).where(y => y.IntPrimitive > 2) }" +
	                "select wb(t) as val1 from SupportBean#keepall as t";
	        RunAssertionAggregationAccess(eplDeclare);

	        var eplAlias = "expression wb alias for {window(*).where(y => y.IntPrimitive > 2)}" +
	                "select wb as val1 from SupportBean#keepall as t";
	        RunAssertionAggregationAccess(eplAlias);
	    }

        [Test]
        public void TestAggregatedResult()
        {
            var fields = "c0,c1".SplitCsv();
            var epl =
                    "expression lambda1 { o => 1 * o.intPrimitive }\n" +
                    "expression lambda2 { o => 3 * o.intPrimitive }\n" +
                    "select sum(lambda1(e)) as c0, sum(lambda2(e)) as c1 from SupportBean as e";
            _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 10, 30 });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 15, 45 });
        }

	    private void RunAssertionAggregationAccess(string epl) {

	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, "val1".Split(','), new Type[]{typeof(ICollection<SupportBean>), typeof(ICollection<SupportBean>)});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            var outArray = _listener.AssertOneGetNewAndReset().Get("val1").UnwrapIntoArray<SupportBean>();
            Assert.AreEqual(0, outArray.Length);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            outArray = _listener.AssertOneGetNewAndReset().Get("val1").UnwrapIntoArray<SupportBean>();
	        Assert.AreEqual(1, outArray.Length);
	        Assert.AreEqual("E2", outArray[0].TheString);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestScalarReturn() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        var eplScalarDeclare = "expression scalarfilter {s => Strvals.where(y => y != 'E1') } " +
	                     "select scalarfilter(t).where(x => x != 'E2') as val1 from SupportCollection as t";
	        RunAssertionScalarReturn(eplScalarDeclare);

            var eplScalarAlias = "expression scalarfilter alias for {Strvals.where(y => y != 'E1')}" +
	                "select scalarfilter.where(x => x != 'E2') as val1 from SupportCollection";
	        RunAssertionScalarReturn(eplScalarAlias);

	        // test with cast and with on-select and where-clause use
	        var inner = "case when myEvent.myObject = 'X' then 0 else cast(myEvent.myObject, long) end ";
	        var eplCaseDeclare = "expression theExpression { myEvent => " + inner + "} " +
	                "on MyEvent as myEvent select mw.* from MyWindow as mw where mw.myObject = theExpression(myEvent)";
	        RunAssertionNamedWindowCast(eplCaseDeclare);

	        var eplCaseAlias = "expression theExpression alias for {" + inner + "}" +
	                "on MyEvent as myEvent select mw.* from MyWindow as mw where mw.myObject = theExpression";
	        RunAssertionNamedWindowCast(eplCaseAlias);
	    }

	    private void RunAssertionNamedWindowCast(string epl) {

	        _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (myObject long)");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow(myObject) select cast(IntPrimitive, long) from SupportBean");
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        var props = new string[] { "myObject" };

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));

	        _epService.EPRuntime.SendEvent(new MyEvent(2));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new MyEvent("X"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), props, new object[] {0L});

	        _epService.EPRuntime.SendEvent(new MyEvent(1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), props, new object[] {1L});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionScalarReturn(string epl) {
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, "val1".Split(','), new Type[]{typeof(ICollection<string>)});

	        _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3,E4"));
	        LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E3", "E4");
	        _listener.Reset();

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestEventTypeAndSODA()
        {
	        var fields= new string[] {"fZero()", "fOne(t)", "fTwo(t,t)", "fThree(t,t)"};
	        var eplDeclared = "" +
	                "expression fZero {10} " +
	                "expression fOne {x => x.IntPrimitive} " +
	                "expression fTwo {(x,y) => x.IntPrimitive+y.IntPrimitive} " +
	                "expression fThree {(x,y) => x.IntPrimitive+100} " +
	                "select fZero(), fOne(t), fTwo(t,t), fThree(t,t) from SupportBean as t";
	        var eplFormatted = "" +
	                "expression fZero {10}" + NEWLINE +
	                "expression fOne {x => x.IntPrimitive}" + NEWLINE +
	                "expression fTwo {(x,y) => x.IntPrimitive+y.IntPrimitive}" + NEWLINE +
	                "expression fThree {(x,y) => x.IntPrimitive+100}" + NEWLINE +
	                "select fZero(), fOne(t), fTwo(t,t), fThree(t,t)" + NEWLINE +
	                "from SupportBean as t";
	        var stmt = _epService.EPAdministrator.CreateEPL(eplDeclared);
	        stmt.AddListener(_listener);

	        RunAssertionTwoParameterArithmetic(stmt, fields);

            stmt.Dispose();
	        var model = _epService.EPAdministrator.CompileEPL(eplDeclared);
	        Assert.AreEqual(eplDeclared, model.ToEPL());
	        Assert.AreEqual(eplFormatted, model.ToEPL(new EPStatementFormatter(true)));
	        stmt = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(eplDeclared, stmt.Text);
	        stmt.AddListener(_listener);

	        RunAssertionTwoParameterArithmetic(stmt, fields);
            stmt.Dispose();

	        var eplAlias = "" +
	                "expression fZero alias for {10} " +
	                "expression fOne alias for {IntPrimitive} " +
	                "expression fTwo alias for {IntPrimitive+IntPrimitive} " +
	                "expression fThree alias for {IntPrimitive+100} " +
	                "select fZero, fOne, fTwo, fThree from SupportBean";
	        var stmtAlias = _epService.EPAdministrator.CreateEPL(eplAlias);
	        stmtAlias.AddListener(_listener);
	        RunAssertionTwoParameterArithmetic(stmtAlias, new string[] {"fZero", "fOne", "fTwo", "fThree"});
	        stmtAlias.Dispose();
	    }

	    private void RunAssertionTwoParameterArithmetic(EPStatement stmt, string[] fields) {
	        var props = stmt.EventType.PropertyNames;
	        EPAssertionUtil.AssertEqualsAnyOrder(props, fields);
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(fields[0]));
	        Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType(fields[1]));
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(fields[2]));
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(fields[3]));
	        var getter = stmt.EventType.GetGetter(fields[3]);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNew(), fields, new object[]{10, 11, 22, 111});
	        Assert.AreEqual(111, getter.Get(_listener.AssertOneGetNewAndReset()));
	    }

        [Test]
	    public void TestOneParameterLambdaReturn() {

	        var eplDeclare = "" +
	                "expression one {x1 => x1.contained.where(y => y.p00 < 10) } " +
	                "expression two {x2 => one(x2).where(y => y.p00 > 1)  } " +
	                "select one(s0c) as val1, two(s0c) as val2 from SupportBean_ST0_Container as s0c";
	        RunAssertionOneParameterLambdaReturn(eplDeclare);

	        var eplAliasWParen = "" +
	         "expression one alias for {contained.where(y => y.p00 < 10)}" +
	         "expression two alias for {one().where(y => y.p00 > 1)}" +
	         "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
	        RunAssertionOneParameterLambdaReturn(eplAliasWParen);

	        var eplAliasNoParen = "" +
	                "expression one alias for {contained.where(y => y.p00 < 10)}" +
	                "expression two alias for {one.where(y => y.p00 > 1)}" +
	                "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
	        RunAssertionOneParameterLambdaReturn(eplAliasNoParen);
	    }

	    private void RunAssertionOneParameterLambdaReturn(string epl) {

	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, "val1,val2".Split(','), new Type[]{typeof(ICollection<SupportBean_ST0>), typeof(ICollection<SupportBean_ST0>)});

	        var theEvent = SupportBean_ST0_Container.Make3Value("E1,K1,1", "E2,K2,2", "E20,K20,20");
	        _epService.EPRuntime.SendEvent(theEvent);
	        var resultVal1 = ((ICollection<object>) _listener.LastNewData[0].Get("val1")).ToArray();
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{theEvent.Contained[0], theEvent.Contained[1]}, resultVal1
	        );
	        var resultVal2 = ((ICollection<object>) _listener.LastNewData[0].Get("val2")).ToArray();
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{theEvent.Contained[1]}, resultVal2
	        );

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestNoParameterArithmetic() {

	        var eplDeclared = "expression getEnumerationSource {1} " +
	                "select getEnumerationSource() as val1, getEnumerationSource()*5 as val2 from SupportBean";
	        RunAssertionNoParameterArithmetic(eplDeclared);

	        var eplDeclaredNoParen = "expression getEnumerationSource {1} " +
	                "select getEnumerationSource as val1, getEnumerationSource*5 as val2 from SupportBean";
	        RunAssertionNoParameterArithmetic(eplDeclaredNoParen);

	        var eplAlias = "expression getEnumerationSource alias for {1} " +
	                "select getEnumerationSource as val1, getEnumerationSource*5 as val2 from SupportBean";
	        RunAssertionNoParameterArithmetic(eplAlias);
	    }

	    private void RunAssertionNoParameterArithmetic(string epl) {

	        var fields= "val1,val2".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?)});

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, 5});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestNoParameterVariable() {
	        var eplDeclared = "expression one {myvar} " +
	                "expression two {myvar * 10} " +
	                "select one() as val1, two() as val2, one() * two() as val3 from SupportBean";
	        RunAssertionNoParameterVariable(eplDeclared);

	        var eplAlias = "expression one alias for {myvar} " +
	                "expression two alias for {myvar * 10} " +
	                "select one() as val1, two() as val2, one * two as val3 from SupportBean";
	        RunAssertionNoParameterVariable(eplAlias);
	    }

	    private void RunAssertionNoParameterVariable(string epl) {

	        _epService.EPAdministrator.CreateEPL("create variable int myvar = 2");

	        var fields= "val1,val2,val3".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(int?), typeof(int?), typeof(int?)});

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, 20, 40});

	        _epService.EPRuntime.SetVariableValue("myvar", 3);
	        _epService.EPRuntime.SendEvent(new SupportBean());
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3, 30, 90});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestWhereClauseExpression() {
	        var eplNoAlias = "expression one {x=>x.BoolPrimitive} select * from SupportBean as sb where one(sb)";
	        RunAssertionWhereClauseExpression(eplNoAlias);

	        var eplAlias = "expression one alias for {BoolPrimitive} select * from SupportBean as sb where one";
	        RunAssertionWhereClauseExpression(eplAlias);
	    }

	    private void RunAssertionWhereClauseExpression(string epl) {
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        var theEvent = new SupportBean();
	        theEvent.BoolPrimitive = true;
	        _epService.EPRuntime.SendEvent(theEvent);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestInvalid() {

	        var epl = "expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=IntPrimitive)} select abc() from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'p00=IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=IntPrimitive)} select abc() from SupportBean]");

	        epl = "expression abc {x=>Strvals.where(x=> x != 'E1')} select abc(str) from SupportCollection str";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc(str)': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'Strvals.where()': Error validating enumeration method 'where', the lambda-parameter name 'x' has already been declared in this context [expression abc {x=>Strvals.where(x=> x != 'E1')} select abc(str) from SupportCollection str]");

	        epl = "expression abc {avg(IntPrimitive)} select abc() from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'avg(IntPrimitive)': Property named 'IntPrimitive' is not valid in any stream [expression abc {avg(IntPrimitive)} select abc() from SupportBean]");

	        epl = "expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=sb.IntPrimitive)} select abc() from SupportBean sb";
	        TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'p00=sb.IntPrimitive': Failed to find a stream named 'sb' (did you mean 'st0'?) [expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=sb.IntPrimitive)} select abc() from SupportBean sb]");

	        epl = "expression abc {window(*)} select abc() from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'window(*)': The 'window' aggregation function requires that at least one stream is provided [expression abc {window(*)} select abc() from SupportBean]");

	        epl = "expression abc {x => IntPrimitive} select abc() from SupportBean";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc()': Parameter count mismatches for declared expression 'abc', expected 1 parameters but received 0 parameters [expression abc {x => IntPrimitive} select abc() from SupportBean]");

	        epl = "expression abc {IntPrimitive} select abc(sb) from SupportBean sb";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc(sb)': Parameter count mismatches for declared expression 'abc', expected 0 parameters but received 1 parameters [expression abc {IntPrimitive} select abc(sb) from SupportBean sb]");

	        epl = "expression abc {x=>} select abc(sb) from SupportBean sb";
	        TryInvalid(epl, "Incorrect syntax near '}' at line 1 column 19 near reserved keyword 'select' [expression abc {x=>} select abc(sb) from SupportBean sb]");

	        epl = "expression abc {IntPrimitive} select abc() from SupportBean sb";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [expression abc {IntPrimitive} select abc() from SupportBean sb]");

	        epl = "expression abc {x=>x} select abc(1) from SupportBean sb";
	        TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'abc(1)': Expression 'abc' requires a stream name as a parameter [expression abc {x=>x} select abc(1) from SupportBean sb]");

	        epl = "expression abc {x=>IntPrimitive} select * from SupportBean sb where abc(sb)";
	        TryInvalid(epl, "Filter expression not returning a boolean value: 'abc(sb)' [expression abc {x=>IntPrimitive} select * from SupportBean sb where abc(sb)]");

	        epl = "expression abc {x=>x.IntPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where abc(*)";
	        TryInvalid(epl, "Error validating expression: Failed to validate filter expression 'abc(*)': Expression 'abc' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead [expression abc {x=>x.IntPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where abc(*)]");
	    }

	    private SupportBean GetSupportBean(int intPrimitive, int? intBoxed) {
	        var b = new SupportBean(null, intPrimitive);
	        b.IntBoxed = intBoxed;
	        return b;
	    }

	    private void TryInvalid(string epl, string message) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private SupportBean[] ToArray(ICollection<object> it) {
	        IList<SupportBean> result = new List<SupportBean>();
	        foreach (var item in it) {
	            result.Add((SupportBean) item);
	        }
	        return result.ToArray();
	    }

        private IDictionary<string, object>[] ToArrayMap(ICollection<object> it)
        {
	        if (it == null) {
	            return null;
	        }
	        var result = new List<IDictionary<string,object>>();
	        foreach (var item in it) {
	            var map = (IDictionary<string,object>) item;
	            result.Add(map);
	        }
	        return result.ToArray();
	    }

	    public class MyEvent
        {
	        public MyEvent(object myObject)
            {
	            MyObject = myObject;
	        }

	        public object MyObject { get; private set; }
        }

	}
} // end of namespace
