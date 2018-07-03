///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestKeyedMethodPropertyGetter 
    {
        private KeyedMethodPropertyGetter _getter;
        private EventBean _theEvent;
        private SupportBeanComplexProps _bean;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _bean = SupportBeanComplexProps.MakeDefaultBean();
            _theEvent = SupportEventBeanFactory.CreateObject(_bean);
            var method = typeof(SupportBeanComplexProps).GetMethod("GetIndexed", new Type[] {typeof(int)});
            _getter = new KeyedMethodPropertyGetter(method, 1, _container.Resolve<EventAdapterService>());
        }
    
        [Test]
        public void TestGet()
        {
            Assert.AreEqual(_bean.GetIndexed(1), _getter.Get(_theEvent));
            Assert.AreEqual(_bean.GetIndexed(1), _getter.Get(_theEvent, 1));
    
            try
            {
                _getter.Get(SupportEventBeanFactory.CreateObject(""));
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // expected
            }
        }
    }
}
