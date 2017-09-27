///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestPropertyResolutionFragment
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
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
	    public void TestMapSimpleTypes() {
	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0int", typeof(int));
	        mapOuter.Put("p0intarray", typeof(int[]));
	        mapOuter.Put("p0map", typeof(IDictionary<string, object>));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1someval", "A");

	        IDictionary<string, object> dataRoot = new Dictionary<string, object>();
	        dataRoot.Put("p0simple", 99);
	        dataRoot.Put("p0array", new int[] {101, 102});
	        dataRoot.Put("p0map", dataInner);

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    //comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // resolve property via fragment
	        Assert.IsNull(eventType.GetFragmentType("p0int"));
	        Assert.IsNull(eventType.GetFragmentType("p0intarray"));
	        Assert.IsNull(eventBean.GetFragment("p0map?"));
	        Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
	        Assert.IsNull(eventBean.GetFragment("p0map('a')?"));
	    }

        [Test]
	    public void TestObjectArraySimpleTypes() {
	        string[] props = {"p0int", "p0intarray", "p0map"};
            object[] types = { typeof(int), typeof(int[]), typeof(IDictionary<string, object>) };
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", props, types);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1someval", "A");
	        var dataRoot = new object[] {99, new int[] {101, 102}, dataInner};

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    //comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // resolve property via fragment
	        Assert.IsNull(eventType.GetFragmentType("p0int"));
	        Assert.IsNull(eventType.GetFragmentType("p0intarray"));
	        Assert.IsNull(eventBean.GetFragment("p0map?"));
	        Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
	        Assert.IsNull(eventBean.GetFragment("p0map('a')?"));
	    }

        [Test]
	    public void TestWrapperFragmentWithMap() {
	        IDictionary<string, object> typeLev0 = new Dictionary<string, object>();
	        typeLev0.Put("p1id", typeof(int));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", typeLev0);

	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0simple", "TypeLev0");
	        mapOuter.Put("p0bean", typeof(SupportBeanComplexProps));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select *, p0simple.p1id + 1 as plusone, p0bean as mybean from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1id", 10);

	        IDictionary<string, object> dataRoot = new Dictionary<string, object>();
	        dataRoot.Put("p0simple", dataInner);
	        dataRoot.Put("p0bean", SupportBeanComplexProps.MakeDefaultBean());

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // resolve property via fragment
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
	        Assert.AreEqual(11, eventBean.Get("plusone"));
	        Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));

	        var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

	        var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");
	        Assert.AreEqual("NestedNestedValue", innerBeanEvent.Get("nested.NestedNested.NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));
	    }

        [Test]
	    public void TestWrapperFragmentWithObjectArray() {
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", new string[] {"p1id"}, new object[] {typeof(int)});
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", new string[] {"p0simple", "p0bean"}, new object[] {"TypeLev0", typeof(SupportBeanComplexProps)});

	        var stmt = _epService.EPAdministrator.CreateEPL("select *, p0simple.p1id + 1 as plusone, p0bean as mybean from TypeRoot");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {new object[]{10}, SupportBeanComplexProps.MakeDefaultBean()}, "TypeRoot");

	        var eventBean = _listener.AssertOneGetNewAndReset();
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // resolve property via fragment
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
	        Assert.AreEqual(11, eventBean.Get("plusone"));
	        Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));

	        var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

	        var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");
	        Assert.AreEqual("NestedNestedValue", innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("mybean.Nested.NestedNested")).Get("NestedNestedValue"));
	    }

        [Test]
	    public void TestNativeBeanFragment() {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBeanComplexProps).FullName);
	        stmt.AddListener(_listener);
	        stmt = _epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBeanCombinedProps).FullName);
	        stmt.AddListener(_listener);

	        // assert nested fragments
	        _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
	        //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));

	        Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Nested").IsFragment);
	        var eventNested = (EventBean) eventBean.GetFragment("Nested");
	        Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));
	        eventNested = (EventBean) eventBean.GetFragment("Nested?");
	        Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));

	        Assert.IsTrue(eventNested.EventType.GetPropertyDescriptor("NestedNested").IsFragment);
	        Assert.AreEqual("NestedNestedValue", ((EventBean) eventNested.GetFragment("NestedNested")).Get("NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean) eventNested.GetFragment("NestedNested?")).Get("NestedNestedValue"));

	        var nestedFragment = (EventBean) eventBean.GetFragment("nested.nestedNested");
	        Assert.AreEqual("NestedNestedValue", nestedFragment.Get("NestedNestedValue"));

	        // assert indexed fragments
	        var eventObject = SupportBeanCombinedProps.MakeDefaultBean();
	        _epService.EPRuntime.SendEvent(eventObject);
	        eventBean = _listener.AssertOneGetNewAndReset();
	        SupportEventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
	        //System.out.println(SupportEventTypeAssertionUtil.print(eventBean));

	        Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsFragment);
	        Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Array").IsIndexed);
	        var eventArray = (EventBean[]) eventBean.GetFragment("Array");
	        Assert.AreEqual(3, eventArray.Length);

	        var eventElement = eventArray[0];
	        Assert.AreSame(eventObject.Array[0].GetMapped("0ma"), eventElement.Get("mapped('0ma')"));
	        Assert.AreSame(eventObject.Array[0].GetMapped("0ma"), ((EventBean)eventBean.GetFragment("Array[0]")).Get("mapped('0ma')"));
	        Assert.AreSame(eventObject.Array[0].GetMapped("0ma"), ((EventBean)eventBean.GetFragment("Array[0]?")).Get("mapped('0ma')"));
	    }

        [Test]
	    public void TestMapFragmentMapNested() {
	        IDictionary<string, object> typeLev0 = new Dictionary<string, object>();
	        typeLev0.Put("p1id", typeof(int));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", typeLev0);

	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0simple", "TypeLev0");
	        mapOuter.Put("p0array", "TypeLev0[]");
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1id", 10);

	        IDictionary<string, object> dataRoot = new Dictionary<string, object>();
	        dataRoot.Put("p0simple", dataInner);
            dataRoot.Put("p0array", new IDictionary<string, object>[] { dataInner, dataInner });

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // resolve property via fragment
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);

	        var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

	        var innerArrayAllEvent = (EventBean[]) eventBean.GetFragment("p0array");
	        Assert.AreEqual(10, innerArrayAllEvent[0].Get("p1id"));

	        var innerArrayElementEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual(10, innerArrayElementEvent.Get("p1id"));

	        // resolve property via getter
	        Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
	        Assert.AreEqual(10, eventBean.Get("p0array[1].p1id"));

	        Assert.IsNull(eventType.GetFragmentType("p0array.p1id"));
	        Assert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
	    }

        [Test]
	    public void TestObjectArrayFragmentObjectArrayNested() {
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", new string[] {"p1id"}, new object[] {typeof(int)});
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", new string[] {"p0simple", "p0array"}, new object[] {"TypeLev0", "TypeLev0[]"});

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);

	        _epService.EPRuntime.SendEvent(new object[] {new object[] {10}, new object[] {new object[] {20}, new object[] {21}}}, "TypeRoot");

	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // resolve property via fragment
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);

	        var innerSimpleEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

	        var innerArrayAllEvent = (EventBean[]) eventBean.GetFragment("p0array");
	        Assert.AreEqual(20, innerArrayAllEvent[0].Get("p1id"));

	        var innerArrayElementEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual(20, innerArrayElementEvent.Get("p1id"));

	        // resolve property via getter
	        Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
	        Assert.AreEqual(21, eventBean.Get("p0array[1].p1id"));

	        Assert.IsNull(eventType.GetFragmentType("p0array.p1id"));
	        Assert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
	    }

        [Test]
	    public void TestMapFragmentMapUnnamed() {
	        IDictionary<string, object> typeLev0 = new Dictionary<string, object>();
	        typeLev0.Put("p1id", typeof(int));

	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0simple", typeLev0);
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1id", 10);

	        IDictionary<string, object> dataRoot = new Dictionary<string, object>();
	        dataRoot.Put("p0simple", dataInner);

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.IsFalse(eventType.GetPropertyDescriptor("p0simple").IsFragment);
	        Assert.IsNull(eventBean.GetFragment("p0simple"));

	        // resolve property via getter
	        Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
	    }

        [Test]
	    public void TestMapFragmentTransposedMapEventBean() {
	        IDictionary<string, object> typeInner = new Dictionary<string, object>();
	        typeInner.Put("p2id", typeof(int));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeInner", typeInner);

	        IDictionary<string, object> typeMap = new Dictionary<string, object>();
	        typeMap.Put("id", typeof(int));
	        typeMap.Put("bean", typeof(SupportBean));
	        typeMap.Put("beanarray", typeof(SupportBean[]));
	        typeMap.Put("complex", typeof(SupportBeanComplexProps));
	        typeMap.Put("complexarray", typeof(SupportBeanComplexProps[]));
	        typeMap.Put("map", "TypeInner");
	        typeMap.Put("maparray", "TypeInner[]");

	        _epService.EPAdministrator.Configuration.AddEventType("TypeMapOne", typeMap);
	        _epService.EPAdministrator.Configuration.AddEventType("TypeMapTwo", typeMap);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from pattern[one=TypeMapOne until two=TypeMapTwo]");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p2id", 2000);
	        IDictionary<string, object> dataMap = new Dictionary<string, object>();
	        dataMap.Put("id", 1);
	        dataMap.Put("bean", new SupportBean("E1", 100));
	        dataMap.Put("beanarray", new SupportBean[] {new SupportBean("E1", 100), new SupportBean("E2", 200)});
	        dataMap.Put("complex", SupportBeanComplexProps.MakeDefaultBean());
	        dataMap.Put("complexarray", new SupportBeanComplexProps[] {SupportBeanComplexProps.MakeDefaultBean()});
	        dataMap.Put("map", dataInner);
            dataMap.Put("maparray", new IDictionary<string, object>[] { dataInner, dataInner });

	        // send event
	        _epService.EPRuntime.SendEvent(dataMap, "TypeMapOne");

	        IDictionary<string, object> dataMapTwo = new Dictionary<string, object>(dataMap);
	        dataMapTwo.Put("id", 2);
	        _epService.EPRuntime.SendEvent(dataMapTwo, "TypeMapOne");

	        IDictionary<string, object> dataMapThree = new Dictionary<string, object>(dataMap);
	        dataMapThree.Put("id", 3);
	        _epService.EPRuntime.SendEvent(dataMapThree, "TypeMapTwo");

	        var eventBean = _listener.AssertOneGetNewAndReset();
	        // System.out.println(SupportEventTypeAssertionUtil.print(eventBean));
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.AreEqual(1, ((EventBean)eventBean.GetFragment("one[0]")).Get("id"));
	        Assert.AreEqual(2, ((EventBean)eventBean.GetFragment("one[1]")).Get("id"));
	        Assert.AreEqual(3, ((EventBean)eventBean.GetFragment("two")).Get("id"));

	        Assert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[0].bean")).Get("theString"));
	        Assert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[1].bean")).Get("theString"));
	        Assert.AreEqual("E1", ((EventBean)eventBean.GetFragment("two.bean")).Get("theString"));

	        Assert.AreEqual("E2", ((EventBean)eventBean.GetFragment("one[0].beanarray[1]")).Get("theString"));
	        Assert.AreEqual("E2", ((EventBean)eventBean.GetFragment("two.beanarray[1]")).Get("theString"));

	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("one[0].complex.nested.nestedNested")).Get("NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("two.complex.nested.nestedNested")).Get("NestedNestedValue"));

	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("one[0].complexarray[0].nested.nestedNested")).Get("NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("two.complexarray[0].nested.nestedNested")).Get("NestedNestedValue"));

	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].map")).Get("p2id"));
	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.map")).Get("p2id"));

	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].maparray[1]")).Get("p2id"));
	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.maparray[1]")).Get("p2id"));
	    }

        [Test]
	    public void TestObjectArrayFragmentTransposedMapEventBean() {
	        _epService.EPAdministrator.Configuration.AddEventType("TypeInner", new string[] {"p2id"}, new object[] {typeof(int)});

	        string[] props = {"id", "bean", "beanarray", "complex", "complexarray", "map", "maparray"};
	        object[] types = {typeof(int), typeof(SupportBean), typeof(SupportBean[]), typeof(SupportBeanComplexProps), typeof(SupportBeanComplexProps[]), "TypeInner", "TypeInner[]"};
	        _epService.EPAdministrator.Configuration.AddEventType("TypeMapOne", props, types);
	        _epService.EPAdministrator.Configuration.AddEventType("TypeMapTwo", props, types);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from pattern[one=TypeMapOne until two=TypeMapTwo]");
	        stmt.AddListener(_listener);

	        var dataInner = new object[] {2000};
	        var dataArray = new object[] {1, new SupportBean("E1", 100),
	        new SupportBean[] {new SupportBean("E1", 100), new SupportBean("E2", 200)},
	        SupportBeanComplexProps.MakeDefaultBean(),
	        new SupportBeanComplexProps[] {SupportBeanComplexProps.MakeDefaultBean()},
	        dataInner, new object[] {dataInner, dataInner}
	                                          };

	        // send event
	        _epService.EPRuntime.SendEvent(dataArray, "TypeMapOne");

	        var dataArrayTwo = new object[dataArray.Length];
	        Array.Copy(dataArray, 0, dataArrayTwo, 0, dataArray.Length);
	        dataArrayTwo[0] = 2;
	        _epService.EPRuntime.SendEvent(dataArrayTwo, "TypeMapOne");

	        var dataArrayThree = new object[dataArray.Length];
	        Array.Copy(dataArray, 0, dataArrayThree, 0, dataArray.Length);
	        dataArrayThree[0] = 3;
	        _epService.EPRuntime.SendEvent(dataArrayThree, "TypeMapTwo");

	        var eventBean = _listener.AssertOneGetNewAndReset();
	        // System.out.println(SupportEventTypeAssertionUtil.print(eventBean));
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("id"));
	        Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("id"));
	        Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("id"));

	        Assert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[0].bean")).Get("theString"));
	        Assert.AreEqual("E1", ((EventBean)eventBean.GetFragment("one[1].bean")).Get("theString"));
	        Assert.AreEqual("E1", ((EventBean)eventBean.GetFragment("two.bean")).Get("theString"));

	        Assert.AreEqual("E2", ((EventBean)eventBean.GetFragment("one[0].beanarray[1]")).Get("theString"));
	        Assert.AreEqual("E2", ((EventBean)eventBean.GetFragment("two.beanarray[1]")).Get("theString"));

	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("one[0].complex.nested.nestedNested")).Get("NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("two.complex.nested.nestedNested")).Get("NestedNestedValue"));

	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("one[0].complexarray[0].nested.nestedNested")).Get("NestedNestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean)eventBean.GetFragment("two.complexarray[0].nested.nestedNested")).Get("NestedNestedValue"));

	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].map")).Get("p2id"));
	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.map")).Get("p2id"));

	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("one[0].maparray[1]")).Get("p2id"));
	        Assert.AreEqual(2000, ((EventBean)eventBean.GetFragment("two.maparray[1]")).Get("p2id"));
	    }

        [Test]
	    public void TestMapFragmentMapBeans() {
	        IDictionary<string, object> typeLev0 = new Dictionary<string, object>();
	        typeLev0.Put("p1simple", typeof(SupportBean));
	        typeLev0.Put("p1array", typeof(SupportBean[]));
	        typeLev0.Put("p1complex", typeof(SupportBeanComplexProps));
	        typeLev0.Put("p1complexarray", typeof(SupportBeanComplexProps[]));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", typeLev0);

	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0simple", "TypeLev0");
	        mapOuter.Put("p0array", "TypeLev0[]");
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1simple", new SupportBean("E1", 11));
	        dataInner.Put("p1array", new SupportBean[] {new SupportBean("A1", 21), new SupportBean("A2", 22)});
	        dataInner.Put("p1complex", SupportBeanComplexProps.MakeDefaultBean());
	        dataInner.Put("p1complexarray", new SupportBeanComplexProps[] {SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean()});

	        IDictionary<string, object> dataRoot = new Dictionary<string, object>();
	        dataRoot.Put("p0simple", dataInner);
            dataRoot.Put("p0array", new IDictionary<string, object>[] { dataInner, dataInner });

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("intPrimitive"));
	        Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("theString"));
	        Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("simpleProperty"));
	        Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("simpleProperty"));
	        Assert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));

	        var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual("E1", assertEvent.Get("p1simple.theString"));
	        Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("intPrimitive"));
	        Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("intPrimitive"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get("NestedNestedValue"));

	        assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
	        Assert.AreEqual("E1", assertEvent.Get("p1simple.theString"));
	        Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("intPrimitive"));
	        Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("intPrimitive"));

	        assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual("E1", assertEvent.Get("p1simple.theString"));
	        Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("intPrimitive"));
	        Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("intPrimitive"));
	    }

        [Test]
	    public void TestObjectArrayFragmentBeans() {
	        string[] propsLev0 = {"p1simple", "p1array", "p1complex", "p1complexarray"};
	        object[] typesLev0 = {typeof(SupportBean), typeof(SupportBean[]), typeof(SupportBeanComplexProps), typeof(SupportBeanComplexProps[])};
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", propsLev0, typesLev0);

	        string[] propsOuter = {"p0simple", "p0array"};
	        object[] typesOuter = {"TypeLev0", "TypeLev0[]"};
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", propsOuter, typesOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);

	        object[] dataInner = {new SupportBean("E1", 11), new SupportBean[] {new SupportBean("A1", 21), new SupportBean("A2", 22)},
	                   SupportBeanComplexProps.MakeDefaultBean(), new SupportBeanComplexProps[] {SupportBeanComplexProps.MakeDefaultBean(), SupportBeanComplexProps.MakeDefaultBean()}
	        };
	        var dataRoot = new object[] {dataInner, new object[] {dataInner,dataInner}};

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("intPrimitive"));
	        Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("theString"));
	        Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("SimpleProperty"));
	        Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("simpleProperty"));
	        Assert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));

	        var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual("E1", assertEvent.Get("p1simple.theString"));
	        Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("intPrimitive"));
	        Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("intPrimitive"));
	        Assert.AreEqual("NestedNestedValue", ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get("NestedNestedValue"));

	        assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
	        Assert.AreEqual("E1", assertEvent.Get("p1simple.theString"));
	        Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("intPrimitive"));
	        Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("intPrimitive"));

	        assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual("E1", assertEvent.Get("p1simple.theString"));
	        Assert.AreEqual(11, ((EventBean) assertEvent.GetFragment("p1simple")).Get("intPrimitive"));
	        Assert.AreEqual(22, ((EventBean) assertEvent.GetFragment("p1array[1]")).Get("intPrimitive"));
	    }

        [Test]
	    public void TestMapFragmentMap3Level() {
	        IDictionary<string, object> typeLev1 = new Dictionary<string, object>();
	        typeLev1.Put("p2id", typeof(int));
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev1", typeLev1);

	        IDictionary<string, object> typeLev0 = new Dictionary<string, object>();
	        typeLev0.Put("p1simple", "TypeLev1");
	        typeLev0.Put("p1array", "TypeLev1[]");
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", typeLev0);

	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0simple", "TypeLev0");
	        mapOuter.Put("p0array", "TypeLev0[]");
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataLev1 = new Dictionary<string, object>();
	        dataLev1.Put("p2id", 10);

	        IDictionary<string, object> dataLev0 = new Dictionary<string, object>();
	        dataLev0.Put("p1simple", dataLev1);
            dataLev0.Put("p1array", new IDictionary<string, object>[] { dataLev1, dataLev1 });

	        IDictionary<string, object> dataRoot = new Dictionary<string, object>();
	        dataRoot.Put("p0simple", dataLev0);
            dataRoot.Put("p0array", new IDictionary<string, object>[] { dataLev0, dataLev0 });

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));

	        // resolve property via fragment
	        var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
	        Assert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

	        assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
	        Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
	        Assert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

	        assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
	        Assert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

	        Assert.AreEqual("TypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
	        Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
	        Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array[0].p1array[0]").FragmentType.GetPropertyDescriptor("p2id").PropertyType);
	        Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
	        Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

	        TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");
	    }

        [Test]
	    public void TestObjectArrayFragment3Level() {
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev1", new string[] {"p2id"}, new object[] {typeof(int)});
	        _epService.EPAdministrator.Configuration.AddEventType("TypeLev0", new string[] {"p1simple", "p1array"}, new object[] {"TypeLev1", "TypeLev1[]"});
	        _epService.EPAdministrator.Configuration.AddEventType("TypeRoot", new string[] {"p0simple", "p0array"}, new object[] {"TypeLev0", "TypeLev0[]"});

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from TypeRoot");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);

	        var dataLev1 = new object[] {10};
	        var dataLev0 = new object[] {dataLev1, new object[] {dataLev1,dataLev1}};
	        var dataRoot = new object[] {dataLev0, new object[] {dataLev0,dataLev0}};

	        // send event
	        _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        //  System.out.println(SupportEventTypeAssertionUtil.print(eventBean));    comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
	        Assert.AreEqual(10, ((EventBean)eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));

	        // resolve property via fragment
	        var assertEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
	        Assert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

	        assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
	        Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
	        Assert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

	        assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
	        Assert.AreEqual(10, ((EventBean)assertEvent.GetFragment("p1simple")).Get("p2id"));

	        Assert.AreEqual("TypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
	        Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
	        Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array[0].p1array[0]").FragmentType.GetPropertyDescriptor("p2id").PropertyType);
	        Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
	        Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

	        TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");
	    }

        [Test]
	    public void TestFragmentMapMulti() {
	        IDictionary<string, object> mapInnerInner = new Dictionary<string, object>();
	        mapInnerInner.Put("p2id", typeof(int));

	        IDictionary<string, object> mapInner = new Dictionary<string, object>();
	        mapInner.Put("p1bean", typeof(SupportBean));
	        mapInner.Put("p1beanComplex", typeof(SupportBeanComplexProps));
	        mapInner.Put("p1beanArray", typeof(SupportBean[]));
	        mapInner.Put("p1innerId", typeof(int));
	        mapInner.Put("p1innerMap", mapInnerInner);
	        _epService.EPAdministrator.Configuration.AddEventType("InnerMap", mapInner);

	        IDictionary<string, object> mapOuter = new Dictionary<string, object>();
	        mapOuter.Put("p0simple", "InnerMap");
	        mapOuter.Put("p0array", "InnerMap[]");
	        _epService.EPAdministrator.Configuration.AddEventType("OuterMap", mapOuter);

	        var stmt = _epService.EPAdministrator.CreateEPL("select * from OuterMap");
	        stmt.AddListener(_listener);

	        IDictionary<string, object> dataInnerInner = new Dictionary<string, object>();
	        dataInnerInner.Put("p2id", 10);

	        IDictionary<string, object> dataInner = new Dictionary<string, object>();
	        dataInner.Put("p1bean", new SupportBean("string1", 2000));
	        dataInner.Put("p1beanComplex", SupportBeanComplexProps.MakeDefaultBean());
	        dataInner.Put("p1beanArray", new SupportBean[] {new SupportBean("string2", 1), new SupportBean("string3", 2)});
	        dataInner.Put("p1innerId", 50);
	        dataInner.Put("p1innerMap", dataInnerInner);

	        IDictionary<string, object> dataOuter = new Dictionary<string, object>();
	        dataOuter.Put("p0simple", dataInner);
            dataOuter.Put("p0array", new IDictionary<string, object>[] { dataInner, dataInner });

	        // send event
	        _epService.EPRuntime.SendEvent(dataOuter, "OuterMap");
	        var eventBean = _listener.AssertOneGetNewAndReset();
	        // System.out.println(SupportEventTypeAssertionUtil.print(eventBean));     comment me in
	        var eventType = eventBean.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(eventType);

	        // Fragment-to-simple
	        Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
	        Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0simple").FragmentType.GetPropertyDescriptor("p1innerId").PropertyType);
	        var p0simpleEvent = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual(50, p0simpleEvent.Get("p1innerId"));
	        p0simpleEvent = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual(50, p0simpleEvent.Get("p1innerId"));

	        // Fragment-to-bean
	        var p0arrayEvents = (EventBean[]) eventBean.GetFragment("p0array");
	        Assert.AreSame(p0arrayEvents[0].EventType, p0simpleEvent.EventType);
	        Assert.AreEqual("string1", eventBean.Get("p0array[0].p1bean.theString"));
	        Assert.AreEqual("string1", ((EventBean) eventBean.GetFragment("p0array[0].p1bean")).Get("theString"));

	        var innerOne = (EventBean) eventBean.GetFragment("p0array[0]");
	        Assert.AreEqual("string1", ((EventBean) innerOne.GetFragment("p1bean")).Get("theString"));
	        Assert.AreEqual("string1", innerOne.Get("p1bean.theString"));
	        innerOne = (EventBean) eventBean.GetFragment("p0simple");
	        Assert.AreEqual("string1", ((EventBean) innerOne.GetFragment("p1bean")).Get("theString"));
	        Assert.AreEqual("string1", innerOne.Get("p1bean.theString"));
	    }

	    private void TryInvalid(EventBean theEvent, string property) {
	        try {
	            theEvent.Get(property);
	            Assert.Fail();
	        } catch (PropertyAccessException) {
	            // expected
	        }
	    }
	}
} // end of namespace
