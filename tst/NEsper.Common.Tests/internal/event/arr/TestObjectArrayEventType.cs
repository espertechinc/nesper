///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.arr
{
    [TestFixture]
    public class TestObjectArrayEventType : AbstractCommonTest
    {
        private ObjectArrayEventType eventType;

        [SetUp]
        public void SetUp()
        {
            var metadata = new EventTypeMetadata("MyType", null, EventTypeTypeClass.STREAM, EventTypeApplicationType.OBJECTARR, NameAccessModifier.INTERNAL, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            string[] names = { "MyInt", "MyIntBoxed", "MyString", "MySupportBean", "MyComplexBean", "MyNullType" };
            object[] types = { typeof(int?), typeof(int?), typeof(string), typeof(SupportBean), typeof(SupportBeanComplexProps), TypeHelper.NullType };

            IDictionary<string, object> namesAndTypes = new LinkedHashMap<string, object>();
            for (var i = 0; i < names.Length; i++)
            {
                namesAndTypes.Put(names[i], types[i]);
            }

            eventType = new ObjectArrayEventType(metadata, namesAndTypes, null, null, null, null,
                SupportEventTypeFactory.GetInstance(container).BEAN_EVENT_TYPE_FACTORY);
        }

        [Test]
        public void TestGetPropertyNames()
        {
            var properties = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(properties, new string[] { "MyInt", "MyIntBoxed", "MyString", "MySupportBean", "MyComplexBean", "MyNullType" });
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("MyInt"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("MyIntBoxed"));
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
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(object[]), eventType.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(eventType.IsProperty("MyInt"));
            Assert.IsTrue(eventType.IsProperty("MyIntBoxed"));
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
        public void TestGetGetter()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            var complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            Assert.AreEqual(null, eventType.GetGetter("dummy"));

            var values = new object[] { 20, 20, "a", nestedSupportBean, complexPropBean, null };
            EventBean eventBean = new ObjectArrayEventBean(values, eventType);

            Assert.AreEqual(20, eventType.GetGetter("MyInt").Get(eventBean));
            Assert.AreEqual(20, eventType.GetGetter("MyIntBoxed").Get(eventBean));
            Assert.AreEqual("a", eventType.GetGetter("MyString").Get(eventBean));
            Assert.AreEqual(nestedSupportBean, eventType.GetGetter("MySupportBean").Get(eventBean));
            Assert.AreEqual(100, eventType.GetGetter("MySupportBean.IntPrimitive").Get(eventBean));
            Assert.AreEqual("NestedValue", eventType.GetGetter("MyComplexBean.Nested.NestedValue").Get(eventBean));
        }

        [Test]
        public void TestGetSuperTypes()
        {
            Assert.IsNull(eventType.SuperTypes);
        }
    }
} // end of namespace
