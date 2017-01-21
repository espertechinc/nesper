///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.subscriber;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using DataMap = IDictionary<string, object>;
    
    [TestFixture]
	public class TestSubscriberBind 
	{
	    private EPServiceProvider _epService;
	    private readonly string[] _fields = "TheString,IntPrimitive".SplitCsv();

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
            var @namespace = typeof (SupportBean).Namespace;
	        config.AddEventTypeAutoName(@namespace);
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestBindings()
        {
	        // just wildcard
	        var stmtJustWildcard = _epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString='E2')");
	        RunAssertionJustWildcard(stmtJustWildcard, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionJustWildcard(stmtJustWildcard, new SupportSubscriberRowByRowSpecificWStmt());
	        stmtJustWildcard.Dispose();

	        // wildcard with props
	        var stmtWildcardWProps = _epService.EPAdministrator.CreateEPL("select *, IntPrimitive + 2, 'x'||TheString||'x' from " + typeof(SupportBean).FullName);
	        RunAssertionWildcardWProps(stmtWildcardWProps, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionWildcardWProps(stmtWildcardWProps, new SupportSubscriberRowByRowSpecificWStmt());
	        stmtWildcardWProps.Dispose();

	        // nested
	        var stmtNested = _epService.EPAdministrator.CreateEPL("select nested, nested.nestedNested from SupportBeanComplexProps");
	        RunAssertionNested(stmtNested, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionNested(stmtNested, new SupportSubscriberRowByRowSpecificWStmt());
	        stmtNested.Dispose();

	        // enum
	        var stmtEnum = _epService.EPAdministrator.CreateEPL("select TheString, supportEnum from SupportBeanWithEnum");
	        RunAssertionEnum(stmtEnum, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionEnum(stmtEnum, new SupportSubscriberRowByRowSpecificWStmt());
	        stmtEnum.Dispose();

	        // null-typed select value
	        var stmtNullSelected = _epService.EPAdministrator.CreateEPL("select null, longBoxed from SupportBean");
	        RunAssertionNullSelected(stmtNullSelected, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionNullSelected(stmtNullSelected, new SupportSubscriberRowByRowSpecificWStmt());
	        stmtNullSelected.Dispose();

	        // widening
	        RunAssertionWidening(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionWidening(EventRepresentationEnum.MAP, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionWidening(EventRepresentationEnum.DEFAULT, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionWidening(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberRowByRowSpecificWStmt());
	        RunAssertionWidening(EventRepresentationEnum.MAP, new SupportSubscriberRowByRowSpecificWStmt());
	        RunAssertionWidening(EventRepresentationEnum.DEFAULT, new SupportSubscriberRowByRowSpecificWStmt());

	        // r-stream select
	        RunAssertionRStreamSelect(new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionRStreamSelect(new SupportSubscriberRowByRowSpecificWStmt());

	        // stream-selected join
	        RunAssertionStreamSelectWJoin(new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionStreamSelectWJoin(new SupportSubscriberRowByRowSpecificWStmt());

	        // stream-wildcard join
	        RunAssertionStreamWildcardJoin(new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionStreamWildcardJoin(new SupportSubscriberRowByRowSpecificWStmt());

	        // bind wildcard join
	        RunAssertionBindWildcardJoin(new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionBindWildcardJoin(new SupportSubscriberRowByRowSpecificWStmt());

	        // output limit
	        RunAssertionOutputLimitNoJoin(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionOutputLimitNoJoin(EventRepresentationEnum.MAP, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionOutputLimitNoJoin(EventRepresentationEnum.DEFAULT, new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionOutputLimitNoJoin(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberRowByRowSpecificWStmt());
	        RunAssertionOutputLimitNoJoin(EventRepresentationEnum.MAP, new SupportSubscriberRowByRowSpecificWStmt());
	        RunAssertionOutputLimitNoJoin(EventRepresentationEnum.DEFAULT, new SupportSubscriberRowByRowSpecificWStmt());

	        // output limit join
	        RunAssertionOutputLimitJoin(new SupportSubscriberRowByRowSpecificNStmt());
	        RunAssertionOutputLimitJoin(new SupportSubscriberRowByRowSpecificWStmt());

	        // binding-to-map
	        RunAssertionBindMap(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberMultirowMapNStmt());
	        RunAssertionBindMap(EventRepresentationEnum.MAP, new SupportSubscriberMultirowMapNStmt());
	        RunAssertionBindMap(EventRepresentationEnum.DEFAULT, new SupportSubscriberMultirowMapNStmt());
	        RunAssertionBindMap(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberMultirowMapWStmt());
	        RunAssertionBindMap(EventRepresentationEnum.MAP, new SupportSubscriberMultirowMapWStmt());
	        RunAssertionBindMap(EventRepresentationEnum.DEFAULT, new SupportSubscriberMultirowMapWStmt());

	        // binding-to-objectarray
	        RunAssertionBindObjectArr(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberMultirowObjectArrayNStmt());
	        RunAssertionBindObjectArr(EventRepresentationEnum.MAP, new SupportSubscriberMultirowObjectArrayNStmt());
	        RunAssertionBindObjectArr(EventRepresentationEnum.DEFAULT, new SupportSubscriberMultirowObjectArrayNStmt());
	        RunAssertionBindObjectArr(EventRepresentationEnum.OBJECTARRAY, new SupportSubscriberMultirowObjectArrayWStmt());
	        RunAssertionBindObjectArr(EventRepresentationEnum.MAP, new SupportSubscriberMultirowObjectArrayWStmt());
	        RunAssertionBindObjectArr(EventRepresentationEnum.DEFAULT, new SupportSubscriberMultirowObjectArrayWStmt());

	        // binding-to-underlying-array
	        RunAssertionBindWildcardIRStream(new SupportSubscriberMultirowUnderlyingNStmt());
	        RunAssertionBindWildcardIRStream(new SupportSubscriberMultirowUnderlyingWStmt());

	        // Object[] and "Object..." binding
	        foreach (var rep in new EventRepresentationEnum[] {EventRepresentationEnum.OBJECTARRAY, EventRepresentationEnum.DEFAULT, EventRepresentationEnum.MAP}) {
	            RunAssertionObjectArrayDelivery(rep, new SupportSubscriberRowByRowObjectArrayPlainNStmt());
	            RunAssertionObjectArrayDelivery(rep, new SupportSubscriberRowByRowObjectArrayPlainWStmt());
	            RunAssertionObjectArrayDelivery(rep, new SupportSubscriberRowByRowObjectArrayVarargNStmt());
	            RunAssertionObjectArrayDelivery(rep, new SupportSubscriberRowByRowObjectArrayVarargWStmt());
	        }

	        // Map binding
	        foreach (var rep in new EventRepresentationEnum[] {EventRepresentationEnum.OBJECTARRAY, EventRepresentationEnum.DEFAULT, EventRepresentationEnum.MAP}) {
	            RunAssertionRowMapDelivery(rep, new SupportSubscriberRowByRowMapNStmt());
	            RunAssertionRowMapDelivery(rep, new SupportSubscriberRowByRowMapWStmt());
	        }

	        // static methods
	        RunAssertionStaticMethod();

	        // IR stream individual calls
	        RunAssertionBindUpdateIRStream(new SupportSubscriberRowByRowFullNStmt());
	        RunAssertionBindUpdateIRStream(new SupportSubscriberRowByRowFullWStmt());

	        // no-params subscriber
	        var stmtNoParamsSubscriber = _epService.EPAdministrator.CreateEPL("select null from SupportBean");
	        RunAssertionNoParams(stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseNStmt());
	        RunAssertionNoParams(stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseWStmt());
	        stmtNoParamsSubscriber.Dispose();

	        // named-method subscriber
	        var stmtNamedMethod = _epService.EPAdministrator.CreateEPL("select TheString from SupportBean");
	        RunAsserionNamedMethod(stmtNamedMethod, new SupportSubscriberMultirowUnderlyingNamedMethodNStmt());
	        RunAsserionNamedMethod(stmtNamedMethod, new SupportSubscriberMultirowUnderlyingNamedMethodWStmt());
	        stmtNamedMethod.Dispose();

	        // prefer the EPStatement-footprint over the non-EPStatement footprint
	        RunAssertionPreferEPStatement();
	    }

        [Test]
	    public void TestSubscriberAndListener()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.CreateEPL("insert into A1 select s.*, 1 as a from SupportBean as s");
	        var stmt = _epService.EPAdministrator.CreateEPL("select a1.* from A1 as a1");

	        var listener = new SupportUpdateListener();
	        var subscriber = new SupportSubscriberRowByRowObjectArrayPlainNStmt();

	        stmt.AddListener(listener);
	        stmt.Subscriber = subscriber;
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        var theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("E1", theEvent.Get("TheString"));
	        Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
	        Assert.That(theEvent.Underlying, Is.InstanceOf<Pair<object, DataMap>>());

	        foreach (var property in stmt.EventType.PropertyNames)
	        {
	            var getter = stmt.EventType.GetGetter(property);
	            getter.Get(theEvent);
	        }
	    }

	    private void RunAssertionBindUpdateIRStream(SupportSubscriberRowByRowFullBase subscriber)
	    {
	        var stmtText = "select irstream TheString, IntPrimitive from " + typeof(SupportBean).FullName + ".win:length_batch(2)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.Subscriber = subscriber;

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        subscriber.AssertOneReceivedAndReset(stmt, 2, 0, new object[][]{new object[] {"E1", 1}, new object[]{"E2", 2}}, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        subscriber.AssertOneReceivedAndReset(stmt, 2, 2, new object[][]{new object[]{"E3", 3}, new object[]{"E4", 4}}, new object[][]{new object[]{"E1", 1}, new object[]{"E2", 2}});
	    }

	    private void RunAssertionBindObjectArr(EventRepresentationEnum eventRepresentationEnum, SupportSubscriberMultirowObjectArrayBase subscriber)
	    {
	        var stmtText = eventRepresentationEnum.GetAnnotationText() + " select irstream TheString, IntPrimitive from " + typeof(SupportBean).FullName + ".win:length_batch(2)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.Subscriber = subscriber;
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        subscriber.AssertOneReceivedAndReset(stmt, _fields, new object[][]{new object[]{"E1", 1}, new object[]{"E2", 2}}, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        subscriber.AssertOneReceivedAndReset(stmt, _fields, new object[][]{new object[]{"E3", 3}, new object[]{"E4", 4}}, new object[][]{new object[]{"E1", 1}, new object[]{"E2", 2}});

	        stmt.Dispose();
	    }

	    private void RunAssertionBindMap(EventRepresentationEnum eventRepresentationEnum, SupportSubscriberMultirowMapBase subscriber)
	    {
	        var stmtText = eventRepresentationEnum.GetAnnotationText() + " select irstream TheString, IntPrimitive from " + typeof(SupportBean).FullName + ".win:length_batch(2)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.Subscriber = subscriber;
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        subscriber.AssertOneReceivedAndReset(stmt, _fields, new object[][]{new object[]{"E1", 1}, new object[]{"E2", 2}}, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        subscriber.AssertOneReceivedAndReset(stmt, _fields, new object[][]{new object[]{"E3", 3}, new object[]{"E4", 4}}, new object[][]{new object[]{"E1", 1}, new object[]{"E2", 2}});

	        stmt.Dispose();
	    }

	    private void RunAssertionWidening(EventRepresentationEnum eventRepresentationEnum, SupportSubscriberRowByRowSpecificBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select BytePrimitive, IntPrimitive, LongPrimitive, FloatPrimitive from SupportBean(TheString='E1')");
	        stmt.Subscriber = subscriber;
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        var bean = new SupportBean();
	        bean.TheString = "E1";
	        bean.BytePrimitive = (byte)1;
	        bean.IntPrimitive = 2;
	        bean.LongPrimitive = 3;
	        bean.FloatPrimitive = 4;
	        _epService.EPRuntime.SendEvent(bean);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{1, 2L, 3d, 4d});

	        stmt.Dispose();
	    }

	    private void RunAssertionObjectArrayDelivery(EventRepresentationEnum eventRepresentationEnum, SupportSubscriberRowByRowObjectArrayBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select TheString, IntPrimitive from SupportBean.std:unique(TheString)");
	        stmt.Subscriber = subscriber;
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertOneAndReset(stmt, new object[]{"E1", 1});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        subscriber.AssertOneAndReset(stmt, new object[]{"E2", 10});

	        stmt.Dispose();
	    }

	    private void RunAssertionRowMapDelivery(EventRepresentationEnum eventRepresentationEnum, SupportSubscriberRowByRowMapBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select irstream TheString, IntPrimitive from SupportBean.std:unique(TheString)");
	        stmt.Subscriber = subscriber;
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertIRStreamAndReset(stmt, _fields, new object[]{"E1", 1}, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
	        subscriber.AssertIRStreamAndReset(stmt, _fields, new object[]{"E2", 10}, null);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
	        subscriber.AssertIRStreamAndReset(stmt, _fields, new object[]{"E1", 2}, new object[]{"E1", 1});

	        stmt.Dispose();
	    }

	    private void RunAssertionNested(EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
	        stmt.Subscriber = subscriber;

	        var theEvent = SupportBeanComplexProps.MakeDefaultBean();
	        _epService.EPRuntime.SendEvent(theEvent);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{theEvent.Nested, theEvent.Nested.NestedNested});
	    }

	    private void RunAssertionEnum(EPStatement stmtEnum, SupportSubscriberRowByRowSpecificBase subscriber) {
	        stmtEnum.Subscriber = subscriber;

	        var theEvent = new SupportBeanWithEnum("abc", SupportEnum.ENUM_VALUE_1);
	        _epService.EPRuntime.SendEvent(theEvent);
	        subscriber.AssertOneReceivedAndReset(stmtEnum, new object[]{theEvent.TheString, theEvent.SupportEnum});
	    }

	    private void RunAssertionNullSelected(EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
	        stmt.Subscriber = subscriber;
	        _epService.EPRuntime.SendEvent(new SupportBean());
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{null, null});
	    }

	    private void RunAssertionStreamSelectWJoin(SupportSubscriberRowByRowSpecificBase subscriber) {
	        var stmt = _epService.EPAdministrator.CreateEPL("select null, s1, s0 from SupportBean.win:keepall() as s0, SupportMarketDataBean.win:keepall() as s1 where s0.TheString = s1.Symbol");
	        stmt.Subscriber = subscriber;

	        var s0 = new SupportBean("E1", 100);
	        var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
	        _epService.EPRuntime.SendEvent(s0);
	        _epService.EPRuntime.SendEvent(s1);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{null, s1, s0});

	        stmt.Dispose();
	    }

	    private void RunAssertionBindWildcardJoin(SupportSubscriberRowByRowSpecificBase subscriber) {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:keepall() as s0, SupportMarketDataBean.win:keepall() as s1 where s0.TheString = s1.Symbol");
	        stmt.Subscriber = subscriber;

	        var s0 = new SupportBean("E1", 100);
	        var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
	        _epService.EPRuntime.SendEvent(s0);
	        _epService.EPRuntime.SendEvent(s1);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0, s1});

	        stmt.Dispose();
	    }

	    private void RunAssertionJustWildcard(EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
	        stmt.Subscriber = subscriber;
	        var theEvent = new SupportBean("E2", 1);
	        _epService.EPRuntime.SendEvent(theEvent);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{theEvent});
	    }

	    private void RunAssertionStreamWildcardJoin(SupportSubscriberRowByRowSpecificBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString || '<', s1.* as s1, s0.* as s0 from SupportBean.win:keepall() as s0, SupportMarketDataBean.win:keepall() as s1 where s0.TheString = s1.Symbol");
	        stmt.Subscriber = subscriber;

	        var s0 = new SupportBean("E1", 100);
	        var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
	        _epService.EPRuntime.SendEvent(s0);
	        _epService.EPRuntime.SendEvent(s1);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E1<", s1, s0});

	        stmt.Dispose();
	    }

	    private void RunAssertionWildcardWProps(EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
	        stmt.Subscriber = subscriber;

	        var s0 = new SupportBean("E1", 100);
	        _epService.EPRuntime.SendEvent(s0);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0, 102, "xE1x"});
	    }

	    private void RunAssertionOutputLimitNoJoin(EventRepresentationEnum eventRepresentationEnum, SupportSubscriberRowByRowSpecificBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select TheString, IntPrimitive from SupportBean output every 2 events");
	        stmt.Subscriber = subscriber;
	        Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmt.EventType.UnderlyingType);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            subscriber.AssertMultipleReceivedAndReset(stmt, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

	        stmt.Dispose();
	    }

	    private void RunAssertionOutputLimitJoin(SupportSubscriberRowByRowSpecificBase subscriber) {
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean.win:keepall(), SupportMarketDataBean.win:keepall() where Symbol = TheString output every 2 events");
	        stmt.Subscriber = subscriber;

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("E1", 0, 1L, ""));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertNoneReceived();

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            subscriber.AssertMultipleReceivedAndReset(stmt, new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 2 } });
	        stmt.Dispose();
	    }

	    private void RunAssertionRStreamSelect(SupportSubscriberRowByRowSpecificBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select rstream s0 from SupportBean.std:unique(TheString) as s0");
	        stmt.Subscriber = subscriber;

	        // send event
	        var s0 = new SupportBean("E1", 100);
	        _epService.EPRuntime.SendEvent(s0);
	        subscriber.AssertNoneReceived();

	        var s1 = new SupportBean("E2", 200);
	        _epService.EPRuntime.SendEvent(s1);
	        subscriber.AssertNoneReceived();

	        var s2 = new SupportBean("E1", 300);
	        _epService.EPRuntime.SendEvent(s2);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0});

	        stmt.Dispose();
	    }

	    private void RunAssertionBindWildcardIRStream(SupportSubscriberMultirowUnderlyingBase subscriber)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:length_batch(2)");
	        stmt.Subscriber = subscriber;

	        var s0 = new SupportBean("E1", 100);
	        var s1 = new SupportBean("E2", 200);
	        _epService.EPRuntime.SendEvent(s0);
	        _epService.EPRuntime.SendEvent(s1);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0, s1}, null);

	        var s2 = new SupportBean("E3", 300);
	        var s3 = new SupportBean("E4", 400);
	        _epService.EPRuntime.SendEvent(s2);
	        _epService.EPRuntime.SendEvent(s3);
	        subscriber.AssertOneReceivedAndReset(stmt, new object[]{s2, s3}, new object[]{s0, s1});

	        stmt.Dispose();
	    }

	    private void RunAssertionStaticMethod()
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from " + typeof(SupportBean).FullName);

	        var subscriber = new SupportSubscriberRowByRowStatic();
	        stmt.Subscriber = subscriber;
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E1", 100 } }, SupportSubscriberRowByRowStatic.GetAndResetIndicate());

	        var subscriberWStmt = new SupportSubscriberRowByRowStaticWStatement();
	        stmt.Subscriber = subscriberWStmt;
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[][]{ new object[] {"E2", 200}}, SupportSubscriberRowByRowStaticWStatement.GetIndicate());
	        Assert.AreEqual(stmt, SupportSubscriberRowByRowStaticWStatement.GetStatements()[0]);
	        subscriberWStmt.Reset();

	        stmt.Dispose();
	    }

	    private void RunAssertionNoParams(EPStatement stmt, SupportSubscriberNoParamsBase subscriber)
        {
	        stmt.Subscriber = subscriber;

	        _epService.EPRuntime.SendEvent(new SupportBean());
	        subscriber.AssertCalledAndReset(stmt);
	    }

	    private void RunAsserionNamedMethod(EPStatement stmt, SupportSubscriberMultirowUnderlyingBase subscriber)
        {
	        stmt.Subscriber = new EPSubscriber(subscriber, "SomeNewDataMayHaveArrived");

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        subscriber.AssertOneReceivedAndReset(stmt, new object[] {"E1"}, null);
	    }

	    private void RunAssertionPreferEPStatement()
        {
	        var subscriber = new SupportSubscriberUpdateBothFootprints();
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean");
	        stmt.Subscriber = subscriber;

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        subscriber.AssertOneReceivedAndReset(stmt, new object[] {"E1", 10});

	        stmt.Dispose();
	    }
	}
} // end of namespace
