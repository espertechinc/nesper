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
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.arr
{
    [TestFixture]
    public class TestObjectArrayEventType : CommonTest
    {
        private ObjectArrayEventType eventType;

        [SetUp]
        public void SetUp()
        {
            var metadata = new EventTypeMetadata("MyType", null, EventTypeTypeClass.STREAM, EventTypeApplicationType.OBJECTARR, NameAccessModifier.PROTECTED, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            string[] names = { "myInt", "myIntBoxed", "myString", "mySupportBean", "myComplexBean", "myNullType" };
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
            EPAssertionUtil.AssertEqualsAnyOrder(properties, new string[] { "myInt", "myIntBoxed", "myString", "mySupportBean", "myComplexBean", "myNullType" });
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myInt"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myIntBoxed"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("myString"));
            Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("mySupportBean"));
            Assert.AreEqual(typeof(SupportBeanComplexProps), eventType.GetPropertyType("myComplexBean"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("mySupportBean.intPrimitive"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("myComplexBean.nested.nestedValue"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myComplexBean.indexed[1]"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("myComplexBean.mapped('a')"));
            Assert.AreEqual(null, eventType.GetPropertyType("myNullType"));

            Assert.IsNull(eventType.GetPropertyType("dummy"));
            Assert.IsNull(eventType.GetPropertyType("mySupportBean.dfgdg"));
            Assert.IsNull(eventType.GetPropertyType("xxx.intPrimitive"));
            Assert.IsNull(eventType.GetPropertyType("myComplexBean.nested.nestedValueXXX"));
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(object[]), eventType.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(eventType.IsProperty("myInt"));
            Assert.IsTrue(eventType.IsProperty("myIntBoxed"));
            Assert.IsTrue(eventType.IsProperty("myString"));
            Assert.IsTrue(eventType.IsProperty("mySupportBean.intPrimitive"));
            Assert.IsTrue(eventType.IsProperty("myComplexBean.nested.nestedValue"));
            Assert.IsTrue(eventType.IsProperty("myComplexBean.indexed[1]"));
            Assert.IsTrue(eventType.IsProperty("myComplexBean.mapped('a')"));
            Assert.IsTrue(eventType.IsProperty("myNullType"));

            Assert.IsFalse(eventType.IsProperty("dummy"));
            Assert.IsFalse(eventType.IsProperty("mySupportBean.dfgdg"));
            Assert.IsFalse(eventType.IsProperty("xxx.intPrimitive"));
            Assert.IsFalse(eventType.IsProperty("myComplexBean.nested.nestedValueXXX"));
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

            Assert.AreEqual(20, eventType.GetGetter("myInt").Get(eventBean));
            Assert.AreEqual(20, eventType.GetGetter("myIntBoxed").Get(eventBean));
            Assert.AreEqual("a", eventType.GetGetter("myString").Get(eventBean));
            Assert.AreEqual(nestedSupportBean, eventType.GetGetter("mySupportBean").Get(eventBean));
            Assert.AreEqual(100, eventType.GetGetter("mySupportBean.intPrimitive").Get(eventBean));
            Assert.AreEqual("nestedValue", eventType.GetGetter("myComplexBean.nested.nestedValue").Get(eventBean));
        }

        [Test]
        public void TestGetSuperTypes()
        {
            Assert.IsNull(eventType.SuperTypes);
        }
    }
} // end of namespace