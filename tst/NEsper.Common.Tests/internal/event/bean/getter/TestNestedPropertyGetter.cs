///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    [TestFixture]
    public class TestNestedPropertyGetter : AbstractCommonTest
    {
        private NestedPropertyGetter getter;
        private NestedPropertyGetter getterNull;
        private EventBean theEvent;
        private SupportBeanCombinedProps bean;
        private readonly BeanEventTypeFactory beanEventTypeFactory;

        [SetUp]
        public void SetUp()
        {
            bean = SupportBeanCombinedProps.MakeDefaultBean();
            theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);

            IList<EventPropertyGetter> getters = new List<EventPropertyGetter>();
            getters.Add(MakeGetterOne(0));
            getters.Add(MakeGetterTwo("0ma"));
            getter = new NestedPropertyGetter(getters, null, typeof(IDictionary<string, object>), null, null);

            getters = new List<EventPropertyGetter>();
            getters.Add(MakeGetterOne(2));
            getters.Add(MakeGetterTwo("0ma"));
            getterNull = new NestedPropertyGetter(getters, null, typeof(IDictionary<string, object>), null, null);
        }

        [Test]
        public void TestGet()
        {
            Assert.AreEqual(bean.GetIndexed(0).GetMapped("0ma"), getter.Get(theEvent));

            // test null value returned
            Assert.IsNull(getterNull.Get(theEvent));
        }

        private KeyedMethodPropertyGetter MakeGetterOne(int index)
        {
            MethodInfo methodOne = typeof(SupportBeanCombinedProps)
                .GetMethod("GetIndexed", new Type[] { typeof(int) });
            Assert.That(methodOne, Is.Not.Null);
            return new KeyedMethodPropertyGetter(methodOne, index, null, null);
        }

        private KeyedMethodPropertyGetter MakeGetterTwo(string key)
        {
            MethodInfo methodTwo = typeof(SupportBeanCombinedProps.NestedLevOne)
                .GetMethod("GetMapped", new Type[] { typeof(string) });
            Assert.That(methodTwo, Is.Not.Null);
            return new KeyedMethodPropertyGetter(methodTwo, key, null, null);
        }
    }
} // end of namespace
