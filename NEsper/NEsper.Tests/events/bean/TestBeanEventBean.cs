///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestBeanEventBean 
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            //SupportEventAdapterService.Reset(
            //    _container.LockManager(),
            //    _container.ClassLoaderProvider());
        }
    
        [Test]
        public void TestGet()
        {
            var testEvent = new SupportBean();
            testEvent.IntPrimitive = 10;

            var eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var eventBean = new BeanEventBean(testEvent, eventType);
    
            Assert.AreEqual(eventType, eventBean.EventType);
            Assert.AreEqual(testEvent, eventBean.Underlying);
    
            Assert.AreEqual(10, eventBean.Get("IntPrimitive"));
    
            // Test wrong property name
            try
            {
                eventBean.Get("dummy");
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex)
            {
                // Expected
                Log.Debug(".TestGet Expected exception, msg=" + ex.Message);
            }
    
            // Test wrong event type - not possible to happen under normal use
            try
            {
                eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
                eventBean = new BeanEventBean(testEvent, eventType);
                eventBean.Get("MyString");
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex)
            {
                // Expected
                Log.Debug(".TestGet Expected exception, msg=" + ex.Message);
            }

            var getter = eventType.GetGetter("MyString");
            try
            {
                getter.Get(new BeanEventBean(new SupportBean_A("id"), eventType));
                Assert.Fail();
            }
            catch (PropertyAccessException ex)
            {
                Assert.AreEqual("Mismatched getter instance to event bean type, expected com.espertech.esper.supportunit.bean.SupportBeanSimple but received com.espertech.esper.supportunit.bean.SupportBean_A", ex.Message);
            }
        }

        [Test]
        public void TestGetComplexProperty()
        {
            var eventCombined = SupportBeanCombinedProps.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(eventCombined);
    
            Assert.AreEqual("0ma0", eventBean.Get("Indexed[0].Mapped('0ma').Value"));
            Assert.AreEqual(typeof(string), eventBean.EventType.GetPropertyType("Indexed[0].Mapped('0ma').Value"));
            Assert.IsNotNull(eventBean.EventType.GetGetter("Indexed[0].Mapped('0ma').Value"));
            Assert.AreEqual("0ma1", eventBean.Get("Indexed[0].Mapped('0mb').Value"));
            Assert.AreEqual("1ma0", eventBean.Get("Indexed[1].Mapped('1ma').Value"));
            Assert.AreEqual("1ma1", eventBean.Get("Indexed[1].Mapped('1mb').Value"));
    
            Assert.AreEqual("0ma0", eventBean.Get("Array[0].Mapped('0ma').Value"));
            Assert.AreEqual("1ma1", eventBean.Get("Array[1].Mapped('1mb').Value"));
            Assert.AreEqual("0ma0", eventBean.Get("Array[0].Mapprop('0ma').Value"));
            Assert.AreEqual("1ma1", eventBean.Get("Array[1].Mapprop('1mb').Value"));
    
            TryInvalidGet(eventBean, "Dummy");
            TryInvalidGet(eventBean, "Dummy[1]");
            TryInvalidGet(eventBean, "Dummy('dd')");
            TryInvalidGet(eventBean, "Dummy.Dummy1");
    
            // indexed getter
            TryInvalidGetFragment(eventBean, "Indexed");
            Assert.AreEqual(typeof(SupportBeanCombinedProps.NestedLevOne), ((EventBean) eventBean.GetFragment("Indexed[0]")).EventType.UnderlyingType);
            Assert.AreEqual("abc", ((EventBean)eventBean.GetFragment("Array[0]")).Get("NestLevOneVal"));
            Assert.AreEqual("abc", ((EventBean)eventBean.GetFragment("Array[2]?")).Get("NestLevOneVal"));
            Assert.IsNull(eventBean.GetFragment("Array[3]?"));
            Assert.IsNull(eventBean.GetFragment("Array[4]?"));
            Assert.IsNull(eventBean.GetFragment("Array[5]?"));

            var eventText = SupportEventTypeAssertionUtil.Print(eventBean);
            //Console.Out.WriteLine(eventText);
    
            var eventComplex = SupportBeanComplexProps.MakeDefaultBean();
            eventBean = SupportEventBeanFactory.CreateObject(eventComplex);
            Assert.AreEqual("NestedValue", ((EventBean)eventBean.GetFragment("Nested")).Get("NestedValue"));
        }
    
        [Test]
        public void TestGetIterableListMap()
        {
            var eventComplex = SupportBeanIterableProps.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(eventComplex);
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean);
    
            // generic interogation : iterable, List and Map
            Assert.AreEqual(typeof(IEnumerable<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("IterableNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("IterableNested[0]"));
            Assert.AreEqual(typeof(IEnumerable<int?>), eventBean.EventType.GetPropertyType("IterableInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("IterableInteger[0]"));
            Assert.AreEqual(typeof(IList<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("ListNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("ListNested[0]"));
            Assert.AreEqual(typeof(IList<int?>), eventBean.EventType.GetPropertyType("ListInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("ListInteger[0]"));
            Assert.AreEqual(typeof(IDictionary<string, SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("MapNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("MapNested('a')"));
            Assert.AreEqual(typeof(IDictionary<string, int>), eventBean.EventType.GetPropertyType("MapInteger"));
            Assert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("MapInteger('a')"));
            Assert.AreEqual(typeof(IEnumerable), eventBean.EventType.GetPropertyType("IterableUndefined"));
            Assert.AreEqual(typeof(IEnumerable<object>), eventBean.EventType.GetPropertyType("IterableObject"));
            Assert.AreEqual(typeof(Object), eventBean.EventType.GetPropertyType("IterableUndefined[0]"));
            Assert.AreEqual(typeof(Object), eventBean.EventType.GetPropertyType("IterableObject[0]"));

            Assert.AreEqual(new EventPropertyDescriptor("IterableNested", typeof(IEnumerable<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), false, false, true, false, true),
                eventBean.EventType.GetPropertyDescriptor("IterableNested"));
            Assert.AreEqual(new EventPropertyDescriptor("IterableInteger", typeof(IEnumerable<int?>), typeof(int?), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("IterableInteger"));
            Assert.AreEqual(new EventPropertyDescriptor("ListNested", typeof(IList<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), false, false, true, false, true),
                eventBean.EventType.GetPropertyDescriptor("ListNested"));
            Assert.AreEqual(new EventPropertyDescriptor("ListInteger", typeof(IList<int?>), typeof(int?), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("ListInteger"));
            Assert.AreEqual(new EventPropertyDescriptor("MapNested", typeof(IDictionary<string, SupportBeanIterableProps.SupportBeanSpecialGetterNested>), typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), false, false, false, true, false),
                eventBean.EventType.GetPropertyDescriptor("MapNested"));
            Assert.AreEqual(new EventPropertyDescriptor("MapInteger", typeof(IDictionary<string, int>), typeof(int), false, false, false, true, false),
                eventBean.EventType.GetPropertyDescriptor("MapInteger"));
            Assert.AreEqual(new EventPropertyDescriptor("IterableUndefined", typeof(IEnumerable), typeof(object), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("IterableUndefined"));
            Assert.AreEqual(new EventPropertyDescriptor("IterableObject", typeof(IEnumerable<object>), typeof(object), false, false, true, false, false),
                eventBean.EventType.GetPropertyDescriptor("IterableObject"));
    
            AssertNestedCollection(eventBean, "IterableNested", "I");
            AssertNestedCollection(eventBean, "ListNested", "L");
            AssertNestedElement(eventBean, "MapNested('a')", "MN1");    // note that property descriptors do not indicate Map-values are fragments
            AssertNestedElement(eventBean, "MapNested('b')", "MN2");
            AssertNestedElement(eventBean, "ListNested[0]", "LN1");
            AssertNestedElement(eventBean, "ListNested[1]", "LN2");
            AssertNestedElement(eventBean, "IterableNested[0]", "IN1");
            AssertNestedElement(eventBean, "IterableNested[1]", "IN2");
    
            Assert.IsNull(eventBean.EventType.GetFragmentType("IterableInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("ListInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("IterableInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("ListInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("MapNested"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("MapInteger"));
        }
    
        [Test]
        public void TestGetIterableListMapContained()
        {
            var eventIterableContained = SupportBeanIterablePropsContainer.MakeDefaultBean();
            var eventBean = SupportEventBeanFactory.CreateObject(eventIterableContained);

            Assert.AreEqual(typeof(IEnumerable<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("Contained.IterableNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("Contained.IterableNested[0]"));
            Assert.AreEqual(typeof(IEnumerable<int?>), eventBean.EventType.GetPropertyType("Contained.IterableInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("Contained.IterableInteger[0]"));
            Assert.AreEqual(typeof(IList<SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("Contained.ListNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("Contained.ListNested[0]"));
            Assert.AreEqual(typeof(IList<int?>), eventBean.EventType.GetPropertyType("Contained.ListInteger"));
            Assert.AreEqual(typeof(int?), eventBean.EventType.GetPropertyType("Contained.ListInteger[0]"));
            Assert.AreEqual(typeof(IDictionary<string, SupportBeanIterableProps.SupportBeanSpecialGetterNested>), eventBean.EventType.GetPropertyType("Contained.MapNested"));
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), eventBean.EventType.GetPropertyType("Contained.MapNested('a')"));
            Assert.AreEqual(typeof(IDictionary<string, int>), eventBean.EventType.GetPropertyType("Contained.MapInteger"));
            Assert.AreEqual(typeof(int), eventBean.EventType.GetPropertyType("Contained.MapInteger('a')"));
    
            AssertNestedElement(eventBean, "Contained.MapNested('a')", "MN1");    // note that property descriptors do not indicate Map-values are fragments
            AssertNestedElement(eventBean, "Contained.MapNested('b')", "MN2");
            AssertNestedElement(eventBean, "Contained.ListNested[0]", "LN1");
            AssertNestedElement(eventBean, "Contained.ListNested[1]", "LN2");
            AssertNestedElement(eventBean, "Contained.IterableNested[0]", "IN1");
            AssertNestedElement(eventBean, "Contained.IterableNested[1]", "IN2");
            AssertNestedCollection(eventBean, "Contained.IterableNested", "I");
            AssertNestedCollection(eventBean, "Contained.ListNested", "L");
    
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.IterableInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.ListInteger"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.IterableInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.ListInteger[0]"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.MapNested"));
            Assert.IsNull(eventBean.EventType.GetFragmentType("Contained.MapInteger"));
        }
    
        private void AssertNestedElement(EventBean eventBean, String propertyName, String value)
        {
            var fragmentTypeOne = eventBean.EventType.GetFragmentType(propertyName);
            Assert.AreEqual(true, fragmentTypeOne.IsNative);
            Assert.AreEqual(false, fragmentTypeOne.IsIndexed);
            Assert.AreEqual(typeof(SupportBeanIterableProps.SupportBeanSpecialGetterNested), fragmentTypeOne.FragmentType.UnderlyingType);
    
            var theEvent = (EventBean) eventBean.GetFragment(propertyName);
            Assert.AreEqual(value, theEvent.Get("NestedValue"));
        }
    
        private void AssertNestedCollection(EventBean eventBean, String propertyName, String prefix)
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
    
        private static void TryInvalidGet(EventBean eventBean, String propName)
        {
            try
            {
                eventBean.Get(propName);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // expected
            }
    
            Assert.IsNull(eventBean.EventType.GetPropertyType(propName));
            Assert.IsNull(eventBean.EventType.GetGetter(propName));
        }
    
        private static void TryInvalidGetFragment(EventBean eventBean, String propName)
        {
            try
            {
                eventBean.GetFragment(propName);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // expected
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
