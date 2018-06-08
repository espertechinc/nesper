///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    using SupportEventAdapterService = core.support.SupportEventAdapterService;

    [TestFixture]
	public class TestCGLibPropertyGetter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private EventBean _unitTestBean;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var testEvent = new SupportBean();
	        testEvent.IntPrimitive = 10;
	        testEvent.TheString = "a";
	        testEvent.DoubleBoxed = null;

	        _unitTestBean = SupportEventBeanFactory.CreateObject(testEvent);
	    }

        [Test]
	    public void TestGetter()
        {
	        var getter = MakeGetter(typeof(SupportBean), "IntPrimitive");
	        Assert.AreEqual(10, getter.Get(_unitTestBean));

	        getter = MakeGetter(typeof(SupportBean), "TheString");
	        Assert.AreEqual("a", getter.Get(_unitTestBean));

	        getter = MakeGetter(typeof(SupportBean), "DoubleBoxed");
	        Assert.AreEqual(null, getter.Get(_unitTestBean));

	        try
            {
	            EventBean eventBean = SupportEventBeanFactory.CreateObject(new object());
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
	        var getter = MakeGetter(typeof(SupportBean), "IntPrimitive");

	        Log.Info(".testPerformance Starting test");

	        for (var i = 0; i < 10; i++) { // Change to 1E8 for performance testing
	            int value = getter.Get(_unitTestBean).AsInt();
	            Assert.AreEqual(10, value);
	        }

	        Log.Info(".testPerformance Done test");
	    }

	    private CGLibPropertyGetter MakeGetter(Type clazz, string propertyName)
        {
	        var fastClass = FastClass.Create(clazz);
	        var baseProperty = clazz.GetProperty(propertyName);
	        var fastProperty = fastClass.GetProperty(baseProperty);
	        var getter = new CGLibPropertyGetter(baseProperty, fastProperty, _container.Resolve<EventAdapterService>());

	        return getter;
	    }

        [Test]
	    public void TestGetterSpecial()
        {
	        var clazz = typeof(SupportBeanComplexProps);
	        var fastClass = FastClass.Create(clazz);

	        // set up bean
	        var bean = SupportBeanComplexProps.MakeDefaultBean();

	        // try mapped property
            var method = clazz.GetMethod("GetMapped", new Type[] { typeof(string) });
	        var fastMethod = fastClass.GetMethod(method);
	        var result = fastMethod.Invoke(bean, new object[]{"keyOne"});
	        Assert.AreEqual("valueOne", result);
	        result = fastMethod.Invoke(bean, new object[]{"keyTwo"});
	        Assert.AreEqual("valueTwo", result);

	        // try index property
	        method = clazz.GetMethod("GetIndexed", new Type[]{typeof(int)});
	        fastMethod = fastClass.GetMethod(method);
	        result = fastMethod.Invoke(bean, new object[]{0});
	        Assert.AreEqual(1, result);
	        result = fastMethod.Invoke(bean, new object[]{1});
	        Assert.AreEqual(2, result);

	        // try nested property
	        method = clazz.GetMethod("GetNested", new Type[]{});
	        fastMethod = fastClass.GetMethod(method);
	        var nested = (SupportBeanComplexProps.SupportBeanSpecialGetterNested) fastMethod.Invoke(bean, new object[]{});

	        var nestedClazz = typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested);
            var methodNested = nestedClazz.GetMethod("GetNestedValue", new Type[] { });
	        var fastClassNested = FastClass.Create(nestedClazz);
	        fastClassNested.GetMethod(methodNested);
	    }
	}
} // end of namespace
