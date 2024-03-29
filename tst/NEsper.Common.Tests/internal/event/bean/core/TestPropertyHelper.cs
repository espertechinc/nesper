///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    [TestFixture]
    public class TestPropertyHelper : AbstractCommonTest
    {
        [Test]
        public void TestAddMappedProperties()
        {
            IList<PropertyStem> result = new List<PropertyStem>();
            PropertyHelper.AddMappedProperties(
                MagicType.GetCachedType<SupportBeanPropertyNames>(), result);
            ClassicAssert.AreEqual(6, result.Count);

            IList<string> propertyNames = new List<string>();
            foreach (PropertyStem desc in result)
            {
                Log.Debug("desc=" + desc.PropertyName);
                propertyNames.Add(desc.PropertyName);
            }
            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { "a", "AB", "ABC", "ab", "abc", "fooBah" }, propertyNames.ToArray());
        }

        [Test]
        public void TestAddIntrospectProperties()
        {
            IList<PropertyStem> result = new List<PropertyStem>();
            PropertyHelper.AddIntrospectProperties(
                MagicType.GetCachedType<SupportBeanPropertyNames>(), result);

            foreach (PropertyStem desc in result)
            {
                Log.Debug("desc=" + desc.PropertyName);
            }

            ClassicAssert.AreEqual(11, result.Count); // for "class" is also in there

            var indexedProperty = result.FirstOrDefault(p => p.PropertyName == "Indexed");
            Assert.That(indexedProperty, Is.Not.Null);
            Assert.That(indexedProperty.AccessorProp, Is.Null);
            Assert.That(indexedProperty.AccessorField, Is.Null);
            Assert.That(indexedProperty.ReadMethod, Is.Not.Null);
        }

        [Test]
        public void TestRemoveDuplicateProperties()
        {
            IList<PropertyStem> result = new List<PropertyStem>();
            result.Add(new PropertyStem("x", (MethodInfo) null, PropertyType.UNDEFINED));
            result.Add(new PropertyStem("x", (MethodInfo) null, PropertyType.UNDEFINED));
            result.Add(new PropertyStem("y", (MethodInfo) null, PropertyType.UNDEFINED));

            PropertyHelper.RemoveDuplicateProperties(result);

            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual("x", result[0].PropertyName);
            ClassicAssert.AreEqual("y", result[1].PropertyName);
        }

        [Test]
        public void TestRemoveProperties()
        {
            IList<PropertyStem> result = new List<PropertyStem>();
            result.Add(new PropertyStem("x", (MethodInfo) null, PropertyType.UNDEFINED));

            // Add all methods from System.Object that have no parameters, these
            // would be getters like GetType(), GetHashCode(), ToString().
            foreach (var method in typeof(object)
                .GetMethods()
                .Where(m => m.ReturnType != typeof(void) &&
                            m.GetParameters().Length == 0))
            {
                result.Add(new PropertyStem(method.Name, method, PropertyType.UNDEFINED));
            }

            // Ensure that there are at least two properties.
            Assert.That(result.Count, Is.GreaterThan(1));

            PropertyHelper.RemovePlatformProperties(result);

            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual("x", result[0].PropertyName);
        }

        [Test]
        public void TestGetGetter()
        {
            var supportEventTypeFactory = SupportEventTypeFactory.GetInstance(container);
            var bean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBeanPropertyNames());
            var property = typeof(SupportBeanPropertyNames).GetProperty("A");
            var getter = PropertyHelper.GetGetter(property, EventBeanTypedEventFactoryCompileTime.INSTANCE,
                supportEventTypeFactory.BEAN_EVENT_TYPE_FACTORY);
            ClassicAssert.AreEqual("", getter.Get(bean));
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace