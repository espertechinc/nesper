///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.core; 
using com.espertech.esper.regression.client;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestMethodResolver 
    {		
        [Test]
    	public void TestResolveMethodStaticOnly()
    	{
            var declClass = typeof(Math);
    		var methodName = "Max";
    		var args = new Type[] { typeof(int), typeof(int) };
    		var expected = typeof(Math).GetMethod(methodName, args);
    		Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));
    		
    		args = new Type[] { typeof(long), typeof(long) };
    		expected = typeof(Math).GetMethod(methodName, args);
    		args = new Type[] { typeof(int), typeof(long) };
    		Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));
    		
    		args = new Type[] { typeof(int), typeof(int) };
    		expected = typeof(Math).GetMethod(methodName, args);
    		args = new Type[] { typeof(int), typeof(int) };
    		Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));
    		
    		args = new Type[] { typeof(long), typeof(long) };
    		expected = typeof(Math).GetMethod(methodName, args);
    		args = new Type[] { typeof(int), typeof(long) };
    		Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));
    		
    		args = new Type[] { typeof(float), typeof(float) };
    		expected = typeof(Math).GetMethod(methodName, args);
    		args = new Type[] { typeof(int), typeof(float?) };
    		Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));
    		
            declClass = typeof(DateTimeHelper);
    		methodName = "GetCurrentTimeMillis";
    		args = new Type[0];
    		expected = typeof(DateTimeHelper).GetMethod(methodName, args);
    		Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));
    	}
    	
        [Test]
        public void TestResolveMethodStaticAndInstance()
        {
            var allowEventBeanType = new bool[10];
            var declClass = typeof(Math);
            var methodName = "Max";
            var args = new Type[] { typeof(int), typeof(int) };
            var expected = typeof(Math).GetMethod(methodName, args);
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, true, null, null));
    
            declClass = typeof(String);
            methodName = "Trim";
            args = new Type[0];
            expected = typeof(String).GetMethod(methodName, args);
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, true, null, null));
        }

        [Test]
    	public void TestResolveMethodNotFound()
    	{
            var allowEventBeanType = new bool[10];
            var declClass = typeof(String);
    		var methodName = "Trim";
    		Type[] args = null;
    		try
    		{
    			MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
    			Assert.Fail();
    		}
    		catch(EngineNoSuchMethodException e)
    		{
    			// Expected
    		}
    		
    		declClass = typeof(Math);
    		methodName = "Moox";
    		args = new Type[] { typeof(int), typeof(int) };
    		try
    		{
    			MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
    			Assert.Fail();
    		}
    		catch(EngineNoSuchMethodException e)
    		{
    			// Expected
    		}

            methodName = "Max";
    		args = new Type[] { typeof(bool), typeof(bool) };
    		try
    		{
    			MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
                Assert.Fail();
    		}
    		catch(EngineNoSuchMethodException e)
    		{
    			// Expected
    		}
    		
    		methodName = "Max";
    		args = new Type[] { typeof(int), typeof(int), typeof(bool) };
    		try
    		{
    			MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
                Assert.Fail();
    		}
    		catch(EngineNoSuchMethodException e)
    		{
    			// Expected
    		}
    	}

        [Test]
        public void TestResolveExtensionMethod()
        {
            var methodResolver = new EngineImportServiceImpl(
                false,
                false,
                false,
                false,
                MathContext.DECIMAL32,
                TimeZoneInfo.Local,
                new ConfigurationEngineDefaults.ThreadingProfile(), 
                null);

            var targetType = typeof(SupportAggMFFunc);
            var targetMethod = "GetName";
            var args = new Type[0];

            var method = methodResolver.ResolveMethod(targetType, targetMethod, args, null, null);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.DeclaringType, Is.EqualTo(typeof(SupportAggMFFuncExtensions)));
            Assert.That(method.Name, Is.EqualTo(targetMethod));
            Assert.That(method.IsExtensionMethod(), Is.True);
        }
    }
}
