///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    [TestFixture]
    public class TestBeanEventType : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            eventTypeSimple = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
            eventTypeComplex = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanComplexProps));
            eventTypeNested = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanCombinedProps));

            objSimple = new SupportBeanSimple("a", 20);
            objComplex = SupportBeanComplexProps.MakeDefaultBean();
            objCombined = SupportBeanCombinedProps.MakeDefaultBean();

            eventSimple = new BeanEventBean(objSimple, eventTypeSimple);
            eventComplex = new BeanEventBean(objComplex, eventTypeComplex);
            eventNested = new BeanEventBean(objCombined, eventTypeNested);
        }

        private BeanEventType eventTypeSimple;
        private BeanEventType eventTypeComplex;
        private BeanEventType eventTypeNested;

        private EventBean eventSimple;
        private EventBean eventComplex;
        private EventBean eventNested;

        private SupportBeanSimple objSimple;
        private SupportBeanComplexProps objComplex;
        private SupportBeanCombinedProps objCombined;

        private static void TryInvalidIsProperty(
            BeanEventType type,
            string property)
        {
            Assert.IsNull(type.GetPropertyType(property));
        }

        private static void RunTest(
            BeanEventType eventType,
            EventBean eventBean,
            params PropTestDesc[] tests)
        {
            foreach (var desc in tests)
            {
                RunTest(desc, eventType, eventBean);
            }
        }

        private static void RunTest(
            BeanEventType eventType,
            EventBean eventBean,
            IList<PropTestDesc> tests)
        {
            foreach (var desc in tests)
            {
                RunTest(desc, eventType, eventBean);
            }
        }

        private static void RunTest(
            PropTestDesc test,
            BeanEventType eventType,
            EventBean eventBean)
        {
            var propertyName = test.PropertyName;

            Assert.AreEqual(
                test.IsProperty,
                eventType.IsProperty(propertyName),
                "IsProperty mismatch on '" + propertyName + "',");
            Assert.AreEqual(
                test.Clazz,
                eventType.GetPropertyType(propertyName),
                "GetPropertyType mismatch on '" + propertyName + "',");

            var getter = eventType.GetGetter(propertyName);
            if (getter == null)
            {
                Assert.IsFalse(test.IsHasGetter, "getGetter null on '" + propertyName + "',");
            }
            else
            {
                Assert.IsTrue(test.IsHasGetter, "getGetter not null on '" + propertyName + "',");
                if (ReferenceEquals(test.GetterReturnValue, typeof(NullReferenceException)))
                {
                    try
                    {
                        getter.Get(eventBean);
                        Assert.Fail("getGetter not throwing null pointer on '" + propertyName);
                    }
                    catch (NullReferenceException)
                    {
                        // expected
                    }
                }
                else
                {
                    var value = getter.Get(eventBean);
                    Assert.AreEqual(
                        test.GetterReturnValue,
                        value,
                        "getter value mismatch on '" + propertyName + "',");
                }
            }
        }

        public class PropTestDesc
        {
            public PropTestDesc(
                string propertyName,
                bool property,
                Type clazz,
                bool hasGetter,
                object getterReturnValue)
            {
                PropertyName = propertyName;
                IsProperty = property;
                Clazz = clazz;
                IsHasGetter = hasGetter;
                GetterReturnValue = getterReturnValue;
            }

            public string PropertyName { get; }

            public bool IsProperty { get; }

            public Type Clazz { get; }

            public bool IsHasGetter { get; }

            public object GetterReturnValue { get; }
        }

        [Test]
        public void TestFragments()
        {
            var nestedTypeFragment = eventTypeComplex.GetFragmentType("Nested");
            var nestedType = nestedTypeFragment.FragmentType;
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested).FullName, nestedType.Name);
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), nestedType.UnderlyingType);
            Assert.AreEqual(typeof(string), nestedType.GetPropertyType("NestedValue"));
            Assert.IsNull(eventTypeComplex.GetFragmentType("Indexed[0]"));

            nestedTypeFragment = eventTypeNested.GetFragmentType("Indexed[0]");
            nestedType = nestedTypeFragment.FragmentType;
            Assert.IsFalse(nestedTypeFragment.IsIndexed);
            Assert.AreEqual(typeof(SupportBeanCombinedProps.NestedLevOne).FullName, nestedType.Name);
            Assert.AreEqual(typeof(IDictionary<string, SupportBeanCombinedProps.NestedLevTwo>), nestedType.GetPropertyType("Mapprop"));

            SupportEventTypeAssertionUtil.AssertConsistency(eventTypeComplex);
            SupportEventTypeAssertionUtil.AssertConsistency(eventTypeNested);
        }

        [Test]
        public void TestGetGetter()
        {
            Assert.AreEqual(null, eventTypeSimple.GetGetter("dummy"));

            var getter = eventTypeSimple.GetGetter("MyInt");
            Assert.AreEqual(20, getter.Get(eventSimple));
            getter = eventTypeSimple.GetGetter("MyString");
            Assert.AreEqual("a", getter.Get(eventSimple));
        }

        [Test]
        public void TestGetPropertyNames()
        {
            var properties = eventTypeSimple.PropertyNames;
            Assert.IsTrue(properties.Length == 2);
            CollectionAssert.Contains(properties, "MyInt");
            CollectionAssert.Contains(properties, "MyString");

            CollectionAssert.Contains(
                eventTypeSimple.PropertyDescriptors,
                new EventPropertyDescriptor("MyInt", typeof(int), null, false, false, false, false, false));

            CollectionAssert.Contains(
                eventTypeSimple.PropertyDescriptors,
                new EventPropertyDescriptor("MyString", typeof(string), typeof(char), false, false, true, false, false));

            properties = eventTypeComplex.PropertyNames;

            CollectionAssert.AreEquivalent(SupportBeanComplexProps.PROPERTIES, properties);
            CollectionAssert.AreEquivalent(
                new[] {
                    new EventPropertyDescriptor(
                        "SimpleProperty",
                        typeof(string),
                        typeof(char),
                        false,
                        false,
                        true,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "MapProperty",
                        typeof(IDictionary<string, string>),
                        typeof(string),
                        false,
                        false,
                        false,
                        true,
                        false),
                    new EventPropertyDescriptor(
                        "MappedProps",
                        typeof(Properties),
                        typeof(string),
                        false,
                        false,
                        false,
                        true,
                        false),
                    new EventPropertyDescriptor(
                        "Mapped",
                        typeof(string),
                        typeof(string),
                        false,
                        true,
                        false,
                        true,
                        false),
                    new EventPropertyDescriptor(
                        "Indexed", 
                        typeof(int), 
                        null,
                        true,
                        false,
                        true,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "IndexedProps",
                        typeof(int[]),
                        typeof(int),
                        false,
                        false,
                        true,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "Nested",
                        typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested),
                        null,
                        false,
                        false,
                        false,
                        false,
                        true),
                    new EventPropertyDescriptor(
                        "ArrayProperty",
                        typeof(int[]),
                        typeof(int),
                        false,
                        false,
                        true,
                        false,
                        false),
                    new EventPropertyDescriptor(
                        "ObjectArray",
                        typeof(object[]),
                        typeof(object),
                        false,
                        false,
                        true,
                        false,
                        false)
                },
                eventTypeComplex.PropertyDescriptors);

            properties = eventTypeNested.PropertyNames;
            CollectionAssert.AreEquivalent(SupportBeanCombinedProps.PROPERTIES, properties);
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string), eventTypeSimple.GetPropertyType("MyString"));
            Assert.IsNull(eventTypeSimple.GetPropertyType("dummy"));
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(SupportBeanSimple), eventTypeSimple.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(eventTypeSimple.IsProperty("MyString"));
            Assert.IsFalse(eventTypeSimple.IsProperty("dummy"));
        }

        [Test]
        public void TestProperties()
        {
            var nestedOne = typeof(SupportBeanCombinedProps.NestedLevOne);
            var nestedOneArr = typeof(SupportBeanCombinedProps.NestedLevOne[]);
            var nestedTwo = typeof(SupportBeanCombinedProps.NestedLevTwo);

            RunTest(
                eventTypeComplex,
                eventComplex,
                new PropTestDesc("SimpleProperty", true, typeof(string), true, "simple"),
                new PropTestDesc("dummy", false, null, false, null),
                new PropTestDesc("Indexed", false, null, false, null),
                new PropTestDesc("Indexed[1]", true, typeof(int), true, 2),
                new PropTestDesc(
                    "Nested",
                    true,
                    typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested),
                    true,
                    objComplex.Nested),
                new PropTestDesc("Nested.NestedValue", true, typeof(string), true, objComplex.Nested.NestedValue),
                new PropTestDesc(
                    "Nested.NestedNested",
                    true,
                    typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNestedNested),
                    true,
                    objComplex.Nested.NestedNested),
                new PropTestDesc(
                    "Nested.NestedNested.NestedNestedValue",
                    true,
                    typeof(string),
                    true,
                    objComplex.Nested.NestedNested.NestedNestedValue),
                new PropTestDesc("Nested.dummy", false, null, false, null));

            RunTest(
                eventTypeComplex,
                eventComplex,
                new PropTestDesc("Mapped", false, null, false, null),
                new PropTestDesc("Mapped('keyOne')", true, typeof(string), true, "valueOne"),
                new PropTestDesc("ArrayProperty", true, typeof(int[]), true, objComplex.ArrayProperty),
                new PropTestDesc("ArrayProperty[1]", true, typeof(int), true, 20));

            RunTest(
                eventTypeComplex,
                eventComplex,
                new PropTestDesc("MapProperty('xOne')", true, typeof(string), true, "yOne"));

            RunTest(
                eventTypeComplex,
                eventComplex,
                new PropTestDesc("google('x')", false, null, false, null),
                new PropTestDesc("Mapped('x')", true, typeof(string), true, null),
                new PropTestDesc("Mapped('x').x", false, null, false, null),
                new PropTestDesc(
                    "MapProperty",
                    true,
                    typeof(IDictionary<string, string>),
                    true,
                    objComplex.MapProperty));

            RunTest(
                eventTypeSimple,
                eventSimple,
                new PropTestDesc("dummy", false, null, false, null),
                new PropTestDesc("MyInt", true, typeof(int), true, objSimple.MyInt),
                new PropTestDesc("MyString", true, typeof(string), true, objSimple.MyString),
                new PropTestDesc("dummy('a')", false, null, false, null),
                new PropTestDesc("dummy[1]", false, null, false, null),
                new PropTestDesc("dummy.Nested", false, null, false, null));

            RunTest(
                eventTypeNested,
                eventNested,
                new PropTestDesc("Indexed", false, null, false, null),
                new PropTestDesc("Indexed[1]", true, nestedOne, true, objCombined.GetIndexed(1)),
                new PropTestDesc("Indexed.Mapped", false, null, false, null),
                new PropTestDesc("Indexed[1].Mapped", false, null, false, null),
                new PropTestDesc("Array", true, nestedOneArr, true, objCombined.Array),
                new PropTestDesc("Array.Mapped", false, null, false, null),
                new PropTestDesc("Array[0]", true, nestedOne, true, objCombined.Array[0]),
                new PropTestDesc("Array[1].Mapped", false, null, false, null),
                new PropTestDesc("Array[1].Mapped('x')", true, nestedTwo, true, objCombined.Array[1].GetMapped("x")),
                new PropTestDesc(
                    "Array[1].Mapped('1mb')",
                    true,
                    nestedTwo,
                    true,
                    objCombined.Array[1].GetMapped("1mb")),
                new PropTestDesc(
                    "Indexed[1].Mapped('x')",
                    true,
                    nestedTwo,
                    true,
                    objCombined.GetIndexed(1).GetMapped("x")),
                new PropTestDesc("Indexed[1].Mapped('x').Value", true, typeof(string), true, null),
                new PropTestDesc(
                    "Indexed[1].Mapped('1mb')",
                    true,
                    nestedTwo,
                    true,
                    objCombined.GetIndexed(1).GetMapped("1mb")),
                new PropTestDesc(
                    "Indexed[1].Mapped('1mb').Value",
                    true,
                    typeof(string),
                    true,
                    objCombined.GetIndexed(1).GetMapped("1mb").Value),
                new PropTestDesc(
                    "Array[1].Mapprop",
                    true,
                    typeof(IDictionary<string, SupportBeanCombinedProps.NestedLevTwo>),
                    true,
                    objCombined.GetIndexed(1).GetMapprop()));

            RunTest(
                eventTypeNested,
                eventNested,
                new PropTestDesc(
                    "Array[1].Mapprop('1ma')",
                    true,
                    typeof(SupportBeanCombinedProps.NestedLevTwo),
                    true,
                    objCombined.Array[1].GetMapped("1ma")),
                new PropTestDesc(
                    "Array[1].Mapprop('1ma').Value",
                    true,
                    typeof(string),
                    true,
                    "1ma0"),
                new PropTestDesc(
                    "Indexed[1].Mapprop",
                    true,
                    typeof(IDictionary<string, SupportBeanCombinedProps.NestedLevTwo>),
                    true,
                    objCombined.GetIndexed(1).GetMapprop()));

            TryInvalidIsProperty(eventTypeComplex, "x[");
            TryInvalidIsProperty(eventTypeComplex, "dummy()");
            TryInvalidIsProperty(eventTypeComplex, "Nested.xx['a']");
            TryInvalidIsProperty(eventTypeNested, "dummy[(");
            TryInvalidIsProperty(eventTypeNested, "Array[1].Mapprop[x].Value");
        }
    }
} // end of namespace
