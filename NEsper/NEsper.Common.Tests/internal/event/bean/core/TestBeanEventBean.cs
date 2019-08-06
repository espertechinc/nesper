///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    [TestFixture]
    public class TestBeanEventBean : AbstractTestBase
    {
        private SupportBean testEvent;

        [SetUp]
        public void SetUp()
        {
            testEvent = new SupportBean();
            testEvent.IntPrimitive = 10;
        }

        private void AssertNestedElement(
            EventBean eventBean,
            string propertyName,
            string value)
        {
            var fragmentTypeOne = eventBean.EventType.GetFragmentType(propertyName);
            Assert.AreEqual(true, fragmentTypeOne.IsNative);
            Assert.AreEqual(false, fragmentTypeOne.IsIndexed);
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), fragmentTypeOne.FragmentType.UnderlyingType);

            var theEvent = (EventBean) eventBean.GetFragment(propertyName);
            Assert.AreEqual(value, theEvent.Get("NestedValue"));
        }

        private void AssertNestedCollection(
            EventBean eventBean,
            string propertyName,
            string prefix)
        {
            var fragmentTypeTwo = eventBean.EventType.GetFragmentType(propertyName);
            Assert.AreEqual(true, fragmentTypeTwo.IsNative);
            Assert.AreEqual(true, fragmentTypeTwo.IsIndexed);
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), fragmentTypeTwo.FragmentType.UnderlyingType);

            var events = (EventBean[]) eventBean.GetFragment(propertyName);
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual(prefix + "N1", events[0].Get("NestedValue"));
            Assert.AreEqual(prefix + "N2", events[1].Get("NestedValue"));
        }

        private static void TryInvalidGet(
            EventBean eventBean,
            string propName)
        {
            Assert.That(() => eventBean.Get(propName), Throws.InstanceOf<PropertyAccessException>());
            Assert.IsNull(eventBean.EventType.GetPropertyType(propName));
            Assert.IsNull(eventBean.EventType.GetGetter(propName));
        }

        private static void TryInvalidGetFragment(
            EventBean eventBean,
            string propName)
        {
            Assert.That(() => eventBean.GetFragment(propName), Throws.InstanceOf<PropertyAccessException>());
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestGet()
        {
            EventType eventType = supportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var eventBean = new BeanEventBean(testEvent, eventType);

            Assert.AreEqual(eventType, eventBean.EventType);
            Assert.AreEqual(testEvent, eventBean.Underlying);

            Assert.AreEqual(10, eventBean.Get("IntPrimitive"));

            // Test wrong property name
            try {
                eventBean.Get("dummy");
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex) {
                // Expected
                log.Debug(".testGetter Expected exception, msg=" + ex.Message);
            }

            // Test wrong event type - not possible to happen under normal use
            try {
                eventType = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
                eventBean = new BeanEventBean(testEvent, eventType);
                eventBean.Get("myString");
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex) {
                // Expected
                log.Debug(".testGetter Expected exception, msg=" + ex.Message);
            }
        }

        [Test]
        public void TestGetComplexProperty()
        {
            var eventCombined = SupportBeanCombinedProps.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventCombined);

            Assert.AreEqual("0ma0", eventBean.Get("Indexed[0].Mapped('0ma').Value"));
            Assert.AreEqual(typeof(string), eventBean.EventType.GetPropertyType("Indexed[0].Mapped('0ma').Value"));
            Assert.IsNotNull(eventBean.EventType.GetGetter("Indexed[0].Mapped('0ma').Value"));
            Assert.AreEqual("0ma1", eventBean.Get("Indexed[0].Mapped('0mb').Value"));
            Assert.AreEqual("1ma0", eventBean.Get("Indexed[1].Mapped('1ma').Value"));
            Assert.AreEqual("1ma1", eventBean.Get("Indexed[1].Mapped('1mb').Value"));

            Assert.AreEqual("0ma0", eventBean.Get("array[0].Mapped('0ma').Value"));
            Assert.AreEqual("1ma1", eventBean.Get("array[1].Mapped('1mb').Value"));
            Assert.AreEqual("0ma0", eventBean.Get("array[0].mapprop('0ma').Value"));
            Assert.AreEqual("1ma1", eventBean.Get("array[1].mapprop('1mb').Value"));

            TryInvalidGet(eventBean, "dummy");
            TryInvalidGet(eventBean, "dummy[1]");
            TryInvalidGet(eventBean, "dummy('dd')");
            TryInvalidGet(eventBean, "dummy.dummy1");

            // indexed getter
            TryInvalidGetFragment(eventBean, "Indexed");
            Assert.AreEqual(
                typeof(SupportBeanCombinedProps.NestedLevOne),
                ((EventBean) eventBean.GetFragment("Indexed[0]")).EventType.UnderlyingType);
            Assert.AreEqual("abc", ((EventBean) eventBean.GetFragment("array[0]")).Get("nestLevOneVal"));
            Assert.AreEqual("abc", ((EventBean) eventBean.GetFragment("array[2]?")).Get("nestLevOneVal"));
            Assert.IsNull(eventBean.GetFragment("array[3]?"));
            Assert.IsNull(eventBean.GetFragment("array[4]?"));
            Assert.IsNull(eventBean.GetFragment("array[5]?"));

            var eventText = SupportEventTypeAssertionUtil.Print(eventBean);
            //System.out.println(eventText);

            var eventComplex = SupportBeanComplexProps.MakeDefaultBean();
            eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventComplex);
            Assert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("Nested")).Get("NestedValue"));
        }

        [Test]
        public void TestGetIterableListMap()
        {
            var eventComplex = SupportBeanIterableProps.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventComplex);
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean);

            // generic interogation : iterable, List and Map
            Assert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("iterableNested"));
            Assert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("iterableNested[0]"));
            Assert.AreEqual(typeof(IEnumerable<int>), eventBean.EventType.GetPropertyType("iterableInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("iterableInteger[0]"));
            Assert.AreEqual(typeof(IList<object>), eventBean.EventType.GetPropertyType("listNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("listNested[0]"));
            Assert.AreEqual(typeof(IList<int>), eventBean.EventType.GetPropertyType("listInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("listInteger[0]"));
            Assert.AreEqual(typeof(IDictionary<string, object>), eventBean.EventType.GetPropertyType("mapNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("mapNested('a')"));
            Assert.AreEqual(typeof(IDictionary<string, object>), eventBean.EventType.GetPropertyType("mapInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("mapInteger('a')"));
            Assert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("iterableUndefined"));
            Assert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("iterableObject"));
            Assert.AreEqual(typeof(object), eventBean.EventType.GetPropertyType("iterableUndefined[0]"));
            Assert.AreEqual(typeof(object), eventBean.EventType.GetPropertyType("iterableObject[0]"));

            Assert.AreEqual(
                new EventPropertyDescriptor(
                    "iterableNested",
                    typeof(IEnumerable<object>),
                    typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                    false,
                    false,
                    true,
                    false,
                    true),
                eventBean.EventType.GetPropertyDescriptor("iterableNested"));
            Assert.AreEqual(
                new EventPropertyDescriptor("iterableInteger", typeof(IEnumerable<int>), typeof(int), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("iterableInteger"));
            Assert.AreEqual(
                new EventPropertyDescriptor(
                    "listNested",
                    typeof(IList<object>),
                    typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                    false,
                    false,
                    true,
                    false,
                    true),
                eventBean.EventType.GetPropertyDescriptor("listNested"));
            Assert.AreEqual(
                new EventPropertyDescriptor("listInteger", typeof(IList<object>), typeof(int?), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("listInteger"));
            Assert.AreEqual(
                new EventPropertyDescriptor(
                    "mapNested",
                    typeof(IDictionary<string, object>),
                    typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                    false,
                    false,
                    false,
                    true,
                    false),
                eventBean.EventType.GetPropertyDescriptor("mapNested"));
            Assert.AreEqual(
                new EventPropertyDescriptor("mapInteger", typeof(IDictionary<string, object>), typeof(int?), false, false, false, true, false),
                eventBean.EventType.GetPropertyDescriptor("mapInteger"));
            Assert.AreEqual(
                new EventPropertyDescriptor("iterableUndefined", typeof(IEnumerable<object>), typeof(object), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("iterableUndefined"));
            Assert.AreEqual(
                new EventPropertyDescriptor("iterableObject", typeof(IEnumerable<object>), typeof(object), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("iterableObject"));

            AssertNestedCollection(eventBean, "iterableNested", "I");
            AssertNestedCollection(eventBean, "listNested", "L");
            AssertNestedElement(eventBean, "mapNested('a')", "MN1"); // note that property descriptors do not indicate Map-values are fragments
            AssertNestedElement(eventBean, "mapNested('b')", "MN2");
            AssertNestedElement(eventBean, "listNested[0]", "LN1");
            AssertNestedElement(eventBean, "listNested[1]", "LN2");
            AssertNestedElement(eventBean, "iterableNested[0]", "IN1");
            AssertNestedElement(eventBean, "iterableNested[1]", "IN2");

            Assert.IsNull(eventBean.EventType.GetFragmentType("iterableInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("listInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("iterableInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("listInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("mapNested"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("mapInteger"));
        }

        [Test]
        public void TestGetIterableListMapContained()
        {
            var eventIterableContained = SupportBeanIterablePropsContainer.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventIterableContained);

            Assert.AreEqual(
                typeof(IEnumerable<object>),
                eventBean.EventType.GetPropertyType("Contained.iterableNested"));
            Assert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("Contained.iterableNested[0]"));
            Assert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("Contained.iterableInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("Contained.iterableInteger[0]"));
            Assert.AreEqual(typeof(IList<object>), eventBean.EventType.GetPropertyType("Contained.listNested"));
            Assert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("Contained.listNested[0]"));
            Assert.AreEqual(typeof(IList<object>), eventBean.EventType.GetPropertyType("Contained.listInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("Contained.listInteger[0]"));
            Assert.AreEqual(typeof(IDictionary<string, object>), eventBean.EventType.GetPropertyType("Contained.mapNested"));
            Assert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("Contained.mapNested('a')"));
            Assert.AreEqual(typeof(IDictionary<string, object>), eventBean.EventType.GetPropertyType("Contained.mapInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("Contained.mapInteger('a')"));

            AssertNestedElement(
                eventBean,
                "Contained.mapNested('a')",
                "MN1"); // note that property descriptors do not indicate Map-values are fragments
            AssertNestedElement(eventBean, "Contained.mapNested('b')", "MN2");
            AssertNestedElement(eventBean, "Contained.listNested[0]", "LN1");
            AssertNestedElement(eventBean, "Contained.listNested[1]", "LN2");
            AssertNestedElement(eventBean, "Contained.iterableNested[0]", "IN1");
            AssertNestedElement(eventBean, "Contained.iterableNested[1]", "IN2");
            AssertNestedCollection(eventBean, "Contained.iterableNested", "I");
            AssertNestedCollection(eventBean, "Contained.listNested", "L");

            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.iterableInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.listInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.iterableInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.listInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.mapNested"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.mapInteger"));
        }
    }
} // end of namespace
