///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.map
{
    [TestFixture]
    public class TestMapEventType : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            var metadata = new EventTypeMetadata(
                "MyType",
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP,
                NameAccessModifier.INTERNAL,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());

            IDictionary<string, object> testTypesMap = new Dictionary<string, object>();
            testTypesMap.Put("MyInt", typeof(int?));
            testTypesMap.Put("MyString", typeof(string));
            testTypesMap.Put("MyNullableString", typeof(string));
            testTypesMap.Put("MySupportBean", typeof(SupportBean));
            testTypesMap.Put("MyComplexBean", typeof(SupportBeanComplexProps));
            testTypesMap.Put("MyNullableSupportBean", typeof(SupportBean));
            testTypesMap.Put("MyNullType", TypeHelper.NullType);
            eventType = new MapEventType(metadata, testTypesMap, null, null, null, null,
                supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
        }

        private MapEventType eventType;

        private IDictionary<string, object> GetTestData()
        {
            IDictionary<string, object> levelThree = new Dictionary<string, object>();
            levelThree.Put("simpleThree", 4000L);
            levelThree.Put("objThree", new SupportBean_D("D1"));

            IDictionary<string, object> levelTwo = new Dictionary<string, object>();
            levelTwo.Put("simpleTwo", 300f);
            levelTwo.Put("objTwo", new SupportBean_C("C1"));
            levelTwo.Put("mapTwo", levelThree);

            IDictionary<string, object> levelOne = new Dictionary<string, object>();
            levelOne.Put("simpleOne", 20);
            levelOne.Put("objOne", new SupportBean_B("B1"));
            levelOne.Put("mapOne", levelTwo);

            IDictionary<string, object> levelZero = new Dictionary<string, object>();
            levelZero.Put("simple", 1d);
            levelZero.Put("obj", new SupportBean_A("A1"));
            levelZero.Put("map", levelOne);
            IDictionary<string, object> noDefZero = new Dictionary<string, object>();
            noDefZero.Put("item", "|nodefmap.item|");
            levelZero.Put("nodefmap", noDefZero);

            return levelZero;
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestEquals()
        {
            var metadata = new EventTypeMetadata(
                "MyType",
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP,
                NameAccessModifier.INTERNAL,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());

            IDictionary<string, object> mapTwo = new LinkedHashMap<string, object>();
            mapTwo.Put("MyInt", typeof(int));
            mapTwo.Put("MySupportBean", typeof(SupportBean));
            mapTwo.Put("MyNullableSupportBean", typeof(SupportBean));
            mapTwo.Put("MyComplexBean", typeof(SupportBeanComplexProps));
            Assert.IsFalse(
                new MapEventType(metadata, mapTwo, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).Equals(eventType));
            mapTwo.Put("MyString", typeof(string));
            mapTwo.Put("MyNullableString", typeof(string));
            mapTwo.Put("MyNullType", null);

            // compare, should equal
            Assert.IsNull(
                new MapEventType(metadata, mapTwo, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).EqualsCompareType(
                    eventType));
            Assert.AreEqual(
                null,
                new MapEventType(metadata, mapTwo, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).EqualsCompareType(
                    eventType));

            // Test boxed and primitive compatible
            IDictionary<string, object> mapOne = new LinkedHashMap<string, object>();
            mapOne.Put("MyInt", typeof(int));
            mapTwo = new LinkedHashMap<string, object>();
            mapTwo.Put("MyInt", typeof(int?));
            Assert.IsNull(
                new MapEventType(metadata, mapOne, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).EqualsCompareType(
                    new MapEventType(metadata, mapTwo, null, null, null, null,
                        supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY)));
        }

        [Test]
        public void TestGetFromMap()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            var complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            IDictionary<string, object> valuesMap = new Dictionary<string, object>();
            valuesMap.Put("MyInt", 20);
            valuesMap.Put("MyString", "a");
            valuesMap.Put("MySupportBean", nestedSupportBean);
            valuesMap.Put("MyComplexBean", complexPropBean);
            valuesMap.Put("MyNullableSupportBean", null);
            valuesMap.Put("MyNullableString", null);

            Assert.AreEqual(20, eventType.GetValue("MyInt", valuesMap));
            Assert.AreEqual(100, eventType.GetValue("MySupportBean.IntPrimitive", valuesMap));
            Assert.AreEqual("NestedValue", eventType.GetValue("MyComplexBean.Nested.NestedValue", valuesMap));
        }

        [Test]
        public void TestGetGetter()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            var complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            Assert.AreEqual(null, eventType.GetGetter("dummy"));

            IDictionary<string, object> valuesMap = new Dictionary<string, object>();
            valuesMap.Put("MyInt", 20);
            valuesMap.Put("MyString", "a");
            valuesMap.Put("MySupportBean", nestedSupportBean);
            valuesMap.Put("MyComplexBean", complexPropBean);
            valuesMap.Put("MyNullableSupportBean", null);
            valuesMap.Put("MyNullableString", null);
            EventBean eventBean = new MapEventBean(valuesMap, eventType);

            var getter = eventType.GetGetter("MyInt");
            Assert.AreEqual(20, getter.Get(eventBean));

            getter = eventType.GetGetter("MyString");
            Assert.AreEqual("a", getter.Get(eventBean));

            getter = eventType.GetGetter("MyNullableString");
            Assert.IsNull(getter.Get(eventBean));

            getter = eventType.GetGetter("MySupportBean");
            Assert.AreEqual(nestedSupportBean, getter.Get(eventBean));

            getter = eventType.GetGetter("MySupportBean.IntPrimitive");
            Assert.AreEqual(100, getter.Get(eventBean));

            getter = eventType.GetGetter("MyNullableSupportBean.IntPrimitive");
            Assert.IsNull(getter.Get(eventBean));

            getter = eventType.GetGetter("MyComplexBean.Nested.NestedValue");
            Assert.AreEqual("NestedValue", getter.Get(eventBean));
        }

        [Test]
        public void TestGetPropertyNames()
        {
            var properties = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(
                properties,
                new[] { "MyInt", "MyString", "MyNullableString", "MySupportBean", "MyComplexBean", "MyNullableSupportBean", "MyNullType" });
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("MyInt"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("MyString"));
            Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("MySupportBean"));
            Assert.AreEqual(typeof(SupportBeanComplexProps), eventType.GetPropertyType("MyComplexBean"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("MySupportBean.IntPrimitive"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("MyComplexBean.Nested.NestedValue"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("MyComplexBean.Indexed[1]"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("MyComplexBean.Mapped('a')"));
            Assert.AreEqual(null, eventType.GetPropertyType("MyNullType"));

            Assert.IsNull(eventType.GetPropertyType("dummy"));
            Assert.IsNull(eventType.GetPropertyType("MySupportBean.dfgdg"));
            Assert.IsNull(eventType.GetPropertyType("xxx.IntPrimitive"));
            Assert.IsNull(eventType.GetPropertyType("MyComplexBean.Nested.NestedValueXXX"));
        }

        [Test]
        public void TestGetSuperTypes()
        {
            Assert.IsNull(eventType.SuperTypes);
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(IDictionary<string, object>), eventType.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(eventType.IsProperty("MyInt"));
            Assert.IsTrue(eventType.IsProperty("MyString"));
            Assert.IsTrue(eventType.IsProperty("MySupportBean.IntPrimitive"));
            Assert.IsTrue(eventType.IsProperty("MyComplexBean.Nested.NestedValue"));
            Assert.IsTrue(eventType.IsProperty("MyComplexBean.Indexed[1]"));
            Assert.IsTrue(eventType.IsProperty("MyComplexBean.Mapped('a')"));
            Assert.IsTrue(eventType.IsProperty("MyNullType"));

            Assert.IsFalse(eventType.IsProperty("dummy"));
            Assert.IsFalse(eventType.IsProperty("MySupportBean.dfgdg"));
            Assert.IsFalse(eventType.IsProperty("xxx.IntPrimitive"));
            Assert.IsFalse(eventType.IsProperty("MyComplexBean.Nested.NestedValueXXX"));
        }

        [Test]
        public void TestNestedMap()
        {
            IDictionary<string, object> levelThree = new Dictionary<string, object>();
            levelThree.Put("simpleThree", typeof(long));
            levelThree.Put("objThree", typeof(SupportBean_D));
            levelThree.Put("nodefmapThree", typeof(IDictionary<string, object>));

            IDictionary<string, object> levelTwo = new Dictionary<string, object>();
            levelTwo.Put("simpleTwo", typeof(float));
            levelTwo.Put("objTwo", typeof(SupportBean_C));
            levelTwo.Put("nodefmapTwo", typeof(IDictionary<string, object>));
            levelTwo.Put("mapTwo", levelThree);

            IDictionary<string, object> levelOne = new Dictionary<string, object>();
            levelOne.Put("simpleOne", typeof(int?));
            levelOne.Put("objOne", typeof(SupportBean_B));
            levelOne.Put("nodefmapOne", typeof(IDictionary<string, object>));
            levelOne.Put("mapOne", levelTwo);

            IDictionary<string, object> levelZero = new Dictionary<string, object>();
            levelZero.Put("simple", typeof(double?));
            levelZero.Put("obj", typeof(SupportBean_A));
            levelZero.Put("nodefmap", typeof(IDictionary<string, object>));
            levelZero.Put("map", levelOne);

            var metadata = new EventTypeMetadata(
                "MyType",
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP,
                NameAccessModifier.INTERNAL,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var mapType = new MapEventType(metadata, levelZero, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
            var testData = GetTestData();
            var theEvent = new MapEventBean(testData, mapType);

            object[][] expected = {
                new object[] {"map.mapOne.simpleTwo", typeof(float?), 300f},
                new object[] {"nodefmap.item?", typeof(object), "|nodefmap.item|"},
                new object[] {"map.objOne", typeof(SupportBean_B), new SupportBean_B("B1")},
                new object[] {"map.simpleOne", typeof(int?), 20},
                new[] {
                    "map.mapOne", typeof(IDictionary<string, object>),
                    testData.Get("map").AsDataMap().Get("mapOne")
                },
                new object[] {"map.mapOne.objTwo", typeof(SupportBean_C), new SupportBean_C("C1")},
                new[] {
                    "map.mapOne.mapTwo", typeof(IDictionary<string, object>),
                    testData.Get("map").AsDataMap().Get("mapOne").AsDataMap().Get("mapTwo")
                },
                new object[] {"map.mapOne.mapTwo.simpleThree", typeof(long?), 4000L},
                new object[] {"map.mapOne.mapTwo.objThree", typeof(SupportBean_D), new SupportBean_D("D1")},
                new object[] {"simple", typeof(double?), 1d},
                new object[] {"obj", typeof(SupportBean_A), new SupportBean_A("A1")},
                new[] {"nodefmap", typeof(IDictionary<string, object>), testData.Get("nodefmap")},
                new[] {"map", typeof(IDictionary<string, object>), testData.Get("map")}
            };

            // assert getter available for all properties
            for (var i = 0; i < expected.Length; i++)
            {
                var propName = (string) expected[i][0];
                Assert.IsNotNull(mapType.GetGetter(propName), "failed for property:" + propName);
            }

            // assert property types
            for (var i = 0; i < expected.Length; i++)
            {
                var propName = (string) expected[i][0];
                var propType = (Type) expected[i][1];
                var mapPropType = mapType.GetPropertyType(propName);
                Assert.AreEqual(propType, mapPropType, "failed for property:" + propName);
            }

            // assert property names
            string[] expectedPropNames = { "simple", "obj", "map", "nodefmap" };
            var receivedPropNames = mapType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expectedPropNames, receivedPropNames);

            // assert get value through (1) type getter  (2) event-get
            for (var i = 0; i < expected.Length; i++)
            {
                var propName = (string) expected[i][0];
                var valueExpected = expected[i][2];
                Assert.AreEqual(valueExpected, mapType.GetGetter(propName).Get(theEvent), "failed for property type-getter:" + propName);
                Assert.AreEqual(valueExpected, theEvent.Get(propName), "failed for property event-getter:" + propName);
            }

            // assert access to objects nested within
            expected = new[] {
                new object[] {"map.objOne.Id", typeof(string), "B1"},
                new object[] {"map.mapOne.objTwo.Id", typeof(string), "C1"},
                new object[] {"obj.Id", typeof(string), "A1"}
            };
            for (var i = 0; i < expected.Length; i++)
            {
                var propName = (string) expected[i][0];
                var propType = (Type) expected[i][1];
                var valueExpected = expected[i][2];
                var getter = mapType.GetGetter(propName);
                Assert.AreEqual(propType, mapType.GetPropertyType(propName), "failed for property:" + propName);
                Assert.AreEqual(valueExpected, getter.Get(theEvent), "failed for property type-getter:" + propName);
                Assert.AreEqual(valueExpected, theEvent.Get(propName), "failed for property event-getter:" + propName);
            }
        }
    }
} // end of namespace
