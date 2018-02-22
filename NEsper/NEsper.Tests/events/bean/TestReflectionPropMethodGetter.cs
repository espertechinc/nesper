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
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestReflectionPropMethodGetter 
    {
        private EventBean _unitTestBean;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            SupportBean testEvent = new SupportBean();
            testEvent.IntPrimitive = 10;
            testEvent.TheString = "a";
            testEvent.DoubleBoxed = null;
    
            _unitTestBean = SupportEventBeanFactory.CreateObject(testEvent);
        }
    
        [Test]
        public void TestGetter()
        {
            ReflectionPropMethodGetter getter = MakeGetter(typeof(SupportBean), "IntPrimitive");
            Assert.AreEqual(10, getter.Get(_unitTestBean));
    
            getter = MakeGetter(typeof(SupportBean), "TheString");
            Assert.AreEqual("a", getter.Get(_unitTestBean));
    
            getter = MakeGetter(typeof(SupportBean), "DoubleBoxed");
            Assert.AreEqual(null, getter.Get(_unitTestBean));
    
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
    
        [Test]
        public void TestPerformance()
        {
            ReflectionPropMethodGetter getter = MakeGetter(typeof(SupportBean), "IntPrimitive");
    
            Log.Info(".testPerformance Starting test");
    
            for (int i = 0; i < 10; i++)   // Change to 1E8 for performance testing
            {
                int value = (int) getter.Get(_unitTestBean);
                Assert.AreEqual(10, value);
            }
    
            Log.Info(".testPerformance Done test");
        }
    
        private ReflectionPropMethodGetter MakeGetter(Type clazz, String propertyName)
        {
            var property = clazz.GetProperty(propertyName);
            //MethodInfo method = clazz.GetMethod(PropertyName, new Type[] {});
            var getter = new ReflectionPropMethodGetter(
                property.GetGetMethod(),
                _container.Resolve<EventAdapterService>());
    
            return getter;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
