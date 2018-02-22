///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestMapEventType
    {
        private MapEventType _eventType;
        private EventAdapterService _eventAdapterService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _eventAdapterService = _container.Resolve<EventAdapterService>();

            EventTypeMetadata metadata = EventTypeMetadata.CreateNonPonoApplicationType(
                ApplicationType.MAP, "typename", true, true,
                true, false, false);
            IDictionary<String, Object> testTypesMap = new Dictionary<String, Object>();
            testTypesMap["MyInt"] = typeof (int);
            testTypesMap["MyString"] = typeof (string);
            testTypesMap["myNullableString"] = typeof (string);
            testTypesMap["mySupportBean"] = typeof (SupportBean);
            testTypesMap["myComplexBean"] = typeof (SupportBeanComplexProps);
            testTypesMap["myNullableSupportBean"] = typeof (SupportBean);
            testTypesMap["myNullType"] = null;
            _eventType = new MapEventType(metadata, "", 1, _eventAdapterService, testTypesMap, null, null, null);
        }

        private IDictionary<String, Object> GetTestData()
        {
            IDictionary<String, Object> levelThree = new Dictionary<String, Object>();
            levelThree["simpleThree"] = 4000L;
            levelThree["objThree"] = new SupportBean_D("D1");

            IDictionary<String, Object> levelTwo = new Dictionary<String, Object>();
            levelTwo["simpleTwo"] = 300f;
            levelTwo["objTwo"] = new SupportBean_C("C1");
            levelTwo["mapTwo"] = levelThree;

            IDictionary<String, Object> levelOne = new Dictionary<String, Object>();
            levelOne["simpleOne"] = 20;
            levelOne["objOne"] = new SupportBean_B("B1");
            levelOne["mapOne"] = levelTwo;

            IDictionary<String, Object> levelZero = new Dictionary<String, Object>();
            levelZero["simple"] = 1d;
            levelZero["obj"] = new SupportBean_A("A1");
            levelZero["map"] = levelOne;
            IDictionary<String, Object> noDefZero = new Dictionary<String, Object>();
            noDefZero["item"] = "|nodefmap.item|";
            levelZero["nodefmap"] = noDefZero;

            return levelZero;
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestEquals()
        {
            EventTypeMetadata metadata = EventTypeMetadata.CreateNonPonoApplicationType(
                   ApplicationType.MAP, "", true, true, true,
                   false, false);

            IDictionary<String, Object> mapTwo = new LinkedHashMap<String, Object>();
            mapTwo["MyInt"] = typeof (int);
            mapTwo["mySupportBean"] = typeof (SupportBean);
            mapTwo["myNullableSupportBean"] = typeof (SupportBean);
            mapTwo["myComplexBean"] = typeof (SupportBeanComplexProps);
            Assert.IsFalse(
                (new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null, null)).Equals(_eventType));
            mapTwo["MyString"] = typeof (string);
            mapTwo["myNullableString"] = typeof (string);
            mapTwo["myNullType"] = null;

            // compare, should equal
            Assert.IsTrue(
                new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null, null).EqualsCompareType(
                    _eventType));
            Assert.IsFalse(
                (new MapEventType(metadata, "google", 1, _eventAdapterService, mapTwo, null, null, null)).
                    EqualsCompareType(_eventType));

            mapTwo["xx"] = typeof (int);
            Assert.IsFalse(
                _eventType.EqualsCompareType(new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null,
                                                              null)));
            mapTwo.Remove("xx");
            Assert.IsTrue(
                _eventType.EqualsCompareType(new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null,
                                                              null)));

            mapTwo["MyInt"] = typeof (int?);
            Assert.IsTrue(
                _eventType.EqualsCompareType(new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null,
                                                              null)));
            mapTwo.Remove("MyInt");
            Assert.IsFalse(
                _eventType.EqualsCompareType(new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null,
                                                              null)));
            mapTwo["MyInt"] = typeof (int);
            Assert.IsTrue(
                _eventType.EqualsCompareType(new MapEventType(metadata, "", 1, _eventAdapterService, mapTwo, null, null,
                                                              null)));

            // Test boxed and primitive compatible
            IDictionary<String, Object> mapOne = new LinkedHashMap<String, Object>();
            mapOne["MyInt"] = typeof (int);
            mapTwo = new LinkedHashMap<String, Object>();
            mapTwo["MyInt"] = typeof (int);

            Assert.IsTrue(
                new MapEventType(metadata, "T1", 1, _eventAdapterService, mapOne, null, null, null).EqualsCompareType(
                    new MapEventType(metadata, "T1", 1, _eventAdapterService, mapTwo, null, null, null)));
        }

        [Test]
        public void TestGetFromMap()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            SupportBeanComplexProps complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            IDictionary<String, Object> valuesMap = new Dictionary<String, Object>();
            valuesMap["MyInt"] = 20;
            valuesMap["MyString"] = "a";
            valuesMap["mySupportBean"] = nestedSupportBean;
            valuesMap["myComplexBean"] = complexPropBean;
            valuesMap["myNullableSupportBean"] = null;
            valuesMap["myNullableString"] = null;

            Assert.AreEqual(20, _eventType.GetValue("MyInt", valuesMap));
            Assert.AreEqual(100, _eventType.GetValue("mySupportBean.IntPrimitive", valuesMap));
            Assert.AreEqual("NestedValue", _eventType.GetValue("myComplexBean.Nested.NestedValue", valuesMap));
        }

        [Test]
        public void TestGetGetter()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            SupportBeanComplexProps complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            Assert.AreEqual(null, _eventType.GetGetter("dummy"));

            IDictionary<String, Object> valuesMap = new Dictionary<String, Object>();
            valuesMap["MyInt"] = 20;
            valuesMap["MyString"] = "a";
            valuesMap["mySupportBean"] = nestedSupportBean;
            valuesMap["myComplexBean"] = complexPropBean;
            valuesMap["myNullableSupportBean"] = null;
            valuesMap["myNullableString"] = null;
            EventBean eventBean = new MapEventBean(valuesMap, _eventType);

            EventPropertyGetter getter = _eventType.GetGetter("MyInt");
            Assert.AreEqual(20, getter.Get(eventBean));

            getter = _eventType.GetGetter("MyString");
            Assert.AreEqual("a", getter.Get(eventBean));

            getter = _eventType.GetGetter("myNullableString");
            Assert.IsNull(getter.Get(eventBean));

            getter = _eventType.GetGetter("mySupportBean");
            Assert.AreEqual(nestedSupportBean, getter.Get(eventBean));

            getter = _eventType.GetGetter("mySupportBean.IntPrimitive");
            Assert.AreEqual(100, getter.Get(eventBean));

            getter = _eventType.GetGetter("myNullableSupportBean.IntPrimitive");
            Assert.IsNull(getter.Get(eventBean));

            getter = _eventType.GetGetter("myComplexBean.Nested.NestedValue");
            Assert.AreEqual("NestedValue", getter.Get(eventBean));

            try
            {
                eventBean = SupportEventBeanFactory.CreateObject(new Object());
                getter.Get(eventBean);
                Assert.IsTrue(false);
            }
            catch (InvalidCastException ex)
            {
                // Expected
                Log.Debug(".testGetGetter Expected exception, msg=" + ex.Message);
            }
        }

        [Test]
        public void TestGetPropertyNames()
        {
            IList<string> properties = _eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(properties,
                                                 new[]
                                                 {
                                                     "MyInt", "MyString", "myNullableString", "mySupportBean",
                                                     "myComplexBean", "myNullableSupportBean", "myNullType"
                                                 });
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof (int), _eventType.GetPropertyType("MyInt"));
            Assert.AreEqual(typeof (string), _eventType.GetPropertyType("MyString"));
            Assert.AreEqual(typeof (SupportBean), _eventType.GetPropertyType("mySupportBean"));
            Assert.AreEqual(typeof (SupportBeanComplexProps), _eventType.GetPropertyType("myComplexBean"));
            Assert.AreEqual(typeof (int), _eventType.GetPropertyType("mySupportBean.IntPrimitive"));
            Assert.AreEqual(typeof (string), _eventType.GetPropertyType("myComplexBean.Nested.NestedValue"));
            Assert.AreEqual(typeof (int), _eventType.GetPropertyType("myComplexBean.Indexed[1]"));
            Assert.AreEqual(typeof (string), _eventType.GetPropertyType("myComplexBean.Mapped('a')"));
            Assert.AreEqual(null, _eventType.GetPropertyType("myNullType"));

            Assert.IsNull(_eventType.GetPropertyType("dummy"));
            Assert.IsNull(_eventType.GetPropertyType("mySupportBean.dfgdg"));
            Assert.IsNull(_eventType.GetPropertyType("xxx.IntPrimitive"));
            Assert.IsNull(_eventType.GetPropertyType("myComplexBean.Nested.NestedValueXXX"));
        }

        [Test]
        public void TestGetSuperTypes()
        {
            Assert.IsNull(_eventType.SuperTypes);
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof (Map), _eventType.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(_eventType.IsProperty("MyInt"));
            Assert.IsTrue(_eventType.IsProperty("MyString"));
            Assert.IsTrue(_eventType.IsProperty("mySupportBean.IntPrimitive"));
            Assert.IsTrue(_eventType.IsProperty("myComplexBean.Nested.NestedValue"));
            Assert.IsTrue(_eventType.IsProperty("myComplexBean.Indexed[1]"));
            Assert.IsTrue(_eventType.IsProperty("myComplexBean.Mapped('a')"));
            Assert.IsTrue(_eventType.IsProperty("myNullType"));

            Assert.IsFalse(_eventType.IsProperty("dummy"));
            Assert.IsFalse(_eventType.IsProperty("mySupportBean.dfgdg"));
            Assert.IsFalse(_eventType.IsProperty("xxx.IntPrimitive"));
            Assert.IsFalse(_eventType.IsProperty("myComplexBean.Nested.NestedValueXXX"));
        }

        [Test]
        public void TestNestedMap()
        {
            IDictionary<String, Object> levelThree = new Dictionary<String, Object>();
            levelThree["simpleThree"] = typeof (long);
            levelThree["objThree"] = typeof (SupportBean_D);
            levelThree["nodefmapThree"] = typeof (Map);

            IDictionary<String, Object> levelTwo = new Dictionary<String, Object>();
            levelTwo["simpleTwo"] = typeof (float);
            levelTwo["objTwo"] = typeof (SupportBean_C);
            levelTwo["nodefmapTwo"] = typeof (Map);
            levelTwo["mapTwo"] = levelThree;

            IDictionary<String, Object> levelOne = new Dictionary<String, Object>();
            levelOne["simpleOne"] = typeof (int);
            levelOne["objOne"] = typeof (SupportBean_B);
            levelOne["nodefmapOne"] = typeof (Map);
            levelOne["mapOne"] = levelTwo;

            IDictionary<String, Object> levelZero = new Dictionary<String, Object>();
            levelZero["simple"] = typeof (double);
            levelZero["obj"] = typeof (SupportBean_A);
            levelZero["nodefmap"] = typeof (Map);
            levelZero["map"] = levelOne;

            EventTypeMetadata metadata = EventTypeMetadata.CreateNonPonoApplicationType(ApplicationType.MAP, "testtype", true, true, true, false, false);
            MapEventType mapType = new MapEventType(metadata, "M1", 1, _eventAdapterService, levelZero, null, null, null);

            IDictionary<String, Object> testData = GetTestData();
            var theEvent = new MapEventBean(testData, mapType);

            var expected = new[]
            {
                new object[] {"map.mapOne.simpleTwo", typeof (float?), 300f},
                new object[] {"nodefmap.item?", typeof (object), "|nodefmap.item|"},
                new object[] {"map.objOne", typeof (SupportBean_B), new SupportBean_B("B1")},
                new object[] {"map.simpleOne", typeof (int?), 20},
                new object[] {"map.mapOne", typeof (Map), ((Map) testData.Get("map")).Get("mapOne")},
                new object[] {"map.mapOne.objTwo", typeof (SupportBean_C), new SupportBean_C("C1")},
                new object[] {"map.mapOne.mapTwo", typeof (Map), ((Map) ((Map) testData.Get("map")).Get("mapOne")).Get("mapTwo")},
                new object[] {"map.mapOne.mapTwo.simpleThree", typeof (long?), 4000L},
                new object[] {"map.mapOne.mapTwo.objThree", typeof (SupportBean_D), new SupportBean_D("D1")},
                new object[] {"simple", typeof (double), 1d},
                new object[] {"obj", typeof (SupportBean_A), new SupportBean_A("A1")},
                new object[] {"nodefmap", typeof (Map), testData.Get("nodefmap")},
                new object[] {"map", typeof (Map), testData.Get("map")},
            };

            // assert getter available for all properties
            for (int i = 0; i < expected.Length; i++)
            {
                var propName = (String) expected[i][0];
                Assert.NotNull(mapType.GetGetter(propName), "failed for property:" + propName);
            }

            // assert property types
            for (int i = 0; i < expected.Length; i++)
            {
                var propName = (String) expected[i][0];
                var propType = (Type) expected[i][1];
                Assert.AreEqual(propType, mapType.GetPropertyType(propName), "failed for property:" + propName);
            }

            // assert property names
            var expectedPropNames = new[] {"simple", "obj", "map", "nodefmap"};
            IList<string> receivedPropNames = mapType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expectedPropNames, receivedPropNames);

            // assert get value through (1) type getter  (2) event-get
            for (int i = 0; i < expected.Length; i++)
            {
                var propName = (String) expected[i][0];
                Object valueExpected = expected[i][2];
                Assert.AreEqual(valueExpected, mapType.GetGetter(propName).Get(theEvent),
                                "failed for property type-getter:" + propName);
                Assert.AreEqual(valueExpected, theEvent.Get(propName),
                                "failed for property event-getter:" + propName);
            }

            // assert access to objects nested within
            expected = new[]
            {
                new object[] {"map.objOne.Id", typeof (string), "B1"},
                new object[] {"map.mapOne.objTwo.Id", typeof (string), "C1"},
                new object[] {"obj.Id", typeof (string), "A1"},
            };
            for (int i = 0; i < expected.Length; i++)
            {
                var propName = (String) expected[i][0];
                var propType = (Type) expected[i][1];
                object valueExpected = expected[i][2];
                EventPropertyGetter getter = mapType.GetGetter(propName);
                Assert.AreEqual(propType, mapType.GetPropertyType(propName),
                                "failed for property:" + propName);
                Assert.AreEqual(valueExpected, getter.Get(theEvent),
                                "failed for property type-getter:" + propName);
                Assert.AreEqual(valueExpected, theEvent.Get(propName),
                                "failed for property event-getter:" + propName);
            }
        }
    }
}