///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;



namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestReflectionPropFieldGetter 
    {
        private EventBean _unitTestBean;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            SupportLegacyBean testEvent = new SupportLegacyBean("a");
            _unitTestBean = SupportEventBeanFactory.CreateObject(testEvent);
        }
    
        [Test]
        public void TestGetter()
        {
            ReflectionPropFieldGetter getter = MakeGetter(typeof(SupportLegacyBean), "fieldLegacyVal");
            Assert.AreEqual("a", getter.Get(_unitTestBean));
    
            try
            {
                EventBean eventBean = SupportEventBeanFactory.CreateObject(new Object());
                getter.Get(eventBean);
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex)
            {
                // Expected
                Log.Debug(".testGetter Expected exception, msg=" + ex.Message);
            }
        }
    
        private ReflectionPropFieldGetter MakeGetter(Type clazz, String fieldName)
        {
            FieldInfo field = clazz.GetField(fieldName);
            ReflectionPropFieldGetter getter = new ReflectionPropFieldGetter(field, _container.Resolve<EventAdapterService>());
            return getter;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
