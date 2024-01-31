///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.support.SupportEventPropUtil;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    [TestFixture]
    public class TestBeanEventBean : AbstractCommonTest
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
            ClassicAssert.AreEqual(true, fragmentTypeOne.IsNative);
            ClassicAssert.AreEqual(false, fragmentTypeOne.IsIndexed);
            ClassicAssert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), fragmentTypeOne.FragmentType.UnderlyingType);

            var theEvent = (EventBean) eventBean.GetFragment(propertyName);
            Assert.That(theEvent, Is.Not.Null);
            Assert.That(theEvent.Get("NestedValue"), Is.EqualTo(value));
        }

        private void AssertNestedCollection(
            EventBean eventBean,
            string propertyName,
            string prefix)
        {
            var fragmentTypeTwo = eventBean.EventType.GetFragmentType(propertyName);
            ClassicAssert.AreEqual(true, fragmentTypeTwo.IsNative);
            ClassicAssert.AreEqual(true, fragmentTypeTwo.IsIndexed);
            ClassicAssert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), fragmentTypeTwo.FragmentType.UnderlyingType);

            var events = (EventBean[]) eventBean.GetFragment(propertyName);
            ClassicAssert.AreEqual(2, events.Length);
            ClassicAssert.AreEqual(prefix + "N1", events[0].Get("NestedValue"));
            ClassicAssert.AreEqual(prefix + "N2", events[1].Get("NestedValue"));
        }

        private static void TryInvalidGet(
            EventBean eventBean,
            string propName)
        {
            Assert.That(() => eventBean.Get(propName), Throws.InstanceOf<PropertyAccessException>());
            ClassicAssert.IsNull(eventBean.EventType.GetPropertyType(propName));
            ClassicAssert.IsNull(eventBean.EventType.GetGetter(propName));
        }

        private static void TryInvalidGetFragment(
            EventBean eventBean,
            string propName)
        {
            Assert.That(() => eventBean.GetFragment(propName), Throws.InstanceOf<PropertyAccessException>());
        }

        [Test]
        public void TestGet()
        {
            EventType eventType = supportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var eventBean = new BeanEventBean(testEvent, eventType);

            ClassicAssert.AreEqual(eventType, eventBean.EventType);
            ClassicAssert.AreEqual(testEvent, eventBean.Underlying);

            ClassicAssert.AreEqual(10, eventBean.Get("IntPrimitive"));

            // Test wrong property name
            Assert.Throws<PropertyAccessException>(() => {
                eventBean.Get("dummy");
                ClassicAssert.IsTrue(false);
            });

            // Test wrong event type - not possible to happen under normal use
            Assert.Throws<PropertyAccessException>(() => {
                eventType = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
                eventBean = new BeanEventBean(testEvent, eventType);
                eventBean.Get("MyString");
                ClassicAssert.IsTrue(false);
            });
        }

        [Test]
        public void TestGetComplexProperty()
        {
            var eventCombined = SupportBeanCombinedProps.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventCombined);

            ClassicAssert.AreEqual("0ma0", eventBean.Get("Indexed[0].Mapped('0ma').Value"));
            ClassicAssert.AreEqual(typeof(string), eventBean.EventType.GetPropertyType("Indexed[0].Mapped('0ma').Value"));
            ClassicAssert.IsNotNull(eventBean.EventType.GetGetter("Indexed[0].Mapped('0ma').Value"));
            ClassicAssert.AreEqual("0ma1", eventBean.Get("Indexed[0].Mapped('0mb').Value"));
            ClassicAssert.AreEqual("1ma0", eventBean.Get("Indexed[1].Mapped('1ma').Value"));
            ClassicAssert.AreEqual("1ma1", eventBean.Get("Indexed[1].Mapped('1mb').Value"));

            ClassicAssert.AreEqual("0ma0", eventBean.Get("Array[0].Mapped('0ma').Value"));
            ClassicAssert.AreEqual("1ma1", eventBean.Get("Array[1].Mapped('1mb').Value"));
            ClassicAssert.AreEqual("0ma0", eventBean.Get("Array[0].Mapprop('0ma').Value"));
            ClassicAssert.AreEqual("1ma1", eventBean.Get("Array[1].Mapprop('1mb').Value"));

            TryInvalidGet(eventBean, "dummy");
            TryInvalidGet(eventBean, "dummy[1]");
            TryInvalidGet(eventBean, "dummy('dd')");
            TryInvalidGet(eventBean, "dummy.dummy1");

            // indexed getter
            TryInvalidGetFragment(eventBean, "Indexed");
            
            Assert.That(eventBean.GetFragment("Indexed[0]").AsEventBean(), Is.Not.Null);
            Assert.That(eventBean.GetFragment("Indexed[0]").AsEventBean().EventType, Is.Not.Null);
            Assert.That(eventBean.GetFragment("Indexed[0]").AsEventBean().EventType.UnderlyingType,
                Is.EqualTo(typeof(SupportBeanCombinedProps.NestedLevOne)));
            
            Assert.That(eventBean.GetFragment("Array[0]"), Is.Not.Null);
            Assert.That(eventBean.GetFragment("Array[0]").AsEventBean(), Is.Not.Null);
            Assert.That(eventBean.GetFragment("Array[0]").AsEventBean().Get("NestLevOneVal"), Is.EqualTo("abc"));

            Assert.That(eventBean.GetFragment("Array[2]?"), Is.Not.Null);
            Assert.That(eventBean.GetFragment("Array[2]?").AsEventBean(), Is.Not.Null);
            Assert.That(eventBean.GetFragment("Array[2]?").AsEventBean().Get("NestLevOneVal"), Is.EqualTo("abc"));

            //ClassicAssert.AreEqual("abc", ((EventBean) eventBean.GetFragment("Array[0]")).Get("NestLevOneVal"));
            //ClassicAssert.AreEqual("abc", eventBean.GetFragment("Array[2]?").AsEventBean().Get("NestLevOneVal"));
            
            ClassicAssert.IsNull(eventBean.GetFragment("Array[3]?"));
            ClassicAssert.IsNull(eventBean.GetFragment("Array[4]?"));
            ClassicAssert.IsNull(eventBean.GetFragment("Array[5]?"));

            var eventText = SupportEventTypeAssertionUtil.Print(eventBean);
            //Console.WriteLine(eventText);

            var eventComplex = SupportBeanComplexProps.MakeDefaultBean();
            eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventComplex);
            ClassicAssert.AreEqual("NestedValue", ((EventBean) eventBean.GetFragment("Nested")).Get("NestedValue"));
        }

        [Test]
        public void TestGetIterableListMap()
        {
            var eventComplex = SupportBeanIterableProps.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventComplex);
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean);

            // generic interrogation : Iterable, List and Map
            ClassicAssert.AreEqual(
                eventBean.EventType.GetPropertyType("IterableNested"),
                typeof(IEnumerable<SupportBeanIterableProps.SupportBeanSpecialGetterNested>));
            ClassicAssert.AreEqual(
                eventBean.EventType.GetPropertyType("IterableNested[0]"),
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested));
            ClassicAssert.AreEqual(typeof(IEnumerable<int>), eventBean.EventType.GetPropertyType("IterableInteger"));
            ClassicAssert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("IterableInteger[0]"));
            ClassicAssert.AreEqual(
                typeof(IList<SupportBeanIterableProps.SupportBeanSpecialGetterNested>),
                eventBean.EventType.GetPropertyType("ListNested"));
            ClassicAssert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("ListNested[0]"));
            ClassicAssert.AreEqual(typeof(IList<int>), eventBean.EventType.GetPropertyType("ListInteger"));
            ClassicAssert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("ListInteger[0]"));
            ClassicAssert.AreEqual(
                typeof(IDictionary<string, SupportBeanIterableProps.SupportBeanSpecialGetterNested>),
                eventBean.EventType.GetPropertyType("MapNested"));
            ClassicAssert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("MapNested('a')"));
            ClassicAssert.AreEqual(typeof(IDictionary<string, int>), eventBean.EventType.GetPropertyType("MapInteger"));
            ClassicAssert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("MapInteger('a')"));
            ClassicAssert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("IterableUndefined"));
            ClassicAssert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("IterableObject"));
            ClassicAssert.AreEqual(typeof(object), eventBean.EventType.GetPropertyType("IterableUndefined[0]"));
            ClassicAssert.AreEqual(typeof(object), eventBean.EventType.GetPropertyType("IterableObject[0]"));

            AssertPropEquals(
                new SupportEventPropDesc(
                        "IterableNested",
                        typeof(IEnumerable<SupportBeanIterableProps.SupportBeanSpecialGetterNested>))
                    .WithComponentType<SupportBeanIterableProps.SupportBeanSpecialGetterNested>()
                    .WithIndexed()
                    .WithFragment(),
                eventBean.EventType.GetPropertyDescriptor("IterableNested"));

            AssertPropEquals(
                new SupportEventPropDesc(
                        "IterableInteger",
                        typeof(IEnumerable<int>)).WithComponentType<int>()
                    .WithIndexed(),
                eventBean.EventType.GetPropertyDescriptor("IterableInteger"));
            AssertPropEquals(new SupportEventPropDesc("ListNested",
                        typeof(IList<SupportBeanIterableProps.SupportBeanSpecialGetterNested>))
                    .WithComponentType<SupportBeanIterableProps.SupportBeanSpecialGetterNested>()
                    .WithIndexed()
                    .WithFragment(),
                eventBean.EventType.GetPropertyDescriptor("ListNested"));
            AssertPropEquals(
                new SupportEventPropDesc("ListInteger", typeof(IList<int>))
                    .WithComponentType<int>()
                    .WithIndexed(),
                eventBean.EventType.GetPropertyDescriptor("ListInteger"));
            AssertPropEquals(
                new SupportEventPropDesc(
                        "MapNested",
                        typeof(IDictionary<string, SupportBeanIterableProps.SupportBeanSpecialGetterNested>))
                    .WithComponentType<SupportBeanIterableProps.SupportBeanSpecialGetterNested>()
                    .WithMapped(),
                eventBean.EventType.GetPropertyDescriptor("MapNested"));
            AssertPropEquals(
                new SupportEventPropDesc("MapInteger", typeof(IDictionary<string, int>))
                    .WithComponentType<int>()
                    .WithMapped(),
                eventBean.EventType.GetPropertyDescriptor("MapInteger"));
            AssertPropEquals(
                new SupportEventPropDesc("IterableUndefined", typeof(IEnumerable<object>))
                    .WithIndexed(),
                eventBean.EventType.GetPropertyDescriptor("IterableUndefined"));
            AssertPropEquals(
                new SupportEventPropDesc("IterableObject", typeof(IEnumerable<object>))
                    .WithComponentType<object>()
                    .WithIndexed(),
                eventBean.EventType.GetPropertyDescriptor("IterableObject"));

            AssertNestedCollection(eventBean, "IterableNested", "I");
            AssertNestedCollection(eventBean, "ListNested", "L");
            AssertNestedElement(
                eventBean,
                "MapNested('a')",
                "MN1"); // note that property descriptors do not indicate Map-values are fragments
            AssertNestedElement(eventBean, "MapNested('b')", "MN2");
            AssertNestedElement(eventBean, "ListNested[0]", "LN1");
            AssertNestedElement(eventBean, "ListNested[1]", "LN2");
            AssertNestedElement(eventBean, "IterableNested[0]", "IN1");
            AssertNestedElement(eventBean, "IterableNested[1]", "IN2");

            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("IterableInteger"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("ListInteger"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("IterableInteger[0]"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("ListInteger[0]"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("MapNested"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("MapInteger"));
        }

        [Test]
        public void TestGetIterableListMapContained()
        {
            var eventIterableContained = SupportBeanIterablePropsContainer.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, eventIterableContained);

            ClassicAssert.AreEqual(
                typeof(IEnumerable<SupportBeanIterableProps.SupportBeanSpecialGetterNested>),
                eventBean.EventType.GetPropertyType("Contained.IterableNested"));
            ClassicAssert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("Contained.IterableNested[0]"));
            ClassicAssert.AreEqual(typeof(IEnumerable<int>), eventBean.EventType.GetPropertyType("Contained.IterableInteger"));
            ClassicAssert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("Contained.IterableInteger[0]"));
            ClassicAssert.AreEqual(typeof(IList<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("Contained.ListNested"));
            ClassicAssert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("Contained.ListNested[0]"));
            ClassicAssert.AreEqual(typeof(IList<int>), eventBean.EventType.GetPropertyType("Contained.ListInteger"));
            ClassicAssert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("Contained.ListInteger[0]"));
            ClassicAssert.AreEqual(typeof(IDictionary<string, SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("Contained.MapNested"));
            ClassicAssert.AreEqual(
                typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested),
                eventBean.EventType.GetPropertyType("Contained.MapNested('a')"));
            ClassicAssert.AreEqual(typeof(IDictionary<string, int>), eventBean.EventType.GetPropertyType("Contained.MapInteger"));
            ClassicAssert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("Contained.MapInteger('a')"));

            AssertNestedElement(
                eventBean,
                "Contained.MapNested('a')",
                "MN1"); // note that property descriptors do not indicate Map-values are fragments
            AssertNestedElement(eventBean, "Contained.MapNested('b')", "MN2");

            AssertNestedElement(eventBean, "Contained.ListNested[0]", "LN1");
            AssertNestedElement(eventBean, "Contained.ListNested[1]", "LN2");
            AssertNestedElement(eventBean, "Contained.IterableNested[0]", "IN1");
            AssertNestedElement(eventBean, "Contained.IterableNested[1]", "IN2");
            AssertNestedCollection(eventBean, "Contained.IterableNested", "I");
            AssertNestedCollection(eventBean, "Contained.ListNested", "L");

            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("Contained.IterableInteger"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("Contained.ListInteger"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("Contained.IterableInteger[0]"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("Contained.ListInteger[0]"));

            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("Contained.MapNested"));
            ClassicAssert.IsNull(eventBean.EventType.GetFragmentType("Contained.MapInteger"));
        }
    }
} // end of namespace