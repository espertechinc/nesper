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
            var declClass = typeof(Math);
            var methodName = "Max";
            var args = new[] { typeof(int), typeof(int) };
            var expected = typeof(Math).GetMethod(methodName, args);
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new[] { typeof(long), typeof(long) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new[] { typeof(int), typeof(long) };
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new[] { typeof(int), typeof(int) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new[] { typeof(int?), typeof(int?) };
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new[] { typeof(long), typeof(long) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new[] { typeof(int?), typeof(long?) };
            Assert.AreEqual(expected, MethodResolver.ResolveMethod(declClass, methodName, args, false, null, null));

            args = new[] { typeof(float), typeof(float) };
            expected = typeof(Math).GetMethod(methodName, args);
            args = new[] { typeof(int?), typeof(float?) };
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
            var declClass = typeof(Math);
            var methodName = "Max";
            var args = new[] { typeof(int), typeof(int) };
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
            var declClass = typeof(string);
            var methodName = "trim";
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
            args = new[] { typeof(int), typeof(int) };
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
            args = new[] { typeof(bool), typeof(bool) };
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
            args = new[] { typeof(int), typeof(int), typeof(bool) };
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
