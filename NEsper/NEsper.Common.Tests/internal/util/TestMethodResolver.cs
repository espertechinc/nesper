///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestMethodResolver : AbstractCommonTest
    {
        [Test]
        public void TestResolveMethodStaticOnly()
        {
            Type declClass = typeof(Math);
            string methodName = "Max";
            Type[] args = new Type[] { typeof(int), typeof(int) };
            var expected = typeof(Math).GetMethod(methodName, args);
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new Type[] { typeof(long), typeof(long) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new Type[] { typeof(int), typeof(long) };
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new Type[] { typeof(int), typeof(int) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new Type[] { typeof(int?), typeof(int?) };
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new Type[] { typeof(long), typeof(long) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new Type[] { typeof(int?), typeof(long?) };
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new Type[] { typeof(float), typeof(float) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new Type[] { typeof(int?), typeof(float?) };
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
            bool[] allowEventBeanType = new bool[10];
            Type declClass = typeof(Math);
            string methodName = "Max";
            Type[] args = new Type[] { typeof(int), typeof(int) };
            var expected = typeof(Math).GetMethod(methodName, args);
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, true, null, null));

            declClass = typeof(string);
            methodName = "Trim";
            args = new Type[0];
            expected = typeof(string).GetMethod(methodName, args);
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, true, null, null));
        }

        [Test]
        public void TestResolveMethodNotFound()
        {
            bool[] allowEventBeanType = new bool[10];
            Type declClass = typeof(string);
            string methodName = "trim";
            Type[] args = null;
            try
            {
                MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
                Assert.Fail();
            }
            catch (MethodResolverNoSuchMethodException)
            {
                // Expected
            }

            declClass = typeof(Math);
            methodName = "moox";
            args = new Type[] { typeof(int), typeof(int) };
            try
            {
                MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
                Assert.Fail();
            }
            catch (MethodResolverNoSuchMethodException)
            {
                // Expected
            }

            methodName = "max";
            args = new Type[] { typeof(bool), typeof(bool) };
            try
            {
                MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
                Assert.Fail();
            }
            catch (MethodResolverNoSuchMethodException)
            {
                // Expected
            }

            methodName = "max";
            args = new Type[] { typeof(int), typeof(int), typeof(bool) };
            try
            {
                MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null);
                Assert.Fail();
            }
            catch (MethodResolverNoSuchMethodException)
            {
                // Expected
            }
        }
    }
} // end of namespace
