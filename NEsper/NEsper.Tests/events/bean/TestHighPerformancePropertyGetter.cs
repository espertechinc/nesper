///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.core.support;

using XLR8.CGLib;

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
    public class TestHighPerformancePropertyGetter 
    {
        private EventBean unitTestBean;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            SupportBean testEvent = new SupportBean();
            testEvent.IntPrimitive = 10;
            testEvent.TheString = "a";
            testEvent.DoubleBoxed = null;
    
            unitTestBean = SupportEventBeanFactory.CreateObject(testEvent);
        }
    
        [Test]
        public void TestGetter()
        {
            var getter = MakeCGIGetter(typeof(SupportBean), "IntPrimitive");
            Assert.AreEqual(10, getter.Get(unitTestBean));

            getter = MakeCGIGetter(typeof(SupportBean), "TheString");
            Assert.AreEqual("a", getter.Get(unitTestBean));

            getter = MakeCGIGetter(typeof(SupportBean), "DoubleBoxed");
            Assert.AreEqual(null, getter.Get(unitTestBean));
    
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
            var getter = MakeCGIGetter(typeof(SupportBean), "IntPrimitive");
    
            Log.Info(".testPerformance Starting test");
    
            for (int i = 0; i < 10; i++)   // Change to 1E8 for performance testing
            {
                int value = (int) getter.Get(unitTestBean);
                Assert.AreEqual(10, value);
            }
    
            Log.Info(".testPerformance Done test");
        }
    
        private LambdaPropertyGetter MakeLambdaGetter(Type clazz, String methodName)
        {
            MethodInfo method = clazz.GetMethod(methodName, new Type[] {});
            LambdaPropertyGetter getter = new LambdaPropertyGetter(method, _container.Resolve<EventAdapterService>());
            return getter;
        }

        private CGLibPropertyGetter MakeCGIGetter(Type clazz, String propertyName)
        {
            FastClass fastClass = FastClass.Create(clazz);
            PropertyInfo propertyInfo = clazz.GetProperty(propertyName);
            FastProperty fastProp = fastClass.GetProperty(propertyInfo);
    
            CGLibPropertyGetter getter = new CGLibPropertyGetter(propertyInfo, fastProp, _container.Resolve<EventAdapterService>());
    
            return getter;
        }

#if false
        [Test]
        public void TestGetterSpecial()
        {
        	Type clazz = typeof(SupportBeanComplexProps);
            FastClass fastClass = FastClass.Create(clazz);
            
            // set up bean
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
    
            // try mapped property
            MethodInfo method = clazz.GetMethod("GetMapped", new Type[] {typeof(string)});
            FastMethod fastMethod = fastClass.GetMethod(method);
        	Object result = fastMethod.Invoke(bean, new Object[] {"keyOne"});
        	Assert.AreEqual("valueOne", result);
        	result = fastMethod.Invoke(bean, new Object[] {"keyTwo"});
        	Assert.AreEqual("valueTwo", result);
        	
        	// try index property
            method = clazz.GetMethod("GetIndexed", new Type[] {typeof(int)});
            fastMethod = fastClass.GetMethod(method);
        	result = fastMethod.Invoke(bean, new Object[] {0});
        	Assert.AreEqual(1, result);
        	result = fastMethod.Invoke(bean, new Object[] {1});
        	Assert.AreEqual(2, result);
    
        	// try nested property
            method = clazz.GetMethod("_GetNested", new Type[] {});
            fastMethod = fastClass.GetMethod(method);               
            SupportBeanComplexProps.SupportBeanSpecialGetterNested nested = (SupportBeanComplexProps.SupportBeanSpecialGetterNested) fastMethod.Invoke(bean, new Object[] {});
    
            Type nestedClazz = typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested);
            MethodInfo methodNested = nestedClazz.GetMethod("_GetNestedValue", new Type[] {});
            FastClass fastClassNested = FastClass.Create(nestedClazz);        
            fastClassNested.GetMethod(methodNested);        
        }
#endif

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
