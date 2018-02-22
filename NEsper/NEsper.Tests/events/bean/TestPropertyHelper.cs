///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.magic;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestPropertyHelper
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }
        
        [Test]
        public void TestMagicProperties()
        {
            var magicType = MagicType.GetCachedType(typeof(SupportBeanPropertyNames));
            var sProperties = new List<SimpleMagicPropertyInfo>(magicType.GetSimpleProperties(true));
            var iProperties = new List<SimpleMagicPropertyInfo>(magicType.GetIndexedProperties(true));
            var mProperties = new List<SimpleMagicPropertyInfo>(magicType.GetMappedProperties(true));

            Assert.AreEqual(6, mProperties.Count);
            Assert.AreEqual(8, sProperties.Count);
            Assert.AreEqual(1, iProperties.Count);

            var iProperty = iProperties.Find(item => item.Name == "Indexed");
            Assert.IsNotNull(iProperty);
            Assert.IsNotNull(iProperty.GetMethod);
        }

        [Test]
        public void TestMappedProperties()
        {
            var magicType = MagicType.GetCachedType(typeof(SupportBeanPropertyNames));
            var properties = new List<SimpleMagicPropertyInfo>(magicType.GetMappedProperties(true));

            Assert.AreEqual(6, properties.Count);
            Assert.IsTrue(properties.Any(item => item.Name == "A"));
            Assert.IsTrue(properties.Any(item => item.Name == "AB"));
            Assert.IsTrue(properties.Any(item => item.Name == "ABC"));
            Assert.IsTrue(properties.Any(item => item.Name == "Ab"));
            Assert.IsTrue(properties.Any(item => item.Name == "Abc"));
            Assert.IsTrue(properties.Any(item => item.Name == "FooBah"));
        }

        [Test]
        public void TestGetGetter()
        {
            FastClass fastClass = FastClass.Create(typeof(SupportBeanPropertyNames));
            EventBean bean = SupportEventBeanFactory.CreateObject(new SupportBeanPropertyNames());
            MethodInfo method = typeof(SupportBeanPropertyNames).GetProperty("A").GetGetMethod();
            EventPropertyGetter getter = PropertyHelper.GetGetter("A", method, fastClass, _container.Resolve<EventAdapterService>());
            Assert.AreEqual("", getter.Get(bean));
        }

        [Test]
        public void TestRemoveClrProperties()
        {
            var method1 = typeof(object).GetMethod("GetHashCode");
            var method2 = typeof(object).GetMethod("ToString");
            var method3 = typeof(object).GetMethod("GetType");

            IList<InternalEventPropDescriptor> result = new List<InternalEventPropDescriptor>();
            result.Add(new InternalEventPropDescriptor("x", (MethodInfo) null, null));
            result.Add(new InternalEventPropDescriptor("GetHashCode", method1, null));
            result.Add(new InternalEventPropDescriptor("ToString", method2, null));
            result.Add(new InternalEventPropDescriptor("GetType", method3, null));

            PropertyHelper.RemoveCLRProperties(result);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("x", result[0].PropertyName);
        }

        [Test]
        public void TestRemoveDuplicateProperties()
        {
            IList<InternalEventPropDescriptor> result = new List<InternalEventPropDescriptor>();
            result.Add(new InternalEventPropDescriptor("x", (MethodInfo) null, null));
            result.Add(new InternalEventPropDescriptor("x", (MethodInfo) null, null));
            result.Add(new InternalEventPropDescriptor("y", (MethodInfo) null, null));

            PropertyHelper.RemoveDuplicateProperties(result);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("x", result[0].PropertyName);
            Assert.AreEqual("y", result[1].PropertyName);
        }
    }
}
