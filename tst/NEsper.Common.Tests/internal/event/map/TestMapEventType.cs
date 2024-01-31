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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            testTypesMap.Put("MyNullType", null);
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
            noDefZero.Put("Item", "|nodefmap.Item|");
            levelZero.Put("nodefmap", noDefZero);

            return levelZero;
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            ClassicAssert.IsFalse(
                new MapEventType(metadata, mapTwo, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).Equals(eventType));
            mapTwo.Put("MyString", typeof(string));
            mapTwo.Put("MyNullableString", typeof(string));
            mapTwo.Put("MyNullType", null);

            // compare, should equal
            ClassicAssert.IsNull(
                new MapEventType(metadata, mapTwo, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).EqualsCompareType(
                    eventType));
            ClassicAssert.AreEqual(
                null,
                new MapEventType(metadata, mapTwo, null, null, null, null, supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY).EqualsCompareType(
                    eventType));

            // Test boxed and primitive compatible
            IDictionary<string, object> mapOne = new LinkedHashMap<string, object>();
            mapOne.Put("MyInt", typeof(int));
            mapTwo = new LinkedHashMap<string, object>();
            mapTwo.Put("MyInt", typeof(int?));
            ClassicAssert.IsNull(
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

            ClassicAssert.AreEqual(20, eventType.GetValue("MyInt", valuesMap));
            ClassicAssert.AreEqual(100, eventType.GetValue("MySupportBean.IntPrimitive", valuesMap));
            ClassicAssert.AreEqual("NestedValue", eventType.GetValue("MyComplexBean.Nested.NestedValue", valuesMap));
        }

        [Test]
        public void TestGetGetter()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            var complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            ClassicAssert.AreEqual(null, eventType.GetGetter("dummy"));

            IDictionary<string, object> valuesMap = new Dictionary<string, object>();
            valuesMap.Put("MyInt", 20);
            valuesMap.Put("MyString", "a");
            valuesMap.Put("MySupportBean", nestedSupportBean);
            valuesMap.Put("MyComplexBean", complexPropBean);
            valuesMap.Put("MyNullableSupportBean", null);
            valuesMap.Put("MyNullableString", null);
            EventBean eventBean = new MapEventBean(valuesMap, eventType);

            var getter = eventType.GetGetter("MyInt");
            ClassicAssert.AreEqual(20, getter.Get(eventBean));

            getter = eventType.GetGetter("MyString");
            ClassicAssert.AreEqual("a", getter.Get(eventBean));

            getter = eventType.GetGetter("MyNullableString");
            ClassicAssert.IsNull(getter.Get(eventBean));

            getter = eventType.GetGetter("MySupportBean");
            ClassicAssert.AreEqual(nestedSupportBean, getter.Get(eventBean));

            getter = eventType.GetGetter("MySupportBean.IntPrimitive");
            ClassicAssert.AreEqual(100, getter.Get(eventBean));

            getter = eventType.GetGetter("MyNullableSupportBean.IntPrimitive");
            ClassicAssert.IsNull(getter.Get(eventBean));

            getter = eventType.GetGetter("MyComplexBean.Nested.NestedValue");
            ClassicAssert.AreEqual("NestedValue", getter.Get(eventBean));
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
            ClassicAssert.AreEqual(typeof(int?), eventType.GetPropertyType("MyInt"));
            ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("MyString"));
            ClassicAssert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("MySupportBean"));
            ClassicAssert.AreEqual(typeof(SupportBeanComplexProps), eventType.GetPropertyType("MyComplexBean"));
            ClassicAssert.AreEqual(typeof(int?), eventType.GetPropertyType("MySupportBean.IntPrimitive"));
            ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("MyComplexBean.Nested.NestedValue"));
            ClassicAssert.AreEqual(typeof(int?), eventType.GetPropertyType("MyComplexBean.Indexed[1]"));
            ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("MyComplexBean.Mapped('a')"));
            ClassicAssert.AreEqual(typeof(object), eventType.GetPropertyType("MyNullType"));

            ClassicAssert.IsNull(eventType.GetPropertyType("dummy"));
            ClassicAssert.IsNull(eventType.GetPropertyType("MySupportBean.dfgdg"));
            ClassicAssert.IsNull(eventType.GetPropertyType("xxx.IntPrimitive"));
            ClassicAssert.IsNull(eventType.GetPropertyType("MyComplexBean.Nested.NestedValueXXX"));
        }

        [Test]
        public void TestGetSuperTypes()
        {
            ClassicAssert.IsNull(eventType.SuperTypes);
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            ClassicAssert.AreEqual(typeof(IDictionary<string, object>), eventType.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            ClassicAssert.IsTrue(eventType.IsProperty("MyInt"));
            ClassicAssert.IsTrue(eventType.IsProperty("MyString"));
            ClassicAssert.IsTrue(eventType.IsProperty("MySupportBean.IntPrimitive"));
            ClassicAssert.IsTrue(eventType.IsProperty("MyComplexBean.Nested.NestedValue"));
            ClassicAssert.IsTrue(eventType.IsProperty("MyComplexBean.Indexed[1]"));
            ClassicAssert.IsTrue(eventType.IsProperty("MyComplexBean.Mapped('a')"));
            ClassicAssert.IsTrue(eventType.IsProperty("MyNullType"));

            ClassicAssert.IsFalse(eventType.IsProperty("dummy"));
            ClassicAssert.IsFalse(eventType.IsProperty("MySupportBean.dfgdg"));
            ClassicAssert.IsFalse(eventType.IsProperty("xxx.IntPrimitive"));
            ClassicAssert.IsFalse(eventType.IsProperty("MyComplexBean.Nested.NestedValueXXX"));
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
                new object[] {"nodefmap.Item?", typeof(object), "|nodefmap.Item|"},
                new object[] {"map.objOne", typeof(SupportBean_B), new SupportBean_B("B1")},
                new object[] {"map.simpleOne", typeof(int?), 20},
                new object[] {"map.mapOne", typeof(IDictionary<string, object>), testData.Get("map").AsDataMap().Get("mapOne") },
                new object[] {"map.mapOne.objTwo", typeof(SupportBean_C), new SupportBean_C("C1")},
                new object[] {"map.mapOne.mapTwo", typeof(IDictionary<string, object>), testData.Get("map").AsDataMap().Get("mapOne").AsDataMap().Get("mapTwo") },
                new object[] {"map.mapOne.mapTwo.simpleThree", typeof(long?), 4000L},
                new object[] {"map.mapOne.mapTwo.objThree", typeof(SupportBean_D), new SupportBean_D("D1")},
                new object[] {"simple", typeof(double?), 1d},
                new object[] {"obj", typeof(SupportBean_A), new SupportBean_A("A1")},
                new object[] {"nodefmap", typeof(IDictionary<string, object>), testData.Get("nodefmap")},
                new object[] {"map", typeof(IDictionary<string, object>), testData.Get("map")}
            };

            // assert getter available for all properties
            for (var i = 0; i < expected.Length; i++)
            {
                var propName = (string) expected[i][0];
                ClassicAssert.IsNotNull(mapType.GetGetter(propName), "failed for property:" + propName);
            }

            // assert property types
            for (var i = 0; i < expected.Length; i++)
            {
                var propName = (string) expected[i][0];
                var propType = (Type) expected[i][1];
                var mapPropType = mapType.GetPropertyType(propName);
                ClassicAssert.AreEqual(propType, mapPropType, "failed for property:" + propName);
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
                ClassicAssert.AreEqual(valueExpected, mapType.GetGetter(propName).Get(theEvent), "failed for property type-getter:" + propName);
                ClassicAssert.AreEqual(valueExpected, theEvent.Get(propName), "failed for property event-getter:" + propName);
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
                ClassicAssert.AreEqual(propType, mapType.GetPropertyType(propName), "failed for property:" + propName);
                ClassicAssert.AreEqual(valueExpected, getter.Get(theEvent), "failed for property type-getter:" + propName);
                ClassicAssert.AreEqual(valueExpected, theEvent.Get(propName), "failed for property event-getter:" + propName);
            }
        }
    }
} // end of namespace
