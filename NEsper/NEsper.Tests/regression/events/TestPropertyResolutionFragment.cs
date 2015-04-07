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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestPropertyResolutionFragment
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(
                SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void TryInvalid(EventBean theEvent, String property)
        {
            try
            {
                theEvent.Get(property);
                Assert.Fail();
            }
            catch (PropertyAccessException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestFragmentMapMulti()
        {
            IDictionary<String, Object> mapInnerInner = new Dictionary<String, Object>();

            mapInnerInner["p2id"] = typeof (int);

            IDictionary<String, Object> mapInner = new Dictionary<String, Object>();

            mapInner["p1bean"] = typeof (SupportBean);
            mapInner["p1beanComplex"] = typeof (SupportBeanComplexProps);
            mapInner["p1beanArray"] = typeof (SupportBean[]);
            mapInner["p1innerId"] = typeof (int);
            mapInner["p1innerMap"] = mapInnerInner;
            _epService.EPAdministrator.Configuration.AddEventType(
                "InnerMap", mapInner);

            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0simple"] = "InnerMap";
            mapOuter["p0array"] = "InnerMap[]";
            _epService.EPAdministrator.Configuration.AddEventType(
                "OuterMap", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from OuterMap");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInnerInner = new Dictionary<String, Object>();

            dataInnerInner["p2id"] = 10;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p1bean"] = new SupportBean("string1", 2000);
            dataInner["p1beanComplex"] = SupportBeanComplexProps.MakeDefaultBean();
            dataInner.Put("p1beanArray", new SupportBean[]
            {
                new SupportBean("string2", 1), new SupportBean("string3", 2)
            }
                );
            dataInner["p1innerId"] = 50;
            dataInner["p1innerMap"] = dataInnerInner;

            IDictionary<String, Object> dataOuter = new Dictionary<String, Object>();

            dataOuter["p0simple"] = dataInner;
            dataOuter.Put("p0array", new Map[]
            {
                dataInner, dataInner
            }
                );

            // send event
            _epService.EPRuntime.SendEvent(dataOuter, "OuterMap");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));     comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // Fragment-to-simple
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0simple").FragmentType.GetPropertyDescriptor("p1innerId").PropertyType);
            var p0simpleEvent = (EventBean) eventBean.GetFragment("p0simple");

            Assert.AreEqual(50, p0simpleEvent.Get("p1innerId"));
            p0simpleEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(50, p0simpleEvent.Get("p1innerId"));

            // Fragment-to-bean
            var p0arrayEvents = (EventBean[]) eventBean.GetFragment(
                "p0array");

            Assert.AreSame(p0arrayEvents[0].EventType, p0simpleEvent.EventType);
            Assert.AreEqual("string1", eventBean.Get("p0array[0].p1bean.TheString"));
            Assert.AreEqual("string1",
                            ((EventBean) eventBean.GetFragment("p0array[0].p1bean")).Get(
                                "TheString"));

            var innerOne = (EventBean) eventBean.GetFragment("p0array[0]");

            Assert.AreEqual("string1",
                            ((EventBean) innerOne.GetFragment("p1bean")).Get("TheString"));
            Assert.AreEqual("string1", innerOne.Get("p1bean.TheString"));
            innerOne = (EventBean) eventBean.GetFragment("p0simple");
            Assert.AreEqual("string1",
                            ((EventBean) innerOne.GetFragment("p1bean")).Get("TheString"));
            Assert.AreEqual("string1", innerOne.Get("p1bean.TheString"));
        }

        [Test]
        public void TestMapFragmentMap3Level()
        {
            IDictionary<String, Object> typeLev1 = new Dictionary<String, Object>();

            typeLev1["p2id"] = typeof (int);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev1", typeLev1);

            IDictionary<String, Object> typeLev0 = new Dictionary<String, Object>();

            typeLev0["p1simple"] = "TypeLev1";
            typeLev0["p1array"] = "TypeLev1[]";
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", typeLev0);

            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0simple"] = "TypeLev0";
            mapOuter["p0array"] = "TypeLev0[]";
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataLev1 = new Dictionary<String, Object>();

            dataLev1["p2id"] = 10;

            IDictionary<String, Object> dataLev0 = new Dictionary<String, Object>();

            dataLev0["p1simple"] = dataLev1;
            dataLev0.Put("p1array", new Map[]
            {
                dataLev1, dataLev1
            });

            IDictionary<String, Object> dataRoot = new Dictionary<String, Object>();

            dataRoot["p0simple"] = dataLev0;
            dataRoot.Put("p0array", new Map[]
            {
                dataLev0, dataLev0
            });

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1simple")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0array[1].p1array[0]")).Get("p2id"));
            Assert.AreEqual(10, ((EventBean) eventBean.GetFragment("p0simple.p1array[0]")).Get("p2id"));

            // resolve property via fragment
            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");

            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));

            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));

            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10, ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));

            Assert.AreEqual("TypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
            Assert.AreEqual(typeof(int), eventType.GetFragmentType("p0array[0].p1array[0]").FragmentType.GetPropertyDescriptor("p2id").PropertyType);
            Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
            Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

            TryInvalid((EventBean) eventBean.GetFragment("p0simple"), "p1simple.p1id");
        }

        [Test]
        public void TestMapFragmentMapBeans()
        {
            IDictionary<String, Object> typeLev0 = new Dictionary<String, Object>();

            typeLev0["p1simple"] = typeof (SupportBean);
            typeLev0["p1array"] = typeof (SupportBean[]);
            typeLev0["p1complex"] = typeof (SupportBeanComplexProps);
            typeLev0["p1complexarray"] = typeof (SupportBeanComplexProps[]);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", typeLev0);

            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0simple"] = "TypeLev0";
            mapOuter["p0array"] = "TypeLev0[]";
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p1simple"] = new SupportBean("E1", 11);
            dataInner.Put("p1array", new SupportBean[]
            {
                new SupportBean("A1", 21), new SupportBean("A2", 22)
            }
                );
            dataInner["p1complex"] = SupportBeanComplexProps.MakeDefaultBean();
            dataInner.Put("p1complexarray",
                          new SupportBeanComplexProps[]
                          {
                              SupportBeanComplexProps.MakeDefaultBean(),
                              SupportBeanComplexProps.MakeDefaultBean()
                          }
                );

            IDictionary<String, Object> dataRoot = new Dictionary<String, Object>();

            dataRoot["p0simple"] = dataInner;
            dataRoot.Put("p0array", new Map[]
            {
                dataInner, dataInner
            }
                );

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine (EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.AreEqual(11, ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get("IntPrimitive"));
            Assert.AreEqual("A2", ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get("TheString"));
            Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get("SimpleProperty"));
            Assert.AreEqual("Simple", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get("SimpleProperty"));
            Assert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get("NestedValue"));
            Assert.AreEqual("NestedNestedValue", ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get("NestedNestedValue"));

            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");

            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual(22,
                            ((EventBean) assertEvent.GetFragment("p1array[1]")).Get(
                                "IntPrimitive"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual(22,
                            ((EventBean) assertEvent.GetFragment("p1array[1]")).Get(
                                "IntPrimitive"));

            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual(22,
                            ((EventBean) assertEvent.GetFragment("p1array[1]")).Get(
                                "IntPrimitive"));
        }

        [Test]
        public void TestMapFragmentMapNested()
        {
            IDictionary<String, Object> typeLev0 = new Dictionary<String, Object>();

            typeLev0["p1id"] = typeof (int);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", typeLev0);

            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0simple"] = "TypeLev0";
            mapOuter["p0array"] = "TypeLev0[]";
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p1id"] = 10;

            IDictionary<String, Object> dataRoot = new Dictionary<String, Object>();

            dataRoot["p0simple"] = dataInner;
            dataRoot.Put("p0array", new Map[]
            {
                dataInner, dataInner
            }
                );

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);

            var innerSimpleEvent = (EventBean) eventBean.GetFragment(
                "p0simple");

            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

            var innerArrayAllEvent = (EventBean[]) eventBean.GetFragment(
                "p0array");

            Assert.AreEqual(10, innerArrayAllEvent[0].Get("p1id"));

            var innerArrayElementEvent = (EventBean) eventBean.GetFragment(
                "p0array[0]");

            Assert.AreEqual(10, innerArrayElementEvent.Get("p1id"));

            // resolve property via getter
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
            Assert.AreEqual(10, eventBean.Get("p0array[1].p1id"));

            Assert.IsNull(eventType.GetFragmentType("p0array.p1id"));
            Assert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
        }

        [Test]
        public void TestMapFragmentMapUnnamed()
        {
            IDictionary<String, Object> typeLev0 = new Dictionary<String, Object>();

            typeLev0["p1id"] = typeof (int);

            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0simple"] = typeLev0;
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p1id"] = 10;

            IDictionary<String, Object> dataRoot = new Dictionary<String, Object>();

            dataRoot["p0simple"] = dataInner;

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.IsFalse(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.IsNull(eventBean.GetFragment("p0simple"));

            // resolve property via getter
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
        }

        [Test]
        public void TestMapFragmentTransposedMapEventBean()
        {
            IDictionary<String, Object> typeInner = new Dictionary<String, Object>();

            typeInner["p2id"] = typeof (int);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeInner", typeInner);

            IDictionary<String, Object> typeMap = new Dictionary<String, Object>();

            typeMap["id"] = typeof (int);
            typeMap["bean"] = typeof (SupportBean);
            typeMap["beanarray"] = typeof (SupportBean[]);
            typeMap["complex"] = typeof (SupportBeanComplexProps);
            typeMap["complexarray"] = typeof (SupportBeanComplexProps[]);
            typeMap["map"] = "TypeInner";
            typeMap["maparray"] = "TypeInner[]";

            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeMapOne", typeMap);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeMapTwo", typeMap);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from pattern[one=TypeMapOne until two=TypeMapTwo]");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p2id"] = 2000;
            IDictionary<String, Object> dataMap = new Dictionary<String, Object>();

            dataMap["id"] = 1;
            dataMap["bean"] = new SupportBean("E1", 100);
            dataMap.Put("beanarray", new SupportBean[]
            {
                new SupportBean("E1", 100), new SupportBean("E2", 200)
            }
                );
            dataMap["complex"] = SupportBeanComplexProps.MakeDefaultBean();
            dataMap.Put("complexarray", new SupportBeanComplexProps[]
            {
                SupportBeanComplexProps.MakeDefaultBean()
            }
                );
            dataMap["map"] = dataInner;
            dataMap.Put("maparray", new Map[]
            {
                dataInner, dataInner
            }
                );

            // send event
            _epService.EPRuntime.SendEvent(dataMap, "TypeMapOne");

            IDictionary<String, Object> dataMapTwo = new Dictionary<String, Object>(dataMap);

            dataMapTwo["id"] = 2;
            _epService.EPRuntime.SendEvent(dataMapTwo, "TypeMapOne");

            IDictionary<String, Object> dataMapThree = new Dictionary<String, Object>(dataMap);

            dataMapThree["id"] = 3;
            _epService.EPRuntime.SendEvent(dataMapThree, "TypeMapTwo");

            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("id"));
            Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("id"));
            Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("id"));

            Assert.AreEqual("E1",
                            ((EventBean) eventBean.GetFragment("one[0].bean")).Get(
                                "TheString"));
            Assert.AreEqual("E1",
                            ((EventBean) eventBean.GetFragment("one[1].bean")).Get(
                                "TheString"));
            Assert.AreEqual("E1",
                            ((EventBean) eventBean.GetFragment("two.bean")).Get("TheString"));

            Assert.AreEqual("E2",
                            ((EventBean) eventBean.GetFragment("one[0].beanarray[1]")).Get(
                                "TheString"));
            Assert.AreEqual("E2",
                            ((EventBean) eventBean.GetFragment("two.beanarray[1]")).Get(
                                "TheString"));

            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("two.complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("one[0].map")).Get("p2id"));
            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("two.map")).Get("p2id"));

            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("one[0].maparray[1]")).Get(
                                "p2id"));
            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("two.maparray[1]")).Get(
                                "p2id"));
        }

        [Test]
        public void TestMapSimpleTypes()
        {
            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0int"] = typeof (int);
            mapOuter["p0intarray"] = typeof (int[]);
            mapOuter["p0map"] = typeof (Map);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;

            var dataInner = new Dictionary<String, Object>();
            dataInner["p1someval"] = "A";

            var dataRoot = new Dictionary<String, Object>();
            dataRoot["p0simple"] = 99;
            dataRoot.Put("p0array", new int[] { 101, 102 });
            dataRoot["p0map"] = dataInner;

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    //comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // resolve property via fragment
            Assert.IsNull(eventType.GetFragmentType("p0int"));
            Assert.IsNull(eventType.GetFragmentType("p0intarray"));
            Assert.IsNull(eventBean.GetFragment("p0map?"));
            Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
            Assert.IsNull(eventBean.GetFragment("p0map('a')?"));
        }

        [Test]
        public void TestNativeBeanFragment()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from " + typeof (SupportBeanComplexProps).FullName);

            stmt.Events += _listener.Update;
            stmt = _epService.EPAdministrator.CreateEPL(
                "select * from " + typeof (SupportBeanCombinedProps).FullName);
            stmt.Events += _listener.Update;

            // assert Nested fragments
            _epService.EPRuntime.SendEvent(
                SupportBeanComplexProps.MakeDefaultBean());
            EventBean eventBean = _listener.AssertOneGetNewAndReset();

            EventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));

            Assert.IsTrue(eventBean.EventType.GetPropertyDescriptor("Nested").IsFragment);
            var eventNested = (EventBean) eventBean.GetFragment("Nested");

            Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));
            eventNested = (EventBean) eventBean.GetFragment("Nested?");
            Assert.AreEqual("NestedValue", eventNested.Get("NestedValue"));

            Assert.IsTrue(
                eventNested.EventType.GetPropertyDescriptor("NestedNested").IsFragment);
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventNested.GetFragment("NestedNested")).Get(
                                "NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventNested.GetFragment("NestedNested?")).Get(
                                "NestedNestedValue"));

            var nestedFragment = (EventBean) eventBean.GetFragment(
                "Nested.NestedNested");

            Assert.AreEqual("NestedNestedValue",
                            nestedFragment.Get("NestedNestedValue"));

            // assert indexed fragments
            SupportBeanCombinedProps eventObject = SupportBeanCombinedProps.MakeDefaultBean();

            _epService.EPRuntime.SendEvent(eventObject);
            eventBean = _listener.AssertOneGetNewAndReset();
            EventTypeAssertionUtil.AssertConsistency(eventBean.EventType);
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));

            Assert.IsTrue(
                eventBean.EventType.GetPropertyDescriptor("Array").IsFragment);
            Assert.IsTrue(
                eventBean.EventType.GetPropertyDescriptor("Array").IsIndexed);
            var eventArray = (EventBean[]) eventBean.GetFragment("Array");

            Assert.AreEqual(3, eventArray.Length);

            EventBean eventElement = eventArray[0];

            Assert.AreSame(eventObject.Array[0].GetMapped("0ma"),
                           eventElement.Get("mapped('0ma')"));
            Assert.AreSame(eventObject.Array[0].GetMapped("0ma"),
                           ((EventBean) eventBean.GetFragment("array[0]")).Get(
                               "mapped('0ma')"));
            Assert.AreSame(eventObject.Array[0].GetMapped("0ma"),
                           ((EventBean) eventBean.GetFragment("array[0]?")).Get(
                               "mapped('0ma')"));
        }

        [Test]
        public void TestObjectArrayFragment3Level()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev1", new String[]
                {
                    "p2id"
                }
                , new Object[]
                {
                    typeof (int)
                }
                );
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", new String[]
                {
                    "p1simple", "p1array"
                }
                , new Object[]
                {
                    "TypeLev1", "TypeLev1[]"
                }
                );
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", new String[]
                {
                    "p0simple", "p0array"
                }
                , new Object[]
                {
                    "TypeLev0", "TypeLev0[]"
                }
                );

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof (Object[]), stmt.EventType.UnderlyingType);

            var dataLev1 = new Object[]
            {
                10
            }
                ;
            var dataLev0 = new Object[]
            {
                dataLev1, new Object[]
                {
                    dataLev1, dataLev1
                }
            }
                ;
            var dataRoot = new Object[]
            {
                dataLev0, new Object[]
                {
                    dataLev0, dataLev0
                }
            }
                ;

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.AreEqual(10,
                            ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get(
                                "p2id"));
            Assert.AreEqual(10,
                            ((EventBean) eventBean.GetFragment("p0array[1].p1simple")).Get(
                                "p2id"));
            Assert.AreEqual(10,
                            ((EventBean) eventBean.GetFragment("p0array[1].p1array[0]")).Get(
                                "p2id"));
            Assert.AreEqual(10,
                            ((EventBean) eventBean.GetFragment("p0simple.p1array[0]")).Get(
                                "p2id"));

            // resolve property via fragment
            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");

            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));

            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[1];
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));

            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual(10, assertEvent.Get("p1simple.p2id"));
            Assert.AreEqual(10,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get("p2id"));

            Assert.AreEqual("TypeLev1", eventType.GetFragmentType("p0array.p1simple").FragmentType.Name);
            Assert.AreEqual(typeof (int), eventType.GetFragmentType("p0array.p1simple").FragmentType.GetPropertyType("p2id"));
            Assert.AreEqual(typeof (int), eventType.GetFragmentType("p0array[0].p1array[0]").FragmentType.GetPropertyDescriptor("p2id").PropertyType);
            Assert.IsFalse(eventType.GetFragmentType("p0simple.p1simple").IsIndexed);
            Assert.IsTrue(eventType.GetFragmentType("p0simple.p1array").IsIndexed);

            TryInvalid((EventBean) eventBean.GetFragment("p0simple"),
                       "p1simple.p1id");
        }

        [Test]
        public void TestObjectArrayFragmentBeans()
        {
            String[] propsLev0 =
                {
                    "p1simple", "p1array", "p1complex", "p1complexarray"
                }
                ;
            Object[] typesLev0 =
                {
                    typeof (SupportBean), typeof (SupportBean[]),
                    typeof (SupportBeanComplexProps), typeof (SupportBeanComplexProps[])
                }
                ;

            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", propsLev0, typesLev0);

            String[] propsOuter =
                {
                    "p0simple", "p0array"
                }
                ;
            Object[] typesOuter =
                {
                    "TypeLev0", "TypeLev0[]"
                }
                ;

            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", propsOuter, typesOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof (Object[]), stmt.EventType.UnderlyingType);

            Object[] dataInner =
                {
                    new SupportBean("E1", 11), new SupportBean[]
                    {
                        new SupportBean("A1", 21), new SupportBean("A2", 22)
                    }
                    , SupportBeanComplexProps.MakeDefaultBean(),
                    new SupportBeanComplexProps[]
                    {
                        SupportBeanComplexProps.MakeDefaultBean(),
                        SupportBeanComplexProps.MakeDefaultBean()
                    }
                }
                ;
            var dataRoot = new Object[]
            {
                dataInner, new Object[]
                {
                    dataInner, dataInner
                }
            }
                ;

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.AreEqual(11,
                            ((EventBean) eventBean.GetFragment("p0simple.p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual("A2",
                            ((EventBean) eventBean.GetFragment("p0simple.p1array[1]")).Get(
                                "TheString"));
            Assert.AreEqual("Simple",
                            ((EventBean) eventBean.GetFragment("p0simple.p1complex")).Get(
                                "SimpleProperty"));
            Assert.AreEqual("Simple",
                            ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0]")).Get(
                                "SimpleProperty"));
            Assert.AreEqual("NestedValue",
                            ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested")).Get(
                                "NestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("p0simple.p1complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            var assertEvent = (EventBean) eventBean.GetFragment("p0simple");

            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual(22,
                            ((EventBean) assertEvent.GetFragment("p1array[1]")).Get(
                                "IntPrimitive"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) assertEvent.GetFragment("p1complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            assertEvent = ((EventBean[]) eventBean.GetFragment("p0array"))[0];
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual(22,
                            ((EventBean) assertEvent.GetFragment("p1array[1]")).Get(
                                "IntPrimitive"));

            assertEvent = (EventBean) eventBean.GetFragment("p0array[0]");
            Assert.AreEqual("E1", assertEvent.Get("p1simple.TheString"));
            Assert.AreEqual(11,
                            ((EventBean) assertEvent.GetFragment("p1simple")).Get(
                                "IntPrimitive"));
            Assert.AreEqual(22,
                            ((EventBean) assertEvent.GetFragment("p1array[1]")).Get(
                                "IntPrimitive"));
        }

        [Test]
        public void TestObjectArrayFragmentObjectArrayNested()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", new String[]
                {
                    "p1id"
                }
                , new Object[]
                {
                    typeof (int)
                }
                );
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", new String[]
                {
                    "p0simple", "p0array"
                }
                , new Object[]
                {
                    "TypeLev0", "TypeLev0[]"
                }
                );

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof (Object[]), stmt.EventType.UnderlyingType);

            _epService.EPRuntime.SendEvent(new Object[]
            {
                new Object[]
                {
                    10
                }
                , new Object[]
                {
                    new Object[]
                    {
                        20
                    }
                    , new Object[]
                    {
                        21
                    }
                }
            }
                                           , "TypeRoot");

            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0array").IsFragment);

            var innerSimpleEvent = (EventBean) eventBean.GetFragment(
                "p0simple");

            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

            var innerArrayAllEvent = (EventBean[]) eventBean.GetFragment(
                "p0array");

            Assert.AreEqual(20, innerArrayAllEvent[0].Get("p1id"));

            var innerArrayElementEvent = (EventBean) eventBean.GetFragment(
                "p0array[0]");

            Assert.AreEqual(20, innerArrayElementEvent.Get("p1id"));

            // resolve property via getter
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));
            Assert.AreEqual(21, eventBean.Get("p0array[1].p1id"));

            Assert.IsNull(eventType.GetFragmentType("p0array.p1id"));
            Assert.IsNull(eventType.GetFragmentType("p0array[0].p1id"));
        }

        [Test]
        public void TestObjectArrayFragmentTransposedMapEventBean()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeInner", new String[]
                {
                    "p2id"
                }
                , new Object[]
                {
                    typeof (int)
                }
                );

            String[] props =
                {
                    "id", "bean", "beanarray", "complex", "complexarray", "map",
                    "maparray"
                }
                ;
            Object[] types =
                {
                    typeof (int), typeof (SupportBean), typeof (SupportBean[]),
                    typeof (SupportBeanComplexProps), typeof (SupportBeanComplexProps[]),
                    "TypeInner", "TypeInner[]"
                }
                ;

            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeMapOne", props, types);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeMapTwo", props, types);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from pattern[one=TypeMapOne until two=TypeMapTwo]");

            stmt.Events += _listener.Update;

            var dataInner = new Object[]
            {
                2000
            }
                ;
            var dataArray = new Object[]
            {
                1, new SupportBean("E1", 100), new SupportBean[]
                {
                    new SupportBean("E1", 100), new SupportBean("E2", 200)
                }
                , SupportBeanComplexProps.MakeDefaultBean(),
                new SupportBeanComplexProps[]
                {
                    SupportBeanComplexProps.MakeDefaultBean()
                }
                , dataInner, new Object[]
                {
                    dataInner, dataInner
                }
            }
                ;

            // send event
            _epService.EPRuntime.SendEvent(dataArray, "TypeMapOne");

            var dataArrayTwo = new Object[dataArray.Length];

            Array.Copy(dataArray, 0, dataArrayTwo, 0, dataArray.Length);
            dataArrayTwo[0] = 2;
            _epService.EPRuntime.SendEvent(dataArrayTwo, "TypeMapOne");

            var dataArrayThree = new Object[dataArray.Length];

            Array.Copy(dataArray, 0, dataArrayThree, 0, dataArray.Length);
            dataArrayThree[0] = 3;
            _epService.EPRuntime.SendEvent(dataArrayThree, "TypeMapTwo");

            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            Assert.AreEqual(1, ((EventBean) eventBean.GetFragment("one[0]")).Get("id"));
            Assert.AreEqual(2, ((EventBean) eventBean.GetFragment("one[1]")).Get("id"));
            Assert.AreEqual(3, ((EventBean) eventBean.GetFragment("two")).Get("id"));

            Assert.AreEqual("E1",
                            ((EventBean) eventBean.GetFragment("one[0].bean")).Get(
                                "TheString"));
            Assert.AreEqual("E1",
                            ((EventBean) eventBean.GetFragment("one[1].bean")).Get(
                                "TheString"));
            Assert.AreEqual("E1",
                            ((EventBean) eventBean.GetFragment("two.bean")).Get("TheString"));

            Assert.AreEqual("E2",
                            ((EventBean) eventBean.GetFragment("one[0].beanarray[1]")).Get(
                                "TheString"));
            Assert.AreEqual("E2",
                            ((EventBean) eventBean.GetFragment("two.beanarray[1]")).Get(
                                "TheString"));

            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("one[0].complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("two.complex.Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("one[0].complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("two.complexarray[0].Nested.NestedNested")).Get(
                                "NestedNestedValue"));

            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("one[0].map")).Get("p2id"));
            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("two.map")).Get("p2id"));

            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("one[0].maparray[1]")).Get(
                                "p2id"));
            Assert.AreEqual(2000,
                            ((EventBean) eventBean.GetFragment("two.maparray[1]")).Get(
                                "p2id"));
        }

        [Test]
        public void TestObjectArraySimpleTypes()
        {
            String[] props = { "p0int", "p0intarray", "p0map" };
            Object[] types = { typeof (int), typeof (int[]), typeof (Map) };

            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", props, types);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from TypeRoot");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p1someval"] = "A";
            var dataRoot = new Object[] { 99, new int[] { 101, 102 }, dataInner };

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    //comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // resolve property via fragment
            Assert.IsNull(eventType.GetFragmentType("p0int"));
            Assert.IsNull(eventType.GetFragmentType("p0intarray"));
            Assert.IsNull(eventBean.GetFragment("p0map?"));
            Assert.IsNull(eventBean.GetFragment("p0intarray[0]?"));
            Assert.IsNull(eventBean.GetFragment("p0map('a')?"));
        }

        [Test]
        public void TestWrapperFragmentWithMap()
        {
            IDictionary<String, Object> typeLev0 = new Dictionary<String, Object>();

            typeLev0["p1id"] = typeof (int);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", typeLev0);

            IDictionary<String, Object> mapOuter = new Dictionary<String, Object>();

            mapOuter["p0simple"] = "TypeLev0";
            mapOuter["p0bean"] = typeof (SupportBeanComplexProps);
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", mapOuter);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select *, p0simple.p1id + 1 as plusone, p0bean as mybean from TypeRoot");

            stmt.Events += _listener.Update;

            IDictionary<String, Object> dataInner = new Dictionary<String, Object>();

            dataInner["p1id"] = 10;

            IDictionary<String, Object> dataRoot = new Dictionary<String, Object>();

            dataRoot["p0simple"] = dataInner;
            dataRoot["p0bean"] = SupportBeanComplexProps.MakeDefaultBean();

            // send event
            _epService.EPRuntime.SendEvent(dataRoot, "TypeRoot");
            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.AreEqual(11, eventBean.Get("plusone"));
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));

            var innerSimpleEvent = (EventBean) eventBean.GetFragment(
                "p0simple");

            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

            var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");

            Assert.AreEqual("NestedNestedValue",
                            innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("mybean.Nested.NestedNested")).Get(
                                "NestedNestedValue"));
        }

        [Test]
        public void TestWrapperFragmentWithObjectArray()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeLev0", new String[]
                {
                    "p1id"
                }
                , new Object[]
                {
                    typeof (int)
                }
                );
            _epService.EPAdministrator.Configuration.AddEventType(
                "TypeRoot", new String[]
                {
                    "p0simple", "p0bean"
                }
                , new Object[]
                {
                    "TypeLev0", typeof (SupportBeanComplexProps)
                }
                );

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select *, p0simple.p1id + 1 as plusone, p0bean as mybean from TypeRoot");

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new Object[]
            {
                new Object[]
                {
                    10
                }
                , SupportBeanComplexProps.MakeDefaultBean()
            }
                                           , "TypeRoot");

            EventBean eventBean = _listener.AssertOneGetNewAndReset();
            // Console.WriteLine(EventTypeAssertionUtil.Print(eventBean));    comment me in
            EventType eventType = eventBean.EventType;

            EventTypeAssertionUtil.AssertConsistency(eventType);

            // resolve property via fragment
            Assert.IsTrue(eventType.GetPropertyDescriptor("p0simple").IsFragment);
            Assert.AreEqual(11, eventBean.Get("plusone"));
            Assert.AreEqual(10, eventBean.Get("p0simple.p1id"));

            var innerSimpleEvent = (EventBean) eventBean.GetFragment(
                "p0simple");

            Assert.AreEqual(10, innerSimpleEvent.Get("p1id"));

            var innerBeanEvent = (EventBean) eventBean.GetFragment("mybean");

            Assert.AreEqual("NestedNestedValue",
                            innerBeanEvent.Get("Nested.NestedNested.NestedNestedValue"));
            Assert.AreEqual("NestedNestedValue",
                            ((EventBean) eventBean.GetFragment("mybean.Nested.NestedNested")).Get(
                                "NestedNestedValue"));
        }
    }
}