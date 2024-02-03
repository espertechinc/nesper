///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;

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
            object[] types = { typeof(int?), typeof(int?), typeof(string), typeof(SupportBean), typeof(SupportBeanComplexProps), null };

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
            ClassicAssert.AreEqual(typeof(int?), eventType.GetPropertyType("MyInt"));
            ClassicAssert.AreEqual(typeof(int?), eventType.GetPropertyType("MyIntBoxed"));
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
        public void TestGetUnderlyingType()
        {
            ClassicAssert.AreEqual(typeof(object[]), eventType.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            ClassicAssert.IsTrue(eventType.IsProperty("MyInt"));
            ClassicAssert.IsTrue(eventType.IsProperty("MyIntBoxed"));
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
        public void TestGetGetter()
        {
            var nestedSupportBean = new SupportBean();
            nestedSupportBean.IntPrimitive = 100;
            var complexPropBean = SupportBeanComplexProps.MakeDefaultBean();

            ClassicAssert.AreEqual(null, eventType.GetGetter("dummy"));

            var values = new object[] { 20, 20, "a", nestedSupportBean, complexPropBean, null };
            EventBean eventBean = new ObjectArrayEventBean(values, eventType);

            ClassicAssert.AreEqual(20, eventType.GetGetter("MyInt").Get(eventBean));
            ClassicAssert.AreEqual(20, eventType.GetGetter("MyIntBoxed").Get(eventBean));
            ClassicAssert.AreEqual("a", eventType.GetGetter("MyString").Get(eventBean));
            ClassicAssert.AreEqual(nestedSupportBean, eventType.GetGetter("MySupportBean").Get(eventBean));
            ClassicAssert.AreEqual(100, eventType.GetGetter("MySupportBean.IntPrimitive").Get(eventBean));
            ClassicAssert.AreEqual("NestedValue", eventType.GetGetter("MyComplexBean.Nested.NestedValue").Get(eventBean));
        }

        [Test]
        public void TestGetSuperTypes()
        {
            ClassicAssert.IsNull(eventType.SuperTypes);
        }
    }
} // end of namespace
