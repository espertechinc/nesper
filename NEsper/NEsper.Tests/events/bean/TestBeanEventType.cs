///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestBeanEventType
    {
        private BeanEventType _eventTypeSimple;
        private BeanEventType _eventTypeComplex;
        private BeanEventType _eventTypeNested;

        private EventBean _eventSimple;
        private EventBean _eventComplex;
        private EventBean _eventNested;

        private SupportBeanSimple _objSimple;
        private SupportBeanComplexProps _objComplex;
        private SupportBeanCombinedProps _objCombined;

        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            _eventTypeSimple = new BeanEventType(null, 0, typeof(SupportBeanSimple), SupportEventAdapterService.Service, null);
            _eventTypeComplex = new BeanEventType(null, 0, typeof(SupportBeanComplexProps), SupportEventAdapterService.Service, null);
            _eventTypeNested = new BeanEventType(null, 0, typeof(SupportBeanCombinedProps), SupportEventAdapterService.Service, null);

            _objSimple = new SupportBeanSimple("a", 20);
            _objComplex = SupportBeanComplexProps.MakeDefaultBean();
            _objCombined = SupportBeanCombinedProps.MakeDefaultBean();

            _eventSimple = new BeanEventBean(_objSimple, _eventTypeSimple);
            _eventComplex = new BeanEventBean(_objComplex, _eventTypeComplex);
            _eventNested = new BeanEventBean(_objCombined, _eventTypeNested);
        }

        [Test]
        public void TestCopyMethod()
        {
            var copyFields = "MyString".Split(',');

            Assert.That(_eventTypeSimple.GetCopyMethod(copyFields), Is.InstanceOf<BeanEventBeanSerializableCopyMethod>());

            var nonSerializable = new BeanEventType(null, 0, typeof(NonSerializableNonCopyable), SupportEventAdapterService.Service, null);
            Assert.That(nonSerializable.GetCopyMethod(copyFields), Is.Null);

            var config = new ConfigurationEventTypeLegacy();
            config.CopyMethod = "MyCopyMethod";

            nonSerializable = new BeanEventType(null, 0, typeof(NonSerializableNonCopyable), SupportEventAdapterService.Service, config);
            try {
                nonSerializable.GetCopyMethod(copyFields);   // also logs error
                Assert.Fail();
            } catch (EPException) {
                // expected
            }

            var myCopyable = new BeanEventType(null, 0, typeof(MyCopyable), SupportEventAdapterService.Service, config);
            Assert.That(myCopyable.GetCopyMethod(copyFields), Is.InstanceOf<BeanEventBeanConfiguredCopyMethod>());   // also logs error

            var myCopyableAndSer = new BeanEventType(null, 0, typeof(MyCopyableAndSerializable), SupportEventAdapterService.Service, config);
            Assert.That(myCopyableAndSer.GetCopyMethod(copyFields), Is.InstanceOf<BeanEventBeanConfiguredCopyMethod>());   // also logs error
        }

        [Test]
        public void TestFragments()
        {
            var nestedTypeFragment = _eventTypeComplex.GetFragmentType("Nested");
            var nestedType = nestedTypeFragment.FragmentType;
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested).FullName, nestedType.Name);
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), nestedType.UnderlyingType);
            Assert.AreEqual(typeof(string), nestedType.GetPropertyType("NestedValue"));
            Assert.IsNull(_eventTypeComplex.GetFragmentType("Indexed[0]"));

            nestedTypeFragment = _eventTypeNested.GetFragmentType("Indexed[0]");
            nestedType = nestedTypeFragment.FragmentType;
            Assert.IsFalse(nestedTypeFragment.IsIndexed);
            Assert.AreEqual(typeof(SupportBeanCombinedProps.NestedLevOne).FullName, nestedType.Name);
            Assert.AreEqual(typeof(IDictionary<string, SupportBeanCombinedProps.NestedLevTwo>), nestedType.GetPropertyType("Mapprop"));

            EventTypeAssertionUtil.AssertConsistency(_eventTypeComplex);
            EventTypeAssertionUtil.AssertConsistency(_eventTypeNested);
        }

        [Test]
        public void TestGetPropertyNames()
        {
            IList<string> properties = _eventTypeSimple.PropertyNames;
            Assert.That(properties.Count, Is.EqualTo(2));
            Assert.That(properties, Contains.Item("MyInt"));
            Assert.That(properties, Contains.Item("MyString"));
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[]
                {
                    new EventPropertyDescriptor("MyInt", typeof(int), null, false, false, false, false, false),
                    new EventPropertyDescriptor("MyString", typeof(string), typeof(char), false, false, true, false, false)
                }, _eventTypeSimple.PropertyDescriptors);

            properties = _eventTypeComplex.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(SupportBeanComplexProps.Properties, properties);

            EPAssertionUtil.AssertEqualsAnyOrder(
                new[]
                {
                    new EventPropertyDescriptor("SimpleProperty", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("MapProperty", typeof(IDictionary<string, string>), typeof(string), false, false, false, true, false),
                    new EventPropertyDescriptor("Mapped", typeof(string), typeof(object), false, true, false, true, false),
                    new EventPropertyDescriptor("Indexed", typeof(int), null, true, false, true, false, false),
                    new EventPropertyDescriptor("IndexedProps", typeof(int[]), typeof(int), false, false, true, false, false),
                    new EventPropertyDescriptor("MappedProps", typeof(IDictionary<string, string>), typeof(string), false, false, false, true, false),
                    new EventPropertyDescriptor("Nested", typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), null, false, false, false, false, true),
                    new EventPropertyDescriptor("ArrayProperty", typeof(int[]), typeof(int), false, false, true, false, false),
                    new EventPropertyDescriptor("ObjectArray", typeof(Object[]), typeof(Object), false, false, true, false, false),
                    new EventPropertyDescriptor("AsArrayProperty", typeof(Array), typeof(Object), false, false, true, false, false)
                }, _eventTypeComplex.PropertyDescriptors);

            properties = _eventTypeNested.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(SupportBeanCombinedProps.PROPERTIES, properties);
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(SupportBeanSimple), _eventTypeSimple.UnderlyingType);
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string), _eventTypeSimple.GetPropertyType("MyString"));
            Assert.IsNull(_eventTypeSimple.GetPropertyType("Dummy"));
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(_eventTypeSimple.IsProperty("MyString"));
            Assert.IsFalse(_eventTypeSimple.IsProperty("Dummy"));
        }

        [Test]
        public void TestGetGetter()
        {
            Assert.AreEqual(null, _eventTypeSimple.GetGetter("Dummy"));

            var getter = _eventTypeSimple.GetGetter("MyInt");
            Assert.AreEqual(20, getter.Get(_eventSimple));
            getter = _eventTypeSimple.GetGetter("MyString");
            Assert.AreEqual("a", getter.Get(_eventSimple));

            try
            {
                // test mismatch between bean and object
                EventType type = SupportEventAdapterService.Service.BeanEventTypeFactory.CreateBeanType(typeof(Object).FullName, typeof(Object), false, false, false);
                EventBean eventBean = new BeanEventBean(new Object(), type);
                getter.Get(eventBean);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // Expected
            }
        }

        [Test]
        public void TestProperties()
        {
            var nestedOne = typeof(SupportBeanCombinedProps.NestedLevOne);
            var nestedOneArr = typeof(SupportBeanCombinedProps.NestedLevOne[]);
            var nestedTwo = typeof(SupportBeanCombinedProps.NestedLevTwo);

            // test nested/combined/indexed/mapped properties
            // PropertyName                 isProperty              getXsSimpleType         hasGetter   getterValue

            var tests = new LinkedList<PropTestDesc>();
            tests.AddLast(new PropTestDesc("SimpleProperty", true, typeof(string), true, "Simple"));
            tests.AddLast(new PropTestDesc("Dummy", false, null, false, null));
            tests.AddLast(new PropTestDesc("Indexed", false, null, false, null));
            tests.AddLast(new PropTestDesc("Indexed[1]", true, typeof(int), true, 2));
            tests.AddLast(new PropTestDesc("Nested", true, typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), true, _objComplex.Nested));
            tests.AddLast(new PropTestDesc("Nested.NestedValue", true, typeof(string), true, _objComplex.Nested.NestedValue));
            tests.AddLast(new PropTestDesc("Nested.NestedNested", true, typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNestedNested), true, _objComplex.Nested.NestedNested));
            tests.AddLast(new PropTestDesc("Nested.NestedNested.NestedNestedValue", true, typeof(string), true, _objComplex.Nested.NestedNested.NestedNestedValue));
            tests.AddLast(new PropTestDesc("Nested.Dummy", false, null, false, null));
            tests.AddLast(new PropTestDesc("Mapped", false, null, false, null));
            tests.AddLast(new PropTestDesc("Mapped('keyOne')", true, typeof(string), true, "valueOne"));
            tests.AddLast(new PropTestDesc("ArrayProperty", true, typeof(int[]), true, _objComplex.ArrayProperty));
            tests.AddLast(new PropTestDesc("ArrayProperty[1]", true, typeof(int), true, 20));
            tests.AddLast(new PropTestDesc("MapProperty('xOne')", true, typeof(string), true, "yOne"));
            tests.AddLast(new PropTestDesc("Google('x')", false, null, false, null));
            tests.AddLast(new PropTestDesc("Mapped('x')", true, typeof(string), true, null));
            tests.AddLast(new PropTestDesc("Mapped('x').x", false, null, false, null));
            tests.AddLast(new PropTestDesc("MapProperty", true, typeof(IDictionary<string,string>), true, _objComplex.MapProperty));
            RunTest(tests, _eventTypeComplex, _eventComplex);

            tests = new LinkedList<PropTestDesc>();
            tests.AddLast(new PropTestDesc("Dummy", false, null, false, null));
            tests.AddLast(new PropTestDesc("MyInt", true, typeof(int), true, _objSimple.MyInt));
            tests.AddLast(new PropTestDesc("MyString", true, typeof(string), true, _objSimple.MyString));
            tests.AddLast(new PropTestDesc("Dummy('a')", false, null, false, null));
            tests.AddLast(new PropTestDesc("Dummy[1]", false, null, false, null));
            tests.AddLast(new PropTestDesc("Dummy.Nested", false, null, false, null));
            RunTest(tests, _eventTypeSimple, _eventSimple);

            tests = new LinkedList<PropTestDesc>();
            tests.AddLast(new PropTestDesc("Indexed", false, null, false, null));
            tests.AddLast(new PropTestDesc("Indexed[1]", true, nestedOne, true, _objCombined.GetIndexed(1)));
            tests.AddLast(new PropTestDesc("Indexed.Mapped", false, null, false, null));
            tests.AddLast(new PropTestDesc("Indexed[1].Mapped", false, null, false, null));
            tests.AddLast(new PropTestDesc("Array", true, nestedOneArr, true, _objCombined.Array));
            tests.AddLast(new PropTestDesc("Array.Mapped", false, null, false, null));
            tests.AddLast(new PropTestDesc("Array[0]", true, nestedOne, true, _objCombined.Array[0]));
            tests.AddLast(new PropTestDesc("Array[1].Mapped", false, null, false, null));
            tests.AddLast(new PropTestDesc("Array[1].Mapped('x')", true, nestedTwo, true, _objCombined.Array[1].GetMapped("x")));
            tests.AddLast(new PropTestDesc("Array[1].Mapped('1mb')", true, nestedTwo, true, _objCombined.Array[1].GetMapped("1mb")));
            tests.AddLast(new PropTestDesc("Indexed[1].Mapped('x')", true, nestedTwo, true, _objCombined.GetIndexed(1).GetMapped("x")));
            tests.AddLast(new PropTestDesc("Indexed[1].Mapped('x').Value", true, typeof(string), true, null));
            tests.AddLast(new PropTestDesc("Indexed[1].Mapped('1mb')", true, nestedTwo, true, _objCombined.GetIndexed(1).GetMapped("1mb")));
            tests.AddLast(new PropTestDesc("Indexed[1].Mapped('1mb').Value", true, typeof(string), true, _objCombined.GetIndexed(1).GetMapped("1mb").Value));
            tests.AddLast(new PropTestDesc("Array[1].Mapprop", true, typeof(IDictionary<string, SupportBeanCombinedProps.NestedLevTwo>), true, _objCombined.GetIndexed(1).Mapprop));
            tests.AddLast(new PropTestDesc("Array[1].Mapprop('1ma')", true, typeof(SupportBeanCombinedProps.NestedLevTwo), true, _objCombined.Array[1].GetMapped("1ma")));
            tests.AddLast(new PropTestDesc("Array[1].Mapprop('1ma').Value", true, typeof(string), true, "1ma0"));
            tests.AddLast(new PropTestDesc("Indexed[1].Mapprop", true, typeof(IDictionary<string, SupportBeanCombinedProps.NestedLevTwo>), true, _objCombined.GetIndexed(1).Mapprop));
            RunTest(tests, _eventTypeNested, _eventNested);

            TryInvalidIsProperty(_eventTypeComplex, "x[");
            TryInvalidIsProperty(_eventTypeComplex, "dummy()");
            TryInvalidIsProperty(_eventTypeComplex, "nested.xx['a']");
            TryInvalidGetPropertyType(_eventTypeComplex, "x[");
            TryInvalidGetPropertyType(_eventTypeComplex, "dummy()");
            TryInvalidGetPropertyType(_eventTypeComplex, "nested.xx['a']");
            TryInvalidGetPropertyType(_eventTypeNested, "dummy[(");
            TryInvalidGetPropertyType(_eventTypeNested, "Array[1].Mapprop[x].value");
        }

        [Test]
        public void TestGetDeepSuperTypes()
        {
            var type = new BeanEventType(null, 1, typeof(ISupportAImplSuperGImplPlus), SupportEventAdapterService.Service, null);

            var deepSuperTypes = new List<EventType>(type.DeepSuperTypes);

            var beanEventTypeFactory = SupportEventAdapterService.Service.BeanEventTypeFactory;

            Assert.AreEqual(5, deepSuperTypes.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(
                    deepSuperTypes.ToArray(),
                    new EventType[]{
                            beanEventTypeFactory.CreateBeanType("e1", typeof(ISupportAImplSuperG), false, false, false),
                            beanEventTypeFactory.CreateBeanType("e2", typeof(ISupportBaseAB), false, false, false),
                            beanEventTypeFactory.CreateBeanType("e3", typeof(ISupportA), false, false, false),
                            beanEventTypeFactory.CreateBeanType("e4", typeof(ISupportB), false, false, false),
                            beanEventTypeFactory.CreateBeanType("e5", typeof(ISupportC), false, false, false)
                    });
        }

        [Test]
        public void TestGetSuper()
        {
            var classes = new LinkedHashSet<Type>();
            BeanEventType.GetSuper(typeof(ISupportAImplSuperGImplPlus), classes);

            Assert.AreEqual(6, classes.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(
                    classes.ToArray(),
                    new[]{
                            typeof(ISupportAImplSuperG), typeof(ISupportBaseAB),
                            typeof(ISupportA), typeof(ISupportB), typeof(ISupportC),
                            typeof(Object)
                    }
            );

            classes.Clear();
            BeanEventType.GetSuper(typeof(Object), classes);
            Assert.AreEqual(0, classes.Count);
        }

        [Test]
        public void TestGetSuperTypes()
        {
            _eventTypeSimple = new BeanEventType(null, 1, typeof(ISupportAImplSuperGImplPlus), SupportEventAdapterService.Service, null);

            var superTypes = _eventTypeSimple.SuperTypes;
            Assert.AreEqual(5, superTypes.Length);
            Assert.IsTrue(superTypes.Contains(t => t.UnderlyingType == typeof(ISupportAImplSuperG)));
            Assert.IsTrue(superTypes.Contains(t => t.UnderlyingType == typeof(ISupportB)));
            Assert.IsTrue(superTypes.Contains(t => t.UnderlyingType == typeof(ISupportC)));

            _eventTypeSimple = new BeanEventType(null, 1, typeof(Object), SupportEventAdapterService.Service, null);
            superTypes = _eventTypeSimple.SuperTypes;
            Assert.AreEqual(null, superTypes);

            var type = new BeanEventType(null, 1, typeof(ISupportD), SupportEventAdapterService.Service, null);
            Assert.AreEqual(3, type.PropertyNames.Length);
            EPAssertionUtil.AssertEqualsAnyOrder(
                    type.PropertyNames,
                    new[] { "D", "BaseD", "BaseDBase" });
        }

        private static void TryInvalidGetPropertyType(BeanEventType type, String property)
        {
            try
            {
                type.GetPropertyType(property);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // expected
            }
        }

        private static void TryInvalidIsProperty(BeanEventType type, String property)
        {
            try
            {
                type.GetPropertyType(property);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // expected
            }
        }

        private static void RunTest(IEnumerable<PropTestDesc> tests, BeanEventType eventType, EventBean eventBean)
        {
            foreach (var desc in tests)
            {
                RunTest(desc, eventType, eventBean);
            }
        }

        private static void RunTest(PropTestDesc test, BeanEventType eventType, EventBean eventBean)
        {
            var propertyName = test.PropertyName;

            Assert.AreEqual(test.IsProperty, eventType.IsProperty(propertyName), "IsProperty mismatch on '" + propertyName + "',");
            Assert.AreEqual(test.Clazz, eventType.GetPropertyType(propertyName), "GetPropertyType mismatch on '" + propertyName + "',");

            var getter = eventType.GetGetter(propertyName);
            if (getter == null)
            {
                Assert.IsFalse(test.HasGetter, "getGetter null on '" + propertyName + "',");
            }
            else
            {
                Assert.IsTrue(test.HasGetter, "getGetter not null on '" + propertyName + "',");
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
                    Assert.AreEqual(test.GetterReturnValue, value, "getter value mismatch on '" + propertyName + "',");
                }
            }
        }

        public class PropTestDesc
        {
            public PropTestDesc(String propertyName, Boolean property, Type clazz, Boolean hasGetter, Object getterReturnValue)
            {
                PropertyName = propertyName;
                IsProperty = property;
                Clazz = clazz;
                HasGetter = hasGetter;
                GetterReturnValue = getterReturnValue;
            }

            public string PropertyName { get; private set; }

            public bool IsProperty { get; private set; }

            public Type Clazz { get; private set; }

            public bool HasGetter { get; private set; }

            public object GetterReturnValue { get; private set; }
        }

        public class NonSerializableNonCopyable
        {
            public string MyString { get; set; }
        }

        public class MyCopyable
        {
            public string MyString { get; set; }

            public MyCopyable MyCopyMethod()
            {
                return this;
            }
        }

        [Serializable]
        public class MyCopyableAndSerializable
        {
            public string MyString { get; set; }

            public MyCopyableAndSerializable MyCopyMethod()
            {
                return this;
            }
        }
    }
}
